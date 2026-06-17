# Ephemeral Azure Demo Deploy — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** One-click deploy of the IoBuild microservices stack to a throwaway Azure VM at `iobuild-v2.arroz.dev`, and one-click destroy to stop billing — all via GitHub Actions.

**Architecture:** GitHub Actions builds 9 Docker images and pushes them to public GHCR, then runs `terraform apply` to create an Azure VM + public IP + NSG and a proxied Cloudflare A record. cloud-init installs Docker on the VM, clones the repo for config files, writes `.env` from secrets, and runs `docker compose -f docker-compose.prod.yml up`. A second workflow runs `terraform destroy`. Terraform state lives in an Azure Storage blob so the two workflows share state.

**Tech Stack:** Terraform (azurerm ~> 3.0, cloudflare ~> 4.0), Azure Linux VM (Ubuntu 22.04, Standard_B2ms), GitHub Actions, Docker Compose, GHCR, Cloudflare DNS.

**Verification model:** This is infra/config, not unit-tested application code. Each task is verified by `terraform validate`/`plan`, YAML/compose linting, or a live smoke test — not by a unit test framework. The final end-to-end verification is the deploy workflow reaching a 200 on `https://iobuild-v2.arroz.dev/health`.

**Repo facts:** owner `hallzyx`, repo `ioBuild-microservices`, git root is `fundamentos_arq/` with the stack under `microservices/`. All new files are additive — no existing file is modified.

---

## File Structure

```
infra/
  main.tf            # terraform block, providers, azurerm backend
  variables.tf       # all input variables
  vm.tf              # RG, vnet/subnet, public IP, NSG, NIC, Linux VM
  dns.tf             # cloudflare zone data + A record
  cloud-init.yaml    # VM first-boot: docker, clone, .env, compose up
  outputs.tf         # public IP, demo URL
  terraform.tfvars.example   # documents required vars (no secrets committed)
  README.md          # one-time bootstrap (state storage) + usage

microservices/
  docker-compose.prod.yml    # GHCR images, no build:

.github/workflows/
  deploy.yml         # build+push matrix → terraform apply → smoke test
  destroy.yml        # terraform destroy
```

Operator-side (documented, not files): GitHub repository secrets and a one-time tfstate storage account.

---

## Task 1: Production compose file (GHCR images)

**Files:**
- Create: `microservices/docker-compose.prod.yml`

- [ ] **Step 1: Create the prod compose file**

Twin of `microservices/docker-compose.yml`, but every built service uses `image:` from GHCR instead of `build:`. Pulled third-party images (mysql, mosquitto, influxdb) are unchanged. Volumes/networks/healthchecks identical to the dev file.

```yaml
# docker-compose.prod.yml — GHCR images, no local build. Used by the Azure VM.
services:
  mysql:
    image: mysql:8.0
    container_name: iobuild-mysql
    restart: unless-stopped
    mem_limit: 384m
    environment:
      - MYSQL_ROOT_PASSWORD=${DB_PASSWORD:-iobuild123}
    ports:
      - "3306:3306"
    volumes:
      - mysql_data:/var/lib/mysql
      - ./mysql/init.sql:/docker-entrypoint-initdb.d/init.sql
    healthcheck:
      test: ["CMD", "mysqladmin", "ping", "-h", "localhost"]
      interval: 10s
      timeout: 5s
      retries: 10
      start_period: 30s
    networks: [iobuild-network]

  iam:
    image: ghcr.io/hallzyx/iobuild-iam:${IMAGE_TAG:-latest}
    container_name: iobuild-iam
    restart: unless-stopped
    mem_limit: 256m
    depends_on:
      mysql: { condition: service_healthy }
    environment:
      - ASPNETCORE_URLS=http://+:5001
      - ASPNETCORE_ENVIRONMENT=Production
      - DB_HOST=mysql
      - DB_PORT=3306
      - DB_USER=root
      - DB_PASSWORD=${DB_PASSWORD:-iobuild123}
      - DB_NAME=iobuild_iam
      - JWT_SECRET=${JWT_SECRET:-dev-fallback-key-minimum-32-characters!!}
    networks: [iobuild-network]
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5001/health"]
      interval: 10s
      timeout: 5s
      retries: 5
      start_period: 20s

  devices:
    image: ghcr.io/hallzyx/iobuild-devices:${IMAGE_TAG:-latest}
    container_name: iobuild-devices
    restart: unless-stopped
    mem_limit: 256m
    depends_on:
      iam: { condition: service_healthy }
      influxdb: { condition: service_healthy }
      mosquitto: { condition: service_started }
    environment:
      - ASPNETCORE_URLS=http://+:5002
      - ASPNETCORE_ENVIRONMENT=Production
      - DB_HOST=mysql
      - DB_PORT=3306
      - DB_USER=root
      - DB_PASSWORD=${DB_PASSWORD:-iobuild123}
      - DB_NAME=iobuild_devices
      - JWT_SECRET=${JWT_SECRET:-dev-fallback-key-minimum-32-characters!!}
      - INFLUXDB_TOKEN=${INFLUXDB_TOKEN:-iobuild-telemetry-token}
      - MQTT_HOST=mosquitto
      - MQTT_PORT=1883
    networks: [iobuild-network]
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5002/health"]
      interval: 10s
      timeout: 5s
      retries: 5
      start_period: 20s

  projects:
    image: ghcr.io/hallzyx/iobuild-projects:${IMAGE_TAG:-latest}
    container_name: iobuild-projects
    restart: unless-stopped
    mem_limit: 256m
    depends_on:
      iam: { condition: service_healthy }
    environment:
      - ASPNETCORE_URLS=http://+:5003
      - ASPNETCORE_ENVIRONMENT=Production
      - DB_HOST=mysql
      - DB_PORT=3306
      - DB_USER=root
      - DB_PASSWORD=${DB_PASSWORD:-iobuild123}
      - DB_NAME=iobuild_projects
      - JWT_SECRET=${JWT_SECRET:-dev-fallback-key-minimum-32-characters!!}
    networks: [iobuild-network]
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5003/health"]
      interval: 10s
      timeout: 5s
      retries: 5
      start_period: 20s

  subscriptions:
    image: ghcr.io/hallzyx/iobuild-subscriptions:${IMAGE_TAG:-latest}
    container_name: iobuild-subscriptions
    restart: unless-stopped
    mem_limit: 256m
    depends_on:
      iam: { condition: service_healthy }
    environment:
      - ASPNETCORE_URLS=http://+:5004
      - ASPNETCORE_ENVIRONMENT=Production
      - DB_HOST=mysql
      - DB_PORT=3306
      - DB_USER=root
      - DB_PASSWORD=${DB_PASSWORD:-iobuild123}
      - DB_NAME=iobuild_subscriptions
      - JWT_SECRET=${JWT_SECRET:-dev-fallback-key-minimum-32-characters!!}
      - STRIPE_SECRET_KEY=${STRIPE_SECRET_KEY}
      - STRIPE_PUBLISHABLE_KEY=${STRIPE_PUBLISHABLE_KEY}
      - STRIPE_WEBHOOK_SECRET=${STRIPE_WEBHOOK_SECRET}
    networks: [iobuild-network]
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5004/health"]
      interval: 10s
      timeout: 5s
      retries: 5
      start_period: 20s

  analytics:
    image: ghcr.io/hallzyx/iobuild-analytics:${IMAGE_TAG:-latest}
    container_name: iobuild-analytics
    restart: unless-stopped
    mem_limit: 256m
    depends_on:
      iam: { condition: service_healthy }
    environment:
      - ASPNETCORE_URLS=http://+:5005
      - ASPNETCORE_ENVIRONMENT=Production
      - DB_HOST=mysql
      - DB_PORT=3306
      - DB_USER=root
      - DB_PASSWORD=${DB_PASSWORD:-iobuild123}
      - DB_NAME=iobuild_analytics
      - Services__DevicesApi=http://devices:5002
      - Services__ProjectsApi=http://projects:5003
    networks: [iobuild-network]
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5005/health"]
      interval: 10s
      timeout: 5s
      retries: 5
      start_period: 20s

  profiles:
    image: ghcr.io/hallzyx/iobuild-profiles:${IMAGE_TAG:-latest}
    container_name: iobuild-profiles
    restart: unless-stopped
    mem_limit: 128m
    depends_on:
      iam: { condition: service_healthy }
    environment:
      - ASPNETCORE_URLS=http://+:5006
      - ASPNETCORE_ENVIRONMENT=Production
      - DB_HOST=mysql
      - DB_PORT=3306
      - DB_USER=root
      - DB_PASSWORD=${DB_PASSWORD:-iobuild123}
      - DB_NAME=iobuild_profiles
      - JWT_SECRET=${JWT_SECRET:-dev-fallback-key-minimum-32-characters!!}
    networks: [iobuild-network]
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5006/health"]
      interval: 10s
      timeout: 5s
      retries: 5
      start_period: 20s

  gateway:
    image: ghcr.io/hallzyx/iobuild-gateway:${IMAGE_TAG:-latest}
    container_name: iobuild-gateway
    restart: unless-stopped
    mem_limit: 128m
    depends_on:
      iam: { condition: service_healthy }
      devices: { condition: service_healthy }
      projects: { condition: service_healthy }
      subscriptions: { condition: service_healthy }
      analytics: { condition: service_healthy }
    environment:
      - ASPNETCORE_URLS=http://+:8080
      - ASPNETCORE_ENVIRONMENT=Docker
    expose: ["8080"]
    networks: [iobuild-network]
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 15s
      timeout: 5s
      retries: 5
      start_period: 30s

  mosquitto:
    image: eclipse-mosquitto:2-openssl
    container_name: iobuild-mosquitto
    restart: unless-stopped
    mem_limit: 16m
    ports:
      - "1883:1883"
    volumes:
      - ./mosquitto/config/mosquitto.conf:/mosquitto/config/mosquitto.conf
      - mosquitto_data:/mosquitto/data
    networks: [iobuild-network]

  influxdb:
    image: influxdb:2.7-alpine
    container_name: iobuild-influxdb
    restart: unless-stopped
    mem_limit: 64m
    environment:
      - DOCKER_INFLUXDB_INIT_MODE=setup
      - DOCKER_INFLUXDB_INIT_USERNAME=admin
      - DOCKER_INFLUXDB_INIT_PASSWORD=${INFLUXDB_PASSWORD:-admin123}
      - DOCKER_INFLUXDB_INIT_ORG=iobuild
      - DOCKER_INFLUXDB_INIT_BUCKET=iobuild-telemetry
      - DOCKER_INFLUXDB_INIT_ADMIN_TOKEN=${INFLUXDB_TOKEN:-iobuild-telemetry-token}
    volumes:
      - influxdb_data:/var/lib/influxdb2
    networks: [iobuild-network]
    healthcheck:
      test: ["CMD", "influx", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5
      start_period: 20s

  simulator:
    image: ghcr.io/hallzyx/iobuild-simulator:${IMAGE_TAG:-latest}
    container_name: iobuild-simulator
    restart: unless-stopped
    mem_limit: 32m
    depends_on:
      - mosquitto
    environment:
      - MQTT_HOST=mosquitto
      - MQTT_PORT=1883
      - DEVICE_COUNT=5
    networks: [iobuild-network]

  frontend:
    image: ghcr.io/hallzyx/iobuild-frontend:${IMAGE_TAG:-latest}
    container_name: iobuild-frontend
    restart: unless-stopped
    depends_on:
      - gateway
    ports:
      - "80:80"
    networks: [iobuild-network]
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:80/"]
      interval: 15s
      timeout: 5s
      retries: 5
      start_period: 10s

networks:
  iobuild-network:
    driver: bridge

volumes:
  mysql_data:
    driver: local
  mosquitto_data:
    driver: local
  influxdb_data:
    driver: local
```

Note vs dev file: `frontend` now publishes `80:80` (was `expose` only, because Dokploy/Traefik used to route). On the VM, Cloudflare hits the host's port 80 directly.

- [ ] **Step 2: Validate compose syntax**

Run: `docker compose -f microservices/docker-compose.prod.yml config -q`
Expected: no output, exit 0 (valid). If `docker` unavailable locally, run `python -c "import yaml,sys; yaml.safe_load(open('microservices/docker-compose.prod.yml'))"` — expected: no error.

- [ ] **Step 3: Commit**

```bash
git add microservices/docker-compose.prod.yml
git commit -m "feat(deploy): add GHCR-based production compose for Azure demo"
```

---

## Task 2: Terraform skeleton (providers, variables, outputs)

**Files:**
- Create: `infra/main.tf`
- Create: `infra/variables.tf`
- Create: `infra/outputs.tf`
- Create: `infra/terraform.tfvars.example`

- [ ] **Step 1: Write `infra/main.tf`**

```hcl
terraform {
  required_version = ">= 1.5.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
    cloudflare = {
      source  = "cloudflare/cloudflare"
      version = "~> 4.0"
    }
  }

  # Remote state in Azure Storage. The storage account/container are created
  # once by infra/README.md bootstrap. Values are passed via -backend-config
  # in CI so no secrets live in this file.
  backend "azurerm" {}
}

provider "azurerm" {
  features {}
}

provider "cloudflare" {
  api_token = var.cloudflare_api_token
}
```

- [ ] **Step 2: Write `infra/variables.tf`**

```hcl
variable "location" {
  description = "Azure region"
  type        = string
  default     = "eastus"
}

variable "resource_group_name" {
  description = "Resource group name"
  type        = string
  default     = "rg-iobuild-demo"
}

variable "vm_size" {
  description = "Azure VM size"
  type        = string
  default     = "Standard_B2ms"
}

variable "admin_username" {
  description = "VM admin username"
  type        = string
  default     = "azureuser"
}

variable "ssh_public_key" {
  description = "SSH public key for the VM admin user"
  type        = string
}

variable "ssh_allow_cidr" {
  description = "CIDR allowed to reach SSH (port 22)"
  type        = string
}

variable "cloudflare_api_token" {
  description = "Cloudflare API token (Zone.DNS edit on arroz.dev)"
  type        = string
  sensitive   = true
}

variable "cloudflare_zone_name" {
  description = "Cloudflare zone"
  type        = string
  default     = "arroz.dev"
}

variable "domain_name" {
  description = "Full FQDN for the demo"
  type        = string
  default     = "iobuild-v2.arroz.dev"
}

variable "image_tag" {
  description = "Docker image tag to deploy (GHCR)"
  type        = string
  default     = "latest"
}

# App secrets injected into the VM .env via cloud-init
variable "db_password" {
  type      = string
  sensitive = true
}
variable "jwt_secret" {
  type      = string
  sensitive = true
}
variable "influxdb_token" {
  type      = string
  sensitive = true
}
variable "influxdb_password" {
  type      = string
  sensitive = true
}
variable "stripe_secret_key" {
  type      = string
  sensitive = true
}
variable "stripe_publishable_key" {
  type      = string
  sensitive = true
}
variable "stripe_webhook_secret" {
  type      = string
  sensitive = true
}
```

- [ ] **Step 3: Write `infra/outputs.tf`**

```hcl
output "vm_public_ip" {
  description = "Public IP of the demo VM"
  value       = azurerm_public_ip.vm.ip_address
}

output "demo_url" {
  description = "Demo URL"
  value       = "https://${var.domain_name}"
}
```

- [ ] **Step 4: Write `infra/terraform.tfvars.example`**

```hcl
# Copy to terraform.tfvars for local runs (DO NOT commit terraform.tfvars).
# In CI these are passed via TF_VAR_* env from GitHub Secrets.
ssh_public_key         = "ssh-ed25519 AAAA... you@host"
ssh_allow_cidr         = "203.0.113.7/32"
cloudflare_api_token   = "cf-token"
db_password            = "change-me"
jwt_secret             = "a-32+char-secret-................"
influxdb_token         = "iobuild-telemetry-token"
influxdb_password      = "admin123"
stripe_secret_key      = "sk_test_..."
stripe_publishable_key = "pk_test_..."
stripe_webhook_secret  = "whsec_..."
```

- [ ] **Step 5: Format and validate**

Run: `cd infra && terraform fmt && terraform init -backend=false && terraform validate`
Expected: `Success! The configuration is valid.` (vm.tf/dns.tf added in later tasks may make validate report missing resources referenced by outputs — if so, this step passes only after Task 3 and Task 4; run validate again then.)

- [ ] **Step 6: Commit**

```bash
git add infra/main.tf infra/variables.tf infra/outputs.tf infra/terraform.tfvars.example
git commit -m "feat(infra): terraform providers, variables, outputs skeleton"
```

---

## Task 3: Azure VM, network, NSG (vm.tf)

**Files:**
- Create: `infra/vm.tf`

- [ ] **Step 1: Write `infra/vm.tf`**

```hcl
resource "azurerm_resource_group" "main" {
  name     = var.resource_group_name
  location = var.location
  tags     = { project = "iobuild-demo", lifecycle = "ephemeral" }
}

resource "azurerm_virtual_network" "main" {
  name                = "iobuild-vnet"
  address_space       = ["10.10.0.0/16"]
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
}

resource "azurerm_subnet" "main" {
  name                 = "iobuild-subnet"
  resource_group_name  = azurerm_resource_group.main.name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = ["10.10.1.0/24"]
}

resource "azurerm_public_ip" "vm" {
  name                = "iobuild-pip"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  allocation_method   = "Static"
  sku                 = "Standard"
}

resource "azurerm_network_security_group" "main" {
  name                = "iobuild-nsg"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name

  security_rule {
    name                       = "allow-http"
    priority                   = 100
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "80"
    source_address_prefix      = "*"
    destination_address_prefix = "*"
  }

  security_rule {
    name                       = "allow-https"
    priority                   = 110
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "443"
    source_address_prefix      = "*"
    destination_address_prefix = "*"
  }

  security_rule {
    name                       = "allow-ssh"
    priority                   = 120
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "22"
    source_address_prefix      = var.ssh_allow_cidr
    destination_address_prefix = "*"
  }
}

resource "azurerm_network_interface" "main" {
  name                = "iobuild-nic"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name

  ip_configuration {
    name                          = "internal"
    subnet_id                     = azurerm_subnet.main.id
    private_ip_address_allocation = "Dynamic"
    public_ip_address_id          = azurerm_public_ip.vm.id
  }
}

resource "azurerm_network_interface_security_group_association" "main" {
  network_interface_id      = azurerm_network_interface.main.id
  network_security_group_id = azurerm_network_security_group.main.id
}

resource "azurerm_linux_virtual_machine" "main" {
  name                  = "iobuild-vm"
  resource_group_name   = azurerm_resource_group.main.name
  location              = azurerm_resource_group.main.location
  size                  = var.vm_size
  admin_username        = var.admin_username
  network_interface_ids = [azurerm_network_interface.main.id]

  admin_ssh_key {
    username   = var.admin_username
    public_key = var.ssh_public_key
  }

  os_disk {
    caching              = "ReadWrite"
    storage_account_type = "Standard_LRS"
    disk_size_gb         = 64
  }

  source_image_reference {
    publisher = "Canonical"
    offer     = "0001-com-ubuntu-server-jammy"
    sku       = "22_04-lts-gen2"
    version   = "latest"
  }

  custom_data = base64encode(templatefile("${path.module}/cloud-init.yaml", {
    image_tag              = var.image_tag
    db_password            = var.db_password
    jwt_secret             = var.jwt_secret
    influxdb_token         = var.influxdb_token
    influxdb_password      = var.influxdb_password
    stripe_secret_key      = var.stripe_secret_key
    stripe_publishable_key = var.stripe_publishable_key
    stripe_webhook_secret  = var.stripe_webhook_secret
  }))

  tags = { project = "iobuild-demo", lifecycle = "ephemeral" }
}
```

- [ ] **Step 2: Validate**

Run: `cd infra && terraform fmt && terraform validate`
Expected: validate will report `cloud-init.yaml` missing (created in Task 5) — that is expected at this point. The HCL syntax itself must be clean (no "Unsupported argument"/"Invalid resource type" errors). If only the templatefile/file-not-found surfaces, proceed; re-validate after Task 5.

- [ ] **Step 3: Commit**

```bash
git add infra/vm.tf
git commit -m "feat(infra): azure vm, network, nsg"
```

---

## Task 4: Cloudflare DNS record (dns.tf)

**Files:**
- Create: `infra/dns.tf`

- [ ] **Step 1: Write `infra/dns.tf`**

```hcl
data "cloudflare_zone" "main" {
  name = var.cloudflare_zone_name
}

resource "cloudflare_record" "demo" {
  zone_id = data.cloudflare_zone.main.id
  name    = var.domain_name
  type    = "A"
  value   = azurerm_public_ip.vm.ip_address
  proxied = true
  ttl     = 1 # 1 = automatic; required to be 1 when proxied
}
```

Note: `cloudflare_record` is the v4 provider resource (uses `value`). The specific `iobuild-v2` A record coexists with the existing `*.arroz.dev` wildcard and takes precedence for this hostname.

- [ ] **Step 2: Validate**

Run: `cd infra && terraform fmt && terraform validate`
Expected: same caveat as Task 3 step 2 (cloud-init.yaml still missing until Task 5). HCL for dns.tf must parse cleanly.

- [ ] **Step 3: Commit**

```bash
git add infra/dns.tf
git commit -m "feat(infra): cloudflare A record for demo domain"
```

---

## Task 5: cloud-init (VM first boot)

**Files:**
- Create: `infra/cloud-init.yaml`

- [ ] **Step 1: Write `infra/cloud-init.yaml`**

```yaml
#cloud-config
package_update: true
packages:
  - ca-certificates
  - curl
  - git

write_files:
  - path: /opt/iobuild/bootstrap.sh
    permissions: "0755"
    content: |
      #!/usr/bin/env bash
      set -euo pipefail

      # Install Docker Engine + compose plugin
      install -m 0755 -d /etc/apt/keyrings
      curl -fsSL https://download.docker.com/linux/ubuntu/gpg -o /etc/apt/keyrings/docker.asc
      chmod a+r /etc/apt/keyrings/docker.asc
      echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.asc] https://download.docker.com/linux/ubuntu $(. /etc/os-release && echo $VERSION_CODENAME) stable" \
        > /etc/apt/sources.list.d/docker.list
      apt-get update -y
      apt-get install -y docker-ce docker-ce-cli containerd.io docker-compose-plugin

      # Clone repo for config files (public repo, no auth)
      rm -rf /opt/iobuild/repo
      git clone --depth 1 https://github.com/hallzyx/ioBuild-microservices.git /opt/iobuild/repo

      cd /opt/iobuild/repo/microservices

      # Write .env from templated secrets
      cat > .env <<'ENVEOF'
      IMAGE_TAG=${image_tag}
      DB_PASSWORD=${db_password}
      JWT_SECRET=${jwt_secret}
      INFLUXDB_TOKEN=${influxdb_token}
      INFLUXDB_PASSWORD=${influxdb_password}
      STRIPE_SECRET_KEY=${stripe_secret_key}
      STRIPE_PUBLISHABLE_KEY=${stripe_publishable_key}
      STRIPE_WEBHOOK_SECRET=${stripe_webhook_secret}
      ENVEOF

      # Pull GHCR images (public) and start the stack
      docker compose -f docker-compose.prod.yml pull
      docker compose -f docker-compose.prod.yml up -d

runcmd:
  - /opt/iobuild/bootstrap.sh > /var/log/iobuild-bootstrap.log 2>&1
```

Note: Terraform `templatefile` substitutes `${...}` placeholders before the file reaches the VM, so the rendered `.env` contains real values. The `<<'ENVEOF'` heredoc is quoted to prevent the *shell* from re-expanding, but Terraform has already injected the values at render time.

- [ ] **Step 2: Validate full Terraform config now that all files exist**

Run: `cd infra && terraform fmt && terraform init -backend=false && terraform validate`
Expected: `Success! The configuration is valid.`

- [ ] **Step 3: Commit**

```bash
git add infra/cloud-init.yaml
git commit -m "feat(infra): cloud-init to install docker and run the stack"
```

---

## Task 6: infra README (bootstrap + usage)

**Files:**
- Create: `infra/README.md`

- [ ] **Step 1: Write `infra/README.md`**

````markdown
# infra/ — Ephemeral Azure demo for IoBuild

`terraform apply` creates a throwaway VM running the full stack at
`https://iobuild-v2.arroz.dev`. `terraform destroy` removes it so Azure
billing stops. Normally driven by GitHub Actions (Deploy / Destroy), but
can be run locally with the variables in `terraform.tfvars.example`.

## One-time bootstrap: remote state storage

Run ONCE. Creates the Azure Storage that holds Terraform state so the
Deploy and Destroy workflows share state.

```bash
az group create -n rg-iobuild-tfstate -l eastus
az storage account create -n iobuildtfstate$RANDOM -g rg-iobuild-tfstate \
  -l eastus --sku Standard_LRS
# note the account name it prints, then:
az storage container create -n tfstate --account-name <account-name>
```

Record `resource_group_name`, `storage_account_name`, `container_name`,
and `key = iobuild-demo.tfstate`. These feed `-backend-config` (see workflows).

## Local usage (optional)

```bash
cp terraform.tfvars.example terraform.tfvars   # fill in real values
terraform init \
  -backend-config="resource_group_name=rg-iobuild-tfstate" \
  -backend-config="storage_account_name=<account-name>" \
  -backend-config="container_name=tfstate" \
  -backend-config="key=iobuild-demo.tfstate"
terraform apply
# ... demo ...
terraform destroy
```

## What gets created / destroyed

- Azure: resource group `rg-iobuild-demo`, VNet, subnet, public IP, NSG, NIC, VM.
- Cloudflare: A record `iobuild-v2.arroz.dev` (proxied).

`destroy` removes all of the above. The tfstate storage account persists
(delete `rg-iobuild-tfstate` manually when retiring the project).
````

- [ ] **Step 2: Commit**

```bash
git add infra/README.md
git commit -m "docs(infra): bootstrap and usage instructions"
```

---

## Task 7: Deploy workflow (build → push → apply → smoke test)

**Files:**
- Create: `.github/workflows/deploy.yml`

- [ ] **Step 1: Write `.github/workflows/deploy.yml`**

```yaml
name: Deploy demo to Azure

on:
  workflow_dispatch:
    inputs:
      image_tag:
        description: "Image tag to build and deploy"
        default: "latest"
        required: false

permissions:
  contents: read
  packages: write

env:
  REGISTRY: ghcr.io
  OWNER: hallzyx

jobs:
  build-push:
    runs-on: ubuntu-latest
    strategy:
      matrix:
        include:
          - { name: iam,           context: microservices, dockerfile: microservices/src/IoBuild.IAM/Dockerfile }
          - { name: devices,       context: microservices, dockerfile: microservices/src/IoBuild.Devices/Dockerfile }
          - { name: projects,      context: microservices, dockerfile: microservices/src/IoBuild.Projects/Dockerfile }
          - { name: subscriptions, context: microservices, dockerfile: microservices/src/IoBuild.Subscriptions/Dockerfile }
          - { name: analytics,     context: microservices, dockerfile: microservices/src/IoBuild.Analytics/Dockerfile }
          - { name: profiles,      context: microservices, dockerfile: microservices/src/IoBuild.Profiles/Dockerfile }
          - { name: gateway,       context: microservices, dockerfile: microservices/src/IoBuild.Gateway/Dockerfile }
          - { name: simulator,     context: microservices/iot-simulator, dockerfile: microservices/iot-simulator/Dockerfile }
          - { name: frontend,      context: microservices/frontend-docker, dockerfile: microservices/frontend-docker/Dockerfile }
    steps:
      - uses: actions/checkout@v4

      - uses: docker/setup-buildx-action@v3

      - name: Log in to GHCR
        uses: docker/login-action@v3
        with:
          registry: ${{ env.REGISTRY }}
          username: ${{ github.actor }}
          password: ${{ secrets.GITHUB_TOKEN }}

      - name: Build and push ${{ matrix.name }}
        uses: docker/build-push-action@v6
        with:
          context: ${{ matrix.context }}
          file: ${{ matrix.dockerfile }}
          push: true
          tags: ${{ env.REGISTRY }}/${{ env.OWNER }}/iobuild-${{ matrix.name }}:${{ inputs.image_tag }}
          build-args: |
            VITE_CLOUDINARY_CLOUD_NAME=${{ secrets.VITE_CLOUDINARY_CLOUD_NAME }}
            VITE_CLOUDINARY_UPLOAD_PRESET=${{ secrets.VITE_CLOUDINARY_UPLOAD_PRESET }}

  terraform-apply:
    needs: build-push
    runs-on: ubuntu-latest
    env:
      ARM_USE_OIDC: "false"
      TF_VAR_image_tag: ${{ inputs.image_tag }}
      TF_VAR_ssh_public_key: ${{ secrets.SSH_PUBLIC_KEY }}
      TF_VAR_ssh_allow_cidr: ${{ secrets.SSH_ALLOW_CIDR }}
      TF_VAR_cloudflare_api_token: ${{ secrets.CLOUDFLARE_API_TOKEN }}
      TF_VAR_db_password: ${{ secrets.DB_PASSWORD }}
      TF_VAR_jwt_secret: ${{ secrets.JWT_SECRET }}
      TF_VAR_influxdb_token: ${{ secrets.INFLUXDB_TOKEN }}
      TF_VAR_influxdb_password: ${{ secrets.INFLUXDB_PASSWORD }}
      TF_VAR_stripe_secret_key: ${{ secrets.STRIPE_SECRET_KEY }}
      TF_VAR_stripe_publishable_key: ${{ secrets.STRIPE_PUBLISHABLE_KEY }}
      TF_VAR_stripe_webhook_secret: ${{ secrets.STRIPE_WEBHOOK_SECRET }}
    steps:
      - uses: actions/checkout@v4

      - name: Azure login
        uses: azure/login@v2
        with:
          creds: ${{ secrets.AZURE_CREDENTIALS }}

      - name: Export ARM creds for Terraform
        run: |
          echo "ARM_CLIENT_ID=$(echo '${{ secrets.AZURE_CREDENTIALS }}' | jq -r .clientId)" >> $GITHUB_ENV
          echo "ARM_CLIENT_SECRET=$(echo '${{ secrets.AZURE_CREDENTIALS }}' | jq -r .clientSecret)" >> $GITHUB_ENV
          echo "ARM_SUBSCRIPTION_ID=$(echo '${{ secrets.AZURE_CREDENTIALS }}' | jq -r .subscriptionId)" >> $GITHUB_ENV
          echo "ARM_TENANT_ID=$(echo '${{ secrets.AZURE_CREDENTIALS }}' | jq -r .tenantId)" >> $GITHUB_ENV

      - uses: hashicorp/setup-terraform@v3

      - name: Terraform init
        working-directory: infra
        run: |
          terraform init \
            -backend-config="resource_group_name=${{ secrets.TFSTATE_RG }}" \
            -backend-config="storage_account_name=${{ secrets.TFSTATE_SA }}" \
            -backend-config="container_name=${{ secrets.TFSTATE_CONTAINER }}" \
            -backend-config="key=iobuild-demo.tfstate"

      - name: Terraform apply
        working-directory: infra
        run: terraform apply -auto-approve

      - name: Output URL
        working-directory: infra
        run: terraform output -raw demo_url

      - name: Smoke test
        run: |
          URL="https://iobuild-v2.arroz.dev/health"
          for i in $(seq 1 40); do
            code=$(curl -s -o /dev/null -w "%{http_code}" "$URL" || true)
            echo "attempt $i: $code"
            if [ "$code" = "200" ]; then echo "stack is live"; exit 0; fi
            sleep 15
          done
          echo "smoke test did not reach 200 (stack may still be booting)"; exit 1
```

- [ ] **Step 2: Lint the workflow YAML**

Run: `python -c "import yaml; yaml.safe_load(open('.github/workflows/deploy.yml'))"`
Expected: no error (valid YAML).

- [ ] **Step 3: Commit**

```bash
git add .github/workflows/deploy.yml
git commit -m "ci: deploy workflow — build/push to GHCR then terraform apply"
```

---

## Task 8: Destroy workflow

**Files:**
- Create: `.github/workflows/destroy.yml`

- [ ] **Step 1: Write `.github/workflows/destroy.yml`**

```yaml
name: Destroy Azure demo

on:
  workflow_dispatch:

permissions:
  contents: read

jobs:
  terraform-destroy:
    runs-on: ubuntu-latest
    env:
      # destroy still needs all TF_VARs declared, even if only used at create time
      TF_VAR_ssh_public_key: ${{ secrets.SSH_PUBLIC_KEY }}
      TF_VAR_ssh_allow_cidr: ${{ secrets.SSH_ALLOW_CIDR }}
      TF_VAR_cloudflare_api_token: ${{ secrets.CLOUDFLARE_API_TOKEN }}
      TF_VAR_db_password: ${{ secrets.DB_PASSWORD }}
      TF_VAR_jwt_secret: ${{ secrets.JWT_SECRET }}
      TF_VAR_influxdb_token: ${{ secrets.INFLUXDB_TOKEN }}
      TF_VAR_influxdb_password: ${{ secrets.INFLUXDB_PASSWORD }}
      TF_VAR_stripe_secret_key: ${{ secrets.STRIPE_SECRET_KEY }}
      TF_VAR_stripe_publishable_key: ${{ secrets.STRIPE_PUBLISHABLE_KEY }}
      TF_VAR_stripe_webhook_secret: ${{ secrets.STRIPE_WEBHOOK_SECRET }}
    steps:
      - uses: actions/checkout@v4

      - name: Azure login
        uses: azure/login@v2
        with:
          creds: ${{ secrets.AZURE_CREDENTIALS }}

      - name: Export ARM creds for Terraform
        run: |
          echo "ARM_CLIENT_ID=$(echo '${{ secrets.AZURE_CREDENTIALS }}' | jq -r .clientId)" >> $GITHUB_ENV
          echo "ARM_CLIENT_SECRET=$(echo '${{ secrets.AZURE_CREDENTIALS }}' | jq -r .clientSecret)" >> $GITHUB_ENV
          echo "ARM_SUBSCRIPTION_ID=$(echo '${{ secrets.AZURE_CREDENTIALS }}' | jq -r .subscriptionId)" >> $GITHUB_ENV
          echo "ARM_TENANT_ID=$(echo '${{ secrets.AZURE_CREDENTIALS }}' | jq -r .tenantId)" >> $GITHUB_ENV

      - uses: hashicorp/setup-terraform@v3

      - name: Terraform init
        working-directory: infra
        run: |
          terraform init \
            -backend-config="resource_group_name=${{ secrets.TFSTATE_RG }}" \
            -backend-config="storage_account_name=${{ secrets.TFSTATE_SA }}" \
            -backend-config="container_name=${{ secrets.TFSTATE_CONTAINER }}" \
            -backend-config="key=iobuild-demo.tfstate"

      - name: Terraform destroy
        working-directory: infra
        run: terraform destroy -auto-approve
```

- [ ] **Step 2: Lint the workflow YAML**

Run: `python -c "import yaml; yaml.safe_load(open('.github/workflows/destroy.yml'))"`
Expected: no error (valid YAML).

- [ ] **Step 3: Commit**

```bash
git add .github/workflows/destroy.yml
git commit -m "ci: destroy workflow — terraform destroy to stop billing"
```

---

## Task 9: Operator setup (GitHub Secrets) — documented, manual

**Files:** none (operator actions). Capture as a checklist in the PR description or `infra/README.md` if preferred.

- [ ] **Step 1: Create the Azure service principal**

Run (operator):
```bash
az ad sp create-for-rbac --name iobuild-demo-sp \
  --role Contributor \
  --scopes /subscriptions/<SUBSCRIPTION_ID> \
  --sdk-auth
```
Copy the JSON output → GitHub secret `AZURE_CREDENTIALS`.

- [ ] **Step 2: Add all GitHub repository secrets**

Settings → Secrets and variables → Actions → New repository secret, for each:
`AZURE_CREDENTIALS`, `CLOUDFLARE_API_TOKEN`, `SSH_PUBLIC_KEY`, `SSH_ALLOW_CIDR`,
`DB_PASSWORD`, `JWT_SECRET`, `INFLUXDB_TOKEN`, `INFLUXDB_PASSWORD`,
`STRIPE_SECRET_KEY`, `STRIPE_PUBLISHABLE_KEY`, `STRIPE_WEBHOOK_SECRET`,
`VITE_CLOUDINARY_CLOUD_NAME`, `VITE_CLOUDINARY_UPLOAD_PRESET`,
`TFSTATE_RG`, `TFSTATE_SA`, `TFSTATE_CONTAINER`.

- [ ] **Step 3: Verify (end-to-end)**

1. Run the bootstrap in `infra/README.md` (creates tfstate storage).
2. Actions → "Deploy demo to Azure" → Run workflow.
3. Wait for the smoke-test step to report `stack is live`.
4. Open `https://iobuild-v2.arroz.dev` in a browser — frontend loads, login works.
5. Actions → "Destroy Azure demo" → Run workflow.
6. Confirm in Azure Portal that `rg-iobuild-demo` is gone.

Expected: deploy reaches a live URL; destroy removes the resource group and the Cloudflare record.

---

## Self-Review Notes

- **Spec coverage:** prod compose (Task 1), Terraform Azure (Task 3) + Cloudflare (Task 4), cloud-init (Task 5), remote state (Task 2 backend + Task 6 bootstrap), deploy/destroy workflows (Tasks 7–8), secrets (Task 9), Flexible TLS (Cloudflare proxied in Task 4 — note: SSL mode "Flexible" is a zone-level Cloudflare setting; set once in the Cloudflare dashboard for `arroz.dev`, documented as part of Task 9 verification). All spec sections map to a task.
- **Image names:** consistent `ghcr.io/hallzyx/iobuild-<svc>` across Task 1 and Task 7.
- **Frontend port:** Task 1 publishes `80:80` (the one intentional difference from the dev compose); documented inline.
- **Validation caveat:** `terraform validate` only fully passes after Task 5 (cloud-init.yaml exists); flagged in Tasks 3–4.
```

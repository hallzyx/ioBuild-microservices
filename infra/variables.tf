variable "location" {
  description = "Azure region (must be allowed by the student subscription policy)"
  type        = string
  default     = "eastus2"
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

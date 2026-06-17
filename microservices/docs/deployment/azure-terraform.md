# Despliegue en Azure — Terraform + Cloudflare

> **Estado actual:** Este es el método de despliegue vigente.  
> El Dokploy + VPS + Traefik de la Iteración 1 fue reemplazado por esta arquitectura.

---

## Visión General

IoBuild se despliega como una **VM efímera en Azure**, creada y destruida con Terraform. La idea es levantarla para una demo y apagarla cuando termina — Azure deja de cobrar ni bien se destruye.

```
GitHub Actions (CI)          Terraform (local)         Azure + Cloudflare
─────────────────────        ──────────────────        ──────────────────────
Build .NET + Vue     →       terraform apply   →       Azure VM (Ubuntu 22.04)
Push a GHCR          →       cloud-init.yaml   →       Docker Compose prod
(una sola vez)               Cloudflare DNS    →       iobuild-v2.arroz.dev
```

---

## Arquitectura de Producción

```
Internet (HTTPS)
      │
      ▼
┌──────────────────────────────────────────────────────────┐
│  CLOUDFLARE                                               │
│  Responsabilidad: Edge proxy + TLS                        │
│  - A record proxied (orange cloud)                        │
│  - TLS Flexible: HTTPS al edge → HTTP a la VM           │
│  - Oculta la IP real de la VM                            │
└─────────────────────────┬────────────────────────────────┘
                          │ HTTP :80 (Azure VM)
                          ▼
┌──────────────────────────────────────────────────────────┐
│  AZURE VM (Ubuntu 22.04, eastus2)                         │
│  NSG: permite :80, :443, :22 (SSH desde IP configurada)  │
│                                                           │
│  ┌────────────────────────────────────────────────────┐  │
│  │  Docker Compose (docker-compose.prod.yml)           │  │
│  │                                                    │  │
│  │  frontend (Nginx :80) ←── port 80:80              │  │
│  │       │ /api/*                                     │  │
│  │       ▼                                            │  │
│  │  gateway (YARP :8080) ← expose only               │  │
│  │       │                                            │  │
│  │  iam, devices, projects, subscriptions,            │  │
│  │  analytics, profiles (expose only, no ports)       │  │
│  │       │                                            │  │
│  │  mysql, influxdb, mosquitto, simulator             │  │
│  └────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────┘
```

**Diferencia con Iteración 1:** Traefik + Dokploy fueron reemplazados por Cloudflare como edge proxy. La VM expone solo el puerto 80 (frontend/Nginx) al host — el resto de los servicios no tiene `ports`, solo `expose` (accesibles únicamente dentro de la red Docker).

---

## Stack de Infraestructura

| Componente | Rol | Detalle |
|-----------|-----|---------|
| **Terraform** | IaC (Infrastructure as Code) | Crea/destruye VM, VNet, IP, NSG, NIC en Azure |
| **Azure Resource Group** | Contenedor lógico | `rg-iobuild-demo` (todo vive aquí, borrar el RG borra todo) |
| **Azure VM** | Compute | Ubuntu 22.04 LTS Gen2, `eastus2` |
| **cloud-init** | Bootstrap automático | Instala Docker, clona el repo, corre `docker compose` al primer arranque |
| **Cloudflare** | DNS + TLS + Edge Proxy | A record proxied, SSL Flexible, esconde IP real |
| **GitHub Actions** | CI / Build | Buildea imágenes Docker y las pushea a GHCR |
| **GHCR** | Container Registry | `ghcr.io/hallzyx/iobuild-*` (imágenes públicas) |
| **Azure Storage** | Estado de Terraform | `iobuildtfstate22892/tfstate/iobuild-demo.tfstate` |

---

## Ciclo de Vida del Deploy

### 1. Prerrequisitos (una sola vez)

```bash
# Autenticarse en Azure
az login
az account show   # verificar suscripción correcta

# Llenar credenciales
cp infra/terraform.tfvars.example infra/terraform.tfvars
# Editar: ssh_public_key, db_password, jwt_secret, stripe_*, influxdb_*, cloudflare_api_token
```

### 2. Construir y pushear imágenes (una vez por set de cambios)

Las imágenes se construyen en GitHub Actions. Disparar el workflow `Build and push images to GHCR` desde GitHub Actions — **no es necesario tener Docker local**.

```
GitHub Actions workflow:
  build → push → ghcr.io/hallzyx/iobuild-{iam,devices,...}:latest
```

### 3. Deploy

```bash
cd infra
./deploy.sh
# Internamente: terraform init → terraform apply
# Output: Demo URL + VM IP
```

Después del `apply`:
1. Azure crea el Resource Group, VNet, IP pública, NSG, NIC, VM
2. cloud-init arranca: instala Docker, clona el repo, escribe el `.env` con los secrets, corre `docker compose -f docker-compose.prod.yml pull && up`
3. La VM tarda ~2-3 minutos en estar lista (download de imágenes GHCR)
4. Cloudflare ya tiene el A record con la IP de la VM (Terraform lo crea automáticamente)

### 4. Verificar que está up

```bash
# Health check (reemplazar con el dominio real)
curl https://iobuild-v2.arroz.dev/health

# Esperar a que todos los servicios estén Healthy
# (mysql puede tardar ~30s extra en inicializar las DBs)
```

### 5. Destroy (apagar y no pagar más)

```bash
cd infra
./destroy.sh
# Internamente: terraform destroy
# Elimina: Resource Group completo (VM, IP, NSG, VNet, NIC)
# Elimina: Cloudflare A record
```

El Storage Account del tfstate (`rg-iobuild-tfstate`) **no se destruye** — persiste para que el próximo deploy/destroy comparta el mismo estado.

---

## Qué crea/destruye Terraform

| Recurso | Crea | Destruye |
|---------|------|---------|
| `rg-iobuild-demo` (Resource Group) | ✅ | ✅ |
| `iobuild-vnet` (10.10.0.0/16) | ✅ | ✅ |
| `iobuild-subnet` (10.10.1.0/24) | ✅ | ✅ |
| `iobuild-pip` (IP pública estática) | ✅ | ✅ |
| `iobuild-nsg` (NSG: 80, 443, 22) | ✅ | ✅ |
| `iobuild-nic` (NIC) | ✅ | ✅ |
| `iobuild-vm` (Linux VM Ubuntu 22.04) | ✅ | ✅ |
| Cloudflare A record `iobuild-v2.arroz.dev` | ✅ | ✅ |
| `rg-iobuild-tfstate` (Storage del state) | ❌ manual | ❌ manual |

---

## TLS — Cloudflare Flexible

La zona `arroz.dev` en Cloudflare está configurada con **SSL/TLS → Flexible**:

```
Usuario → HTTPS :443 → Cloudflare (termina TLS) → HTTP :80 → VM
```

El A record está **proxied** (nube naranja), lo que significa:
- Cloudflare sirve el certificado TLS al usuario
- La VM recibe tráfico HTTP plano (no necesita certificado propio)
- La IP real de la VM está oculta

**¿Por qué Flexible y no Full?** La VM no tiene certificado TLS. Flexible es el trade-off correcto para una demo efímera: HTTPS para el usuario, sin la complejidad de gestionar certificados dentro de la VM.

---

## Secrets — Cómo se inyectan

Los secrets se declaran en `terraform.tfvars` (no se commitean, está en `.gitignore`).

Terraform pasa los valores a `cloud-init.yaml` via `templatefile()`. cloud-init escribe un `.env` en la VM con los valores:

```bash
# .env generado en la VM por cloud-init
IMAGE_TAG=latest
DB_PASSWORD=****
JWT_SECRET=****
INFLUXDB_TOKEN=****
STRIPE_SECRET_KEY=****
STRIPE_PUBLISHABLE_KEY=****
STRIPE_WEBHOOK_SECRET=****
```

`docker-compose.prod.yml` lee este `.env` para inyectar variables en cada contenedor.

**Nunca hay secrets en el repositorio.** El `.env` solo existe en la VM mientras está viva.

---

## Comparativa: Iteración 1 vs Deploy Actual

| Aspecto | Iteración 1 (Dokploy) | Deploy Actual (Azure + Terraform) |
|---------|----------------------|----------------------------------|
| **Orquestador** | Dokploy (UI web) | Terraform (código declarativo) |
| **TLS** | Traefik + Let's Encrypt | Cloudflare (Flexible, edge) |
| **VM/Servidor** | VPS genérico | Azure VM (ephemeral, tagged) |
| **Deploy** | Push a rama → Dokploy autodeploy | `./deploy.sh` (local, con `az login`) |
| **Imágenes** | Build en VPS | Build en GitHub Actions → GHCR |
| **Ciclo de vida** | VM siempre prendida | Efímero: create/destroy por demo |
| **State management** | Ninguno (Dokploy stateful) | Terraform state en Azure Storage |
| **DNS** | Cloudflare A record manual | Terraform crea/destruye el record |

---

## Troubleshooting

### La URL da 502 justo después del deploy

Normal. La VM tarda ~2-3 minutos en descargar las imágenes GHCR y levantar todos los contenedores. Esperar y refrescar.

### Los servicios dan "Unhealthy" en el health check

MySQL inicializa las 6 bases de datos desde `init.sql` al primer arranque — esto toma ~30-60s. El resto de los servicios tienen `depends_on: mysql: condition: service_healthy` y esperan. Si pasados 2 minutos siguen unhealthy, revisar logs:

```bash
ssh azureuser@<vm-ip>
docker compose -f /opt/iobuild/repo/microservices/docker-compose.prod.yml logs mysql
```

### `compose up` falla con dependency timeout

cloud-init tiene un loop de hasta 6 reintentos con 45s entre intentos para manejar este caso exacto (health checks de mysql/iam tardan en pasar). Normalmente se resuelve solo.

### SSH a la VM

```bash
ssh -i ~/.ssh/<tu-llave-privada> azureuser@<vm-public-ip>
# La IP aparece en el output de deploy.sh o en: terraform output vm_public_ip
```

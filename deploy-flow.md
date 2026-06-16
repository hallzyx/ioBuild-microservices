# Deploy Flow — IoBuild en Azure (guía paso a paso)

> Documento de aprendizaje + log de lo que fuimos haciendo para desplegar
> `microservices/` en una VM efímera de Azure, con dominio en Cloudflare,
> manejado desde GitHub Actions (un botón para crear, otro para destruir).
>
> Léelo de arriba a abajo: primero los conceptos, después el log real fase por fase.

---

## El concepto general (qué estamos haciendo y por qué)

Queremos prender el proyecto en internet para una demo, mostrarlo en un dominio
real, y después **apagarlo y borrarlo** para que Azure deje de cobrar. La clave
es separar DOS cosas que la gente suele confundir:

1. **La infraestructura** (la máquina virtual, su IP, el dominio). Esto se
   **crea y se destruye**. La herramienta para esto es **Terraform**.
2. **La aplicación** (los contenedores Docker). Esto corre *adentro* de la VM.

Terraform es como un plano de construcción declarativo: vos describís "quiero una
VM con esta IP y este dominio", corrés `apply` y lo construye; corrés `destroy` y
lo demuele. Por eso es ideal para algo efímero: lo levantás para la demo y lo
tirás abajo después.

### Glosario rápido (términos que vas a ver)

| Término | Qué es, en criollo |
|---------|--------------------|
| **Suscripción** | Tu "cuenta de facturación" en Azure. Vos usás "Azure for Students". |
| **Resource Group (RG)** | Una carpeta que agrupa recursos. Borrar el RG borra todo lo de adentro. |
| **VM** | La máquina virtual: un servidor Linux en la nube donde corre Docker. |
| **IP pública** | La dirección de internet por la que se llega a la VM. |
| **NSG** | Network Security Group: el firewall de la VM (qué puertos se abren). |
| **Storage Account** | Un "disco" en la nube. Lo usamos para guardar el estado de Terraform. |
| **tfstate** | El archivo donde Terraform anota qué creó. Sin esto, no sabe qué destruir. |
| **Service Principal (SP)** | Un "usuario robot" sin persona detrás. GitHub Actions lo usa para entrar a tu Azure. |
| **RBAC / rol Contributor** | Permisos. "Contributor" = puede crear/borrar recursos, pero no dar permisos a otros. |
| **GitHub Secret** | Una clave guardada cifrada en GitHub, que los workflows leen sin exponerla. |
| **Cloudflare proxy** | Cloudflare se pone delante de tu VM: da HTTPS y esconde la IP real. |
| **cloud-init** | Un script que corre la PRIMERA vez que la VM arranca (instala Docker, levanta la app). |

---

## El mapa de fases

| Fase | Qué hace | Estado |
|------|----------|--------|
| 1. Llave SSH | Crear la llave para entrar a la VM si hace falta | ✅ |
| 2. Storage del state | Crear la "cajita" donde vive el tfstate | ✅ |
| 3. Service Principal | Crear el usuario robot para GitHub Actions | ❌ Bloqueado por UPC → pivote a Terraform local |
| 4. Llenar terraform.tfvars + token Cloudflare | Cargar claves localmente | ⏳ (tu turno) |
| 5. Cloudflare SSL | Poner el modo TLS en "Flexible" | ⏳ (tu turno) |
| 6. Build imágenes + deploy.sh / destroy.sh | Buildear y levantar/destruir | ⏳ |

---

## Log fase por fase

### Fase 1 — Llave SSH ✅

**Comando ejecutado:**
```bash
ssh-keygen -t ed25519 -f ~/.ssh/iobuild_demo -N "" -C "iobuild-demo"
```

**Resultado:** se generaron dos archivos en `~/.ssh/`:
- `iobuild_demo` → llave **privada**. Se queda en tu máquina, nunca se sube a ningún lado.
- `iobuild_demo.pub` → llave **pública**. Esta es la que cargamos como GitHub Secret `SSH_PUBLIC_KEY`, y Terraform la mete en la VM.

**Concepto:** SSH usa criptografía de clave pública. La VM guarda tu llave pública;
solo quien tenga la privada correspondiente puede entrar. Por eso la privada es
secreta y la pública se puede repartir sin riesgo. Para conectarte a la VM (más
adelante, si hace falta debuggear) usarás:
```bash
ssh -i ~/.ssh/iobuild_demo azureuser@<IP-de-la-VM>
```

---

### Fase 2 — Storage para el tfstate ✅

**Lo que creamos:**
- Resource Group `rg-iobuild-tfstate` (región `eastus2`)
- Storage Account `iobuildtfstate22892` (nombre único global)
- Container `tfstate` dentro del storage

**⚠️ Gotcha grande que aprendimos:** "Azure for Students" tiene una política
llamada **"Allowed resource deployment regions"** que SOLO deja crear recursos en:
`westus3`, `brazilsouth`, `northcentralus`, `eastus2`, `chilecentral`.

El primer intento en `eastus` falló con `RequestDisallowedByAzure`. Cómo se
diagnostica:
```bash
az policy assignment list --query "[].parameters" -o json | rg -i listOfAllowed -A6
```

**Por qué elegimos `eastus2`:** en cuentas de estudiante la capacidad de VMs es
compartida y limitada; las regiones grandes (como eastus2) tienen mucha más
disponibilidad de máquinas libres y precios más bajos. Verificamos que el tamaño
`Standard_B2ms` exista ahí antes de comprometernos:
```bash
az vm list-skus -l eastus2 --size Standard_B2ms --query "[].restrictions"
```

**Importante:** esta región tiene que ser la misma en Terraform. Por eso cambiamos
el default de la variable `location` en `infra/variables.tf` de `eastus` a `eastus2`.

**Concepto — tfstate:** Terraform guarda en este archivo el "antes y después" de
tu infra. Cuando corrés `apply`, compara lo que querés con lo que ya existe (según
el state) y hace solo la diferencia. Cuando corrés `destroy`, lee el state para
saber exactamente qué borrar. Guardarlo en un storage central (no en tu disco)
permite que el workflow de Deploy y el de Destroy, que son ejecuciones separadas,
compartan la misma memoria.

**Datos del backend (ya cableados en `deploy.sh`/`destroy.sh`):**
- `resource_group_name` = `rg-iobuild-tfstate`
- `storage_account_name` = `iobuildtfstate22892`
- `container_name` = `tfstate`

---

### Fase 3 — Service Principal ❌ BLOQUEADO (y por qué importa entenderlo)

**Comando intentado:**
```bash
az ad sp create-for-rbac --name iobuild-demo-sp --role Contributor \
  --scopes /subscriptions/<sub-id> --sdk-auth
```

**Resultado:** falló con
`Insufficient privileges... Directory permission is needed to register the application`.

**Concepto clave — Suscripción vs Directorio (Entra ID):** en Azure hay dos planos
de control distintos:
- La **suscripción** = dónde viven los recursos y la facturación. Sos dueño de la tuya
  (por eso podés crear VMs, storage, etc.).
- El **directorio (Entra ID)** = el registro de identidades (usuarios, apps, robots).
  El tuyo lo administra la **UPC**, y tienen apagada la opción de que los alumnos
  registren aplicaciones.

Un **Service Principal** es una identidad de app → vive en el directorio → necesitás
permiso del directorio para crearlo. No lo tenés. No es un error tuyo, es política
del tenant de la universidad.

**El pivote — Terraform local:** en vez de un robot, Terraform usa **tu propio login
de `az`**. El provider `azurerm` detecta tu sesión activa de Azure CLI y se autentica
con ella. Resultado:
- Ya NO necesitamos Service Principal ni GitHub Secrets de Azure.
- El deploy/destroy se corre desde tu máquina con `infra/deploy.sh` y `infra/destroy.sh`.
- El build de imágenes SÍ sigue en GitHub Actions (workflow `Build and push images`),
  porque eso solo usa el token de GitHub, no toca Azure.

**Archivos que cambiaron por el pivote:**
- `.github/workflows/deploy.yml` y `destroy.yml` → eliminados (hacían Terraform en Actions).
- `.github/workflows/build-images.yml` → nuevo, solo buildea+pushea a GHCR.
- `infra/deploy.sh`, `infra/destroy.sh` → nuevos, corren Terraform local.
- `infra/variables.tf` → región default `eastus2`.

---

### Fase 4 — Llenar `terraform.tfvars` + token de Cloudflare ⏳ (tu turno)

Ya te dejé `infra/terraform.tfvars` con la llave SSH y tu IP puestas. Te faltan:

**a) Token de Cloudflare.** Es la credencial para que Terraform cree el registro DNS.
Cómo sacarlo:
1. Cloudflare dashboard → ícono de perfil (arriba a la derecha) → **My Profile** → **API Tokens**.
2. **Create Token** → plantilla **Edit zone DNS**.
3. En "Zone Resources" elegí **Specific zone → arroz.dev**.
4. Create → copiá el token (se muestra UNA sola vez).
5. Pegalo en `terraform.tfvars` en `cloudflare_api_token`.

**Concepto:** un API token de Cloudflare es como una llave con permisos acotados.
Esta solo puede editar DNS de `arroz.dev`, nada más. Si se filtra, el daño es mínimo.

**b) Secretos de la app.** Copiá desde tu `microservices/.env` a `terraform.tfvars`:
`db_password`, `jwt_secret`, `influxdb_token`, `influxdb_password`, y los tres `stripe_*`.

**Por qué acá y no en el repo:** `terraform.tfvars` está en `.gitignore` → nunca se
sube a GitHub. Terraform lo lee solo en tu máquina y mete esos valores en el `.env`
que cloud-init escribe dentro de la VM.

---

### Fase 5 — SSL: Configuration Rule (Flexible solo para iobuild-v2) ⏳ (tu turno)

**Contexto importante (lo descubrimos en vivo):** la zona `arroz.dev` está
COMPARTIDA — 20 registros DNS, todos apuntando al VPS de Dokploy (`167.86.75.9`):
arquitory, knb, n8n, sparkhub, vehiflow, licita-verify, chacra-chain, etc. La zona
está en modo **Full** (Cloudflare habla HTTPS con los orígenes; el VPS sirve HTTPS
vía Traefik). Por eso **NO se puede cambiar el modo SSL global** — romperíamos esos
~19 servicios.

**Sobre el TLD `.dev` (verificado):** `.dev` está en la HSTS preload list con
`include_subdomains`, así que los navegadores fuerzan HTTPS en TODO `*.dev`. PERO eso
se cumple en el **edge de Cloudflare** (que sirve cert válido), no en la VM. El tramo
Cloudflare→VM es invisible para el navegador. Por eso Flexible es suficiente, siempre
que el registro esté **proxied** (lo está).
Fuentes: https://ma.ttias.be/chrome-force-dev-domains-https-via-preloaded-hsts/ ,
https://developers.cloudflare.com/ssl/origin-configuration/ssl-modes/

**Solución — override por-host con una Configuration Rule** (Free incluye 10):
1. `arroz.dev` → **Rules** → **Configuration Rules** → **Create rule**
2. Nombre: `iobuild-v2 flexible SSL`
3. When incoming requests match: Field=**Hostname**, Operator=**equals**,
   Value=**`iobuild-v2.arroz.dev`**
4. Then → Settings: activar **SSL** → **Flexible**
5. **Deploy**

La zona queda en Full para todos; solo `iobuild-v2` usa Flexible.

**Concepto — modos SSL (tramo Cloudflare → origen):**
- **Flexible**: CF→origen por HTTP. La VM solo necesita puerto 80. Lo que usamos.
- **Full**: CF→origen por HTTPS; el origen necesita certificado.
- **Full (strict)**: como Full pero con cert válido/confiable.

**Trade-off:** con Flexible, el tramo CF→VM de ESE host va en texto plano. Aceptable
para un demo efímero. El plan B "correcto" sería un Origin Certificate de Cloudflare
en la VM + Full, pero implica meterle TLS al stack (no vale la pena para esto).

---

### Fase 6 — Build + Deploy + Destroy ⏳

**Gotcha de GitHub Actions:** un workflow con `workflow_dispatch` solo se puede
DISPARAR si el archivo existe en la rama **default** (`main`). Por eso llevamos
`build-images.yml` a `main` (solo ese archivo + el fix del `.gitignore`), mientras
el infra de Terraform sigue sin tocar `main`. Se dispara con:
```bash
gh workflow run build-images.yml --ref main
```

**1) Buildear imágenes (una vez, o cada vez que cambie el código):**
- GitHub → tu repo → pestaña **Actions** → workflow **Build and push images to GHCR** → **Run workflow**.
- Cuando termine, andá a tu perfil de GitHub → **Packages** y marcá cada paquete
  `iobuild-*` como **Public** (Package settings → Danger Zone → Change visibility → Public).
  Esto permite que la VM los baje sin credenciales.
  - URL directa por paquete: `https://github.com/users/hallzyx/packages/container/iobuild-<nombre>/settings`
  - Nota: NO se puede automatizar — GitHub solo permite cambiar visibilidad de
    paquetes de usuario por la web, y el token de `gh` necesitaría scope `write:packages`.
  - Los 9: iam, devices, projects, subscriptions, analytics, profiles, gateway, frontend, simulator.

**2) Deploy (desde tu terminal):**
```bash
cd infra
./deploy.sh
```
Te imprime la URL y la IP. La VM tarda ~2-3 min en bajar imágenes y arrancar.

**3) Probar:** abrí `https://iobuild-v2.arroz.dev`.

**4) Destroy (cuando termines la demo):**
```bash
cd infra
./destroy.sh
```
Borra la VM, la IP y el DNS. Azure deja de cobrar el cómputo.

---

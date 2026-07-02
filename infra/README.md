# infra/ — Demo efímera de Azure para IoBuild (Terraform local)

`./deploy.sh` crea una VM descartable corriendo el stack completo en
`https://iobuild-v2.arroz.dev`. `./destroy.sh` la elimina para que Azure deje
de facturar. Terraform corre **localmente** usando tu propio `az login` (el
tenant de la UPC bloquea service principals, por lo que GitHub Actions no
puede autenticarse contra Azure).
El build de imágenes sigue haciéndose en GitHub Actions (`Build and push images to GHCR`).

## Prerrequisitos

- `az login` activo (`az account show` debe mostrar tu suscripción).
- Terraform instalado (`terraform -version`).
- Imágenes publicadas en GHCR (correr una vez el workflow de GitHub Actions "Build and push images").
- `terraform.tfvars` completado con valores reales (ver `terraform.tfvars.example`).

## Bootstrap único: almacenamiento remoto del estado (ya hecho)

El estado de Terraform vive en un blob de Azure Storage para que `deploy` y
`destroy` compartan la misma memoria. Se creó una vez con:

```bash
az group create -n rg-iobuild-tfstate -l eastus2
az storage account create -n <nombre-unico> -g rg-iobuild-tfstate -l eastus2 --sku Standard_LRS
KEY=$(az storage account keys list -g rg-iobuild-tfstate -n <nombre-unico> --query '[0].value' -o tsv)
az storage container create -n tfstate --account-name <nombre-unico> --account-key "$KEY"
```

Valores actuales (ya cableados en `deploy.sh` / `destroy.sh`):
- `resource_group_name = rg-iobuild-tfstate`
- `storage_account_name = iobuildtfstate22892`
- `container_name = tfstate`
- `key = iobuild-demo.tfstate`

## Uso

```bash
cd infra
./deploy.sh     # terraform init + apply → VM + DNS + app
# ... corré tu demo ...
./destroy.sh    # terraform destroy → deja de facturar
```

## TLS — Cloudflare Flexible

El registro A está proxied (nube naranja). Configurá el modo SSL/TLS de la
zona en **Flexible** una vez en el dashboard de Cloudflare para `arroz.dev`
(SSL/TLS → Overview → Flexible). Cloudflare sirve HTTPS en el edge y llega a
la VM por HTTP en el puerto 80.

## Qué se crea / se destruye

- Azure: resource group `rg-iobuild-demo`, VNet, subnet, IP pública, NSG, NIC, VM (`eastus2`).
- Cloudflare: registro A `iobuild-v2.arroz.dev` (proxied).

`destroy` elimina todo lo anterior. El storage account del tfstate persiste
(borrar `rg-iobuild-tfstate` manualmente al retirar el proyecto).

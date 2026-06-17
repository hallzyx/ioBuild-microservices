# infra/ — Ephemeral Azure demo for IoBuild (local Terraform)

`./deploy.sh` creates a throwaway VM running the full stack at
`https://iobuild-v2.arroz.dev`. `./destroy.sh` removes it so Azure billing
stops. Terraform runs **locally** using your own `az login` (the UPC tenant
blocks service principals, so GitHub Actions cannot authenticate to Azure).
Image building still happens in GitHub Actions (`Build and push images to GHCR`).

## Prerequisites

- `az login` active (`az account show` should print your subscription).
- Terraform installed (`terraform -version`).
- Images pushed to GHCR (run the GitHub Actions "Build and push images" workflow once).
- `terraform.tfvars` filled with real values (see `terraform.tfvars.example`).

## One-time bootstrap: remote state storage (already done)

The Terraform state lives in an Azure Storage blob so deploy and destroy share
the same memory. Created once with:

```bash
az group create -n rg-iobuild-tfstate -l eastus2
az storage account create -n <unique-name> -g rg-iobuild-tfstate -l eastus2 --sku Standard_LRS
KEY=$(az storage account keys list -g rg-iobuild-tfstate -n <unique-name> --query '[0].value' -o tsv)
az storage container create -n tfstate --account-name <unique-name> --account-key "$KEY"
```

Current values (wired into `deploy.sh` / `destroy.sh`):
- `resource_group_name = rg-iobuild-tfstate`
- `storage_account_name = iobuildtfstate22892`
- `container_name = tfstate`
- `key = iobuild-demo.tfstate`

## Usage

```bash
cd infra
./deploy.sh     # terraform init + apply → VM + DNS + app
# ... run your demo ...
./destroy.sh    # terraform destroy → billing stops
```

## TLS — Cloudflare Flexible

The A record is proxied (orange cloud). Set the zone SSL/TLS mode to
**Flexible** once in the Cloudflare dashboard for `arroz.dev`
(SSL/TLS → Overview → Flexible). Cloudflare serves HTTPS at the edge and
reaches the VM over HTTP on port 80.

## What gets created / destroyed

- Azure: resource group `rg-iobuild-demo`, VNet, subnet, public IP, NSG, NIC, VM (`eastus2`).
- Cloudflare: A record `iobuild-v2.arroz.dev` (proxied).

`destroy` removes all of the above. The tfstate storage account persists
(delete `rg-iobuild-tfstate` manually when retiring the project).

#!/usr/bin/env bash
# Deploy the IoBuild demo to Azure using your local `az` login.
# Prereqs: `az login` active, terraform.tfvars filled in, images pushed to GHCR.
set -euo pipefail
cd "$(dirname "$0")"

# Remote state backend (created once via az — see README.md)
TFSTATE_RG="rg-iobuild-tfstate"
TFSTATE_SA="iobuildtfstate22892"
TFSTATE_CONTAINER="tfstate"

echo "==> terraform init"
terraform init -input=false \
  -backend-config="resource_group_name=${TFSTATE_RG}" \
  -backend-config="storage_account_name=${TFSTATE_SA}" \
  -backend-config="container_name=${TFSTATE_CONTAINER}" \
  -backend-config="key=iobuild-demo.tfstate"

echo "==> terraform apply"
terraform apply -auto-approve

echo
echo "=================================================="
echo " Demo URL: $(terraform output -raw demo_url)"
echo " VM IP:    $(terraform output -raw vm_public_ip)"
echo "=================================================="
echo "La VM tarda ~2-3 min en bajar las imágenes y levantar todo."
echo "Si la URL da 502 al toque, esperá a que termine docker compose."

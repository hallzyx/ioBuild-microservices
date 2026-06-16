#!/usr/bin/env bash
# Destroy the IoBuild demo from Azure so billing stops.
# Prereqs: `az login` active.
set -euo pipefail
cd "$(dirname "$0")"

TFSTATE_RG="rg-iobuild-tfstate"
TFSTATE_SA="iobuildtfstate22892"
TFSTATE_CONTAINER="tfstate"

echo "==> terraform init"
terraform init -input=false \
  -backend-config="resource_group_name=${TFSTATE_RG}" \
  -backend-config="storage_account_name=${TFSTATE_SA}" \
  -backend-config="container_name=${TFSTATE_CONTAINER}" \
  -backend-config="key=iobuild-demo.tfstate"

echo "==> terraform destroy"
terraform destroy -auto-approve

echo
echo "Listo. La VM, su IP y el registro DNS fueron eliminados."
echo "Azure dejó de cobrar por el cómputo. El storage del state queda (centavos)."

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

## TLS — Cloudflare Flexible

The A record is proxied (orange cloud). Set the zone SSL/TLS mode to
**Flexible** once in the Cloudflare dashboard for `arroz.dev`
(SSL/TLS → Overview → Flexible). Cloudflare serves HTTPS at the edge and
reaches the VM over HTTP on port 80.

## What gets created / destroyed

- Azure: resource group `rg-iobuild-demo`, VNet, subnet, public IP, NSG, NIC, VM.
- Cloudflare: A record `iobuild-v2.arroz.dev` (proxied).

`destroy` removes all of the above. The tfstate storage account persists
(delete `rg-iobuild-tfstate` manually when retiring the project).

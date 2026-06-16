# Ephemeral Azure Demo Deploy — Design

**Date:** 2026-06-16
**Status:** Approved (design phase)
**Scope:** Deploy `microservices/` (IoBuild stack) to a throwaway Azure VM for live demos, with one-click deploy and one-click destroy via GitHub Actions, served at `iobuild-v2.arroz.dev`.

## Goal

Spin up the full IoBuild stack on an Azure VM with a single click, run a live demo at a real domain, then tear everything down with another single click so Azure billing stops. No existing project file is modified — all artifacts are new and isolated.

## Non-Goals

- Not a production-grade, highly-available deployment. This is an ephemeral demo environment.
- No persistent data across deploy/destroy cycles (MySQL/InfluxDB volumes die with the VM).
- No Dokploy. The VM lifecycle (create/destroy) is the whole point, and Dokploy does not manage that — it assumes a long-lived server, which keeps billing the clock running.

## Why Terraform over Dokploy

Two separate concerns are being conflated:

1. **Infra lifecycle** — create VM + DNS, then destroy them. Must be ephemeral. This is Terraform's core competency: `apply` = up, `destroy` = down.
2. **App deployment** — build images, `docker compose up`. Runs inside the VM via cloud-init.

Dokploy only addresses concern 2 on a server you already keep running. It does not create or destroy the cloud VM, so it does not stop billing. Terraform addresses both concerns in one lifecycle, and via the Cloudflare provider the DNS record is created and destroyed alongside the VM — one source of truth.

## Architecture

### Two-button flow (GitHub Actions `workflow_dispatch`)

```
[Deploy ▶]
  Job A (build):  docker buildx → push 8 images to GHCR (public)
  Job B (infra):  terraform apply
                  → Azure RG + VM + public IP + NSG
                  → Cloudflare A record iobuild-v2.arroz.dev → VM IP (proxied)
                  → cloud-init installs Docker, writes .env, docker compose up

[Destroy ▶]
  terraform destroy → removes VM, IP, NSG, and the Cloudflare DNS record
                      → Azure billing stops
```

### Single entry point

The frontend Nginx (`microservices/frontend-docker/nginx.conf`) already:
- proxies `/api/` → `gateway:8080`
- proxies `/health` → `gateway:8080`
- serves the Vue SPA with router fallback

So `frontend:80` is the only port that must be reachable from outside. No extra reverse proxy (Traefik/Caddy) is needed. Cloudflare terminates TLS at the edge and forwards to the VM on port 80.

## New Artifacts (nothing existing is modified)

```
infra/
  main.tf            # providers (azurerm, cloudflare), backend config
  vm.tf              # RG, VM (Standard_B2ms, Ubuntu 22.04), public IP, NSG
  dns.tf             # Cloudflare A record iobuild-v2.arroz.dev → VM IP, proxied
  cloud-init.yaml    # installs Docker, clones repo, writes .env, runs compose
  variables.tf       # subscription, secrets, domain, ssh allow-IP
  outputs.tf         # public IP, URL
  backend bootstrap  # one-time storage account for remote tfstate (documented, run once)

docker-compose.prod.yml   # twin of docker-compose.yml, uses image: ghcr.io/... (no build:)

.github/workflows/
  deploy.yml         # build+push to GHCR, then terraform apply
  destroy.yml        # terraform destroy
```

### Files used but NOT modified

- `microservices/docker-compose.yml` — stays as the local-dev file.
- `microservices/src/**/Dockerfile`, `frontend-docker/Dockerfile`, `iot-simulator/Dockerfile` — CI builds them as-is.
- `microservices/frontend-docker/nginx.conf` — already routes `/api`, no change.
- `microservices/.env` — not touched; the VM's `.env` is generated from GitHub Secrets.

Rationale: dev compose and prod deploy have different lifecycles. Keeping them in separate files avoids conditional `build`/`image` logic and guarantees local behavior never changes due to demo work. The duplicated `docker-compose.prod.yml` is healthy duplication — two small clear files beat one large conditional one.

## Component Details

### docker-compose.prod.yml

Identical service topology to `docker-compose.yml` except the 8 built services replace `build:` with:

```yaml
image: ghcr.io/hallzyx/iobuild-<service>:${IMAGE_TAG:-latest}
```

Services and images (owner `hallzyx`, repo `ioBuild-microservices`):

| Service        | Image                                   |
|----------------|-----------------------------------------|
| iam            | ghcr.io/hallzyx/iobuild-iam             |
| devices        | ghcr.io/hallzyx/iobuild-devices         |
| projects       | ghcr.io/hallzyx/iobuild-projects        |
| subscriptions  | ghcr.io/hallzyx/iobuild-subscriptions   |
| analytics      | ghcr.io/hallzyx/iobuild-analytics       |
| profiles       | ghcr.io/hallzyx/iobuild-profiles        |
| gateway        | ghcr.io/hallzyx/iobuild-gateway         |
| frontend       | ghcr.io/hallzyx/iobuild-frontend        |

Pulled images (`mysql:8.0`, `eclipse-mosquitto`, `influxdb`) stay as-is. `simulator` is also built/pushed (small Python image): `ghcr.io/hallzyx/iobuild-simulator`.

The `frontend` is built with `VITE_*` build args in CI (baked at build time), so the deployed image already contains the correct frontend config.

GHCR packages are **public** (repo is public), so the VM pulls without credentials.

### Terraform — Azure (vm.tf)

- Resource Group (e.g. `rg-iobuild-demo`), tagged for easy identification.
- VM: `Standard_B2ms` (2 vCPU / 8 GB) — sized for running ~12 containers comfortably; build no longer happens on the VM, so this is for runtime only.
- Ubuntu 22.04 LTS, SSH key auth.
- Public IP (Standard, static for the VM's life).
- NSG rules:
  - 80/tcp from anywhere (Cloudflare reaches it).
  - 443/tcp from anywhere (reserved; Flexible mode hits 80, but open for future Full mode).
  - 22/tcp restricted to the operator's IP (variable `ssh_allow_cidr`).

### Terraform — Cloudflare (dns.tf)

- A record `iobuild-v2.arroz.dev` → VM public IP, `proxied = true` (orange cloud).
- Zone `arroz.dev` (wildcard `*.arroz.dev` already exists; this is a specific record that takes precedence).
- Created and destroyed within the same Terraform lifecycle.

### TLS — Cloudflare Flexible

- SSL/TLS mode: **Flexible**. Cloudflare serves HTTPS at the edge; Cloudflare → origin is HTTP on port 80.
- Zero certificates on the VM; works the instant DNS propagates.
- Trade-off: the Cloudflare→VM leg is plaintext. Acceptable for an ephemeral demo. Upgrading to **Full** later means adding a Cloudflare Origin Certificate + a TLS-terminating proxy on the VM — out of scope now.
- Stripe webhook: reachable at `https://iobuild-v2.arroz.dev/api/...` via the edge HTTPS. (Webhook secret comes from GitHub Secrets.)

### cloud-init (cloud-init.yaml)

On first boot:
1. Install Docker Engine + compose plugin.
2. `git clone https://github.com/hallzyx/ioBuild-microservices.git` (public) for config files only: `microservices/docker-compose.prod.yml`, `microservices/mosquitto/`, `microservices/mysql/init.sql`.
3. Write `microservices/.env` from values templated by Terraform (sourced from GitHub Secrets).
4. `docker compose -f docker-compose.prod.yml pull && docker compose -f docker-compose.prod.yml up -d`.

Expected cold start: ~2–3 min (pull + start), since no image build happens on the VM.

### Remote state

- Terraform backend: Azure Storage (blob). A small storage account + container created **once** as a bootstrap step (documented in `infra/`), costs cents/month.
- Required because deploy and destroy run as separate workflow invocations; without persistent state, `destroy` would not know what to remove.

### Secrets (GitHub repository secrets)

| Secret                     | Used by                          |
|----------------------------|----------------------------------|
| `AZURE_CREDENTIALS`        | Terraform azurerm auth (SP)      |
| `CLOUDFLARE_API_TOKEN`     | Terraform cloudflare provider    |
| `SSH_PUBLIC_KEY`           | VM admin key                     |
| `SSH_ALLOW_CIDR`           | NSG rule for port 22             |
| `DB_PASSWORD`              | MySQL + services                 |
| `JWT_SECRET`               | services                         |
| `INFLUXDB_TOKEN`, `INFLUXDB_PASSWORD` | influxdb + devices    |
| `STRIPE_SECRET_KEY`, `STRIPE_PUBLISHABLE_KEY`, `STRIPE_WEBHOOK_SECRET` | subscriptions |
| `VITE_CLOUDINARY_CLOUD_NAME`, `VITE_CLOUDINARY_UPLOAD_PRESET` | frontend build args (CI) |

## Cost

- VM billed per hour. A few hours of demo ≈ a couple of USD.
- `destroy` removes the VM, IP, and NSG → compute billing stops.
- Only the tfstate storage account persists (cents/month). It can be deleted manually when the project is fully retired.

## Error Handling / Operational Notes

- If `terraform apply` fails mid-run, state records what was created; re-running `destroy` cleans partial resources.
- If GHCR push fails, `deploy` stops before `apply` (build job gates the infra job).
- DNS propagation via Cloudflare proxy is near-instant; if the demo URL 502s right after deploy, it is usually cloud-init still pulling images — wait for compose to finish.
- Health: each service has a healthcheck; `gateway` waits on dependencies. The `/health` path is proxied for a quick liveness check.

## Testing / Validation

- `terraform validate` + `terraform plan` in CI before `apply` (catches config errors).
- A post-deploy smoke step in `deploy.yml`: curl `https://iobuild-v2.arroz.dev/health` until 200 (with timeout) to confirm the stack is live before declaring success.
- Manual: open the URL, log in, exercise one device/telemetry flow during the demo.

## Open Questions

None blocking. Future enhancements (out of scope): Full TLS with origin cert, persistent data via managed DB, auto-destroy timer.

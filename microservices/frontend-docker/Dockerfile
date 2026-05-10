# IoBuild Frontend — Dockerfile (build auto-contenido)
# Build context: . (este directorio contiene la copia del source + nginx.conf)

# ── Stage 1: Build ──
FROM node:20-alpine AS build
WORKDIR /app

# Source completo (copia independiente del monolite original)
COPY IoBuild-Frontend/package*.json ./
RUN npm ci

COPY IoBuild-Frontend/ .

RUN npm run build

# ── Stage 2: Runtime (Nginx) ──
FROM nginx:alpine
RUN apk add --no-cache curl
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

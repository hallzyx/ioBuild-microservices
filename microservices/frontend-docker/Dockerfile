# IoBuild Frontend — Dockerfile (build auto-contenido)
# Build context: . (este directorio contiene la copia del source + nginx.conf)

# ── Build args (inyectados desde docker-compose o .env) ──
ARG VITE_CLOUDINARY_CLOUD_NAME
ARG VITE_CLOUDINARY_UPLOAD_PRESET

# ── Stage 1: Build ──
FROM node:20-alpine AS build
WORKDIR /app

# Source completo (copia independiente del monolite original)
COPY IoBuild-Frontend/package*.json ./
RUN npm ci

COPY IoBuild-Frontend/ .

# Inyectar Cloudinary vars desde build args al .env.production
RUN if [ -n "$VITE_CLOUDINARY_CLOUD_NAME" ]; then \
      echo "VITE_CLOUDINARY_CLOUD_NAME=$VITE_CLOUDINARY_CLOUD_NAME" >> .env.production; \
    fi && \
    if [ -n "$VITE_CLOUDINARY_UPLOAD_PRESET" ]; then \
      echo "VITE_CLOUDINARY_UPLOAD_PRESET=$VITE_CLOUDINARY_UPLOAD_PRESET" >> .env.production; \
    fi

RUN npm run build

# ── Stage 2: Runtime (Nginx) ──
FROM nginx:alpine
RUN apk add --no-cache curl
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

# IoBuild Frontend — Dockerfile (build auto-contenido)
# Build context: . (este directorio contiene la copia del source + nginx.conf)

# ── Stage 1: Build ──
FROM node:20-alpine AS build
WORKDIR /app

# Build args must be re-declared inside the stage to be usable here; an ARG
# declared before FROM lives outside the build stage and is empty in RUN steps.
ARG VITE_CLOUDINARY_CLOUD_NAME
ARG VITE_CLOUDINARY_UPLOAD_PRESET

# Source completo (copia independiente del monolite original)
COPY IoBuild-Frontend/package*.json ./
RUN npm ci

COPY IoBuild-Frontend/ .

# Inyectar Cloudinary vars desde build args. Se escribe en .env.production.local,
# que en Vite tiene mayor precedencia que .env.production, para que estos valores
# ganen sobre cualquier placeholder commiteado en el repo.
RUN if [ -n "$VITE_CLOUDINARY_CLOUD_NAME" ]; then \
      echo "VITE_CLOUDINARY_CLOUD_NAME=$VITE_CLOUDINARY_CLOUD_NAME" >> .env.production.local; \
    fi && \
    if [ -n "$VITE_CLOUDINARY_UPLOAD_PRESET" ]; then \
      echo "VITE_CLOUDINARY_UPLOAD_PRESET=$VITE_CLOUDINARY_UPLOAD_PRESET" >> .env.production.local; \
    fi

RUN npm run build

# ── Stage 2: Runtime (Nginx) ──
FROM nginx:alpine
RUN apk add --no-cache curl
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

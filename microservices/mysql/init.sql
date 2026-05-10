-- IoBuild Microservices — MySQL Init Script
-- Este script se ejecuta automáticamente al iniciar el contenedor MySQL
-- Crea las 5 bases de datos independientes (una por microservicio)

CREATE DATABASE IF NOT EXISTS iobuild_iam CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE DATABASE IF NOT EXISTS iobuild_devices CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE DATABASE IF NOT EXISTS iobuild_projects CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE DATABASE IF NOT EXISTS iobuild_subscriptions CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE DATABASE IF NOT EXISTS iobuild_analytics CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

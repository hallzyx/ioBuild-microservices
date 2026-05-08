# Contexto Histórico y Estrategia de Refactorización - Proyecto IoBuild

**Instrucción para el Agente:** 
Utiliza esta narrativa histórica como base fáctica y obligatoria para redactar los puntos **5.1.3 (Pattern Based Custom Software Library)** y especialmente el **5.1.4 (Framework Pattern Driven Refactoring Report)**. Este es el "Antes y Después" real del proyecto en .NET.

## 1. El "Antes" (Estado Inicial / Monolito Legado)
- **Origen:** El proyecto IoBuild se heredó del curso anterior ("Aplicaciones Web") estructurado como un monolito tradicional en .NET.
- **Problemas Arquitectónicos:**
  - Todo el código (manejo de usuarios, registro de dispositivos IoT, control de proyectos) residía en un único proyecto de Web API.
  - Alto acoplamiento: Los controladores dependían directamente de las clases de acceso a datos (`DbContext`).
  - No existía separación de Bounded Contexts, lo que impedía escalar los módulos críticos (como la ingesta de telemetría IoT) de forma independiente.

## 2. El "Después" (Refactorización hacia Microservicios)
Para cumplir con los drivers arquitectónicos diseñados (ADD v3), el monolito fue deconstruido en la siguiente estructura de microservicios:

### Extracción de Librería Compartida (Aplica para 5.1.3)
- Se creó el proyecto `IoBuild.Shared` (Class Library).
- **Propósito:** Evitar la duplicación de código en la nueva arquitectura distribuida.
- **Contenido centralizado aquí:** 
  - Configuraciones base de JWT y utilitarios de seguridad.
  - Middleware de manejo global de excepciones (`GlobalExceptionHandlerMiddleware`) implementando el patrón Decorator.
  - DTOs base y enums compartidos.

### Segregación de Microservicios (Aplica para 5.1.4 y 5.1.2)
El monolito se dividió en proyectos API independientes que aplican patrones GoF y principios SOLID:
1. `IoBuild.IAM.Api`: Aisló toda la lógica de autenticación, roles y validación de usuarios.
2. `IoBuild.Devices.Api`: Aisló la gestión de hardware y preparación para telemetría.
3. `IoBuild.Projects.Api`: Aisló la gestión de clientes (constructores) y proyectos de infraestructura.
- **Patrones inyectados en la refactorización:**
  - **Inyección de Dependencias (Singleton/Scoped):** En `Program.cs` se eliminó el acoplamiento duro.
  - **Repository & Facade (Service Layer):** Se separó la lógica de negocio de los controladores, creando una capa de servicios (`IAuthService`, `IDeviceService`) que actúan como fachada antes de tocar Entity Framework Core.

## 3. Implementación de Calidad (Aplica para 5.1.1)
- Como parte de la transición, se introdujo una suite de pruebas estructurada (xUnit/NUnit) que no existía en el monolito original.
- Se sentaron las bases para Pruebas BDD usando herramientas compatibles con el ecosistema .NET (como SpecFlow) para mapear las User Stories directamente a pruebas de comportamiento.
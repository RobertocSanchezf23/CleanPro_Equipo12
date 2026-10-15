# ADR-002 — Repository por Aggregate y Unit of Work con un Aggregate modificado por transacción

| Campo | Contenido |
|---|---|
| **Título** | Persistencia con Repository por Aggregate + Unit of Work; los eventos se publican al confirmar |
| **Estado** | Propuesta (pendiente de revisión en clase) |
| **Fecha** | 14/10/2026 |

## Contexto

Con Domain Model (ADR-001), los objetos de negocio necesitan guardarse sin que el dominio conozca SQL ni EF Core. El BRD exige que la facturación esté vinculada al servicio y sus incidencias (RES-04) y que toda incidencia llegue a administración (RB-06, PA-01). Si una incidencia se guarda pero el aviso falla, o el aviso sale sin que la incidencia se haya guardado, se repite el problema PA-01. El caso es ficticio: no se construye base de datos (indicación del docente).

## Alternativas

| Alternativa | Consecuencia |
|---|---|
| Acceso a datos directo en los Application Services (SQL / DbContext) | Rápido, pero mezcla reglas con persistencia y viola D de SOLID; no se puede probar sin base de datos. |
| Un repositorio genérico `IRepository<T>` con CRUD | Menos código, pero los métodos no hablan el lenguaje del negocio y exponen operaciones que el dominio no necesita (viola I). |
| **Un Repository por Aggregate Root con métodos del negocio + Unit of Work** | Más interfaces, pero cada una expresa lo que el dominio necesita (`CuadrillaTieneServicioEnAsync`, `ExisteFacturaParaServicioAsync`), y la Unit of Work define el límite de la transacción. |

## Decisión

- Un Repository por Aggregate Root: `ICotizacionRepository`, `IServicioRepository`, `IFacturaRepository`, más `ICatalogoDeExtras` de solo lectura. Las interfaces viven en `CleanPro.Domain`.
- `IUnitOfWork.ConfirmarAsync()` cierra cada caso de uso. **Cada transacción modifica un solo Aggregate**; puede leer otros (CU-03 lee la Cotización y crea el Servicio).
- Los Domain Events se guardan en la misma transacción y se publican con `IPublicadorDeEventos` **después** de confirmar. En la versión real, con una tabla Outbox.
- El código base usa implementaciones `EnMemoria`. La implementación real propuesta es EF Core.

## Consecuencias

- **Beneficios:** el dominio no conoce la base de datos; una incidencia y su aviso a administración se guardan juntos (PA-01); las pruebas usan repositorios en memoria.
- **Costos:** más interfaces y clases; el equipo debe respetar el contrato (nada se guarda hasta confirmar; SOLID S-4).
- **Riesgos:** dos coordinadores podrían programar la misma cuadrilla a la misma hora al mismo tiempo (RES-01).
- **Trabajo posterior:** restricción única (cuadrilla + fecha/hora) o concurrencia optimista en la implementación real.

## Trazabilidad

RB-06, RES-01, RES-04 · BN-06, BN-08 · PA-01, PA-02 · US-15, US-19, US-20 · CU-01 a CU-05 · SOLID S-4, S-5, S-7

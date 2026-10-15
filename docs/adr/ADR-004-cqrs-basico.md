# ADR-004 — CQRS básico: Commands por el dominio, Queries con modelo de lectura

| Campo | Contenido |
|---|---|
| **Título** | Separar operaciones que cambian el estado de las consultas y reportes |
| **Estado** | Propuesta (pendiente de revisión en clase) |
| **Fecha** | 14/10/2026 |

## Contexto

El BRD pide 5 reportes (REP-01 a REP-05) y consultas para varios roles (US-17, US-21, CE-05). Esas consultas cruzan servicio, incidencias y factura en una sola vista. Si se usan los aggregates para leer, hay que cargar objetos completos solo para mostrar una tabla, y los repositorios se llenan de métodos de reporte (hallazgo SOLID S-5).

## Alternativas

| Alternativa | Consecuencia |
|---|---|
| Leer todo a través de los repositorios y aggregates | Un solo modelo, pero lento para reportes y mezcla responsabilidades. |
| CQRS completo con bases separadas y sincronización | Demasiado para el tamaño del caso; agrega consistencia eventual sin necesidad. |
| **CQRS básico**: Commands por los Application Services y el dominio; Queries con handlers y DTOs propios sobre el mismo almacenamiento | Separación clara de responsabilidades sin infraestructura extra. |

## Decisión

- **Commands** (8, por ejemplo `ProgramarServicioCommand` y `FacturarServicioCommand`): los recibe el método del Application Service correspondiente, pasan por el dominio y se confirman con la Unit of Work.
- **Queries** (`ServiciosProgramadosQuery`, `ServiciosDeCuadrillaQuery`, `FacturacionPorServicioQuery`): handlers que usan `IConsultasDeServicios` e `IConsultasDeFacturacion` y devuelven DTOs planos. Nunca cambian el estado.
- No se separa el almacenamiento. El catálogo de extras no usa CQRS porque es un listado simple de referencia.

## Consecuencias

- **Beneficios:** los repositorios quedan pequeños y con lenguaje del negocio; los reportes no pueden romper invariantes; cada lado cambia por razones distintas.
- **Costos:** más clases (Query, Handler, DTO).
- **Riesgos:** duplicar nombres entre DTO y Aggregate puede confundir; se mitiga con el sufijo `Dto` y con DTOs que nunca se usan para escribir.

## Trazabilidad

REP-02, REP-04, REP-05 · US-03, US-17, US-21 · BN-10 · CE-05 · SOLID S-5

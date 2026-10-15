# CleanPro — Diseño arquitectural del sistema (Unidad 3)

**Materia:** Componentes de Lógica de Negocio · FIME UANL
**Profesor:** Miguel Ángel Gómez Marroquín
**Equipo 12:**

| Integrante | Matrícula |
|---|---|
| Roberto Carlos Sánchez Flores | 2151262 |
| Donovan Adrian Serrato Lores | 2154304 |
| Laura Sofía Vargas Ramírez | 2152053 |

## Empresa y alcance

CleanPro presta servicios de limpieza residencial y empresarial. Esta actividad convierte el análisis de la Unidad 2 (BRD, User Stories, BPMN y Glosario, en [`docs/u2/`](docs/u2)) en una arquitectura de software.

**Proceso principal diseñado:** cotización → respuesta del cliente → programación y asignación de cuadrilla → ejecución con incidencias → facturación.

| ID | Caso de uso | User Stories |
|---|---|---|
| CU-01 | Generar y enviar cotización | US-01, US-06, US-07 |
| CU-02 | Responder cotización (aceptar / rechazar / vencida) | US-02, US-22 |
| CU-03 | Programar servicio y asignar cuadrilla | US-08, US-09, US-03 |
| CU-04 | Registrar incidencia (incluye acceso denegado y autorización de cargos) | US-15, US-16, US-11, US-19 |
| CU-05 | Facturar servicio | US-20, US-05 |

## Decisiones principales

| Decisión | Documento |
|---|---|
| **Domain Model** como patrón de lógica de negocio (reportes con consultas simples) | [ADR-001](docs/adr/ADR-001-patron-logica-negocio.md) |
| Repository por Aggregate + Unit of Work; un Aggregate modificado por transacción; eventos publicados al confirmar | [ADR-002](docs/adr/ADR-002-repository-unit-of-work.md) |
| Incidencia como Entity dentro del Aggregate Servicio | [ADR-003](docs/adr/ADR-003-incidencia-dentro-de-servicio.md) |
| CQRS básico: Commands por el dominio, Queries con DTOs de lectura | [ADR-004](docs/adr/ADR-004-cqrs-basico.md) |

## Entregables

| # | Entregable | Ubicación |
|---|---|---|
| 01 | Informe de arquitectura (Reporte FIME) | [`ACT-F03. DISEÑO ARQUITECTURAL DEL SISTEMA_Equipo 12.pdf`](docs/) |
| 02 | Mapa de dominio y diagrama de clases | [`diagrams/src`](diagrams/src) (`.drawio`) · [`diagrams/png`](diagrams/png) |
| 03 | 2 diagramas de secuencia (CU-03 y CU-05) | [`diagrams/src`](diagrams/src) · [`diagrams/png`](diagrams/png) |
| 04 | Matriz de trazabilidad actualizada | [`Matriz_Trazabilidad_CleanPro_U3.pdf`](docs/matriz-trazabilidad.pdf) |
| 05 | Bitácora de IA | [`Bitacora_IA_CleanPro_U3.pdf`](docs/bitacora-ia.pdf) |
| 06 | Código base (C# / .NET 8) | [`src/`](src) y [`tests/`](tests) |
| — | ADR | [`docs/adr/`](docs/adr) |

## Estructura del repositorio

```
├── CleanPro.sln
├── docs/
│   ├── u2/                         ← insumos de la Unidad 2
│   ├── adr/                        ← ADR-001 a ADR-004
│   ├── matriz-trazabilidad.pdf
│   ├── bitacora-ia.pdf
│   └── Reporte_FIME_CleanPro_U3.pdf
├── diagrams/
│   ├── src/                        ← archivos fuente draw.io
│   └── png/                        ← exportaciones
├── src/
│   ├── CleanPro.Domain/            ← Aggregates, Value Objects, Domain Services, eventos, interfaces de Repository
│   ├── CleanPro.Application/       ← Application Services, Commands, Queries, DTOs, IUnitOfWork
│   └── CleanPro.Infrastructure/    ← implementaciones en memoria + composición
└── tests/
    └── CleanPro.Tests/             ← 20 escenarios, uno por regla de negocio
```

Las dependencias apuntan hacia el dominio: `Infrastructure → Application → Domain`. El dominio no depende de nada.

## Cómo revisar

1. **Diagramas:** abrir los `.drawio` en [app.diagrams.net](https://app.diagrams.net) (*Archivo → Abrir desde → Dispositivo*) o ver los PNG.
2. **Código:** empezar por `src/CleanPro.Domain/Servicios/Servicio.cs` y `src/CleanPro.Domain/Facturacion/GeneradorDeFactura.cs`, donde viven las reglas más importantes (RB-07, RB-11, RB-14).
3. **Pruebas:** cada prueba de `tests/CleanPro.Tests` tiene en su nombre la regla del BRD que verifica.

### Ejecutar las pruebas

Se necesita el [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```bash
dotnet test
```

El código usa repositorios **en memoria** porque el caso es ficticio y no se construye base de datos. La implementación real propuesta (EF Core + transacción + Outbox) está descrita en ADR-002.

## Supuestos pendientes de validar con el negocio

- H-04: la duración y el personal los captura el coordinador; solo el precio tiene fórmula (RB-03).
- H-05: el 10% de descuento recurrente no se aplica a los extras.
- H-06: cada programación crea un solo servicio aunque la frecuencia sea recurrente.
- Días hábiles = lunes a viernes; una cuadrilla está ocupada si ya tiene un servicio a la misma fecha y hora.

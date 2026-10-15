# ADR-001 — Domain Model como patrón de lógica de negocio

| Campo | Contenido |
|---|---|
| **Título** | Usar Domain Model para las operaciones de escritura de CleanPro |
| **Estado** | Propuesta (pendiente de revisión en clase) |
| **Fecha** | 14/10/2026 |

## Contexto

CleanPro gestiona el ciclo cotización → servicio → incidencias → factura. El BRD tiene 15 reglas de negocio (RB-01 a RB-15), 6 restricciones y 6 situaciones excepcionales. Los problemas principales son errores de facturación (PA-02), cambios que no llegan a administración (PA-01) y poca evidencia de incidencias y extras (PA-04). Las reglas dependen del **estado** de cada cotización y de cada servicio (RB-02, RB-07, RB-08, RB-11, RB-14).

## Alternativas

| Alternativa | Consecuencia |
|---|---|
| Transaction Script | Simple, pero duplica las validaciones de estado y el cálculo de precio en cada operación, con riesgo de seguir facturando mal. |
| Table Module | Útil para reportes por tabla, pero no protege las reglas de un servicio individual (acceso denegado, cargos por autorizar). |
| Domain Model | Las reglas viven en Cotización, Servicio y Factura y se protegen las transiciones de estado. Cuesta más diseño y requiere Repository y Unit of Work. |

## Decisión

Se usa **Domain Model** para CU-01 a CU-05. Las **consultas y reportes** (REP-01 a REP-05) se resuelven con handlers de lectura que devuelven DTOs, sin pasar por el modelo. El **catálogo de extras** se trata como datos de referencia.

## Consecuencias

- **Beneficios:** cálculo de precio en un solo lugar (RB-03); estados protegidos (no programar sin aceptación, no facturar sin terminación o con cargos sin autorizar); lenguaje del glosario en el código; Domain Events para notificar a administración.
- **Costos:** más clases y diseño previo; curva de aprendizaje del equipo.
- **Riesgos:** sobremodelar conceptos simples. Se mitiga dejando el catálogo y los reportes fuera del modelo.
- **Trabajo posterior:** definir Repository y Unit of Work (ADR-002) y separar Commands y Queries (etapa 6).

## Trazabilidad

BN-01, BN-05, BN-07, BN-08 · RB-02, RB-03, RB-07, RB-08, RB-11, RB-13, RB-14 · RES-03, RES-04 · US-01, US-02, US-06, US-15, US-20 · PA-01, PA-02, PA-04 · CE-01, CE-02 · CU-01 a CU-05

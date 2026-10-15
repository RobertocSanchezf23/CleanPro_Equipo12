# ADR-003 — Incidencia como Entity dentro del Aggregate Servicio

| Campo | Contenido |
|---|---|
| **Título** | La Incidencia vive dentro del límite de consistencia del Servicio |
| **Estado** | Propuesta (pendiente de revisión en clase) |
| **Fecha** | 14/10/2026 |

## Contexto

Las incidencias son el centro de los problemas del BRD (PA-01, PA-04) y afectan directamente al cobro. Hay reglas que relacionan al servicio con **todas** sus incidencias al mismo tiempo:
- RES-03: las incidencias deben quedar documentadas antes de registrar la terminación.
- RB-14: no se puede facturar con cargos adicionales sin autorizar.
- RB-11: la incidencia "cliente no permite acceso" cambia el estado del servicio a No realizado.
- SUP-03: cada incidencia pertenece a un único servicio.

## Alternativas

| Alternativa | Consecuencia |
|---|---|
| Incidencia como Aggregate propio (con `ServicioId`) | Aggregates más pequeños, pero RB-14 y RB-11 requerirían coordinar dos aggregates en la misma operación, y el Servicio no podría proteger sus propias reglas. |
| **Incidencia como Entity dentro de Servicio** | El Servicio protege todas las reglas en un solo lugar y en una sola transacción. Cuesta cargar el Servicio completo con sus incidencias. |

## Decisión

`Incidencia` es una Entity dentro del Aggregate `Servicio`. Solo se crea o modifica a través de la raíz: `Servicio.RegistrarIncidencia()`, `Servicio.AutorizarCargo()`, `Servicio.RegistrarAccesoDenegado()`. `CargoAdicional` es un Value Object de la incidencia.

## Consecuencias

- **Beneficios:** `VerificarQueEsFacturable()` puede revisar todos los cargos sin consultar otra fuente; una incidencia nunca queda "suelta" sin servicio (SUP-03); el evento `IncidenciaRegistrada` se emite desde la raíz.
- **Costos:** cada operación sobre un servicio carga todas sus incidencias. Es aceptable porque un servicio de limpieza tiene pocas incidencias.
- **Riesgos:** si el supervisor y el coordinador modifican el mismo servicio al mismo tiempo (registrar incidencia y autorizar cargo), se necesita control de concurrencia.
- **Trabajo posterior:** revisar esta decisión si en el futuro las incidencias se gestionan fuera del servicio (por ejemplo, reclamaciones posteriores).

## Trazabilidad

RB-06, RB-11, RB-12, RB-14 · RES-03 · SUP-03 · BN-05, BN-08 · US-11, US-15, US-16 · PA-01, PA-04 · CU-04, CU-05

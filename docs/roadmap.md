# JCB Estudio — Roadmap de Entregables

**Versión:** 1.0
**Fecha:** 2026-07-29
**Depende de:** [requerimientos-funcionales.md](requerimientos-funcionales.md), [modelo-de-datos.md](modelo-de-datos.md)

Este documento ordena todos los entregables del proyecto — análisis y construcción — y qué funcionalidad tiene cada uno. Reemplaza las menciones sueltas a "próximo entregable" que había en los documentos anteriores.

---

## Etapa de análisis

| Entregable | Contenido | Estado |
|---|---|---|
| **E1** — Requerimientos Funcionales | Reglas de negocio de todos los módulos | ✅ Hecho |
| **E2** — Modelo de Datos (DER) | Entidades, campos, relaciones | ✅ Hecho (en revisión continua junto con E1) |
| **E3** — Wireframes | Pantallas clave: alta de proyecto, carga/cierre de sesión, tarifa del proyecto, horas disponibles, carga de pagos | Pendiente |

---

## Etapa de construcción — MVP (achicado, decisión 2026-07-29)

**Sin módulo de Presupuestos ni módulo de Reportes** (ambos pasan a Fase 2). El objetivo del MVP es resolver el problema original: "no sabemos qué le cobramos o qué falta", con el mínimo posible de pantallas.

| Entregable | Funcionalidad | Depende de |
|---|---|---|
| **E4** — Base técnica + Clientes | ASP.NET Identity (roles Administrador/Operador), login con Gmail. ABM de Clientes completo, con los campos que faltan (Ciudad, ComoLlego, CondicionesCobro, etc.) | — (Clientes ya está ~80% construido) |
| **E5** — Proyectos + Tarifa | ABM de Proyectos (cliente, estado, productor responsable, horas contratadas opcionales). Tarifa acordada **cargada a mano** (sin módulo de Presupuestos), con `FechaAcuerdo` propia para el aviso de vigencia (3 meses) | E4 |
| **E6** — Sesiones y Tareas | ABM de Sesiones — **pantalla de carga día a día** (reemplaza la anotación en el celular): fecha/hora de inicio y fin, descripción, estudio/locación, detalle de tareas. Cálculo de horas disponibles | E5 |
| **E7** — Pagos + información en pantalla | ABM de Pagos (parciales, saldo pendiente). Horas del mes, horas por cliente y facturación mensual **visibles en las pantallas normales** de Proyectos/Clientes — sin módulo de reportes ni exportación a PDF | E5, E6 |

**El MVP cierra en E7.** A partir de ahí, JCB Estudio puede trabajar el día a día completo: cargar clientes, armar proyectos con su tarifa, anotar cada sesión, y ver cuánto se cobró y cuánto falta.

---

## Fase 2 (sin fecha — se retoma cuando el MVP esté validado en uso real)

| Entregable | Funcionalidad |
|---|---|
| **Módulo de Presupuestos** | Múltiples cotizaciones por proyecto, flag `Aceptado`, vínculo automático con `Tarifa` (reemplaza la carga a mano del MVP) |
| **Reportes formales** | Filtros, historial, **exportación a PDF**, más reportes adicionales: horas por proyecto, proyectos activos/finalizados, clientes más frecuentes, horas por técnico |
| **Portal de Clientes** | Login con Gmail/DNI, vinculación automática por DNI, sección "Mi perfil" (resumen de proyectos — contenido exacto a definir) |
| **Fase 2 adicional** | Fotos adjuntas a sesiones, recordatorios/tareas pendientes, redes sociales del cliente (Instagram, YouTube) |

---

## Fase 3 (mucho más adelante, sin fecha)

| Entregable | Funcionalidad |
|---|---|
| **E-Fase3** | El cliente podrá aprobar presupuestos y hacer pagos online desde su perfil |

---

## Qué cambia si más adelante se retoma el módulo de Presupuestos

`Tarifa` ya existe desde el MVP con su propio `FechaAcuerdo`. Al construir el módulo de Presupuestos en Fase 2, el cambio es aditivo: se agrega la tabla `Presupuesto` y la regla "al aceptar un presupuesto, se actualiza la Tarifa" — no hace falta rehacer nada de lo construido en el MVP.

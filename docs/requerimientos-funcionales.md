# JCB Estudio — Documento de Requerimientos Funcionales

**Versión:** 1.2
**Fecha:** 2026-07-29
**Origen:** Relevamiento con el cliente (JCB Estudio - Requerimientos.pdf) + decisiones tomadas el 2026-07-29 + revisión del análisis (Sesión, Usuarios, Presupuesto↔Tarifa, ASP.NET Identity) + achique del MVP, todo el mismo día.
**Estado:** Base para iniciar diseño de datos y wireframes. Ver [roadmap.md](roadmap.md) para el orden de entregables de construcción. Contiene supuestos marcados explícitamente que deben confirmarse antes de cerrar el alcance definitivo.

---

## 1. Objetivo del sistema

Reemplazar el registro manual (celular / papel) de horas trabajadas por JCB Estudio (estudio de grabación audio/visual) por un sistema que permita:

- Organizar el trabajo por cliente y por proyecto.
- Registrar sesiones de trabajo con su detalle, horas y responsable.
- Saber en todo momento cuánto se le cobró a cada cliente y cuánto falta cobrar.
- Controlar horas disponibles en proyectos con bolsa de horas contratada.
- Emitir reportes básicos de horas y facturación.

Usuarios del sistema: el equipo de JCB Estudio (Alfredo, Lucía y quien se sume), con roles diferenciados (ver [Módulo 7](#7-usuarios-y-roles)).

---

## 2. Alcance

### MVP (fase 1) — **achicado, decisión 2026-07-29**
- Módulo Clientes (ya iniciado en el código).
- Módulo Proyectos, con Tarifa acordada **cargada directamente y a mano** — **sin módulo de Presupuestos**: el precio se sigue negociando de palabra, como hacen hoy, y el staff carga el número final en `Tarifa`.
- Módulo Sesiones y Tareas — pantalla de carga día a día (alta/edición/baja).
- Pagos parciales y saldo pendiente.
- Aviso de vencimiento de la tarifa acordada (3 meses), calculado al vuelo a partir de `Tarifa.FechaAcuerdo` — sin entidad `Alerta`.
- Roles Administrador / Operador.
- Historial de modificaciones (auditoría).
- **Horas y facturación visibles en pantalla** (horas del mes, horas por cliente, facturación mensual) — **sin módulo de Reportes ni exportación a PDF**. Es información que ya se muestra en las pantallas normales de Proyectos/Clientes, no una pantalla de reportería aparte.

### Fase 2 (no bloquea el MVP)
- **Módulo formal de Presupuestos**: múltiples cotizaciones por proyecto, flag `Aceptado`, vínculo automático con `Tarifa` (ver [Módulo 5](#5-módulo-presupuestos-fase-2)). Sacado del MVP el 2026-07-29 para achicarlo — la negociación queda verbal por ahora.
- **Reportes formales con exportación a PDF** (filtros, historial). Sacado del MVP el 2026-07-29 — la misma información ya se ve en pantalla, solo falta la exportación.
- Recordatorios / tareas pendientes.
- Redes sociales del cliente (Instagram, YouTube).
- Reportes adicionales: horas por proyecto, proyectos activos/finalizados, clientes más frecuentes, horas por técnico.
- Adjuntar fotos a sesiones — **decisión 2026-07-29: no se modela por el momento** (sin entidad `FotoAdjunta`, sin definición de almacenamiento).
- **Portal de clientes (decisión 2026-07-29):** el cliente tendrá su propio perfil para consultar (solo lectura) el avance de sus proyectos. A nivel resumen: horas usadas, horas disponibles y estado del proyecto — sin acceso al detalle interno de cada sesión (ver [Módulo 8](#8-módulo-sesiones) y [Módulo 7](#7-usuarios-y-roles)).

### Fase 3 (mucho más adelante — no se modela todavía)
- El cliente podrá aprobar presupuestos y hacer pagos online desde su perfil. El propio cliente (dueño de JCB Estudio) marcó esto como "para mucho más adelante"; no se diseña ni se modela hasta que se retome explícitamente.

---

## 3. Módulo Clientes

**Estado de implementación:** ya existe (`Shared/Models/Cliente.cs`, `ClientesController`, CRUD completo con validaciones).

### Campos ya implementados
Tipo (Persona/Empresa), Nombre, Apellido, Dni, RazonSocial, Cuit, CondiciónFiscal, Email, Teléfono, Dirección, FechaAlta.

### Campos del relevamiento que faltan agregar
| Campo | Aplica a | Nota |
|---|---|---|
| Ciudad | Persona | del relevamiento |
| Instagram / YouTube | Ambos | Fase 2 |
| Observaciones | Ambos | texto libre |
| Cómo llegó al estudio | Ambos | texto libre o catálogo simple |
| Condiciones especiales de cobro | Ambos | texto libre (ej: "50% al reservar, resto al finalizar") — por cliente, puede diferir del default |
| Vínculo a tarifa personalizada | Ambos | ver [Módulo 6](#6-tarifas-y-precios) |

**Supuesto abierto:** el relevamiento menciona "Empresa (si corresponde)" como campo distinto de "Razón social" al hablar de clientes tipo empresa. No quedó claro si son lo mismo o si "Empresa" es un nombre comercial distinto de la razón social legal. **A confirmar con el cliente antes del DER.**

### Reglas de negocio
- Un cliente puede tener varios proyectos activos en simultáneo.

---

## 4. Módulo Proyectos

| Campo | Obligatorio | Regla |
|---|---|---|
| Nombre | Sí | = nombre del cliente. Si un cliente tiene varios proyectos simultáneos, requiere un identificador adicional (ej. sufijo o descripción corta) — **a definir en wireframes**, no está resuelto en el relevamiento. |
| Cliente | Sí | Un proyecto pertenece a un solo cliente. |
| FechaInicio | Sí | — |
| FechaFinEstimada | No | El cliente indicó que no se usa. |
| Estado | Sí | Enum: `Presupuesto`, `EnCurso`, `EnPausa`, `Finalizado`, `Cancelado`. **`EnPausa` es un supuesto no confirmado explícitamente por el cliente en el relevamiento — validar.** |
| ProductorResponsable | Sí | Referencia a un Usuario (Alfredo, Lucía, etc.) |
| HorasContratadas | No (nullable) | **Decisión 2026-07-29: por defecto se asume que el proyecto NO tiene una bolsa de horas pactada.** El campo existe pero puede quedar vacío. Cuando tiene valor, el sistema calcula horas disponibles (ver [Módulo 6](#6-tarifas-y-precios)). |

### Reglas de negocio
- **Revisión 2026-07-29 (achique del MVP):** el proyecto ya no tiene un módulo de Presupuestos en el MVP. La tarifa se negocia de palabra y se carga directamente en `Tarifa` (ver [Módulo 6](#6-tarifas-y-precios)). El módulo formal de Presupuestos (varias cotizaciones, `Aceptado`, vínculo automático con Tarifa) queda documentado en el [Módulo 5](#5-módulo-presupuestos-fase-2) para cuando se retome en Fase 2.
- El valor de la hora se fija al acordar el proyecto y no cambia dentro del mismo proyecto.

---

## 5. Módulo Presupuestos (Fase 2)

**Decisión 2026-07-29: este módulo queda fuera del MVP.** Se documenta tal cual se había definido, para construirlo en Fase 2 si deciden retomarlo. Hasta entonces, el precio se negocia de palabra y se carga directamente en `Tarifa` (ver [Módulo 6](#6-tarifas-y-precios)).

- Un proyecto puede tener **N presupuestos** (uno por cada variante de alcance que el cliente evalúe: ej. "grabar sin afinar" vs "con videoclip").
- Cada presupuesto tiene un campo simple `Aceptado` (sí/no) — no es un flujo formal de aprobación, es solo la marca de cuál de los N presupuestos es el que efectivamente se acordó con el cliente.
- El sistema registra `FechaAcuerdo` (fecha en la que se pactó el precio), porque es la fecha base para la regla de vigencia.
- **Regla de vigencia:** el precio acordado se mantiene válido durante **3 meses** desde `FechaAcuerdo` (motivo: variación del dólar). **En el MVP, esta regla se aplica igual, pero sobre `Tarifa.FechaAcuerdo` en vez de `Presupuesto.FechaAcuerdo`** (ver [Módulo 11](#11-alertas)).
- Al marcarse `Aceptado`, su `ValorHora` se copia a la `Tarifa` del proyecto (ver [Módulo 6](#6-tarifas-y-precios)).

---

## 6. Tarifas y Precios

- **Decisión 2026-07-29:** el precio siempre se asume **personalizado por cliente** (no hay una tarifa estándar global por defecto). Cada cliente/proyecto tiene su propio valor de hora o de paquete.
- Modalidad de cobro: **por hora** o **por proyecto cerrado** (paquete). Se define al acordar el proyecto.
- El valor de la hora puede variar entre clientes, pero no cambia dentro de un mismo proyecto una vez acordado.
- Horas extra (cuando la modalidad es por hora): se cobran al mismo valor que las horas normales, sin recargo.
- No existe precio mínimo por sesión (eso es lo que distingue la modalidad "por hora" de "por paquete").
- Pueden regalarse horas como cortesía comercial (decisión manual caso a caso, no una regla automática).
- No se contempla descuento de horas sin cobrarlas (política: nunca se descuenta).
- El sistema debe calcular el importe automáticamente en base a horas cargadas × tarifa vigente.

### Tarifa en el MVP (revisión 2026-07-29 — achique del MVP)

Sin módulo de Presupuestos, `Tarifa` se carga **directamente a mano** al definir el proyecto: el staff negocia de palabra con el cliente y anota el valor acordado (por hora o por paquete) junto con la fecha en que se acordó (`Tarifa.FechaAcuerdo`, que reemplaza a `Presupuesto.FechaAcuerdo` para la regla de vigencia de 3 meses — ver [Módulo 11](#11-alertas)).

Cuando en Fase 2 se construya el módulo de Presupuestos, la relación queda así: `Presupuesto` es la cotización (puede haber varias versiones), y `Tarifa` es el valor operativo vigente que usa el sistema para calcular el importe de cada sesión. Al marcar un `Presupuesto` como `Aceptado`, su `ValorHora` se copia a la `Tarifa` del proyecto — recién ahí deja de cargarse a mano.

### Horas disponibles (solo aplica si el proyecto tiene `HorasContratadas`)
```
Disponible = HorasContratadas − Σ(horas de todas las sesiones del proyecto)
```
Ejemplo del relevamiento: 150 hs contratadas − 5 hs (sesión 1) − 8 hs (sesión 2) = 137 hs disponibles.

Si el proyecto **no** tiene `HorasContratadas` (caso asumido por defecto), no aplica este cálculo; solo se acumulan horas trabajadas para reporting.

---

## 7. Usuarios y Roles

**Decisión 2026-07-29: sí habrá administrador.** Roles definidos:

- **Administrador** (staff — Alfredo y Lucía): control total. Ve todos los proyectos y clientes, gestiona usuarios, tarifas y presupuestos.
- **Operador** (staff): ve únicamente los trabajos/sesiones que él mismo cargó.
- **Cliente** (decisión 2026-07-29, revisión del análisis): el propio cliente puede tener un usuario para loguearse y consultar sus proyectos. Ve **únicamente sus propios proyectos**, en modo **solo consulta** y a **nivel resumen** (horas usadas, horas disponibles, estado del proyecto) — **no ve el detalle de cada sesión ni las observaciones internas del staff**. El nivel de detalle exacto que sí puede ver queda a definir más adelante (asumido como resumen por ahora, confirmar antes de construir la pantalla del portal).

### Vínculo Cliente ↔ Usuario
Se evaluaron dos formas de modelarlo:
- (a) Unificar en una tabla `Persona` genérica de la que cuelgan `Cliente` y `Usuario`.
- (b) Mantener `Cliente` y `Usuario` como entidades separadas, y que `Usuario` tenga una referencia opcional a `Cliente` (nula para el staff, con valor para un login de cliente).

**Decisión 2026-07-29: se adopta la opción (b).** Motivo: no mezclar en una misma tabla los datos comerciales/fiscales del cliente con los datos de acceso/credenciales, que tienen ciclos de vida y necesidades de seguridad distintos.

### Autenticación (decisión 2026-07-29)

- Se implementa con **ASP.NET Identity** (maneja usuarios, contraseñas, roles y logins externos de forma nativa — no se arma un sistema de login a medida).
- Los roles `Administrador`, `Operador` y `Cliente` se implementan como **Roles de Identity**, no como un campo enum aparte.
- Además del login con usuario/contraseña, se permite **iniciar sesión con Gmail** (proveedor externo de Google vía ASP.NET Identity).
- **Nota (a confirmar):** Google no entrega el DNI de la persona. Para que funcione la vinculación por DNI ([ver más abajo](#cómo-se-vincula-un-usuario-cliente-con-su-cliente-revisión-2026-07-29)), la primera vez que alguien se loguea con Gmail habría que pedirle el DNI en una pantalla intermedia antes de intentar la vinculación. Lo dejo como supuesto de flujo, no como algo que hayas confirmado explícitamente.

### Cómo se vincula un Usuario-Cliente con su Cliente (revisión 2026-07-29)

1. El `Cliente` lo da de alta el **Administrador** (staff), como ya funciona hoy en el CRUD de Clientes — el cliente no se autoregistra como cliente comercial.
2. Cuando esa persona inicia sesión como Cliente, el sistema busca coincidencia por **DNI** contra los clientes ya cargados.
3. **Si coincide:** se vincula el `Usuario` a ese `Cliente` (se completa `Usuario.ClienteId`) y accede a su portal.
4. **Si no coincide:** no se vincula nada, y se le muestra un aviso amigable (no un error técnico), del estilo: *"Todavía no encontramos tu registro en JCB Estudio. Contactate con nosotros para darte de alta."*

**Pendiente:** falta terminar de definir qué contenido exacto tiene la sección "Mi perfil" del cliente una vez vinculado (quedó cortado en la conversación) — no se modela todavía hasta tener esa lista completa.

### Reglas de negocio
- Se requiere **historial de modificaciones** (auditoría): quién creó/modificó cada registro y cuándo. Aplica como mínimo a Proyectos, Presupuestos, Sesiones y Pagos.
- Un usuario con rol Cliente solo puede ver datos de **su propio** cliente; nunca de otro.

---

## 8. Módulo Sesiones

Por cada sesión se registra:

| Campo | Obligatorio | Nota |
|---|---|---|
| FechaInicio (fecha + hora) | Sí | **Revisión 2026-07-29:** reemplaza a los campos separados `Fecha` + `HoraInicio`. Motivo: una sesión puede arrancar un día y terminar de madrugada al día siguiente (ej. graban toda la noche), y con `Fecha` única no se podía representar ese cruce de día. |
| FechaFin (fecha + hora) | No | Reemplaza a `HoraFin`. Puede quedar vacío (sesión abierta). Puede caer en un día calendario distinto al de `FechaInicio`. |
| CantidadHoras | Sí (al cerrar) | |
| Responsable (operador de turno) | Sí | Referencia a Usuario |
| Descripción | Sí | |
| Estudio / Sala / Locación | Sí | Puede ser una locación externa (grabaciones en otros lugares) |
| Observaciones | No | Uso interno del staff — **no visible para el rol Cliente** (ver [Módulo 7](#7-usuarios-y-roles)). |
| Fotos adjuntas | No | **Decisión 2026-07-29: no se modela por el momento.** Sin entidad `FotoAdjunta` ni definición de almacenamiento; queda completamente fuera hasta que se retome. |
| UsuarioQueCargó | Sí | Auditoría |

**Revisión 2026-07-29 — se elimina el campo `Estado` (Abierta/Cerrada):** es un dato derivado, no una entidad propia. Si `FechaFin` está vacío, la sesión está abierta; si tiene valor, está cerrada. Mantenerlo como campo aparte generaba riesgo de desincronización (quedar cargado un `FechaFin` pero no actualizar el estado, o viceversa). Confirmado con el cliente que no hay casos de cerrar sin `FechaFin` ni de reabrir una sesión ya cerrada.

### Detalle de tarea (dentro de una sesión, puede haber varias)
Catálogo de tipo de trabajo: Grabación, Mezcla, Mastering, Edición, Producción (musical/visual/audiovisual), Ensayo, Otro.

Catálogo de tareas específicas (ejemplos del relevamiento): Grabación de voces/batería/guitarras/bajos/teclados/acordeón/bandoneón/percusión, Corrección de afinación, Edición multimedia, Filmación, Edición de voces/batería/acordeón/bandoneón, Mezcla tema, Mastering, Exportación de stems, Revisión con cliente, Cambios solicitados por cliente, Backup del proyecto.

No se registra equipamiento utilizado (descartado explícitamente por el cliente).

---

## 9. Facturación y Pagos

- **Decisión 2026-07-29: el sistema debe permitir N pagos parciales** (no limitado a "seña + saldo"). Cada pago registra: fecha, monto, medio de pago, concepto.
- Medios de pago: Mercado Pago, Transferencia, Efectivo.
- Se registran señas como un tipo de pago parcial más.
- El sistema calcula el saldo pendiente: `Total acordado − Σ(pagos registrados)`, donde "Total acordado" sale de `Tarifa` (el monto del paquete si es modalidad `PorPaquete`, u horas cargadas × valor hora si es `PorHora`) — **revisión 2026-07-29:** ya no sale de `Presupuesto`, que quedó fuera del MVP.
- Debe poder listarse pagos pendientes (clientes con saldo > 0).

---

## 10. Información de horas y facturación (MVP) — antes "Reportes"

**Revisión 2026-07-29 (achique del MVP):** no hay módulo de Reportes ni exportación a PDF en el MVP. La información se muestra **directamente en las pantallas normales** de Proyectos y Clientes, sin una sección aparte para "generar" nada:

1. Horas trabajadas por mes.
2. Horas por cliente.
3. Facturación mensual.

**Fase 2 — Reportes formales:** cuando se retome, se construye un módulo de Reportes con filtros y **exportación a PDF**, más los reportes adicionales: horas por proyecto, proyectos activos, proyectos finalizados, clientes más frecuentes, horas trabajadas por técnico/operador.

---

## 11. Alertas

**Decisión 2026-07-29: no se modela una entidad `Alerta` por el momento.** El aviso de vencimiento se resuelve **calculándolo al vuelo**, sin persistir nada: al listar proyectos, el sistema compara `Tarifa.FechaAcuerdo + 3 meses` contra la fecha de hoy y marca visualmente los vencidos (ej. una etiqueta "vencido"). **Revisión 2026-07-29 (achique del MVP):** la fecha base pasa a ser `Tarifa.FechaAcuerdo` en vez de `Presupuesto.FechaAcuerdo`, porque el módulo de Presupuestos quedó en Fase 2. No hay tabla ni historial de alertas atendidas/pendientes — si más adelante hace falta ese historial, ahí sí se justifica una entidad aparte.

- (Fase 2) Recordatorios de tareas pendientes por proyecto — sigue pendiente de definir cómo se implementa.

---

## 12. Glosario de decisiones registradas (2026-07-29)

| # | Pregunta abierta | Decisión |
|---|---|---|
| 1 | ¿Habrá administrador? | Sí. Roles Administrador / Operador. |
| 2 | Prioridad de reportes | Horas por mes, horas por cliente, facturación mensual. |
| 3 | ¿Lista de precios personalizada? | Sí, siempre personalizada por cliente/proyecto. |
| 4 | ¿Pagos parciales? | Sí, N pagos parciales sin límite fijo. |
| 5 | ¿Se pactan las horas? | Se asume que NO por defecto; el campo queda opcional. |
| 6 | ¿Qué pasa si vence el presupuesto? | Alerta al equipo, sin recálculo automático. |

### Revisión adicional (mismo día, tras comentarios sobre Sesión y Usuarios)

| # | Tema | Decisión |
|---|---|---|
| 7 | Campo `Estado` de Sesión | Se elimina; se deriva de si `FechaFin` está vacío o no. |
| 8 | Sesión que cruza medianoche | `Fecha` + `HoraInicio` + `HoraFin` se reemplazan por `FechaInicio` y `FechaFin` (fecha y hora juntos). |
| 9 | ¿El cliente puede ser usuario del sistema? | Sí, se suma un rol `Cliente` con acceso de solo consulta a sus propios proyectos (nivel resumen, sin detalle de sesión). Portal es Fase 2. |
| 10 | Vínculo entre Cliente y Usuario | `Usuario` referencia opcionalmente a `Cliente` (no se unifican en una tabla `Persona`). |
| 11 | ¿El cliente podrá aprobar presupuestos / pagar online? | A futuro (Fase 3), no se modela todavía. |
| 12 | ¿Se usa la entidad `Alerta`? | No por el momento. El aviso de vencimiento de presupuesto se calcula al vuelo, sin persistir nada. |
| 13 | ¿Se usa la entidad `FotoAdjunta`? | No por el momento. Fotos adjuntas a sesiones queda completamente fuera de alcance hasta que se retome. |
| 14 | ¿Cómo se relacionan Presupuesto y Tarifa? | Presupuesto es la cotización (puede haber varias); al marcarse `Aceptado`, su `ValorHora` se copia a la Tarifa del proyecto, que es la que usa el sistema para calcular el importe de cada sesión. |
| 15 | ¿Cómo sabe el sistema cuál presupuesto es el aceptado? | Campo simple `Aceptado` (sí/no) en Presupuesto — no es un flujo formal de aprobación, solo la marca de cuál se acordó. |
| 16 | ¿Para autenticación? | ASP.NET Identity, con roles de Identity y login externo con Gmail. |
| 17 | Achicar el MVP: ¿el módulo de Presupuestos entra? | No. Queda para Fase 2. En el MVP la tarifa se negocia de palabra y se carga directo en `Tarifa` (con su propia `FechaAcuerdo`). |
| 18 | Achicar el MVP: ¿entra el módulo de Reportes? | No como módulo con exportación. La información (horas por mes, por cliente, facturación) se ve en pantalla en el MVP; el módulo formal con PDF queda para Fase 2. |

## 13. Supuestos aún sin confirmar (pendientes de validar con el cliente)

- Estado de proyecto `En pausa`: no tuvo un "sí" explícito en el relevamiento original (a diferencia de los demás estados). Asumido como válido por lógica de negocio.
- Campo "Empresa" vs "Razón social" en clientes tipo empresa: posible redundancia o campos distintos, sin aclarar.
- Cómo se identifica un proyecto cuando un mismo cliente tiene varios proyectos simultáneos con el mismo nombre (el cliente dijo que el nombre del proyecto = nombre del cliente).
- Nivel de detalle exacto que verá el rol Cliente en el portal (asumido como resumen: horas usadas/disponibles y estado — "eso lo decidimos más adelante", palabras del cliente).

---

## 14. Próximos entregables

Ver [roadmap.md](roadmap.md) para el detalle completo y el orden de construcción del MVP.

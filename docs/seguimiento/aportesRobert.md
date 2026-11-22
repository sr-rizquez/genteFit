# Aportes de Robert — Producto 3

**Proyecto:** GenteFit  
**Rama de Git:** producto3  
**Commit:** 1a20775  
**Fecha:** 22/11/2026

## 1. Trabajo realizado

Durante el desarrollo del Producto 3 se ampliaron las funcionalidades de GenteFit, principalmente en la capa de negocio y en la aplicación web.

Se creó el proyecto `GenteFit.Negocio` para centralizar la lógica de las operaciones y se integró con la aplicación web.

## 2. Gestión de clientes

Se ampliaron las operaciones de gestión de clientes:

- Creación y modificación de clientes.
- Búsqueda por nombre, apellidos, DNI y correo electrónico.
- Comprobación de datos duplicados.
- Baja lógica de clientes.
- Cancelación de reservas al dar de baja a un cliente.
- Liberación de plazas y promoción de personas en lista de espera cuando corresponde.

También se modificaron las vistas de clientes para adaptarlas a las nuevas operaciones.

## 3. Gestión de reservas

Se implementó la gestión de reservas de actividades y sesiones:

- Consulta de actividades disponibles.
- Aplicación de filtros por actividad, intensidad y sala.
- Creación de reservas.
- Comprobación de plazas disponibles.
- Incorporación a lista de espera cuando no quedan plazas.
- Consulta de las reservas de cada cliente.
- Cancelación de reservas.
- Promoción automática de la primera persona en espera cuando se libera una plaza.
- Consulta del estado de ocupación de una sesión.

Para las operaciones de reserva y cancelación se utilizaron transacciones para mantener la consistencia de los datos.

## 4. Gestión de actividades y sesiones

Se incorporaron operaciones para administrar las actividades:

- Creación y modificación de actividades.
- Eliminación o baja lógica según la existencia de sesiones asociadas.
- Asignación de monitores a actividades.
- Programación de sesiones.
- Creación de sesiones repetidas semanalmente.
- Comprobación de solapamientos de horarios.
- Cancelación de sesiones y de sus reservas.

También se añadió la gestión de monitores y salas, incluyendo la validación de datos y las comprobaciones necesarias antes de realizar cambios.

## 5. Usuarios y autenticación

Se implementó el sistema de acceso a la aplicación:

- Inicio y cierre de sesión.
- Acceso mediante nombre de usuario o correo electrónico.
- Verificación de contraseñas mediante PBKDF2 con SHA-256.
- Gestión de usuarios.
- Creación de cuentas y asignación de roles.
- Cambio de contraseña y de rol.
- Activación y desactivación de usuarios.
- Protección para evitar la desactivación del último administrador activo.

Se configuró la autenticación mediante cookies y se añadieron restricciones de acceso según el rol del usuario.

## 6. Cambios en la aplicación web

Se añadieron controladores, modelos y vistas para las nuevas funcionalidades:

- Actividades.
- Monitores.
- Salas.
- Sesiones.
- Reservas.
- Usuarios.
- Cuenta e inicio de sesión.

También se modificó el menú de navegación para mostrar las opciones correspondientes a cada rol.

## 7. Pruebas y compilación

Se creó el proyecto `GenteFit.Pruebas` con pruebas unitarias relacionadas con la autenticación:

- Generación y formato de hashes.
- Uso de sales diferentes.
- Validación de contraseñas.
- Inicio de sesión mediante nombre de usuario o correo electrónico.
- Rechazo de contraseñas incorrectas.
- Rechazo de usuarios inactivos.

La solución se compiló correctamente, sin errores. Se registraron advertencias de dependencias y una advertencia relacionada con la configuración de autorización.

## 8. Publicación en Git

Los cambios se añadieron y publicaron en la rama `producto3` del repositorio compartido.

**Commit:** `1a20775`  
**Mensaje:** `Implementa gestion de clientes, reservas, actividades y usuarios`

Se comprobó que el repositorio local quedó sincronizado con GitHub y sin cambios pendientes.

---

**Nota:** La integración con la base de datos y la importación/exportación XML de Odoo ya formaban parte del trabajo del equipo. No se atribuyen como desarrollos nuevos de esta aportación.

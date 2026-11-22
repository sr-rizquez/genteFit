# Incidencias y comprobaciones — Robert — Producto 3

**Proyecto:** GenteFit  
**Rama de Git:** producto3  
**Fecha de actualización:** 22/11/2026

## 1. Compilación de la solución

Se ejecutó la compilación de la solución:

`dotnet build .\src\GenteFit.Demo\GenteFit.Demo.slnx`

El resultado fue correcto, con **0 errores y 29 advertencias**.

Las advertencias detectadas incluyen:

- Avisos de seguridad en dependencias NuGet, principalmente en `System.Security.Cryptography.Pkcs`.
- Avisos de seguridad de nivel bajo en `NuGet.Packaging` y `NuGet.Protocol`.
- Una advertencia de ASP.NET Core relacionada con la combinación de los atributos `[AllowAnonymous]` y `[Authorize]` en `CuentaController`.

Estas advertencias no impidieron la compilación, pero conviene revisarlas antes de dar el desarrollo por cerrado.

## 2. Ejecución de las pruebas unitarias

Se creó el proyecto `GenteFit.Pruebas` con 10 pruebas relacionadas con la autenticación.

En una ejecución anterior se obtuvo un resultado de 10 pruebas correctas.

Sin embargo, en una ejecución posterior Windows bloqueó la carga del archivo `GenteFit.Pruebas.dll` al ejecutar las pruebas.

La revisión del registro de eventos de Windows mostró bloqueos relacionados con Smart App Control y las políticas de integridad de código.

Por tanto, el problema detectado está relacionado con las restricciones de seguridad del equipo y no demuestra por sí mismo que exista un error en las pruebas o en el código.

No se modificó la configuración de seguridad de Windows para intentar evitar el bloqueo.

## 3. Herramientas de Entity Framework

Al consultar las migraciones mediante `dotnet ef`, se obtuvo el mensaje:

`Could not load assembly 'GenteFit.Datos'. Ensure it is referenced by startup project 'GenteFit.Web'.`

Se comprobó que el proyecto `GenteFit.Web` contiene la referencia a `GenteFit.Datos` y que el archivo DLL existe en la carpeta de compilación.

Este resultado apunta a un problema de resolución de ensamblados durante la ejecución de la herramienta. No permite concluir que la base de datos o las migraciones no estén implementadas.

La integración de base de datos ya funciona en el equipo de otro integrante, por lo que conviene revisar la configuración local antes de modificar esa parte del proyecto.

## 4. Integración con la base de datos y XML de Odoo

La integración con la base de datos y las operaciones XML de Odoo ya estaban desarrolladas en el proyecto del equipo.

No se realizaron cambios para sustituir o rehacer esas implementaciones.

Queda pendiente comprobar el funcionamiento conjunto de la aplicación en el entorno de los demás integrantes.

## 5. Advertencias de Git

Durante la preparación de los cambios aparecieron avisos indicando que algunos archivos pasarían de finales de línea LF a CRLF.

Estos avisos no impidieron añadir, confirmar ni publicar los archivos.

Las carpetas de compilación (`bin`, `obj`) y `.vs` permanecieron ignoradas por Git y no se incluyeron en el commit.

## 6. Estado de publicación

Los cambios se publicaron correctamente en GitHub.

- **Rama:** `producto3`
- **Commit:** `1a20775`
- **Estado local:** sin cambios pendientes.
- **Estado remoto:** sincronizado con `origin/producto3`.

## 7. Comprobaciones pendientes

- Revisar y actualizar las dependencias que presentan avisos de seguridad, coordinándolo con el equipo.
- Revisar la advertencia de autorización de `CuentaController`.
- Volver a ejecutar las pruebas unitarias en un entorno donde Windows permita cargar el ensamblado.
- Comprobar el inicio de sesión y los permisos de cada rol en la aplicación.
- Verificar las operaciones de reservas, cancelaciones y listas de espera con la base de datos.
- Comprobar el funcionamiento conjunto de la integración con Odoo y el resto de funcionalidades en el entorno compartido.

---

Este documento recoge las incidencias observadas durante el trabajo y las comprobaciones que todavía conviene realizar. No se consideran fallos confirmados aquellos casos en los que el entorno local impidió completar la prueba.

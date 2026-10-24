# GenteFit — FP.452 Técnicas de persistencia de datos con .NET y apps ERP

Proyecto de la asignatura FP.452 de la UOC. Aplicación de gestión para el centro deportivo GenteFit, desarrollada con .NET, Entity Framework Core y SQL Server.

## Integrantes

- Robert Jose Junior Rizquez Alicano
- Carlos Torras Consolación
- Argos Anton Panchón

## Descripción del producto

Este repositorio contiene el Producto 2 de la asignatura: la capa de acceso a datos de la aplicación GenteFit, implementada con Entity Framework Core sobre SQL Server. Incluye una demo de consola que permite realizar las operaciones básicas de alta, baja, consulta y modificación de clientes, así como la exportación e importación de datos en formato XML.

## Estructura del repositorio

genteFit/
  src/
    GenteFit.Datos/       Biblioteca de clases con las entidades, su configuración, los errores, el repositorio, el DbContext y las migraciones
    GenteFit.Demo/        Aplicación de consola con el menú CRUD y el intercambio XML
    GenteFit.Web/         Proyecto web MVC (reservado para el Producto 3)
  docs/                   Documentación del producto
  README.md               Este archivo

## Requisitos previos

Antes de ejecutar el proyecto, es necesario tener instalado lo siguiente:

### 1. Visual Studio 2022

1. Descarga Visual Studio Community 2022 desde https://visualstudio.microsoft.com/downloads/
2. Ejecuta el instalador y marca la carga de trabajo "Desarrollo de escritorio de .NET" (.NET desktop development).
3. Pulsa "Instalar" y espera a que termine.

### 2. SQL Server Express LocalDB

1. Abre el Visual Studio Installer.
2. Pulsa "Modificar" en tu instalación de Visual Studio.
3. En "Componentes individuales", busca "SQL Server Express LocalDB" y marca la casilla.
4. Pulsa "Modificar" para instalar.

Para verificar que se instaló correctamente, abre una terminal y ejecuta:

sqllocaldb info

Si responde con MSSQLLocalDB, está correctamente instalado.

### 3. .NET SDK

Para verificar si ya lo tienes instalado, abre una terminal y ejecuta:

dotnet --version

Si responde con un número (por ejemplo, 8.0.xxx o 10.0.xxx), ya lo tienes. Si no, instálalo desde https://dotnet.microsoft.com/download

## Instalación y ejecución

### Paso 1: Clonar el repositorio

1. Abre Visual Studio.
2. En la pantalla de inicio, selecciona "Clonar un repositorio".
3. Pega la URL del repositorio de GitHub del grupo.
4. Elige una carpeta local donde guardar el proyecto.
5. Pulsa "Clonar".
6. Abre el archivo genteFit.sln (o el nombre del archivo de solución) haciendo doble clic sobre él en el Explorador de soluciones.

### Paso 2: Restaurar los paquetes NuGet

1. En el Explorador de soluciones, haz clic derecho sobre la solución.
2. Selecciona "Restaurar paquetes NuGet".
3. Espera a que termine.

Si no aparece la opción o falla, ve a Herramientas > Administrador de paquetes NuGet > Configuración del Administrador de paquetes y asegúrate de que las dos opciones de "Restauración de paquetes" están marcadas.

### Paso 3: Verificar la cadena de conexión

En Windows no hay que tocar nada: si no existe la variable de entorno GENTEFIT_BD, GenteFitContext usa LocalDB (Server=(localdb)\MSSQLLocalDB;Database=GenteFitDB). En macOS, ver el apartado "Conexión a la base de datos" más abajo.

### Paso 4: Crear la base de datos

Este paso hay que hacerlo una sola vez en cada ordenador. Crea la base de datos GenteFitDB con las 9 tablas del modelo y los datos iniciales.

1. Abre una terminal (PowerShell o CMD).
2. Navega hasta la carpeta src del proyecto:

cd C:\Proyectos\genteFit\src

(Ajusta la ruta según donde hayas clonado).

3. Ejecuta este comando:

dotnet ef database update --project GenteFit.Datos\GenteFit.Datos.csproj --startup-project GenteFit.Datos\GenteFit.Datos.csproj

4. Espera. Si funciona, verás un mensaje como Applying migration '..._ModeloInicial' y luego Done.

Si el comando dotnet ef no se reconoce, ejecuta primero:

dotnet tool install --global dotnet-ef

Y luego repite el paso 3.

### Paso 5: Establecer GenteFit.Demo como proyecto de inicio

1. En el Explorador de soluciones, haz clic derecho sobre el proyecto GenteFit.Demo.
2. Selecciona "Establecer como proyecto de inicio".
3. Comprueba que en la barra de herramientas superior de Visual Studio, el desplegable de proyecto de inicio dice GenteFit.Demo.

### Paso 6: Ejecutar la aplicación

1. Pulsa Ctrl+F5 (ejecutar sin depurar).
2. Se abrirá una ventana de consola con el menú:

--- GENTEFIT ---
1. Alta de cliente
2. Baja de cliente
3. Consulta de clientes
4. Modificación de cliente
5. Exportar a XML
6. Importar desde XML
0. Salir
Opción:

3. Prueba cada opción:
   - 1: escribe un nombre y email. Debería decir "Cliente añadido".
   - 3: debería listar el cliente que acabas de crear.
   - 5: genera un archivo clientes.xml en la carpeta de ejecución.
   - 6: importa los clientes de ese XML.

## Capa de persistencia (GenteFit.Datos)

### Qué he implementado

He convertido en código el diagrama de clases y el modelo entidad-relación del Producto 1:

- Las 8 entidades del dominio (Usuario, Cliente, Actividad, Monitor, Sala, Sesion, Reserva y LoteIntercambio) y sus 6 enumeraciones (Rol, TipoActividad, NivelIntensidad, EstadoSesion, EstadoReserva y TipoLote).
- Una clase de configuración con Fluent API por cada entidad, que traslada a la base de datos los detalles del modelo entidad-relación: nombres de tabla, longitudes, índices únicos, claves foráneas, la tabla intermedia `monitor_actividad`, restricciones CHECK y datos iniciales.
- El contexto `GenteFitContext` completo, con los 8 `DbSet`.
- Las excepciones de negocio E-01 a E-06 y un traductor que convierte los errores de SQL Server en esas excepciones.
- La interfaz `IRepositorio<T>` y su implementación genérica `RepositorioEF<T>`.
- La migración `ModeloInicial`, que crea la base de datos completa, y el script SQL equivalente en `docs/GenteFitDB.sql`.

### Cómo he organizado el proyecto

```
src/GenteFit.Datos/
  Entidades/        Las 8 clases del dominio y los enums (Enums.cs)
  Configuracion/    Una clase IEntityTypeConfiguration por entidad (ClienteConfig, SesionConfig...)
  Errores/          Excepciones.cs y TraductorErroresSql.cs
  Repositorios/     IRepositorio.cs y RepositorioEF.cs
  Migrations/       La migración ModeloInicial (la genera Entity Framework)
  GenteFitContext.cs
docs/
  GenteFitDB.sql    Script SQL de la base de datos completa
```

Todas las clases están en el namespace `GenteFit.Datos`, el mismo que ya usaban la demo y la web, para no tener que cambiar nada en esos proyectos. Las carpetas solo sirven para ordenar.

### Decisiones que he tomado

- **Fluent API en lugar de anotaciones de datos.** Así las entidades quedan limpias, solo con los datos del diagrama de clases, y puedo configurar cosas que las anotaciones no permiten, como el índice filtrado de reservas, el comportamiento al borrar o la tabla `monitor_actividad`.
- **Nombres en snake_case, como en el modelo ER.** He usado el paquete `EFCore.NamingConventions`, que convierte automáticamente columnas, claves e índices (`FechaHoraInicio` pasa a `fecha_hora_inicio`). Los nombres de las tablas los fijo con `ToTable()` para que estén en singular, como en el ER (`cliente`, `lote_intercambio`). Descarté poner cada nombre a mano con `HasColumnName` porque eran unas 60 líneas más y fáciles de equivocar.
- **NVARCHAR en lugar de VARCHAR.** El ER indica `VARCHAR`, pero mantengo `NVARCHAR` (Unicode) para que se guarden bien acentos, «ñ», «ç» y apóstrofes, que es lo que pide la rúbrica. Los datos iniciales ya los incluyen a propósito: «Sala Montjuïc», «Sala d'Spinning», «Núñez O'Connor».
- **Enums guardados como texto.** En la base de datos se ve `CONFIRMADA` en lugar de `0`, y reordenar un enum no estropea los datos guardados.
- **Identificadores.** Cada tabla tiene un `id` numérico como clave primaria, y las claves naturales (DNI y email del cliente, email del monitor, nombre de sala, código de lote) son índices únicos.
- **He sustituido la migración `InitialCreate` por `ModeloInicial`.** La anterior solo creaba una tabla `Clientes` con otras columnas, y como aún no había datos reales era más limpio empezar con una única migración que refleje el modelo del P1.
- **He añadido la excepción de E-06** (`ActividadConSesionesException`), que estaba en la tabla de errores del P1 pero no en el diagrama de excepciones, y una `BaseDatosException` para los errores técnicos (BD-01 a BD-04).

### Qué garantiza la base de datos

| Regla | Cómo la he implementado | Error |
|---|---|---|
| DNI y email del cliente no se repiten | Índices únicos en `dni` y `email` | E-04 |
| No hay dos sesiones en la misma sala a la misma hora | Índice único `(id_sala, fecha_hora_inicio)` | E-05 |
| Un cliente no tiene dos reservas vivas en la misma sesión | Índice único filtrado `(id_sesion, id_cliente) WHERE estado <> 'CANCELADA'` | E-01 |
| No se borra una actividad, sala o monitor con sesiones | `OnDelete(DeleteBehavior.Restrict)` en las claves foráneas de `sesion` | E-06 |
| La duración y el aforo son mayores que 0 | Restricciones CHECK `ck_actividad_duracion` y `ck_sala_aforo` | BD-03 |

### Cómo traduzco los errores

Si SQL Server rechaza una operación, lanza una `SqlException` con un mensaje técnico en inglés. `GenteFitContext` sobrescribe `SaveChanges` para capturarla y `TraductorErroresSql` la convierte en una excepción de GenteFit con su código y un mensaje en español, según el número de error y el nombre del índice o la restricción:

| Error de SQL Server | Excepción | Código |
|---|---|---|
| 2601 / 2627 en el índice de sesión | `SolapamientoSesionException` | E-05 |
| 2601 / 2627 en el índice de reserva | `ReservaDuplicadaException` | E-01 |
| 2601 / 2627 en cualquier otro índice único | `DatosDuplicadosException` | E-04 |
| 515: falta un dato obligatorio (columna `NOT NULL`) | `BaseDatosException` | E-04 |
| 2628 / 8152: el valor es más largo de lo que admite la columna (por ejemplo, un DNI de más de 9 caracteres) | `BaseDatosException` | BD-05 |
| 547 al borrar una actividad con sesiones | `ActividadConSesionesException` | E-06 |
| 547 al borrar otro registro con dependencias | `BaseDatosException` | BD-02 |
| 547 por una restricción CHECK | `BaseDatosException` | BD-03 |
| 547 por una clave foránea inexistente | `BaseDatosException` | BD-04 |
| Errores de conexión (servidor apagado o inaccesible, tiempo agotado, login incorrecto) | `BaseDatosException` | BD-01 |
| Cualquier otro (el mensaje incluye el texto original de SQL Server) | `BaseDatosException` | BD-99 |

Todas heredan de `GenteFitException`, que tiene la propiedad `Codigo`, así que basta con un `catch (GenteFitException ex)` para atraparlas. El error original queda guardado en `InnerException`. `RepositorioEF<T>` también traduce los errores que saltan al consultar, por ejemplo si la base de datos no está disponible.

### Cómo se usa el repositorio

```csharp
using var contexto = new GenteFitContext();
var clientes = new RepositorioEF<Cliente>(contexto);

var cliente = clientes.ObtenerPorId(1);          // obtener
var activos = clientes.Listar(c => c.Activo);    // consultar con filtro
clientes.Anyadir(new Cliente { Dni = "12345678Z", Nombre = "Ana", Apellidos = "García", Email = "ana@correo.com" });
contexto.SaveChanges();                          // grabar
```

`Anyadir`, `Actualizar` y `Eliminar` solo apuntan el cambio; se guarda todo junto, en una transacción, al llamar a `SaveChanges()`.

### Conexión a la base de datos

`GenteFitContext` lee la cadena de conexión de la variable de entorno `GENTEFIT_BD`. Si no existe, usa LocalDB. Así el mismo código funciona en todos los equipos y no hay ninguna contraseña en el repositorio.

- **Windows:** no hay que configurar nada. Se usa LocalDB, igual que antes.
- **macOS:** SQL Server no se puede instalar en macOS, así que lo ejecuto en Docker y guardo la cadena de conexión en `~/.zshrc`. Hay que usar una contraseña propia, sin `!`, `$` ni `;`:

```bash
docker run --platform linux/amd64 -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=<contraseña>" -p 1433:1433 --name gentefit-sql -d mcr.microsoft.com/mssql/server:2022-latest
```

```bash
echo 'export GENTEFIT_BD="Server=localhost,1433;Database=GenteFitDB;User Id=sa;Password=<contraseña>;TrustServerCertificate=True;Connect Timeout=5"' >> ~/.zshrc
```

### Cómo poner al día la base de datos después de mi cambio

Como he sustituido `InitialCreate` por `ModeloInicial`, quien ya tuviera la base de datos creada tiene que borrarla y volver a crearla una vez. Desde la carpeta `src`:

```bash
git switch master
git pull
dotnet ef database drop --project GenteFit.Datos --force
dotnet ef database update --project GenteFit.Datos
```

En Visual Studio también se puede hacer desde la Consola del Administrador de paquetes, con GenteFit.Datos como proyecto predeterminado:

```
Drop-Database -Force
Update-Database
```

Quedan 9 tablas (`usuario`, `cliente`, `actividad`, `monitor`, `monitor_actividad`, `sala`, `sesion`, `reserva` y `lote_intercambio`) con estos datos iniciales: 6 actividades, 2 salas, 2 monitores y 3 sesiones en noviembre de 2026.

Para volver a dejar la base de datos como recién creada después de hacer pruebas, basta con repetir los dos comandos `drop` y `update`.

### Migraciones

Las migraciones las creo yo, para que no se dupliquen ni entren en conflicto. Si hace falta un cambio en el modelo, me lo decís. Los demás solo tenéis que aplicarlas con `dotnet ef database update`. Cada vez que creo una migración nueva, regenero el script SQL:

```bash
dotnet ef migrations script --project GenteFit.Datos -o ../docs/GenteFitDB.sql
```

La base de datos no se sube al repositorio. Lo que se comparte es el código y las migraciones, y cada uno crea su propia base de datos con ellas.

### Cambios que necesita la demo

Con el modelo nuevo, la demo compila, pero hay que adaptarla:

1. El alta de cliente tiene que pedir también DNI y apellidos, que ahora son obligatorios.
2. La baja tiene que ser lógica (`FechaBaja = DateTime.Today` y `Activo = false`), no un `Remove`.
3. Cada opción del menú debería capturar `GenteFitException` y mostrar `[{ex.Codigo}] {ex.Message}`, para que se vea la reacción a errores sin que el programa se cierre.

## Notas

- La base de datos se crea sola con el Paso 4. No hace falta instalar SQL Server Management Studio ni crear tablas a mano.
- El archivo clientes.xml se genera en GenteFit.Demo\bin\Debug\net10.0\ (o la versión de .NET que uses) cuando se usa la opción 5. La opción 6 lo lee desde ahí.
- Si Visual Studio muestra warnings sobre vulnerabilidades en paquetes NuGet, son advertencias, no errores. No impiden que el proyecto funcione.
- Si algo falla, lo primero es comprobar que LocalDB responde (sqllocaldb info) y que los paquetes NuGet se restauraron correctamente.

## Documentación adicional

- Documento comparativo de ORM y componentes XML: docs/comparativa-orm.md (pendiente de añadir)
- Enlace al vídeo demo: (pendiente de añadir)

## Uso de inteligencia artificial

En este producto se ha utilizado IA generativa como herramienta de apoyo para la revisión de código y la redacción de documentación. Todo el contenido generado ha sido revisado y validado por los integrantes del grupo.

## Pendiente

### Base de datos

- [ ] Unir la rama `persistencia` a `master`.
- [ ] Recrear la base de datos en cada equipo con `dotnet ef database drop` y `dotnet ef database update`, porque la migración `InitialCreate` se ha sustituido por `ModeloInicial`.

### Demo

- [ ] Pedir también DNI y apellidos en el alta de cliente.
- [ ] Cambiar la baja de cliente por una baja lógica (`FechaBaja` y `Activo = false`) en lugar de `Remove`.
- [ ] Capturar `GenteFitException` en cada opción del menú y mostrar el código y el mensaje del error.
- [ ] Añadir opciones que demuestren los errores de la base de datos: DNI duplicado, clave foránea inexistente, borrar una actividad con sesiones y base de datos no disponible.
- [ ] Actualizar en este README el menú de la demo y la ruta de los ficheros XML.

### XML

- [ ] Crear las carpetas `xml/entrada/` (ficheros de prueba para importar) y `xml/salida/` (ficheros exportados).
- [ ] Usar rutas fijas para importar y exportar, que no dependan de la carpeta desde la que se ejecuta el programa.
- [ ] Añadir `xml/salida/` al `.gitignore`.
- [ ] Preparar los ficheros de prueba: uno correcto con acentos y apóstrofes, uno mal formado, uno con una codificación incorrecta y uno con datos erróneos.
- [ ] Demostrar la tolerancia a errores: fuente no disponible, XML mal formado y caracteres especiales.
- [ ] Decidir qué hacer con `xml/clientes.xml`, que ahora está vacío.

### Documentación y entrega

- [ ] Redactar la comparativa de más de 3 componentes ORM y más de 3 componentes XML, con la justificación de los elegidos (PDF de 3 a 4 páginas).
- [ ] Añadir el documento comparativo a `docs/` y actualizar su enlace en «Documentación adicional».
- [ ] Indicar en el PDF que el intercambio con Odoo es en XML y no en CSV, como aparece por error en el Producto 1.
- [ ] Incluir la declaración de uso de IA.
- [ ] Grabar el vídeo de unos 5 minutos con la participación de los tres, subirlo a YouTube como oculto y preparar el documento con el enlace.
- [ ] Añadir el vídeo al Site del grupo y su enlace a este README.
- [ ] Preparar el ZIP de la solución, sin las carpetas `bin`, `obj` ni contraseñas.
- [ ] Actualizar el acta con las decisiones del Producto 2.

### Para el Producto 3

- [ ] Implementar los métodos del dominio del diagrama de clases (`Sesion.AnyadirReserva()`, `Sesion.CancelarReserva()`, `Cliente.DarDeBaja()`…).
- [ ] Implementar los servicios de la capa de negocio (`ServicioReservas`, `ServicioClientes`, `ServicioActividades`…).
- [ ] Adaptar `GenteFit.Web` al modelo nuevo.
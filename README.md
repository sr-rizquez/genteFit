# GenteFit — FP.452 Técnicas de persistencia de datos con .NET y apps ERP

Proyecto de la asignatura FP.452 de la UOC. Aplicación de gestión para el centro deportivo GenteFit, desarrollada con .NET 10, Entity Framework Core 10 y SQL Server.

## Integrantes

- Robert Jose Junior Rizquez Alicano
- Carlos Torras Consolación
- Argos Anton Panchón

## Qué contiene el Producto 2

La capa de acceso a datos de GenteFit, con Entity Framework Core sobre SQL Server, y una demo de consola para probarla:

- Alta, baja lógica, consulta y modificación de clientes contra la base de datos.
- Exportación e importación de clientes en XML, con tolerancia a errores.
- Una opción que provoca a propósito errores de la base de datos para ver cómo se tratan.
- Los prototipos con los que comparamos cinco componentes XML.

## Estructura del repositorio

```
genteFit/
  src/
    GenteFit.Datos/       Entidades, configuración, errores, repositorio, DbContext y migraciones
    GenteFit.Demo/        Demo de consola (menú CRUD, XML y pruebas de errores) y la solución GenteFit.Demo.slnx
    GenteFit.Web/         Proyecto web MVC (reservado para el Producto 3)
  prototipos/xml/         Comparativa de XmlDocument, LINQ to XML, XmlReader, XmlSerializer y DataSet
  xml/entrada/            Ficheros XML de prueba para importar
  docs/GenteFitDB.sql     Script SQL de la base de datos completa
```

## Cómo ejecutar el programa

Hacen falta tres cosas: el SDK de .NET 10, un SQL Server y la base de datos creada con la migración. Los pasos cambian según el sistema operativo; sigue solo el apartado del tuyo.

La cadena de conexión no está en el código. El programa lee la variable de entorno `GENTEFIT_BD` y, si no existe, usa SQL Server LocalDB (solo en Windows). Por eso en el repositorio no hay ninguna contraseña.

### Windows

**1. Instalar el SDK de .NET 10**

Descárgalo de https://dotnet.microsoft.com/download/dotnet/10.0 (apartado SDK, instalador x64) y ejecútalo. Para comprobarlo, abre PowerShell y escribe:

```powershell
dotnet --version
```

Tiene que salir un número que empiece por `10.`.

**2. Instalar SQL Server Express LocalDB**

Si tienes Visual Studio 2026, abre el Visual Studio Installer, pulsa «Modificar», ve a «Componentes individuales», marca «SQL Server Express LocalDB» y pulsa «Modificar».

Sin Visual Studio, descarga SQL Server Express desde https://www.microsoft.com/sql-server/sql-server-downloads, ejecútalo, elige «Descargar medios», marca «LocalDB» e instala el fichero `SqlLocalDB.msi` que se descarga.

Para comprobarlo:

```powershell
sqllocaldb info
```

Tiene que salir `MSSQLLocalDB`. Con LocalDB no hay que configurar la variable `GENTEFIT_BD`.

**3. Descargar el proyecto**

```powershell
git clone https://github.com/sr-rizquez/genteFit.git
cd genteFit
```

También se puede descomprimir el ZIP de la entrega y abrir PowerShell dentro de la carpeta `genteFit`.

**4. Instalar la herramienta de migraciones y crear la base de datos**

```powershell
dotnet tool install --global dotnet-ef
dotnet ef database update --project src\GenteFit.Datos
```

Si `dotnet ef` no se reconoce, cierra PowerShell y ábrelo otra vez. Al final tiene que salir `Applying migration '20261024173844_ModeloInicial'` y `Done.`

**5. Ejecutar la demo**

```powershell
dotnet run --project src\GenteFit.Demo
```

Con Visual Studio 2026: abre `src\GenteFit.Demo\GenteFit.Demo.slnx`, haz clic derecho en GenteFit.Demo, «Establecer como proyecto de inicio», y pulsa Ctrl+F5.

### macOS

SQL Server no se instala en macOS, así que se ejecuta en Docker.

**1. Instalar el SDK de .NET 10**

Descarga el instalador para macOS (Arm64 en Mac con Apple Silicon, x64 en Mac con Intel) desde https://dotnet.microsoft.com/download/dotnet/10.0 y ejecútalo. Comprueba en la Terminal:

```bash
dotnet --version
```

**2. Instalar Docker Desktop**

Descárgalo de https://www.docker.com/products/docker-desktop/ y ábrelo. En Mac con Apple Silicon, entra en Settings → General y marca «Use Rosetta for x86_64/amd64 emulation on Apple Silicon». Pulsa «Apply & restart».

**3. Arrancar SQL Server**

Elige una contraseña para el usuario `sa`. Tiene que tener al menos 8 caracteres con mayúsculas, minúsculas y números, y sin `!`, `$` ni `;`. Sustituye `<contraseña>` por ella:

```bash
docker run --platform linux/amd64 -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=<contraseña>" -p 1433:1433 --name gentefit-sql -d mcr.microsoft.com/mssql/server:2022-latest
```

La primera vez tarda unos minutos en descargar la imagen. Comprueba que está en marcha con `docker ps`: tiene que aparecer `gentefit-sql`. Si después reinicias el Mac, vuelve a arrancarlo con `docker start gentefit-sql`.

**4. Guardar la cadena de conexión**

Con la misma contraseña:

```bash
echo 'export GENTEFIT_BD="Server=localhost,1433;Database=GenteFitDB;User Id=sa;Password=<contraseña>;TrustServerCertificate=True;Connect Timeout=5"' >> ~/.zshrc
source ~/.zshrc
```

**5. Descargar el proyecto**

```bash
git clone https://github.com/sr-rizquez/genteFit.git
cd genteFit
```

O descomprime el ZIP de la entrega y entra en la carpeta `genteFit`.

**6. Instalar la herramienta de migraciones y crear la base de datos**

```bash
dotnet tool install --global dotnet-ef
export PATH="$PATH:$HOME/.dotnet/tools"
dotnet ef database update --project src/GenteFit.Datos
```

Al final tiene que salir `Applying migration '20261024173844_ModeloInicial'` y `Done.`

**7. Ejecutar la demo**

```bash
dotnet run --project src/GenteFit.Demo
```

También se puede abrir con Visual Studio Code (extensión C# Dev Kit) o con Rider.

### Linux

Los pasos son los mismos que en macOS, con SQL Server en Docker. Están probados para Ubuntu y Debian.

**1. Instalar el SDK de .NET 10**

En Ubuntu 24.04 o posterior:

```bash
sudo apt-get update
sudo apt-get install -y dotnet-sdk-10.0
```

Si tu distribución no lo tiene en sus repositorios, sigue https://learn.microsoft.com/dotnet/core/install/linux. Comprueba con `dotnet --version`.

**2. Instalar Docker**

```bash
sudo apt-get install -y docker.io
sudo systemctl enable --now docker
sudo usermod -aG docker $USER
```

Cierra la sesión y vuelve a entrar para poder usar `docker` sin `sudo`.

**3. Arrancar SQL Server**

Con la contraseña que elijas (las mismas reglas que en macOS):

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=<contraseña>" -p 1433:1433 --name gentefit-sql -d mcr.microsoft.com/mssql/server:2022-latest
```

**4. Guardar la cadena de conexión**

```bash
echo 'export GENTEFIT_BD="Server=localhost,1433;Database=GenteFitDB;User Id=sa;Password=<contraseña>;TrustServerCertificate=True;Connect Timeout=5"' >> ~/.bashrc
source ~/.bashrc
```

**5 a 7. Descargar el proyecto, crear la base de datos y ejecutar la demo**

Igual que en macOS, pasos 5, 6 y 7.

### Qué tiene que salir

Al ejecutar la demo aparece este menú:

```
GENTEFIT · Gestión de clientes

CLIENTES
  1. Alta de cliente
  2. Baja de cliente
  3. Consulta de clientes
  4. Modificación de cliente

INTERCAMBIO XML
  5. Exportar clientes a XML
  6. Importar clientes desde XML

PRUEBAS
  7. Probar errores de la base de datos

  0. Salir

Elige una opción:
```

Los errores salen en rojo y los mensajes correctos en verde. Ningún error cierra el programa.

- **1:** alta de cliente. Los campos con * son obligatorios y cada uno se valida al escribirlo: DNI con la letra correcta, nombre y apellidos solo con letras, email con @ y teléfono solo con números. Escribe 0 para cancelar. DNI válidos para probar: 12345678Z, 11111111H, 22222222J.
- **2:** baja lógica. El cliente no se borra: se le pone la fecha de baja y queda inactivo.
- **3:** lista los clientes con su estado y cuántos están activos.
- **4:** modifica un cliente. Dejar un campo vacío lo conserva.
- **5:** exporta todos los clientes a `xml/salida/clientes.xml`.
- **6:** importa un fichero de `xml/entrada/`. Prueba `clientes.xml` (correcto, con acentos y apóstrofes), `clientes_datos_erroneos.xml`, `clientes_mal_formado.xml`, `clientes_codificacion.xml`, `clientes_estructura_cambiada.xml` y un nombre que no exista, por ejemplo `no_existe`.
- **7:** intenta guardar datos incorrectos sin validarlos, para ver cómo responde la base de datos. No se guarda nada.

Para probar la base de datos no disponible, en macOS o Linux para SQL Server con `docker stop gentefit-sql` y usa la opción 3: sale `[BD-01]` y el programa sigue funcionando. Después, `docker start gentefit-sql`.

### Prototipos XML

Ejecutan los cinco componentes comparados con los mismos ficheros y guardan los resultados en `prototipos/xml/resultados.md`. No necesitan base de datos:

```bash
dotnet run --project prototipos/xml
```

### Si algo falla

| Qué pasa | Qué hacer |
|---|---|
| `dotnet` no se reconoce | Cierra y vuelve a abrir la terminal después de instalar el SDK |
| `dotnet ef` no se reconoce | En macOS y Linux, `export PATH="$PATH:$HOME/.dotnet/tools"`. En Windows, cierra y vuelve a abrir PowerShell |
| Sale `[BD-01]` al arrancar o al consultar | SQL Server no está en marcha. Windows: `sqllocaldb start MSSQLLocalDB`. macOS o Linux: `docker start gentefit-sql` |
| `Login failed for user 'sa'` | La contraseña de `GENTEFIT_BD` no coincide con la del contenedor. Revisa `~/.zshrc` o `~/.bashrc` y abre una terminal nueva |
| El contenedor se para solo nada más arrancar | La contraseña no cumple los requisitos. Bórralo con `docker rm gentefit-sql` y repite el `docker run` con otra |
| En Mac, el contenedor no arranca | Activa Rosetta en Docker Desktop (paso 2 de macOS) |
| `Invalid object name 'cliente'` | Falta crear la base de datos: repite `dotnet ef database update --project src/GenteFit.Datos` |
| Avisos de vulnerabilidades de paquetes NuGet al compilar | Son advertencias, no errores. No impiden ejecutar el programa |

Para dejar la base de datos como recién creada:

```bash
dotnet ef database drop --project src/GenteFit.Datos --force
dotnet ef database update --project src/GenteFit.Datos
```

Si no se puede usar `dotnet ef`, el script `docs/GenteFitDB.sql` crea la misma base de datos desde cualquier cliente de SQL Server (por ejemplo, SQL Server Management Studio o la extensión SQL Server de Visual Studio Code).

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

`GenteFitContext` lee la cadena de conexión de la variable de entorno `GENTEFIT_BD`. Si no existe, usa LocalDB. Así el mismo código funciona en todos los equipos y no hay ninguna contraseña en el repositorio. Los pasos para cada sistema están en «Cómo ejecutar el programa».

### Migraciones

Las migraciones las creo yo, para que no se dupliquen ni entren en conflicto. Si hace falta un cambio en el modelo, me lo decís. Los demás solo tenéis que aplicarlas con `dotnet ef database update`. Cada vez que creo una migración nueva, regenero el script SQL:

```bash
dotnet ef migrations script --project GenteFit.Datos -o ../docs/GenteFitDB.sql
```

La base de datos no se sube al repositorio. Lo que se comparte es el código y las migraciones, y cada uno crea su propia base de datos con ellas.

### Cómo he adaptado la demo

- Cada opción del menú está dentro de un `try` / `catch (GenteFitException)` que muestra el código y el mensaje en rojo y vacía el `ChangeTracker`, para que un guardado fallido no afecte a la siguiente operación.
- Todas las opciones usan `RepositorioEF<Cliente>` en lugar de acceder directamente al contexto, así los errores de conexión también salen traducidos.
- El alta y la modificación validan cada campo antes de guardarlo. El DNI o el email repetidos no se pueden comprobar al escribir, así que los rechaza la base de datos (E-04): el programa valida el formato y la base de datos garantiza que no se repitan.
- La opción 7 guarda a propósito datos sin validar para enseñar la reacción de la base de datos: dato obligatorio vacío (E-04), valor demasiado largo (BD-05), DNI repetido (E-04), clave foránea inexistente (BD-04), restricción CHECK (BD-03) y borrar una actividad con sesiones (E-06).

## Intercambio XML

He usado LINQ to XML (`XDocument`), que viene incluido en .NET. Los ficheros están en la carpeta `xml` del repositorio, que el programa busca subiendo desde su carpeta de ejecución, así que funciona igual con `dotnet run` y con Visual Studio:

```
xml/
  entrada/                       Ficheros de prueba para importar (se suben al repositorio)
    clientes.xml                   3 clientes correctos, con acentos, ñ, ç y apóstrofes
    clientes_datos_erroneos.xml    8 clientes: 1 correcto y 7 con un error distinto cada uno
    clientes_mal_formado.xml       Un & sin escapar y una etiqueta sin cerrar
    clientes_codificacion.xml      Guardado en ISO-8859-1 pero declara UTF-8
    clientes_estructura_cambiada.xml  Elementos en otro orden y datos que GenteFit no conoce
  salida/                        Lo que genera la exportación (excluido en .gitignore)
    clientes.xml
```

### Exportación

Escribe todos los clientes en `xml/salida/clientes.xml`, en UTF-8, con el DNI como atributo y el resto de datos como elementos. Los campos vacíos (teléfono, fecha de baja) no se escriben. `XDocument` escapa automáticamente los caracteres especiales de XML (`&`, `<`, `>`), y los acentos y apóstrofes se guardan tal cual.

### Importación

Distingue dos niveles de error:

- **Errores que impiden leer el fichero:** no se importa nada y se muestra el error.
- **Errores en un cliente concreto:** se descarta ese cliente, indicando su línea en el fichero y el motivo, y se importan los demás. Cada cliente se guarda por separado, así que uno incorrecto no impide guardar el resto.

Cada cliente se valida con las mismas reglas que el alta. La importación solo añade clientes nuevos: si un DNI ya existe en la base de datos o está repetido en el fichero, el cliente se descarta y los datos que ya había no se modifican. Por eso importar el mismo fichero dos veces no duplica nada.

El fichero se abre con `DtdProcessing.Prohibit` para evitar ataques XXE, y con `LoadOptions.SetLineInfo` para poder indicar la línea de cada error.

| Error | Código |
|---|---|
| El fichero no existe, no hay permiso para leerlo o no se puede abrir | X-01 |
| El XML está mal formado o tiene una codificación incorrecta (indica línea y posición) | X-02 |
| La raíz del fichero no es `<clientes>` | X-03 |
| No se puede escribir el fichero exportado | X-04 |

Resultado de los ficheros de prueba:

| Fichero | Resultado |
|---|---|
| `clientes.xml` | Importados 3 de 3 |
| `clientes_datos_erroneos.xml` | Importado 1 de 8; se descartan la letra del DNI incorrecta, el nombre con números, el email que falta, el email sin @, el teléfono con letras, el DNI que falta y el DNI repetido |
| `clientes_mal_formado.xml` | X-02 en la línea 5 |
| `clientes_codificacion.xml` | X-02: carácter no válido en la codificación |
| `clientes_estructura_cambiada.xml` | Importados 2 de 2: se ignoran los elementos y atributos desconocidos y el orden no importa |
| `no_existe.xml` | X-01 |
| `clientes.xml` por segunda vez | Importados 0 de 3: ya existen |

## Documentación adicional

- Comparativa de ORM y componentes XML y justificación: PDF `producto2Justificacion(P59)` de la entrega.
- Resultados de los prototipos XML: `prototipos/xml/resultados.md`.
- Vídeo demostrativo: enlace en el documento `producto2VideoDemo(P59)` de la entrega.

## Uso de inteligencia artificial

En este producto se ha utilizado IA generativa como herramienta de apoyo para la revisión de código.

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
    GenteFit.Datos/       Biblioteca de clases con la entidad Cliente, el DbContext y las migraciones
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

1. En el Explorador de soluciones, despliega el proyecto GenteFit.Datos.
2. Abre el archivo GenteFitContext.cs.
3. Comprueba que la línea que contiene UseSqlServer(...) tiene exactamente esta cadena:

optionsBuilder.UseSqlServer(@"Server=(localdb)\MSSQLLocalDB;Database=GenteFitDB;Trusted_Connection=True;TrustServerCertificate=True;");

Si ya está así, no toques nada. Si es distinta, cámbiala por esa y guarda el archivo.

### Paso 4: Crear la base de datos

Este paso hay que hacerlo una sola vez en cada ordenador. Crea la base de datos GenteFitDB y la tabla Clientes.

1. Abre una terminal (PowerShell o CMD).
2. Navega hasta la carpeta src del proyecto:

cd C:\Proyectos\genteFit\src

(Ajusta la ruta según donde hayas clonado).

3. Ejecuta este comando:

dotnet ef database update --project GenteFit.Datos\GenteFit.Datos.csproj --startup-project GenteFit.Datos\GenteFit.Datos.csproj

4. Espera. Si funciona, verás un mensaje como Applying migration '..._InitialCreate' y luego Done.

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

## Notas

- La base de datos se crea sola con el Paso 4. No hace falta instalar SQL Server Management Studio ni crear tablas a mano.
- El archivo clientes.xml se genera en GenteFit.Demo\bin\Debug\net10.0\ (o la versión de .NET que uses) cuando se usa la opción 5. La opción 6 lo lee desde ahí.
- Si Visual Studio muestra warnings sobre vulnerabilidades en paquetes NuGet, son advertencias, no errores. No impiden que el proyecto funcione.
- Si algo falla, lo primero es comprobar que LocalDB responde (sqllocaldb info) y que los paquetes NuGet se restauraron correctamente.

## Documentación adicional

- Documento comparativo de ORM y componentes XML: docs/comparativa-orm.md
- Enlace al vídeo demo: (pendiente de añadir)

## Uso de inteligencia artificial

En este producto se ha utilizado IA generativa como herramienta de apoyo para la revisión de código y la redacción de documentación. Todo el contenido generado ha sido revisado y validado por los integrantes del grupo.
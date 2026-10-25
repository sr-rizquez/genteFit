# Resultados de los prototipos XML

Ejecutado el 25/10/2026 18:24 con .NET 10.0.13.

## Fichero correcto con acentos y apóstrofes

Fichero: `clientes.xml`

| Componente | Clientes leídos | Sin DNI o email | Primer cliente | Tiempo (ms) | Memoria asignada (MB) | Resultado |
| --- | --- | --- | --- | --- | --- | --- |
| XmlDocument | 3 | 0 | Àngels D'Alòs Núñez | 0 | 0,0 | Correcto |
| LINQ to XML | 3 | 0 | Àngels D'Alòs Núñez | 0 | 0,0 | Correcto |
| XmlReader | 3 | 0 | Àngels D'Alòs Núñez | 0 | 0,0 | Correcto |
| XmlSerializer | 3 | 0 | Àngels D'Alòs Núñez | 0 | 0,0 | Correcto |
| DataSet | 3 | 0 | Àngels D'Alòs Núñez | 0 | 0,1 | Correcto |

## Estructura cambiada

Fichero: `clientes_estructura_cambiada.xml`

| Componente | Clientes leídos | Sin DNI o email | Primer cliente | Tiempo (ms) | Memoria asignada (MB) | Resultado |
| --- | --- | --- | --- | --- | --- | --- |
| XmlDocument | 2 | 0 | Jordi Cases Rovira | 0 | 0,0 | Correcto |
| LINQ to XML | 2 | 0 | Jordi Cases Rovira | 0 | 0,0 | Correcto |
| XmlReader | 2 | 0 | Jordi Cases Rovira | 0 | 0,0 | Correcto |
| XmlSerializer | 2 | 0 | Jordi Cases Rovira | 0 | 0,0 | Correcto |
| DataSet | - | - | - | 4 | - | Error: DataException: Column name 'origen' is defined for different mapping types. |

## Datos incorrectos (solo lectura, sin validar)

Fichero: `clientes_datos_erroneos.xml`

| Componente | Clientes leídos | Sin DNI o email | Primer cliente | Tiempo (ms) | Memoria asignada (MB) | Resultado |
| --- | --- | --- | --- | --- | --- | --- |
| XmlDocument | 8 | 2 | Laia Vidal Soler | 0 | 0,0 | Correcto |
| LINQ to XML | 8 | 2 | Laia Vidal Soler | 0 | 0,0 | Correcto |
| XmlReader | 8 | 2 | Laia Vidal Soler | 0 | 0,0 | Correcto |
| XmlSerializer | 8 | 2 | Laia Vidal Soler | 0 | 0,0 | Correcto |
| DataSet | 8 | 2 | Laia Vidal Soler | 0 | 0,1 | Correcto |

## XML mal formado

Fichero: `clientes_mal_formado.xml`

| Componente | Clientes leídos | Sin DNI o email | Primer cliente | Tiempo (ms) | Memoria asignada (MB) | Resultado |
| --- | --- | --- | --- | --- | --- | --- |
| XmlDocument | - | - | - | 1 | - | Error: XmlException: An error occurred while parsing EntityName. Line 5, position 19. |
| LINQ to XML | - | - | - | 0 | - | Error: XmlException: An error occurred while parsing EntityName. Line 5, position 19. |
| XmlReader | - | - | - | 0 | - | Error: XmlException: An error occurred while parsing EntityName. Line 5, position 19. |
| XmlSerializer | - | - | - | 2 | - | Error: InvalidOperationException → XmlException: An error occurred while parsing EntityName. Line 5, position 19. |
| DataSet | - | - | - | 0 | - | Error: XmlException: An error occurred while parsing EntityName. Line 5, position 19. |

## Codificación incorrecta

Fichero: `clientes_codificacion.xml`

| Componente | Clientes leídos | Sin DNI o email | Primer cliente | Tiempo (ms) | Memoria asignada (MB) | Resultado |
| --- | --- | --- | --- | --- | --- | --- |
| XmlDocument | - | - | - | 0 | - | Error: XmlException: Invalid character in the given encoding. Line 5, position 13. |
| LINQ to XML | - | - | - | 0 | - | Error: XmlException: Invalid character in the given encoding. Line 5, position 13. |
| XmlReader | - | - | - | 0 | - | Error: XmlException: Invalid character in the given encoding. Line 5, position 13. |
| XmlSerializer | - | - | - | 0 | - | Error: InvalidOperationException → XmlException: Invalid character in the given encoding. Line 5, position 13. |
| DataSet | - | - | - | 0 | - | Error: XmlException: Invalid character in the given encoding. Line 5, position 13. |

## Fichero inexistente

Fichero: `no_existe.xml`

| Componente | Clientes leídos | Sin DNI o email | Primer cliente | Tiempo (ms) | Memoria asignada (MB) | Resultado |
| --- | --- | --- | --- | --- | --- | --- |
| XmlDocument | - | - | - | 0 | - | Error: FileNotFoundException: Could not find file 'C:\Users\carlo\source\repos\genteFit\xml\entrada\no_existe.xml'. |
| LINQ to XML | - | - | - | 0 | - | Error: FileNotFoundException: Could not find file 'C:\Users\carlo\source\repos\genteFit\xml\entrada\no_existe.xml'. |
| XmlReader | - | - | - | 0 | - | Error: FileNotFoundException: Could not find file 'C:\Users\carlo\source\repos\genteFit\xml\entrada\no_existe.xml'. |
| XmlSerializer | - | - | - | 0 | - | Error: FileNotFoundException: Could not find file 'C:\Users\carlo\source\repos\genteFit\xml\entrada\no_existe.xml'. |
| DataSet | - | - | - | 0 | - | Error: FileNotFoundException: Could not find file 'C:\Users\carlo\source\repos\genteFit\xml\entrada\no_existe.xml'. |

## Fichero grande (50.000 clientes, generado con XmlWriter)

Fichero: `gentefit_clientes_grande.xml`

| Componente | Clientes leídos | Sin DNI o email | Primer cliente | Tiempo (ms) | Memoria asignada (MB) | Resultado |
| --- | --- | --- | --- | --- | --- | --- |
| XmlDocument | 50000 | 0 | Núria L'Hereu Ibáñez | 350 | 43,2 | Correcto |
| LINQ to XML | 50000 | 0 | Núria L'Hereu Ibáñez | 260 | 33,6 | Correcto |
| XmlReader | 50000 | 0 | Núria L'Hereu Ibáñez | 177 | 38,3 | Correcto |
| XmlSerializer | 50000 | 0 | Núria L'Hereu Ibáñez | 78 | 18,9 | Correcto |
| DataSet | 50000 | 0 | Núria L'Hereu Ibáñez | 1273 | 179,9 | Correcto |

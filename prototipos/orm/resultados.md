# Resultados de los prototipos ORM

Ejecutado el 25/10/2026 21:06 con .NET 10.0.13, contra SQL Server 2022.

Cada ORM inserta 1.000 clientes en una transacción, los lee todos y busca 500 por su DNI, uno a uno. Antes de medir se hace una vuelta de calentamiento, porque la primera llamada incluye la compilación JIT y la carga del modelo.

| ORM | Insertar (ms) | Leer todos (ms) | Buscar por DNI (ms) | Clientes leídos | Encontrados | Resultado |
| --- | --- | --- | --- | --- | --- | --- |
| Entity Framework Core | 244 | 6 | 505 | 1000 | 500 | Correcto |
| Dapper | 776 | 6 | 380 | 1000 | 500 | Correcto |
| LINQ to DB | 769 | 3 | 418 | 1000 | 500 | Correcto |
| RepoDB | 183 | 2 | 350 | 1000 | 500 | Correcto |

NHibernate no se ha probado: necesita definir el mapeo de cada tabla en ficheros XML o con FluentNHibernate, y no aporta nada que no haga ya Entity Framework Core.

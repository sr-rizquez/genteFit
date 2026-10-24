IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [migration_id] nvarchar(150) NOT NULL,
        [product_version] nvarchar(32) NOT NULL,
        CONSTRAINT [pk___ef_migrations_history] PRIMARY KEY ([migration_id])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [actividad] (
    [id] int NOT NULL IDENTITY,
    [nombre] nvarchar(80) NOT NULL,
    [tipo] nvarchar(20) NOT NULL,
    [descripcion] nvarchar(500) NULL,
    [intensidad] nvarchar(10) NOT NULL,
    [duracion_minutos] int NOT NULL,
    [activa] bit NOT NULL,
    CONSTRAINT [pk_actividad] PRIMARY KEY ([id]),
    CONSTRAINT [ck_actividad_duracion] CHECK ([duracion_minutos] > 0)
);

CREATE TABLE [cliente] (
    [id] int NOT NULL IDENTITY,
    [dni] nvarchar(9) NOT NULL,
    [nombre] nvarchar(60) NOT NULL,
    [apellidos] nvarchar(100) NOT NULL,
    [email] nvarchar(150) NOT NULL,
    [telefono] nvarchar(15) NULL,
    [fecha_nacimiento] date NULL,
    [fecha_alta] date NOT NULL,
    [fecha_baja] date NULL,
    [activo] bit NOT NULL,
    CONSTRAINT [pk_cliente] PRIMARY KEY ([id])
);

CREATE TABLE [monitor] (
    [id] int NOT NULL IDENTITY,
    [nombre] nvarchar(60) NOT NULL,
    [apellidos] nvarchar(100) NOT NULL,
    [email] nvarchar(150) NOT NULL,
    [activo] bit NOT NULL,
    CONSTRAINT [pk_monitor] PRIMARY KEY ([id])
);

CREATE TABLE [sala] (
    [id] int NOT NULL IDENTITY,
    [nombre] nvarchar(60) NOT NULL,
    [aforo] int NOT NULL,
    CONSTRAINT [pk_sala] PRIMARY KEY ([id]),
    CONSTRAINT [ck_sala_aforo] CHECK ([aforo] > 0)
);

CREATE TABLE [usuario] (
    [id] int NOT NULL IDENTITY,
    [nombre_usuario] nvarchar(50) NOT NULL,
    [email] nvarchar(150) NOT NULL,
    [hash_contrasena] nvarchar(255) NOT NULL,
    [rol] nvarchar(20) NOT NULL,
    [activo] bit NOT NULL,
    [id_cliente] int NULL,
    CONSTRAINT [pk_usuario] PRIMARY KEY ([id]),
    CONSTRAINT [fk_usuario_cliente_id_cliente] FOREIGN KEY ([id_cliente]) REFERENCES [cliente] ([id]) ON DELETE SET NULL
);

CREATE TABLE [monitor_actividad] (
    [id_actividad] int NOT NULL,
    [id_monitor] int NOT NULL,
    CONSTRAINT [pk_monitor_actividad] PRIMARY KEY ([id_actividad], [id_monitor]),
    CONSTRAINT [fk_monitor_actividad_actividad_id_actividad] FOREIGN KEY ([id_actividad]) REFERENCES [actividad] ([id]) ON DELETE CASCADE,
    CONSTRAINT [fk_monitor_actividad_monitor_id_monitor] FOREIGN KEY ([id_monitor]) REFERENCES [monitor] ([id]) ON DELETE CASCADE
);

CREATE TABLE [sesion] (
    [id] int NOT NULL IDENTITY,
    [fecha_hora_inicio] datetime2 NOT NULL,
    [estado] nvarchar(12) NOT NULL,
    [id_actividad] int NOT NULL,
    [id_sala] int NOT NULL,
    [id_monitor] int NOT NULL,
    CONSTRAINT [pk_sesion] PRIMARY KEY ([id]),
    CONSTRAINT [fk_sesion_actividad_id_actividad] FOREIGN KEY ([id_actividad]) REFERENCES [actividad] ([id]) ON DELETE NO ACTION,
    CONSTRAINT [fk_sesion_monitor_id_monitor] FOREIGN KEY ([id_monitor]) REFERENCES [monitor] ([id]) ON DELETE NO ACTION,
    CONSTRAINT [fk_sesion_sala_id_sala] FOREIGN KEY ([id_sala]) REFERENCES [sala] ([id]) ON DELETE NO ACTION
);

CREATE TABLE [lote_intercambio] (
    [id] int NOT NULL IDENTITY,
    [codigo] nvarchar(20) NOT NULL,
    [tipo] nvarchar(12) NOT NULL,
    [fecha_desde] date NOT NULL,
    [fecha_hasta] date NOT NULL,
    [fecha_generacion] datetime2 NOT NULL,
    [ruta_fichero] nvarchar(260) NOT NULL,
    [num_registros] int NOT NULL,
    [id_usuario] int NOT NULL,
    CONSTRAINT [pk_lote_intercambio] PRIMARY KEY ([id]),
    CONSTRAINT [fk_lote_intercambio_usuarios_id_usuario] FOREIGN KEY ([id_usuario]) REFERENCES [usuario] ([id]) ON DELETE NO ACTION
);

CREATE TABLE [reserva] (
    [id] int NOT NULL IDENTITY,
    [fecha_solicitud] datetime2 NOT NULL,
    [estado] nvarchar(12) NOT NULL,
    [fecha_cancelacion] datetime2 NULL,
    [id_sesion] int NOT NULL,
    [id_cliente] int NOT NULL,
    CONSTRAINT [pk_reserva] PRIMARY KEY ([id]),
    CONSTRAINT [fk_reserva_cliente_id_cliente] FOREIGN KEY ([id_cliente]) REFERENCES [cliente] ([id]) ON DELETE NO ACTION,
    CONSTRAINT [fk_reserva_sesiones_id_sesion] FOREIGN KEY ([id_sesion]) REFERENCES [sesion] ([id]) ON DELETE NO ACTION
);

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'id', N'activa', N'descripcion', N'duracion_minutos', N'intensidad', N'nombre', N'tipo') AND [object_id] = OBJECT_ID(N'[actividad]'))
    SET IDENTITY_INSERT [actividad] ON;
INSERT INTO [actividad] ([id], [activa], [descripcion], [duracion_minutos], [intensidad], [nombre], [tipo])
VALUES (1, CAST(1 AS bit), NULL, 55, N'ALTA', N'Body Pump', N'BODY_PUMP'),
(2, CAST(1 AS bit), NULL, 55, N'ALTA', N'Body Combat', N'BODY_COMBAT'),
(3, CAST(1 AS bit), NULL, 30, N'MEDIA', N'Core', N'CORE'),
(4, CAST(1 AS bit), N'Estiramientos y respiración para todos los niveles', 60, N'BAJA', N'Yoga', N'YOGA'),
(5, CAST(1 AS bit), NULL, 45, N'ALTA', N'Spinning', N'SPINNING'),
(6, CAST(1 AS bit), NULL, 50, N'MEDIA', N'Zumba', N'ZUMBA');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'id', N'activa', N'descripcion', N'duracion_minutos', N'intensidad', N'nombre', N'tipo') AND [object_id] = OBJECT_ID(N'[actividad]'))
    SET IDENTITY_INSERT [actividad] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'id', N'activo', N'apellidos', N'email', N'nombre') AND [object_id] = OBJECT_ID(N'[monitor]'))
    SET IDENTITY_INSERT [monitor] ON;
INSERT INTO [monitor] ([id], [activo], [apellidos], [email], [nombre])
VALUES (1, CAST(1 AS bit), N'Núñez O''Connor', N'mariajose@gentefit.test', N'María José'),
(2, CAST(1 AS bit), N'Puig i Font', N'jordi@gentefit.test', N'Jordi');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'id', N'activo', N'apellidos', N'email', N'nombre') AND [object_id] = OBJECT_ID(N'[monitor]'))
    SET IDENTITY_INSERT [monitor] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'id', N'aforo', N'nombre') AND [object_id] = OBJECT_ID(N'[sala]'))
    SET IDENTITY_INSERT [sala] ON;
INSERT INTO [sala] ([id], [aforo], [nombre])
VALUES (1, 16, N'Sala Montjuïc'),
(2, 16, N'Sala d''Spinning');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'id', N'aforo', N'nombre') AND [object_id] = OBJECT_ID(N'[sala]'))
    SET IDENTITY_INSERT [sala] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'id', N'estado', N'fecha_hora_inicio', N'id_actividad', N'id_monitor', N'id_sala') AND [object_id] = OBJECT_ID(N'[sesion]'))
    SET IDENTITY_INSERT [sesion] ON;
INSERT INTO [sesion] ([id], [estado], [fecha_hora_inicio], [id_actividad], [id_monitor], [id_sala])
VALUES (1, N'PROGRAMADA', '2026-11-02T18:30:00.0000000', 4, 1, 1),
(2, N'PROGRAMADA', '2026-11-03T19:00:00.0000000', 3, 1, 1),
(3, N'PROGRAMADA', '2026-11-04T20:00:00.0000000', 5, 2, 2);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'id', N'estado', N'fecha_hora_inicio', N'id_actividad', N'id_monitor', N'id_sala') AND [object_id] = OBJECT_ID(N'[sesion]'))
    SET IDENTITY_INSERT [sesion] OFF;

CREATE UNIQUE INDEX [ix_cliente_dni] ON [cliente] ([dni]);

CREATE UNIQUE INDEX [ix_cliente_email] ON [cliente] ([email]);

CREATE UNIQUE INDEX [ix_lote_intercambio_codigo] ON [lote_intercambio] ([codigo]);

CREATE INDEX [ix_lote_intercambio_id_usuario] ON [lote_intercambio] ([id_usuario]);

CREATE UNIQUE INDEX [ix_monitor_email] ON [monitor] ([email]);

CREATE INDEX [ix_monitor_actividad_id_monitor] ON [monitor_actividad] ([id_monitor]);

CREATE INDEX [ix_reserva_id_cliente] ON [reserva] ([id_cliente]);

CREATE UNIQUE INDEX [ix_reserva_id_sesion_id_cliente] ON [reserva] ([id_sesion], [id_cliente]) WHERE [estado] <> 'CANCELADA';

CREATE UNIQUE INDEX [ix_sala_nombre] ON [sala] ([nombre]);

CREATE INDEX [ix_sesion_id_actividad] ON [sesion] ([id_actividad]);

CREATE INDEX [ix_sesion_id_monitor] ON [sesion] ([id_monitor]);

CREATE UNIQUE INDEX [ix_sesion_id_sala_fecha_hora_inicio] ON [sesion] ([id_sala], [fecha_hora_inicio]);

CREATE UNIQUE INDEX [ix_usuario_email] ON [usuario] ([email]);

CREATE UNIQUE INDEX [ix_usuario_id_cliente] ON [usuario] ([id_cliente]) WHERE [id_cliente] IS NOT NULL;

CREATE UNIQUE INDEX [ix_usuario_nombre_usuario] ON [usuario] ([nombre_usuario]);

INSERT INTO [__EFMigrationsHistory] ([migration_id], [product_version])
VALUES (N'20261024173844_ModeloInicial', N'10.0.13');

COMMIT;
GO


-- =============================================
-- Script: Tablas para Reporte de Bajas de Activos Fijos
-- Fecha: 2026-07-13
-- =============================================

-- Crear tabla de Motivos de Baja
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MotivoBaja')
BEGIN
    CREATE TABLE MotivoBaja (
        IdMotivoBaja INT IDENTITY(1,1) NOT NULL,
        Nombre VARCHAR(200) NOT NULL,
        CONSTRAINT PK_MotivoBaja PRIMARY KEY CLUSTERED (IdMotivoBaja)
    );

    -- Insertar los 3 motivos predeterminados
    INSERT INTO MotivoBaja (Nombre) VALUES
    ('Baja Pendiente Materias Primas'),
    ('Traslado'),
    ('Baja (Destino Final)');

    PRINT 'Tabla MotivoBaja creada y datos insertados.';
END
ELSE
BEGIN
    PRINT 'La tabla MotivoBaja ya existe.';
END
GO

-- Crear tabla de Bajas de Activos
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BajaActivo')
BEGIN
    CREATE TABLE BajaActivo (
        IdBaja INT IDENTITY(1,1) NOT NULL,
        NoInventario BIGINT NOT NULL,
        SubNumero INT NOT NULL,
        FechaSubmayor DATETIME NOT NULL,
        MotivoBajaId INT NOT NULL,
        Observaciones VARCHAR(500) NULL,
        UsuarioId INT NOT NULL,
        FechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT PK_BajaActivo PRIMARY KEY CLUSTERED (IdBaja),
        CONSTRAINT FK_BajaActivo_MotivoBaja FOREIGN KEY (MotivoBajaId) REFERENCES MotivoBaja(IdMotivoBaja)
    );

    -- Crear índices para búsquedas frecuentes
    CREATE INDEX IX_BajaActivo_NoInventario_SubNumero_FechaSubmayor
    ON BajaActivo (NoInventario, SubNumero, FechaSubmayor);

    PRINT 'Tabla BajaActivo creada.';
END
ELSE
BEGIN
    PRINT 'La tabla BajaActivo ya existe.';
END
GO

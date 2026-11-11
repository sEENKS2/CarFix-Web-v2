-- Nombres ficticios para presentar la base local. No modifica cuentas ni datos fiscales.
-- Solo reemplaza valores originales de demostración; conserva claves y relaciones.
SET NOCOUNT ON;
SET XACT_ABORT ON;
IF DB_NAME() <> N'CarFix_Local_20261111'
    THROW 50001, 'Este script solo se aplica a la base local de presentación.', 1;

BEGIN TRANSACTION;

UPDATE Clientes SET Nombre = N'Martín', Apellido = N'Pereyra'
WHERE Id = 1 AND Nombre = N'ClienteDemo1' AND Apellido = N'Ejemplo1';
UPDATE Clientes SET Nombre = N'Luciana', Apellido = N'Ferreyra'
WHERE Id = 2 AND Nombre = N'TecnicoDemo1' AND Apellido = N'Ejemplo2';
UPDATE Clientes SET Nombre = N'Ricardo', Apellido = N'Molina'
WHERE Id = 3 AND Nombre = N'ClienteDemo3' AND Apellido = N'Ejemplo3';

UPDATE Tecnicos SET Nombre = N'Gabriel', Apellido = N'Acosta'
WHERE Id = 1 AND Nombre = N'TecnicoDemo1' AND Apellido = N'Ejemplo1';
UPDATE Tecnicos SET Nombre = N'Federico', Apellido = N'Ríos'
WHERE Id = 2 AND Nombre = N'TecnicoDemo2' AND Apellido = N'Ejemplo2';
UPDATE Tecnicos SET Nombre = N'Sergio', Apellido = N'Benítez'
WHERE Id = 3 AND Nombre = N'TecnicoDemo3' AND Apellido = N'Ejemplo3';

UPDATE Proveedores SET Nombre = N'Repuestos del Parque'
WHERE Id = 1 AND Nombre = N'ProveedorDemo1';
UPDATE Proveedores SET Nombre = N'Distribuidora Automotriz del Sur'
WHERE Id = 2 AND Nombre = N'ProveedorDemo2';
UPDATE Proveedores SET Direccion = N'Los Aromos 1240'
WHERE Id = 1 AND Direccion = N'Domicilio ficticio 1';
UPDATE Proveedores SET Direccion = N'Av. del Trabajo 2850'
WHERE Id = 2 AND Direccion = N'Domicilio ficticio 2';

IF EXISTS (SELECT 1 FROM Vehiculos WHERE Dominio IN (N'AE482KM', N'AC735RT', N'AF216LS') AND Id NOT IN (1,2,3))
    THROW 50002, 'Una de las patentes propuestas ya está en uso.', 1;

UPDATE Vehiculos SET Dominio = N'AE482KM' WHERE Id = 1 AND Dominio = N'DEMO001';
UPDATE Vehiculos SET Dominio = N'AC735RT' WHERE Id = 2 AND Dominio = N'DEMO002';
UPDATE Vehiculos SET Dominio = N'AF216LS' WHERE Id = 3 AND Dominio = N'DEMO003';

COMMIT TRANSACTION;
PRINT 'Datos de presentación actualizados. Identificadores, relaciones y cuentas conservados.';

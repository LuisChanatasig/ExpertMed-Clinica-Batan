---==========Modificaciones de base de datos=========---

---====CREACION DE SP DE OBTENER LA OFICINA=====---
CREATE PROCEDURE sp_GetMedicalOfficeById
    @MedicalOfficeId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        medicaloffice_id,
        medicaloffice_establishmentid,
        medicaloffice_name,
        medicaloffice_location,
        medicaloffice_description,
        medicaloffice_status
    FROM medical_offices
    WHERE medicaloffice_id = @MedicalOfficeId;
END

---====ACTUALIZAR DATO USERS=====---
update users set user_establishment_id = 18 where users_id = 42

---====ACTUALIZAR TABLAS DE ESTABLECIMIENTO====---
ALTER TABLE establishment 
ADD establishment_logo VARBINARY(MAX);
---====CREAR SP PARA EL ESTABLECIMIENTO====---

CREATE PROCEDURE sp_CreateEstablishment
    @establishment_name NVARCHAR(255),
    @establishment_address NVARCHAR(255) = NULL,
    @establishment_emissionpoint NVARCHAR(4) = NULL,
    @establishment_pointofsale NVARCHAR(4) = NULL,
    @establishment_creationdate DATETIME = NULL,
    @establishment_modificationdate DATETIME = NULL,
    @establishment_sequential_billing INT = NULL,
    @establishment_logo VARBINARY(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        -- Validación simple del campo obligatorio
        IF (@establishment_name IS NULL OR LTRIM(RTRIM(@establishment_name)) = '')
        BEGIN
            RAISERROR('El nombre del establecimiento es obligatorio.', 16, 1);
            RETURN;
        END

        -- Insertar datos
        INSERT INTO establishment (
            establishment_name,
            establishment_address,
            establishment_emissionpoint,
            establishment_pointofsale,
            establishment_creationdate,
            establishment_modificationdate,
            establishment_sequential_billing,
            establishment_logo
        )
        VALUES (
            @establishment_name,
            @establishment_address,
            @establishment_emissionpoint,
            @establishment_pointofsale,
            ISNULL(@establishment_creationdate, GETDATE()), -- Default: ahora
            @establishment_modificationdate,
            @establishment_sequential_billing,
            @establishment_logo
        );

        -- Obtener el ID generado
        DECLARE @NewID INT = SCOPE_IDENTITY();

        PRINT 'Establecimiento creado exitosamente con ID: ' + CAST(@NewID AS NVARCHAR);
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();

        RAISERROR('Error al crear el establecimiento: %s', @ErrorSeverity, @ErrorState, @ErrorMessage);
    END CATCH
END;

---====Listar Establecimientos====---
CREATE PROCEDURE sp_ListEstablishment
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        establishment_id AS Id,
        establishment_name AS Name,
        establishment_address AS Address,
        establishment_emissionpoint AS EmissionPoint,
        establishment_pointofsale AS PointOfSale,
        establishment_creationdate AS CreationDate,
        establishment_modificationdate AS ModificationDate,
        establishment_sequential_billing AS SequentialBilling,
        establishment_logo AS Logo
    FROM Establishment;
END;

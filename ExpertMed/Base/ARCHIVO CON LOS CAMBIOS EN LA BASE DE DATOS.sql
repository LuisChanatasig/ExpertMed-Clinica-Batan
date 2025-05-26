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
---====Obtener Establecimientos por Id====---

CREATE PROCEDURE sp_GetEstablishmentById
    @EstablishmentId INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- Verificar si existe
        IF NOT EXISTS (
            SELECT 1 
            FROM establishment 
            WHERE establishment_id = @EstablishmentId
        )
        BEGIN
            RAISERROR('No se encontró el establecimiento con el ID %d.', 16, 1, @EstablishmentId);
            RETURN;
        END

        -- Si existe, devolverlo
        SELECT 
            establishment_id,
            establishment_name,
            establishment_address,
            establishment_emissionpoint,
            establishment_pointofsale,
            establishment_creationdate,
            establishment_modificationdate,
            establishment_sequential_billing,
            establishment_logo
        FROM establishment
        WHERE establishment_id = @EstablishmentId;
    END TRY

    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();

        RAISERROR('Error al obtener el establecimiento: %s', @ErrorSeverity, @ErrorState, @ErrorMessage);
    END CATCH
END;


---====Actualizar Establecimientos====---
CREATE PROCEDURE sp_UpdateEstablishment
    @EstablishmentId INT,
    @Name NVARCHAR(255),
    @Address NVARCHAR(255) = NULL,
    @EmissionPoint NVARCHAR(4) = NULL,
    @PointOfSale NVARCHAR(4) = NULL,
    @SequentialBilling INT = NULL,
    @Logo VARBINARY(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- Validar existencia
        IF NOT EXISTS (
            SELECT 1
            FROM establishment
            WHERE establishment_id = @EstablishmentId
        )
        BEGIN
            RAISERROR('No se encontró el establecimiento con ID %d.', 16, 1, @EstablishmentId);
            RETURN;
        END

        -- Actualizar registro
        UPDATE establishment
        SET
            establishment_name = @Name,
            establishment_address = @Address,
            establishment_emissionpoint = @EmissionPoint,
            establishment_pointofsale = @PointOfSale,
            establishment_sequential_billing = @SequentialBilling,
            establishment_modificationdate = GETDATE(),
            -- Actualiza logo solo si no es NULL (permite mantener el actual si no se cambia)
            establishment_logo = CASE 
                                    WHEN @Logo IS NOT NULL THEN @Logo 
                                    ELSE establishment_logo 
                                 END
        WHERE establishment_id = @EstablishmentId;

        -- Confirmar éxito
        SELECT 'Establecimiento actualizado correctamente.' AS Message;
    END TRY

    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();

        RAISERROR('Error al actualizar el establecimiento: %s', @ErrorSeverity, @ErrorState, @ErrorMessage);
    END CATCH
END;

---====CONFIGURACIONES YA CON LOS NUEVOS REQUERIMIENTOS====---

---===Modificar el sp GetDoctorsByAssistant para traer a especialidad===---
ALTER PROCEDURE [dbo].[GetDoctorsByAssistant]
    @AssistantUserId INT,
    @UserProfile INT
AS
BEGIN
    SET NOCOUNT ON;

    IF @UserProfile = 1
    BEGIN
        -- Administrador: ver todos los médicos
        SELECT 
            u.users_id,
            u.users_names,
            u.users_surcenames,
            s.speciality_name
        FROM users u
        LEFT JOIN specialities s ON s.speciality_id = u.users_specialityid
        WHERE u.users_profileid = 2
        ORDER BY u.users_names;
    END
    ELSE IF @UserProfile = 3
    BEGIN
        -- Asistente: médicos asociados
        SELECT 
            d.users_id,
            d.users_names,
            d.users_surcenames,
            s.speciality_name
        FROM users d
        INNER JOIN assistant_doctor_relationship dar ON dar.doctor_userid = d.users_id
        LEFT JOIN specialities s ON s.speciality_id = d.users_specialityid
        WHERE dar.assistant_userid = @AssistantUserId
          AND dar.relationship_status = 1
        ORDER BY d.users_names;
    END
    ELSE IF @UserProfile = 4
    BEGIN
        -- Recepcionista: médicos del mismo establecimiento
        DECLARE @UserEstablishmentId INT;

        SELECT @UserEstablishmentId = user_establishment_id
        FROM users
        WHERE users_id = @AssistantUserId;

        SELECT 
            u.users_id,
            u.users_names,
            u.users_surcenames,
            s.speciality_name
        FROM users u
        LEFT JOIN specialities s ON s.speciality_id = u.users_specialityid
        WHERE u.users_profileid = 2
          AND u.user_establishment_id = @UserEstablishmentId
        ORDER BY u.users_names;
    END
    ELSE
    BEGIN
        SELECT TOP 0 users_id, users_names, users_surcenames, NULL AS speciality_name FROM users;
    END
END


---===Modificar el sp de crear la cita===---

ALTER PROCEDURE [dbo].[sp_CreateAppointment]
    @appointment_createdate DATETIME,
    @appointment_modifydate DATETIME,
    @appointment_createuser INT,
    @appointment_modifyuser INT,
    @appointment_date DATETIME,
    @appointment_hour TIME(7),
    @appointment_patientid INT,
    @appointment_status INT,
    @appointment_medicalofficeid INT = NULL,
    @doctor_userid INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @ErrorMessage NVARCHAR(4000);
        DECLARE @NewAppointmentId INT;
        DECLARE @UserProfile INT;

        -- Validación de existencia del paciente
        IF NOT EXISTS (SELECT 1 FROM patient WHERE patient_id = @appointment_patientid)
        BEGIN
            SET @ErrorMessage = 'El paciente especificado no existe.';
            THROW 50001, @ErrorMessage, 1;
        END

        -- Obtener el perfil del usuario creador
        SELECT @UserProfile = users_profileid FROM users WHERE users_id = @appointment_createuser;

		-- Si es perfil 4, validar doctorUserId y que pertenece al mismo establecimiento
IF @UserProfile = 4
BEGIN
    IF @doctor_userid IS NULL
    BEGIN
        SET @ErrorMessage = 'Debe especificar un médico para perfil 4.';
        THROW 50009, @ErrorMessage, 1;
    END

    DECLARE @EstablishmentId INT;

    -- Obtener el establecimiento del usuario perfil 4 desde la tabla users
    SELECT @EstablishmentId = user_establishment_id FROM users WHERE users_id = @appointment_createuser;

    -- Validar que el doctor pertenece al mismo establecimiento
    IF NOT EXISTS (
        SELECT 1
        FROM users
        WHERE users_id = @doctor_userid
          AND user_establishment_id = @EstablishmentId
          AND users_profileid = 2 -- perfil médico
    )
    BEGIN
        SET @ErrorMessage = 'El médico asignado no pertenece al mismo establecimiento del usuario perfil 4.';
        THROW 50010, @ErrorMessage, 1;
    END
END



        IF @UserProfile NOT IN (1, 2, 3,4)
        BEGIN
            SET @ErrorMessage = 'El perfil del usuario no es válido para crear citas.';
            THROW 50003, @ErrorMessage, 1;
        END

        -- EMERGENCIA: Asignar cualquier consultorio disponible del médico
        IF @appointment_status = 5 AND @appointment_medicalofficeid IS NULL
        BEGIN
            SELECT TOP 1 @appointment_medicalofficeid = moa.medical_office_id
            FROM medical_office_assignments moa
            WHERE moa.users_id = @doctor_userid
              AND moa.assignment_status = 1
            ORDER BY NEWID();

            IF @appointment_medicalofficeid IS NULL
            BEGIN
                SET @ErrorMessage = 'El médico no tiene consultorios asignados.';
                THROW 50006, @ErrorMessage, 1;
            END
        END

        -- Validar asignación de consultorio para citas normales
        IF @appointment_status <> 5 AND @appointment_medicalofficeid IS NOT NULL
        BEGIN
DECLARE @ValidUserId INT = 
    CASE 
        WHEN @UserProfile IN (3, 4) THEN @doctor_userid 
        ELSE @appointment_createuser 
    END;

            IF NOT EXISTS (
                SELECT 1 
                FROM medical_office_assignments 
                WHERE users_id = @ValidUserId
                  AND medical_office_id = @appointment_medicalofficeid
                  AND assignment_status = 1
            )
            BEGIN
                SET @ErrorMessage = 'El consultorio no está asignado al médico correspondiente.';
                THROW 50005, @ErrorMessage, 1;
            END
        END

        -- EMERGENCIA: si ya hay una cita en ese consultorio y hora, reagendar la anterior
        IF @appointment_status = 5
        BEGIN
            DECLARE @ExistingAppointmentId INT;

            SELECT TOP 1 @ExistingAppointmentId = appointment_id
            FROM appointment
            WHERE appointment_date = @appointment_date
              AND appointment_hour = @appointment_hour
              AND appointment_medicalofficeid = @appointment_medicalofficeid;

            IF @ExistingAppointmentId IS NOT NULL
            BEGIN
                DECLARE @ScheduleStart TIME(7), @ScheduleEnd TIME(7), @Interval INT;
                SELECT TOP 1 
                    @ScheduleStart = start_time,
                    @ScheduleEnd = end_time,
                    @Interval = appointment_interval
                FROM user_schedules
                WHERE users_id = @doctor_userid
                  AND schudels_status = 1;

                IF @ScheduleStart IS NULL
                BEGIN
                    SET @ErrorMessage = 'No se encontró un horario válido para el médico.';
                    THROW 50007, @ErrorMessage, 1;
                END

                DECLARE @ReassignDate DATE = @appointment_date;
                DECLARE @ReassignHour TIME(7) = NULL;
                DECLARE @i INT = 0;

                -- Buscar próxima hora disponible hasta 5 días después
                WHILE @ReassignHour IS NULL AND @i < 5
                BEGIN
                    ;WITH Times AS (
                        SELECT CAST(@ScheduleStart AS TIME(7)) AS SlotTime
                        UNION ALL
                        SELECT DATEADD(MINUTE, @Interval, SlotTime)
                        FROM Times
                        WHERE DATEADD(MINUTE, @Interval, SlotTime) < @ScheduleEnd
                    )
                    SELECT TOP 1 @ReassignHour = SlotTime
                    FROM Times t
                    WHERE NOT EXISTS (
                        SELECT 1 FROM appointment a
                        WHERE a.appointment_date = @ReassignDate
                          AND a.appointment_hour = t.SlotTime
                          AND a.appointment_medicalofficeid = @appointment_medicalofficeid
                    )
                    OPTION (MAXRECURSION 100);

                    IF @ReassignHour IS NULL
                    BEGIN
                        SET @ReassignDate = DATEADD(DAY, 1, @ReassignDate);
                        SET @i += 1;
                    END
                END

                IF @ReassignHour IS NULL
                BEGIN
                    SET @ErrorMessage = 'No se pudo reubicar la cita desplazada por emergencia.';
                    THROW 50008, @ErrorMessage, 1;
                END

                -- Actualizar la cita desplazada
                UPDATE appointment
                SET appointment_date = @ReassignDate,
                    appointment_hour = @ReassignHour,
                    appointment_modifydate = @appointment_modifydate,
                    appointment_modifyuser = @appointment_modifyuser
                WHERE appointment_id = @ExistingAppointmentId;
            END
        END
        ELSE
        BEGIN
            -- Validación estándar para citas no emergencia
            IF EXISTS (
                SELECT 1 FROM appointment
                WHERE appointment_patientid = @appointment_patientid
                  AND appointment_date = @appointment_date
                  AND appointment_hour = @appointment_hour
            )
            BEGIN
                SET @ErrorMessage = 'El paciente ya tiene una cita programada en la misma fecha y hora.';
                THROW 50002, @ErrorMessage, 1;
            END
        END

        -- Insertar nueva cita
        INSERT INTO appointment (
            appointment_createdate,
            appointment_modifydate,
            appointment_createuser,
            appointment_modifyuser,
            appointment_date,
            appointment_hour,
            appointment_patientid,
            appointment_status,
            appointment_medicalofficeid
        )
        VALUES (
            @appointment_createdate,
            @appointment_modifydate,
            @appointment_createuser,
            @appointment_modifyuser,
            @appointment_date,
            @appointment_hour,
            @appointment_patientid,
            @appointment_status,
            @appointment_medicalofficeid
        );

        SET @NewAppointmentId = SCOPE_IDENTITY();

        -- Relación asistente - médico
        IF @UserProfile = 3
        BEGIN
            IF @doctor_userid IS NOT NULL AND NOT EXISTS (
                SELECT 1 
                FROM assistant_doctor_relationship 
                WHERE assistant_userid = @appointment_createuser 
                  AND doctor_userid = @doctor_userid 
                  AND relationship_status = 1
            )
            BEGIN
                SET @ErrorMessage = 'El asistente no está autorizado para este médico.';
                THROW 50004, @ErrorMessage, 1;
            END

            INSERT INTO assistant_doctor_appointment (
                assistant_userid,
                doctor_userid,
                appointment_id,
                created_by,
                creation_date,
                relationship_status
            )
            VALUES (
                @appointment_createuser,
                @doctor_userid,
                @NewAppointmentId,
                @appointment_createuser,
                GETDATE(),
                1
            );
        END

		-- Relación coordinador - médico (perfil 4)
IF @UserProfile = 4
BEGIN
    INSERT INTO assistant_doctor_appointment (
        assistant_userid,
        doctor_userid,
        appointment_id,
        created_by,
        creation_date,
        relationship_status
    )
    VALUES (
        @appointment_createuser,
        @doctor_userid,
        @NewAppointmentId,
        @appointment_createuser,
        GETDATE(),
        1
    );
END

        COMMIT TRANSACTION;

        -- Respuesta final
        SELECT 
            1 AS success,
            'Cita creada exitosamente' AS message,
            @NewAppointmentId AS appointmentId,
            CASE WHEN @appointment_status = 5 THEN 1 ELSE 0 END AS esEmergencia
        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        SELECT 
            0 AS success,
            ERROR_MESSAGE() AS message
        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER;
    END CATCH
END


---===Modificar el obtener consultorios===---
ALTER PROCEDURE [dbo].[sp_GetAvailableOffices]
    @UserId INT,
    @Date DATE,
    @Hour TIME(7),
    @DoctorUserId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Obtener perfil del usuario
    DECLARE @UserProfileId INT;
    SELECT @UserProfileId = users_profileid FROM users WHERE users_id = @UserId;

    -- Determinar el médico objetivo según el perfil
    DECLARE @TargetUserId INT = 
        CASE 
            WHEN @UserProfileId IN (3, 4) -- Asistente o Coordinador
                THEN ISNULL(@DoctorUserId, 0)
            ELSE @UserId -- Si es médico u otro
        END;

    -- Validar que se haya definido un médico válido
    IF @TargetUserId = 0
    BEGIN
        RAISERROR('No se proporcionó un médico válido.', 16, 1);
        RETURN;
    END

    -- Validar que el TargetUser sea un médico activo
    IF NOT EXISTS (
        SELECT 1 
        FROM users 
        WHERE users_id = @TargetUserId 
          AND users_profileid = 2
    )
    BEGIN
        RAISERROR('El usuario proporcionado no es un médico válido.', 16, 1);
        RETURN;
    END

    -- Consultorios asignados al médico y no ocupados en ese horario
    SELECT 
        mo.medicaloffice_id, 
        mo.medicaloffice_name
    FROM medical_office_assignments a
    INNER JOIN medical_offices mo ON mo.medicaloffice_id = a.medical_office_id
    WHERE a.users_id = @TargetUserId
      AND a.assignment_status = 1
      AND mo.medicaloffice_status = 1
      AND NOT EXISTS (
          SELECT 1 
          FROM appointment ap
          WHERE ap.appointment_date = @Date
            AND ap.appointment_hour = @Hour
            AND ap.appointment_medicalofficeid = mo.medicaloffice_id
      )
    ORDER BY mo.medicaloffice_name;
END

select * from appointment

select * from assistant_doctor_appointment 

delete from assistant_doctor_appointment where assistant_userid = 43 

delete from appointment where appointment_createuser = 43
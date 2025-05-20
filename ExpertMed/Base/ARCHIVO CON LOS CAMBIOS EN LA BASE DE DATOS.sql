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


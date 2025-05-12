using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpertMed.Migrations
{
    /// <inheritdoc />
    public partial class pacientes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "catalogs",
                columns: table => new
                {
                    catalog_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    catalog_name = table.Column<string>(type: "nvarchar(2500)", maxLength: 2500, nullable: false),
                    catalog_category = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    category_status = table.Column<int>(type: "int", nullable: true, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__catalogs__9871D9500A81A69B", x => x.catalog_id);
                });

            migrationBuilder.CreateTable(
                name: "countries",
                columns: table => new
                {
                    country_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    country_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    country_iso = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: true),
                    country_code = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: true),
                    country_nationality = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    country_status = table.Column<int>(type: "int", nullable: true, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__countrie__7E8CD055511F6D94", x => x.country_id);
                });

            migrationBuilder.CreateTable(
                name: "diagnosis",
                columns: table => new
                {
                    diagnosis_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    diagnosis_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    diagnosis_description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    diagnosis_category = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    diagnosis_cie10 = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    diagnosis_status = table.Column<int>(type: "int", nullable: true, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__diagnosi__D49E32B47AF704DC", x => x.diagnosis_id);
                });

            migrationBuilder.CreateTable(
                name: "images",
                columns: table => new
                {
                    images_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    images_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    images_description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    images_category = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    images_cie10 = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    images_status = table.Column<int>(type: "int", nullable: true, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__images__FA2651F7ADED15C4", x => x.images_id);
                });

            migrationBuilder.CreateTable(
                name: "laboratories",
                columns: table => new
                {
                    laboratories_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    laboratories_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    laboratories_description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    laboratories_category = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    laboratories_cie10 = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    laboratories_status = table.Column<int>(type: "int", nullable: true, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__laborato__949BB039323204CF", x => x.laboratories_id);
                });

            migrationBuilder.CreateTable(
                name: "medications",
                columns: table => new
                {
                    medications_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    medications_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    medications_description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    medications_category = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    medications_distinctive = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    medications_concentration = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    medications_cie10 = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    medications_status = table.Column<int>(type: "int", nullable: true, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__medicati__CF638DC5C2A78939", x => x.medications_id);
                });

            migrationBuilder.CreateTable(
                name: "profiles",
                columns: table => new
                {
                    profile_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    profile_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    profile_description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    profile_status = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__profiles__AEBB701F14359BCE", x => x.profile_id);
                });

            migrationBuilder.CreateTable(
                name: "specialities",
                columns: table => new
                {
                    speciality_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    speciality_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    specialty_description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    speciality_category = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    speciality_status = table.Column<int>(type: "int", nullable: true, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__speciali__E82ED62067BC73C0", x => x.speciality_id);
                });

            migrationBuilder.CreateTable(
                name: "user_schedules",
                columns: table => new
                {
                    schudels_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    users_id = table.Column<int>(type: "int", nullable: false),
                    start_time = table.Column<TimeOnly>(type: "time", nullable: false),
                    end_time = table.Column<TimeOnly>(type: "time", nullable: false),
                    appointment_interval = table.Column<int>(type: "int", nullable: false),
                    create_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    update_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    schudels_status = table.Column<int>(type: "int", nullable: true, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__user_sch__0D64968B8934C57B", x => x.schudels_id);
                });

            migrationBuilder.CreateTable(
                name: "vat_billing",
                columns: table => new
                {
                    vatbilling_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    vatbilling_percentage = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    vatbilling_code = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    vatbilling_rate = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    vatbilling_status = table.Column<int>(type: "int", nullable: true, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__vat_bill__B6D1E35EF0427F09", x => x.vatbilling_id);
                });

            migrationBuilder.CreateTable(
                name: "provinces",
                columns: table => new
                {
                    province_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    province_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    province_demony = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    province_prefix = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    province_code = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    province_iso = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    province_status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    province_countryid = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__province__08DCB60F47592316", x => x.province_id);
                    table.ForeignKey(
                        name: "FK_province_countryid",
                        column: x => x.province_countryid,
                        principalTable: "countries",
                        principalColumn: "country_id");
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    users_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    users_document_number = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: false),
                    users_names = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    users_surcenames = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    users_phone = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    users_email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    users_creationdate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    users_modificationdate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    users_address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    users_profilephoto = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    users_profilephoto64 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    users_senecytcode = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    users_xkeytaxo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    users_xpasstaxo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    users_sequential_billing = table.Column<int>(type: "int", nullable: true),
                    users_login = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    users_password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    users_description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    users_establishment_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    users_establishment_address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    users_establishment_emissionpoint = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    users_establishment_pointofsale = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    users_profileid = table.Column<int>(type: "int", nullable: true),
                    users_specialityid = table.Column<int>(type: "int", nullable: true),
                    users_countryid = table.Column<int>(type: "int", nullable: true),
                    users_vatpercentageid = table.Column<int>(type: "int", nullable: true),
                    users_status = table.Column<int>(type: "int", nullable: true, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__users__EAA7D14B37748633", x => x.users_id);
                    table.ForeignKey(
                        name: "FK_users_countryid",
                        column: x => x.users_countryid,
                        principalTable: "countries",
                        principalColumn: "country_id");
                    table.ForeignKey(
                        name: "FK_users_profileid",
                        column: x => x.users_profileid,
                        principalTable: "profiles",
                        principalColumn: "profile_id");
                    table.ForeignKey(
                        name: "FK_users_specialityid",
                        column: x => x.users_specialityid,
                        principalTable: "specialities",
                        principalColumn: "speciality_id");
                    table.ForeignKey(
                        name: "FK_users_vatpercentageid",
                        column: x => x.users_vatpercentageid,
                        principalTable: "vat_billing",
                        principalColumn: "vatbilling_id");
                });

            migrationBuilder.CreateTable(
                name: "assistant_doctor_relationship",
                columns: table => new
                {
                    assistandoctor_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    assitandoctor_date = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    relationship_status = table.Column<int>(type: "int", nullable: false),
                    assistant_userid = table.Column<int>(type: "int", nullable: true),
                    doctor_userid = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__assistan__017A7BA914D54344", x => x.assistandoctor_id);
                    table.ForeignKey(
                        name: "FK_assistan_userid",
                        column: x => x.assistant_userid,
                        principalTable: "users",
                        principalColumn: "users_id");
                    table.ForeignKey(
                        name: "FK_doctor_userid",
                        column: x => x.doctor_userid,
                        principalTable: "users",
                        principalColumn: "users_id");
                });

            migrationBuilder.CreateTable(
                name: "patient",
                columns: table => new
                {
                    patient_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    patient_creationdate = table.Column<DateTime>(type: "datetime", nullable: true),
                    patient_modificationdate = table.Column<DateTime>(type: "datetime", nullable: true),
                    patient_creationuser = table.Column<int>(type: "int", nullable: true),
                    patient_modificationuser = table.Column<int>(type: "int", nullable: true),
                    patient_documenttype = table.Column<int>(type: "int", nullable: true),
                    patient_documentnumber = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: true),
                    patient_firstname = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    patient_middlename = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    patient_secondlastname = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    patient_gender = table.Column<int>(type: "int", nullable: true),
                    patient_birthdate = table.Column<DateOnly>(type: "date", nullable: true),
                    patient_age = table.Column<int>(type: "int", nullable: true),
                    patient_bloodtype = table.Column<int>(type: "int", nullable: true),
                    patient_donor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    patient_maritalstatus = table.Column<int>(type: "int", nullable: true),
                    patient_vocational_training = table.Column<int>(type: "int", nullable: true),
                    patient_landline_phone = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    patient_email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    patient_nationality = table.Column<int>(type: "int", nullable: true),
                    patient_province = table.Column<int>(type: "int", nullable: true),
                    patient_address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    patient_ocupation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    patient_company = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    patient_healt_insurance = table.Column<int>(type: "int", nullable: true),
                    patient_code = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    patient_status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    patient_cellular_phone = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    patient_firstsurname = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DoctorName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__patient__4D5CE4766CB5056D", x => x.patient_id);
                    table.ForeignKey(
                        name: "FK_patient_bloodtype",
                        column: x => x.patient_bloodtype,
                        principalTable: "catalogs",
                        principalColumn: "catalog_id");
                    table.ForeignKey(
                        name: "FK_patient_creationuser",
                        column: x => x.patient_creationuser,
                        principalTable: "users",
                        principalColumn: "users_id");
                    table.ForeignKey(
                        name: "FK_patient_documenttype",
                        column: x => x.patient_documenttype,
                        principalTable: "catalogs",
                        principalColumn: "catalog_id");
                    table.ForeignKey(
                        name: "FK_patient_gender",
                        column: x => x.patient_gender,
                        principalTable: "catalogs",
                        principalColumn: "catalog_id");
                    table.ForeignKey(
                        name: "FK_patient_healt_insurance",
                        column: x => x.patient_healt_insurance,
                        principalTable: "catalogs",
                        principalColumn: "catalog_id");
                    table.ForeignKey(
                        name: "FK_patient_maritalstatus",
                        column: x => x.patient_maritalstatus,
                        principalTable: "catalogs",
                        principalColumn: "catalog_id");
                    table.ForeignKey(
                        name: "FK_patient_modificationuser",
                        column: x => x.patient_modificationuser,
                        principalTable: "users",
                        principalColumn: "users_id");
                    table.ForeignKey(
                        name: "FK_patient_nationality",
                        column: x => x.patient_nationality,
                        principalTable: "countries",
                        principalColumn: "country_id");
                    table.ForeignKey(
                        name: "FK_patient_province",
                        column: x => x.patient_province,
                        principalTable: "catalogs",
                        principalColumn: "catalog_id");
                    table.ForeignKey(
                        name: "FK_patient_vocational_training",
                        column: x => x.patient_vocational_training,
                        principalTable: "catalogs",
                        principalColumn: "catalog_id");
                });

            migrationBuilder.CreateTable(
                name: "consultation",
                columns: table => new
                {
                    consultation_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    consultation_creationdate = table.Column<DateTime>(type: "datetime", nullable: true),
                    consultation_usercreate = table.Column<int>(type: "int", nullable: true),
                    consultation_patient = table.Column<int>(type: "int", nullable: false),
                    consultation_speciality = table.Column<int>(type: "int", nullable: true),
                    consultation_historyclinic = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    consultation_sequential = table.Column<int>(type: "int", nullable: true),
                    consultation_reason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    consultation_disease = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    consultation_familiaryname = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    consultation_warningsings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    consultation_nonpharmacologycal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    consultation_familiarytype = table.Column<int>(type: "int", nullable: true),
                    consultation_familiaryphone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    consultation_temperature = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    consultation_respirationrate = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    consultation_bloodpressuredAS = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    consultation_bloodpresuredDIS = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    consultation_pulse = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    consultation_weight = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    consultation_size = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    consultation_treatmentplan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    consultation_observation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    consultation_personalbackground = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    consultation_disablilitydays = table.Column<int>(type: "int", nullable: true),
                    consultation_type = table.Column<int>(type: "int", nullable: true, defaultValue: 1),
                    consultation_status = table.Column<int>(type: "int", nullable: true, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__consulta__650FE0FB3271D9FB", x => x.consultation_id);
                    table.ForeignKey(
                        name: "FK_consultation_patient",
                        column: x => x.consultation_patient,
                        principalTable: "patient",
                        principalColumn: "patient_id");
                    table.ForeignKey(
                        name: "FK_consultation_speciality",
                        column: x => x.consultation_speciality,
                        principalTable: "specialities",
                        principalColumn: "speciality_id");
                    table.ForeignKey(
                        name: "FK_consultation_usercreate",
                        column: x => x.consultation_usercreate,
                        principalTable: "users",
                        principalColumn: "users_id");
                });

            migrationBuilder.CreateTable(
                name: "doctor_patient",
                columns: table => new
                {
                    doctor_patient_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    doctor_userid = table.Column<int>(type: "int", nullable: false),
                    patient_id = table.Column<int>(type: "int", nullable: false),
                    creation_date = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    relationship_status = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__doctor_p__ED861B137F1430D0", x => x.doctor_patient_id);
                    table.ForeignKey(
                        name: "FK__doctor_pa__docto__607251E5",
                        column: x => x.doctor_userid,
                        principalTable: "users",
                        principalColumn: "users_id");
                    table.ForeignKey(
                        name: "FK__doctor_pa__patie__6166761E",
                        column: x => x.patient_id,
                        principalTable: "patient",
                        principalColumn: "patient_id");
                });

            migrationBuilder.CreateTable(
                name: "allergies_consultation",
                columns: table => new
                {
                    allergies_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    allergies_consultationid = table.Column<int>(type: "int", nullable: true),
                    allergies_creationdate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    allergies_catalogid = table.Column<int>(type: "int", nullable: false),
                    allergies_observation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    allergies_status = table.Column<int>(type: "int", nullable: true, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__allergie__1079FBD0C0A411FA", x => x.allergies_id);
                    table.ForeignKey(
                        name: "FK_allergies_catalogid",
                        column: x => x.allergies_catalogid,
                        principalTable: "catalogs",
                        principalColumn: "catalog_id");
                    table.ForeignKey(
                        name: "FK_allergies_consultationid",
                        column: x => x.allergies_consultationid,
                        principalTable: "consultation",
                        principalColumn: "consultation_id");
                });

            migrationBuilder.CreateTable(
                name: "appointment",
                columns: table => new
                {
                    appointment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    appointment_createdate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    appointment_modifydate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    appointment_createuser = table.Column<int>(type: "int", nullable: true),
                    appointment_modifyuser = table.Column<int>(type: "int", nullable: true),
                    appointment_date = table.Column<DateTime>(type: "datetime", nullable: false),
                    appointment_hour = table.Column<TimeOnly>(type: "time", nullable: false),
                    appointment_patientid = table.Column<int>(type: "int", nullable: true),
                    appointment_consultationid = table.Column<int>(type: "int", nullable: true),
                    appointment_status = table.Column<int>(type: "int", nullable: true, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__appointm__A50828FCE9ED1661", x => x.appointment_id);
                    table.ForeignKey(
                        name: "FK_appointment_consultationid",
                        column: x => x.appointment_consultationid,
                        principalTable: "consultation",
                        principalColumn: "consultation_id");
                    table.ForeignKey(
                        name: "FK_appointment_createuser",
                        column: x => x.appointment_createuser,
                        principalTable: "users",
                        principalColumn: "users_id");
                    table.ForeignKey(
                        name: "FK_appointment_modifyuser",
                        column: x => x.appointment_modifyuser,
                        principalTable: "users",
                        principalColumn: "users_id");
                    table.ForeignKey(
                        name: "FK_appointment_patientid",
                        column: x => x.appointment_patientid,
                        principalTable: "patient",
                        principalColumn: "patient_id");
                });

            migrationBuilder.CreateTable(
                name: "diagnosis_consultation",
                columns: table => new
                {
                    diagnosis_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    diagnosis_consultationid = table.Column<int>(type: "int", nullable: true),
                    diagnosis_diagnosisid = table.Column<int>(type: "int", nullable: true),
                    diagnosis_observation = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    diagnosis_presumptive = table.Column<bool>(type: "bit", nullable: true),
                    diagnosis_definitive = table.Column<bool>(type: "bit", nullable: true),
                    diagnosis_sequential = table.Column<int>(type: "int", nullable: true),
                    diagnosis_status = table.Column<int>(type: "int", nullable: true, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__diagnosi__D49E32B460371B2B", x => x.diagnosis_id);
                    table.ForeignKey(
                        name: "FK_diagnosis_consultationid",
                        column: x => x.diagnosis_consultationid,
                        principalTable: "consultation",
                        principalColumn: "consultation_id");
                    table.ForeignKey(
                        name: "FK_diagnosis_diagnosisid",
                        column: x => x.diagnosis_diagnosisid,
                        principalTable: "diagnosis",
                        principalColumn: "diagnosis_id");
                });

            migrationBuilder.CreateTable(
                name: "familiary_background",
                columns: table => new
                {
                    familiary_background_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    familiary_background_consultationid = table.Column<int>(type: "int", nullable: true),
                    familiary_background_heartdisease = table.Column<bool>(type: "bit", nullable: true),
                    familiary_background_heartdisease_observation = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    familiary_background_relatshcatalog_heartdisease = table.Column<int>(type: "int", nullable: true),
                    familiary_background_diabetes = table.Column<bool>(type: "bit", nullable: true),
                    familiary_background_diabetes_observation = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    familiary_background_relatshcatalog_diabetes = table.Column<int>(type: "int", nullable: true),
                    familiary_background_dxcardiovascular = table.Column<bool>(type: "bit", nullable: true),
                    familiary_background_dxcardiovascular_observation = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    familiary_background_relatshcatalog_dxcardiovascular = table.Column<int>(type: "int", nullable: true),
                    familiary_background_hypertension = table.Column<bool>(type: "bit", nullable: true),
                    familiary_background_hypertension_observation = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    familiary_background_relatshcatalog_hypertension = table.Column<int>(type: "int", nullable: true),
                    familiary_background_cancer = table.Column<bool>(type: "bit", nullable: true),
                    familiary_background_cancer_observation = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    familiary_background_relatshcatalog_cancer = table.Column<int>(type: "int", nullable: true),
                    familiary_background_tuberculosis = table.Column<bool>(type: "bit", nullable: true),
                    familiary_background_tuberculosis_observation = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    familiary_background_relatsh_tuberculosis = table.Column<int>(type: "int", nullable: true),
                    familiary_background_dxmental = table.Column<bool>(type: "bit", nullable: true),
                    familiary_background_dxmental_observation = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    familiary_background_relatshcatalog_dxmental = table.Column<int>(type: "int", nullable: true),
                    familiary_background_dxinfectious = table.Column<bool>(type: "bit", nullable: true),
                    familiary_background_dxinfectious_observation = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    familiary_background_relatshcatalog_dxinfectious = table.Column<int>(type: "int", nullable: true),
                    familiary_background_malformation = table.Column<bool>(type: "bit", nullable: true),
                    familiary_background_malformation_observation = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    familiary_background_relatshcatalog_malformation = table.Column<int>(type: "int", nullable: true),
                    familiary_background_other = table.Column<bool>(type: "bit", nullable: true),
                    familiary_background_other_observation = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    familiary_background_relatshcatalog_other = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__familiar__79624F3D4D02A2A7", x => x.familiary_background_id);
                    table.ForeignKey(
                        name: "FK_familiary_background_consultationid",
                        column: x => x.familiary_background_consultationid,
                        principalTable: "consultation",
                        principalColumn: "consultation_id");
                    table.ForeignKey(
                        name: "FK_familiary_background_relatsh_tuberculosis",
                        column: x => x.familiary_background_relatsh_tuberculosis,
                        principalTable: "catalogs",
                        principalColumn: "catalog_id");
                    table.ForeignKey(
                        name: "FK_familiary_background_relatshcatalog_cancer",
                        column: x => x.familiary_background_relatshcatalog_cancer,
                        principalTable: "catalogs",
                        principalColumn: "catalog_id");
                    table.ForeignKey(
                        name: "FK_familiary_background_relatshcatalog_diabetes",
                        column: x => x.familiary_background_relatshcatalog_diabetes,
                        principalTable: "catalogs",
                        principalColumn: "catalog_id");
                    table.ForeignKey(
                        name: "FK_familiary_background_relatshcatalog_dxcardiovascular",
                        column: x => x.familiary_background_relatshcatalog_dxcardiovascular,
                        principalTable: "catalogs",
                        principalColumn: "catalog_id");
                    table.ForeignKey(
                        name: "FK_familiary_background_relatshcatalog_dxinfectious",
                        column: x => x.familiary_background_relatshcatalog_dxinfectious,
                        principalTable: "catalogs",
                        principalColumn: "catalog_id");
                    table.ForeignKey(
                        name: "FK_familiary_background_relatshcatalog_dxmental",
                        column: x => x.familiary_background_relatshcatalog_dxmental,
                        principalTable: "catalogs",
                        principalColumn: "catalog_id");
                    table.ForeignKey(
                        name: "FK_familiary_background_relatshcatalog_heartdisease",
                        column: x => x.familiary_background_relatshcatalog_heartdisease,
                        principalTable: "catalogs",
                        principalColumn: "catalog_id");
                    table.ForeignKey(
                        name: "FK_familiary_background_relatshcatalog_hypertension",
                        column: x => x.familiary_background_relatshcatalog_hypertension,
                        principalTable: "catalogs",
                        principalColumn: "catalog_id");
                    table.ForeignKey(
                        name: "FK_familiary_background_relatshcatalog_malformation",
                        column: x => x.familiary_background_relatshcatalog_malformation,
                        principalTable: "catalogs",
                        principalColumn: "catalog_id");
                    table.ForeignKey(
                        name: "FK_familiary_background_relatshcatalog_other",
                        column: x => x.familiary_background_relatshcatalog_other,
                        principalTable: "catalogs",
                        principalColumn: "catalog_id");
                });

            migrationBuilder.CreateTable(
                name: "images_consutlation",
                columns: table => new
                {
                    images_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    images_consultationid = table.Column<int>(type: "int", nullable: true),
                    images_imagesid = table.Column<int>(type: "int", nullable: true),
                    images_amount = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    images_observation = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    images_sequential = table.Column<int>(type: "int", nullable: true),
                    images_status = table.Column<int>(type: "int", nullable: true, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__images_c__FA2651F7014D3056", x => x.images_id);
                    table.ForeignKey(
                        name: "FK_images_consultationid",
                        column: x => x.images_consultationid,
                        principalTable: "consultation",
                        principalColumn: "consultation_id");
                    table.ForeignKey(
                        name: "FK_images_imagesid",
                        column: x => x.images_imagesid,
                        principalTable: "images",
                        principalColumn: "images_id");
                });

            migrationBuilder.CreateTable(
                name: "laboratories_consultation",
                columns: table => new
                {
                    laboratories_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    laboratories_consultationid = table.Column<int>(type: "int", nullable: true),
                    laboratories_laboratoriesid = table.Column<int>(type: "int", nullable: true),
                    laboratories_amount = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    laboratories_observation = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    laboratories_sequential = table.Column<int>(type: "int", nullable: true),
                    laboratories_status = table.Column<int>(type: "int", nullable: true, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__laborato__949BB03979F91D77", x => x.laboratories_id);
                    table.ForeignKey(
                        name: "FK_laboratories_consultationid",
                        column: x => x.laboratories_consultationid,
                        principalTable: "consultation",
                        principalColumn: "consultation_id");
                    table.ForeignKey(
                        name: "FK_laboratories_laboratoriesid",
                        column: x => x.laboratories_laboratoriesid,
                        principalTable: "laboratories",
                        principalColumn: "laboratories_id");
                });

            migrationBuilder.CreateTable(
                name: "medications_consultation",
                columns: table => new
                {
                    medications_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    medications_consultationid = table.Column<int>(type: "int", nullable: true),
                    medications_medicationsid = table.Column<int>(type: "int", nullable: true),
                    medications_amount = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    medications_observation = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    medications_sequential = table.Column<int>(type: "int", nullable: true),
                    medications_status = table.Column<int>(type: "int", nullable: true, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__medicati__CF638DC5DB142D75", x => x.medications_id);
                    table.ForeignKey(
                        name: "FK_medications_consultationid",
                        column: x => x.medications_consultationid,
                        principalTable: "consultation",
                        principalColumn: "consultation_id");
                    table.ForeignKey(
                        name: "FK_medications_medicationsid",
                        column: x => x.medications_medicationsid,
                        principalTable: "medications",
                        principalColumn: "medications_id");
                });

            migrationBuilder.CreateTable(
                name: "organs_systems",
                columns: table => new
                {
                    organssystems_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    organssystems_consultationid = table.Column<int>(type: "int", nullable: true),
                    organssystems_respiratory = table.Column<bool>(type: "bit", nullable: true),
                    organssystems_respiratory_obs = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    organssystems_cardiovascular = table.Column<bool>(type: "bit", nullable: true),
                    organssystems_cardiovascular_obs = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    organssystems_digestive = table.Column<bool>(type: "bit", nullable: true),
                    organssystems_digestive_obs = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    organssystems_genital = table.Column<bool>(type: "bit", nullable: true),
                    organssystems_genital_obs = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    organssystems_urinary = table.Column<bool>(type: "bit", nullable: true),
                    organssystems_urinary_obs = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    organssystems_skeletal_m = table.Column<bool>(type: "bit", nullable: true),
                    organssystems_skeletal_m_obs = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    organssystems_endrocrine = table.Column<bool>(type: "bit", nullable: true),
                    organssystems_endocrine = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    organssystems_lymphatic = table.Column<bool>(type: "bit", nullable: true),
                    organssystems_lymphatic_obs = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    organssystems_nervous = table.Column<bool>(type: "bit", nullable: true),
                    organssystems_nervous_obs = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__organs_s__FEA205F7088992C8", x => x.organssystems_id);
                    table.ForeignKey(
                        name: "FK_organssystems_consultationid",
                        column: x => x.organssystems_consultationid,
                        principalTable: "consultation",
                        principalColumn: "consultation_id");
                });

            migrationBuilder.CreateTable(
                name: "physical_examination",
                columns: table => new
                {
                    physicalexamination_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    physicalexamination_consultationid = table.Column<int>(type: "int", nullable: true),
                    physicalexamination_head = table.Column<bool>(type: "bit", nullable: true),
                    physicalexamination_head_obs = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    physicalexamination_neck = table.Column<bool>(type: "bit", nullable: true),
                    physicalexamination_neck_obs = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    physicalexamination_chest = table.Column<bool>(type: "bit", nullable: true),
                    physicalexamination_chest_obs = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    physicalexamination_abdomen = table.Column<bool>(type: "bit", nullable: true),
                    physicalexamination_abdomen_obs = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    physicalexamination_pelvis = table.Column<bool>(type: "bit", nullable: true),
                    physicalexamination_pelvis_obs = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    physicalexamination_limbs = table.Column<bool>(type: "bit", nullable: true),
                    physicalexamination_limbs_obs = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__physical__3846C7A7B8AE3C81", x => x.physicalexamination_id);
                    table.ForeignKey(
                        name: "FK_physicalexamination_consultationid",
                        column: x => x.physicalexamination_consultationid,
                        principalTable: "consultation",
                        principalColumn: "consultation_id");
                });

            migrationBuilder.CreateTable(
                name: "surgeries_consultation",
                columns: table => new
                {
                    surgeries_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    surgeries_consultationid = table.Column<int>(type: "int", nullable: true),
                    surgeries_creationdate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    surgeries_catalogid = table.Column<int>(type: "int", nullable: true),
                    surgeries_observation = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    surgeries_status = table.Column<int>(type: "int", nullable: true, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__surgerie__0D3E1F77EFA352D6", x => x.surgeries_id);
                    table.ForeignKey(
                        name: "FK_surgeries_catalogid",
                        column: x => x.surgeries_catalogid,
                        principalTable: "catalogs",
                        principalColumn: "catalog_id");
                    table.ForeignKey(
                        name: "FK_surgeries_consultationid",
                        column: x => x.surgeries_consultationid,
                        principalTable: "consultation",
                        principalColumn: "consultation_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_allergies_consultation_allergies_catalogid",
                table: "allergies_consultation",
                column: "allergies_catalogid");

            migrationBuilder.CreateIndex(
                name: "IX_allergies_consultation_allergies_consultationid",
                table: "allergies_consultation",
                column: "allergies_consultationid");

            migrationBuilder.CreateIndex(
                name: "IX_appointment_appointment_consultationid",
                table: "appointment",
                column: "appointment_consultationid");

            migrationBuilder.CreateIndex(
                name: "IX_appointment_appointment_createuser",
                table: "appointment",
                column: "appointment_createuser");

            migrationBuilder.CreateIndex(
                name: "IX_appointment_appointment_modifyuser",
                table: "appointment",
                column: "appointment_modifyuser");

            migrationBuilder.CreateIndex(
                name: "IX_appointment_appointment_patientid",
                table: "appointment",
                column: "appointment_patientid");

            migrationBuilder.CreateIndex(
                name: "IX_assistant_doctor_relationship_assistant_userid",
                table: "assistant_doctor_relationship",
                column: "assistant_userid");

            migrationBuilder.CreateIndex(
                name: "IX_assistant_doctor_relationship_doctor_userid",
                table: "assistant_doctor_relationship",
                column: "doctor_userid");

            migrationBuilder.CreateIndex(
                name: "IX_consultation_consultation_patient",
                table: "consultation",
                column: "consultation_patient");

            migrationBuilder.CreateIndex(
                name: "IX_consultation_consultation_speciality",
                table: "consultation",
                column: "consultation_speciality");

            migrationBuilder.CreateIndex(
                name: "IX_consultation_consultation_usercreate",
                table: "consultation",
                column: "consultation_usercreate");

            migrationBuilder.CreateIndex(
                name: "IX_diagnosis_consultation_diagnosis_consultationid",
                table: "diagnosis_consultation",
                column: "diagnosis_consultationid");

            migrationBuilder.CreateIndex(
                name: "IX_diagnosis_consultation_diagnosis_diagnosisid",
                table: "diagnosis_consultation",
                column: "diagnosis_diagnosisid");

            migrationBuilder.CreateIndex(
                name: "IX_doctor_patient_doctor_userid",
                table: "doctor_patient",
                column: "doctor_userid");

            migrationBuilder.CreateIndex(
                name: "IX_doctor_patient_patient_id",
                table: "doctor_patient",
                column: "patient_id");

            migrationBuilder.CreateIndex(
                name: "IX_familiary_background_familiary_background_consultationid",
                table: "familiary_background",
                column: "familiary_background_consultationid");

            migrationBuilder.CreateIndex(
                name: "IX_familiary_background_familiary_background_relatsh_tuberculosis",
                table: "familiary_background",
                column: "familiary_background_relatsh_tuberculosis");

            migrationBuilder.CreateIndex(
                name: "IX_familiary_background_familiary_background_relatshcatalog_cancer",
                table: "familiary_background",
                column: "familiary_background_relatshcatalog_cancer");

            migrationBuilder.CreateIndex(
                name: "IX_familiary_background_familiary_background_relatshcatalog_diabetes",
                table: "familiary_background",
                column: "familiary_background_relatshcatalog_diabetes");

            migrationBuilder.CreateIndex(
                name: "IX_familiary_background_familiary_background_relatshcatalog_dxcardiovascular",
                table: "familiary_background",
                column: "familiary_background_relatshcatalog_dxcardiovascular");

            migrationBuilder.CreateIndex(
                name: "IX_familiary_background_familiary_background_relatshcatalog_dxinfectious",
                table: "familiary_background",
                column: "familiary_background_relatshcatalog_dxinfectious");

            migrationBuilder.CreateIndex(
                name: "IX_familiary_background_familiary_background_relatshcatalog_dxmental",
                table: "familiary_background",
                column: "familiary_background_relatshcatalog_dxmental");

            migrationBuilder.CreateIndex(
                name: "IX_familiary_background_familiary_background_relatshcatalog_heartdisease",
                table: "familiary_background",
                column: "familiary_background_relatshcatalog_heartdisease");

            migrationBuilder.CreateIndex(
                name: "IX_familiary_background_familiary_background_relatshcatalog_hypertension",
                table: "familiary_background",
                column: "familiary_background_relatshcatalog_hypertension");

            migrationBuilder.CreateIndex(
                name: "IX_familiary_background_familiary_background_relatshcatalog_malformation",
                table: "familiary_background",
                column: "familiary_background_relatshcatalog_malformation");

            migrationBuilder.CreateIndex(
                name: "IX_familiary_background_familiary_background_relatshcatalog_other",
                table: "familiary_background",
                column: "familiary_background_relatshcatalog_other");

            migrationBuilder.CreateIndex(
                name: "IX_images_consutlation_images_consultationid",
                table: "images_consutlation",
                column: "images_consultationid");

            migrationBuilder.CreateIndex(
                name: "IX_images_consutlation_images_imagesid",
                table: "images_consutlation",
                column: "images_imagesid");

            migrationBuilder.CreateIndex(
                name: "IX_laboratories_consultation_laboratories_consultationid",
                table: "laboratories_consultation",
                column: "laboratories_consultationid");

            migrationBuilder.CreateIndex(
                name: "IX_laboratories_consultation_laboratories_laboratoriesid",
                table: "laboratories_consultation",
                column: "laboratories_laboratoriesid");

            migrationBuilder.CreateIndex(
                name: "IX_medications_consultation_medications_consultationid",
                table: "medications_consultation",
                column: "medications_consultationid");

            migrationBuilder.CreateIndex(
                name: "IX_medications_consultation_medications_medicationsid",
                table: "medications_consultation",
                column: "medications_medicationsid");

            migrationBuilder.CreateIndex(
                name: "IX_organs_systems_organssystems_consultationid",
                table: "organs_systems",
                column: "organssystems_consultationid");

            migrationBuilder.CreateIndex(
                name: "IX_patient_patient_bloodtype",
                table: "patient",
                column: "patient_bloodtype");

            migrationBuilder.CreateIndex(
                name: "IX_patient_patient_creationuser",
                table: "patient",
                column: "patient_creationuser");

            migrationBuilder.CreateIndex(
                name: "IX_patient_patient_documenttype",
                table: "patient",
                column: "patient_documenttype");

            migrationBuilder.CreateIndex(
                name: "IX_patient_patient_gender",
                table: "patient",
                column: "patient_gender");

            migrationBuilder.CreateIndex(
                name: "IX_patient_patient_healt_insurance",
                table: "patient",
                column: "patient_healt_insurance");

            migrationBuilder.CreateIndex(
                name: "IX_patient_patient_maritalstatus",
                table: "patient",
                column: "patient_maritalstatus");

            migrationBuilder.CreateIndex(
                name: "IX_patient_patient_modificationuser",
                table: "patient",
                column: "patient_modificationuser");

            migrationBuilder.CreateIndex(
                name: "IX_patient_patient_nationality",
                table: "patient",
                column: "patient_nationality");

            migrationBuilder.CreateIndex(
                name: "IX_patient_patient_province",
                table: "patient",
                column: "patient_province");

            migrationBuilder.CreateIndex(
                name: "IX_patient_patient_vocational_training",
                table: "patient",
                column: "patient_vocational_training");

            migrationBuilder.CreateIndex(
                name: "IX_physical_examination_physicalexamination_consultationid",
                table: "physical_examination",
                column: "physicalexamination_consultationid");

            migrationBuilder.CreateIndex(
                name: "IX_provinces_province_countryid",
                table: "provinces",
                column: "province_countryid");

            migrationBuilder.CreateIndex(
                name: "IX_surgeries_consultation_surgeries_catalogid",
                table: "surgeries_consultation",
                column: "surgeries_catalogid");

            migrationBuilder.CreateIndex(
                name: "IX_surgeries_consultation_surgeries_consultationid",
                table: "surgeries_consultation",
                column: "surgeries_consultationid");

            migrationBuilder.CreateIndex(
                name: "IX_users_users_countryid",
                table: "users",
                column: "users_countryid");

            migrationBuilder.CreateIndex(
                name: "IX_users_users_profileid",
                table: "users",
                column: "users_profileid");

            migrationBuilder.CreateIndex(
                name: "IX_users_users_specialityid",
                table: "users",
                column: "users_specialityid");

            migrationBuilder.CreateIndex(
                name: "IX_users_users_vatpercentageid",
                table: "users",
                column: "users_vatpercentageid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "allergies_consultation");

            migrationBuilder.DropTable(
                name: "appointment");

            migrationBuilder.DropTable(
                name: "assistant_doctor_relationship");

            migrationBuilder.DropTable(
                name: "diagnosis_consultation");

            migrationBuilder.DropTable(
                name: "doctor_patient");

            migrationBuilder.DropTable(
                name: "familiary_background");

            migrationBuilder.DropTable(
                name: "images_consutlation");

            migrationBuilder.DropTable(
                name: "laboratories_consultation");

            migrationBuilder.DropTable(
                name: "medications_consultation");

            migrationBuilder.DropTable(
                name: "organs_systems");

            migrationBuilder.DropTable(
                name: "physical_examination");

            migrationBuilder.DropTable(
                name: "provinces");

            migrationBuilder.DropTable(
                name: "surgeries_consultation");

            migrationBuilder.DropTable(
                name: "user_schedules");

            migrationBuilder.DropTable(
                name: "diagnosis");

            migrationBuilder.DropTable(
                name: "images");

            migrationBuilder.DropTable(
                name: "laboratories");

            migrationBuilder.DropTable(
                name: "medications");

            migrationBuilder.DropTable(
                name: "consultation");

            migrationBuilder.DropTable(
                name: "patient");

            migrationBuilder.DropTable(
                name: "catalogs");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "countries");

            migrationBuilder.DropTable(
                name: "profiles");

            migrationBuilder.DropTable(
                name: "specialities");

            migrationBuilder.DropTable(
                name: "vat_billing");
        }
    }
}

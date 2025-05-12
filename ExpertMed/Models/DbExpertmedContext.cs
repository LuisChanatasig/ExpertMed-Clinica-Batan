using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ExpertMed.Models;

public partial class DbExpertmedContext : DbContext
{
    public DbExpertmedContext()
    {
    }

    public DbExpertmedContext(DbContextOptions<DbExpertmedContext> options)
        : base(options)
    {
    }
    // Dentro de tu DbContext
    public DbSet<AppointmentViewModel> AppointmentViewModels { get; set; }

    public virtual DbSet<AllergiesConsultation> AllergiesConsultations { get; set; }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<AssistantDoctorAppointment> AssistantDoctorAppointments { get; set; }

    public virtual DbSet<AssistantDoctorRelationship> AssistantDoctorRelationships { get; set; }

    public virtual DbSet<Billing> Billings { get; set; }

    public virtual DbSet<BillingDetail> BillingDetails { get; set; }

    public virtual DbSet<Catalog> Catalogs { get; set; }

    public virtual DbSet<Consultation> Consultations { get; set; }

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<Diagnosis> Diagnoses { get; set; }

    public virtual DbSet<DiagnosisConsultation> DiagnosisConsultations { get; set; }

    public virtual DbSet<DoctorPatient> DoctorPatients { get; set; }

    public virtual DbSet<Establishment> Establishments { get; set; }

    public virtual DbSet<FamiliaryBackground> FamiliaryBackgrounds { get; set; }

    public virtual DbSet<Image> Images { get; set; }

    public virtual DbSet<ImagesConsutlation> ImagesConsutlations { get; set; }

    public virtual DbSet<LaboratoriesConsultation> LaboratoriesConsultations { get; set; }

    public virtual DbSet<Laboratory> Laboratories { get; set; }

    public virtual DbSet<Medication> Medications { get; set; }

    public virtual DbSet<MedicationsConsultation> MedicationsConsultations { get; set; }

    public virtual DbSet<OrgansSystem> OrgansSystems { get; set; }

    public virtual DbSet<Patient> Patients { get; set; }

    public virtual DbSet<PhysicalExamination> PhysicalExaminations { get; set; }

    public virtual DbSet<Profile> Profiles { get; set; }

    public virtual DbSet<Province> Provinces { get; set; }

    public virtual DbSet<Speciality> Specialities { get; set; }

    public virtual DbSet<SurgeriesConsultation> SurgeriesConsultations { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserSchedule> UserSchedules { get; set; }

    public virtual DbSet<UserScheduleDay> UserScheduleDays { get; set; }

    public virtual DbSet<VatBilling> VatBillings { get; set; }

    //    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
    //        => optionsBuilder.UseSqlServer("Server=localhost;Database=DB_EXPERTMED;User Id=sa;Password=3xperMed2o25-$;TrustServerCertificate=True;");


    //    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
    //        => optionsBuilder.UseSqlServer("Server=localhost;Database=DB_EXPERTMED;User Id=sa;Password=Sur2o22--;TrustServerCertificate=True;");

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=DB_EXPERTMED_BATAN;User Id=sa;Password=Sur2o22--;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PatientDTO>().HasNoKey(); // sin PK porque no es una entidad real

        modelBuilder.Entity<AppointmentViewModel>().HasNoKey();        // Configurar AppointmentViewModel como entidad sin clave

        modelBuilder.Entity<AllergiesConsultation>(entity =>
        {
            entity.HasKey(e => e.AllergiesId).HasName("PK__allergie__1079FBD0C0A411FA");

            entity.ToTable("allergies_consultation");

            entity.Property(e => e.AllergiesId).HasColumnName("allergies_id");
            entity.Property(e => e.AllergiesCatalogid).HasColumnName("allergies_catalogid");
            entity.Property(e => e.AllergiesConsultationid).HasColumnName("allergies_consultationid");
            entity.Property(e => e.AllergiesCreationdate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("allergies_creationdate");
            entity.Property(e => e.AllergiesObservation).HasColumnName("allergies_observation");
            entity.Property(e => e.AllergiesStatus)
                .HasDefaultValue(1)
                .HasColumnName("allergies_status");

            entity.HasOne(d => d.AllergiesCatalog).WithMany(p => p.AllergiesConsultations)
                .HasForeignKey(d => d.AllergiesCatalogid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_allergies_catalogid");

            entity.HasOne(d => d.AllergiesConsultationNavigation).WithMany(p => p.AllergiesConsultations)
                .HasForeignKey(d => d.AllergiesConsultationid)
                .HasConstraintName("FK_allergies_consultationid");
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.AppointmentId).HasName("PK__appointm__A50828FCE9ED1661");

            entity.ToTable("appointment");

            entity.Property(e => e.AppointmentId).HasColumnName("appointment_id");
            entity.Property(e => e.AppointmentConsultationid).HasColumnName("appointment_consultationid");
            entity.Property(e => e.AppointmentCreatedate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("appointment_createdate");
            entity.Property(e => e.AppointmentCreateuser).HasColumnName("appointment_createuser");
            entity.Property(e => e.AppointmentDate)
                .HasColumnType("datetime")
                .HasColumnName("appointment_date");
            entity.Property(e => e.AppointmentHour).HasColumnName("appointment_hour");
            entity.Property(e => e.AppointmentModifydate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("appointment_modifydate");
            entity.Property(e => e.AppointmentModifyuser).HasColumnName("appointment_modifyuser");
            entity.Property(e => e.AppointmentPatientid).HasColumnName("appointment_patientid");
            entity.Property(e => e.AppointmentStatus)
                .HasDefaultValue(1)
                .HasColumnName("appointment_status");

            entity.HasOne(d => d.AppointmentConsultation).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.AppointmentConsultationid)
                .HasConstraintName("FK_appointment_consultationid");

            entity.HasOne(d => d.AppointmentCreateuserNavigation).WithMany(p => p.AppointmentAppointmentCreateuserNavigations)
                .HasForeignKey(d => d.AppointmentCreateuser)
                .HasConstraintName("FK_appointment_createuser");

            entity.HasOne(d => d.AppointmentModifyuserNavigation).WithMany(p => p.AppointmentAppointmentModifyuserNavigations)
                .HasForeignKey(d => d.AppointmentModifyuser)
                .HasConstraintName("FK_appointment_modifyuser");

            entity.HasOne(d => d.AppointmentPatient).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.AppointmentPatientid)
                .HasConstraintName("FK_appointment_patientid");
        });

        modelBuilder.Entity<AssistantDoctorAppointment>(entity =>
        {
            entity.HasKey(e => new { e.AssistantUserid, e.DoctorUserid, e.AppointmentId }).HasName("PK__assistan__55C1F616CDC3FB41");

            entity.ToTable("assistant_doctor_appointment");

            entity.Property(e => e.AssistantUserid).HasColumnName("assistant_userid");
            entity.Property(e => e.DoctorUserid).HasColumnName("doctor_userid");
            entity.Property(e => e.AppointmentId).HasColumnName("appointment_id");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreationDate)
                .HasColumnType("datetime")
                .HasColumnName("creation_date");
            entity.Property(e => e.RelationshipStatus).HasColumnName("relationship_status");

            entity.HasOne(d => d.Appointment).WithMany(p => p.AssistantDoctorAppointments)
                .HasForeignKey(d => d.AppointmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__assistant__appoi__7E02B4CC");

            entity.HasOne(d => d.AssistantUser).WithMany(p => p.AssistantDoctorAppointmentAssistantUsers)
                .HasForeignKey(d => d.AssistantUserid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__assistant__assis__7C1A6C5A");

            entity.HasOne(d => d.DoctorUser).WithMany(p => p.AssistantDoctorAppointmentDoctorUsers)
                .HasForeignKey(d => d.DoctorUserid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__assistant__docto__7D0E9093");
        });

        modelBuilder.Entity<AssistantDoctorRelationship>(entity =>
        {
            entity.HasKey(e => e.AssistandoctorId).HasName("PK__assistan__017A7BA914D54344");

            entity.ToTable("assistant_doctor_relationship");

            entity.Property(e => e.AssistandoctorId).HasColumnName("assistandoctor_id");
            entity.Property(e => e.AssistantUserid).HasColumnName("assistant_userid");
            entity.Property(e => e.AssitandoctorDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("assitandoctor_date");
            entity.Property(e => e.DoctorUserid).HasColumnName("doctor_userid");
            entity.Property(e => e.RelationshipStatus).HasColumnName("relationship_status");

            entity.HasOne(d => d.AssistantUser).WithMany(p => p.AssistantDoctorRelationshipAssistantUsers)
                .HasForeignKey(d => d.AssistantUserid)
                .HasConstraintName("FK_assistan_userid");

            entity.HasOne(d => d.DoctorUser).WithMany(p => p.AssistantDoctorRelationshipDoctorUsers)
                .HasForeignKey(d => d.DoctorUserid)
                .HasConstraintName("FK_doctor_userid");
        });

        modelBuilder.Entity<Billing>(entity =>
        {
            entity.HasKey(e => e.BillingId).HasName("PK__billing__50157129FBD5014C");

            entity.ToTable("billing");

            entity.Property(e => e.BillingId).HasColumnName("billing_id");
            entity.Property(e => e.AppointmentId).HasColumnName("appointment_id");
            entity.Property(e => e.BillingCreationdate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("billing_creationdate");
            entity.Property(e => e.BillingPaymentMethod)
                .HasMaxLength(255)
                .HasColumnName("billing_payment_method");
            entity.Property(e => e.BillingProofOfPayment).HasColumnName("billing_proof_of_payment");
            entity.Property(e => e.BillingSequential).HasColumnName("billing_sequential");
            entity.Property(e => e.BillingTotal)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("billing_total");

            entity.HasOne(d => d.Appointment).WithMany(p => p.Billings)
                .HasForeignKey(d => d.AppointmentId)
                .HasConstraintName("FK_billing_appointment_id");
        });

        modelBuilder.Entity<BillingDetail>(entity =>
        {
            entity.HasKey(e => e.BillingDetailsId).HasName("PK__billing___BBA375560BA5BD3D");

            entity.ToTable("billing_details");

            entity.Property(e => e.BillingDetailsId).HasColumnName("billing_details_id");
            entity.Property(e => e.BillingDetailsAddress)
                .HasMaxLength(255)
                .HasColumnName("billing_details_address");
            entity.Property(e => e.BillingDetailsCinumber)
                .HasMaxLength(255)
                .HasColumnName("billing_details_cinumber");
            entity.Property(e => e.BillingDetailsDocumenttype)
                .HasMaxLength(255)
                .HasColumnName("billing_details_documenttype");
            entity.Property(e => e.BillingDetailsEmail)
                .HasMaxLength(255)
                .HasColumnName("billing_details_email");
            entity.Property(e => e.BillingDetailsNames)
                .HasMaxLength(255)
                .HasColumnName("billing_details_names");
            entity.Property(e => e.BillingDetailsPhone)
                .HasMaxLength(255)
                .HasColumnName("billing_details_phone");
            entity.Property(e => e.BillingId).HasColumnName("billing_id");

            entity.HasOne(d => d.Billing).WithMany(p => p.BillingDetails)
                .HasForeignKey(d => d.BillingId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_billing_details_billing");
        });

        modelBuilder.Entity<Catalog>(entity =>
        {
            entity.HasKey(e => e.CatalogId).HasName("PK__catalogs__9871D9500A81A69B");

            entity.ToTable("catalogs");

            entity.Property(e => e.CatalogId).HasColumnName("catalog_id");
            entity.Property(e => e.CatalogCategory)
                .HasMaxLength(200)
                .HasColumnName("catalog_category");
            entity.Property(e => e.CatalogName)
                .HasMaxLength(2500)
                .HasColumnName("catalog_name");
            entity.Property(e => e.CategoryStatus)
                .HasDefaultValue(1)
                .HasColumnName("category_status");
        });

        modelBuilder.Entity<Consultation>(entity =>
        {
            entity.HasKey(e => e.ConsultationId).HasName("PK__consulta__650FE0FB3271D9FB");

            entity.ToTable("consultation");

            entity.Property(e => e.ConsultationId).HasColumnName("consultation_id");
            entity.Property(e => e.ConsultationBloodpressuredAs)
                .HasMaxLength(10)
                .HasColumnName("consultation_bloodpressuredAS");
            entity.Property(e => e.ConsultationBloodpresuredDis)
                .HasMaxLength(10)
                .HasColumnName("consultation_bloodpresuredDIS");
            entity.Property(e => e.ConsultationCreationdate)
                .HasColumnType("datetime")
                .HasColumnName("consultation_creationdate");
            entity.Property(e => e.ConsultationDisablilitydays).HasColumnName("consultation_disablilitydays");
            entity.Property(e => e.ConsultationDisease).HasColumnName("consultation_disease");
            entity.Property(e => e.ConsultationEvolutionNotes).HasColumnName("consultation_evolution_notes");
            entity.Property(e => e.ConsultationFamiliaryname).HasColumnName("consultation_familiaryname");
            entity.Property(e => e.ConsultationFamiliaryphone)
                .HasMaxLength(20)
                .HasColumnName("consultation_familiaryphone");
            entity.Property(e => e.ConsultationFamiliarytype).HasColumnName("consultation_familiarytype");
            entity.Property(e => e.ConsultationHistoryclinic)
                .HasMaxLength(255)
                .HasColumnName("consultation_historyclinic");
            entity.Property(e => e.ConsultationNonpharmacologycal).HasColumnName("consultation_nonpharmacologycal");
            entity.Property(e => e.ConsultationObservation).HasColumnName("consultation_observation");
            entity.Property(e => e.ConsultationPatient).HasColumnName("consultation_patient");
            entity.Property(e => e.ConsultationPersonalbackground).HasColumnName("consultation_personalbackground");
            entity.Property(e => e.ConsultationPulse)
                .HasMaxLength(3)
                .HasColumnName("consultation_pulse");
            entity.Property(e => e.ConsultationReason)
                .HasMaxLength(255)
                .HasColumnName("consultation_reason");
            entity.Property(e => e.ConsultationRespirationrate)
                .HasMaxLength(10)
                .HasColumnName("consultation_respirationrate");
            entity.Property(e => e.ConsultationSequential).HasColumnName("consultation_sequential");
            entity.Property(e => e.ConsultationSize)
                .HasMaxLength(3)
                .HasColumnName("consultation_size");
            entity.Property(e => e.ConsultationSpeciality).HasColumnName("consultation_speciality");
            entity.Property(e => e.ConsultationStatus)
                .HasDefaultValue(1)
                .HasColumnName("consultation_status");
            entity.Property(e => e.ConsultationTemperature)
                .HasMaxLength(10)
                .HasColumnName("consultation_temperature");
            entity.Property(e => e.ConsultationTherapies).HasColumnName("consultation_therapies");
            entity.Property(e => e.ConsultationTreatmentplan).HasColumnName("consultation_treatmentplan");
            entity.Property(e => e.ConsultationType)
                .HasDefaultValue(1)
                .HasColumnName("consultation_type");
            entity.Property(e => e.ConsultationUsercreate).HasColumnName("consultation_usercreate");
            entity.Property(e => e.ConsultationWarningsings).HasColumnName("consultation_warningsings");
            entity.Property(e => e.ConsultationWeight)
                .HasMaxLength(3)
                .HasColumnName("consultation_weight");

            entity.HasOne(d => d.ConsultationPatientNavigation).WithMany(p => p.Consultations)
                .HasForeignKey(d => d.ConsultationPatient)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_consultation_patient");

            entity.HasOne(d => d.ConsultationSpecialityNavigation).WithMany(p => p.Consultations)
                .HasForeignKey(d => d.ConsultationSpeciality)
                .HasConstraintName("FK_consultation_speciality");

            entity.HasOne(d => d.ConsultationUsercreateNavigation).WithMany(p => p.Consultations)
                .HasForeignKey(d => d.ConsultationUsercreate)
                .HasConstraintName("FK_consultation_usercreate");
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.HasKey(e => e.CountryId).HasName("PK__countrie__7E8CD055511F6D94");

            entity.ToTable("countries");

            entity.Property(e => e.CountryId).HasColumnName("country_id");
            entity.Property(e => e.CountryCode)
                .HasMaxLength(6)
                .HasColumnName("country_code");
            entity.Property(e => e.CountryIso)
                .HasMaxLength(6)
                .HasColumnName("country_iso");
            entity.Property(e => e.CountryName)
                .HasMaxLength(255)
                .HasColumnName("country_name");
            entity.Property(e => e.CountryNationality)
                .HasMaxLength(250)
                .HasColumnName("country_nationality");
            entity.Property(e => e.CountryStatus)
                .HasDefaultValue(1)
                .HasColumnName("country_status");
        });

        modelBuilder.Entity<Diagnosis>(entity =>
        {
            entity.HasKey(e => e.DiagnosisId).HasName("PK__diagnosi__D49E32B47AF704DC");

            entity.ToTable("diagnosis");

            entity.Property(e => e.DiagnosisId).HasColumnName("diagnosis_id");
            entity.Property(e => e.DiagnosisCategory)
                .HasMaxLength(255)
                .HasColumnName("diagnosis_category");
            entity.Property(e => e.DiagnosisCie10)
                .HasMaxLength(20)
                .HasColumnName("diagnosis_cie10");
            entity.Property(e => e.DiagnosisDescription)
                .HasMaxLength(255)
                .HasColumnName("diagnosis_description");
            entity.Property(e => e.DiagnosisName)
                .HasMaxLength(255)
                .HasColumnName("diagnosis_name");
            entity.Property(e => e.DiagnosisStatus)
                .HasDefaultValue(1)
                .HasColumnName("diagnosis_status");
        });

        modelBuilder.Entity<DiagnosisConsultation>(entity =>
        {
            entity.HasKey(e => e.DiagnosisId).HasName("PK__diagnosi__D49E32B460371B2B");

            entity.ToTable("diagnosis_consultation");

            entity.Property(e => e.DiagnosisId).HasColumnName("diagnosis_id");
            entity.Property(e => e.DiagnosisConsultationid).HasColumnName("diagnosis_consultationid");
            entity.Property(e => e.DiagnosisDefinitive).HasColumnName("diagnosis_definitive");
            entity.Property(e => e.DiagnosisDiagnosisid).HasColumnName("diagnosis_diagnosisid");
            entity.Property(e => e.DiagnosisObservation)
                .HasMaxLength(255)
                .HasColumnName("diagnosis_observation");
            entity.Property(e => e.DiagnosisPresumptive).HasColumnName("diagnosis_presumptive");
            entity.Property(e => e.DiagnosisSequential).HasColumnName("diagnosis_sequential");
            entity.Property(e => e.DiagnosisStatus)
                .HasDefaultValue(1)
                .HasColumnName("diagnosis_status");

            entity.HasOne(d => d.DiagnosisConsultationNavigation).WithMany(p => p.DiagnosisConsultations)
                .HasForeignKey(d => d.DiagnosisConsultationid)
                .HasConstraintName("FK_diagnosis_consultationid");

            entity.HasOne(d => d.DiagnosisDiagnosis).WithMany(p => p.DiagnosisConsultations)
                .HasForeignKey(d => d.DiagnosisDiagnosisid)
                .HasConstraintName("FK_diagnosis_diagnosisid");
        });

        modelBuilder.Entity<DoctorPatient>(entity =>
        {
            entity.HasKey(e => e.DoctorPatientId).HasName("PK__doctor_p__ED861B137F1430D0");

            entity.ToTable("doctor_patient");

            entity.Property(e => e.DoctorPatientId).HasColumnName("doctor_patient_id");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("creation_date");
            entity.Property(e => e.DoctorUserid).HasColumnName("doctor_userid");
            entity.Property(e => e.PatientId).HasColumnName("patient_id");
            entity.Property(e => e.RelationshipStatus)
                .HasDefaultValue(1)
                .HasColumnName("relationship_status");

            entity.HasOne(d => d.DoctorUser).WithMany(p => p.DoctorPatients)
                .HasForeignKey(d => d.DoctorUserid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__doctor_pa__docto__607251E5");

            entity.HasOne(d => d.Patient).WithMany(p => p.DoctorPatients)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__doctor_pa__patie__6166761E");
        });

        modelBuilder.Entity<Establishment>(entity =>
        {
            entity.HasKey(e => e.EstablishmentId).HasName("PK__establis__95F24A085E6DE2E5");

            entity.ToTable("establishment");

            entity.Property(e => e.EstablishmentId).HasColumnName("establishment_id");
            entity.Property(e => e.EstablishmentAddress)
                .HasMaxLength(255)
                .HasColumnName("establishment_address");
            entity.Property(e => e.EstablishmentCreationdate)
                .HasColumnType("datetime")
                .HasColumnName("establishment_creationdate");
            entity.Property(e => e.EstablishmentEmissionpoint)
                .HasMaxLength(4)
                .HasColumnName("establishment_emissionpoint");
            entity.Property(e => e.EstablishmentModificationdate)
                .HasColumnType("datetime")
                .HasColumnName("establishment_modificationdate");
            entity.Property(e => e.EstablishmentName)
                .HasMaxLength(255)
                .HasColumnName("establishment_name");
            entity.Property(e => e.EstablishmentPointofsale)
                .HasMaxLength(4)
                .HasColumnName("establishment_pointofsale");
            entity.Property(e => e.EstablishmentSequentialBilling).HasColumnName("establishment_sequential_billing");
        });

        modelBuilder.Entity<FamiliaryBackground>(entity =>
        {
            entity.HasKey(e => e.FamiliaryBackgroundId).HasName("PK__familiar__79624F3D4D02A2A7");

            entity.ToTable("familiary_background");

            entity.Property(e => e.FamiliaryBackgroundId).HasColumnName("familiary_background_id");
            entity.Property(e => e.FamiliaryBackgroundCancer).HasColumnName("familiary_background_cancer");
            entity.Property(e => e.FamiliaryBackgroundCancerObservation)
                .HasMaxLength(255)
                .HasColumnName("familiary_background_cancer_observation");
            entity.Property(e => e.FamiliaryBackgroundConsultationid).HasColumnName("familiary_background_consultationid");
            entity.Property(e => e.FamiliaryBackgroundDiabetes).HasColumnName("familiary_background_diabetes");
            entity.Property(e => e.FamiliaryBackgroundDiabetesObservation)
                .HasMaxLength(255)
                .HasColumnName("familiary_background_diabetes_observation");
            entity.Property(e => e.FamiliaryBackgroundDxcardiovascular).HasColumnName("familiary_background_dxcardiovascular");
            entity.Property(e => e.FamiliaryBackgroundDxcardiovascularObservation)
                .HasMaxLength(255)
                .HasColumnName("familiary_background_dxcardiovascular_observation");
            entity.Property(e => e.FamiliaryBackgroundDxinfectious).HasColumnName("familiary_background_dxinfectious");
            entity.Property(e => e.FamiliaryBackgroundDxinfectiousObservation)
                .HasMaxLength(255)
                .HasColumnName("familiary_background_dxinfectious_observation");
            entity.Property(e => e.FamiliaryBackgroundDxmental).HasColumnName("familiary_background_dxmental");
            entity.Property(e => e.FamiliaryBackgroundDxmentalObservation)
                .HasMaxLength(255)
                .HasColumnName("familiary_background_dxmental_observation");
            entity.Property(e => e.FamiliaryBackgroundHeartdisease).HasColumnName("familiary_background_heartdisease");
            entity.Property(e => e.FamiliaryBackgroundHeartdiseaseObservation)
                .HasMaxLength(255)
                .HasColumnName("familiary_background_heartdisease_observation");
            entity.Property(e => e.FamiliaryBackgroundHypertension).HasColumnName("familiary_background_hypertension");
            entity.Property(e => e.FamiliaryBackgroundHypertensionObservation)
                .HasMaxLength(255)
                .HasColumnName("familiary_background_hypertension_observation");
            entity.Property(e => e.FamiliaryBackgroundMalformation).HasColumnName("familiary_background_malformation");
            entity.Property(e => e.FamiliaryBackgroundMalformationObservation)
                .HasMaxLength(255)
                .HasColumnName("familiary_background_malformation_observation");
            entity.Property(e => e.FamiliaryBackgroundOther).HasColumnName("familiary_background_other");
            entity.Property(e => e.FamiliaryBackgroundOtherObservation)
                .HasMaxLength(255)
                .HasColumnName("familiary_background_other_observation");
            entity.Property(e => e.FamiliaryBackgroundRelatshTuberculosis).HasColumnName("familiary_background_relatsh_tuberculosis");
            entity.Property(e => e.FamiliaryBackgroundRelatshcatalogCancer).HasColumnName("familiary_background_relatshcatalog_cancer");
            entity.Property(e => e.FamiliaryBackgroundRelatshcatalogDiabetes).HasColumnName("familiary_background_relatshcatalog_diabetes");
            entity.Property(e => e.FamiliaryBackgroundRelatshcatalogDxcardiovascular).HasColumnName("familiary_background_relatshcatalog_dxcardiovascular");
            entity.Property(e => e.FamiliaryBackgroundRelatshcatalogDxinfectious).HasColumnName("familiary_background_relatshcatalog_dxinfectious");
            entity.Property(e => e.FamiliaryBackgroundRelatshcatalogDxmental).HasColumnName("familiary_background_relatshcatalog_dxmental");
            entity.Property(e => e.FamiliaryBackgroundRelatshcatalogHeartdisease).HasColumnName("familiary_background_relatshcatalog_heartdisease");
            entity.Property(e => e.FamiliaryBackgroundRelatshcatalogHypertension).HasColumnName("familiary_background_relatshcatalog_hypertension");
            entity.Property(e => e.FamiliaryBackgroundRelatshcatalogMalformation).HasColumnName("familiary_background_relatshcatalog_malformation");
            entity.Property(e => e.FamiliaryBackgroundRelatshcatalogOther).HasColumnName("familiary_background_relatshcatalog_other");
            entity.Property(e => e.FamiliaryBackgroundTuberculosis).HasColumnName("familiary_background_tuberculosis");
            entity.Property(e => e.FamiliaryBackgroundTuberculosisObservation)
                .HasMaxLength(255)
                .HasColumnName("familiary_background_tuberculosis_observation");

            entity.HasOne(d => d.FamiliaryBackgroundConsultation).WithMany(p => p.FamiliaryBackgrounds)
                .HasForeignKey(d => d.FamiliaryBackgroundConsultationid)
                .HasConstraintName("FK_familiary_background_consultationid");

            entity.HasOne(d => d.FamiliaryBackgroundRelatshTuberculosisNavigation).WithMany(p => p.FamiliaryBackgroundFamiliaryBackgroundRelatshTuberculosisNavigations)
                .HasForeignKey(d => d.FamiliaryBackgroundRelatshTuberculosis)
                .HasConstraintName("FK_familiary_background_relatsh_tuberculosis");

            entity.HasOne(d => d.FamiliaryBackgroundRelatshcatalogCancerNavigation).WithMany(p => p.FamiliaryBackgroundFamiliaryBackgroundRelatshcatalogCancerNavigations)
                .HasForeignKey(d => d.FamiliaryBackgroundRelatshcatalogCancer)
                .HasConstraintName("FK_familiary_background_relatshcatalog_cancer");

            entity.HasOne(d => d.FamiliaryBackgroundRelatshcatalogDiabetesNavigation).WithMany(p => p.FamiliaryBackgroundFamiliaryBackgroundRelatshcatalogDiabetesNavigations)
                .HasForeignKey(d => d.FamiliaryBackgroundRelatshcatalogDiabetes)
                .HasConstraintName("FK_familiary_background_relatshcatalog_diabetes");

            entity.HasOne(d => d.FamiliaryBackgroundRelatshcatalogDxcardiovascularNavigation).WithMany(p => p.FamiliaryBackgroundFamiliaryBackgroundRelatshcatalogDxcardiovascularNavigations)
                .HasForeignKey(d => d.FamiliaryBackgroundRelatshcatalogDxcardiovascular)
                .HasConstraintName("FK_familiary_background_relatshcatalog_dxcardiovascular");

            entity.HasOne(d => d.FamiliaryBackgroundRelatshcatalogDxinfectiousNavigation).WithMany(p => p.FamiliaryBackgroundFamiliaryBackgroundRelatshcatalogDxinfectiousNavigations)
                .HasForeignKey(d => d.FamiliaryBackgroundRelatshcatalogDxinfectious)
                .HasConstraintName("FK_familiary_background_relatshcatalog_dxinfectious");

            entity.HasOne(d => d.FamiliaryBackgroundRelatshcatalogDxmentalNavigation).WithMany(p => p.FamiliaryBackgroundFamiliaryBackgroundRelatshcatalogDxmentalNavigations)
                .HasForeignKey(d => d.FamiliaryBackgroundRelatshcatalogDxmental)
                .HasConstraintName("FK_familiary_background_relatshcatalog_dxmental");

            entity.HasOne(d => d.FamiliaryBackgroundRelatshcatalogHeartdiseaseNavigation).WithMany(p => p.FamiliaryBackgroundFamiliaryBackgroundRelatshcatalogHeartdiseaseNavigations)
                .HasForeignKey(d => d.FamiliaryBackgroundRelatshcatalogHeartdisease)
                .HasConstraintName("FK_familiary_background_relatshcatalog_heartdisease");

            entity.HasOne(d => d.FamiliaryBackgroundRelatshcatalogHypertensionNavigation).WithMany(p => p.FamiliaryBackgroundFamiliaryBackgroundRelatshcatalogHypertensionNavigations)
                .HasForeignKey(d => d.FamiliaryBackgroundRelatshcatalogHypertension)
                .HasConstraintName("FK_familiary_background_relatshcatalog_hypertension");

            entity.HasOne(d => d.FamiliaryBackgroundRelatshcatalogMalformationNavigation).WithMany(p => p.FamiliaryBackgroundFamiliaryBackgroundRelatshcatalogMalformationNavigations)
                .HasForeignKey(d => d.FamiliaryBackgroundRelatshcatalogMalformation)
                .HasConstraintName("FK_familiary_background_relatshcatalog_malformation");

            entity.HasOne(d => d.FamiliaryBackgroundRelatshcatalogOtherNavigation).WithMany(p => p.FamiliaryBackgroundFamiliaryBackgroundRelatshcatalogOtherNavigations)
                .HasForeignKey(d => d.FamiliaryBackgroundRelatshcatalogOther)
                .HasConstraintName("FK_familiary_background_relatshcatalog_other");
        });

        modelBuilder.Entity<Image>(entity =>
        {
            entity.HasKey(e => e.ImagesId).HasName("PK__images__FA2651F7ADED15C4");

            entity.ToTable("images");

            entity.Property(e => e.ImagesId).HasColumnName("images_id");
            entity.Property(e => e.ImagesCategory)
                .HasMaxLength(255)
                .HasColumnName("images_category");
            entity.Property(e => e.ImagesCie10)
                .HasMaxLength(20)
                .HasColumnName("images_cie10");
            entity.Property(e => e.ImagesDescription)
                .HasMaxLength(255)
                .HasColumnName("images_description");
            entity.Property(e => e.ImagesName)
                .HasMaxLength(255)
                .HasColumnName("images_name");
            entity.Property(e => e.ImagesStatus)
                .HasDefaultValue(1)
                .HasColumnName("images_status");
        });

        modelBuilder.Entity<ImagesConsutlation>(entity =>
        {
            entity.HasKey(e => e.ImagesId).HasName("PK__images_c__FA2651F7014D3056");

            entity.ToTable("images_consutlation");

            entity.Property(e => e.ImagesId).HasColumnName("images_id");
            entity.Property(e => e.ImagesAmount)
                .HasMaxLength(255)
                .HasColumnName("images_amount");
            entity.Property(e => e.ImagesConsultationid).HasColumnName("images_consultationid");
            entity.Property(e => e.ImagesImagesid).HasColumnName("images_imagesid");
            entity.Property(e => e.ImagesObservation)
                .HasMaxLength(255)
                .HasColumnName("images_observation");
            entity.Property(e => e.ImagesSequential).HasColumnName("images_sequential");
            entity.Property(e => e.ImagesStatus)
                .HasDefaultValue(1)
                .HasColumnName("images_status");

            entity.HasOne(d => d.ImagesConsultation).WithMany(p => p.ImagesConsutlations)
                .HasForeignKey(d => d.ImagesConsultationid)
                .HasConstraintName("FK_images_consultationid");

            entity.HasOne(d => d.ImagesImages).WithMany(p => p.ImagesConsutlations)
                .HasForeignKey(d => d.ImagesImagesid)
                .HasConstraintName("FK_images_imagesid");
        });

        modelBuilder.Entity<LaboratoriesConsultation>(entity =>
        {
            entity.HasKey(e => e.LaboratoriesId).HasName("PK__laborato__949BB03979F91D77");

            entity.ToTable("laboratories_consultation");

            entity.Property(e => e.LaboratoriesId).HasColumnName("laboratories_id");
            entity.Property(e => e.LaboratoriesAmount)
                .HasMaxLength(255)
                .HasColumnName("laboratories_amount");
            entity.Property(e => e.LaboratoriesConsultationid).HasColumnName("laboratories_consultationid");
            entity.Property(e => e.LaboratoriesLaboratoriesid).HasColumnName("laboratories_laboratoriesid");
            entity.Property(e => e.LaboratoriesObservation)
                .HasMaxLength(255)
                .HasColumnName("laboratories_observation");
            entity.Property(e => e.LaboratoriesSequential).HasColumnName("laboratories_sequential");
            entity.Property(e => e.LaboratoriesStatus)
                .HasDefaultValue(1)
                .HasColumnName("laboratories_status");

            entity.HasOne(d => d.LaboratoriesConsultationNavigation).WithMany(p => p.LaboratoriesConsultations)
                .HasForeignKey(d => d.LaboratoriesConsultationid)
                .HasConstraintName("FK_laboratories_consultationid");

            entity.HasOne(d => d.LaboratoriesLaboratories).WithMany(p => p.LaboratoriesConsultations)
                .HasForeignKey(d => d.LaboratoriesLaboratoriesid)
                .HasConstraintName("FK_laboratories_laboratoriesid");
        });

        modelBuilder.Entity<Laboratory>(entity =>
        {
            entity.HasKey(e => e.LaboratoriesId).HasName("PK__laborato__949BB039323204CF");

            entity.ToTable("laboratories");

            entity.Property(e => e.LaboratoriesId).HasColumnName("laboratories_id");
            entity.Property(e => e.LaboratoriesCategory)
                .HasMaxLength(255)
                .HasColumnName("laboratories_category");
            entity.Property(e => e.LaboratoriesCie10)
                .HasMaxLength(20)
                .HasColumnName("laboratories_cie10");
            entity.Property(e => e.LaboratoriesDescription)
                .HasMaxLength(255)
                .HasColumnName("laboratories_description");
            entity.Property(e => e.LaboratoriesName)
                .HasMaxLength(255)
                .HasColumnName("laboratories_name");
            entity.Property(e => e.LaboratoriesStatus)
                .HasDefaultValue(1)
                .HasColumnName("laboratories_status");
        });

        modelBuilder.Entity<Medication>(entity =>
        {
            entity.HasKey(e => e.MedicationsId).HasName("PK__medicati__CF638DC5C2A78939");

            entity.ToTable("medications");

            entity.Property(e => e.MedicationsId).HasColumnName("medications_id");
            entity.Property(e => e.MedicationsCategory)
                .HasMaxLength(255)
                .HasColumnName("medications_category");
            entity.Property(e => e.MedicationsCie10)
                .HasMaxLength(20)
                .HasColumnName("medications_cie10");
            entity.Property(e => e.MedicationsConcentration)
                .HasMaxLength(255)
                .HasColumnName("medications_concentration");
            entity.Property(e => e.MedicationsDescription)
                .HasMaxLength(255)
                .HasColumnName("medications_description");
            entity.Property(e => e.MedicationsDistinctive)
                .HasMaxLength(255)
                .HasColumnName("medications_distinctive");
            entity.Property(e => e.MedicationsName)
                .HasMaxLength(255)
                .HasColumnName("medications_name");
            entity.Property(e => e.MedicationsStatus)
                .HasDefaultValue(1)
                .HasColumnName("medications_status");
        });

        modelBuilder.Entity<MedicationsConsultation>(entity =>
        {
            entity.HasKey(e => e.MedicationsId).HasName("PK__medicati__CF638DC5DB142D75");

            entity.ToTable("medications_consultation");

            entity.Property(e => e.MedicationsId).HasColumnName("medications_id");
            entity.Property(e => e.MedicationsAmount)
                .HasMaxLength(255)
                .HasColumnName("medications_amount");
            entity.Property(e => e.MedicationsConsultationid).HasColumnName("medications_consultationid");
            entity.Property(e => e.MedicationsMedicationsid).HasColumnName("medications_medicationsid");
            entity.Property(e => e.MedicationsObservation)
                .HasMaxLength(255)
                .HasColumnName("medications_observation");
            entity.Property(e => e.MedicationsSequential).HasColumnName("medications_sequential");
            entity.Property(e => e.MedicationsStatus)
                .HasDefaultValue(1)
                .HasColumnName("medications_status");

            entity.HasOne(d => d.MedicationsConsultationNavigation).WithMany(p => p.MedicationsConsultations)
                .HasForeignKey(d => d.MedicationsConsultationid)
                .HasConstraintName("FK_medications_consultationid");

            entity.HasOne(d => d.MedicationsMedications).WithMany(p => p.MedicationsConsultations)
                .HasForeignKey(d => d.MedicationsMedicationsid)
                .HasConstraintName("FK_medications_medicationsid");
        });

        modelBuilder.Entity<OrgansSystem>(entity =>
        {
            entity.HasKey(e => e.OrganssystemsId).HasName("PK__organs_s__FEA205F7088992C8");

            entity.ToTable("organs_systems");

            entity.Property(e => e.OrganssystemsId).HasColumnName("organssystems_id");
            entity.Property(e => e.OrganssystemsCardiovascular).HasColumnName("organssystems_cardiovascular");
            entity.Property(e => e.OrganssystemsCardiovascularObs)
                .HasMaxLength(255)
                .HasColumnName("organssystems_cardiovascular_obs");
            entity.Property(e => e.OrganssystemsConsultationid).HasColumnName("organssystems_consultationid");
            entity.Property(e => e.OrganssystemsDigestive).HasColumnName("organssystems_digestive");
            entity.Property(e => e.OrganssystemsDigestiveObs)
                .HasMaxLength(255)
                .HasColumnName("organssystems_digestive_obs");
            entity.Property(e => e.OrganssystemsEndocrine)
                .HasMaxLength(255)
                .HasColumnName("organssystems_endocrine");
            entity.Property(e => e.OrganssystemsEndrocrine).HasColumnName("organssystems_endrocrine");
            entity.Property(e => e.OrganssystemsGenital).HasColumnName("organssystems_genital");
            entity.Property(e => e.OrganssystemsGenitalObs)
                .HasMaxLength(255)
                .HasColumnName("organssystems_genital_obs");
            entity.Property(e => e.OrganssystemsLymphatic).HasColumnName("organssystems_lymphatic");
            entity.Property(e => e.OrganssystemsLymphaticObs)
                .HasMaxLength(255)
                .HasColumnName("organssystems_lymphatic_obs");
            entity.Property(e => e.OrganssystemsNervous).HasColumnName("organssystems_nervous");
            entity.Property(e => e.OrganssystemsNervousObs)
                .HasMaxLength(255)
                .HasColumnName("organssystems_nervous_obs");
            entity.Property(e => e.OrganssystemsOrgansenses).HasColumnName("organssystems_organsenses");
            entity.Property(e => e.OrganssystemsOrgansensesObs)
                .HasMaxLength(255)
                .HasColumnName("organssystems_organsenses_Obs");
            entity.Property(e => e.OrganssystemsRespiratory).HasColumnName("organssystems_respiratory");
            entity.Property(e => e.OrganssystemsRespiratoryObs)
                .HasMaxLength(255)
                .HasColumnName("organssystems_respiratory_obs");
            entity.Property(e => e.OrganssystemsSkeletalM).HasColumnName("organssystems_skeletal_m");
            entity.Property(e => e.OrganssystemsSkeletalMObs)
                .HasMaxLength(255)
                .HasColumnName("organssystems_skeletal_m_obs");
            entity.Property(e => e.OrganssystemsUrinary).HasColumnName("organssystems_urinary");
            entity.Property(e => e.OrganssystemsUrinaryObs)
                .HasMaxLength(255)
                .HasColumnName("organssystems_urinary_obs");

            entity.HasOne(d => d.OrganssystemsConsultation).WithMany(p => p.OrgansSystems)
                .HasForeignKey(d => d.OrganssystemsConsultationid)
                .HasConstraintName("FK_organssystems_consultationid");
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.PatientId).HasName("PK__patient__4D5CE4766CB5056D");

            entity.ToTable("patient");

            entity.Property(e => e.PatientId).HasColumnName("patient_id");
            entity.Property(e => e.PatientAddress)
                .HasMaxLength(200)
                .HasColumnName("patient_address");
            entity.Property(e => e.PatientAge).HasColumnName("patient_age");
            entity.Property(e => e.PatientBirthdate).HasColumnName("patient_birthdate");
            entity.Property(e => e.PatientBloodtype).HasColumnName("patient_bloodtype");
            entity.Property(e => e.PatientCellularPhone)
                .HasMaxLength(200)
                .HasColumnName("patient_cellular_phone");
            entity.Property(e => e.PatientCode)
                .HasMaxLength(5)
                .HasColumnName("patient_code");
            entity.Property(e => e.PatientCompany)
                .HasMaxLength(200)
                .HasColumnName("patient_company");
            entity.Property(e => e.PatientCreationdate)
                .HasColumnType("datetime")
                .HasColumnName("patient_creationdate");
            entity.Property(e => e.PatientCreationuser).HasColumnName("patient_creationuser");
            entity.Property(e => e.PatientDocumentnumber)
                .HasMaxLength(14)
                .HasColumnName("patient_documentnumber");
            entity.Property(e => e.PatientDocumenttype).HasColumnName("patient_documenttype");
            entity.Property(e => e.PatientDonor)
                .HasMaxLength(50)
                .HasColumnName("patient_donor");
            entity.Property(e => e.PatientEmail)
                .HasMaxLength(200)
                .HasColumnName("patient_email");
            entity.Property(e => e.PatientFirstname)
                .HasMaxLength(200)
                .HasColumnName("patient_firstname");
            entity.Property(e => e.PatientFirstsurname)
                .HasMaxLength(200)
                .HasColumnName("patient_firstsurname");
            entity.Property(e => e.PatientGender).HasColumnName("patient_gender");
            entity.Property(e => e.PatientHealtInsurance).HasColumnName("patient_healt_insurance");
            entity.Property(e => e.PatientLandlinePhone)
                .HasMaxLength(200)
                .HasColumnName("patient_landline_phone");
            entity.Property(e => e.PatientMaritalstatus).HasColumnName("patient_maritalstatus");
            entity.Property(e => e.PatientMiddlename)
                .HasMaxLength(200)
                .HasColumnName("patient_middlename");
            entity.Property(e => e.PatientModificationdate)
                .HasColumnType("datetime")
                .HasColumnName("patient_modificationdate");
            entity.Property(e => e.PatientModificationuser).HasColumnName("patient_modificationuser");
            entity.Property(e => e.PatientNationality).HasColumnName("patient_nationality");
            entity.Property(e => e.PatientOcupation)
                .HasMaxLength(200)
                .HasColumnName("patient_ocupation");
            entity.Property(e => e.PatientProvince).HasColumnName("patient_province");
            entity.Property(e => e.PatientSecondlastname)
                .HasMaxLength(200)
                .HasColumnName("patient_secondlastname");
            entity.Property(e => e.PatientStatus)
                .HasDefaultValue(1)
                .HasColumnName("patient_status");
            entity.Property(e => e.PatientVocationalTraining).HasColumnName("patient_vocational_training");

            entity.HasOne(d => d.PatientBloodtypeNavigation).WithMany(p => p.PatientPatientBloodtypeNavigations)
                .HasForeignKey(d => d.PatientBloodtype)
                .HasConstraintName("FK_patient_bloodtype");

            entity.HasOne(d => d.PatientCreationuserNavigation).WithMany(p => p.PatientPatientCreationuserNavigations)
                .HasForeignKey(d => d.PatientCreationuser)
                .HasConstraintName("FK_patient_creationuser");

            entity.HasOne(d => d.PatientDocumenttypeNavigation).WithMany(p => p.PatientPatientDocumenttypeNavigations)
                .HasForeignKey(d => d.PatientDocumenttype)
                .HasConstraintName("FK_patient_documenttype");

            entity.HasOne(d => d.PatientGenderNavigation).WithMany(p => p.PatientPatientGenderNavigations)
                .HasForeignKey(d => d.PatientGender)
                .HasConstraintName("FK_patient_gender");

            entity.HasOne(d => d.PatientHealtInsuranceNavigation).WithMany(p => p.PatientPatientHealtInsuranceNavigations)
                .HasForeignKey(d => d.PatientHealtInsurance)
                .HasConstraintName("FK_patient_healt_insurance");

            entity.HasOne(d => d.PatientMaritalstatusNavigation).WithMany(p => p.PatientPatientMaritalstatusNavigations)
                .HasForeignKey(d => d.PatientMaritalstatus)
                .HasConstraintName("FK_patient_maritalstatus");

            entity.HasOne(d => d.PatientModificationuserNavigation).WithMany(p => p.PatientPatientModificationuserNavigations)
                .HasForeignKey(d => d.PatientModificationuser)
                .HasConstraintName("FK_patient_modificationuser");

            entity.HasOne(d => d.PatientNationalityNavigation).WithMany(p => p.Patients)
                .HasForeignKey(d => d.PatientNationality)
                .HasConstraintName("FK_patient_nationality");

            entity.HasOne(d => d.PatientProvinceNavigation).WithMany(p => p.Patients)
                .HasForeignKey(d => d.PatientProvince)
                .HasConstraintName("FK_patient_province");

            entity.HasOne(d => d.PatientVocationalTrainingNavigation).WithMany(p => p.PatientPatientVocationalTrainingNavigations)
                .HasForeignKey(d => d.PatientVocationalTraining)
                .HasConstraintName("FK_patient_vocational_training");
        });

        modelBuilder.Entity<PhysicalExamination>(entity =>
        {
            entity.HasKey(e => e.PhysicalexaminationId).HasName("PK__physical__3846C7A7B8AE3C81");

            entity.ToTable("physical_examination");

            entity.Property(e => e.PhysicalexaminationId).HasColumnName("physicalexamination_id");
            entity.Property(e => e.PhysicalexaminationAbdomen).HasColumnName("physicalexamination_abdomen");
            entity.Property(e => e.PhysicalexaminationAbdomenObs)
                .HasMaxLength(255)
                .HasColumnName("physicalexamination_abdomen_obs");
            entity.Property(e => e.PhysicalexaminationChest).HasColumnName("physicalexamination_chest");
            entity.Property(e => e.PhysicalexaminationChestObs)
                .HasMaxLength(255)
                .HasColumnName("physicalexamination_chest_obs");
            entity.Property(e => e.PhysicalexaminationConsultationid).HasColumnName("physicalexamination_consultationid");
            entity.Property(e => e.PhysicalexaminationHead).HasColumnName("physicalexamination_head");
            entity.Property(e => e.PhysicalexaminationHeadObs)
                .HasMaxLength(255)
                .HasColumnName("physicalexamination_head_obs");
            entity.Property(e => e.PhysicalexaminationLimbs).HasColumnName("physicalexamination_limbs");
            entity.Property(e => e.PhysicalexaminationLimbsObs)
                .HasMaxLength(255)
                .HasColumnName("physicalexamination_limbs_obs");
            entity.Property(e => e.PhysicalexaminationNeck).HasColumnName("physicalexamination_neck");
            entity.Property(e => e.PhysicalexaminationNeckObs)
                .HasMaxLength(255)
                .HasColumnName("physicalexamination_neck_obs");
            entity.Property(e => e.PhysicalexaminationPelvis).HasColumnName("physicalexamination_pelvis");
            entity.Property(e => e.PhysicalexaminationPelvisObs)
                .HasMaxLength(255)
                .HasColumnName("physicalexamination_pelvis_obs");

            entity.HasOne(d => d.PhysicalexaminationConsultation).WithMany(p => p.PhysicalExaminations)
                .HasForeignKey(d => d.PhysicalexaminationConsultationid)
                .HasConstraintName("FK_physicalexamination_consultationid");
        });

        modelBuilder.Entity<Profile>(entity =>
        {
            entity.HasKey(e => e.ProfileId).HasName("PK__profiles__AEBB701F14359BCE");

            entity.ToTable("profiles");

            entity.Property(e => e.ProfileId).HasColumnName("profile_id");
            entity.Property(e => e.ProfileDescription)
                .HasMaxLength(255)
                .HasColumnName("profile_description");
            entity.Property(e => e.ProfileName)
                .HasMaxLength(50)
                .HasColumnName("profile_name");
            entity.Property(e => e.ProfileStatus)
                .HasDefaultValue(1)
                .HasColumnName("profile_status");
        });

        modelBuilder.Entity<Province>(entity =>
        {
            entity.HasKey(e => e.ProvinceId).HasName("PK__province__08DCB60F47592316");

            entity.ToTable("provinces");

            entity.Property(e => e.ProvinceId).HasColumnName("province_id");
            entity.Property(e => e.ProvinceCode)
                .HasMaxLength(4)
                .HasColumnName("province_code");
            entity.Property(e => e.ProvinceCountryid).HasColumnName("province_countryid");
            entity.Property(e => e.ProvinceDemony)
                .HasMaxLength(255)
                .HasColumnName("province_demony");
            entity.Property(e => e.ProvinceIso)
                .HasMaxLength(5)
                .HasColumnName("province_iso");
            entity.Property(e => e.ProvinceName)
                .HasMaxLength(255)
                .HasColumnName("province_name");
            entity.Property(e => e.ProvincePrefix)
                .HasMaxLength(100)
                .HasColumnName("province_prefix");
            entity.Property(e => e.ProvinceStatus)
                .HasDefaultValue(1)
                .HasColumnName("province_status");

            entity.HasOne(d => d.ProvinceCountry).WithMany(p => p.Provinces)
                .HasForeignKey(d => d.ProvinceCountryid)
                .HasConstraintName("FK_province_countryid");
        });

        modelBuilder.Entity<Speciality>(entity =>
        {
            entity.HasKey(e => e.SpecialityId).HasName("PK__speciali__E82ED62067BC73C0");

            entity.ToTable("specialities");

            entity.Property(e => e.SpecialityId).HasColumnName("speciality_id");
            entity.Property(e => e.SpecialityCategory)
                .HasMaxLength(500)
                .HasColumnName("speciality_category");
            entity.Property(e => e.SpecialityName)
                .HasMaxLength(255)
                .HasColumnName("speciality_name");
            entity.Property(e => e.SpecialityStatus)
                .HasDefaultValue(1)
                .HasColumnName("speciality_status");
            entity.Property(e => e.SpecialtyDescription)
                .HasMaxLength(500)
                .HasColumnName("specialty_description");
        });

        modelBuilder.Entity<SurgeriesConsultation>(entity =>
        {
            entity.HasKey(e => e.SurgeriesId).HasName("PK__surgerie__0D3E1F77EFA352D6");

            entity.ToTable("surgeries_consultation");

            entity.Property(e => e.SurgeriesId).HasColumnName("surgeries_id");
            entity.Property(e => e.SurgeriesCatalogid).HasColumnName("surgeries_catalogid");
            entity.Property(e => e.SurgeriesConsultationid).HasColumnName("surgeries_consultationid");
            entity.Property(e => e.SurgeriesCreationdate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("surgeries_creationdate");
            entity.Property(e => e.SurgeriesObservation)
                .HasMaxLength(255)
                .HasColumnName("surgeries_observation");
            entity.Property(e => e.SurgeriesStatus)
                .HasDefaultValue(1)
                .HasColumnName("surgeries_status");

            entity.HasOne(d => d.SurgeriesCatalog).WithMany(p => p.SurgeriesConsultations)
                .HasForeignKey(d => d.SurgeriesCatalogid)
                .HasConstraintName("FK_surgeries_catalogid");

            entity.HasOne(d => d.SurgeriesConsultationNavigation).WithMany(p => p.SurgeriesConsultations)
                .HasForeignKey(d => d.SurgeriesConsultationid)
                .HasConstraintName("FK_surgeries_consultationid");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UsersId).HasName("PK__users__EAA7D14B37748633");

            entity.ToTable("users");

            entity.Property(e => e.UsersId).HasColumnName("users_id");
            entity.Property(e => e.UserEstablishmentId).HasColumnName("user_establishment_id");
            entity.Property(e => e.UsersAddress)
                .HasMaxLength(255)
                .HasColumnName("users_address");
            entity.Property(e => e.UsersCountryid).HasColumnName("users_countryid");
            entity.Property(e => e.UsersCreationdate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("users_creationdate");
            entity.Property(e => e.UsersDescription).HasColumnName("users_description");
            entity.Property(e => e.UsersDocumentNumber)
                .HasMaxLength(14)
                .HasColumnName("users_document_number");
            entity.Property(e => e.UsersEmail)
                .HasMaxLength(255)
                .HasColumnName("users_email");
            entity.Property(e => e.UsersLogin)
                .HasMaxLength(255)
                .HasColumnName("users_login");
            entity.Property(e => e.UsersModificationdate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("users_modificationdate");
            entity.Property(e => e.UsersNames)
                .HasMaxLength(255)
                .HasColumnName("users_names");
            entity.Property(e => e.UsersPassword)
                .HasMaxLength(255)
                .HasColumnName("users_password");
            entity.Property(e => e.UsersPhone)
                .HasMaxLength(255)
                .HasColumnName("users_phone");
            entity.Property(e => e.UsersProfileid).HasColumnName("users_profileid");
            entity.Property(e => e.UsersProfilephoto).HasColumnName("users_profilephoto");
            entity.Property(e => e.UsersProfilephoto64).HasColumnName("users_profilephoto64");
            entity.Property(e => e.UsersSenecytcode)
                .HasMaxLength(255)
                .HasColumnName("users_senecytcode");
            entity.Property(e => e.UsersSpecialityid).HasColumnName("users_specialityid");
            entity.Property(e => e.UsersStatus)
                .HasDefaultValue(1)
                .HasColumnName("users_status");
            entity.Property(e => e.UsersSurcenames)
                .HasMaxLength(255)
                .HasColumnName("users_surcenames");
            entity.Property(e => e.UsersVatpercentageid).HasColumnName("users_vatpercentageid");
            entity.Property(e => e.UsersXkeytaxo).HasColumnName("users_xkeytaxo");
            entity.Property(e => e.UsersXpasstaxo).HasColumnName("users_xpasstaxo");

            entity.HasOne(d => d.UserEstablishment).WithMany(p => p.Users)
                .HasForeignKey(d => d.UserEstablishmentId)
                .HasConstraintName("FK_users_establishment_id");

            entity.HasOne(d => d.UsersCountry).WithMany(p => p.Users)
                .HasForeignKey(d => d.UsersCountryid)
                .HasConstraintName("FK_users_countryid");

            entity.HasOne(d => d.UsersProfile).WithMany(p => p.Users)
                .HasForeignKey(d => d.UsersProfileid)
                .HasConstraintName("FK_users_profileid");

            entity.HasOne(d => d.UsersSpeciality).WithMany(p => p.Users)
                .HasForeignKey(d => d.UsersSpecialityid)
                .HasConstraintName("FK_users_specialityid");

            entity.HasOne(d => d.UsersVatpercentage).WithMany(p => p.Users)
                .HasForeignKey(d => d.UsersVatpercentageid)
                .HasConstraintName("FK_users_vatpercentageid");
        });

        modelBuilder.Entity<UserSchedule>(entity =>
        {
            entity.HasKey(e => e.SchudelsId).HasName("PK__user_sch__0D64968B8934C57B");

            entity.ToTable("user_schedules");

            entity.Property(e => e.SchudelsId).HasColumnName("schudels_id");
            entity.Property(e => e.AppointmentInterval).HasColumnName("appointment_interval");
            entity.Property(e => e.CreateAt)
                .HasColumnType("datetime")
                .HasColumnName("create_at");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.SchudelsStatus)
                .HasDefaultValue(1)
                .HasColumnName("schudels_status");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
            entity.Property(e => e.UpdateAt)
                .HasColumnType("datetime")
                .HasColumnName("update_at");
            entity.Property(e => e.UsersId).HasColumnName("users_id");
        });

        modelBuilder.Entity<UserScheduleDay>(entity =>
        {
            entity.HasKey(e => e.ScheduleDayId).HasName("PK__user_sch__A48D6CBBDEF97F03");

            entity.ToTable("user_schedule_days");

            entity.Property(e => e.ScheduleDayId).HasColumnName("schedule_day_id");
            entity.Property(e => e.ScheduleId).HasColumnName("schedule_id");
            entity.Property(e => e.WorkingDay)
                .HasMaxLength(20)
                .HasColumnName("working_day");

            entity.HasOne(d => d.Schedule).WithMany(p => p.UserScheduleDays)
                .HasForeignKey(d => d.ScheduleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_user_schedule_days_user_schedules");
        });

        modelBuilder.Entity<VatBilling>(entity =>
        {
            entity.HasKey(e => e.VatbillingId).HasName("PK__vat_bill__B6D1E35EF0427F09");

            entity.ToTable("vat_billing");

            entity.Property(e => e.VatbillingId).HasColumnName("vatbilling_id");
            entity.Property(e => e.VatbillingCode)
                .HasMaxLength(4)
                .HasColumnName("vatbilling_code");
            entity.Property(e => e.VatbillingPercentage)
                .HasMaxLength(255)
                .HasColumnName("vatbilling_percentage");
            entity.Property(e => e.VatbillingRate)
                .HasMaxLength(5)
                .HasColumnName("vatbilling_rate");
            entity.Property(e => e.VatbillingStatus)
                .HasDefaultValue(1)
                .HasColumnName("vatbilling_status");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

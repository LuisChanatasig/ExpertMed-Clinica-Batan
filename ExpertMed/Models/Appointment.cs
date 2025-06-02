using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ExpertMed.Models
{
    public partial class Appointment
    {
        public int AppointmentId { get; set; }

        public DateTime? AppointmentCreatedate { get; set; }

        public DateTime? AppointmentModifydate { get; set; }

        public int? AppointmentCreateuser { get; set; }

        public int? AppointmentModifyuser { get; set; }

        public DateTime AppointmentDate { get; set; }


        [JsonConverter(typeof(JsonTimeOnlyConverter))]
        public TimeOnly AppointmentHour { get; set; }


        public int? AppointmentPatientid { get; set; }

        public int? AppointmentConsultationid { get; set; }

        public int? AppointmentStatus { get; set; }
        [NotMapped]
        public int? AppointmentMedicalofficeid { get; set; }
        [NotMapped]
        public string? MedicalOfficeName { get; set; }



        // Propiedades añadidas para los nombres completos
        [NotMapped]
        public string? PatientName { get; set; }  // Nombre completo del paciente
        [NotMapped]
        public string? PatientInsuranceName { get; set; }  // Nombre completo del paciente
        [NotMapped]
        public string? DoctorName { get; set; }   // Nombre completo del doctor
        [NotMapped]
        public string? DoctorName2 { get; set; }   // Nombre completo del doctor
        [NotMapped]
        public int DoctorUserId { get; set; }   // Nombre completo del doctor


        // Relacionados con el paciente, doctor y usuario
        public virtual Consultation? AppointmentConsultation { get; set; }

        public virtual User? AppointmentCreateuserNavigation { get; set; }

        public virtual User? AppointmentModifyuserNavigation { get; set; }

        public virtual Patient? AppointmentPatient { get; set; }

        public virtual ICollection<AssistantDoctorAppointment> AssistantDoctorAppointments { get; set; } = new List<AssistantDoctorAppointment>();

        public virtual ICollection<Billing> Billings { get; set; } = new List<Billing>();
    }

    public class JsonTimeOnlyConverter : JsonConverter<TimeOnly>
    {
        private const string TimeFormat = "HH:mm";

        public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return TimeOnly.ParseExact(reader.GetString(), TimeFormat);
        }

        public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(TimeFormat));
        }
    }

}
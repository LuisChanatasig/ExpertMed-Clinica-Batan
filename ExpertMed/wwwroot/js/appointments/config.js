// config.js - Configuración centralizada
const AppConfig = {
    // IDs de perfil
    PROFILE_ID: typeof perfilId !== 'undefined' ? perfilId : null,
    USER_ID: typeof usuarioId !== 'undefined' ? usuarioId : null,

    // Endpoints del servidor (deberás reemplazar con @Url.Action en el archivo real)
    ENDPOINTS: {
        APPOINTMENT_GET_BY_ID: '/Appointment/AppointmentGetById',
        APPOINTMENT_LIST: '/Appointment/AppointmentList',
        MODIFY_APPOINTMENT: '/Appointment/ModifyAppointment',
        DESACTIVATE_APPOINTMENT: '/Appointment/DesactivateAppointment',
        GET_AVAILABLE_OFFICES: '/Appointment/GetAvailableOffices',
        GET_AVAILABLE_HOURS: '/Appointment/GetAvailableHours',
        INSERT_VITAL_SIGNS: '/Appointment/InsertVitalSigns',
        SEND_WHATSAPP_REMINDER: '/Appointment/SendWhatsAppReminder',
        NEW_CONSULTATION: '/Consultation/NewConsultation',
        CONSULTATION_FOLLOWUP: '/Consultation/ConsultationFollowUp',
        CONSULTATION_LIST: '/Consultation/ConsultationList',
        BILLING: '/Billing/Facturacion'
    },

    // Configuración DataTable
    DATATABLE: {
        PAGE_LENGTH: 10,
        ORDER_COLUMN: 3, // Columna de fecha
        ORDER_DIR: 'desc',
        EXPORT_COLUMNS: [2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12],
        DATE_COLUMN_INDEX: 3
    },

    // Estados de cita
    APPOINTMENT_STATUS: {
        ACTIVE: 1,
        CANCELLED: 2,
        FOLLOWUP: 3,
        FINISHED: 4,
        EMERGENCY: 5
    },

    // Estados de pago
    PAYMENT_STATUS: {
        UNPAID: 0,
        PAID: 1
    },

    // Perfiles con acceso especial
    SPECIAL_PROFILES: [1, 3, 4, 8],

    // Validaciones
    BLOOD_PRESSURE_REGEX: /^\d{3}\/\d{2,3}$/
};

// Hacer disponible globalmente
window.AppConfig = AppConfig;
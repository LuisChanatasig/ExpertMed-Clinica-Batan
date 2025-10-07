// appointment-list-main.js - Archivo principal de inicialización
// Este archivo orquesta todos los módulos

$(document).ready(function () {
    console.log('Inicializando sistema de gestión de citas...');

    // Inicializar todos los módulos
    DataTableManager.init();
    AppointmentManager.init();
    ScheduleManager.init();
    VitalSignsManager.init();

    // Inicializar filtro de fechas
    const dateFilter = new DateRangeFilter(
        'appointmentTable',
        AppConfig.DATATABLE.DATE_COLUMN_INDEX
    );

    console.log('Sistema inicializado correctamente');
});

// ========================================
// FUNCIONES GLOBALES (para compatibilidad con HTML)
// ========================================

// Estas funciones son llamadas desde el HTML, por lo que deben estar en el scope global

function filterAppointments(status, isPaidOnly = false, status2 = null) {
    AppointmentManager.filterAppointments(status, isPaidOnly, status2);
}

function openOptionModal(id, status, patientId) {
    AppointmentManager.openOptionsModal(id, status, patientId);
}

function openRescheduleModal() {
    AppointmentManager.openRescheduleModal();
}

function cancelAppointment() {
    AppointmentManager.cancelAppointment();
}

function startConsultation() {
    AppointmentManager.startConsultation();
}

function startFollowupConsultation() {
    AppointmentManager.startFollowupConsultation();
}

function setAppointmentData(id, patientId) {
    AppointmentManager.setAppointmentData(id, patientId);
}

function payAppointment() {
    AppointmentManager.payAppointment();
}

function sendReminderMessage() {
    AppointmentManager.sendReminder();
}

function prepareVitalSigns() {
    VitalSignsManager.prepareForm();
}

function submitVitalSigns() {
    VitalSignsManager.submit();
}

function viewPatientHistory() {
    AppointmentManager.viewPatientHistory();
}
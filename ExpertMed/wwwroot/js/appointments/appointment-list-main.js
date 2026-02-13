/**
 * appointment-list-main.js 
 * Punto de entrada principal y orquestador de módulos para la lista de citas.
 */

$(document).ready(function () {
    'use strict';

    console.log('=== Inicializando Sistema de Gestión de Citas ===');

    /**
     * 1. Verificación de Dependencias
     * Asegura que todos los scripts necesarios (config, managers, etc.) estén cargados.
     */
    function checkDependencies() {
        const dependencies = [
            { name: 'jQuery', check: typeof $ !== 'undefined' },
            { name: 'DataTables', check: typeof $.fn.DataTable !== 'undefined' },
            { name: 'AppConfig', check: typeof AppConfig !== 'undefined' },
            { name: 'DataTableManager', check: typeof DataTableManager !== 'undefined' },
            { name: 'AppointmentManager', check: typeof AppointmentManager !== 'undefined' },
            { name: 'ScheduleManager', check: typeof ScheduleManager !== 'undefined' },
            { name: 'VitalSignsManager', check: typeof VitalSignsManager !== 'undefined' },
            { name: 'DateRangeFilter', check: typeof DateRangeFilter !== 'undefined' }
        ];

        let allLoaded = true;
        dependencies.forEach(dep => {
            if (!dep.check) {
                console.error(`❌ ${dep.name} no está cargado`);
                allLoaded = false;
            } else {
                console.log(`✓ ${dep.name} cargado`);
            }
        });
        return allLoaded;
    }

    /**
     * 2. Inicialización de Módulos
     */
    if (checkDependencies()) {
        // Inicializar Managers
        DataTableManager.init();
        AppointmentManager.init();
        ScheduleManager.init();
        VitalSignsManager.init();

        // Inicializar Filtro de Fechas (Redirección al Servidor)
        // Se asume que AppConfig.DATATABLE.DATE_COLUMN_INDEX es 3
        const dateFilter = new DateRangeFilter(
            'appointmentTable',
            AppConfig.DATATABLE.DATE_COLUMN_INDEX || 3
        );

        setupGlobalEventHandlers();
        console.log('=== Sistema inicializado correctamente ===');
    } else {
        console.error('No se pudo inicializar el sistema debido a dependencias faltantes.');
    }

    /**
     * 3. Manejadores de Eventos Globales (DOM)
     */
    function setupGlobalEventHandlers() {
        // Prevenir envío de formularios con Enter en campos de búsqueda
        $('input[type="search"]').on('keypress', function (e) {
            if (e.which === 13) {
                e.preventDefault();
                return false;
            }
        });

        // Ajustar tabla al redimensionar sidebar
        $('#sidebarToggle').on('click', function () {
            setTimeout(function () {
                if (typeof DataTableManager !== 'undefined' && DataTableManager.isInitialized) {
                    DataTableManager.adjustColumns();
                }
            }, 350);
        });

        // Escuchar cuando DataTable esté listo
        $(document).on('datatable:ready', function (event, api) {
            console.log('✓ DataTable procesado. Filas:', api.rows().count());
        });
    }
});

// ==============================================================================
// FUNCIONES GLOBALES (Scope window)
// Estas funciones se exponen para ser llamadas directamente desde los onclick del HTML
// ==============================================================================

/**
 * Filtra las citas recargando la página y preservando los parámetros de fecha.
 */
window.filterAppointments = function (status, isPaidOnly = false, status2 = null) {
    if (typeof AppConfig === 'undefined' || !AppConfig.ENDPOINTS.APPOINTMENT_LIST) {
        console.error("AppConfig.ENDPOINTS.APPOINTMENT_LIST no definido");
        return;
    }

    // Obtener parámetros actuales de la URL para preservar startDate y endDate
    const params = new URLSearchParams(window.location.search);

    // Seteamos los nuevos estados (nombres coinciden con AppointmentController)
    params.set('appointmentStatus', status);
    params.set('isPaidOnly', isPaidOnly);

    if (status2 !== null && status2 !== undefined) {
        params.set('appointmentStatus2', status2);
    } else {
        params.delete('appointmentStatus2');
    }

    const url = `${AppConfig.ENDPOINTS.APPOINTMENT_LIST}?${params.toString()}`;
    console.log('Navegando a filtro:', url);
    window.location.href = url;
};

/**
 * Puentes (Bridges) hacia AppointmentManager y otros módulos
 */
window.openOptionModal = function (id, status, patientId) {
    AppointmentManager.openOptionsModal(id, status, patientId);
};

window.openRescheduleModal = function () {
    AppointmentManager.openRescheduleModal();
};

window.cancelAppointment = function () {
    AppointmentManager.cancelAppointment();
};

window.startConsultation = function () {
    AppointmentManager.startConsultation();
};

window.startFollowupConsultation = function () {
    AppointmentManager.startFollowupConsultation();
};

window.setAppointmentData = function (id, patientId) {
    AppointmentManager.setAppointmentData(id, patientId);
};

window.payAppointment = function () {
    AppointmentManager.payAppointment();
};

window.registerCreditPayment = function () {
    AppointmentManager.registerCreditPayment();
};

window.sendReminderMessage = function () {
    AppointmentManager.sendReminder();
};

window.prepareVitalSigns = function () {
    VitalSignsManager.prepareForm();
};

window.submitVitalSigns = function () {
    VitalSignsManager.submit();
};

window.viewPatientHistory = function () {
    AppointmentManager.viewPatientHistory();
};
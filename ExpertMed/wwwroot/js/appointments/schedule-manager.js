// schedule-manager.js - Gestión de horarios y consultorios

const ScheduleManager = {
    /**
     * Inicializa el manager
     */
    init() {
        this.bindEvents();
    },

    /**
     * Carga consultorios disponibles
     */
    async loadAvailableOffices() {
        const date = FormHelper.getValue('selectedDate');
        const time = FormHelper.getValue('appointmentTime');
        const doctorUserId = FormHelper.getValue('doctorUserId');

        if (!date || !time) {
            console.log('Fecha u hora no seleccionadas');
            return;
        }

        try {
            const response = await AppointmentAPI.getAvailableOffices(date, time, doctorUserId);
            this.renderOffices(response);
        } catch (error) {
            console.error('Error al cargar consultorios:', error);
        }
    },

    /**
     * Renderiza los consultorios disponibles
     */
    renderOffices(response) {
        const $container = $('#consultoriosContainer').empty();
        const officesList = Array.isArray(response.offices) ? response.offices : [];

        if (response.success && officesList.length > 0) {
            // Autoasignar el primer consultorio
            const firstOffice = officesList[0];
            FormHelper.setValue('AppointmentMedicalofficeid', firstOffice.medicalOfficeId);

            $('#confirmAppointmentBtn').show().prop('disabled', false);

            // Mostrar visualmente
            $container.append(`
                <div class="col">
                    <div class="card consultorio-card selected" data-id="${firstOffice.medicalOfficeId}">
                        <span class="checkmark"><i class="ri-check-line"></i></span>
                        <div class="card-body text-center">
                            <h5>${firstOffice.medicalOfficeName}</h5>
                            <p><i class="mdi mdi-hospital-building"></i> #${firstOffice.medicalOfficeId}</p>
                        </div>
                    </div>
                </div>
            `);
        } else {
            $container.append(
                '<div class="col-12 text-center text-muted">No hay consultorios disponibles.</div>'
            );
        }
    },

    /**
     * Muestra el modal de horas disponibles
     */
    async showAvailableHours() {
        const date = FormHelper.getValue('selectedDate');

        if (!date) {
            ErrorHandler.showAlert('Seleccione una fecha primero');
            return;
        }

        const doctorId = FormHelper.getValue('doctorUserId');

        try {
            const response = await AppointmentAPI.getAvailableHours(date, doctorId);
            this.renderAvailableHours(response);
            ModalManager.show('hoursModalgrid');
        } catch (error) {
            console.error('Error al cargar horas:', error);
        }
    },

    /**
     * Renderiza las horas disponibles
     */
    renderAvailableHours(response) {
        const hoursList = Array.isArray(response.hours) ? response.hours : response;
        const $container = $('#availableHoursContainer').empty();

        if (hoursList.length > 0) {
            hoursList.forEach(hour => {
                $container.append(
                    `<button class="btn btn-outline-primary btn-sm m-1" data-hour="${hour}">${hour}</button>`
                );
            });
        } else {
            $container.append('<p class="text-muted">No hay horas disponibles.</p>');
        }
    },

    /**
     * Maneja la selección de una hora
     */
    async handleHourSelection(selectedHour) {
        const date = FormHelper.getValue('selectedDate');
        const doctorId = FormHelper.getValue('doctorUserId');

        // Cerrar modal de horas
        ModalManager.hide('hoursModalgrid');

        try {
            const response = await AppointmentAPI.getAvailableOffices(date, selectedHour, doctorId);

            if (!response.success || !Array.isArray(response.offices) || response.offices.length === 0) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Sin consultorio',
                    text: 'No hay consultorios disponibles para esa hora'
                });
                return;
            }

            const firstOffice = response.offices[0];

            // Pre-rellenar campos del modal
            FormHelper.setValues({
                appointmentTime: selectedHour,
                selectedDate: date,
                AppointmentMedicalofficeid: firstOffice.medicalOfficeId
            });

            // Actualizar doctor
            $('#doctorUserId').val(doctorId).trigger('change');

            // Mostrar contenedor de consultorios
            $('#consultoriosContainer').closest('.col-xxl-12').removeAttr('hidden');

            // Limpiar campos adicionales
            FormHelper.clearFields(
                'appointmentReason',
                'appointmentInsuranceAuthCode'
            );
            $('#appointmentInsuranceCompanyId').val('').trigger('change');

            // Activar botón y abrir modal
            $('#confirmAppointmentBtn').prop('disabled', false).show();
            ModalManager.show('appointmentModalgrid');

        } catch (error) {
            console.error('Error al seleccionar hora:', error);
        }
    },

    /**
     * Vincula eventos del módulo
     */
    bindEvents() {
        // Botón ver horas
        $('#viewHoursButton').on('click', () => {
            this.showAvailableHours();
        });

        // Click en hora disponible
        $('#availableHoursContainer').on('click', 'button', function () {
            const selectedHour = $(this).data('hour');
            ScheduleManager.handleHourSelection(selectedHour);
        });

        // Cambio en fecha/hora -> recargar consultorios
        $('#selectedDate, #appointmentTime').on('change', () => {
            this.loadAvailableOffices();
        });
    }
};

// Hacer disponible globalmente
window.ScheduleManager = ScheduleManager;
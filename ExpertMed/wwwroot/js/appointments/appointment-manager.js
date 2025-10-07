// appointment-manager.js - Lógica principal de gestión de citas

const AppointmentManager = {
    currentAppointmentId: null,
    currentPatientId: null,

    /**
     * Inicializa el manager
     */
    init() {
        this.bindGlobalEvents();
    },

    /**
     * Abre el modal de opciones de cita
     */
    async openOptionsModal(id, status, patientId) {
        try {
            const data = await AppointmentAPI.getById(id);
            console.log('Datos de cita:', data);

            // Guardar IDs actuales
            this.currentAppointmentId = data.appointmentId;
            this.currentPatientId = data.patientId;

            // Rellenar campos ocultos
            FormHelper.setValues({
                appointmentIdInput: data.appointmentId,
                appointmentStatus: data.status,
                appointmentPatientId: data.patientId,
                appointmentPaymentStatus: data.paymentStatus
            });

            // Mostrar/ocultar botones según el estado
            this.updateModalButtons(data);

            // Abrir modal
            ModalManager.show('optionModal');

        } catch (error) {
            console.error('Error al abrir modal de opciones:', error);
        }
    },

    /**
     * Actualiza la visibilidad de botones según el estado de la cita
     */
    updateModalButtons(data) {
        const { status, hasConsultation, paymentStatus } = data;

        // Ocultar inicialmente todos los botones de consulta
        $('#startConsultCol, #startFollowupCol').hide();

        // Mostrar "Empezar Consulta" si corresponde
        if ([0, 1, 5].includes(status) && !hasConsultation) {
            $('#startConsultCol').show();
        }

        // Mostrar "Seguimiento" si corresponde
        if (status === 3 && !hasConsultation) {
            $('#startFollowupCol').show();
        }

        // Mostrar botones comunes
        $('#rescheduleCol, #cancelCol, #reminderCol, #vitalSignsCol').show();

        // Mostrar botón de pago solo si no está pagada
        if (paymentStatus === 0) {
            $('#payCol').show();
        } else {
            $('#payCol').hide();
        }
    },

    /**
     * Abre el modal de reprogramación
     */
    async openRescheduleModal() {
        try {
            const id = FormHelper.getRequiredValue('appointmentIdInput');
            const data = await AppointmentAPI.getById(id);

            console.log('Datos para reprogramar:', data);

            // Rellenar formulario
            this.populateRescheduleForm(data);

            // Habilitar botón y abrir modal
            $('#confirmAppointmentBtn').prop('disabled', false).show();
            ModalManager.show('appointmentModalgrid');

        } catch (error) {
            if (error.message.includes('requerido')) {
                ErrorHandler.showAlert('ID de cita no encontrado');
            }
        }
    },

    /**
     * Rellena el formulario de reprogramación
     */
    populateRescheduleForm(data) {
        FormHelper.setValues({
            appointment: data.appointmentId,
            patientId: data.patientId,
            selectedDate: data.date,
            appointmentTime: data.time,
            appointmentReason: data.appointmentReason
        });

        // Médico (con trigger para actualizar select)
        if (data.doctorUserId) {
            $('#doctorUserId').val(data.doctorUserId).trigger('change');
        }

        // Aseguradora
        $('#appointmentInsuranceCompanyId')
            .val(data.appointmentInsuranceCompanyId || '')
            .trigger('change');

        // Consultorio
        if (data.medicalOfficeId) {
            $(`#office-${data.medicalOfficeId}`).prop('checked', true);
        }
    },

    /**
     * Confirma la modificación de una cita
     */
    async confirmAppointment() {
        try {
            const payload = this.buildAppointmentPayload();
            console.log('Enviando payload:', payload);

            const response = await AppointmentAPI.modify(payload);

            if (response.success) {
                await Swal.fire({
                    icon: 'success',
                    title: '¡Listo!',
                    text: response.message
                });

                window.location.href = AppConfig.ENDPOINTS.APPOINTMENT_LIST;
            } else {
                ErrorHandler.showAlert(response.message || 'No se pudo reprogramar la cita');
            }

        } catch (error) {
            console.error('Error al confirmar cita:', error);
        }
    },

    /**
     * Construye el payload para modificar una cita
     */
    buildAppointmentPayload() {
        return {
            AppointmentId: FormHelper.getValue('appointment'),
            AppointmentPatientid: FormHelper.getValue('patientId'),
            AppointmentDate: FormHelper.getValue('selectedDate'),
            AppointmentHour: FormHelper.getValue('appointmentTime'),
            AppointmentMedicalofficeid: FormHelper.getValue('AppointmentMedicalofficeid'),
            AppointmentStatus: FormHelper.getValue('appointmentStatus'),
            DoctorUserId: FormHelper.getValue('doctorUserId') || null,
            AppointmentInsuranceCompanyId: FormHelper.getValue('appointmentInsuranceCompanyId') || null,
            AppointmentInsuranceAuthCode: FormHelper.getValue('appointmentInsuranceAuthCode') || null,
            AppointmentReason: FormHelper.getValue('appointmentReason') || null
        };
    },

    /**
     * Cancela una cita
     */
    async cancelAppointment() {
        const result = await Swal.fire({
            title: '¿Seguro de cancelar?',
            text: 'Esta acción desactivará la cita',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Sí, cancelar',
            cancelButtonText: 'No',
            confirmButtonColor: '#d33'
        });

        if (!result.isConfirmed) return;

        try {
            const id = FormHelper.getRequiredValue('appointmentIdInput');
            const response = await AppointmentAPI.cancel(id);

            await Swal.fire({
                icon: 'success',
                title: 'Cancelada',
                text: response.message
            });

            window.location.href = AppConfig.ENDPOINTS.APPOINTMENT_LIST;

        } catch (error) {
            console.error('Error al cancelar cita:', error);
        }
    },

    /**
     * Inicia una consulta
     */
    async startConsultation() {
        try {
            const id = FormHelper.getRequiredValue('appointmentIdInput');
            const data = await AppointmentAPI.getById(id);

            if (!data.patientId) {
                ErrorHandler.showAlert('Paciente no encontrado');
                return;
            }

            window.location.href =
                `${AppConfig.ENDPOINTS.NEW_CONSULTATION}?patientId=${data.patientId}`;

        } catch (error) {
            console.error('Error al iniciar consulta:', error);
        }
    },

    /**
     * Inicia una consulta de seguimiento
     */
    startFollowupConsultation() {
        try {
            const patientId = FormHelper.getRequiredValue('appointmentPatientId');
            window.location.href =
                `${AppConfig.ENDPOINTS.CONSULTATION_FOLLOWUP}?patientid=${patientId}`;

        } catch (error) {
            ErrorHandler.showAlert('ID de paciente no encontrado');
        }
    },

    /**
     * Proceder al pago de una cita
     */
    payAppointment() {
        try {
            const id = FormHelper.getRequiredValue('appointmentIdInput');
            const patientId = FormHelper.getRequiredValue('appointmentPatientId');

            window.location.href =
                `${AppConfig.ENDPOINTS.BILLING}?appointmentId=${id}&patientId=${patientId}`;

        } catch (error) {
            ErrorHandler.showAlert('Faltan datos para procesar el pago');
        }
    },

    /**
     * Envía recordatorio por WhatsApp
     */
    sendReminder() {
        try {
            const id = FormHelper.getRequiredValue('appointmentIdInput');
            window.location.href =
                `${AppConfig.ENDPOINTS.SEND_WHATSAPP_REMINDER}?appointmentId=${id}&userProfile=${AppConfig.PROFILE_ID}`;

        } catch (error) {
            ErrorHandler.showAlert('ID de cita no encontrado');
        }
    },

    /**
     * Ver historial del paciente
     */
    viewPatientHistory() {
        try {
            const patientId = FormHelper.getRequiredValue('appointmentPatientId');

            // Cerrar modal actual
            ModalManager.hide('optionModal');

            window.location.href =
                `${AppConfig.ENDPOINTS.CONSULTATION_LIST}?patientId=${encodeURIComponent(patientId)}`;

        } catch (error) {
            ErrorHandler.showAlert('No se pudo obtener el ID del paciente para ver el historial');
        }
    },

    /**
     * Establece los datos de una cita (para usar desde tabla)
     */
    setAppointmentData(id, patientId) {
        FormHelper.setValues({
            appointmentIdInput: id,
            appointmentPatientId: patientId
        });
        this.currentAppointmentId = id;
        this.currentPatientId = patientId;
    },

    /**
     * Filtra citas por estado
     */
    filterAppointments(status, isPaidOnly = false, status2 = null) {
        let url = `${AppConfig.ENDPOINTS.APPOINTMENT_LIST}?appointmentStatus=${status}&isPaidOnly=${isPaidOnly}`;

        if (status2 !== null) {
            url += `&appointmentStatus2=${status2}`;
        }

        window.location.href = url;
    },

    /**
     * Vincula eventos globales
     */
    bindGlobalEvents() {
        // Limpiar backdrops al cerrar modales
        $('#optionModal').on('hidden.bs.modal', () => {
            ModalManager.cleanBackdrops();
        });

        // Confirmar cita
        $('#confirmAppointmentBtn').on('click', () => {
            this.confirmAppointment();
        });

        // Cambios en fecha/hora -> recargar consultorios
        $('#selectedDate, #appointmentTime').on('change', () => {
            this.loadAvailableOffices();
        });
    }
};

// Hacer disponible globalmente
window.AppointmentManager = AppointmentManager;
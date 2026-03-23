// vital-signs.js - Gestión de signos vitales

const VitalSignsManager = {

    /**
     * Inicializa el manager
     */
    init() {
        this.bindEvents();
    },

    /**
     * Prepara el formulario de signos vitales
     */
    prepareForm() {
        const appointmentId = FormHelper.getValue('appointmentIdInput');
        const patientId = FormHelper.getValue('appointmentPatientId');

        FormHelper.setValues({
            vsAppointmentId: appointmentId,
            vsPatientId: patientId
        });
    },

    /**
     * Convierte a float o retorna null si es NaN/vacío
     */
    _safeFloat(id) {
        const v = parseFloat(FormHelper.getValue(id));
        return isNaN(v) ? null : v;
    },

    /**
     * Convierte a int o retorna null si es NaN/vacío
     */
    _safeInt(id) {
        const v = parseInt(FormHelper.getValue(id));
        return isNaN(v) ? null : v;
    },

    /**
     * Envía los signos vitales al servidor
     */
    async submit() {
        try {
            // Validar presión arterial
            const bloodPressure = FormHelper.getValue('bloodPressure');
            const bpValidation = ValidationHelper.validateBloodPressure(bloodPressure);

            if (!bpValidation.valid) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Presión arterial inválida',
                    text: bpValidation.message
                });
                return;
            }

            // Construcción del payload — campos opcionales usan safeFloat para evitar NaN
            const data = {
                appointmentId: parseInt(FormHelper.getValue('vsAppointmentId')),
                patientId: parseInt(FormHelper.getValue('vsPatientId')),
                temperature: this._safeFloat('temperature'),
                respiratoryRate: this._safeInt('respiratoryRate'),
                bloodPressureAS: bpValidation.systolic,
                bloodPressureDIS: bpValidation.diastolic,
                pulse: FormHelper.getValue('heartRate'),
                weight: FormHelper.getValue('weight'),
                size: FormHelper.getValue('height'),

                // Campos opcionales — llegan como null si están vacíos
                bmi: this._safeFloat('bmi'),
                abdominalPerimeter: this._safeFloat('abdominalPerimeter'),
                capillaryHemoglobin: this._safeFloat('capillaryHemoglobin'),
                capillaryGlucose: this._safeFloat('capillaryGlucose'),
                spo2: this._safeFloat('spo2'),

                createdBy: AppConfig.USER_ID
            };

            console.log('Enviando signos vitales:', data);

            // Enviar al servidor
            const response = await AppointmentAPI.insertVitalSigns(data);

            if (response.success) {
                await Swal.fire({
                    icon: 'success',
                    title: '¡Guardado!',
                    text: response.message
                });

                // Limpiar formulario y cerrar
                FormHelper.resetForm('vitalSignsForm');
                bootstrap.Offcanvas.getInstance($('#vitalSignsCanvas')).hide();
            } else {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: response.message
                });
            }

        } catch (error) {
            console.error('Error al guardar signos vitales:', error);
            Swal.fire({
                icon: 'error',
                title: 'Error inesperado',
                text: 'Ocurrió un error al guardar los signos vitales. Intente nuevamente.'
            });
        }
    },

    /**
     * Vincula eventos del módulo
     */
    bindEvents() {
        // Formato automático de presión arterial
        $('#bloodPressure').on('input', function() {
            this.value = ValidationHelper.formatBloodPressure(this.value);
        });

        // Cálculo IMC en vivo (kg / m²)
        $('#weight, #height').on('input', function() {
            const rawPeso = $('#weight').val();
            const rawTalla = $('#height').val();

            const peso = parseFloat(rawPeso.replace(',', '.'));
            const talla = parseFloat(rawTalla.replace(',', '.'));

            if (!isNaN(peso) && !isNaN(talla) && talla > 0) {
                const imc = peso / (talla * talla);
                $('#bmi').val(imc.toFixed(1));
            } else {
                $('#bmi').val('');
            }
        });
    }
};

// Hacer disponible globalmente
window.VitalSignsManager = VitalSignsManager;
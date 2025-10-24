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

            // 🔹 Construcción del payload
            const data = {
                appointmentId: parseInt(FormHelper.getValue('vsAppointmentId')),
                patientId: parseInt(FormHelper.getValue('vsPatientId')),
                temperature: parseFloat(FormHelper.getValue('temperature')),
                respiratoryRate: parseInt(FormHelper.getValue('respiratoryRate')),
                bloodPressureAS: bpValidation.systolic,
                bloodPressureDIS: bpValidation.diastolic,
                pulse: FormHelper.getValue('heartRate'),
                weight: FormHelper.getValue('weight'),
                size: FormHelper.getValue('height'),

                // *** ✅ Nuevos campos ***
                bmi: parseFloat(FormHelper.getValue('bmi')),
                abdominalPerimeter: parseFloat(FormHelper.getValue('abdominalPerimeter')),
                capillaryHemoglobin: parseFloat(FormHelper.getValue('capillaryHemoglobin')),
                capillaryGlucose: parseFloat(FormHelper.getValue('capillaryGlucose')),
                spo2: parseFloat(FormHelper.getValue('spo2')),

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
        }
    },

    /**
     * Vincula eventos del módulo
     */
    bindEvents() {
        // Formato automático de presión arterial
        $('#bloodPressure').on('input', function () {
            this.value = ValidationHelper.formatBloodPressure(this.value);
        });

        // ✅ Calcula IMC en vivo
        $('#weight, #height').on('input', function () {
            const peso = parseFloat($('#weight').val());
            const talla = parseFloat($('#height').val()) / 100;

            if (!isNaN(peso) && !isNaN(talla) && talla > 0) {
                const imc = peso / (talla * talla);
                $('#bmi').val(imc.toFixed(2));
            } else {
                $('#bmi').val('');
            }
        });
    }
};

// Hacer disponible globalmente
window.VitalSignsManager = VitalSignsManager;

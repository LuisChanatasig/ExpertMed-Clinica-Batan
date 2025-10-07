// helpers.js - Funciones de utilidad reutilizables

// ==================== MANEJO DE ERRORES ====================
const ErrorHandler = {
    handle(error, context = '') {
        console.error(`Error en ${context}:`, error);

        const message = error.responseJSON?.message ||
            error.message ||
            'Ocurrió un error inesperado';

        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: message,
            footer: context ? `Contexto: ${context}` : ''
        });
    },

    showAlert(message) {
        Swal.fire({
            icon: 'warning',
            title: 'Atención',
            text: message
        });
    }
};

// ==================== MANEJO DE MODALES ====================
const ModalManager = {
    show(modalId) {
        const $modal = $(`#${modalId}`);
        if ($modal.length) {
            const modal = new bootstrap.Modal($modal);
            modal.show();
            return modal;
        }
        console.error(`Modal #${modalId} no encontrado`);
        return null;
    },

    hide(modalId) {
        const modalEl = document.getElementById(modalId);
        if (modalEl) {
            const modal = bootstrap.Modal.getInstance(modalEl);
            if (modal) {
                modal.hide();
            }
        }
    },

    cleanBackdrops() {
        $('.modal-backdrop').remove();
        document.body.classList.remove('modal-open');
        document.body.style.paddingRight = '';
    }
};

// ==================== MANEJO DE FORMULARIOS ====================
const FormHelper = {
    getValue(inputId) {
        return $(`#${inputId}`).val();
    },

    setValue(inputId, value) {
        $(`#${inputId}`).val(value);
    },

    getRequiredValue(inputId) {
        const value = this.getValue(inputId);
        if (!value) {
            throw new Error(`El campo ${inputId} es requerido`);
        }
        return value;
    },

    getValues(...inputIds) {
        return inputIds.map(id => this.getValue(id));
    },

    setValues(dataMap) {
        Object.entries(dataMap).forEach(([key, value]) => {
            this.setValue(key, value);
        });
    },

    resetForm(formId) {
        $(`#${formId}`)[0]?.reset();
    },

    clearFields(...inputIds) {
        inputIds.forEach(id => this.setValue(id, ''));
    }
};

// ==================== VALIDACIONES ====================
const ValidationHelper = {
    requireFields(...values) {
        const missing = values.filter(v => !v);
        if (missing.length > 0) {
            throw new Error('Campos requeridos faltantes');
        }
    },

    validateBloodPressure(bp) {
        const cleaned = bp.replace(/[^\d/]/g, '');
        const parts = cleaned.split('/');

        if (parts.length !== 2 ||
            parts[0].length !== 3 ||
            parts[1].length < 2 ||
            parts[1].length > 3) {
            return {
                valid: false,
                message: 'Formato presión debe ser 3 dígitos/2-3 dígitos (ej: 120/80)'
            };
        }

        return {
            valid: true,
            systolic: parts[0],
            diastolic: parts[1]
        };
    },

    formatBloodPressure(value) {
        let v = value.replace(/[^\d]/g, '').slice(0, 6);
        if (v.length > 3) {
            v = v.slice(0, 3) + '/' + v.slice(3);
        }
        return v;
    }
};

// ==================== UTILIDADES DE FECHA ====================
const DateHelper = {
    parseDMY(dateStr) {
        if (!dateStr) return null;
        const parts = dateStr.split('/');
        if (parts.length !== 3) return null;

        const [dd, mm, yyyy] = parts.map(p => parseInt(p, 10));
        if (isNaN(dd) || isNaN(mm) || isNaN(yyyy)) return null;

        return new Date(yyyy, mm - 1, dd, 0, 0, 0, 0);
    },

    atStartOfDay(date) {
        const d = new Date(date);
        d.setHours(0, 0, 0, 0);
        return d;
    },

    atEndOfDay(date) {
        const d = new Date(date);
        d.setHours(23, 59, 59, 999);
        return d;
    },

    getWeekRange(today = new Date()) {
        const d = new Date(today);
        d.setHours(0, 0, 0, 0);

        const day = d.getDay();
        const diffToMonday = (day + 6) % 7;

        const monday = new Date(d);
        monday.setDate(d.getDate() - diffToMonday);

        const sunday = new Date(monday);
        sunday.setDate(monday.getDate() + 6);

        return {
            start: this.atStartOfDay(monday),
            end: this.atEndOfDay(sunday)
        };
    },

    getMonthRange(today = new Date()) {
        const y = today.getFullYear();
        const m = today.getMonth();
        const first = new Date(y, m, 1);
        const last = new Date(y, m + 1, 0);

        return {
            start: this.atStartOfDay(first),
            end: this.atEndOfDay(last)
        };
    },

    getYearRange(today = new Date()) {
        const y = today.getFullYear();
        const first = new Date(y, 0, 1);
        const last = new Date(y, 11, 31);

        return {
            start: this.atStartOfDay(first),
            end: this.atEndOfDay(last)
        };
    }
};

// ==================== ESTADOS DE CARGA ====================
const LoadingHelper = {
    show(button, text = 'Cargando...') {
        const $btn = $(button);
        $btn.data('original-html', $btn.html());
        $btn.prop('disabled', true)
            .html(`<span class="spinner-border spinner-border-sm me-2"></span>${text}`);
    },

    hide(button) {
        const $btn = $(button);
        const originalHtml = $btn.data('original-html');
        if (originalHtml) {
            $btn.prop('disabled', false).html(originalHtml);
        }
    }
};

// Hacer disponibles globalmente
window.ErrorHandler = ErrorHandler;
window.ModalManager = ModalManager;
window.FormHelper = FormHelper;
window.ValidationHelper = ValidationHelper;
window.DateHelper = DateHelper;
window.LoadingHelper = LoadingHelper;
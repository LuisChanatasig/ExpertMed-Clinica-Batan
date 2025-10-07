// date-filter.js - Filtros de fecha para DataTable

class DateRangeFilter {
    constructor(tableId, dateColumnIndex) {
        this.tableId = tableId;
        this.dateColumnIndex = dateColumnIndex;
        this.start = null;
        this.end = null;
        this.table = null;

        this.init();
    }

    init() {
        // Esperar a que DataTable esté inicializada
        $(document).ready(() => {
            this.table = $(`#${this.tableId}`).DataTable();
            this.setupFilter();
            this.bindEvents();

            // Aplicar filtro inicial (hoy)
            const today = new Date();
            this.setRange(today, today, 'Hoy');
        });
    }

    setupFilter() {
        const self = this;

        $.fn.dataTable.ext.search.push(function (settings, data) {
            if (settings.nTable.id !== self.tableId) {
                return true;
            }

            const dateStr = data[self.dateColumnIndex];
            const date = DateHelper.parseDMY(dateStr);

            if (!date) return true;

            if (self.start && date < self.start) return false;
            if (self.end && date > self.end) return false;

            return true;
        });
    }

    setRange(start, end, labelText) {
        this.start = start ? DateHelper.atStartOfDay(start) : null;
        this.end = end ? DateHelper.atEndOfDay(end) : null;

        this.updateLabel(labelText);

        if (this.table) {
            this.table.draw();
        }
    }

    updateLabel(text) {
        const $label = $('#lblRangoActivo');

        if (text) {
            $label.text(text).show();
        } else {
            $label.hide().text('');
        }
    }

    clearRange() {
        this.setRange(null, null, '');
        FormHelper.clearFields('fDesdeFecha', 'fHastaFecha');
    }

    bindEvents() {
        // Botón Hoy
        $('#btnHoy').on('click', () => {
            const today = new Date();
            this.setRange(today, today, 'Hoy');
        });

        // Botón Esta Semana
        $('#btnSemana').on('click', () => {
            const { start, end } = DateHelper.getWeekRange();
            this.setRange(start, end, 'Esta semana');
        });

        // Botón Este Mes
        $('#btnMes').on('click', () => {
            const { start, end } = DateHelper.getMonthRange();
            this.setRange(start, end, 'Este mes');
        });

        // Botón Este Año
        $('#btnAnio').on('click', () => {
            const { start, end } = DateHelper.getYearRange();
            this.setRange(start, end, 'Este año');
        });

        // Aplicar rango personalizado
        $('#btnAplicarRango').on('click', () => {
            this.applyCustomRange();
        });

        // Limpiar rango
        $('#btnLimpiarRango').on('click', () => {
            this.clearRange();
        });
    }

    applyCustomRange() {
        const desdeVal = FormHelper.getValue('fDesdeFecha');
        const hastaVal = FormHelper.getValue('fHastaFecha');

        let start = null, end = null;

        if (desdeVal) {
            const [y, m, d] = desdeVal.split('-').map(n => parseInt(n, 10));
            start = new Date(y, m - 1, d);
        }

        if (hastaVal) {
            const [y2, m2, d2] = hastaVal.split('-').map(n => parseInt(n, 10));
            end = new Date(y2, m2 - 1, d2);
        }

        // Validación
        if (start && end && end < start) {
            Swal.fire({
                icon: 'warning',
                title: 'Rango inválido',
                text: 'La fecha "Hasta" debe ser mayor o igual a la fecha "Desde"'
            });
            return;
        }

        const label = `Del ${desdeVal || '…'} al ${hastaVal || '…'}`;
        this.setRange(start, end, label);
    }
}

// Hacer disponible globalmente
window.DateRangeFilter = DateRangeFilter;
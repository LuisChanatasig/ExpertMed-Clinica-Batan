/**
 * date-filter.js - Gestión de filtrado por rangos de fecha vía servidor
 * Diseñado para ExpertMed-Clinica-Batan
 */
class DateRangeFilter {
    constructor() {
        this.init();
    }

    init() {
        $(document).ready(() => {
            this.bindEvents();
            this.syncInputsFromUrl();
        });
    }

    /**
     * Redirige a la URL actual añadiendo o actualizando los parámetros de fecha
     * Preserva los estados de cita (appointmentStatus, isPaidOnly, etc.)
     */
    applyDateRedirect(startDate, endDate) {
        if (typeof AppConfig === 'undefined' || !AppConfig.ENDPOINTS.APPOINTMENT_LIST) {
            console.error("AppConfig no definido");
            return;
        }

        const params = new URLSearchParams(window.location.search);

        if (startDate) params.set('startDate', startDate);
        else params.delete('startDate');

        if (endDate) params.set('endDate', endDate);
        else params.delete('endDate');

        // Construir URL final
        const url = `${AppConfig.ENDPOINTS.APPOINTMENT_LIST}?${params.toString()}`;

        console.log('Filtrando por fecha:', url);
        window.location.href = url;
    }

    /**
     * Formatea objetos Date a string YYYY-MM-DD para el input y el controlador
     */
    formatDate(date) {
        const d = new Date(date);
        let month = '' + (d.getMonth() + 1);
        let day = '' + d.getDate();
        const year = d.getFullYear();

        if (month.length < 2) month = '0' + month;
        if (day.length < 2) day = '0' + day;

        return [year, month, day].join('-');
    }

    bindEvents() {
        // Botón Hoy
        $('#btnHoy').on('click', () => {
            const today = this.formatDate(new Date());
            this.applyDateRedirect(today, today);
        });

        // Botón Esta Semana (Lunes a Domingo)
        $('#btnSemana').on('click', () => {
            const curr = new Date();
            const first = curr.getDate() - curr.getDay() + (curr.getDay() === 0 ? -6 : 1);
            const last = first + 6;

            const firstday = this.formatDate(new Date(curr.setDate(first)));
            const lastday = this.formatDate(new Date(curr.setDate(last)));

            this.applyDateRedirect(firstday, lastday);
        });

        // Botón Este Mes
        $('#btnMes').on('click', () => {
            const date = new Date();
            const firstDay = this.formatDate(new Date(date.getFullYear(), date.getMonth(), 1));
            const lastDay = this.formatDate(new Date(date.getFullYear(), date.getMonth() + 1, 0));

            this.applyDateRedirect(firstDay, lastDay);
        });

        // Botón Este Año
        $('#btnAnio').on('click', () => {
            const year = new Date().getFullYear();
            this.applyDateRedirect(`${year}-01-01`, `${year}-12-31`);
        });

        // Aplicar rango personalizado
        $('#btnAplicarRango').on('click', () => {
            const desde = $('#fDesdeFecha').val();
            const hasta = $('#fHastaFecha').val();

            if (!desde || !hasta) {
                Swal.fire({
                    icon: 'info',
                    title: 'Atención',
                    text: 'Debe seleccionar ambas fechas para aplicar el filtro.'
                });
                return;
            }

            if (new Date(hasta) < new Date(desde)) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Rango inválido',
                    text: 'La fecha "Hasta" no puede ser menor a la fecha "Desde".'
                });
                return;
            }

            this.applyDateRedirect(desde, hasta);
        });

        // Limpiar rango (Vuelve al default del SP: Hoy)
        $('#btnLimpiarRango').on('click', () => {
            this.applyDateRedirect(null, null);
        });
    }

    /**
     * Mantiene los inputs de fecha visualmente sincronizados con la URL
     */
    syncInputsFromUrl() {
        const params = new URLSearchParams(window.location.search);
        const start = params.get('startDate');
        const end = params.get('endDate');

        if (start) $('#fDesdeFecha').val(start);
        if (end) $('#fHastaFecha').val(end);

        if (start && end) {
            $('#lblRangoActivo').text(`Filtrando: ${start} al ${end}`).show();
        }
    }
}

// Inicializar globalmente
window.dateRangeFilter = new DateRangeFilter();
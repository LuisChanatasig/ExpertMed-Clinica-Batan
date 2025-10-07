// datatable-init.js - Inicialización y configuración de DataTable

const DataTableManager = {
    table: null,

    /**
     * Inicializa el DataTable
     */
    init() {
        $(document).ready(() => {
            this.table = $('#appointmentTable').DataTable({
                responsive: true,
                pageLength: AppConfig.DATATABLE.PAGE_LENGTH,
                order: [[AppConfig.DATATABLE.ORDER_COLUMN, AppConfig.DATATABLE.ORDER_DIR]],
                dom: 'Bfrtip',
                buttons: this.getButtons(),
                language: {
                    url: '//cdn.datatables.net/plug-ins/1.11.5/i18n/es-ES.json'
                },
                stateSave: true
            });
        });
    },

    /**
     * Configuración de botones de exportación
     */
    getButtons() {
        const exportColumns = AppConfig.DATATABLE.EXPORT_COLUMNS;

        return [
            {
                extend: 'excelHtml5',
                text: '<i class="mdi mdi-file-excel-outline"></i> Excel',
                titleAttr: 'Exportar a Excel',
                className: 'btn btn-sm btn-success',
                exportOptions: {
                    columns: exportColumns
                }
            },
            {
                extend: 'pdfHtml5',
                text: '<i class="mdi mdi-file-pdf-outline"></i> PDF',
                titleAttr: 'Exportar a PDF',
                className: 'btn btn-sm btn-danger',
                exportOptions: {
                    columns: exportColumns
                },
                orientation: 'landscape',
                pageSize: 'LETTER'
            },
            {
                extend: 'print',
                text: '<i class="mdi mdi-printer"></i> Imprimir',
                titleAttr: 'Imprimir',
                className: 'btn btn-sm btn-secondary',
                exportOptions: {
                    columns: exportColumns
                }
            }
        ];
    },

    /**
     * Redibuja la tabla
     */
    redraw() {
        if (this.table) {
            this.table.draw();
        }
    }
};

// Hacer disponible globalmente
window.DataTableManager = DataTableManager;
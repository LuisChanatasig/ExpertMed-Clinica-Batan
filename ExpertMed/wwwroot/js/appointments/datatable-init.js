// datatable-init.js - Inicialización optimizada para tabla de citas
const DataTableManager = {
    table: null,
    isInitialized: false,

    /**
     * Inicializa el DataTable
     */
    init() {
        if (this.isInitialized) {
            console.log('DataTable ya inicializado');
            return;
        }

        console.log('Iniciando DataTable...');

        // Esperar a que el DOM esté completamente listo
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', () => {
                this.initializeTable();
            });
        } else {
            this.initializeTable();
        }
    },

    /**
     * Inicializa la tabla
     */
    initializeTable() {
        const $table = $('#appointmentTable');

        if ($table.length === 0) {
            console.error('Tabla #appointmentTable no encontrada');
            return;
        }

        // Verificar que haya filas
        const rowCount = $table.find('tbody tr').length;
        console.log(`Filas encontradas: ${rowCount}`);

        // Pequeño delay para asegurar renderizado completo
        setTimeout(() => {
            try {
                this.createDataTable($table);
            } catch (error) {
                console.error('Error al inicializar DataTable:', error);
            }
        }, 100);
    },

    /**
     * Crea la instancia de DataTable
     */
    createDataTable($table) {
        // Destruir instancia previa si existe
        if ($.fn.DataTable.isDataTable($table)) {
            console.log('Destruyendo instancia previa...');
            $table.DataTable().destroy();
        }

        // Configuración de DataTable
        this.table = $table.DataTable({
            // ===== CONFIGURACIÓN BÁSICA =====
            responsive: true,
            pageLength: AppConfig.DATATABLE.PAGE_LENGTH,
            order: [[3, 'desc']], // Ordenar por fecha descendente

            // ===== LAYOUT =====
            dom: '<"row"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>>' +
                '<"row"<"col-sm-12"B>>' +
                '<"row"<"col-sm-12"tr>>' +
                '<"row"<"col-sm-12 col-md-5"i><"col-sm-12 col-md-7"p>>',

            // ===== BOTONES DE EXPORTACIÓN =====
            buttons: this.getExportButtons(),

            // ===== IDIOMA =====
            language: {
                url: '//cdn.datatables.net/plug-ins/1.11.5/i18n/es-ES.json',
                processing: 'Procesando...',
                search: 'Buscar:',
                lengthMenu: 'Mostrar _MENU_ registros',
                info: 'Mostrando _START_ a _END_ de _TOTAL_ registros',
                infoEmpty: 'Mostrando 0 a 0 de 0 registros',
                infoFiltered: '(filtrado de _MAX_ registros totales)',
                paginate: {
                    first: 'Primero',
                    last: 'Último',
                    next: 'Siguiente',
                    previous: 'Anterior'
                },
                emptyTable: 'No hay datos disponibles',
                zeroRecords: 'No se encontraron coincidencias'
            },

            // ===== CONFIGURACIÓN DE RENDIMIENTO =====
            deferRender: true,
            processing: false,
            stateSave: false, // Desactivado para evitar conflictos con filtros del servidor

            // ===== DIMENSIONES =====
            autoWidth: false,
            scrollX: true,
            scrollCollapse: true,

            // ===== DEFINICIÓN DE COLUMNAS =====
            columnDefs: [
                // Columnas ocultas (ID y ID PATIENT)
                {
                    targets: [0, 1],
                    visible: false,
                    searchable: false
                },
                // Última columna (Acciones) no ordenable
                {
                    targets: -1,
                    orderable: false,
                    searchable: false,
                    className: 'text-center'
                },
                // Columna de consultorio
                {
                    targets: 2,
                    className: 'text-center'
                },
                // Columna de fecha
                {
                    targets: 3,
                    type: 'date',
                    render: function (data) {
                        return data; // Mantener formato dd/MM/yyyy
                    }
                },
                // Columna de estado de pago
                {
                    targets: -6, // Ajustar según tu estructura
                    className: 'text-center'
                },
                // Columna de estado
                {
                    targets: -2,
                    className: 'text-center'
                }
            ],

            // ===== CALLBACKS =====
            drawCallback: function (settings) {
                // Ajustar columnas después de dibujar
                this.api().columns.adjust();

                // Re-inicializar tooltips de Bootstrap si los usas
                const tooltipTriggerList = [].slice.call(
                    document.querySelectorAll('[data-bs-toggle="tooltip"]')
                );
                tooltipTriggerList.map(function (tooltipTriggerEl) {
                    return new bootstrap.Tooltip(tooltipTriggerEl);
                });
            },

            initComplete: function (settings, json) {
                const api = this.api();

                // Ajustar columnas y responsividad
                api.columns.adjust().responsive.recalc();

                DataTableManager.isInitialized = true;

                console.log('✓ DataTable inicializado correctamente');
                console.log(`  - Total de filas: ${api.rows().count()}`);
                console.log(`  - Columnas: ${api.columns().count()}`);

                // Trigger evento personalizado
                $(document).trigger('datatable:ready', [api]);

                // Ocultar spinner si existe
                $('.datatable-loading').removeClass('datatable-loading');
            }
        });

        // Configurar event listeners
        this.setupEventListeners();

        console.log('DataTable creado exitosamente');
    },

    /**
     * Obtiene la configuración de botones de exportación
     */
    getExportButtons() {
        const cleanData = function (data) {
            return typeof data === 'string'
                ? $('<div>').html(data).text().trim()
                : data;
        };

        const exportAllConfig = {
            columns: ':not(:last-child)',
            modifier: {
                page: 'all',
                search: 'applied'
            },
            format: {
                body: function (data) {
                    return cleanData(data);
                }
            }
        };

        return [
            // ===== EXCEL =====
            {
                extend: 'excelHtml5',
                text: '<i class="mdi mdi-file-excel"></i> Excel',
                titleAttr: 'Exportar a Excel',
                className: 'btn btn-sm btn-success',
                exportOptions: exportAllConfig,
                title: 'Listado de Citas',
                filename: 'Listado_Citas_' + new Date().toISOString().slice(0, 10)
            },

            // ===== PDF =====
            {
                extend: 'pdfHtml5',
                text: '<i class="mdi mdi-file-pdf"></i> PDF',
                titleAttr: 'Exportar a PDF',
                className: 'btn btn-sm btn-danger',
                exportOptions: exportAllConfig,
                orientation: 'landscape',
                pageSize: 'LEGAL',
                title: 'Listado de Citas',
                customize: function (doc) {
                    // Tamaños
                    doc.defaultStyle.fontSize = 8;
                    doc.styles.tableHeader.fontSize = 9;

                    // Header bonito
                    doc.styles.tableHeader.fillColor = '#4a6cf7';
                    doc.styles.tableHeader.color = 'white';
                    doc.styles.tableHeader.alignment = 'center';

                    // Centrar tabla
                    doc.content[1].alignment = 'center';

                    // Márgenes
                    doc.pageMargins = [20, 20, 20, 20];

                    // Auto width columnas
                    const colCount = doc.content[1].table.body[0].length;
                    doc.content[1].table.widths = Array(colCount).fill('*');

                    // Título más elegante
                    doc.content.splice(0, 0, {
                        text: 'REPORTE DE CITAS',
                        style: 'header',
                        alignment: 'center',
                        margin: [0, 0, 0, 10]
                    });
                }
            },

            // ===== PRINT =====
            {
                extend: 'print',
                text: '<i class="mdi mdi-printer"></i> Imprimir',
                titleAttr: 'Imprimir',
                className: 'btn btn-sm btn-secondary',
                exportOptions: exportAllConfig,
                title: 'Listado de Citas',
                customize: function (win) {
                    $(win.document.body)
                        .css('font-size', '10pt')
                        .prepend('<h3 style="text-align:center;">Listado de Citas</h3>');

                    $(win.document.body).find('table')
                        .addClass('compact')
                        .css('font-size', 'inherit');
                }
            }
        ];
    },
    /**
     * Configura event listeners
     */
    setupEventListeners() {
        let resizeTimer;

        // Redimensionar ventana
        $(window).on('resize.datatable', () => {
            clearTimeout(resizeTimer);
            resizeTimer = setTimeout(() => {
                this.adjustColumns();
            }, 250);
        });

        // Al mostrar el collapse de filtros
        $('#filterCollapse').on('shown.bs.collapse', () => {
            setTimeout(() => {
                this.adjustColumns();
            }, 350);
        });

        // Al ocultar el collapse de filtros
        $('#filterCollapse').on('hidden.bs.collapse', () => {
            setTimeout(() => {
                this.adjustColumns();
            }, 350);
        });

        // Cambio de orientación en móviles
        $(window).on('orientationchange.datatable', () => {
            setTimeout(() => {
                this.adjustColumns();
            }, 300);
        });
    },

    /**
     * Ajusta las columnas
     */
    adjustColumns() {
        if (this.table && this.isInitialized) {
            try {
                this.table.columns.adjust().responsive.recalc();
            } catch (error) {
                console.warn('No se pudo ajustar columnas:', error);
            }
        }
    },

    /**
     * Redibuja la tabla
     */
    redraw() {
        if (this.table) {
            this.table.draw();
            setTimeout(() => {
                this.adjustColumns();
            }, 100);
        }
    },

    /**
     * Destruye la instancia
     */
    destroy() {
        if (this.table) {
            $(window).off('.datatable');
            $('#filterCollapse').off('.bs.collapse');

            this.table.destroy();
            this.table = null;
            this.isInitialized = false;

            console.log('DataTable destruido');
        }
    },

    /**
     * Obtiene la instancia actual
     */
    getInstance() {
        return this.table;
    },

    /**
     * Aplica filtro por columna
     */
    filterByColumn(columnIndex, value) {
        if (this.table) {
            this.table.column(columnIndex).search(value).draw();
        }
    },

    /**
     * Limpia todos los filtros
     */
    clearFilters() {
        if (this.table) {
            this.table.search('').columns().search('').draw();
        }
    }
};

// Hacer disponible globalmente
window.DataTableManager = DataTableManager;

// Exportar para módulos
if (typeof module !== 'undefined' && module.exports) {
    module.exports = DataTableManager;
}
/**
 * consultationList.js
 * Lógica para la vista ConsultationList - ExpertMed
 * Sin dependencia de jQuery
 */


(function () {
    'use strict';

    // ─── Instancias de modales (cacheadas, no se recrean en cada click) ───────
    let _optionModal   = null;
    let _documentModal = null;

    // ─── Estado interno ───────────────────────────────────────────────────────
    let _currentConsultationId  = null;
    let _currentAppointmentStatus = null;

    // ─── Mapeo de tipo de documento → URL ─────────────────────────────────────
    function getDocumentUrl(type) {
        const urls = window.AppUrls || {};
        const map = {
            justificante : urls.medicalCertificate,
            formulario   : urls.medicalForm,
            formulario2  : urls.medicalForm2,
            receta       : urls.medicationRecipe,
            laboratorio  : urls.laboratoryDoc,
            imagen       : urls.imageDoc,
        };
        return map[type] || null;
    }

    // ─── Persistencia de collapses en sessionStorage ──────────────────────────
    function saveCollapseState(id, isOpen) {
        try {
            const key = 'collapse_' + id;
            isOpen
                ? sessionStorage.setItem(key, '1')
                : sessionStorage.removeItem(key);
        } catch (_) { /* sessionStorage no disponible */ }
    }

    function restoreCollapseStates() {
        document.querySelectorAll('.collapse').forEach(function (el) {
            try {
                if (sessionStorage.getItem('collapse_' + el.id) === '1') {
                    el.classList.add('show');
                }
            } catch (_) { /* noop */ }
        });
    }

    function bindCollapseEvents() {
        document.querySelectorAll('[data-bs-toggle="collapse"]').forEach(function (trigger) {
            trigger.addEventListener('click', function () {
                const targetId = this.getAttribute('href')?.replace('#', '') ||
                                 this.getAttribute('data-bs-target')?.replace('#', '');
                if (!targetId) return;
                const panel = document.getElementById(targetId);
                if (!panel) return;
                // Bootstrap dispara la clase 'show' después del click;
                // lo comprobamos en el próximo tick
                setTimeout(function () {
                    saveCollapseState(targetId, panel.classList.contains('show'));
                }, 0);
            });
        });
    }

    // ─── Búsqueda en tiempo real (sin jQuery, usa data-search precalculado) ───
    function initSearch() {
        const input = document.getElementById('consultaSearch');
        if (!input) return;

        input.addEventListener('input', function () {
            const filtro = this.value.toLowerCase().trim();
            document.querySelectorAll('.grupo-consulta').forEach(function (el) {
                const hayCoincidencia = filtro === '' ||
                    (el.dataset.search || '').includes(filtro);
                el.style.display = hayCoincidencia ? '' : 'none';
            });
        });
    }

    // ─── Modal de opciones ────────────────────────────────────────────────────
    function initOptionModal() {
        const el = document.getElementById('optionModal');
        if (!el) return;
        _optionModal = new bootstrap.Modal(el, { keyboard: false });

        // Limpiar estado al cerrar
        el.addEventListener('hidden.bs.modal', function () {
            _currentConsultationId    = null;
            _currentAppointmentStatus = null;
            document.getElementById('consultationId').value = '';
        });
    }

    window.openOptionModal = function (consultationId, appointmentStatus) {
        if (!_optionModal) return;

        _currentConsultationId    = consultationId;
        _currentAppointmentStatus = appointmentStatus;

        document.getElementById('consultationId').value = consultationId;

        const btnFinish = document.getElementById('btnFinishConsult');
        if (btnFinish) {
            btnFinish.classList.toggle('d-none', appointmentStatus === 4);
        }

        _optionModal.show();
    };

    window.finishConsultation = function () {
        const id = _currentConsultationId;
        if (!id) { console.warn('No hay consultationId activo.'); return; }
        window.location.href = (window.AppUrls?.consultationUpdate || '') +
            '?consultationId=' + encodeURIComponent(id);
    };

    window.openReviewConsulta = function () {
        const id = _currentConsultationId;
        if (!id) { alert('No se encontró un ID de consulta válido.'); return; }
        window.location.href = (window.AppUrls?.consultationDetails || '') +
            '?consultationId=' + encodeURIComponent(id);
    };

    // ─── Modal de documentos ──────────────────────────────────────────────────
    function initDocumentModal() {
        const el = document.getElementById('optionsDocuments');
        if (!el) return;
        _documentModal = new bootstrap.Modal(el, { keyboard: false });

        // Resetear selector al cerrar
        el.addEventListener('hidden.bs.modal', function () {
            const select = document.getElementById('documentType');
            if (select) select.selectedIndex = 0;
            enableDownloadButton();
        });
    }

    window.openModalDocument = function () {
        if (!_documentModal) {
            console.error("El modal 'optionsDocuments' no fue encontrado.");
            return;
        }
        _documentModal.show();
    };

    window.enableDownloadButton = function () {
        const select = document.getElementById('documentType');
        const btn    = document.getElementById('downloadButton');
        if (!select || !btn) return;
        btn.disabled = (select.value === 'Seleccione un documento' || select.selectedIndex === 0);
    };

    window.downloadDocument = function () {
        const select  = document.getElementById('documentType');
        const btn     = document.getElementById('downloadButton');
        const txtSpan = document.getElementById('downloadText');
        const spinner = document.getElementById('downloadSpinner');
        const id      = _currentConsultationId;

        if (!id)     { alert('No se encontró un ID de consulta válido.'); return; }
        if (!select) { return; }

        const actionUrl = getDocumentUrl(select.value);
        if (!actionUrl) { alert('Opción no válida.'); return; }

        // UI: estado de carga
        if (txtSpan) txtSpan.textContent = 'Generando...';
        if (spinner) spinner.classList.remove('d-none');
        if (btn)     btn.disabled = true;

        window.open(actionUrl + '?consultationId=' + encodeURIComponent(id), '_blank');

        // Restaurar botón tras 2 s
        setTimeout(function () {
            if (txtSpan) txtSpan.textContent = 'Descargar';
            if (spinner) spinner.classList.add('d-none');
            if (btn)     btn.disabled = false;
        }, 2000);
    };

    // ─── Cambio de tamaño de página ───────────────────────────────────────────
    window.changePageSize = function () {
        const select = document.getElementById('pageSizeSelect');
        if (!select) return;
        const baseUrl = window.AppUrls?.consultationList || '';
        window.location.href = baseUrl + '?page=1&pageSize=' + encodeURIComponent(select.value);
    };

    // ─── Notificaciones SweetAlert (llamada desde la vista) ──────────────────
    window.showToast = function (type, message) {
        if (typeof Swal === 'undefined' || !message) return;
        Swal.fire({
            title          : type === 'success' ? '¡Éxito!' : 'Error',
            text           : message,
            icon           : type,
            confirmButtonText: 'OK',
            timer          : 3000,
            timerProgressBar: true,
        });
    };

    // ─── Init ─────────────────────────────────────────────────────────────────
    document.addEventListener('DOMContentLoaded', function () {
        initSearch();
        initOptionModal();
        initDocumentModal();
        restoreCollapseStates();
        bindCollapseEvents();
    });

})();
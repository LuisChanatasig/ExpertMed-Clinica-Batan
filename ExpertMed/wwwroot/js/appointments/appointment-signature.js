/**
 * appointment-signature.js
 * Versión final optimizada para ExpertMed - Gestión de Firma y Asistencia
 */
const AppointmentSignature = (function () {
    let currentToken = null;
    let pollTimer = null;

    // Gestión de interfaz de estados
    function setSigUI(status, infoText) {
        const badge = document.getElementById("sigBadge");
        const info = document.getElementById("sigInfo");
        const btnReg = document.getElementById("btnRegistrarAsistencia");

        if (info) info.textContent = infoText || "";

        if (status === 1) { // FIRMADO
            if (badge) { badge.className = "badge bg-success"; badge.textContent = "Firmado"; }
            if (btnReg) btnReg.disabled = false;
        } else if (status === 2) { // EXPIRADO
            if (badge) { badge.className = "badge bg-danger"; badge.textContent = "Expirado"; }
            if (btnReg) btnReg.disabled = true;
        } else { // PENDIENTE
            if (badge) { badge.className = "badge bg-warning"; badge.textContent = "Pendiente"; }
            if (btnReg) btnReg.disabled = true;
        }
    }

    return {
        // Inicializa el modal de la cita seleccionada
        init: function (appId, patientId) {
            if (pollTimer) clearInterval(pollTimer);

            // Asignación de IDs a campos ocultos
            const inputApp = document.getElementById("checkInAppId");
            const inputPat = document.getElementById("checkInPatientId");
            if (inputApp) inputApp.value = appId;
            if (inputPat) inputPat.value = patientId;

            // Reset de elementos visuales
            const img = document.getElementById("qrImg");
            const ph = document.getElementById("qrPlaceholder");
            const urlInp = document.getElementById("signUrl");
            const btnReg = document.getElementById("btnRegistrarAsistencia");

            if (img) img.style.display = "none";
            if (ph) ph.style.display = "block";
            if (urlInp) urlInp.value = "";
            if (btnReg) {
                btnReg.disabled = true;
                btnReg.innerHTML = '<i class="ri-check-double-line me-1"></i> Confirmar Asistencia';
            }

            setSigUI(0, "Haga clic en 'Generar QR' para iniciar...");

            const modalEl = document.getElementById('modalCheckIn');
            if (modalEl) {
                const modal = new bootstrap.Modal(modalEl);
                modal.show();
            }
        },

        // Crea la solicitud de firma y muestra el QR
        generarQrFirma: async function () {
            try {
                const appIdVal = document.getElementById("checkInAppId")?.value;
                const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');

                if (!tokenInput) throw new Error("No se encontró el token de seguridad (AntiForgeryToken).");

                const url = AppConfig.ENDPOINTS.SIGNATURE_CREATE;

                const resp = await fetch(url, {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/x-www-form-urlencoded",
                        "RequestVerificationToken": tokenInput.value
                    },
                    body: new URLSearchParams({ patientCode: 'APP-' + appIdVal })
                });

                const data = await resp.json();
                if (!data.ok) throw new Error(data.message || "Error al crear solicitud.");

                currentToken = data.token;
                document.getElementById("SignatureToken").value = currentToken;
                document.getElementById("signUrl").value = data.signUrl;

                if (typeof QRCode === "undefined") throw new Error("Librería QRCode no disponible.");

                // Renderizado del QR
                const img = document.getElementById("qrImg");
                const ph = document.getElementById("qrPlaceholder");
                const tmp = document.createElement("div");
                new QRCode(tmp, { text: data.signUrl, width: 180, height: 180 });

                setTimeout(() => {
                    const canvas = tmp.querySelector("canvas");
                    if (canvas && img) {
                        img.src = canvas.toDataURL("image/png");
                        img.style.display = "inline-block";
                        if (ph) ph.style.display = "none";
                    }
                }, 150);

                // Iniciar escucha del servidor
                AppointmentSignature.startPolling();

            } catch (e) {
                console.error("Error en generarQrFirma:", e);
                Swal.fire("Error", e.message, "error");
            }
        },

        // Revisa el estado de la firma cada 2 segundos
        startPolling: function () {
            if (pollTimer) clearInterval(pollTimer);
            pollTimer = setInterval(async () => {
                const url = `${AppConfig.ENDPOINTS.SIGNATURE_STATUS}?token=${encodeURIComponent(currentToken)}`;
                try {
                    const resp = await fetch(url);
                    const data = await resp.json();
                    if (data.status === 1) { // Éxito: Paciente firmó
                        setSigUI(1, "¡Firma recibida correctamente!");
                        clearInterval(pollTimer);
                        Swal.fire({ icon: 'success', title: '¡Firma Lista!', text: 'Ya puede confirmar el ingreso.', timer: 2000, showConfirmButton: false });
                    } else if (data.status === 2) { // Expirado
                        setSigUI(2, "La solicitud ha expirado.");
                        clearInterval(pollTimer);
                    }
                } catch (e) { }
            }, 2000);
        },

        // Copia el link manual al portapapeles
        copiarLink: function () {
            const copyText = document.getElementById("signUrl");
            if (copyText && copyText.value) {
                copyText.select();
                navigator.clipboard.writeText(copyText.value);
                Swal.fire({ icon: 'success', title: 'Copiado', timer: 1000, showConfirmButton: false });
            }
        },

        // Envía la confirmación final al controlador de Citas
        confirmarAsistencia: function () {
            const appId = document.getElementById("checkInAppId").value;
            const patientId = document.getElementById("checkInPatientId").value;
            const tokenAnti = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

            if (!currentToken) {
                Swal.fire("Error", "No hay un token de firma activo.", "error");
                return;
            }

            // UI Feedback: Bloquear botón y mostrar carga
            const btn = document.getElementById("btnRegistrarAsistencia");
            btn.disabled = true;
            btn.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span> Procesando...';

            $.post(AppConfig.ENDPOINTS.CONFIRM_ATTENDANCE, {
                appointmentId: appId,
                patientId: patientId,
                signatureToken: currentToken,
                __RequestVerificationToken: tokenAnti
            })
                .done(function (res) {
                    if (res.success) {
                        Swal.fire({ icon: 'success', title: '¡Asistencia Registrada!', showConfirmButton: false, timer: 1500 })
                            .then(() => location.reload());
                    } else {
                        Swal.fire("Error", res.message, "error");
                        btn.disabled = false;
                        btn.innerHTML = '<i class="ri-check-double-line me-1"></i> Confirmar Asistencia';
                    }
                })
                .fail(function () {
                    Swal.fire("Error", "Error de comunicación con el servidor.", "error");
                    btn.disabled = false;
                    btn.innerHTML = '<i class="ri-check-double-line me-1"></i> Confirmar Asistencia';
                });
        },

        detener: function () { if (pollTimer) clearInterval(pollTimer); }
    };
})();

// Puentes Globales para el HTML
window.initCheckIn = AppointmentSignature.init;
window.generarQrFirmaCheckIn = AppointmentSignature.generarQrFirma;
window.confirmarAsistenciaFinal = AppointmentSignature.confirmarAsistencia;
window.detenerPolling = AppointmentSignature.detener;
window.copiarLinkFirma = AppointmentSignature.copiarLink;
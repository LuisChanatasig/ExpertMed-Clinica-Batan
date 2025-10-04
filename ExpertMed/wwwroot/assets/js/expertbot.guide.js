/* global introJs */
(function (root, factory) {
    if (typeof define === "function" && define.amd) {
        define([], factory);
    } else if (typeof module === "object" && module.exports) {
        module.exports = factory();
    } else {
        root.ExpertBotGuide = factory();
    }
})(typeof self !== "undefined" ? self : this, function () {
    "use strict";

    // ==========================
    // Defaults
    // ==========================
    const defaultOptions = {
        // Tours
        steps: [],                  // tour principal por pantalla
        extraTours: {},             // { menu: [ {selector, html, position}, ... ] }
        routes: {},                 // { 'citas': '/Appointment/AppointmentList', ... }
        menuHelp: null,             // HTML con resumen del menú visible
        storagePrefix: null,

        // UI/UX
        greetMessage: "¡Hola! Haz clic en mí para empezar 👋", // admite {name}
        tips: [
            '💡 Tip: Escribe "/ayuda" para ver comandos.',
            "🎯 ¿Citas o pacientes? Puedo guiarte.",
            "🗺️ ¿Tour rápido? Escribe \"/tour\".",
            "🧭 Escribe \"/menu\" para el tour del menú.",
            "⚡ Usa \"/ir citas\" para navegar directo."
        ],
        enableVoice: true,
        autoSuggestTour: true,
        resetHotkey: { ctrl: true, alt: true, key: "t" },
        onTourComplete: null,

        // Mini-NLP local
        commands: {
            "/tour": "Inicia el tour guiado.",
            "/menu": "Muestra el tour del menú (o el resumen).",
            "/ir <módulo>": "Navega directo al módulo.",
            "/ayuda": "Muestra los comandos.",
            "/perfil": "Describe tu rol actual.",
            "/funciones": "Lista tus accesos rápidos."
        },
        knowledge: {
            saludos: [
                "¡Hola {name}! Soy ExpertBot, tu asistente 👋",
                "¡Hola {name}! ¿Listo para trabajar más rápido? 🚀",
                "Bienvenido, {name}. ¿Qué necesitas hoy?"
            ],
            ayuda: {
                citas: "Agenda, edita y gestiona citas médicas.",
                pacientes: "Registra/edita pacientes y consulta historiales.",
                consultas: "Registra diagnósticos y tratamientos.",
                laboratorio: "Gestiona solicitudes y resultados.",
                imagenes: "Gestiona estudios e informes.",
                reportes: "Métricas y reportes de gestión."
            }
        },
        profileText: null,
        functionsList: [],

        // Dónde renderizar (si autoRender)
        container: "body",
        autoRender: false,
        ui: {
            avatar: "#botAvatar",
            bubble: "#botSpeechBubble",
            dialog: "#botChatExpanded",
            closeBtn: "#closeChatBtn",
            form: "#chatForm",
            input: "#chatPrompt",
            sendBtn: "#btnEnviar",
            micBtn: "#btnVoz",
            response: "#chatResponse",
            tourSuggest: "#tourSuggestion",
            startTourBtn: "#startTourBtn",
            dismissTourBtn: "#dismissTourBtn",
            message: "#botMessage"
        },
        voiceLang: "es-ES",
        i18n: { next: "Siguiente", prev: "Atrás", skip: "Omitir", done: "Listo" },

        // Personalización
        userName: null
    };

    // ==========================
    // Helpers
    // ==========================
    const interpolate = (str, map) =>
        (str || "").replace(/\{(\w+)\}/g, (_, k) =>
            map && map[k] != null ? String(map[k]) : ""
        );

    function asElement(x) {
        if (!x) return null;
        if (x instanceof Element) return x;
        if (typeof x === "string") return document.querySelector(x);
        return null;
    }

    function resolveContainer(containerOpt) {
        return asElement(containerOpt) || document.body;
    }

    function renderDefaultUI(root, ui) {
        // Evita duplicar si ya existe
        if (root.querySelector(ui.avatar) && root.querySelector(ui.dialog)) return;

        const wrap = document.createElement("div");
        wrap.className = "expertbot-guide";
        wrap.setAttribute("data-ebg", "");
        wrap.innerHTML = `
      <div class="bot-speech-bubble" id="botSpeechBubble" role="status" aria-live="polite">
        <div class="fw-semibold mb-1">ExpertBot</div>
        <div id="botMessage">¡Hola! Haz clic en mí para empezar 👋</div>
      </div>

      <div class="bot-chat-expanded" id="botChatExpanded" role="dialog" aria-modal="true" aria-labelledby="botDialogTitle" aria-hidden="true">
        <div class="bot-chat-header">
          <div class="d-flex align-items-center gap-2">
            <i class="mdi mdi-robot fs-5" aria-hidden="true"></i>
            <span id="botDialogTitle" class="fw-semibold">ExpertBot - Tu Guía</span>
          </div>
          <button type="button" class="btn btn-sm btn-link text-white p-0" id="closeChatBtn" aria-label="Cerrar chat">
            <i class="mdi mdi-close fs-5" aria-hidden="true"></i>
          </button>
        </div>

        <div class="bot-chat-body">
          <div class="tour-suggestion" id="tourSuggestion" style="display:none">
            <div class="d-flex align-items-center gap-2 mb-2">
              <i class="mdi mdi-map-marker-radius text-success fs-4" aria-hidden="true"></i>
              <div class="fw-semibold">¿Necesitas un tour?</div>
            </div>
            <p class="mb-2 small">Te guío por todas las funciones paso a paso.</p>
            <div class="d-flex gap-2 justify-content-center">
              <button type="button" class="btn btn-success btn-sm" id="startTourBtn">
                <i class="mdi mdi-play" aria-hidden="true"></i> Iniciar Tour
              </button>
              <button type="button" class="btn btn-outline-secondary btn-sm" id="dismissTourBtn">Ahora no</button>
            </div>
          </div>

          <div id="chatResponse" class="mb-3" style="min-height:80px;background:#f8f9fa;border-radius:10px;padding:15px" aria-live="polite">
            <em class="text-muted">Escribe algo para empezar a conversar...</em>
          </div>
        </div>

        <div class="chat-input-group">
          <form id="chatForm" class="needs-validation" novalidate>
            <div class="input-group">
              <input type="text" id="chatPrompt" name="prompt" class="form-control"
                     placeholder="Escribe /tour, /menu o /ir citas..." required
                     aria-label="Escribe tu consulta para ExpertBot" />
              <button type="submit" id="btnEnviar" class="btn btn-success" title="Enviar">
                <i class="mdi mdi-send" aria-hidden="true"></i>
              </button>
              <button type="button" id="btnVoz" class="btn btn-outline-secondary" title="Usar micrófono" aria-label="Usar micrófono">
                <i class="mdi mdi-microphone" aria-hidden="true"></i>
              </button>
            </div>
          </form>
        </div>
      </div>

      <button class="bot-avatar btn btn-primary rounded-circle shadow-lg" id="botAvatar"
              aria-haspopup="dialog" aria-expanded="false" title="Abrir asistente">
        <i class="mdi mdi-robot-outline fs-4"></i>
      </button>
    `;
        root.appendChild(wrap);
    }

    function getUI(root, ui) {
        const q = (sel) => (typeof sel === "string" ? root.querySelector(sel) : sel);
        return {
            botAvatar: q(ui.avatar),
            speechBubble: q(ui.bubble),
            chatDialog: q(ui.dialog),
            closeChatBtn: q(ui.closeBtn),
            chatForm: q(ui.form),
            chatInput: q(ui.input),
            sendBtn: q(ui.sendBtn),
            micBtn: q(ui.micBtn),
            chatResponse: q(ui.response),
            tourSuggestion: q(ui.tourSuggest),
            startTourBtn: q(ui.startTourBtn),
            dismissTourBtn: q(ui.dismissTourBtn),
            botMessage: q(ui.message)
        };
    }

    // ==========================
    // Core init
    // ==========================
    function init(userOpts) {
        const opt = Object.assign({}, defaultOptions, userOpts || {});

        // Normaliza profileText
        opt.profileText =
            opt.profileText && String(opt.profileText).trim()
                ? opt.profileText
                : "Perfil no identificado.";

        // storagePrefix dinámico si no viene
        if (!opt.storagePrefix) {
            const pathKey =
                (location.pathname || "/").replace(/[^\w]/g, "_").slice(-80) || "root";
            opt.storagePrefix = "exp.page." + pathKey;
        }
        const KEYS = {
            tourDone: `${opt.storagePrefix}.tourDone`,
            tourDismissed: `${opt.storagePrefix}.tourDismissed`,
            botGreeted: `${opt.storagePrefix}.botGreeted`
        };

        const root = resolveContainer(opt.container);
        if (opt.autoRender) renderDefaultUI(root, opt.ui);

        const {
            botAvatar,
            speechBubble,
            chatDialog,
            closeChatBtn,
            chatForm,
            chatInput,
            sendBtn,
            micBtn,
            chatResponse,
            tourSuggestion,
            startTourBtn,
            dismissTourBtn,
            botMessage
        } = getUI(root, opt.ui);

        if (!botAvatar || !speechBubble || !chatDialog) {
            console.warn(
                "[ExpertBotGuide] Controles no encontrados. Verifica `ui` o usa autoRender:true."
            );
            return { startTour: () => { }, open: () => { }, close: () => { } };
        }

        // Saludo personalizado
        const name = (opt.userName && String(opt.userName).trim()) || null;
        let greet =
            String(opt.greetMessage || "").trim() || defaultOptions.greetMessage;
        greet = name
            ? /\{name\}/i.test(greet)
                ? interpolate(greet, { name })
                : `¡Hola, ${name}!`
            : greet;
        if (botMessage) botMessage.textContent = greet;

        // Estado
        let dialogOpen = false;
        let randomTipIntervalId = null;
        let trapHandler = null;
        let outsideClickHandler = null;

        // Helpers
        const setBusy = (busy) => {
            if (chatResponse) chatResponse.setAttribute("aria-busy", busy ? "true" : "false");
            if (sendBtn) sendBtn.disabled = !!busy;
            botAvatar.classList.toggle("talking", !!busy);
        };

        const showSpeech = (msg, ms = 3500) => {
            const finalMsg = name ? interpolate(msg, { name }) : msg;
            if (botMessage) botMessage.textContent = finalMsg;
            speechBubble.classList.add("show");
            window.setTimeout(() => speechBubble.classList.remove("show"), ms);
        };

        // ---- Navegación rápida
        function normalizeKey(s) {
            return (s || "")
                .normalize("NFD")
                .replace(/[\u0300-\u036f]/g, "")
                .toLowerCase()
                .replace(/[^a-z0-9]+/g, " ")
                .trim();
        }

        function tryNavigate(target) {
            const key = normalizeKey(target);
            const routes = opt.routes || {};
            const url = routes[key] || routes[target];
            if (url) {
                window.location.href = url;
                return { ok: true, msg: `Abriendo ${target}…` };
            }
            // Abrir secciones de menú (collapse) por id conocido
            const guessId = {
                consultorios: "sidebarConsultorios",
                establecimientos: "sidebarEstablecimientos",
                usuarios: "sidebarUsuarios",
                pacientes: "sidebarPacientes",
                agenda: "sidebarAppointment",
                consultas: "sidebarConsultas",
                solicitudes: "sidebarLab",
                resultados: "sidebarResultados",
                imagenes: "sidebarSolicitudesImagen",
                "resultados imagen": "sidebarResultadosImagen",
                reportes: "sidebarReportes",
                fisioterapia: "sidebarFisioterapia",
                facturacion: "sidebarFacturacion"
            };
            const id = guessId[key];
            if (id) {
                const trigger = document.querySelector(`a[href="#${id}"]`);
                if (trigger) {
                    trigger.click();
                    return { ok: true, msg: `Sección ${target} abierta.` };
                }
            }
            return {
                ok: false,
                msg: opt.menuHelp || "No encuentro esa ruta. Escribe /menu para ver secciones."
            };
        }

        // Dialog accesible
        const openDialog = () => {
            if (dialogOpen) return;
            dialogOpen = true;
            chatDialog.classList.add("show");
            chatDialog.setAttribute("aria-hidden", "false");
            botAvatar.setAttribute("aria-expanded", "true");

            // Sugerir tour si aplica
            const dismissed = localStorage.getItem(KEYS.tourDismissed) === "1";
            const done = localStorage.getItem(KEYS.tourDone) === "1";
            if (tourSuggestion)
                tourSuggestion.style.display =
                    !done && opt.autoSuggestTour && !dismissed ? "block" : "none";

            // Focus trap
            const focusables = chatDialog.querySelectorAll(
                'button, [href], input, textarea, [tabindex]:not([tabindex="-1"])'
            );
            const first = focusables[0],
                last = focusables[focusables.length - 1];
            trapHandler = (e) => {
                if (e.key !== "Tab") return;
                if (e.shiftKey && document.activeElement === first) {
                    e.preventDefault();
                    last?.focus();
                } else if (!e.shiftKey && document.activeElement === last) {
                    e.preventDefault();
                    first?.focus();
                }
            };
            chatDialog.addEventListener("keydown", trapHandler);
            window.setTimeout(() => chatInput?.focus(), 50);

            // Cerrar por clic fuera
            outsideClickHandler = (e) => {
                if (!chatDialog.contains(e.target) && !botAvatar.contains(e.target)) closeDialog();
            };
            document.addEventListener("mousedown", outsideClickHandler, { capture: true });
        };

        const closeDialog = () => {
            if (!dialogOpen) return;
            dialogOpen = false;
            chatDialog.classList.remove("show");
            chatDialog.setAttribute("aria-hidden", "true");
            botAvatar.setAttribute("aria-expanded", "false");
            if (trapHandler) chatDialog.removeEventListener("keydown", trapHandler);
            if (outsideClickHandler)
                document.removeEventListener("mousedown", outsideClickHandler, {
                    capture: true
                });
            botAvatar.focus();
        };

        botAvatar.addEventListener("click", openDialog);
        botAvatar.addEventListener("keydown", (e) => {
            if (e.key === "Enter" || e.key === " ") {
                e.preventDefault();
                openDialog();
            }
        });
        closeChatBtn?.addEventListener("click", closeDialog);
        document.addEventListener("keydown", (e) => {
            if (e.key === "Escape" && dialogOpen) closeDialog();
        });

        // Greet una sola vez por storagePrefix
        if (localStorage.getItem(KEYS.botGreeted) !== "1") {
            window.setTimeout(() => showSpeech(greet, 5000), 900);
            localStorage.setItem(KEYS.botGreeted, "1");
        }

        // Tips periódicos
        const startTips = () => {
            if (randomTipIntervalId) return;
            randomTipIntervalId = window.setInterval(() => {
                const typing = document.activeElement === chatInput;
                if (dialogOpen || typing || document.hidden) return;
                if (Math.random() < 0.35 && opt.tips?.length) {
                    const tip = opt.tips[Math.floor(Math.random() * opt.tips.length)];
                    showSpeech(tip, 3000);
                }
            }, 30000);
        };
        const stopTips = () => {
            if (randomTipIntervalId) {
                clearInterval(randomTipIntervalId);
                randomTipIntervalId = null;
            }
        };
        startTips();
        document.addEventListener("visibilitychange", () => {
            if (document.hidden) stopTips();
            else startTips();
        });

        // Mini-NLP
        const knowledge = opt.knowledge || {};

        function helpHtml() {
            const cmds = Object.entries(opt.commands || {})
                .map(([c, d]) => `<code>${c}</code> - ${d}`)
                .join("<br>");
            const extras =
                "Citas • Pacientes • Consultas • Laboratorio • Imágenes • Reportes";
            return `<strong>🤖 Comandos:</strong><div class="mt-2">${cmds}</div><div class="mt-2"><strong>También puedes preguntar por:</strong><br/>${extras}</div>`;
        }

        function profileHtml() {
            return `<strong>👤 Tu perfil:</strong><br>${opt.profileText}`;
        }

        function functionsHtml() {
            const lis = (opt.functionsList || []).map((t) => `• ${t}`).join("<br>");
            return `<strong>🎯 Funciones disponibles:</strong><br>${lis}<br><br><em>Usa los accesos rápidos de la página.</em>`;
        }

        function buildStepsFrom(list) {
            const steps = [];
            (list || []).forEach((s) => {
                const element = s.element || document.querySelector(s.selector);
                if (!element) return;
                steps.push({ element, intro: s.html, position: s.position || "bottom" });
            });
            return steps;
        }

        // --- Tour de menú automático (fallback si no hay extraTours.menu)
        function buildAutoMenuSteps() {
            const steps = [];
            const nav = document.querySelector("#navbar-nav");
            if (nav) {
                steps.push({
                    element: nav,
                    intro: "Este es tu <strong>menú lateral</strong>. Se adapta a tu perfil.",
                    position: "right"
                });
            }

            const order = [
                "sidebarConsultorios",
                "sidebarEstablecimientos",
                "sidebarUsuarios",
                "sidebarPacientes",
                "sidebarAppointment",
                "sidebarConsultas",
                "sidebarLab",
                "sidebarResultados",
                "sidebarSolicitudesImagen",
                "sidebarResultadosImagen",
                "sidebarReportes",
                "sidebarFisioterapia",
                "sidebarFacturacion"
            ];

            order.forEach((id) => {
                const trigger = document.querySelector(`a[href="#${id}"]`);
                if (trigger) {
                    const txt = (trigger.innerText || trigger.textContent || "").trim();
                    const tip = `Haz clic para abrir la sección <strong>${txt}</strong>.`;
                    steps.push({ element: trigger, intro: tip, position: "right" });
                }
            });

            const brand = document.querySelector(".navbar-brand-box");
            if (brand)
                steps.push({
                    element: brand,
                    intro: "Logo — atajo para volver al <strong>inicio</strong>.",
                    position: "right"
                });

            if (!steps.length) {
                steps.push({
                    element: document.body,
                    intro:
                        'No pude localizar el menú. Verifica que el layout ya esté cargado y vuelve a ejecutar <code>/menu</code>.',
                    position: "bottom"
                });
            }
            return steps;
        }

        function startTour(force = false, which = null) {
            if (typeof introJs !== "function") {
                console.warn("[ExpertBotGuide] introJs no está disponible. Incluye intro.js.");
                return;
            }
            if (!force && localStorage.getItem(KEYS.tourDone) === "1" && !which) {
                showSpeech("Ya conoces el sistema 😉 Usa /tour para repetir.", 4000);
                return;
            }

            let steps =
                which === "menu"
                    ? (opt.extraTours && opt.extraTours.menu && buildStepsFrom(opt.extraTours.menu)) ||
                    buildAutoMenuSteps()
                    : buildStepsFrom(opt.steps);

            if (!steps.length) {
                showSpeech("No tengo pasos para mostrar aquí 😅", 2800);
                return;
            }

            const intro = introJs();
            intro.setOptions({
                steps,
                nextLabel: opt.i18n.next,
                prevLabel: opt.i18n.prev,
                skipLabel: opt.i18n.skip,
                doneLabel: opt.i18n.done,
                showProgress: true,
                exitOnOverlayClick: false,
                tooltipClass: "rounded-3 shadow",
                scrollToElement: true,
                scrollPadding: 24
            });
            intro.onbeforechange(() => showSpeech("Sigamos…", 1600));
            intro.oncomplete(() => {
                localStorage.setItem(KEYS.tourDone, "1");
                showSpeech("¡Tour completado! 🎉", 3000);
                if (typeof opt.onTourComplete === "function") {
                    try {
                        opt.onTourComplete();
                    } catch { }
                }
            });
            intro.onexit(() => localStorage.setItem(KEYS.tourDone, "1"));
            intro.start();
        }

        // Procesamiento de entradas
        function process(input) {
            const t = (input || "").toLowerCase().trim();
            if (!t) return { kind: "empty" };

            // Commands
            if (t === "/tour") return { kind: "cmd", action: "tour" };

            if (t === "/menu" || t === "menu" || t === "/tour menu" || t === "tour menu") {
                // si hay tour explícito lo usamos; si no, se usa el automático en startTour
                return { kind: "cmd", action: "tour", which: "menu" };
            }

            const ir = t.match(/^\/ir\s+(.+)$/);
            if (ir) {
                const nav = tryNavigate(ir[1]);
                if (nav.ok) return { kind: "text", text: nav.msg };
                return { kind: "html", html: nav.msg };
            }

            if (t === "/ayuda" || t === "ayuda") return { kind: "html", html: helpHtml() };
            if (t === "/perfil" || t.includes("perfil") || t.includes("rol"))
                return { kind: "html", html: profileHtml() };
            if (t === "/funciones" || t.includes("funciones"))
                return { kind: "html", html: functionsHtml() };

            // Saludos
            if (/^(hola|buen[oa]s|hello|hi)/.test(t)) {
                const arr = knowledge.saludos || ["Hola {name} 👋"];
                const grTpl = arr[Math.floor(Math.random() * arr.length)];
                const gr = name ? interpolate(grTpl, { name }) : grTpl.replace("{name}", "");
                return { kind: "text", text: gr };
            }

            // Intenciones simples
            if (t.includes("cita"))
                return { kind: "text", text: knowledge.ayuda?.citas || "Citas." };
            if (t.includes("pacient"))
                return { kind: "text", text: knowledge.ayuda?.pacientes || "Pacientes." };
            if (t.includes("consulta"))
                return { kind: "text", text: knowledge.ayuda?.consultas || "Consultas." };
            if (t.includes("laborat") || t.includes("anális"))
                return {
                    kind: "text",
                    text: knowledge.ayuda?.laboratorio || "Laboratorio."
                };
            if (t.includes("imagen") || t.includes("radio"))
                return { kind: "text", text: knowledge.ayuda?.imagenes || "Imágenes." };
            if (t.includes("reporte") || t.includes("estad"))
                return { kind: "text", text: knowledge.ayuda?.reportes || "Reportes." };

            if (/(gracias|chao|bye|adios)/.test(t)) {
                return {
                    kind: "text",
                    text: name
                        ? `¡De nada, ${name}! Usa /tour cuando quieras ver el recorrido. 👋`
                        : "¡De nada! Usa /tour cuando quieras ver el recorrido. 👋"
                };
            }

            return {
                kind: "html",
                html: `No estoy seguro de eso. Prueba:<br>
           • <code>/tour</code> para un recorrido guiado<br>
           • <code>/menu</code> para ver el menú o su tour<br>
           • <code>/ir &lt;módulo&gt;</code> (ej. <code>/ir citas</code>)<br>
           • <code>/ayuda</code> para ver comandos<br>
           • Pregunta por: citas, pacientes, consultas, laboratorio, imágenes, reportes`
            };
        }

        // Chat handler
        chatForm?.addEventListener("submit", (e) => {
            e.preventDefault();
            const val = chatInput?.value?.trim();
            if (!val) {
                chatInput?.focus();
                return;
            }

            const res = process(val);
            if (res.kind === "cmd" && res.action === "tour") {
                chatInput.value = "";
                if (dialogOpen) closeDialog();
                startTour(true, res.which || null);
                return;
            }

            setBusy(true);
            if (chatResponse) {
                chatResponse.innerHTML = `<div class='d-flex align-items-center gap-2'><div class='spinner-border spinner-border-sm text-success'></div><em>ExpertBot pensando...</em></div>`;
            }

            window.setTimeout(() => {
                if (!chatResponse) return;
                if (res.kind === "text") {
                    chatResponse.textContent = `ExpertBot: ${res.text}`;
                } else if (res.kind === "html") {
                    chatResponse.innerHTML = `<div class="alert alert-info mb-0 p-3"><strong>ExpertBot:</strong><br>${res.html}</div>`;
                } else {
                    chatResponse.textContent =
                        "¿Podrías reformular? Usa /ayuda para ver opciones.";
                }
                if (chatInput) chatInput.value = "";
                setBusy(false);
            }, 600 + Math.random() * 600);
        });

        // Voz
        if (opt.enableVoice) {
            const SpeechReco = window.SpeechRecognition || window.webkitSpeechRecognition;
            if (SpeechReco && micBtn) {
                const reco = new SpeechReco();
                reco.lang = opt.voiceLang || "es-ES";
                reco.continuous = false;
                reco.interimResults = false;
                micBtn.addEventListener("click", () => {
                    try {
                        reco.start();
                        micBtn.innerHTML = '<i class="mdi mdi-microphone-off text-danger"></i>';
                        micBtn.disabled = true;
                        showSpeech("Te escucho…", 2000);
                    } catch { }
                });
                reco.onresult = (e) => {
                    const txt = e.results?.[0]?.[0]?.transcript;
                    if (txt && chatInput) chatInput.value = txt;
                };
                reco.onend = () => {
                    micBtn.innerHTML = '<i class="mdi mdi-microphone"></i>';
                    micBtn.disabled = false;
                    chatInput?.focus();
                };
                reco.onerror = () => {
                    micBtn.innerHTML =
                        '<i class="mdi mdi-microphone-alert text-warning"></i>';
                    micBtn.disabled = false;
                    showSpeech("No pude escucharte bien", 2000);
                };
            } else if (micBtn) {
                micBtn.disabled = true;
                micBtn.title = "Tu navegador no soporta reconocimiento de voz";
            }
        }

        // Botones banner tour
        startTourBtn?.addEventListener("click", () => {
            if (tourSuggestion) tourSuggestion.style.display = "none";
            closeDialog();
            window.setTimeout(() => startTour(true), 250);
        });
        dismissTourBtn?.addEventListener("click", () => {
            localStorage.setItem(KEYS.tourDismissed, "1");
            if (tourSuggestion) tourSuggestion.style.display = "none";
            showSpeech("Perfecto, cuando quieras lo vemos.", 2600);
        });

        // Hotkey reset
        document.addEventListener("keydown", (e) => {
            const hk = opt.resetHotkey;
            if (
                !!hk &&
                !!hk.ctrl === e.ctrlKey &&
                !!hk.alt === e.altKey &&
                (hk.key || "").toLowerCase() === e.key.toLowerCase()
            ) {
                localStorage.removeItem(KEYS.tourDone);
                localStorage.removeItem(KEYS.tourDismissed);
                showSpeech("Tour reiniciado. Usa /tour o haz clic en mí.", 3800);
            }
        });

        // API pública
        return {
            startTour: (force, which) => startTour(!!force, which || null),
            open: openDialog,
            close: closeDialog
        };
    }

    // Expose
    return { init };
});
  
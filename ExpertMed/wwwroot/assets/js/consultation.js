document
    .getElementById("selectDiagnosis")
    .addEventListener("click", function () {
        const select = document.getElementById("DiagnosisConsultation");
        const opt = select.options[select.selectedIndex];
        if (!opt.value) return;

        const diagnosisId = opt.value;
        const diagnosisName = opt.getAttribute("data-name") || opt.text;

        const tableBody = document.querySelector("#selectedDiagnosesTable tbody");
        const row = document.createElement("tr");

        // **Este es el cambio clave**:
        row.dataset.id = diagnosisId;

        // Columna visible con el nombre
        const nameCell = document.createElement("td");
        nameCell.textContent = diagnosisName;

        // Tus checkboxes
        const presCell = document.createElement("td");
        presCell.innerHTML = `<input type="checkbox" name="presumptive_${diagnosisId}">`;
        const defCell = document.createElement("td");
        defCell.innerHTML = `<input type="checkbox" name="definitive_${diagnosisId}">`;

        // Botón de eliminar
        const actionCell = document.createElement("td");
        actionCell.innerHTML = `
      <button type="button" 
              class="btn btn-outline-danger btn-icon" 
              onclick="removeDiagnosisRow(this)">
        <i class="ri-delete-bin-5-line"></i>
      </button>`;

        row.append(nameCell, presCell, defCell, actionCell);
        tableBody.appendChild(row);

        isFormChanged = true;  // marca el formulario como "cambiado" para que auto-save lo pille
    });


// Función para eliminar la fila
function removeDiagnosisRow(button) {
    const row = button.closest("tr");
    row.remove();
}

// Selección de medicamento desde dropdown
// Medicamentos
document.getElementById("selectMedications").addEventListener("click", function () {
    const select = document.getElementById("MedicationsConsultation");
    const selectedOption = select.options[select.selectedIndex];
    if (!selectedOption || !selectedOption.value) return;

    const medicationsId = selectedOption.value;
    const medicationsName = selectedOption.getAttribute("data-name") || selectedOption.innerText;
    const tableBody = document.querySelector("#selectedMedicationsTable tbody");
    const row = document.createElement("tr");

    // Asignamos data-id para poder leerlo luego con tr.dataset.id
    row.dataset.id = medicationsId;

    row.innerHTML = `
        <td hidden>${medicationsId}</td>
        <td>${medicationsName}</td>
        <td><input type="number" name="amount_${medicationsId}" id="amount_${medicationsId}"
                   class="form-control medication-amount" min="1"></td>
        <td><input type="text"   name="observation_${medicationsId}" id="observation_${medicationsId}"
                   class="form-control medication-observation"></td>
        <td>
            <button type="button" class="btn btn-outline-warning btn-sm add-favorite"
                    title="Guardar como favorito">
                <i class="mdi mdi-star-outline"></i>
            </button>
            <button type="button" class="btn btn-outline-danger btn-sm"
                    onclick="removeMedicationsRow(this)">
                <i class="ri-delete-bin-5-line"></i>
            </button>
        </td>
    `;
    tableBody.appendChild(row);
});


// Guardar medicamento como favorito
document.addEventListener("click", function (event) {
    if (!event.target.closest(".add-favorite")) return;

    const button = event.target.closest(".add-favorite");
    const row = button.closest("tr");

    if (!row) {
        console.warn("No se encontró la fila del medicamento (tr).");
        return;
    }

    // CAMBIO: Usar dataset.id en lugar de getAttribute("data-medication-id")
    const medicationsId = row.dataset.id;  // ← CORRECCIÓN AQUÍ
    const amount = row.querySelector(".medication-amount")?.value || "";
    const observation = row.querySelector(".medication-observation")?.value || "";

    const medicoInput = document.getElementById("medicoId");
    if (!medicoInput || !medicoInput.value || !medicationsId) {
        Swal.fire("Error", "Faltan datos para guardar el favorito.", "error");
        return;
    }

    const usersId = parseInt(medicoInput.value);

    fetch(addFavoriteUrl, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            usersId,
            medicationsId: parseInt(medicationsId),
            cantidad: amount || null,
            observacion: observation || null
        })
    })
        .then(res => {
            if (!res.ok) {
                return res.json().then(err => Promise.reject(err));
            }
            return res.json();
        })
        .then(res => {
            Swal.fire("Guardado", res.message || "Medicamento agregado a favoritos.", "success");
            button.classList.remove("btn-outline-warning");
            button.classList.add("btn-warning");
            button.innerHTML = `<i class="mdi mdi-star"></i>`;
            button.disabled = true;
        })
        .catch(err => {
            console.error("Error al guardar favorito:", err);
            const errorMsg = err.message || "No se pudo guardar el favorito.";
            Swal.fire("Error", errorMsg, "error");
        });
});
// Cargar favoritos en el offcanvas
document.addEventListener("DOMContentLoaded", function () {
    const medicoId = document.getElementById("medicoId").value;

    fetch(`${getFavoritesUrl}?userId=${medicoId}`)
        .then(res => res.ok ? res.json() : Promise.reject(res))
        .then(data => {
            const list = document.getElementById("favoritosList");
            list.innerHTML = "";

            if (data.length === 0) {
                list.innerHTML = '<li class="list-group-item text-muted">Sin favoritos</li>';
                return;
            }

            data.forEach(fav => {
                const li = document.createElement("li");
                li.className = "list-group-item";
                li.innerHTML = `
                    <div class="d-flex justify-content-between align-items-center w-100">
                        <div class="me-2">
                            <strong>${fav.medicationName}</strong><br>
                            <small><strong>Cantidad:</strong> ${fav.cantidad || "-"} | <strong>Obs:</strong> ${fav.observacion || "-"}</small>
                        </div>
                        <div class="btn-group">
                            <button class="btn btn-sm btn-success me-1" onclick="insertarFavorito(${fav.medicationId}, '${fav.medicationName}', '${fav.cantidad || ""}', '${fav.observacion || ""}')">
                                <i class="mdi mdi-plus"></i>
                            </button>
                            <button class="btn btn-sm btn-outline-danger" onclick="eliminarFavorito(${fav.medicationId})">
                                <i class="mdi mdi-trash-can-outline"></i>
                            </button>
                        </div>
                    </div>
                `;
                list.appendChild(li);
            });
        })
        .catch(err => {
            console.error(err);
            Swal.fire("Error", "No se pudieron cargar los favoritos.", "error");
        });
});

// Buscador de favoritos
document.getElementById("buscarFavorito").addEventListener("input", function () {
    const filtro = this.value.toLowerCase();
    const items = document.querySelectorAll("#favoritosList li");

    items.forEach(item => {
        const texto = item.innerText.toLowerCase();
        item.style.display = texto.includes(filtro) ? "" : "none";
    });
});

// Insertar favorito desde el offcanvas
function insertarFavorito(medicationsId, medicationsName, cantidad, observacion) {
    const tableBody = document.querySelector("#selectedMedicationsTable tbody");

    if (document.querySelector(`[data-id="${medicationsId}"]`)) {
        Swal.fire("Aviso", "El medicamento ya está en la tabla.", "info");
        return;
    }

    const row = document.createElement("tr");
    row.dataset.id = medicationsId;   // ✅ usar data-id consistente

    row.innerHTML = `
        <td hidden>${medicationsId}</td>
        <td>${medicationsName}</td>
        <td><input type="number" value="${cantidad}" class="form-control medication-amount" min="1"></td>
        <td><input type="text" value="${observacion}" class="form-control medication-observation"></td>
        <td>
            <button type="button" class="btn btn-warning btn-sm" disabled title="Ya es favorito">
                <i class="mdi mdi-star"></i>
            </button>
            <button type="button" class="btn btn-outline-danger btn-sm" onclick="removeMedicationsRow(this)">
                <i class="ri-delete-bin-5-line"></i>
            </button>
        </td>
    `;

    tableBody.appendChild(row);

    const canvas = bootstrap.Offcanvas.getInstance(document.getElementById('offcanvasFavoritos'));
    if (canvas) canvas.hide();
}

// Eliminar favorito
function eliminarFavorito(medicationsId) {
    const medicoId = document.getElementById("medicoId").value;
    if (!medicoId || !medicationsId) return;

    Swal.fire({
        title: "¿Eliminar favorito?",
        text: "Esta acción no se puede deshacer.",
        icon: "warning",
        showCancelButton: true,
        confirmButtonText: "Sí, eliminar",
        cancelButtonText: "Cancelar"
    }).then((result) => {
        if (result.isConfirmed) {
            fetch(`${deleteFavoriteUrl}?userId=${medicoId}&medicationId=${medicationsId}`, {
                method: "DELETE"
            })
                .then(res => res.ok ? res.json() : Promise.reject(res))
                .then(res => {
                    Swal.fire("Eliminado", res.message || "Favorito eliminado", "success");
                    const li = document.querySelector(`#favoritosList li button[onclick*='${medicationsId}']`)?.closest("li");
                    if (li) li.remove();
                })
                .catch(err => {
                    console.error(err);
                    Swal.fire("Error", "No se pudo eliminar el favorito.", "error");
                });
        }
    });
}

// Eliminar fila de la tabla principal
function removeMedicationsRow(button) {
    const row = button.closest("tr");
    row.remove();
}


// Imágenes
document.getElementById("selectImages").addEventListener("click", function () {
    const select = document.getElementById("ImagesConsultation");
    const selectedOption = select.options[select.selectedIndex];
    const observacionGeneral = document.getElementById("imagesGeneralObservation")?.value.trim() || "";
    if (!selectedOption || !selectedOption.value) return;

    const imagesId = selectedOption.value;
    const imagesName = selectedOption.getAttribute("data-name") || selectedOption.innerText;
    const tableBody = document.querySelector("#selectedImagesTable tbody");
    const row = document.createElement("tr");

    // Data-id unificado
    row.dataset.id = imagesId;

    row.innerHTML = `
        <td hidden>${imagesId}</td>
        <td>${imagesName}</td>
        <td><input type="number" name="amount_${imagesId}" id="amount_${imagesId}"
                   class="form-control" min="1"></td>
        <td><input type="hidden" name="observation_${imagesId}"
                   id="observation_${imagesId}"
                   value="${observacionGeneral}"></td>
        <td>
            <button type="button" class="btn btn-outline-danger btn-icon waves-effect waves-light"
                    onclick="removeImagesRow(this)">
                <i class="ri-delete-bin-5-line"></i>
            </button>
        </td>
    `;
    tableBody.appendChild(row);
});


// Función para eliminar la fila
function removeImagesRow(button) {
    const row = button.closest("tr");
    row.remove();
}

// Laboratorios
document.getElementById("selectLaboratories").addEventListener("click", function () {
    const select = document.getElementById("LaboratoriesConsultation");
    const selectedOption = select.options[select.selectedIndex];
    const observacionGeneral = document.getElementById("laboratoriesGeneralObservation")?.value.trim() || "";
    if (!selectedOption || !selectedOption.value) return;

    const laboratoriesId = selectedOption.value;
    const laboratoriesName = selectedOption.getAttribute("data-name") || selectedOption.innerText;
    const tableBody = document.querySelector("#selectedLaboratoriesTable tbody");
    const row = document.createElement("tr");

    // Mismo data-id
    row.dataset.id = laboratoriesId;

    row.innerHTML = `
        <td hidden>${laboratoriesId}</td>
        <td>${laboratoriesName}</td>
        <td><input type="number" name="amount_${laboratoriesId}"
                   id="amount_${laboratoriesId}" value="1"
                   class="form-control" min="1"></td>
        <td><input type="hidden" name="observation_${laboratoriesId}"
                   id="observation_${laboratoriesId}"
                   value="${observacionGeneral}"></td>
        <td>
            <button type="button" class="btn btn-outline-danger btn-icon waves-effect waves-light"
                    onclick="removeLaboratoriesRow(this)">
                <i class="ri-delete-bin-5-line"></i>
            </button>
        </td>
    `;
    tableBody.appendChild(row);
});


// Función para eliminar la fila
function removeLaboratoriesRow(button) {
    const row = button.closest("tr");
    row.remove();
}


// Sincronizar observación general de LABORATORIOS con las filas
document.getElementById("laboratoriesGeneralObservation").addEventListener("input", function (e) {
    const newValue = e.target.value.trim();
    document.querySelectorAll('#selectedLaboratoriesTable input[id^="observation_"]').forEach(input => {
        input.value = newValue;
    });
});

// Sincronizar observación general de IMÁGENES con las filas
document.getElementById("imagesGeneralObservation").addEventListener("input", function (e) {
    const newValue = e.target.value.trim();
    document.querySelectorAll('#selectedImagesTable input[id^="observation_"]').forEach(input => {
        input.value = newValue;
    });
});


//Speech
var recognition;
var recognizing = false;

function toggleDictation(textareaId, iconId) {
    if (recognizing) {
        stopDictation(iconId);
    } else {
        startDictation(textareaId, iconId);
    }
}

function startDictation(textareaId, iconId) {
    if (window.hasOwnProperty("webkitSpeechRecognition")) {
        recognition = new webkitSpeechRecognition();

        recognition.continuous = true; // Permite que la grabación sea continua
        recognition.interimResults = false;

        recognition.lang = "es-ES"; // Cambia el idioma según sea necesario

        recognition.onstart = function () {
            recognizing = true;
            updateIconState(iconId);
            console.log("Reconocimiento de voz iniciado. Por favor, hable.");
        };

        recognition.onresult = function (event) {
            const newText = event.results[event.results.length - 1][0].transcript;
            document.getElementById(textareaId).value += " " + newText; // Concatena al texto existente
        };

        recognition.onerror = function (event) {
            console.error("Error en el reconocimiento de voz: ", event.error);
        };

        recognition.onend = function () {
            recognizing = false;
            updateIconState(iconId);
            console.log("El reconocimiento de voz ha finalizado.");
        };

        recognition.start();
    } else {
        alert("Tu navegador no soporta el reconocimiento de voz.");
    }
}

function stopDictation(iconId) {
    if (recognition && recognizing) {
        recognizing = false;
        recognition.stop();
        updateIconState(iconId);
        console.log("Reconocimiento de voz detenido.");
    }
}

function updateIconState(iconId) {
    var icon = document.getElementById(iconId);

    if (recognizing) {
        icon.classList.remove("ri-mic-fill");
        icon.classList.add("ri-mic-off-fill");
    } else {
        icon.classList.remove("ri-mic-off-fill");
        icon.classList.add("ri-mic-fill");
    }
}

// Asignar eventos de clic a los iconos
document
    .getElementById("dictationIcon1")
    .addEventListener("click", function () {
        toggleDictation("consultation_personalbackground", "dictationIcon1");
    });

document
    .getElementById("dictationIcon2")
    .addEventListener("click", function () {
        toggleDictation("consultation_disease", "dictationIcon2");
    });

document
    .getElementById("dictationIcon3")
    .addEventListener("click", function () {
        toggleDictation("consultation_treatmentplan", "dictationIcon3");
    });

document
    .getElementById("dictationIcon4")
    .addEventListener("click", function () {
        toggleDictation("consultation_nonpharmacologycal", "dictationIcon4");
    });

document
    .getElementById("dictationIcon5")
    .addEventListener("click", function () {
        toggleDictation("consultation_warningsings", "dictationIcon5");
    });
// Asignar eventos de clic al icono
document
    .getElementById("dictationIcon6")
    .addEventListener("click", function () {
        toggleDictation("consultation_observation", "dictationIcon6");
    });

//Preecion Arterial
document
    .getElementById("bloodPressureInput")
    .addEventListener("input", function (e) {
        let value = e.target.value.replace(/\D/g, ""); // Elimina cualquier caracter que no sea un dígito

        if (value.length > 3) {
            value = value.slice(0, 3) + "/" + value.slice(3); // Inserta el '/'
        }

        e.target.value = value; // Actualiza el campo de entrada con el nuevo valor

        // Opcional: Si deseas actualizar los campos ocultos para diastólica y sistólica
        if (value.length >= 5) {
            document.getElementById("consultation_bloodpresuredDIS").value =
                value.slice(4, 6);
            document.getElementById("consultation_bloodpressuredAS").value =
                value.slice(0, 3);
        } else {
            document.getElementById("consultation_bloodpresuredDIS").value = "";
            document.getElementById("consultation_bloodpressuredAS").value =
                value.slice(0, 3);
        }
    });

document.addEventListener("DOMContentLoaded", function () {
    const tabla = document.getElementById("procedimientosTable").querySelector("tbody");
    const addBtn = document.getElementById("addProcedureBtn");

    addBtn.addEventListener("click", function () {
        const rowCount = tabla.rows.length;
        const newRow = document.createElement("tr");
        newRow.innerHTML = `
                <td><input type="text" name="Procedures[${rowCount}].procedure_name" class="form-control" /></td>
                <td><input type="date" name="Procedures[${rowCount}].procedure_date" class="form-control" /></td>
                <td class="text-center"><button type="button" class="btn btn-sm btn-danger removeRow">−</button></td>
            `;
        tabla.appendChild(newRow);
    });

    tabla.addEventListener("click", function (e) {
        if (e.target.classList.contains("removeRow")) {
            const row = e.target.closest("tr");
            row.remove();
            renumerarProcedimientos();
        }
    });

    function renumerarProcedimientos() {
        [...tabla.rows].forEach((row, index) => {
            row.querySelectorAll("input").forEach(input => {
                if (input.name.includes("procedure_name")) {
                    input.name = `Procedures[${index}].procedure_name`;
                } else if (input.name.includes("procedure_date")) {
                    input.name = `Procedures[${index}].procedure_date`;
                }
            });
        });
    }
});


// Helper functions
const $ = id => document.getElementById(id);
const getValue = (id, defaultValue = null) => $(id)?.value ?? defaultValue;
const getInt = (id, defaultValue = 0) =>
    parseInt($(id)?.value, 10) || defaultValue;
const getChecked = id => {
    const el = $(id);
    return el != null ? el.checked : null;
};
const debounce = (fn, ms = 300) => {
    let timer;
    return (...args) => {
        clearTimeout(timer);
        timer = setTimeout(() => fn.apply(this, args), ms);
    };
};

// Estado global
let autoSaveInterval = null;
let autoSaveController = null;
let consultationId = null;
let isFormChanged = false;
let isSaving = false;

/**
 * Construye el DTO de la consulta.
 * @param {boolean} isFinal – true para envío definitivo, false para auto-guardado
 */
function getFormData(isFinal = false) {
    const mapTable = (selector, mapper) =>
        Array.from(document.querySelectorAll(selector + " tbody tr")).map(mapper);

    // 1) Obtener el consultationId del input hidden
    const hiddenInput = document.getElementById("consultationId");
    let currentConsultationId = null;
    console.log(hiddenInput);
    if (hiddenInput && hiddenInput.value) {
        currentConsultationId = hiddenInput.value;
        console.log("Usando consultationId del input hidden:", currentConsultationId);
    } else {
        // Si no se encuentra, usar el valor de la variable global
        currentConsultationId = consultationId;
        console.log("Usando consultationId de la variable global:", currentConsultationId);
    }

    // 1) Construye primero el DTO con los campos simples
    const dto = {
        ConsultationId: currentConsultationId, 
        ConsultationDate: getValue("consultation_date"),
        ConsultationUsercreate: getInt("consultation_usercreate", null),
        ConsultationPatient: getInt("consultation_patient", null),
        ConsultationSpeciality: getInt("consultation_speciality", null),
        ConsultationHistoryclinic: getValue("consultation_historyclinic"),
        ConsultationReason: getValue("consultation_reason"),
        ConsultationDisease: getValue("consultation_disease"),
        ConsultationFamiliaryname: getValue("consultation_familiaryname"),
        ConsultationWarningsings: getValue("consultation_warningsings"),
        ConsultationNonpharmacologycal: getValue("consultation_nonpharmacologycal"),
        ConsultationFamiliarytype: getInt("consultation_familiarytype", null),
        ConsultationFamiliaryphone: getValue("consultation_familiaryphone"),
        ConsultationTemperature: getValue("consultation_temperature", "0.0"),
        ConsultationRespirationrate: getValue("consultation_respirationrate", "0"),
        ConsultationBloodpressuredAs: getValue("consultation_bloodpressuredAS", "000"),
        ConsultationBloodpresuredDis: getValue("consultation_bloodpresuredDIS", "000"),
        ConsultationPulse: getValue("consultation_pulse", "0"),
        ConsultationWeight: getValue("consultation_weight", "0.0"),
        ConsultationSize: getValue("consultation_size", "0.0"),
        ConsultationTreatmentplan: getValue("consultation_treatmentplan"),
        ConsultationObservation: getValue("consultation_observation"),
        ConsultationPersonalbackground: getValue("consultation_personalbackground"),
        ConsultationDisablilitydays: getInt("consultation_disablilitydays"),
        ConsultationEvolutionNotes: getValue("consultation_evolution_notes"),
        ConsultationTherapies: getValue("consultation_therapies"),
        ConsultationType: getInt("consultation_type", null),
        ConsultationStatus: getInt("consultation_status", 1),
        ConsultationHasdisease: getChecked("cert_tiene_enfermedad") ?? false,
        ConsultationDiseaseobservation: getValue("cert_observacion"),
        ConsultationContingencytype: getValue("cert_tipo_contingencia"),
        ConsutationHasSymptoms: getChecked("cert_tiene_sintomas") ?? false,

        // Flag para el SP
        ConsultationIsFinal: isFinal
    };

    // 2) Alergias directo desde el <select multiple>
    const allergyOpts = Array.from(
        document.getElementById("allergiesSelect").selectedOptions
    );
    dto.AllergiesConsultations = allergyOpts.map(opt => ({
        AllergiesCatalogid: parseInt(opt.value, 10),
        AllergiesObservation: "",
        AllergiesStatus: 1
    }));

    // 3) Cirugías directo desde el <select multiple>
    const surgeryOpts = Array.from(
        document.getElementById("surgeriesSelect").selectedOptions
    );
    dto.SurgeriesConsultations = surgeryOpts.map(opt => ({
        SurgeriesCatalogid: parseInt(opt.value, 10),
        SurgeriesObservation: "",
        SurgeriesStatus: 1
    }));

    // 4) El resto de TVPs como antes, usando mapTable si usas tablas dinámicas
    // Medicamentos
    dto.MedicationsConsultations = Array.from(
        document.querySelectorAll("#selectedMedicationsTable tbody tr")
    ).map(tr => ({
        // Aquí debería haberse asignado tr.dataset.id al crear la fila
        MedicationsMedicationsid: parseInt(tr.dataset.id, 10),
        MedicationsAmount: tr.querySelector('input[name^="amount_"]')?.value ?? null,
        MedicationsObservation: tr.querySelector('input[name^="observation_"]')?.value ?? null,
        MedicationsStatus: 1
    }));

    // Laboratorios
    dto.LaboratoriesConsultations = Array.from(
        document.querySelectorAll("#selectedLaboratoriesTable tbody tr")
    ).map(tr => ({
        LaboratoriesLaboratoriesid: parseInt(tr.dataset.id, 10),
        LaboratoriesAmount: tr.querySelector('input[name^="amount_"]')?.value ?? "",
        LaboratoriesObservation: tr.querySelector('input[name^="observation_"]')?.value ?? null,
        LaboratoriesStatus: 1
    }));

    // Imágenes
    dto.ImagesConsultations = Array.from(
        document.querySelectorAll("#selectedImagesTable tbody tr")
    ).map(tr => ({
        ImagesImagesid: parseInt(tr.dataset.id, 10),
        ImagesAmount: tr.querySelector('input[name^="amount_"]')?.value ?? "",
        ImagesObservation: tr.querySelector('input[name^="observation_"]')?.value ?? null,
        ImagesStatus: 1
    }));

    // Diagnósticos (para referencia)
    dto.DiagnosisConsultations = Array.from(
        document.querySelectorAll(
            "#selectedDiagnosesTable tbody tr[data-id]"
        )
    ).map(tr => {
        const id = tr.dataset.id;
        return {
            DiagnosisDiagnosisid: parseInt(id, 10),
            DiagnosisPresumptive: tr.querySelector(`input[name="presumptive_${id}"]`)
                ?.checked ?? false,
            DiagnosisDefinitive: tr.querySelector(`input[name="definitive_${id}"]`)
                ?.checked ?? false,
            DiagnosisObservation: null,
            DiagnosisStatus: 1
        };
    });


    dto.Procedures = mapTable("#procedimientosTable", tr => ({
        procedure_name:
            tr.querySelector('input[name*="procedure_name"]')?.value ?? null,
        procedure_date:
            tr.querySelector('input[name*="procedure_date"]')?.value ?? null
    }));

    // Órganos y sistemas
    const organDefs = [
        { key: "Organsenses", obs: "organssystems_organsenses_Obs" },
        { key: "Respiratory", obs: "organssystems_respiratory_obs" },
        { key: "Cardiovascular", obs: "organssystems_cardiovascular_obs" },
        { key: "Digestive", obs: "organssystems_digestive_obs" },
        { key: "Genital", obs: "organssystems_genital_obs" },
        { key: "Urinary", obs: "organssystems_urinary_obs" },
        { key: "SkeletalM", obs: "organssystems_skeletal_m_obs" },
        { key: "Endocrine", obs: "organssystems_endocrine_obs" },
        { key: "Lymphatic", obs: "organssystems_lymphatic_obs" },
        { key: "Nervous", obs: "organssystems_nervous_obs" }
    ];

    dto.OrgansSystem = {};
    organDefs.forEach(({ key, obs }) => {
        if (key === "Endocrine") {
            dto.OrgansSystem["OrganssystemsEndrocrine"] = getChecked("organssystems_endocrine");
            dto.OrgansSystem["OrganssystemsEndocrine"] = getValue(obs);
        } else {
            dto.OrgansSystem[`Organssystems${key}`] = getChecked(`organssystems_${key.toLowerCase()}`);
            dto.OrgansSystem[`Organssystems${key}Obs`] = getValue(obs);
        }
    });

    // Examen físico completo
    dto.PhysicalExamination = {
        PhysicalexaminationHead: getChecked("physicalexamination_head"),
        PhysicalexaminationHeadObs: getValue("physicalexamination_head_obs"),
        PhysicalexaminationNeck: getChecked("physicalexamination_neck"),
        PhysicalexaminationNeckObs: getValue("physicalexamination_neck_obs"),
        PhysicalexaminationChest: getChecked("physicalexamination_chest"),
        PhysicalexaminationChestObs: getValue("physicalexamination_chest_obs"),
        PhysicalexaminationAbdomen: getChecked("physicalexamination_abdomen"),
        PhysicalexaminationAbdomenObs: getValue("physicalexamination_abdomen_obs"),
        PhysicalexaminationPelvis: getChecked("physicalexamination_pelvis"),
        PhysicalexaminationPelvisObs: getValue("physicalexamination_pelvis_obs"),
        PhysicalexaminationLimbs: getChecked("physicalexamination_limbs"),
        PhysicalexaminationLimbsObs: getValue("physicalexamination_limbs_obs")
    };

    // Antecedentes familiares completo
    dto.FamiliaryBackground = {
        FamiliaryBackgroundHeartdisease: getChecked("familiary_background_heartdisease"),
        FamiliaryBackgroundHeartdiseaseObservation: getValue("familiary_background_heartdisease_observation"),
        FamiliaryBackgroundRelatshcatalogHeartdisease: getInt("familiary_background_relatshcatalog_heartdisease", null),
        FamiliaryBackgroundDiabetes: getChecked("familiary_background_diabetes"),
        FamiliaryBackgroundDiabetesObservation: getValue("familiary_background_diabetes_observation"),
        FamiliaryBackgroundRelatshcatalogDiabetes: getInt("familiary_background_relatshcatalog_diabetes", null),
        FamiliaryBackgroundDxcardiovascular: getChecked("familiary_background_dxcardiovascular"),
        FamiliaryBackgroundDxcardiovascularObservation: getValue("familiary_background_dxcardiovascular_observation"),
        FamiliaryBackgroundRelatshcatalogDxcardiovascular: getInt("familiary_background_relatshcatalog_dxcardiovascular", null),
        FamiliaryBackgroundHypertension: getChecked("familiary_background_hypertension"),
        FamiliaryBackgroundHypertensionObservation: getValue("familiary_background_hypertension_observation"),
        FamiliaryBackgroundRelatshcatalogHypertension: getInt("familiary_background_relatshcatalog_hypertension", null),
        FamiliaryBackgroundCancer: getChecked("familiary_background_cancer"),
        FamiliaryBackgroundCancerObservation: getValue("familiary_background_cancer_observation"),
        FamiliaryBackgroundRelatshcatalogCancer: getInt("familiary_background_relatshcatalog_cancer", null),
        FamiliaryBackgroundTuberculosis: getChecked("familiary_background_tuberculosis"),
        FamiliaryBackgroundTuberculosisObservation: getValue("familiary_background_tuberculosis_observation"),
        FamiliaryBackgroundRelatshcatalogTuberculosis: getInt("familiary_background_relatshcatalog_tuberculosis", null),
        FamiliaryBackgroundDxmental: getChecked("familiary_background_dxmental"),
        FamiliaryBackgroundDxmentalObservation: getValue("familiary_background_dxmental_observation"),
        FamiliaryBackgroundRelatshcatalogDxmental: getInt("familiary_background_relatshcatalog_dxmental", null),
        FamiliaryBackgroundDxinfectious: getChecked("familiary_background_dxinfectious"),
        FamiliaryBackgroundDxinfectiousObservation: getValue("familiary_background_dxinfectious_observation"),
        FamiliaryBackgroundRelatshcatalogDxinfectious: getInt("familiary_background_relatshcatalog_dxinfectious", null),
        FamiliaryBackgroundMalformation: getChecked("familiary_background_malformation"),
        FamiliaryBackgroundMalformationObservation: getValue("familiary_background_malformation_observation"),
        FamiliaryBackgroundRelatshcatalogMalformation: getInt("familiary_background_relatshcatalog_malformation", null),
        FamiliaryBackgroundOther: getChecked("familiary_background_other"),
        FamiliaryBackgroundOtherObservation: getValue("familiary_background_other_observation"),
        FamiliaryBackgroundRelatshcatalogOther: getInt("familiary_background_relatshcatalog_other", null)
    };

    return dto;
}

// Indicador visual
function showSaveIndicator(status) {
    let indicator = $("autosave-indicator");
    if (!indicator) {
        indicator = document.createElement("div");
        indicator.id = "autosave-indicator";
        Object.assign(indicator.style, {
            position: "fixed",
            top: "20px",
            right: "20px",
            padding: "10px 15px",
            borderRadius: "5px",
            color: "#fff",
            fontWeight: "bold",
            zIndex: 9999,
            transition: "opacity 0.3s"
        });
        document.body.appendChild(indicator);
    }
    const states = {
        saving: { text: "💾 Guardando...", bg: "#17a2b8" },
        saved: { text: "✅ Guardado automáticamente", bg: "#28a745" },
        error: { text: "❌ Error al guardar", bg: "#dc3545" }
    };
    const { text, bg } = states[status] || states.error;
    indicator.textContent = text;
    indicator.style.backgroundColor = bg;
    indicator.style.opacity = "1";
    if (status === "saved") setTimeout(() => { indicator.style.opacity = "0.7"; }, 2000);
}

// Auto-guardado con AbortController
async function autoSaveConsultation() {
    if (isSaving || !isFormChanged) return;
    isSaving = true;
    showSaveIndicator("saving");
    if (autoSaveController) autoSaveController.abort();
    autoSaveController = new AbortController();

    try {
        const dto = getFormData(false);
        console.log("→ Enviando auto-save DTO", dto);
        const body = JSON.stringify(dto);
        console.log("—> RAW fetch body:", body);
        const res = await fetch(consultaUrl, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(dto),
            signal: autoSaveController.signal
        });
        const json = await res.json();
        console.log("← Respuesta auto-save", json);

        if (res.ok && typeof json.consultationId === "number") {
            consultationId = json.consultationId;
            console.log("★ consultationId asignado a", consultationId);
            isFormChanged = false;
            showSaveIndicator("saved");
        } else {
            console.error("Error auto-save:", json);
            showSaveIndicator("error");
        }
    } catch (err) {
        if (err.name !== "AbortError") {
            console.error("Network error:", err);
            showSaveIndicator("error");
        }
    } finally {
        isSaving = false;
    }
}

// Detectar cambios (debounced)
function setupChangeDetection() {
    const form = $("consultationForm");
    if (!form) return;
    const markChanged = () => { isFormChanged = true; };
    form.addEventListener("input", debounce(markChanged));
    form.addEventListener("change", debounce(markChanged));
    ["#selectedMedicationsTable", "#selectedLaboratoriesTable",
        "#selectedImagesTable", "#selectedDiagnosesTable", "#procedimientosTable"
    ].forEach(sel => {
        const table = document.querySelector(sel);
        if (table) {
            table.addEventListener("input", debounce(markChanged));
            table.addEventListener("change", debounce(markChanged));
        }
    });
}

// Iniciar / detener auto-guardado
function startAutoSave() {
    setupChangeDetection();
    autoSaveInterval = setInterval(autoSaveConsultation, 15000);
    console.log("Auto-guardado iniciado cada 15 s");
}
function stopAutoSave() {
    clearInterval(autoSaveInterval);
    autoSaveInterval = null;
    if (autoSaveController) autoSaveController.abort();
    console.log("Auto-guardado detenido");
}

// Envío manual (final)
async function submitFormManually() {
    stopAutoSave();
    try {
        const dto = getFormData(true);  // <-- envío definitivo: isFinal = true
        const res = await fetch(consultaUrl, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(dto)
        });
        const json = await res.json();
        if (res.ok) {
            window.location.href = redirectUrl;
        } else {
            console.error("Error envío manual:", json);
            startAutoSave();
        }
    } catch (err) {
        console.error("Network error:", err);
        startAutoSave();
    }
}
// ============================================
// FUNCIONES DE PRE-CARGA PARA EDICIÓN
// ============================================

function preloadDiagnosis(id, name, presumptive, definitive) {
    const tableBody = document.querySelector("#selectedDiagnosesTable tbody");
    if (!tableBody) return;

    const row = document.createElement("tr");
    row.dataset.id = id;

    const nameCell = document.createElement("td");
    nameCell.textContent = name;

    const presCell = document.createElement("td");
    presCell.innerHTML = `<input type="checkbox" name="presumptive_${id}" ${presumptive ? 'checked' : ''}>`;

    const defCell = document.createElement("td");
    defCell.innerHTML = `<input type="checkbox" name="definitive_${id}" ${definitive ? 'checked' : ''}>`;

    const actionCell = document.createElement("td");
    actionCell.innerHTML = `
        <button type="button" class="btn btn-outline-danger btn-icon" onclick="removeDiagnosisRow(this)">
            <i class="ri-delete-bin-5-line"></i>
        </button>`;

    row.append(nameCell, presCell, defCell, actionCell);
    tableBody.appendChild(row);
}

function preloadMedication(id, name, amount, observation) {
    const tableBody = document.querySelector("#selectedMedicationsTable tbody");
    if (!tableBody) return;

    const row = document.createElement("tr");
    row.dataset.id = id;

    row.innerHTML = `
        <td hidden>${id}</td>
        <td>${name}</td>
        <td><input type="number" name="amount_${id}" value="${amount || ''}" class="form-control medication-amount" min="1"></td>
        <td><input type="text" name="observation_${id}" value="${observation || ''}" class="form-control medication-observation"></td>
        <td>
            <button type="button" class="btn btn-outline-warning btn-sm add-favorite" title="Guardar como favorito">
                <i class="mdi mdi-star-outline"></i>
            </button>
            <button type="button" class="btn btn-outline-danger btn-sm" onclick="removeMedicationsRow(this)">
                <i class="ri-delete-bin-5-line"></i>
            </button>
        </td>
    `;

    tableBody.appendChild(row);
}

function preloadLaboratory(id, name, amount, observation) {
    const tableBody = document.querySelector("#selectedLaboratoriesTable tbody");
    if (!tableBody) return;

    const row = document.createElement("tr");
    row.dataset.id = id;

    row.innerHTML = `
        <td hidden>${id}</td>
        <td>${name}</td>
        <td><input type="number" name="amount_${id}" value="${amount || '1'}" class="form-control" min="1"></td>
        <td><input type="hidden" name="observation_${id}" value="${observation || ''}"></td>
        <td>
            <button type="button" class="btn btn-outline-danger btn-icon waves-effect waves-light" onclick="removeLaboratoriesRow(this)">
                <i class="ri-delete-bin-5-line"></i>
            </button>
        </td>
    `;

    tableBody.appendChild(row);
}

function preloadImage(id, name, amount, observation) {
    const tableBody = document.querySelector("#selectedImagesTable tbody");
    if (!tableBody) return;

    const row = document.createElement("tr");
    row.dataset.id = id;

    row.innerHTML = `
        <td hidden>${id}</td>
        <td>${name}</td>
        <td><input type="number" name="amount_${id}" value="${amount || ''}" class="form-control" min="1"></td>
        <td><input type="hidden" name="observation_${id}" value="${observation || ''}"></td>
        <td>
            <button type="button" class="btn btn-outline-danger btn-icon waves-effect waves-light" onclick="removeImagesRow(this)">
                <i class="ri-delete-bin-5-line"></i>
            </button>
        </td>
    `;

    tableBody.appendChild(row);
}

// Exponer las funciones globalmente
window.preloadDiagnosis = preloadDiagnosis;
window.preloadMedication = preloadMedication;
window.preloadLaboratory = preloadLaboratory;
window.preloadImage = preloadImage;
// Inicialización
document.addEventListener("DOMContentLoaded", () => {
    startAutoSave();
    const form = $("consultationForm");
    if (form) form.addEventListener("submit", e => {
        e.preventDefault();
        submitFormManually();
    });
    window.addEventListener("beforeunload", stopAutoSave);
});

// Exponer control manual
window.pauseAutoSave = stopAutoSave;
window.resumeAutoSave = startAutoSave;
window.forceAutoSave = autoSaveConsultation;

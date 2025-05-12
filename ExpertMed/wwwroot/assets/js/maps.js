const addressInput = document.getElementById('addressInput');
const suggestionsContainer = document.getElementById('suggestions');
const mapContainer = document.getElementById('map');
const mapButton = document.getElementById('mapButton');

const apiKey = 'fc26fc0381bb4bdcb433a1ba63e0dfe3'; // Reemplaza con tu API Key de Geoapify

let map; // Referencia al mapa
let marker; // Referencia al marcador
let userLocation = null; // Guardará la ubicación del usuario
let mapVisible = false; // Estado para alternar el mapa

// Obtener la ubicación del usuario
navigator.geolocation.getCurrentPosition(
    (position) => {
        userLocation = [position.coords.latitude, position.coords.longitude];
        console.log('Ubicación del usuario:', userLocation);
    },
    (error) => {
        console.error('No se pudo obtener la ubicación del usuario:', error);
        userLocation = [19.432608, -99.133209]; // Ubicación predeterminada: Ciudad de México
    }
);

// Inicializar el mapa
function initializeMap(center) {
    if (!map) {
        map = L.map('map').setView(center, 13);

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: 'Map data © <a href="https://openstreetmap.org">OpenStreetMap</a> contributors'
        }).addTo(map);

        // Evento de clic en el mapa
        map.on('click', async (e) => {
            const { lat, lng } = e.latlng;

            // Obtener dirección a partir de coordenadas
            try {
                const response = await fetch(`https://api.geoapify.com/v1/geocode/reverse?lat=${lat}&lon=${lng}&apiKey=${apiKey}`);
                const data = await response.json();

                if (data.features && data.features.length > 0) {
                    const address = data.features[0].properties.formatted;

                    // Actualizar el campo de entrada
                    addressInput.value = address;

                    // Agregar o mover el marcador
                    if (marker) {
                        marker.setLatLng([lat, lng]);
                    } else {
                        marker = L.marker([lat, lng]).addTo(map);
                    }

                    // Mostrar dirección en el marcador
                    marker.bindPopup(`<b>${address}</b>`).openPopup();
                }
            } catch (error) {
                console.error('Error al obtener dirección:', error);
            }
        });
    } else {
        map.setView(center, 13);
    }
}

// Mostrar u ocultar el mapa al hacer clic en el botón
mapButton.addEventListener('click', async () => {
    mapVisible = !mapVisible; // Alternar el estado del mapa
    if (mapVisible) {
        mapContainer.style.display = 'block';

        // Verificar si el input tiene una dirección
        const address = addressInput.value.trim();
        if (address) {
            try {
                const response = await fetch(`https://api.geoapify.com/v1/geocode/search?text=${encodeURIComponent(address)}&apiKey=${apiKey}`);
                const data = await response.json();

                if (data.features && data.features.length > 0) {
                    const [lon, lat] = data.features[0].geometry.coordinates;

                    // Centrar el mapa en la dirección y colocar el marcador
                    initializeMap([lat, lon]);
                    if (marker) {
                        marker.setLatLng([lat, lon]);
                    } else {
                        marker = L.marker([lat, lon]).addTo(map);
                    }

                    // Mostrar la dirección en el marcador
                    marker.bindPopup(`<b>${data.features[0].properties.formatted}</b>`).openPopup();
                } else {
                    console.error('No se encontró la dirección ingresada.');
                }
            } catch (error) {
                console.error('Error al buscar la dirección:', error);
            }
        } else {
            // Si no hay dirección, centrar en la ubicación del usuario
            initializeMap(userLocation || [19.432608, -99.133209]);
        }

        mapButton.innerHTML = '<i class="ri-map-pin-line"></i> Ocultar Mapa';
    } else {
        mapContainer.style.display = 'none';
        mapButton.innerHTML = '<i class="ri-map-pin-line"></i> Mostrar Mapa';
    }
});

// Autocompletado
addressInput.addEventListener('input', async () => {
    const query = addressInput.value.trim();

    if (query.length < 3) {
        suggestionsContainer.style.display = 'none';
        return;
    }

    try {
        const response = await fetch(`https://api.geoapify.com/v1/geocode/autocomplete?text=${encodeURIComponent(query)}&apiKey=${apiKey}`);
        const data = await response.json();

        suggestionsContainer.innerHTML = '';

        if (data.features && data.features.length > 0) {
            suggestionsContainer.style.display = 'block';

            data.features.forEach((feature) => {
                const suggestionItem = document.createElement('div');
                suggestionItem.classList.add('suggestion-item');
                suggestionItem.textContent = feature.properties.formatted;

                suggestionItem.addEventListener('click', () => {
                    addressInput.value = feature.properties.formatted;
                    suggestionsContainer.style.display = 'none';
                    initializeMap(feature.geometry.coordinates.reverse());
                });

                suggestionsContainer.appendChild(suggestionItem);
            });
        } else {
            suggestionsContainer.style.display = 'none';
        }
    } catch (error) {
        console.error('Error en el autocompletado:', error);
    }
});

// api.js - Gestión centralizada de peticiones al servidor

const AppointmentAPI = {
    cache: new Map(),

    /**
     * Obtiene los datos de una cita por ID (con caché)
     */
    async getById(id) {
        const cacheKey = `appointment_${id}`;

        if (this.cache.has(cacheKey)) {
            console.log(`Usando caché para cita ${id}`);
            return this.cache.get(cacheKey);
        }

        try {
            const data = await $.get(AppConfig.ENDPOINTS.APPOINTMENT_GET_BY_ID, {
                id,
                userProfile: AppConfig.PROFILE_ID
            });

            this.cache.set(cacheKey, data);
            return data;
        } catch (error) {
            ErrorHandler.handle(error, 'obtener datos de la cita');
            throw error;
        }
    },

    /**
     * Invalida el caché de una cita específica
     */
    invalidateCache(id) {
        this.cache.delete(`appointment_${id}`);
    },

    /**
     * Limpia todo el caché
     */
    clearCache() {
        this.cache.clear();
    },

    /**
     * Modifica una cita existente
     */
    async modify(appointmentData) {
        try {
            const response = await $.ajax({
                url: AppConfig.ENDPOINTS.MODIFY_APPOINTMENT,
                type: 'POST',
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                data: JSON.stringify(appointmentData)
            });

            // Invalidar caché de esta cita
            if (appointmentData.AppointmentId) {
                this.invalidateCache(appointmentData.AppointmentId);
            }

            return response;
        } catch (error) {
            ErrorHandler.handle(error, 'modificar la cita');
            throw error;
        }
    },

    /**
     * Cancela/desactiva una cita
     */
    async cancel(appointmentId) {
        try {
            const response = await fetch(AppConfig.ENDPOINTS.DESACTIVATE_APPOINTMENT, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    AppointmentId: appointmentId,
                    AppointmentModifyuser: AppConfig.USER_ID
                })
            });

            const data = await response.json();
            this.invalidateCache(appointmentId);

            return data;
        } catch (error) {
            ErrorHandler.handle(error, 'cancelar la cita');
            throw error;
        }
    },

    /**
     * Obtiene los consultorios disponibles
     */
    async getAvailableOffices(date, hour, doctorUserId) {
        try {
            const response = await $.get(AppConfig.ENDPOINTS.GET_AVAILABLE_OFFICES, {
                date,
                hour,
                doctorUserId
            });

            return response;
        } catch (error) {
            ErrorHandler.handle(error, 'cargar consultorios disponibles');
            throw error;
        }
    },

    /**
     * Obtiene las horas disponibles
     */
    async getAvailableHours(date, doctorUserId) {
        try {
            const response = await $.get(AppConfig.ENDPOINTS.GET_AVAILABLE_HOURS, {
                userId: AppConfig.USER_ID,
                date,
                doctorUserId
            });

            return response;
        } catch (error) {
            ErrorHandler.handle(error, 'cargar horas disponibles');
            throw error;
        }
    },

    /**
     * Inserta signos vitales
     */
    async insertVitalSigns(vitalSignsData) {
        try {
            const response = await fetch(AppConfig.ENDPOINTS.INSERT_VITAL_SIGNS, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(vitalSignsData)
            });

            return await response.json();
        } catch (error) {
            ErrorHandler.handle(error, 'guardar signos vitales');
            throw error;
        }
    }
};

// Hacer disponible globalmente
window.AppointmentAPI = AppointmentAPI;
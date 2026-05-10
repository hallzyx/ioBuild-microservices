import { defineStore } from "pinia";
import { ref, computed } from "vue";
import { ClientApi } from "../infrastructure/client-api.js";
import { ClientAssembler } from "../infrastructure/client.assembler.js";

const clientApi = new ClientApi();

export const useClientStore = defineStore("clients", () => {
    const clients = ref([]);
    const errors = ref([]);
    const clientsLoaded = ref(false);
    const builderId = parseInt(import.meta.env.VITE_USER_ID || '1');

    const clientsCount = computed(() => (clientsLoaded.value ? clients.value.length : 0));

    function fetchClients() {
        return clientApi
            .getAllClients()
            .then((response) => {
                clients.value = ClientAssembler.toEntitiesFromResponse(response);
                clientsLoaded.value = true;
            })
            .catch((error) => {
                errors.value.push(error);
                console.error("Error fetching clients:", error);
            });
    }

    function getClientById(id) {
        const idInt = parseInt(id);
        return clients.value.find((c) => c.id === idInt);
    }

    function addClient(client) {
        const resource = ClientAssembler.toResourceFromEntity(client);
        return clientApi
            .createClient(resource)
            .then((response) => {
                const newClient = ClientAssembler.toEntityFromResource(response.data);
                clients.value.push(newClient);
                return newClient;
            })
            .catch((error) => {
                errors.value.push(error);
                throw error;
            });
    }

    function updateClient(client) {
        const resource = ClientAssembler.toResourceFromEntity(client);
        return clientApi
            .updateClient(resource)
            .then((response) => {
                const updated = ClientAssembler.toEntityFromResource(response.data);
                const index = clients.value.findIndex((c) => c.id === updated.id);
                if (index !== -1) clients.value[index] = updated;
                return updated;
            })
            .catch((error) => {
                errors.value.push(error);
                throw error;
            });
    }

    function deleteClient(id) {
        return clientApi
            .deleteClient(id)
            .then(() => {
                const index = clients.value.findIndex((c) => c.id === id);
                if (index !== -1) clients.value.splice(index, 1);
            })
            .catch((error) => {
                errors.value.push(error);
                throw error;
            });
    }

    return {
        clients,
        errors,
        clientsLoaded,
        clientsCount,
        fetchClients,
        getClientById,
        addClient,
        updateClient,
        deleteClient
    };
});


import { BaseApi } from "../../shared/infrastructure/base-api.js";
import { BaseEndpoint } from "../../shared/infrastructure/base-endpoint.js";

const clientsEndpointPath = import.meta.env.VITE_CLIENTS_ENDPOINT_PATH || '/external_clients';

export class ClientApi extends BaseApi {
    #clientsEndpoint;

    constructor() {
        super();
        this.#clientsEndpoint = new BaseEndpoint(this, clientsEndpointPath);
    }

    async getAllClients() {
        return this.#clientsEndpoint.getAll();
    }

    async getClientsByBuilderId(builderId) {
        const response = await this.#clientsEndpoint.getAll();
        const allClients = response.data;
        // Aquí puedes filtrar por builderId si es necesario
        // Por ahora retornamos todos
        return { data: allClients };
    }

    getClientById(id) {
        return this.#clientsEndpoint.getById(id);
    }

    createClient(resource) {
        return this.#clientsEndpoint.create(resource);
    }

    updateClient(resource) {
        return this.#clientsEndpoint.update(resource.id, resource);
    }

    deleteClient(id) {
        return this.#clientsEndpoint.delete(id);
    }
}


import { Client } from "../domain/model/client.entity.js";

export class ClientAssembler {
    static toEntityFromResource(resource) {
        return new Client({ ...resource });
    }

    static toEntitiesFromResponse(response) {
        if (response.status !== 200) {
            console.error(`${response.status} - ${response.statusText}`);
            return [];
        }

        const resources = response.data || [];
        return resources.map((r) => this.toEntityFromResource(r));
    }

    static toResourceFromEntity(entity) {
        return {
            id: entity.id,
            fullName: entity.fullName,
            projectId: entity.projectId || 0,
            projectName: entity.projectName || "",
            accountStatement: entity.accountStatement,
            email: entity.email,
            phoneNumber: entity.phoneNumber || "",
            address: entity.address || ""
        };
    }
}


import { Project } from "../domain/model/project.entity.js";

export class ProjectAssembler {
    static toEntityFromResource(resource) {
        return new Project({ ...resource });
    }

    static toEntitiesFromResponse(response) {
        if (!response || (!response.status && !response.data)) {
            console.error(`${response.status} - ${response.statusText}`);
            return [];
        }

        const resources = response.data || [];
        return resources.map((r) => this.toEntityFromResource(r));
    }
}

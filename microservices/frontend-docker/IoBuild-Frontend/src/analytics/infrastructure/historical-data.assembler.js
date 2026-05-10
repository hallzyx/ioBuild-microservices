import { HistoricalDataPoint } from "../domain/model/historical-data-point.entity.js";

export class HistoricalDataAssembler {
    static toEntity(resource) {
        return new HistoricalDataPoint({
            timestamp: new Date(resource.timestamp),
            value: resource.value,
            type: resource.type
        });
    }

    static toEntityList(resources) {
        return resources.map(resource => this.toEntity(resource));
    }
}

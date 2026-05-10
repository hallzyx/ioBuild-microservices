export class Project {
    constructor({
                    id = null,
                    name = "",
                    description = "",
                    location = "",
                    totalUnits = 0,
                    occupiedUnits = 0,
                    status = "active",
                    builderId = 1,
                    createdDate = null,
                    imageUrl = "",
                }) {
        this.id = id;
        this.name = name;
        this.description = description;
        this.location = location;
        this.totalUnits = totalUnits;
        this.occupiedUnits = occupiedUnits;
        this.status = status;
        this.builderId = builderId;
        this.createdDate = createdDate;
        this.imageUrl = imageUrl;
    }
}

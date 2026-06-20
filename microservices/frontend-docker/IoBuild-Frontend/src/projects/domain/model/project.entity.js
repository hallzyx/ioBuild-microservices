const PROJECT_STATUS_LABELS = { 0: 'Planned', 1: 'On going', 2: 'On hold', 3: 'Completed', 4: 'Cancelled' };

export class Project {
    constructor({
                    id = null,
                    name = "",
                    description = "",
                    location = "",
                    totalUnits = 0,
                    occupiedUnits = 0,
                    status = "active",
                    builderId = null,
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

    // Display-only label for the numeric backend status enum. Keeps `status`
    // raw (numeric) so it round-trips correctly on update (PUT expects an int).
    get statusLabel() {
        return typeof this.status === 'number'
            ? (PROJECT_STATUS_LABELS[this.status] ?? 'Unknown')
            : this.status;
    }
}

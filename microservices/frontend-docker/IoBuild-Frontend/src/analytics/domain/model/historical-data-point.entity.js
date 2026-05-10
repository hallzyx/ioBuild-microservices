export class HistoricalDataPoint {
    constructor({
        timestamp = new Date(),
        value = 0,
        type = ''
    } = {}) {
        this.timestamp = timestamp;
        this.value = value;
        this.type = type;
    }
}

export class Device {
  constructor(id=null, name="", type="", location="", projectId=1, status = "", macAddress="") {
    this.id = id;
    this.name = name;
    this.type = type; // canonical code (e.g., temperature, energy)
    this.location = location;
    this.projectId = projectId;
    this.status = status;
    this.macAddress = macAddress;
  }

  isOnline() {
    return this.status === 'Online';
  }

  toggleStatus() {
    this.status = this.status === 'Online' ? 'Offline' : 'Online';
  }
}

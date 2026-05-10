import { Device } from '../domain/model/device.entity.js';

export class DeviceAssembler {
  static toEntity(deviceData) {
    const statuses = ['Online', 'Offline'];
    const status = deviceData.status && (deviceData.status === 'Online' || deviceData.status === 'Offline')
      ? deviceData.status
      : statuses[Math.floor(Math.random() * statuses.length)];

    return new Device(
      deviceData.id,
      deviceData.name,
      deviceData.type,
      deviceData.location,
      deviceData.projectId,
      status,
      deviceData.macAddress || ""
    );
  }

  static toEntityList(devicesData) {
    return devicesData.map(deviceData => this.toEntity(deviceData));
  }

  static toResource(device) {
    return {
      id: device.id,
      name: device.name,
      type: device.type,
      location: device.location,
      projectId: device.projectId,
      status: device.status,
      macAddress: device.macAddress
    };
  }
}

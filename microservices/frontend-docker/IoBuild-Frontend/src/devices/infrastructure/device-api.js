import {BaseApi} from "@/shared/infrastructure/base-api.js";
import { DeviceAssembler } from './device.assembler.js';

export class DeviceApi extends BaseApi {
  constructor() {
    super();
    this.devicesEndpoint = import.meta.env.VITE_DEVICES_ENDPOINT_PATH;
  }

  async getAllDevices() {
    try {
      const response = await this.http.get(`${this.devicesEndpoint}`);
      return DeviceAssembler.toEntityList(response.data);
    } catch (error) {
      console.error('Error fetching devices:', error);
      throw new Error('No se pudieron cargar los dispositivos');
    }
  }

  async getDeviceById(id) {
    try {
      const response = await this.http.get(`${this.devicesEndpoint}/${id}`);
      return DeviceAssembler.toEntity(response.data);
    } catch (error) {
      console.error('Error fetching device:', error);
      throw new Error('No se pudo cargar el dispositivo');
    }
  }

  async updateDevice(device) {
    try {
      const deviceResource = DeviceAssembler.toResource(device);
      const response = await this.http.put(`${this.devicesEndpoint}/${device.id}`, deviceResource);
      return DeviceAssembler.toEntity(response.data);
    } catch (error) {
      console.error('Error updating device:', error);
      throw new Error('No se pudo actualizar el dispositivo');
    }
  }

  async createDevice(device) {
    try {
      const deviceResource = DeviceAssembler.toResource(device);
      const response = await this.http.post(`${this.devicesEndpoint}`, deviceResource);
      return DeviceAssembler.toEntity(response.data);
    } catch (error) {
      console.error('Error creating device:', error);
      throw new Error('No se pudo crear el dispositivo');
    }
  }

  async deleteDevice(id) {
    try {
      await this.http.delete(`${this.devicesEndpoint}/${id}`);
      return true;
    } catch (error) {
      console.error('Error deleting device:', error);
      throw new Error('No se pudo eliminar el dispositivo');
    }
  }
}

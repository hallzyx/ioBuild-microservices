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

  /**
   * Create a device. When `unitId` is provided, the server routes to the
   * OwnerCustom factory and assigns the device to that unit.
   * @param {object} device - device payload; include `unitId` for owner-custom devices
   * @returns {Promise<Device>}
   */
  async createDevice(device) {
    try {
      const deviceResource = DeviceAssembler.toResource(device);
      // Pass unitId through if present (owner-custom path)
      if (device.unitId != null) {
        deviceResource.unitId = device.unitId;
      }
      const response = await this.http.post(`${this.devicesEndpoint}`, deviceResource);
      return DeviceAssembler.toEntity(response.data);
    } catch (error) {
      console.error('Error creating device:', error);
      throw error; // re-throw so callers can inspect status (409, 403, 400)
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

  async getDeviceTypes() {
    try {
      const response = await this.http.get(`${this.devicesEndpoint}/types`);
      // Response shape: { deviceTypes: [{ code, displayName, controllableAttributes }, ...] }
      return response.data.deviceTypes ?? [];
    } catch (error) {
      console.error('Error fetching device types:', error);
      throw new Error('Could not load device type catalog');
    }
  }

  /**
   * Send a single-attribute actuation command to a device.
   * Body key is `attribute` (ADR-B5) — NOT attributeName.
   * @param {number} deviceId
   * @param {string} attribute - verbatim catalog attribute name
   * @param {*} value - numeric or string scalar
   * @returns {Promise<object>} CommandResultResource: { deviceId, attribute, value, acceptedAt }
   */
  async sendCommand(deviceId, attribute, value) {
    // Let the caller handle HTTP errors (status codes matter for UX)
    const response = await this.http.post(
      `${this.devicesEndpoint}/${deviceId}/commands`,
      { attribute, value }
    );
    return response.data;
  }

  /**
   * GET /api/v1/devices/{id}/status
   * Returns { deviceId, status, lastSeen, temperatureC, voltageV, desired? }
   * `desired` is the shadow's desired state dict, present only when the owner
   * has issued at least one command to this device.
   *
   * @param {number} deviceId
   * @returns {Promise<object>}
   */
  async getDeviceStatus(deviceId) {
    const response = await this.http.get(`${this.devicesEndpoint}/${deviceId}/status`);
    return response.data;
  }
}

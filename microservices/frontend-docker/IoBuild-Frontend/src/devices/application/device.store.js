import { defineStore } from 'pinia';
import { DeviceApi } from '../infrastructure/device-api.js';

export const useDeviceStore = defineStore('device', {
  state: () => ({
    devices: [],
    deviceTypes: [],
    /** Owner-scoped custom device types (populated by fetchCustomTypes). */
    customDeviceTypes: [],
    loading: false,
    error: null,
    selectedDevice: null
  }),

  getters: {
    onlineDevices: (state) => state.devices.filter(device => device.isOnline()),
    offlineDevices: (state) => state.devices.filter(device => !device.isOnline()),
    devicesByType: (state) => {
      const grouped = {};
      state.devices.forEach(device => {
        if (!grouped[device.type]) {
          grouped[device.type] = [];
        }
        grouped[device.type].push(device);
      });
      return grouped;
    },
    totalDevices: (state) => state.devices.length
  },

  actions: {
    async fetchDevices() {
      this.loading = true;
      this.error = null;
      try {
        const deviceApi = new DeviceApi();
        const list = await deviceApi.getAllDevices();
        const reverseCategoryMap = {
          'Sensor de Temperatura': 'temperature',
          'Medidor Energético': 'energy',
          'Sensor de Humedad': 'humidity',
          'Control de Acceso': 'door',
          'Sensor de Agua': 'water',
          'Sistema de Seguridad': 'security',
          'Control de Iluminación': 'lighting'
        };
        this.devices = list.map(d => {
          if (!d.type && d.category) {
            d.type = reverseCategoryMap[d.category] || d.category;
          }
          return d;
        });
        console.log('[DeviceStore] fetched devices:', this.devices.map(d => ({ id: d.id, type: d.type, category: d.category, keys: Object.keys(d) })));
      } catch (error) {
        this.error = error.message;
        console.error('Error in fetchDevices:', error);
      } finally {
        this.loading = false;
      }
    },

    async fetchDeviceById(id) {
      this.loading = true;
      this.error = null;
      try {
        const deviceApi = new DeviceApi();
        this.selectedDevice = await deviceApi.getDeviceById(id);
        return this.selectedDevice;
      } catch (error) {
        this.error = error.message;
        console.error('Error in fetchDeviceById:', error);
        return null;
      } finally {
        this.loading = false;
      }
    },

    async updateDevice(device) {
      this.loading = true;
      this.error = null;
      try {
        const deviceApi = new DeviceApi();
        const updatedDevice = await deviceApi.updateDevice(device);
        await this.fetchDevices(); // Recargar la lista de dispositivos
        if (this.selectedDevice && this.selectedDevice.id === updatedDevice.id) {
          this.selectedDevice = updatedDevice;
        }
        return updatedDevice;
      } catch (error) {
        this.error = error.message;
        console.error('Error in updateDevice:', error);
        return null;
      } finally {
        this.loading = false;
      }
    },

    async createDevice(deviceData) {
      this.loading = true;
      this.error = null;
      try {
        const deviceApi = new DeviceApi();
        const payload = { ...deviceData, status: 'active' };
        const newDevice = await deviceApi.createDevice(payload);
        await this.fetchDevices(); // Recargar la lista de dispositivos
        console.log('[DeviceStore] Device created and list refreshed:', newDevice);
        console.log('[DeviceStore] Total devices:', this.devices.length);
        return newDevice;
      } catch (error) {
        this.error = error.message;
        console.error('Error in createDevice:', error);
        return null;
      } finally {
        this.loading = false;
      }
    },

    async deleteDevice(id) {
      this.loading = true;
      this.error = null;
      try {
        const deviceApi = new DeviceApi();
        await deviceApi.deleteDevice(id);
        await this.fetchDevices(); // Recargar la lista de dispositivos
        if (this.selectedDevice && this.selectedDevice.id === id) {
          this.selectedDevice = null;
        }
        return true;
      } catch (error) {
        this.error = error.message;
        console.error('Error in deleteDevice:', error);
        return false;
      } finally {
        this.loading = false;
      }
    },

    async loadDeviceTypes() {
      try {
        const deviceApi = new DeviceApi();
        this.deviceTypes = await deviceApi.getDeviceTypes();
      } catch (error) {
        console.error('Error in loadDeviceTypes:', error);
        this.deviceTypes = [];
      }
    },

    /**
     * Fetch owner-scoped custom device types and store them in `customDeviceTypes`.
     * Called by my-unit-devices.vue on mount so custom attributes flow into DeviceControlPanel.
     * @param {string|number} ownerId
     */
    async fetchCustomTypes(ownerId) {
      try {
        const deviceApi = new DeviceApi();
        this.customDeviceTypes = await deviceApi.listCustomTypes(ownerId);
        console.log('[DeviceStore] fetchCustomTypes:', this.customDeviceTypes.length, 'types for owner', ownerId);
      } catch (error) {
        console.error('Error in fetchCustomTypes:', error);
        this.customDeviceTypes = [];
      }
    },

    /**
     * Create a new owner-scoped custom device type.
     * On success, refreshes `customDeviceTypes` for the owner.
     * On error, re-throws so the caller (dialog) can inspect status codes.
     * @param {{ typeCode: string, displayName: string, attributes: Array }} payload
     * @param {string|number} ownerId - used to refresh the list after creation
     * @returns {Promise<object>} CustomDeviceTypeDto
     */
    async createCustomType(payload, ownerId) {
      const deviceApi = new DeviceApi();
      const dto = await deviceApi.createCustomType(payload);
      // Refresh the list so the dialog and my-unit-devices reflect the new type immediately
      await this.fetchCustomTypes(ownerId);
      return dto;
    },

    setSelectedDevice(device) {
      this.selectedDevice = device;
    },

    clearError() {
      this.error = null;
    }
  }
});

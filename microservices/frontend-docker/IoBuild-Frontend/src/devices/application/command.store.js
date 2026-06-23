import { defineStore } from 'pinia';
import { ref } from 'vue';
import { DeviceApi } from '../infrastructure/device-api.js';

const deviceApi = new DeviceApi();

export const useCommandStore = defineStore('command', () => {
  const sending = ref(false);
  const lastResult = ref(null);
  const error = ref(null);

  /**
   * Send a single-attribute command to a device.
   * @param {number} deviceId
   * @param {string} attribute - verbatim catalog key (e.g. 'targetTemperature')
   * @param {*} value - numeric or string scalar
   * @returns {{ success: boolean, data?: object, status?: number }}
   */
  async function sendCommand(deviceId, attribute, value) {
    sending.value = true;
    error.value = null;
    lastResult.value = null;

    try {
      const data = await deviceApi.sendCommand(deviceId, attribute, value);
      lastResult.value = data;
      return { success: true, data };
    } catch (err) {
      const status = err.response?.status;
      const message =
        status === 403
          ? 'Permission denied. You can only control devices in your own unit.'
          : status === 400
          ? 'Invalid command: ' + (err.response?.data?.message || 'check the attribute and value.')
          : status === 404
          ? 'Device not found.'
          : 'Failed to send command. Please try again.';

      error.value = message;
      return { success: false, status, message };
    } finally {
      sending.value = false;
    }
  }

  function clearError() {
    error.value = null;
  }

  return {
    sending,
    lastResult,
    error,
    sendCommand,
    clearError,
  };
});

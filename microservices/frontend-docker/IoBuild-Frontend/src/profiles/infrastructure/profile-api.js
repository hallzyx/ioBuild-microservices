import { BaseApi } from "../../shared/infrastructure/base-api.js";

const profilesEndpoint = import.meta.env.VITE_PROFILES_ENDPOINT_PATH;
const usersEndpoint = import.meta.env.VITE_USERS_ENDPOINT_PATH;

/**
 * Profile API class to interact with profiles endpoints
 * @extends BaseApi
 * @class
 */
export class ProfileApi extends BaseApi {
  constructor() {
    super();
  }

  /**
   * Get profile by user ID
   * @param {number} userId - User ID
   * @returns {Promise} Response with profile data
   */
  async getProfile(userId) {
    if (!userId) throw new Error('User ID is required for fetching profile.');

    try {
      const response = await this.http.get(`${usersEndpoint}/${userId}/profiles`);
      // Backend returns an array of profiles, get the first one
      return Array.isArray(response.data) ? response.data[0] : response.data;
    } catch (error) {
      console.error('Error fetching profile:', error);
      throw error;
    }
  }

  /**
   * Create a new profile
   * @param {Object} profileResource - Profile data
   * @returns {Promise} Response with created profile
   */
  async createProfile(profileResource) {
    try {
      const response = await this.http.post(profilesEndpoint, profileResource);
      return response.data;
    } catch (error) {
      console.error('Error creating profile:', error);
      throw error;
    }
  }

  /**
   * Update an existing profile
   * @param {number} profileId - Profile ID
   * @param {Object} updatedData - Updated profile data
   * @returns {Promise} Response with updated profile
   */
  async updateProfile(profileId, updatedData) {
    if (!profileId) throw new Error('Profile ID is required for updating profile.');

    try {
      const response = await this.http.put(`${profilesEndpoint}/${profileId}`, updatedData);
      return response.data;
    } catch (error) {
      console.error('Error updating profile:', error);
      throw error;
    }
  }

    /**
     * Set second email by user ID
     * @param {number} userId - User ID
     * @param {string} secondEmail - Second email address
     * @returns {Promise} Response with updated profile
     */
    async setSecondEmailByUserId(userId, secondEmail) {
        if (!userId) throw new Error('User ID is required for setting second email.');

        try {
            // Enviar JSON puro en el body y el userId como query param, sin usar `data` dentro del config.
            const response = await this.http.post(
                `${profilesEndpoint}/second-email`,
                { secondEmail },
                { params: { userId } }
            );
            return response.data;
        } catch (error) {
            console.error('Error setting second email:', error);
            throw error;
        }
    }
}

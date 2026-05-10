import { BaseApi } from "../../shared/infrastructure/base-api.js";

const authenticationEndpoint = import.meta.env.VITE_AUTHENTICATION_ENDPOINT_PATH;
const usersEndpoint = import.meta.env.VITE_USERS_ENDPOINT_PATH;

/**
 * IAM API class to interact with authentication endpoints
 * @extends BaseApi
 * @class
 */
export class IamApi extends BaseApi {
    constructor() {
        super();
    }

    /**
     * Sign in with email and password
     * @param {Object} signInResource - Object containing email and password
     * @returns {Promise} Response with authenticated user and token
     */
    signIn(signInResource) {
        return this.http.post(`${authenticationEndpoint}/sign-in`, signInResource);
    }

    /**
     * Sign up with email, password and role
     * @param {Object} signUpResource - Object containing email, password and role
     * @returns {Promise} Response with created user
     */
    signUp(signUpResource) {
        return this.http.post(`${authenticationEndpoint}/sign-up`, signUpResource);
    }

    /**
     * Get all users
     * @returns {Promise} Response with all users
     */
    getUsers() {
        return this.http.get(usersEndpoint);
    }

    /**
     * Get user by ID
     * @param {number} id - User ID
     * @returns {Promise} Response with user data
     */
    getUserById(id) {
        return this.http.get(`${usersEndpoint}/${id}`);
    }

    /**
     * Change password for a given user
     * @param {number} userId - User ID
     * @param {Object} updatePasswordResource - { currentPassword, newPassword, confirmNewPassword }
     * @returns {Promise} API response
     */
    changePassword(userId, updatePasswordResource) {
        return this.http.put(`${usersEndpoint}/${userId}/password`, updatePasswordResource);
    }
}

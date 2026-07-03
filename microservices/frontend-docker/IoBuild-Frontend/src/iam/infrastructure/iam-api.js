import { BaseApi } from "../../shared/infrastructure/base-api.js";

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
        return this.http.post(`/sessions`, signInResource);
    }

    /**
     * Sign up with email, password and role
     * @param {Object} signUpResource - Object containing email, password and role
     * @returns {Promise} Response with created user
     */
    signUp(signUpResource) {
        return this.http.post(`${usersEndpoint}`, signUpResource);
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
}

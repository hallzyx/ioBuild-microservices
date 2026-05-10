import axios from 'axios';

const platformApi = import.meta.env.VITE_API_URL;

export class BaseApi {
    #http;
    constructor() {
        this.#http = axios.create({
            baseURL: platformApi
        });

        // Add a request interceptor to include the JWT token
        this.#http.interceptors.request.use(
            (config) => {
                const token = localStorage.getItem('token');
                if (token) {
                    config.headers.Authorization = `Bearer ${token}`;
                }
                return config;
            },
            (error) => {
                return Promise.reject(error);
            }
        );

        // Add a response interceptor to handle 401 Unauthorized
        this.#http.interceptors.response.use(
            (response) => response,
            (error) => {
                if (error.response?.status === 401) {
                    // Si YA estamos en una página pública (/login, /register-*),
                    // no redirigir — dejar que el componente muestre el error
                    const path = window.location.pathname;
                    const isPublicPage = path === '/login' 
                        || path === '/iam/login'
                        || path.startsWith('/iam/register');
                    if (!isPublicPage) {
                        localStorage.removeItem('token');
                        localStorage.removeItem('currentUser');
                        window.location.href = '/iam/login';
                    }
                }
                return Promise.reject(error);
            }
        );
    }

    get http() {
        return this.#http;
    }
}

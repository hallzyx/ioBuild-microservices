// Lazy-loaded components
const login = () => import('./views/login.vue');
const registerBuilder = () => import('./views/register-builder.vue');
const registerOwner = () => import('./views/register-owner.vue');

const iamRoutes = [
    { 
        path: 'login',
        name: 'login', 
        component: login, 
        meta: { 
            title: 'Login',
            public: true // Route accessible without authentication
        } 
    },
    { 
        path: 'register-builder',
        name: 'register-builder', 
        component: registerBuilder, 
        meta: { 
            title: 'Register as Builder',
            public: true
        } 
    },
    { 
        path: 'register-owner',
        name: 'register-owner', 
        component: registerOwner, 
        meta: { 
            title: 'Register as Owner',
            public: true
        } 
    }
];

export default iamRoutes;

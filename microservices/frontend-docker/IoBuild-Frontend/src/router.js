import {createRouter, createWebHistory} from "vue-router";
import automationRoutes from "./devices/presentation/automation-routes.js";
import subscriptionsRoutes from "./subscriptions/presentation/subscriptions-routes.js";
import profilesRoutes from "./profiles/presentation/profiles-routes.js";
import projectsRoutes from "./projects/presentation/projects-routes.js";
import analyticsRoutes from "./analytics/presentation/analytics-routes.js";
import { clientsRoutes } from "./clients/presentation/clients-routes.js";
import iamRoutes from "./iam/presentation/iam-routes.js";

const routes = [

    {
        path: '/iam',
        name: 'iam',
        children: iamRoutes
    },
    {
        path: '/analytics',
        name: 'analytics',
        children: analyticsRoutes
    },
    {
        path: '/devices',
        name: 'automation',
        children: automationRoutes
    },
    {
        path: '/profiles',
        name: 'profiles',
        redirect: '/profiles/profile',
        children: profilesRoutes
    },
    {
        path: '/projects',
        name: 'projects',
        children:  projectsRoutes
    },
    {
        path: '/clients',
        name: 'clients-module',
        children: clientsRoutes
    },
    {
        path: '/subscriptions',
        name: 'subscriptions',
        children:  subscriptionsRoutes
    },
    {
        path: '/configuration',
        name: 'configuration',
        component: () => import('./shared/presentation/views/configuration.vue'),
    },

    {
        path: '/',
        redirect: '/iam/login'
    },
    {
        path: '/home',
        name: 'home',
        redirect: '/analytics/dashboard'
    },

    {
        path: '/:pathMatch(.*)*',
        name: 'NotFound',
        component: () => import('./shared/presentation/views/page-not-found.vue'),
    }
]

const router = createRouter({
    history: createWebHistory(import.meta.env.BASE_URL),
    routes: routes
});

router.beforeEach((to, from, next) => {
    console.log(`Navigating from ${from.name} to ${to.name}`);
    
    // Update page title
    let baseTitle = 'IoBuild';
    document.title = `${baseTitle} - ${to.meta['title'] || ''}`;
    
    // Check if route requires authentication
    const isPublicRoute = to.meta?.public === true;
    const token = localStorage.getItem('token');
    const isAuthenticated = !!token;
    
    console.log('Route check:', { 
        path: to.path, 
        isPublic: isPublicRoute, 
        isAuthenticated 
    });
    
    // If route is not public and user is not authenticated, redirect to login page
    if (!isPublicRoute && !isAuthenticated) {
        console.log('Redirecting to login - not authenticated');
        next('/iam/login');
    } 
    // If user is authenticated and trying to access login, redirect to home
    else if (to.path === '/iam/login' && isAuthenticated) {
        console.log('Redirecting to home - already authenticated');
        next('/home');
    } 
    // Otherwise, allow navigation
    else {
        next();
    }
});

export default router;
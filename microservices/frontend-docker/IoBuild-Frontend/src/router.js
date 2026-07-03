import {createRouter, createWebHistory} from "vue-router";
import automationRoutes from "./devices/presentation/automation-routes.js";
import subscriptionsRoutes from "./subscriptions/presentation/subscriptions-routes.js";
import profilesRoutes from "./profiles/presentation/profiles-routes.js";
import projectsRoutes from "./projects/presentation/projects-routes.js";
import analyticsRoutes from "./analytics/presentation/analytics-routes.js";
import { clientsRoutes } from "./clients/presentation/clients-routes.js";
import iamRoutes from "./iam/presentation/iam-routes.js";
import { SubscriptionApi } from "./subscriptions/infrastructure/subscription-api.js";

const subscriptionApi = new SubscriptionApi();

// A builder may only use the platform once they hold an active subscription.
// Returns true when the current builder has an active subscription (fail-closed
// on error so paid features stay gated).
async function builderHasActiveSubscription() {
    try {
        const currentUser = JSON.parse(localStorage.getItem('currentUser') || 'null');
        if (!currentUser?.id) return false;
        const { data } = await subscriptionApi.getSubscriptionByBuilderId(currentUser.id);
        return !!data && String(data.status).toLowerCase() === 'active';
    } catch (error) {
        console.error('[subscription-gate] check failed:', error);
        return false;
    }
}

const routes = [

    {
        path: '/iam',
        name: 'iam',
        children: iamRoutes
    },
    {
        path: '/analytics',
        children: analyticsRoutes
    },
    {
        path: '/devices',
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
        children:  subscriptionsRoutes
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

router.beforeEach(async (to, from, next) => {
    // Update page title
    let baseTitle = 'IoBuild';
    document.title = `${baseTitle} - ${to.meta['title'] || ''}`;

    // Check if route requires authentication
    const isPublicRoute = to.meta?.public === true;
    const token = localStorage.getItem('token');
    const isAuthenticated = !!token;

    // If route is not public and user is not authenticated, redirect to login page
    if (!isPublicRoute && !isAuthenticated) {
        next('/iam/login');
        return;
    }
    // If user is authenticated and trying to access login, redirect to home
    if (to.path === '/iam/login' && isAuthenticated) {
        next('/home');
        return;
    }

    // ── Subscription gate (builders only) ──────────────────────────────
    // A builder without an active subscription may only reach the subscriptions
    // section (and iam, to log in/out). Everything else redirects there, since
    // those features are part of what the subscription pays for.
    if (isAuthenticated) {
        const currentUser = JSON.parse(localStorage.getItem('currentUser') || 'null');
        const isBuilder = String(currentUser?.role).toLowerCase() === 'builder';
        const isAllowedWithoutSub = to.path.startsWith('/subscriptions')
            || to.path.startsWith('/iam');

        if (isBuilder && !isAllowedWithoutSub) {
            // Retry up to 2 times with a short delay to handle the race window
            // between Stripe checkout redirect and the incoming webhook setting
            // status to 'active'. Without this, the first navigation after
            // checkout fails the gate before the webhook fires.
            let active = await builderHasActiveSubscription();
            if (!active) {
                await new Promise((r) => setTimeout(r, 1500));
                active = await builderHasActiveSubscription();
            }
            if (!active) {
                next('/subscriptions/my-subscription');
                return;
            }
        }
    }

    // ── Role gate: Owner-only routes ────────────────────────────────────
    // Routes with meta.requiresRole = 'owner' are blocked for non-Owner users.
    // Builders are redirected to the analytics dashboard; unauthenticated users
    // are already redirected to login above.
    if (to.meta?.requiresRole) {
        const currentUser = JSON.parse(localStorage.getItem('currentUser') || 'null');
        const userRole = String(currentUser?.role ?? '').toLowerCase();
        const requiredRole = String(to.meta.requiresRole).toLowerCase();
        if (userRole !== requiredRole) {
            // Builder or unexpected role: send to their own dashboard
            next('/analytics/dashboard');
            return;
        }
    }

    next();
});

export default router;
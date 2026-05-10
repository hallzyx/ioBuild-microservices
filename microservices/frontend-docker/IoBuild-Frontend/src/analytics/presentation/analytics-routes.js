const analyticsDashboard = () => import("./views/analytics-dashboard.view.vue");

const analyticsRoutes = [
    {
        path: '',
        redirect: 'dashboard'
    },
    {
        path: 'dashboard',
        name: 'analytics-dashboard',
        component: analyticsDashboard,
        meta: { title: 'Analytics Dashboard' }
    }
];

export default analyticsRoutes;

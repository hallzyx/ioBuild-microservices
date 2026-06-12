const subscriptionsRoutes = [
    {
        path: "",
        redirect: "my-subscription"
    },
    {
        path: "my-subscription",
        name: "my-subscription",
        meta: { title: 'My Subscription' },
        component: () => import("./views/my-subscription.vue"),
    }
];
export default subscriptionsRoutes;
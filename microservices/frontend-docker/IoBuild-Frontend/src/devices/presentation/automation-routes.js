import DeviceManagement from './views/device-management.vue';

export const automationRoutes = [
  {
    path: '',
    redirect: '/devices/device-management'
  },
  {
    path: 'device-management', // Ruta final: /devices/device-management
    name: 'device-management',
    component: DeviceManagement,
    alias: ['/devices/devices', 'devices'], // Compatibilidad con rutas antiguas
    meta: {
      title: 'Device Management',
      requiresAuth: true
    }
  }
];

export default automationRoutes;
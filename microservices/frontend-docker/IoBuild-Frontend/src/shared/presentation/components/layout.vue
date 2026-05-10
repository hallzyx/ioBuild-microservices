<script setup lang="js">
import { useI18n } from "vue-i18n";
import { ref, computed, onMounted } from "vue";
import { useRouter } from "vue-router";
import LanguageSwitcher from "./language-switcher.vue";
import { useProfileStore } from "../../../profiles/application/profile.store.js";
import { useIamStore } from "../../../iam/application/iam.store.js";

const { t } = useI18n();
const router = useRouter();
const drawer = ref(false);
const profileStore = useProfileStore();
const iamStore = useIamStore();

const toggleDrawer = () => {
  drawer.value = !drawer.value;
};


const currentUser = computed(() => iamStore.currentUser);
const userRole = computed(() => currentUser.value?.role?.toLowerCase() || 'builder');


onMounted(async () => {
  const userId = currentUser.value?.id;
  if (userId && !profileStore.profileLoaded) {
    await profileStore.fetchProfile(userId);
    console.log('layout: fetched profile', profileStore.profile);


    if (profileStore.profile) {
      const payload = {
        username: profileStore.profile.username,
        name: profileStore.profile.name,
        photoUrl: profileStore.profile.photoUrl
      };
      console.log('layout: updating IAM user with profile payload', payload);
      iamStore.updateUserProfile(payload);
      console.log('layout: currentUser after update', iamStore.currentUser);
    }
  }
});


const userName = computed(() => {
  return currentUser.value?.username || currentUser.value?.name || 'Usuario';
});

const userPhoto = computed(() => {
  return currentUser.value?.photoUrl || 'https://via.placeholder.com/40x40/10B981/FFFFFF?text=U';
});


const handleLogout = () => {
  iamStore.signOut();
  router.push('/login');
};

const items = [

  { label: 'option.home', to: '/analytics/dashboard', use_role: 'builder', type: 'builder', icon: 'pi pi-home' },
  { label: 'option.profile', to: '/profiles/profile', use_role: 'builder', type: 'builder', icon: 'pi pi-user' },
  { label: 'option.projects', to: '/projects', use_role: 'builder', type: 'builder', icon: 'pi pi-folder' },
  { label: 'option.clients', to: '/clients', use_role: 'builder', type: 'builder', icon: 'pi pi-users' },
  { label: 'option.subscription', to: '/subscriptions/my-subscription', use_role: 'builder', type: 'builder', icon: 'pi pi-credit-card' },
  { label: 'option.configuration', to: '/configuration', use_role: 'builder', type: 'builder', icon: 'pi pi-cog' },


  { label: 'option.home', to: '/analytics/dashboard', use_role: 'owner', type: 'owner', icon: 'pi pi-home' },
  { label: 'option.profile', to: '/profiles/profile', use_role: 'owner', type: 'owner', icon: 'pi pi-user' },
  { label: 'option.device-management', to: '/devices/device-management', use_role: 'owner', type: 'owner', icon: 'pi pi-microchip' },
  { label: 'option.configuration', to: '/configuration', use_role: 'owner', type: 'owner', icon: 'pi pi-cog' },
];

const filteredItems = computed(() => {
  const currentRole = userRole.value;
  return items.filter(item => {
    const itemRole = String(item.use_role || 'builder').trim().toLowerCase();
    return itemRole === currentRole;
  });
});
</script>

<template>
  <pv-toast/>
  <pv-confirm-dialog/>

  <div class="app-container">
    <header class="app-header">
      <pv-toolbar class="custom-toolbar">
        <template #start>
          <div class="header-start">
            <pv-button
                class="menu-button"
                icon="pi pi-bars"
                text
                rounded
                @click="toggleDrawer"
            />            <router-link to="/analytics/dashboard" class="logo-link">
              <img
                src="/IoBuild-Logo.png"
                alt="IoBuild Logo"
                class="header-logo"
              />
            </router-link>
          </div>
        </template>

        <template #center>
          <div class="welcome-message">
            <span class="welcome-text">{{ t('header.welcome') }}, {{ userName }}</span>
          </div>
        </template>

        <template #end>
          <div class="header-end">

            <pv-button
              class="notification-button"
              icon="pi pi-bell"
              text
              rounded
              severity="secondary"
            />


            <router-link
              to="/profiles/profile"
              class="user-info-link"
              @click="drawer = false"
            >
              <div class="user-info">
                <img
                  :src="userPhoto"
                  :alt="userName"
                  class="user-avatar"
                />
                <div class="user-details">
                  <span class="user-name">{{ userName }}</span>
                  <span class="user-role">{{ t(`profile.${userRole}Role`) }}</span>
                </div>
              </div>
            </router-link>


            <language-switcher/>


            <pv-button
              class="logout-button"
              icon="pi pi-sign-out"
              :label="t('iam.logout')"
              text
              rounded
              severity="danger"
              @click="handleLogout"
            />
          </div>
        </template>
      </pv-toolbar>
    </header>


    <div class="body-container">

      <div v-if="drawer" class="custom-sidebar" @click.self="drawer = false">
        <div class="sidebar-content">
          <nav class="sidebar-menu">
            <router-link
                v-for="item in filteredItems"
                :key="item.label"
                :to="item.to"
                class="menu-item"
                @click="drawer = false"
            >
              <i :class="item.icon" class="menu-icon"></i>
              <span class="menu-label">{{ t(item.label) }}</span>
              <i class="pi pi-chevron-right menu-arrow"></i>
            </router-link>
            
            <!-- Botón de Logout en el sidebar -->
            <div class="menu-item logout-item" @click="handleLogout">
              <i class="pi pi-sign-out menu-icon"></i>
              <span class="menu-label">{{ t('iam.logout') }}</span>
              <i class="pi pi-chevron-right menu-arrow"></i>
            </div>
          </nav>
        </div>
      </div>

      <main class="main-content" :class="{ 'with-sidebar': drawer }">
        <router-view v-slot="{ Component }">
          <transition name="fade" mode="out-in">
            <component :is="Component" />
          </transition>
        </router-view>
      </main>
    </div>
  </div>
</template>

<style scoped>
.app-container {
  min-height: 100vh;
  width: 100%;
  margin: 0;
  padding: 0;
  background: linear-gradient(135deg, #f0fdf4 0%, #e6fffa 100%);
}

.app-header {
  position: sticky;
  top: 0;
  z-index: 1001;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
}

.custom-toolbar {
  background: white !important;
  border: none;
  padding: 0.75rem 1.5rem;
  border-bottom: 1px solid #e5e7eb;
}

.header-start {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.menu-button {
  color: #374151 !important;
  transition: transform 0.2s ease;
}

.menu-button:hover {
  transform: scale(1.1);
  background: rgba(0, 0, 0, 0.05) !important;
}

.header-logo {
  height: 40px;
  width: auto;
  object-fit: contain;
}

.logo-link {
  display: flex;
  align-items: center;
  text-decoration: none;
  border-radius: 8px;
  transition: all 0.2s ease;
  padding: 0.25rem;
}

.logo-link:hover {
  background: rgba(16, 185, 129, 0.1);
  transform: scale(1.05);
}

.logo-link:active {
  transform: scale(0.98);
}

.logo-link .header-logo {
  transition: all 0.2s ease;
}

.logo-link:hover .header-logo {
  filter: brightness(1.1);
}

.welcome-message {
  display: flex;
  align-items: center;
}

.welcome-text {
  font-size: 1.125rem;
  font-weight: 600;
  color: #374151;
}

.header-end {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.notification-button {
  color: #6b7280 !important;
  position: relative;
}

.notification-button:hover {
  background: rgba(0, 0, 0, 0.05) !important;
}

.logout-button {
  color: #ef4444 !important;
  font-weight: 500;
}

.logout-button:hover {
  background: rgba(239, 68, 68, 0.1) !important;
  color: #dc2626 !important;
}

.user-info-link {
  text-decoration: none;
  color: inherit;
  border-radius: 8px;
  transition: all 0.2s ease;
}

.user-info-link:hover {
  background: rgba(16, 185, 129, 0.05);
  transform: translateY(-1px);
}

.user-info-link:active {
  transform: translateY(0);
}

.user-info {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.5rem;
  border-radius: 8px;
  transition: background-color 0.2s ease;
  cursor: pointer;
}

.user-info:hover {
  background: transparent;
}

.user-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  object-fit: cover;
  border: 2px solid #e5e7eb;
}

.user-details {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
}

.user-name {
  font-size: 0.875rem;
  font-weight: 600;
  color: #374151;
  line-height: 1.2;
}

.user-role {
  font-size: 0.75rem;
  color: #6b7280;
  line-height: 1.2;
}

:deep(.custom-drawer .p-drawer) {
  width: 320px !important;
  box-shadow: 4px 0 20px rgba(0, 0, 0, 0.06);
  background: white;
  border-radius: 0;
  z-index: 1000;
}

:deep(.custom-drawer .p-drawer .p-drawer-content) {
  padding: 0 !important;
  background: white;
}


:deep(.custom-drawer .p-drawer-mask) {
  z-index: 999;
}

.drawer-header {
  padding: 1.5rem 1.5rem;
  background: linear-gradient(135deg, #34d399 0%, #2dd4bf 100%);
  color: white;
  width: 100%;
  box-sizing: border-box;
}

.drawer-brand {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.drawer-logo {
  height: 45px;
  width: auto;
  object-fit: contain;
}

.brand-info {
  display: flex;
  flex-direction: column;
}

.drawer-title {
  margin: 0;
  font-size: 1.5rem;
  font-weight: 700;
  text-shadow: 0 2px 4px rgba(0, 0, 0, 0.12);
}

.drawer-subtitle {
  margin: 0.25rem 0 0 0;
  font-size: 0.875rem;
  opacity: 0.9;
}

.drawer-content {
  padding: 1rem 0;
}

.drawer-menu {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.menu-item {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1rem 1.5rem;
  color: black;
  text-decoration: none;
  transition: all 0.3s ease;
  position: relative;
  overflow: hidden;
  margin: 0.25rem 1rem;
  border-radius: 8px;
}

.menu-item::before {
  content: '';
  position: absolute;
  left: 0;
  top: 0;
  height: 100%;
  width: 4px;
  background: black;
  transform: scaleY(0);
  transition: transform 0.3s ease;
  border-radius: 0 4px 4px 0;
}

.menu-item:hover {
  background: rgba(0, 0, 0, 0.1);
  color: black;
}

.menu-item:hover::before {
  transform: scaleY(1);
}

.menu-item.router-link-active {
  background: black;
  color: white;
  font-weight: 600;
  border-radius: 8px;
}

.menu-item.router-link-active::before {
  transform: scaleY(1);
  background: white;
}

.menu-icon {
  font-size: 1.25rem;
  width: 1.5rem;
  transition: transform 0.3s ease;
}

.menu-item:hover .menu-icon {
  transform: scale(1.15);
}

.menu-label {
  flex: 1;
  font-size: 0.95rem;
}

.menu-arrow {
  font-size: 0.875rem;
  opacity: 0;
  transform: translateX(-10px);
  transition: all 0.3s ease;
}

.menu-item:hover .menu-arrow {
  opacity: 1;
  transform: translateX(0);
}


.logout-item {
  cursor: pointer;
  margin-top: auto;
  border-top: 1px solid rgba(0, 0, 0, 0.1);
  color: #ef4444;
}

.logout-item:hover {
  background: rgba(239, 68, 68, 0.1);
  color: #dc2626;
}

.logout-item::before {
  background: #ef4444;
}



.body-container {
  position: relative;
  min-height: calc(100vh - 76px);
}


.custom-sidebar {
  position: fixed;
  top: 76px; /* Altura del header */
  left: 0;
  width: 100%;
  height: calc(100vh - 76px);
  background: rgba(0, 0, 0, 0.5);
  z-index: 1000;
  animation: fadeIn 0.3s ease;
}

.sidebar-content {
  width: 320px;
  height: 100%;
  background: #10B981;
  box-shadow: 4px 0 20px rgba(0, 0, 0, 0.06);
  animation: slideIn 0.3s ease;
  overflow-y: auto;
}

.sidebar-header {
  padding: 1.5rem;
  background: linear-gradient(135deg, #34d399 0%, #2dd4bf 100%);
  color: white;
}

.sidebar-brand {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.sidebar-logo {
  height: 45px;
  width: auto;
  object-fit: contain;
}

.brand-info {
  display: flex;
  flex-direction: column;
}

.sidebar-title {
  margin: 0;
  font-size: 1.5rem;
  font-weight: 700;
  text-shadow: 0 2px 4px rgba(0, 0, 0, 0.12);
}

.sidebar-subtitle {
  margin: 0.25rem 0 0 0;
  font-size: 0.875rem;
  opacity: 0.9;
}

.sidebar-menu {
  padding: 1rem 0;
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

/* Animaciones */
@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

@keyframes slideIn {
  from { transform: translateX(-320px); }
  to { transform: translateX(0); }
}

.main-content {
  padding: 2rem;
  min-height: calc(100vh - 76px);
  transition: all 0.3s ease;
}

.main-content.with-sidebar {
  margin-left: 0; /* No empujar el contenido cuando el sidebar está abierto */
}

.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease, transform 0.3s ease;
}

.fade-enter-from {
  opacity: 0;
  transform: translateY(10px);
}

.fade-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}

@media (max-width: 768px) {
  .welcome-message {
    display: none;
  }

  .user-details {
    display: none;
  }

  .header-end {
    gap: 0.5rem;
  }

  .header-logo {
    height: 32px;
  }
}

@media (max-width: 480px) {
  .custom-toolbar {
    padding: 0.5rem 1rem;
  }

  .header-start {
    gap: 0.5rem;
  }
}
</style>
<script setup>
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import ToggleSwitch from 'primevue/toggleswitch';
import Button from 'primevue/button';
import AlternateMailForm from '../components/alternate-mail.form.vue';
import ChangePassword from '../components/change-password.vue';
import { useProfileStore } from '../../../profiles/application/profile.store.js';

const { t } = useI18n();
const profileStore = useProfileStore();

// Listas de claves (sin objetos)
const notificationKeys = ['expiration', 'system', 'customer', 'push'];
const securityKeys = ['password', '2fa', 'sessions', 'alternate'];
const supportKeys = ['faqs', 'contact'];

// Estado reactivo para los toggles (solo notificaciones)
const notificationStates = ref({
  expiration: true,
  system: true,
  customer: true,
  push: true
});

// Estado para la modal de correo alternativo y el correo
const showAlternateEmailModal = ref(false);
const alternateEmail = ref(''); // podrías cargarlo desde una API o store

// Estado para la modal de cambio de contraseña
const showChangePasswordModal = ref(false);

const handleSaveAlternateEmail = async (email) => {
  alternateEmail.value = email;
  try {
    await profileStore.setSecondEmail(email);
  } catch (error) {
    console.error('Error saving alternate email:', error);
  }
};

const supportUrls = {
  faqs: 'https://upc-pre-1asi0730-7461-ccaritatech.github.io/landing-page-CcaritaTech/faq.html',
  contact: 'https://upc-pre-1asi0730-7461-ccaritatech.github.io/landing-page-CcaritaTech/index.html'
};
</script>

<template>
  <div class="configuration-container">
    <h1 class="title">{{ $t('configuration.title') }}</h1>

    <!-- Notifications and Security Sections -->
    <div class="flex-container">
      <!-- Notifications Section -->
      <div class="section-card">
        <h2 class="section-title">{{ $t('configuration.notifications.title') }}</h2>
        <div class="items-list">
          <div v-for="item in notificationKeys" :key="item" class="item-row">
            <span>{{ $t(`configuration.notifications.${item}`) }}</span>
            <ToggleSwitch v-model="notificationStates[item]" />
          </div>
        </div>
      </div>

      <!-- Security and Privacy Section -->
      <div class="section-card">
        <h2 class="section-title">{{ $t('configuration.security.title') }}</h2>
        <div class="items-list">
          <div v-for="item in securityKeys" :key="item" class="item-row">
            <span>{{ $t(`configuration.security.${item}`) }}</span>
            <!-- Botón especial para correo alternativo -->
            <Button
                v-if="item === 'alternate'"
                icon="pi pi-cog"
                text
                class="icon-button"
                @click="showAlternateEmailModal = true"
            />
            <!-- Botón especial para cambio de contraseña -->
            <Button
                v-else-if="item === 'password'"
                icon="pi pi-cog"
                text
                class="icon-button"
                @click="showChangePasswordModal = true"
            />
            <!-- Resto de acciones de seguridad deshabilitadas por ahora -->
            <Button
                v-else
                icon="pi pi-cog"
                text
                class="icon-button"
                disabled
            />
          </div>
        </div>
      </div>
    </div>

    <!-- Support and Help Section -->
    <div class="section-card">
      <h2 class="section-title">{{ $t('configuration.support.title') }}</h2>
      <div class="items-list">
        <a
            v-for="item in supportKeys"
            :key="item"
            :href="supportUrls[item]"
            target="_blank"
            rel="noopener noreferrer"
            class="support-link"
        >
          <span>{{ $t(`configuration.support.${item}`) }}</span>
          <Button icon="pi pi-chevron-right" text class="icon-button" />
        </a>
      </div>
    </div>

    <!-- Modal para correo alternativo -->
    <AlternateMailForm
        :visible="showAlternateEmailModal"
        :modelValue="alternateEmail"
        @update:visible="showAlternateEmailModal = $event"
        @save="handleSaveAlternateEmail"
    />

    <!-- Modal para cambio de contraseña -->
    <ChangePassword
        :visible="showChangePasswordModal"
        @update:visible="showChangePasswordModal = $event"
        @close="showChangePasswordModal = false"
    />
  </div>
</template>

<style scoped>
.configuration-container {
  padding: 24px;
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
  max-width: 1200px;
  margin: 0 auto;
}

.title {
  font-size: 28px;
  font-weight: 700;
  color: #1e293b;
  margin-bottom: 32px;
}

.section-title {
  font-size: 18px;
  font-weight: 600;
  color: #1e293b;
  margin-bottom: 16px;
}

.flex-container {
  display: flex;
  gap: 20px;
  margin-bottom: 32px;
}

.section-card {
  flex: 1;
  min-width: 0;
}

.items-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

/* Estilo común para filas interactivas */
.item-row,
.support-link {
  display: flex;
  justify-content: space-between;
  align-items: center;
  height: 48px;
  padding: 0 16px;
  background-color: #f0fdf4;
  border: 1px solid #d1d5db;
  border-radius: 8px;
  transition: all 0.2s ease;
  box-sizing: border-box;
}

.item-row:hover,
.support-link:hover {
  background-color: #ecfdf5;
  border-color: #9ca3af;
}

.item-row span,
.support-link span {
  font-size: 14px;
  color: #1e293b;
  font-weight: 500;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  flex: 1;
  margin-right: 12px;
}

.support-link {
  text-decoration: none;
  color: inherit;
  cursor: pointer;
}

.icon-button {
  color: #4b5563;
  width: 32px;
  height: 32px;
  padding: 0;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
}

.icon-button:hover {
  background-color: #e5e7eb;
}

/* Estilos para ToggleSwitch */
.p-toggleswitch {
  width: 40px;
  height: 20px;
  display: flex;
  align-items: center;
}

.p-toggleswitch .p-toggleswitch-slider {
  width: 40px;
  height: 20px;
  border-radius: 10px;
  background: #cbd5e1;
  transition: background-color 0.2s;
}

.p-toggleswitch .p-toggleswitch-slider:before {
  width: 16px;
  height: 16px;
  margin-top: 2px;
  margin-left: 2px;
  background: white;
  border-radius: 50%;
  transition: transform 0.2s;
}

.p-toggleswitch.p-toggleswitch-checked .p-toggleswitch-slider {
  background: #10b981;
}

.p-toggleswitch.p-toggleswitch-checked .p-toggleswitch-slider:before {
  transform: translateX(20px);
}

/* Responsive */
@media (max-width: 768px) {
  .flex-container {
    flex-direction: column;
    gap: 16px;
  }

  .configuration-container {
    padding: 16px;
  }
}

.change-password-modal {
  margin-top: 24px;
}
</style>
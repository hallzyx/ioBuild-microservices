<script setup>
import { ref, onMounted, computed } from 'vue';
import { useClientStore } from '../../application/client.store.js';
import { useRoute, useRouter } from 'vue-router';
import { useConfirm } from 'primevue/useconfirm';
import { useToast } from 'primevue/usetoast';
import { useI18n } from 'vue-i18n';
import ClientEditDialog from '../components/client-edit-dialog.vue';

const clientStore = useClientStore();
const route = useRoute();
const router = useRouter();
const confirm = useConfirm();
const toast = useToast();
const { t } = useI18n();

const clientId = computed(() => parseInt(route.params.id));
const client = computed(() => clientStore.getClientById(clientId.value));
const showEditDialog = ref(false);

onMounted(async () => {
  if (!clientStore.clientsLoaded) {
    await clientStore.fetchClients();
  }

  // Si viene con query parameter edit=true, abrir el diálogo automáticamente
  if (route.query.edit === 'true') {
    showEditDialog.value = true;
  }
});

const goBack = () => {
  router.push({ name: 'clients' });
};

const getSeverity = (status) => {
  switch (status) {
    case 'Active':
      return 'success';
    case 'Stand by':
      return 'warn';
    case 'Suspended':
      return 'danger';
    default:
      return 'info';
  }
};

// Función para traducir el estado
const getStatusLabel = (status) => {
  const statusMap = {
    'Active': 'active',
    'Stand by': 'standBy',
    'Suspended': 'suspended'
  };
  return t(`clients.status.${statusMap[status] || 'active'}`);
};

const handleEdit = () => {
  showEditDialog.value = true;
};

const handleSaveEdit = async (updatedClient) => {
  try {
    await clientStore.updateClient(updatedClient);
    toast.add({
      severity: 'success',
      summary: t('clients.messages.updateSuccess'),
      detail: t('clients.messages.updateSuccess'),
      life: 3000
    });
  } catch (error) {
    toast.add({
      severity: 'error',
      summary: t('clients.messages.updateError'),
      detail: t('clients.messages.updateError'),
      life: 3000
    });
  }
};

const handleDelete = () => {
  confirm.require({
    message: t('clients.messages.deleteConfirmMessage', { name: client.value.fullName }),
    header: t('clients.messages.deleteConfirmTitle'),
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: t('clients.actions.cancel'),
    acceptLabel: t('clients.actions.delete'),
    rejectClass: 'p-button-secondary p-button-outlined',
    acceptClass: 'p-button-danger',
    accept: async () => {
      try {
        await clientStore.deleteClient(client.value.id);
        toast.add({
          severity: 'success',
          summary: t('clients.messages.deleteSuccess'),
          detail: t('clients.messages.deleteSuccess'),
          life: 3000
        });
        // Redirigir a la lista después de eliminar
        setTimeout(() => {
          router.push({ name: 'clients' });
        }, 500);
      } catch (error) {
        toast.add({
          severity: 'error',
          summary: t('clients.messages.deleteError'),
          detail: t('clients.messages.deleteError'),
          life: 3000
        });
      }
    }
  });
};
</script>

<template>
  <div class="p-4">
    <pv-toast />

    <pv-button
      :label="t('clients.actions.goBack')"
      icon="pi pi-arrow-left"
      @click="goBack"
      text
      class="mb-4"
    />

    <pv-card v-if="client" class="client-profile-card">
      <template #title>
        <div class="flex align-items-center gap-3">
          <i class="pi pi-user text-4xl"></i>
          <div>
            <h1 class="text-3xl font-bold m-0">{{ client.fullName }}</h1>
            <pv-tag
              :value="getStatusLabel(client.accountStatement)"
              :severity="getSeverity(client.accountStatement)"
              class="mt-2"
            />
          </div>
        </div>
      </template>

      <template #content>
        <div class="grid">
          <div class="col-12 md:col-6">
            <div class="mb-4">
              <label class="block font-semibold text-gray-700 mb-2">
                <i class="pi pi-envelope mr-2"></i>{{ t('clients.fields.email') }}
              </label>
              <p class="text-lg text-gray-900">{{ client.email }}</p>
            </div>
          </div>

          <div class="col-12 md:col-6">
            <div class="mb-4">
              <label class="block font-semibold text-gray-700 mb-2">
                <i class="pi pi-phone mr-2"></i>{{ t('clients.fields.phoneNumber') }}
              </label>
              <p class="text-lg text-gray-900">{{ client.phoneNumber || 'N/A' }}</p>
            </div>
          </div>

          <div class="col-12 md:col-6">
            <div class="mb-4">
              <label class="block font-semibold text-gray-700 mb-2">
                <i class="pi pi-map-marker mr-2"></i>{{ t('clients.fields.address') }}
              </label>
              <p class="text-lg text-gray-900">{{ client.address || 'N/A' }}</p>
            </div>
          </div>

          <div class="col-12 md:col-6">
            <div class="mb-4">
              <label class="block font-semibold text-gray-700 mb-2">
                <i class="pi pi-building mr-2"></i>{{ t('clients.fields.project') }}
              </label>
              <p class="text-lg text-gray-900">{{ client.projectName || t('clients.messages.noProject') }}</p>
            </div>
          </div>
        </div>

        <pv-divider />

        <div class="flex gap-2 justify-content-end">
          <pv-button
            :label="t('clients.actions.editClient')"
            icon="pi pi-pencil"
            severity="secondary"
            outlined
            @click="handleEdit"
          />
          <pv-button
            :label="t('clients.actions.deleteClient')"
            icon="pi pi-trash"
            severity="danger"
            outlined
            @click="handleDelete"
          />
        </div>
      </template>
    </pv-card>

    <pv-card v-else class="client-profile-card">
      <template #content>
        <div class="text-center py-4">
          <i class="pi pi-exclamation-triangle text-4xl text-orange-500 mb-3"></i>
          <p class="text-xl text-gray-500">{{ t('clients.messages.clientNotFound') }}</p>
          <pv-button
            :label="t('clients.actions.goBack')"
            icon="pi pi-arrow-left"
            @click="goBack"
            class="mt-3"
          />
        </div>
      </template>
    </pv-card>

    <ClientEditDialog
      v-model:visible="showEditDialog"
      :client="client"
      @save="handleSaveEdit"
    />
  </div>
</template>

<style scoped>
.p-4 {
  background: white !important;
}


:deep(.client-profile-card) {
  background: white !important;
}

:deep(.client-profile-card ) {
  background: white !important;
}

:deep(.client-profile-card) {
  background: white !important;
  color: #111827 !important;
}

:deep(.client-profile-card) {
  background: white !important;
  color: #111827 !important;
}

:deep(.client-profile-card) {
  background: white !important;
  color: #111827 !important;
}

:deep(.client-profile-card) {
  background: white !important;
  color: #111827 !important;
}


:deep(.client-profile-card h1),
:deep(.client-profile-card h2),
:deep(.client-profile-card h3),
:deep(.client-profile-card p),
:deep(.client-profile-card label),
:deep(.client-profile-card span) {
  color: #111827 !important;
}

:deep(.client-profile-card .grid) {
  background: white !important;
}


:deep(.client-profile-card) {
  border-top-color: #e5e7eb !important;
}

:deep(.client-profile-card .pi-user) {
  color: #3b82f6 !important;
}

:deep(.client-profile-card .text-3xl) {
  color: #111827 !important;
}

:deep(.client-profile-card .text-lg) {
  color: #111827 !important;
}


:deep(.client-profile-card .pi-exclamation-triangle) {
  color: #f97316 !important;
}

/* Estilos para los botones de acción */
:deep(.client-profile-card .p-button.p-button-secondary.p-button-outlined) {
  background: #d1fae5 !important;
  border-color: #10b981 !important;
  color: #047857 !important;
}

:deep(.client-profile-card .p-button.p-button-secondary.p-button-outlined:hover) {
  background: #a7f3d0 !important;
  border-color: #059669 !important;
  color: #065f46 !important;
}

:deep(.client-profile-card .p-button.p-button-danger.p-button-outlined) {
  background: #fee2e2 !important;
  border-color: #ef4444 !important;
  color: #dc2626 !important;
}

:deep(.client-profile-card .p-button.p-button-danger.p-button-outlined:hover) {
  background: #fecaca !important;
  border-color: #dc2626 !important;
  color: #991b1b !important;
}

/* Estilos para el diálogo de confirmación */
:deep(.p-confirm-dialog) {
  background: white !important;
}

:deep(.p-confirm-dialog .p-dialog-content) {
  background: white !important;
  color: #111827 !important;
}

:deep(.p-confirm-dialog .p-dialog-header) {
  background: white !important;
  color: #111827 !important;
}

:deep(.p-confirm-dialog .p-dialog-footer) {
  background: white !important;
}

:deep(.p-confirm-dialog .p-confirm-dialog-message) {
  color: #111827 !important;
}

/* Estilos para toast notifications */
:deep(.p-toast) {
  color: #111827 !important;
}

:deep(.p-toast .p-toast-message) {
  background: white !important;
  color: #111827 !important;
}
</style>

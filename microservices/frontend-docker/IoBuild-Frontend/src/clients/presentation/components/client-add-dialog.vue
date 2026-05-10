<script setup>
import { ref, watch, computed, onMounted } from 'vue';
import { Client } from '../../domain/model/client.entity.js';
import { ProjectsFacade } from '../../infrastructure/projects.facade.js';

const props = defineProps({
  visible: {
    type: Boolean,
    required: true
  }
});

const emit = defineEmits(['update:visible', 'save']);

const projectsFacade = new ProjectsFacade();
const localVisible = ref(props.visible);
const projects = ref([]);
const formData = ref(new Client({
  fullName: '',
  email: '',
  phoneNumber: '',
  address: '',
  projectName: '',
  accountStatement: 'Active'
}));

// Load projects on component mount
onMounted(async () => {
  await projectsFacade.fetchProjects();
  projects.value = projectsFacade.getProjects();
});

// Computed property to format projects for dropdown
const projectOptions = computed(() => {
  return projects.value.map(project => ({
    label: project.name,
    value: project.id
  }));
});

watch(() => props.visible, (newVal) => {
  localVisible.value = newVal;
  if (newVal) {
    // Reset form when dialog opens
    formData.value = new Client({
      fullName: '',
      email: '',
      phoneNumber: '',
      address: '',
      projectId: 0,
      projectName: '',
      accountStatement: 'Active'
    });
  }
});

watch(localVisible, (newVal) => {
  emit('update:visible', newVal);
});

// Watch for project selection to update projectName
watch(() => formData.value.projectId, (newProjectId) => {
  if (newProjectId) {
    formData.value.projectName = projectsFacade.getProjectNameById(newProjectId);
  } else {
    formData.value.projectName = '';
  }
});

const handleSave = () => {
  // Validar que el nombre y email no estén vacíos
  if (!formData.value.fullName || !formData.value.email) {
    alert('Please fill in at least Full Name and Email');
    return;
  }

  emit('save', formData.value);
  localVisible.value = false;
};

const handleCancel = () => {
  localVisible.value = false;
};
</script>

<template>
  <pv-dialog
    v-model:visible="localVisible"
    modal
    header="Add New Client"
    :style="{ width: '600px' }"
    class="client-add-dialog"
  >
    <div class="grid">
      <div class="col-12 mb-3">
        <label for="fullName" class="block mb-2 font-semibold">Full Name *</label>
        <pv-input-text
          id="fullName"
          v-model="formData.fullName"
          class="w-full"
          placeholder="Enter full name"
        />
      </div>

      <div class="col-12 mb-3">
        <label for="email" class="block mb-2 font-semibold">Email *</label>
        <pv-input-text
          id="email"
          v-model="formData.email"
          class="w-full"
          type="email"
          placeholder="Enter email address"
        />
      </div>

      <div class="col-12 mb-3">
        <label for="phoneNumber" class="block mb-2 font-semibold">Phone Number</label>
        <pv-input-text
          id="phoneNumber"
          v-model="formData.phoneNumber"
          class="w-full"
          placeholder="Enter phone number"
        />
      </div>

      <div class="col-12 mb-3">
        <label for="address" class="block mb-2 font-semibold">Address</label>
        <pv-input-text
          id="address"
          v-model="formData.address"
          class="w-full"
          placeholder="Enter address"
        />
      </div>

      <div class="col-12 mb-3">
        <label for="projectId" class="block mb-2 font-semibold">Project</label>
        <pv-select
          id="projectId"
          v-model="formData.projectId"
          :options="projectOptions"
          optionLabel="label"
          optionValue="value"
          placeholder="Select a project"
          class="w-full"
          :disabled="projectOptions.length === 0"
        />
      </div>
    </div>

    <template #footer>
      <pv-button
        label="Cancel"
        icon="pi pi-times"
        @click="handleCancel"
        severity="danger"
        outlined
      />
      <pv-button
        label="Add Client"
        icon="pi pi-check"
        @click="handleSave"
        severity="success"
      />
    </template>
  </pv-dialog>
</template>

<style scoped>
/* Forzar fondo blanco en el diálogo principal */
:deep(.p-dialog) {
  background: white !important;
}

:deep(.p-dialog .p-dialog-header) {
  background: white !important;
  color: #111827 !important;
}

:deep(.p-dialog .p-dialog-content) {
  background: white !important;
  color: #111827 !important;
}

:deep(.p-dialog .p-dialog-footer) {
  background: white !important;
}

/* Estilos para inputs */
:deep(.p-inputtext) {
  background: white !important;
  color: #111827 !important;
  border-color: #d1d5db !important;
}

:deep(.p-inputtext:enabled:hover) {
  background: white !important;
  border-color: #9ca3af !important;
}

:deep(.p-inputtext:enabled:focus) {
  background: white !important;
  border-color: #3b82f6 !important;
  box-shadow: 0 0 0 0.2rem rgba(59, 130, 246, 0.25) !important;
}

/* Estilos para el select */
:deep(.p-select) {
  background: white !important;
  color: #111827 !important;
  border-color: #d1d5db !important;
}

:deep(.p-select:hover) {
  background: white !important;
  border-color: #9ca3af !important;
}

:deep(.p-select:focus) {
  background: white !important;
  border-color: #3b82f6 !important;
  box-shadow: 0 0 0 0.2rem rgba(59, 130, 246, 0.25) !important;
}

:deep(.p-select .p-select-label) {
  background: white !important;
  color: #111827 !important;
}

:deep(.p-select .p-select-dropdown) {
  background: white !important;
  color: #111827 !important;
}

:deep(label) {
  color: #374151 !important;
}

/* Grid del formulario */
:deep(.grid) {
  background: white !important;
}

/* Estilos para los botones del footer */
:deep(.p-button) {
  color: white !important;
}

:deep(.p-button.p-button-danger.p-button-outlined) {
  background: #fee2e2 !important;
  border-color: #ef4444 !important;
  color: #dc2626 !important;
}

:deep(.p-button.p-button-danger.p-button-outlined:hover) {
  background: #fecaca !important;
  border-color: #dc2626 !important;
  color: #991b1b !important;
}

:deep(.p-button-success) {
  background: #10b981 !important;
  border-color: #10b981 !important;
  color: white !important;
}

:deep(.p-button-success:hover) {
  background: #059669 !important;
  border-color: #059669 !important;
  color: white !important;
}
</style>

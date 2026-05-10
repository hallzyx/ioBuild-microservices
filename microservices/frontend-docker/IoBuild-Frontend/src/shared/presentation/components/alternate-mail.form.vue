<template>
  <Dialog
      v-model:visible="visible"
      :header="t('configuration.email-form.title')"
      :modal="true"
      :style="{ width: '420px' }"
      :breakpoints="{ '576px': '90vw' }"
      @update:visible="onDialogVisibleChange"
  >
    <div class="dialog-content">
      <div class="dialog-icon-wrapper">
        <span class="dialog-icon pi pi-envelope"></span>
      </div>

      <div class="dialog-text">
        <h3 class="dialog-title">{{ t('configuration.security.alternate') }}</h3>
        <p class="dialog-description">
          {{ t('configuration.email-form.description') }}
        </p>
      </div>

      <div class="field">
        <label for="alternateEmail" class="field-label">{{ t('configuration.email-form.placeholder') }}</label>
        <InputText
            id="alternateEmail"
            v-model="email"
            type="email"
            autocomplete="email"
            :placeholder="t('configuration.email-form.placeholder')"
            :class="['field-input', { 'p-invalid': error }]"
            :disabled="loading"
        />
        <small v-if="error" class="p-error">{{ error }}</small>
      </div>
    </div>

    <template #footer>
      <div class="dialog-footer">
        <Button
            :label="t('configuration.email-form.cancel')"
            icon="pi pi-times"
            text
            class="btn-cancel"
            @click="handleCancel"
            :disabled="loading"
        />
        <Button
            :label="t('configuration.email-form.save')"
            icon="pi pi-check"
            class="btn-save"
            @click="handleSave"
            :loading="loading"
        />
      </div>
    </template>
  </Dialog>
</template>

<script setup>
import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import Dialog from 'primevue/dialog';
import InputText from 'primevue/inputtext';
import Button from 'primevue/button';

const { t } = useI18n();

const props = defineProps({
  visible: {
    type: Boolean,
    default: false
  },
  modelValue: {
    type: String,
    default: ''
  }
});

// El padre controla visibilidad y valor inicial; este componente maneja solo el estado de edición local y validaciones.
const emit = defineEmits(['update:visible', 'save', 'cancel']);

const email = ref(props.modelValue || '');
const error = ref('');
const loading = ref(false);

// Computed-like getter para enlazar el v-model:visible al prop controlado por el padre
const visible = computed({
  get: () => props.visible,
  set: (value) => emit('update:visible', value)
});

watch(
    () => props.modelValue,
    (newVal) => {
      // Solo sincronizamos desde el padre cuando el diálogo está visible,
      // para no sobrescribir el reseteo local tras guardar/cancelar.
      if (visible.value) {
        email.value = newVal || '';
      }
    }
);

watch(email, () => {
  // Al modificar el email limpiamos el mensaje de error para una mejor UX
  if (error.value) {
    error.value = '';
  }
});

const validate = () => {
  error.value = '';
  const value = email.value.trim();
  if (!value) {
    error.value = t('configuration.email-form.emailRequired');
    return false;
  }
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  if (!emailRegex.test(value)) {
    error.value = t('configuration.email-form.emailInvalid');
    return false;
  }
  return true;
};

const resetForm = () => {
  // Siempre limpiamos el input cuando se cierra o se cancela
  email.value = '';
  error.value = '';
};

const handleSave = async () => {
  if (!validate()) return;
  loading.value = true;

  try {
    await new Promise((resolve) => setTimeout(resolve, 300));

    const trimmed = email.value.trim();
    emit('save', trimmed);
    resetForm();
    emit('update:visible', false);
  } catch (err) {
    error.value = t('configuration.email-form.errorSaving');
  } finally {
    loading.value = false;
  }
};

const handleCancel = () => {
  resetForm();
  emit('update:visible', false);
  emit('cancel');
};

const onDialogVisibleChange = (value) => {
  if (!value) {
    // Si se cierra por click fuera o por la X del diálogo, tratamos como cancelación
    handleCancel();
  } else {
    emit('update:visible', true);
  }
};
</script>

<style scoped>
.dialog-content {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
  background: linear-gradient(135deg, #f8fafc 0%, #eff6ff 100%);
  border-radius: 18px;
  padding: 24px 20px 16px 20px;
  box-shadow: 0 10px 30px rgba(15, 23, 42, 0.08);
}

.dialog-icon-wrapper {
  width: 48px;
  height: 48px;
  border-radius: 16px;
  background: rgba(59, 130, 246, 0.08);
  display: flex;
  align-items: center;
  justify-content: center;
}

.dialog-icon {
  font-size: 1.4rem;
  color: #2563eb;
}

.dialog-text {
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
}

.dialog-title {
  margin: 0;
  font-size: 1.1rem;
  font-weight: 700;
  color: #0f172a;
}

.dialog-description {
  margin: 0;
  font-size: 0.9rem;
  color: #6b7280;
  line-height: 1.4;
}

.field {
  margin-top: 0.5rem;
}

.field-label {
  font-size: 0.85rem;
  font-weight: 600;
  color: #374151;
  margin-bottom: 6px;
  display: block;
}

.field-input {
  width: 100%;
  height: 44px;
  font-size: 0.95rem;
  border-radius: 12px;
  border: 1px solid #d1d5db;
  padding: 0 14px;
  background: #ffffff;
  color: #111827;
  box-sizing: border-box;
  transition: border-color 0.2s, box-shadow 0.2s, background-color 0.2s;
}

.field-input:focus {
  border-color: #2563eb;
  box-shadow: 0 0 0 1px rgba(37, 99, 235, 0.2);
  outline: none;
}

.p-error {
  color: #ef4444;
  font-size: 0.8rem;
  margin-top: 6px;
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 0.75rem;
}

.btn-cancel {
  color: #6b7280;
}

.btn-save {
  background: linear-gradient(135deg, #22c55e, #16a34a);
  border: none;
}

.btn-save:enabled:hover {
  background: linear-gradient(135deg, #16a34a, #15803d);
}

.p-dialog .p-dialog-content {
  padding: 0;
}

.p-dialog .p-dialog_header {
  border-top-left-radius: 18px;
  border-top-right-radius: 18px;
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif !important;
  font-size: 20px !important;
  font-weight: 700 !important;
  color: #1e293b !important;
  letter-spacing: 0.01em;
}

.p-dialog .p-dialog-footer {
  border-bottom-left-radius: 18px;
  border-bottom-right-radius: 18px;
  padding-bottom: 18px;
  padding-right: 18px;
}

.p-button {
  border-radius: 10px !important;
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
  font-size: 0.9rem;
  font-weight: 600;
  padding: 0.6rem 1.2rem;
}
</style>
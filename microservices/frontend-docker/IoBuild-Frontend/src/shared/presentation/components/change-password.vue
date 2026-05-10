<script setup>
import { computed, ref, watch, computed as vueComputed } from 'vue'
import Dialog from 'primevue/dialog'
import Button from 'primevue/button'
import { useI18n } from 'vue-i18n';
import { useIamStore } from '../../../iam/application/iam.store.js';

const { t } = useI18n();
const iamStore = useIamStore();

const props = defineProps({
  visible: {
    type: Boolean,
    default: false,
  },
})

const emit = defineEmits(['update:visible', 'close'])

const visible = vueComputed({
  get: () => props.visible,
  set: (value) => emit('update:visible', value),
})

const currentPassword = ref('')
const newPassword = ref('')
const confirmPassword = ref('')

const errors = ref({
  currentPassword: '',
  newPassword: '',
  confirmPassword: '',
  form: '',
})

const isSubmitting = ref(false)

const passwordStrength = computed(() => {
  const pwd = newPassword.value || ''
  if (!pwd) return { score: 0, label: 'Too weak', level: 'empty' }

  let score = 0

  if (pwd.length >= 8) score += 1
  if (pwd.length >= 12) score += 1
  if (/[A-Z]/.test(pwd)) score += 1
  if (/[0-9]/.test(pwd)) score += 1
  if (/[^A-Za-z0-9]/.test(pwd)) score += 1

  if (score <= 1) return { score, label: 'Weak', level: 'weak' }
  if (score <= 3) return { score, label: 'Medium', level: 'medium' }
  return { score, label: 'Strong', level: 'strong' }
})

const strengthPercentage = computed(() => {
  switch (passwordStrength.value.level) {
    case 'weak':
      return 33
    case 'medium':
      return 66
    case 'strong':
      return 100
    default:
      return 0
  }
})

watch(visible, (value) => {
  if (!value) {
    resetForm()
  }
})

function resetForm() {
  currentPassword.value = ''
  newPassword.value = ''
  confirmPassword.value = ''
  errors.value = {
    currentPassword: '',
    newPassword: '',
    confirmPassword: '',
    form: '',
  }
  isSubmitting.value = false
}

function validateForm() {
  errors.value = {
    currentPassword: '',
    newPassword: '',
    confirmPassword: '',
    form: '',
  }

  let isValid = true

  if (!currentPassword.value) {
    errors.value.currentPassword = 'Current password is required.'
    isValid = false
  }

  if (!newPassword.value) {
    errors.value.newPassword = 'New password is required.'
    isValid = false
  } else if (newPassword.value.length < 8) {
    errors.value.newPassword = 'New password must be at least 8 characters.'
    isValid = false
  }

  if (!confirmPassword.value) {
    errors.value.confirmPassword = 'Please confirm your new password.'
    isValid = false
  } else if (confirmPassword.value !== newPassword.value) {
    errors.value.confirmPassword = 'Passwords do not match.'
    isValid = false
  }

  return isValid
}

async function onSubmit() {
  if (!validateForm()) return

  isSubmitting.value = true

  try {
    await iamStore.changePassword(currentPassword.value, newPassword.value, confirmPassword.value)
    visible.value = false
    emit('close')
  } catch (error) {
    errors.value.form = t('configuration.password-form.error') || 'There was a problem updating your password. Please try again.'
  } finally {
    isSubmitting.value = false
  }
}

function handleCancel() {
  visible.value = false
  emit('close')
}

function onDialogVisibleChange(value) {
  if (!value) {
    handleCancel()
  } else {
    emit('update:visible', true)
  }
}
</script>

<template>
  <Dialog
      v-model:visible="visible"
      :header="t('configuration.password-form.title')"
      :modal="true"
      :style="{ width: '420px' }"
      :breakpoints="{ '576px': '90vw' }"
      @update:visible="onDialogVisibleChange"
  >
    <div class="dialog-content">
      <div class="dialog-icon-wrapper">
        <span class="dialog-icon pi pi-lock"></span>
      </div>

      <div class="dialog-text">
        <h3 class="dialog-title">{{ t('configuration.security.password') }}</h3>
        <p class="dialog-description">
          {{ t('configuration.password-form.description') }}
        </p>
      </div>

      <form class="card-body" @submit.prevent="onSubmit">
        <div class="form-group">
          <label for="currentPassword" class="label">{{ t('configuration.password-form.current') }}</label>
          <input
              id="currentPassword"
              v-model="currentPassword"
              type="password"
              class="input"
              autocomplete="current-password"
          />
          <p v-if="errors.currentPassword" class="error">{{ errors.currentPassword }}</p>
        </div>

        <div class="form-group">
          <label for="newPassword" class="label">{{ t('configuration.password-form.new') }}</label>
          <input
              id="newPassword"
              v-model="newPassword"
              type="password"
              class="input"
              autocomplete="new-password"
          />
          <div class="strength-wrapper">
            <div class="strength-bar">
              <div
                  class="strength-bar-fill"
                  :class="`strength--${passwordStrength.level}`"
                  :style="{ width: strengthPercentage + '%' }"
              ></div>
            </div>
            <span class="strength-label">{{ passwordStrength.label }}</span>
          </div>
          <p v-if="errors.newPassword" class="error">{{ errors.newPassword }}</p>
        </div>

        <div class="form-group">
          <label for="confirmPassword" class="label">{{ t('configuration.password-form.confirm') }}</label>
          <input
              id="confirmPassword"
              v-model="confirmPassword"
              type="password"
              class="input"
              autocomplete="new-password"
          />
          <p v-if="errors.confirmPassword" class="error">{{ errors.confirmPassword }}</p>
        </div>

        <p v-if="errors.form" class="error form-error">{{ errors.form }}</p>
      </form>
    </div>

    <template #footer>
      <div class="dialog-footer">
        <Button
            :label="t('configuration.password-form.cancel')"
            icon="pi pi-times"
            text
            class="btn-cancel"
            @click="handleCancel"
            :disabled="isSubmitting"
        />
        <Button
            :label="t('configuration.password-form.save')"
            icon="pi pi-check"
            class="btn-save"
            @click="onSubmit"
            :loading="isSubmitting"
        />
      </div>
    </template>
  </Dialog>
</template>

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

.card-body {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.label {
  font-size: 0.875rem;
  font-weight: 500;
  color: #0f172a;
}

.input {
  padding: 0.5rem 0.75rem;
  border-radius: 0.5rem;
  border: 1px solid #cbd5e1;
  font-size: 0.875rem;
  outline: none;
  transition: border-color 0.15s ease, box-shadow 0.15s ease;
}

.input:focus {
  border-color: #3b82f6;
  box-shadow: 0 0 0 1px rgba(59, 130, 246, 0.4);
}

.error {
  font-size: 0.75rem;
  color: #b91c1c;
}

.form-error {
  margin-top: 0.5rem;
}

.strength-wrapper {
  margin-top: 0.5rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.strength-bar {
  flex: 1;
  height: 0.35rem;
  border-radius: 999px;
  background-color: #e2e8f0;
  overflow: hidden;
}

.strength-bar-fill {
  height: 100%;
  width: 0;
  transition: width 0.2s ease, background-color 0.2s ease;
}

.strength-label {
  font-size: 0.75rem;
  color: #64748b;
  min-width: 3rem;
  text-align: right;
}

.strength--empty {
  background-color: transparent;
}

.strength--weak {
  background-color: #f97316;
}

.strength--medium {
  background-color: #eab308;
}

.strength--strong {
  background-color: #22c55e;
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
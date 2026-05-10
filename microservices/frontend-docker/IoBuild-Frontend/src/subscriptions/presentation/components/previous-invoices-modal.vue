<script setup>
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const props = defineProps({
  visible: { type: Boolean, default: false },
  invoices: { type: Array, default: () => [] },
  loading: { type: Boolean, default: false },
  error: { type: String, default: '' }
});

const emit = defineEmits(['close']);
</script>

<template>
  <pv-dialog
    :visible="props.visible"
    @update:visible="emit('close')"
    :modal="true"
    :style="{ width: '600px' }"
    :header="t('subscriptions.previous-invoices')"
  >
    <div v-if="props.loading" class="flex justify-center items-center py-4">
      <pv-progress-spinner />
    </div>
    <div v-else>
      <div v-if="props.error" class="text-red-500 mb-3">{{ props.error }}</div>
      <div v-if="!props.invoices.length" class="text-gray-500">
        {{ t('subscriptions.no-invoices') }}
      </div>
      <div v-else class="space-y-3">
        <div
          v-for="invoice in props.invoices"
          :key="invoice.id"
          class="border rounded-lg p-3 flex justify-between items-center bg-white"
        >
          <div>
            <div class="font-semibold">{{ invoice.period || invoice.id }}</div>
            <div class="text-sm text-gray-600">
              {{ t('subscriptions.invoice-due-date-label') }}:
              <span>{{ invoice.dueDate || invoice.date }}</span>
            </div>
            <div class="text-sm text-gray-600" v-if="invoice.billingStart || invoice.billingEnd">
              {{ t('subscriptions.invoice-billing-period-label') }}:
              <span>{{ invoice.billingStart }} - {{ invoice.billingEnd }}</span>
            </div>
          </div>
          <div class="text-right">
            <div class="font-bold">
              {{ invoice.currency || '$' }} {{ invoice.amount?.toFixed ? invoice.amount.toFixed(2) : invoice.amount }}
            </div>
            <div class="text-xs" :class="invoice.status === 'paid' ? 'text-green-600' : 'text-yellow-600'">
              {{ invoice.status === 'paid' ? t('subscriptions.invoice-status-paid') : t('subscriptions.invoice-status-pending') }}
            </div>
            <a
              v-if="invoice.downloadUrl && invoice.downloadUrl !== '#'"
              :href="invoice.downloadUrl"
              target="_blank"
              rel="noopener noreferrer"
              class="text-xs text-blue-600 underline mt-1 inline-block"
            >
              {{ t('subscriptions.invoice-view-details') }}
            </a>
          </div>
        </div>
      </div>
    </div>
  </pv-dialog>
</template>

<style scoped>
</style>
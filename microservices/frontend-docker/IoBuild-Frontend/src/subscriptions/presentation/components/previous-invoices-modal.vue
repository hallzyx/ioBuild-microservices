<script setup>
const props = defineProps({
  visible: { type: Boolean, default: false },
  invoices: { type: Array, default: () => [] },
  loading: { type: Boolean, default: false },
  error: { type: String, default: '' }
});

const emit = defineEmits(['close']);

const openReceipt = (url) => {
  if (url) window.open(url, '_blank');
};
</script>

<template>
  <pv-dialog
    :visible="props.visible"
    @update:visible="emit('close')"
    :modal="true"
    :style="{ width: '700px' }"
    header="My Invoices"
  >
    <div v-if="props.loading" class="flex justify-content-center align-items-center py-8">
      <pv-progress-spinner />
    </div>

    <div v-else>
      <div v-if="props.error" class="text-red-500 mb-3">{{ props.error }}</div>

      <div v-if="!props.invoices.length && !props.error" class="text-center text-gray-500 py-6">
        No invoices yet.
      </div>

      <pv-data-table
        v-else-if="props.invoices.length"
        :value="props.invoices"
        class="p-datatable-sm"
        striped-rows
      >
        <pv-column field="date" header="Date" style="min-width: 120px" />
        <pv-column field="description" header="Description" style="min-width: 140px" />
        <pv-column header="Amount" style="min-width: 110px">
          <template #body="{ data }">
            {{ data.currency }} {{ Number(data.amount).toFixed(2) }}
          </template>
        </pv-column>
        <pv-column header="Status" style="min-width: 90px">
          <template #body="{ data }">
            <span :class="data.status === 'paid' ? 'text-green-600 font-semibold' : 'text-yellow-600 font-semibold'">
              {{ data.status === 'paid' ? 'Paid' : data.status }}
            </span>
          </template>
        </pv-column>
        <pv-column header="Receipt" style="min-width: 100px">
          <template #body="{ data }">
            <pv-button
              v-if="data.downloadUrl"
              label="Download"
              icon="pi pi-download"
              size="small"
              text
              @click="openReceipt(data.downloadUrl)"
            />
            <span v-else class="text-gray-400 text-sm">—</span>
          </template>
        </pv-column>
      </pv-data-table>
    </div>
  </pv-dialog>
</template>

<style scoped>
</style>

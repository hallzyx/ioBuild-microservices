<template>
  <div class="toggle-item">
    <div class="label">{{ label }}</div>
    <pv-input-switch v-model="isActive" class="p-inputswitch-sm" />
  </div>
</template>

<script setup>
import { defineProps, defineModel } from 'vue';

defineProps({
  label: String,
});

const isActive = defineModel('isActive', {
    type: Boolean,
    required: true
});
</script>

<style scoped>
.toggle-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 15px;
  background-color: #f6fff6;
  border-radius: 6px;
  border: 1px solid #e0e0e0;
}
.label { font-size: 1em; color: #333; }
</style>
<template>
  <div class="account-info-section">
    <pv-float-label class="field">
      <pv-input-text :modelValue="info.fullName" :placeholder="$t('profile.infoFullName')" readonly class="p-inputtext-sm"/>
      <label>{{ $t('profile.infoFullName') }}</label>
    </pv-float-label>

    <pv-float-label class="field">
      <pv-input-text :modelValue="info.email" :placeholder="$t('profile.infoEmail')" readonly class="p-inputtext-sm"/>
      <label>{{ $t('profile.infoEmail') }}</label>
    </pv-float-label>

    <pv-float-label class="field">
      <pv-input-text :modelValue="info.phone" :placeholder="$t('profile.infoPhone')" readonly class="p-inputtext-sm"/>
      <label>{{ $t('profile.infoPhone') }}</label>
    </pv-float-label>

    <pv-float-label class="field" v-if="info.address">
      <pv-input-text :modelValue="info.address" :placeholder="$t('profile.infoAddress')" readonly class="p-inputtext-sm"/>
      <label>{{ $t('profile.infoAddress') }}</label>
    </pv-float-label>
  </div>
</template>

<script setup>
import { defineProps } from 'vue';
defineProps({
  info: Object,
});
</script>

<style scoped>
.field { margin-bottom: 15px; width: 100%; }
.p-inputtext-sm { width: 100%; padding: 10px 12px; }
</style>
<template>
  <div class="language-section">
    <pv-float-label>
      <pv-input-text :modelValue="language" readonly />
      <label>{{ $t('profile.sectionAppLanguage') }}</label>
    </pv-float-label>
  </div>
</template>

<script setup>
import { defineProps } from 'vue';
defineProps({
  language: String,
});
</script>

<style scoped>
.language-section { width: 100%; }
</style>
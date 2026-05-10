<script setup>
import {useI18n} from "vue-i18n";
import { computed, ref} from "vue";

const { t, locale, availableLocales } = useI18n();

const languageOptions = computed(() => {
  return availableLocales.map(lang => ({
    label: lang === 'en' ? 'EN' : 'ES',
    value: lang
  }));
});
const selectRef = ref(null);

</script>

<template>
  <div class="language-select-wrapper">
    <pv-select
      ref="selectRef"
      v-model="locale"
      :options="languageOptions"
      option-label="label"
      option-value="value"
      class="language-select custom-green-select"
      placeholder="Idioma"
    />
  </div>
</template>

<style>
</style>
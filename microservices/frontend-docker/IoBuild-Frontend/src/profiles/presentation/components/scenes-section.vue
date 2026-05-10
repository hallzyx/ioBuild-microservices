<template>
  <div class="scenes-section">
    <ToggleItem
      :label="$t('profile.sceneEnergySaving')"
      v-model:is-active="scenesState.energySaving"
    />

    <ToggleItem
      :label="$t('profile.sceneMovieMode')"
      v-model:is-active="scenesState.movieMode"
    />

    <ToggleItem
      :label="$t('profile.scenePushNotif1')"
      v-model:is-active="scenesState.pushNotifications1"
    />

    <ToggleItem
      :label="$t('profile.scenePushNotif2')"
      v-model:is-active="scenesState.pushNotifications2"
    />

    <ToggleItem
      :label="$t('profile.scenePushNotif3')"
      v-model:is-active="scenesState.pushNotifications3"
    />
  </div>
</template>

<script setup>
import { defineModel } from 'vue';
import ToggleItem from './toggle-item.vue';

const scenesState = defineModel('scenesState', {
    type: Object,
    required: true
});
</script>

<style scoped>
.scenes-section { display: flex; flex-direction: column; gap: 10px; }
</style>
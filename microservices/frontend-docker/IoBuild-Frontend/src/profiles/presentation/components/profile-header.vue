<template>
  <div class="profile-header">
    <div class="profile-avatar">
      <img :src="avatarUrl || 'https://via.placeholder.com/60'" alt="Avatar" class="avatar-img">
    </div>
    <div class="profile-info">
      <h2 class="name">{{ name }}</h2>
      <p class="role">{{ role }}</p>
    </div>
    <pv-button
      :label="$t('profile.editButton')"
      class="p-button-success edit-button"
      @click="$emit('editProfile')"
    />
  </div>
</template>

<script setup>
import { defineProps, defineEmits } from 'vue';

defineProps({
  name: String,
  role: String,
  avatarUrl: String,
});

defineEmits(['editProfile']);
</script>

<style scoped>
.profile-header {
  display: flex;
  align-items: center;
  background-color: #e6ffe6;
  padding: 15px 25px;
  border-radius: 8px;
  margin-bottom: 20px;
}
.profile-avatar { margin-right: 15px; }
.avatar-img { width: 60px; height: 60px; border-radius: 50%; object-fit: cover; }
.profile-info { flex-grow: 1; }
.name { font-size: 1.4em; margin: 0; font-weight: 600; color: #1e1e1e; }
.role { font-size: 1em; margin: 0; color: #4c4c4c; text-transform: capitalize; }
.edit-button {
  background-color: #14a04d;
  border: none;
  font-weight: bold;
}
</style>
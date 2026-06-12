<template>
  <div class="profile-header">
    <div class="profile-avatar">
      <img :src="avatarUrl || 'data:image/svg+xml,%3Csvg xmlns=%22http://www.w3.org/2000/svg%22 width=%2260%22 height=%2260%22%3E%3Ccircle cx=%2230%22 cy=%2230%22 r=%2230%22 fill=%22%23CCCCCC%22/%3E%3Ctext x=%2230%22 y=%2236%22 text-anchor=%22middle%22 fill=%22white%22 font-size=%2224%22 font-family=%22sans-serif%22%3E%3F%3C/text%3E%3C/svg%3E'" alt="Avatar" class="avatar-img">
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
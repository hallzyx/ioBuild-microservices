<script setup>
import { ref, computed, onMounted } from "vue";
import { useI18n } from "vue-i18n";
import { useRoute, useRouter } from "vue-router";
import useProjectStore from "../../application/project.store.js";
import { Project } from "../../domain/model/project.entity.js";

const { t } = useI18n();
const route = useRoute();
const router = useRouter();
const store = useProjectStore();

const form = ref({
  name: "",
  description: "",
  location: "",
  totalUnits: 0,
  occupiedUnits: 0,
  imageUrl: ""
});
const isEdit = computed(() => !!route.params.id);

onMounted(() => {
  if (isEdit.value) {
    const existing = store.getProjectById(route.params.id);
    if (existing) form.value = { ...existing };
    else router.push({ name: "projects-management" });
  }

  // Cargar Cloudinary widget
  loadCloudinaryScript();
});

const loadCloudinaryScript = () => {
  if (!window.cloudinary) {
    const script = document.createElement('script');
    script.src = 'https://widget.cloudinary.com/v2.0/global/all.js';
    script.type = 'text/javascript';
    document.head.appendChild(script);
  }
};

const cloudinaryName = import.meta.env.VITE_CLOUDINARY_CLOUD_NAME;
const cloudinaryPreset = import.meta.env.VITE_CLOUDINARY_UPLOAD_PRESET;

const openUploadModal = () => {
  if (!window.cloudinary) {
    console.error('Cloudinary widget not loaded');
    return;
  }

  window.cloudinary.openUploadWidget(
    {
      cloud_name: cloudinaryName,
      upload_preset: cloudinaryPreset
    },
    (error, result) => {
      if (!error && result && result.event === "success") {
        form.value.imageUrl = result.info.secure_url || result.info.url;
      }
    }
  ).open();
};

const save = async () => {
  const project = new Project({
    id: isEdit.value ? route.params.id : null,
    ...form.value,
  });
  if (isEdit.value) await store.updateProject(project);
  else await store.addProject(project);
  router.push({ name: "projects-management" });
};

const cancel = () => {
  if (isEdit.value) {
    router.push({ name: "projects-management-details", params: { id: route.params.id } });
  } else {
    router.push({ name: "projects-management" });
  }
};
</script>


<template>
  <div class="p-4 lg:p-4 bg-gray-50 min-h-screen">
    <div class="max-w-3xl mx-auto">
      <!-- Header -->
      <div class="mb-6">
        <div class="flex items-center gap-2 text-sm text-gray-600 mb-2">
          <i class="pi pi-home"></i>
          <span>/</span>
          <span class="text-emerald-600 font-medium">{{ isEdit ? t("projects.edit-title") : t("projects.new-title") }}</span>
        </div>
        <h1 class="text-2xl lg:text-3xl font-bold text-gray-900">
          {{ isEdit ? t("projects.edit-title") : t("projects.new-title") }}
        </h1>
      </div>

      <!-- Form Card -->
      <div class="bg-white rounded-lg shadow-sm border border-gray-200 p-6 lg:p-4">
        <form @submit.prevent="save">
          <!-- Name Field -->
          <div class="form-field mb-4">
            <label class="form-label">
              <i class="pi pi-tag text-emerald-600 mr-2"></i>
              {{ t("projects.fields.name") }}
              <span class="text-red-500 ml-1">*</span>
            </label>
            <pv-input-text
                v-model="form.name"
                class="w-full input-enhanced"
                required
                :placeholder="t('projects.fields.name-placeholder')"
            />
          </div>

          <!-- Description Field -->
          <div class="form-field mb-4">
            <label class="form-label">
              <i class="pi pi-align-left text-emerald-600 mr-2"></i>
              {{ t("projects.fields.description") }}
            </label>
            <pv-textarea
                v-model="form.description"
                class="w-full input-enhanced"
                rows="3"
                :placeholder="t('projects.fields.description-placeholder')"
            />
          </div>

          <!-- Location Field -->
          <div class="form-field mb-4">
            <label class="form-label">
              <i class="pi pi-map-marker text-emerald-600 mr-2"></i>
              {{ t("projects.fields.location") }}
            </label>
            <pv-input-text
                v-model="form.location"
                class="w-full input-enhanced"
                :placeholder="t('projects.fields.location-placeholder')"
            />
          </div>

          <!-- Units Fields -->
          <div class="form-field mb-4">
            <label class="form-label">
              <i class="pi pi-building text-emerald-600 mr-2"></i>
              {{ t("projects.fields.total-units") }}
            </label>
            <pv-input-number
                v-model="form.totalUnits"
                :min="0"
                class="w-full input-enhanced"
                placeholder="0"
            />
          </div>

          <div v-if="isEdit" class="form-field mb-4">
            <label class="form-label">
              <i class="pi pi-check-circle text-emerald-600 mr-2"></i>
              {{ t("projects.fields.occupied-units") }}
            </label>
            <pv-input-number
                v-model="form.occupiedUnits"
                :min="0"
                class="w-full input-enhanced"
                placeholder="0"
            />
          </div>

          <!-- Image URL Field -->
          <div class="form-field mb-4">
            <label class="form-label">
              <i class="pi pi-image text-emerald-600 mr-2"></i>
              {{ t("projects.fields.image-url") }}
            </label>
            <pv-button
                :label="t('projects.actions.upload-image')"
                icon="pi pi-cloud-upload"
                @click="openUploadModal"
                class="mb-3"
            />
            <div v-if="form.imageUrl" class="mt-3">
              <img :src="form.imageUrl" alt="Uploaded image" style="max-width: 400px; border-radius: 8px;" />
            </div>
          </div>

          <!-- Action Buttons -->
          <div class="flex gap-3 pt-4 border-t border-gray-200">
            <pv-button
                type="submit"
                :label="t('projects.actions.save')"
                icon="pi pi-check"
                class="custom-green-button flex-1 md:flex-initial"
            />
            <pv-button
                :label="t('projects.actions.cancel')"
                icon="pi pi-times"
                severity="secondary"
                class="cancel-button flex-1 md:flex-initial"
                @click="cancel"
            />
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<style scoped>
.form-field {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.form-label {
  font-size: 0.875rem;
  font-weight: 600;
  color: #374151;
  display: flex;
  align-items: center;
}

:deep(.input-enhanced input),
:deep(.input-enhanced textarea),
:deep(.input-enhanced .p-inputnumber-input) {
  border-color: #e5e7eb !important;
  transition: all 0.2s ease !important;
}

:deep(.input-enhanced input:focus),
:deep(.input-enhanced textarea:focus),
:deep(.input-enhanced .p-inputnumber-input:focus) {
  border-color: #10B981 !important;
  box-shadow: 0 0 0 3px rgba(16, 185, 129, 0.1) !important;
}

:deep(.custom-green-button) {
  background-color: #10B981 !important;
  border-color: #10B981 !important;
  color: white !important;
  transition: all 0.2s ease !important;
  padding: 0.625rem 1.5rem !important;
  font-weight: 600 !important;
}

:deep(.custom-green-button:hover) {
  background-color: #059669 !important;
  border-color: #059669 !important;
  transform: translateY(-1px);
  box-shadow: 0 4px 6px -1px rgba(16, 185, 129, 0.3) !important;
}

:deep(.custom-green-button:focus) {
  box-shadow: 0 0 0 3px rgba(16, 185, 129, 0.3) !important;
}

:deep(.cancel-button) {
  background-color: white !important;
  border-color: #e5e7eb !important;
  color: #6b7280 !important;
  transition: all 0.2s ease !important;
  padding: 0.625rem 1.5rem !important;
  font-weight: 600 !important;
}

:deep(.cancel-button:hover) {
  background-color: #f9fafb !important;
  border-color: #d1d5db !important;
  color: #374151 !important;
}
</style>
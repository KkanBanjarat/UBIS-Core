<script setup lang="ts">
defineProps<{
  title: string
  description?: string
}>()
</script>

<template>
  <div class="flex flex-col gap-1 sm:flex-row sm:items-center sm:justify-between mb-6">
    <div>
      <h1 class="text-xl font-semibold text-base-content">{{ title }}</h1>
      <p v-if="description" class="text-sm text-base-content/60 mt-0.5">{{ description }}</p>
    </div>
    <div class="flex items-center gap-2">
      <slot name="actions" />
    </div>
  </div>
</template>
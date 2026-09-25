<template>
  <div class="drawer lg:drawer-open"> <input id="app-drawer" type="checkbox" class="drawer-toggle" />
    <div class="drawer-content flex min-h-screen flex-col bg-[#f6f8f7]">
      <AppNavbar />
      <main class="min-w-0 flex-1 p-3 sm:p-4 lg:p-5"> <router-view /> </main>
    </div>
    <div class="drawer-side z-40"> <label for="app-drawer" aria-label="close sidebar" class="drawer-overlay"></label>
      <AppSidebar />
    </div>
  </div>
</template>
<script setup lang="ts">
import AppSidebar from './layout/AppSidebar.vue'
import AppNavbar from './layout/AppNavbar.vue' 
</script>
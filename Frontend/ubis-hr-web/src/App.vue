<template>
  <div v-if="!isAuthReady" class="min-h-screen flex flex-col items-center justify-center gap-4 bg-base-100">
    <div class="size-12 rounded-xl bg-gradient-to-br from-primary to-primary/70 flex items-center justify-center text-primary-content font-bold text-lg shadow-md">
      U
    </div>
    <span class="loading loading-spinner loading-md text-primary"></span>
  </div>

  <router-view v-else />
</template>

<script setup lang="ts">
  import { onMounted, ref } from 'vue'
  import { useRouter } from 'vue-router'
  import { useAuthStore } from './stores/authStore'

  const router = useRouter()
  const auth = useAuthStore()
  const isAuthReady = ref(false)

  onMounted(async () => {
    try {
      const loggedIn = await auth.handleSsoRedirect()
      const currentPath = router.currentRoute.value.path

      if (loggedIn && (currentPath === '/login' || currentPath === '/')) {
        await router.replace('/dashboard')   // replace ดีกว่า push (ไม่ทิ้ง History ให้กด Back กลับมาหน้า Login ได้)
      }
    } finally {
      isAuthReady.value = true   // ปลดล็อกให้เรนเดอร์หน้าจริงเมื่อ Auth เสร็จแล้วเท่านั้น
    }
  })
</script>
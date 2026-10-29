<template>
  <main class="login-page">
    <!-- ฝั่งฟอร์ม -->
    <section class="login-main">
      <div class="top-brand">
        <span class="brand-icon">U</span>
        <span>UBIS CORE</span>
      </div>

      <div class="login-content">
        <div class="mobile-brand">Enterprise Business Platform</div>

        <header class="login-heading">
          <span class="eyebrow">WELCOME TO UBIS CORE</span>
          <h1>Sign In</h1>
          <p>เข้าสู่ระบบเพื่อเริ่มต้นการทำงานของคุณ</p>
        </header>

        <!-- Microsoft SSO -->
        <button type="button" class="microsoft-button" @click="onLoginWithMicrosoft">
          <svg width="21" height="21" viewBox="0 0 24 24" aria-hidden="true">
            <rect x="1" y="1" width="10" height="10" fill="#F25022" />
            <rect x="13" y="1" width="10" height="10" fill="#7FBA00" />
            <rect x="1" y="13" width="10" height="10" fill="#00A4EF" />
            <rect x="13" y="13" width="10" height="10" fill="#FFB900" />
          </svg>
          <span>Sign in with Microsoft 365</span>
        </button>

        <div class="login-divider">
          <span>หรือเข้าสู่ระบบด้วยอีเมล</span>
        </div>

        <!-- แสดงฟอร์มทันทีตามภาพตัวอย่าง -->
        <form class="login-form" @submit.prevent="login">
          <div class="form-field">
            <label for="login-email">
              Email <span class="required">*</span>
            </label>

            <input id="login-email" v-model="email" type="email" autocomplete="username" placeholder="name@company.com"
              required :disabled="loading" />
          </div>

          <div class="form-field">
            <label for="login-password">
              Password <span class="required">*</span>
            </label>

            <input id="login-password" v-model="password" type="password" autocomplete="current-password"
              placeholder="กรอกรหัสผ่านของคุณ" required :disabled="loading" />
          </div>

          <div v-if="errorMessage" class="login-error" role="alert" aria-live="polite">
            {{ errorMessage }}
          </div>

          <button type="submit" class="submit-button" :disabled="loading" :aria-busy="loading">
            <span v-if="loading" class="button-spinner"></span>
            <span>{{ loading ? 'กำลังเข้าสู่ระบบ...' : 'เข้าสู่ระบบ' }}</span>

            <svg v-if="!loading" width="19" height="19" viewBox="0 0 24 24" fill="none" stroke="currentColor"
              stroke-width="1.8" aria-hidden="true">
              <path d="M5 12h14M13 6l6 6-6 6" />
            </svg>
          </button>
        </form>

        <div class="access-note">
          <span class="access-dot"></span>
          สำหรับบุคลากรที่ได้รับสิทธิ์เข้าใช้งานจาก IT
        </div>
      </div>

      <footer class="login-footer">
        © {{ new Date().getFullYear() }} UBIS Group.
        <span>All rights reserved.</span>
      </footer>
    </section>

    <!-- ฝั่งแบรนด์ -->
    <aside class="brand-panel" aria-label="UBIS CORE">
      <div class="panel-grid" aria-hidden="true"></div>
      <div class="panel-orbit orbit-one" aria-hidden="true"></div>
      <div class="panel-orbit orbit-two" aria-hidden="true"></div>

      <div class="panel-top">
        <span class="panel-dot"></span>
        ENTERPRISE BUSINESS PLATFORM
      </div>

      <div class="brand-center">
        <div class="brand-emblem">
          <svg viewBox="0 0 120 120" fill="none" aria-hidden="true">
            <path d="M33 30v38c0 17 10 27 27 27s27-10 27-27V30" stroke="currentColor" stroke-width="15"
              stroke-linecap="round" />
            <circle cx="60" cy="28" r="8" fill="currentColor" />
          </svg>
        </div>

        <div class="panel-brand-name">
          UBIS <span>CORE</span>
        </div>

        <div class="panel-brand-caption">
          Enterprise Business Platform
        </div>

        <div class="brand-message">
          <h2>Smarter work.<br />Simpler business.</h2>
          <p>
            เชื่อมโยงข้อมูลและกระบวนการทำงาน<br />
            เพื่อขับเคลื่อนธุรกิจไปด้วยกัน
          </p>
        </div>

        <div class="platform-badge">
          <span class="badge-symbol">U</span>
          <div>
            <span>One platform. Connected teams.</span>
            <strong>Built for UBIS Group</strong>
          </div>
        </div>
      </div>

      <div class="panel-footer">
        <span>CONNECTED · SIMPLE · EFFICIENT</span>
        <span>UBIS GROUP</span>
      </div>
    </aside>
  </main>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '../stores/authStore'

const router = useRouter()
const route = useRoute()
const auth = useAuthStore()

const errorMessage = ref('')
const loading = ref(false)
const email = ref('')
const password = ref('')

async function onLoginWithMicrosoft() {
  errorMessage.value = ''
  try {
    await auth.loginWithSso()
  } catch (err) {
    console.error('Microsoft login failed:', err)
    const e = err as { errorCode?: string; message?: string }
    errorMessage.value = `เข้าสู่ระบบด้วย Microsoft ไม่สำเร็จ: ${e?.errorCode ?? e?.message ?? 'ไม่ทราบสาเหตุ'}`
  }
}

async function login() {
  if (!email.value || !password.value) {
    errorMessage.value = 'กรุณากรอก email และ password'
    return
  }

  loading.value = true
  errorMessage.value = ''

  try {
    await auth.login(email.value, password.value)
    router.replace((route.query.redirect as string) || '/dashboard')
  } catch (err) {
    const e = err as { response?: { data?: { message?: string } } }
    errorMessage.value =
      e?.response?.data?.message ?? 'Email หรือ Password ไม่ถูกต้อง'
  } finally {
    loading.value = false
  }
}
</script>
<style scoped src="../assets/css/Login.css"></style>
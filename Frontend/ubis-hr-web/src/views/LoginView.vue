<template>
  <div
    class="relative min-h-screen overflow-hidden bg-[#f5faf7] flex items-center justify-center px-4 py-8"
  >
    <!-- Background -->
    <div class="pointer-events-none absolute inset-0 overflow-hidden">
      <div
        class="absolute -right-40 -top-40 h-[520px] w-[520px] rounded-full bg-emerald-300/20 blur-3xl"
      ></div>

      <div
        class="absolute -bottom-40 -left-40 h-[480px] w-[480px] rounded-full bg-green-200/20 blur-3xl"
      ></div>

      <div
        class="absolute inset-0 opacity-[0.025]"
        style="
          background-image:
            linear-gradient(#166534 1px, transparent 1px),
            linear-gradient(90deg, #166534 1px, transparent 1px);
          background-size: 40px 40px;
        "
      ></div>
    </div>

    <!-- Main -->
    <div class="relative z-10 w-full max-w-[940px]">
      <div  class="overflow-hidden rounded-[28px] border border-white/80 bg-white/85 shadow-[0_30px_80px_rgba(31,41,55,0.13)] backdrop-blur-xl">
        <div class="grid lg:grid-cols-[0.9fr_1.1fr]">
          <!-- LEFT : Minimal Brand -->
          <div class="relative hidden min-h-[590px] overflow-hidden bg-gradient-to-br from-[#087f45] via-[#10a85a] to-[#28c76f] p-10 lg:flex lg:flex-col lg:justify-between">
            <!-- Soft glow -->
            <div class="absolute -right-32 -top-32 h-80 w-80 rounded-full bg-white/10 blur-3xl"></div>
            <div class="absolute -bottom-40 -left-32 h-96 w-96 rounded-full bg-emerald-950/10 blur-3xl"></div>
            <!-- Abstract circles -->
            <div class="absolute -right-20 bottom-24 size-72 rounded-full border border-white/[0.08]"></div>
            <div class="absolute -right-4 bottom-40 size-44 rounded-full border border-white/[0.08]"></div>
            <div  class="absolute right-20 bottom-24 size-12 rounded-full bg-white/[0.08]"></div>

            <!-- Brand -->
            <div class="relative">
              <div class="flex items-center gap-3">
                <div class="flex size-11 items-center justify-center rounded-2xl bg-white/15 text-lg font-bold text-white ring-1 ring-white/20 backdrop-blur">
                  U
                </div>

                <div>
                  <div class="text-lg font-bold tracking-wide text-white">
                    UBIS CORE
                  </div>
                  <div class="text-[11px] text-white/55">
                    Enterprise Business Platform
                  </div>
                </div>
              </div>
            </div>

            <!-- Center message -->
            <div class="relative max-w-sm">
              <div class="mb-5 h-1 w-10 rounded-full bg-white/70"></div>
              <h2 class="text-[34px] font-bold leading-[1.15] tracking-tight text-white">
                Smarter work.
                <br />
                <span class="text-white/65">Simpler business.</span>
              </h2>

              <p  class="mt-5 max-w-xs text-sm leading-6 text-white/55">
                ศูนย์กลางการทำงานสำหรับการจัดการข้อมูล
                และกระบวนการทางธุรกิจของ UBIS Group
              </p>
            </div>
            <!-- Footer -->
            <div class="relative">
              <div class="text-[10px] font-medium tracking-[0.18em] text-white/35">
                UBIS GROUP
              </div>
            </div>
          </div>

          <!-- RIGHT : Login -->
          <div class="relative flex min-h-[590px] flex-col justify-center px-7 py-10 sm:px-12 lg:px-14">
            <!-- Mobile brand -->
            <div class="mb-8 flex items-center gap-3 lg:hidden">
              <div class="flex size-11 items-center justify-center rounded-2xl bg-emerald-50 text-lg font-bold text-emerald-600 ring-1 ring-emerald-100"
              >
                U
              </div>

              <div>
                <div class="font-bold tracking-wide text-gray-800">
                  UBIS CORE
                </div>

                <div class="text-[11px] text-gray-400">
                  Enterprise Business Platform
                </div>
              </div>
            </div>

            <!-- Heading -->
            <div class="mb-8">
              <div
                class="mb-3 inline-flex items-center gap-1.5 rounded-full border border-emerald-100 bg-emerald-50 px-3 py-1 text-[11px] font-semibold text-emerald-600"
              >
                <span class="size-1.5 rounded-full bg-emerald-500"></span>
                Welcome back
              </div>

              <h1
                class="text-[30px] font-bold tracking-tight text-gray-800"
              >
                ยินดีต้อนรับกลับค่ะ
              </h1>

              <p class="mt-2 text-sm leading-6 text-gray-400">
                เข้าสู่ระบบเพื่อเริ่มต้นการทำงานของคุณ
              </p>
            </div>

            <!-- Microsoft SSO -->
            <button
              type="button"
              class="group flex w-full items-center justify-center gap-3 rounded-2xl border border-gray-200 bg-white px-4 py-3.5 text-sm font-semibold text-gray-700 shadow-sm transition-all duration-200 hover:-translate-y-0.5 hover:border-emerald-200 hover:bg-emerald-50/40 hover:shadow-md active:translate-y-0"
              @click="onLoginWithMicrosoft"
            >
              <div
                class="flex size-7 items-center justify-center rounded-lg border border-gray-100 bg-white shadow-sm"
              >
                <svg width="17" height="17" viewBox="0 0 24 24">
                  <rect x="2" y="2" width="9" height="9" fill="#F25022" />
                  <rect x="2" y="13" width="9" height="9" fill="#00A4EF" />
                  <rect x="13" y="2" width="9" height="9" fill="#7FBA00" />
                  <rect x="13" y="13" width="9" height="9" fill="#FFB900" />
                </svg>
              </div>

              <span>Sign in with Microsoft 365</span>

              <svg
                class="ml-auto size-4 text-gray-300 transition-all duration-200 group-hover:translate-x-0.5 group-hover:text-emerald-500"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                stroke-width="2"
              >
                <path d="m9 18 6-6-6-6" />
              </svg>
            </button>

            <!-- Divider -->
            <div class="my-6 flex items-center gap-3">
              <div class="h-px flex-1 bg-gray-100"></div>

              <span class="text-[11px] font-medium text-gray-300">
                หรือ
              </span>

              <div class="h-px flex-1 bg-gray-100"></div>
            </div>

            <!-- Password toggle -->
            <template v-if="!showPasswordForm">
              <button
                type="button"
                class="group flex w-full items-center justify-between rounded-2xl border border-gray-100 bg-gray-50/70 px-4 py-3.5 text-left transition-all duration-200 hover:border-emerald-200 hover:bg-emerald-50/40"
                @click="showPasswordForm = true"
              >
                <div>
                  <div
                    class="text-sm font-semibold text-gray-600 transition-colors group-hover:text-gray-800"
                  >
                    เข้าสู่ระบบด้วย Email / Password
                  </div>

                  <div class="mt-0.5 text-[11px] text-gray-400">
                    สำหรับบัญชีผู้ใช้งานภายใน
                  </div>
                </div>

                <div
                  class="flex size-8 items-center justify-center rounded-xl bg-white text-gray-300 shadow-sm ring-1 ring-gray-100 transition-all group-hover:bg-emerald-500 group-hover:text-white"
                >
                  <svg
                    class="size-4"
                    viewBox="0 0 24 24"
                    fill="none"
                    stroke="currentColor"
                    stroke-width="2"
                  >
                    <path d="m9 18 6-6-6-6" />
                  </svg>
                </div>
              </button>
            </template>

            <!-- Password form -->
            <form
              v-else
              class="flex flex-col gap-4"
              @submit.prevent="login"
            >
              <!-- Email -->
              <div>
                <label
                  class="mb-2 block text-xs font-semibold text-gray-600"
                >
                  Email
                </label>

                <input
                  v-model="email"
                  type="text"
                  autocomplete="username"
                  placeholder="example@company.com"
                  class="input input-bordered h-12 w-full rounded-xl border-gray-200 bg-white text-sm outline-none transition-all focus:border-emerald-400 focus:ring-4 focus:ring-emerald-500/10"
                />
              </div>

              <!-- Password -->
              <div>
                <label
                  class="mb-2 block text-xs font-semibold text-gray-600"
                >
                  Password
                </label>

                <input
                  v-model="password"
                  type="password"
                  autocomplete="current-password"
                  placeholder="••••••••"
                  class="input input-bordered h-12 w-full rounded-xl border-gray-200 bg-white text-sm outline-none transition-all focus:border-emerald-400 focus:ring-4 focus:ring-emerald-500/10"
                />
              </div>

              <!-- Error -->
              <div
                v-if="errorMessage"
                class="flex items-start gap-2 rounded-xl border border-red-100 bg-red-50 px-3 py-2.5 text-sm text-red-600"
              >
                <svg
                  class="mt-0.5 size-4 shrink-0"
                  viewBox="0 0 24 24"
                  fill="none"
                  stroke="currentColor"
                  stroke-width="2"
                >
                  <circle cx="12" cy="12" r="10" />
                  <path d="M12 8v4" />
                  <path d="M12 16h.01" />
                </svg>

                <span>{{ errorMessage }}</span>
              </div>

              <!-- Submit -->
              <button type="submit"
                class="group flex h-12 w-full items-center justify-center gap-2 rounded-xl bg-gradient-to-r from-[#10b981] to-[#20b85f] text-sm font-semibold text-white shadow-lg shadow-emerald-500/20 transition-all duration-200 hover:-translate-y-0.5 hover:shadow-xl hover:shadow-emerald-500/25 active:translate-y-0 disabled:cursor-not-allowed disabled:opacity-60"
                :disabled="loading">
                <span v-if="loading"class="loading loading-spinner loading-sm"></span>

                <template v-else>
                  <span>เข้าสู่ระบบ</span>
                  <svg
                    class="size-4 transition-transform duration-200 group-hover:translate-x-0.5"
                    viewBox="0 0 24 24"
                    fill="none"
                    stroke="currentColor"
                    stroke-width="2"
                  >
                    <path d="M5 12h14" />
                    <path d="m13 6 6 6-6 6" />
                  </svg>
                </template>
              </button>

              <!-- Back -->
              <button type="button"
                class="py-1 text-center text-xs font-medium text-gray-400 transition-colors hover:text-gray-700"
                @click="showPasswordForm = false">
                ← กลับไปเลือกวิธีเข้าสู่ระบบ
              </button>
            </form>

            <!-- Security note -->
            <div class="mt-8 border-t border-gray-100 pt-5">
              <div class="flex items-start gap-2 text-[11px] leading-5 text-gray-400">
                <svg class="mt-0.5 size-3.5 shrink-0 text-emerald-500"
                  viewBox="0 0 24 24"
                  fill="none"
                  stroke="currentColor"
                  stroke-width="2">
                  <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10Z" />
                  <path d="m9 12 2 2 4-4" />
                </svg>
                <span>ระบบสำหรับบุคลากรที่ได้รับสิทธิ์จาก IT เท่านั้น</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Footer -->
      <div class="mt-5 text-center text-[10px] font-medium tracking-[0.16em] text-gray-400">
        UBIS CORE · SECURE BUSINESS PLATFORM
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '../stores/authStore'

const router = useRouter()
const route = useRoute()
const auth = useAuthStore()

const errorMessage = ref('')
const showPasswordForm = ref(false)
const loading = ref(false)
const email = ref('')
const password = ref('')

async function onLoginWithMicrosoft() {
  await auth.loginWithSso()
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
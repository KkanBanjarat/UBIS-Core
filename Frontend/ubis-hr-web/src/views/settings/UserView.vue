<template>
  <div class="min-h-full bg-base-200/30 p-4 sm:p-6 lg:p-7">
    <div class="mx-auto max-w-[1600px] space-y-5">

      <!-- ==================== Header ==================== -->
      <div class="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <div class="flex items-center gap-2.5">
            <div
              class="flex h-10 w-10 items-center justify-center rounded-xl bg-primary/10 text-primary"
            >
              <i class="bi bi-people-fill text-lg"></i>
            </div>

            <div>
              <h1 class="text-xl font-bold tracking-tight text-base-content sm:text-2xl">
                ผู้ใช้งานระบบ
              </h1>
              <p class="mt-0.5 text-xs text-base-content/45 sm:text-sm">
                จัดการบัญชีผู้ใช้งานและสิทธิ์การเข้าใช้งานระบบ
              </p>
            </div>
          </div>
        </div>

        <div
          class="flex w-fit items-center gap-2 rounded-xl border border-base-300/70 bg-base-100 px-3 py-2 shadow-sm"
        >
          <span class="flex h-7 w-7 items-center justify-center rounded-lg bg-base-200 text-base-content/60">
            <i class="bi bi-person-check"></i>
          </span>

          <div class="leading-tight">
            <div class="text-sm font-semibold text-base-content">
              {{ totalCount }}
            </div>
            <div class="text-[11px] text-base-content/40">
              ผู้ใช้งานทั้งหมด
            </div>
          </div>
        </div>
      </div>

      <!-- ==================== Sync Result ==================== -->
      <div
        v-if="syncResult"
        class="flex items-start gap-3 rounded-xl border border-success/20 bg-success/5 px-4 py-3.5 shadow-sm"
        role="status"
      >
        <div
          class="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-success/10 text-success"
        >
          <i class="bi bi-check-circle-fill"></i>
        </div>

        <div class="min-w-0 flex-1">
          <div class="text-sm font-semibold text-success">
            Sync จาก Microsoft สำเร็จ
          </div>

          <div class="mt-1 text-xs leading-relaxed text-base-content/55">
            ทั้งหมด {{ syncResult.total }} คน
            <span class="mx-1 text-base-content/20">•</span>
            เพิ่มใหม่ {{ syncResult.created }}
            <span class="mx-1 text-base-content/20">•</span>
            อัปเดต {{ syncResult.updated }}
            <span class="mx-1 text-base-content/20">•</span>
            ผูกผู้ใช้เดิม {{ syncResult.linked }}
            <span class="mx-1 text-base-content/20">•</span>
            ไม่เปลี่ยน {{ syncResult.unchanged }}
            <span class="mx-1 text-base-content/20">•</span>
            ข้าม {{ syncResult.skipped }}
          </div>
        </div>

        <button
          class="btn btn-ghost btn-xs btn-circle text-base-content/40 hover:text-error"
          aria-label="ปิดข้อความ"
          @click="syncResult = null"
        >
          <i class="bi bi-x-lg"></i>
        </button>
      </div>

      <!-- ==================== Filter ==================== -->
      <div
        class="rounded-2xl border border-base-300/60 bg-base-100 p-4 shadow-sm sm:p-5"
      >
        <div class="mb-3 flex items-center justify-between">
          <div class="flex items-center gap-2">
            <span
              class="flex h-8 w-8 items-center justify-center rounded-lg bg-base-200 text-base-content/55"
            >
              <i class="bi bi-funnel"></i>
            </span>

            <div>
              <div class="text-sm font-semibold text-base-content">
                ค้นหาและกรองข้อมูล
              </div>
              <div class="text-[11px] text-base-content/40">
                ค้นหาผู้ใช้งานตามข้อมูลที่ต้องการ
              </div>
            </div>
          </div>

          <button
            v-if="hasActiveFilters"
            class="btn btn-ghost btn-xs gap-1.5 text-base-content/45 hover:bg-error/5 hover:text-error"
            title="ล้างตัวกรอง"
            @click="clearFilters"
          >
            <i class="bi bi-arrow-counterclockwise"></i>
            ล้างตัวกรอง
          </button>
        </div>

        <div class="grid grid-cols-1 gap-3 lg:grid-cols-[minmax(280px,1fr)_180px_210px]">
          <!-- Search -->
          <div class="relative">
            <i
              class="bi bi-search pointer-events-none absolute left-3.5 top-1/2 z-10 -translate-y-1/2 text-base-content/35"
            ></i>

            <input
              v-model="filter.search"
              @input="onSearchInput"
              type="text"
              placeholder="ค้นหาชื่อ, อีเมล, รหัสพนักงาน..."
              class="input h-11 w-full border-base-300/70 bg-base-50 pl-10 text-sm transition-all placeholder:text-base-content/35 focus:border-primary focus:outline-none focus:ring-4 focus:ring-primary/10"
            />
          </div>

          <!-- Status -->
          <FormSelect
            v-model="filter.status"
            :options="statusOptions"
            placeholder="สถานะ: ทั้งหมด"
          />

          <!-- Source -->
          <FormSelect
            v-model="filter.source"
            :options="sourceOptions"
            placeholder="ที่มา: ทั้งหมด"
          />
        </div>
      </div>

      <!-- ==================== Main Table ==================== -->
      <div
        class="overflow-hidden rounded-2xl border border-base-300/60 bg-base-100 shadow-sm"
      >
        <!-- Toolbar -->
        <div
          class="flex flex-col gap-3 border-b border-base-200/80 px-4 py-4 sm:flex-row sm:items-center sm:justify-between sm:px-5"
        >
          <div class="flex items-center gap-3">
            <div>
              <div class="text-sm font-semibold text-base-content">
                รายชื่อผู้ใช้งาน
              </div>
              <div class="text-[11px] text-base-content/40">
                จัดการบัญชีและสถานะการใช้งาน
              </div>
            </div>

            <div class="hidden h-8 w-px bg-base-200 sm:block"></div>

            <label class="flex items-center gap-2 text-xs text-base-content/45">
              <span>แสดง</span>

              <select
                v-model.number="filter.pageSize"
                class="select select-bordered h-8 min-h-8 w-[72px] border-base-300/70 bg-base-100 px-2 text-xs focus:outline-none focus:ring-2 focus:ring-primary/10"
              >
                <option :value="10">10</option>
                <option :value="20">20</option>
                <option :value="50">50</option>
                <option :value="100">100</option>
              </select>

              <span>รายการ</span>
            </label>
          </div>

          <div class="flex gap-2">
            <button
              class="btn btn-sm h-9 gap-2 border-base-300 bg-base-100 px-3 text-base-content/65 shadow-none hover:border-primary/30 hover:bg-primary/5 hover:text-primary"
              :disabled="isSyncing"
              @click="onSync"
            >
              <span
                v-if="isSyncing"
                class="loading loading-spinner loading-xs"
              ></span>

              <i v-else class="bi bi-microsoft"></i>

              <span>Sync จาก Microsoft</span>
            </button>

            <button
              class="btn btn-primary btn-sm h-9 gap-2 px-4 shadow-sm shadow-primary/20"
              @click="openCreate"
            >
              <i class="bi bi-plus-lg"></i>
              <span>เพิ่มผู้ใช้</span>
            </button>
          </div>
        </div>

        <!-- Loading -->
        <div v-if="isLoading" class="p-5">
          <div class="space-y-2">
            <div
              v-for="i in 7"
              :key="i"
              class="skeleton h-[62px] w-full rounded-xl"
            ></div>
          </div>
        </div>

        <!-- Error -->
        <div
          v-else-if="errorMessage"
          class="flex min-h-[360px] flex-col items-center justify-center gap-3 px-5 text-center"
        >
          <div
            class="flex h-14 w-14 items-center justify-center rounded-2xl bg-error/10 text-error"
          >
            <i class="bi bi-exclamation-triangle text-xl"></i>
          </div>

          <div>
            <p class="text-sm font-semibold text-base-content">
              ไม่สามารถโหลดข้อมูลได้
            </p>
            <p class="mt-1 max-w-md text-xs text-error/70">
              {{ errorMessage }}
            </p>
          </div>

          <button
            class="btn btn-sm border-base-300 bg-base-100"
            @click="fetchUsers"
          >
            <i class="bi bi-arrow-clockwise"></i>
            ลองใหม่
          </button>
        </div>

        <template v-else>
          <!-- Empty -->
          <div
            v-if="users.length === 0"
            class="flex min-h-[360px] flex-col items-center justify-center gap-4 px-5 text-center"
          >
            <div
              class="flex h-16 w-16 items-center justify-center rounded-2xl bg-base-200 text-base-content/25"
            >
              <i class="bi bi-people text-2xl"></i>
            </div>

            <div>
              <p class="text-sm font-semibold text-base-content/70">
                {{
                  hasActiveFilters
                    ? "ไม่พบผู้ใช้งานตามเงื่อนไข"
                    : "ยังไม่มีผู้ใช้งานในระบบ"
                }}
              </p>

              <p class="mt-1 text-xs text-base-content/40">
                {{
                  hasActiveFilters
                    ? "ลองเปลี่ยนคำค้นหาหรือตัวกรองแล้วค้นหาอีกครั้ง"
                    : "สามารถ Sync จาก Microsoft หรือเพิ่มผู้ใช้ใหม่ได้"
                }}
              </p>
            </div>

            <button
              v-if="hasActiveFilters"
              class="btn btn-sm btn-ghost text-primary"
              @click="clearFilters"
            >
              <i class="bi bi-arrow-counterclockwise"></i>
              ล้างตัวกรอง
            </button>
          </div>

          <template v-else>
            <!-- Table -->
            <div class="overflow-x-auto">
              <table class="table min-w-[920px]">
                <thead>
                  <tr
                    class="border-b border-base-200 bg-base-200/30 text-[11px] font-semibold uppercase tracking-wide text-base-content/45"
                  >
                    <th class="py-3.5 pl-5 font-semibold">
                      ผู้ใช้งาน
                    </th>

                    <th class="py-3.5 font-semibold">
                      รหัสพนักงาน
                    </th>

                    <th class="py-3.5 font-semibold">
                      ที่มา
                    </th>

                    <th class="py-3.5 font-semibold">
                      เข้าใช้ล่าสุด
                    </th>

                    <th class="py-3.5 text-center font-semibold">
                      สถานะ
                    </th>

                    <th class="py-3.5 pr-5 text-right font-semibold">
                      จัดการ
                    </th>
                  </tr>
                </thead>

                <tbody>
                  <tr
                    v-for="(u, idx) in users"
                    :key="u.id"
                    class="group border-b border-base-200/60 transition-all duration-150 last:border-0 hover:bg-primary/[0.025]"
                  >
                    <!-- User -->
                    <td class="py-3.5 pl-5">
                      <div class="flex items-center gap-3">
                        <div
                          class="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl shadow-sm ring-1 ring-black/5"
                          :class="avatarColor(idx)"
                        >
                          <span class="text-sm font-bold">
                            {{ initials(u.displayName) }}
                          </span>
                        </div>

                        <div class="min-w-0">
                          <div
                            class="truncate text-sm font-semibold text-base-content"
                          >
                            {{ u.displayName }}
                          </div>

                          <div
                            class="mt-0.5 max-w-[300px] truncate text-xs text-base-content/40"
                          >
                            {{ u.email }}
                          </div>
                        </div>
                      </div>
                    </td>

                    <!-- Employee Code -->
                    <td>
                      <span
                        v-if="u.employeeCode"
                        class="rounded-lg bg-base-200/70 px-2.5 py-1 font-mono text-xs text-base-content/65"
                      >
                        {{ u.employeeCode }}
                      </span>

                      <span v-else class="text-xs text-base-content/25">
                        -
                      </span>
                    </td>

                    <!-- Source -->
                    <td>
                      <span
                        class="inline-flex items-center gap-1.5 rounded-lg px-2.5 py-1 text-[11px] font-medium"
                        :class="
                          u.isEntra
                            ? 'bg-info/10 text-info'
                            : 'bg-base-200 text-base-content/50'
                        "
                      >
                        <i
                          :class="
                            u.isEntra
                              ? 'bi bi-microsoft'
                              : 'bi bi-person-plus'
                          "
                        ></i>

                        {{ u.isEntra ? "Microsoft" : "สร้างในระบบ" }}
                      </span>
                    </td>

                    <!-- Last Login -->
                    <td>
                      <div class="flex items-center gap-2 text-xs text-base-content/50">
                        <span
                          class="flex h-7 w-7 items-center justify-center rounded-lg bg-base-200/70"
                        >
                          <i class="bi bi-clock"></i>
                        </span>

                        <span class="whitespace-nowrap">
                          {{ formatDate(u.lastLoginAt) }}
                        </span>
                      </div>
                    </td>

                    <!-- Status -->
                    <td class="text-center">
                      <label class="inline-flex cursor-pointer items-center">
                        <input
                          type="checkbox"
                          class="toggle toggle-success toggle-sm"
                          :checked="u.isActive"
                          :aria-label="`เปิด/ปิดการใช้งาน ${u.displayName}`"
                          @change="toggleActive(u)"
                        />
                      </label>
                    </td>

                    <!-- Actions -->
                    <td class="pr-5">
                      <div
                        class="flex items-center justify-end gap-1 opacity-70 transition-opacity group-hover:opacity-100"
                      >
                        <button
                          class="btn btn-ghost btn-sm btn-square h-8 w-8 rounded-lg text-base-content/40 hover:bg-warning/10 hover:text-warning"
                          title="แก้ไข"
                          @click="openEdit(u)"
                        >
                          <i class="bi bi-pencil-square"></i>
                        </button>

                        <button
                          class="btn btn-ghost btn-sm btn-square h-8 w-8 rounded-lg text-base-content/40 hover:bg-info/10 hover:text-info"
                          title="ตั้งรหัสผ่านใหม่"
                          @click="openResetPassword(u)"
                        >
                          <i class="bi bi-key"></i>
                        </button>

                        <div class="mx-1 h-5 w-px bg-base-200"></div>

                        <button
                          class="btn btn-ghost btn-sm btn-square h-8 w-8 rounded-lg text-base-content/40 hover:bg-error/10 hover:text-error"
                          title="ลบ"
                          @click="confirmDelete(u)"
                        >
                          <i class="bi bi-trash3"></i>
                        </button>
                      </div>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>

            <!-- Pagination -->
            <div
              class="flex flex-col gap-3 border-t border-base-200/70 px-5 py-4 sm:flex-row sm:items-center sm:justify-between"
            >
              <p class="text-xs text-base-content/40">
                แสดง
                <span class="font-semibold text-base-content/60">
                  {{ (filter.page - 1) * filter.pageSize + 1 }}–{{
                    Math.min(filter.page * filter.pageSize, totalCount)
                  }}
                </span>
                จาก
                <span class="font-semibold text-base-content/60">
                  {{ totalCount }}
                </span>
                รายการ
              </p>

              <div class="flex items-center gap-1">
                <button
                  class="btn btn-ghost btn-sm btn-square h-8 w-8 rounded-lg"
                  :disabled="filter.page === 1"
                  @click="goToPage(1)"
                >
                  <i class="bi bi-chevron-double-left text-xs"></i>
                </button>

                <button
                  class="btn btn-ghost btn-sm btn-square h-8 w-8 rounded-lg"
                  :disabled="filter.page === 1"
                  @click="goToPage(filter.page - 1)"
                >
                  <i class="bi bi-chevron-left text-xs"></i>
                </button>

                <div class="mx-1 flex items-center gap-1">
                  <button
                    v-for="p in pageWindow"
                    :key="p"
                    class="btn btn-sm h-8 min-h-8 min-w-8 rounded-lg px-2 text-xs"
                    :class="
                      p === filter.page
                        ? 'btn-primary shadow-sm shadow-primary/20'
                        : 'btn-ghost text-base-content/55'
                    "
                    @click="goToPage(p)"
                  >
                    {{ p }}
                  </button>
                </div>

                <button
                  class="btn btn-ghost btn-sm btn-square h-8 w-8 rounded-lg"
                  :disabled="filter.page >= totalPages"
                  @click="goToPage(filter.page + 1)"
                >
                  <i class="bi bi-chevron-right text-xs"></i>
                </button>

                <button
                  class="btn btn-ghost btn-sm btn-square h-8 w-8 rounded-lg"
                  :disabled="filter.page >= totalPages"
                  @click="goToPage(totalPages)"
                >
                  <i class="bi bi-chevron-double-right text-xs"></i>
                </button>
              </div>
            </div>
          </template>
        </template>
      </div>
    </div>

    <!-- ==================== Create / Edit Modal ==================== -->
    <dialog ref="formDialog" class="modal">
      <div
        class="modal-box w-11/12 max-w-lg overflow-hidden rounded-2xl border border-base-300/60 bg-base-100 p-0 shadow-2xl"
      >
        <!-- Modal Header -->
        <div class="border-b border-base-200 px-6 py-5">
          <div class="flex items-center gap-3">
            <div
              class="flex h-10 w-10 items-center justify-center rounded-xl bg-primary/10 text-primary"
            >
              <i
                :class="
                  isCreate
                    ? 'bi bi-person-plus-fill'
                    : 'bi bi-person-gear'
                "
              ></i>
            </div>

            <div>
              <h3 class="font-semibold text-base-content">
                {{ isCreate ? "เพิ่มผู้ใช้" : "แก้ไขผู้ใช้งาน" }}
              </h3>

              <p class="mt-0.5 text-xs text-base-content/40">
                {{
                  isCreate
                    ? "สร้างบัญชีผู้ใช้งานสำหรับเข้าสู่ระบบ"
                    : "แก้ไขข้อมูลบัญชีผู้ใช้งาน"
                }}
              </p>
            </div>
          </div>
        </div>

        <form class="space-y-4 px-6 py-5" @submit.prevent="submitForm">
          <!-- Microsoft Info -->
          <div
            v-if="isEntraEdit"
            class="flex gap-3 rounded-xl border border-info/20 bg-info/5 p-3.5"
          >
            <i class="bi bi-microsoft mt-0.5 text-info"></i>

            <p class="text-xs leading-relaxed text-info/80">
              ผู้ใช้นี้มาจาก Microsoft ชื่อและอีเมลแก้ไขที่นี่ไม่ได้
              สามารถแก้ไขได้เฉพาะรหัสพนักงานและสถานะการใช้งาน
            </p>
          </div>

          <!-- Email -->
          <div>
            <label
              class="mb-1.5 block text-xs font-medium text-base-content/65"
              for="uf-email"
            >
              อีเมล
            </label>

            <div class="relative">
              <i
                class="bi bi-envelope absolute left-3.5 top-1/2 -translate-y-1/2 text-base-content/30"
              ></i>

              <input
                id="uf-email"
                v-model="form.email"
                type="email"
                class="input w-full border-base-300/70 bg-base-50 pl-10 text-sm focus:border-primary focus:outline-none focus:ring-4 focus:ring-primary/10"
                :disabled="isEntraEdit"
              />
            </div>
          </div>

          <!-- Name -->
          <div>
            <label
              class="mb-1.5 block text-xs font-medium text-base-content/65"
              for="uf-name"
            >
              ชื่อแสดงผล
            </label>

            <div class="relative">
              <i
                class="bi bi-person absolute left-3.5 top-1/2 -translate-y-1/2 text-base-content/30"
              ></i>

              <input
                id="uf-name"
                v-model="form.displayName"
                type="text"
                class="input w-full border-base-300/70 bg-base-50 pl-10 text-sm focus:border-primary focus:outline-none focus:ring-4 focus:ring-primary/10"
                :disabled="isEntraEdit"
              />
            </div>
          </div>

          <!-- Employee Code -->
          <div>
            <label
              class="mb-1.5 block text-xs font-medium text-base-content/65"
              for="uf-code"
            >
              รหัสพนักงาน
            </label>

            <div class="relative">
              <i
                class="bi bi-person-vcard absolute left-3.5 top-1/2 -translate-y-1/2 text-base-content/30"
              ></i>

              <input
                id="uf-code"
                v-model="form.employeeCode"
                type="text"
                class="input w-full border-base-300/70 bg-base-50 pl-10 text-sm focus:border-primary focus:outline-none focus:ring-4 focus:ring-primary/10"
              />
            </div>
          </div>

          <!-- Password -->
          <div v-if="isCreate">
            <label
              class="mb-1.5 block text-xs font-medium text-base-content/65"
              for="uf-pw"
            >
              รหัสผ่าน
            </label>

            <div class="relative">
              <i
                class="bi bi-lock absolute left-3.5 top-1/2 -translate-y-1/2 text-base-content/30"
              ></i>

              <input
                id="uf-pw"
                v-model="form.password"
                type="password"
                autocomplete="new-password"
                placeholder="อย่างน้อย 8 ตัวอักษร"
                class="input w-full border-base-300/70 bg-base-50 pl-10 text-sm focus:border-primary focus:outline-none focus:ring-4 focus:ring-primary/10"
              />
            </div>
          </div>

          <!-- Active -->
          <label
            v-if="!isCreate"
            class="flex cursor-pointer items-center justify-between rounded-xl border border-base-200 bg-base-200/30 px-4 py-3"
          >
            <div class="flex items-center gap-3">
              <div
                class="flex h-8 w-8 items-center justify-center rounded-lg bg-success/10 text-success"
              >
                <i class="bi bi-check-circle"></i>
              </div>

              <div>
                <div class="text-sm font-medium text-base-content">
                  เปิดใช้งานบัญชี
                </div>
                <div class="text-[11px] text-base-content/40">
                  อนุญาตให้ผู้ใช้งานเข้าสู่ระบบ
                </div>
              </div>
            </div>

            <input
              v-model="form.isActive"
              type="checkbox"
              class="toggle toggle-success toggle-sm"
            />
          </label>

          <!-- Error -->
          <div
            v-if="formError"
            class="flex items-start gap-2 rounded-xl border border-error/20 bg-error/5 px-3.5 py-3 text-xs text-error"
            role="alert"
          >
            <i class="bi bi-exclamation-circle mt-0.5"></i>
            <span>{{ formError }}</span>
          </div>

          <!-- Actions -->
          <div class="flex justify-end gap-2 border-t border-base-200 pt-4">
            <button
              type="button"
              class="btn btn-ghost btn-sm"
              @click="formDialog?.close()"
            >
              ยกเลิก
            </button>

            <button
              type="submit"
              class="btn btn-primary btn-sm min-w-24 shadow-sm shadow-primary/20"
              :disabled="isSaving"
            >
              <span
                v-if="isSaving"
                class="loading loading-spinner loading-xs"
              ></span>

              <i v-else class="bi bi-check-lg"></i>

              บันทึก
            </button>
          </div>
        </form>
      </div>

      <form method="dialog" class="modal-backdrop">
        <button>ปิด</button>
      </form>
    </dialog>

    <!-- ==================== Reset Password Modal ==================== -->
    <dialog ref="pwDialog" class="modal">
      <div
        class="modal-box w-11/12 max-w-md overflow-hidden rounded-2xl border border-base-300/60 bg-base-100 p-0 shadow-2xl"
      >
        <div class="border-b border-base-200 px-6 py-5">
          <div class="flex items-center gap-3">
            <div
              class="flex h-10 w-10 items-center justify-center rounded-xl bg-info/10 text-info"
            >
              <i class="bi bi-key-fill"></i>
            </div>

            <div>
              <h3 class="font-semibold text-base-content">
                ตั้งรหัสผ่านใหม่
              </h3>

              <p class="mt-0.5 text-xs text-base-content/40">
                กำหนดรหัสผ่านใหม่สำหรับบัญชีนี้
              </p>
            </div>
          </div>
        </div>

        <form
          class="space-y-4 px-6 py-5"
          @submit.prevent="submitResetPassword"
        >
          <!-- Target User -->
          <div
            class="flex items-center gap-3 rounded-xl bg-base-200/50 p-3"
          >
            <div
              class="flex h-9 w-9 shrink-0 items-center justify-center rounded-xl bg-info/10 text-info"
            >
              <i class="bi bi-person"></i>
            </div>

            <div class="min-w-0">
              <div class="truncate text-sm font-semibold text-base-content">
                {{ pwTarget?.displayName }}
              </div>

              <div class="truncate text-xs text-base-content/40">
                {{ pwTarget?.email }}
              </div>
            </div>
          </div>

          <!-- Microsoft Notice -->
          <div
            v-if="pwTarget?.isEntra"
            class="flex gap-3 rounded-xl border border-info/20 bg-info/5 p-3.5"
          >
            <i class="bi bi-microsoft mt-0.5 text-info"></i>

            <p class="text-xs leading-relaxed text-info/80">
              ผู้ใช้นี้ Login ด้วย Microsoft ตามปกติ
              รหัสผ่านนี้จะใช้เป็นรหัสสำรองเมื่อ Login ด้วย Microsoft ไม่ได้
            </p>
          </div>

          <!-- Password -->
          <div>
            <label
              class="mb-1.5 block text-xs font-medium text-base-content/65"
              for="pw-new"
            >
              รหัสผ่านใหม่
            </label>

            <div class="relative">
              <i
                class="bi bi-lock absolute left-3.5 top-1/2 -translate-y-1/2 text-base-content/30"
              ></i>

              <input
                id="pw-new"
                v-model="newPassword"
                type="password"
                autocomplete="new-password"
                placeholder="อย่างน้อย 8 ตัวอักษร"
                class="input w-full border-base-300/70 bg-base-50 pl-10 text-sm focus:border-primary focus:outline-none focus:ring-4 focus:ring-primary/10"
              />
            </div>
          </div>

          <!-- Error -->
          <div
            v-if="pwError"
            class="flex items-start gap-2 rounded-xl border border-error/20 bg-error/5 px-3.5 py-3 text-xs text-error"
            role="alert"
          >
            <i class="bi bi-exclamation-circle mt-0.5"></i>
            <span>{{ pwError }}</span>
          </div>

          <!-- Actions -->
          <div class="flex justify-end gap-2 border-t border-base-200 pt-4">
            <button
              type="button"
              class="btn btn-ghost btn-sm"
              @click="pwDialog?.close()"
            >
              ยกเลิก
            </button>

            <button
              type="submit"
              class="btn btn-primary btn-sm min-w-24 shadow-sm shadow-primary/20"
              :disabled="isSaving"
            >
              <span
                v-if="isSaving"
                class="loading loading-spinner loading-xs"
              ></span>

              <i v-else class="bi bi-check-lg"></i>

              บันทึก
            </button>
          </div>
        </form>
      </div>

      <form method="dialog" class="modal-backdrop">
        <button>ปิด</button>
      </form>
    </dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted } from "vue";
import { storeToRefs } from "pinia";
import { useUserStore } from "../../stores/userStore";
import type { OptionItem } from "../../stores/employeeStore.ts";
import type { UserItem, UserFilter, SyncResult } from "../../types/User";
import FormSelect from "../../components/ui/FormSelect.vue";
import { notify, extractErrorMessage } from "../../utils/notify";

const userStore = useUserStore();
const { users, totalCount, isLoading, isSyncing, errorMessage } =
  storeToRefs(userStore);

// ========================================
// Filter & Pagination (ฝั่ง Server เหมือนหน้ารายชื่อพนักงาน)
// ========================================
const defaultFilter: UserFilter = {
  search: "",
  status: null,
  source: null,
  page: 1,
  pageSize: 10,
};
const filter = reactive<UserFilter>({ ...defaultFilter });

const statusOptions: OptionItem[] = [
  { id: "active", label: "ใช้งานอยู่" },
  { id: "inactive", label: "ปิดการใช้งาน" },
];
const sourceOptions: OptionItem[] = [
  { id: "entra", label: "Microsoft" },
  { id: "local", label: "สร้างในระบบ" },
];

const totalPages = computed(
  () => Math.ceil(totalCount.value / filter.pageSize) || 1,
);
const hasActiveFilters = computed(
  () => !!filter.search || !!filter.status || !!filter.source,
);

const pageWindow = computed(() => {
  const maxButtons = 5;
  let start = Math.max(1, filter.page - Math.floor(maxButtons / 2));
  let end = start + maxButtons - 1;
  if (end > totalPages.value) {
    end = totalPages.value;
    start = Math.max(1, end - maxButtons + 1);
  }
  const pages: number[] = [];
  for (let p = start; p <= end; p++) pages.push(p);
  return pages;
});

function fetchUsers() {
  return userStore.fetchList(filter);
}
function resetPageAndFetch() {
  filter.page = 1;
  fetchUsers();
}
function goToPage(p: number) {
  filter.page = p;
  fetchUsers();
}
function clearFilters() {
  Object.assign(filter, defaultFilter);
  fetchUsers();
}

let debounceTimer: ReturnType<typeof setTimeout>;
function onSearchInput() {
  clearTimeout(debounceTimer);
  debounceTimer = setTimeout(resetPageAndFetch, 300);
}

watch(
  () => [filter.status, filter.source, filter.pageSize],
  resetPageAndFetch,
);

// ========================================
// Display helpers
// ========================================
const avatarPalette = [
  "bg-emerald-100 text-emerald-700",
  "bg-teal-100 text-teal-700",
  "bg-green-100 text-green-700",
  "bg-lime-100 text-lime-800",
  "bg-cyan-100 text-cyan-700",
  "bg-emerald-200 text-emerald-800",
];
const avatarColor = (idx: number) => avatarPalette[idx % avatarPalette.length];
const initials = (name: string) => name?.charAt(0)?.toUpperCase() ?? "?";
const formatDate = (d?: string | null) =>
  d
    ? new Date(d).toLocaleString("th-TH", {
        dateStyle: "short",
        timeStyle: "short",
      })
    : "-";

// ========================================
// Sync จาก Microsoft (Entra)
// ========================================
const syncResult = ref<SyncResult | null>(null);

async function onSync() {
  syncResult.value = null;
  try {
    syncResult.value = await userStore.syncEntra();
    await fetchUsers();
  } catch (err: any) {
    await notify.error(extractErrorMessage(err), "Sync ไม่สำเร็จ");
  }
}

// ========================================
// เปิด/ปิดการใช้งาน
// ========================================
async function toggleActive(u: UserItem) {
  try {
    await userStore.update(u.id, {
      email: u.email,
      displayName: u.displayName,
      employeeCode: u.employeeCode ?? null,
      employeeId: u.employeeId ?? null,
      isActive: !u.isActive,
    });
  } catch (err: any) {
    await notify.error(extractErrorMessage(err), "เปลี่ยนสถานะไม่สำเร็จ");
  }
  // โหลดหน้าปัจจุบันใหม่เสมอ เพื่อให้ปุ่ม Toggle ตรงกับข้อมูลจริง
  await fetchUsers();
}

// ========================================
// Modal: เพิ่ม / แก้ไข
// ========================================
const formDialog = ref<HTMLDialogElement>();
const isCreate = ref(true);
const editing = ref<UserItem | null>(null);
const isSaving = ref(false);
const formError = ref("");
const form = reactive({
  email: "",
  displayName: "",
  employeeCode: "",
  password: "",
  isActive: true,
});

const isEntraEdit = computed(() => !isCreate.value && !!editing.value?.isEntra);

function openCreate() {
  isCreate.value = true;
  editing.value = null;
  Object.assign(form, {
    email: "",
    displayName: "",
    employeeCode: "",
    password: "",
    isActive: true,
  });
  formError.value = "";
  formDialog.value?.showModal();
}

function openEdit(u: UserItem) {
  isCreate.value = false;
  editing.value = u;
  Object.assign(form, {
    email: u.email,
    displayName: u.displayName,
    employeeCode: u.employeeCode ?? "",
    password: "",
    isActive: u.isActive,
  });
  formError.value = "";
  formDialog.value?.showModal();
}

async function submitForm() {
  formError.value = "";

  if (!isEntraEdit.value) {
    if (!form.email.trim() || !form.email.includes("@"))
      return (formError.value = "กรุณากรอกอีเมลให้ถูกต้อง");
    if (!form.displayName.trim())
      return (formError.value = "กรุณากรอกชื่อแสดงผล");
  }
  if (isCreate.value && form.password.length < 8)
    return (formError.value = "รหัสผ่านต้องมีอย่างน้อย 8 ตัวอักษร");

  isSaving.value = true;
  try {
    if (isCreate.value) {
      await userStore.create({
        email: form.email.trim(),
        displayName: form.displayName.trim(),
        employeeCode: form.employeeCode.trim() || null,
        employeeId: null,
        password: form.password,
        isActive: true,
      });
    } else {
      await userStore.update(editing.value!.id, {
        email: form.email.trim(),
        displayName: form.displayName.trim(),
        employeeCode: form.employeeCode.trim() || null,
        employeeId: editing.value!.employeeId ?? null,
        isActive: form.isActive,
      });
    }
    formDialog.value?.close();
    if (isCreate.value) filter.page = 1;
    await fetchUsers();
    await notify.success(
      isCreate.value ? "เพิ่มผู้ใช้สำเร็จ" : "บันทึกการแก้ไขสำเร็จ",
    );
  } catch (err: any) {
    // แสดงในกล่องเอง เพราะ Popup อาจไปอยู่หลัง Modal
    formError.value = extractErrorMessage(err);
  } finally {
    isSaving.value = false;
  }
}

// ========================================
// Modal: ตั้งรหัสผ่านใหม่ (ทุกคน ผู้ใช้ Microsoft ใช้เป็นรหัสสำรอง)
// ========================================
const pwDialog = ref<HTMLDialogElement>();
const pwTarget = ref<UserItem | null>(null);
const newPassword = ref("");
const pwError = ref("");

function openResetPassword(u: UserItem) {
  pwTarget.value = u;
  newPassword.value = "";
  pwError.value = "";
  pwDialog.value?.showModal();
}

async function submitResetPassword() {
  pwError.value = "";
  if (newPassword.value.length < 8)
    return (pwError.value = "รหัสผ่านต้องมีอย่างน้อย 8 ตัวอักษร");

  isSaving.value = true;
  try {
    await userStore.resetPassword(pwTarget.value!.id, newPassword.value);
    pwDialog.value?.close();
    await notify.success("ตั้งรหัสผ่านใหม่สำเร็จ");
  } catch (err: any) {
    pwError.value = extractErrorMessage(err);
  } finally {
    isSaving.value = false;
  }
}

// ========================================
// ลบ
// ========================================
async function confirmDelete(u: UserItem) {
  const ok = await notify.confirm(
    `ยืนยันลบผู้ใช้ "${u.displayName}" ใช่หรือไม่`,
    "ยืนยันการลบ",
  );
  if (!ok) return;

  try {
    await userStore.remove(u.id);
    // ลบแถวสุดท้ายของหน้า ให้ถอยไปหน้าก่อนหน้า
    if (users.value.length === 1 && filter.page > 1) filter.page -= 1;
    await fetchUsers();
    await notify.success("ลบผู้ใช้สำเร็จ");
  } catch (err: any) {
    await notify.error(extractErrorMessage(err), "ลบไม่สำเร็จ");
  }
}

onMounted(fetchUsers);
</script>
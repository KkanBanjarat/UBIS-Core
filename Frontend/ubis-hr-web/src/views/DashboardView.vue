```vue
<template>
  <!-- Loading -->
  <div
    v-if="loading"
    class="flex min-h-[70vh] items-center justify-center"
  >
    <div class="flex flex-col items-center gap-3">
      <span
        class="loading loading-spinner loading-md text-emerald-500"
      ></span>
      <span class="text-xs text-slate-400">
        กำลังโหลดข้อมูล...
      </span>
    </div>
  </div>

  <!-- Dashboard -->
  <div v-else-if="employee"
    class="mx-auto max-w-[1500px] space-y-5 pb-10">
    <!-- ========================================================= -->
    <!-- HERO -->
    <!-- ========================================================= -->
    <section class="relative overflow-hidden rounded-2xl border border-emerald-100 bg-white shadow-sm">
      <!-- Background -->
      <div class="absolute inset-0 bg-gradient-to-br from-emerald-50 via-white to-sky-50"></div>
      <div class="absolute -right-20 -top-32 size-80 rounded-full bg-emerald-200/30 blur-3xl"></div>
      <div class="absolute -bottom-32 left-1/3 size-72 rounded-full bg-sky-200/20 blur-3xl"></div>
      <!-- Decorative -->
      <div class="absolute right-10 top-10 hidden size-40 rounded-full border border-emerald-200/40 lg:block"></div>
      <div class="absolute right-20 top-20 hidden size-20 rounded-full border border-emerald-200/40 lg:block"></div>
      <!-- Content -->
      <div class="relative px-5 py-7 sm:px-7 lg:px-9 lg:py-8">
        <div class="flex flex-col gap-6 lg:flex-row lg:items-center lg:justify-between">
          <!-- Profile -->
          <div class="flex items-center gap-5">
            <!-- Avatar -->
            <div class="relative">
              <div class="flex size-20 shrink-0 items-center justify-center rounded-2xl bg-gradient-to-br from-emerald-500 to-green-600 text-2xl font-bold text-white shadow-lg shadow-emerald-500/20 ring-4 ring-white sm:size-24 sm:text-3xl">
                {{ initials }}
              </div>
              <span class="absolute bottom-1 right-1 size-4 rounded-full border-2 border-white bg-emerald-500 shadow-sm"></span>
            </div>
            <!-- Name -->
            <div class="min-w-0">
              <div class="mb-2 inline-flex items-center gap-1.5 rounded-full border border-emerald-100 bg-white/80 px-2.5 py-1 text-[10px] font-semibold text-emerald-600 backdrop-blur">
                <span class="size-1.5 rounded-full bg-emerald-500"></span>
                EMPLOYEE PROFILE
              </div>
              <h1 class="truncate text-xl font-bold tracking-tight text-slate-800 sm:text-2xl">
                {{ employee.prefixNameTh }}
                {{ employee.fNameTh }}
                {{ employee.lNameTh }}
              </h1>

              <div class="mt-2 flex flex-wrap items-center gap-x-4 gap-y-1.5 text-xs text-slate-500 sm:text-sm">
                <span class="font-medium text-emerald-700">
                  {{ employee.positionNameTh || "-" }}
                </span>
                <span v-if="employee.branchNameTh"
                  class="flex items-center gap-1">
                  <svg class="size-3.5"
                    fill="none"
                    stroke="currentColor"
                    stroke-width="1.8"
                    viewBox="0 0 24 24">
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      d="M3 21h18M5 21V7l8-4v18M13 21V7l6 3v11"
                    />
                  </svg>
                  {{ employee.branchNameTh }}
                </span>
                <span class="flex items-center gap-1">
                  <svg class="size-3.5"
                    fill="none"
                    stroke="currentColor"
                    stroke-width="1.8"
                    viewBox="0 0 24 24">
                    <path stroke-linecap="round"
                      stroke-linejoin="round"
                      d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"/>
                  </svg>
                  {{ formatDate(employee.hireDate) }}
                </span>
              </div>
            </div>
          </div>
          <!-- Status -->
          <div class="flex items-center gap-3 self-start rounded-xl border border-white/80 bg-white/70 px-4 py-3 shadow-sm backdrop-blur lg:self-center">
            <span class="flex size-9 items-center justify-center rounded-lg bg-emerald-50 text-emerald-600">
              <svg class="size-4"
                fill="none"
                stroke="currentColor"
                stroke-width="2"
                viewBox="0 0 24 24" >
                <path stroke-linecap="round"
                  stroke-linejoin="round"
                  d="M9 12l2 2 4-4"/>
                <circle cx="12" cy="12" r="9"/>
              </svg>
            </span>
            <div>
              <div class="text-xs text-slate-400">
                สถานะการทำงาน
              </div>
              <div v-if="employee.status === 'Active'"
                class="mt-0.5 text-sm font-semibold text-emerald-600">
                ปฏิบัติงาน
              </div>
              <div v-else
                class="mt-0.5 text-sm font-semibold text-red-500">
                {{ employee.status }}
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- ========================================================= -->
    <!-- QUICK STATS -->
    <!-- ========================================================= -->
    <section class="grid grid-cols-2 gap-3 lg:grid-cols-4">
      <!-- Employee ID -->
      <div class="group rounded-2xl border border-slate-200 bg-white p-4 shadow-sm transition-all hover:-translate-y-0.5 hover:border-emerald-200 hover:shadow-md">
        <div class="flex items-start justify-between">
          <div class="flex size-9 items-center justify-center rounded-xl bg-emerald-50 text-emerald-600">
            <svg class="size-4"
              fill="none"
              stroke="currentColor"
              stroke-width="1.8"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                d="M15 7h3a2 2 0 012 2v10a2 2 0 01-2 2H6a2 2 0 01-2-2V9a2 2 0 012-2h3"
              />
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                d="M9 5a3 3 0 016 0v4H9V5z"
              />
            </svg>
          </div>
          <span class="text-[10px] text-slate-300">
            ID
          </span>
        </div>
        <div class="mt-4">
          <div class="text-xs font-medium text-slate-400">รหัสพนักงาน</div>
          <div class="mt-1 truncate text-[15px] font-bold text-slate-700">
            {{ employee.empId || "-" }}
          </div>
        </div>
      </div>
      <!-- Position -->
      <div class="group rounded-2xl border border-slate-200 bg-white p-4 shadow-sm transition-all hover:-translate-y-0.5 hover:border-emerald-200 hover:shadow-md">
        <div class="flex items-start justify-between">
          <div class="flex size-9 items-center justify-center rounded-xl bg-sky-50 text-sky-600">
            <svg class="size-4"
              fill="none"
              stroke="currentColor"
              stroke-width="1.8"
              viewBox="0 0 24 24">
              <path stroke-linecap="round"
                stroke-linejoin="round"
                d="M20 21v-2a4 4 0 00-4-4H8a4 4 0 00-4 4v2"/>
              <circle cx="12" cy="7" r="4"/>
            </svg>
          </div>
          <span class="text-[10px] text-slate-300">ROLE</span>
        </div>
        <div class="mt-4">
          <div class="text-xs font-medium text-slate-400">
            ตำแหน่ง
          </div>
          <div class="mt-1 truncate text-[15px] font-bold text-slate-700">{{ employee.positionNameEn || "-" }}</div>
          <div class="mt-1 truncate text-[12px] font-normal text-slate-400">{{ employee.positionNameTh || "-" }}</div>
        </div>
      </div>
      <!-- Level -->
      <div class="group rounded-2xl border border-slate-200 bg-white p-4 shadow-sm transition-all hover:-translate-y-0.5 hover:border-emerald-200 hover:shadow-md">
        <div class="flex items-start justify-between">
          <div class="flex size-9 items-center justify-center rounded-xl bg-violet-50 text-violet-600">
            <svg class="size-4"
              fill="none"
              stroke="currentColor"
              stroke-width="1.8"
              viewBox="0 0 24 24">
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                d="M4 19V5m0 0h11l-2 3 2 3H4"
              />
            </svg>
          </div>
          <span class="text-[10px] text-slate-300">LEVEL</span>
        </div>
        <div class="mt-4">
          <div class="text-xs font-medium text-slate-400">ระดับตำแหน่ง</div>
          <div class="mt-1 truncate text-[15px] font-bold text-slate-700">
            L{{employee.positionLevel}} : {{ employee.positionLevelNameEn || "-" }} 
          </div>
          <div class="mt-1 truncate text-[12px] font-normal text-slate-400">{{ employee.positionLevelNameTh || "-" }}</div>
        </div>
      </div>
      <!-- Benefit -->
      <div class="group rounded-2xl border border-slate-200 bg-white p-4 shadow-sm transition-all hover:-translate-y-0.5 hover:border-emerald-200 hover:shadow-md" >
        <div class="flex items-start justify-between">
          <div class="flex size-9 items-center justify-center rounded-xl bg-amber-50 text-amber-600">
            <svg class="size-4"
              fill="none"
              stroke="currentColor"
              stroke-width="1.8"
              viewBox="0 0 24 24">
              <path stroke-linecap="round"
                stroke-linejoin="round"
                d="M20 12v7a2 2 0 01-2 2H6a2 2 0 01-2-2v-7"/>
              <path stroke-linecap="round"
                stroke-linejoin="round"/>
            </svg>
          </div>
          <span class="text-[10px] text-slate-300">
            BENEFIT
          </span>
        </div>

        <div class="mt-4">
          <div class="text-xs font-medium text-slate-400">กลุ่มสวัสดิการ</div>
          <div class="mt-1 text-[15px] font-bold text-slate-700">
            {{ employee.benefitPlans?.length ?? 0 }}

            <span class="text-sm font-normal text-slate-400">
              กลุ่ม
            </span>
          </div>
        </div>
      </div>
    </section>

    <!-- ========================================================= -->
    <!-- TABS -->
    <!-- ========================================================= -->
    <div
      class="flex items-center gap-1 overflow-x-auto border-b border-slate-200"
    >
      <!-- Profile -->
      <button
        type="button"
        @click="activeTab = 'profile'"
        class="relative whitespace-nowrap px-4 py-3 text-sm font-medium transition-colors"
        :class="
          activeTab === 'profile'
            ? 'text-emerald-600'
            : 'text-slate-500 hover:text-slate-800'
        "
      >
        โปรไฟล์

        <span
          v-if="activeTab === 'profile'"
          class="absolute bottom-0 left-3 right-3 h-0.5 rounded-full bg-emerald-500"
        ></span>
      </button>

      <!-- Benefits -->
      <button
        type="button"
        @click="activeTab = 'benefit'"
        class="relative whitespace-nowrap px-4 py-3 text-sm font-medium transition-colors"
        :class="
          activeTab === 'benefit'
            ? 'text-emerald-600'
            : 'text-slate-500 hover:text-slate-800'
        "
      >
        สวัสดิการ

        <span
          v-if="activeTab === 'benefit'"
          class="absolute bottom-0 left-3 right-3 h-0.5 rounded-full bg-emerald-500"
        ></span>
      </button>

      <!-- Organization -->
      <button
        type="button"
        @click="activeTab = 'team'"
        class="relative whitespace-nowrap px-4 py-3 text-sm font-medium transition-colors"
        :class="
          activeTab === 'team'
            ? 'text-emerald-600'
            : 'text-slate-500 hover:text-slate-800'
        "
      >
        Organization

        <span
          v-if="activeTab === 'team'"
          class="absolute bottom-0 left-3 right-3 h-0.5 rounded-full bg-emerald-500"
        ></span>
      </button>
    </div>

    <!-- ========================================================= -->
    <!-- PROFILE -->
    <!-- ========================================================= -->
    <div
      v-if="activeTab === 'profile'"
      class="grid grid-cols-1 gap-5 lg:grid-cols-2"
    >
      <!-- Personal Information -->
      <section
        class="overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm"
      >
        <div class="border-b border-slate-100 px-5 py-4">
          <h2 class="text-base font-semibold text-slate-800">
            ข้อมูลส่วนตัว
          </h2>

          <p class="mt-0.5 text-xs text-slate-400">
            ข้อมูลพื้นฐานของพนักงาน
          </p>
        </div>

        <div class="divide-y divide-slate-100">
          <!-- Employee ID -->
          <div class="grid grid-cols-[130px_1fr] gap-4 px-5 py-3.5">
            <span class="text-xs font-medium text-slate-400">
              รหัสพนักงาน
            </span>

            <span class="text-[15px] font-medium text-slate-700">
              {{ employee.empId || "-" }}
            </span>
          </div>

          <!-- Employee Type -->
          <div class="grid grid-cols-[130px_1fr] gap-4 px-5 py-3.5">
            <span class="text-xs font-medium text-slate-400">
              ประเภทพนักงาน
            </span>

            <span class="text-[15px] font-medium text-slate-700">
              {{ employee.employeeTypeNameTh || "-" }}
            </span>
          </div>

          <!-- Position -->
          <div class="grid grid-cols-[130px_1fr] gap-4 px-5 py-3.5">
            <span class="text-xs font-medium text-slate-400">
              ตำแหน่ง
            </span>

            <span class="text-[15px] font-medium text-slate-700">
              {{ employee.positionNameTh || "-" }}
            </span>
          </div>

          <!-- Job Level -->
          <div class="grid grid-cols-[130px_1fr] gap-4 px-5 py-3.5">
            <span class="text-xs font-medium text-slate-400">
              ระดับตำแหน่ง
            </span>

            <span class="text-[15px] font-medium text-slate-700">
              {{ employee.positionLevel || "-" }}
            </span>
          </div>

          <!-- Supervisor -->
          <div
            v-if="employee.reportToId"
            class="grid grid-cols-[130px_1fr] gap-4 px-5 py-3.5"
          >
            <span class="text-xs font-medium text-slate-400">
              ผู้บังคับบัญชา
            </span>

            <span class="text-[15px] font-medium text-slate-700">
              {{ employee.reportToNameTh }}
            </span>
          </div>

          <!-- Email -->
          <div class="grid grid-cols-[130px_1fr] gap-4 px-5 py-3.5">
            <span class="text-xs font-medium text-slate-400">
              อีเมล
            </span>

            <span
              class="break-all text-[15px] font-medium text-slate-700"
            >
              {{ employee.email || "-" }}
            </span>
          </div>

          <!-- Start Date -->
          <div class="grid grid-cols-[130px_1fr] gap-4 px-5 py-3.5">
            <span class="text-xs font-medium text-slate-400">
              วันที่เริ่มงาน
            </span>

            <span class="text-[15px] font-medium text-slate-700">
              {{ formatDate(employee.hireDate) }}
            </span>
          </div>
        </div>
      </section>

      <!-- Organization Information -->
      <section
        class="overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm"
      >
        <div class="border-b border-slate-100 px-5 py-4">
          <h2 class="text-base font-semibold text-slate-800">
            ข้อมูลองค์กร
          </h2>

          <p class="mt-0.5 text-xs text-slate-400">
            หน่วยงานและโครงสร้างองค์กร
          </p>
        </div>

        <div class="divide-y divide-slate-100">
          <!-- Company -->
          <div class="grid grid-cols-[130px_1fr] gap-4 px-5 py-3.5">
            <span class="text-xs font-medium text-slate-400">
              บริษัท
            </span>

            <span class="text-[15px] font-medium text-slate-700">
              {{ employee.companyNameTh || "-" }}
            </span>
          </div>

          <!-- Group -->
          <div class="grid grid-cols-[130px_1fr] gap-4 px-5 py-3.5">
            <span class="text-xs font-medium text-slate-400">
              สายงาน
            </span>

            <span class="text-[15px] font-medium text-slate-700">
              {{ employee.groupNameTh || "-" }}
            </span>
          </div>

          <!-- Division -->
          <div class="grid grid-cols-[130px_1fr] gap-4 px-5 py-3.5">
            <span class="text-xs font-medium text-slate-400">
              ฝ่าย
            </span>

            <span class="text-[15px] font-medium text-slate-700">
              {{ employee.departmentNameTh || "-" }}
            </span>
          </div>

          <!-- Department -->
          <div class="grid grid-cols-[130px_1fr] gap-4 px-5 py-3.5">
            <span class="text-xs font-medium text-slate-400">
              แผนก
            </span>

            <span class="text-[15px] font-medium text-slate-700">
              {{ employee.divisionNameTh || "-" }}
            </span>
          </div>

          <!-- Section -->
          <div class="grid grid-cols-[130px_1fr] gap-4 px-5 py-3.5">
            <span class="text-xs font-medium text-slate-400">
              ส่วน
            </span>

            <span class="text-[15px] font-medium text-slate-700">
              {{ employee.sectionNameTh || "-" }}
            </span>
          </div>

          <!-- Branch -->
          <div class="grid grid-cols-[130px_1fr] gap-4 px-5 py-3.5">
            <span class="text-xs font-medium text-slate-400">
              สาขา
            </span>

            <span class="text-[15px] font-medium text-slate-700">
              {{ employee.branchNameTh || "-" }}
            </span>
          </div>
        </div>
      </section>
    </div>

    <!-- ========================================================= -->
    <!-- BENEFITS -->
    <!-- ========================================================= -->
    <div
      v-if="activeTab === 'benefit'"
      class="space-y-5"
    >
      <section
        class="overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm"
      >
        <!-- Header -->
        <div
          class="flex flex-col gap-3 border-b border-slate-100 px-5 py-4 sm:flex-row sm:items-center sm:justify-between"
        >
          <div>
            <h2 class="text-base font-semibold text-slate-800">
              สวัสดิการ
            </h2>

            <p class="mt-0.5 text-xs text-slate-400">
              สิทธิประโยชน์ที่ได้รับตามกลุ่มพนักงาน
            </p>
          </div>

          <span
            class="w-fit rounded-full bg-emerald-50 px-3 py-1.5 text-xs font-semibold text-emerald-600"
          >
            {{ employee.benefitPlans?.length ?? 0 }} กลุ่ม
          </span>
        </div>

        <!-- Content -->
        <div class="p-5">
          <!-- Has Benefits -->
          <div
            v-if="employee.benefitPlans?.length"
            class="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3"
          >
            <div
              v-for="p in employee.benefitPlans"
              :key="p.id"
              class="border border-slate-200 bg-white p-5 transition-colors hover:border-emerald-200 hover:bg-emerald-50/20"
            >
              <!-- Plan Name -->
              <div class="flex items-center justify-between gap-3">
                <p class="text-[15px] font-semibold text-slate-700">
                  {{ p.nameTh }}
                </p>

                <span
                  class="size-2 shrink-0 rounded-full bg-emerald-400"
                ></span>
              </div>

              <!-- Benefit Items -->
              <div
                v-if="p.items?.length"
                class="mt-4 divide-y divide-slate-100"
              >
                <div
                  v-for="item in p.items"
                  :key="item.benefitId"
                  class="flex items-center justify-between gap-4 py-3"
                >
                  <span
                    class="min-w-0 truncate text-sm text-slate-500"
                  >
                    {{ item.benefitNameTh }}
                  </span>

                  <span
                    class="shrink-0 text-sm font-semibold text-slate-700"
                  >
                    {{ item.limitAmount.toLocaleString("th-TH") }}
                    บาท
                  </span>
                </div>
              </div>

              <!-- Empty Items -->
              <p
                v-else
                class="mt-4 text-sm text-slate-400"
              >
                ยังไม่มีสวัสดิการที่กำหนด
              </p>
            </div>
          </div>

          <!-- No Benefits -->
          <div
            v-else
            class="flex flex-col items-center justify-center border border-dashed border-slate-200 bg-slate-50/50 py-14 text-center"
          >
            <div
              class="flex size-14 items-center justify-center rounded-2xl bg-slate-100 text-slate-400"
            >
              <svg
                class="size-6"
                fill="none"
                stroke="currentColor"
                stroke-width="1.7"
                viewBox="0 0 24 24"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  d="M20 12v7a2 2 0 01-2 2H6a2 2 0 01-2-2v-7"
                />
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  d="M12 21V5"
                />
              </svg>
            </div>

            <p class="mt-4 text-sm font-medium text-slate-600">
              ยังไม่ได้กำหนดกลุ่มสวัสดิการ
            </p>

            <p class="mt-1 text-xs text-slate-400">
              กรุณาติดต่อฝ่ายบุคคล
            </p>
          </div>
        </div>
      </section>
    </div>

    <!-- ========================================================= -->
    <!-- ORGANIZATION -->
    <!-- ========================================================= -->
    <div
      v-if="activeTab === 'team'"
      class="space-y-5"
    >
      <section
        class="overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm"
      >
        <div
          class="flex items-center gap-3 border-b border-slate-100 px-5 py-4"
        >
          <div
            class="flex size-9 items-center justify-center rounded-xl bg-violet-50 text-violet-600"
          >
            <svg
              class="size-4"
              fill="none"
              stroke="currentColor"
              stroke-width="1.8"
              viewBox="0 0 24 24"
            >
              <circle
                cx="12"
                cy="7"
                r="3"
              />
              <circle
                cx="5"
                cy="17"
                r="3"
              />
              <circle
                cx="19"
                cy="17"
                r="3"
              />
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                d="M12 10v3m0 0H5m7 0h7"
              />
            </svg>
          </div>

          <div>
            <h2 class="text-base font-semibold text-slate-800">
              สายการบังคับบัญชา
            </h2>

            <p class="text-xs text-slate-400">
              Organization chart
            </p>
          </div>
        </div>

        <div
          v-if="employee.orgChart"
          class="overflow-x-auto p-6"
        >
          <div class="flex min-w-fit justify-center py-4">
            <OrgChartNode
              :node="employee.orgChart"
              variant="dashboard"
            />
          </div>
        </div>

        <div
          v-else
          class="flex flex-col items-center justify-center py-16 text-center"
        >
          <div
            class="flex size-14 items-center justify-center rounded-2xl bg-slate-100 text-slate-400"
          >
            <svg
              class="size-6"
              fill="none"
              stroke="currentColor"
              stroke-width="1.7"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                d="M12 20V10m0 0l-4 4m4-4l4 4M5 4h14"
              />
            </svg>
          </div>

          <p class="mt-4 text-sm font-medium text-slate-500">
            ไม่มีข้อมูลสายการบังคับบัญชา
          </p>

          <p class="mt-1 text-xs text-slate-400">
            Organization chart ยังไม่มีข้อมูล
          </p>
        </div>
      </section>
    </div>
  </div>

  <!-- Error -->
  <div
    v-else
    class="mx-auto mt-10 max-w-2xl rounded-2xl border border-red-100 bg-red-50 p-6 text-center"
  >
    <div
      class="mx-auto flex size-12 items-center justify-center rounded-2xl bg-white text-red-500 shadow-sm"
    >
      <svg
        class="size-5"
        fill="none"
        stroke="currentColor"
        stroke-width="1.8"
        viewBox="0 0 24 24"
      >
        <circle
          cx="12"
          cy="12"
          r="9"
        />
        <path
          stroke-linecap="round"
          d="M12 8v4m0 4h.01"
        />
      </svg>
    </div>

    <p class="mt-3 text-sm font-semibold text-red-600">
      ไม่พบข้อมูลพนักงาน
    </p>

    <p class="mt-1 text-xs text-red-400">
      กรุณาลองใหม่อีกครั้ง หรือติดต่อฝ่าย IT
    </p>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { useAuthStore } from "../stores/authStore.ts";
import type { Employee } from "../types/Employee";
import OrgChartNode from "../components/employee/OrgChartNode.vue";

const authStore = useAuthStore();

const employee = ref<Employee | null>(null);

const loading = ref(true);

const activeTab = ref("profile");

const initials = computed(
  () =>
    employee.value?.fNameEn?.charAt(0)?.toUpperCase() ?? "",
);

const formatDate = (
  dateString: string | null | undefined,
) => {
  if (!dateString) return "-";

  return new Date(dateString).toLocaleDateString("th-TH", {
    year: "numeric",
    month: "long",
    day: "numeric",
  });
};

onMounted(async () => {
  try {
    loading.value = true;

    const emp = await authStore.fetchCurrentEmployee();

    if (emp) {
      employee.value = emp;
    }
  } catch (error) {
    console.error("Failed to fetch employee:", error);
  } finally {
    loading.value = false;
  }
});
</script>
```

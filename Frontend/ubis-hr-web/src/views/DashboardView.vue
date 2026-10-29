<template>
  <main class="employee-dashboard">
    <section v-if="loading" class="ed-state" aria-busy="true" role="status"><span class="loading loading-spinner loading-md"></span><h2>กำลังโหลดข้อมูลพนักงาน</h2><p>กรุณารอสักครู่</p></section>
    <section v-else-if="loadError" class="ed-state ed-state--error" role="alert"><i class="bi bi-wifi-off"></i><h2>โหลดข้อมูลไม่สำเร็จ</h2><p>ระบบตอบกลับช้าหรือเชื่อมต่อไม่ได้ กรุณาลองใหม่อีกครั้ง</p><button class="ed-button" type="button" @click="loadEmployee">ลองใหม่</button></section>
    <template v-else-if="employee">
      <header class="ed-hero">
        <div class="ed-identity"><div class="ed-avatar" aria-hidden="true">{{ initials }}</div><div class="ed-identity__text"><p class="ed-eyebrow">UBIS CORE <span>/</span> EMPLOYEE WORKSPACE</p><h1>{{ employee.prefixNameTh }} {{ employee.fNameTh }} {{ employee.lNameTh }}</h1><div class="ed-identity__meta"><span>{{ employee.positionNameTh || '-' }}</span><span v-if="employee.branchNameTh"><i class="bi bi-buildings"></i>{{ employee.branchNameTh }}</span></div></div></div>
        <div class="ed-hero__right"><span class="ed-status" :class="employee.status === 'Active' ? 'is-active' : 'is-inactive'"><span></span>{{ employee.status === 'Active' ? 'ปฏิบัติงาน' : employee.status || 'ไม่ระบุสถานะ' }}</span><span class="ed-hire-date">เริ่มงาน {{ formatDate(employee.hireDate) }}</span></div>
      </header>

      <section class="ed-stats" aria-label="ข้อมูลพนักงานโดยสรุป">
        <div class="ed-stat"><span class="ed-stat__icon"><i class="bi bi-person-badge"></i></span><div><p>รหัสพนักงาน</p><strong>{{ employee.empId || '-' }}</strong></div></div>
        <div class="ed-stat"><span class="ed-stat__icon is-blue"><i class="bi bi-briefcase"></i></span><div><p>ตำแหน่ง</p><strong>{{ employee.positionNameEn || '-' }}</strong><small>{{ employee.positionNameTh || '-' }}</small></div></div>
        <div class="ed-stat"><span class="ed-stat__icon is-purple"><i class="bi bi-layers"></i></span><div><p>ระดับตำแหน่ง</p><strong>L{{ employee.positionLevel ?? '-' }} · {{ employee.positionLevelNameEn || '-' }}</strong><small>{{ employee.positionLevelNameTh || '-' }}</small></div></div>
        <div class="ed-stat"><span class="ed-stat__icon is-amber"><i class="bi bi-gift"></i></span><div><p>กลุ่มสวัสดิการ</p><strong>{{ employee.benefitPlans?.length ?? 0 }} <span>กลุ่ม</span></strong></div></div>
      </section>

      <section v-if="pendingTotal > 0" class="ed-pending" aria-label="รายการรออนุมัติ">
        <div class="ed-pending__title"><span class="ed-pending__icon"><i class="bi bi-clock-history"></i></span><div><h2>รอคุณพิจารณา <strong>{{ pendingTotal }}</strong> รายการ</h2><p>เอกสารที่ถึงคิวให้คุณอนุมัติ</p></div></div>
        <div class="ed-pending__types"><button v-for="p in pendingItems" :key="p.docType" type="button" @click="goApprove()">{{ p.label }}<span>{{ p.qtyWaitApprove }}</span></button></div>
        <button type="button" class="ed-button ed-button--amber" @click="goApprove()">ไปหน้าอนุมัติ<i class="bi bi-arrow-right"></i></button>
      </section>

      <nav class="ed-nav" aria-label="หมวดข้อมูลพนักงาน"><div class="ed-nav__tabs"><button type="button" :class="{ 'is-selected': activeTab === 'profile' }" :aria-current="activeTab === 'profile' ? 'page' : undefined" @click="activeTab = 'profile'"><i class="bi bi-person-vcard"></i>โปรไฟล์</button><button type="button" :class="{ 'is-selected': activeTab === 'benefit' }" :aria-current="activeTab === 'benefit' ? 'page' : undefined" @click="activeTab = 'benefit'"><i class="bi bi-gift"></i>สวัสดิการ<span>{{ employee.benefitPlans?.length ?? 0 }}</span></button><button type="button" :class="{ 'is-selected': activeTab === 'team' }" :aria-current="activeTab === 'team' ? 'page' : undefined" @click="activeTab = 'team'"><i class="bi bi-diagram-3"></i>โครงสร้างองค์กร</button></div><span class="ed-nav__note">ข้อมูลพนักงานของคุณ</span></nav>

      <div v-if="activeTab === 'profile'" class="ed-profile-grid">
        <section class="ed-panel"><header class="ed-panel__heading"><div class="ed-panel__title"><span class="ed-section-icon"><i class="bi bi-person-vcard"></i></span><div><h2>ข้อมูลส่วนตัว</h2><p>ข้อมูลพื้นฐานและการทำงาน</p></div></div><span class="ed-panel__index">01</span></header><dl class="ed-data-list"><div v-for="field in profileFields" :key="field.label" class="ed-data-row"><dt>{{ field.label }}</dt><dd>{{ field.value ?? '-' }}</dd></div></dl></section>
        <section class="ed-panel"><header class="ed-panel__heading"><div class="ed-panel__title"><span class="ed-section-icon is-blue"><i class="bi bi-buildings"></i></span><div><h2>ข้อมูลองค์กร</h2><p>หน่วยงานและโครงสร้างการทำงาน</p></div></div><span class="ed-panel__index">02</span></header><dl class="ed-data-list"><div v-for="field in organizationFields" :key="field.label" class="ed-data-row"><dt>{{ field.label }}</dt><dd>{{ field.value || '-' }}</dd></div></dl></section>
      </div>

      <section v-else-if="activeTab === 'benefit'" class="ed-panel">
        <header class="ed-panel__heading ed-panel__heading--benefit"><div class="ed-panel__title"><span class="ed-section-icon is-amber"><i class="bi bi-gift"></i></span><div><h2>สวัสดิการของคุณ <span class="ed-count">{{ employee.benefitPlans?.length ?? 0 }} กลุ่ม</span></h2><p>สิทธิประโยชน์ที่ได้รับตามกลุ่มพนักงาน</p></div></div><label v-if="employee.benefitPlans?.length" class="ed-search"><i class="bi bi-search"></i><input v-model="benefitSearch" type="search" placeholder="ค้นหากลุ่มหรือชื่อสวัสดิการ" aria-label="ค้นหาสวัสดิการ" /></label></header>
        <div v-if="employee.benefitPlans?.length && filteredPlans.length" class="ed-benefit-grid"><article v-for="p in filteredPlans" :key="p.id" class="ed-plan"><header><h3>{{ p.nameTh }}</h3><span>{{ p.items?.length ?? 0 }} รายการ</span></header><div v-if="p.items?.length" class="ed-table-wrap"><table class="ed-benefit-table"><caption class="ed-sr-only">สวัสดิการในกลุ่ม {{ p.nameTh }}</caption><thead><tr><th scope="col">สวัสดิการ</th><th scope="col">วงเงิน (บาท)</th></tr></thead><tbody><tr v-for="item in p.items" :key="item.benefitId"><td>{{ item.benefitNameTh }}</td><td>{{ formatAmount(item.limitAmount) }}</td></tr></tbody></table></div><p v-else class="ed-plan__empty">ยังไม่มีสวัสดิการที่กำหนด</p></article></div>
        <div v-else-if="employee.benefitPlans?.length" class="ed-state ed-state--inside"><i class="bi bi-search"></i><h3>ไม่พบสวัสดิการที่ค้นหา</h3><button class="ed-button ed-button--outline" type="button" @click="benefitSearch = ''">ล้างคำค้นหา</button></div>
        <div v-else class="ed-state ed-state--inside"><i class="bi bi-gift"></i><h3>ยังไม่ได้กำหนดกลุ่มสวัสดิการ</h3><p>กรุณาติดต่อฝ่ายบุคคล</p></div>
      </section>

      <section v-else-if="activeTab === 'team'" class="ed-panel"><header class="ed-panel__heading"><div class="ed-panel__title"><span class="ed-section-icon is-purple"><i class="bi bi-diagram-3"></i></span><div><h2>สายการบังคับบัญชา</h2><p>Organization chart</p></div></div><span class="ed-chart-hint"><i class="bi bi-arrows-move"></i>เลื่อนเพื่อดูโครงสร้างทั้งหมด</span></header><div v-if="orgChartLoading" class="ed-state ed-state--inside" role="status" aria-busy="true"><span class="loading loading-spinner"></span><p>กำลังโหลดสายการบังคับบัญชา</p></div><div v-else-if="orgChartError" class="ed-state ed-state--inside" role="alert"><h3>โหลดสายการบังคับบัญชาไม่สำเร็จ</h3><button class="ed-button" type="button" @click="loadOrgChart">ลองใหม่</button></div><div v-else-if="employee.orgChart" class="ed-chart-scroll" tabindex="0" role="region" aria-label="แผนผังสายการบังคับบัญชา เลื่อนเพื่อดูข้อมูล"><div class="ed-chart-canvas"><OrgChartNode :node="employee.orgChart" variant="dashboard" /></div></div><div v-else class="ed-state ed-state--inside"><i class="bi bi-diagram-3"></i><h3>ไม่มีข้อมูลสายการบังคับบัญชา</h3><p>ยังไม่มีข้อมูลโครงสร้างองค์กรของพนักงาน</p></div></section>
    </template>
    <section v-else class="ed-state ed-state--error"><i class="bi bi-person-exclamation"></i><h2>ไม่พบข้อมูลพนักงาน</h2><p>กรุณาลองใหม่อีกครั้ง หรือติดต่อฝ่าย IT</p><button class="ed-button" type="button" @click="loadEmployee">ลองใหม่</button></section>
  </main>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from "vue";
import { useAuthStore } from "../stores/authStore.ts";
import { useRouter } from "vue-router";
import { useEmployeeStore } from "../stores/employeeStore.ts";
import type { Employee } from "../types/Employee";
import OrgChartNode from "../components/employee/OrgChartNode.vue";
const router = useRouter();
const APPROVE_PATH = "/approvals";
const docTypeLabel: Record<string, string> = {
  BenefitClaim: "ใบเบิกสวัสดิการ",
  PettyCash: "ใบเบิกเงินสดย่อย",
};
const authStore = useAuthStore();
const employeeStore = useEmployeeStore();
const orgChartLoading = ref(false);
const orgChartLoaded = ref(false);
const orgChartError = ref(false);
const employee = ref<Employee | null>(null);
const loading = ref(true);
const loadError = ref<"network" | null>(null);
const activeTab = ref("profile");
const initials = computed(
  () => (employee.value?.fNameTh?.charAt(0) || employee.value?.fNameEn?.charAt(0) || "U").toUpperCase(),
);
const formatDate = (
  dateString: string | null | undefined,
) => {
  if (!dateString) return "-";
  const date = new Date(dateString);
  if (Number.isNaN(date.getTime())) return "-";
  return date.toLocaleDateString("th-TH", {
    year: "numeric",
    month: "long",
    day: "numeric",
  });
};
const pendingItems = computed(() =>
  (employee.value?.approvalSummary ?? [])
    .filter((x) => x.qtyWaitApprove > 0)
    .map((x) => ({
      ...x,
      label: docTypeLabel[x.docType] ?? x.docType,
    })),
);
const pendingTotal = computed(() =>
  pendingItems.value.reduce((sum, x) => sum + x.qtyWaitApprove, 0),
);
function goApprove() {
  router.push({
    path: APPROVE_PATH
  });
}
async function loadEmployee() {
  loading.value = true;
  loadError.value = null;
  orgChartLoaded.value = false;
  orgChartError.value = false;
  try {
    employee.value = await authStore.fetchCurrentEmployee(false, true);
    // console.log(employee.value?.approvalSummary);
  } catch (error: any) {
    employee.value = null;
    if (error?.response?.status !== 404) loadError.value = "network";
    console.error("Failed to fetch employee:", error);
  } finally {
    loading.value = false;
  }
}
async function loadOrgChart() {
  if (!employee.value || orgChartLoading.value || orgChartLoaded.value) return;
  orgChartLoading.value = true;
  orgChartError.value = false;
  try {
    const detail = await employeeStore.getById(employee.value.id, true);
    employee.value.orgChart = detail.orgChart ?? null;
    orgChartLoaded.value = true;
  } catch (err) {
    orgChartError.value = true;
    console.error("Failed to load org chart:", err);
  } finally {
    orgChartLoading.value = false;
  }
}
watch(activeTab, (tab) => {
  if (tab === "team") loadOrgChart();
});
onMounted(loadEmployee);

const profileFields = computed(() => employee.value ? [
  { label: "รหัสพนักงาน", value: employee.value.empId },
  { label: "ประเภทพนักงาน", value: employee.value.employeeTypeNameTh },
  { label: "ตำแหน่ง", value: employee.value.positionNameTh },
  { label: "ระดับตำแหน่ง", value: employee.value.positionLevel },
  ...(employee.value.reportToId ? [{ label: "ผู้บังคับบัญชา", value: employee.value.reportToNameTh }] : []),
  { label: "อีเมล", value: employee.value.email },
  { label: "วันที่เริ่มงาน", value: formatDate(employee.value.hireDate) },
] : []);
const organizationFields = computed(() => employee.value ? [
  { label: "บริษัท", value: employee.value.companyNameTh },
  { label: "สายงาน", value: employee.value.groupNameTh },
  { label: "ฝ่าย", value: employee.value.departmentNameTh },
  { label: "แผนก", value: employee.value.divisionNameTh },
  { label: "ส่วน", value: employee.value.sectionNameTh },
  { label: "สาขา", value: employee.value.branchNameTh },
] : []);
const benefitSearch = ref("");
const filteredPlans = computed(() => {
  const q = benefitSearch.value.trim().toLocaleLowerCase();
  return (employee.value?.benefitPlans ?? []).map(plan => ({
    ...plan,
    items: !q || (plan.nameTh ?? "").toLocaleLowerCase().includes(q)
      ? plan.items
      : plan.items?.filter(item => (item.benefitNameTh ?? "").toLocaleLowerCase().includes(q)),
  })).filter(plan => !q || (plan.nameTh ?? "").toLocaleLowerCase().includes(q) || !!plan.items?.length);
});
function formatAmount(value: number | null | undefined) {
  return value == null || !Number.isFinite(Number(value)) ? "-" : Number(value).toLocaleString("th-TH", { maximumFractionDigits: 2 });
}

</script>


<style scoped src="../assets/css/EmployeeDashboard.css"></style>

<template>
  <div class="space-y-3">
    <!-- ยังไม่มี docNumber (เอกสารยังไม่ถูกสร้าง) -->
    <p v-if="!docNumber" class="text-xs text-base-content/40">
      บันทึกเอกสารก่อน ถึงจะแนบไฟล์ได้
    </p>

    <template v-else>
      <!-- ปุ่มอัปโหลด -->
      <div v-if="!readonly" class="flex items-center gap-2">
        <input
          ref="fileInputRef"
          type="file"
          class="hidden"
          @change="onFileSelected"
        />
        <button
          type="button"
          class="btn btn-sm btn-ghost border border-base-300 gap-1.5"
          :disabled="isUploading"
          @click="fileInputRef?.click()"
        >
          <Paperclip class="size-3.5" />
          {{ isUploading ? 'กำลังอัปโหลด...' : 'แนบไฟล์' }}
        </button>
        <span class="text-[11px] text-base-content/35">PDF, รูปภาพ, Excel, Word ไม่เกิน 10 MB</span>
      </div>

      <!-- Loading -->
      <div v-if="isLoading" class="space-y-1.5">
        <div v-for="i in 2" :key="i" class="skeleton h-10 w-full rounded-lg"></div>
      </div>

      <!-- รายการไฟล์ -->
      <div v-else-if="items.length > 0" class="space-y-1.5">
        <div
  v-for="item in items"
  :key="item.id"
  class="flex items-center gap-2.5 rounded-lg border border-base-200 px-3 py-2"
>
  <!-- ถ้าเป็นรูปภาพ โชว์ Thumbnail คลิกเพื่อดูขยาย -->
  <img
    v-if="isImage(item) && previewUrls[item.id]"
    :src="previewUrls[item.id]"
    class="size-9 rounded object-cover cursor-pointer shrink-0"
    @click="openPreview(item)"
  />
  <FileIcon v-else class="size-4 text-base-content/40 shrink-0" />

  <div class="min-w-0 flex-1">
    <p class="text-xs font-medium truncate">{{ item.fileName }}</p>
    <p class="text-[11px] text-base-content/35">{{ formatSize(item.fileSize) }}</p>
  </div>
  <button type="button" class="btn btn-ghost btn-xs btn-square" title="ดาวน์โหลด" @click="downloadFile(item)">
    <Download class="size-3.5" />
  </button>
  <button
    v-if="!readonly"
    type="button"
    class="btn btn-ghost btn-xs btn-square text-error/70"
    title="ลบ"
    @click="removeFile(item)"
  >
    <Trash2 class="size-3.5" />
  </button>
</div>

<!-- Modal ดูรูปขยาย -->
<dialog ref="previewDialogRef" class="modal">
  <div class="modal-box max-w-3xl p-2">
    <img v-if="previewImageUrl" :src="previewImageUrl" class="w-full rounded-lg" />
  </div>
  <form method="dialog" class="modal-backdrop">
    <button>close</button>
  </form>
</dialog>
      </div>

      <p v-else class="text-xs text-base-content/35">ยังไม่มีไฟล์แนบ</p>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import { Paperclip, Download, Trash2, FileIcon } from 'lucide-vue-next'
import hrApi from '../../services/hrApi'
import { notify, extractErrorMessage } from '../../utils/notify'
import type { Attachment } from '../../types/Attachment'

const props = defineProps<{
  docType: string
  docNumber: string | null
  readonly?: boolean
  target?: HTMLElement | null
}>()

const items = ref<Attachment[]>([])
const isLoading = ref(false)
const isUploading = ref(false)
const fileInputRef = ref<HTMLInputElement>()
const previewUrls = ref<Record<string, string>>({})
const previewDialogRef = ref<HTMLDialogElement>()
const previewImageUrl = ref('')

function isImage(item: Attachment) {
  return item.contentType.startsWith('image/')
}

async function fetchList() {
  if (!props.docNumber) return
  isLoading.value = true
  try {
    const res = await hrApi.get('/Attachment', {
      params: { docType: props.docType, docNumber: props.docNumber },
    })
    items.value = res.data

    for (const item of items.value) {
      if (isImage(item) && !previewUrls.value[item.id]) {
        const fileRes = await hrApi.get(item.url, { responseType: 'blob' })
        previewUrls.value[item.id] = window.URL.createObjectURL(fileRes.data)
      }
    }
  } catch (err) {
    console.error('Failed to load attachments:', err)
  } finally {
    isLoading.value = false
  }
}

function openPreview(item: Attachment) {
  previewImageUrl.value = previewUrls.value[item.id]
  previewDialogRef.value?.showModal()
}

async function onFileSelected(e: Event) {
  const input = e.target as HTMLInputElement
  const file = input.files?.[0]
  if (!file || !props.docNumber) return

  isUploading.value = true
  try {
    const formData = new FormData()
    formData.append('docType', props.docType)
    formData.append('docNumber', props.docNumber)
    formData.append('file', file)

    await hrApi.post('/Attachment', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    })

    await fetchList()
    await notify.success('แนบไฟล์สำเร็จ', 'สำเร็จ', props.target)
  } catch (err) {
    await notify.error(extractErrorMessage(err), 'แนบไฟล์ไม่สำเร็จ', props.target)
  } finally {
    isUploading.value = false
    input.value = ''
  }
}

async function downloadFile(item: Attachment) {
  try {
    const res = await hrApi.get(item.url, { responseType: 'blob' })
    const blobUrl = window.URL.createObjectURL(res.data)
    const link = document.createElement('a')
    link.href = blobUrl
    link.download = item.fileName
    link.click()
    window.URL.revokeObjectURL(blobUrl)
  } catch (err) {
    await notify.error('ดาวน์โหลดไฟล์ไม่สำเร็จ', 'เกิดข้อผิดพลาด', props.target)
  }
}

async function removeFile(item: Attachment) {
  const ok = await notify.confirm(`ยืนยันลบไฟล์ "${item.fileName}" ใช่หรือไม่`, 'ยืนยันการลบ', props.target)
  if (!ok) return
  try {
    await hrApi.delete(`/Attachment/${item.id}`)
    await fetchList()
    await notify.success('ลบไฟล์สำเร็จ', 'สำเร็จ', props.target)
  } catch (err) {
    await notify.error(extractErrorMessage(err), 'ลบไม่สำเร็จ', props.target)
  }
}

function formatSize(bytes: number) {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / 1024 / 1024).toFixed(1)} MB`
}

// เปิด Modal Edit เอกสารคนละใบ ต้องโหลดรายการไฟล์ใหม่ตาม docNumber ที่เปลี่ยน
watch(() => props.docNumber, fetchList)
onMounted(fetchList)
</script>
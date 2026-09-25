import Swal from 'sweetalert2'

const SWAL_BASE = {
  confirmButtonText: 'ตกลง',
  buttonsStyling: false,
  customClass: {
    popup: 'rounded-2xl shadow-lg',
    title: 'text-lg font-semibold',
    actions: 'gap-3',
    confirmButton: 'btn btn-primary px-6',
    cancelButton: 'btn btn-ghost border border-base-300 px-6',
  },
}

export const notify = {
  success(message: string, title = 'สำเร็จ', target?: HTMLElement | null) {
    return Swal.fire({
      ...SWAL_BASE,
      icon: 'success',
      title,
      text: message,
      target: target ?? document.body,
      customClass: { ...SWAL_BASE.customClass, confirmButton: 'btn btn-success px-6' },
    })
  },
  error(message: string, title = 'เกิดข้อผิดพลาด', target?: HTMLElement | null) {
    return Swal.fire({
      ...SWAL_BASE,
      icon: 'error',
      title,
      text: message,
      target: target ?? document.body,
      customClass: { ...SWAL_BASE.customClass, confirmButton: 'btn btn-error px-6' },
    })
  },
  async confirm(message: string, title = 'ยืนยันการทำรายการ', target?: HTMLElement | null) {
    const result = await Swal.fire({
      ...SWAL_BASE,
      icon: 'warning',
      title,
      text: message,
      showCancelButton: true,
      cancelButtonText: 'ยกเลิก',
      confirmButtonText: 'ยืนยัน',
      target: target ?? document.body,
      customClass: { ...SWAL_BASE.customClass, confirmButton: 'btn btn-error px-6' },
      reverseButtons: true,
    })
    return result.isConfirmed
  },
}

export function extractErrorMessage(err: any): string {
  const data = err?.response?.data
  if (typeof data === 'string') return data
  return data?.message || err?.message || 'เกิดข้อผิดพลาดที่ไม่ทราบ'
}
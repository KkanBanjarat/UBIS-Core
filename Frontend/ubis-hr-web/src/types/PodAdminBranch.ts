export interface BranchAdminItem {
  id: string;
  userId: string;
  employeeId: string | null;
  employeeNameTh: string | null;
  isPrimary: boolean;
  updatedAt: string;
  updatedBy: string;
}

export interface BranchAdminGroup {
  branchId: string;
  branchCode: string;
  branchNameTh: string;
  branchNameEn: string;
  admins: BranchAdminItem[];
}

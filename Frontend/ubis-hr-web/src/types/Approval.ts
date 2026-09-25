export interface Approval {
  transApproveId: number;
  docType: string;
  docNumber: string;
  docRev: number;
  round: number;
  stepNo: number;
  docDate: string;
  employeeNameTh: string;
  documentDetail: string | null;
}

export interface ApprovalStep {
  stepNo: number;
  approverNameTh: string;
  status: string;
  actualApproveNameTh: string | null;
  approvedDate: string | null;
}

export interface ApprovalReason {
  status: string;
  reason: string;
  createdBy: string;
  createdAt: string;
}

export interface ApprovalTrail {
  steps: ApprovalStep[];
  reasons: ApprovalReason[];
}

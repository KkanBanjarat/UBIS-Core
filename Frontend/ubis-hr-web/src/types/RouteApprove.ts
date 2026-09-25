export type ApproverType = "ManagerChain" | "FixedEmployee" | "BranchAdmin";

export interface RouteApprove {
  id: string;
  docType: string;
  stepNo: number;
  stepName: string;
  approverType: ApproverType;
  minPositionLevel: number | null;
  fixedEmployeeId: string | null;
  fixedEmployeeNameTh: string | null;
  organizationUnitId: string | null;
  organizationUnitNameTh: string | null;
  isActive: boolean;
  updatedBy: string;
  updatedAt: string;
}

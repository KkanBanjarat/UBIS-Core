import { InteractionRequiredAuthError } from "@azure/msal-browser";
import { msalInstance, ensureMsalInitialized } from "./msalConfig";

// สิทธิ์แบบ Delegated (ทำแทนผู้ที่ Login อยู่) ต้องให้แอดมินของ tenant กด Grant admin consent ที่ Azure ครั้งเดียว
const GRAPH_SCOPES = ["User.Read.All"];

const GRAPH_USERS_URL =
  "https://graph.microsoft.com/v1.0/users" +
  "?$select=id,displayName,mail,userPrincipalName,accountEnabled,employeeId,userType" +
  "&$top=999";

// ตรงกับ EntraUserInputDto ฝั่ง .NET
export interface EntraUserInput {
  entraObjectId: string;
  email: string;
  displayName: string;
  employeeCode: string | null;
  isActive: boolean;
}

async function getGraphToken(): Promise<string> {
  await ensureMsalInitialized();

  const account =
    msalInstance.getActiveAccount() ?? msalInstance.getAllAccounts()[0];
  if (!account) {
    throw new Error(
      "ต้อง Login ด้วย Microsoft ก่อนจึงจะ Sync ได้ (ถ้า Login ด้วยรหัสผ่านหรือเพิ่งปิดแท็บไป ให้ Logout แล้ว Login ด้วย Microsoft ใหม่)",
    );
  }

  try {
    const result = await msalInstance.acquireTokenSilent({
      scopes: GRAPH_SCOPES,
      account,
    });
    return result.accessToken;
  } catch (err) {
    if (err instanceof InteractionRequiredAuthError) {
      throw new Error(
        "ยังไม่ได้รับสิทธิ์อ่านรายชื่อผู้ใช้ ให้แอดมินของ tenant กด Grant admin consent สิทธิ์ User.Read.All (Delegated) ที่ Azure แล้ว Login ใหม่",
      );
    }
    throw err;
  }
}

export async function fetchEntraUsers(): Promise<EntraUserInput[]> {
  const token = await getGraphToken();

  const raw: any[] = [];
  let url: string | null = GRAPH_USERS_URL;
  while (url) {
    const res: Response = await fetch(url, {
      headers: { Authorization: `Bearer ${token}` },
    });
    if (!res.ok) {
      throw new Error(`เรียก Microsoft Graph ไม่สำเร็จ (${res.status})`);
    }
    const json = await res.json();
    raw.push(...json.value);
    url = json["@odata.nextLink"] ?? null;
  }

  return raw
    .filter((u) => u.id && u.userType !== "Guest")
    .map((u) => ({
      entraObjectId: u.id as string,
      email: String(u.mail ?? u.userPrincipalName ?? "")
        .trim()
        .toLowerCase(),
      displayName: (u.displayName ?? "") as string,
      employeeCode: u.employeeId ? String(u.employeeId).trim() : null,
      isActive: u.accountEnabled ?? true,
    }));
}

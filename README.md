# UBIS Core (HR)

ระบบ HR ภายในองค์กร: เบิกเงินสดย่อย, เบิกสวัสดิการ, ระบบอนุมัติเอกสารหลายลำดับ, จัดการผู้ใช้/Role/เมนู/สิทธิ์

## สารบัญ
1. [สถาปัตยกรรม](#1-สถาปัตยกรรม)
2. [เริ่มต้นใช้งาน](#2-เริ่มต้นใช้งาน)
3. [การตั้งค่า](#3-การตั้งค่า)
4. [โครงสร้างโค้ด .NET](#4-โครงสร้างโค้ด-net)
5. [สิทธิ์การใช้งาน](#5-สิทธิ์การใช้งาน)
6. [ระบบอนุมัติเอกสาร](#6-ระบบอนุมัติเอกสาร)
7. [Frontend](#7-frontend)
8. [คำสั่งที่ใช้บ่อย](#8-คำสั่งที่ใช้บ่อย)
9. [ปัญหาที่พบบ่อย](#9-ปัญหาที่พบบ่อย)
10. [Checklist ก่อนส่งงาน](#10-checklist-ก่อนส่งงาน)

## 1. สถาปัตยกรรม

| ส่วน | เทคโนโลยี |
|---|---|
| Frontend | Vue 3, Pinia, Axios, Vite, Tailwind CSS 4, DaisyUI 5 |
| HR API | .NET 10, EF Core, Npgsql (PostgreSQL) |
| Access API | .NET 10, EF Core, Npgsql (PostgreSQL) |
| ยืนยันตัวตน | JWT (ออกโดย Access API) + Microsoft Entra ID (SSO) |
| แจ้งเตือน | Teams ผ่าน Power Automate (Adaptive Card) |

- **Access API** ดูแลผู้ใช้ Role เมนู สิทธิ์ และออก JWT
- **HR API** ตรวจ JWT ที่ Access ออกให้ แล้วให้บริการงานเอกสารและอนุมัติ
- ทั้งสอง API ต้องใช้ `Jwt:Secret` **ค่าเดียวกัน**

```
UBIS-Core/
├─ .vscode/                      # launch.json, tasks.json, settings.json
├─ UBIS.slnx
├─ Frontend/ubis-hr-web/         # Vue 3
└─ Services/
   ├─ HR/      (UBIS.HR.Api | .Application | .Infrastructure | .Domain)
   └─ Access/  (UBIS.Access.Api | .Application | .Infrastructure | .Domain)
```

## 2. เริ่มต้นใช้งาน

**สิ่งที่ต้องมี:** .NET SDK 10, Node.js, PostgreSQL (TODO: ระบุเวอร์ชัน), `dotnet-ef` (`dotnet tool install --global dotnet-ef`)

1. สร้างฐานข้อมูล HR และ Access (TODO: ระบุวิธีสร้างตั้งต้น)
2. ตั้งค่า secret ของแต่ละ API (ดูหัวข้อ 3)
3. ติดตั้ง package หน้าบ้าน: `cd Frontend/ubis-hr-web && npm install`
4. รันระบบ

### รันด้วย VS Code (แนะนำ)

เปิดโฟลเดอร์ root ใน VS Code แล้วกด **F5** เลือกชุดรัน

| ชุด | รันอะไร |
|---|---|
| `BeHr` | HR API |
| `BeAccess` | Access API |
| `FeWeb` | Frontend (Vite) + เปิด Chrome ที่ `http://localhost:5173` |
| `Backend All` | HR + Access |
| `Full Stack` | HR + Access + Frontend |

- ทุกชุด build ก่อนรันอัตโนมัติ และ Stop ชุดรวมแล้วหยุดพร้อมกัน
- task `frontend: dev` เรียก `vite.cmd` ใช้ได้เฉพาะ Windows (macOS/Linux เปลี่ยนเป็น `vite`)
- ต้องติดตั้ง C# Dev Kit และ Chrome

### รันด้วยคำสั่ง

```powershell
dotnet run --project Services/Access/UBIS.Access.Api
dotnet run --project Services/HR/UBIS.HR.Api
cd Frontend/ubis-hr-web; npm run dev     # http://localhost:5173
```

Swagger ของ Access (Development): `/BeAccess` (TODO: ระบุพอร์ตของ API ทั้งสอง)

## 3. การตั้งค่า

เก็บค่าลับด้วย `dotnet user-secrets` เท่านั้น (ห้ามใส่ใน `appsettings.json` และห้าม commit)

```powershell
cd Services/Access/UBIS.Access.Api
dotnet user-secrets set "Jwt:Secret" "<ค่าลับ>"
```

| Key | ใช้ใน | ใช้ทำอะไร |
|---|---|---|
| `ConnectionStrings:HrDatabase` | HR | ฐานข้อมูล HR |
| `ConnectionStrings:AccessDatabase` | Access | ฐานข้อมูล Access |
| `Jwt:Secret`, `Jwt:ExpiresInMinutes` | HR, Access | ลงนาม/ตรวจ JWT (อายุเริ่มต้น 480 นาที) |
| `Azure:TenantId`, `Azure:ClientId` | Access | Entra ID (ClientId ต้องตรงกับ `VITE_AZURE_CLIENT_ID`) |
| `Azure:ClientSecret` | Access | ดึงรายชื่อผู้ใช้จาก Microsoft Graph (Sync) ต้องมีสิทธิ์ `User.Read.All` แบบ Application + Admin consent |
| `SecurityHeaders:MySecretValue` | Access | API Key ใน header `X-Api-Key` (middleware บังคับทุก request) |
| `ApiKey` | Access | endpoint ระหว่างระบบ |
| `Teams:WebhookUrl`, `Teams:FrontendUrl` | HR | แจ้งเตือน Teams และลิงก์กลับเข้าระบบ |

Frontend (`.env`): `VITE_AZURE_CLIENT_ID`, `VITE_AZURE_TENANT_ID` (TODO: เพิ่ม URL ของ API)

## 4. โครงสร้างโค้ด .NET

```
Api            → Controllers, Program.cs (DI, auth, middleware)
Application    → Interfaces (IXxxService), Dtos
Infrastructure → Services, Repositories (IXxxRepos), DbContext, Teams/Email
Domain         → Entities (มาจาก scaffold)
```

- Controller บาง: รับ DTO → เรียก Service → แปลงผลเป็น HTTP
- Service interface อยู่ `Application/Interfaces` ส่วน implementation อยู่ `Infrastructure/Services` ลงทะเบียนใน `Program.cs` ด้วย `AddScoped`
- **Entity และ DbContext มาจาก scaffold ห้ามแก้ด้วยมือ** (รันซ้ำแล้วหาย)
- การกรอง/แบ่งหน้าทำที่ Server ด้วย `XxxFilterDto` และ `PagedResultDto<T>` (`Items`, `TotalCount`)

**การจัดการ Error**
- กฎธุรกิจไม่ผ่าน: `throw new InvalidOperationException("ข้อความไทย")` → Controller ตอบ `409`
- ข้อมูลไม่ถูกต้อง: `ArgumentException` → `400`
- ไม่พบข้อมูล: Service คืน `null`/`false` → `404`

**Transaction**
- งานที่แก้หลายตารางครอบด้วย `ITransactionService.RunAsync(...)` (ซ้อนกันได้ ตัวนอกสุด commit)
- งานนอกฐานข้อมูล (เช่น แจ้ง Teams) ใช้ `OnCommittedAsync(...)` ให้ทำหลัง commit สำเร็จเท่านั้น
- เตรียมข้อมูลที่ต้องอ่านเยอะ (เช่น หาผู้อนุมัติ) ก่อนเข้า transaction

## 5. สิทธิ์การใช้งาน

- สิทธิ์กำหนดต่อ **เมนู**: 0 ไม่มีสิทธิ์, 1 Read, 2 Write, 3 All
- Access API ใส่สิทธิ์ใน JWT: claim `perm` (`รหัสสิทธิ์:scope`) และ `menu` (`รหัสเมนู:ระดับ`)
- หน้าบ้านดึงเมนูและระดับจาก `GET /me/menus` แล้วสร้าง route จากข้อมูลนั้น (อัปเดตเองเบื้องหลัง) ส่วนค่าใน JWT เปลี่ยนเมื่อล็อกอินใหม่
- ใช้ `usePermission(code)` ซ่อนปุ่มตามระดับ

| ระดับ | ทำได้ |
|---|---|
| Read (1) | ดู ค้นหา พรีวิว/พิมพ์ |
| Write (2) | Read + สร้าง/แก้ไข/ส่งอนุมัติ/ดึงกลับ |
| All (3) | Write + ลบ และงานผู้ดูแล |

**เพิ่มหน้าใหม่**
1. สร้างไฟล์ view ใต้ `src/views/...` (ต้องอยู่ใต้ `views/` เพราะ router โหลดด้วย `import.meta.glob("../views/**/*.vue")`)
2. เพิ่มเมนูที่หน้า **เมนู**: `path`, `componentPath` (เช่น `views/xxx/XxxView.vue`), `permissionCode`
3. กำหนดระดับให้แต่ละ Role ที่หน้า **Role และสิทธิ์**
4. ในหน้าใช้
```typescript
const { canWrite, canAll } = usePermission('petty-cash') // = permissionCode ของเมนู
```

> การซ่อนปุ่มเป็น UX เท่านั้น การบังคับสิทธิ์ระดับ endpoint ฝั่ง Backend อยู่ระหว่างดำเนินการ (TODO)

## 6. ระบบอนุมัติเอกสาร

ใช้ร่วมกันทุกเอกสารผ่าน `IApprovalDocumentService` (ตอนนี้รองรับ `PettyCash`, `BenefitClaim`)

- ส่งอนุมัติ → สร้างลำดับ (ลำดับที่ 1 = `WaitApprove`, ที่เหลือ = `Pending`) → อนุมัติทีละลำดับ
- ไม่อนุมัติ/ตีกลับ: ลำดับที่เหลือถูกปิด และบันทึกเหตุผล
- ดึงกลับ (Recall): ลำดับเดิมเป็น `Recalled` ส่งใหม่จะเริ่ม `Round` ถัดไป
- สถานะเอกสาร: `Draft → WaitApprove → Approved | Rejected (ตีกลับ) | Disapproved (ไม่อนุมัติ)`
- หน้า "รายการรออนุมัติ" กรอง/ค้นหาที่ฐานข้อมูล (`GET /Approval/my-pending` รับ `search`, `docType`, `dateFrom`, `dateTo`)

**เพิ่มเอกสารชนิดใหม่**
1. ตาราง/Entity + เลขที่เอกสารด้วย `IDocNumberService.GenerateAsync(DocType)`
2. Service ของเอกสาร implement `IApprovalDocumentService`: `DocType`, `GetSummaryAsync`, `GetSummariesAsync(docNumbers, filter)` (กรองที่ DB), `MarkApprovedAsync`, `MarkDeniedAsync`
3. `SubmitAsync`/`RecallAsync` ครอบด้วย `_tx.RunAsync`
   - Submit: เปลี่ยนเป็น `WaitApprove` → `ApprovalService.CreateApprovalAsync(docType, docNum, 1, approvers)` (หาผู้อนุมัติด้วย `IApprovalRouteResolverService.ResolveAsync`)
   - Recall: เปลี่ยนเป็น `Draft` → `ApprovalService.RecallAsync(...)`
4. ลงทะเบียน DI ให้ resolve ได้เป็น `IApprovalDocumentService`
5. หน้าบ้าน: เพิ่มชนิดเอกสารใน `config/approvalDocuments`
6. ตั้งสายอนุมัติของ DocType ใหม่ที่หน้า "สายอนุมัติเอกสาร"

**แจ้งเตือน Teams:** ส่งการ์ดถึงผู้อนุมัติรายคนผ่าน Power Automate flow "UBIS Approval Notify" หลัง commit สำเร็จ ปุ่มในการ์ดลิงก์ไป `/approvals?docType=..&docNumber=..` การตอบ 2xx ของ webhook แปลว่ารับคำขอ ไม่ได้แปลว่าส่งถึง ให้ดูผลที่ Run history ของ flow

## 7. Frontend

- State อยู่ใน Pinia (`stores/xxxStore.ts`) เรียก API ผ่าน `services/hrApi` (HR) และ `services/accessApi` (Access)
- รายการแบบแบ่งหน้า/ค้นหาทำที่ Server: ช่องค้นหา debounce 300–400 ms ตัวเลือกอื่นเรียกทันที
- กัน response ซ้อนกัน (ผลเก่าทับผลใหม่) ด้วย request id
```typescript
let reqId = 0
const my = ++reqId
const res = await api.get(...)
if (my !== reqId) return
```
- ตั้ง `isLoading` เริ่มต้นเป็น `true` และแยก `errorMessage` ออกจากสถานะ "ไม่มีข้อมูล" เพื่อไม่ให้ขึ้น "ไม่มีรายการ" ระหว่างโหลดหรือเมื่อ error
- แจ้งผลด้วย `notify.success/error/confirm/info` (`utils/notify`)
- Modal ใช้ `<dialog>` ของ DaisyUI ถ้าจะแสดง SweetAlert ทับ modal ให้ส่ง element ของ dialog เป็น `target` ไม่เช่นนั้นกล่องจะไปอยู่หลัง modal

## 8. คำสั่งที่ใช้บ่อย

สร้าง Entity/DbContext จากฐานข้อมูล (Database-First) รันที่โฟลเดอร์ root `-f` เขียนทับไฟล์เดิม ให้ commit ก่อนเสมอ

```powershell
# HR
dotnet ef dbcontext scaffold "Name=ConnectionStrings:HrDatabase" Npgsql.EntityFrameworkCore.PostgreSQL -o ../UBIS.HR.Domain/Entities -c HrDbContext --context-dir Data --namespace UBIS.HR.Domain.Entities --context-namespace UBIS.HR.Infrastructure.Data -f --project Services/HR/UBIS.HR.Infrastructure --startup-project Services/HR/UBIS.HR.Api

# Access
dotnet ef dbcontext scaffold "Name=ConnectionStrings:AccessDatabase" Npgsql.EntityFrameworkCore.PostgreSQL -o ../UBIS.Access.Domain/Entities -c AccessDbContext --context-dir Data --namespace UBIS.Access.Domain.Entities --context-namespace UBIS.Access.Infrastructure.Data -f --project Services/Access/UBIS.Access.Infrastructure --startup-project Services/Access/UBIS.Access.Api
```

## 9. ปัญหาที่พบบ่อย

| อาการ | สาเหตุ / วิธีแก้ |
|---|---|
| HR API ตอบ 401 ทั้งที่ล็อกอินแล้ว | `Jwt:Secret` ของ HR และ Access ไม่ตรงกัน แก้ให้เหมือนกัน รีสตาร์ททั้งคู่ แล้วล็อกอินใหม่ |
| ทุก request ได้ 401 "ไม่พบ API Key" | ต้องแนบ header `X-Api-Key` (ค่าจาก `SecurityHeaders:MySecretValue`) ยกเว้น Swagger/health |
| เมนู/ปุ่มเก่าค้างหลังแก้สิทธิ์ | รีเฟรชหรือล็อกอินใหม่ (หน้าบ้านดึงเมนูใหม่เบื้องหลัง แต่ค่าใน JWT เปลี่ยนเมื่อออก token ใหม่) |
| Teams ไม่ได้รับการ์ดทั้งที่ log ว่า "Sent" | ดู Run history ของ flow "UBIS Approval Notify" |
| กดลิงก์จาก Teams แล้วไม่เปิดรายละเอียด | ตรวจ `Teams:FrontendUrl` และดูว่ารายการยังอยู่ในคิวของผู้ใช้นั้นหรือไม่ |
| `dotnet user-secrets set` ฟ้อง Missing parameter | ต้องมีทั้ง key และ value: `set "Teams:WebhookUrl" "<url>"` |
| scaffold แล้วโค้ดหาย | `-f` เขียนทับ ให้ commit ก่อนและตรวจ `git diff` หลังรัน |
| Sync ผู้ใช้จาก Microsoft ไม่ผ่าน | ตรวจ `Azure:*`, สิทธิ์ `User.Read.All` (Application) และ Admin consent |


-- ปรับคอลัมน์ตัวเลขทศนิยมทั้งหมดของ HR เป็น numeric(18,3)
-- ให้ตรงกับ HasPrecision(18, 3) ใน HrDbContext และ DecimalPrecision.Places = 3
-- (เดิมคอลัมน์ยอดเงินเป็น numeric(18,2) ทำให้ DB ปัดยอดบรรทัด/ยอดรวมเหลือ 2 ตำแหน่ง
--  แต่ยอดแจกแจงใน CalcBreakdown เก็บ 3 ตำแหน่ง ยอดจึงไม่เท่ากัน)
-- ตรวจก่อนรัน: ค่าเดิมที่มีทศนิยมเกิน 3 ตำแหน่งจะถูกปัดเศษ
BEGIN;

ALTER TABLE tb_accommodation_rate
    ALTER COLUMN "DomesticRate"      TYPE numeric(18,3),
    ALTER COLUMN "InternationalRate" TYPE numeric(18,3);

ALTER TABLE tb_approval_limit
    ALTER COLUMN "AmountFrom" TYPE numeric(18,3),
    ALTER COLUMN "AmountTo"   TYPE numeric(18,3);

ALTER TABLE tb_benefit_claim
    ALTER COLUMN "TotalAmount" TYPE numeric(18,3);

ALTER TABLE tb_benefit_claim_line
    ALTER COLUMN "Amount"      TYPE numeric(18,3),
    ALTER COLUMN "FuelPrice"   TYPE numeric(18,3),
    ALTER COLUMN "LimitAmount" TYPE numeric(18,3),
    ALTER COLUMN "Qty"         TYPE numeric(18,3),
    ALTER COLUMN "Rate"        TYPE numeric(18,3);

ALTER TABLE tb_benefit_plan_item
    ALTER COLUMN "LimitAmount" TYPE numeric(18,3);

ALTER TABLE tb_doc_limit
    ALTER COLUMN "LimitAmount" TYPE numeric(18,3);

ALTER TABLE tb_fuel_rate
    ALTER COLUMN "MaxPrice"  TYPE numeric(18,3),
    ALTER COLUMN "MinPrice"  TYPE numeric(18,3),
    ALTER COLUMN "RatePerKm" TYPE numeric(18,3);

ALTER TABLE tb_per_diem_rate
    ALTER COLUMN "RatePerDay" TYPE numeric(18,3);

ALTER TABLE tb_petty_cash_line
    ALTER COLUMN "Amount"      TYPE numeric(18,3),
    ALTER COLUMN "LimitAmount" TYPE numeric(18,3),
    ALTER COLUMN "Qty"         TYPE numeric(18,3);

ALTER TABLE tb_petty_cash_request
    ALTER COLUMN "TotalAmount" TYPE numeric(18,3);

-- แก้ข้อมูลเดิม: ยอดบรรทัดที่ถูกปัดเหลือ 2 ตำแหน่ง ให้กลับมาเท่ากับผลรวมยอดแจกแจง
UPDATE tb_benefit_claim_line l
SET "Amount" = b.amount
FROM (
    SELECT l2."Id", round(sum((e->>'Amount')::numeric), 3) AS amount
    FROM tb_benefit_claim_line l2,
         jsonb_array_elements(l2."CalcBreakdown") e
    WHERE jsonb_typeof(l2."CalcBreakdown") = 'array'
    GROUP BY l2."Id"
) b
WHERE b."Id" = l."Id" AND l."Amount" <> b.amount;

-- คำนวณยอดรวมเอกสารใหม่จากยอดบรรทัด
UPDATE tb_benefit_claim h
SET "TotalAmount" = s.total
FROM (
    SELECT "BenefitClaimId", sum("Amount") AS total
    FROM tb_benefit_claim_line
    WHERE NOT "IsDelete"
    GROUP BY "BenefitClaimId"
) s
WHERE s."BenefitClaimId" = h."Id" AND h."TotalAmount" <> s.total;

COMMIT;

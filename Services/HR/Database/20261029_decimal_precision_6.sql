-- ปรับคอลัมน์ตัวเลขทศนิยมทั้งหมดของ HR จาก numeric(18,2) เป็น numeric(18,6)
-- ให้ตรงกับ HasPrecision(18, 6) ใน HrDbContext (การเพิ่ม scale ไม่ทำให้ข้อมูลเดิมเสีย)
BEGIN;

ALTER TABLE tb_accommodation_rate
    ALTER COLUMN "DomesticRate"      TYPE numeric(18,6),
    ALTER COLUMN "InternationalRate" TYPE numeric(18,6);

ALTER TABLE tb_approval_limit
    ALTER COLUMN "AmountFrom" TYPE numeric(18,6),
    ALTER COLUMN "AmountTo"   TYPE numeric(18,6);

ALTER TABLE tb_benefit_claim
    ALTER COLUMN "TotalAmount" TYPE numeric(18,6);

ALTER TABLE tb_benefit_claim_line
    ALTER COLUMN "Amount"       TYPE numeric(18,6),
    ALTER COLUMN "FuelPrice"   TYPE numeric(18,6),
    ALTER COLUMN "LimitAmount" TYPE numeric(18,6),
    ALTER COLUMN "Qty"          TYPE numeric(18,6),
    ALTER COLUMN "Rate"         TYPE numeric(18,6);

ALTER TABLE tb_benefit_plan_item
    ALTER COLUMN "LimitAmount" TYPE numeric(18,6);

ALTER TABLE tb_doc_limit
    ALTER COLUMN "LimitAmount" TYPE numeric(18,6);

ALTER TABLE tb_fuel_rate
    ALTER COLUMN "MaxPrice"    TYPE numeric(18,6),
    ALTER COLUMN "MinPrice"    TYPE numeric(18,6),
    ALTER COLUMN "RatePerKm"  TYPE numeric(18,6);

ALTER TABLE tb_per_diem_rate
    ALTER COLUMN "RatePerDay" TYPE numeric(18,6);

ALTER TABLE tb_petty_cash_line
    ALTER COLUMN "Amount"       TYPE numeric(18,6),
    ALTER COLUMN "LimitAmount" TYPE numeric(18,6),
    ALTER COLUMN "Qty"          TYPE numeric(18,6);

ALTER TABLE tb_petty_cash_request
    ALTER COLUMN "TotalAmount" TYPE numeric(18,6);

COMMIT;

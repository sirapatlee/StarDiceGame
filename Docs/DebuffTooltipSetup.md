# ตั้งค่า Debuff Icon + Tooltip ใน Unity (Step by Step)

เอกสารนี้อธิบายวิธีตั้งค่า UI ให้ระบบ Debuff ใหม่ใช้งานได้ครบ โดยตอนนี้ระบบรองรับ 2 โหมด:
- **โหมดแนะนำ:** แสดง debuff เป็น **Sprite Image icon** จาก Unity Store / sprite asset ของทีม
- **โหมด fallback:** แสดง debuff เป็น **TextMeshPro icon** (`🔥`, `❄️`) แบบเดิม
- ทั้งสองโหมดรองรับ tooltip ตอนเอาเมาส์ชี้

> สคริปต์ที่ใช้: `PlayerUIController` + `DebuffTooltipHoverHandler` + `DebuffSpriteIconHoverHandler`

---

## 1) เตรียม EventSystem

1. เปิด Scene ที่ใช้เล่นบอร์ดหลัก
2. ดูใน Hierarchy ว่ามี `EventSystem` แล้วหรือยัง
3. ถ้ายังไม่มี ให้สร้าง: `GameObject > UI > Event System`

เหตุผล: ระบบ hover ของ tooltip ใช้ event pointer จาก EventSystem

---

## 2) วิธีที่แนะนำ: ตั้งค่า Debuff เป็น Sprite Image Icon

### 2.1 สร้าง Container สำหรับ icon

1. ไปที่ Panel สถานะผู้เล่น (status panel)
2. สร้าง GameObject UI ใหม่ชื่อ `DebuffIconContainer`
3. แนะนำให้ใส่ component เหล่านี้:
   - `Horizontal Layout Group`
   - `Content Size Fitter` (Horizontal Fit = Preferred Size)
4. ตั้ง spacing ตามต้องการ เช่น `6 - 12`

> ระบบจะสร้าง icon ใต้ container นี้ให้อัตโนมัติ

### 2.2 เตรียม Sprite icon ที่ได้จาก Unity Store

1. Import icon asset เข้ามาในโปรเจกต์
2. เลือกไฟล์รูปที่ต้องการใช้ เช่น Burn / Ice
3. ใน Inspector ตั้งค่า Texture ให้ใช้งานกับ UI ได้ตามปกติ
4. ลาก Sprite ที่ต้องการไปผูกใน `PlayerUIController`

ช่องที่ต้องใช้:
- `Burn Debuff Sprite`
- `Ice Debuff Sprite`

### 2.3 (Optional) ถ้าอยากใช้ prefab icon ของตัวเอง

ถ้าทีมมี prefab UI icon ที่จัด layout/outline/shadow มาแล้ว ให้สร้าง prefab 1 ชิ้นแล้วใส่:
- `Image`
- `DebuffSpriteIconHoverHandler`

จากนั้นลากใส่ช่อง `Debuff Icon Prefab`

> ถ้าไม่ใส่ prefab ระบบจะสร้าง `Image` ให้เองอัตโนมัติ

### 2.4 ผูก Reference ใน PlayerUIController

เลือก GameObject ที่มี `PlayerUIController` แล้วตั้งค่าดังนี้:

1. `Debuff Icon Container` → ลาก `DebuffIconContainer`
2. `Burn Debuff Sprite` → ลาก sprite ของ Burn
3. `Ice Debuff Sprite` → ลาก sprite ของ Ice
4. `Debuff Tooltip Root` → ลาก `DebuffTooltip`
5. `Debuff Tooltip Text` → ลาก `DebuffTooltipText`
6. (Optional) `Debuff Icon Prefab` → ลาก prefab icon ของทีม
7. (Optional) `Debuff Icon Size` → ปรับขนาด icon กรณีให้ระบบสร้างอัตโนมัติ

เมื่อผูก `Debuff Icon Container` แล้ว ระบบจะใช้ **Sprite Image mode** ทันที และจะซ่อน text fallback ให้อัตโนมัติ

---

## 3) เตรียม Tooltip Panel

1. ใต้ Canvas สร้าง Panel ใหม่ชื่อ `DebuffTooltip`
2. ใส่ลูกเป็น `Text - TextMeshPro` ชื่อ `DebuffTooltipText`
3. ตั้งค่า `DebuffTooltip`:
   - ปิด active ไว้ตั้งแต่เริ่มเกม (`SetActive(false)`)
   - ใส่พื้นหลังอ่านง่าย
4. ตั้งค่า `DebuffTooltipText`:
   - เปิด Word Wrap
   - ปรับขนาดให้รองรับ 2 บรรทัดขึ้นไป

---

## 4) โหมด fallback: ถ้ายังไม่ใช้ Sprite ให้ใช้ TMP Text แบบเดิม

ถ้ายังไม่พร้อมทำ icon sprite สามารถใช้แบบเดิมได้:

1. ไปที่ Panel สถานะผู้เล่น (status panel)
2. สร้าง `Text - TextMeshPro` ใหม่ 1 ตัว
3. ตั้งชื่อแนะนำ: `DebuffIconText`
4. ตั้งค่าพื้นฐาน:
   - เปิด `Rich Text`
   - เปิด `Raycast Target` (สำคัญมาก)
   - จัดตำแหน่งให้อยู่ในแถวสถานะ
5. ลากไปผูกที่ช่อง `Debuff Text`

หมายเหตุ:
- ถ้าอยากผูกแบบ auto-find ให้ชื่อ object มีคำว่า `debuff`
- แต่แนะนำให้ลากผูกใน Inspector โดยตรงเพื่อชัวร์ที่สุด
- ถ้ามี `Debuff Icon Container` อยู่แล้ว ระบบจะใช้ sprite icon ก่อน และซ่อน text fallback อัตโนมัติ

---

## 5) ตรวจ PlayerState / Event ว่าต่อครบ

ระบบนี้ต้องใช้ flow ด้าน gameplay ครบตามนี้:

1. Ice ต้องเข้าผ่าน `ApplyIceDebuff()` (ไม่เซ็ต flag ตรง ๆ)
2. Burn ใช้ `ApplyBurnDebuff(turns)`
3. ตอน consume ต้องลด/ล้างสถานะตามเทิร์น

ในโค้ดล่าสุดของโปรเจกต์มีการต่อไว้แล้ว:
- Ice event เรียก `ApplyIceDebuff()`
- Burn event เรียก `ApplyBurnDebuff(3)`
- PlayerState เก็บลำดับการติด debuff เพื่อใช้เรียง icon

---

## 6) วิธีเทสใน Play Mode

1. เข้า Play Mode
2. ทำให้ผู้เล่นติด Burn ก่อน แล้วติด Ice ตาม
3. ถ้าใช้ Sprite mode:
   - เช็กว่า icon Burn กับ Ice โผล่ตาม sprite ที่ผูกไว้
   - เช็กว่าเรียงซ้าย → ขวาตามลำดับที่ติด
4. ถ้าใช้ Text fallback:
   - เช็กว่า icon เรียงเป็น `🔥  ❄️`
5. เอาเมาส์ชี้ที่ Burn icon ต้องเห็นข้อความ Burn + เทิร์นคงเหลือ
6. เอาเมาส์ชี้ที่ Ice icon ต้องเห็นข้อความ Ice + คงเหลือ 1 ครั้ง
7. รอ Burn หมด แล้วเช็กว่า Ice เลื่อนมาอยู่ซ้าย (แทนที่ตำแหน่งเดิม)

---

## 7) Troubleshooting (ปัญหาที่เจอบ่อย)

### ไม่ขึ้น tooltip ตอน hover
- เช็ก `EventSystem` มีอยู่จริง
- ถ้าใช้ Text fallback ให้เช็ก `DebuffIconText` เปิด `Raycast Target`
- ถ้าใช้ Sprite mode ให้เช็กว่า icon object มี `Image` และรับ raycast ได้
- เช็ก Canvas/GraphicRaycaster ยังทำงาน
- เช็กว่า `Debuff Tooltip Root` และ `Debuff Tooltip Text` ถูกลากผูก

### เห็นแต่เครื่องหมาย `-`
- หมายถึงตอนนั้นยังไม่มี debuff ที่ active
- ข้อความ `-` จะแสดงเฉพาะใน Text fallback mode
- ถ้าใช้ Sprite mode แล้วไม่มี debuff ระบบจะซ่อน icon ทั้งหมดแทน

### icon ไม่ขึ้นใน Sprite mode
- เช็กว่าได้ลาก `Debuff Icon Container` แล้ว
- เช็กว่าได้ลาก `Burn Debuff Sprite` / `Ice Debuff Sprite` แล้ว
- ถ้าใช้ prefab เอง ให้เช็กว่ามี component `Image`

### icon ไม่เรียงตามลำดับที่ติด
- ตรวจว่า Ice ถูก apply ผ่าน `ApplyIceDebuff()`
- ตรวจว่า Burn ถูก apply ผ่าน `ApplyBurnDebuff(turns)`
- ตรวจว่าไม่มีจุดอื่นไปเซ็ต `hasIceEffect = true` ตรง ๆ

---

## 8) Checklist ส่งงานให้ทีม

- [ ] มี EventSystem ในฉาก
- [ ] มี DebuffTooltip + DebuffTooltipText
- [ ] ถ้าใช้ Sprite mode: มี DebuffIconContainer
- [ ] ถ้าใช้ Sprite mode: ผูก Burn Debuff Sprite / Ice Debuff Sprite ครบ
- [ ] ถ้าใช้ Text fallback: มี DebuffIconText และเปิด Raycast Target
- [ ] ผูกช่องใน PlayerUIController ครบ
- [ ] เทส Burn/Ice hover ผ่านใน Play Mode

# คู่มือ Setup MainLight Heal Gimmick (ละเอียด)

อัปเดต: 2026-04-03

เอกสารนี้อธิบายการตั้งค่า **MainLight Heal Gimmick** ให้ทำงานครบ:
- เปลี่ยนบางช่องเป็น `TileType.Heal`
- เปลี่ยน visual/texture ของช่องตาม `RouteManager`
- นับเทิร์นเพื่อคืนค่าช่องเดิม

---

## 1) โครงสร้างที่ต้องมีใน Scene

ขั้นต่ำควรมี 3 อย่างนี้:
1. `RouteManager` (ถือข้อมูล nodeConnections + tileVisualSettings)
2. `MainLightHealGimmickController`
3. `MainLightHealGimmickTurnTicker`

> ถ้าอยากให้ระบบทำงานแบบ “ตามเทิร์นจริง” ต้องมี `GameTurnManager` ใน scene ด้วย

---

## 2) Setup `MainLightHealGimmickController`

ไฟล์สคริปต์: `Assets/0StarDice0/Scripts/ExtraBoardGimmick/MainLightHealGimmickController.cs`

### 2.1 ช่อง Reference
- `routeManager` → ลาก `RouteManager` ใน scene มาใส่
  - ไม่ใส่ก็ได้ แต่ระบบจะพยายามหาเองด้วย `RouteManager.TryGet(...)`

### 2.2 พารามิเตอร์หลัก
- `enableMainLightHealGimmick` = `true`
- `mainLightHealOnlyInMainLight` = `true` (จะทำงานเฉพาะ scene ชื่อ `MainLight`)
- `mainLightHealDurationTurns` = จำนวนเทิร์นที่ช่อง Heal คงอยู่
- `mainLightHealMinTiles` / `mainLightHealMaxTiles` = จำนวนช่องที่แปลงต่อครั้ง
- `enableAutoTriggerByTurn` = เปิดการสุ่ม trigger ตามเทิร์น
- `autoTriggerIntervalTurns` = ทุกกี่เทิร์นให้ trigger 1 ครั้ง
- `autoTriggerOnlyPlayerTurn` = ถ้าเปิด จะนับเฉพาะเทิร์นผู้เล่น

### 2.3 เงื่อนไขชื่อ Scene
ในโค้ดตรวจชื่อ scene ตรงตัว `MainLight`:
- ถ้า scene ที่เล่นไม่ได้ชื่อ `MainLight` และเปิด `mainLightHealOnlyInMainLight`
  → trigger จะไม่เกิด

---

## 3) Setup `MainLightHealGimmickTurnTicker`

ไฟล์สคริปต์: `Assets/0StarDice0/Scripts/ExtraBoardGimmick/MainLightHealGimmickTurnTicker.cs`

### 3.1 ช่อง Reference
- `healGimmickController` → ลาก controller มาใส่
  - ถ้าไม่ใส่ สคริปต์จะหาเองตอน `Awake`

### 3.2 พารามิเตอร์
- `enableTurnTick` = `true`
- `triggerOnceIfNoTurnManager` = `true` (fallback)
- `simulateTurnsIfNoTurnManager` = `true` (จำลองการนับเทิร์นเมื่อไม่มี GameTurnManager)
- `simulatedTurnIntervalSeconds` = คาบเวลา 1 เทิร์นจำลอง (เช่น 1.0 วินาที)
- `verboseLog` = เปิดเฉพาะตอน debug

### 3.3 การทำงาน
- ปกติ: subscribe `GameTurnManager.OnTurnChanged` แล้วส่งต่อไป `TickTurn(isAITurn)`
- ถ้าไม่เจอ `GameTurnManager`:
  - ถ้า `triggerOnceIfNoTurnManager = true` จะเรียก `TriggerGimmick()` 1 ครั้งตอนเริ่ม
  - ถ้า `simulateTurnsIfNoTurnManager = true` จะมี fallback loop เรียก `TickTurn(false)` เป็นระยะ ๆ
    ตามค่า `simulatedTurnIntervalSeconds` เพื่อให้ duration ลดและ restore ได้

---

## 4) Setup `RouteManager` สำหรับให้เปลี่ยน texture/visual ได้จริง

ไฟล์สคริปต์: `Assets/0StarDice0/Scripts/MainGame/_RouteManager/RouteManager.cs`

1. ใน `tileVisualSettings` ต้องมีรายการสำหรับ `TileType.Heal`
2. กำหนดอย่างน้อยหนึ่งอย่างในรายการนั้น:
   - `sprite` หรือ
   - `material` หรือ
   - `texture`

> ตอน gimmick trigger จะเรียก `routeManager.ApplyTileVisual(chosenNode)` โดยตรง
> ดังนั้นถ้า `TileType.Heal` ไม่มี visual setting จะเหมือน “ไม่เปลี่ยน texture”

---

## 5) สรุปคำถามสำคัญ: ต้องผูกกับ GameTurnManager ไหม?

### กรณี Production (แนะนำ)
**ควรผูกกับ `GameTurnManager` ที่มีอยู่แล้ว**
- จะได้พฤติกรรมสอดคล้องระบบเทิร์นจริง
- countdown duration และ trigger interval ทำงานตาม turn flow จริง

### กรณีไม่มี GameTurnManager ใน scene นั้น
- ใช้ fallback ได้ (`triggerOnceIfNoTurnManager = true`)
- แต่จะเป็นการ trigger ครั้งเดียวตอนเริ่ม scene ไม่ใช่ turn-driven เต็มรูปแบบ

ดังนั้นคำตอบสั้น ๆ คือ:
- **มี GameTurnManager อยู่แล้ว → ผูกเลย (ดีที่สุด)**
- fallback มีไว้กันระบบเงียบใน scene ที่ถูกตัด manager ออก

---

## 6) Checklist ตรวจเร็วเมื่อบอกว่า "ไม่ทำงาน"

1. Scene name คือ `MainLight` จริงไหม
2. `enableMainLightHealGimmick = true` ไหม
3. `RouteManager` ถูกอ้างอิงได้ไหม
4. มี `TileType.Heal` ใน `tileVisualSettings` ไหม
5. มี `GameTurnManager` ใน scene หรือยัง
6. ถ้าไม่มี `GameTurnManager` เปิด `triggerOnceIfNoTurnManager` ไหม
7. ถ้าไม่มี `GameTurnManager` เปิด `simulateTurnsIfNoTurnManager` ไหม
8. เปิด `verboseLog` แล้วเห็น log จาก ticker/controller หรือไม่

---

## 7) วิธีทดสอบแบบสั้น

1. เข้า scene MainLight
2. กด Play
3. ที่ component `MainLightHealGimmickController` ใช้ ContextMenu: `Trigger MainLight Heal Gimmick`
4. ดูว่ามีช่องที่ `type` เป็น Heal และ texture/visual เปลี่ยน
5. ถ้ามี GameTurnManager ให้เล่นต่อหลายเทิร์น ดูการคืนค่าเดิมเมื่อครบระยะเวลา

---

## 8) วิธีผูก `GameTurnManager` แบบทีละขั้น

1. ใน Scene ต้องมี GameObject ที่มีคอมโพเนนต์ `GameTurnManager`
2. ใน Scene เดียวกันต้องมี `MainLightHealGimmickTurnTicker`
3. ให้แน่ใจว่า `enableTurnTick = true` ที่ ticker
4. ให้แน่ใจว่า `healGimmickController` ชี้ไปที่ `MainLightHealGimmickController` (ไม่ใส่ก็ได้ ระบบหาเองตอน `Awake`)
5. ตอน Play:
   - ticker จะเรียก `GameTurnManager.TryGet(...)`
   - ถ้าเจอ จะ subscribe กับ `OnTurnChanged`
   - ทุกครั้งที่เปลี่ยนเทิร์นจะส่ง `isAITurn` เข้า `TickTurn(isAITurn)`

> สรุป: ปกติไม่ต้อง drag event เองใน Inspector (ไม่ใช่ UnityEvent ที่ต้องกดผูกมือ) เพราะ ticker subscribe ในโค้ดให้อัตโนมัติ

---

## 9) ตัวอย่าง flow จริง (ตั้งค่า: Trigger ทุก 2 เทิร์น, Duration 2 เทิร์น)

สมมติค่าใน `MainLightHealGimmickController`:
- `enableAutoTriggerByTurn = true`
- `autoTriggerIntervalTurns = 2`
- `mainLightHealDurationTurns = 2`
- `autoTriggerOnlyPlayerTurn = false`

### ลำดับการทำงาน

- เริ่มเกม:
  - `autoTriggerTurnsLeft = 2`
  - `mainLightHealTurnsLeft = 0` (ยังไม่มีชุด Heal ที่ active)

- เมื่อจบเทิร์นครั้งที่ 1 (`OnTurnChanged` ครั้งแรก):
  1. `TickActiveGimmickDuration()` → ยังไม่มี active ชุด Heal (ไม่ลดอะไร)
  2. ลด `autoTriggerTurnsLeft` จาก 2 → 1
  3. ยังไม่ trigger

- เมื่อจบเทิร์นครั้งที่ 2:
  1. `TickActiveGimmickDuration()` → ยังไม่มี active ชุด Heal
  2. ลด `autoTriggerTurnsLeft` จาก 1 → 0
  3. เรียก `TriggerGimmick()`:
     - สุ่มเลือกช่องที่เข้าเงื่อนไข
     - เก็บค่าเดิมไว้ (`originalType`, `originalEventName`)
     - เปลี่ยนเป็น `TileType.Heal`
     - ตั้ง `eventName` เป็นค่า default ของ Heal
     - เรียก `ApplyTileVisual(...)` เพื่อเปลี่ยนภาพทันที
  4. ตั้ง `mainLightHealTurnsLeft = 2`
  5. รีเซ็ต `autoTriggerTurnsLeft` กลับเป็น 2

- เมื่อจบเทิร์นครั้งที่ 3:
  1. `TickActiveGimmickDuration()` ลด `mainLightHealTurnsLeft` จาก 2 → 1 (ยังไม่คืนค่า)
  2. ลด `autoTriggerTurnsLeft` จาก 2 → 1

- เมื่อจบเทิร์นครั้งที่ 4:
  1. `TickActiveGimmickDuration()` ลด `mainLightHealTurnsLeft` จาก 1 → 0
  2. ถึง 0 แล้วเรียก `RestoreMainLightHealTiles()`:
     - คืน `type` เดิม
     - คืน `eventName` เดิม
     - เรียก `ApplyTileVisual(...)` เพื่อคืนภาพตามช่องเดิม
  3. ระบบจะ `ResetAutoTriggerCounter()` และ **ไม่ trigger ซ้ำในเทิร์นเดียวกับที่เพิ่ง restore**
     (แนว KISS เพื่อให้เห็นช่วงที่ช่องกลับเป็นปกติจริงก่อน)

**สรุปคำตอบ:** ช่อง “กลับคืนเป็นช่องก่อนหน้าได้” แน่นอน เพราะมีการเก็บค่าเดิมต่อช่องไว้ก่อนเปลี่ยน และ restore เมื่อครบ duration

---

## 10) จำเป็นต้องทำ `tileVisualSettings` ทุก TileType ไหม?

ไม่จำเป็นต้องครบทุกชนิดแบบบังคับ แต่มีผลดังนี้:

1. **ชนิดที่อยากให้เปลี่ยนภาพชัดเจน** (เช่น `Heal`) ควรมี setting แน่นอน
2. ถ้าชนิดไหนไม่มี setting:
   - ระบบ logic ยังทำงาน (type/event เปลี่ยนได้)
   - แต่ภาพ/texture อาจไม่เปลี่ยน จนเหมือน “ไม่ทำงาน”
3. แนะนำขั้นต่ำ:
   - ตั้ง `TileType.Heal` ให้ครบ (sprite/material/texture อย่างน้อย 1 อย่าง)
   - ตั้งชนิดหลักที่ใช้บ่อยในด่านนั้น (เช่น Normal, Event, Heal, Shop, Boss)

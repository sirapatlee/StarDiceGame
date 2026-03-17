using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NormaUIManager : MonoBehaviour
{
    [Header("Selection Panel (Popup)")]
    public GameObject selectionPanel;
    public TextMeshProUGUI titleText;
    public Button starBtn;
    public Button winBtn;
    public TextMeshProUGUI starBtnText;
    public TextMeshProUGUI winBtnText;

    [Header("Submit Quest Panel")]
    public GameObject submitPanel; // หน้าต่างถามว่า "ส่งเควสไหม?"
    public Button submitBtn;       // ปุ่ม "ส่งเควส (Submit)"

    [Header("Info Display (On Screen HUD) - ของเดิม")]
    public TextMeshProUGUI currentRankText;
    public TextMeshProUGUI currentGoalText;

    [Header("Detailed Progress HUD (ช่องใหม่ที่เพิ่งเพิ่ม)")]
    public TextMeshProUGUI rankProgressText; // สำหรับโชว์: Rank 1 / 6
    public TextMeshProUGUI starProgressText; // สำหรับโชว์: Stars 10 / 30
    public TextMeshProUGUI winProgressText;  // สำหรับโชว์: Wins 1 / 2

    // ตัวแปรสำหรับคอยเช็คว่าดาว/การชนะ เปลี่ยนแปลงไปหรือยัง (เอาไว้อัปเดต UI แบบ Real-time)
    private int lastStars = -1;
    private int lastWins = -1;

    private void Start()
    {
        // 1. ซ่อน Popup เลือก Norma ไว้ก่อน
        if (selectionPanel != null) selectionPanel.SetActive(false);


        if (submitPanel != null) submitPanel.SetActive(false);
        
        // 🟢 ผูกปุ่มกดส่งเควส
        if (submitBtn != null)
        {
            submitBtn.onClick.RemoveAllListeners();
            submitBtn.onClick.AddListener(OnSubmitClicked);
        }

        // 2. Setup ปุ่มเลือก
        if (starBtn != null)
        {
            starBtn.onClick.RemoveAllListeners();
            starBtn.onClick.AddListener(() => OnChoose(NormaType.Stars));
        }

        if (winBtn != null)
        {
            winBtn.onClick.RemoveAllListeners();
            winBtn.onClick.AddListener(() => OnChoose(NormaType.Wins));
        }



        // 3. ดึงข้อมูลมาโชว์ทันทีที่เกิด
        UpdateInfoUI();
    }

    private void Update()
    {
        // ระบบอัปเดตตัวเลขแบบ Real-time: คอยดูว่าดาวหรือจำนวนชนะของผู้เล่นเปลี่ยนไปไหม
        if (GameTurnManager.CurrentPlayer != null)
        {
            int currentStars = GameTurnManager.CurrentPlayer.PlayerStar;
            int currentWins = GameTurnManager.CurrentPlayer.WinCount;

            // ถ้ามีการได้ดาวเพิ่ม หรือชนะเพิ่ม ให้สั่งอัปเดตหน้าจอทันที!
            if (currentStars != lastStars || currentWins != lastWins)
            {
                lastStars = currentStars;
                lastWins = currentWins;
                UpdateInfoUI();
            }
        }
    }

  public void UpdateInfoUI()
    {
        if (!NormaSystem.TryGet(out var normaSystem)) return;

        // 1. ดึงข้อมูลปัจจุบันของผู้เล่น
        int currentStars = 0;
        int currentWins = 0;
        if (GameTurnManager.CurrentPlayer != null)
        {
            currentStars = GameTurnManager.CurrentPlayer.PlayerStar;
            currentWins = GameTurnManager.CurrentPlayer.WinCount;
        }

        // 2. คำนวณเลเวลถัดไป เพื่อเอาไปดึงเป้าหมายจากระบบ (ถ้าเลเวลตันแล้ว ก็ให้ดึงเป้าหมายสูงสุด)
        int nextRank = normaSystem.currentNormaRank + 1;
        if (nextRank > normaSystem.maxNormaRank) 
        {
            nextRank = normaSystem.maxNormaRank;
        }

        // 🟢 ดึงเป้าหมายของ "ดาว" และ "ต่อสู้" ของเลเวลนี้ออกมาเตรียมไว้เลย
        int reqStars = normaSystem.GetRequirement(nextRank, NormaType.Stars);
        int reqWins = normaSystem.GetRequirement(nextRank, NormaType.Wins);

        // 3. อัปเดต UI ให้มีเครื่องหมาย / เสมอ!
        
        // --- ส่วน Rank ---
        if (rankProgressText != null)
            rankProgressText.text = $"Goal: {normaSystem.currentNormaRank} / {normaSystem.maxNormaRank}";
        
        // --- ส่วน ดาว (มี / เสมอ) ---
        if (starProgressText != null)
            starProgressText.text = $"Stars: {currentStars} / {reqStars}";

        // --- ส่วน ชนะต่อสู้ (มี / เสมอ) ---
        if (winProgressText != null)
            winProgressText.text = $"Wins: {currentWins} / {reqWins}";

        // 4. (ส่วนเสริม) เน้นย้ำให้ผู้เล่นรู้ว่า "ตอนนี้กำลังทำเควสอะไรอยู่"
        if (currentGoalText != null)
        {
            if (normaSystem.selectedNorma == NormaType.Stars)
                currentGoalText.text = $"Active Quest: Stars";
            else
                currentGoalText.text = $"Active Quest: Wins";
        }
    }

    public void ShowSelectionPanel(int nextLevel)
    {
        if (selectionPanel == null) return;
        selectionPanel.SetActive(true);
        if (titleText != null) titleText.text = $"Select your quest!";

        if (NormaSystem.TryGet(out var normaSystem))
        {
            string starReqText = normaSystem.GetRequirementText(nextLevel, NormaType.Stars);
            string winReqText = normaSystem.GetRequirementText(nextLevel, NormaType.Wins);
            if (starBtnText != null) starBtnText.text = $"Collect {starReqText} Stars";
            if (winBtnText != null) winBtnText.text = $"Win {winReqText} Battles";
        }
    }

    private void OnChoose(NormaType type)
    {
        if (NormaSystem.TryGet(out var normaSystem)) normaSystem.SelectNorma(type);
        if (selectionPanel != null) selectionPanel.SetActive(false);

        // สั่งอัปเดตหน้าจอทันทีเมื่อเลือกเควสเสร็จ
        UpdateInfoUI(); 
    }

    // สั่งให้เปิดหน้าต่างนี้ขึ้นมา
    public void ShowSubmitPanel()
    {
        if (submitPanel != null) submitPanel.SetActive(true);
    }

    // เมื่อผู้เล่นกดปุ่ม "ส่งเควส"
    private void OnSubmitClicked()
    {
        // 1. ปิดหน้าต่างนี้ทิ้งไป
        if (submitPanel != null) submitPanel.SetActive(false);

        // 2. สั่งอัปเลเวลของจริง! (มันจะไปเด้งหน้าต่างเลือกเควสถัดไปต่อให้อัตโนมัติ)
        if (NormaSystem.TryGet(out var normaSystem))
        {
            normaSystem.NormaLevelUp(); 
        }
    }
}
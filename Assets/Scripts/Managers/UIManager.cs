using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("좌측 UI")]  
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI levelLabelText;
    public Image expBarFill;
    public TextMeshProUGUI expFractionText;
    public TextMeshProUGUI descriptionText;

    [Header("상단 UI")]  
    public TextMeshProUGUI gachaTicketText;
    public TextMeshProUGUI goldText;

    [Header("경험치 바")]  
    public Image gachaExpBarFill;           // 가챠 경험치 바 Fill
    public TextMeshProUGUI gachaExpText;    // "현재Exp/필요Exp" 텍스트

    [Header("메인 창")]
    public GameObject inventoryWindow;
    public GameObject statsWindow;

    [Header("메인 메뉴 버튼")]
    public Button inventoryButton;
    public Button statsButton;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 초기 상태: 두 창 모두 숨김
        if (inventoryWindow != null) inventoryWindow.SetActive(false);
        if (statsWindow      != null) statsWindow.SetActive(false);

        // 버튼 클릭 리스너 연결
        if (inventoryButton != null)
            inventoryButton.onClick.AddListener(ToggleInventoryWindow);
        if (statsButton != null)
            statsButton.onClick.AddListener(ToggleStatsWindow);

        // UI 초기 갱신
        UpdateTopBarUI();
        UpdateLeftPanelUI();
    }

    /// <summary>
    /// 상단바의 골드, 뽑기 티켓 수 갱신
    /// </summary>
    public void UpdateTopBarUI()
    {
        var gm = GameManager.Instance;
        if (gachaTicketText != null)
            gachaTicketText.text = gm.gachaTickets.ToString();
        if (goldText != null)
            goldText.text = gm.gold.ToString("N0");
    }

    /// <summary>
    /// 좌측 패널의 플레이어 정보(이름/레벨/경험치/설명) 갱신
    /// </summary>
    public void UpdateLeftPanelUI()
    {
        var gm = GameManager.Instance;
        if (playerNameText != null)
            playerNameText.text = gm.playerName;
        if (levelLabelText != null)
            levelLabelText.text = $"Lv {gm.playerLevel}";
        if (expFractionText != null)
            expFractionText.text = $"{gm.currentExp}/{gm.expToNextLevel}";
        if (expBarFill != null)
            expBarFill.fillAmount = gm.expToNextLevel > 0 ? (float)gm.currentExp / gm.expToNextLevel : 0f;
        if (descriptionText != null)
            descriptionText.text = gm.playerDescription;
    }

    /// <summary>
    /// 가챠 경험치 UI 갱신
    /// </summary>
    public void UpdateGachaExpUI(long currentExp, long neededExp)
    {
        if (gachaExpBarFill != null)
            gachaExpBarFill.fillAmount = neededExp > 0 ? (float)currentExp / neededExp : 0f;
        if (gachaExpText != null)
            gachaExpText.text = $"{currentExp}/{neededExp}";
    }

    /// <summary>
    /// 인벤토리 창 켜기/끄기
    /// </summary>
    public void ToggleInventoryWindow()
    {
        if (inventoryWindow == null) return;
        bool show = !inventoryWindow.activeSelf;
        inventoryWindow.SetActive(show);
        if (show)
        {
            InventoryWindowController.Instance.RefreshInventoryUI();
            statsWindow?.SetActive(false);
        }
    }

    /// <summary>
    /// 스탯 창 켜기/끄기
    /// </summary>
    public void ToggleStatsWindow()
    {
        if (statsWindow == null) return;
        bool show = !statsWindow.activeSelf;
        statsWindow.SetActive(show);
        if (show)
        {
            StatsWindowController.Instance.RefreshStats();
            inventoryWindow?.SetActive(false);
        }
    }
}
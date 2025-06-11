using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// StatsWindowController: 씬에 배치된 StatRow를 직접 참조해 스탯 UI를 업데이트하고,
/// Strengthen 버튼 클릭 시 GameManager로 강화 요청을 합니다.
/// </summary>
public class StatsWindowController : MonoBehaviour
{
    public static StatsWindowController Instance { get; private set; }

    [Header("닫기 버튼")]
    public Button closeButton;
    
    [Header("공격력 강화 관련")]  
    public TextMeshProUGUI baseClickTitleText;
    public TextMeshProUGUI baseClickLevelText;
    public TextMeshProUGUI baseClickCurrentText;
    public TextMeshProUGUI baseClickNextText;
    public Button          baseClickUpgradeButton;
    public TextMeshProUGUI baseClickUpgradeButtonText;
    public TextMeshProUGUI baseClickCostText;

    [Header("자동공격 강화 관련")]  
    public TextMeshProUGUI autoClickTitleText;
    public TextMeshProUGUI autoClickLevelText;
    public TextMeshProUGUI autoClickCurrentText;
    public TextMeshProUGUI autoClickNextText;
    public Button          autoClickUpgradeButton;
    public TextMeshProUGUI autoClickUpgradeButtonText;
    public TextMeshProUGUI autoClickCostText;

    [Header("치명타 확률 관련")]  
    public TextMeshProUGUI critRateTitleText;
    public TextMeshProUGUI critRateLevelText;
    public TextMeshProUGUI critRateCurrentText;
    public TextMeshProUGUI critRateNextText;
    public Button          critRateUpgradeButton;
    public TextMeshProUGUI critRateUpgradeButtonText;
    public TextMeshProUGUI critRateCostText;

    [Header("치명타 데미지 관련")]  
    public TextMeshProUGUI critDamageTitleText;
    public TextMeshProUGUI critDamageLevelText;
    public TextMeshProUGUI critDamageCurrentText;
    public TextMeshProUGUI critDamageNextText;
    public Button          critDamageUpgradeButton;
    public TextMeshProUGUI critDamageUpgradeButtonText;
    public TextMeshProUGUI critDamageCostText;

    private void Awake()
    {
        // 싱글톤 인스턴스 설정
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        if (closeButton != null)
            closeButton.onClick.AddListener(() => gameObject.SetActive(false));

        // 버튼 리스너 등록
        baseClickUpgradeButton.onClick.AddListener(OnUpgradeBaseClick);
        autoClickUpgradeButton.onClick.AddListener(OnUpgradeAutoClick);
        critRateUpgradeButton.onClick.AddListener(OnUpgradeCritRate);
        critDamageUpgradeButton.onClick.AddListener(OnUpgradeCritDamage);
    }
    
    private void Update()
    {
        if (gameObject.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        // 창이 활성화될 때마다 최신 스탯 반영
        RefreshStats();
    }

    /// <summary>
    /// 화면에 보일 네 가지 스탯을 모두 업데이트
    /// </summary>
    public void RefreshStats()
    {
        RefreshBaseClickStat();
        RefreshAutoClickStat();
        RefreshCritRateStat();
        RefreshCritDamageStat();
    }

    private void RefreshBaseClickStat()
    {
        int   level   = GameManager.Instance.attackLevel;
        long  current = GameManager.Instance.attackPower;
        long  next    = GameManager.Instance.GetNextAttackPower();
        long  cost    = GameManager.Instance.GetAttackUpgradeCost();

        baseClickTitleText.text        = "공격력";
        baseClickLevelText.text        = $"Lv. {level}";
        baseClickCurrentText.text      = current.ToString("N0");
        baseClickNextText.text         = $"→ {next:N0}";
        baseClickUpgradeButtonText.text = "능력치 강화";
        baseClickCostText.text  = $"{cost:N0}G";
        baseClickCostText.color = GameManager.Instance.gold >= cost
            ? Color.white
            : Color.red;
        baseClickUpgradeButton.interactable = (GameManager.Instance.gold >= cost);
    }

    public void OnUpgradeBaseClick()
    {
        long cost = GameManager.Instance.GetAttackUpgradeCost();
        if (GameManager.Instance.SpendGold(cost))
        {
            GameManager.Instance.UpgradeAttack();
            UIManager.Instance.UpdateTopBarUI();
            RefreshBaseClickStat();
        }
    }

    private void RefreshAutoClickStat()
    {
        int   level   = GameManager.Instance.autoClickLevel;
        long  current = GameManager.Instance.autoClickPower;
        long  next    = GameManager.Instance.GetNextAutoClickPower();
        long  cost    = GameManager.Instance.GetAutoClickUpgradeCost();

        autoClickTitleText.text        = "자동공격";
        autoClickLevelText.text        = $"Lv. {level}";
        autoClickCurrentText.text      = current.ToString("N0");
        autoClickNextText.text         = $"→ {next:N0}";
        autoClickUpgradeButtonText.text = "능력치 강화";
        autoClickCostText.text  = $"{cost:N0}G";
        autoClickCostText.color = GameManager.Instance.gold >= cost
            ? Color.white
            : Color.red;
        autoClickUpgradeButton.interactable = (GameManager.Instance.gold >= cost);
    }

    public void OnUpgradeAutoClick()
    {
        long cost = GameManager.Instance.GetAutoClickUpgradeCost();
        if (GameManager.Instance.SpendGold(cost))
        {
            GameManager.Instance.UpgradeAutoClick();
            UIManager.Instance.UpdateTopBarUI();
            RefreshAutoClickStat();
        }
    }

    private void RefreshCritRateStat()
    {
        int   level   = GameManager.Instance.critRateLevel;
        long  current = Mathf.RoundToInt(GameManager.Instance.critRate * 100f);
        long  next    = Mathf.RoundToInt(GameManager.Instance.GetNextCritRate() * 100f);
        long  cost    = GameManager.Instance.GetCritRateUpgradeCost();

        critRateTitleText.text        = "치명타 확률";
        critRateLevelText.text        = $"Lv. {level}";
        critRateCurrentText.text      = $"{current}%";
        critRateNextText.text         = $"→ {next}%";
        critRateUpgradeButtonText.text = "능력치 강화";
        critRateCostText.text  = $"{cost:N0}G";
        critRateCostText.color = GameManager.Instance.gold >= cost
            ? Color.white
            : Color.red;
        critRateUpgradeButton.interactable = (GameManager.Instance.gold >= cost);
    }

    public void OnUpgradeCritRate()
    {
        long cost = GameManager.Instance.GetCritRateUpgradeCost();
        if (GameManager.Instance.SpendGold(cost))
        {
            GameManager.Instance.UpgradeCritRate();
            UIManager.Instance.UpdateTopBarUI();
            RefreshCritRateStat();
        }
    }

    private void RefreshCritDamageStat()
    {
        int   level   = GameManager.Instance.critDamageLevel;
        long  current = Mathf.RoundToInt(GameManager.Instance.critDamage * 100f);
        long  next    = Mathf.RoundToInt(GameManager.Instance.GetNextCritDamage() * 100f);
        long  cost    = GameManager.Instance.GetCritDamageUpgradeCost();

        critDamageTitleText.text        = "치명타 데미지";
        critDamageLevelText.text        = $"Lv. {level}";
        critDamageCurrentText.text      = $"{current}%";
        critDamageNextText.text         = $"→ {next}%";
        critDamageUpgradeButtonText.text = "능력치 강화";
        critDamageCostText.text  = $"{cost:N0}G";
        critDamageCostText.color = GameManager.Instance.gold >= cost
            ? Color.white
            : Color.red;
        critDamageUpgradeButton.interactable = (GameManager.Instance.gold >= cost);
    }

    public void OnUpgradeCritDamage()
    {
        long cost = GameManager.Instance.GetCritDamageUpgradeCost();
        if (GameManager.Instance.SpendGold(cost))
        {
            GameManager.Instance.UpgradeCritDamage();
            UIManager.Instance.UpdateTopBarUI();
            RefreshCritDamageStat();
        }
    }
}
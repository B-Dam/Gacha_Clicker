using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("플레이어 정보")]
    public string playerName = "Player";
    public int playerLevel = 1;
    public long currentExp = 0;
    public long expToNextLevel = 100;
    [TextArea] public string playerDescription = "간단한 설명 혹은 스토리가 들어갈 자리입니다.";

    [Header("현재 상태")]
    public long gold = 0;
    public int gachaTickets = 0;
    public int gachaLevel = 1;

    [Header("스탯")]
    public int attackLevel = 1;
    public long attackPower = 1;
    public int autoClickLevel = 1;
    public long autoClickPower = 0;
    public int critRateLevel = 1;
    [Range(0f,1f)] public float critRate = 0.05f;
    public int critDamageLevel = 1;
    public float critDamage = 1.5f;

    // 통계용
    public long totalClicks = 0;
    public long totalGoldEarned = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    // 경험치, 레벨 업
    public void AddExp(long amount)
    {
        currentExp += amount;
        if (currentExp >= expToNextLevel)
        {
            currentExp -= expToNextLevel;
            playerLevel++;
            expToNextLevel = Mathf.RoundToInt(expToNextLevel * 1.2f);
        }
        UIManager.Instance.UpdateLeftPanelUI();
    }

    // 골드 관련
    public void AddGold(long amount)
    {
        gold += amount;
        totalGoldEarned += amount;
        UIManager.Instance.UpdateTopBarUI();
    }
    public bool SpendGold(long amount)
    {
        if (gold < amount) return false;
        gold -= amount;
        UIManager.Instance.UpdateTopBarUI();
        return true;
    }

    // 가챠 티켓 관련
    public void AddGachaTicket(int amount)
    {
        gachaTickets += amount;
        UIManager.Instance.UpdateTopBarUI();
    }
    public bool SpendGachaTicket(int amount)
    {
        if (gachaTickets < amount) return false;
        gachaTickets -= amount;
        UIManager.Instance.UpdateTopBarUI();
        return true;
    }

    // 스탯 업그레이드
    public long GetNextAttackPower()      => attackPower + attackLevel;
    public long GetAttackUpgradeCost()    => 100 * attackLevel;
    public void UpgradeAttack()
    {
        if (!SpendGold(GetAttackUpgradeCost())) return;
        attackLevel++;
        attackPower = GetNextAttackPower();
    }

    public long GetNextAutoClickPower()  => autoClickPower + autoClickLevel;
    public long GetAutoClickUpgradeCost() => 200 * autoClickLevel;
    public void UpgradeAutoClick()
    {
        if (!SpendGold(GetAutoClickUpgradeCost())) return;
        autoClickLevel++;
        autoClickPower = GetNextAutoClickPower();
    }

    public float GetNextCritRate()        => critRate + 0.01f * critRateLevel;
    public long GetCritRateUpgradeCost()  => 300 * critRateLevel;
    public void UpgradeCritRate()
    {
        if (!SpendGold(GetCritRateUpgradeCost())) return;
        critRateLevel++;
        critRate = GetNextCritRate();
    }

    public float GetNextCritDamage()      => critDamage + 0.1f * critDamageLevel;
    public long GetCritDamageUpgradeCost()=> 400 * critDamageLevel;
    public void UpgradeCritDamage()
    {
        if (!SpendGold(GetCritDamageUpgradeCost())) return;
        critDamageLevel++;
        critDamage = GetNextCritDamage();
    }
}
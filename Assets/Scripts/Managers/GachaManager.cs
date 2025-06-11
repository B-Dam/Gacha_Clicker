using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GachaManager : MonoBehaviour
{
    public static GachaManager Instance { get; private set; }
    private List<ItemData> allItems;
    private long gachaExp = 0;
    private readonly long[] expThresholds = new long[] { 0,100,300,600,1000,1500,2100,2800,3600,4500,long.MaxValue };
    private Dictionary<int, Dictionary<ItemRarity, float>> probabilityTable;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
        allItems = Resources.LoadAll<ItemData>("ScriptableObjects").ToList();
        InitializeProbabilityTable();
    }

    private void InitializeProbabilityTable()
    {
        probabilityTable = new Dictionary<int, Dictionary<ItemRarity, float>>();
        for (int lvl = 1; lvl <= 10; lvl++)
        {
            float common = Mathf.Lerp(0.80f, 0.40f, (lvl-1)/9f);
            float rare   = Mathf.Lerp(0.15f, 0.37f, (lvl-1)/9f);
            float epic   = Mathf.Lerp(0.04f, 0.18f, (lvl-1)/9f);
            float leg    = Mathf.Lerp(0.01f, 0.05f, (lvl-1)/9f);
            probabilityTable[lvl] = new Dictionary<ItemRarity, float> {
                { ItemRarity.Common, common },
                { ItemRarity.Rare,   rare   },
                { ItemRarity.Epic,   epic   },
                { ItemRarity.Legendary, leg }
            };
        }
    }

    /// <summary>
    /// 현재 경험치 기반으로 가챠 레벨 계산 (1~10)
    /// </summary>
    public int GetCurrentGachaLevel()
    {
        for (int i = 1; i < expThresholds.Length; i++)
            if (gachaExp < expThresholds[i]) return i;
        return 10;
    }

    /// <summary>
    /// UI에서 가중치 테이블 가져오기용
    /// </summary>
    public Dictionary<ItemRarity,float> GetProbabilityTable(int level)
    {
        if (probabilityTable.ContainsKey(level)) return probabilityTable[level];
        return probabilityTable[1];
    }

    /// <summary>
    /// 가챠 1회 뽑기
    /// </summary>
    public ItemData TrySinglePull()
    {
        int level = GetCurrentGachaLevel();
        int ticketCost = level * 5;
        if (!GameManager.Instance.SpendGachaTicket(ticketCost)) return null;
        gachaExp += 100;
        return PullByLevel(level);
    }

    /// <summary>
    /// 가챠 10회 뽑기
    /// </summary>
    public List<ItemData> TryTenPull()
    {
        List<ItemData> results = new List<ItemData>();
        int level = GetCurrentGachaLevel();
        int ticketCost = level * 5 * 10;
        if (!GameManager.Instance.SpendGachaTicket(ticketCost)) return results;
        for (int i = 0; i < 10; i++)
        {
            gachaExp += 100;
            var item = PullByLevel(level);
            if (item != null)
                results.Add(item);
        }
        return results;
    }

    private ItemData PullByLevel(int level)
    {
        var weights = probabilityTable[level];
        float total = weights.Values.Sum();
        float roll = Random.Range(0f, total);
        float acc = 0f;
        foreach (var kv in weights)
        {
            acc += kv.Value;
            if (roll <= acc)
            {
                var list = allItems.Where(i => i.rarity == kv.Key).ToList();
                if (list.Count == 0) return null;
                return list[Random.Range(0, list.Count)];
            }
        }
        return null;
    }
}
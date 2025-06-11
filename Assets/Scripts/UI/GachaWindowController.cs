using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// GachaWindowController: 가챠 창을 관리합니다.
/// - 가챠 메인 이미지
/// - 1회/10회 뽑기 버튼과 비용 표시(부족 시 빨간색)
/// - 현재 가챠 레벨 및 확률 표시
/// - 설명문
/// - 뽑은 아이템 리스트 및 상세 툴팁
/// </summary>
public class GachaWindowController : MonoBehaviour
{
    
    [Header("닫기 버튼")]
    public Button closeButton;
    
    [Header("메인 이미지")]   
    public Image gachaMainImage;

    [Header("가챠 버튼")]
    public Button singlePullButton;
    public Button tenPullButton;
    public TextMeshProUGUI singleCostText;
    public TextMeshProUGUI tenCostText;

    [Header("가챠 레벨, 확률 텍스트")]  
    public TextMeshProUGUI gachaLevelText;
    public TextMeshProUGUI probabilityText;

    [Header("설명문")]  
    public TextMeshProUGUI descriptionText;

    [Header("가챠 결과창")]
    public Transform resultContainer;        // 뽑기 결과 슬롯 부모
    public GameObject resultSlotPrefab;      // 결과 슬롯 프리팹 (IconImage 포함)
    public GameObject detailTooltipPanel;    // 상세 정보 패널

    private void Awake()
    {
        // 버튼 리스너 연결
        singlePullButton.onClick.AddListener(OnSinglePull);
        tenPullButton.onClick.AddListener(OnTenPull);
        
        if (closeButton != null)
            closeButton.onClick.AddListener(() => gameObject.SetActive(false));
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
        // 창 열 때마다 UI 갱신
        RefreshWindow();
    }

    /// <summary>
    /// 가챠 창의 모든 UI 요소를 업데이트
    /// </summary>
    public void RefreshWindow()
    {
        // 1) 비용 표시 & 색상
        int lvl = GameManager.Instance.gachaLevel;
        int cost1 = 5;
        int cost10 = cost1 * 10;
        singleCostText.text = cost1.ToString();
        tenCostText.text    = cost10.ToString();
        singleCostText.color = GameManager.Instance.gachaTickets < cost1 ? Color.red : Color.black;
        tenCostText.color    = GameManager.Instance.gachaTickets < cost10 ? Color.red : Color.black;

        // 2) 레벨 & 확률 요약
        gachaLevelText.text = $"Gacha Lv. {lvl}";
        probabilityText.text = BuildProbabilityText(lvl);

        // 3) 설명문
        descriptionText.text = "뽑을수록 희귀도가 상승합니다!";

        // 4) 결과 초기화
        ClearResults();
    }

    private string BuildProbabilityText(int lvl)
    {
        // GachaManager 내 확률 테이블 참조
        var table = GachaManager.Instance.GetProbabilityTable(lvl);
        return $"Common: {table[ItemRarity.Common]*100f:0.##}%  " +
               $"Rare: {table[ItemRarity.Rare]*100f:0.##}%  " +
               $"Epic: {table[ItemRarity.Epic]*100f:0.##}%  " +
               $"Legendary: {table[ItemRarity.Legendary]*100f:0.##}%";
    }

    private void OnSinglePull()
    {
        // 1회 뽑기
        var item = GachaManager.Instance.TrySinglePull();
        if (item != null)
        {
            // 인벤토리에 추가
            InventoryManager.Instance.AddItem(item);
            // 화면에도 표시
            AddResult(item);
        }
        
        if (InventoryWindowController.Instance != null && 
            InventoryWindowController.Instance.gameObject.activeSelf)
        {
            InventoryWindowController.Instance.RefreshInventoryUI();
        }
    }

    private void OnTenPull()
    {
        // 10회 뽑기
        var items = GachaManager.Instance.TryTenPull();
        if (items == null) return;

        foreach (var item in items)
        {
            if (item == null) continue;
            InventoryManager.Instance.AddItem(item);  // 인벤토리에 추가
            AddResult(item);  // UI 표시
        }
           
        if (InventoryWindowController.Instance != null && 
            InventoryWindowController.Instance.gameObject.activeSelf)
        {
            InventoryWindowController.Instance.RefreshInventoryUI();
        }
    }

    /// <summary>
    /// 뽑힌 아이템을 결과 컨테이너에 슬롯으로 추가하고,
    /// 마우스 오버 시 툴팁을 마우스 위치에 표시합니다.
    /// </summary>
    private void AddResult(ItemData item)
    {
        if (resultSlotPrefab == null || resultContainer == null)
        {
            Debug.LogError("resultSlotPrefab 또는 resultContainer가 할당되지 않았습니다!");
            return;
        }
        
        GameObject go = Instantiate(resultSlotPrefab, resultContainer);
        
        var img = go.GetComponent<Image>();
        img.sprite = item.icon;

        // EventTrigger 세팅
        var trigger = go.AddComponent<EventTrigger>();

        // Pointer Enter > 툴팁 보이기
        var entryEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        entryEnter.callback.AddListener((data) =>
        {
            DetailTooltip tooltip = detailTooltipPanel.GetComponent<DetailTooltip>();
            tooltip.Show(item);
        });
        trigger.triggers.Add(entryEnter);

        // Pointer Exit > 툴팁 숨기기
        var entryExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        entryExit.callback.AddListener((data) =>
        {
            DetailTooltip tooltip = detailTooltipPanel.GetComponent<DetailTooltip>();
            tooltip.Hide();
        });
        trigger.triggers.Add(entryExit);
    }

    private void ClearResults()
    {
        foreach (Transform child in resultContainer)
            Destroy(child.gameObject);
        detailTooltipPanel.GetComponent<DetailTooltip>().Hide();
    }

    private void AddTooltipEvent(EventTrigger trigger, EventTriggerType type, UnityEngine.Events.UnityAction action)
    {
        var entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener((_) => action());
        trigger.triggers.Add(entry);
    }
}
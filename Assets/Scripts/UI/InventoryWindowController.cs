using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryWindowController : MonoBehaviour
{
    public static InventoryWindowController Instance { get; private set; }

    [Header("닫기")] public Button closeButton;

    [Header("슬롯 & 그리드")] public GameObject slotPrefab; // IconImage, BorderImage, EquipOverlay
    public Transform slotsContainer;
    public int inventorySize = 20;

    [Header("정렬 & 필터")] public Button sortByRarityBtn;
    public Button sortByPriceBtn;
    public TMP_Dropdown filterDropdown; // 0=All,1=Weapon,2=Accessory

    [Header("상세 패널 UI")] public GameObject detailsPanel;
    public Image detailIcon;
    public TextMeshProUGUI detailName;
    public TextMeshProUGUI detailRarity;
    public TextMeshProUGUI detailStats;
    public Button equipButton;
    public Button sellButton;

    private enum SortType
    {
        None,
        Rarity,
        Price
    }

    private SortType currentSort = SortType.None;
    private bool rarityAsc = false, priceAsc = false;

    List<GameObject> slotObjects = new List<GameObject>();

    // 클릭해서 상세패널 띄운 슬롯의 displayIndex
    private int selectedDisplayIndex = -1;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        closeButton?.onClick.AddListener(() => gameObject.SetActive(false));
    }

    void Start()
    {
        // 빈 슬롯 20개 생성
        for (int i = 0; i < inventorySize; i++)
            slotObjects.Add(Instantiate(slotPrefab, slotsContainer));

        sortByRarityBtn.onClick.AddListener(OnSortByRarity);
        sortByPriceBtn.onClick.AddListener(OnSortByPrice);
        filterDropdown.onValueChanged.AddListener(_ => RefreshInventoryUI());

        equipButton.onClick.AddListener(OnEquipButton);
        sellButton.onClick.AddListener(OnSellButton);

        detailsPanel?.SetActive(false);
        RefreshInventoryUI();
    }

    private void Update()
    {
        if (gameObject.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            gameObject.SetActive(false);
        }
    }

    void OnEnable() => RefreshInventoryUI();

    void OnSortByRarity()
    {
        if (currentSort == SortType.Rarity) rarityAsc = !rarityAsc;
        else
        {
            currentSort = SortType.Rarity;
            rarityAsc = false;
        }

        sortByRarityBtn.GetComponentInChildren<TMP_Text>().text = $"희귀도 {(rarityAsc ? "↑" : "↓")}";
        sortByPriceBtn.GetComponentInChildren<TMP_Text>().text = "가격";
        RefreshInventoryUI();
    }

    void OnSortByPrice()
    {
        if (currentSort == SortType.Price) priceAsc = !priceAsc;
        else
        {
            currentSort = SortType.Price;
            priceAsc = false;
        }

        sortByPriceBtn.GetComponentInChildren<TMP_Text>().text = $"가격 {(priceAsc ? "↑" : "↓")}";
        sortByRarityBtn.GetComponentInChildren<TMP_Text>().text = "희귀도";
        RefreshInventoryUI();
    }

    public void RefreshInventoryUI()
    {
        // 1) 장착 슬롯과 인벤토리 리스트 분리
        var wSlot = InventoryManager.Instance.weaponSlot;
        var aSlot = InventoryManager.Instance.accessorySlot;
        var inv = InventoryManager.Instance.slots.Take(inventorySize).ToList();

        // 2) display 리스트 구성: [무기장착, 악세장착] + inv.Skip(2) → 필터 → 정렬 → 빈 슬롯 채우기
        var display = new List<InventorySlot> { wSlot, aSlot };
        var rest = inv.Skip(2).ToList();

        // 타입 필터
        if (filterDropdown.value == 1)
            rest = rest.Where(s => s.itemData?.itemType == ItemType.Weapon).ToList();
        else if (filterDropdown.value == 2)
            rest = rest.Where(s => s.itemData?.itemType == ItemType.Accessory).ToList();

        // 아이템만 정렬, 빈 슬롯은 뒤로
        var itemsOnly = rest.Where(s => s.itemData != null).ToList();
        var emptyOnly = rest.Where(s => s.itemData == null).ToList();

        if (currentSort == SortType.Rarity)
            itemsOnly = (rarityAsc
                    ? itemsOnly.OrderBy(s => (int)s.itemData.rarity)
                    : itemsOnly.OrderByDescending(s => (int)s.itemData.rarity)
                ).ToList();
        else if (currentSort == SortType.Price)
            itemsOnly = (priceAsc
                    ? itemsOnly.OrderBy(s => s.itemData.sellPrice)
                    : itemsOnly.OrderByDescending(s => s.itemData.sellPrice)
                ).ToList();

        rest = itemsOnly.Concat(emptyOnly).ToList();

        // 빈 슬롯 채우기
        while (display.Count + rest.Count < inventorySize)
            rest.Add(new InventorySlot { itemData = null });

        display.AddRange(rest);

        // 3) 슬롯 오브젝트에 매핑
        for (int i = 0; i < slotObjects.Count; i++)
        {
            var go = slotObjects[i];
            var icon = go.transform.Find("IconImage").GetComponent<Image>();
            var border = go.transform.Find("BorderImage").GetComponent<Image>();
            var overlay = go.transform.Find("EquipOverlay").gameObject;
            var btn = go.GetComponent<Button>();
            var drag = go.GetComponent<SlotDragHandler>();

            var slot = display[i];
            int realIndex = -1;
            if (i >= 2 && slot.itemData != null)
                realIndex = inv.IndexOf(slot);

            // 0·1번 칸은 drag disabled, 2번 이후만 drag 가능
            if (drag != null)
                drag.enabled = (i >= 2);

            if (slot.itemData != null)
            {
                icon.sprite = slot.itemData.icon;
                icon.enabled = true;
                border.color = GetColorByRarity(slot.itemData.rarity);

                // EquipOverlay: 장착 슬롯(0,1)만, 실제 장착된 경우에만 표시
                bool isEquipped = (i == 0 && wSlot.itemData != null)
                                  || (i == 1 && aSlot.itemData != null);
                overlay.SetActive(isEquipped);

                btn.interactable = (i >= 2);
                btn.onClick.RemoveAllListeners();
                if (i >= 2)
                    btn.onClick.AddListener(() => OnSlotClicked(realIndex));
            }
            else
            {
                // 빈 슬롯
                icon.enabled = false;
                border.color = Color.gray;
                overlay.SetActive(false);
                btn.interactable = false;
                btn.onClick.RemoveAllListeners();
            }
        }

        // 4) 상세 패널 닫기
        detailsPanel?.SetActive(false);
    }

    private void OnSlotClicked(int displayIndex)
    {
        var wSlot = InventoryManager.Instance.weaponSlot;
        var aSlot = InventoryManager.Instance.accessorySlot;
        var inv = InventoryManager.Instance.slots.Take(inventorySize).ToList();
        var display = new List<InventorySlot> { wSlot, aSlot }
                      .Concat(inv.Skip(2)).ToList();

        selectedDisplayIndex = displayIndex;
        var slot = display[displayIndex];

        if (slot.itemData == null)
        {
            detailsPanel?.SetActive(false);
            return;
        }

        // 상세패널 세팅
        var d = slot.itemData;
        detailIcon.sprite = d.icon;
        detailName.text = d.itemName;
        detailRarity.text = d.rarity.ToString();
        detailRarity.color = GetColorByRarity(d.rarity);
        detailStats.text = d.itemType == ItemType.Weapon
            ? $"+{d.baseValue} 공격력\n+{d.comboBonusPercent}% 콤보\n {d.sellPrice}G"
            : $"+{d.baseValue} 자동공격\n+{d.comboBonusPercent}% 콤보\n {d.sellPrice}G";

        equipButton.interactable = displayIndex >= 2;
        sellButton.interactable = displayIndex >= 2;

        detailsPanel?.SetActive(true);
    }

    private void OnEquipButton()
    {
        if (selectedDisplayIndex >= 2)
        {
            InventoryManager.Instance.EquipItem(selectedDisplayIndex);
            RefreshInventoryUI();
        }
    }

    private void OnSellButton()
    {
        if (selectedDisplayIndex >= 2)
        {
            InventoryManager.Instance.SellItem(selectedDisplayIndex);
            RefreshInventoryUI();
        }
    }

    private Color GetColorByRarity(ItemRarity r)
    {
        switch (r)
        {
            case ItemRarity.Common: return Color.gray;
            case ItemRarity.Rare: return Color.blue;
            case ItemRarity.Epic: return new Color(0.5f, 0, 0.5f);
            case ItemRarity.Legendary: return new Color(1f, 0.8f, 0f);
            case ItemRarity.Immortal: return Color.red;
            case ItemRarity.Arcana: return new Color(0f, 0.8f, 1f);
            default: return Color.white;
        }
    }
}
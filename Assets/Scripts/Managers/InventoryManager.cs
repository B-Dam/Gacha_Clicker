using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    public ItemData itemData;
}

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("인벤토리 설정")]
    public int maxSlots = 20;
    public List<InventorySlot> slots = new List<InventorySlot>();

    [Header("장착 슬롯")]
    public InventorySlot weaponSlot;
    public InventorySlot accessorySlot;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else Destroy(gameObject);
    }

    private void Initialize()
    {
        slots.Clear();
        for (int i = 0; i < maxSlots; i++)
            slots.Add(new InventorySlot { itemData = null });
        weaponSlot    = new InventorySlot { itemData = null };
        accessorySlot = new InventorySlot { itemData = null };
    }

    /// <summary>
    /// 가챠나 기타로 얻은 아이템을 처리합니다.
    /// 1) Weapon이면 weaponSlot이 비어 있을 때만 장착
    /// 2) Accessory이면 accessorySlot이 비어 있을 때만 장착
    /// 3) 그 외 혹은 장착 슬롯이 차 있으면 무조건 slots[2]부터 빈 칸을 찾아 채웁니다.
    /// </summary>
    public bool AddItem(ItemData data)
    {
        if (data == null) return false;

        // 1) Weapon 자동 장착
        if (data.itemType == ItemType.Weapon)
        {
            if (weaponSlot.itemData == null)
            {
                weaponSlot.itemData = data;
                return true;
            }
        }
        // 2) Accessory 자동 장착
        else if (data.itemType == ItemType.Accessory)
        {
            if (accessorySlot.itemData == null)
            {
                accessorySlot.itemData = data;
                return true;
            }
        }
        else
        {
            // Weapon/Accessory 외는 인벤에도 안 받음
            return false;
        }

        // 3) 자동 장착 못 했으면 인벤 slots[2..] 영역부터 빈 칸 채우기
        for (int i = 2; i < slots.Count; i++)
        {
            if (slots[i].itemData == null)
            {
                slots[i].itemData = data;
                return true;
            }
        }

        // 4) 빈칸 없으면 실패
        return false;
    }

    /// <summary>
    /// Equip 버튼 클릭 시 호출됩니다.
    /// inventory[slotIndex] 의 아이템과,
    /// weaponSlot 또는 accessorySlot 의 아이템을
    /// 서로 자리만 교환합니다.
    /// </summary>
    public bool EquipItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count) return false;
        var data = slots[slotIndex].itemData;
        if (data == null) return false;

        // 어느 종류 장착 슬롯을 업데이트할지 결정
        bool isWeapon = data.itemType == ItemType.Weapon;
        int equipIdx  = isWeapon ? 0 : 1;  // 0=weaponSlot, 1=accessorySlot

        // 현재 장착된 아이템
        InventorySlot equipSlot = isWeapon
            ? weaponSlot
            : accessorySlot;

        // 서로 교환: inventory[slotIndex] ↔ equipSlot
        slots[slotIndex].itemData = equipSlot.itemData;
        if (isWeapon)
            weaponSlot.itemData    = data;
        else
            accessorySlot.itemData = data;

        return true;
    }

    /// <summary>
    /// 장착 해제 버튼(또는 EquipItem 내부)에서 호출:
    /// EquipSlot → slots 리스트의 빈 칸(앞쪽 우선)에 복귀
    /// </summary>
    public void UnequipItem(ItemType type)
    {
        InventorySlot temp = null;
        if (type == ItemType.Weapon && weaponSlot.itemData != null)
        {
            temp = new InventorySlot { itemData = weaponSlot.itemData };
            weaponSlot.itemData = null;
        }
        else if (type == ItemType.Accessory && accessorySlot.itemData != null)
        {
            temp = new InventorySlot { itemData = accessorySlot.itemData };
            accessorySlot.itemData = null;
        }

        if (temp != null)
        {
            // 빈 칸 찾아서 복귀
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].itemData == null)
                {
                    slots[i].itemData = temp.itemData;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 판매 : slots 리스트에서 제거 > 빈 칸으로 남김
    /// </summary>
    public bool SellItem(int invIndex)
    {
        if (invIndex < 0 || invIndex >= slots.Count) return false;
        var data = slots[invIndex].itemData;
        if (data == null) return false;
        GameManager.Instance.AddGold(data.sellPrice);
        slots[invIndex].itemData = null;
        return true;
    }

    /// <summary>
    /// 드래그 앤 드랍 : slots 리스트 내 두 인덱스 교환
    /// (EquipSlot은 dragIndex<0로 설정되어 드래그 금지)
    /// </summary>
    public void SwapSlots(int a, int b)
    {
        if (a < 0 || b < 0) return;
        if (a >= slots.Count || b >= slots.Count) return;

        var tmp = slots[a];
        slots[a] = slots[b];
        slots[b] = tmp;
    }
}
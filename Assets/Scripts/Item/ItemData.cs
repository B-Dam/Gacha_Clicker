using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "GachaClicker/ItemData")]
public class ItemData : ScriptableObject
{
    public string itemName;            // 아이템 이름
    public ItemRarity rarity;          // 등급 (Common, Rare, Epic, Legendary)
    public ItemType itemType;          // 종류 (Weapon/Accessory)
    public Sprite icon;                // UI 아이콘
    public int baseValue;              // 등급별 기본 스탯 수치 (예: Weapon이면 공격력, Accessory면 자동공격 속도)
    public float comboBonusPercent;    // 콤보 보너스 퍼센트
    public int sellPrice;              // 판매 시 얻는 Gold
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 수동/자동 클릭 처리.
/// 인벤토리 리스트 [0] 슬롯의 baseValue를 클릭 보너스로 사용,
/// 인벤토리 리스트 [1] 슬롯의 comboBonusPercent를 콤보 보너스로 사용합니다.
/// </summary>
public class ClickButtonController : MonoBehaviour
{
    public static ClickButtonController Instance { get; private set; }
    
    public Button clickButton;
    public AudioSource clickSound;
    
    public int baseClickPower = 1;    // 기본 클릭 파워

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (clickButton != null)
            clickButton.onClick.AddListener(OnClickChest);
    }

    /// <summary>유저가 클릭했을 때 호출</summary>
    public void OnClickChest()
    {
        // 무기 슬롯
        int weaponBonus = 0;
        var slots = InventoryManager.Instance.slots;
        if (slots.Count > 0 && slots[0].itemData != null)
            weaponBonus = slots[0].itemData.baseValue;

        // 악세 슬롯 콤보 %
        float comboPerc = 0f;
        if (slots.Count > 1 && slots[1].itemData != null)
            comboPerc = slots[1].itemData.comboBonusPercent;

        // 실제 파워
        float totalPower = (baseClickPower + weaponBonus)
                           * (1f + comboPerc / 100f);
        int goldGained = Mathf.RoundToInt(totalPower);

        GameManager.Instance.AddGold(goldGained);
        UIManager.Instance.UpdateTopBarUI();
        clickSound?.Play();
    }

    /// <summary>AutoClickManager에서 호출</summary>
    public void PerformAutoClick()
    {
        OnClickChest();
    }
}
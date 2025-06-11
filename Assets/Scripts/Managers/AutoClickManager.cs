using UnityEngine;

/// <summary>
/// accessory 슬롯(인벤토리 리스트 [1])의 baseValue를 읽어서
/// autoClickInterval을 계산합니다.
/// </summary>
public class AutoClickManager : MonoBehaviour
{
    public static AutoClickManager Instance { get; private set; }

    private float autoClickInterval = Mathf.Infinity;
    private float lastClickTime     = 0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// 인벤토리 첫 두 슬롯: [0]=무기, [1]=악세서리
    /// 악세 슬롯의 baseValue가 speed입니다.
    /// </summary>
    public void UpdateAutoClickInterval()
    {
        var slots = InventoryManager.Instance.slots;
        int speed = 0;
        if (slots.Count > 1 && slots[1].itemData != null)
            speed = slots[1].itemData.baseValue;

        autoClickInterval = speed > 0
            ? 1f / speed
            : Mathf.Infinity;
    }

    private void Update()
    {
        if (Time.time - lastClickTime >= autoClickInterval)
        {
            ClickButtonController.Instance.PerformAutoClick();
            lastClickTime = Time.time;
        }
    }
}
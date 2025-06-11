using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// SlotDragHandler:
/// - slotIndex < 2 (장착칸)이면 드래그 불가
/// - slotIndex >= 2면 InventoryManager.SwapSlots 호출하여 순서 교환
/// </summary>
public class SlotDragHandler : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector]
    public int slotIndex; // realIndex: InventoryManager.slots

    private Transform originalParent;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>()
                      ?? gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData e)
    {
        if (slotIndex < 2) return; // 장착칸 드래그 금지
        originalParent = transform.parent;
        transform.SetParent(transform.root);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData e)
    {
        if (slotIndex < 2) return;
        transform.position = e.position;
    }

    public void OnEndDrag(PointerEventData e)
    {
        canvasGroup.blocksRaycasts = true;
        if (slotIndex >= 2 && e.pointerEnter != null)
        {
            var target = e.pointerEnter.GetComponentInParent<SlotDragHandler>();
            if (target != null && target.slotIndex >= 2 && target.slotIndex != slotIndex)
            {
                InventoryManager.Instance.SwapSlots(slotIndex, target.slotIndex);
                InventoryWindowController.Instance.RefreshInventoryUI();
            }
        }
        transform.SetParent(originalParent);
        transform.localPosition = Vector3.zero;
    }
}
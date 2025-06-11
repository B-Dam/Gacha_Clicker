using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DetailTooltip : MonoBehaviour
{
    [Header("툴팁 정보")]
    public Image            iconImage;
    public TextMeshProUGUI  nameText;
    public TextMeshProUGUI  rarityText;
    public TextMeshProUGUI  statsText;

    private RectTransform rt;
    private Vector2 margin = new Vector2(10f, 10f);

    private void Awake()    
    {
        rt = GetComponent<RectTransform>();
        Hide();
    }

    private void Update()
    {
        // 툴팁이 활성화돼 있으면 매 프레임 마우스 위치로 이동
        if (gameObject.activeSelf)
        {
            Vector2 mousePos = Input.mousePosition;
            // 툴팁 크기 읽기
            Vector2 size = rt.rect.size;
            // 중앙 피벗 기준 오프셋 (반너비+margin, -반높이-margin)
            Vector2 offset = new Vector2(size.x/2 + margin.x, -size.y/2 - margin.y);
            rt.position = mousePos + offset;
        }
    }

    /// <summary>아이템 정보를 세팅하고 툴팁 보이기</summary>
    public void Show(ItemData item)
    {
        iconImage.sprite = item.icon;
        nameText.text    = item.itemName;
        rarityText.text  = item.rarity.ToString();
        statsText.text   = item.itemType == ItemType.Weapon
            ? $"+{item.baseValue} 공격력\n+{item.comboBonusPercent}% 콤보보너스"
            : $"+{item.baseValue} 자동공격\n+{item.comboBonusPercent}% 콤보보너스";
        gameObject.SetActive(true);
    }

    /// <summary>툴팁 숨기기</summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
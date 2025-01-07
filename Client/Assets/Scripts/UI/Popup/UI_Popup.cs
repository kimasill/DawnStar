using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_Popup : UI_Base
{
    public override void Init()
    {
        Managers.UI.SetCanvas(gameObject, true);
    }

    public virtual void ClosePopupUI()
    {
        Managers.UI.ClosePopupUI(this);
    }

    protected virtual void UpdatePopupPosition(Transform transform, PointerEventData eventData)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint);

        RectTransform rectTransform = transform.GetComponent<RectTransform>();
        // 마우스 위치의 오른쪽에 팝업의 좌상단이 위치하도록 조정
        localPoint.x += rectTransform.rect.width / 2 + 25;
        localPoint.y -= rectTransform.rect.height / 2;

        RectTransform parentRect = transform.parent as RectTransform;
        Vector2 minPosition = parentRect.rect.min - rectTransform.rect.min;
        Vector2 maxPosition = parentRect.rect.max - rectTransform.rect.max;


        localPoint.x = Mathf.Clamp(localPoint.x, minPosition.x, maxPosition.x); 
        localPoint.y = Mathf.Clamp(localPoint.y, minPosition.y, maxPosition.y);

        transform.GetComponent<RectTransform>().localPosition = localPoint;
    }
}

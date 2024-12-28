using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class UI_EventHandler : MonoBehaviour, IPointerClickHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IEndDragHandler
{
    public Action<PointerEventData> OnClickHandler = null;
    public Action<PointerEventData> OnRightClickHandler = null;
    public Action<PointerEventData> OnDragHandler = null;
    public Action<PointerEventData> OnBeginDragHandler = null;
    public Action<PointerEventData> OnEndDragHandler = null;
    public Action<PointerEventData> OnMouseOverHandler = null;
    public Action<PointerEventData> OnMouseOutHandler = null;

    public void OnPointerClick(PointerEventData eventData)
	{
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnClickHandler?.Invoke(eventData);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightClickHandler?.Invoke(eventData);
        }
	}

	public void OnDrag(PointerEventData eventData)
    {
		if (OnDragHandler != null)
            OnDragHandler.Invoke(eventData);
	}

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (OnMouseOverHandler != null)
            OnMouseOverHandler.Invoke(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (OnMouseOutHandler != null)
            OnMouseOutHandler.Invoke(eventData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (OnBeginDragHandler != null)
            OnBeginDragHandler.Invoke(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (OnEndDragHandler != null)
            OnEndDragHandler.Invoke(eventData);
    }
}

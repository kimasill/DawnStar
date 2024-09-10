using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_Map_Icon : UI_Base
{
    [SerializeField]
    public List<GameObject> Icons { get; } = new List<GameObject>();

    [SerializeField]
    public List<GameObject> ToolTips { get; } = new List<GameObject>();

    public override void Init()
    {

        // 산하의 Icon 오브젝트들을 리스트로 가져옴
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Icon")) // 아이콘 태그를 가진 오브젝트만 추가
            {
                Icons.Add(child.gameObject);
                AddEventTriggers(child.gameObject);

                // 각 아이콘 아래에 있는 툴팁을 리스트에 추가하고 비활성화
                Transform tooltipTransform = child.Find("UI_IconTooltip");
                if (tooltipTransform != null)
                {
                    GameObject tooltip = tooltipTransform.gameObject;
                    tooltip.SetActive(false); // 초기에는 비활성화
                    ToolTips.Add(tooltip);
                }
            }
        }
    }

    private void AddEventTriggers(GameObject icon)
    {
        EventTrigger trigger = icon.AddComponent<EventTrigger>();

        EventTrigger.Entry entryEnter = new EventTrigger.Entry();
        entryEnter.eventID = EventTriggerType.PointerEnter;
        entryEnter.callback.AddListener((data) => { OnPointerEnter((PointerEventData)data, icon); });
        trigger.triggers.Add(entryEnter);

        EventTrigger.Entry entryExit = new EventTrigger.Entry();
        entryExit.eventID = EventTriggerType.PointerExit;
        entryExit.callback.AddListener((data) => { OnPointerExit((PointerEventData)data, icon); });
        trigger.triggers.Add(entryExit);

        // 클릭 이벤트 추가 (나중을 위해 남겨둠)
        icon.BindEvent((e) => { OnIconClick(icon); });
    }

    private void OnPointerEnter(PointerEventData data, GameObject icon)
    {
        int index = Icons.IndexOf(icon);
        if (index >= 0 && index < ToolTips.Count)
        {
            GameObject tooltip = ToolTips[index];
            tooltip.SetActive(true);
            UpdateTooltip(icon, tooltip);
        }
    }

    private void OnPointerExit(PointerEventData data, GameObject icon)
    {
        int index = Icons.IndexOf(icon);
        if (index >= 0 && index < ToolTips.Count)
        {
            GameObject tooltip = ToolTips[index];
            tooltip.SetActive(false);
        }
    }

    private void OnIconClick(GameObject icon)
    {
        // 클릭 이벤트 처리 (나중을 위해 남겨둠)
        Debug.Log($"{icon.name} 아이콘이 클릭되었습니다.");
    }

    private void UpdateTooltip(GameObject icon, GameObject tooltip)
    {
        Text tooltipText = tooltip.GetComponentInChildren<Text>();
        if (tooltipText != null)
        {
            // 아이콘에 대한 정보를 툴팁에 표시
            tooltipText.text = $"아이콘 이름: {icon.name}";
        }
    }
}
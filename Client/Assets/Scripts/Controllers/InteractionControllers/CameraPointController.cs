using System.Collections.Generic;
using Data;
using Google.Protobuf.Protocol;
using UnityEngine;

public class CameraPointController : InteractionController
{
    private bool _isTriggered = false;
    private List<int> _targetInteraction;
    private int _questId = 0;
    private bool _questTriggered = false;

    protected override void Init()
    {
        base.Init();
        // 추가 초기화 로직이 필요하면 여기에 작성
    }
    public override void Interact()
    {
        _questTriggered = Managers.Quest.IsQuestInProgress(_questId);
        if (ActivateTrigger() == true)
        {
            if (InteractionData.cameraMove)
            {
                InteractionController ic = Managers.Map.GetInteractionById(_targetInteraction[0]);
                StartCoroutine(InteractionCameraMove(ic.transform));
            }
        }
        base.Interact();
    }
    protected override void HandleCameraInteraction(InteractionData data)
    {
        CameraPointData cameraData = (CameraPointData)data;
        _targetInteraction = cameraData.targetInteraction;
        _questId = cameraData.questId;
    }
    public bool ActivateTrigger()
    {
        if (_isTriggered && _questTriggered == false)
            return false;

        _isTriggered = true;
        return true;
    }
    public override void Interact(bool success, bool action, List<int> ids = null)
    {

        base.Interact(success, action, ids);
    }
}
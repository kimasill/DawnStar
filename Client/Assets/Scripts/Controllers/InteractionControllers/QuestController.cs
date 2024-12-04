using System.Collections.Generic;
using Data;
using Google.Protobuf.Protocol;
using UnityEngine;

public class QuestController : InteractionController
{
    public int StartId { get; private set; }
    public int EndId { get; private set; }

    protected override void Init()
    {
        base.Init();
        // 추가 초기화 로직이 필요하면 여기에 작성
    }
    public override void Interact()
    {
        if (StartId > 0)
            Managers.Quest.StartQuest(StartId);
        else if (EndId > 0)
            Managers.Quest.EndQuest(EndId);
        base.Interact();
    }
    protected override void HandleQuestInteraction(InteractionData data)
    {
        QuestSignData questData = (QuestSignData)data;
        StartId = questData.startId;
        EndId = questData.endId;
    }
    public override void Interact(bool success, bool action, List<int> ids = null)
    {
        base.Interact(success, action, ids);
        // 추가 상호작용 로직이 필요하면 여기에 작성
    }
}
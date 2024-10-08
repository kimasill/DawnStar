using Google.Protobuf.Protocol;
using Server.Data;
using Server.Game;
using System;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Linq;
using Server.DB;

public class Quest
{
    public QuestInfo Info { get; } = new QuestInfo();
    public int QuestDbId {
        get { return Info.QuestDbId; }
        set { Info.QuestDbId = value; } 
    }
    public int TemplateId {
        get { return Info.TemplateId; }
        set { Info.TemplateId = value; }
    }
    public string Type {
        get { return Info.QuestType; }
        set { Info.QuestType = value; }
    }
    public bool IsCompleted {
        get { return Info.Completed; }
        set { Info.Completed = value; }
    }
    public int Progress {
        get { return Info.Progress; }
        set { Info.Progress = value; }
    }


    public static Quest MakeQuest(QuestDb questDb)
    {
        Quest quest = null;
        QuestData questData = null;
        DataManager.QuestDict.TryGetValue(questDb.TemplateId, out questData);

        if (questData == null)
            return null;

        quest = new Quest()
        {
            QuestDbId = questDb.QuestDbId,
            TemplateId = questData.id,            
            Type = questData.questType,
            
            IsCompleted = questDb.Completed,
            Progress = questDb.Progress
        };

        return quest;
    }
}

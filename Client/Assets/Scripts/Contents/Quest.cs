using Data;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;

public class Quest
{
    public QuestInfo Info { get; } = new QuestInfo();
    public string Title
    {
        get { return Info.Title; }
        set { Info.Title = value; }
    }
    public Dictionary<int, Script> Description { get; set; }
    public string QuestDescription
    {
        get { return Info.Description; }
        set { Info.Description = value; }
    }
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
    
    public static Quest MakeQuest(QuestInfo questInfo)
    {
        Quest quest=null;
        QuestData questData = null;
        Managers.Data.QuestDict.TryGetValue(questInfo.TemplateId, out questData);

        if (questData == null)
            return null;

        quest = new Quest();
        quest.TemplateId = questData.id;
        quest.Title = questData.title;
        quest.Type = questData.questType;
        quest.QuestDescription = questData.description;

        quest.IsCompleted = questInfo.Completed;
        quest.Progress = questInfo.Progress;
        
        return quest;
    }
}

internal class ExtensionOfNativeClassAttribute : Attribute
{
}
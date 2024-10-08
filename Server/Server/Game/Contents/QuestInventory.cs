using Server.Game;
using System.Collections.Generic;
using System;
using System.Linq;

public class QuestInventory
{
    public Dictionary<int, Quest> Quests { get; } = new Dictionary<int, Quest>();
    public Quest CurrentQuest { get; set; } = null;
    public void Add(Quest quest)
    {
        if (Quests.ContainsKey(quest.TemplateId))
        {
            Quests[quest.TemplateId].Progress = quest.Progress;
        }
        else
        {
            Quests[quest.TemplateId] = quest;
        }

        if (CurrentQuest == null)
            CurrentQuest = quest;
    }

    public void Remove(int questId)
    {
        if (Quests.ContainsKey(questId))
        {
            Quests.Remove(questId);
        }
    }

    public void UpdateQuest(int questId, int progress)
    {
        if (Quests.ContainsKey(questId))
        {
            Quests[questId].Progress = progress;
        }
    }

    public Quest Get(int questId)
    {
        Quests.TryGetValue(questId, out Quest quest);
        return quest;
    }

    public Quest Find(Func<Quest, bool> condition)
    {
        return Quests.Values.FirstOrDefault(condition);
    }

    public int? GetSlot(int questId, int progress)
    {
        if (Quests.ContainsKey(questId) && Quests[questId].Progress >= progress)
        {
            return questId;
        }
        return null;
    }

    public int? GetEmptySlot()
    {
        return Quests.Count < 100 ? (int?)Quests.Count : null; // Assuming max 100 quests
    }

    public int GetQuestProperty(int templateId)
    {
        return Quests.ContainsKey(templateId) ? Quests[templateId].Progress : 0;
    }

    public void SetQuestProperty(int progress, int templateId, Player player)
    {
        if (Quests.ContainsKey(templateId))
        {
            Quests[templateId].Progress = progress;
        }
        else
        {
            Quest quest = new Quest
            {
                TemplateId = templateId,
                Progress = progress
            };
            Add(quest);
        }
    }
}
using Google.Protobuf.Protocol;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestManager
{
    public Dictionary<int, Quest> _quests = new Dictionary<int, Quest>();
    public int Add(QuestInfo questInfo)
    {
        int questId = questInfo.TemplateId;
        if (Managers.Data.ScriptDict.TryGetValue(questId, out Data.ScriptData scriptData))
        {
            Quest quest = new Quest
            {
                Id = questId,
                Description = scriptData.script,
                Type = questInfo.QuestType
            };
            _quests.Add(quest.Id, quest);
        }
        else
        {
            Debug.LogWarning($"퀘스트 ID {questId}를 찾을 수 없습니다.");
        }
        return questId;
    }

    public void StartQuest(int questId)
    {
        if (_quests.TryGetValue(questId, out Quest quest))
        {
            BaseScene currentScene = Managers.Scene.CurrentScene;
            UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
            switch (quest.Type)
            {

                case "epic":                    
                    currentScene.ShowDescriptionUI(quest.Description);
                    break;
                case "story":                    
                    if (gameSceneUI != null)
                    {
                        UI_GameWindow gameWindow = gameSceneUI.GameWindow;
                        if(!gameWindow.isActiveAndEnabled)
                            gameSceneUI.SetActive<UI_GameWindow>(gameWindow, true);
                        if (gameWindow != null)
                        {
                            gameWindow.ShowStoryPanel(quest.Description);
                        }
                        else
                        {
                            Debug.LogWarning("GameWindow를 찾을 수 없습니다.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("GameSceneUI를 찾을 수 없습니다.");
                    }
                    break;
            }
            
        }
        else
        {
            Debug.LogWarning($"퀘스트 ID {questId}를 찾을 수 없습니다.");
        }
    }

    public void UpdateQuest(QuestInfo questInfo)
    {
        int questId = questInfo.TemplateId;
        if (_quests.ContainsKey(questId))
        {
            _quests.Remove(questId);
            CheckNextQuest(questInfo);
        }
        else
        {
            Debug.LogWarning($"퀘스트 ID {questId}를 찾을 수 없습니다.");
        }
    }

    //가장 최근 퀘스트 종료
    public void EndQuest()
    {
        if (_quests.Count == 0)
        {
            Debug.LogWarning("완료할 퀘스트가 없습니다.");
            return;
        }
        else
        {
            //퀘스트 가장 최근꺼 종료
            int latestQuestId = _quests.Keys.Max();
            Quest latestQuest = _quests[latestQuestId];

            C_QuestComplete questCompletePacket = new C_QuestComplete();
            questCompletePacket.QuestDbId = latestQuest.Id;
            Managers.Network.Send(questCompletePacket);
        }
    }

    //Connection : Next Quest Id
    private void CheckNextQuest(QuestInfo questInfo)
    {
        // 다음 퀘스트로 가는 연결점이 있는지 확인
        if (questInfo.Connection != 0)
        {
            // 다음 퀘스트 요청
            C_StartQuest startQuest= new C_StartQuest();
            startQuest.TemplateId = questInfo.Connection;
            Managers.Network.Send(startQuest);
        }
        else
        {
            Debug.Log("다음 퀘스트가 없습니다.");
        }
    }
}

public class Quest
{
    public int Id { get; set; }
    public List<string> Description { get; set; }
    public string Type { get; set; }
}
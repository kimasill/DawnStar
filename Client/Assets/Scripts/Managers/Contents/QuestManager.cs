using Data;
using Google.Protobuf.Protocol;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public class QuestManager
{
    private Dictionary<int, Quest> _quests = new Dictionary<int, Quest>();
    public Dictionary<int, Quest> Quests { get { return _quests; } }
    public Quest CurrentQuest { get; private set; }
    public void Add(Quest quest)
    {
        if(_quests.ContainsKey(quest.TemplateId))
        {           
            UpdateQuestProgress(quest.TemplateId, quest.Progress);
            return;
        }        
        
        if (Managers.Data.ScriptDict.TryGetValue(quest.TemplateId, out ScriptData scriptData))
        {
            quest.Description = scriptData.scripts.ToDictionary(s => s.id, s => s);
        }
        else
        {
            Debug.LogWarning($"퀘스트 Script {quest.TemplateId}를 찾을 수 없습니다.");
        }       

        _quests.Add(quest.TemplateId, quest);
    }
    public Quest GetQuest(int questId)
    {
        if (_quests.TryGetValue(questId, out Quest quest))
        {
            return quest;
        }
        else
        {
            Debug.LogWarning($"퀘스트 ID {questId}를 찾을 수 없습니다.");
            return null;
        }
    }

    public void StartQuest(int id)
    {
        Quest quest = Managers.Quest.GetQuest(id);
        if (quest == null)
        {
            Debug.LogWarning("퀘스트가 없습니다.");
            return;
        }
        if(!_quests.ContainsKey(quest.TemplateId))
        {
            Debug.LogWarning("퀘스트가 등록되지 않았습니다.");
            return;
        }
        Debug.Log($"퀘스트{quest.TemplateId} 진행도 : {quest.Progress} ");
        BaseScene currentScene = Managers.Scene.CurrentScene;
        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        if(quest.Progress == 1)
        {
            switch (quest.Type)
            {
                case "epic":
                    currentScene.ShowDescriptionUI(quest.Description.Values.First().script);
                    break;
                case "story":
                    if (gameSceneUI != null)
                    {
                        UI_GameWindow gameWindow = gameSceneUI.GameWindow;
                        if (!gameWindow.isActiveAndEnabled)
                            gameSceneUI.SetActive<UI_GameWindow>(gameWindow, true);
                        if (gameWindow != null)
                        {
                            gameWindow.ShowStoryPanel(quest.Description, false);
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
                case "interaction":
                    currentScene.StartInteractionQuest(quest.TemplateId);
                    break;
                case "scene":
                    Debug.Log("Start Story Scene");
                    ScriptData scriptData = Managers.Data.ScriptDict[quest.TemplateId];
                    currentScene.ShowStoryScene(scriptData);
                    break;
                case "battle":
                    Debug.Log("Start Battle Scene");
                    currentScene.StartBattleQuest(quest);
                    break;
            }
            Debug.Log($"퀘스트{quest.TemplateId} 진행 시작");
            quest.Progress = 2;
            UI_GameScene gameScene = (UI_GameScene)Managers.UI.SceneUI;
            gameScene.NotificationUI.ShowQuestStartNoti(quest.Title);
            CurrentQuest = quest;
        }        
    }


    public void ShowQuestScript(int questId, int scriptId = 1)
    {
        ScriptData questScriptData = null;
        Managers.Data.ScriptDict.TryGetValue(questId, out questScriptData);

        if (questScriptData != null && questScriptData.scripts.Count >= 1)
        {
            UI_GameScene gameScene = (UI_GameScene)Managers.UI.SceneUI;
            UI_GameWindow gameWindow = gameScene.GameWindow;
            gameWindow.StoryPanel.ShowStoryPanel(questScriptData, scriptId);
        }
    }

    public void UpdateQuest(QuestInfo questInfo)
    {
        int questId = questInfo.TemplateId;
        if (_quests.ContainsKey(questId))
        {
            _quests[questId].IsCompleted = true;
            Quest quest = GetQuest(questId);
            UI_GameScene gameScene = (UI_GameScene)Managers.UI.SceneUI;
            gameScene.NotificationUI.ShowQuestCompleteNoti(quest.Title);
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
            questCompletePacket.TemplateId = latestQuest.TemplateId;
            Managers.Network.Send(questCompletePacket);
        }
    }

    public void EndQuest(int questId)
    {
        if (_quests.ContainsKey(questId))
        {
            C_QuestComplete questCompletePacket = new C_QuestComplete();
            questCompletePacket.TemplateId = questId;
            Managers.Network.Send(questCompletePacket);
        }
        else
        {
            Debug.LogWarning($"퀘스트 ID {questId}를 찾을 수 없습니다.");
        }
    }

    //Connection : Next Quest Id
    private void CheckNextQuest(QuestInfo questInfo)
    {
        // 다음 퀘스트로 가는 연결점이 있는지 확인
        if (questInfo.Connection != 0)
        {
            // 다음 퀘스트 요청
            C_StartQuest startQuest = new C_StartQuest();
            startQuest.TemplateId = questInfo.Connection;
            Managers.Network.Send(startQuest);
        }
        else
        { 
            Debug.Log("다음 퀘스트가 없습니다.");
        }
    }

    public int GetCurrentQuestId()
    {
        // 현재 진행 중인 퀘스트 ID를 반환하는 로직을 구현
        // 예시: 진행 중인 퀘스트 ID를 반환
        // Test Code : 가장 마지막 퀘스트 반환
        return _quests.Keys.Max();
        //return _quests.FirstOrDefault(q => !q.Value.IsCompleted).Key;
    }

    public bool IsQuestInProgress(int questId)
    {
        // 퀘스트 진행 상태를 확인하는 로직을 구현
        // 예시: 현재 진행 중인 퀘스트 목록에서 questId를 찾음
        return _quests.TryGetValue(questId, out Quest quest) && !quest.IsCompleted;
    }
    public List<string> GetQuestScripts(int questId, int scriptListId)
    {
        // 퀘스트 스크립트 데이터를 가져오는 로직을 구현
        // 예시: questId와 scriptListId에 맞는 스크립트를 반환
        if (!_quests.TryGetValue(questId, out Quest quest))
        {
            Debug.LogWarning($"퀘스트 ID {questId}를 찾을 수 없습니다.");
            return null;
        }
        Managers.Data.ScriptDict.TryGetValue(questId, out ScriptData scriptData);
        return scriptData.scripts[scriptListId-1].script;
    }
    public void Clear()
    {
        _quests.Clear();
    }
    public void UpdateQuestProgress(int questId, int progress)
    {
        if (_quests.TryGetValue(questId, out Quest quest))
        {
            quest.Progress = progress;
        }
        else
        {
            Debug.LogWarning($"퀘스트 ID {questId}를 찾을 수 없습니다.");
        }
    }
}

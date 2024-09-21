using Data;
using Google.Protobuf.Protocol;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestManager
{
    public Dictionary<int, Quest> _quests = new Dictionary<int, Quest>();
    public int Add(QuestInfo questInfo)
    {
        if(_quests.ContainsKey(questInfo.TemplateId))
        {
            Debug.LogWarning($"이미 퀘스트 ID {questInfo.TemplateId}가 존재합니다.");
            return -1;
        }
        int questId = questInfo.TemplateId;
        Quest quest = new Quest { };
        if (Managers.Data.ScriptDict.TryGetValue(questId, out ScriptData scriptData))
        {
            quest.Description = scriptData.scripts.ToDictionary(s => s.id, s => s);
        }
        else
        {
            Debug.LogWarning($"퀘스트 Script {questId}를 찾을 수 없습니다.");
        }

        quest.Id = questId;
        quest.Type = questInfo.QuestType;
        quest.IsCompleted = false;

        _quests.Add(quest.Id, quest);

        return questId;
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

    public void StartQuest(int questId)
    {
        if (_quests.TryGetValue(questId, out Quest quest))
        {

            BaseScene currentScene = Managers.Scene.CurrentScene;
            UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
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
                case "interaction":
                    currentScene.StartInteractionQuest(quest);
                    break;
                case "scene":
                    Debug.Log("Start Story Scene");
                    ScriptData scriptData = Managers.Data.ScriptDict[questId];
                    currentScene.ShowStoryScene(scriptData);
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
            _quests[questId].IsCompleted = true;
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
        return _quests.FirstOrDefault(q => !q.Value.IsCompleted).Key;
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
}

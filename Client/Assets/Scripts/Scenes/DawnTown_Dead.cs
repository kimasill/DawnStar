using Data;
using Google.Protobuf.Protocol;
using UnityEngine;

public class DawnTownDead : DawnTown
{
    protected override void Init()
    {
        base.Init();
        SceneType = Define.Scene.DawnTownDead;

        Managers.Map.LoadMap(4); // DawnTownDead 맵 로드

        Screen.SetResolution(1920, 1080, false);
        _sceneUi.SetActive(_sceneUi.GameWindow, true);
        StartQuest07();
    }

    public override void Clear()
    {
        // 씬 클리어 시 필요한 작업을 여기에 작성
    }

    private void StartQuest07()
    {
        if(Managers.Quest.CurrentQuest != null)
        {
            return;
        }
        // 퀘스트 07 시작
        if(Managers.Quest.Quests == null || Managers.Quest.Quests.Count == 0)
        {
            Debug.Log("퀘스트 목록이 비어있습니다. 퀘스트 7번을 시작합니다.");
            C_StartQuest quest = new C_StartQuest() { TemplateId = 7 };
            Managers.Network.Send(quest);
            return;
        }

        // 퀘스트 6번이 완료되었는지 확인
        Quest quest6 = Managers.Quest.GetQuest(6);
        if (quest6 != null && quest6.IsCompleted)
        {
            Debug.Log("퀘스트 6번이 완료되었습니다. 퀘스트 7번을 시작합니다.");
            C_StartQuest quest = new C_StartQuest() { TemplateId = 7 };
            Managers.Network.Send(quest);
            return;
        }

        // 현재 진행 중인 퀘스트가 7번인지 확인
        if (Managers.Quest.GetCurrentQuestId() == 7)
        {
            Debug.Log("현재 진행 중인 퀘스트가 7번입니다.");
            C_StartQuest quest = new C_StartQuest() { TemplateId = 7 };
            Managers.Network.Send(quest);
        }
    }

    public override void StartBattleQuest(Quest quest)
    {
        if(quest.TemplateId == 7)
        {
            Managers.Quest.ShowQuestScript(7);
            GameObject  map = Managers.Map.CurrentGrid.gameObject;
            if (map == null)
            {
                Debug.LogError("MapData not found for scene: " + SceneType);
                return;
            }

            // 스폰 포인트 오브젝트 찾기
            GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("Spawn");
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogError("No spawn points found in the scene.");
                return;
            }

            C_RequestMonster spawnMonster = new C_RequestMonster();
            foreach (GameObject spawnPoint in spawnPoints)
            {
                string spawnPointName = spawnPoint.name;                
                int spawnPointId;

                //오브젝트 이름_숫자 형태로 되어있는 스폰 포인트 이름에서 숫자만 추출
                if (!int.TryParse(spawnPointName.Substring(spawnPointName.Length - 1), out spawnPointId))
                {
                    Debug.LogError("Spawn point name is not in the correct format.");
                    return;
                }                
                spawnMonster.Id.Add(spawnPointId);                               
            }            
            Managers.Network.Send(spawnMonster);
        }   
    }

    public void OnPlayerExit()
    {
        // 플레이어가 DawnTownDead 씬을 나갈 때 호출되는 메서드
        Debug.Log("Player is exiting DawnTownDead scene.");
        // 다음 씬으로 이동하는 로직 추가
        //Managers.Scene.LoadScene(Define.Scene.NextScene); // Define.Scene.NextScene는 다음 씬의 Enum 값
    }
}
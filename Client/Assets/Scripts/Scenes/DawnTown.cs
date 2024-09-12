using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DawnTown : BaseScene
{
    protected UI_GameScene _sceneUi;
    protected UI_Description _description;
    public override void Clear()
    {
    }

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.DawnTown;

        Managers.Map.LoadMap(1);

        Screen.SetResolution(640, 480, false);

        _sceneUi = Managers.UI.ShowSceneUI<UI_GameScene>();
        _sceneUi.GetMap("001");
        _description = Managers.UI.ShowPopupUI<UI_Description>();
        _description.gameObject.SetActive(false);
    }

    public override void ShowDescriptionUI(List<string> description)
    {
        _description.gameObject.SetActive(true);      
        _description.ShowDescription(description);
    }
    protected override void InitializeNPCs()
    {
        NPCController[] npcControllers = Managers.Map.CurrentGrid.gameObject.GetComponentsInChildren<NPCController>();
        Dictionary<string, NPCData> npcDict = Managers.Data.NPCDict;
        foreach (NPCController npc in npcControllers)
        {
            string npcName = npc.gameObject.name;
            if (npcDict.TryGetValue(npcName, out NPCData npcData))
            {
                npc.TemplateId = npcData.id;
                AddNPC(npcData.id, npc.gameObject);
            }
        }
    }

}

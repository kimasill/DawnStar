using Data;
using Google.Protobuf.Protocol;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class BaseScene : MonoBehaviour
{
    public Define.Scene SceneType { get; protected set; } = Define.Scene.Unknown;
    Dictionary<int, GameObject> _npcs = new Dictionary<int, GameObject>();
    public Camera MainCamera { get { return Camera.main; }}
    public float ZoomLevel = 2f;

    private Texture2D _customCursor;
    void Awake()
	{
		Init();
	}

	protected virtual void Init()
    {
        Object obj = GameObject.FindObjectOfType(typeof(EventSystem));
        if (obj == null)
            Managers.Resource.Instantiate("UI/EventSystem").name = "@EventSystem";
        _customCursor = Managers.Resource.Load<Texture2D>("Textures/Images/Icons/Cursor/Cursor_Basic");
        if (_customCursor != null)
        {
            Vector2 hotspot = new Vector2(_customCursor.width * 6 / 64f, _customCursor.height * 19 / 64f);
            Cursor.SetCursor(_customCursor, hotspot, CursorMode.Auto);
        }
        else
        {
            Debug.LogError("Failed to load cursor image.");
        }

        Screen.SetResolution(1920, 1080, false);
    }
    protected void RequestShop(int mapId)
    {
        Managers.Shop.Clear();
        Managers.Shop.InitializeShop(mapId);
    }
    public virtual void ShowDescriptionUI(List<string> description)
    {        
    }
    public virtual void StartInteractionQuest(int questId) { }
    public virtual void CheckInteractionQuest(int questId) { }
    public virtual void CheckOnSceneLoadedQuest() 
    {
        if (Managers.Scene.IsSceneLoaded)
        {
            return;
        }
        C_StartQuest startQuestPacket = new C_StartQuest();
        startQuestPacket.TemplateId = 0;
        Managers.Network.Send(startQuestPacket);
    }
    public virtual void ShowStoryScene(ScriptData scriptData) 
    {
        UI_GameScene gameUI = Managers.UI.SceneUI as UI_GameScene;
        UI_StoryScene storyScene = gameUI.StoryScene;
        gameUI.GameWindow.gameObject.SetActive(false);
        storyScene.gameObject.SetActive(true);
        storyScene.LoadStoryData(scriptData);
        storyScene.ShowStory();
    }
    public virtual void StartBattleQuest(Quest quest) { }
    protected void InitializeNPCs()
    {
        NPCController[] npcControllers = Managers.Map.CurrentGrid.gameObject.GetComponentsInChildren<NPCController>();
        if(npcControllers == null)
            return;
        Dictionary<string, NPCData> npcDict = Managers.Data.NPCDict;
        foreach (NPCController npc in npcControllers)
        {
            string npcName = npc.gameObject.name;
            npc.SetNPC(npcName);     
            AddNPC(npc.TemplateId, npc.gameObject);
        }
    }
    
    public void AddNPC(int id, GameObject npc)
    {
        if (!_npcs.ContainsKey(id))
        {
            _npcs.Add(id, npc);
        }
    }

    public void RemoveNPC(int id)
    {
        if (_npcs.ContainsKey(id))
        {
            _npcs.Remove(id);
        }
    }
    public void ClearNPCs()
    {
        _npcs.Clear();
    }

    public Dictionary<int, GameObject> GetNPCs()
    {
        return _npcs;
    }

    public void OnApplicationQuit()
    {
        Managers.Clear();
        ClearNPCs();
    }

    public abstract void Clear();
}

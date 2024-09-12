using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class BaseScene : MonoBehaviour
{
    public Define.Scene SceneType { get; protected set; } = Define.Scene.Unknown;
    Dictionary<int, GameObject> _npcs = new Dictionary<int, GameObject>();

    void Awake()
	{
		Init();
	}

	protected virtual void Init()
    {
        Object obj = GameObject.FindObjectOfType(typeof(EventSystem));
        if (obj == null)
            Managers.Resource.Instantiate("UI/EventSystem").name = "@EventSystem";
    }
    protected abstract void InitializeNPCs();
    public virtual void ShowDescriptionUI(List<string> description)
    {        
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

    public Dictionary<int, GameObject> GetNPCs()
    {
        return _npcs;
    }

    public abstract void Clear();
}

using Data;
using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager
{
	public MyPlayerController MyPlayer { get; set; }
	//id 에따라 관리
	Dictionary<int, GameObject> _objects = new Dictionary<int, GameObject>();
    public static GameObjectType GetObjectType(int id)
    {
		int type = (id >> 24) & 0x7F; 
        return (GameObjectType)type;
    }
	public void Add(ObjectInfo info, bool myPlayer = false, bool activate = true) 
	{ 
		if(MyPlayer != null && MyPlayer.Id == info.ObjectId) 
            return;
        if (_objects.ContainsKey(info.ObjectId))
            return;
        Debug.Log($" myplayer:{myPlayer}, objectId :{info.ObjectId }");
        GameObjectType type = GetObjectType(info.ObjectId);
		if(type == GameObjectType.Player)
        {
            if (myPlayer)
            {
                GameObject go = Managers.Resource.Instantiate("Creature/MyPlayer");
                go.name = info.Name;
                _objects.Add(info.ObjectId, go);

                MyPlayer = go.GetComponent<MyPlayerController>();
                MyPlayer.Id = info.ObjectId;
                MyPlayer.PosInfo = info.Position;
				MyPlayer.Stat.MergeFrom(info.StatInfo);			            				
                MyPlayer.SyncPos();
                Managers.Inventory.RefreshEquipment(MyPlayer.Equipment);
            }
            else
            {
                GameObject go = Managers.Resource.Instantiate("Creature/PlayerWarrior");
                go.name = info.Name;
                _objects.Add(info.ObjectId, go);

                PlayerController pc = go.GetComponent<PlayerController>();
                pc.Id = info.ObjectId;
                pc.PosInfo = info.Position;
				pc.Stat.MergeFrom(info.StatInfo);
                pc.SyncPos();
            }
        }
        else if(type == GameObjectType.Monster)
        {
            GameObject go = Managers.Resource.Instantiate($"Creature/{info.Name}");
            Managers.Data.MonsterDict.TryGetValue(info.TemplateId, out MonsterData monsterData);
            go.name = info.Name;
            _objects.Add(info.ObjectId, go);

            MonsterController mc = go.GetComponent<MonsterController>();
            mc.Id = info.ObjectId;
            mc.PosInfo = info.Position;
			mc.Stat = info.StatInfo;
            mc.SyncPos();
        }
		else if(type == GameObjectType.Projectile)
        {
            GameObject go = Managers.Resource.Instantiate("Creature/Arrow");
            go.name = info.Name;
            _objects.Add(info.ObjectId, go);

            ArrowController ac = go.GetComponent<ArrowController>();
			ac.PosInfo = info.Position;
			ac.Stat = info.StatInfo;			
            ac.SyncPos();
        }
        else if(type == GameObjectType.Item)
        {
            GameObject go = Managers.Resource.Instantiate($"Item/Item");
            go.name = info.Name;
            _objects.Add(info.ObjectId, go);

            ItemController ic = go.GetComponent<ItemController>();
            ic.Id = info.ObjectId;
            ic.PosInfo = info.Position;
            ic.SyncPos();
        }
    }

    //임시 아이디 생성
    public void GenerateId(GameObjectType type, out int id)
    {
        int typeCode = (int)type;
        Debug.Log($"typeCode : {typeCode}");
        int newId;
        do
        {
            newId = (typeCode << 24) | UnityEngine.Random.Range(1, 0x7F);
        } while (_objects.ContainsKey(newId));
        Debug.Log($"newId : {newId}");
        id = newId;
    }

    public bool IsPlayingAnim(int id)
    {
        if (_objects.ContainsKey(id))
        {
            GameObject go = FindById(id);
            if (go == null)
                return false;

            CreatureController cc = go.GetComponent<CreatureController>();
            if (cc == null)
                return false;
            if (GameObjectType.Monster == GetObjectType(id))
            {
                if (cc.IsPlayingDieAnimation())
                {
                    return true;
                }
                else return false;
            }
        }
        return false;
    }
    public IEnumerator RemoveAfterAnimation(int id)
    {
        GameObject go = FindById(id);
        CreatureController cc = go.GetComponent<CreatureController>();
        Animator animator = cc.GetComponent<Animator>();
        if (animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            while (stateInfo.IsName("DEATH") && stateInfo.normalizedTime < 1.0f)
            {
                yield return null;
                stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            }
        }
        Remove(id);
    }

    public void Remove(int id)
	{
        //if (MyPlayer != null && MyPlayer.Id == id)
        //    return;
		if(_objects.ContainsKey(id) == false)
            return;

        GameObject go = FindById(id);
		if (go == null)
            return;

		_objects.Remove(id);
		Managers.Resource.Destroy(go);
	}

	public GameObject FindCreature(Vector3Int cellPos)
	{
		foreach (GameObject obj in _objects.Values)
		{
			CreatureController cc = obj.GetComponent<CreatureController>();
			if (cc == null)
				continue;

			if (cc.CellPos == cellPos)
				return obj;
		}

		return null;
	}

    public GameObject FindItem(Vector3Int cellPos)
    {
        foreach (GameObject obj in _objects.Values)
        {
            ItemController ic = obj.GetComponent<ItemController>();
            if (ic == null)
                continue;

            if (ic.CellPos == cellPos)
                return obj;
        }

        return null;
    }

    public GameObject FindById(int id)
    {
        GameObject go = null;
        _objects.TryGetValue(id, out go);
        return go;
    }

	public GameObject Find(Func<GameObject, bool> condition)
	{
		foreach (GameObject obj in _objects.Values)
		{
			if (condition.Invoke(obj))
				return obj;
		}

		return null;
	}

	public void Clear()
	{
		foreach (GameObject obj in _objects.Values)
		{
			Managers.Resource.Destroy(obj);
		}
        _objects.Clear();
		MyPlayer = null;
	}
}

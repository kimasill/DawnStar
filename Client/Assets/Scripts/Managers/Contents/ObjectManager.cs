using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager
{
    List<GameObject> _objects = new List<GameObject>();

    public void AddObject(GameObject go)
    {
        _objects.Add(go);
    }

    public void RemoveObject(GameObject go)
    {
        if (_objects.Contains(go))
        {
            _objects.Remove(go);
        }
    }
    public GameObject Find(Vector3Int cellPos)
    {
        foreach (GameObject go in _objects)
        {
            CreatureController cc = go.GetComponent<CreatureController>();
            if (cc == null)
            {
                continue;
            }
            if (cc.CellPos == cellPos)
            {
                return go;
            }
        }
        return null;
    }
    public void Clear()
    {
        _objects.Clear();
    }

    internal GameObject Find(Func<GameObject, bool> condition)
    {
        foreach (GameObject go in _objects)
        {
            if(condition.Invoke(go))
            {
                return go;
            }
        }
        return null;
    }
}

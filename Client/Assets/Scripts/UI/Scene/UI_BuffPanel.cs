using Data;
using System;
using System.Collections.Generic;
using UnityEngine;

public class UI_BuffPanel : UI_Base
{
    private Dictionary<int, Tuple<UI_Buff_Icon, float>> _buffIcons = new Dictionary<int, Tuple<UI_Buff_Icon, float>>();
    private Dictionary<int, Tuple<UI_Buff_Icon, float>> _debuffIcons = new Dictionary<int, Tuple<UI_Buff_Icon, float>>();
    bool _isInit = false;

    public override void Init()
    {
        _isInit = true;
    }

    public void RefreshUI()
    {
        if (_isInit == false)
        {
            Init();
            _isInit = true;
        }
    }

    public void AddBuff(int buffId, float value)
    {
        if (value == 0)
        {
            RemoveBuff(buffId);
            return;
        }

        Managers.Data.BuffDict.TryGetValue(buffId, out BuffData buffData);
        if (buffData == null)
            return;

        Sprite sprite = Managers.Resource.Load<Sprite>(buffData.icon);
        if (_buffIcons.ContainsKey(buffId))
        {
            _buffIcons[buffId].Item1.SetIcon(sprite);
            _buffIcons[buffId] = new Tuple<UI_Buff_Icon, float>(_buffIcons[buffId].Item1, value);
        }
        else
        {
            GameObject go = Managers.Resource.Instantiate("UI/Scene/UI_Buff_Icon", transform);
            UI_Buff_Icon buffIcon = go.GetComponent<UI_Buff_Icon>();
            buffIcon.SetIcon(sprite);
            _buffIcons.Add(buffId, new Tuple<UI_Buff_Icon, float>(buffIcon, value));
        }
        _buffIcons[buffId].Item1.Key = buffData.name;
        _buffIcons[buffId].Item1.Value = value;
    }

    public void AddDebuff(int buffId, float value)
    {
        if (value == 0)
        {
            RemoveDebuff(buffId);
            return;
        }

        Managers.Data.DebuffDict.TryGetValue(buffId, out DebuffData debuffData);
        if (debuffData == null)
            return;

        Sprite sprite = Managers.Resource.Load<Sprite>(debuffData.icon);
        if (_debuffIcons.ContainsKey(buffId))
        {
            _debuffIcons[buffId].Item1.SetIcon(sprite);
            _debuffIcons[buffId].Item1.SetFrame(false);
            _debuffIcons[buffId] = new Tuple<UI_Buff_Icon, float>(_debuffIcons[buffId].Item1, value);
        }
        else
        {
            GameObject go = Managers.Resource.Instantiate("UI/Scene/UI_Buff_Icon", transform);
            UI_Buff_Icon buffIcon = go.GetComponent<UI_Buff_Icon>();
            buffIcon.SetIcon(sprite);
            _debuffIcons.Add(buffId, new Tuple<UI_Buff_Icon, float>(buffIcon, value));
        }

        _debuffIcons[buffId].Item1.Key = debuffData.name;
        _debuffIcons[buffId].Item1.Value = value;
    }

    public void RemoveBuff(int buffId)
    {
        if (_buffIcons.ContainsKey(buffId))
        {
            Destroy(_buffIcons[buffId].Item1.gameObject);
            _buffIcons.Remove(buffId);
        }
    }

    public void RemoveDebuff(int buffId)
    {
        if (_debuffIcons.ContainsKey(buffId))
        {
            Destroy(_debuffIcons[buffId].Item1.gameObject);
            _debuffIcons.Remove(buffId);
        }
    }
}
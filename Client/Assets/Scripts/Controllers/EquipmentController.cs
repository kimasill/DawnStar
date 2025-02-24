using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Google.Protobuf.Protocol;
using Data;
using static Item;
using System;

public class EquipmentController : MonoBehaviour
{
    public List<SpriteRenderer> _itemList = new List<SpriteRenderer>();
    public List<SpriteRenderer> _eyeList = new List<SpriteRenderer>();
    public List<SpriteRenderer> _hairList = new List<SpriteRenderer>();
    public List<SpriteRenderer> _bodyList = new List<SpriteRenderer>();
    public List<SpriteRenderer> _clothList = new List<SpriteRenderer>();
    public List<SpriteRenderer> _armorList = new List<SpriteRenderer>();
    public List<SpriteRenderer> _pantList = new List<SpriteRenderer>();
    public List<SpriteRenderer> _weaponList = new List<SpriteRenderer>();
    public List<SpriteRenderer> _backList = new List<SpriteRenderer>();

    public Weapon Weapon;
    public Weapon Shield;
    public Cloth Cloth;
    public Armor Helmet;
    public Armor Armor;
    public Armor Boots;
    public Armor Back;

    public SPUM_HorseSpriteList _spHorseSPList;
    public string _spHorseString;
    // Start is called before the first frame update

    public Texture2D _bodyTexture;
    public string _bodyString;

    public List<string> _hairListString = new List<string>();
    public List<string> _clothListString = new List<string>();
    public List<string> _armorListString = new List<string>();
    public List<string> _pantListString = new List<string>();
    public List<string> _weaponListString = new List<string>();
    public List<string> _backListString = new List<string>();

    public void Refresh(Dictionary<int, Item> items)
    {
        foreach (var item in items.Values)
        {
            if(item.Equipped)
                SetItemInSlot(item);
            else if (item.Equipped == false)
                RemoveItemFromSlot(item);
        }
    }
    public void EquipItem(Item item)
    {
        if (item.Equipped)
        {
            SetItemInSlot(item);
            Managers.Sound.Play("Effect/Equip", Define.Sound.Effect);
        }
        else if (item.Equipped == false)
        {
            RemoveItemFromSlot(item);
            Managers.Sound.Play("Effect/Unequip", Define.Sound.Effect);
        }
    }
    public void SetItemInSlot(Item item)
    {
        ItemData itemData = null;
        Managers.Data.ItemDict.TryGetValue(item.TemplateId, out itemData);

        Sprite[] sprite = null;

        if (itemData.prefabPath == null)
        {
            sprite = Managers.Resource.LoadAll<Sprite>(itemData.iconPath);
        }
        else sprite = Managers.Resource.LoadAll<Sprite>(itemData.prefabPath);

        if (item is Weapon weapon)
        {
            if (weapon.WeaponType == WeaponType.Sword || weapon.WeaponType == WeaponType.Bow)
            {
                Weapon = weapon;
                _weaponList[0].sprite = sprite[0];
                _weaponList[0].transform.localScale = new Vector3(2, 2, 1); // 크기를 2배로 설정
            }
            else if (weapon.WeaponType == WeaponType.Shield)
            {
                Shield = weapon;
                _weaponList[3].sprite = sprite[0];
                _weaponList[3].transform.localScale = new Vector3(2, 2, 1); // 크기를 2배로 설정
            }
        }
        else if (item is Armor armor)
        {
            if (armor.ArmorType == ArmorType.Helmet)
            {
                Helmet = armor;                
                _hairList[2].sprite = sprite[0];
            }
            else if (armor.ArmorType == ArmorType.Armor)
            {
                Armor = armor;
                foreach (var t in sprite)
                {
                    if (t.name == "Body")
                    {
                        _armorList[0].sprite = t;
                    }
                    else if (t.name == "Left")
                    {
                        _armorList[1].sprite = t;
                        //_armorList[1].transform.localScale = new Vector3(2, 2, 1); // 크기를 2배로 설정
                    }
                    else if (t.name == "Right")
                    {
                        _armorList[2].sprite = t;
                        //_armorList[2].transform.localScale = new Vector3(2, 2, 1); // 크기를 2배로 설정
                    }
                }
            }
            else if (armor.ArmorType == ArmorType.Boots)
            {
                Boots = armor;
                foreach (var t in sprite)
                {
                    if (t.name == "Left")
                    {
                        _pantList[0].sprite = t;
                        //_armorList[1].transform.localScale = new Vector3(2, 2, 1); // 크기를 2배로 설정
                    }
                    else if (t.name == "Right")
                    {
                        _pantList[1].sprite = t;
                        //_armorList[2].transform.localScale = new Vector3(2, 2, 1); // 크기를 2배로 설정
                    }
                }
            }
            else if (armor.ArmorType == ArmorType.Back)
            {
                Back = armor;
                Sprite backSprite = Managers.Resource.Load<Sprite>(itemData.prefabPath);
                _backList[0].sprite = backSprite;
                //_backList[0].transform.localScale = new Vector3(2, 2, 1); // 크기를 2배로 설정
            }
        }
    }
    public void RemoveItemFromSlot(Item item)
    {
        ItemData itemData = null;
        Managers.Data.ItemDict.TryGetValue(item.TemplateId, out itemData);
        if (itemData == null)
            return;

        if (item is Weapon weapon)
        {
            if (weapon.WeaponType == WeaponType.Sword || weapon.WeaponType == WeaponType.Bow)
            {
                if(Weapon == weapon)
                {
                    Weapon = null;
                    _weaponList[0].sprite = null;
                    _weaponList[0].transform.localScale = Vector3.one; // 크기를 원래 크기로 설정
                }
            }
            else if (weapon.WeaponType == WeaponType.Shield)
            {
                if (Shield == weapon)
                {
                    Shield = null;
                    _weaponList[3].sprite = null;
                    _weaponList[3].transform.localScale = Vector3.one; // 크기를 원래 크기로 설정
                }
            }
        }
        else if (item is Armor armor)
        {
            if (armor.ArmorType == ArmorType.Helmet)
            {
                if (Helmet == armor)
                {
                    Helmet = null;
                    _hairList[2].sprite = null;                    
                }
            }
            else if (armor.ArmorType == ArmorType.Armor)
            {
                if (Armor == armor)
                {
                    Armor = null;
                    _armorList[0].sprite = null;
                    _armorList[1].sprite = null;
                    _armorList[2].sprite = null;
                }
            }
            else if (armor.ArmorType == ArmorType.Boots)
            {
                if (Boots == armor)
                {
                    Boots = null;
                    _pantList[0].sprite = null;
                    _pantList[1].sprite = null;
                }
            }
            else if (armor.ArmorType == ArmorType.Back)
            {
                if (Back == armor)
                {
                    Back = null;
                    _backList[0].sprite = null;
                }
            }
        }
    }

    public void Reset()
    {
        for(var i = 0 ; i < _hairList.Count;i++)
        {
            if(_hairList[i]!=null) _hairList[i].sprite = null;
        }
        for(var i = 0 ; i < _clothList.Count;i++)
        {
            if(_clothList[i]!=null) _clothList[i].sprite = null;
        }
        for(var i = 0 ; i < _armorList.Count;i++)
        {
            if(_armorList[i]!=null) _armorList[i].sprite = null;
        }
        for(var i = 0 ; i < _pantList.Count;i++)
        {
            if(_pantList[i]!=null) _pantList[i].sprite = null;
        }
        for(var i = 0 ; i < _weaponList.Count;i++)
        {
            if(_weaponList[i]!=null) _weaponList[i].sprite = null;
        }
        for(var i = 0 ; i < _backList.Count;i++)
        {
            if(_backList[i]!=null) _backList[i].sprite = null;
        }
    }

    public void LoadSpriteSting()
    {

    }

    public void LoadSpriteStingProcess(List<SpriteRenderer> SpList, List<string> StringList)
    {
        for(var i = 0 ; i < StringList.Count ; i++)
        {
            if(StringList[i].Length > 1)
            {

                // Assets/SPUM/SPUM_Sprites/BodySource/Species/0_Human/Human_1.png
            }
        }
    }

    public void LoadSprite(EquipmentController data)
    {
        //스프라이트 데이터 연동
        SetSpriteList(_hairList,data._hairList);
        SetSpriteList(_bodyList,data._bodyList);
        SetSpriteList(_clothList,data._clothList);
        SetSpriteList(_armorList,data._armorList);
        SetSpriteList(_pantList,data._pantList);
        SetSpriteList(_weaponList,data._weaponList);
        SetSpriteList(_backList,data._backList);
        SetSpriteList(_eyeList,data._eyeList);
        
        if(data._spHorseSPList!=null)
        {
            SetSpriteList(_spHorseSPList._spList,data._spHorseSPList._spList);
            _spHorseSPList = data._spHorseSPList;
        }
        else
        {
            _spHorseSPList = null;
        }

        //색 데이터 연동.
        if(_eyeList.Count> 2 &&  data._eyeList.Count > 2 )
        {
            _eyeList[2].color = data._eyeList[2].color;
            _eyeList[3].color = data._eyeList[3].color;
        }

        _hairList[3].color = data._hairList[3].color;
        _hairList[0].color = data._hairList[0].color;
        //꺼져있는 오브젝트 데이터 연동.x
        _hairList[0].gameObject.SetActive(!data._hairList[0].gameObject.activeInHierarchy);
        _hairList[3].gameObject.SetActive(!data._hairList[3].gameObject.activeInHierarchy);

        _hairListString = data._hairListString;
        _clothListString = data._clothListString;
        _pantListString = data._pantListString;
        _armorListString = data._armorListString;
        _weaponListString = data._weaponListString;
        _backListString = data._backListString;
    }

    public void SetSpriteList(List<SpriteRenderer> tList, List<SpriteRenderer> tData)
    {
        for(var i = 0 ; i < tData.Count;i++)
        {
            if(tData[i]!=null) 
            {
                tList[i].sprite = tData[i].sprite;
                tList[i].color = tData[i].color;
            }
            else tList[i] = null;
        }
    }

    public void ResyncData()
    {
        SyncPath(_hairList,_hairListString);
        SyncPath(_clothList,_clothListString);
        SyncPath(_armorList,_armorListString);
        SyncPath(_pantList,_pantListString);
        SyncPath(_weaponList,_weaponListString);
        SyncPath(_backList,_backListString);
    }

    public void SyncPath(List<SpriteRenderer> _objList, List<string> _pathList)
    {
        for(var i = 0 ; i < _pathList.Count ; i++)
        {
            if(_pathList[i].Length > 1 ) 
            {
                string tPath = _pathList[i];
                tPath = tPath.Replace("Assets/Resources/","");
                tPath = tPath.Replace(".png","");
                
                Sprite[] tSP = Resources.LoadAll<Sprite>(tPath);
                if(tSP.Length > 1)
                {
                    _objList[i].sprite = tSP[i];
                }
                else if (tSP.Length > 0)
                {
                    _objList[i].sprite = tSP[0];
                }
            }
            else
            {
                _objList[i].sprite = null;
            }
        }
    }
}

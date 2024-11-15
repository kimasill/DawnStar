using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Matching : UI_Scene
{
    enum Images
    {
        EntranceButton,
        MatchingButton,
        MatchingCancelButton
    }

    enum Texts
    {
        LevelText,
        MaxPlayerText,
        MonstersText,
        MatchingNoticeText,
        TitleText,
    }

    enum GameObjects
    {
        MatchingSpinner,
        ItemIconScrollViewContent
    }

    private bool _isMatching = false;

    [SerializeField]
    private List<UI_ItemIcon> _itemIcons = new List<UI_ItemIcon>();
    [SerializeField]
    private GameObject _content = null;
    public MapData MapData { get; set; }
    public override void Init()
    {
        base.Init();
        
        Bind<Text>(typeof(Texts));
        Bind<Button>(typeof(Images));
        Bind<GameObject>(typeof(GameObjects));

        GetImage((int)Images.EntranceButton).gameObject.BindEvent(OnClickEnterButton, Define.UIEvent.Click);
        GetImage((int)Images.MatchingButton).gameObject.BindEvent(OnClickMatchingButton, Define.UIEvent.Click);
        GetImage((int)Images.MatchingCancelButton).gameObject.BindEvent(OnClickMatchingButton, Define.UIEvent.Click);
    }

    private void OnClickEnterButton(PointerEventData evt)
    {
        C_EnterDungeon enterDungeonPacket = new C_EnterDungeon();
        enterDungeonPacket.MapId = MapData.id;
        enterDungeonPacket.AdmitType = AdmitType.None;
        Managers.Network.Send(enterDungeonPacket);
    }

    private void OnClickMatchingButton(PointerEventData evt)
    {
        if (!_isMatching)
        {
            _isMatching = true;
            GetText((int)Texts.MatchingNoticeText).text = "매칭 중";
            GetObject((int)GameObjects.MatchingSpinner).SetActive(true);
            GetImage((int)Images.MatchingCancelButton).gameObject.SetActive(true);
            C_EnterDungeon enterDungeonPacket = new C_EnterDungeon();
            enterDungeonPacket.MapId = MapData.id;
            enterDungeonPacket.AdmitType = AdmitType.Matching;
            Managers.Network.Send(enterDungeonPacket);
        }
    }

    private void OnClickMatchingCancelButton(PointerEventData evt)
    {
        if (_isMatching)
        {
            _isMatching = false;
            GetText((int)Texts.MatchingNoticeText).text = "";
            GetObject((int)GameObjects.MatchingSpinner).SetActive(false);
            GetImage((int)Images.MatchingCancelButton).gameObject.SetActive(false);

            C_EnterDungeon enterDungeonPacket = new C_EnterDungeon();
            enterDungeonPacket.MapId = MapData.id;
            enterDungeonPacket.AdmitType = AdmitType.Cancel;
            Managers.Network.Send(enterDungeonPacket);
        }
    }

    private IEnumerator SimulateMatching()
    {
        // 매칭 성공을 시뮬레이션하기 위해 3초 대기
        yield return new WaitForSeconds(3f);

        _isMatching = false;
        GetText((int)Texts.MatchingNoticeText).text = "매칭 성공";
        GetObject((int)GameObjects.MatchingSpinner).SetActive(false);
    }
    public void RefreshUI(int mapId)
    {
        MapData mapData = Managers.Data.MapDict.TryGetValue(mapId, out mapData) ? mapData : null;
        if (mapData == null) return;
        MapData = mapData;
        foreach (UI_ItemIcon icon in _itemIcons)
        {
            if (icon != null)
            {
                Managers.Resource.Destroy(icon.gameObject);
            }
        }

        DungeonData dungeonData = mapData.dungeon;
        if (dungeonData != null)
        {
            GetText((int)Texts.TitleText).text = dungeonData.name;
            GetText((int)Texts.LevelText).text = "레벨 제한: " + mapData.dungeon.level;
            GetText((int)Texts.MaxPlayerText).text = "최대 인원: " + mapData.dungeon.maxPlayer;
            GetText((int)Texts.MonstersText).text = "몬스터: ";
            MonsterData monsterData = null;
            foreach (int monsterId in mapData.dungeon.monsters)
            {
                if (Managers.Data.MonsterDict.TryGetValue(monsterId, out monsterData))
                {
                    GetText((int)Texts.MonstersText).text += monsterData.name + " ";
                }
            }

            foreach (int itemId in dungeonData.rewards)
            {
                if (Managers.Data.ItemDict.TryGetValue(itemId, out Data.ItemData itemData))
                {
                    UI_ItemIcon itemIcon = Managers.Resource.Instantiate("UI/Scene/UI_ItemIcon", _content.transform).GetComponent<UI_ItemIcon>();
                    _itemIcons.Add(itemIcon);
                    ItemInfo itemInfo = new ItemInfo()
                    {
                        ItemDbId = 0,
                        TemplateId = itemId,
                        Count = 1,
                        Equipped = false
                    };
                    itemInfo.Options.AddRange(itemData.options);

                    Item item = Item.MakeItem(itemInfo);                    
                    itemIcon.SetItem(item);                                                           
                }
            }
        }    
    }
}
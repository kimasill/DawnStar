using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Matching : UI_Scene
{
    enum Buttons
    {
        EnterButton,
        MatchButton
    }

    enum Texts
    {
        StatusText,
        DungeonInfoText
    }

    enum GameObjects
    {
        MatchingSpinner,
        ItemIconScrollViewContent
    }

    private bool _isMatching = false;
    [SerializeField]
    private UI_ItemDescription _itemDescription;

    [SerializeField]
    private List<Image> _itemIcons = new List<Image>();

    public override void Init()
    {
        base.Init();

        Bind<Button>(typeof(Buttons));
        Bind<Text>(typeof(Texts));
        Bind<GameObject>(typeof(GameObjects));

        GetButton((int)Buttons.EnterButton).gameObject.BindEvent(OnClickEnterButton);
        GetButton((int)Buttons.MatchButton).gameObject.BindEvent(OnClickMatchButton);
    }

    private void OnClickEnterButton(PointerEventData evt)
    {
        if (_isMatching)
        {
            // 파티를 유지한 채로 입장 요청
            Debug.Log("입장 요청");
            // 서버에 입장 요청을 보내는 로직을 추가하세요.
        }
    }

    private void OnClickMatchButton(PointerEventData evt)
    {
        if (!_isMatching)
        {
            _isMatching = true;
            GetText((int)Texts.StatusText).text = "매칭 중";
            GetObject((int)GameObjects.MatchingSpinner).SetActive(true);

            // 서버에 매칭 요청을 보내는 로직을 추가하세요.
            StartCoroutine(SimulateMatching());
        }
    }

    private IEnumerator SimulateMatching()
    {
        // 매칭 성공을 시뮬레이션하기 위해 3초 대기
        yield return new WaitForSeconds(3f);

        _isMatching = false;
        GetText((int)Texts.StatusText).text = "매칭 성공";
        GetObject((int)GameObjects.MatchingSpinner).SetActive(false);

        // 매칭 성공 후 던전 정보를 업데이트
        UpdateDungeonInfo();
    }

    private void UpdateDungeonInfo()
    {
        // 던전 데이터를 가져오는 로직을 추가하세요.
        Data.DungeonData dungeonData = GetDungeonData();

        if (dungeonData != null)
        {
            // 던전 정보를 텍스트에 작성
            GetText((int)Texts.DungeonInfoText).text = "던전 정보: " + dungeonData.ToString();

            // 아이템 아이디 리스트를 읽어서 아이템 스프라이트를 추가
            foreach (int itemId in dungeonData.ItemIdList)
            {
                if (Managers.Data.ItemDict.TryGetValue(itemId, out Data.ItemData itemData))
                {
                    Sprite itemSprite = Managers.Resource.Load<Sprite>(itemData.iconPath);
                    if (itemSprite != null)
                    {
                        GameObject itemIcon = new GameObject("ItemIcon");
                        Image image = itemIcon.AddComponent<Image>();
                        image.sprite = itemSprite;

                        ItemIcon itemIconComponent = itemIcon.AddComponent<ItemIcon>();
                        itemIconComponent.SetItem(itemId);

                        itemIcon.transform.SetParent(GetObject((int)GameObjects.ItemIconScrollViewContent).transform, false);
                    }
                }
            }
        }
    }

    private Data.DungeonData GetDungeonData()
    {
        // 던전 데이터를 가져오는 로직을 구현하세요.
        return new Data.DungeonData();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 마우스 오버한 아이템 아이콘을 찾습니다.
        foreach (var icon in _itemIcons)
        {
            if (eventData.pointerEnter == icon.gameObject)
            {
                // UI_ItemDescription 객체를 생성하고 정보를 설정합니다.
                _itemDescription = Instantiate(_itemDescription, transform);
                _itemDescription.SetItem(icon.GetComponent<Item>());
                _itemDescription.gameObject.SetActive(true);
                break;
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 마우스가 아이템 아이콘에서 벗어났을 때 UI_ItemDescription 객체를 비활성화합니다.
        if (_itemDescription != null)
        {
            _itemDescription.gameObject.SetActive(false);
        }
    }
}
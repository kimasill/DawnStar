// UI_Map.cs

using Google.Protobuf.WellKnownTypes;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

public class UI_Map : UI_Base
{
    [SerializeField]
    private GameObject _viewPort;
    [SerializeField]
    private GameObject _playerIcon;
    [SerializeField]
    private Scrollbar _scrollbar;
    [SerializeField]
    private UI_Map_Icon _icon;

    private Vector2 _originalSize;
    private ScrollRect _scrollRect;
    private Vector2 _dragStartPosition;
    private Vector2 _contentStartPosition;
    private List<GameObject> _items;    
    private RectTransform _mapRectTransform;

    public RectTransform MapRectTransform
    {
        get
        {
            if (_mapRectTransform == null)
                _mapRectTransform = Get<Image>((int)Images.MapImage).rectTransform;
            return _mapRectTransform;
        }
    }

    enum Texts
    {
        ScrollPercentageText,
    }

    enum Images
    {
        MapImage,
    }


    public override void Init()
    {
        if (_playerIcon == null)
            _playerIcon = Managers.Resource.Instantiate("UI/UI_PlayerIcon", transform).gameObject;
                
        _scrollRect = GetComponentInChildren<ScrollRect>();

        if (_scrollRect == null)
        {
            Debug.LogError("ScrollRect component not found in children.");
            return;
        }

        _originalSize = _scrollRect.content.rect.size;        

        // Scrollbar 이벤트 핸들러 등록
        _scrollbar.onValueChanged.AddListener(OnScrollValueChanged);

        Bind<Image>(typeof(Images));
        Bind<TMP_Text>(typeof(Texts));
        _mapRectTransform = Get<Image>((int)Images.MapImage).rectTransform;

        if (_mapRectTransform == null)
        {
            Debug.LogError("Map RectTransform is not assigned.");
        }
        _items = _icon.GetComponentsInChildren<TMP_Text>().ToList().ConvertAll(item => item.gameObject);
        Image[] images = _scrollRect.content.GetComponentsInChildren<Image>();
        foreach (Image image in images) {
            _items.Add(image.gameObject);
        }
    }
    public void ResetMapSize()
    {
        if (_mapRectTransform != null)
        {
            _mapRectTransform.sizeDelta = _originalSize;
            foreach (var icon in _items)
            {
                icon.SetActive(true); // 아이콘 다시 활성화
            }
        }
        else
        {
            Debug.LogError("MapRectTransform is null.");
        }            
    }

    public void ZoomMap(float delta)
    {
        if (_scrollRect == null || _scrollRect.content == null)
            return;

        // ScrollView의 contentContainer를 사용하여 content의 크기를 조정
        Vector2 viewportSize = _scrollRect.viewport.rect.size;

        // 최대 크기는 원래 크기, 최소 크기는 뷰포트 크기와 일치할 때
        float newWidth = Mathf.Lerp(viewportSize.x, _originalSize.x, delta);
        float newHeight = Mathf.Lerp(viewportSize.y, _originalSize.y, delta);

        // 가로 세로 비율을 유지하도록 크기 조정
        float aspectRatio = _originalSize.x / _originalSize.y;
        if (newWidth / newHeight > aspectRatio)
        {
            newWidth = newHeight * aspectRatio;
        }
        else
        {
            newHeight = newWidth / aspectRatio;
        }

        Vector2 newSize = new Vector2(newWidth, newHeight);
        Vector2 sizeDelta = newSize - _scrollRect.content.sizeDelta;

        // 콘텐츠의 크기를 조정
        _scrollRect.content.sizeDelta = newSize;

        // 맵 크기가 100%를 넘어가면 아이콘 비활성화
        bool isZoomedIn = newSize.x > _originalSize.x || newSize.y > _originalSize.y;
        foreach (var icon in _items)
        {
            icon.SetActive(!isZoomedIn);
        }

        UpdatePlayerPosition();
        _icon.UpdateIcons();
        CenterPlayerIcon();
        GetTextMeshPro((int)Texts.ScrollPercentageText).text = $"{(int)(delta * 100 * 2)}%";
    }
    public void UpdatePlayerPosition()
    {
        if (_playerIcon == null || _scrollRect == null)
            return;

        // 플레이어의 현재 월드 위치를 가져옵니다.
        Vector3 playerWorldPosition = Managers.Object.MyPlayer.CellPos;

        // 월드 위치를 맵의 로컬 위치로 변환합니다.
        Vector2 playerMapPosition = WorldToMapPosition(playerWorldPosition);

        // 플레이어 아이콘의 위치를 업데이트합니다.
        _playerIcon.GetComponent<RectTransform>().anchoredPosition = playerMapPosition;
    }

    private Vector2 WorldToMapPosition(Vector3 worldPosition)
    {
        // 월드 좌표를 맵 좌표로 변환하는 로직을 구현합니다.
        // 예를 들어, 맵의 크기와 월드의 크기를 기준으로 비율을 계산할 수 있습니다.
        float mapWidth = _scrollRect.content.rect.width;
        float mapHeight = _scrollRect.content.rect.height;

        float worldWidth = Managers.Map.SizeX;
        float worldHeight = Managers.Map.SizeY;

        float x = (worldPosition.x - Managers.Map.MinX) / worldWidth * mapWidth;
        float y = (worldPosition.y - Managers.Map.MinY) / worldHeight * mapHeight;

        return new Vector2(x - mapWidth / 2, y - mapHeight / 2);
    }

    private void CenterPlayerIcon()
    {
        if (_playerIcon == null || _viewPort == null || _scrollRect == null)
        {
            Debug.LogError("PlayerIcon, ViewPort, or ScrollRect is not assigned.");
            return;
        }

        // 플레이어 아이콘의 현재 위치를 가져옴
        Vector3 playerIconPosition = _playerIcon.transform.localPosition;

        // 뷰포트와 콘텐츠의 크기를 가져옴
        Vector2 viewportSize = _scrollRect.GetComponent<RectTransform>().sizeDelta;
        Vector2 contentSize = _scrollRect.content.rect.size;

        // 플레이어 아이콘을 뷰포트의 중앙에 위치시키기 위해 콘텐츠의 오프셋을 계산
        Vector2 contentOffset = new Vector2(
            playerIconPosition.x + (contentSize.x/2) - viewportSize.x / 2,
            playerIconPosition.y + (contentSize.y/2) - viewportSize.y / 2
        );

        // 콘텐츠 이동량이 맵 바깥으로 나가지 않도록 조정
        contentOffset.x = -Mathf.Clamp(contentOffset.x, 0, contentSize.x - viewportSize.x);
        contentOffset.y = Mathf.Clamp(contentOffset.y, 0, contentSize.y - viewportSize.y);


        // ScrollView의 콘텐츠 이동
        _scrollRect.content.anchoredPosition = contentOffset;
    }

    private void OnScrollValueChanged(float value)
    {
        // Scrollbar 값에 따라 맵 확대/축소
        float zoomFactor = Mathf.Lerp(0.0f, 1.0f, value); // 0.5배에서 2배까지 확대/축소
        ZoomMap(zoomFactor);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 dragDelta = eventData.position - _dragStartPosition;
        Vector2 newContentPosition = _contentStartPosition - dragDelta;

        _scrollRect.content.anchoredPosition = newContentPosition;

        _scrollRect.horizontalNormalizedPosition = newContentPosition.x / (_scrollRect  .content.sizeDelta.x - _scrollRect.transform.localScale.x);
        _scrollRect.verticalNormalizedPosition = newContentPosition.y / (_scrollRect.content.sizeDelta.y - _scrollRect.transform.localScale.y);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 드래그 종료 시 추가 로직이 필요하면 여기에 작성
    }
    public void OnPointerEnter(PointerEventData data, GameObject icon)
    {
        int index = _items.IndexOf(icon);
        if (index >= 0 && index < _items.Count)
        {
            GameObject tooltip = _items[index];
            tooltip.SetActive(true);
            UpdateTooltip(icon, tooltip);
        }
    }

    public void OnPointerExit(PointerEventData data, GameObject icon)
    {
        int index = _items.IndexOf(icon);
        if (index >= 0 && index < _items.Count)
        {
            GameObject tooltip = _items[index];
            tooltip.SetActive(false);
        }
    }

    private void UpdateTooltip(GameObject icon, GameObject tooltip)
    {
        // 툴팁 업데이트 로직
    }

    public void OnCloseMap()
    {
        ResetMap();
        gameObject.SetActive(false); // 맵 UI 비활성화
    }

    public void OnOpenMap()
    {
        gameObject.SetActive(true); // 맵 UI 활성화
        UpdatePlayerPosition();
        CenterPlayerIcon();
    }

    public void ResetMap()
    {
        // 맵 크기 초기화
        _scrollRect.content.sizeDelta = _originalSize;

        // 아이콘 위치 초기화
        foreach (var icon in _items)
        {
            icon.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }

        // 스크롤바 초기화
        _scrollbar.value = 0.5f;

        // 텍스트 초기화
        GetTextMeshPro((int)Texts.ScrollPercentageText).text = "100%";
    }
}

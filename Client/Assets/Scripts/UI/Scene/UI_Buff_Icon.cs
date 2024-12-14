using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Buff_Icon : UI_Base
{
    Image _infoWindow;
    public string Key { get; set; }
    public float Value { get; set; }
    bool _isTriggered = false;
    enum Images
    {
        Buff_Icon_InfoWindow,
        Buff_Icon_Frame,
        Buff_Icon_Image
    }

    enum Texts
    {
        Buff_Icon_InfoWindow_Text_Key,
        Buff_Icon_InfoWindow_Text_Value
    }

    public void SetIcon(Sprite icon)
    {
        GetImage((int)Images.Buff_Icon_Image).sprite = icon;
    }

    public void SetFrame(bool isBuff)
    {
        if (!isBuff)
        {
            GetImage((int)Images.Buff_Icon_Frame).color = Color.red;
        }
    }

    public override void Init()
    {
        Bind<Image>(typeof(Images));
        Bind<TMP_Text>(typeof(Texts));

        gameObject.BindEvent(OnPointerEnter, Define.UIEvent.MouseOver);
        gameObject.BindEvent(OnPointerExit, Define.UIEvent.MouseOut);

        _infoWindow = GetImage((int)Images.Buff_Icon_InfoWindow);
        _infoWindow.gameObject.SetActive(false);
    }

    public override void OnPointerEnter(PointerEventData evt)
    {
        if (_isTriggered)
            return;
        _isTriggered = true;

        _infoWindow.gameObject.SetActive(true);
        GetTextMeshPro((int)Texts.Buff_Icon_InfoWindow_Text_Key).text = Key;
        GetTextMeshPro((int)Texts.Buff_Icon_InfoWindow_Text_Value).text = $"+{Value.ToString()}"; 
    }

    public override void OnPointerExit(PointerEventData evt)
    {
        if (!_isTriggered)
            return;
        _isTriggered = false;

        _infoWindow.gameObject.SetActive(false);
    }
}
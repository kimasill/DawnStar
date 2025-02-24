using Google.Protobuf.Protocol;
using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Menu : UI_Base
{
    enum Images
    {
        Menu_SoundButton,
        Menu_ExitButton,
        Menu_ScreenButton,
        Menu_Panel,
        BackWardButton
    }

    private GameObject CurrentPanel = null;
    private UI_SoundSettings SoundSettings;
    private UI_ScreenSettings ScreenSettings;
    private bool _isMenuOpen = false;
    private bool _isInit = false;
    public override void Init()
    {
        if (_isInit) return;
        Bind<Image>(typeof(Images));

        GetImage((int)Images.Menu_SoundButton).gameObject.BindEvent(OnClickSoundImage);
        GetImage((int)Images.Menu_ScreenButton).gameObject.BindEvent(OnClickScreenImage);
        GetImage((int)Images.Menu_ExitButton).gameObject.BindEvent(OnClickExitImage);
        GetImage((int)Images.BackWardButton).gameObject.BindEvent(OnClickBackwardImage, Define.UIEvent.Click);
        SoundSettings = GetComponentInChildren<UI_SoundSettings>();
        ScreenSettings = GetComponentInChildren<UI_ScreenSettings>();
        SoundSettings.gameObject.SetActive(false);
        ScreenSettings.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    public void ToggleMenu()
    {
        if(_isInit == false)
        {
            Init();
            _isInit = true;
        }
        _isMenuOpen = !_isMenuOpen;
        gameObject.SetActive(_isMenuOpen);
    }

    private void OnClickSoundImage(PointerEventData evt)
    {
        // 사운드 설정 창 열기        
        if (SoundSettings != null)
        {
            GetImage((int)Images.Menu_Panel).gameObject.SetActive(false);
            SoundSettings.gameObject.SetActive(true);
            CurrentPanel = SoundSettings.gameObject;
        }
    }

    private void OnClickScreenImage(PointerEventData evt)
    {
        if (ScreenSettings != null)
        {
            GetImage((int)Images.Menu_Panel).gameObject.SetActive(false);
            ScreenSettings.gameObject.SetActive(true);
            CurrentPanel = ScreenSettings.gameObject;
        }
    }

    private void OnClickExitImage(PointerEventData evt)
    {
        C_Quit quit = new C_Quit();
        Managers.Network.Send(quit);


        // 클라이언트 종료 처리
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnClickBackwardImage(PointerEventData evt)
    {
        Debug.Log("Backward");
        if (CurrentPanel != null)
        {
            CurrentPanel.SetActive(false);
            GetImage((int)Images.Menu_Panel).gameObject.SetActive(true);
            CurrentPanel = null;
        }
        else ToggleMenu();
    }
}
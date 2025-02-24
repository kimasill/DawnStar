using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ScreenSettings : UI_Base
{
    enum GameObjects
    {
        FullScreenToggle,
        WindowedToggle,
    }

    private Toggle _fullScreenToggle;
    private Toggle _windowedToggle;

    public override void Init()
    {
        Bind<GameObject>(typeof(GameObjects));

        _fullScreenToggle = GetObject((int)GameObjects.FullScreenToggle).GetComponent<Toggle>();
        _windowedToggle = GetObject((int)GameObjects.WindowedToggle).GetComponent<Toggle>();

        _fullScreenToggle.onValueChanged.AddListener(OnFullScreenToggleChanged);
        _windowedToggle.onValueChanged.AddListener(OnWindowedToggleChanged);

        // 초기 상태 설정
        _fullScreenToggle.isOn = Screen.fullScreen;
        _windowedToggle.isOn = !Screen.fullScreen;
    }

    private void OnFullScreenToggleChanged(bool isOn)
    {
        if (isOn)
        {
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            _windowedToggle.isOn = false;
        }
    }

    private void OnWindowedToggleChanged(bool isOn)
    {
        if (isOn)
        {
            Screen.fullScreenMode = FullScreenMode.Windowed;
            _fullScreenToggle.isOn = false;
        }
    }
}
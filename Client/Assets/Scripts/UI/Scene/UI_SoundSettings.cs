using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_SoundSettings : UI_Base
{
    enum GameObjects
    {
        SoundPanel_TotalSoundSlider,
        SoundPanel_BGMSoundSlider,
    }

    public override void Init()
    {
        Bind<GameObject>(typeof(GameObjects));
        Scrollbar totalSoundSlider = GetObject((int)GameObjects.SoundPanel_TotalSoundSlider).GetComponent<Scrollbar>();
        totalSoundSlider.onValueChanged.AddListener(OnTotalSoundSliderChanged);

        Scrollbar bgmSoundSlider = GetObject((int)GameObjects.SoundPanel_BGMSoundSlider).GetComponent<Scrollbar>();
        bgmSoundSlider.onValueChanged.AddListener(OnBGMSoundSliderChanged);
    }

    private void OnTotalSoundSliderChanged(float value)
    {
        Managers.Sound.SetTotalVolume(value);
    }

    private void OnBGMSoundSliderChanged(float value)
    {
        Managers.Sound.SetBGMVolume(value);
    }
}
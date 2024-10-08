using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Loading : UI_Popup
{
    enum Texts
    {
        LoadingText
    }
    enum Images
    {
        LoadingImage
    }

    public override void Init()
    {
        base.Init();
        Bind<TMP_Text>(typeof(Texts));
        Bind<Image>(typeof(Images));
    }

    public void SetLoadingText(string text)
    {
        GetTextMeshPro((int)Texts.LoadingText).text = text;
    }

    public void SetLoadingImage(string path)
    {
        GetImage((int)Images.LoadingImage).sprite = Managers.Resource.Load<Sprite>(path);
    }
}
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
        Bind<Text>(typeof(Texts));
        GetText((int)Texts.LoadingText).text = "Loading...";
    }

    public void SetLoadingText(string text)
    {
        GetText((int)Texts.LoadingText).text = text;
    }

    public void SetLoadingImage(string path)
    {
        GetImage((int)Images.LoadingImage).sprite = Managers.Resource.Load<Sprite>(path);
    }
}
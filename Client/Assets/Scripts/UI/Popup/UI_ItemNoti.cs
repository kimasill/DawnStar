using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ItemNoti : UI_Base
{
    enum Images
    {
        ItemImage
    }

    enum Texts
    {
        ItemNameText
    }
    CanvasGroup _canvasGroup;



    public override void Init()
    {
        Bind<Image>(typeof(Images));
        Bind<TMP_Text>(typeof(Texts));        
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        gameObject.SetActive(false);
    }
    public void ShowItemNoti(Sprite itemSprite, string itemName, int itemCount)
    {
        gameObject.SetActive(true);
        GetImage((int)Images.ItemImage).sprite = itemSprite;
        GetTextMeshPro((int)Texts.ItemNameText).text= $"{itemName} x {itemCount}";        
        StartCoroutine(HideItemNoti());
    }

    private IEnumerator HideItemNoti()
    {
        yield return new WaitForSeconds(5f); // 3초 후에 알림을 숨깁니다.
        float fadeDuration = 1f; // 서서히 사라지는 시간
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
            yield return null;
        }
        gameObject.SetActive(false);
    }
}
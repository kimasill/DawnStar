using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemTableController : InteractionController
{
    [SerializeField]
    public GameObject Item;

    protected override void Init()
    {
        base.Init();
        Item = GetComponent<GameObject>();
    }

    public override void Interact(bool success, bool action, List<int> ids=null)
    {
        if (success && action)
        {
            AfterInteract();
        }
    }

    private void AfterInteract()
    {
        _isInteracted = false;
        CanInteract = false;
        var items = Item.GetComponentsInChildren<GameObject>();
        foreach (var item in items)
        {
            item.SetActive(false);
        }
    }

    private IEnumerator FadeOutSprites(GameObject target, float duration)
    {
        float elapsedTime = 0f;
        SpriteRenderer[] spriteRenderers = target.GetComponentsInChildren<SpriteRenderer>();

        while (elapsedTime < duration)
        {
            float alpha = 1f - (elapsedTime / duration);

            foreach (var spriteRenderer in spriteRenderers)
            {
                Color itemColor = spriteRenderer.color;
                itemColor.a = alpha;
                spriteRenderer.color = itemColor;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        foreach (var spriteRenderer in spriteRenderers)
        {
            Color itemColor = spriteRenderer.color;
            itemColor.a = 0f;
            spriteRenderer.color = itemColor;
        }
    }

    public override void DeactivateInteraction()
    {
        base.DeactivateInteraction();
        Item.SetActive(false);
    }
}
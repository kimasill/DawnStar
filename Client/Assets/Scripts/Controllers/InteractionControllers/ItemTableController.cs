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
    }

    public override void Interact(bool success, bool action)
    {
        if (success && action)
        {
            StartCoroutine(CoInteract());
        }
    }

    private IEnumerator CoInteract()
    {
        float fadeDuration = 1f; // Fade out duration in seconds
        float elapsedTime = 0f; // Elapsed time since starting the fade

        // Get all SpriteRenderers in the Item object and its children
        SpriteRenderer[] spriteRenderers = Item.GetComponentsInChildren<SpriteRenderer>();

        while (elapsedTime < fadeDuration)
        {
            float alpha = 1f - (elapsedTime / fadeDuration); // Calculate the alpha value based on elapsed time

            // Apply the alpha value to all SpriteRenderers
            foreach (var spriteRenderer in spriteRenderers)
            {
                Color itemColor = spriteRenderer.color; // Get the current color of the sprite
                itemColor.a = alpha; // Set the alpha value of the color
                spriteRenderer.color = itemColor; // Apply the new color to the sprite
            }

            elapsedTime += Time.deltaTime; // Increase the elapsed time
            yield return null; // Wait for the next frame
        }

        // Ensure all sprites are fully transparent at the end of the fade
        foreach (var spriteRenderer in spriteRenderers)
        {
            Color itemColor = spriteRenderer.color;
            itemColor.a = 0f;
            spriteRenderer.color = itemColor;
        }

        _isInteracted = false;
        CanInteract = false;
    }

    public override void DeactivateInteraction()
    {
        base.DeactivateInteraction();
        Item.SetActive(false);
    }
}
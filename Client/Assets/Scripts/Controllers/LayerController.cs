using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LayerController : MonoBehaviour
{
    public Grid _grid;

    public void Start()
    {
        Init();
    }
    public void Init()
    {
        AdjustTilemapLayers();
    }

    void AdjustTilemapLayers()
    {
        if (_grid == null)
        {
            Debug.LogError("Grid is not assigned.");
            return;
        }

        // Grid 내의 모든 자식 오브젝트를 가져옴
        SpriteRenderer[] spriteRenderers = _grid.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            // sortingLayerName이 "Object"인 오브젝트만 정렬
            if (spriteRenderer.sortingLayerName == "Object")
            {
                Vector3Int cellPosition = _grid.WorldToCell(spriteRenderer.transform.position);
                spriteRenderer.sortingOrder += -cellPosition.y * 10;
            }
        }

        TilemapRenderer[] tilemapRenderers = _grid.GetComponentsInChildren<TilemapRenderer>();
        foreach (TilemapRenderer tilemapRenderer in tilemapRenderers)
        {
            if (tilemapRenderer.sortingLayerName == "Object")
            {
                Tilemap tilemap = tilemapRenderer.GetComponent<Tilemap>();
                if (tilemap != null)
                {
                    AdjustTilemapSorting(tilemap, tilemapRenderer);
                }
            }
        }
    }
    void AdjustTilemapSorting(Tilemap tilemap, TilemapRenderer tilemapRenderer)
    {
        BoundsInt bounds = tilemap.cellBounds;
        int order = -100000;
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (tilemap.HasTile(pos))
            {
                if (-pos.y > order)
                    order = -pos.y;
            }
        }
        tilemapRenderer.sortingOrder += order * 10;
    }
}
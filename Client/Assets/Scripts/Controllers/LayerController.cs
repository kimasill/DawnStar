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

        Tilemap[] tilemaps = _grid.GetComponentsInChildren<Tilemap>();
        foreach (Tilemap tilemap in tilemaps)
        {
            if (tilemap.gameObject.layer == LayerMask.NameToLayer("Block"))
            {
                foreach (var pos in tilemap.cellBounds.allPositionsWithin)
                {
                    if (tilemap.HasTile(pos))
                    {
                        // 타일을 개별 GameObject로 변환
                        GameObject tileGameObject = new GameObject("Tile_" + pos);
                        tileGameObject.transform.position = tilemap.CellToWorld(pos) + tilemap.tileAnchor;
                        tileGameObject.transform.SetParent(tilemap.transform);

                        // SpriteRenderer 추가 및 설정
                        SpriteRenderer spriteRenderer = tileGameObject.AddComponent<SpriteRenderer>();
                        spriteRenderer.sprite = tilemap.GetSprite(pos);
                        spriteRenderer.sortingOrder = -pos.y;
                    }
                }
            }
        }
    }
}
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
        _grid = GetComponent<Grid>();
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
                    AdjustTilemapSorting(tilemap, tilemapRenderer.sortingOrder);
                }
                tilemapRenderer.enabled = false;
            }
        }
    }
    void AdjustTilemapSorting(Tilemap tilemap, int indSortingOrder)
    {
        BoundsInt bounds = tilemap.cellBounds;
        Dictionary<int, List<Vector3Int>> ySortedTiles = new Dictionary<int, List<Vector3Int>>();

        // 타일을 y축 기준으로 그룹화
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (tilemap.HasTile(pos))
            {
                if (!ySortedTiles.ContainsKey(pos.y))
                {
                    ySortedTiles[pos.y] = new List<Vector3Int>();
                }
                ySortedTiles[pos.y].Add(pos);
            }
        }

        // y축 기준으로 그룹화된 타일들을 처리
        foreach (var kvp in ySortedTiles)
        {
            int y = kvp.Key;
            List<Vector3Int> positions = kvp.Value;

            GameObject tileGroupObject = new GameObject($"TileGroup_{y}");
            tileGroupObject.transform.parent = tilemap.transform;
            tileGroupObject.transform.localPosition = tilemap.transform.localPosition;

            foreach (Vector3Int pos in positions)
            {
                TileBase tile = tilemap.GetTile(pos);
                if (tile != null)
                {
                    GameObject tileObject = new GameObject($"Tile_{pos.x}_{pos.y}");
                    tileObject.transform.parent = tileGroupObject.transform;
                    tileObject.transform.localPosition = tilemap.CellToLocalInterpolated(pos + tilemap.tileAnchor);
                    SpriteRenderer spriteRenderer = tileObject.AddComponent<SpriteRenderer>();
                    spriteRenderer.sprite = tilemap.GetSprite(pos);
                    //Vector3Int cellPosition = _grid.WorldToCell(spriteRenderer.transform.position);

                    spriteRenderer.sortingOrder = -pos.y * 10 + indSortingOrder;
                    spriteRenderer.sortingLayerName = tilemap.GetComponent<TilemapRenderer>().sortingLayerName;
                }
            }
        }
    }

    Texture2D duplicateTexture(Texture2D source)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(
            source.width,
            source.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear);

        Graphics.Blit(source, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        Texture2D readableText = new Texture2D(source.width, source.height);
        readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        return readableText;
    }
}

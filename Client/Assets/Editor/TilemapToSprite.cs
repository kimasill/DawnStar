using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapToSprite : MonoBehaviour
{
    public Tilemap tilemap;
    public TilemapRenderer tilemapRenderer;
    public SpriteRenderer spriteRendererPrefab; // Prefab으로 SpriteRenderer를 받습니다.
    public int sortingOrderOffset = 10; // Sorting Order 간격 설정
    public string spriteSavePath = "";

    public void Generate()
    {
        // 타일맵의 바운딩 박스를 가져옵니다.
        BoundsInt bounds = tilemap.cellBounds;

        int tileWidth = (int)tilemap.cellSize.x;
        int tileHeight = (int)tilemap.cellSize.y;

        // y축 기준으로 타일들을 묶습니다.
        for (int y = 0; y < bounds.size.y; y++)
        {
            // 텍스처를 생성합니다.
            Texture2D texture = new Texture2D(bounds.size.x * tileWidth, tileHeight); // 타일 크기 16x16 가정

            for (int x = 0; x < bounds.size.x; x++)
            {
                // 타일의 스프라이트를 가져옵니다.
                TileBase tileBase = tilemap.GetTile(new Vector3Int(x, y, 0)); // TileBase로 가져옵니다.
                if (tileBase != null && tileBase is Tile tile) // tileBase가 Tile 타입인지 확인합니다.
                {
                    Sprite tileSprite = tile.sprite; // Tile.sprite를 사용합니다.
                    if (tileSprite != null && tileSprite.texture != null) // tileSprite와 tileSprite.texture가 null인지 확인합니다.
                    {
                        // 스프라이트의 픽셀 데이터를 텍스처에 복사합니다.
                        Color[] pixels = tileSprite.texture.GetPixels();
                        texture.SetPixels(x * tileWidth, 0, tileWidth, tileHeight, pixels);
                    }
                }
            }
            texture.Apply();

            // 텍스처를 스프라이트로 변환합니다.
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            string spriteName = $"{tilemap.gameObject.name}_{y}.png";

            GameObject tilemapObject = GameObject.Find(tilemap.gameObject.name);
            if (tilemapObject == null)
            {
                tilemapObject = new GameObject(tilemap.gameObject.name);
            }
            SpriteRenderer spriteRenderer = Instantiate(spriteRendererPrefab, tilemapObject.transform);
            spriteRenderer.sprite = sprite;
            spriteRenderer.sortingOrder = -y * sortingOrderOffset + tilemapRenderer.sortingOrder;

            // SpriteRenderer의 위치를 설정합니다.
            spriteRenderer.transform.position = new Vector3(bounds.xMin + bounds.size.x / 2f, bounds.yMin + y + 0.5f, 0);
        }

        // 기존 Tilemap을 비활성화합니다.
        tilemap.gameObject.SetActive(false);
    }
}

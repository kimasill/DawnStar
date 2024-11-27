#if UNITY_EDITOR
using NUnit;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SpriteObjectGenerator : MonoBehaviour
{
    [MenuItem("Tools/TilemapToSprite/Generate Map")]
    private static void ApplyTilemapToSprite()
    {
        // 맵 프리펩 경로
        string mapPrefabPath = "Prefabs/Map/";        
        string spriteSavePath = "Assets/Resources/Textures/Sprites/Maps/"; // Sprite 저장 경로
        // 타일맵 이름
        
        
        
        GameObject[] gameObjects = Resources.LoadAll<GameObject>(mapPrefabPath);
        foreach (GameObject mapPrefab in gameObjects)
        {
            if (mapPrefab == null)
            {
                Debug.LogError("맵 프리펩을 찾을 수 없습니다.");
                return;
            }

            // TilemapToSprite 컴포넌트 추가
            TilemapToSprite tilemapToSprite = mapPrefab.AddComponent<TilemapToSprite>();

            // 타일맵 설정
            // Tilemap_Env 게임 오브젝트 찾기
            Transform tilemapEnvTransform = mapPrefab.transform.Find("Tilemap_Env");
            if (tilemapEnvTransform == null)
            {
                Debug.LogError("Tilemap_Env 게임 오브젝트를 찾을 수 없습니다.");
                return;
            }
            Tilemap[] tilemaps = tilemapEnvTransform.GetComponentsInChildren<Tilemap>();
            if (tilemaps.Length == 0)
            {
                Debug.LogError("Tilemap_Env 하위에 타일맵이 없습니다.");
                return;
            }
            foreach (Tilemap tilemap in tilemaps)
            {
                tilemapToSprite.tilemap = tilemap;
                tilemapToSprite.tilemapRenderer = tilemap.GetComponent<TilemapRenderer>();
                if (tilemapToSprite.tilemap == null)
                {
                    Debug.LogError("타일맵을 찾을 수 없습니다.");
                    return;
                }

                // SpriteRenderer Prefab 설정
                tilemapToSprite.spriteRendererPrefab = Resources.Load<SpriteRenderer>("Prefabs/Sprite/SpriteRenderer");

                // Sprite 저장 경로 설정
                tilemapToSprite.spriteSavePath = spriteSavePath;
                tilemapToSprite.Generate();
            }
            // 맵 프리펩 저장
            string prefabName = $"{mapPrefab.name}";
            PrefabUtility.SaveAsPrefabAsset(mapPrefab, mapPrefabPath + prefabName + "_Gen.prefab");
        }
    }
}
#endif
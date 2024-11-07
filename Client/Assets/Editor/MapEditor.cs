using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;
using Unity.VisualScripting;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MapEditor : MonoBehaviour
{
#if UNITY_EDITOR

    // % (Ctrl), # (Shift), & (Alt)

    [MenuItem("Tools/GenerateMap %#g")]
    private static void GenerateMap()
    {
        GenerateByPath("Assets/Resources/Map");
        GenerateByPath("../Common/MapData");
        GenerateSpawnPointByPath("Assets/Resources/Map");
        GenerateSpawnPointByPath("../Common/MapData");
    }

    private static void GenerateByPath(string pathPrefix)
    {
        GameObject[] gameObjects = Resources.LoadAll<GameObject>("Prefabs/Map");

        foreach (GameObject go in gameObjects)
        {
            Tilemap tmBase = Util.FindChild<Tilemap>(go, "Tilemap_Base", true);
            Tilemap tm = Util.FindChild<Tilemap>(go, "Tilemap_Collision", true);

            using (var writer = File.CreateText($"{pathPrefix}/{go.name}.txt"))
            {
                writer.WriteLine(tmBase.cellBounds.xMin);
                writer.WriteLine(tmBase.cellBounds.xMax);
                writer.WriteLine(tmBase.cellBounds.yMin);
                writer.WriteLine(tmBase.cellBounds.yMax);

                for (int y = tmBase.cellBounds.yMax; y >= tmBase.cellBounds.yMin; y--)
                {
                    for (int x = tmBase.cellBounds.xMin; x <= tmBase.cellBounds.xMax; x++)
                    {
                        TileBase tile = tm.GetTile(new Vector3Int(x, y, 0));
                        if (tile != null)
                            writer.Write("1");
                        else
                            writer.Write("0");
                    }
                    writer.WriteLine();
                }
            }
        }
    }

    private static void GenerateSpawnPointByPath(string pathPrefix)
    {
        GameObject[] gameObjects = Resources.LoadAll<GameObject>("Prefabs/Map");
        foreach (var go in gameObjects)
        {
            GameObject spawnPoint = go.transform.Find("SpawnPoint")?.gameObject;
            if (spawnPoint == null)
            {
                continue;
            }
            Transform spawnPointTransform = spawnPoint.transform;

            List<Tilemap> monsterTilemaps = new List<Tilemap>();
            Tilemap tmBase = Util.FindChild<Tilemap>(go, "Tilemap_Base", true);

            foreach (Transform child in spawnPointTransform)
            {
                Tilemap tilemap = child.GetComponent<Tilemap>();
                if (tilemap != null)
                {
                    monsterTilemaps.Add(tilemap);
                }
            }

            if (monsterTilemaps.Count == 0)
            {
                Debug.LogError("SpawnPoint 아래에 타일맵이 없습니다.");
                return;
            }

            // tmBase의 경계를 사용하여 맵 데이터 초기화
            BoundsInt bounds = tmBase.cellBounds;
            int width = bounds.xMax - bounds.xMin + 1;
            int height = bounds.yMax - bounds.yMin + 1;
            string[,] mapData = new string[width, height];

            // 모든 위치를 0으로 초기화
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    mapData[x, y] = "0";
                }
            }
            foreach (Tilemap tilemap in monsterTilemaps)
            {
                foreach (Vector3Int pos in tilemap.cellBounds.allPositionsWithin)
                {
                    TileBase tile = tilemap.GetTile(pos);
                    if (tile != null)
                    {
                        string tileName = tilemap.gameObject.name;
                        if (tileName.StartsWith("Monster_"))
                        {
                            int monsterId = int.Parse(tileName.Substring(8));
                            Vector3Int cellPosition = new Vector3Int(
                                pos.x - bounds.xMin,
                                pos.y - bounds.yMin,
                                0
                            );
                            mapData[cellPosition.x, cellPosition.y] = monsterId.ToString();
                            Debug.Log($"MonsterId: {monsterId}, Position: {cellPosition}");
                        }
                    }
                }
            }

            string fileName = $"{go.name}_SpawnPoints";
            SaveMapData(pathPrefix, fileName, mapData, bounds);
        }
    }
    private static void SaveMapData(string pathPrefix, string fileName, string[,] mapData, BoundsInt bounds)
    {
        string filePath = Path.Combine(pathPrefix, $"{fileName}.txt");

        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine(bounds.xMin);
            writer.WriteLine(bounds.xMax);
            writer.WriteLine(bounds.yMin);
            writer.WriteLine(bounds.yMax);

            for (int y = bounds.yMax; y >= bounds.yMin; y--)
            {
                for (int x = bounds.xMin; x <= bounds.xMax; x++)
                {
                    writer.Write(mapData[x - bounds.xMin, y - bounds.yMin]);
                    if (x < bounds.xMax)
                    {
                        writer.Write(","); // 쉼표를 구분자로 사용
                    }
                }
                writer.WriteLine();
            }
        }

        Debug.Log($"맵 데이터가 저장되었습니다: {filePath}");
    }

    private void OnDisable()
    {
        // 리소스 정리 코드 추가
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
    }

#endif
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;
using Codice.Client.BaseCommands;




#if UNITY_EDITOR
using UnityEditor;
#endif
public class MapEditor : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Tools/GenereateMap %#m")]
    private static void GenerateMap()
    {
        GameObject[] gameObjects = Resources.LoadAll<GameObject>("Prefabs/Map");
        foreach (GameObject go in gameObjects)
        {
            Tilemap tilemapBase = Util.FindChild<Tilemap>(go, "Tilemap_Base", true);
            Tilemap tilemap = Util.FindChild<Tilemap>(go, "Tilemap_Collision", true);            

            using (var writer = File.CreateText($"Assets/Resources/Map/{go.name}.txt"))
            {
                writer.WriteLine(tilemapBase.cellBounds.xMin);
                writer.WriteLine(tilemapBase.cellBounds.xMax);
                writer.WriteLine(tilemapBase.cellBounds.yMin);
                writer.WriteLine(tilemapBase.cellBounds.yMax);

                for (int y = tilemapBase.cellBounds.yMax; y >= tilemapBase.cellBounds.yMin; y--)
                {
                    for (int x = tilemapBase.cellBounds.xMin; x <= tilemapBase.cellBounds.xMax; x++)
                    {
                        TileBase tile = tilemap.GetTile(new Vector3Int(x, y, 0));
                        if (tile != null)
                        {
                            writer.Write("1");
                        }
                        else
                        {
                            writer.Write("0");
                        }
                    }
                    writer.WriteLine();
                }
            }
        }
        //리스트에 넣는 방식
        //List<Vector3Int> blocked = new List<Vector3Int>();

        //foreach (Vector3Int position in tilemap.cellBounds.allPositionsWithin)
        //{
        //    if (tilemap.HasTile(position))
        //    {
        //        blocked.Add(position);
        //    }
        //}
    }
#endif
}

using Server.Game.Room;
using System.Collections.Generic;

namespace Server.Game
{
    public class SkillLogic
    {
        public static List<Vector2Int> GetBentAttackTiles(Vector2Int center, Vector2Int lookDir, int range)
        {
            List<Vector2Int> tiles = new List<Vector2Int>();
            int nX = lookDir.x != 0 ? (lookDir.x > 0 ? 1 : -1) : 0;
            int nY = center.y != 0 ? (center.y > 0 ? 1 : -1) : 0;
            for (int x = 0; x < range; x++)
            {
                for (int y = 0; y < range; y++)
                {
                    Vector2Int tile = new Vector2Int(x, y);
                    if (nY > 0 || nY < 0)
                    {
                        tile = new Vector2Int(center.x + x * nX, center.y + y * -nY);
                    }
                    else
                        tile = new Vector2Int(center.x + x * -nX, center.y);
                    tiles.Add(tile);
                }
            }
            return tiles;
        }
    }
}

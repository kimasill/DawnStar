using Google.Protobuf.Protocol;
using Server.Data;
using Server.Game.Room;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Server.Game
{
    public class SkillLogic
    {
        public static List<Vector2Int> GetBentAttackTiles(Vector2Int center, LookDir lookDir, int range)
        {
            List<Vector2Int> tiles = new List<Vector2Int>();
            int nX = 0;            
            if (lookDir == LookDir.LookLeft)
            {
                nX = -1;
            }
            else if (lookDir == LookDir.LookRight)
            {
                nX = 1;
            }


            for (int x = 0; x <= range; x++)
            {
                for (int y = -range/2; y <= range/2; y++)
                {
                    Vector2Int tile = new Vector2Int(center.x + x * nX, center.y + y);
                    tiles.Add(tile);
                }
            }

            return tiles;
        }

        public static List<Vector2Int> GetRandomSpots(GameObject user, SkillData skillData, GameRoom room)
        {
            Random rand = new Random();
            List<Vector2Int> skillPos = new List<Vector2Int>();

            int count = rand.Next(skillData.spot.minCount, skillData.spot.maxCount + 1);

            for (int i = 0; i < count; i++)
            {
                Vector2Int pos = new Vector2Int(rand.Next((int)-skillData.spot.range, (int)(skillData.spot.range + 1)), rand.Next((int)-skillData.spot.range, (int)(skillData.spot.range + 1)));
                if (room.Map.CanGo(pos, false))
                    skillPos.Add(user.CellPos + pos);
                else
                    i--;
            }
            return skillPos;
        }
    }
}

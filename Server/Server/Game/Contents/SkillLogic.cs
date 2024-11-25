using Google.Protobuf.Protocol;
using Server.Data;
using Server.Game.Room;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Server.Game
{
    public class SkillLogic
    {
        public static List<Vector2Int> GetBentAttackTiles(Vector2Int center, LookDir lookDir, int range)
        {
            List<Vector2Int> tiles = new List<Vector2Int>();

            // 스킬 범위 내의 타일 계산
            for (int x = 0; x < range; x++)
            {
                for(int y = -range/2; y < range/2 + 1; y++)
                {
                    Vector2Int xOffset = GetOffsetByDirection(lookDir, x);
                    Vector2Int yOffset = new Vector2Int(0, y);
                    Vector2Int tile = center + xOffset + yOffset;
                    tiles.Add(tile);                    
                }
            }

            return tiles;
        }
        public static List<Vector2Int> GetRectAttackTiles(Vector2Int center, MoveDir moveDir, int range)
        {
            List<Vector2Int> tiles = new List<Vector2Int>();
            Vector2Int start = center;

            // 사각형의 중심을 이동
            if (moveDir == MoveDir.Left || moveDir == MoveDir.Right)
            {
                start.x -= range / 2;
            }
            else if (moveDir == MoveDir.Up || moveDir == MoveDir.Down)
            {
                start.y -= range / 2;
            }

            // 사각형 범위 내의 타일 계산
            for (int x = 0; x < range; x++)
            {
                for (int y = 0; y < range; y++)
                {
                    Vector2Int tile = start + new Vector2Int(x, y);
                    tiles.Add(tile);
                }
            }

            return tiles;
        }
        private static Vector2Int GetOffsetByDirection(LookDir lookDir, int distance)
        {
            switch (lookDir)
            {
                case LookDir.LookLeft:
                    return new Vector2Int(-distance, 0);
                case LookDir.LookRight:
                    return new Vector2Int(distance, 0);
                default:
                    return new Vector2Int(0, 0);
            }
        }
        private static Vector2Int GetOffsetByDirection(MoveDir moveDir, int distance)
        {
            switch (moveDir)
            {
                case MoveDir.Up:
                    return new Vector2Int(0, distance);
                case MoveDir.Down:
                    return new Vector2Int(0, -distance);
                case MoveDir.Left:
                    return new Vector2Int(-distance, 0);
                case MoveDir.Right:
                    return new Vector2Int(distance, 0);
                default:
                    return new Vector2Int(0, 0);
            }
        }

        public static List<Vector2Int> GetRandomSpots(GameObject user, SkillData skillData, GameRoom room)
        {
            Random rand = new Random();
            List<Vector2Int> skillPos = new List<Vector2Int>();

            int count = rand.Next(skillData.spot.minCount, skillData.spot.maxCount + 1);

            for (int i = 0; i < count; i++)
            {
                Vector2Int pos = new Vector2Int(
                    rand.Next(-skillData.range, skillData.range + 1),
                    rand.Next(-skillData.range, skillData.range + 1)
                );
                pos += user.CellPos;
                skillPos.Add(pos);
            }
            return skillPos;
        }
        public static List<Vector2Int> GetAllTargetsInRange(Vector2Int center, int range)
        {
            List<Vector2Int> tiles = new List<Vector2Int>();

            for (int x = -range/2; x < range/2 + 1; x++)
            {
                for (int y = -range / 2; y < range /2 + 1; y++)
                {
                    Vector2Int tile = center + new Vector2Int(x, y);
                    tiles.Add(tile);
                }
            }

            return tiles;
        }

        public static List<Vector2Int> GetTargetsInLine(Vector2Int center, MoveDir dir, int range)
        {

            List<Vector2Int> tiles = new List<Vector2Int>();

            for (int i = 0; i < range; i++)
            {
                Vector2Int offset = GetOffsetByDirection(dir, i);
                Vector2Int tile = center + offset;
                tiles.Add(tile);
            }

            return tiles;
        }
    }
}

using Google.Protobuf.Protocol;
using Server.Data;
using Server.Game.Room;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game
{
    public partial class Skill
    {
        private void MoveSkill(SkillData data, GameObject target = null)
        {
            if (data.shape == null)
                return;

            Vector2Int dir = GetDir(data.shape.direction);
            Vector2Int destPos = new Vector2Int();
            if (target != null)
            {
                if (data.shape.shapeType == ShapeType.ShapeLine)
                {
                    destPos = target.CellPos - dir;
                }
            }
            else
            {
                if (data.shape.shapeType == ShapeType.ShapeLine)
                {
                    destPos = Owner.CellPos + new Vector2Int(dir.x * data.range, dir.y * data.range);

                    if (!Owner.Room.Map.CanGo(destPos))
                    {
                        Vector2Int currentPos = Owner.CellPos;
                        while (!Owner.Room.Map.CanGo(currentPos))
                        {
                            currentPos -= dir;
                        }
                        destPos = currentPos;
                    }
                }
            }
            if (Owner.Room.Map.ApplyMove(Owner, destPos))
            {
                Owner.CellPos = destPos;
                S_ChangePosition movePacket = new S_ChangePosition();
                movePacket.ObjectId = Owner.Id;
                movePacket.Position = Owner.PosInfo;
                Owner.Room.Broadcast(Owner.CellPos, movePacket);
            }
        }

    }
}

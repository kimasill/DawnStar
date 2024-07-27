using Google.Protobuf.Protocol;
using Server.Game.Room;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game
{
    public class Arrow : Projectile
    {
        public GameObject Owner { get; set; }

        long _nextMoveTick = 0;
        public override void Update()
        {
            if (Data == null || Data.projectile == null || Owner == null || Room == null)
                return;
            if(_nextMoveTick >= Environment.TickCount64)            
                return;

            long tick = (long)(1000 / Data.projectile.speed); //1초에 몇번 이동할 것인가
            _nextMoveTick = Environment.TickCount64 + tick;

            Vector2Int destPos = GetFrontCellPos();
            if (Room.Map.CanGo(destPos))
            {
                CellPos = destPos;
                S_Move movePacket = new S_Move();
                movePacket.ObjectId = Id;
                movePacket.Position = PosInfo;
                Room.BroadCast(movePacket);
                Console.WriteLine("move arrow");
            }
            else
            {
                GameObject target = Room.Map.Find(destPos);
                if(target != null)
                {
                    target.OnDamaged(this, Data.damage + Owner.Stat.Attack); //피격판정                    
                }
                Room.LeaveGame(Id);
            }
        }
    }
}

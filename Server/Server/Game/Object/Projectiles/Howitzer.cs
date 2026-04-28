using Google.Protobuf.Protocol;
using Server.Game.Room;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game
{
    public class Howitzer : Magic
    {
        public GameObject Owner { get; set; }
        GameObject attacker = null;
        public Vector2Int DestPos { get; set; }
        GameRoom room = null;
        private bool _scheduled = false;
        private int _travelMs = 0;
        private const int MoveBroadcastDelayMs = 100;

        public override void Update()
        {
            if (room != null && IsComplete)
            {
                room.Enqueue(room.LeaveGame, Id);
                return;
            }
            if (Data == null || Data.spot == null || Data.projectile == null || Room == null)
                return;

            room = Room;
            attacker = Owner ?? attacker;

            // Prevent double-scheduling; this object is updated once on spawn in EnterGame.
            if (_scheduled)
                return;
            _scheduled = true;

            Vector2Int dir = DestPos - CellPos;
            int dist = dir.cellDistanceFromZero;

            int tick = (int)(1000 / Data.projectile.speed);
            long travelMsLong = (long)tick * dist;
            if (travelMsLong < 0)
                travelMsLong = 0;
            if (travelMsLong > int.MaxValue)
                travelMsLong = int.MaxValue;
            _travelMs = (int)travelMsLong;

            int castDelayMs = Math.Max(_travelMs, MoveBroadcastDelayMs + 1);

            room.EnqueueAfter(MoveBroadcastDelayMs, MoveToDestIfAlive);
            room.EnqueueAfter(castDelayMs, Cast);

            // Failsafe: if Cast never runs (queue hiccup), ensure cleanup.
            room.EnqueueAfter(castDelayMs + 3000, ForceDespawnIfStuck);
        }

        private void MoveToDestIfAlive()
        {
            if (room == null || Room == null || IsComplete)
                return;

            // If it was already removed from room, don't re-apply movement (prevents resurrection).
            if (Room != room)
                return;

            State = CreatureState.Moving;
            if (room.Map.ApplyMove(this, DestPos, checkObjects: false))
            {
                CellPos = DestPos;
                S_Move movePacket = new S_Move();
                movePacket.ObjectId = Id;
                movePacket.Position = PosInfo;
                room.Broadcast(CellPos, movePacket);
            }
        }

        private void ForceDespawnIfStuck()
        {
            if (room == null || Room == null)
                return;
            if (IsComplete)
                return;

            DespawnAnim = false;
            IsComplete = true;
            room.Enqueue(room.LeaveGame, Id);
        }
        public void Cast()
        {
            if (IsComplete)
                return;

            var activeRoom = room ?? Room;
            if (activeRoom == null)
            {
                IsComplete = true;
                return;
            }

            attacker ??= Owner;
            attacker ??= this;

            try
            {
                List<Vector2Int> targetPositions = new List<Vector2Int>();
                if (Data.shape != null)
                {
                    targetPositions = SkillLogic.GetAllTargetsInRange(CellPos, (int)Data.shape.range);
                }
                else
                {
                    targetPositions.Add(CellPos);
                }

                foreach (Vector2Int pos in targetPositions)
                {
                    List<GameObject> cellObjects = activeRoom.Map.Find(pos);
                    if (cellObjects == null)
                        continue;

                    List<GameObject> targets = new List<GameObject>(cellObjects);
                    foreach (GameObject target in targets)
                    {
                        if (target == null || target == this || target == attacker)
                            continue;
                        if (!target.IsSkillTargetable)
                            continue;
                        if (attacker is Monster && target is Monster)
                            continue;
                        if (attacker is Player && target is Player)
                            continue;

                        target.OnDamaged(this, Data.damage + attacker.TotalAttack); // 피격 판정
                        OnHit?.Invoke(target);
                    }
                }
            }
            finally
            {
                DespawnAnim = true;
                IsComplete = true;
                if (Room != null)
                    activeRoom.Enqueue(activeRoom.LeaveGame, Id);
            }
        }

        public override GameObject GetOwner()
        {
            return Owner ?? this;
        }
    }
}

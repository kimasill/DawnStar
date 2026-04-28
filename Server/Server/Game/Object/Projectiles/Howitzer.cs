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
        int _moveRange = 0;
        GameRoom room = null;
        private bool _scheduled = false;
        private int _travelMs = 0;
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

            // Land at DestPos exactly when Cast triggers to avoid "ghost" shells.
            room.EnqueueAfter(_travelMs, MoveToDestIfAlive);
            room.EnqueueAfter(_travelMs, Cast);

            // Failsafe: if Cast never runs (queue hiccup), ensure cleanup.
            room.EnqueueAfter(_travelMs + 3000, ForceDespawnIfStuck);
        }

        private void MoveToDestIfAlive()
        {
            if (room == null || Room == null || IsComplete)
                return;

            // If it was already removed from room, don't re-apply movement (prevents resurrection).
            if (Room != room)
                return;

            State = CreatureState.Moving;
            if (room.Map.ApplyMove(this, DestPos))
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
                List<GameObject> targets = new List<GameObject>(activeRoom.Map.Find(pos));
                if (targets.Count > 0)
                {
                    foreach (GameObject target in targets)
                    {
                        if (target == attacker)
                            continue;
                        if (target != null && target.IsSkillTargetable)
                        {
                            target.OnDamaged(this, Data.damage + attacker.TotalAttack); // 피격 판정
                            OnHit?.Invoke(target);                  
                        }
                    }
                }
            }
            DespawnAnim = true;
            activeRoom.Enqueue(activeRoom.LeaveGame, Id);
            IsComplete = true;
        }

        public override GameObject GetOwner()
        {
            return Owner;
        }
    }
}
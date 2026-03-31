using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Game.Room
{
    public class InterestManagement
    {
        public Player Owner { get; private set; }
        public HashSet<GameObject> PreviousObjects { get; private set; } = new HashSet<GameObject>();

        public InterestManagement(Player owner)
        {
            Owner = owner;
        }

        public void Refresh()
        {
            PreviousObjects.Clear();
        }

        public HashSet<GameObject> GatherObjects()
        {
            if (Owner == null || Owner.Room == null)
            {
                return null;
            }

            HashSet<GameObject> objects = new HashSet<GameObject>();

            Vector2Int cellPos = Owner.CellPos;
            List<Zone> zones = Owner.Room.GetAdjacentZone(cellPos);

            foreach (Zone zone in zones)
            {
                if (zone == null)
                    continue;

                foreach (Player player in zone.Players)
                {
                    if (player == null)
                        continue;
                    int dx = player.CellPos.x - cellPos.x;
                    int dy = player.CellPos.y - cellPos.y;
                    if (Math.Abs(dx) > GameRoom.VisionCells || Math.Abs(dy) > GameRoom.VisionCells)
                    {
                        continue;
                    }
                    objects.Add(player);
                }

                foreach (Monster monster in zone.Monsters)
                {
                    if (monster == null)
                        continue;
                    int dx = monster.CellPos.x - cellPos.x;
                    int dy = monster.CellPos.y - cellPos.y;
                    if (Math.Abs(dx) > GameRoom.VisionCells || Math.Abs(dy) > GameRoom.VisionCells)
                    {
                        continue;
                    }
                    objects.Add(monster);
                }

                foreach (Projectile projectile in zone.Projectiles)
                {
                    if (projectile == null)
                        continue;
                    int dx = projectile.CellPos.x - cellPos.x;
                    int dy = projectile.CellPos.y - cellPos.y;
                    if (Math.Abs(dx) > GameRoom.VisionCells || Math.Abs(dy) > GameRoom.VisionCells)
                    {
                        continue;
                    }
                    objects.Add(projectile);
                }

                foreach (Magic magic in zone.Magics)
                {
                    if (magic == null)
                        continue;
                    int dx = magic.CellPos.x - cellPos.x;
                    int dy = magic.CellPos.y - cellPos.y;
                    if (Math.Abs(dx) > GameRoom.VisionCells || Math.Abs(dy) > GameRoom.VisionCells)
                    {
                        continue;
                    }
                    objects.Add(magic);
                }
            }
            return objects;
        }

        public void Update()
        {
            if (Owner == null || Owner.Room == null)
            {
                return;
            }

            HashSet<GameObject> currentObjects = GatherObjects();

            List<GameObject> added = currentObjects.Except(PreviousObjects).ToList();
            if (added.Count > 0)
            {
                S_Spawn spawnPacket = new S_Spawn();
                foreach (GameObject obj in added)
                {
                    if (obj == Owner)
                    {
                        continue;
                    }
                    else if (obj.ObjectType == GameObjectType.Player)
                    {
                        Player player = obj as Player;
                        if (player.Session == null)
                        {
                            continue;
                        }
                        // 프로젝트: 시야에 잡힌 타 플레이어 장비 정보를 룸 큐에서 지연 동기화
                        Owner.Room.EnqueueAfter(100, Owner.Room.HandleEquippedItemList, Owner, player, true);
                    }
                    ObjectInfo info = new ObjectInfo();
                    info.MergeFrom(obj.Info);
                    spawnPacket.Objects.Add(info);
                }
                Owner.Session.Send(spawnPacket);
            }

            List<GameObject> removed = PreviousObjects.Except(currentObjects).ToList();
            if (removed.Count > 0)
            {
                S_Despawn despawnPacket = new S_Despawn();
                foreach (GameObject obj in removed)
                {
                    despawnPacket.ObjectId.Add(obj.Id);
                }
                Owner.Session.Send(despawnPacket);
            }
            PreviousObjects = currentObjects;
            // 프로젝트: 시야 갱신 주기(틱) 예약 — 맵·부하에 맞게 조정 가능
            Owner.Room.EnqueueAfter(500, Update);
        }
    }
}

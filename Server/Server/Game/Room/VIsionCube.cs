using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game.Room
{
    public class VIsionCube
    {
        public Player Owner { get; private set; }
        public HashSet<GameObject> PreviousObjects { get; private set; } = new HashSet<GameObject>();

        public VIsionCube(Player owner)
        {
            Owner = owner;            
        }
        private bool _initialize = false;
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

                foreach(Monster monster in zone.Monsters)
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

                foreach(Projectile projectile in zone.Projectiles)
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

            if(_initialize == false)
            {
                Owner.Room.PushAfter(1000, Update);
                _initialize = true;
                return;
            }
                
            HashSet<GameObject> currentObjects = GatherObjects();

            // 기존에 없었는데 새로 생긴 애들 spawn
            List<GameObject> added = currentObjects.Except(PreviousObjects).ToList();
            if (added.Count > 0)
            {
                S_Spawn spawnPacket = new S_Spawn();
                foreach (GameObject obj in added)
                {
                    if(obj == Owner)
                    {
                        continue;
                    }
                    ObjectInfo info = new ObjectInfo();
                    info.MergeFrom(obj.Info);
                    spawnPacket.Objects.Add(info);                    
                }
                Owner.Session.Send(spawnPacket);
            }

            // 기존에 있었는데 사라진 애들 despawn
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
            Owner.Room.PushAfter(500, Update);
        }
    }
}

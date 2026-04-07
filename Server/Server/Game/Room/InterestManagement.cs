using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;

namespace Server.Game.Room
{
    public class InterestManagement
    {
        private const int UpdateIntervalMs = 500;
        private const int EquipSyncDelayMs = 100;

        public Player Owner { get; private set; }

        private HashSet<GameObject> _previousObjects = new HashSet<GameObject>();
        private readonly HashSet<GameObject> _currentObjects = new HashSet<GameObject>();
        private readonly List<GameObject> _spawnBuffer = new List<GameObject>();
        private readonly List<int> _despawnBuffer = new List<int>();

        public InterestManagement(Player owner)
        {
            Owner = owner;
        }

        public void Refresh()
        {
            _previousObjects.Clear();
        }

        public void Update()
        {
            if (Owner == null || Owner.Room == null)
                return;

            GatherVisibleObjects();
            ProcessSpawns();
            ProcessDespawns();

            (_previousObjects, _) = (_currentObjects, _previousObjects);
            // _previousObjects now points to the set we just built;
            // we will clear _currentObjects at the start of next GatherVisibleObjects.

            Owner.Room.EnqueueAfter(UpdateIntervalMs, Update);
        }

        private void GatherVisibleObjects()
        {
            _currentObjects.Clear();

            Vector2Int ownerPos = Owner.CellPos;
            List<Zone> zones = Owner.Room.GetAdjacentZone(ownerPos);

            foreach (Zone zone in zones)
            {
                if (zone == null)
                    continue;

                CollectFromZone(zone.Players, ownerPos);
                CollectFromZone(zone.Monsters, ownerPos);
                CollectFromZone(zone.Projectiles, ownerPos);
                CollectFromZone(zone.Magics, ownerPos);
            }
        }

        private void CollectFromZone<T>(HashSet<T> objects, Vector2Int ownerPos) where T : GameObject
        {
            foreach (T obj in objects)
            {
                if (obj == null)
                    continue;

                int dx = obj.CellPos.x - ownerPos.x;
                int dy = obj.CellPos.y - ownerPos.y;
                if (Math.Abs(dx) > GameRoom.VisionCells || Math.Abs(dy) > GameRoom.VisionCells)
                    continue;

                _currentObjects.Add(obj);
            }
        }

        private void ProcessSpawns()
        {
            _spawnBuffer.Clear();

            foreach (GameObject obj in _currentObjects)
            {
                if (!_previousObjects.Contains(obj))
                    _spawnBuffer.Add(obj);
            }

            if (_spawnBuffer.Count == 0)
                return;

            S_Spawn spawnPacket = new S_Spawn();
            foreach (GameObject obj in _spawnBuffer)
            {
                if (obj == Owner)
                    continue;

                if (obj.ObjectType == GameObjectType.Player)
                {
                    Player player = obj as Player;
                    if (player == null || player.Session == null)
                        continue;

                    Owner.Room.EnqueueAfter(EquipSyncDelayMs, Owner.Room.HandleEquippedItemList, Owner, player, true);
                }

                ObjectInfo info = new ObjectInfo();
                info.MergeFrom(obj.Info);
                spawnPacket.Objects.Add(info);
            }

            if (spawnPacket.Objects.Count > 0)
                Owner.Session.Send(spawnPacket);
        }

        private void ProcessDespawns()
        {
            _despawnBuffer.Clear();

            foreach (GameObject obj in _previousObjects)
            {
                if (!_currentObjects.Contains(obj))
                    _despawnBuffer.Add(obj.Id);
            }

            if (_despawnBuffer.Count == 0)
                return;

            S_Despawn despawnPacket = new S_Despawn();
            foreach (int objectId in _despawnBuffer)
            {
                despawnPacket.ObjectId.Add(objectId);
            }

            Owner.Session.Send(despawnPacket);
        }
    }
}

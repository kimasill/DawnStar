using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using Server.DB;
using Server.Game.Job;
using Server.Game.Room;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace Server.Game
{
    public partial class GameRoom : TaskQueue
    {
        public const int VisionCells = 20;
        public int RoomId { get; set; }

        Dictionary<int, Player> _players = new Dictionary<int, Player>();
        Dictionary<int, Monster> _monsters = new Dictionary<int, Monster>();
        Dictionary<int, Projectile> _projectiles = new Dictionary<int, Projectile>();
        Dictionary<int, Magic> _magics = new Dictionary<int, Magic>();
        public Zone[,] Zones { get; private set ; }
        public int ZoneCells { get; private set; }

        private TaskCompletionSource<bool> _initializationTcs = new TaskCompletionSource<bool>();

        public Map Map { get; private set; } = new Map();

        public Zone GetZone(Vector2Int cellPos)
        {
            int x = (cellPos.x - Map.MinX) / ZoneCells;
            int y = (Map.MaxY - cellPos.y) / ZoneCells;

            if (x < 0 || x >= Zones.GetLength(1))
                return null;
            if (y < 0 || y >= Zones.GetLength(0))
                return null;

            return GetZone(y, x);
        }

        public Zone GetZone(int indexY, int indexX)
        {
            if (indexX < 0 || indexX >= Zones.GetLength(1))
                return null;
            if (indexY < 0 || indexY >= Zones.GetLength(0))
                return null;

            return Zones[indexY, indexX];
        }

        public void Init(int mapId, int zoneCells)
        {
            Map.LoadMap(mapId);
            Map.LoadSpawnPoints(mapId);
            Map.LoadInteractionPoints(mapId, this);

            ZoneCells = zoneCells;
            int countY = (Map.SizeY + zoneCells - 1) / zoneCells;
            int countX = (Map.SizeX + zoneCells - 1) / zoneCells;
            Zones = new Zone[countY, countX];
            for (int y = 0; y < countY; y++)
            {
                for (int x = 0; x < countX; x++)
                {
                    Zones[y, x] = new Zone(y, x);
                }
            }
            HandleSpawnMonster();
            _initializationTcs.SetResult(true);
        }

        public void Update()
        {
            ExecuteAll();
        }

        Random _rand = new Random();

        public void EnterGame(GameObject gameObject, bool randPos=false)
        {
            if (gameObject == null)
                return;

            if (randPos)
            {
                Vector2Int respawnPos;
                while (true)
                {
                    respawnPos.x = _rand.Next(Map.MinX, Map.MaxX + 1);
                    respawnPos.y = _rand.Next(Map.MinY, Map.MaxY + 1);
                    if (Map.Find(respawnPos).Count == 0)
                    {
                        gameObject.CellPos = respawnPos;
                        break;
                    }
                }
            }

            Map.ApplyEnter(gameObject);

            Player excludePlayer = null;
            switch (gameObject)
            {
                case Player player:
                    excludePlayer = player;
                    if (!EnterPlayer(player))
                        return;
                    break;
                case Monster monster:
                    if (!EnterMonster(monster))
                        return;
                    break;
                case Projectile projectile:
                    if (!EnterProjectile(projectile))
                        return;
                    break;
                case Magic magic:
                    if (!EnterMagic(magic))
                        return;
                    break;
                default:
                    return;
            }

            S_Spawn spawnPacket = new S_Spawn();
            spawnPacket.Objects.Add(gameObject.Info);
            Broadcast(gameObject.CellPos, spawnPacket, excludePlayer);
        }

        private bool EnterPlayer(Player player)
        {
            if (_players.ContainsKey(player.Id))
                LeaveGame(player.Id);

            player.Info.MapInfo = player.MapInfo;
            _players.Add(player.Id, player);
            player.Room = this;
            player.IsDead = false;

            player.RefreshAdditionalStat();

            Map.ApplyMove(player, new Vector2Int(player.CellPos.x, player.CellPos.y));
            Console.WriteLine($"Player Room Id:{RoomId}");

            var zone = GetZone(player.CellPos);
            if (zone == null)
                return false;

            zone.Players.Add(player);

            S_EnterGame enterPacket = new S_EnterGame();
            enterPacket.Player = player.Info;
            if (player.Session != null)
                player.Session.Send(enterPacket);
            else
                Console.WriteLine("Error: player.Session is null");

            player.Vision.Refresh();
            player.Update();
            return true;
        }

        private bool EnterMonster(Monster monster)
        {
            _monsters.Add(monster.Id, monster);
            monster.Room = this;

            var zone = GetZone(monster.CellPos);
            if (zone == null)
                return false;

            zone.Monsters.Add(monster);
            Console.WriteLine($"Monster Id:{monster.Id} Type:{monster.MonsterType} Added");
            Map.ApplyMove(monster, new Vector2Int(monster.CellPos.x, monster.CellPos.y));

            monster.Update();
            return true;
        }

        private bool EnterProjectile(Projectile projectile)
        {
            _projectiles.Add(projectile.Id, projectile);
            projectile.Room = this;

            var zone = GetZone(projectile.CellPos);
            if (zone == null)
                return false;

            zone.Projectiles.Add(projectile);
            projectile.Update();
            return true;
        }

        private bool EnterMagic(Magic magic)
        {
            if (magic.IsComplete)
            {
                LeaveGame(magic.Id);
                return false;
            }

            if (!_magics.ContainsKey(magic.Id))
                _magics.Add(magic.Id, magic);
            else
                Console.WriteLine($"Magic with ID {magic.Id} already exists.");

            magic.Room = this;

            var zone = GetZone(magic.CellPos);
            if (zone == null)
                return false;

            zone.Magics.Add(magic);
            magic.Update();
            return true;
        }

        
        public void LeaveGame(int objectId)
        {
            LeaveGame(objectId, true);
        }

        public void LeaveGame(int objectId, bool save)
        {
            GameObjectType type = EntityRegistry.GetObjectType(objectId);

            var (cellPos, despawnAnim) = type switch
            {
                GameObjectType.Player     => LeavePlayer(objectId, save),
                GameObjectType.Monster    => LeaveMonster(objectId),
                GameObjectType.Projectile => LeaveProjectile(objectId),
                GameObjectType.Magic      => LeaveMagic(objectId),
                _ => (default(Vector2Int?), false)
            };

            if (cellPos == null)
                return;

            S_Despawn despawnPacket = new S_Despawn();
            despawnPacket.ObjectId.Add(objectId);
            despawnPacket.DespawnAnim = despawnAnim;
            Broadcast(cellPos.Value, despawnPacket);
        }

        private (Vector2Int?, bool) LeavePlayer(int objectId, bool save)
        {
            if (!_players.Remove(objectId, out Player player))
                return (null, false);

            Vector2Int cellPos = player.CellPos;
            player.OnLeaveGame(save);
            Map.ApplyLeave(player);
            player.Room = null;

            S_LeaveGame leavePacket = new S_LeaveGame();
            player.Session.Send(leavePacket);
            return (cellPos, false);
        }

        private (Vector2Int?, bool) LeaveMonster(int objectId)
        {
            if (!_monsters.Remove(objectId, out Monster monster))
                return (null, false);

            Vector2Int cellPos = monster.CellPos;
            Map.ApplyLeave(monster);
            monster.Room = null;
            return (cellPos, false);
        }

        private (Vector2Int?, bool) LeaveProjectile(int objectId)
        {
            if (!_projectiles.Remove(objectId, out Projectile projectile))
                return (null, false);

            Vector2Int cellPos = projectile.CellPos;
            bool despawnAnim = projectile.DespawnAnim;
            Map.ApplyLeave(projectile);
            projectile.Room = null;
            return (cellPos, despawnAnim);
        }

        private (Vector2Int?, bool) LeaveMagic(int objectId)
        {
            if (!_magics.Remove(objectId, out Magic magic))
                return (null, false);
            if (magic == null)
                return (null, false);

            Vector2Int cellPos = magic.CellPos;
            bool despawnAnim = magic.DespawnAnim;
            Map.ApplyLeave(magic);
            magic.Room = null;
            return (cellPos, despawnAnim);
        }

        Player FindPlayer(Func<GameObject, bool> condition)
        {
            foreach (Player player in _players.Values)
            {
                if (condition.Invoke(player))
                    return player;
            }

            return null;
        }

        public Player FindCloesetPlayer(Vector2Int pos, int range)
        {
            List<Player> players = GetAdjacentPlayers(pos, range);
            players.Sort((left, right) =>
            {
                int leftDist = (left.CellPos - pos).cellDistanceFromZero;
                int rightDist = (right.CellPos - pos).cellDistanceFromZero;
                return leftDist - rightDist;
            });
            
            foreach(Player player in players)
            {
                List<Vector2Int> path = Map.FindPath(pos, player.CellPos, checkObjects: true);
                if (path.Count < 2 || path.Count > range)
                    continue;

                return player;
            }

            return null;
        }
        public int GetPlayerCount()
        {
            return _players.Count;
        }

        public void Broadcast(Vector2Int pos, IMessage packet, Player excludePlayer = null)
        {
            List<Zone> zones = GetAdjacentZone(pos);            
            foreach(Player p in zones.SelectMany(z => z.Players))
            {
                if (p == excludePlayer)
                    continue;
                int dx = p.CellPos.x - pos.x;
                int dy = p.CellPos.y - pos.y;
                if (Math.Abs(dx) > GameRoom.VisionCells || Math.Abs(dy) > GameRoom.VisionCells)
                {
                    continue;
                }
                p.Session.Send(packet);
            }
        }

        public List<Player> GetAdjacentPlayers(Vector2Int pos, int range)
        {
            List<Zone> zones = GetAdjacentZone(pos, range);
            return zones.SelectMany(z => z.Players).ToList();
        }

        public List<Zone> GetAdjacentZone(Vector2Int cellPos, int range = GameRoom.VisionCells)
        {
            HashSet<Zone> zones = new HashSet<Zone>();

            int maxY = cellPos.y + range;
            int minY = cellPos.y - range;
            int maxX = cellPos.x + range;
            int minX = cellPos.x - range;

            if (ZoneCells == 0)
            {
                throw new InvalidOperationException("ZoneCells cannot be zero.");
            }
            Vector2Int topLeft = new Vector2Int(minX, maxY);

            int minIndexY = (Map.MaxY - topLeft.y) / ZoneCells;
            int minIndexX = (topLeft.x - Map.MinX) / ZoneCells;

            Vector2Int bottomRight = new Vector2Int(maxX, minY);
            int maxIndexY = (Map.MaxY - bottomRight.y) / ZoneCells;
            int maxIndexX = (bottomRight.x - Map.MinX) / ZoneCells;

            for (int x = minIndexX; x <= maxIndexX; x++)
            {
                for(int y = minIndexY; y <= maxIndexY; y++)
                {
                    Zone zone = GetZone(y, x);
                    if (zone == null)
                        continue;

                    zones.Add(zone);
                } 
            }

            int[] delta = new int[2] {-range , +range };
            foreach(int dy in delta)
            {
               foreach(int dx in delta)
                {
                    int y = cellPos.y + dy;
                    int x = cellPos.x + dx;
                    Zone zone = GetZone(new Vector2Int(x, y));
                    if (zone == null)
                        continue;
                    zones.Add(zone);
                }
            }
            return zones.ToList();
        }

        public void ResetRoom()
        {
            if(_players.Count == 0)
            {
                foreach (var monster in _monsters)
                {
                    LeaveGame(monster.Value.Id);
                }
                _projectiles.Clear();
            }            
        }

        public Task WaitForInitializationAsync()
        {
            return _initializationTcs.Task;
        }
    }
}

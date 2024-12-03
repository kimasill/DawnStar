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
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;

namespace Server.Game
{
    public partial class GameRoom : JobSerializer
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

            // Zone
            ZoneCells = zoneCells; // 10
                                   // 1~10 칸 = 1존
                                   // 11~20칸 = 2존
                                   // 21~30칸 = 3존
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

        // 누군가 주기적으로 호출해줘야 한다
        public void Update()
        {
            Flush();
        }

        Random _rand = new Random();
        // ...


        
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
                    if (Map.Find(respawnPos) == null)
                    {
                        gameObject.CellPos = respawnPos;
                        break;
                    }
                }
            }

            GameObjectType type = ObjectManager.GetObjectType(gameObject.Id);
            Player player = null;
            if (type == GameObjectType.Player)
            {
                player = gameObject as Player;
                if (player == null)
                    return;
                player.Info.MapInfo = player.MapInfo;

                _players.Add(gameObject.Id, player);
                player.Room = this;
                player.IsDead = false;

                player.RefreshAdditionalStat();

                Map.ApplyMove(player, new Vector2Int(player.CellPos.x, player.CellPos.y));
                Console.WriteLine($"Player Room Id:{RoomId}");

                var zone = GetZone(player.CellPos);
                if (zone == null)
                {
                    // Handle the error, log it, or initialize the zone
                    return;
                }
                zone.Players.Add(player);
                // 본인한테 정보 전송
                S_EnterGame enterPacket = new S_EnterGame();
                enterPacket.Player = player.Info;
                if (player.Session != null)
                {
                    player.Session.Send(enterPacket);
                }
                else
                {
                    Console.WriteLine("Error: player.Session is null");
                }
                player.Vision.Update();
            }
            else if (type == GameObjectType.Monster)
            {
                Monster monster = gameObject as Monster;
                _monsters.Add(gameObject.Id, monster);
                monster.Room = this;                
                var zone = GetZone(monster.CellPos);
                if (zone == null)
                {
                    // Handle the error, log it, or initialize the zone
                    return;
                }
                zone.Monsters.Add(monster);
                Console.WriteLine($"Monster Id:{monster.Id} Type:{monster.MonsterType} Added");
                Map.ApplyMove(monster, new Vector2Int(monster.CellPos.x, monster.CellPos.y));

                monster.Update();
            }
            else if (type == GameObjectType.Projectile)
            {
                Projectile projectile = gameObject as Projectile;
                _projectiles.Add(gameObject.Id, projectile);
                projectile.Room = this;

                GetZone(projectile.CellPos).Projectiles.Add(projectile);
                projectile.Update();
            }
            else if (type == GameObjectType.Magic)
            {
                Magic magic = gameObject as Magic;
                _magics.Add(gameObject.Id, magic);
                magic.Room = this;

                GetZone(magic.CellPos).Magics.Add(magic);
                magic.Update();
            }
            else
            {
                return;
            }

            // 타인한테 정보 전송
            // null 전달 변경
            S_Spawn spawnPacket = new S_Spawn();
            spawnPacket.Objects.Add(gameObject.Info);
            Broadcast(gameObject.CellPos, spawnPacket, player);
        }

        

        public void LeaveGame(int objectId)
        {
            GameObjectType type = ObjectManager.GetObjectType(objectId);
            Vector2Int cellPos;
            bool despawnAnim = false;

            if (type == GameObjectType.Player)
            {
                Player player = null;
                if (_players.Remove(objectId, out player) == false)
                    return;
                cellPos = player.CellPos;

                player.OnLeaveGame();
                Map.ApplyLeave(player);
                player.Room = null;

                // 본인한테 정보 전송
                {
                    S_LeaveGame leavePacket = new S_LeaveGame();
                    player.Session.Send(leavePacket);
                }
            }
            else if (type == GameObjectType.Monster)
            {
                Monster monster = null;
                if (_monsters.Remove(objectId, out monster) == false)
                    return;
                cellPos = monster.CellPos;
                Map.ApplyLeave(monster);
                monster.Room = null;
            }
            else if (type == GameObjectType.Projectile)
            {
                Projectile projectile = null;
                if (_projectiles.Remove(objectId, out projectile) == false)
                    return;
                cellPos = projectile.CellPos;
                despawnAnim = projectile.DespawnAnim;
                Map.ApplyLeave(projectile);                
                projectile.Room = null;
            }
            else if(type == GameObjectType.Magic)
            {
                Magic magic = null;
                if (_magics.Remove(objectId, out magic) == false)
                    return;
                cellPos = magic.CellPos;
                Map.ApplyLeave(magic);
                magic.Room = null;
            }
            else return;
            //타인한테 정보 전송
            {
                S_Despawn despawnPacket = new S_Despawn();
                despawnPacket.ObjectId.Add(objectId);
                despawnPacket.DespawnAnim = despawnAnim;
                Broadcast(cellPos, despawnPacket);
            }
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

        // 시야 모서리 겹치는 Zone
        public List<Zone> GetAdjacentZone(Vector2Int cellPos, int range = GameRoom.VisionCells)
        {
            HashSet<Zone> zones = new HashSet<Zone>();

            int maxY = cellPos.y + range;
            int minY = cellPos.y - range;
            int maxX = cellPos.x + range;
            int minX = cellPos.x - range;

            if (ZoneCells == 0)
            {
                // Handle the case where ZoneCells is zero
                throw new InvalidOperationException("ZoneCells cannot be zero.");   
            }
            //좌측 상단
            Vector2Int topLeft = new Vector2Int(minX, maxY);

            int minIndexY = (Map.MaxY - topLeft.y) / ZoneCells;
            int minIndexX = (topLeft.x - Map.MinX) / ZoneCells;

            //우측 하단
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

                HandleSpawnMonster();
            }            
        }

        public Task WaitForInitializationAsync()
        {
            return _initializationTcs.Task;
        }
    }
}

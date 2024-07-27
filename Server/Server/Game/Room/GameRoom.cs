using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using Server.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game.Room
{
    public class GameRoom
    {
        object _lock = new object();
        public int RoomId { get; set; }

        Dictionary<int, Player> _players = new Dictionary<int, Player>();
        Dictionary<int, Projectile> _projectiles = new Dictionary<int, Projectile>();
        Dictionary<int, Monster> _monsters = new Dictionary<int, Monster>();

        public Map Map { get; private set; } = new Map();

        public void Init(int mapId)
        {
            Map.LoadMap(mapId);
            Monster monster = ObjectManager.Instance.Add<Monster>();
            monster.CellPos = new Vector2Int(5, 5);
            EnterGame(monster);
        }
        public void Update()
        {
            lock (_lock)
            {
                foreach (Monster monster in _monsters.Values)
                {
                    monster.Update();
                }
                foreach (Projectile projectile in _projectiles.Values)
                {
                    projectile.Update();
                }
            }
        }
        public void EnterGame(GameObject gameObject)
        {
            if (gameObject == null)
                return;

            GameObjectType type = ObjectManager.GetObjectType(gameObject.Info.ObjectId);
            lock (_lock)
            {
                if(type == GameObjectType.Player)
                {
                    Player player = gameObject as Player;
                    _players.Add(gameObject.Id, player);
                    Map.ApplyMove(player, new Vector2Int(player.CellPos.x, player.CellPos.y));
                    player.Room = this;
                    //본인한테 정보전송
                    {
                        S_EnterGame enterPacket = new S_EnterGame();
                        enterPacket.Player = player.Info;
                        player.Session.Send(enterPacket);

                        S_Spawn spawnPacket = new S_Spawn();
                        foreach (Player p in _players.Values)
                        {
                            if (player != p)
                                spawnPacket.Objects.Add(p.Info);
                        }
                        foreach (Monster m in _monsters.Values)
                        {
                            spawnPacket.Objects.Add(m.Info);
                        }
                        foreach (Projectile p in _projectiles.Values)
                        {
                            spawnPacket.Objects.Add(p.Info);
                        }
                        player.Session.Send(spawnPacket);
                    }
                    
                }
                else if(type == GameObjectType.Projectile)
                {
                    Projectile projectile = gameObject as Projectile;
                    _projectiles.Add(gameObject.Id, projectile);
                    projectile.Room = this;
                }
                else if(type == GameObjectType.Monster)
                {
                    Monster monster = gameObject as Monster;
                    _monsters.Add(gameObject.Id, monster);
                    monster.Room = this;
                    Map.ApplyMove(monster, new Vector2Int(monster.CellPos.x, monster.CellPos.y));
                }

                //다른 플레이어에게 정보전송
                {
                    S_Spawn spawnPacket = new S_Spawn();
                    spawnPacket.Objects.Add(gameObject.Info);
                    foreach (Player p in _players.Values)
                    {
                        if (p.Id != gameObject.Id)
                            p.Session.Send(spawnPacket);
                    }
                }
            }
        }

        public void LeaveGame(int objectId)
        {
            GameObjectType type = ObjectManager.GetObjectType(objectId);
            lock (_lock)
            {
                if(type == GameObjectType.Player)
                {
                    Player player = null;
                    if (_players.Remove(objectId, out player) == false)
                        return;

                    player.Room = null;
                    Map.ApplyLeave(player);
                    //본인한테 정보전송
                    {
                        S_LeaveGame leavePacket = new S_LeaveGame();
                        player.Session.Send(leavePacket);
                    }
                }
                else if(type == GameObjectType.Projectile)
                {
                    Projectile projectile = null;
                    if (_projectiles.Remove(objectId, out projectile) == false)
                        return;

                    projectile.Room = null;
                }
                else if (type == GameObjectType.Monster)
                {
                    Monster monster = null;
                    if (_monsters.Remove(objectId, out monster) == false)
                        return;

                    monster.Room = null;
                    Map.ApplyLeave(monster);
                }
                

                //다른 플레이어에게 정보전송
                {
                    S_Despawn despawnPacket = new S_Despawn();
                    despawnPacket.ObjectId.Add(objectId);
                    foreach (Player p in _players.Values)
                    {
                        if(p.Id != objectId)
                            p.Session.Send(despawnPacket);
                    }
                }
            }
        }

        public void HandleMove(Player player, C_Move movePacket)
        {
            if (player == null)
                return;

            lock (_lock)
            {
                PositionInfo movePosInfo = movePacket.Position;
                ObjectInfo info = player.Info;

                if (movePosInfo.PosX != info.Position.PosX || movePosInfo.PosY != info.Position.PosY)
                {
                    if (Map.CanGo(new Vector2Int(movePosInfo.PosX, movePosInfo.PosY)) == false)
                        return;
                }
                info.Position.State = movePosInfo.State;
                info.Position.MoveDir = movePosInfo.MoveDir;
                Map.ApplyMove(player, new Vector2Int(movePosInfo.PosX, movePosInfo.PosY));

                //방에 있는 모든 플레이어에게 이동 패킷을 전송
                S_Move resMovePacket = new S_Move();
                resMovePacket.ObjectId = player.Info.ObjectId;
                resMovePacket.Position = movePacket.Position;

                BroadCast(resMovePacket);
            }
        }

        public void HandleSkill(Player player, C_Skill skillPacket)
        {
            if (player == null)
                return;

            lock (_lock)
            {
                ObjectInfo info = player.Info;
                if (info.Position.State != CreatureState.Idle)
                    return;

                //Animation
                info.Position.State = CreatureState.Skill;

                S_Skill resSkillPacket = new S_Skill() { Info = new SkillInfo() };
                resSkillPacket.ObjectId = player.Info.ObjectId;
                resSkillPacket.Info.SkillId = skillPacket.Info.SkillId;
                BroadCast(resSkillPacket);

                Data.Skill skillData = null;
                if (DataManager.SkillDict.TryGetValue(skillPacket.Info.SkillId, out skillData) == false)
                    return;

                switch (skillData.skillType)
                {
                    case SkillType.SkillAttack:
                        {
                            Vector2Int skillPos = player.GetFrontCellPos(info.Position.MoveDir);
                            GameObject target = Map.Find(skillPos);
                            if (target != null)
                            {
                                //스킬 사용
                                Console.WriteLine("Hit");
                                //HandleSkill(player, new C_Skill());
                            }
                        }
                        break;
                    case SkillType.SkillProjectile:
                        {
                            //TODO : Arrow
                            Arrow arrow = ObjectManager.Instance.Add<Arrow>();
                            if (arrow == null)
                                return;

                            arrow.Owner = player;
                            arrow.Data = skillData;
                            arrow.PosInfo.State = CreatureState.Moving;
                            arrow.PosInfo.MoveDir = info.Position.MoveDir;
                            arrow.PosInfo.PosX = info.Position.PosX;
                            arrow.PosInfo.PosY = info.Position.PosY;
                            arrow.Speed = skillData.projectile.speed;
                            EnterGame(arrow);
                        }
                        break;                
                }
            }
        }
        public Player FindPlayer(Func<GameObject, bool> condition)
        {
            lock (_lock)
            {
                foreach (Player p in _players.Values)
                {
                    if (condition.Invoke(p))
                        return p;
                }
            }
            return null;
        }
        public void BroadCast(IMessage packet)
        {
            lock (_lock)
            {
                foreach (Player p in _players.Values)
                {
                    p.Session.Send(packet);
                }
            }
        }
    }
}

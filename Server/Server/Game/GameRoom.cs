using Google.Protobuf;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game
{
    public class GameRoom
    {
        object _lock = new object();
        public int RoomId { get; set; }

        List<Player> _players = new List<Player>();

        public void EnterGame(Player newPlayer)
        {
            if(newPlayer == null)
                return;

            lock (_lock)
            {
                _players.Add(newPlayer);
                newPlayer.Room = this;
            }

            //본인한테 정보전송
            {
                S_EnterGame enterPacket = new S_EnterGame();
                enterPacket.Player = newPlayer.Info;
                newPlayer.Session.Send(enterPacket);

                S_Spawn spawnPacket = new S_Spawn();
                foreach (Player p in _players)
                {
                    if (newPlayer != p)
                        spawnPacket.Player.Add(p.Info);
                }
                newPlayer.Session.Send(spawnPacket);
            }
            //다른 플레이어에게 정보전송
            {
                S_Spawn spawnPacket = new S_Spawn();
                spawnPacket.Player.Add(newPlayer.Info);
                foreach (Player p in _players)
                {
                    if (newPlayer != p)
                        p.Session.Send(spawnPacket);
                }
            }
        }

        public void LeaveGame(int playerId)
        {
            lock (_lock)
            {
                Player player = _players.Find(p => p.Info.PlayerId == playerId);
                if(player == null)
                    return;
                
                _players.Remove(player);
                player.Room = null;

                //본인한테 정보전송
                {
                    S_LeaveGame leavePacket = new S_LeaveGame();
                    player.Session.Send(leavePacket);
                }

                //다른 플레이어에게 정보전송
                {
                    S_Despawn despawnPacket = new S_Despawn();
                    despawnPacket.PlayerId.Add(player.Info.PlayerId);
                    foreach (Player p in _players)
                    {
                        p.Session.Send(despawnPacket);
                    }
                }
            }
        }

        public void HandleMove(Player player, C_Move movePacket)
        {
            if(player == null)
                return;

            lock (_lock)
            {
                PlayerInfo info = player.Info;
                info.Position = movePacket.Position;

                //방에 있는 모든 플레이어에게 이동 패킷을 전송
                S_Move resMovePacket = new S_Move();
                resMovePacket.PlayerId = player.Info.PlayerId;
                resMovePacket.Position = movePacket.Position;

                BroadCast(resMovePacket);
            }
        }

        public void HandleSkill(Player player, C_Skill skillPacket)
        {
            if(player == null)
                return;

            lock (_lock)
            {
                PlayerInfo info = player.Info;
                if(info.Position.State != CreatureState.Idle)
                    return;
                //TODO: 스킬 사용 가능 여부 검사
                //통과
                info.Position.State = CreatureState.Skill;

                S_Skill resSkillPacket = new S_Skill() { Info = new SkillInfo() };
                resSkillPacket.PlayerId = player.Info.PlayerId;
                resSkillPacket.Info.SkillId = 1;
                BroadCast(resSkillPacket);

                //데미지
            }
        }

        public void BroadCast(IMessage packet)
        {
            lock (_lock)
            {
                foreach (Player p in _players)
                {
                    p.Session.Send(packet);
                }
            }
        }
    }
}

using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using Server.Game.Job;
using Server.Game.Room;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Server.Game
{
    public partial class GameRoom : JobSerializer
    {    
        public void HandleMove(Player player, C_Move movePacket)
        {
            if (player == null)
                return;

            // TODO : 검증
            PositionInfo movePosInfo = movePacket.Position;
            ObjectInfo info = player.Info;
            info.Position.State = movePosInfo.State;
            //Console.WriteLine(info.Position.State);
            // 다른 좌표로 이동할 경우, 갈 수 있는지 체크
            if (movePosInfo.PosX != info.Position.PosX || movePosInfo.PosY != info.Position.PosY)
            {
                if (Map.CanGo(new Vector2Int(movePosInfo.PosX, movePosInfo.PosY)) == false)
                    return;
            }

            
            info.Position.MoveDir = movePosInfo.MoveDir;
            info.Position.LookDir = movePosInfo.LookDir;
            Map.ApplyMove(player, new Vector2Int(movePosInfo.PosX, movePosInfo.PosY));
            // 다른 플레이어한테도 알려준다
            S_Move resMovePacket = new S_Move();
            resMovePacket.ObjectId = player.Info.ObjectId;
            resMovePacket.Position = movePacket.Position;

            Broadcast(player.CellPos, resMovePacket);
        }

        public void HandleSkill(Player player, C_Skill skillPacket)
        {
            if (player == null)
                return;
            ObjectInfo info = player.Info;
            Console.WriteLine(player.Info.Position.State);
            if (info.Position.State != CreatureState.Idle)
                return;

            // TODO : 스킬 사용 가능 여부 체크
            info.Position.State = CreatureState.Skill;
            S_Skill skill = new S_Skill() { Info = new SkillInfo() };
            skill.ObjectId = info.ObjectId;
            skill.Info.SkillId = skillPacket.Info.SkillId;
            Broadcast(player.CellPos, skill);

            Data.Skill skillData = null;
            if (DataManager.SkillDict.TryGetValue(skillPacket.Info.SkillId, out skillData) == false)
                return;

            switch (skillData.skillType)
            {
                case SkillType.SkillAttack:
                    {
                        List<Vector2Int> skillPos = new List<Vector2Int>();
                        List<GameObject> targets = new List<GameObject>();
                        if (skillData.shape.shapeType == ShapeType.ShapeBent)
                        {
                            skillPos.Add(player.GetFrontCellPos());
                            skillPos.Add(player.GetPosFromLookDir(player.CellPos, info.Position.LookDir));
                        }
                        foreach (Vector2Int pos in skillPos)
                        {
                            GameObject target = Map.Find(pos);
                            if (target != null)
                            {
                                // LookDir에 따라 공격 방향 처리
                                // TODO : 데미지 계산
                                // TODO : MoveDir 방향에도 공격 처리
                                Console.WriteLine("Hit GameObject !");
                                target.OnDamaged(player, player.TotalAttack); //피격판정      
                            }
                        }
                        break;
                    }
                case SkillType.SkillProjectile:
                    {
                        Arrow arrow = ObjectManager.Instance.Add<Arrow>();
                        if (arrow == null)
                            return;

                        arrow.Owner = player;
                        arrow.Data = skillData;
                        arrow.PosInfo.State = CreatureState.Moving;
                        arrow.PosInfo.MoveDir = player.PosInfo.MoveDir;
                        arrow.PosInfo.PosX = player.PosInfo.PosX;
                        arrow.PosInfo.PosY = player.PosInfo.PosY;
                        arrow.Speed = skillData.projectile.speed;
                        Push(EnterGame, arrow, false);
                    }
                    break;
            }
        }
    }
}

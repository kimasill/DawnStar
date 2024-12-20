using Google.Protobuf.Protocol;
using Server.Data;
using Server.Game.Job;
using Server.Game.Room;
using System;
using System.Collections.Generic;

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
            if (info.Position.State != CreatureState.Idle)
                return;

            // TODO : 스킬 사용 가능 여부 체크
            info.Position.State = CreatureState.Skill;
            S_Skill skill = new S_Skill() { Info = new SkillInfo() };
            skill.ObjectId = info.ObjectId;
            skill.Info.SkillId = skillPacket.Info.SkillId;

            SkillData skillData = null;
            if (DataManager.SkillDict.TryGetValue(skillPacket.Info.SkillId, out skillData) == false)
                return;
            bool isCool;
            if(skillPacket.Info.SkillId == 1)
            {
                isCool = player.Skill.HandleSkillCool(skillData, true);                
            }
            else
            {
                isCool = player.Skill.HandleSkillCool(skillData);
            }
            if (!isCool)
            {
                return;
            }
            Broadcast(player.CellPos, skill);
            player.Skill.StartSkill(player, skillData);
        }

        public int CalculateDamage(GameObject attacker, int id, int damage, GameObject victim)
        {
            if (attacker == null || victim == null) // victim null 확인 추가
                return damage;

            S_Damage damagePacket = new S_Damage();
            if (attacker is Player player)
            {
                // TODO: critical 확률 계산
                if (player.TotalCriticalChance > 0)
                {
                    Random random = new Random();
                    int randomValue = random.Next(0, 100);
                    if (randomValue < player.TotalCriticalChance)
                    {
                        // critical 공격
                        damage = damage * player.TotalCriticalDamage;
                        damagePacket.Critical = true;
                    }
                }
            }

            // 방어력을 적용한 데미지 계산 (victim null 확인 추가)
            if (victim != null)
            {
                float defenseFactor = (float)Math.Pow(victim.TotalDefense, 0.7f);
                damage = (int)(damage * (1 - (defenseFactor / (defenseFactor + 100))));
            }

            if (victim.TotalDamageReduce != 0)
            {
                damage = (int)(damage * (1-victim.TotalDamageReduce));
            }

            if (damage >= 0)
            {
                damagePacket.Damage = damage;
                damagePacket.ObjectId = id;
                Broadcast(attacker.CellPos, damagePacket);
            }
            return damage;
        }
    }
}

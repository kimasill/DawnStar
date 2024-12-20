using Google.Protobuf.Protocol;
using Server.Data;
using Server.Game.Room;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Server.Game
{
    public partial class Skill
    {
        private async void ApplyBuff(SkillData data, GameObject target)
        {
            await Task.Delay((int)(1000 * data.term));
            // Buff 적용 로직
            if (data.buff != null)
            {
                // Buff 시작 시 로직
                Console.WriteLine($"Buff {data.buff.name} applied with value {data.buff.value}");
                target.ApplyBuff(data.buff);
            }
            else if (data.buffList != null)
            {
                foreach (var buff in data.buffList)
                {
                    // Buff 시작 시 로직
                    Console.WriteLine($"Buff {buff.name} applied with value {buff.value}");
                    target.ApplyBuff(buff);
                }
            }
        }
        private async void ApplyDeBuff(SkillData data, GameObject target)
        {
            await Task.Delay((int)(1000 * data.term));
            if (data.debuff != null)
            {
                // Debuff 시작 시 로직
                Console.WriteLine($"Debuff {data.debuff.name} applied with value {data.debuff.value}");
                target.ApplyDebuff(data.debuff);
            }
            else if (data.debuffList != null)
            {
                foreach (var debuff in data.debuffList)
                {
                    // Debuff 시작 시 로직
                    Console.WriteLine($"Debuff {debuff.name} applied with value {debuff.value}");
                    target.ApplyDebuff(debuff);
                }
            }
        }
        private async void ApplyMarkDebuff(SkillData skillData, GameObject target)
        {
            if (target == null)
                return;

            await Task.Delay((int)(1000 * skillData.term));
            S_Effect effectPacket = new S_Effect();
            effectPacket.ObjectId = target.Id;
            effectPacket.SkillId = skillData.id;

            DataManager.DebuffDict.TryGetValue(skillData.debuffList[0].id, out DebuffData debuffData);

            if (debuffData == null)
                return;

            effectPacket.Prefab = debuffData.prefab;

            Owner.Room.Broadcast(target.CellPos, effectPacket);    
            

            if (skillData.debuff != null)
            {
                target.ApplyDebuff(skillData.debuff);
            }
            else if (skillData.debuffList != null)
            {
                foreach (var debuff in skillData.debuffList)
                {
                    target.ApplyDebuff(debuff);
                }
            }
        }
        private void ApplyAfterEffect(SkillData skill, GameObject target)
        {
            if (skill == null)
                return;
            if (skill.debuff != null)
            {
                GameObject applyTarget = skill.debuff.isTarget ? target : Owner;
                HandleDebuffSkill(skill, applyTarget);
            }
            else if (skill.debuffList != null)
            {
                foreach (var debuff in skill.debuffList)
                {
                    GameObject applyTarget = debuff.isTarget ? target : Owner;
                    HandleDebuffSkill(skill, applyTarget);
                }
            }

            if (skill.buff != null)
            {
                GameObject applyTarget = skill.buff.isTarget ? target : Owner;
                HandleBuffSkill(skill, applyTarget);
            }
            else if (skill.buffList != null)
            {
                foreach (var buff in skill.buffList)
                {
                    GameObject applyTarget = buff.isTarget ? target : Owner;
                    HandleBuffSkill(skill, applyTarget);
                }
            }
        }
        private async void BuffExchange(SkillData skill)
        {
            if (skill.buff == null || skill.debuff == null)
                return;
            await Task.Delay((int)(1000 * Owner.TotalInvokeSpeed));
            await Task.Delay((int)(skill.term * 1000));
            ApplyBuff(skill, Owner);
            ApplyDeBuff(skill, Owner);
        }
        private async void RealTimeByEnemyNum(SkillData data, GameObject target = null)
        {
            if (target == null)
                return;
            // data의 시간 동안 반복한다. 1. 적 찾기 2. target에게 버프/디버프 적용
            long tick = 0;
            int enemyNum = 0;

            if (data.debuff != null)
            {
                tick = Environment.TickCount64 + (long)(data.debuff.duration * 1000);
            }
            else if (data.buff != null)
            {
                tick = Environment.TickCount64 + (long)(data.buff.duration * 1000);
            }
            int buff = -1;
            int debuff = -1;
            while (true)
            {
                List<Vector2Int> enemies = SkillLogic.GetAllTargetsInRange(Owner.CellPos, data.range);
                if (enemyNum < enemies.Count)
                {
                    enemyNum = enemies.Count - enemyNum;
                    if (data.debuff != null && enemyNum > 0)
                    {
                        target.ApplyDebuff(data.debuff, enemyNum);
                        debuff = target.Debuffs.Last().Key;
                    }
                    else if (data.buff != null && enemyNum > 0)
                    {
                        target.ApplyBuff(data.buff, enemyNum);
                        buff = target.Buffs.Last().Key;
                    }
                }
                else if (enemyNum > enemies.Count)
                {
                    enemyNum = enemyNum - enemies.Count;
                    if (debuff >= 0 && enemyNum > 0)
                    {
                        target.RemoveDebuff(data.debuff, enemyNum);
                    }
                    if (buff >= 0 && enemyNum > 0)
                    {
                        target.RemoveBuff(data.buff, enemyNum);
                    }
                }
                await Task.Delay(1000);

                if (Environment.TickCount64 > tick)
                    break;
            }
        }
        private async void DOT(SkillData data, GameObject target)
        {
            if (data.debuff == null || target == null)
                return;

            int tickInterval = 1000; // 1초 간격
            int totalTicks = data.debuff.duration;

            for (int i = 0; i < totalTicks; i++)
            {
                await Task.Delay(tickInterval);
                target.OnDamaged(Owner, (int)(Owner.TotalAttack * data.debuff.value));
            }

            Console.WriteLine($"Debuff {data.debuff.name} ended");
        }
    }
}

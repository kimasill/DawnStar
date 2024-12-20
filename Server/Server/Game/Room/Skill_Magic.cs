using Google.Protobuf.Protocol;
using Server.Data;
using Server.Game.Room;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game
{
    public partial class Skill
    {
        private async void SpotAttack(SkillData data, List<Vector2Int> skillPos)
        {
            await Task.Delay((int)(1000 * Owner.TotalInvokeSpeed));
            int count = 0;
            foreach (Vector2Int pos in skillPos)
            {
                if (Owner == null || Owner.Room == null || count > data.spot.maxCount)
                    return;
                count++;
                SpotAttack spot = ObjectManager.Instance.Add<SpotAttack>();
                spot.Owner = Owner;
                spot.Owner.Room = Owner.Room;
                spot.Data = data;
                spot.PosInfo.PosX = pos.x;
                spot.PosInfo.PosY = pos.y;
                spot.PosInfo.MoveDir = MoveDir.Down;
                spot.PosInfo.State = CreatureState.Moving;
                spot.Delay = data.spot.delay;
                spot.TemplateId = data.id;
                Owner.Room.Push(Owner.Room.EnterGame, spot, false);

                if (data.term != 0)
                    await Task.Delay((int)(data.term * 1000));
            }
        }
        public async void MagicBall(SkillData data)
        {
            for (int i = 0; i < data.count; i++)
            {
                MagicBall magicBall = ObjectManager.Instance.Add<MagicBall>();
                magicBall.Owner = Owner;
                magicBall.Target = _target;
                magicBall.Owner.Room = Owner.Room;
                magicBall.Data = data;
                magicBall.PosInfo.State = CreatureState.Moving;
                magicBall.PosInfo.MoveDir = Owner.PosInfo.MoveDir;
                magicBall.PosInfo.PosX = Owner.PosInfo.PosX;
                magicBall.PosInfo.PosY = Owner.PosInfo.PosY;
                magicBall.Speed = data.projectile.speed;
                magicBall.DespawnAnim = true;
                magicBall.TemplateId = data.id;

                if (data.projectile.isRandom)
                {
                    magicBall.DestPos = SkillLogic.GetRandomPos(Owner.CellPos, data.range);
                }

                if (data.debuff != null)
                {
                    magicBall.OnHit = (target) => { HandleDebuffSkill(data, target); };
                }
                Owner.Room.Push(Owner.Room.EnterGame, magicBall, false);
                await Task.Delay((int)(data.term * 1000));
            }
        }
    }
}

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
            await Task.Delay((int)(1000 * Owner.TotalInvokeSpeed));
            for (int i = 0; i < data.count; i++)
            {
                if (Owner == null || Owner.Room == null)
                {
                    return;
                }

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
        public async void ProjectileCurve(SkillData data)
        {
            await Task.Delay((int)(1000 * Owner.TotalInvokeSpeed));
            for (int i = 0; i < data.count; i++)
            {
                Howitzer howitzer = ObjectManager.Instance.Add<Howitzer>();
                howitzer.Owner = Owner;
                howitzer.Owner.Room = Owner.Room;
                howitzer.Data = data;
                howitzer.PosInfo.State = CreatureState.Moving;
                howitzer.PosInfo.MoveDir = Owner.PosInfo.MoveDir;
                howitzer.PosInfo.PosX = Owner.PosInfo.PosX;
                howitzer.PosInfo.PosY = Owner.PosInfo.PosY;
                howitzer.Speed = data.projectile.speed;
                howitzer.DespawnAnim = true;
                howitzer.TemplateId = data.id;

                if (data.projectile.isRandom)
                {
                    howitzer.DestPos = SkillLogic.GetRandomPos(_target.CellPos, (int)data.spot.range);
                }
                else
                {
                    howitzer.DestPos = _target.CellPos;
                }

                if (data.debuff != null)
                {
                    howitzer.OnHit = (target) => { HandleDebuffSkill(data, target); };
                }
                Owner.Room.Push(Owner.Room.EnterGame, howitzer, false);
                await Task.Delay((int)(data.term * 1000));
            }
        }
    }
}

using Google.Protobuf.Protocol;
using System.Collections;
using UnityEngine;

public class SkeletonWarriorController : MonsterController
{
    protected override void Init()
    {
        base.Init();
    }

    protected override void UpdateAnimation()
    {
        if (Animator == null)
        {
            return;
        }
        if (State == CreatureState.Skill)
        {
            switch (LookDir)
            {
                case LookDir.LookLeft:
                    _sprite.flipX = true;
                    break;
                case LookDir.LookRight:
                    _sprite.flipX = false;
                    break;
            }
            if (SkillId == 1)
                Animator.Play("ATTACK");
            else if (SkillId == 9)
                Animator.Play("ATTACK_STRONG");
        }
        else
        {
            base.UpdateAnimation();
        }
    }
}
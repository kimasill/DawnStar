using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrisonKeeperController : MonsterController
{
    int _phase = 1;
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
            if (SkillId == 1)
                Animator.Play("ATTACK");
            else if (SkillId == 17)
                Animator.Play("SKILL");
            else if (SkillId == 19)
                Animator.Play("SKILL");
            else if (SkillId == 18)
            {
                Animator.Play("IDLE");
                _phase = 2;
            }
            switch (LookDir)
            {
                case LookDir.LookLeft:
                    _sprite.flipX = true;
                    break;
                case LookDir.LookRight:
                    _sprite.flipX = false;
                    break;
            }
        }
        if (State == CreatureState.Idle)
        {
            if (_phase == 1)
                Animator.Play("IDLE");
            else if (_phase == 2)
                Animator.Play("IDLE2");
            switch (LookDir)
            {
                case LookDir.LookLeft:
                    _sprite.flipX = true;
                    break;
                case LookDir.LookRight:
                    _sprite.flipX = false;
                    break;
            }
        }
        else
        {
            base.UpdateAnimation();
        }
    }
}
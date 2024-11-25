using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrcMageController : MonsterController
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
            if (SkillId == 11)
            {
                Animator.Play("ATTACK");
            }                
            else if (SkillId == 12 || SkillId == 13)
            {
                Animator.Play("ATTACK_STRONG");        
            }
            SkillId = 0;
        }
        else
        {
            base.UpdateAnimation();
        }
    }
}
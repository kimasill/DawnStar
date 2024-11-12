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
            if (_skillId == 11)
            {
                Animator.Play("ATTACK");
                StartCoroutine(CoStartBasicAttack());
            }                
            else if (_skillId == 12 || _skillId == 13)
            {
                Animator.Play("ATTACK_STRONG");
                StartCoroutine(CoStartBasicAttack());
            }               
        }
        else
        {
            base.UpdateAnimation();
        }
    }
}
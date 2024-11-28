using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SatyrKingController : MonsterController
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
            {
                Animator.Play("ATTACK");
                StartCoroutine(StartEffectCoroutine());
            }

        }
        else
        {
            base.UpdateAnimation();
        }
    }
    IEnumerator StartEffectCoroutine()
    {
        UseEffect("Effect/Pulling");
        yield return new WaitForSeconds(0.4f);
        UseEffect("Effect/Pulling");
    }
}
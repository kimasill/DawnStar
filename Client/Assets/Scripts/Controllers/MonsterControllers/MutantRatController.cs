using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MutantRatController : MonsterController
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
            if(SkillId == 1)
            {
                StartPsychicsCoroutine(PlayBasicAttack());
            }
            else if(SkillId == 31)
            {
                StartPsychicsCoroutine(PlaySkillAttack());
            }
            SkillId = 0;
        }
        else
        {
            base.UpdateAnimation();
        }
    }

    private IEnumerator PlayBasicAttack()
    {
        Animator.Play("ATTACK");
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length / Animator.speed);

        State = CreatureState.Idle;
    }

    private IEnumerator PlaySkillAttack()
    {
        Animator.Play("SKILL");
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length / Animator.speed);


        State = CreatureState.Idle;
    }
}
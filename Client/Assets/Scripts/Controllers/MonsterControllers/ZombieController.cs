using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieController : MonsterController
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
            StartPsychicsCoroutine(PlayBasicAttack());
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


        Animator.Play("IDLE");
        yield return new WaitForSeconds((Animator.GetCurrentAnimatorStateInfo(0).length / Animator.speed) * 2);
    }
}
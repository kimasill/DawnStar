using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotController : MonsterController
{
    bool _isRevealed = false;
    protected override void Init()
    {
        base.Init();

        Animator.Play("REVEAL", 0, 0);
        Animator.speed = 0;

    }
    protected override void UpdateAnimation()
    {
        if (Animator == null)
        {
            return;
        }

        if (State == CreatureState.Idle)
        {
            if (_isRevealed)
            {
                Animator.Play("IDLE");
            }
        }
        else if (State == CreatureState.Skill)
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
            if (SkillId == 13)
            {
                StartPsychicsCoroutine(PlayAnimationClip(Animator, "ATTACK"));
            }
            else if (SkillId == 38)
            {
                StartPsychicsCoroutine(PlayAnimationClip(Animator ,"CANNON"));
            }
            SkillId = 0;
        }
        else if (State == CreatureState.Moving)
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
            if (!_isRevealed)
            {
                _isRevealed = true;
                StartCoroutine(PlayRevealingAnim());
            }
            else
            {
                Animator.Play("WALK");
            }                   
        }
        else
        {
            base.UpdateAnimation();
        }
    }

    private IEnumerator PlayRevealingAnim()
    {
        Animator.speed = 1;
        Animator.Play("REVEAL");

        yield return Util.WaitForAnimation(Animator, "REVEAL");

        Animator.Play("WALK");
    }
}
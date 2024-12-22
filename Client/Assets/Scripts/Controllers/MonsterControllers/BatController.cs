using Google.Protobuf.Protocol;
using System.Collections;
using System.Linq;
using UnityEngine;

public class BatController : MonsterController
{
    private bool _isTakingOff = false;
    private CreatureState _prev = CreatureState.Idle;

    [SerializeField]
    CreatureState CreatureState = CreatureState.Idle;

    protected override void Init()
    {
        base.Init();
    }

    protected override void UpdateAnimation()
    {
        if (Animator == null)
            return;

        switch (LookDir)
        {
            case LookDir.LookLeft:
                _sprite.flipX = true;
                break;
            case LookDir.LookRight:
                _sprite.flipX = false;
                break;
        }
        if (State == CreatureState.Idle)
        {
            CreatureState = CreatureState.Idle;
            string idleAnimation ="IDLE";
            Animator.Play(idleAnimation);
            _prev = CreatureState.Idle;
        }
        else if (State == CreatureState.Moving)
        {
            CreatureState = CreatureState.Moving;
            Animator.Play("WALK");
            _prev = CreatureState.Moving;
        }
        else if (State == CreatureState.Skill)
        {
            if (_prev == CreatureState.Moving)
            {
                StartPsychicsCoroutine(PlayMoveToAttackAnimation());
                _prev = CreatureState.Skill;
            }
            else if(_prev == CreatureState.Skill)
            {
                StartPsychicsCoroutine(PlayAttackAnimation());
            }
        }
        else
        {
            base.UpdateAnimation();
        }
    }
    private IEnumerator PlayMoveToAttackAnimation()
    {
        Animator.Play("MOVETOINSTANCE");
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length / Animator.speed);

        Animator.Play("INSTANCE");
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length / Animator.speed);

        Animator.Play("ATTACK");
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length / Animator.speed);

        State = CreatureState.Idle;
    }

    private IEnumerator PlayAttackAnimation()
    {
        Animator.Play("ATTACK");
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length / Animator.speed);

        Animator.Play("INSTANCE");
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length / Animator.speed * 2);

        State = CreatureState.Idle;
    }
}
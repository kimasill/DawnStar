using Google.Protobuf.Protocol;
using System.Collections;
using UnityEngine;

public class SkeletonShieldWarriorController : MonsterController
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
            if (_skillId == 1)
                Animator.Play("ATTACK");
            else if (_skillId == 9)
                Animator.Play("ATTACK_STRONG");
            else if (_skillId == 10)
            {
                StartCoroutine(DefenseRoutine());
                _skillId = 0;
            }
        }
        else
        {
            base.UpdateAnimation();
        }
    }
    private IEnumerator DefenseRoutine()
    {
        Animator.SetTrigger("Defense");
        yield return new WaitForSeconds(0.1f); // 방어 모션 시작 후 0.1초 대기
        Animator.speed = 0; // 애니메이션 정지
        yield return new WaitForSeconds(3f); // 3초간 정지
        Animator.speed = 1; // 애니메이션 재개
    }
}
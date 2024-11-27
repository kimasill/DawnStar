using Google.Protobuf.Protocol;
using System.Collections;
using UnityEngine;

public class SkeletonMageController : MonsterController
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
            if (SkillId == 20 || SkillId == 23)
            {
                switch (LookDir)
                {
                    case LookDir.LookLeft:
                        
                        if (SkillId == 20)
                        {
                            StartMovementCoroutine(UseSkillRoutine());
                        }
                        else if (SkillId == 23)
                        {
                            Animator.Play("ATTACK");
                        }
                        _sprite.flipX = true;
                        break;
                    case LookDir.LookRight:
                        
                        if (SkillId == 20)
                        {
                            StartMovementCoroutine(UseSkillRoutine());
                        }
                        else if (SkillId == 23)
                        {
                            Animator.Play("ATTACK");
                        }
                        _sprite.flipX = false;
                        break;
                }
            }
        }
        else
        {
            base.UpdateAnimation();
        }
    }

    private IEnumerator UseSkillRoutine()
    {
        // 준비 동작
        Animator.Play("SKILL_PREP");
        float prepAnimationLength = Animator.GetCurrentAnimatorStateInfo(0).length / Animator.speed; // SKILL_PREP 애니메이션의 실제 재생 시간 계산
        yield return new WaitForSeconds(prepAnimationLength);

        // 루프 동작
        Animator.Play("SKILL_LOOP");
        yield return new WaitForSeconds(1.4f);

        // 마무리 동작
        Animator.Play("SKILL_FINISH");
        float finishAnimationLength = Animator.GetCurrentAnimatorStateInfo(0).length / Animator.speed; // SKILL_FINISH 애니메이션의 실제 재생 시간 계산
        yield return new WaitForSeconds(finishAnimationLength);

        Animator.Play("IDLE");
    }
}
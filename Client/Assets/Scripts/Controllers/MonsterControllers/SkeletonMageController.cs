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
            switch (LookDir)
            {
                case LookDir.LookLeft:
                    _sprite.flipX = true;
                    break;
                case LookDir.LookRight:
                    _sprite.flipX = false;
                    break;
            }

            if (SkillId == 20)
            {
                StartCoroutine(UseSkillRoutine());
            }
            else if(SkillId == 23)
            {
                Animator.Play("ATTACK");
            }
            SkillId = 0;
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
        yield return new WaitUntil(() => Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f);

        // 루프 동작
        Animator.Play("SKILL_LOOP");
        yield return new WaitForSeconds(1.4f);

        // 마무리 동작
        Animator.Play("SKILL_FINISH");
        yield return new WaitUntil(() => Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f);
    }
}
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrcController : MonsterController
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
            {
                Animator.Play("ATTACK");
            }
            else if (_skillId == 10)
            {
                StartCoroutine(PlaySkill10Animation());
                _skillId = 0;
            }
        }
        else
        {
            base.UpdateAnimation();
        }
    }

    private IEnumerator PlaySkill10Animation()
    {
        // 첫 번째 애니메이션 실행
        Animator.Play("ATTACK_SPIN_START");
        // 첫 번째 애니메이션이 끝날 때까지 대기
        yield return new WaitUntil(() => Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f);

        // 두 번째 애니메이션 실행 (2초간)
        Animator.Play("ATTACK_SPIN_LOOP");
        yield return new WaitForSeconds(2.0f);

        // 세 번째 애니메이션 실행
        Animator.Play("ATTACK_SPIN_END");
        // 세 번째 애니메이션이 끝날 때까지 대기
        yield return new WaitUntil(() => Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f);
        State = CreatureState.Idle;
    }
}

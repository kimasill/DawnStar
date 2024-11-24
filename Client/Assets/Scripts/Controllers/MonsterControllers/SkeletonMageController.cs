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

            if (_skillId == 1)
            {
                StartCoroutine(UseSkillRoutine());
                _skillId = 0;
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
        yield return new WaitForSeconds(1f); // 준비 동작 시간

        // 루프 동작
        Animator.Play("SKILL_LOOP");
        yield return new WaitForSeconds(2f); // 루프 동작 시간

        // 마무리 동작
        Animator.Play("SKILL_FINISH");
        yield return new WaitForSeconds(1f); // 마무리 동작 시간

        // FireBlaster 소환
        SummonFireBlaster();
    }

    private void SummonFireBlaster()
    {
        Vector3 summonPosition = transform.position + (LookDir == LookDir.LookLeft ? Vector3.left : Vector3.right);
        GameObject fireBlaster = Instantiate(Resources.Load<GameObject>("Magic/FireBlaster"), summonPosition, Quaternion.identity);
        // FireBlaster 초기화 코드 추가 가능
    }
}
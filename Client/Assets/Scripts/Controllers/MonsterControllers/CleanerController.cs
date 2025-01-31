using Google.Protobuf.Protocol;
using System.Collections;
using UnityEngine;

public class CleanerController : MonsterController
{
    private int _skillCount = 0;
    private const int ComboThreshold = 2; // 2회당 한번씩 콤보 사용

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
                _skillCount++;
                if (_skillCount % ComboThreshold == 0)
                {
                    StartPsychicsCoroutine(PlayComboAttack());
                }
                else
                {
                    StartPsychicsCoroutine(PlayAnimationClip(Animator, "ATTACK"));
                }
            }
            else if (SkillId == 28)
            {
                Animator.speed = 1;
                StartPsychicsCoroutine(PlayAnimationClip(Animator, "COMBO"));
                _skillCount = 0;

            }
            SkillId = 0;
        }
        else
        {
            base.UpdateAnimation();
        }
    }
    private IEnumerator PlayComboAttack()
    {
        Animator.Play("ATTACK_COMBO");
        Util.WaitForAnimation(Animator, "ATTACK_COMBO");
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length / Animator.speed);
    }
}
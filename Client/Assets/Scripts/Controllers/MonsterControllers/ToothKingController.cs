using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToothKingController : MonsterController
{
    bool _comboTrigger = false;
    const int _attackSkillId = 9;
    const int _magicSkillId = 39;
    const int _buffSkillId = 40;
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
            if (SkillId == _attackSkillId)
            {
                if(!_comboTrigger)
                {                    
                    StartPsychicsCoroutine(AttackCoroutine());
                }
                else if (_comboTrigger)
                {
                    StartPsychicsCoroutine(PlayComboAttack());
                }
            }
            else if(SkillId == _magicSkillId)
            {
                StartCoroutine(PlaySkill());
            }
            else if(SkillId == _buffSkillId)
            {
                Animator.Play("IDLE");
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
        _sprite.flipX = !_sprite.flipX;
        Animator.Play("ATTACK_COMBO");
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length / Animator.speed);
        _sprite.flipX = !_sprite.flipX;
        State = CreatureState.Idle;
    }
    private IEnumerator PlaySkill()
    {
        Managers.Data.SkillDict.TryGetValue(SkillId, out SkillData skill);
        float duration = skill.duration;

        Animator.Play("SKILL_PREP");
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length / Animator.speed);

        Animator.Play("SKILL_LOOP");
        yield return new WaitForSeconds(duration * 1000);

        Animator.Play("SKILL_END");
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length / Animator.speed);
        State = CreatureState.Idle;
    }

}
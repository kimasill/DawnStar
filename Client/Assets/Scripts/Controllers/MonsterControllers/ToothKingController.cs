using Data;
using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToothKingController : MonsterController
{
    bool _comboTrigger = false;
    const int _attackSkillId = 9;
    const int _magicSkillId = 39;
    const int _buffSkillId = 40;
    string _basicAttackEffectR = "Effect/Boss/SkeletonBossAtk1FX1";
    string _basicAttackEffectL = "Effect/Boss/SkeletonBossAtk2FX2";
    string _loopSkillEffect = "Effect/Boss/SkeletonBossAtk3FX";
    bool _isSkill = false;
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
                if(LookDir == LookDir.LookLeft)
                    StartEffectCoroutine(CoUseEffect(_basicAttackEffectL, delay:1000));
                else if (LookDir == LookDir.LookRight)
                    StartEffectCoroutine(CoUseEffect(_basicAttackEffectR, delay: 1000));
                if (!_comboTrigger)
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
        if(State == CreatureState.Stiff)
        {
            if (!_isSkill)
            {
                switch (LookDir)
                {
                    case LookDir.LookLeft:
                        StartPsychicsCoroutine(PlayAnimationClip(Animator, "HURT"));
                        _sprite.flipX = true;
                        break;
                    case LookDir.LookRight:
                        StartPsychicsCoroutine(PlayAnimationClip(Animator, "HURT"));
                        _sprite.flipX = false;
                        break;
                }
                Managers.Sound.Play("Effect/Hit_Monster");
            }
        }
        else
        {
            base.UpdateAnimation();
        }
    }
    protected override void UpdateStiff()
    {
        if(_isSkill)
        {
            return;
        }
        base.UpdateStiff();
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
        _isSkill = true;
        Animator.Play("SKILL_PREP");
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length / Animator.speed);
        StartEffectCoroutine(CoUseEffect(_loopSkillEffect, duration: duration*1000));
        Animator.Play("SKILL_LOOP");
        yield return new WaitForSeconds(duration * 1000);

        Animator.Play("SKILL_END");
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length / Animator.speed);
        State = CreatureState.Idle;
        _isSkill = false;
    }

}
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrisonKeeperController : MonsterController
{
    int _phase = 1;
    protected override void Init()
    {
        base.Init();
    }
    protected override void UpdateAnimation()
    {
        if (Animator == null || _sprite == null)
        {
            return;
        }
        if (State == CreatureState.Skill)
        {
            if (SkillId == 1)
                StartPsychicsCoroutine(PlayAnimationClip(Animator, "ATTACK"));
            else if (SkillId == 17)
                StartPsychicsCoroutine(PlayAnimationClip(Animator, "SKILL"));
            else if (SkillId == 19)
                StartPsychicsCoroutine(PlayAnimationClip(Animator, "IDLE"));
            else if (SkillId == 18)
            {
                StartPsychicsCoroutine(PlayAnimationClip(Animator, "IDLE"));
                _phase = 2;
            }
            switch (LookDir)
            {
                case LookDir.LookLeft:
                    _sprite.flipX = true;
                    break;
                case LookDir.LookRight:
                    _sprite.flipX = false;
                    break;
            }
        }
        if (State == CreatureState.Idle)
        {
            if (_phase == 1)
                Animator.Play("IDLE");
            else if (_phase == 2)
                Animator.Play("IDLE2");
            switch (LookDir)
            {
                case LookDir.LookLeft:
                    _sprite.flipX = true;
                    break;
                case LookDir.LookRight:
                    _sprite.flipX = false;
                    break;
            }
        }
        else
        {
            base.UpdateAnimation();
        }
    }
}
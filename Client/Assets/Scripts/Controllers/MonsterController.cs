using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class MonsterController : CreatureController
{
    int _nextMoveTick = 0;
    protected override void Init()
	{
		base.Init();
        _nextMoveTick = Environment.TickCount + 200;
    }

	protected override void UpdateIdle()
	{
		base.UpdateIdle();
	}

    protected override void UpdateStiff()
    {
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
            if (SkillId != 0)
                StartPsychicsCoroutine(AttackCoroutine());

            SkillId = 0;
        }
        else
        {
            base.UpdateAnimation();
        }
    }

    public IEnumerator AttackCoroutine()
    {
        Animator.Play("ATTACK");
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length/Animator.speed);
        State = CreatureState.Idle;
    }

    public override void OnDamaged()
	{
        base.OnDamaged();
    }
    public override void UseSkill(S_Skill skill)
    {
        base.UseSkill(skill);
    }

    
}

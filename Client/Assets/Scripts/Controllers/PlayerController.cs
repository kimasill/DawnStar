using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class PlayerController : CreatureController
{
	protected Coroutine _coSkill;
    protected bool _rangedSkill = false;



	protected override void Init()
	{
		base.Init();
	}

    protected override void UpdateAnimation()
    {
        if (_animator == null)
        {
            return;
        }
        if (State == CreatureState.Idle)
        {
            switch (LookDir)
            {
                case LookDir.LookLeft:
                    _animator.Play("IDLE");
                    gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
                    break;
                case LookDir.LookRight:
                    _animator.Play("IDLE");
                    gameObject.transform.rotation = Quaternion.Euler(0, 180, 0);
                    break;
            }
        }
        else if (State == CreatureState.Moving)
        {
            switch (LookDir)
            {
                case LookDir.LookLeft:
                    _animator.Play("WALK");
                    gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
                    break;
                case LookDir.LookRight:
                    _animator.Play("WALK");
                    gameObject.transform.rotation = Quaternion.Euler(0, 180, 0);
                    break;
            }
        }
        else if (State == CreatureState.Skill)
        {
            switch (LookDir)
            {
                case LookDir.LookLeft:
                    _animator.Play(_rangedSkill ? "ATTACK_WEAPON_RIGHT" : "ATTACK");
                    _sprite.flipX = true;
                    break;
                case LookDir.LookRight:
                    _animator.Play(_rangedSkill ? "ATTACK_WEAPON_RIGHT" : "ATTACK");
                    _sprite.flipX = false;
                    break;
            }
        }
        else
        {

        }
    }

    protected override void UpdateController()
	{
		base.UpdateController();
	}
	// 키보드 입력
	
	public override void UseSkill(int skillId)
	{
		if (skillId == 1)
		{
			_coSkill = StartCoroutine("CoStartPunch");
		}
		else if (skillId == 2)
        {
            _coSkill = StartCoroutine("CoStartShootArrow");
        }
        else
        {
            Debug.Log("Invalid skill ID");
        }
	}

	IEnumerator CoStartPunch()
	{
		// 대기 시간
		_rangedSkill = false;
		State = CreatureState.Skill;
		yield return new WaitForSeconds(0.5f);//서버에서도 체크해야함
		State = CreatureState.Idle;
		_coSkill = null;
		CheckUpdatedFlag();

    }
	IEnumerator CoStartShootArrow()
	{
		// 대기 시간
		_rangedSkill = true;
		State = CreatureState.Skill;
		yield return new WaitForSeconds(0.3f);
		State = CreatureState.Idle;
		_coSkill = null;
		CheckUpdatedFlag();
	}
	protected virtual void CheckUpdatedFlag(){ }
	public override void OnDamaged()
	{
		Debug.Log("Player HIT !");
	}
}

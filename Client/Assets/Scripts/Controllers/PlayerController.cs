using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static Define;

public class PlayerController : CreatureController
{
	protected Coroutine _coSkill;
    protected bool _rangedSkill = false;
    protected EquipmentController _equipmentController;
    protected SortingGroup _sortingLayer;

    protected float AttackSpeed { get;  set; }
    public EquipmentController Equipment 
    { 
        get {
            if (_equipmentController == null)
                _equipmentController = GetComponentInChildren<EquipmentController>();
            return 
                _equipmentController; 
        } 
        private set { } 
    }


    protected override void Init()
	{
        base.Init();
        _sortingLayer = GetComponent<SortingGroup>();   
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
            Debug.Log("PlayerController UpdateAnimation State == CreatureState.Skill");
            switch (LookDir)
            {
                case LookDir.LookLeft:
                    _animator.Play(_rangedSkill ? "ATTACK_WEAPON_RIGHT" : "ATTACK");
                    break;
                case LookDir.LookRight:
                    _animator.Play(_rangedSkill ? "ATTACK_WEAPON_RIGHT" : "ATTACK");
                    break;
            }
        }
        else if(State == CreatureState.Stiff)
        {
            switch (LookDir)
            {
                case LookDir.LookLeft:
                    _animator.Play("HURT");
                    gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
                    break;
                case LookDir.LookRight:
                    _animator.Play("HURT");
                    gameObject.transform.rotation = Quaternion.Euler(0, 180, 0);
                    break;
            }
        }
        else if (State == CreatureState.Dead)
        {
            switch (LookDir)
            {
                case LookDir.LookLeft:
                    _animator.Play("DEATH");
                    gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
                    break;
                case LookDir.LookRight:
                    _animator.Play("DEATH");
                    gameObject.transform.rotation = Quaternion.Euler(0, 180, 0);
                    break;
            }
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
			_coSkill = StartCoroutine("CoStartBasicAttack");
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
    private IEnumerator CoStartShootArrow()
    {
        // 대기 시간
        _rangedSkill = true;
        State = CreatureState.Skill;
        yield return StartCoroutine(AdjustAnimation(AttackSpeed));
        State = CreatureState.Idle;
        _coSkill = null;
        CheckUpdatedFlag();
    }
    IEnumerator CoStartBasicAttack()
    {
        // 대기 시간
        _rangedSkill = false;
        State = CreatureState.Skill;
        yield return StartCoroutine(AdjustAnimation(AttackSpeed));
        State = CreatureState.Idle;
        _coSkill = null;
        CheckUpdatedFlag();
    }
    protected virtual void CheckUpdatedFlag(){ }
	public override void OnDamaged()
	{
		Debug.Log("Player HIT !");
        base.OnDamaged();
    }
    protected override void UpdateSortingLayer()
    {
        _sortingLayer.sortingOrder = -CellPos.y;
    }
}

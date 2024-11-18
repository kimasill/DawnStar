using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using static Define;

public class PlayerController : CreatureController
{
	
    protected EquipmentController _equipmentController;
    protected SortingGroup _sortingLayer;
    protected bool _isAttacking = false;
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
    public int EquipDamage { get; protected set; }
    public int EquipDefense { get; protected set; }
    public int EquipAvoidance { get; protected set; }
    public int EquipAccuracy { get; protected set; }
    public int EquipCriticalChance { get; protected set; }
    public int EquipCriticalDamage { get; protected set; }
    public float EquipAttackSpeed { get; protected set; }
    public float EquipSpeed { get; protected set; }
    public float EquipInvokeSpeed { get; protected set; }
    public float EquipCoolTime { get; protected set; }
    public int EquipHp { get; protected set; }
    public int EquipUp { get; protected set; }

    protected override void Init()
	{
        base.Init();
        _sortingLayer = GetComponent<SortingGroup>();   
    }

    protected override void UpdateAnimation()
    {
        if (Animator == null)
        {
            return;
        }
        if (State == CreatureState.Idle)
        {
            switch (LookDir)
            {
                case LookDir.LookLeft:
                    Animator.Play("IDLE");
                    gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
                    break;
                case LookDir.LookRight:
                    Animator.Play("IDLE");
                    gameObject.transform.rotation = Quaternion.Euler(0, 180, 0);
                    break;
            }
        }
        else if (State == CreatureState.Moving)
        {
            switch (LookDir)
            {
                case LookDir.LookLeft:
                    Animator.Play("WALK");
                    gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
                    break;
                case LookDir.LookRight:
                    Animator.Play("WALK");
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
                    Animator.Play(_rangedSkill ? "ATTACK_WEAPON_RIGHT" : "ATTACK");
                    break;
                case LookDir.LookRight:
                    Animator.Play(_rangedSkill ? "ATTACK_WEAPON_RIGHT" : "ATTACK");
                    break;
            }
        }
        else if(State == CreatureState.Stiff)
        {
            switch (LookDir)
            {
                case LookDir.LookLeft:
                    Animator.Play("HURT");
                    gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
                    break;
                case LookDir.LookRight:
                    Animator.Play("HURT");
                    gameObject.transform.rotation = Quaternion.Euler(0, 180, 0);
                    break;
            }
        }
        else if (State == CreatureState.Dead)
        {
            switch (LookDir)
            {
                case LookDir.LookLeft:
                    Animator.Play("DEATH");
                    gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
                    break;
                case LookDir.LookRight:
                    Animator.Play("DEATH");
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
            _coSkill = StartCoroutine("CoStartSkill", skillId);
        }
    }
    private IEnumerator CoStartSkill(int skillId)
    {
        _rangedSkill = false;
        _isAttacking = true;
        SkillData skillData = null;
        Managers.Data.SkillDict.TryGetValue(skillId, out skillData);
        if (skillData == null)
            yield break;

        State = CreatureState.Skill;        
        GameObject skill = Managers.Resource.Instantiate($"{skillData.prefab}", transform);
        skill.transform.position += new Vector3(0, 0.5f, 0);
        SkillController skillController = skill.GetComponent<SkillController>();
        if (skillController == null)
            yield break;

        skillController.Init(skillData, gameObject);
        yield return StartCoroutine(skillController.ExecuteSkill());
        State = CreatureState.Idle;
        _coSkill = null;
        _isAttacking = false;
        CheckUpdatedFlag();
    }
    private IEnumerator CoStartShootArrow()
    {
        // 대기 시간
        _rangedSkill = true;
        _isAttacking = true;
        State = CreatureState.Skill;
        Animator.speed = Stat.AttackSpeed + AdditionalAttackSpeed;
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length);
        Animator.speed = 1.0f;
        State = CreatureState.Idle;
        _coSkill = null;
        _isAttacking = false;
        CheckUpdatedFlag();
    }
    private IEnumerator CoStartBasicAttack()
    {
        // 대기 시간
        _rangedSkill = false;
        _isAttacking = true;
        State = CreatureState.Skill;
        Animator.speed = Stat.AttackSpeed + AdditionalAttackSpeed;
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length);
        Animator.speed = 1.0f;
        State = CreatureState.Idle;
        _coSkill = null;
        _isAttacking = false;
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

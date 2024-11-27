using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
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
    public float EquipUpRegen { get; protected set; }

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
	
	public override void UseSkill(S_Skill skill)
	{
        if (gameObject.activeSelf == false)
        {
            return;
        }
        _isAttacking = true;
        _rangedSkill = false;

        SkillId = skill.Info.SkillId;
        State = CreatureState.Skill;        
        SkillData skillData = null;
        Managers.Data.SkillDict.TryGetValue(SkillId, out skillData);
        if (skillData == null)
            return;
        if (skillData.prefab != null && skillData.IsObject != true)
        {
            UseEffect(skillData, skill.Phase);
        }
        StartMovementCoroutine(CoUseSkill());
    }
    public IEnumerator CoUseSkill()
    {
        AnimatorStateInfo stateInfo = Animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(stateInfo.length / Animator.speed); // SkillData에서 지속 시간 가져오기
        Animator.speed = 1.0f;
        State = CreatureState.Idle;
        UpdateSkillFlag(false);
        CheckUpdatedFlag();
    }
    
    private IEnumerator CoStartShootArrow()
    {
        // 대기 시간
        _rangedSkill = true;
        _isAttacking = true;
        State = CreatureState.Skill;
        Animator.speed = TotalAttackSpeed;
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length);
        Animator.speed = 1.0f;
        State = CreatureState.Idle;
        _coSkill = null;
        _isAttacking = false;
        CheckUpdatedFlag();
    }
    private IEnumerator CoStartBasicAttack()
    {
        Animator.speed = TotalAttackSpeed;
        AnimatorStateInfo stateInfo = Animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(stateInfo.length / Animator.speed); // SkillData에서 지속 시간 가져오기
        Animator.speed = 1.0f;
        State = CreatureState.Idle;
        _coSkill = null;
        UpdateSkillFlag(false);
        CheckUpdatedFlag();
    }
    protected virtual void CheckUpdatedFlag(){ }
	public override void OnDamaged()
	{
		Debug.Log("Player HIT !");
        base.OnDamaged();
    }
    public virtual void UpdateSkillFlag(bool flag)
    {
        if(flag)
        {
            _isAttacking = true;
        }
        else
        {
            _coSkill = null;
            _isAttacking = false;
        }
    }
    protected override void UpdateSortingLayer()
    {
        _sortingLayer.sortingOrder = -CellPos.y * 10;
    }
}

using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing.Text;
using UnityEngine;
using static Define;

public class CreatureController : BaseController
{
	HpBar _hpBar;
	StatInfo _stat = new StatInfo();
    protected Coroutine _coSkill;
    protected bool _rangedSkill = false;
    public float TotalAttackSpeed 
	{
		get { return Stat.AttackSpeed + AttackSpeed; }		
	}
	protected float AttackSpeed { get; set; } = 0.0f;
    protected int _skillId;
    protected string _animation;
    public override StatInfo Stat
	{
		get { return base.Stat; }
		set
		{
			base.Stat = value;
            UpdateHpBar();
		}
	}

    public override int Hp
	{
		get { return Stat.Hp; }
		set 
		{
			base.Stat.Hp = value;
			UpdateHpBar();
		}
	}

	protected virtual void AddHpBar()
	{
		GameObject go = Managers.Resource.Instantiate("UI/HpBar", transform);
		go.transform.localPosition = new Vector3(0, 0.5f, 0);
		go.name = "HpBar";
		_hpBar = go.GetComponent<HpBar>();
		UpdateHpBar();
	}

	protected virtual void UpdateHpBar()
	{
		if (_hpBar == null)
			return;
		float ratio = 0.0f;
        if (Stat.MaxHp >0)
        {
            ratio = ((float)Hp/ Stat.MaxHp);			
        }
        _hpBar.InitializeFrame(Stat.MaxHp);
        _hpBar.SetHpBar(ratio);		
    }	

	protected override void Init()
	{
		base.Init();
		AddHpBar();
    }

	public virtual void OnDamaged()
	{
        State = CreatureState.Stiff;
	}

    public virtual void OnDead()
    {
        State = CreatureState.Dead;
    }
	public virtual void OnHealed()
	{
		_coSkill = StartCoroutine("StartHeal");
    }

	protected IEnumerator StartHeal()
	{
        GameObject heal = Managers.Resource.Instantiate("Effect/Heal", transform);
        heal.transform.position = transform.position + new Vector3(0, 0.5f, 0);
        Animator animator = heal.GetComponent<Animator>();
        animator.Play("START");
        yield return new WaitForSeconds(0.01f);
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        Managers.Resource.Destroy(heal);
    }

    public virtual void UseSkill(int skillId)
    {
        _rangedSkill = false;
        SkillData skillData = null;
		Managers.Data.SkillDict.TryGetValue(skillId, out skillData);
        if (skillData == null)
            return;

        State = CreatureState.Skill;
        if (skillData.skillType == SkillType.SkillAttack) _animation = "ATTACK";
        else if (skillData.skillType == SkillType.SkillBuff) _animation = "BUFF";
        else if (skillData.skillType == SkillType.SkillProjectile) _animation = "PROJECTILE";
        GameObject skill = Managers.Resource.Instantiate($"{skillData.prefab}", transform);
        SkillController skillController = skill.GetComponent<SkillController>();
        if (skillController == null)
            return;
        skillController.Init(skillData, gameObject);
        skillController.ExecuteSkill();
    }

    public bool IsPlayingDieAnimation()
    {
        Animator animator = GetComponent<Animator>();
        if (animator == null)
            return false;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName("DEATH") && stateInfo.normalizedTime < 1.0f;
    }
}

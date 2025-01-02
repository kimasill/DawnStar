using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Define;

public class CreatureController : BaseController
{
	HpBar _hpBar;
    UpBar _upBar;
    StatInfo _stat = new StatInfo();
    protected List<Coroutine> _coSkills = new List<Coroutine>();
    protected Coroutine _coMovement;
    protected bool _rangedSkill = false;

    public float TotalAttackSpeed 
	{
		get { return Stat.AttackSpeed + AdditionalAttackSpeed; }		
	}
    protected int _additionalAttack;
    public virtual int AdditionalAttack { get; set; }

    protected int _additionalDefense;
    public virtual int AdditionalDefense { get; set; }

    protected float _additionalInvokeSpeed;
    public virtual float AdditionalInvokeSpeed { get; set; }

    protected float _additionalCoolTime;
    public virtual float AdditionalCoolTime { get; set; }

    protected int _additionalCriticalChance;
    public virtual int AdditionalCriticalChance { get; set; }

    protected int _additionalCriticalDamage;
    public virtual int AdditionalCriticalDamage { get; set; }

    protected int _additionalAvoidance;
    public virtual int AdditionalAvoidance { get; set; }

    protected int _additionalAccuracy;
    public virtual int AdditionalAccuracy { get; set; }

    protected float _additionalAttackSpeed;
    public virtual float AdditionalAttackSpeed { get; set; }

    protected float _additionalSpeed;
    public virtual float AdditionalSpeed { get; set; }

    protected int _additionalHp;
    public virtual int AdditionalHp { get; set; }

    protected int _additionalUp;
    public virtual int AdditionalUp { get; set; }
    protected float TotalInvokeDelay { get { return Stat.InvokeSpeed + AdditionalInvokeSpeed; } }
    protected int SkillId;
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

    public int Up
    {
        get { return Stat.Up; }
        set
        {
            base.Stat.Up = value;
            UpdateUpBar();
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
    protected void StartSkillCoroutine(IEnumerator coroutine)
    {
        Coroutine co = StartCoroutine(coroutine);
        _coSkills.Add(co);
    }
    protected void StartPsychicsCoroutine(IEnumerator coroutine)
    {
        if (_coMovement != null)
        {
            StopCoroutine(_coMovement);
        }
        _coMovement = StartCoroutine(coroutine);
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

    protected virtual void UpdateUpBar()
    {
        if (_upBar == null)
            return;
        float ratio = 0.0f;
        if (Stat.MaxUp > 0)
        {
            ratio = ((float)Up / Stat.MaxUp);
        }
        _upBar.InitializeFrame(Stat.MaxUp);
        _upBar.SetUpBar(ratio);
    }
    public virtual void RefreshPoints()
    {
        UpdateHpBar();
        UpdateUpBar();
    }
    protected override void Init()
	{
		base.Init();
		AddHpBar();
    }
    protected override void UpdateAnimation()
    {
        if (Animator == null)
        {
            return;
        }
        else if (State == CreatureState.Stiff)
        {
            switch (LookDir)
            {
                case LookDir.LookLeft:
                    StartPsychicsCoroutine(PlayAnimationClip(Animator,"HURT"));
                    _sprite.flipX = true;
                    break;
                case LookDir.LookRight:
                    StartPsychicsCoroutine(PlayAnimationClip(Animator, "HURT"));
                    _sprite.flipX = false;
                    break;
            }
        }
        else
        {
            base.UpdateAnimation();
        }
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
        StartSkillCoroutine(StartHeal());
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

    public virtual void UseSkill(S_Skill skill)
    {
        _rangedSkill = false;
        SkillId = skill.Info.SkillId;
        State = CreatureState.Skill;        
        SkillData skillData = null;
        Managers.Data.SkillDict.TryGetValue(SkillId, out skillData);
        if (skillData == null)
            return;
        if ((skillData.prefab != null || skillData.prefabs != null) && skillData.isObject != true)
        {
            UseSkillEffect(skillData, skill.Phase);
        }
    }
    public async void UseSkillEffect(SkillData skillData, int phase = 0)
    {
        // 스킬 이펙트 생성 및 초기화
        if(TotalInvokeDelay>0)
        {
            await Task.Delay((int)(1000 * TotalInvokeDelay));
        }
        if (gameObject == null)
            return;
        GameObject skillObj = null;
        if (skillData.prefabs!=null)
        {
            if (skillData.fix)
            {
                UseEffect(skillData.prefabs[phase]);
                return;
            }
            else
            {
                skillObj = Managers.Resource.Instantiate($"{skillData.prefabs[phase]}", transform);
            }           
        }
        else
        {
            if (skillData.fix)
            {
                UseEffect(skillData.prefab);
                return;
            }
            else
            {
                skillObj = Managers.Resource.Instantiate($"{skillData.prefab}", transform);
            }   
        }
        SkillController skillController = skillObj.GetComponent<SkillController>();
        if (skillController == null)
            return;

        skillController.Init(skillData, gameObject);
        // 새로운 코루틴 실행
        StartSkillCoroutine(skillController.ExecuteSkill());
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

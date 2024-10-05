using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using UnityEngine;
using static Define;

public class CreatureController : BaseController
{
	HpBar _hpBar;
	StatInfo _stat = new StatInfo();
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

	protected void AddHpBar()
	{
		GameObject go = Managers.Resource.Instantiate("UI/HpBar", transform);
		go.transform.localPosition = new Vector3(0, 0.5f, 0);
		go.name = "HpBar";
		_hpBar = go.GetComponent<HpBar>();
		UpdateHpBar();
	}

	void UpdateHpBar()
	{
		if (_hpBar == null)
			return;
		float ratio = 0.0f;
        if (Stat.MaxHp >0)
        {
            ratio = ((float)Hp/ Stat.MaxHp);			
        }
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
        if (this is MonsterController) // Check if the instance is of type MonsterController
        {
            StartCoroutine(FadeOut());
        }
    }

    private IEnumerator FadeOut()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        Color originalColor = spriteRenderer.color;
        float fadeDuration = 1.0f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
    }

    public virtual void UseSkill(int skillId)
    {
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

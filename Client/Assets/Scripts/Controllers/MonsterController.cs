using Data;
using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class MonsterController : CreatureController
{
    int _nextMoveTick = 0;
    public MonsterGrade Grade { get; private set; }
    public UI_BossHpBar BossHpBar { get; private set; }
    protected override void Init()
    { 
        MonsterData monsterData = null;
        Managers.Data.MonsterDict.TryGetValue(TemplateId, out monsterData);
        if (monsterData == null)
        {
            Debug.Log("Monster Data is null");
            return;
        }
        Grade = monsterData.grade;

        base.Init();
        
        _nextMoveTick = Environment.TickCount + 200;
    }

	protected override void UpdateIdle()
	{
		base.UpdateIdle();
	}
    protected override void AddHpBar()
    {
        if(Grade == MonsterGrade.Boss)
        {
            UI_GameWindow gameWindow = null;
            if (GameScene == null)
            {
                UI_GameScene gameScene = Managers.UI.SceneUI as UI_GameScene;
                gameWindow = gameScene.GameWindow;
            }
            else
            {
                gameWindow = GameScene.GameWindow;
            }

            BossHpBar = gameWindow.BossHpBar;
            BossHpBar.gameObject.SetActive(true);
            BossHpBar.Name.text = name;
            UpdateHpBar();
        }
        else
        {
            base.AddHpBar();
        }   
    }

    public override void RemoveHpBar()
    {
        if(Grade == MonsterGrade.Boss)
        {
            if (BossHpBar != null)
            {
                BossHpBar.gameObject.SetActive(false);
            }
        }
        else
        {
            base.RemoveHpBar();
        }
    }

    protected override void UpdateHpBar()
    {
        if (Grade == MonsterGrade.Boss)
        {
            if (BossHpBar != null && BossHpBar.HPBar != null)
            {
                float ratio = (float)Hp / Stat.MaxHp;
                BossHpBar.HPBar.UpdateHpBar(ratio, Hp, Stat.MaxHp);
            }
        }
        else
        {
            base.UpdateHpBar();
        }
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
        else if (State == CreatureState.Stiff)
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
        else
        {
            base.UpdateAnimation();
        }
    }

    public IEnumerator AttackCoroutine()
    {
        Animator.Play("ATTACK");
        yield return new WaitForEndOfFrame(); // 애니메이션 상태가 변경될 때까지 대기
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length / Animator.speed);
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

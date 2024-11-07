using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class MonsterController : CreatureController
{
    protected override void Init()
	{
		base.Init();
	}

	protected override void UpdateIdle()
	{
		base.UpdateIdle();
	}

    protected override void UpdateStiff()
    {
    }

    public override void OnDamaged()
	{
        base.OnDamaged();
    }

    public override void UseSkill(int skillId)
    { 
        _skillId = skillId;
		State = CreatureState.Skill;        
    }
}

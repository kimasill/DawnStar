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

    public override void OnDamaged()
	{
        base.OnDamaged();
    }
    public override void UseSkill(S_Skill skill)
    {
        base.UseSkill(skill);
    }

    
}

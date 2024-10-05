using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class MonsterController : CreatureController
{
    private long _stiffEndTick = 0;
    //Coroutine _coSkill;
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
        if (_stiffEndTick == 0)
        {
            _stiffEndTick = Environment.TickCount + 1000;
        }
        if (_stiffEndTick > Environment.TickCount)
            return;
        _stiffEndTick = 0;
        State = CreatureState.Idle;
    }

    public override void OnDamaged()
	{
        base.OnDamaged();
    }



    public override void UseSkill(int skillId)
    {
        if (skillId == 1)
        {
			State = CreatureState.Skill;
        }
    }
}

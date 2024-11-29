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

    protected override void UpdateMoving()
    {

        Vector3 destPos = Managers.Map.CurrentGrid.CellToWorld(CellPos) + new Vector3(0.5f, 0.5f);
        Vector3 moveDir = destPos - transform.position;

        // 움직임 보간
        transform.position = Vector3.Lerp(transform.position, destPos, Speed * Time.deltaTime);

        // 도착 여부 체크
        float dist = moveDir.magnitude;
        if (dist < Speed * Time.deltaTime)
        {
            transform.position = destPos;
        }
        else
        {
            State = CreatureState.Moving;
        }

        UpdateSortingLayer();
    }
}

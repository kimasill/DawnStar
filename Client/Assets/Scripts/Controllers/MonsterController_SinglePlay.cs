using Data;
using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class MonsterController_SinglePlay : CreatureController
{
    private PlayerController _target;
    private int _searchRange = 10;
    private int _chaseRange = 15;
    private int _skillRange = 5;
    private long _nextSearchTick;
    private long _nextMoveTick;
    private long _coolTick;

    protected override void Init()
    {
        base.Init();
        State = CreatureState.Idle;
    }

    protected override void UpdateIdle()
    {
        if (_target == null)
        {
            _target = Managers.Object.MyPlayer;
        }
        else
        {
            float distance = Vector3.Distance(transform.position, _target.transform.position);
            if (distance <= _chaseRange)
            {
                State = CreatureState.Moving;
            }
        }
    }

    protected override void UpdateMoving()
    {
        if (_target == null)
        {
            State = CreatureState.Idle;
            return;
        }
        if (_nextMoveTick > Environment.TickCount)
            return;
        int moveTick = (int)(1000 / Speed);
        _nextMoveTick = Environment.TickCount + moveTick;

        List<Vector2Int> path = Managers.Map.FindPath(CellPos, _target.CellPos, true);

        float distance = Vector3.Distance(transform.position, _target.transform.position);
        if (distance <= _skillRange)
        {
            State = CreatureState.Skill;
        }
        else if (distance > _chaseRange)
        {
            _target = null;
            State = CreatureState.Idle;
        }
        else
        {
            // 타겟을 향해 이동
            Vector3 direction = (_target.transform.position - transform.position).normalized;
            transform.position += direction * Time.deltaTime * 3; // 이동 속도
        }
    }

    protected override void UpdateSkill()
    {
        if (_target == null)
        {
            State = CreatureState.Idle;
            return;
        }

        float distance = Vector3.Distance(transform.position, _target.transform.position);
        if (distance > _skillRange)
        {
            State = CreatureState.Moving;
        }
        else
        {
            Skill skillData = null;
            Managers.Data.SkillDict.TryGetValue(1, out skillData);
            // 공격 로직
            _target.OnDamaged();

            int coolTick = (int)(skillData.coolTime * 1000);
            _coolTick += Environment.TickCount + coolTick;
        }
        if (_coolTick > Environment.TickCount)
            return;
        _coolTick = 0;
    }

    protected override void UpdateDead()
    {
        // 죽음 애니메이션 출력
        State = CreatureState.Dead;

        // 일정 시간 후 오브젝트 제거
        Destroy(gameObject, 3.0f);
    }

    public override void OnDamaged()
    {
        // 맞았을 때 애니메이션 출력
        _animator.Play("HURT");

        // HP 감소 로직
        Stat.Hp -= 10; // 예시로 10만큼 감소
        if (Stat.Hp <= 0)
        {
            Stat.Hp = 0;
            State = CreatureState.Dead;
        }

        // HP 바 업데이트
        UpdateHpBar();
    }
    private void UpdateHpBar()
    {
        // HP 바 업데이트 로직
    }
}
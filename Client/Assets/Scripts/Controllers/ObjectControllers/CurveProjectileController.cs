using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class CurveProjectileController : BaseController
{
    private float _timeElapsed;
    private float _duration;
    private Vector3Int _initialCellPos;
    private Vector3 _startPos;
    private Vector3 _endPos;
    float height = 3.0f; // 최대 높이
    private float _maxLifetime = 5.0f; // 최대 지속 시간 (초)
    private float _lifetimeElapsed = 0.0f; // 경과 시간
    public Action AfterAnimationAction { get; set; }
    Coroutine _coroutine;
    protected override void Init()
    {
        State = CreatureState.Idle;
        base.Init();
        _timeElapsed = 0;
        _duration = 0;
        _initialCellPos = CellPos;
        _lifetimeElapsed = 0;
    }
    public override void HandleMovePos(PositionInfo position)
    {
        base.HandleMovePos(position);
        _startPos = Managers.Map.CurrentGrid.CellToWorld(_initialCellPos);
        _endPos = Managers.Map.CurrentGrid.CellToWorld(new Vector3Int(position.PosX, position.PosY, 0)) + new Vector3(0.5f, 0.5f);

        Vector3 dir = _initialCellPos - CellPos;
        float distance = MathF.Abs(dir.x) + MathF.Abs(dir.y);
        _duration = distance / SkillData.projectile.speed;
        _duration *= 0.9f;
        _timeElapsed = 0;
        height = Mathf.Max(distance / 2.0f, height); // 최대 높이
        _maxLifetime = _duration + 2f;
        State = CreatureState.Moving;
    }
    public override void SyncPos()
    {
        if (Managers.Map.CurrentGrid == null)
            return;
        Vector3 destPos = Managers.Map.CurrentGrid.CellToWorld(CellPos) + new Vector3(0.5f, 1f);
        transform.position = destPos;
    }
    public override void RefreshData()
    {
        _initialCellPos = CellPos;
        _startPos = Managers.Map.CurrentGrid.CellToWorld(CellPos);
    }
    protected override void UpdateAnimation()
    {
        if (Animator == null)
        {
            Animator = GetComponent<Animator>();
        }
        Animator.speed = 1;
        Animator.Play("START");
    }
    protected override void UpdateIdle()
    {
        if (_lifetimeElapsed > 0)
        {
            _lifetimeElapsed += Time.deltaTime;
            if (_lifetimeElapsed >= _maxLifetime && gameObject != null)
            {
                StartCoroutine(DespawnAnim());
            }
        }
    }
    protected override void UpdateMoving()
    {
        _timeElapsed += Time.deltaTime;
        _lifetimeElapsed += Time.deltaTime;
        float t = _timeElapsed / _duration;
        
        if (t >= 1f)
        {
            transform.position = _endPos;
            State = CreatureState.Idle;
        }
        else
        {
            float parabola = 4 * height * t * (1 - t); // 포물선 공식
            Vector3 currentPos = Vector3.Lerp(_startPos, _endPos, t) + new Vector3(0, parabola, 0);
            transform.position = currentPos;
            //transform.position = Vector3.Lerp(transform.position, currentPos, SkillData.projectile.speed * Time.deltaTime);
        }

        if (_lifetimeElapsed >= _maxLifetime && gameObject != null)
        {
            StartCoroutine(DespawnAnim());
        }
        UpdateSortingLayer();
    }

    public override IEnumerator DespawnAnim()
    {
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
        }

        if (Animator != null)
        {
            Animator.speed = 1;
            Animator.Play("END");
            yield return Util.WaitForAnimation(Animator, "END");
        }

        if (this != null && gameObject != null)
        {
            gameObject.SetActive(false);
            AfterAnimationAction?.Invoke();
        }
    }
}
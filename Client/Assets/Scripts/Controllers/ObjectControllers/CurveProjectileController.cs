using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections;
using UnityEngine;

public class CurveProjectileController : BaseController
{
    private float _timeElapsed;
    private float _duration;
    private Vector3Int _initialCellPos;
    private Vector3 _startPos;
    private Vector3 _endPos;
    float height = 5.0f; // 최대 높이 (임시)
    public Action AfterAnimationAction { get; set; }
    Coroutine _coroutine;
    protected override void Init()
    {
        State = CreatureState.Idle;
        base.Init();
        _timeElapsed = 0;
    }
    public override void HandleMovePos(PositionInfo position)
    {
        base.HandleMovePos(position);

        _endPos = Managers.Map.CurrentGrid.CellToWorld(CellPos);
        Vector3 dir = CellPos - _initialCellPos;
        float distance = Math.Abs(dir.x) + Math.Abs(dir.y);
        _duration = distance * (1000/SkillData.projectile.speed );
        _endPos += new Vector3(0.5f, 0.5f);
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
    protected override void UpdateMoving()
    {
        _timeElapsed += Time.deltaTime;
        float t = _timeElapsed / _duration;
        
        if (t >= 1.0f)
        {
            transform.position = _endPos;
            State = CreatureState.Idle;
        }
        else
        {
            float parabola = 4 * height * t * (1 - t); // 포물선 공식
            transform.position = Vector3.Lerp(_startPos, _endPos, t) + new Vector3(0, parabola, 0);
            State = CreatureState.Moving;
        }

        UpdateSortingLayer();
    }

    protected override IEnumerator DespawnAnim()
    {
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
        }
        Animator.speed = 1;
        Animator.Play("END");
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length);
        gameObject.SetActive(false);
        AfterAnimationAction?.Invoke();
    }
}
using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseController : MonoBehaviour
{
    public int Id { get; set; }
    StatInfo _stat = new StatInfo();
    public virtual StatInfo Stat
    {
        get { return _stat; }
        set
        {
            if (_stat.Equals(value))
                return;
            _stat.MergeFrom(value);
        }
    }

    public int Exp
    {
        get { return Stat.TotalExp; }
        set { Stat.TotalExp = value; }
    }

    public int Level
    {
        get { return Stat.Level; }
        set { Stat.Level = value; }
    }
    public float Speed
    {
        get { return Stat.Speed; }
        set { Stat.Speed = value; }
    }

    public virtual int Hp
    {
        get { return Stat.Hp; }
        set
        {
            Stat.Hp = value;
        }
    }
    protected bool _updated = false;
    private long _stiffEndTick = 0;
    PositionInfo _positionInfo = new PositionInfo();
    public PositionInfo PosInfo
    {
        get { return _positionInfo; }
        set
        {
            if (_positionInfo.Equals(value))
                return;
            CellPos = new Vector3Int(value.PosX, value.PosY, 0);
            State = value.State;
            Dir = value.MoveDir;
            LookDir = value.LookDir;
        }
    }
    public void SyncPos()
    {
        if (Managers.Map.CurrentGrid == null)
            return;
        Vector3 destPos = Managers.Map.CurrentGrid.CellToWorld(CellPos) + new Vector3(0.5f, 0.5f);
        transform.position = destPos;
    }

    public Vector3Int CellPos
    {
        get
        {
            return new Vector3Int(_positionInfo.PosX, _positionInfo.PosY, 0);
        }
        set
        {
            if (PosInfo.PosX == value.x && PosInfo.PosY == value.y)
                return;
            PosInfo.PosX = value.x;
            PosInfo.PosY = value.y;
            _updated = true;
        }
    }

    protected Animator _animator;
    protected SpriteRenderer _sprite;

    [SerializeField]
    public virtual CreatureState State
    {
        get { return PosInfo.State; }
        set
        {
            if (PosInfo.State == value)
                return;

            PosInfo.State = value;
            Debug.Log("State : " + value);
            UpdateAnimation();
            _updated = true;
        }
    }

    public LookDir LookDir
    {
        get { return PosInfo.LookDir; }
        set
        {
            if (PosInfo.LookDir == value)
                return;           

            PosInfo.LookDir = value;

            UpdateAnimation();
            _updated = true;
        }
    }

    public MoveDir Dir
    {
        get { return PosInfo.MoveDir; }
        set
        {
            if (PosInfo.MoveDir == value)
                return;

            PosInfo.MoveDir = value;

            UpdateAnimation();
            _updated = true;
        }
    }

    public MoveDir GetDirFromVec(Vector3Int dir)
    {
        if (dir.x > 0)
            return MoveDir.Right;
        else if (dir.x < 0)
            return MoveDir.Left;
        else if (dir.y > 0)
            return MoveDir.Up;
        else
            return MoveDir.Down;
    }

    public Vector3Int GetFrontCellPos()
    {
        Vector3Int cellPos = CellPos;

        switch (Dir)
        {
            case MoveDir.Up:
                cellPos += Vector3Int.up;
                break;
            case MoveDir.Down:
                cellPos += Vector3Int.down;
                break;
            case MoveDir.Left:
                cellPos += Vector3Int.left;
                break;
            case MoveDir.Right:
                cellPos += Vector3Int.right;
                break;
        }

        return cellPos;
    }
    protected virtual void UpdateSortingLayer()
    {
        if (_sprite == null)
            return;

        _sprite.sortingOrder = -CellPos.y;
    }
    protected virtual void UpdateAnimation()
    {
        if (_animator == null)
        {
            return;
        }
        if (State == CreatureState.Idle)
        {
            switch (LookDir)
            {
                case LookDir.LookLeft:                    
                    _animator.Play("IDLE");
                    _sprite.flipX = true;
                    break;
                case LookDir.LookRight:
                    _animator.Play("IDLE");
                    _sprite.flipX = false;
                    break;
            }
        }
        else if (State == CreatureState.Moving)
        {
            switch (LookDir)
            {
                case LookDir.LookLeft:
                    _animator.Play("WALK");
                    _sprite.flipX = true;
                    break;
                case LookDir.LookRight:
                    _animator.Play("WALK");
                    _sprite.flipX = false;
                    break;
            }
        }
        else if (State == CreatureState.Skill)
        {
            switch (LookDir)
            {
                case LookDir.LookLeft:
                    _animator.Play("ATTACK");
                    _sprite.flipX = true;
                    break;
                case LookDir.LookRight:
                    _animator.Play("ATTACK");
                    _sprite.flipX = false;
                    break;
            }
        }
        else if (State == CreatureState.Stiff)
        {
            switch (LookDir)
            {
                case LookDir.LookLeft:
                    _animator.Play("HURT");
                    _sprite.flipX = true;
                    break;
                case LookDir.LookRight:
                    _animator.Play("HURT");
                    _sprite.flipX = false;
                    break;
            }
        }
        else if(State == CreatureState.Dead)
        {
            switch (LookDir)
            {
                case LookDir.LookLeft:
                    _animator.Play("DEATH");
                    _sprite.flipX = true;
                    break;
                case LookDir.LookRight:
                    _animator.Play("DEATH");
                    _sprite.flipX = false;
                    break;
            }

            StartCoroutine(DisableAfterDelay(1.0f));
        }
    }
    private IEnumerator DisableAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }

    void Start()
    {
        Init();
    }

    void Update()
    {
        UpdateController();
    }

    protected virtual void Init()
    {
        _animator = GetComponent<Animator>();
        _sprite = GetComponent<SpriteRenderer>();
        Vector3 pos = Managers.Map.CurrentGrid.CellToWorld(CellPos) + new Vector3(0.5f, 0.5f);
        transform.position = pos;
        UpdateAnimation();
    }

    protected virtual void UpdateController()
    {
        switch (State)
        {
            case CreatureState.Idle:
                UpdateIdle();
                break;
            case CreatureState.Moving:
                UpdateMoving();
                break;
            case CreatureState.Skill:
                UpdateSkill();
                break;
            case CreatureState.Stiff:
                UpdateStiff();
                break;
            case CreatureState.Dead:
                UpdateDead();
                break;
        }
    }

    protected virtual void UpdateIdle()
    {
    }
    protected virtual void UpdateStiff()
    {
        if (_stiffEndTick == 0)
        {
            _stiffEndTick = Environment.TickCount + 500;
        }
        if (_stiffEndTick > Environment.TickCount)
            return;
        _stiffEndTick = 0;
        State = CreatureState.Idle;
    }
    // 스르륵 이동하는 것을 처리
    protected virtual void UpdateMoving()
    {
        Vector3 destPos = Managers.Map.CurrentGrid.CellToWorld(CellPos) + new Vector3(0.5f, 0.5f);
        Vector3 moveDir = destPos - transform.position;

        // 도착 여부 체크
        float dist = moveDir.magnitude;
        if (dist < Speed * Time.deltaTime)
        {
            transform.position = destPos;
            MoveToNextPos();
        }
        else
        {
            transform.position += moveDir.normalized * Speed * Time.deltaTime;
            State = CreatureState.Moving;
        }
        UpdateSortingLayer();
    }

    protected virtual void MoveToNextPos()
    {

    }

    protected virtual void UpdateSkill()
    {

    }

    protected virtual void UpdateDead()
    {

    }
}

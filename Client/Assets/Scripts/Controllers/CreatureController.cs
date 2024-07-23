using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class CreatureController : MonoBehaviour
{
    #region Properties
    public float _speed = 5.0f;
    public Vector3Int CellPos{ get; set; } = Vector3Int.zero;
    protected SpriteRenderer _sprite;
    [SerializeField]
    protected CreatureState _state = CreatureState.Idle;
    protected Animator _animator;
    protected MoveDir _dir = MoveDir.Down;
    protected MoveDir _lastDir = MoveDir.Down;
    #endregion

    public virtual CreatureState State
    {
        get { return _state; }
        set
        {
            if (_state == value)
            {
                return;
            }
            _state = value;        
            UpdateAnimation();
        }
    }       
    public MoveDir Dir
    {
        get { return _dir; }
        set
        {
            if (_dir == value)
            {
                return;
            }
            _dir = value;
            if(value != MoveDir.None)
                _lastDir = value;
            UpdateAnimation();
        }
    }

    public MoveDir GetDirFromVec(Vector3Int dir)
    {
        if (dir.x > 0)
        {
            return MoveDir.Right;
        }
        else if (dir.x < 0)
        {
            return MoveDir.Left;
        }
        else if (dir.y > 0)
        {
            return MoveDir.Up;
        }
        else if (dir.y < 0)
        {
            return MoveDir.Down;
        }
        else
            return MoveDir.None;
    }
    public Vector3Int GetFrontCellPos()
    {
        Vector3Int cellpos = CellPos;
        switch (_lastDir)
        {
            case MoveDir.Up:
                cellpos += Vector3Int.up;
                break;
            case MoveDir.Down:
                cellpos += Vector3Int.down;
                break;
            case MoveDir.Left:
                cellpos += Vector3Int.left;
                break;
            case MoveDir.Right:
                cellpos += Vector3Int.right;
                break;
        }
        return cellpos;
    }
    protected virtual void UpdateAnimation()
    {
        if (_state == CreatureState.Moving)
        {
            switch (_dir)
            {
                case MoveDir.Up:
                    _animator.Play("WALK_BACK");
                    _sprite.flipX = false;
                    break;
                case MoveDir.Down:
                    _animator.Play("WALK_FRONT");
                    _sprite.flipX = false;
                    break;
                case MoveDir.Left:
                    _animator.Play("WALK_RIGHT");
                    _sprite.flipX = true;
                    break;
                case MoveDir.Right:
                    _animator.Play("WALK_RIGHT");
                    _sprite.flipX = false;
                    break;
            }
        }
        else if (_state == CreatureState.Idle)
        {
            switch (_lastDir)
            {
                case MoveDir.Up:
                    _animator.Play("IDLE_BACK");
                    _sprite.flipX = false;
                    break;
                case MoveDir.Down:
                    _animator.Play("IDLE_FRONT");
                    _sprite.flipX = false;
                    break;
                case MoveDir.Left:
                    _animator.Play("IDLE_RIGHT");
                    _sprite.flipX = true;
                    break;
                case MoveDir.Right:
                    _animator.Play("IDLE_RIGHT");
                    _sprite.flipX = false;
                    break;
            }
        }
        else if (_state == CreatureState.Skill)
        {
            switch (_lastDir)
            {
                case MoveDir.Up:
                    _animator.Play("ATTACK_BACK");
                    _sprite.flipX = false;
                    break;
                case MoveDir.Down:
                    _animator.Play("ATTACK_FRONT");
                    _sprite.flipX = false;
                    break;
                case MoveDir.Left:
                    _animator.Play("ATTACK_RIGHT");
                    _sprite.flipX = true;
                    break;
                case MoveDir.Right:
                    _animator.Play("ATTACK_RIGHT");
                    _sprite.flipX = false;
                    break;
            }
        }
        else if (_state == CreatureState.Dead)
        {
            //_animator.Play("DIE");
        }
    }

    void Start()
    {
        InIt();
    }
    protected virtual void InIt()
    {
        _animator = GetComponent<Animator>();
        _sprite = GetComponent<SpriteRenderer>();
        Vector3 pos = Managers.Map.CurrentGrid.CellToWorld(CellPos) + new Vector3(0.5f, 0.5f);
        transform.position = pos;
    }
    protected virtual void UpdateController()
    {
        switch(State)
        {
            case CreatureState.Idle:
                UpdateIdle();
                break;
            case CreatureState.Moving:
                UpdatePosition();
                break;
            case CreatureState.Skill:
                break;
            case CreatureState.Dead:
                break;
        }
        
        
    }
    // Update is called once per frame
    void Update()
    {
        UpdateController();
    }
    protected virtual void UpdateIdle(){ }
    protected virtual void UpdatePosition()
    {        
        Vector3 destPos = Managers.Map.CurrentGrid.CellToWorld(CellPos) + new Vector3(0.5f, 0.5f);
        Vector3 moveDir = destPos - transform.position;

        //방향벡터크기 -> 도착 여부 체크
        float dist = moveDir.magnitude;
        if (dist < _speed * Time.deltaTime)
        {
            transform.position = destPos;
            MoveToNextPosition();
        }
        else
        {
            transform.position += moveDir.normalized * _speed * Time.deltaTime;
            _state = CreatureState.Moving;
        }
    }

    protected virtual void MoveToNextPosition()
    {
        if (_dir == MoveDir.None)
        {
            State = CreatureState.Idle;
            return;
        }

        Vector3Int destPos = CellPos;
        switch (_dir)
        {
            case MoveDir.Up:
                destPos += Vector3Int.up;
                break;
            case MoveDir.Down:
                destPos += Vector3Int.down;
                break;
            case MoveDir.Left:
                destPos += Vector3Int.left;
                break;
            case MoveDir.Right:
                destPos += Vector3Int.right;
                break;
        }
        if (Managers.Map.CanGo(destPos))
        {
            if (Managers.Object.Find(destPos) == null)
            {
                CellPos = destPos;
            }
        }
    }
    protected virtual void UpdateSkill(){ }
    protected virtual void UpdateDead(){ }

    public virtual void OnDamaged(){ }
}

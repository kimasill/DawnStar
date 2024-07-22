using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class CreatureController : MonoBehaviour
{
    public float _speed = 5.0f;
    protected Vector3Int _cellPos = Vector3Int.zero;
    protected SpriteRenderer _sprite;

    public CreatureState _state = CreatureState.Idle;
    public CreatureState State
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

    protected Animator _animator;
    public MoveDir _dir = MoveDir.Down;
    MoveDir _lastDir = MoveDir.Down;
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
            switch (_dir)
            {
                case MoveDir.Up:
                    _animator.Play("ATTACK_BACK");
                    break;
                case MoveDir.Down:
                    _animator.Play("ATTACK_FRONT");
                    break;
                case MoveDir.Left:
                    _animator.Play("ATTACK_RIGHT");
                    break;
                case MoveDir.Right:
                    _animator.Play("ATTACK_RIGHT");
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
        Vector3 pos = Managers.Map.CurrentGrid.CellToWorld(_cellPos) + new Vector3(0.5f, 0.5f);
        transform.position = pos;
    }
    protected virtual void UpdateController()
    {
        UpdatePosition();
        UpdateIsMoving();
    }
    // Update is called once per frame
    void Update()
    {
        UpdateController();
    }  
    void UpdatePosition()
    {
        if (State != CreatureState.Moving)
            return;
        Vector3 destPos = Managers.Map.CurrentGrid.CellToWorld(_cellPos) + new Vector3(0.5f, 0.5f);
        Vector3 moveDir = destPos - transform.position;

        //방향벡터크기 -> 도착 여부 체크
        float dist = moveDir.magnitude;
        if (dist < _speed * Time.deltaTime)
        {
            transform.position = destPos;
            //animation 변경
            _state = CreatureState.Idle;
            if(_dir == MoveDir.None)
                UpdateAnimation();
        }
        else
        {
            transform.position += moveDir.normalized * _speed * Time.deltaTime;
            _state = CreatureState.Moving;
        }
    }

    void UpdateIsMoving()
    {
        if (State == CreatureState.Idle && _dir != MoveDir.None)
        {
            Vector3Int destPos = _cellPos;
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
                _cellPos = destPos;
                State = CreatureState.Moving;
            }
        }
    }
}

using Data;
using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class BaseController : MonoBehaviour
{
    public int Id { get; set; }
    StatInfo _stat = new StatInfo();
    protected List<Coroutine> _coEffects = new List<Coroutine>();
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

    public virtual int Exp
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
    protected bool _sorted = false;
    protected int _individualSortingOrder = 0;
    private long _stiffEndTick = 0;
    protected float _attackTime;
    PositionInfo _positionInfo = new PositionInfo();
    public SkillData SkillData { get; set; }
    protected UI_GameScene GameScene { get; private set; }
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
    public virtual void HandleMovePos(PositionInfo position)
    {
        PosInfo = position;
    }

    public virtual void RefreshData()
    {
    }

    public virtual void UpdatePositionSmooth()
    {
        Vector3 destPos = Managers.Map.CurrentGrid.CellToWorld(CellPos) + new Vector3(0.5f, 0.5f);
        transform.position = Vector3.Lerp(transform.position, destPos, 0.2f);
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

    public Animator Animator;
    protected float _animatorSpeed;
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

        if(!_sorted)
        {
            _individualSortingOrder = _sprite.sortingOrder;
            _sorted = true;
        }
        _sprite.sortingOrder = -CellPos.y * 10 + _individualSortingOrder;
    }
    protected virtual void UpdateAnimation()
    {
        if (Animator == null)
        {
            return;
        }
        if (State == CreatureState.Idle)
        {
            switch (LookDir)
            {
                case LookDir.LookLeft:                    
                    Animator.Play("IDLE");
                    _sprite.flipX = true;
                    break;
                case LookDir.LookRight:
                    Animator.Play("IDLE");
                    _sprite.flipX = false;
                    break;
            }
        }
        else if (State == CreatureState.Moving)
        {
            switch (LookDir)
            {
                case LookDir.LookLeft:
                    Animator.Play("WALK");
                    _sprite.flipX = true;
                    break;
                case LookDir.LookRight:
                    Animator.Play("WALK");
                    _sprite.flipX = false;
                    break;
            }
        }
        else if (State == CreatureState.Skill)
        {
            switch (LookDir)
            {
                case LookDir.LookLeft:
                    Animator.Play("ATTACK");
                    _sprite.flipX = true;
                    break;
                case LookDir.LookRight:
                    Animator.Play("ATTACK");
                    _sprite.flipX = false;
                    break;
            }
        }
        else if (State == CreatureState.Stiff)
        {
            switch (LookDir)
            {
                case LookDir.LookLeft:                    
                    Animator.Play("HURT");                    
                    _sprite.flipX = true;
                    break;
                case LookDir.LookRight:
                    Animator.Play("HURT");
                    _sprite.flipX = false;
                    break;
            }
        }
        else if(State == CreatureState.Dead)
        {
            switch (LookDir)
            {
                case LookDir.LookLeft:
                    Animator.Play("DEATH");
                    _sprite.flipX = true;
                    break;
                case LookDir.LookRight:
                    Animator.Play("DEATH");
                    _sprite.flipX = false;
                    break;
            }

            StartCoroutine(DisableAfterDelay(Animator.GetCurrentAnimatorStateInfo(0).length));
        }
    }
    protected virtual IEnumerator DisableAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }
    public virtual IEnumerator DespawnAnim()
    {
        yield return null;
    }

    protected virtual IEnumerator PlayAnimationClip(Animator animator, string clip, Action action = null)
    {
        Animator.Play(clip);
        float AnimationLength = Animator.GetCurrentAnimatorStateInfo(0).length / Animator.speed; // SKILL_PREP 애니메이션의 실제 재생 시간 계산
        yield return new WaitForSeconds(AnimationLength);
        if(action != null)
            action.Invoke();
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
        Animator = GetComponent<Animator>();
        if(Animator != null)
            _animatorSpeed = Animator.speed;
        _sprite = GetComponent<SpriteRenderer>();
        Vector3 pos = Managers.Map.CurrentGrid.CellToWorld(CellPos) + new Vector3(0.5f, 0.5f);
        transform.position = pos;

        GameScene = Managers.UI.SceneUI as UI_GameScene;
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

    public void ShowDamage(int damage, bool isCritical)
    {
        GameObject go = Managers.Resource.Instantiate("UI/Damage");
        Damage uiDamage = go.GetComponent<Damage>();
        uiDamage.SetDamage(damage, isCritical);
        uiDamage.ShowDamage(transform.position);
    }

    protected virtual void MoveToNextPos(){}
    protected virtual void StartEffectCoroutine(IEnumerator coroutine)
    {
        Coroutine co = StartCoroutine(coroutine);
        _coEffects.Add(co);
    }
    public virtual void ActivateSkillEffect(string prefab, int skillId)
    {
        SkillData skillData = null;
        Managers.Data.SkillDict.TryGetValue(skillId, out skillData);
        if (skillData == null)
            return;

        StartEffectCoroutine(CoUseSkillEffect(prefab, skillData.duration));
    }
    public virtual void UseEffect(string prefab)
    {
        StartEffectCoroutine(CoUseEffect(prefab));
    }
    public virtual IEnumerator CoUseSkillEffect(string prefab, float duration)
    {
        GameObject effect = Managers.Resource.Instantiate(prefab, transform);
        Animator animator = effect.GetComponent<Animator>();
        animator.Play("START");
        yield return new WaitForSeconds(duration);
        Managers.Resource.Destroy(effect);
    }
    public virtual IEnumerator CoUseEffect(string prefab, int delay = 0, float duration = 0)
    {
        if (delay > 0)
            yield return new WaitForSeconds(delay);
        GameObject effect = Managers.Resource.Instantiate(prefab, transform);

        int dir = 1;
        if (LookDir == LookDir.LookLeft)
            dir = -1;

        effect.transform.localPosition = new Vector3(effect.transform.localPosition.x * dir , effect.transform.localPosition.y, 0);
        Animator animator = effect.GetComponent<Animator>();
        animator.Play("START");
        if(duration > 0)
            yield return new WaitForSeconds(duration);
        else
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f);
        }
        Managers.Resource.Destroy(effect);
    }
    protected bool CheckAnimationClip(string inspectorName)
    {
        if (Animator == null)
            return false;

        // Base Layer의 현재 상태 정보를 가져옵니다.
        AnimatorStateInfo stateInfo = Animator.GetCurrentAnimatorStateInfo(0);

        // 현재 상태의 이름이 inspectorName과 일치하는지 확인합니다.
        return stateInfo.IsName(inspectorName);
    }
    protected virtual void UpdateSkill()
    {

    }

    protected virtual void UpdateDead()
    {

    }
}

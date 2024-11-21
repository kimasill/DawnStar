using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class SkillController : MonoBehaviour
{
    public SkillData SkillData { get; private set; }
    public CreatureController User { get; private set; }
    SpriteRenderer _sprite;
    Animator _animator;
    public void Init(SkillData skillData, GameObject user)
    {
        SkillData = skillData;
        User = user.GetComponent<CreatureController>();
        _sprite = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
    }
    public IEnumerator ExecuteSkill()
    {
        if (SkillData == null || User == null)
            yield break; 
        UpdateAnimation();

        if (SkillData.skillLogicType == SkillLogicType.Combat)
        {
            UpdateUserSkillFlag();
        }
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(stateInfo.length / _animator.speed);

        if(User.State != CreatureState.Idle)
        {
            UpdateUserSkillFlag();
        }

        Managers.Resource.Destroy(gameObject);
    }
    private void UpdateUserSkillFlag()
    {
        User.State = CreatureState.Idle;
        if (User is PlayerController)
        {
            PlayerController pc = User as PlayerController;
            pc.UpdateSkillFlag(false);
        }
    }
    public void UpdateAnimation()
    {
        _animator.speed = User.TotalAttackSpeed;
        float range = SkillData.shape.range;

        switch (User.PosInfo.MoveDir)
        {
            case MoveDir.Up:
                transform.localPosition = new Vector3(0, range / 2, 0);
                _sprite.flipX = false;
                break;
            case MoveDir.Down:
                transform.localPosition = new Vector3(0, -range / 2 , 0);
                _sprite.flipX = false;
                break;
            case MoveDir.Left:
                transform.localPosition = new Vector3(-range / 2, 0, 0);
                _sprite.flipX = false;
                break;
            case MoveDir.Right:
                transform.localPosition = new Vector3(-range / 2, 0, 0);
                _sprite.flipX = false;
                break;
        }
        transform.localPosition+= new Vector3(0, 0.5f, 0); 
    }
}

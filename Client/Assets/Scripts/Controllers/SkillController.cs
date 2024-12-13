using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using UnityEditor;
using UnityEngine;

public class SkillController : MonoBehaviour
{
    public SkillData SkillData { get; private set; }
    public CreatureController User { get; private set; }
    SpriteRenderer _sprite;
    Animator _animator;
    bool _isSkillEnd = false; // 스킬 종료 여부 확인 변수 추가
    float posX = 0;
    float posY = 0;

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
        if (User is PlayerController)
        {
            if (SkillData.skillLogicType == SkillLogicType.Combat)
            {
                _animator.speed = User.TotalAttackSpeed;
            }
            else
            {
                _animator.speed = 1;
            }
        }
        // 스킬 애니메이션 재생
        PlayAnimation();

        // 스킬 로직 실행
        yield return StartCoroutine(ProcessSkillLogic());

        Managers.Resource.Destroy(gameObject);
    }
    private IEnumerator ProcessSkillLogic()
    {
        if (SkillData.skillLogicType == SkillLogicType.Combat)
        {
            UpdateUserSkillFlag();
        }
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        if (SkillData.effectDuration>0)
        {
            yield return new WaitForSeconds(SkillData.effectDuration * 1000);
        }
        else
        {
            yield return new WaitForSeconds(stateInfo.length / _animator.speed);
        }
        if (User.State != CreatureState.Idle)
        {
            UpdateUserSkillFlag();
        }
    }
    private void PlayAnimation()
    {
        int sign = 1;
        if (User is PlayerController)
            sign = -1;

        posX = transform.localPosition.x;
        posY = transform.localPosition.y;

        switch (User.PosInfo.MoveDir)
        {
            case MoveDir.Up:
                transform.localPosition = new Vector3(0, posX, 0);
                if(SkillData.skillType == SkillType.SkillAttack)
                {
                    transform.rotation = Quaternion.Euler(0, 0, 90);
                }
                _sprite.flipX = false;
                break;
            case MoveDir.Down:
                transform.localPosition = new Vector3(0, -posX, 0);
                if (SkillData.skillType == SkillType.SkillAttack)
                {
                    transform.rotation = Quaternion.Euler(0, 0, -90);
                }
                _sprite.flipX = false;
                break;
            case MoveDir.Left:
                    transform.localPosition = new Vector3(-posX, posY, 0);
                    transform.rotation = Quaternion.Euler(0, 180, 0);
                    _sprite.flipX = false;
                break;
            case MoveDir.Right:
                    transform.localPosition = new Vector3(sign*posX, posY, 0);
                    transform.rotation = Quaternion.Euler(0, 0, 0);
                    _sprite.flipX = false;
                break;
        }
    }
    private void UpdateUserSkillFlag()
    {
        if (User is PlayerController)
        {
            PlayerController pc = User as PlayerController;
            pc.UpdateSkillFlag(false);            
        }
    }
}

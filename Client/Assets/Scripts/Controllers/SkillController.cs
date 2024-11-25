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

        // 스킬 지속 시간 동안 대기
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(stateInfo.length / _animator.speed); // SkillData에서 지속 시간 가져오기

        if (User.State != CreatureState.Idle)
        {
            UpdateUserSkillFlag();
        }
    }
    private void PlayAnimation()
    {
        // 스킬 애니메이션 재생
        _animator.speed = User.TotalAttackSpeed;
        float range = SkillData.shape.range;

        switch (User.PosInfo.MoveDir)
        {
            case MoveDir.Up:
                transform.localPosition = new Vector3(0, range / 2, 0);
                transform.rotation = Quaternion.Euler(0, 0, -90);
                _sprite.flipX = false;
                break;
            case MoveDir.Down:
                transform.localPosition = new Vector3(0, -range / 2, 0);
                transform.rotation = Quaternion.Euler(0, 0, 90);
                _sprite.flipX = false;
                break;
            case MoveDir.Left:
                transform.localPosition = new Vector3(-range / 2, 0, 0);
                transform.rotation = Quaternion.Euler(0, 0, 0);
                _sprite.flipX = false;
                break;
            case MoveDir.Right:
                transform.localPosition = new Vector3(-range / 2, 0, 0);
                transform.rotation = Quaternion.Euler(0, 0, 0);
                _sprite.flipX = false;
                break;
        }
        transform.localPosition += new Vector3(0, 0.5f, 0);
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
}

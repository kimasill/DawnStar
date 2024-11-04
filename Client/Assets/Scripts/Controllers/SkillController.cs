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
        _sprite = User.GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
    }
    public void ExecuteSkill()
    {
        if (SkillData == null || User == null)
            return;

        User.Animator.StopPlayback(); // Stop the current animation
        UpdateAnimation();
        Managers.Resource.Destroy(gameObject);
    }

    public IEnumerator UpdateAnimation()
    {
        _animator.speed = User.TotalAttackSpeed;
        if (User.PosInfo.LookDir == LookDir.LookLeft)
        {
            transform.localPosition = new Vector3(-SkillData.shape.range, 0, 0);
            _sprite.flipX = true;
        }
        else
        {
            transform.localPosition = new Vector3(SkillData.shape.range, 0, 0);
            _sprite.flipX = false;
        }

        _animator.Play("START");
        yield return new WaitForSeconds(0.01f);
        yield return new WaitForSeconds(User.Animator.GetCurrentAnimatorStateInfo(0).length);
    }
}

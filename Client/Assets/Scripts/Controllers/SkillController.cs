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
        yield return new WaitForSeconds(0.01f);
        yield return new WaitForSeconds(User.Animator.GetCurrentAnimatorStateInfo(0).length);
        
        Managers.Resource.Destroy(gameObject);
    }

    public void UpdateAnimation()
    {
        _animator.speed = User.Stat.AttackSpeed;
        float range = SkillData.shape.range;
        if (User.PosInfo.LookDir == LookDir.LookLeft)
        {
            transform.localPosition = new Vector3(-range, 0, 0);
            _sprite.flipX = true;
        }
        else if((User.PosInfo.LookDir == LookDir.LookRight))
        {
            transform.localPosition = new Vector3(-range, 0, 0);
            _sprite.flipX = false;
        } 
    }
}

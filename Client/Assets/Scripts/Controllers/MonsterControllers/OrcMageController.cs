using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrcMageController : MonsterController
{
    protected override void Init()
    {
        base.Init();
    }
    protected override void UpdateAnimation()
    {
        if (_animator == null)
        {
            return;
        }
        if (State == CreatureState.Skill)
        {
            switch (LookDir)
            {
                case LookDir.LookLeft:
                    _sprite.flipX = true;
                    break;
                case LookDir.LookRight:
                    _sprite.flipX = false;
                    break;
            }
            if (_skillId == 1)
                _animator.Play("ATTACK");
            else if (_skillId == 9)
                _animator.Play("ATTACK_STRONG");
        }
        else
        {
            base.UpdateAnimation();
        }
    }
}
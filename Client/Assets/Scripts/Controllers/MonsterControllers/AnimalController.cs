using Google.Protobuf.Protocol;
using System.Collections;
using UnityEngine;

public class AnimalController : MonsterController
{
    private bool _isTakingOff = false;
    private bool _isLanding = false;
    private float _idleAnimationTimer = 0f;
    private float _idleAnimationInterval = 1f; // 1초 간격
    protected override void Init()
    {
        base.Init();
    }

    protected override void UpdateAnimation()
    {
        if (_animator == null)
            return;

        switch (LookDir)
        {
            case LookDir.LookLeft:
                _sprite.flipX = true;
                break;
            case LookDir.LookRight:
                _sprite.flipX = false;
                break;
        }

        if (State == CreatureState.Idle)
        {
            _idleAnimationTimer += Time.deltaTime;
            if (_idleAnimationTimer >= _idleAnimationInterval)
            {
                _idleAnimationTimer = 0f;
                // 3개의 IDLE 상태 랜덤 출력
                string idleAnimation = Random.Range(0, 3) == 0 ? "IDLE_1" : Random.Range(0, 3) == 1 ? "IDLE_2" : "IDLE_3";
                _animator.Play(idleAnimation);
            }
        }
        else if (State == CreatureState.Moving)
        {
            _animator.Play("WALK");
        }
        else if (State == CreatureState.Dead)
        {
            StartCoroutine(DisableAfterDelay(1.0f));
        }
    }
}
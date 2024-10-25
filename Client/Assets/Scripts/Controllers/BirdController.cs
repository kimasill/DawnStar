using Google.Protobuf.Protocol;
using System.Collections;
using UnityEngine;

public class BirdController : MonsterController
{
    private bool _isTakingOff = false;
    private bool _isLanding = false;
    private float _idleAnimationTimer = 0f;
    private float _idleAnimationInterval = 1f; // 1ÃÊ °£°Ý

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
            if (_isLanding)
            {
                StartCoroutine(PlayLandingAnimation());
            }
            else
            {
                _idleAnimationTimer += Time.deltaTime;
                if (_idleAnimationTimer >= _idleAnimationInterval)
                {

                    string idleAnimation = Random.Range(0, 2) == 0 ? "IDLE_1" : "IDLE_2";
                    _animator.Play(idleAnimation);
                }
            }
        }
        else if (State == CreatureState.Moving)
        {
            if (!_isTakingOff)
            {
                StartCoroutine(PlayTakeOffAnimation());
            }
            else
            {
                _animator.Play("WALK");
            }
        }
        else if (State == CreatureState.Dead)
        {
            StartCoroutine(DisableAfterDelay(0.1f));
        }
    }

    private IEnumerator DisableAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }

    private IEnumerator PlayTakeOffAnimation()
    {
        _isTakingOff = true;
        _animator.Play("LIFTING");
        yield return new WaitForSeconds(_animator.GetCurrentAnimatorStateInfo(0).length);
        _isTakingOff = false;
    }

    private IEnumerator PlayLandingAnimation()
    {
        _isLanding = true;
        _animator.Play("LANDING");
        yield return new WaitForSeconds(_animator.GetCurrentAnimatorStateInfo(0).length);
        _isLanding = false;
    }
}
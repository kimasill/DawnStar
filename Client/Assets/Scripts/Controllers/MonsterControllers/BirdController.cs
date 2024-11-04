using Google.Protobuf.Protocol;
using System.Collections;
using System.Linq;
using UnityEngine;

public class BirdController : MonsterController
{
    private bool _isTakingOff = false;
    private CreatureState _prev = CreatureState.Idle;
    private float _idleAnimationTimer = 0f;
    private float _idleAnimationInterval = 1f; // 1√  ∞£∞›
    [SerializeField]
    CreatureState CreatureState = CreatureState.Idle;

    protected override void Init()
    {
        base.Init();
    }

    protected override void UpdateAnimation()
    {
        if (Animator == null)
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
            CreatureState = CreatureState.Idle;
            if (_prev == CreatureState.Moving)
            {
                StartCoroutine(PlayLandingAndIdleAnimation());
            }
            else
            {
                string idleAnimation = Random.Range(0, 2) == 0 ? "IDLE1" : "IDLE2";
                Animator.Play(idleAnimation);
            }
            _prev = CreatureState.Idle;
        }
        else if (State == CreatureState.Moving)
        {
            CreatureState = CreatureState.Moving;
            if (_prev == CreatureState.Idle)
            {                
                StartCoroutine(PlayLiftingAndWalkAnimation());
            }
            else
            {
                Animator.Play("WALK");                
            }
            _prev = CreatureState.Moving;
        }
        else if (State == CreatureState.Dead)
        {
            StartCoroutine(DisableAfterDelay(0.1f));
        }
    }
    private IEnumerator PlayLandingAndIdleAnimation()
    {
        Animator.Play("LANDING", 0, 0);
        yield return new WaitForSeconds(0.01f);
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length);
        string idleAnimation = Random.Range(0, 2) == 0 ? "IDLE1" : "IDLE2";
        Animator.Play(idleAnimation);
    }

    private IEnumerator PlayLiftingAndWalkAnimation()
    {
        Animator.Play("LIFTING", 0, 0);
        yield return new WaitForSeconds(0.01f);
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length);
        Animator.Play("WALK");
    }
}
using Google.Protobuf.Protocol;
using System;
using System.Collections;
using UnityEngine;

public class LoopMagicController : BaseController
{
    public Action AfterAnimationAction { get; set; }
    Coroutine _coroutine;
    protected override void Init()
    {
        State = CreatureState.Moving;
        base.Init();
    }

    protected override void UpdateAnimation()
    {
        if (Animator == null)
        {
            Animator = GetComponent<Animator>();
        }
        _coroutine = StartCoroutine(StartLoopAnim());
    }
    protected IEnumerator StartLoopAnim()
    {
        Animator.speed = 1;
        Animator.Play("START");
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length);

        Animator.Play("LOOP");        
    }

    public override IEnumerator DespawnAnim()
    {
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
        }
        if (this == null || Animator == null)
        {
            yield break;
        }
        Animator.speed = 1;
        Animator.Play("END");
        yield return new WaitForEndOfFrame();
        if(Animator == null)
        {
            yield break;
        }
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length - 0.05f);
        if (this != null)
        {
            gameObject.SetActive(false);
            AfterAnimationAction?.Invoke();
        }
    }
}
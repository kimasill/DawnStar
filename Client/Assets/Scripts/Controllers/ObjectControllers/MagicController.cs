using Google.Protobuf.Protocol;
using System;
using System.Collections;
using UnityEngine;

public class MagicController : BaseController
{
    public Action AfterAnimationAction { get; set; }
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
        StartCoroutine(PlayAnimationAndDisappear());
    }
    protected IEnumerator PlayAnimationAndDisappear()
    {
        Animator.speed = 1;
        Animator.Play("START");
        if(SkillData.sound != null)
            Managers.Sound.Play($"{SkillData.sound}");
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length);
        gameObject.SetActive(false);
        AfterAnimationAction?.Invoke();
    }
}
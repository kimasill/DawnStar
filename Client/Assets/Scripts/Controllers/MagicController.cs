using Google.Protobuf.Protocol;
using System.Collections;
using UnityEngine;

public class MagicController : BaseController
{
    protected override void Init()
    {
        State = CreatureState.Moving;
        base.Init();
    }

    protected override void UpdateAnimation()
    {
        StartCoroutine(PlayAnimationAndDisappear());
    }
    protected IEnumerator PlayAnimationAndDisappear()
    {
        Animator.Play("START");
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length);
        gameObject.SetActive(false);
        Managers.Resource.Destroy(gameObject); 
    }
}
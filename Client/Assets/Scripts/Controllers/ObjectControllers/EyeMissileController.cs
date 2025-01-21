using Google.Protobuf.Protocol;
using System.Collections;
using UnityEngine;

public class EyeMissileController : BaseController
{
    private bool _isExploding = false;

    protected override void Init()
    {
        base.Init();
        PlaySpawnAnimation();
        State = CreatureState.Moving;
    }

    private void PlaySpawnAnimation()
    {
        Animator.Play("SPAWN");
        Animator.speed = 1;
        StartCoroutine(WaitForSpawnAnimation());
    }

    private IEnumerator WaitForSpawnAnimation()
    {
        yield return new WaitUntil(() => Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f);
        PlayLoopAnimation();
    }

    private void PlayLoopAnimation()
    {
        Animator.Play("LOOP");
    }

    protected override void UpdateAnimation()
    {
        // 이동 중일 때 애니메이션 업데이트 로직을 추가할 수 있습니다.
    }

    public override IEnumerator DespawnAnim()
    {
        if (Animator == null)
        {
            Animator = GetComponent<Animator>();
        }
        yield return StartCoroutine(PlayImpactAnimationAndDisappear());
    }

    private IEnumerator PlayImpactAnimationAndDisappear()
    {
        Animator.Play("IMPACT");
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f);
        gameObject.SetActive(false);
    }
}
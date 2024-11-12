using Google.Protobuf.Protocol;
using System.Collections;
using UnityEngine;

public class ProjectileExController : BaseController
{
    private bool _isExploding = false;

    protected override void Init()
    {
        switch (Dir)
        {
            case MoveDir.Up:
                transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case MoveDir.Down:
                transform.rotation = Quaternion.Euler(0, 0, -180);
                break;
            case MoveDir.Left:
                transform.rotation = Quaternion.Euler(0, 0, 90);
                break;
            case MoveDir.Right:
                transform.rotation = Quaternion.Euler(0, 0, -90);
                break;
        }
        base.Init();
        Animator.Play("START",0,0);
        Animator.speed = 0;
        State = CreatureState.Moving;        
    }

    protected override void UpdateAnimation()
    {
    }
    public override IEnumerator DespawnAnim()
    {
        if(Animator == null)
        {
            Animator = GetComponent<Animator>();
        }
        yield return StartCoroutine(PlayAnimationAndDisappear());
    }

    private IEnumerator PlayAnimationAndDisappear()
    {
        Animator.speed = 1;
        Animator.Play("START");
        yield return new WaitUntil(() => Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f);
        gameObject.SetActive(false);
    }
}
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
        Animator.Play("START", 0, 0);
        State = CreatureState.Moving;

        base.Init();
    }

    protected override void UpdateAnimation()
    {
    }
    public void OnLeavePacketReceived()
    {
        if (!_isExploding)
        {
            _isExploding = true;
            StartCoroutine(PlayAnimationAndDisappear());
        }
    }

    private IEnumerator PlayAnimationAndDisappear()
    {
        Animator.Play("START");
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length);
        gameObject.SetActive(false);
        Managers.Resource.Destroy(gameObject);
    }
}
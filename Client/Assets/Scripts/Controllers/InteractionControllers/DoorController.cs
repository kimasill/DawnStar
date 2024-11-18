using System.Collections;
using UnityEngine;

public class DoorController : InteractionController
{
    private bool _isOpen = false;
    [SerializeField]
    private Animator _decorator;
    protected override void Init()
    {
        base.Init();
        Animator.Play("OPEN", 0, 0);
        Animator.speed = 0;
        if(_decorator != null)
        {
            _decorator.Play("OPEN", 0, 0);
            _decorator.speed = 0;
        }        
    }

    public void OpenDoor()
    {
        if (_isOpen)
            return;

        _isOpen = true;
        StartCoroutine(CoOpenDoor());
    }

    public void CloseDoor()
    {
        if (!_isOpen)
            return;

        _isOpen = false;
        StartCoroutine(CoCloseDoor());
    }
    private IEnumerator CoOpenDoor()
    {
        Animator.speed = 1;
        Animator.Play("OPEN");
        // 애니메이션 재생 시간 동안 대기
        yield return new WaitForSeconds(0.01f);
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length);
        foreach(var cellPos in CellPoses)
        {
            Managers.Map.SetCollision(cellPos, false);
        }        
        Animator.Play("CLOSE", 0, 0);
        Animator.speed = 0;       
    }

    private IEnumerator CoCloseDoor()
    {
        Animator.speed = 1;
        Animator.Play("CLOSE");
        yield return new WaitForSeconds(0.01f);
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length - 0.05f);
        foreach (var cellPos in CellPoses)
        {
            Managers.Map.SetCollision(cellPos, true);
        }
        Animator.Play("OPEN", 0, 0);
        Animator.speed = 0;
    }
    public void HandleOpenPacket()
    {
        OpenDoor();
    }
    public void HandleClosePacket()
    {
        CloseDoor();
    }
    public override void Interact(bool success, bool action)
    {
        if (success)
        {
            if (_isOpen)
                CloseDoor();
            else
                OpenDoor();
        }
        else if(success == false && action)
        {
            // 상호작용 실패 처리 - notification
        }
    }
}
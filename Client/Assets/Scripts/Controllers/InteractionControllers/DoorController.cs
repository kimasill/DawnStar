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

        StartCoroutine(CoOpenDoor());
        _isOpen = true;
    }

    public void CloseDoor()
    {
        if (!_isOpen)
            return;
        
        StartCoroutine(CoCloseDoor());
        _isOpen = false;
    }
    private IEnumerator CoOpenDoor()
    {
        Animator.speed = 1;
        Animator.Play("OPEN");
        if (_decorator != null)
        {
            _decorator.speed = 1;
            _decorator.Play("OPEN");
        }
        yield return new WaitForSeconds(0.01f);
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length - 0.05f); // 애니메이션 재생 시간 동안 대기
        if (CellPoses != null)
        {
            foreach (var cellPos in CellPoses)
            {
                Managers.Map.SetCollision(cellPos, false);
            }
        }
        Animator.speed = 0;
        if (_decorator != null)
        {
            _decorator.speed = 0;
        }
    }

    private IEnumerator CoCloseDoor()
    {
        Animator.speed = 1;
        Animator.Play("CLOSE");
        yield return new WaitForSeconds(0.01f);
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length - 0.05f);
        if (CellPoses != null)
        {
            foreach (var cellPos in CellPoses)
            {
                Managers.Map.SetCollision(cellPos, true);
            }
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
            InteractAction();
        }
        _isInteracted = false;
    }
}
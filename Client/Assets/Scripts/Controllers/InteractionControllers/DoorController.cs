using System.Collections;
using System.Collections.Generic;
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
        yield return new WaitUntil(() => Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f); // 애니메이션 재생 시간 동안 대기
        if (CellPoses != null)
        {
            foreach (var cellPos in CellPoses)
            {
                Managers.Map.SetCollision(cellPos, false);
            }
        }
        Animator.speed = 0;
        if (CheckAnimatorLayer("CLOSE") == true)
            Animator.Play("CLOSE", 0, 0);
        if (_decorator != null)
        {
            _decorator.speed = 0;
            if (CheckAnimatorLayer("CLOSE") == true)
                _decorator.Play("CLOSE", 0, 0);
        }
    }

    private IEnumerator CoCloseDoor()
    {
        Animator.Play("CLOSE");
        Animator.speed = 1;        
        if (_decorator != null)
        {
            _decorator.speed = 1;
            _decorator.Play("CLOSE");
        }
        yield return new WaitUntil(() => Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f);
        if (CellPoses != null)
        {
            foreach (var cellPos in CellPoses)
            {
                Managers.Map.SetCollision(cellPos, true);
            }
        }
        Animator.speed = 0;
        if (CheckAnimatorLayer("OPEN") == true)
            Animator.Play("OPEN", 0, 0);
        if (_decorator != null)
        {
            _decorator.speed = 0;
            if (CheckAnimatorLayer("OPEN") == true)
                _decorator.Play("OPEN", 0, 0);
        }
    }
    public void HandleOpenPacket()
    {
        OpenDoor();
    }
    public void HandleClosePacket()
    {
        CloseDoor();
    }
    public override void Interact(bool success, bool action, List<int> ids = null)
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
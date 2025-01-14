using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : InteractionController
{
    private bool _isOpen = false;
    [SerializeField]
    private Animator _decorator;

    public Action _openAction;
    public Action _closeAction;

    protected override void Init()
    {
        base.Init();
        if (Animator != null)
        {
            Animator.Play("OPEN", 0, 0);
            Animator.speed = 0;
        }
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
        if(Animator == null)
        {
            gameObject.SetActive(false);
        }
        else
        {
            Animator.speed = 1;
            if (CheckAnimationClip("CLOSE"))
            {
                Animator.Play("OPEN"); // OPEN 애니메이션 재생
                if (_decorator != null)
                {
                    _decorator.speed = 1;
                    _decorator.Play("OPEN");
                }
                yield return new WaitUntil(() => Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f); // 애니메이션 재생 시간 동안 대기
                                                                                                                   // 애니메이션 종료 대기
                Animator.speed = 0;
                Animator.Play("CLOSE", 0, 0); // CLOSE 애니메이션 첫 프레임으로 고정
                if (_decorator != null)
                {
                    _decorator.speed = 0;
                    _decorator.Play("CLOSE", 0, 0);
                }
            }
            else
            {
                Animator.Play("OPEN");
                yield return new WaitUntil(() => Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f); // 애니메이션 종료 대기
                Animator.speed = 0;
                Animator.Play("OPEN", 0, 0.95f); // CLOSE 애니메이션 마지막 프레임으로 고정
                if (_decorator != null)
                {
                    _decorator.speed = 0;
                    _decorator.Play("OPEN", 0, 0.95f);
                }
            }
            if(_openAction != null)
                _openAction.Invoke();
        }
        if (CellPoses != null)
        {
            foreach (var cellPos in CellPoses)
            {
                Managers.Map.SetCollision(cellPos, false);
            }
        }
    }

    private IEnumerator CoCloseDoor()
    {
        if (Animator == null)
        {
            gameObject.SetActive(true);
        }
        else
        {
            Animator.speed = 1;
            if (CheckAnimationClip("OPEN"))
            {
                Animator.Play("CLOSE"); // CLOSE 애니메이션 재생
                if (_decorator != null)
                {
                    _decorator.speed = 1;
                    _decorator.Play("CLOSE");
                }
                yield return new WaitUntil(() => Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f); // 애니메이션 재생 시간 동안 대기
                                                                                                                   // 애니메이션 종료 대기
                Animator.speed = 0;
                Animator.Play("OPEN", 0, 0); // OPEN 애니메이션 첫 프레임으로 고정
                if (_decorator != null)
                {
                    _decorator.speed = 0;
                    _decorator.Play("OPEN", 0, 0);
                }

            }
            else
            {
                Animator.Play("CLOSE");
                yield return new WaitUntil(() => Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f); // 애니메이션 종료 대기
                Animator.speed = 0;
                Animator.Play("CLOSE", 0, 0.95f); // CLOSE 애니메이션 마지막 프레임으로 고정
                if (_decorator != null)
                {
                    _decorator.speed = 0;
                    _decorator.Play("CLOSE", 0, 0.95f);
                }
            }
            if(_closeAction != null)
                _closeAction.Invoke();            
        }
        if (CellPoses != null)
        {
            foreach (var cellPos in CellPoses)
            {
                Managers.Map.SetCollision(cellPos, true);
            }
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
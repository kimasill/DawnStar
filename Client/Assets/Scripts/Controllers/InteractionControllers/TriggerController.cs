using System.Collections;
using UnityEngine;

public class TriggerController : InteractionController
{
    private bool _isTriggered = false;
    [SerializeField]
    private Animator _animator;

    protected override void Init()
    {
        base.Init();
        _animator.Play("IDLE", 0, 0);
        _animator.speed = 0;
    }

    public void ActivateTrigger()
    {
        if (_isTriggered)
            return;

        _isTriggered = true;
        StartCoroutine(CoActivateTrigger());
    }

    public void DeactivateTrigger()
    {
        if (!_isTriggered)
            return;

        _isTriggered = false;
        StartCoroutine(CoDeactivateTrigger());
    }

    private IEnumerator CoActivateTrigger()
    {
        _animator.speed = 1;
        _animator.Play("ACTIVATE");
        yield return new WaitForSeconds(0.01f);
        yield return new WaitForSeconds(_animator.GetCurrentAnimatorStateInfo(0).length);
        _animator.Play("IDLE", 0, 0);
        _animator.speed = 0;
    }

    private IEnumerator CoDeactivateTrigger()
    {
        _animator.speed = 1;
        _animator.Play("DEACTIVATE");
        yield return new WaitForSeconds(0.01f);
        yield return new WaitForSeconds(_animator.GetCurrentAnimatorStateInfo(0).length);
        _animator.Play("IDLE", 0, 0);
        _animator.speed = 0;
    }

    public void HandleActivatePacket()
    {
        ActivateTrigger();
    }

    public void HandleDeactivatePacket()
    {
        DeactivateTrigger();
    }

    public override void Interact(bool success, bool action)
    {
        if (success)
        {
            if (_isTriggered)
                DeactivateTrigger();
            else
                ActivateTrigger();
        }
        else if (success == false && action)
        {
            // 상호작용 실패 처리 - notification
        }
        _isInteracted = false;
    }
}
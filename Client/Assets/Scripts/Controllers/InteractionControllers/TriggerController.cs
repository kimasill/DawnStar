using System.Collections;
using UnityEngine;

public class TriggerController : InteractionController
{
    private bool _isTriggered = false;

    protected override void Init()
    {
        base.Init();
        Animator.Play("ACTIVATE", 0, 0);
        Animator.speed = 0;
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
        Animator.speed = 1;
        Animator.Play("ACTIVATE");
        yield return new WaitUntil(() => Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f);
        Animator.Play("DEACTIVATE", 0, 0);
        Animator.speed = 0;
    }

    private IEnumerator CoDeactivateTrigger()
    {
        Animator.speed = 1;
        Animator.Play("DEACTIVATE");
        yield return new WaitUntil(() => Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f);
        Animator.Play("ACTIVATE", 0, 0);
        Animator.speed = 0;
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
            InteractAction();
        }
        _isInteracted = false;
    }
}
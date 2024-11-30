using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerController : InteractionController
{
    private bool _isTriggered = false;
    private List<int> _targetInteraction;

    protected override void Init()
    {
        base.Init();
        Animator.Play("ACTIVATE", 0, 0);
        Animator.speed = 0;
    }
    protected override void HandleTriggerInteraction(InteractionData data)
    {
        TriggerData triggerData = (TriggerData)data;
        _targetInteraction = triggerData.targetInteraction;
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

    public override void Interact(bool success, int id, bool action)
    {
        if (success)
        {
            if (_isTriggered)
            {
                DeactivateTrigger();
                if(InteractionData.cameraMove)
                {
                    InteractionController ic = Managers.Map.GetInteractionById(id);
                    Vector3 targetPosition = new Vector3(ic.transform.position.x, ic.transform.position.y, -10);
                    StartCoroutine(InteractionCameraMove(targetPosition));
                                        
                }
            }               
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
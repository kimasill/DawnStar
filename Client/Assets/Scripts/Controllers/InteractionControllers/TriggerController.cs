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
    public bool ActivateTrigger()
    {
        if (_isTriggered)
            return false;

        _isTriggered = true;
        StartCoroutine(CoActivateTrigger());
        return true;
    }

    public bool DeactivateTrigger()
    {
        if (!_isTriggered)
            return false;

        _isTriggered = false;
         StartCoroutine(CoDeactivateTrigger());
        return true;
    }

    private IEnumerator CoActivateTrigger()
    {
        Animator.speed = 1;
        Animator.Play("ACTIVATE");
        yield return new WaitUntil(() => Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f);
        Animator.speed = 0;
        Animator.Play("DEACTIVATE", 0, 0);        
    }

    private IEnumerator CoDeactivateTrigger()
    {
        Animator.speed = 1;
        Animator.Play("DEACTIVATE");
        yield return new WaitUntil(() => Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f);
        Animator.speed = 0;
        Animator.Play("ACTIVATE", 0, 0);        
    }

    public override void Interact(bool success, bool action, List<int> ids=null)
    {
        if (success)
        {
            if (_isTriggered)
            {
                DeactivateTrigger();
            }               
            else
            {
                if(ActivateTrigger() == true)
                {
                    if (InteractionData.cameraMove)
                    {
                        InteractionController ic = Managers.Map.GetInteractionById(ids[0]);
                        StartCoroutine(InteractionCameraMove(ic.transform));
                    }
                }
            }
                
        }
        else if (success == false && action)
        {
            InteractAction();
        }
        _isInteracted = false;
    }
}
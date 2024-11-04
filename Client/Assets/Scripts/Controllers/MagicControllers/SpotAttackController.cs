using Google.Protobuf.Protocol;
using System.Collections;
using UnityEngine;

public class SpotAttackController : BaseController
{   
    protected override void Init()
    {
        State = CreatureState.Moving;
        base.Init();
    }

    protected override void UpdateAnimation()
    {
        Animator.Play("START");
    }
}
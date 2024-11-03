using Google.Protobuf.Protocol;
using System.Collections;
using UnityEngine;

public class MagicBallController : BaseController
{
    private bool _isExploding = false;
    protected override void Init()
    {
        State = CreatureState.Moving;
        base.Init();
    }

    protected override void UpdateAnimation()
    {
    }
}
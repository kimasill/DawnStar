using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define
{
    public enum Scene
    {
        Unknown,
        Login,
        Lobby,
        DawnTown,
        DawnTownStore,
        DawnTownHome,
        DawnTownDead,
        EastEnd,
        Prison,
        Crypt,
        Laboratory,
        Depth,
        Game,
    }

    public enum Sound
    {
        Bgm,
        Effect,
        MaxCount,
    }

    public enum UIEvent
    {
        Click,
        RightClick,
        Drag,
        MouseOver,
        MouseOut,
    }
}

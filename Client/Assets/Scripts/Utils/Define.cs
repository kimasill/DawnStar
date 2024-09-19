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
        DawnTown_Store,
        DawnTown_Home,
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
        Drag,
        MouseOver,
        MouseOut,
    }
}

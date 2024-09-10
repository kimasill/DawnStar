using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DawnTown_Store : DawnTown
{
    protected override void Init()
    {
        base.Init();

        // 다른 맵을 로드
        Managers.Map.LoadMap(2);

        // 추가적인 초기화 작업이 필요하면 여기에 작성
    }

    public override void Clear()
    {
        // 필요에 따라 Clear 메서드를 구현
    }
}
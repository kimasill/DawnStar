using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//이미지 찾고 교체
public class UI_Item_Stat : UI_Base
{
    public TMP_Text Name;
    public override void Init()
    {        
        Name = gameObject.GetComponent<TMP_Text>();
    }
}

using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//이미지 찾고 교체
public class UI_Text : UI_Base
{
    public int Id {get; private set; }
    public TMP_Text Text;
    public override void Init()
    {
        Text = gameObject.GetComponent<TMP_Text>();
    }

    public void SetText(int id, string text)
    {
        Id = id;
        Text.text = text;
    }
    public void SetText(string text)
    {
        Text.text = text;
    }
}

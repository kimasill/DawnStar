using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;

public class UI_PartyMember : UI_Base, IPointerClickHandler
{
    private int _memberId;
    private TMP_Text _memberName;
    public int MemberId { get { return _memberId; } }
    public UI_HpBar HpBar { get; private set; }
    public UI_UpBar UpBar { get; private set; }
    private List<string> _selectElements = new List<string>();
    private List<Action> _actions = new List<Action>();

    enum Texts
    {
        PartyMember_NameText
    }

    public override void Init()
    {
        Bind<TMP_Text>(typeof(Texts));
        HpBar = GetComponentInChildren<UI_HpBar>();
        UpBar = GetComponentInChildren<UI_UpBar>();
        _memberName = GetTextMeshPro((int)Texts.PartyMember_NameText);
        gameObject.BindEvent(OnPointerClick);
        _selectElements.Add("ÆÄÆ¼ Å»Åð");
        _actions.Add(OnClickLeaveParty);
    }

    public void SetInfo(ObjectInfo member)
    {
        _memberId = member.ObjectId;
        _memberName.text = member.Name;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            UI_Select selectUI = Managers.UI.ShowPopupUI<UI_Select>();
            selectUI.OpenUI(eventData, _selectElements, _actions);
        }
    }

    private void OnClickLeaveParty()
    {
        C_PartyLeave leavePartyPacket = new C_PartyLeave();
        Managers.Network.Send(leavePartyPacket);        
    }
}
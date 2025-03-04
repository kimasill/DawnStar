using Google.Protobuf.Protocol;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Party : UI_Base
{
    private List<UI_PartyMember> _partyMembers = new List<UI_PartyMember>();
    private List<ObjectInfo> _members = new List<ObjectInfo>();
    public bool IsParty { get; private set; }

    public override void Init()
    {
    }

    public void ApplyParty(S_Party partyPacket)
    {
        _members.Clear();
        if(partyPacket.PartyMembers.Select(x => x.ObjectId).Contains(Managers.Object.MyPlayer.Id))
        {
            IsParty = true;
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
            return;
        }
        foreach (var member in partyPacket.PartyMembers)
        {
            if (member == null)
                continue;
            _members.Add(member);
        }
        RefreshUI();
    }

    private void RefreshUI()
    {
        if(_members.Count == 0)
        {
            gameObject.SetActive(false);
            return;
        }
        foreach (var member in _partyMembers)
        {
            Destroy(member.gameObject);           
        }
        _partyMembers.Clear();
        // Add new UI_PartyMember objects
        foreach (var member in _members)
        {
            if (member == null)
                continue;
            GameObject go = Managers.Resource.Instantiate("UI/Scene/UI_PartyMember", transform);
            UI_PartyMember partyMember = go.GetOrAddComponent<UI_PartyMember>();
            partyMember.SetInfo(member);
            _partyMembers.Add(partyMember);
        }
    }

    public UI_PartyMember GetPartyMember(int memberId)
    {
        foreach(var partyMember in _partyMembers)
        {
            if (partyMember.MemberId == memberId)
                return partyMember;
        }
        return null;
    }
}
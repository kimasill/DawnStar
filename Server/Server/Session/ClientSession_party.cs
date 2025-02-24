using Google.Protobuf.Protocol;
using Server.Game;
using Server.Game.Contents;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public partial class ClientSession : PacketSession
    {
        private Party _currentParty;
        public Party CurrentParty { get { return _currentParty; } }

        public void CreateParty()
        {
            if (_currentParty != null)
            {
                // 이미 파티에 속해있음
                return;
            }

            _currentParty = PartySystem.Instance.CreateParty();
            if (_currentParty != null)
            {
                _currentParty.AddMember(MyPlayer);
            }
        }

        public void InviteToParty(ClientSession targetSession)
        {
            if (_currentParty == null)
            {
                CreateParty();
            }

            targetSession.JoinParty(_currentParty);
        }

        public void AcceptPartyInvite(int partyId)
        {
            Party party = PartySystem.Instance.GetParty(partyId);
            if (party != null)
            {
                JoinParty(party);
            }
        }

        public void LeaveParty()
        {
            if (_currentParty != null)
            {
                List<Player> members = new List<Player>(_currentParty.Members);

                _currentParty.RemoveMember(MyPlayer);               
                
                if (_currentParty.Members.Count == 0)
                {
                    PartySystem.Instance.RemoveParty(_currentParty.PartyId);
                }
                S_Party party = new S_Party();
                party.PartyId = _currentParty.PartyId;
                party.PartyMembers.AddRange(_currentParty.Members.Select(member => member.Info));

                foreach (var member in members)
                {
                    member.Session.Send(party);
                }

                _currentParty = null;
            }
        }

        public void JoinParty(Party party)
        {
            if (_currentParty != null)
            {
                LeaveParty();
            }

            _currentParty = party;
            _currentParty.AddMember(MyPlayer);

            S_Party joinParty = new S_Party();
            joinParty.PartyId = _currentParty.PartyId;
            joinParty.PartyMembers.AddRange(_currentParty.Members.Select(member => member.Info));

            foreach (var member in _currentParty.Members)
            {
                member.Session.Send(joinParty);
            }
        }
    }
}


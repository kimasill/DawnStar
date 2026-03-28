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
            if (_currentParty == null)
                return;

            List<Player> membersSnapshot = new List<Player>(_currentParty.Members);
            int partyId = _currentParty.PartyId;

            _currentParty.RemoveMember(MyPlayer);

            if (_currentParty.Members.Count == 0)
                PartySystem.Instance.RemoveParty(partyId);

            S_Party packet = new S_Party { PartyId = partyId };
            if (_currentParty != null && _currentParty.Members.Count > 0)
                packet.PartyMembers.AddRange(_currentParty.Members.Select(m => m.Info));

            foreach (Player member in membersSnapshot)
            {
                member.Session?.Send(packet);
            }

            _currentParty = null;
        }

        public void JoinParty(Party party)
        {
            if (party == null)
                return;

            if (_currentParty != null)
                LeaveParty();

            _currentParty = party;
            if (!_currentParty.AddMember(MyPlayer))
                return;

            S_Party joinParty = new S_Party();
            joinParty.PartyId = _currentParty.PartyId;
            joinParty.PartyMembers.AddRange(_currentParty.Members.Select(member => member.Info));

            foreach (var member in _currentParty.Members)
            {
                member.Session?.Send(joinParty);
            }
        }
    }
}


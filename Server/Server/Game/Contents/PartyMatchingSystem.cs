using Server.Game;
using Server.Game.Contents;
using System;
using System.Collections.Generic;

namespace Server
{
    public class PartyMatchingSystem
    {
        private static PartyMatchingSystem _instance = new();
        public static PartyMatchingSystem Instance => _instance;

        private Dictionary<int, List<ClientSession>> _waitingLists = new Dictionary<int, List<ClientSession>>();

        private PartyMatchingSystem() { }

        public void Register(ClientSession session, int mapId)
        {
            if (!_waitingLists.ContainsKey(mapId))
            {
                _waitingLists[mapId] = new List<ClientSession>();
            }

            if (_waitingLists[mapId].Contains(session))
                return;

            _waitingLists[mapId].Add(session);
            TryMatch(mapId);
        }

        public void Unregister(ClientSession session, int mapId)
        {
            if (_waitingLists.ContainsKey(mapId))
            {
                _waitingLists[mapId].Remove(session);
            }
        }

        private void TryMatch(int mapId)
        {
            if (_waitingLists.ContainsKey(mapId) && _waitingLists[mapId].Count >= 4)
            {
                List<ClientSession> matchedSessions = _waitingLists[mapId].GetRange(0, 4);
                _waitingLists[mapId].RemoveRange(0, 4);

                Party newParty = new Party(PartySystem.Instance.CreateParty().PartyId);
                foreach (var session in matchedSessions)
                {
                    session.JoinParty(newParty);
                }
                EnterMap(newParty, mapId);
            }
        }

        public void EnterMap(Party party, int mapId)
        {
            GameRoom room = GameLogic.Instance.Add(mapId);
            foreach (var member in party.Members)
            {
                member.Room.Push(member.Room.HandleMapChanged, member, mapId, room);
            }
        }
    }
}
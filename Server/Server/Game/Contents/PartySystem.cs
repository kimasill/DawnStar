using System;
using System.Collections.Generic;

namespace Server.Game.Contents
{
    internal class PartySystem
    {
        private static PartySystem _instance = new PartySystem();
        public static PartySystem Instance => _instance;

        private int _partyIdCounter = 1;
        private Dictionary<int, Party> _parties = new Dictionary<int, Party>();
        private const int MaxParties = 100; // 최대 파티 개수

        private PartySystem() { }

        public Party CreateParty()
        {
            if (_parties.Count >= MaxParties)
                return null; // 최대 파티 개수를 초과하면 null 반환

            int newPartyId = _partyIdCounter++;
            Party newParty = new Party(newPartyId);
            _parties.Add(newPartyId, newParty);
            return newParty;
        }

        public bool RemoveParty(int partyId)
        {
            return _parties.Remove(partyId);
        }

        public Party GetParty(int partyId)
        {
            _parties.TryGetValue(partyId, out Party party);
            return party;
        }

        public int GetTotalParties()
        {
            return _parties.Count;
        }
    }
}
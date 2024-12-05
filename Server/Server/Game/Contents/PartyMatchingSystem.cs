using Google.Protobuf.Protocol;
using Server.Data;
using Server.Game;
using Server.Game.Contents;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;

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
            Console.WriteLine($"Matching Register: {session.SessionId}");
            Console.WriteLine($"Waiting Count: {_waitingLists[mapId].Count}/4");
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
                Console.WriteLine($"Matching Success: {mapId}, Party Id: {newParty.PartyId}");
                EnterMap(newParty, mapId);
            }
        }

        public void EnterMap(Party party, int mapId)
        {
            GameRoom room = GameLogic.Instance.Add(mapId);
            MapData mapData = DataManager.MapDict.TryGetValue(mapId, out mapData) ? mapData : null;
            if(mapData == null)
            {
                return;
            }
            foreach (var member in party.Members)
            {
                member.Session.ServerState = PlayerServerState.ServerStateGame;
                member.Room.Push(member.Room.HandleMapChanged, member, mapData, mapData.portals.First().id, room);
            }
        }
    }
}
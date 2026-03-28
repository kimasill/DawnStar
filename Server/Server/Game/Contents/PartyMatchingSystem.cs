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
            if (!_waitingLists.TryGetValue(mapId, out List<ClientSession> queue))
            {
                queue = new List<ClientSession>();
                _waitingLists[mapId] = queue;
            }

            queue.RemoveAll(s => s == null || s.MyPlayer == null || s.ServerState != PlayerServerState.ServerStateGame || s.MyPlayer.Room == null);

            if (queue.Contains(session))
                return;

            if (session.CurrentParty != null)
                return;

            queue.Add(session);
            TryMatch(mapId);
        }

        public void Unregister(ClientSession session, int mapId)
        {
            if (_waitingLists.TryGetValue(mapId, out List<ClientSession> queue))
                queue.Remove(session);
        }

        private void TryMatch(int mapId)
        {
            if (!_waitingLists.TryGetValue(mapId, out List<ClientSession> queue))
                return;

            queue.RemoveAll(s => s == null || s.MyPlayer == null || s.ServerState != PlayerServerState.ServerStateGame || s.MyPlayer.Room == null);

            if (queue.Count < 4)
                return;

            List<ClientSession> matchedSessions = queue.GetRange(0, 4);
            queue.RemoveRange(0, 4);

            Party newParty = new Party(PartySystem.Instance.CreateParty().PartyId);
            foreach (var session in matchedSessions)
            {
                session.JoinParty(newParty);
            }

            EnterMap(newParty, mapId);
        }

        public void EnterMap(Party party, int mapId)
        {
            if (party == null || party.Members.Count == 0)
                return;

            GameRoom room = GameLogic.Instance.Add(mapId);
            if (!DataManager.MapDict.TryGetValue(mapId, out MapData mapData) || mapData == null)
                return;

            PortalData entryPortal = mapData.portals?.FirstOrDefault();
            if (entryPortal == null)
                return;

            foreach (var member in party.Members)
            {
                if (member?.Session == null)
                    continue;

                member.Session.ServerState = PlayerServerState.ServerStateGame;
                GameLogic.Instance.UpdateRoom(member.Room);
                room.Enqueue(member.Room.HandleMapChanged, member, mapData, entryPortal.id, room);
            }
        }
    }
}
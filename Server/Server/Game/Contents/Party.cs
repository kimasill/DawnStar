using Server.Game;
using System;
using System.Collections.Generic;

namespace Server
{
    public class Party
    {
        public int PartyId { get; private set; }
        public List<Player> Members { get; private set; } = new List<Player>();
        private const int MaxMembers = 4;

        public Party(int partyId)
        {
            PartyId = partyId;
        }

        public bool AddMember(Player player)
        {
            if (player == null)
                return false;
            if (Members.Count >= MaxMembers)
                return false;
            if (Members.Contains(player))
            {
                return false;
            }
            Members.Add(player);
            return true;
        }

        public void RemoveMember(Player player)
        {
            Members.Remove(player);
        }

        public bool IsFull()
        {
            return Members.Count >= MaxMembers;
        }
    }
}
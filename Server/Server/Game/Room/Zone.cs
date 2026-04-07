using System;
using System.Collections.Generic;

namespace Server.Game
{
    public class Zone
    {
        public int IndexY { get; private set; }
        public int IndexX { get; private set; }

        public HashSet<Player> Players { get; set; } = new HashSet<Player>();
        public HashSet<Monster> Monsters { get; set; } = new HashSet<Monster>();
        public HashSet<Projectile> Projectiles { get; set; } = new HashSet<Projectile>();
        public HashSet<Magic> Magics { get; set; } = new HashSet<Magic>();

        public Zone(int y, int x)
        {
            IndexY = y;
            IndexX = x;
        }

        public void Add(GameObject gameObject)
        {
            switch (gameObject)
            {
                case Player p:      Players.Add(p); break;
                case Monster m:     Monsters.Add(m); break;
                case Projectile pr: Projectiles.Add(pr); break;
                case Magic mg:      Magics.Add(mg); break;
            }
        }

        public void Remove(GameObject gameObject)
        {
            switch (gameObject)
            {
                case Player p:      Players.Remove(p); break;
                case Monster m:     Monsters.Remove(m); break;
                case Projectile pr: Projectiles.Remove(pr); break;
                case Magic mg:      Magics.Remove(mg); break;
            }
        }

        public Player FindOnePlayer(Func<Player, bool> condition)
        {
            foreach (Player player in Players)
            {
                if (condition.Invoke(player))
                    return player;
            }
            return null;
        }

        public List<Player> FindAllPlayers(Func<Player, bool> condition)
        {
            List<Player> findList = new List<Player>();
            foreach (Player player in Players)
            {
                if (condition.Invoke(player))
                    findList.Add(player);
            }
            return findList;
        }
    }
}

using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game
{
    public class Player
    {
        public PlayerInfo Info { get; set; } = new PlayerInfo() { Position = new PositionInfo() };
        public GameRoom Room { get; set; }
        public ClientSession Session { get; set; }
    }
}

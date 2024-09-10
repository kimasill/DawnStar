using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using Server.DB;
using Server.Game.Job;
using Server.Game.Room;
using System;
using System.Collections.Generic;
using System.Text;
using static Server.Game.Item;
using DbTransaction = Server.DB.DbTransaction;

namespace Server.Game
{
    public partial class GameRoom : JobSerializer
    {
        public void HandleStartQuest(Player player, int questId)
        {
            if (player == null)
                return;

            player.HandleStartQuest(questId);
        }

        public void HandleQuestComplete(Player player, int questId)
        {
            if (player == null)
                return;

            player.HandleQuestComplete(questId);
        }
    }
}
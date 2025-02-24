using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DB;
using Server.Game;
using Server.Game.Room;
using Server.Migrations;
using Server.Utils;
using ServerCore;
using SharedDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Server
{
    public partial class ClientSession : PacketSession
    {
        public int AccountDbId { get; private set; }
        public List<LobbyPlayerInfo> LobbyPlayers { get; set; } = new List<LobbyPlayerInfo>();

        public void HandleLogin(C_Login loginPacket)
        {
            // TODO : 이런 저런 보안 체크
            if (ServerState != PlayerServerState.ServerStateLogin)
                return;

            // TODO : 문제가 있긴 있다
            // - 동시에 다른 사람이 같은 UniqueId을 보낸다면?
            // - 악의적으로 여러번 보낸다면
            // - 쌩뚱맞은 타이밍에 그냥 이 패킷을 보낸다면?

            LobbyPlayers.Clear();

            using (AppDbContext db = new AppDbContext())
            using (SharedDbContext sharedDb = new SharedDbContext())            
            {
                TokenDb token = sharedDb.Tokens
                   .Where(t => t.Token == int.Parse(loginPacket.UniqueId))
                   .FirstOrDefault();

                if (token == null)
                {
                    // 토큰이 유효하지 않음
                    return;
                }

                AccountDb findAccount = db.Accounts
                    .Include(a => a.Players)
                    .Where(a => a.AccountName == token.AccountDbId.ToString())
                    .FirstOrDefault();

                if (findAccount != null)
                {
                    // AccountDbId 메모리에 기억
                    AccountDbId = findAccount.AccountDbId;

                    S_Login loginOk = new S_Login() { LoginOk = 1 };
                    foreach (PlayerDb playerDb in findAccount.Players)
                    {
                        LobbyPlayerInfo lobbyPlayer = new LobbyPlayerInfo()
                        {
                            PlayerDbId = playerDb.PlayerDbId,
                            Name = playerDb.PlayerName,
                            StatInfo = new StatInfo()
                            {
                                Level = playerDb.Level,
                                Hp = playerDb.Hp,
                                Up = playerDb.Up,
                                HpRegen = playerDb.HpRegen,
                                UpRegen = playerDb.UpRegen,
                                MaxHp = playerDb.MaxHp,
                                MaxUp = playerDb.MaxUp,
                                Attack = playerDb.Attack,
                                Defense = playerDb.Defense,
                                Accuracy = playerDb.Accuracy,
                                Avoid = playerDb.Avoid,
                                Speed = playerDb.Speed,
                                TotalExp = playerDb.Exp,
                                StatPoint = playerDb.StatPoint,
                                CriticalChance = playerDb.CriticalChance,
                                CriticalDamage = playerDb.CriticalDamage,
                                PotionPerformance = playerDb.PotionPerformance,
                                MaxPotion = playerDb.MaxPotion
                            }
                        };
                        if (playerDb.Realizations == null || playerDb.Realizations.Count == 0)
                        {
                            playerDb.Realizations = new List<int>() { 0, 0, 0, 0 };
                        }
                        lobbyPlayer.StatInfo.Realizations.Clear();
                        lobbyPlayer.StatInfo.Realizations.AddRange(playerDb.Realizations);                    

                        // 메모리에도 들고 있다
                        LobbyPlayers.Add(lobbyPlayer);

                        // 패킷에 넣어준다
                        loginOk.Players.Add(lobbyPlayer);
                    }

                    Send(loginOk);
                    // 로비로 이동
                    ServerState = PlayerServerState.ServerStateLobby;
                }
                else
                {
                    AccountDb newAccount = new AccountDb() { AccountName = token.AccountDbId.ToString()};
                    db.Accounts.Add(newAccount);
                    bool success = db.SaveChangesEx();
                    if (success == false)
                        return;

                    // AccountDbId 메모리에 기억
                    AccountDbId = newAccount.AccountDbId;

                    S_Login loginOk = new S_Login() { LoginOk = 1 };
                    Send(loginOk);
                    // 로비로 이동
                    ServerState = PlayerServerState.ServerStateLobby;
                }
            }
        }

        public void HandleEnterGame(C_EnterGame enterGamePacket)
        {
            if (ServerState != PlayerServerState.ServerStateLobby)
                return;

            LobbyPlayerInfo playerInfo = LobbyPlayers.Find(p => p.Name == enterGamePacket.Name);
            if (playerInfo == null)
                return;

            int mapId = 1;
            bool isFirstLogin = false;
            MyPlayer = ObjectManager.Instance.Add<Player>();
            {
                MyPlayer.PlayerDbId = playerInfo.PlayerDbId;
                MyPlayer.Info.Name = playerInfo.Name;
                MyPlayer.Info.Position.State = CreatureState.Idle;
                MyPlayer.Info.Position.MoveDir = MoveDir.Down;
                MyPlayer.Info.Position.LookDir = LookDir.LookRight;

                MyPlayer.Stat.MergeFrom(playerInfo.StatInfo);
                if (MyPlayer.Stat.Hp <= 0)
                {
                    MyPlayer.Stat.Hp = MyPlayer.Stat.MaxHp;
                }
                S_QuestList questListPacket = new S_QuestList();
                using (AppDbContext db = new AppDbContext())
                {
                    List<QuestDb> quests = db.Quests
                        .Where(q => q.OwnerDbId == playerInfo.PlayerDbId)
                        .ToList();
                    foreach (QuestDb questDb in quests)
                    {
                        Quest quest = Quest.MakeQuest(questDb);
                        if (quest != null)
                        {
                            MyPlayer.Quest.Add(quest);
                            QuestInfo info = new QuestInfo();
                            info.MergeFrom(quest.Info);
                            questListPacket.Quests.Add(info);
                        }
                    }
                }
                Send(questListPacket);

                if (MyPlayer.Quest.Quests.Count == 0)
                {
                    isFirstLogin = true;
                    ServerState = PlayerServerState.ServerStateSingle;
                }

                if (isFirstLogin)
                {
                    MyPlayer = SingleGameSetting(MyPlayer);
                }
                else
                {
                    using (AppDbContext db = new AppDbContext())
                    {
                        PlayerDb player = db.Players.FirstOrDefault(p => p.PlayerDbId == playerInfo.PlayerDbId);
                        if (player != null)
                        {
                            MyPlayer.Info.Position.PosX = player.PosX;
                            MyPlayer.Info.Position.PosY = player.PosY;
                            MyPlayer.MapInfo.MapDbId = player.MapDbId;
                        }
                    }
                }
                S_ItemList itemListPacket = new S_ItemList();
                using (AppDbContext db = new AppDbContext())
                {
                    List<ItemDb> items = db.Items
                        .Where(i => i.OwnerDbId == playerInfo.PlayerDbId)
                        .ToList();
                    foreach (ItemDb itemDb in items)
                    {
                        Item item = Item.MakeItem(itemDb);
                        if (item != null)
                        {
                            MyPlayer.Inven.Add(item);

                            ItemInfo info = new ItemInfo();
                            info.MergeFrom(item.Info);
                            itemListPacket.Items.Add(info);
                        }
                    }
                }
                Send(itemListPacket);
                MyPlayer.Session = this;
                using (AppDbContext db = new AppDbContext())
                {
                    MapDb mapDb = db.Maps
                        .Where(m => m.MapDbId == MyPlayer.MapInfo.MapDbId).FirstOrDefault();

                    if (mapDb != null)
                    {
                        MapData mapData = DataManager.MapDict.TryGetValue(mapDb.TemplateId, out mapData) ? mapData : null;
                        if (mapData == null)
                            return;
                        if(mapData.type == MapType.Dungeon)
                        {                          
                            MyPlayer.PosInfo.PosX = (int)mapData.portals[0].posX;
                            MyPlayer.PosInfo.PosY = (int)mapData.portals[0].posY;
                        }
                        ChangeServerState(mapDb.TemplateId);
                        MyPlayer.MapInfo.TemplateId = mapDb.TemplateId;
                        MyPlayer.MapInfo.Scene = mapDb.Scene;
                        MyPlayer.MapInfo.MapName = mapDb.MapName;
                        mapId = mapDb.TemplateId;                    
                    }
                }
            }

            UpdateMapInteractions(MyPlayer, mapId);
            UpdateMapChests(MyPlayer, mapId);
            MyPlayer.Skill = new Skill(MyPlayer);

            if (ServerState == PlayerServerState.ServerStateSingle)
            {
                GameLogic.Instance.Push(() =>
                {
                    GameRoom room = GameLogic.Instance.Add(mapId);
                    room.Push(room.EnterGame, MyPlayer, false);
                });
            }
            else
            {
                ServerState = PlayerServerState.ServerStateGame;
                GameLogic.Instance.Push(() =>
                {
                    GameRoom room = GameLogic.Instance.FindByMapId(mapId); // 플레이어의 mapId로 룸을 찾음
                    if (room == null)
                    {
                        room = GameLogic.Instance.Add(mapId); // 룸이 없으면 새로 생성
                        Console.WriteLine($"Created new game room for mapId: {mapId}");
                    }
                    room.Push(room.EnterGame, MyPlayer, false);
                });
            }
        }
        public bool ChangeServerState(int mapId)
        {
            DataManager.MapDict.TryGetValue(mapId, out MapData mapData);
            if (mapData == null)
                return false;
            if(mapData.type == MapType.Quest)
            {
                ServerState = PlayerServerState.ServerStateSingle;
            }
            else
            {
                ServerState = PlayerServerState.ServerStateGame;
            }
            return false;
        }

        private Player SingleGameSetting(Player player)
        {
            MapData mapData;
            if (DataManager.MapDict.TryGetValue(001, out mapData) && mapData != null)
            {
                foreach (PortalData portal in mapData.portals)
                {
                    if (portal == null) continue;
                    if (portal.id == 100)
                    {
                        player.MapInfo.TemplateId = mapData.id;
                        player.PosInfo.PosX = (int)portal.posX;
                        player.PosInfo.PosY = (int)portal.posY;
                        player.MapInfo.MapName = "DawnTown";
                        player.MapInfo.Scene = player.MapInfo.MapName;
                        MapDb mapDb = new MapDb()
                        {
                            PlayerDbId = player.PlayerDbId,
                            TemplateId = mapData.id,
                            MapName = mapData.name,
                            Scene = "DawnTown"
                        };
                        DB.DbTransaction.SavePlayerMap(player, mapDb);
                    }
                }
                return player;
            }
            return null;
        }
        public void UpdateMapChests(Player player, int mapId)
        {
            List<int> chestIds = new List<int>();
            DataManager.MapDict.TryGetValue(mapId, out MapData mapData);
            if (mapData == null || mapData.chests == null)
                return;

            
            using (AppDbContext db = new AppDbContext())
            {
                List<ChestDb> chests = db.Chests
                    .Where(c => c.MapDbId == MyPlayer.MapInfo.MapDbId)
                    .ToList();
                foreach (var chest in mapData.chests)
                {
                    ChestDb chestDb = chests.FirstOrDefault(c => c.ChestId == chest.chestId);
                    if (chestDb == null)
                    {
                        chestDb = new ChestDb()
                        {
                            TemplateId = chest.templateId,
                            MapDbId = MyPlayer.MapInfo.MapDbId,
                            ChestId = chest.chestId,
                            Opened = false
                        };
                        chestIds.Add(chest.chestId);
                        DbTransaction.UpdateChestDb(player, chestDb);
                    }
                    else
                    {
                        if (chestDb.Opened == false)
                            chestIds.Add(chestDb.ChestId);
                    }
                }
            }
            MyPlayer.MapInfo.ChestIds.Clear();
            MyPlayer.MapInfo.ChestIds.AddRange(chestIds);
        }
        public void UpdateMapInteractions(Player player, int mapId)
        {
            List<int> interactionIds = new List<int>();

            using (AppDbContext db = new AppDbContext())
            {
                List<InteractionDb> interactions = db.Interactions
                    .Where(i => i.MapDbId == MyPlayer.MapInfo.MapDbId)
                    .ToList();
                foreach (var interaction in interactions)
                {
                    if(interaction.Completed)
                        interactionIds.Add(interaction.TemplateId);
                }
            }
            MyPlayer.MapInfo.InteractionIds.Clear();
            MyPlayer.MapInfo.InteractionIds.AddRange(interactionIds);
        }
        public void HandleCreatePlayer(C_CreatePlayer createPacket)
            {
                // TODO : 이런 저런 보안 체크
                if (ServerState != PlayerServerState.ServerStateLobby)
                    return;

                using (AppDbContext db = new AppDbContext())
                {
                    PlayerDb findPlayer = db.Players
                        .Where(p => p.PlayerName == createPacket.Name).FirstOrDefault();

                    if (findPlayer != null)
                    {
                        // 이름이 겹친다
                        Send(new S_CreatePlayer());
                    }
                    else
                    {
                        // 1레벨 스탯 정보 추출
                        StatData stat = null;
                        DataManager.StatDict.TryGetValue(1, out stat);

                        PlayerDb newPlayerDb = new PlayerDb()
                        {
                            PlayerName = createPacket.Name,
                            Level = stat.Level,
                            Hp = stat.MaxHp,
                            MaxHp = stat.MaxHp,
                            Up = stat.MaxUp,
                            MaxUp = stat.MaxUp,
                            HpRegen = stat.HpRegen,
                            UpRegen = stat.UpRegen,
                            Attack = stat.Attack,                        
                            Speed = stat.Speed,
                            Avoid = stat.Avoid,
                            Accuracy = stat.Accuracy,
                            CriticalChance = stat.CriticalChance,
                            CriticalDamage = stat.CriticalDamage,
                            PotionPerformance = 1,
                            MaxPotion = 5,
                            Exp = 0,
                            AccountDbId = AccountDbId
                        };

                        db.Players.Add(newPlayerDb);
                        bool success = db.SaveChangesEx();
                        if (success == false)
                            return;

                        // 메모리에 추가
                        LobbyPlayerInfo lobbyPlayer = new LobbyPlayerInfo()
                        {
                            PlayerDbId = newPlayerDb.PlayerDbId,
                            Name = createPacket.Name,
                            StatInfo = new StatInfo()
                            {
                                Level = stat.Level,
                                Hp = stat.MaxHp,
                                MaxHp = stat.MaxHp,
                                Up = stat.MaxUp,
                                MaxUp = stat.MaxUp,
                                HpRegen = stat.HpRegen,
                                UpRegen = stat.UpRegen,
                                Attack = stat.Attack,
                                Speed = stat.Speed,
                                PotionPerformance = 0,
                                MaxPotion = 5,
                                TotalExp = 0
                            }
                        };

                        // 메모리에도 들고 있다
                        LobbyPlayers.Add(lobbyPlayer);

                        // 클라에 전송
                        S_CreatePlayer newPlayer = new S_CreatePlayer() { Player = new LobbyPlayerInfo() };
                        newPlayer.Player.MergeFrom(lobbyPlayer);

                        Send(newPlayer);
                    }
                }
            }
        }
    }


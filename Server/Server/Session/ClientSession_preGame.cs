using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DB;
using Server.Game;
using Server.Game.Room;
using Server.Migrations;
using Server.Utils;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
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
            {
                AccountDb findAccount = db.Accounts
                    .Include(a => a.Players)
                    .Where(a => a.AccountName == loginPacket.UniqueId).FirstOrDefault();

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
                                MaxHp = playerDb.MaxHp,
                                Attack = playerDb.Attack,
                                Speed = playerDb.Speed,
                                TotalExp = playerDb.Exp,
                                Gold = playerDb.Gold,
                            }                            
                        };

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
                    AccountDb newAccount = new AccountDb() { AccountName = loginPacket.UniqueId };
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
            bool isFirstLogin = false;
            MyPlayer = ObjectManager.Instance.Add<Player>();
            {
                MyPlayer.PlayerDbId = playerInfo.PlayerDbId;
                MyPlayer.Info.Name = playerInfo.Name;
                MyPlayer.Info.Position.State = CreatureState.Idle;
                MyPlayer.Info.Position.MoveDir = MoveDir.Down;
                MyPlayer.Info.Position.LookDir = LookDir.LookRight;
                using (AppDbContext db = new AppDbContext())
                {
                    PlayerDb player = db.Players.FirstOrDefault(p => p.PlayerDbId == playerInfo.PlayerDbId);
                    if (player != null)
                    {
                        MyPlayer.Info.Position.PosX = player.PosX;
                        MyPlayer.Info.Position.PosY = player.PosY;
                    }
                }
                MyPlayer.Stat.MergeFrom(playerInfo.StatInfo);
                if(MyPlayer.Stat.Hp <= 0)
                {
                    MyPlayer.Stat.Hp = MyPlayer.Stat.MaxHp;
                }


                if (MyPlayer.Stat.Level == 0)
                {
                    isFirstLogin = true;
                    ServerState = PlayerServerState.ServerStateSingle;
                }
                MyPlayer.Session = this;
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
                    //클라한테 아이템 전달
                }
                Send(itemListPacket);
                ServerState = PlayerServerState.ServerStateSingle;
                if (ServerState == PlayerServerState.ServerStateSingle)
                {
                    if(isFirstLogin)
                    {
                        MyPlayer = SingleGameSetting(MyPlayer);
                    }
                    GameLogic.Instance.Push(() =>
                    {
                        GameRoom room = GameLogic.Instance.Add(1);
                        Console.WriteLine($"Player Room Id:{room.RoomId}");
                        room.Push(room.EnterGame, MyPlayer, false);
                    });
                }                
                else
                {
                    ServerState = PlayerServerState.ServerStateGame;
                    //GameLogic 담당 스레드에 등록
                    GameLogic.Instance.Push(() =>
                    {
                        GameRoom room = GameLogic.Instance.Find(1); //멀티서버
                        Console.WriteLine($"Player Room Id:{room.RoomId}");
                        room.Push(room.EnterGame, MyPlayer, false);
                    });
                }
            }
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
                        Vector2Int respawnPos = new Vector2Int((int)(portal.posX * 3.2), (int)(portal.posY * 3.2));
                        player.CellPos = respawnPos;
                        player.MapInfo.TemplateId = mapData.id;
                        player.MapInfo.PortalId = portal.id;
                        player.MapInfo.MapName = mapData.name;
                        player.MapInfo.Scene = "DawnTown";
                        DB.DbTransaction.SavePlayerMap(player, player.MapInfo);
                    }
                }
                return player;
            }
            return null;
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

                    // DB에 플레이어 만들어줘야 함
                    PlayerDb newPlayerDb = new PlayerDb()
                    {
                        PlayerName = createPacket.Name,
                        Level = stat.Level,
                        Hp = stat.MaxHp,
                        MaxHp = stat.MaxHp,
                        Attack = stat.Attack,                        
                        Speed = stat.Speed,
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
                            Attack = stat.Attack,
                            Speed = stat.Speed,
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

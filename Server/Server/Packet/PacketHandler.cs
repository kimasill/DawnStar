using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;
using Server.Game;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

class PacketHandler
{
	//(RedZone)
	public static void C_MoveHandler(PacketSession session, IMessage packet)
	{
		//클라이언트의 이동 패킷을 받았을 때
		C_Move movePacket = packet as C_Move;
		ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"C_Move ({movePacket.Position.PosX}, {movePacket.Position.PosY}"); ;

		Player player = clientSession.MyPlayer;
		if (player == null)
			return;

		GameRoom room = player.Room;
		if(room == null)
			return;

		room.HandleMove(player, movePacket);
		//검증
		//좌표이동
		
    }

	public static void C_SkillHandler(PacketSession session, IMessage packet) {
        
		C_Skill skillPacket = packet as C_Skill;
        ClientSession clientSession = session as ClientSession;

        Player player = clientSession.MyPlayer;
        if (player == null)
            return;

        GameRoom room = player.Room;
        if (room == null)
            return;

        room.HandleSkill(player, skillPacket);
    }
}

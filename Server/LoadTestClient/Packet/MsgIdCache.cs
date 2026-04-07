using Google.Protobuf;
using Google.Protobuf.Protocol;
using System.Collections.Generic;

// Auto-generated: MsgId lookup cache to avoid runtime Enum.Parse
public static class MsgIdCache
{
	static readonly Dictionary<string, MsgId> _nameToId = new Dictionary<string, MsgId>()
	{
		{ "S_EnterGame", MsgId.SEnterGame },
		{ "S_LeaveGame", MsgId.SLeaveGame },
		{ "S_Spawn", MsgId.SSpawn },
		{ "S_Despawn", MsgId.SDespawn },
		{ "C_Move", MsgId.CMove },
		{ "S_Move", MsgId.SMove },
		{ "C_Skill", MsgId.CSkill },
		{ "S_Skill", MsgId.SSkill },
		{ "S_ChangeHp", MsgId.SChangeHp },
		{ "S_Die", MsgId.SDie },
		{ "S_Connected", MsgId.SConnected },
		{ "C_Login", MsgId.CLogin },
		{ "S_Login", MsgId.SLogin },
		{ "C_EnterGame", MsgId.CEnterGame },
		{ "C_CreatePlayer", MsgId.CCreatePlayer },
		{ "S_CreatePlayer", MsgId.SCreatePlayer },
		{ "S_ItemList", MsgId.SItemList },
		{ "S_AddItem", MsgId.SAddItem },
		{ "C_EquipItem", MsgId.CEquipItem },
		{ "S_EquipItem", MsgId.SEquipItem },
		{ "S_ChangeStat", MsgId.SChangeStat },
		{ "S_Ping", MsgId.SPing },
		{ "C_Pong", MsgId.CPong },
		{ "S_Tutorial", MsgId.STutorial },
		{ "C_MapChange", MsgId.CMapChange },
		{ "S_MapChange", MsgId.SMapChange },
		{ "S_StartQuest", MsgId.SStartQuest },
		{ "C_StartQuest", MsgId.CStartQuest },
		{ "S_QuestComplete", MsgId.SQuestComplete },
		{ "C_QuestComplete", MsgId.CQuestComplete },
		{ "C_BuyItem", MsgId.CBuyItem },
		{ "S_ShopList", MsgId.SShopList },
		{ "C_RequestShop", MsgId.CRequestShop },
		{ "S_BuyItem", MsgId.SBuyItem },
		{ "S_ChangePosition", MsgId.SChangePosition },
		{ "C_ChangePosition", MsgId.CChangePosition },
		{ "C_RemoveItem", MsgId.CRemoveItem },
		{ "C_LootItem", MsgId.CLootItem },
		{ "C_RequestMonster", MsgId.CRequestMonster },
		{ "S_DropItem", MsgId.SDropItem },
		{ "S_RemoveItem", MsgId.SRemoveItem },
		{ "S_Respawn", MsgId.SRespawn },
		{ "C_Respawn", MsgId.CRespawn },
		{ "S_Loading", MsgId.SLoading },
		{ "S_QuestList", MsgId.SQuestList },
		{ "S_ChangeExp", MsgId.SChangeExp },
		{ "S_Damage", MsgId.SDamage },
		{ "C_OpenChest", MsgId.COpenChest },
		{ "S_MakeChest", MsgId.SMakeChest },
		{ "C_UseItem", MsgId.CUseItem },
		{ "S_SkillCool", MsgId.SSkillCool },
		{ "C_SelectStat", MsgId.CSelectStat },
		{ "C_EnterDungeon", MsgId.CEnterDungeon },
		{ "S_PartyInvite", MsgId.SPartyInvite },
		{ "C_Interaction", MsgId.CInteraction },
		{ "S_Interaction", MsgId.SInteraction },
		{ "S_ChangeAdditionalStat", MsgId.SChangeAdditionalStat },
		{ "C_RequestStat", MsgId.CRequestStat },
		{ "S_Effect", MsgId.SEffect },
		{ "S_BossKill", MsgId.SBossKill },
		{ "C_Enhance", MsgId.CEnhance },
		{ "S_Enhance", MsgId.SEnhance },
		{ "C_MakeItem", MsgId.CMakeItem },
		{ "C_SortItem", MsgId.CSortItem },
		{ "S_Buff", MsgId.SBuff },
		{ "S_ChangeUp", MsgId.SChangeUp },
		{ "C_SellItem", MsgId.CSellItem },
		{ "C_Enchant", MsgId.CEnchant },
		{ "S_Enchant", MsgId.SEnchant },
		{ "S_SystemNotice", MsgId.SSystemNotice },
		{ "C_Chat", MsgId.CChat },
		{ "S_Chat", MsgId.SChat },
		{ "S_UpdateItemList", MsgId.SUpdateItemList },
		{ "S_Party", MsgId.SParty },
		{ "C_PartyLeave", MsgId.CPartyLeave },
		{ "C_Quit", MsgId.CQuit },
		{ "C_Party", MsgId.CParty },
		{ "S_Quit", MsgId.SQuit },
		{ "C_RequestVision", MsgId.CRequestVision },
	};

	public static MsgId GetMsgId(IMessage packet)
	{
		string name = packet.Descriptor.Name;
		if (_nameToId.TryGetValue(name, out MsgId id))
			return id;

		throw new System.ArgumentException($"Unknown packet type: {name}");
	}
}
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using Server.DB;
using Server.Game.Job;
using Server.Game.Room;
using Server.Migrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Server.Game.Item;
using DbTransaction = Server.DB.DbTransaction;

namespace Server.Game
{
    public partial class GameRoom : JobSerializer
    {
        public void HandleStartQuest(Player player, int questId = 0)
        {
            if (player == null)
                return;

            using (AppDbContext db = new AppDbContext())
            {
                // 주어진 questId가 0이 아니면 해당 퀘스트를 찾고, 0이면 가장 최근 퀘스트를 찾습니다.
                List<QuestDb> quests = db.Quests
                    .Where(q => q.OwnerDbId == player.PlayerDbId && (questId == 0 || q.TemplateId == questId))
                    .OrderByDescending(q => q.QuestDbId)
                    .ToList();
                
                QuestDb questDb = quests.FirstOrDefault();                

                if (questDb == null)
                {
                    if(questId == 0)
                    {
                        // 퀘스트가 없으면 새로 0번 퀘스트를 할당합니다.
                        questDb = new QuestDb()
                        {
                            TemplateId = 0,
                            Progress = 0,
                            Completed = false,                            
                        };
                    }
                    else
                    {
                        QuestData questData = DataManager.QuestDict.GetValueOrDefault(questId);
                        if (questData == null)
                            return;
                        questDb = new QuestDb()
                        {
                            TemplateId = questData.id,
                            Progress = 0,
                            Completed = false,                            
                        };
                    }
                }
                else
                {
                    if(questDb.Completed)
                        return;

                    if (player.Quest.CurrentQuest.TemplateId == questDb.TemplateId && player.Quest.CurrentQuest.Progress == 1)//메모리에 들고있으면 안함
                        return;               
                    questDb.Progress = 1;
                    player.Quest.CurrentQuest = Quest.MakeQuest(questDb);
                }
                DbTransaction.SaveStartQuest(player, questDb, player.Room);
            }
        }

        public void HandleUpdateQuest(Player player, int questId, int progress)
        {
            if (player == null)
                return;

            // 퀘스트 진행 상태 업데이트
            Quest quest = player.Quest.CurrentQuest; // 현재 퀘스트 정보 가져오기
            if (quest == null || quest.TemplateId != questId)
                return;

            // 퀘스트 진행 상태 업데이트
            quest.Progress = progress;

            QuestDb questDb = new QuestDb()
            {
                TemplateId = quest.TemplateId,
                Progress = quest.Progress,
                Completed = quest.IsCompleted,
            };

            // DB에 퀘스트 진행 상태 저장
            DbTransaction.SaveQuestDB(player, questDb, player.Room);
        }

        public void HandleQuestComplete(Player player, int questId)
        {
            if (player == null)
                return;

            // 퀘스트 완료 처리
            Quest quest = player.Quest.Get(questId);
            if (quest == null || quest.TemplateId != questId)
                return;

            // 퀘스트 완료 상태로 변경
            if (quest.IsCompleted == false)
                quest.IsCompleted = true;
            quest.Progress = 100;

            QuestDb questDb = new QuestDb()
            {
                QuestDbId = quest.QuestDbId,
                TemplateId = quest.TemplateId,
                Progress = quest.Progress,
                Completed = quest.IsCompleted,
            };

            // DB에 퀘스트 완료 상태 저장
            DbTransaction.SaveCompleteQuest(player, questDb, player.Room);                
        }
    }
}
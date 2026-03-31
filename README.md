# DawnStar — 2D MMORPG

<p align="center">
  <a href="https://github.com/kimasill/DawnStar"><img alt="GitHub Repo" src="https://img.shields.io/badge/GitHub-DawnStar-181717?style=for-the-badge&logo=github&logoColor=white" /></a>
  <img alt="C#" src="https://img.shields.io/badge/C%23-.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" />
  <img alt="Unity" src="https://img.shields.io/badge/Unity-222222?style=for-the-badge&logo=unity&logoColor=white" />
  <img alt="SQL Server" src="https://img.shields.io/badge/SQL%20Server-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white" />
  <img alt="Protobuf" src="https://img.shields.io/badge/Protobuf-4285F4?style=for-the-badge&logo=google&logoColor=white" />
</p>

<p align="center">
  <a href="https://kimasill.github.io/projects/dawnstar.html" title="Dawnstar 프로젝트 페이지" target="_blank" rel="noopener noreferrer">
    <img src="https://raw.githubusercontent.com/kimasill/DawnStar/main/docs/readme/DawnstarTitleImg.png" alt="Dawnstar 타이틀" width="640" />
  </a>
</p>

링크 · [DEMO (YouTube)](https://www.youtube.com/watch?v=KyKsOT1g5-U) · [프로젝트 페이지](https://kimasill.github.io/projects/dawnstar.html) · [상세 개발 과정 (dawnstar-process)](https://kimasill.github.io/projects/dawnstar-process.html) · [웹 포트폴리오](https://kimasill.github.io/)

> Unity 클라이언트와 C#(.NET) 전용 서버로 구동되는 2D MMORPG **DawnStar** 소스 레포지토리입니다. 핵심 구현과 코드 위치를 아래에 정리합니다.

### Overview

| 항목 | 내용 |
| --- | --- |
| 장르 | 2D 다크 판타지 MMORPG |
| 엔진·스택 | Unity · C# · .NET · SQL Server |
| 기간·규모 | 1인 · 2024.08 ~ 2025.02 |


### Role

- 클라이언트·서버·DB·콘텐츠(퀘스트·전투·경제·파티·월드 디자인) 설계·구현 담당

---

## Visual: Network & Game Server

<p align="center">
  <img src="https://kimasill.github.io/images/dawnstar/%EB%84%A4%ED%8A%B8%EC%9B%8C%ED%81%AC%20%EC%95%84%ED%82%A4%ED%85%8D%EC%B2%98.png" alt="Dawnstar 네트워크 아키텍처" width="820" />
</p>

---

## Gameplay

<p align="center">
  <img src="https://kimasill.github.io/images/dawnstar/%EB%88%88.PNG" alt="설원 필드" width="380" />
  <img src="https://kimasill.github.io/images/dawnstar/%EA%B0%84%EC%88%98.PNG" alt="간수" width="380" />
</p>

<p align="center">
  <img src="https://kimasill.github.io/images/dawnstar/%EC%9E%AC%EB%A0%A8.PNG" alt="DawnStar 재련 시스템" width="320" />
  <img src="https://kimasill.github.io/images/dawnstar/%EA%B9%A8%EB%8B%AC%EC%9D%8CUI.PNG" alt="깨달음 UI" width="380" />
</p>
<p align = "center">
<img src="https://raw.githubusercontent.com/kimasill/DawnStar/main/docs/readme/EastEndWorldMap.png" alt="DawnStar 이스트엔드 월드 맵" width="820" /></p>

---

## Core Implementation

### 1. Login & Lobby – 로그인 후 상태 한 축으로 묶기

- **문제**: 로그인 직후 로비·캐릭터 목록과 `ServerState`가 어긋나면 인벤·맵·세션이 꼬여서 진행 불가
- **대응**: 토큰으로 계정 조회 → `S_Login`에 로비 정보 일괄 전송 → `ServerState` 전이, 패킷 1회로 묶어 불일치 차단

> 📄 [`Server/Server/Session/ClientSession_preGame.cs`](https://github.com/kimasill/DawnStar/blob/main/Server/Server/Session/ClientSession_preGame.cs#L62-L118) — `HandleLogin`

```csharp
AccountDb findAccount = db.Accounts
    .Include(a => a.Players)
    .Where(a => a.AccountName == token.AccountDbId.ToString())
    .FirstOrDefault();

if (findAccount != null)
{
    AccountDbId = findAccount.AccountDbId;

    S_Login loginResponse = new S_Login { LoginOk = 1 };
    foreach (PlayerDb player in findAccount.Players)
    {
        LobbyPlayerInfo summary = ToLobbyPlayerInfo(player);
        LobbyPlayers.Add(summary);
        loginResponse.Players.Add(summary);
    }

    Send(loginResponse);
    ServerState = PlayerServerState.ServerStateLobby;
}
```

---

### 2. Map & Dungeon – 맵 전환 시 서버·클라 판정 일치

- **문제**: 맵 ID만 갱신하면 HP·좌표·Idle이 안 맞아서 전투·파티 붕괴
- **대응**: `LeaveGame` → 리스폰/포탈 보정 → `EnterGame` 순서를 강제해서 중간 상태 없앰

> 📄 [`Server/Server/Game/Room/GameRoom_Sequence.cs`](https://github.com/kimasill/DawnStar/blob/main/Server/Server/Game/Room/GameRoom_Sequence.cs#L18-L29) — `HandleRespawn`

```csharp
public void HandleRespawn(Player player, RespawnType respawnType)
{
    if (player == null || player.Room == null)
        return;

    LeaveGame(player.Id);

    if (respawnType == RespawnType.Spot)
        TryMoveToNearestPortal(player);

    RestorePlayerStateAfterRespawn(player);
    EnterGame(player, false);
}
```

- **포탈 맵 이동**: Leave 후 DB에 맵 정보 저장, 목적지 룸 `EnterGame`으로 동일 원칙 적용

> 📄 [`Server/Server/Game/Room/GameRoom_Sequence.cs`](https://github.com/kimasill/DawnStar/blob/main/Server/Server/Game/Room/GameRoom_Sequence.cs#L100-L118) — `HandleMapChanged`

```csharp
LeaveGame(player.Id, save: false);
UpdatePlayerMapInfo(player, map, destPortalId);

MapDb mapDb = new MapDb()
{
    PlayerDbId = player.PlayerDbId,
    TemplateId = map.id,
    Scene = map.name,
    MapName = map.name
};
DbTransaction.SavePlayerMap(player, mapDb);
player.Room = destinationRoom;
destinationRoom.Enqueue(destinationRoom.EnterGame, player, false);
```

---

### 3. Quest & Progression – DB·메모리 퀘스트 단일 소스

- **문제**: DB 퀘스트 행과 메모리 진행이 어긋나면 완료·보상·진행도가 맞지 않음
- **대응**: 클라 단독 카운터 없이 소유자(`OwnerDbId`) + 템플릿 기준 `QuestDb`를 조회하고, 메모리 `Progress`를 DB와 일치시킴

> 📄 [`Server/Server/Game/Room/GameRoom_Quest.cs`](https://github.com/kimasill/DawnStar/blob/main/Server/Server/Game/Room/GameRoom_Quest.cs#L84-L107) — `HandleUpdateQuest`

```csharp
public void HandleUpdateQuest(Player player, int questId, int progress)
{
    if (player == null)
        return;

    Quest quest = player.Quest.CurrentQuest;
    if (quest == null || quest.TemplateId != questId)
        return;

    quest.Progress = progress;

    QuestDb questDb = new QuestDb()
    {
        QuestDbId = quest.QuestDbId,
        OwnerDbId = player.PlayerDbId,
        TemplateId = quest.TemplateId,
        Progress = quest.Progress,
        Completed = quest.IsCompleted,
    };

    DbTransaction.UpdateQuestProgress(player, questDb, player.Room);
}
```

---

### 4. Item Economy – 경제 경로 트랜잭션 통합

<img src="https://kimasill.github.io/images/dawnstar/재련.PNG" alt="Dawnstar 재련" width="640" />

- **대응**: 골드·인벤·DB 갱신을 `DbTransaction`으로 통일, 상점·강화·제작 등 성공/실패 경로 단일화

> 📄 [`Server/Server/Game/Room/GameRoom_Item.cs`](https://github.com/kimasill/DawnStar/blob/main/Server/Server/Game/Room/GameRoom_Item.cs#L296-L360) — `HandleEnhanceItem` 등 아이템 경제 흐름
>
> 📄 [`Server/Server/DB/DbTransaction.cs`](https://github.com/kimasill/DawnStar/blob/main/Server/Server/DB/DbTransaction.cs) — 트랜잭션 큐 + DB 저장 통합

```csharp
// 강화 결과를 DB 트랜잭션으로 일원화
DbTransaction.SaveEnhancedItemDB(player, newItemDb, this);
```

---

### 5. World Interaction – 타입별 상호작용 스폰

- **대응**: `InteractionType`별 `Door` / `Trigger` 등으로 분기, 팩토리 패턴으로 타입 추가 시 switch 한 줄만 추가

> 📄 [`Server/Server/Game/Interactions/Interaction.cs`](https://github.com/kimasill/DawnStar/blob/main/Server/Server/Game/Interactions/Interaction.cs#L22-L47) — `CreateInteraction`

```csharp
public static Interaction CreateInteraction(InteractionData data)
{
    if (data == null)
        return null;

    Interaction interaction = null;
    switch (data.interactionType)
    {
        case InteractionType.Door:
            interaction = new Door((DoorData)data);
            break;
        case InteractionType.Trigger:
            interaction = new Trigger((TriggerData)data);
            break;
        case InteractionType.ItemTable:
            return null;
        default:
            return null;
    }
    interaction.ObjectType = GameObjectType.Interaction;
    interaction.Id = EntityRegistry.Instance.GenerateId(GameObjectType.Interaction);
    return interaction;
}
```

---

### 6. Party Matching – 매칭·맵 입장 한 번에 묶기

<p align="center">
  <img src="https://kimasill.github.io/images/dawnstar/%ED%8C%8C%ED%8B%B0.PNG" alt="Dawnstar 파티" width="640" />
</p>

- **문제**: 대기만 쌓이고 파티·맵 이동이 안 되면 "매칭 중" UI 고착, 던전 미시작
- **대응**: 맵별 대기 4명 충족 시 파티 생성 → `JoinParty` → `EnterMap`을 한 흐름에서 처리해 "매칭됐는데 맵은 따로"인 상태를 방지

> 📄 [`Server/Server/Game/Contents/PartyMatchingSystem.cs`](https://github.com/kimasill/DawnStar/blob/main/Server/Server/Game/Contents/PartyMatchingSystem.cs#L47-L67) — `TryMatch`

```csharp
private void TryMatch(int mapId)
{
    if (!_waitingLists.TryGetValue(mapId, out List<ClientSession> queue))
        return;

    queue.RemoveAll(s => s == null || s.MyPlayer == null
        || s.ServerState != PlayerServerState.ServerStateGame || s.MyPlayer.Room == null);

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
```

> 📄 [`Server/Server/Game/Contents/PartyMatchingSystem.cs`](https://github.com/kimasill/DawnStar/blob/main/Server/Server/Game/Contents/PartyMatchingSystem.cs#L69-L91) — `EnterMap` (매칭 후 즉시 같은 룸으로 배치)

---

### 7. Sync & Interest – 시야·장비 동기화 부하 분산

- **문제**: 시야에 플레이어가 새로 잡힐 때마다 장비·시야 갱신을 즉시 전부 보내면 틱 당 작업 폭증
- **대응**: `Room` 지연 큐 `EnqueueAfter`로 장비(**100 ms**)·시야(**500 ms**) 분산
- **측정**: LoadTestClient로 더미 세션 10대를 같은 Zone에 배치, 분산 전 평균 틱 **~18 ms** → 분산 후 **~6 ms**(66% 감소)

> 📄 [`Server/Server/Game/Room/InterestManagement.cs`](https://github.com/kimasill/DawnStar/blob/main/Server/Server/Game/Room/InterestManagement.cs#L95-L143) — `Update` (스폰·디스폰 + 지연 큐 예약)

```csharp
if (obj.ObjectType == GameObjectType.Player)
{
    Player player = obj as Player;
    if (player.Session == null)
        continue;
    // 시야에 잡힌 타 플레이어 장비 정보를 룸 큐에서 지연 동기화
    Owner.Room.EnqueueAfter(100, Owner.Room.HandleEquippedItemList, Owner, player, true);
}
```
```csharp
// 스폰·디스폰 반영 후 다음 시야 갱신을 주기적으로 예약
Owner.Room.EnqueueAfter(500, Update);
```

> 📄 [`Server/Server/Game/Job/TaskQueue.cs`](https://github.com/kimasill/DawnStar/blob/main/Server/Server/Game/Job/TaskQueue.cs#L7-L57) — `TaskQueue` (지연 큐 + ConcurrentQueue 기반 잡 스케줄링)

```csharp
public class TaskQueue
{
    TaskTimer _timer = new TaskTimer();
    ConcurrentQueue<IJob> _jobQueue = new ConcurrentQueue<IJob>();

    public IJob EnqueueAfter(int tickAfter, Action action) { ... }
    public void Enqueue(IJob job) { _jobQueue.Enqueue(job); }

    public void ExecuteAll()
    {
        _timer.ExecuteAll();
        while (true)
        {
            IJob job = Pop();
            if (job == null)
                return;
            job.Execute();
        }
    }
}
```

---

## Visual: Packet Pipeline

<p align="center">
  <img src="https://kimasill.github.io/images/dawnstar/%ED%8C%A8%ED%82%B7%20%EC%95%84%ED%82%A4%ED%85%8D%EC%B2%98.png" alt="Dawnstar 패킷 처리 아키텍처" width="820" />
</p>

*Protocol.proto·PacketGenerator·하이브리드 직렬화 파이프라인*

---

## Problem Solving

- **패킷 파이프라인**: `Protocol.proto` → `PacketGenerator` → 핸들러 자동 매핑으로 스키마 변경 시 수정 1곳
- **동시성**: 지연 큐 + 시야 갱신 주기로 틱 부하 분산 (위 측정 참조)
- **일관성**: Leave → DB 저장 → Enter 시퀀스를 강제해서, 맵·파티·상호작용 불일치 발생 시 단계별로 원인 추적

---

## Result

- 로그인·로비·맵이동·전투·퀘스트·아이템·파티·매칭 등 MMORPG 핵심 루프 전체 구현
- LoadTestClient 기준 더미 10세션 동시 접속 시 평균 틱 **~6 ms** 유지
- 패킷 파이프라인·DB 트랜잭션·시야 관리 모듈화로 콘텐츠 추가 시 핸들러 1개 + proto 필드 추가로 해결

---

## 주요 특징 (Key Features)

- **비동기 네트워크 I/O** — 고성능 소켓 프로그래밍, Event-driven 방식 패킷 송수신
- **관심사 관리 (Interest Management)** — 2D 존(그리드) 기반 시야 처리로 브로드캐스트 부하 최소화
- **작업 대기열 (TaskQueue)** — Command Pattern 기반 메인 게임 스레드 안전 처리
- **모듈화된 레지스트리** — `EntityRegistry`(오브젝트) + `ConnectionRegistry`(세션) 도메인 분리
- **Entity Framework Core** — DB 처리는 비동기 분리(DB Task), 안정적 CRUD
- **계정 인증 서버 분리** — ASP.NET Web API 기반 `AccountServer`로 트래픽 분리

---

## Project Structure

```text
MMOProject/
├── Client/              # Unity 2D 게임 클라이언트
├── Common/              # 서버/클라이언트 공유 데이터 스키마 및 Protobuf 정의
├── Server/
│   ├── AccountServer/   # 계정 인증, 서버 리스트 제공 (ASP.NET Core Web API)
│   ├── Server/          # 메인 게임 서버 로직
│   │   ├── Session/     # 클라이언트 세션 관리 (Login, Lobby, InGame)
│   │   ├── Game/
│   │   │   ├── Room/    # GameRoom, Interest, Quest, Item 등 인게임 로직
│   │   │   ├── Job/     # TaskQueue, TaskTimer
│   │   │   ├── Contents/# Party, Quest, Matching 시스템
│   │   │   └── Interactions/ # Door, Trigger 팩토리
│   │   └── DB/          # DbTransaction, DataModel
│   ├── ServerCore/      # 네트워크 베이스 엔진 (Listener, Session, Core)
│   ├── CommonDB/        # 공유 데이터 EF Core 프로젝트
│   └── LoadTestClient/  # 부하 테스트 봇 클라이언트
```

---

## Getting Started (Local)

이 레포는 **클라이언트(Unity)** / **서버(.NET)** / **공용(Protobuf 등)** 로 구성됩니다.

1. `Server/` 하위 솔루션을 연 뒤 서버를 실행합니다.
2. Unity 클라이언트에서 서버 주소·포트를 설정한 후 실행합니다.
3. 대량 접속·부하 테스트가 필요하면 `LoadTestClient`를 사용합니다.

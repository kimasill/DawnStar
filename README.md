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
    <img src="https://kimasill.github.io/images/dawnstar/DawnstarTitleImg.png" alt="Dawnstar 타이틀" width="640" />
  </a>
</p>

링크 · [프로젝트 페이지](https://kimasill.github.io/projects/dawnstar.html) · [상세 개발 과정 (dawnstar-process)](https://kimasill.github.io/projects/dawnstar-process.html) · [웹 포트폴리오](https://kimasill.github.io/)

### Overview

| 항목 | 내용 |
| --- | --- |
| 장르 | 2D 다크 판타지 MMORPG |
| 엔진·스택 | Unity · C# · .NET · SQL Server |
| 기간·규모 | 1인 · 2024.08 ~ 2025.02 |

### Role

- 클라이언트·서버·DB·콘텐츠(퀘스트·전투·경제·파티·월드 디자인) 설계·구현.

---

## Visual: Network & Game Server

<p align="center">
  <img src="https://kimasill.github.io/images/dawnstar/%EB%84%A4%ED%8A%B8%EC%9B%8C%ED%81%AC%20%EC%95%84%ED%82%A4%ED%85%8D%EC%B2%98.png" alt="Dawnstar 네트워크 아키텍처" width="820" />
</p>

<p align="center">
  <img src="https://kimasill.github.io/images/dawnstar/%EA%B2%8C%EC%9E%84%EC%84%9C%EB%B2%84%EC%95%84%ED%82%A4%ED%85%8D%EC%B2%98.png" alt="Dawnstar 게임 서버 아키텍처" width="820" />
</p>

---

## Core Implementation

### 1. Login & Lobby – 로그인 후 상태 한 축으로 묶기

로그인 직후 로비·캐릭터 목록과 `ServerState`가 어긋나면, 입장·복원·퀘스트·인벤 복구까지 이어지는 흐름이 중간에 끊긴다.

인벤토리 초기화가 되지 않거나나 맵·세션 상태가 꼬이면 유저 입장에서는 진행 불가에 가깝다. 원인도 클라·서버·DB 중 어디인지 파악하기 어렵게 디버깅 지점이 흩어진다.

토큰으로 계정을 조회한 뒤 `S_Login`에 로비 캐릭터 정보를 한 번에 실어 보내고 세션을 `Lobby`로 옮겼다. 응답을 여러 번 쪼개지 않고, 패킷 한 번에 스냅샷과 상태 전이를 묶는 쪽을 택했다.

로그인 직후 꼬이는 문제는 결국 `loginResponse` 에 담아 보냄으로써 해결되었다.

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

맵 ID만 갱신하고 HP·좌표·Idle을 서버와 맞추지 않으면 전투·파티 동기화가 한 번에 무너진다.

처음에는 맵 전환만 빠르게 붙였다가 불일치가 잦아서, `LeaveGame`으로 정리한 뒤 `EnterGame`으로 같은 룸에 다시 넣는 순서를 고정했다. 이후에는 `Leave -> Enter` 시퀀스 하나만 추적해도 재현 구간이 금방 드러났다.

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

포탈을 통한 맵 이동도 같은 원칙 — Leave 후 DB에 맵 정보를 저장하고, 목적지 룸에 Enter하는 형태로 통일했다.

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

DB에 있는 퀘스트 행과 메모리 진행 정보가 어긋나면 완료·보상·진행도 판정이 흔들린다. 이미 받은 보상이 사라지거나 조건을 채워도 완료가 안 될수있기 때문이다.

클라이언트 쪽 카운터만 올리는 식도 잠깐 검토했으나, 소유자(`OwnerDbId`)·템플릿 기준으로 `QuestDb`를 읽고 메모리 Progress를 그에 맞추는 편이 분기가 늘어날 때 추적이 쉬웠다.

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

골드·인벤·DB 갱신이 함수마다 분산되지 않게 하기위해서, 비용 검증 뒤 인벤과 DB 반영을 `DbTransaction` 으로 묶어, 상점·강화·제작 어디서든 성공/실패가 같은 경로에 남게 했다.

> 📄 [`Server/Server/Game/Room/GameRoom_Item.cs`](https://github.com/kimasill/DawnStar/blob/main/Server/Server/Game/Room/GameRoom_Item.cs#L296-L360) — `HandleEnhanceItem` 등 아이템 경제 흐름
>
> 📄 [`Server/Server/DB/DbTransaction.cs`](https://github.com/kimasill/DawnStar/blob/main/Server/Server/DB/DbTransaction.cs) — 트랜잭션 큐 + DB 저장 통합

```csharp
// 강화 결과를 DB 트랜잭션으로 일원화
DbTransaction.SaveEnhancedItemDB(player, newItemDb, this);
```

---

### 5. World Interaction – 타입별 상호작용 스폰

`InteractionType`별로 `Door`, `Trigger` 등 구체 타입으로 분기해 인스턴스를 만든다. 직렬화 데이터를 단일 클래스에 우겨 넣으면 스키마가 바뀔 때마다 서버가 같이 깨져서, 팩토리 패턴에 가깝게 구현했다.

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

<img src="https://kimasill.github.io/images/dawnstar/파티.PNG" alt="Dawnstar 파티" width="640" />

대기열만 쌓이고 파티 생성·맵 이동이 없으면 유저는 "매칭 중" UI에 갇히고 던전은 시작되지 않는다.

맵별/채널 별로 4명이 차면 파티를 만들고 `JoinParty` 뒤 `EnterMap`까지 한 틱에서 처리했다. "매칭 완료"와 "같은 맵 배치"를 나누면 중간에 인원이 흩어지는 경우가 있어, 한 번에 묶었다.

결국 유저 기준으로는 "매칭됐다"보다 "같이 들어갔다"가 보여야 했다. 4명이 찼을 때 바로 같은 맵으로 넘기는 방식이 그 요구에 가장 가까웠다.

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

시야에 플레이어가 새로 잡힐 때마다 장비 풀 동기화와 시야 갱신을 즉시 전부 밀어 넣으면, 맵이 붐빌 때 틱당 작업이 폭증하고 전 유저의 체감 동기화가 함께 나빠진다.

이벤트마다 스로틀을 새로 박는 대신, 기존 `Room`의 지연 큐에 `EnqueueAfter`로 장비(예: **100ms**)·시야(예: **500ms**)를 넣었다. 기존 코드를 크게 건드리지 않아도 돼서 이쪽을 먼저 썼다.

가시 범위·순서 같은 MMO식 골격은 유지하고 부하를 완충하려는 의도로 작업했다. 로컬에서 더미 세션 10대를 같은 Zone에 몰아넣고 틱 시간을 찍어 봤을 때, 즉시 전송 대비 지연 큐 쪽이 틱 스파이크가 뚜렷하게 낮았고, 이 값(100ms / 500ms)을 기준으로 고정했다.

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

- **패킷 파이프라인** — 수동 직렬화와 `PacketGenerator`를 거쳐 최종 기준을 `Protocol.proto`에 두었다. 스키마가 바뀔 때 손볼 곳을 프로토콜 정의와 자동 생성 핸들러로 묶어 `Write/Read` 분기 누락 위험을 낮췄다.
- **동시성·동기화** — MMO 기본 틀은 유지하고, 부하 대응은 지연 큐와 시야 갱신 주기로 풀었다.
- **세계 일관성** — 맵만 바꾸는 수준에서 끝내지 않았다. 맵 상태, 상호작용, 파티가 서버·DB와 같은 순서로 맞물리게 시퀀스를 쪼개 두었고, 상태가 어긋났을 때도 어느 단계에서 틀어졌는지 바로 좁혀갈 수 있게 했다.

---

## Result

- MMORPG 의 핵심요소 전반을 구현 하였으며 지속적인 업데이트가 가능한 구조를 완성함.

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

이 레포는 **클라이언트(Unity)** / **서버(.NET)** / **공용(Protobuf 등)** 프로젝트로 구성됩니다.

1. `Server/` 하위 솔루션을 열고 서버를 실행합니다.
2. 클라이언트는 Unity 프로젝트를 열어 서버 주소/포트 설정 후 실행합니다.
3. 대량 접속/부하 테스트는 `LoadTestClient`를 이용합니다.

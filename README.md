# DawnStar - 2D MMO Game Project

<p align="center">
  <a href="https://github.com/kimasill/DawnStar"><img alt="GitHub Repo" src="https://img.shields.io/badge/GitHub-DawnStar-181717?style=for-the-badge&logo=github&logoColor=white" /></a>
  <img alt="C#" src="https://img.shields.io/badge/C%23-.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" />
  <img alt="Unity" src="https://img.shields.io/badge/Unity-222222?style=for-the-badge&logo=unity&logoColor=white" />
  <img alt="SQL Server" src="https://img.shields.io/badge/SQL%20Server-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white" />
  <img alt="Protobuf" src="https://img.shields.io/badge/Protobuf-4285F4?style=for-the-badge&logo=google&logoColor=white" />
</p>

> Unity 클라이언트 + C#(.NET) 전용 서버 기반 2D MMORPG.  
> 로그인/캐릭터 라이프사이클, 맵/던전 시퀀스, 퀘스트/성장, 아이템 경제, 파티 매칭, Interest/TaskQueue 동기화까지 **플레이 루프 전체를 서버 흐름 안에서 완결**하는 것을 목표로 했습니다.

## Links

- **Portfolio (PDF/웹)**: `https://kimasill.github.io/`
- **Project Page**: `https://kimasill.github.io/projects/dawnstar.html`

## System Architecture (High-level)

<p align="center">
  <img src="https://kimasill.github.io/images/dawnstar/%EB%84%A4%ED%8A%B8%EC%9B%8C%ED%81%AC%20%EC%95%84%ED%82%A4%ED%85%8D%EC%B2%98.png" alt="Network Architecture" width="820" />
</p>
<p align="center">
  <img src="https://kimasill.github.io/images/dawnstar/%EA%B2%8C%EC%9E%84%EC%84%9C%EB%B2%84%EC%95%84%ED%82%A4%ED%85%8D%EC%B2%98.png" alt="Game Server Architecture" width="820" />
</p>

DawnStar는 유니티(Unity) 기반의 클라이언트와 C# (.NET Core) 기반의 전용 게임 서버로 구성된 2D MMORPG 프로젝트입니다. 안정적인 처리와 높은 동시 접속자 수용을 목표로 다중 스레드 기반 서버 아키텍처 및 게임 로직 최적화가 적용되어 있습니다.

## 주요 특징 (Key Features)

- **비동기 네트워크 I/O (Asynchronous Network)**
  - 고성능 소켓 프로그래밍을 활용한 Event-driven 방식의 빠르고 안정적인 패킷 송수신을 지원합니다.
- **관심사 관리 (Interest Management / Vision)**
  - 2D 존(그리드, 섹터) 기반의 시야 처리를 통해 서버 브로드캐스트 부하를 최소화하고, 클라이언트에게 필요한 주변 정보만을 효율적으로 전송합니다.
- **작업 대기열 (Task Queue / Job Manager)**
  - 멀티스레드 환경에서의 Lock 경합 및 데드락 문제를 방지하기 위해 Command Pattern 방식의 `TaskQueue`를 도입하여, 메인 게임 스레드에서 안전하게 로직을 처리합니다.
- **모듈화된 레지스트리 (Entity & Connection Registry)**
  - `EntityRegistry`(오브젝트 관리) 및 `ConnectionRegistry`(세션 관리)를 통해 접속 중인 클라이언트부터 월드 내 오브젝트 관리까지 체계적인 도메인 분리 방식을 채택했습니다.
- **데이터베이스 연동 (Entity Framework Core)**
  - 메인 게임 스레드의 지연(blocking)을 억제하기 위해 DB 관련 처리는 비동기로 분리(DB Task) 처리하며, EF Core를 통해 안정적인 데이터(캐릭터, 인벤토리, 게임 정보) CRUD를 구현합니다.
- **계정 인증 서버 (Account Server 분리)**
  - ASP.NET Web API 기반의 `AccountServer`를 구현하여, 로그인/인증 트래픽과 인게임 트래픽을 분리하여 시스템 확장성을 높였습니다.

## 상위 구조 및 디렉토리 개요 (Project Structure)

프로젝트 레포지토리는 크게 클라이언트, 백엔드 서버군, 공통 요소를 담당하는 디렉토리로 구성됩니다.

```text
MMOProject/
├── Client/              # Unity 2D 게임 클라이언트 프로젝트 목록 (에셋, 스크립트 등)
├── Common/              # 서버/클라이언트가 공유하는 데이터 스키마 및 Protobuf 정의 파일
├── Server/
│   ├── UserSettings/    # 프로젝트별 설정 파일 경로
│   ├── AccountServer/   # 계정 인증, 서버 리스트 제공 (ASP.NET Core Web API)
│   ├── Server/          # 메인 게임 서버 로직 (Game Instance, Entity, 로직 스레드 등)
│   ├── ServerCore/      # 네트워크 베이스 엔진 (Listener, Session, Core System)
│   ├── CommonDB/        # 계정, 공유 데이터 처리를 위한 EF Core 프로젝트│   
│   └── LoadTestClient/  # (구 DummyClient) 인프라 부하 테스트를 위한 봇 클라이언트
```

## 기술 스택 (Tech Stack)

### 백엔드 (Game Server / API)
* **언어:** C# 12 (.NET 8.0)
* **네트워크:** .NET Core Sockets, Google Protobuf (v3.27+)
* **데이터베이스:** Entity Framework Core (SQL Server)
* **구조:** Multi-Threaded Task Queue 기반 논블로킹 로직루프 구성 (GameLogic Task, DB Task, Network Task 분리 등)

### 프론트엔드 (Client)
* **엔진:** Unity3D
* **언어:** C#
* **통신:** 전용 Socket Client, HTTP (로그인 및 서버 리스트용)
**클라이언트 실행 (Client Execution)**
   - 계정 가입 및 인증을 확인한 이후, 서버의 IP와 웹 API 포트로 접속이 이루어집니다. `LoadTestClient`를 통해 대량의 봇(Bot)을 동시 접속시켜 서버 퍼포먼스를 점검해 볼 수 있습니다.

---

## 핵심 구현 & 트러블슈팅 (Portfolio Highlights)

PDF에서 요약했던 핵심 이슈를, 여기서는 “왜 그 선택을 했는지 / 코드가 어디에 있는지”까지 연결합니다.

### 1) 로그인 이후 상태를 한 축으로 묶기 (Login → Lobby → EnterGame)

- **문제**: 로그인 직후 상태가 쪼개져 있으면(로비/캐릭터/세션), 입장/복원/인벤/퀘스트 흐름이 쉽게 꼬이고 디버깅 지점도 분산됨
- **해결**: 토큰 검증 이후 `S_Login`에 로비 캐릭터 스냅샷을 담아 **패킷 1회 + 상태 전이 1회**로 고정
- **코드**: `Server/Server/Session/ClientSession_preGame.cs`

```csharp
S_Login loginResponse = new S_Login { LoginOk = 1 };
foreach (PlayerDb player in findAccount.Players)
{
    LobbyPlayerInfo summary = ToLobbyPlayerInfo(player);
    LobbyPlayers.Add(summary);
    loginResponse.Players.Add(summary);
}
Send(loginResponse);
ServerState = PlayerServerState.ServerStateLobby;
```

### 2) 맵 전환/리스폰을 시퀀스로 고정하기 (Leave → Restore → Enter)

- **문제**: 맵 ID만 바꾸는 식의 전환은 서버/클라 판정 불일치로 전투/파티/동기화가 연쇄 붕괴
- **해결**: `LeaveGame`으로 정리 후 리스폰 상태를 복원하고, `EnterGame`으로 재진입하는 시퀀스를 고정
- **코드**: `Server/Server/Game/Room/GameRoom_Sequence.cs`

### 3) Interest Management 부하 완충 (장비 지연 + 시야 갱신 주기)

- **문제**: 시야에 새 오브젝트가 잡힐 때마다 즉시 “풀 동기화”를 하면 틱당 작업이 폭증해 전체 체감이 악화
- **해결**: 룸의 지연 큐(TaskQueue)에 **장비 동기화(예: 100ms)**, **시야 갱신(예: 500ms)** 을 넣어 스파이크를 완화
- **코드**: `Server/Server/Game/Room/InterestManagement.cs`, `Server/Server/Game/Job/TaskQueue.cs`

```csharp
// 시야에 잡힌 타 플레이어 장비 정보를 룸 큐에서 지연 동기화
Owner.Room.EnqueueAfter(100, Owner.Room.HandleEquippedItemList, Owner, player, true);
// 시야 갱신 주기 예약
Owner.Room.EnqueueAfter(500, Update);
```

---

## Getting Started (Local)

이 레포는 **클라이언트(Unity)** / **서버(.NET)** / **공용(Protobuf 등)** 프로젝트로 구성됩니다.

- `Server/` 하위 솔루션을 열고 서버를 실행합니다.
- 클라이언트는 Unity 프로젝트를 열어 서버 주소/포트 설정 후 실행합니다.
- 대량 접속/부하 테스트는 `LoadTestClient`를 이용합니다.

> 자세한 실행 절차는 환경별로 다르므로, 추후 `docs/`로 분리해 보강할 예정입니다.

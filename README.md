# DawnStar - 2D MMO Game Project

DawnStar는 유니티(Unity) 기반의 클라이언트와 C# (.NET Core) 기반의 전용 게임 서버로 구성된 2D MMORPG 프로젝트입니다. 안정적인 처리와 높은 동시 접속자 수용을 목표로 다중 스레드 기반 서버 아키텍처 및 게임 로직 최적화가 적용되어 있습니다.

## 📌 주요 특징 (Key Features)

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

## 📁 상위 구조 및 디렉토리 개요 (Project Structure)

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
│   ├── CommonDB/        # 계정, 공유 데이터 처리를 위한 EF Core 프로젝트
│   ├── PacketGenerator/ # Protobuf 패킷 파서 및 자동 생성 파일 빌드 툴
│   └── LoadTestClient/  # (구 DummyClient) 인프라 부하 테스트를 위한 봇 클라이언트
```

## 🛠 기술 스택 (Tech Stack)

### 백엔드 (Game Server / API)
* **언어:** C# 12 (.NET 8.0)
* **네트워크:** .NET Core Sockets, Google Protobuf (v3.27+)
* **데이터베이스:** Entity Framework Core (SQL Server)
* **구조:** Multi-Threaded Task Queue 기반 논블로킹 로직루프 구성 (GameLogic Task, DB Task, Network Task 분리 등)

### 프론트엔드 (Client)
* **엔진:** Unity3D
* **언어:** C#
* **통신:** 전용 Socket Client, HTTP (로그인 및 서버 리스트용)

## 🚀 시작하기 (Getting Started)

1. **사전 준비 (Prerequisites)**
   - Visual Studio 2022 이상 (.NET 8 SDK 포함)
   - MS SQL Server (EF Core 마이그레이션 적용 및 DB 생성)
   - Unity (해당 프로젝트 버전 호환 빌드)

2. **패킷 생성 (Packet Generator)**
   - `Common` 폴더에서 Protobuf 패킷 프로토콜을 변경한 후 `PacketGenerator` (GenPackets.bat 등)를 실행하면, 자동으로 서버와 클라이언트의 패킷 핸들러 및 C# 코드가 복사·생성됩니다.

3. **서버 실행 (Server Execution)**
   - Visual Studio에서 시작 프로젝트를 다중 프로젝트로 설정하여 `Server`(인게임), `AccountServer`(로그인)를 동시에 구동합니다.

4. **클라이언트 실행 (Client Execution)**
   - 계정 가입 및 인증을 확인한 이후, 서버의 IP와 웹 API 포트로 접속이 이루어집니다. `LoadTestClient`를 통해 대량의 봇(Bot)을 동시 접속시켜 서버 퍼포먼스를 점검해 볼 수 있습니다.

## 📝 개발자 노트 (Developer Notes)
본 프로젝트는 지속적인 아키텍처 개선(Refactoring) 작업을 통해 상용 서버급의 구조적 전문성을 확보해가고 있습니다. 최근의 튜토리얼 잔재 청산 및 네이밍 리팩토링(VisionCube → InterestManagement, ObjectManager → EntityRegistry 등)을 바탕으로 한층 더 직관적인 도메인 주도 설계 기반으로 발전하는 중입니다.

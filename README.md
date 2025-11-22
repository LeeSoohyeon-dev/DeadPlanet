# Dead Planet

**Dead Planet**은 Unity 6로 제작된 모바일 탑다운 슈팅 게임입니다. 외계 행성을 배경으로 적들을 처치하는 것이 목표입니다.

---

## 다운로드
<a href="https://play.google.com/store/apps/details?id=com.in.deadplanet" style="text-decoration: none;">
  <img src="https://play.google.com/intl/en_us/badges/static/images/badges/ko_badge_web_generic.png" alt="Google Play에서 다운로드" height="80"/>
</a>
<a href="https://play.google.com/store/apps/details?id=com.in.deadplanet" style="text-decoration: none;">
 <img src="./Images/Dead_Planet_QR.png" alt="QR Code" height="80" style="display: inline-block; vertical-align: middle; border: none;"/>
</a>

---

### 주요 특징
- **상태 패턴**으로 적 타입별 행동 패턴 구현
- **BFS 공간 탐색 알고리즘**을 사용한 버프 시스템
- **DOTween**을 활용한 UI 애니메이션
- **애니메이션 리깅**을 활용해 무기 5종 IK 설정
- **UniTask**를 활용한 비동기 시퀀스 구현으로 이벤트 순차/병렬 처리

---

## 프로젝트 구조

```
2.Scripts/

│

├── Character/

│   ├── Player/

│   │   ├── Core/              # 플레이어 핵심 로직

│   │   ├── Combat/            # 전투, 무기, 이동

│   │   ├── Health/            # 체력 관리

│   │   └── Interaction/       # 플레이어 아이템 상호작용

│   │

│   ├── Enemy/

│   │   ├── Core/              # AI, 이동, 비주얼

│   │   ├── Combat/            # 적 무기 시스템, 수류탄 투척

│   │   ├── Health/            # 체력 관리

│   │   ├── Loot/              # 아이템 드롭

│   │   ├── State Machine/     # 상태 기계 패턴

│   │   └── Type/              # 적 타입별 구현

│   │

│   └── Health/                # 공통 체력 시스템

│

├── Weapons/

│   ├── Core/                  # 무기, 총알 기본 로직, Factory

│   ├── Model/                 # 무기 모델 데이터

│   └── Enemy Weapon Data/     # 적 무기 데이터

│

├── Game System/

│   ├── Game/                  # 게임 시스템 및 이벤트

│   ├── Audio/                 # 오디오 매니저 (우선순위 시스템)

│   ├── Buff/                  # 버프 시스템

│   ├── Object Pool/           # 오브젝트 풀링

│   └── Interface/             # 공통 인터페이스

│

├── UI/                        # 사용자 인터페이스

├── Mission/                   # 미션 시스템

└── Interaction/               # 상호작용 오브젝트

```
---

## 주요 알고리즘

### 1. BFS 공간 탐색 알고리즘 (버프 스폰 위치 최적화)

Queue + HashSet을 활용한 BFS(너비 우선 탐색) 알고리즘으로 버프 스폰 위치를 최적화

- 플레이어 위치에서 시작하여 가장 가까운 유효한 위치 보장
- 8방향 탐색으로 체계적인 공간 탐색
- NavMesh 샘플링과 다중 조건 검증 (플레이어/적/버프 거리)
- 적 리스트 1초 간격 캐싱으로 성능 최적화
- O(1) HashSet 조회, 최대 탐색 반복 제한으로 안정적 동작

### 2. 가중치 기반 버프 스폰 시스템

플레이어 상태에 따른 동적 가중치 계산을 통해 버프 스폰 시스템을 구현

- 플레이어 상태(체력, 탄약)에 따른 동적 가중치 계산
- 체력 < 30%: 체력 회복 버프 가중치 +40%
- 탄약 부족 시: 탄약 회복 버프 가중치 +30%
- 누적 가중치 합산 방식으로 확률 계산
- 실시간 플레이어 상태 체크로 동적 조정

### 3. 가중치 활용 타겟 선정 알고리즘 (모바일 조준 보정)

Physics.OverlapSphere와 가중치 점수 계산을 활용하여 모바일 환경에 최적화된 조준 보정 시스템을 구현

- Physics.OverlapSphere로 범위 내 적 검색
- 거리 점수(60%)와 각도 점수(40%)를 정규화하여 가중치 적용
- 각도 필터링(assistAngle/2)으로 시야 밖 적 제외하여 성능 최적화
- Slerp 보간으로 플레이어 회전과 탄환 방향을 부드럽게 보정

### 4. 최소 재생 간격 및 우선순위 시스템 (사운드 중복 재생 방지)

Cooldown-based 알고리즘과 HashSet 기반 우선순위 비교를 통해 사운드 중복 재생을 방지하고 중요 사운드를 보호하는 시스템을 구현했습니다.

- Cooldown-based 알고리즘: Dictionary<SoundType, float>로 각 사운드의 마지막 재생 시간 기록
- HashSet 기반 우선순위 비교: 현재 재생 중인 사운드 추적 및 우선순위 비교

---

## 디자인 패턴

### 1. State 패턴
적 타입별 행동 패턴 구현

- **Melee**: Idle, Move, Chase, Attack, Recovery, Dead

- **Range**: Idle, Move, Battle, AdvancePlayer, ThrowGrenade, Dead  

- **Boss**: Idle, Move, Attack, JumpAttack, Ability, Dead

### 2. Factory & Object Pool 패턴

**Factory Pattern**: WeaponFactory로 Weapon 객체 생성 로직 중앙화

**Object Pool Pattern**: Unity의 `IObjectPool<T>`를 활용한 효율적인 객체 재사용 시스템

- 총알, 이펙트, 픽업 아이템, 버프 등 고빈도 생성 객체 풀링

- Dictionary<GameObject, IObjectPool>를 활용한 O(1) 조회 성능

- 동적 풀 생성: 존재하지 않는 프리팹 요청 시 런타임에 자동 생성

- 최대 풀 크기 제한으로 메모리 관리

- UniTask 기반 비동기 지연 반환으로 파티클/이펙트 재생 보장

### 3. Event Bus 패턴

`GameEvents` static 클래스를 통한 중앙화된 이벤트 시스템으로 컴포넌트 간 직접 참조 없이 통신

**주요 이벤트:**
- 게임 상태: `OnGameVictory`, `OnGameOver`, `OnGameRestart`
- 오디오 제어: `OnPlaySound`, `OnStopBGM`, `OnStopAllAudio`
- 타임 관리: `OnSlowMotion`, `OnPauseTime`, `OnResumeTime`
- UI 업데이트: `OnPlayerHealthChanged`, `OnWeaponUIUpdate`, `OnMissionUIUpdate`, `OnLootButtonUpdate`

---

## 주요 시스템

### 무기 시스템

- 5종의 무기별 고유한 효과와 애니메이션 리깅

- 2개 무기 슬롯 & 교체 시스템

- ScriptableObject 기반 데이터 관리

- Factory Pattern을 통한 무기 생성
  
### 적 시스템

State Machine 패턴으로 구현된 3가지 적 타입

#### 근접 적 (Melee)

- 다양한 근접 무기
  
- ScriptableObject 기반 공격 패턴
  
- Shield 타입: 방패 내구도 시스템

#### 원거리 적 (Range)

- 5가지 무기와 무기 타입별 리깅, 레이어
  
- Grenade: 물리 기반 포물선 계산 수류탄 투척

#### 보스 (Boss)

- 화염방사기 (지속 대미지)
  
- 점프 공격 (광역 대미지 + 넉백)
  
---

### 버프 시스템

#### 시스템 특징

- **3가지 버프 타입**: 탄약 회복, 체력 회복, 실드 (내구도 방어막)

- **BFS 기반 스폰 위치 최적화**: 플레이어/적/버프 간 최적 거리 확보

- **가중치 기반 적응형 드랍**: 플레이어 상태에 따른 동적 확률 조정

### 모바일 조준 보정 시스템

#### 가중합산 타겟 선정 알고리즘

- **Physics.OverlapSphere**: 범위 내 적 검색

- **가중치 점수 계산**: 거리 점수(60%) + 각도 점수(40%)

- **각도 필터링**: 시야 밖 적 제외로 성능 최적화

# 🌌 Dead Planet

## 📖 소개

**Dead Planet**은 Unity 6로 제작된 모바일 3인칭 탑다운 슈팅 게임입니다. 외계 행성을 배경으로 적들을 처치하는 것이 목표입니다.

### ✨ 주요 특징

- 🎯 **무기 교체 시스템** - 5종의 무기를 자유롭게 교체
- 🤖 **다양한 적** - State Machine 기반의 다양한 능력을 가진 적들
- 🎨 **Animation Rigging** - IK를 활용한 무기별 리깅
- ⚡ **성능 최적화** - 오브젝트 풀링 및 이벤트 기반 아키텍처

---

## 🛠️ 개발 환경

| 항목      | 내용                                  |
| ------- | ----------------------------------- |
| **엔진**  | Unity 6 (Universal Render Pipeline) |
| **플랫폼** | Mobile (Android)                    |
| **언어**  | C#                                  |


---

## 📁 프로젝트 구조

```
Assets/!_Project_Main/2.Scripts/
│
├── 🎮 Character/
│   ├── Player/
│   │   ├── Core/              # 플레이어 핵심 로직
│   │   ├── Combat/            # 전투, 무기, 이동
│   │   ├── Health/            # 체력 관리
│   │   └── Interaction/       # 아이템 상호작용
│   │
│   ├── Enemy/
│   │   ├── Core/              # AI, 이동, 비주얼
│   │   ├── Combat/            # 적 무기 시스템
│   │   ├── Health/            # 체력 관리
│   │   ├── Loot/              # 아이템 드롭
│   │   ├── State Machine/     # 상태 기계 패턴
│   │   └── Type/              # 적 타입별 구현
│   │
│   └── Health/                # 공통 체력 시스템
│
├── 🔫 Weapons/
│   ├── Core/                  # 무기, 총알 기본 로직
│   ├── Model/                 # 무기 모델 데이터
│   └── EnemyWeaponData/       # 적 무기 데이터
│
├── ⚙️ GameSystem/
│   ├── Game/                  # 게임 시스템 및 이벤트
│   ├── Audio/                 # 오디오 매니저
│   ├── Object Pool/           # 오브젝트 풀링
│   └── Interface/             # 공통 인터페이스
│
├── 🎨 UI/                     # 사용자 인터페이스
├── 🎯 Mission/                # 미션 시스템
└── 🔗 Interaction/            # 상호작용 오브젝트
```

---

## 🎮 핵심 시스템

### 🔫 무기 시스템


#### 시스템 특징

- 2개 무기 슬롯 & 교체 시스템
- ScriptableObject 기반 데이터 관리
- Animation Rigging
- 무기별 애니메이션 레이어
- 물리 기반 총알 시스템
  
#### 플레이어 무기 (5종)

- Pistol (권총)
- Revolver (리볼버)
- Assault Rifle (돌격소총)
- Shotgun (샷건)
- Rifle (소총)

### 🤖 적 시스템

State Machine 패턴으로 구현된 3가지 적 타입:

#### 1️⃣ 근접 적 (Melee)

```
🗡️ 특징
- 순찰 → 추적 → 공격 패턴
- 다양한 근접 무기 (검, 도끼 등)
- ScriptableObject 기반 공격 패턴
- Shield 타입: 방패 내구도 시스템
```

#### 2️⃣ 원거리 적 (Range)

```
🎯 특징
- 5가지 무기 타입 (플레이어와 동일)
- 조준 시스템 (느린 조준 → 정조준 → 발사)
- 엄폐 및 전진 패턴
- Raycast 기반 시야 판정

🔥 특수 능력
- Unstoppable: 무한 탄약 + 느린 전진 사격
- Grenade: 포물선 계산 수류탄 투척
```

#### 3️⃣ 보스 (Boss)

```
💀 복합 공격 패턴
- 근접 공격 (다양한 애니메이션)
- 화염방사기 (Particle + 지속 대미지)
- 점프 공격 (광역 대미지 + 넉백)

⚙️ 구현
- Physics.OverlapSphere 판정
- HashSet 중복 대미지 방지
- AddExplosionForce 물리 효과
```

---

## 🔧 핵심 설계 패턴

### 1. State Machine 패턴

각 적 타입은 고유한 여러 상태를 가집니다:

- **Melee**: Idle, Move, Chase, Attack, Recovery, Dead
- **Range**: Idle, Move, Battle, AdvancePlayer, ThrowGrenade, Dead  
- **Boss**: Idle, Move, Attack, JumpAttack, Ability, Dead

```csharp
기본 구조
├── EnemyState (상태 기본 클래스)
├── EnemyStateMachine (상태 전환 관리)
└── 타입별 구체적인 상태 클래스 구현
```

### 2. 이벤트 시스템

```csharp
GameEvents (정적 클래스) - 느슨한 결합
├── 게임 상태 (승리, 패배, 재시작)
├── 오디오 제어
└── UI 업데이트
```

### 3. 애니메이션 시스템

#### Animation Rigging 활용

- **TwoBoneIKConstraint**: 왼손이 무기 홀드 포인트 추적
- **MultiAimConstraint**: 원거리 적의 상체가 플레이어 추적
- **Rig Weight 제어**: 재장전/무기교체 시 IK 자동 제어

#### 애니메이션 이벤트

- **PlayerAnimationEvents**: 재장전, 무기 교체 타이밍 등
- **Enemy_AnimationEvents**: 공격 판정, 수류탄 투척, 점프 착지 등

#### 무기별 레이어 시스템

- **Player**: 4개 레이어 (handgun, assault, shotgun, common)
- **Enemy_Range**: 3개 레이어 (assault, handgun, common)

---

## 📝 라이선스

이 프로젝트는 개인 포트폴리오 목적으로 제작되었습니다.

---

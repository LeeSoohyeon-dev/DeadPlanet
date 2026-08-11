# Dead Planet

  

**Dead Planet**은 Unity 6로 제작된 모바일(안드로이드) 탑다운 슈팅 게임입니다. 외계 행성을 배경으로 적들을 처치하는 것이 목표입니다.

  

---

  

## APK 파일 다운로드

<a href="https://drive.google.com/file/d/17L4gmFcx8CV3zH5SIx-POS2ffaXJ8KUf/view?usp=drive_link" style="text-decoration: none;">

  <img src="https://img.shields.io/badge/Google_Drive-4285F4?style=for-the-badge&logo=googledrive&logoColor=white" alt="Google Drive에서 다운로드" height="60"/>

</a>

  

---

  

## 프로젝트 구조

  

```

2.Scripts/

├── Character/

│   ├── Player/          # Core, Combat(이동·사격·조준보정), Health, Interaction

│   ├── Enemy/           # Core(AI·이동·비주얼), Combat, Health, Loot,

│   │                    # State Machine, Type(Melee/Range/Boss별 상태 클래스)

│   └── Health/          # 공통 체력 시스템

├── Weapons/             # 무기·총알 로직, ScriptableObject 데이터

├── Game System/

│   ├── Game/            # GameManager, TimeManager, GameEvents(이벤트 버스)

│   ├── Audio/           # 사운드 우선순위 매니저

│   ├── Buff/            # 버프 스폰·가중치 선택·적용

│   └── Object Pool/     # 제네릭 오브젝트 풀 + 세대 카운터

├── UI/                  # 화면 전환, 인게임 HUD, DOTween 연출

├── Mission/             # 미션·웨이브·적 카운트 관리

└── Interaction/         # 무기/버프 픽업

```

  

---

  

## 비동기 처리: Coroutine → UniTask

  

프로젝트의 모든 비동기 작업(UI 페이드, 게임 시퀀스, 사운드 딜레이, 슬로우모션, 풀 반환 딜레이)을 UniTask로 구현했다.

  

- `GetCancellationTokenOnDestroy()`로 GameObject 파괴 시 자동 취소

- 경쟁이 생기는 예약은 자체 `CancellationTokenSource`로 명시 취소 (트러블슈팅 4번)

- 코루틴 대비 할당이 적고, try/catch로 취소를 명시적으로 다룰 수 있음

  

---

  

## 시스템 설계

  

### 게임플레이

  

- **무기 시스템** — 5종(Pistol, Revolver, Assault Rifle, Shotgun, Rifle), 2슬롯 교체, ScriptableObject(`Weapon_Data`) 기반 스탯, 무기별 Animation Rigging(TwoBoneIK로 왼손 부착, 재장전/교체 시 Rig Weight 자동 제어)

- **조준 보정(모바일)** — 반경 내 적을 거리 60% + 각도 40% 가중치로 스코어링해 타겟 선정, 캐릭터 회전과 탄도를 각각 보간 강도를 달리해 보정. 죽은 적(래그돌)은 타겟에서 제외

- **버프 시스템** — 플레이어 주변 링(5~15m)을 격자 탐색해 NavMesh 위 유효 위치(적·다른 버프와 최소 거리 확보)를 찾고, 실패 시 랜덤 샘플링으로 폴백. 버프 종류는 플레이어 상태(체력·탄약)에 따라 동적 가중치로 선택

- **웨이브 스폰** — 웨이브가 진행될수록 스폰 수가 증가하는 그룹 단위 적 스폰

- **미션 시스템** — 킬 카운트 미션, 이벤트 기반 진행도 갱신

  

### 적 AI — State Machine 패턴

  

| 타입 | 상태 | 특징 |

|---|---|---|

| Melee | Idle, Move, Chase, Attack, Recovery, Dead | 무기별 공격 데이터(SO), Shield 타입은 방패 내구도 |

| Range | Idle, Move, Battle, AdvancePlayer, ThrowGrenade, Dead | 정지 사격 ↔ 전진 패턴, 수류탄 투척(포물선 계산) |

| Boss | Idle, Move, Attack, JumpAttack, Ability, Dead | 화염방사(지속 대미지), 점프 공격(광역 넉백 + 착지 예고) |

  

- `EnemyState` 기본 클래스 + `EnemyStateMachine`으로 전이 관리

- 상태 전이 시 같은 프레임에 이전 상태 로직이 이어 실행되지 않도록 전이 직후 early return을 규칙화

- 사망 처리: 피격 → 사망 판정 → 드랍 → `DeadState`(래그돌) → 일정 시간 후 물리·콜라이더 비활성화로 시체 부하 제거

  

### 이벤트 버스 패턴

  

`public static event Action` + `Raise` 래퍼로 발행을 통제한 중앙 이벤트 허브. 게임 상태(승리/패배/재시작), 사운드, UI 갱신이 이벤트로 흐르므로 매니저 간 직접 참조가 없다.

  

- 구독 해제 규칙: MonoBehaviour는 `OnDisable`/`OnDestroy`에서, 순수 C# 객체(미션)는 소유자(`MissionManager`)가 파괴될 때 대신 해제

- static 이벤트 + 씬 재시작 조합에서 생기는 구독 누수를 수명주기 규칙으로 차단

  

### 우선순위 기반 오디오 매니저

  

모든 사운드는 `GameEvents.RaisePlaySound(SoundType)` 이벤트로 요청되고, 재생 여부는 `AudioManager`가 단독으로 결정한다. 요청하는 쪽은 "무엇을 재생할지"만 알고, "지금 재생해도 되는지"는 매니저의 정책이 판단한다.

  

**재생 결정 파이프라인** — 요청 1건은 세 단계를 통과해야 재생된다:

  

1. **쿨다운 검사** — `Dictionary<SoundType, float>`에 기록한 마지막 재생 시각과 사운드별 최소 재생 간격(`minPlayInterval`)을 비교. 연사 중 발사음처럼 같은 사운드 요청이 프레임 단위로 몰려도 스팸이 되지 않는다

2. **우선순위 판정** — `HashSet<SoundType>`으로 추적하는 "현재 재생 중인 사운드"의 최고 우선순위와 비교해, 낮으면 재생을 포기하고 높으면 낮은 우선순위 사운드들을 정지시키고 진입한다

3. **BGM 정책** — 우선순위 0은 BGM과 공존, 0보다 크면 BGM을 중단. 승리·게임오버 징글 같은 연출 사운드가 전투 소음과 BGM에 묻히지 않도록 보호하는 규칙

  

**데이터 주도 설정** — 기본값(`defaultSettings`) 하나에 예외 딕셔너리(`exceptionSettings`)로 타입별 오버라이드를 얹는 구조. 새 사운드를 추가할 때 코드 수정 없이 인스펙터에서 간격·우선순위만 지정하면 된다.

  

**GC 회피** — 정지·정리 대상을 매번 새 리스트로 만들지 않고 재사용 버퍼에 모아서 처리. 순회 중 컬렉션 수정 예외와 매 프레임 할당을 동시에 피한다.

  

---

  

## 트러블슈팅

  

### 1. 시간이 지나면 버프가 더 이상 스폰되지 않는 버그

  

**문제**: 플레이 5분쯤 지나면 버프가 맵에 전혀 나타나지 않음.

  

**원인**: 스폰 위치를 `List<Vector3>`에 y=0으로 평탄화해 저장했는데, 획득 시에는 실제 월드 좌표(y≈0.5)로 `Remove()`를 호출했다. `Vector3`는 컴포넌트 단위 값 비교라 두 좌표가 절대 일치하지 않았고, 리스트가 무한히 증가하면서 "다른 버프와 최소 거리 5m" 검사가 맵 전체를 잠식해 유효한 스폰 위치가 사라졌다.

  

**해결**: float 좌표를 딕셔너리/리스트의 키로 쓰는 것 자체가 함정이라고 판단하고, 위치 값 대신 **스폰된 `Pickup_Buff` 인스턴스 참조를 추적**하도록 변경했다. 획득·파괴 시 참조로 제거하므로 부동소수 오차와 무관하게 동작한다.

  

```csharp

// Before: 값 비교가 실패해 리스트가 무한 증가

spawnedBuffPositions.Remove(position);   // y가 달라 절대 매칭 안 됨

  

// After: 참조 기반 추적

private List<Pickup_Buff> spawnedBuffs;

public void OnBuffCollected(Pickup_Buff buff) => spawnedBuffs.Remove(buff);

```

  

### 2. 재시작을 반복하면 슛 버튼 한 번에 여러 발이 나가는 버그

  

**문제**: 게임오버 후 재시작을 N번 하면 슛 버튼 1회 터치에 N+1발이 발사되고, 콘솔에 `MissingReferenceException`이 쌓임.

  

**원인**: UI는 `DontDestroyOnLoad`로 씬 재시작에도 살아남지만 Player는 씬과 함께 파괴되고 새로 생성된다. 새 Player가 매번 `ShootButton.onClick.AddListener(...)`를 호출하는데 이전 Player의 리스너를 아무도 해제하지 않아, 파괴된 Player를 캡처한 리스너가 재시작마다 누적됐다.

  

**해결**: 등록하는 델리게이트를 필드에 보관하고 `OnDestroy()`에서 `RemoveListener`로 해제. 같은 원리로 `PlayerIA`(Input System, `IDisposable`)도 `OnDestroy()`에서 `Dispose()`하고, static 이벤트(`GameEvents`)를 구독하는 미션 객체는 `MissionManager.OnDestroy()`에서 구독을 정리하도록 수명주기를 통일했다.

  

**교훈**: `DontDestroyOnLoad` 객체와 씬 객체가 서로를 참조하는 경계에서는 "누가 언제 구독을 해제하는가"를 항상 명시해야 한다.

  

### 3. 오브젝트 풀 지연 반환이 사용 중인 오브젝트를 회수하는 레이스

  

**문제**: 피격 이펙트가 재생 도중 갑자기 사라지거나, 풀에서 `InvalidOperationException`(이중 Release)이 발생.

  

**원인**: `ReturnObject(obj, 1f)`처럼 지연 반환을 예약한 뒤, 타이머 만료 전에 오브젝트가 다른 경로로 반환되고 곧바로 재대여되는 경우가 있다. 만료된 타이머는 `activeSelf == true` 가드를 통과해 **다른 용도로 사용 중인 오브젝트를 회수**해 버렸다.

  

**해결**: 풀 오브젝트마다 **세대(generation) 카운터**를 두고, 대여 시마다 증가시켰다. 반환 예약은 예약 시점의 세대를 기억하고, 만료 시 세대가 달라져 있으면(이미 반환-재대여됨) 예약을 무시한다.

  

```csharp

// 예약 시점의 세대 기억 → 발화 시 세대가 다르면 무효

int generationAtSchedule = pooledObj.Generation;

await UniTask.WaitForSeconds(delay, cancellationToken: ct);

if (pooledObj.Generation != generationAtSchedule) return;

```

  

추가로 풀에 **프리웜**을 넣어 첫 발사/첫 이펙트 순간의 `Instantiate` 스파이크를 제거했다 (`defaultCapacity`는 내부 컬렉션 용량일 뿐 인스턴스를 미리 만들어주지 않는다).

  

### 4. 게임오버의 시간 정지가 저절로 풀리는 경쟁 조건

  

**문제**: 처치 연출용 슬로우모션이 걸린 직후에 게임오버가 겹치면, 시간 정지로 멈췄던 게임이 1~2초 뒤 저절로 다시 흐르면서 게임오버 화면 뒤로 게임이 계속 진행됨. 타이밍이 겹칠 때만 발생해 재현이 어려웠다.

  

**원인**: 슬로우모션은 "N초 뒤 timeScale 복원"을 fire-and-forget 태스크로 예약한다. 이 대기는 슬로우모션 중에도 타이머가 흘러야 하므로 **unscaled time 기준**인데, 바로 그 때문에 게임오버가 `timeScale = 0`으로 정지시킨 뒤에도 타이머는 계속 흘러 만료 시점에 복원을 실행했다. **전역 상태(timeScale)를 두 비동기 흐름이 각자 조작하면 "나중에 실행된 쪽이 이기는" 구조**가 된 것.

  

**해결**: 슬로우모션 예약을 자체 `CancellationTokenSource`로 들고, 시간 정지와 새 슬로우모션 요청이 대기 중인 복구 예약을 **명시적으로 취소**하도록 했다. CTS는 `GetCancellationTokenOnDestroy()`와 링크해 오브젝트 파괴 시 자동 취소도 함께 보장한다.

  

```csharp

public void PauseTime()

{

    CancelPendingSlowMotion();   // 대기 중인 복구 타이머 무효화

    targetTimeScale = 0;

}

  

public void SlowMotionFor(float seconds)

{

    CancelPendingSlowMotion();   // 중복 예약 방지

    slowMotionCts = CancellationTokenSource.CreateLinkedTokenSource(

        this.GetCancellationTokenOnDestroy());

    SlowMotionForAsync(seconds, slowMotionCts.Token).Forget();

}

```

  

**교훈**: timeScale 같은 전역 상태는 소유자를 한 곳(`TimeManager`)으로 모으는 것만으로는 부족하고, **그 소유자 안에서 예약된 미래의 쓰기까지 취소 가능해야** 상태 전환이 원자적이 된다.

  

### 5. 일부 원거리 적이 조준만 하고 영영 사격하지 않는 버그

  

**문제**: 적이 몰려 나오는 웨이브에서 간헐적으로 원거리 적이 조준 자세만 취한 채 한 발도 쏘지 않음. 콘솔에 에러 한 줄이 남은 뒤로는 아무 증상이 없어(조용한 실패) 원인 추적이 어려웠다.

  

**원인**: 무기 모델 탐색이 실패하면 UniTask로 100ms×5회 **비동기 재시도**하는 경로가 있는데, `Enemy_Range.Start()`는 그와 무관하게 **동기적으로** `SetupWeapon()`을 호출해 총구(`gunPoint`)를 연결한다. 재시도 경로로 빠진 적은 이 시점에 모델이 없어 연결에 실패하고, 이후 재시도가 성공해도 `gunPoint`를 다시 연결하는 코드가 없었다. 발사 함수의 `if (gunPoint == null) return;` 가드가 실패를 조용히 삼켜 증상이 겉으로 드러나지 않았다.

  

**해결**: 재시도 성공 시점에 `SetupWeapon()`을 다시 호출해 의존 초기화를 재실행하도록 연결했다.

  

```csharp

// Enemy_Visuals — 재시도 성공 시점에 의존 초기화 재실행

currentWeaponModel.SetActive(true);

OverrideAnimatorControllerIfCan();

GetComponent<Enemy_Range>()?.SetupWeapon();   // gunPoint 재연결

```

  

**교훈**: fire-and-forget 비동기 초기화는 "성공 여부"만이 아니라 **완료 시점에 걸려 있는 의존 작업**까지 다시 트리거해야 한다. 그리고 null 가드는 방어 수단이지, 초기화 실패를 숨기는 용도가 되면 버그를 은폐한다.

  

### 6. 상태 전이 순서(Exit → Enter) 때문에 증발하는 플래그

  

**문제**: 수류탄 투척 모션 중(수류탄이 손을 떠나기 전)에 적을 처치하면 손에 들려 있던 수류탄이 발밑에 떨어져 터지는 연출을 만들었는데, 실제로는 수류탄이 그냥 사라짐.

  

**원인**: 상태 머신의 `ChangeState()`는 이전 상태 `Exit()` → 새 상태 `Enter()` 순서로 실행된다. "투척 진행 중" 플래그를 관례대로 `Exit()`에서 리셋했더니, **`DeadState_Range.Enter()`가 그 값을 읽기 전에 이미 지워져** 낙하 분기가 한 번도 실행되지 않았다.

  

**해결**: 플래그 리셋을 `Exit()`이 아니라 정상 전이 경로(투척 완료 시점)에서만 하도록 옮기고, 순서 의존을 주석으로 명시했다.

  

```csharp

// ThrowGrenadeState_Range.Update()

if (hasTriggerCalled)

{

    // Exit에서 리셋하면 사망 전이 시 DeadState.Enter가 읽기 전에 지워지므로 여기서만 리셋

    isThrowInProgress = false;

    stateMachine.ChangeState(enemy.battleState);

}

```

  

**교훈**: 새 상태의 `Enter()`가 **이전 상태의 정보**를 읽어야 하는 전이에서는 `Exit()` 정리가 함정이 된다. 전이 컨텍스트가 필요한 상태 머신은 정리 시점을 규칙이 아니라 데이터 수명 기준으로 정해야 한다.

  

### 7. 애니메이션 이벤트가 유실되면 보스가 상태에 영구히 갇히는 문제

  

**문제**: 드물게 보스가 화염방사를 끝낸 뒤 아무 행동도 하지 않고 멈춤. 재현 빈도가 낮아 원인을 특정하기 어려웠다.

  

**원인**: 상태 종료 전이가 애니메이션 이벤트(`hasTriggerCalled`)에 의존하는데, 상태 전환 인터럽트나 클립 블렌딩으로 이벤트 프레임이 스킵되면 전이 조건이 영원히 충족되지 않는다. **애니메이션 이벤트는 전달이 보장되는 신호가 아니었다.**

  

**해결**: 두 겹으로 방어했다. ① 이벤트가 유실돼도 `stateTimer < -EXIT_TIMEOUT`(3초 여유)이면 강제로 상태를 탈출. ② 화염 지속시간은 상태 진입이 아니라 **화염이 실제로 점화되는 애니메이션 이벤트 시점**부터 다시 측정해, 준비 모션 길이와 무관하게 일정한 지속시간을 보장.

  

```csharp

// AbilityState_Boss

public override void AbilityTrigger()

{

    // 화염이 실제로 켜지는 시점부터 지속시간 측정 (애니메이션 준비 구간 제외)

    stateTimer = enemy.flamethrowDuration;

    enemy.ActivateFlamethrower(true);

}

  

// Update() — 애님 이벤트 유실 대비 탈출 장치

if (stateTimer < -EXIT_TIMEOUT)

    stateMachine.ChangeState(enemy.moveState);

```

  

같은 계열의 버그로, 공격 애니메이션 도중 사망하면 종료 이벤트를 못 받아 **근접 공격 판정이 켜진 채로 남는 문제**(시체 옆을 지나가면 데미지)를 `DeadState.Enter()`에서 판정을 강제 종료하는 것으로 해결했다.

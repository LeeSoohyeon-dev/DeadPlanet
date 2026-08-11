using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameEvents
{
    public static event Action OnGameVictory;
    public static event Action OnGameOver;
    public static event Action OnGameRestart;

    public static event Action<SoundType> OnPlaySound;
    public static event Action<SoundType> OnStopSound;
    public static event Action OnStopBGM;

    public static event Action<float> OnSlowMotion;
    public static event Action OnPauseTime;
    public static event Action OnResumeTime;

    public static event Action<float, float> OnPlayerHealthChanged;
    public static event Action<List<Weapon>, Weapon> OnWeaponUIUpdate;
    public static event Action<int, int> OnMissionUIUpdate;
    public static event Action<bool> OnLootButtonUpdate;

    public static event Action<Enemy> OnAnyEnemyDied;

    // Domain Reload 비활성화 대비: 플레이 시작 시 static 이벤트 구독 초기화
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticEvents()
    {
        OnGameVictory = null;
        OnGameOver = null;
        OnGameRestart = null;

        OnPlaySound = null;
        OnStopSound = null;
        OnStopBGM = null;

        OnSlowMotion = null;
        OnPauseTime = null;
        OnResumeTime = null;

        OnPlayerHealthChanged = null;
        OnWeaponUIUpdate = null;
        OnMissionUIUpdate = null;
        OnLootButtonUpdate = null;

        OnAnyEnemyDied = null;
    }

    public static void RaiseGameVictory() => OnGameVictory?.Invoke();

    public static void RaiseGameOver() => OnGameOver?.Invoke();

    public static void RaiseGameRestart() => OnGameRestart?.Invoke();

    public static void RaisePlaySound(SoundType soundType) => OnPlaySound?.Invoke(soundType);

    public static void RaiseStopSound(SoundType soundType) => OnStopSound?.Invoke(soundType);

    public static void RaiseStopBGM() => OnStopBGM?.Invoke();

    public static void RaiseSlowMotion(float duration) => OnSlowMotion?.Invoke(duration);

    public static void RaisePauseTime() => OnPauseTime?.Invoke();

    public static void RaiseResumeTime() => OnResumeTime?.Invoke();

    public static void RaisePlayerHealthChanged(float currentHealth, float maxHealth) =>
        OnPlayerHealthChanged?.Invoke(currentHealth, maxHealth);

    public static void RaiseWeaponUIUpdate(List<Weapon> weaponSlots, Weapon currentWeapon) =>
        OnWeaponUIUpdate?.Invoke(weaponSlots, currentWeapon);

    public static void RaiseMissionUIUpdate(int remainingEnemies, int killedEnemies) =>
        OnMissionUIUpdate?.Invoke(remainingEnemies, killedEnemies);

    public static void RaiseLootButtonUpdate(bool isActive) => OnLootButtonUpdate?.Invoke(isActive);

    public static void RaiseAnyEnemyDied(Enemy enemy) => OnAnyEnemyDied?.Invoke(enemy);
}

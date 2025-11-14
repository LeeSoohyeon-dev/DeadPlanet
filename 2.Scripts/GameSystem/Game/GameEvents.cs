using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameEvents
{
    public static Action OnGameVictory;
    public static Action OnGameOver;
    public static Action OnGameRestart;

    public static Action<SoundType> OnPlaySound;
    public static Action<SoundType> OnStopSound;
    public static Action OnStopBGM;

    public static Action<float> OnSlowMotion;
    public static Action OnPauseTime;
    public static Action OnResumeTime;

    public static Action<float, float> OnPlayerHealthChanged;
    public static Action<List<Weapon>, Weapon> OnWeaponUIUpdate;
    public static Action<int, int> OnMissionUIUpdate;
    public static Action<bool> OnLootButtonUpdate;

    public static Action<Enemy> OnAnyEnemyDied;
}

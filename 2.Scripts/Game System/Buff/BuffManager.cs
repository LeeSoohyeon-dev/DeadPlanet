using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class BuffManager : MonoBehaviour
{
	public static BuffManager instance;
	private List<Buff> activeBuffs = new List<Buff>();
	private Player player;

	private int currentShieldAmount;

	[Header("Visual Effects")]
	[SerializeField] private GameObject collectParticlePrefab;
	[SerializeField] private GameObject shieldEffect;
	[SerializeField] private GameObject shieldBreakParticlePrefab;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			// 같은 GameObject의 다른 매니저를 함께 파괴하지 않도록 컴포넌트만 제거
			Destroy(this);
			return;
		}
	}

	private void Start()
	{
		player = GameManager.GetPlayer();
	}

	private void EnsurePlayerReference()
	{
		if (player == null)
			player = GameManager.GetPlayer();
	}

	public void ApplyBuff(BuffData buffData)
	{
		if (buffData == null)
			return;

		switch (buffData.buffType)
		{
			case BuffType.HealthRestore:
				ApplyHealthRestore(buffData);
				break;

			case BuffType.Shield:
				ExpireShieldBuffs();
				ApplyShield(buffData);
				break;

			case BuffType.AmmoRestore:
				ApplyAmmoRestore(buffData);
				break;
		}

		if (buffData.duration > 0 && buffData.buffType == BuffType.Shield)
		{
			Buff buff = new Buff(buffData);
			activeBuffs.Add(buff);
			
			buff.StartDuration(this.GetCancellationTokenOnDestroy(), RemoveShield);
		}

        GameEvents.RaisePlaySound(SoundType.BuffPickup);
	}

	private void ApplyHealthRestore(BuffData buffData)
	{
		EnsurePlayerReference();

		if (player == null || player.health == null)
			return;

		int healAmount = Mathf.RoundToInt(buffData.effectValue);
		player.health.RestoreHealth(healAmount);
	}

	private void ApplyShield(BuffData buffData)
	{
		int shieldAmount = Mathf.RoundToInt(buffData.effectValue);
		currentShieldAmount = shieldAmount;

		if (shieldEffect != null)
		{
			shieldEffect.SetActive(true);
		}
	}

	private void RemoveShield()
	{
		currentShieldAmount = 0;

		if (shieldEffect != null)
		{
			shieldEffect.SetActive(false);
		}

		activeBuffs.RemoveAll(b => !b.isActive);
	}

	private void ApplyAmmoRestore(BuffData buffData)
	{
		EnsurePlayerReference();

		if (player == null || player.weaponController == null)
			return;

		int ammoAmount = Mathf.RoundToInt(buffData.effectValue);

		foreach (Weapon weapon in player.weaponController.GetWeaponSlots())
		{
			weapon.totalReserveAmmo += ammoAmount;
		}

		player.weaponController.UpdateWeaponUI();
	}

	public int ProcessDamageWithShield(int incomingDamage)
	{
		if (currentShieldAmount <= 0)
		{
			if (shieldEffect != null && shieldEffect.activeSelf)
			{
				shieldEffect.SetActive(false);
			}

			return incomingDamage;
		}

		if (currentShieldAmount >= incomingDamage)
		{
			currentShieldAmount -= incomingDamage;

			if (currentShieldAmount <= 0)
			{
				BreakShield();
			}

			return 0;
		}

		int remainingDamage = incomingDamage - currentShieldAmount;
		currentShieldAmount = 0;

		BreakShield();

		return remainingDamage;
	}

	private void BreakShield()
	{
		ExpireShieldBuffs();

		if (shieldEffect != null && shieldEffect.activeSelf)
		{
			shieldEffect.SetActive(false);
		}

		GameEvents.RaisePlaySound(SoundType.ShieldBreak);

		SpawnShieldBreakParticle();
	}

	private void SpawnShieldBreakParticle()
	{
		if (shieldBreakParticlePrefab == null || ObjectPool.instance == null)
			return;

		EnsurePlayerReference();
		if (player == null)
			return;

		GameObject particle = ObjectPool.instance.GetObject(shieldBreakParticlePrefab, player.transform);
		if (particle == null)
			return;

		particle.transform.position = player.transform.position;

		ParticleSystem ps = particle.GetComponent<ParticleSystem>();
		float returnDelay = ps != null ? ps.main.duration + ps.main.startLifetime.constantMax : 2f;
		ObjectPool.instance.ReturnObject(particle, returnDelay);
	}

	private void ExpireShieldBuffs()
	{
		var buffsToExpire = new List<Buff>(activeBuffs);

		foreach (Buff buff in buffsToExpire)
		{
			if (buff.isActive && buff.data.buffType == BuffType.Shield)
			{
				buff.Expire();
			}
		}
		activeBuffs.RemoveAll(b => !b.isActive);
	}

	public int GetCurrentShield() => currentShieldAmount;

	public GameObject GetCollectParticlePrefab() => collectParticlePrefab;

	public void ClearAllBuffs()
	{
		var buffsToClear = new List<Buff>(activeBuffs);

		foreach (Buff buff in buffsToClear)
		{
			buff.Expire();
		}
		activeBuffs.Clear();
	}

	private void OnDestroy()
	{
		if (instance == this)
		{
			instance = null;
		}
	}
}

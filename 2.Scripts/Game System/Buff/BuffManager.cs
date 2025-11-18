using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cysharp.Threading.Tasks;

public class BuffManager : MonoBehaviour
{
	public static BuffManager instance;
	private List<Buff> activeBuffs = new List<Buff>();
	private Player player;

	[Header("Shield Settings")]
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
			Destroy(gameObject);
			return;
		}
	}

	private void Start()
	{
		player = GameManager.instance?.player;
	}

	private void EnsurePlayerReference()
	{
		if (player != null)
			return;

		if (GameManager.instance != null && GameManager.instance.player != null)
		{
			player = GameManager.instance.player;
			return;
		}

		player = FindFirstObjectByType<Player>();
		if (player != null && GameManager.instance != null)
		{
			GameManager.instance.player = player;
		}
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
				ExpireExistingShieldBuff();
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
			
			System.Action onExpire = () => RemoveShield(buffData);
			buff.StartDuration(this.GetCancellationTokenOnDestroy(), onExpire);
		}

		GameEvents.OnPlaySound?.Invoke(SoundType.BuffPickup);
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

	private void RemoveShield(BuffData buffData)
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

		if (currentShieldAmount <= 0 && shieldEffect != null && shieldEffect.activeSelf)
		{
			shieldEffect.SetActive(false);
		}

		if (currentShieldAmount <= 0)
			return incomingDamage;

		bool wasShieldActive = currentShieldAmount > 0;

		if (currentShieldAmount >= incomingDamage)
		{
			currentShieldAmount -= incomingDamage;

			if (currentShieldAmount <= 0)
			{
				ExpireAllShieldBuffs();
				
				if (shieldEffect != null && shieldEffect.activeSelf)
				{
					shieldEffect.SetActive(false);
				}

				GameEvents.OnPlaySound?.Invoke(SoundType.ShieldBreak);

				if (shieldBreakParticlePrefab != null && ObjectPool.instance != null)
				{
					EnsurePlayerReference();
					if (player != null)
					{
						GameObject particle = ObjectPool.instance.GetObject(shieldBreakParticlePrefab, player.transform);
						if (particle != null)
						{
							particle.transform.position = player.transform.position;

							ParticleSystem ps = particle.GetComponent<ParticleSystem>();
							if (ps != null)
							{
								float duration = ps.main.duration + ps.main.startLifetime.constantMax;
								ObjectPool.instance.ReturnObject(particle, duration);
							}
							else
							{
								ObjectPool.instance.ReturnObject(particle, 2f);
							}
						}
					}
				}
			}

			return 0;
		}
		else
		{
			int remainingDamage = incomingDamage - currentShieldAmount;
			currentShieldAmount = 0;

			ExpireAllShieldBuffs();

			if (wasShieldActive)
			{
				if (shieldEffect != null && shieldEffect.activeSelf)
				{
					shieldEffect.SetActive(false);
				}

				GameEvents.OnPlaySound?.Invoke(SoundType.ShieldBreak);

				if (shieldBreakParticlePrefab != null && ObjectPool.instance != null)
				{
					EnsurePlayerReference();
					if (player != null)
					{
						GameObject particle = ObjectPool.instance.GetObject(shieldBreakParticlePrefab, player.transform);
						if (particle != null)
						{
							particle.transform.position = player.transform.position;

							ParticleSystem ps = particle.GetComponent<ParticleSystem>();
							if (ps != null)
							{
								float duration = ps.main.duration + ps.main.startLifetime.constantMax;
								ObjectPool.instance.ReturnObject(particle, duration);
							}
							else
							{
								ObjectPool.instance.ReturnObject(particle, 2f);
							}
						}
					}
				}
			}

			return remainingDamage;
		}
	}

	private void ExpireExistingShieldBuff()
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

	private void ExpireAllShieldBuffs()
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

	public List<Buff> GetActiveBuffs() => activeBuffs.Where(b => b.isActive).ToList();

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
}

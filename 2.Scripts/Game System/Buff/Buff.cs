using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

public class Buff
{
	public BuffData data { get; private set; }
	public float remainingTime { get; private set; }
	public bool isActive { get; private set; }

	private CancellationTokenSource cts;
	private System.Action onExpire;

	public Buff(BuffData buffData)
	{
		data = buffData;
		remainingTime = buffData.duration;
		isActive = true;
	}

	public void StartDuration(CancellationToken parentToken, System.Action onExpireCallback = null)
	{
		onExpire = onExpireCallback;
		cts = CancellationTokenSource.CreateLinkedTokenSource(parentToken);
		UpdateDurationAsync(cts.Token).Forget();
	}

	private async UniTask UpdateDurationAsync(CancellationToken ct)
	{
		try
		{
			while (isActive)
			{
				if (remainingTime <= 0)
				{
					Expire();
					return;
				}

				await UniTask.WaitForSeconds(0.1f, cancellationToken: ct);
				remainingTime -= 0.1f;
			}
		}
		catch (OperationCanceledException)
		{
			Debug.Log("[Buff] 버프 지속시간 업데이트 취소");
		}
	}

	public void Expire()
	{
		if (!isActive)
			return;

		isActive = false;
		cts?.Cancel();
		cts?.Dispose();
		onExpire?.Invoke();
	}

	public void RefreshDuration()
	{
		remainingTime = data.duration;
	}
}

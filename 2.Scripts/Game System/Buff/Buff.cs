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
			await UniTask.WaitForSeconds(remainingTime, cancellationToken: ct);

			remainingTime = 0f;
			Expire();
		}
		catch (OperationCanceledException)
		{
			cts?.Dispose();
			cts = null;
		}
	}

	public void Expire()
	{
		if (!isActive)
			return;

		isActive = false;
		cts?.Cancel();
		cts?.Dispose();
		cts = null;
		onExpire?.Invoke();
	}
}

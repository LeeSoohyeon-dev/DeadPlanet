using UnityEngine;

[CreateAssetMenu(fileName = "New Buff", menuName = "Buff System/Buff Data")]
public class BuffData : ScriptableObject
{
	[Header("Buff Info")]
	public BuffType buffType;
	public string buffName;
	[TextArea(2, 4)] public string description;

	[Header("Effect Values")]
	public float effectValue;
	public float duration;
}

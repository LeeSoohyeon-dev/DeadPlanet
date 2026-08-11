using UnityEngine;

public class PooledObject : MonoBehaviour
{
    public GameObject originalPrefab;

    // 대여될 때마다 증가하는 세대 번호 — 지연 반환 예약의 유효성 검사에 사용
    public int Generation { get; private set; }

    public void IncrementGeneration() => Generation++;
}

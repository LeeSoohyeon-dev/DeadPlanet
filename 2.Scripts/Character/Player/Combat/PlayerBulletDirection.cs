using UnityEngine;

public class PlayerBulletDirection : MonoBehaviour
{
    private Player player;
    private PlayerSmartAssist smartAssist;

    private void Awake()
    {
        player = GetComponent<Player>();
        smartAssist = GetComponent<PlayerSmartAssist>();
    }

    public Vector3 BulletDirection()
    {
        if (smartAssist != null && smartAssist.HasActiveTarget())
        {
            Vector3 assistedDirection = smartAssist.GetBulletDirection();
            if (assistedDirection != Vector3.zero)
            {
                return assistedDirection;
            }
        }

        Transform gunPoint = player?.weapon?.GunPoint();

        Vector3 baseDirection;
        if (gunPoint != null)
        {
            baseDirection = gunPoint.forward;
        }
        else
        {
            baseDirection = transform.forward;
        }

        baseDirection.y = 0f;
        baseDirection = baseDirection.normalized;

        return baseDirection;
    }
}

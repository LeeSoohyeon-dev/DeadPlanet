using UnityEngine;

public class PlayerBulletDirection : MonoBehaviour
{
    private Player player;

    private void Start()
    {
        player = GetComponent<Player>();
    }


    public Vector3 BulletDirection()
    {
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

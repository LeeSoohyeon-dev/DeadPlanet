using UnityEngine;

public class PlayerSmartAssist : MonoBehaviour
{
    private Player player;
    
    [SerializeField] private bool enableSmartAssist = true;
    [SerializeField] private float assistRange = 12f;
    [SerializeField] private float assistAngle = 60f;
    [SerializeField] private LayerMask enemyLayerMask = -1;
    
    
    [Header("Rotation")]
    [SerializeField] private bool enableRotationAssist = true;
    [SerializeField] private float rotationAssistStrength = 0.5f;
    [SerializeField] private float rotationSpeed = 3f;
    [SerializeField] private bool useGunPointForRotation = true;
    //회전 보정 오프셋
    [SerializeField] private float yawOffsetDegrees = 0f; 
    
    
    
    private Transform currentTarget;
    
    private void Start()
    {

        player = GetComponent<Player>();
        
    }
    
    private void Update()
    {
        if (!enableSmartAssist || player.health.isDead)
            return;
            
        UpdateCurrentTarget();
        UpdatePlayerRotation();
    }
    
    private void UpdateCurrentTarget()
    {
        Transform newTarget = FindBestTarget();
        
        if (newTarget != currentTarget)
        {
            currentTarget = newTarget;
        }
    }
    
    private void UpdatePlayerRotation()
    {
        if (!enableRotationAssist || currentTarget == null)
            return;
            
        Vector3 targetDirection = GetDirectionToTarget();
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        
        transform.rotation = Quaternion.Slerp(
            transform.rotation, 
            targetRotation, 
            rotationAssistStrength * rotationSpeed * Time.deltaTime
        );
    }
    
    private Transform FindBestTarget()
    {
        Vector3 playerPos = transform.position;
        Vector3 playerForward = transform.forward;
        playerForward.y = 0f;
        
        Collider[] enemiesInRange = Physics.OverlapSphere(playerPos, assistRange, enemyLayerMask);
        
        Transform bestTarget = null;
        float bestScore = 0f;

        
        foreach (Collider enemy in enemiesInRange)
        {
            if (enemy.GetComponent<Enemy>() == null && enemy.GetComponentInParent<Enemy>() == null)
                continue;
                
            Vector3 enemyPos = enemy.bounds.center;
            enemyPos.y = playerPos.y;
            
            Vector3 directionToEnemy = (enemyPos - playerPos).normalized;
            float distance = Vector3.Distance(playerPos, enemyPos);
            float angle = Vector3.Angle(playerForward, directionToEnemy);
            
            if (angle > assistAngle / 2f)
                continue;
                
            float distanceScore = 1f - (distance / assistRange);
            float angleScore = 1f - (angle / (assistAngle / 2f));
            float totalScore = (distanceScore * 0.6f) + (angleScore * 0.4f);
            
            if (totalScore > bestScore)
            {
                bestScore = totalScore;
                bestTarget = enemy.transform;
            }
        }
        
        return bestTarget;
    }
    
    
    
    private Vector3 GetDirectionToTarget()
    {
        if (currentTarget == null)
            return transform.forward;
            
        Vector3 playerPos = transform.position;
        Vector3 originPos = playerPos;
        Transform gunPoint = null;
        if (useGunPointForRotation && player != null && player.weapon != null)
        {
            gunPoint = player.weapon.GunPoint();
        }
        if (gunPoint != null)
        {
            originPos = gunPoint.position;
        }
        
        Vector3 targetPos = currentTarget.GetComponent<Collider>()?.bounds.center ?? currentTarget.position;
        
        targetPos.y = playerPos.y;
        originPos.y = playerPos.y;
        
        Vector3 direction = (targetPos - originPos).normalized;
        
        if (Mathf.Abs(yawOffsetDegrees) > 0.001f)
        {
            direction = Quaternion.Euler(0f, yawOffsetDegrees, 0f) * direction;
        }
        
        return direction;
    }
    
    
     
    
    
    public void SetSmartAssistEnabled(bool enabled)
    {
        enableSmartAssist = enabled;
    }
    
    
    
    public void SetRotationAssistEnabled(bool enabled)
    {
        enableRotationAssist = enabled;
    }
    
    public void SetRotationAssistStrength(float strength)
    {
        rotationAssistStrength = Mathf.Clamp01(strength);
    }
    
    private void OnDrawGizmosSelected()
    {
        if (!enableSmartAssist)
            return;
            
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, assistRange);
        
        Vector3 forward = transform.forward;
        Vector3 leftBoundary = Quaternion.Euler(0, -assistAngle / 2f, 0) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, assistAngle / 2f, 0) * forward;
        
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * assistRange);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * assistRange);
        
        if (currentTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, currentTarget.position);
            Gizmos.DrawWireSphere(currentTarget.position, 1f);
        }
    }
}

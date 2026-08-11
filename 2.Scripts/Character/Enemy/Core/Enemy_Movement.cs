using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Enemy_Movement : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 1.5f;
    public float runSpeed = 3f;
    public float turnSpeed;

    private bool isManualMovement;
    private bool isManualRotation;

    [SerializeField] private Transform[] patrolPoints;
    private Vector3[] patrolPointsPosition;
    private int currentPatrolIndex;

    private Enemy enemy;
    private NavMeshAgent agent;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        agent = GetComponent<NavMeshAgent>();
    }

    public void FaceTarget(Vector3 target, float turnSpeed = 0)
    {
        Vector3 direction = target - transform.position;

        // 목표가 자기 위치와 같으면 LookRotation이 zero-vector 경고를 뱉음
        if (direction.sqrMagnitude < 0.0001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        Vector3 currentEulerAngles = transform.rotation.eulerAngles;

        if (turnSpeed == 0)
        {
            turnSpeed = this.turnSpeed;
        }

        float yRotation = Mathf.LerpAngle(currentEulerAngles.y, targetRotation.eulerAngles.y, turnSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(currentEulerAngles.x, yRotation, currentEulerAngles.z);
    }

    public void ActivateManualMovement(bool manualMovement) => this.isManualMovement = manualMovement;
    public bool IsManualMovementActive() => isManualMovement;

    public void ActivateManualRotation(bool manualRotation) => this.isManualRotation = manualRotation;
    public bool IsManualRotationActive() => isManualRotation;

    public Vector3 GetPatrolDestination()
    {
        // 순찰 지점이 없는(런타임 스폰) 적은 제자리 대기
        if (patrolPointsPosition == null || patrolPointsPosition.Length == 0)
            return transform.position;

        Vector3 destination = patrolPointsPosition[currentPatrolIndex];

        currentPatrolIndex++;

        if (currentPatrolIndex >= patrolPointsPosition.Length)
        {
            currentPatrolIndex = 0;
        }

        return destination;
    }

    public void InitializePatrolPoints()
    {
        patrolPointsPosition = new Vector3[patrolPoints.Length];

        for (int i = 0; i < patrolPoints.Length; i++)
        {
            patrolPointsPosition[i] = patrolPoints[i].position;
            patrolPoints[i].gameObject.SetActive(false);
        }
    }

    public virtual void BulletImpact(Vector3 force, Vector3 hitPoint, Rigidbody rb)
    {
        if (enemy.health.IsDead)
            StartCoroutine(DeathImpactCourutine(force, hitPoint, rb));
    }

    private IEnumerator DeathImpactCourutine(Vector3 force, Vector3 hitPoint, Rigidbody rb)
    {
        yield return new WaitForSeconds(.1f);

        rb.AddForceAtPosition(force, hitPoint, ForceMode.Impulse);
    }

    public float GetWalkSpeed() => walkSpeed;
    public float GetRunSpeed() => runSpeed;
}

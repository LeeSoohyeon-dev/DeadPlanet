using System.Collections.Generic;
using UnityEngine;

public enum GrenadePerk { Unavalible, CanThrowGrenade}
public class Enemy_Range : Enemy
{
    public GrenadePerk grenadePerk;

    [Header("Grenade")]
    public GameObject grenadePrefab;
    public int grenadeDamage;
    public float impactPower;
    public float explosionTimer = .75f;
    public float timeToTarget = 1.2f;
    public float grenadeExtraRange = 2f;
    public float grenadeCooldown;
    public float safeDistance;
    private float lastTimeGrenadeThrown = -10;
    [SerializeField] private Transform grenadeStartPoint;

    [Header("Advance")]
    public float advanceSpeed;
    public float advanceStoppingDistance;

    [Header("Weapon")]
    public float attackDelay;
    public Enemy_RangeWeaponType weaponType;
    public Enemy_RangeWeaponData weaponData;

    public Transform gunPoint;
    public Transform weaponHolder;
    public GameObject bulletPrefab;

    [Header("Aim")]
    public float slowAim = 4;
    public float fastAim = 20;
    public Transform aim;
    public Transform playersBody;
    public LayerMask whatToIgnore;

    [SerializeField] List<Enemy_RangeWeaponData> avalibleWeaponData;

    public IdleState_Range idleState { get; private set; }
    public MoveState_Range moveState { get; private set; }
    public BattleState_Range battleState { get; private set; }
    public AdvancePlayerState_Range advancePlayerState { get; private set; }
    public ThrowGrenadeState_Range throwGrenadeState { get; private set; }
    public DeadState_Range deadState { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        idleState = new IdleState_Range(this, stateMachine, "isIdle");
        moveState = new MoveState_Range(this, stateMachine, "isMoving");
        battleState = new BattleState_Range(this, stateMachine, "isInBattle");
        advancePlayerState = new AdvancePlayerState_Range(this, stateMachine, "isAdvancing");
        throwGrenadeState = new ThrowGrenadeState_Range(this, stateMachine, "isThrowingGrenade");
        deadState = new DeadState_Range(this, stateMachine, "isIdle");
    }

    protected override void Start()
    {
        base.Start();

        playersBody = player.GetComponent<Player>().playerBody;
        aim.parent = null;

        stateMachine.Initialize(idleState);
        visuals.SetupLook();
        SetupWeapon();
    }

    protected override void Update()
    {
        base.Update();
        if(player.health.isDead)
        {
            stateMachine.ChangeState(idleState);
            return;
        }
        stateMachine.currentState.Update();
    }
    public override void Die()
    {
        base.Die();

        if (stateMachine.currentState != deadState)
            stateMachine.ChangeState(deadState);
    }
    public bool CanThrowGrenade()
    {
        if (grenadePerk == GrenadePerk.Unavalible)
            return false;

        if(Vector3.Distance(player.transform.position, transform.position) < safeDistance)
            return false;

        if (Time.time > grenadeCooldown + lastTimeGrenadeThrown)
            return true;

        return false;
    }

public void ThrowGrenade()
{
    lastTimeGrenadeThrown = Time.time;
    visuals.EnableGrenadeModel(false);

    if (grenadeStartPoint == null)
    {
        Debug.LogError("grenadeStartPoint가 null입니다!");
        return;
    }

    GameObject newGrenade = ObjectPool.instance.GetObject(grenadePrefab, grenadeStartPoint);
    Enemy_Grenade newGrenadeScript = newGrenade.GetComponent<Enemy_Grenade>();

    if (stateMachine.currentState == deadState)
    {
        newGrenadeScript.SetupGrenade(whatIsAlly, transform.position, 1, explosionTimer, impactPower, grenadeDamage);
        return;
    }

    Vector3 launchPosition = grenadeStartPoint.position;
    Vector3 targetPosition = player.transform.position;
    Vector3 directionToPlayer = targetPosition - launchPosition;
    float horizontalDistance = new Vector3(directionToPlayer.x, 0, directionToPlayer.z).magnitude;

    float estimatedTimeToTarget = horizontalDistance * 0.25f;

    newGrenadeScript.SetupGrenade(whatIsAlly, targetPosition, estimatedTimeToTarget, explosionTimer, impactPower, grenadeDamage, launchPosition);
}
    public override void EnterBattleMode()
    {
        if (ai.isInBattleMode)
            return;

        base.EnterBattleMode();

        stateMachine.ChangeState(battleState);
    }

    public void FireSingleBullet()
    {
        anim.SetTrigger("Shoot");
        GameEvents.OnPlaySound?.Invoke(SoundType.EnemyShoot);

        Vector3 bulletsDirection = (aim.position - gunPoint.position).normalized;

        GameObject newBullet = ObjectPool.instance.GetObject(bulletPrefab,gunPoint);
        newBullet.transform.rotation = Quaternion.LookRotation(gunPoint.forward);

        newBullet.GetComponent<Bullet>().BulletSetup(whatIsAlly, weaponData.bulletDamage);

        Rigidbody rbNewBullet = newBullet.GetComponent<Rigidbody>();

        Vector3 bulletDirectionWithSpread = weaponData.ApplyWeaponSpread(bulletsDirection);

        rbNewBullet.mass = 20 / weaponData.bulletSpeed;
        rbNewBullet.linearVelocity = bulletDirectionWithSpread * weaponData.bulletSpeed;

    }
    private void SetupWeapon()
    {
        List<Enemy_RangeWeaponData> filteredData = new List<Enemy_RangeWeaponData>();

        foreach (var weaponData in avalibleWeaponData)
        {
            if (weaponData.weaponType == weaponType)
                filteredData.Add(weaponData);
        }

        if (filteredData.Count > 0)
        {
            int random = Random.Range(0, filteredData.Count);
            weaponData = filteredData[random];
        }

        gunPoint = visuals.currentWeaponModel.GetComponent<Enemy_RangeWeaponModel>().gunPoint;
    }

    public void UpdateAimPosition()
    {
        float aimSpeed = IsAimOnPlayer() ? fastAim : slowAim;
        aim.position = Vector3.MoveTowards(aim.position, playersBody.position, aimSpeed * Time.deltaTime);
    }

    public bool IsAimOnPlayer()
    {
        float distanceAimToPlayer = Vector3.Distance(aim.position, player.transform.position);

        return distanceAimToPlayer < 2;
    }

    public bool IsSeeingPlayer()
    {
        Vector3 myPosition = transform.position + Vector3.up;
        Vector3 directionToPlayer = playersBody.position - myPosition;

        if (Physics.Raycast(myPosition, directionToPlayer, out RaycastHit hit, Mathf.Infinity, ~whatToIgnore))
        {
            if (hit.transform.root == player.transform.root)
            {
                UpdateAimPosition();
                return true;
            }
        }

        return false;
    }
}

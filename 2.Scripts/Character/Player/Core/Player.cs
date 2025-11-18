using UnityEngine;

public class Player : MonoBehaviour
{
    public Transform playerBody;
    public PlayerIA inputAction { get; private set; }
    public PlayerBulletDirection aim { get; private set; }
    public PlayerMovement movement { get; private set; }
    public PlayerWeaponController weapon { get; private set; }
    public PlayerWeaponController weaponController { get; private set; }
    public PlayerWeaponVisuals weaponVisuals { get; private set; }
    public PlayerInteraction interaction { get; private set; }
    public Player_Health health { get; private set; }

    public Animator anim { get; private set; }

    private void Awake()
    {
        inputAction = new PlayerIA();

        anim = GetComponentInChildren<Animator>();
        health = GetComponent<Player_Health>();
        aim = GetComponent<PlayerBulletDirection>();
        movement = GetComponent<PlayerMovement>();
        weapon = GetComponent<PlayerWeaponController>();
        weaponController = GetComponent<PlayerWeaponController>();
        weaponVisuals = GetComponent<PlayerWeaponVisuals>();
    }
    private void OnEnable()
    {
        inputAction.Enable();
    }
    private void OnDisable()
    {
        inputAction.Disable();
    }

}

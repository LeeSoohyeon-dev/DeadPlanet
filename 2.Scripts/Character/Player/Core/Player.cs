using UnityEngine;

public class Player : MonoBehaviour
{
    public Transform playerBody;
    public PlayerIA inputAction { get; private set; }
    public PlayerBulletDirection aim { get; private set; }
    public PlayerMovement movement { get; private set; }
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
        weaponController = GetComponent<PlayerWeaponController>();
        weaponVisuals = GetComponent<PlayerWeaponVisuals>();

        if (GameManager.instance != null)
        {
            GameManager.instance.player = this;
        }
    }
    private void OnEnable()
    {
        inputAction.Enable();
    }
    private void OnDisable()
    {
        inputAction.Disable();
    }

    private void OnDestroy()
    {
        inputAction.Dispose();
    }

}

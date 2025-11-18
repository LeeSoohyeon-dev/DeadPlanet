using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Joystick joystick;
    private float verticalVelocity;

    private Player player;
    private PlayerIA playerIA;
    private CharacterController characterController;
    private Animator animator;

    [Header("Movement")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float turnSpeed;

    private float speed;
    private Vector3 moveDirection;
    private Vector2 keyboardInput;
    public Vector2 moveInput { get; private set; }
    private bool isRunning;

    private void Start()
    {
        player = GetComponent<Player>();
        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        joystick = UI.instance.GetComponentInChildren<FloatingJoystick>();
        SetupInput();
        speed = walkSpeed;
    }

    private void Update()
    {
        if (player.health.isDead)
            return;

        Vector2 joystickInput = Vector2.zero;
        if (joystick != null)
        {
            joystickInput = joystick.Direction;
        }

        Vector2 rawInput;
        if (joystickInput.magnitude > 0.1f)
        {
            rawInput = joystickInput;
        }
        else
        {
            rawInput = keyboardInput;
        }

        moveInput = rawInput;

        ApplyMovement();
        ApplyRotation();
        AnimatorControllers();
    }

    private void SetupInput()
    {
        playerIA = GetComponent<Player>().inputAction;

        playerIA.Character.Movement.performed += context => keyboardInput = context.ReadValue<Vector2>();
        playerIA.Character.Movement.canceled += context => keyboardInput = Vector2.zero;

        playerIA.Character.Run.performed += context =>
        {
            isRunning = true;
            speed = runSpeed;
        };
        playerIA.Character.Run.canceled += context =>
        {
            isRunning = false;
            speed = walkSpeed;
        };
    }

    private void AnimatorControllers()

    {
        float xVelocity = Vector3.Dot(moveDirection.normalized, transform.right);
        float zVelocity = Vector3.Dot(moveDirection.normalized, transform.forward);
        bool playRunAnimation = isRunning && moveInput.magnitude > 0.1f;

        animator.SetFloat("xVelocity", xVelocity, 0.1f, Time.deltaTime);
        animator.SetFloat("zVelocity", zVelocity, 0.1f, Time.deltaTime);
        animator.SetBool("isRunning", playRunAnimation);
    }
    private void ApplyMovement()
    {
        moveDirection = new Vector3(moveInput.x, 0, moveInput.y);
        ApplyGravity();

        if (moveDirection.magnitude > 0.1f)
        {
            characterController.Move(moveDirection * Time.deltaTime * speed);
        }
    }

    private void ApplyRotation()
    {
        if (moveInput.magnitude > 0.1f)
        {
            Vector3 lookDirection = new Vector3(moveInput.x, 0, moveInput.y);
            lookDirection.Normalize();
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
        }
    }

    private void ApplyGravity()
    {
        if (!characterController.isGrounded)
        {
            verticalVelocity -= 9.81f * Time.deltaTime;
            moveDirection.y = verticalVelocity;
        }
        else
        {
            verticalVelocity = -0.5f;
        }

    }
}
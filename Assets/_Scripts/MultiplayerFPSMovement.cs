using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

public class MultiplayerFPSMovement : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float sprintMultiplier = 2.0f;
    Vector3 currentMovement = Vector3.zero;
    Vector3 fallingMovement = Vector3.zero;
    Vector3 velocity = Vector3.zero;

    [Header("Jump Parameters")]
    [SerializeField] private float jumpForce = 5.0f;
    [SerializeField] private float gravity = 9.81f;
    bool canJump;
    [SerializeField] LayerMask GroundMask = new LayerMask();
    [SerializeField] float CheckRadius = 0.4f;
    [SerializeField] GameObject Feet;

    [Header("Input Map")]
    [SerializeField] private InputActionAsset PlayerInputs;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private Vector2 moveInput;

    [Header("Animations")]
    [SerializeField] Animator animator;
    float LegsY;


    CharacterController controller = null;
    bool isMoving;

    void Awake()
    {
        InputSystem.AddDevice<Keyboard>();
        moveAction = PlayerInputs.FindActionMap("OnGround").FindAction("Move");
        jumpAction = PlayerInputs.FindActionMap("OnGround").FindAction("Jump");
        sprintAction = PlayerInputs.FindActionMap("OnGround").FindAction("Sprint");

        moveAction.performed += context => moveInput = context.ReadValue<Vector2>();
        moveAction.canceled += context => moveInput = Vector2.zero;
        canJump = false;
        controller = GetComponent<CharacterController>();
        
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
        
    }

    private void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
        sprintAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
        sprintAction.Disable();
    }

    private void Update()
    {
        if (!isLocalPlayer) return;
        HandleMovement();
    }

    void HandleMovement()
    {
        float speedMultipier = sprintAction.ReadValue<float>() > 0 ? sprintMultiplier : 1f;
        
        float inputY = moveInput.y > 0 ? moveInput.y * speedMultipier : moveInput.y;
        
        if (moveInput.y != 0)
        {
            if (sprintAction.ReadValue<float>() != 0)
            {
                LegsY = 1f;
            }
            else
            {
                LegsY = 0.5f;
            }
        }
        else { LegsY = 0f; }
        animator.SetFloat("LegsY", LegsY);

        float verticalSpeed = inputY * walkSpeed;
        float horizontalSpeed = moveInput.x * walkSpeed;
        animator.SetFloat("LegsX", moveInput.x);

        Vector3 horizontalMovement = new Vector3(horizontalSpeed, 0, verticalSpeed);

        currentMovement = transform.rotation * horizontalMovement;
        controller.Move(currentMovement * Time.deltaTime);

        HandleGravityAndJumping();

        isMoving = moveInput.y != 0 || moveInput.x != 0;
    }

    void HandleGravityAndJumping()
    {
        bool grounded = Physics.CheckSphere(Feet.transform.position, CheckRadius, GroundMask);

        if (grounded && fallingMovement.y <= 0)
        {
            fallingMovement.y = -2;
        }

        fallingMovement.y -= gravity * Time.deltaTime;

        if (grounded && jumpAction.triggered)
        {
            fallingMovement.y = Mathf.Sqrt(jumpForce * -2 * -gravity);
        }

        controller.Move(fallingMovement * Time.deltaTime);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(Feet.transform.position, CheckRadius);
    }
}

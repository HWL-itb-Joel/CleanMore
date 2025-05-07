using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

public class MultiplayerFPSLook : NetworkBehaviour
{
    [Header("Look Sensitivity")]
    [SerializeField] private float mouseSensitivity = 2.0f;
    [SerializeField] private float upRange = 80.0f;
    [SerializeField] private float downRange = 50.0f;
    private float verticalRotation;

    [Header("Input Map")]
    [SerializeField] private InputActionAsset PlayerInputs;
    private InputAction lookAction;
    private Vector2 lookInput;

    [SerializeField] GameObject player;
    [SerializeField] Animator animator;

    [Header("Camera References")]
    [SerializeField] private Transform cameraTransform; // Tu c?mara
    [SerializeField] private Transform thirdPersonPivot; // Un objeto detr?s o encima del jugador

    public bool firstPersonEnabled = true;
    private float thirdPersonDistance = 5f;

    private void Awake()
    {
        if (!isLocalPlayer) { return; }
        lookAction = PlayerInputs.FindActionMap("OnGround").FindAction("Look");
        lookAction.performed += context => lookInput = context.ReadValue<Vector2>();
        lookAction.canceled += context => lookInput = Vector2.zero;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnEnable()
    {
        lookAction.Enable();
    }

    private void OnDisable()
    {
        lookAction.Disable();
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        if (Keyboard.current.vKey.wasPressedThisFrame) // Usa tecla V para cambiar c?mara
        {
            firstPersonEnabled = !firstPersonEnabled;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        if (firstPersonEnabled)
        {
            HandleFirstPersonRotation();
        }
        else
        {
            HandleThirdPersonRotation();
        }
    }

    void HandleFirstPersonRotation()
    {
        float mouseXRotation = lookInput.x * mouseSensitivity;
        verticalRotation -= lookInput.y * mouseSensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, -upRange, downRange);

        float verticalPercent = Normalize(verticalRotation, downRange, -upRange);
        animator.SetFloat("HeightCamera", verticalPercent);
        transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
        player.transform.Rotate(0, mouseXRotation, 0);

        cameraTransform.localPosition = Vector3.zero; // Asegura que est? en la cabeza
        cameraTransform.localRotation = Quaternion.identity;
    }

    void HandleThirdPersonRotation()
    {
        float mouseXRotation = lookInput.x * mouseSensitivity;
        float mouseYRotation = lookInput.y * mouseSensitivity;

        // Orbitar alrededor del jugador
        verticalRotation -= mouseYRotation;
        verticalRotation = Mathf.Clamp(verticalRotation, -upRange, downRange);

        thirdPersonPivot.Rotate(0, mouseXRotation, 0);
        cameraTransform.position = thirdPersonPivot.position - thirdPersonPivot.forward * thirdPersonDistance + Vector3.up * 2;
        cameraTransform.LookAt(thirdPersonPivot.position + Vector3.up * 1.5f);
    }

    float Normalize(float value, float min, float max)
    {
        return (value - min) / (max - min);
    }
}

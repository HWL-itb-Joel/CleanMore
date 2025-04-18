using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;


public class MultiplayerFPSLook : NetworkBehaviour
{
    [Header("Look Sensitivity")]
    [SerializeField] private float mouseSensitivity = 2.0f;
    [SerializeField] private float upDownRange = 80.0f;
    private float verticalRotation;

    [Header("Input Map")]
    [SerializeField] private InputActionAsset PlayerInputs;
    private InputAction lookAction;
    private Vector2 lookInput;

    [SerializeField] GameObject player;

    private void Awake()
    {
        lookAction = PlayerInputs.FindActionMap("OnGround").FindAction("Look");
        lookAction.performed += context => lookInput = context.ReadValue<Vector2>();
        lookAction.canceled += context => lookInput = Vector2.zero;
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
        HandleRotation();
    }

    void HandleRotation()
    {
        float mouseXRotation = lookInput.x * mouseSensitivity;

        verticalRotation -= lookInput.y * mouseSensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, -upDownRange, upDownRange);

        transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
        player.transform.Rotate(0, mouseXRotation, 0);
    }
}

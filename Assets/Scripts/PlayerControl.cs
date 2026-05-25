using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Movement speed in units per second.")]
    public float moveSpeed = 8f;

    [Tooltip("How fast the player rotates to face movement direction.")]
    public float rotationSpeed = 10f;

    [Header("Jumping")]
    [Tooltip("How high the player jumps.")]
    public float jumpHeight = 2f;

    [Header("Gravity")]
    [Tooltip("Gravity force applied to the player.")]
    public float gravity = -20f;

    [Header("Camera Reference")]
    [Tooltip("Assign the Main Camera here — movement will be relative to it.")]
    public Transform cameraTransform;

    // Internal
    private CombatController combatController;
    private CharacterController controller;
    private Vector3 velocity;
    private Vector2 moveInput;
    private bool jumpPressed;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        combatController = GetComponent<CombatController>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Called automatically by the Input System when move input changes
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    // Called automatically by the Input System when jump is pressed
    public void OnJump(InputValue value)
    {
        if (value.isPressed)
            jumpPressed = true;
    }

    void Update()
    {
        HandleMovement();
        if (Keyboard.current.fKey.isPressed)
            combatController?.TryAttack();
    }

    void HandleMovement()
    {
        bool isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        // Build movement direction relative to camera
        Vector3 move = Vector3.zero;
        if (cameraTransform != null)
        {
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;

            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            move = (camForward * moveInput.y + camRight * moveInput.x);
        }
        else
        {
            move = new Vector3(moveInput.x, 0f, moveInput.y);
        }

        if (move.magnitude > 1f)
            move.Normalize();

        // Rotate to face movement direction
        if (move.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        controller.Move(move * moveSpeed * Time.deltaTime);

        // Jump
        if (jumpPressed && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpPressed = false;
        }
        else
        {
            jumpPressed = false;
        }

        // Gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

    }
}
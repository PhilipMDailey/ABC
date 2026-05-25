using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The player transform the camera will follow.")]
    public Transform target;

    [Header("Distance")]
    [Tooltip("How far the camera sits behind the player.")]
    public float distance = 5f;

    [Tooltip("Minimum zoom distance.")]
    public float minDistance = 2f;

    [Tooltip("Maximum zoom distance.")]
    public float maxDistance = 10f;

    [Tooltip("How fast scrolling zooms the camera.")]
    public float zoomSpeed = 2f;

    [Header("Height")]
    [Tooltip("How high above the target the camera looks from.")]
    public float heightOffset = 1.5f;

    [Header("Rotation")]
    [Tooltip("Mouse sensitivity for horizontal rotation.")]
    public float sensitivityX = 0.2f;

    [Tooltip("Mouse sensitivity for vertical rotation.")]
    public float sensitivityY = 0.2f;

    [Tooltip("Minimum vertical angle (looking down).")]
    public float minVerticalAngle = -20f;

    [Tooltip("Maximum vertical angle (looking up).")]
    public float maxVerticalAngle = 60f;

    [Header("Smoothing")]
    [Tooltip("How smoothly the camera follows the target.")]
    public float followSmoothing = 10f;

    // Internal
    private float yaw;
    private float pitch;

    void Start()
    {
        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null) return;

        HandleInput();
        UpdateCamera();
    }

    void HandleInput()
    {
        // Mouse delta from new Input System
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        yaw += mouseDelta.x * sensitivityX;
        pitch -= mouseDelta.y * sensitivityY;
        pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);

        // Scroll to zoom
        float scroll = Mouse.current.scroll.ReadValue().y;
        distance -= scroll * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);
    }

    void UpdateCamera()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 targetPosition = target.position + Vector3.up * heightOffset;
        Vector3 desiredPosition = targetPosition - rotation * Vector3.forward * distance;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSmoothing * Time.deltaTime);
        transform.LookAt(targetPosition);
    }
}
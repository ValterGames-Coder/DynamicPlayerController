using System;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(PlayerMovement), 
    typeof(PlayerCamera))]
public class PlayerCharacter : MonoBehaviour
{
    #region  --- Variables ---
    [SerializeField] private PlayerMovementConfig playerMovementConfig;
    [SerializeField] private PlayerCameraConfig playerCameraConfig;
    private PlayerMovement _playerMovement;
    private PlayerCamera _playerCamera;

    #endregion

    #region --- Unity Methods ---
    private void Start()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        _playerCamera = GetComponent<PlayerCamera>();
        _playerMovement.Init(playerMovementConfig);
        _playerCamera.Init(playerCameraConfig);
    }

    private void FixedUpdate()
    {
        _playerMovement.UpdateGroundCollision();
        _playerMovement.UpdateMovement();
        _playerMovement.UpdateForce();
    }

    private void Update()
    {
        _playerCamera.UpdatePosition();
    }

    private void LateUpdate()
    {
        _playerCamera.UpdateSlant();
        _playerCamera.UpdateRotate();
    }
    
    #endregion
}

[Serializable]
public struct PlayerCameraConfig
{
    public PlayerInput input;
    public Transform cameraHolder;
    public float sensitivity; // default 0.1f
    [Header("Slant")]
    public float slantForce;
    public float slantSmooth;
    [Header("Sprint")]
    public float sprintZoom;
    public float sprintAngle;
    public float sprintSmooth;
    public AnimationCurve sprintCurve;
    [Header("Slide")] 
    public float slideYPosition;
    public float slideSmooth;
}

[Serializable]
public struct PlayerMovementConfig
{
    public PlayerInput input;
    public Transform playerMesh;
    [Header("Ground")]
    public Transform groundCheck;
    [Range(0f, 1f)] public float radiusCheck;
    public LayerMask groundMask;
    public float groundDrag;
    [Header("Movement")]
    public float speed;
    [Range(0f, 1f)] public float airMultiplier;
    public float jumpForce;
    [Header("Sprint")]
    public AnimationCurve sprintCurve;
    public float sprintForce;
    public float sprintSmooth;
    [Header("Slope")]
    [Range(0f, 90f)] public float maxSlopeAngle;
    [Header("Slide")] 
    public float slideYScale;
    public float slideSpeed;
}
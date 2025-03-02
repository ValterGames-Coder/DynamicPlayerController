using System;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    #region --- Variables ---
    private GameInput _gameInput;
    // Input Variables
    public Vector2 MovementVector { get; private set; }
    public Vector2 LookVector { get; private set; }
    public event Action OnSprint;
    public event Action OnJump;
    public event Action OnSlidePressed;
    public event Action OnSlideEnd;
    
    #endregion

    #region --- Unity Methods ---
    private void Awake()
    {
        _gameInput = new GameInput();
        _gameInput.Player.Enable();
        // Movement vector
        _gameInput.Player.Movement.performed += ctx => 
            MovementVector = ctx.ReadValue<Vector2>();
        _gameInput.Player.Movement.canceled += _ => MovementVector = Vector2.zero;
        // Look vector
        _gameInput.Player.Look.performed += ctx => 
            LookVector = ctx.ReadValue<Vector2>();
        _gameInput.Player.Look.canceled += _ => LookVector = Vector2.zero;
        // Sprint input
        _gameInput.Player.Sprint.performed += _ => OnSprint?.Invoke();
        // Jump input
        _gameInput.Player.Jump.performed += _ => OnJump?.Invoke();
        // Slide input
        _gameInput.Player.Slide.performed += _ => OnSlidePressed?.Invoke();
        _gameInput.Player.Slide.canceled += _ => OnSlideEnd?.Invoke();
    }
    
    #endregion
}
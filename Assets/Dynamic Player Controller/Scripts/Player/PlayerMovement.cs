using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerInput), typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    #region --- Variables ---
    private PlayerMovementConfig _playerConfig;
    private Rigidbody _rigidbody;
    private Vector3 _moveDirection;
    private bool _isGrounded;
    private Coroutine _sprintCoroutine;
    private RaycastHit _slopeHit;
    // Slide
    private float _startYScale;
    private bool _isSliding;
    
    #endregion

    #region --- Logic ---
    public void Init(PlayerMovementConfig playerConfig)
    {
        _rigidbody = GetComponent<Rigidbody>();
        _playerConfig = playerConfig;
        _startYScale = _playerConfig.playerMesh.localScale.y;
        _playerConfig.input.OnSprint += Sprint;
        _playerConfig.input.OnJump += Jump;
        _playerConfig.input.OnSlidePressed += StartSlide;
        _playerConfig.input.OnSlideEnd += EndSlide;
    }

    public void UpdateMovement()
    {
        // Move direction
        _moveDirection = transform.forward * _playerConfig.input.MovementVector.y 
                         + transform.right * _playerConfig.input.MovementVector.x;
        // Drag
        if (_isGrounded)
            _rigidbody.drag = _playerConfig.groundDrag;
        else
            _rigidbody.drag = 0;
        // Slope
        if (OnSlope())
            _rigidbody.AddForce(GetSlopeDirection() * (_playerConfig.speed * 20f), ForceMode.Force);
        // Slide
        else if (_isSliding)
            _rigidbody.AddForce(_moveDirection * (_playerConfig.slideSpeed * 10f), ForceMode.Force);
        // On ground
        else if(_isGrounded)
            _rigidbody.AddForce(_moveDirection * (_playerConfig.speed * 10f), ForceMode.Force); 
        // In air
        else if(!_isGrounded)
            _rigidbody.AddForce(_moveDirection * (_playerConfig.speed * 10f * _playerConfig.airMultiplier), ForceMode.Force);
        // Turn gravity off while on slope
        _rigidbody.useGravity = !OnSlope();
    }

    public void UpdateForce()
    {
        if (OnSlope())
        {
            if (_rigidbody.velocity.magnitude > _playerConfig.speed)
                _rigidbody.velocity = _rigidbody.velocity.normalized * _playerConfig.speed;
        }
        // Limiting speed on ground or in air
        else
        {
            Vector3 flatVel = new Vector3(_rigidbody.velocity.x, 0f, _rigidbody.velocity.z);
            // Limit velocity if needed
            if (flatVel.magnitude > _playerConfig.speed)
            {
                Vector3 limitedVel = flatVel.normalized *  _playerConfig.speed;
                _rigidbody.velocity = new Vector3(limitedVel.x, _rigidbody.velocity.y, limitedVel.z);
            }
        }
    }
    
    public void UpdateGroundCollision() => _isGrounded =
        Physics.CheckSphere(_playerConfig.groundCheck.position, _playerConfig.radiusCheck, _playerConfig.groundMask);

    #endregion
    
    #region --- Jump ---
    private void Jump()
    {
        if (!_isGrounded)
            return;
        Vector3 jumpVector = Vector3.up * _playerConfig.jumpForce;
        _rigidbody.AddForce(jumpVector, ForceMode.Impulse);
    }
    
    #endregion
    
    #region --- Sprint ---
    private void Sprint()
    {
        if (_sprintCoroutine != null)
            StopCoroutine(_sprintCoroutine);
        _sprintCoroutine = StartCoroutine(SmoothSprint());
    }
    
    private IEnumerator SmoothSprint()
    {
        float time = 0f;
        while (time < _playerConfig.sprintForce)
        {
            _rigidbody.AddForce(_moveDirection * Mathf.Lerp(_playerConfig.sprintForce * _playerConfig.sprintCurve.Evaluate(time), 0, time), ForceMode.Impulse);
            time += Time.deltaTime * _playerConfig.sprintSmooth;
            yield return null;
        }
    }
    
    #endregion
    
    #region --- Slope ---
    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out _slopeHit, 1f))
        {
            float angle = Vector3.Angle(Vector3.up, _slopeHit.normal);
            return angle < _playerConfig.maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeDirection() =>
        Vector3.ProjectOnPlane(_moveDirection, _slopeHit.normal).normalized;
    
    #endregion

    #region --- Sliding ---
    private void StartSlide()
    {
        if (_playerConfig.input.MovementVector.magnitude == 0)
            return;

        _isSliding = true;
        _playerConfig.playerMesh.localScale = new Vector3(
            _playerConfig.playerMesh.localScale.x,
            _playerConfig.slideYScale,
            _playerConfig.playerMesh.localScale.z);
        _rigidbody.AddForce(Vector3.down * 5f, ForceMode.Impulse);
    }

    private void EndSlide()
    {
        if (!_isSliding)
            return;

        _isSliding = false;
        _playerConfig.playerMesh.localScale = new Vector3(
            _playerConfig.playerMesh.localScale.x,
            _startYScale,
            _playerConfig.playerMesh.localScale.z);
    }

    #endregion
}

using System.Collections;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    #region --- Variables ---
    private PlayerCameraConfig _playerConfig;
    private Camera _camera;
    private Coroutine _sprintCoroutine;
    private float _startFieldOfView;
    private float _lookRotationX, _lookRotationY;
    private float _slantRotation;
    private float _sprintRotation;
    private float _startYPosition;
    private float _slideYPosition;
    private bool _isSliding;
    
    #endregion
    
    #region --- Logic ---
    public void Init(PlayerCameraConfig playerConfig)
    {
        _playerConfig = playerConfig;
        _startYPosition = _playerConfig.cameraHolder.localPosition.y;
        _slideYPosition = _startYPosition;
        _camera = Camera.main;
        _startFieldOfView = _camera!.fieldOfView;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _playerConfig.input.OnSprint += SprintZoom;
        _playerConfig.input.OnSlidePressed += StartSlide;
        _playerConfig.input.OnSlideEnd += EndSlide;
    }

    public void UpdateRotate()
    {
        // Rotate camera
        _lookRotationY += _playerConfig.input.LookVector.x * _playerConfig.sensitivity;
        _lookRotationX -= _playerConfig.input.LookVector.y * _playerConfig.sensitivity;
        _lookRotationX = Mathf.Clamp(_lookRotationX, -90, 90);
        _playerConfig.cameraHolder.transform.rotation = 
            Quaternion.Euler(_lookRotationX, _lookRotationY, _slantRotation + _sprintRotation);
        // Rotate body
        transform.rotation = Quaternion.Euler(0, _lookRotationY, 0);
    }

    public void UpdatePosition()
    {
        Vector3 holderPosition = _playerConfig.cameraHolder.localPosition;
        holderPosition.y = _slideYPosition;
        _playerConfig.cameraHolder.localPosition = Vector3.Lerp(
            _playerConfig.cameraHolder.localPosition,
            holderPosition,
            _playerConfig.slideSmooth * Time.deltaTime);
    }
    
    #endregion

    #region --- Slant ---
    public void UpdateSlant()
    {
        _slantRotation = Mathf.Lerp(_slantRotation,
            -_playerConfig.input.MovementVector.x * _playerConfig.slantForce,
            _playerConfig.slantSmooth * Time.deltaTime);
    }
    
    #endregion

    #region --- Sprint ---
    private void SprintZoom()
    {
        _camera.fieldOfView = _startFieldOfView;
        if (_sprintCoroutine != null)
            StopCoroutine(_sprintCoroutine);

        _sprintCoroutine = StartCoroutine(SmoothSprint());
    }

    private IEnumerator SmoothSprint()
    {
        float time = 0f;
        int xDirection = -Mathf.RoundToInt(_playerConfig.input.MovementVector.x);
        int zDirection = Mathf.RoundToInt(_playerConfig.input.MovementVector.y);
        while (time < _playerConfig.sprintZoom)
        {
            _camera.fieldOfView = 
                _startFieldOfView - Mathf.Lerp(zDirection * _playerConfig.sprintZoom * 
                                               _playerConfig.sprintCurve.Evaluate(time), 0, 
                    time);
            _sprintRotation = 
                Mathf.Lerp(xDirection * _playerConfig.sprintAngle * 
                           _playerConfig.sprintCurve.Evaluate(time), 0, 
                    time);
            time += Time.deltaTime * _playerConfig.sprintSmooth;
            yield return null;
        }
    }
    
    #endregion

    #region --- Slide ---
    private void StartSlide()
    {
        if (_playerConfig.input.MovementVector.magnitude == 0)
            return;
        
        _isSliding = true;
        _slideYPosition = _playerConfig.slideYPosition;
    }

    private void EndSlide()
    {
        if (!_isSliding)
            return;

        _isSliding = false;
        _slideYPosition = _startYPosition;
    }

    #endregion
}
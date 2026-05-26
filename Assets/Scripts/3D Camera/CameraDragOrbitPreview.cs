using UnityEngine;
// 加这一行，启用新输入系统
using UnityEngine.InputSystem;

public class CameraDragOrbitSmooth : MonoBehaviour
{
    [Header("环绕目标点")]
    public Transform targetPoint;

    [Header("控制参数")]
    public float dragSpeed = 0.2f;
    public float smoothTime = 0.08f;

    [Header("视角限制")]
    public float minPitch = 0f;
    public float maxPitch = 80f;

    private float _yaw;
    private float _pitch;
    private float _distance;

    private float _targetYaw;
    private float _targetPitch;

    private float _yawVelocity;
    private float _pitchVelocity;

    void Start()
    {
        if (targetPoint == null) return;

        Vector3 offset = transform.position - targetPoint.position;

        _distance = offset.magnitude;

        _yaw = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;

        float horizontalDistance = new Vector2(offset.x, offset.z).magnitude;
        _pitch = Mathf.Atan2(offset.y, horizontalDistance) * Mathf.Rad2Deg;

        _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);

        _targetYaw = _yaw;
        _targetPitch = _pitch;
    }

    void LateUpdate()
    {
        if (targetPoint == null) return;

        // ========== 这里全部替换成新输入系统 ==========
        bool isDragging =
            Mouse.current.leftButton.isPressed ||
            Mouse.current.rightButton.isPressed ||
            Mouse.current.middleButton.isPressed;

        if (isDragging)
        {
            float mouseX = Mouse.current.delta.x.ReadValue();
            float mouseY = Mouse.current.delta.y.ReadValue();

            _targetYaw += mouseX * dragSpeed;
            _targetPitch -= mouseY * dragSpeed;

            _targetPitch = Mathf.Clamp(_targetPitch, minPitch, maxPitch);
        }
        // ===========================================

        _yaw = Mathf.SmoothDampAngle(_yaw, _targetYaw, ref _yawVelocity, smoothTime);
        _pitch = Mathf.SmoothDampAngle(_pitch, _targetPitch, ref _pitchVelocity, smoothTime);

        float pitchRad = _pitch * Mathf.Deg2Rad;
        float yawRad = _yaw * Mathf.Deg2Rad;

        float x = _distance * Mathf.Cos(pitchRad) * Mathf.Sin(yawRad);
        float y = _distance * Mathf.Sin(pitchRad);
        float z = _distance * Mathf.Cos(pitchRad) * Mathf.Cos(yawRad);

        transform.position = targetPoint.position + new Vector3(x, y, z);
        transform.LookAt(targetPoint.position);
    }
}
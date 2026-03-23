using UnityEngine;

[RequireComponent(typeof(Camera))]
public class IsometricCamera : MonoBehaviour
{
    [Header("Isometric 설정")]
    [SerializeField] private float orthoSize  = 12f;
    [SerializeField] private float pitchAngle = 35.264f;  // 클래식 아이소메트릭
    [SerializeField] private float yawAngle   = 45f;      // 대각선 방향
    [SerializeField] private float distance   = 30f;

    [Header("추적 대상 (선택)")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3   offset = Vector3.zero;

    private Camera _cam;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        Apply();
    }

    private void LateUpdate()
    {
        Vector3    basePos = target != null ? target.position + offset : offset;
        Quaternion rot     = Quaternion.Euler(pitchAngle, yawAngle, 0f);

        transform.rotation = rot;
        transform.position = basePos - rot * Vector3.forward * distance;
    }

    private void Apply()
    {
        _cam.orthographic     = true;
        _cam.orthographicSize = orthoSize;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_cam == null) _cam = GetComponent<Camera>();
        if (_cam != null) Apply();
    }
#endif
}

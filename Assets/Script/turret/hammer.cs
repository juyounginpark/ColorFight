using UnityEngine;
using System.Collections;

public class hammer : MonoBehaviour
{
    [Header("Idle Wobble")]
    [SerializeField] private float idleAngle   = 8f;    // 평소 흔들 범위
    [SerializeField] private float idleSpeed   = 2f;    // 평소 흔들 속도

    [Header("Pre-Strike Wobble")]
    [SerializeField] private float buildAngle  = 18f;   // 찍기 전 긴장 흔들 범위
    [SerializeField] private float buildSpeed  = 5f;    // 찍기 전 긴장 흔들 속도
    [SerializeField] private float buildTime   = 1.0f;  // 긴장 흔들 지속 시간

    [Header("Windup & Strike")]
    [SerializeField] private float windupAngle    = -25f;  // 뒤로 당기는 각도
    [SerializeField] private float windupDuration = 0.15f;
    [SerializeField] private float swingAngle     = 90f;
    [SerializeField] private float swingDuration  = 0.16f;
    [SerializeField] private float holdDuration   = 0.1f;
    [SerializeField] private float returnDuration = 0.55f;

    [Header("Interval")]
    [SerializeField] private float interval = 2f;       // 평소 흔들 지속 시간

    [Header("Impact Shake")]
    [SerializeField] private float shakeAmount    = 0.06f;
    [SerializeField] private float shakeDuration  = 0.1f;

    [Header("Camera Shake")]
    [SerializeField] private float camShakeAmount   = 0.2f;
    [SerializeField] private float camShakeDuration = 0.25f;

    private float _baseZ;
    private float _currentZ;      // 항상 현재 각도를 추적

    private void Start()
    {
        _baseZ    = transform.localEulerAngles.z;
        _currentZ = _baseZ;
        StartCoroutine(HammerLoop());
    }

    private IEnumerator HammerLoop()
    {
        while (true)
        {
            // 평소 흔들
            yield return StartCoroutine(IdlePhase(interval, idleAngle, idleSpeed));
            // 긴장 흔들
            yield return StartCoroutine(IdlePhase(buildTime, buildAngle, buildSpeed));
            // 내려치기
            yield return StartCoroutine(Windup());
            yield return StartCoroutine(Strike());
        }
    }

    // ── 흔들 (평소 & 긴장 공용) ──────────────────
    private IEnumerator IdlePhase(float duration, float angle, float speed)
    {
        float elapsed = 0f;
        float phaseOffset = Time.time * speed; // 끊기지 않게 현재 time 기준으로 시작

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t     = (Time.time * speed) ;
            float wobble = Mathf.Sin(t) * angle;

            // 현재 각도에서 목표로 부드럽게 이동
            _currentZ = Mathf.LerpAngle(_currentZ, _baseZ + wobble, Time.deltaTime * 8f);
            SetZ(_currentZ);
            yield return null;
        }
    }

    // ── 뒤로 당기기 ──────────────────────────────
    private IEnumerator Windup()
    {
        float startZ  = _currentZ;
        float targetZ = _baseZ + windupAngle;
        float t = 0f;

        while (t < windupDuration)
        {
            t += Time.deltaTime;
            float ratio = Mathf.SmoothStep(0f, 1f, t / windupDuration);
            _currentZ = Mathf.LerpAngle(startZ, targetZ, ratio);
            SetZ(_currentZ);
            yield return null;
        }
        _currentZ = targetZ;
        SetZ(_currentZ);
    }

    // ── 내려치기 → 충격 → 복귀 ───────────────────
    private IEnumerator Strike()
    {
        float targetZ = _baseZ + swingAngle;

        // 1. 내려치기 (EaseIn)
        float startZ = _currentZ;
        float t = 0f;
        while (t < swingDuration)
        {
            t += Time.deltaTime;
            float ratio = Mathf.Pow(Mathf.Clamp01(t / swingDuration), 2f);
            _currentZ = Mathf.LerpAngle(startZ, targetZ, ratio);
            SetZ(_currentZ);
            yield return null;
        }
        _currentZ = targetZ;
        SetZ(_currentZ);

        // 2. 카메라 + 오브젝트 충격
        if (Camera.main != null)
            StartCoroutine(ShakeCamera(Camera.main.transform));

        float elapsed = 0f;
        Vector3 originPos = transform.localPosition;
        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float s = Random.Range(-shakeAmount, shakeAmount) * (1f - elapsed / shakeDuration);
            transform.localPosition = originPos + new Vector3(s, s * 0.5f, 0f);
            yield return null;
        }
        transform.localPosition = originPos;

        // 3. 바닥 고정
        yield return new WaitForSeconds(holdDuration);

        // 4. 복귀 (EaseOut)
        startZ = _currentZ;
        t = 0f;
        while (t < returnDuration)
        {
            t += Time.deltaTime;
            float ratio = 1f - Mathf.Pow(1f - Mathf.Clamp01(t / returnDuration), 2f);
            _currentZ = Mathf.LerpAngle(startZ, _baseZ, ratio);
            SetZ(_currentZ);
            yield return null;
        }
        _currentZ = _baseZ;
        SetZ(_currentZ);
    }

    private void SetZ(float z)
    {
        Vector3 e = transform.localEulerAngles;
        e.z = z;
        transform.localEulerAngles = e;
    }

    private IEnumerator ShakeCamera(Transform cam)
    {
        Vector3 origin  = cam.localPosition;
        float   elapsed = 0f;

        while (elapsed < camShakeDuration)
        {
            elapsed += Time.deltaTime;
            float strength = camShakeAmount * (1f - elapsed / camShakeDuration);
            cam.localPosition = origin + new Vector3(
                Random.Range(-1f, 1f) * strength,
                Random.Range(-1f, 1f) * strength,
                0f);
            yield return null;
        }
        cam.localPosition = origin;
    }
}

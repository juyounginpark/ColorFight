using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class cannon : MonoBehaviour
{
    [Header("Barrel")]
    [SerializeField] private Transform barrel;
    [SerializeField] private float defaultAngle = -45f;

    [Header("Barrel Angle Limit (X축)")]
    [SerializeField] private float minAngle = -80f;
    [SerializeField] private float maxAngle =   0f;

    [Header("Control")]
    [SerializeField] private float rotateSpeed     = 120f;
    [SerializeField] private float baseRotateSpeed = 120f;

    [Header("Trajectory")]
    [SerializeField] private GameObject dotPrefab;
    [SerializeField] private float timeStep    = 0.1f;
    [SerializeField] private float launchSpeed = 15f;
    [SerializeField] private float dotRadius   = 0.3f;

    [Header("Fire")]
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private float      fireInterval = 3f;
    [SerializeField] private Transform  ballParent;          // 비워두면 자동 생성

    [Header("Recoil Animation")]
    [SerializeField] private float recoilDistance  = 0.3f;   // barrel 뒤로 밀리는 거리
    [SerializeField] private float recoilDuration  = 0.08f;  // 뒤로 밀리는 시간
    [SerializeField] private float returnDuration  = 0.25f;  // 복귀 시간
    [SerializeField] private float baseTiltAngle   = 3f;     // base 뒤로 기울어지는 각도
    [SerializeField] private float shakeAmount     = 0.05f;  // base 흔들림 크기

    private bool  _selected    = false;
    private bool  _justClicked = false;
    private float _currentAngle;
    private float _fireTimer   = 0f;
    private bool  _isRecoiling = false;

    private static cannon _selectedCannon;
    private readonly List<GameObject> _dots = new List<GameObject>();

    private void Start()
    {
        Rigidbody rb = GetComponentInChildren<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        if (ballParent == null)
            ballParent = new GameObject($"{gameObject.name}_Balls").transform;

        _currentAngle = defaultAngle;
        ApplyAngle();
    }

    private void Update()
    {
        HandleSelection();
        HandleAutoFire();
        if (!_selected) return;
        HandleRotation();
    }

    // ── 선택 ────────────────────────────────────
    private void HandleSelection()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform.IsChildOf(transform))
        {
            if (_selectedCannon != null && _selectedCannon != this)
                _selectedCannon.Deselect();

            _selected      = !_selected;
            _selectedCannon = _selected ? this : null;
            _justClicked   = true;

            Debug.Log($"[cannon] {gameObject.name} selected={_selected}");
            if (!_selected) HideDots();
        }
        else if (_selected)
        {
            Deselect();
        }
    }

    // ── 회전 + 포물선 ───────────────────────────
    private void HandleRotation()
    {
        if (_justClicked) { _justClicked = false; return; }

        if (Input.GetMouseButton(0))
        {
            // 마우스 X → base Y축 회전
            float baseD = Input.GetAxis("Mouse X") * baseRotateSpeed * Time.deltaTime;
            transform.Rotate(0f, baseD, 0f, Space.World);

            // 마우스 Y → barrel X축 각도 조절
            float barrelD = Input.GetAxis("Mouse Y") * rotateSpeed * Time.deltaTime;
            _currentAngle = Mathf.Clamp(_currentAngle - barrelD, minAngle, maxAngle);
            ApplyAngle();

            UpdateTrajectory();
        }
        else
        {
            HideDots();
        }
    }

    // ── 자동 발사 ────────────────────────────────
    private void HandleAutoFire()
    {
        // 이 캐논이 선택된 상태에서 홀드 중일 때만 멈춤 (다른 캐논은 영향 없음)
        if (_selected && Input.GetMouseButton(0))
        {
            _fireTimer = 0f;
            return;
        }

        _fireTimer += Time.deltaTime;
        if (_fireTimer >= fireInterval)
        {
            _fireTimer = 0f;
            FireBall();
        }
    }

    private void FireBall()
    {
        if (ballPrefab == null || barrel == null) return;

        GameObject go = Instantiate(ballPrefab, barrel.position, barrel.rotation, ballParent);
        ball b = go.GetComponent<ball>();
        if (b != null)
            b.Launch(barrel.position, barrel.forward * launchSpeed);

        if (!_isRecoiling)
            StartCoroutine(RecoilAnimation());
    }

    // ── 반동 애니메이션 ──────────────────────────
    private IEnumerator RecoilAnimation()
    {
        _isRecoiling = true;

        Vector3 barrelOrigin = barrel.localPosition;
        Quaternion baseOrigin = transform.localRotation;

        // 1. barrel 뒤로 + base 기울기
        float t = 0f;
        while (t < recoilDuration)
        {
            t += Time.deltaTime;
            float ratio = Mathf.SmoothStep(0f, 1f, t / recoilDuration);

            barrel.localPosition = barrelOrigin - barrel.localRotation * Vector3.forward * (recoilDistance * ratio);
            transform.localRotation = baseOrigin * Quaternion.Euler(baseTiltAngle * ratio, 0f, 0f);
            yield return null;
        }

        // 2. 흔들림
        float shakeTime = 0.12f;
        float elapsed   = 0f;
        while (elapsed < shakeTime)
        {
            elapsed += Time.deltaTime;
            float s = Random.Range(-shakeAmount, shakeAmount);
            barrel.localPosition = barrelOrigin - barrel.localRotation * Vector3.forward * recoilDistance
                                 + new Vector3(s, s * 0.5f, 0f);
            yield return null;
        }

        // 3. 원위치로 복귀 (스프링)
        t = 0f;
        Vector3 recoiledPos  = barrel.localPosition;
        Quaternion recoiledRot = transform.localRotation;
        while (t < returnDuration)
        {
            t += Time.deltaTime;
            float ratio = Mathf.SmoothStep(0f, 1f, t / returnDuration);

            barrel.localPosition    = Vector3.Lerp(recoiledPos, barrelOrigin, ratio);
            transform.localRotation = Quaternion.Slerp(recoiledRot, baseOrigin, ratio);
            yield return null;
        }

        barrel.localPosition    = barrelOrigin;
        transform.localRotation = baseOrigin;
        _isRecoiling = false;
    }

    private void Deselect()
    {
        _selected = false;
        if (_selectedCannon == this) _selectedCannon = null;
        HideDots();
    }

    private void ApplyAngle()
    {
        if (barrel == null) return;
        Vector3 e = barrel.localEulerAngles;
        e.x = _currentAngle;
        barrel.localEulerAngles = e;
    }

    // ── 포물선 ───────────────────────────────────
    private void UpdateTrajectory()
    {
        if (dotPrefab == null)
        {
            Debug.LogWarning("[cannon] dotPrefab이 할당되지 않았습니다.");
            return;
        }
        if (barrel == null)
        {
            Debug.LogWarning("[cannon] barrel이 할당되지 않았습니다.");
            return;
        }

        Vector3 origin   = barrel.position;
        Vector3 velocity = barrel.forward * launchSpeed;
        Vector3 gravity  = Physics.gravity;

        Debug.Log($"[cannon] barrel.forward={barrel.forward}, velocity={velocity}, origin={origin}");

        int active = 0;

        for (int step = 1; step <= 1000; step++)
        {
            float   t   = step * timeStep;
            Vector3 pos = origin + velocity * t + 0.5f * gravity * (t * t);

            if (active >= _dots.Count)
                _dots.Add(Instantiate(dotPrefab));

            _dots[active].transform.position = pos;
            _dots[active].SetActive(true);
            active++;

            // RedTile / BlueTile 감지 → 마지막 점 표시 후 종료
            bool hitTile = false;
            foreach (Collider c in Physics.OverlapSphere(pos, dotRadius, ~0, QueryTriggerInteraction.Ignore))
            {
                if (c.CompareTag("RedTile") || c.CompareTag("BlueTile"))
                { hitTile = true; break; }
            }
            if (hitTile) break;
        }

        for (int i = active; i < _dots.Count; i++)
            _dots[i].SetActive(false);

        Debug.Log($"[cannon] 포물선 점 {active}개 표시");
    }

    private void HideDots()
    {
        foreach (var dot in _dots)
            if (dot != null) dot.SetActive(false);
    }

    private void OnDestroy()
    {
        foreach (var dot in _dots)
            if (dot != null) Destroy(dot);
    }
}

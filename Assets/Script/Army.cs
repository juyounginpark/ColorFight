using UnityEngine;
using System.Collections.Generic;

public class Army : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed        = 3f;
    [SerializeField] private float stoppingDistance = 0.5f;

    [Header("Tile Paint")]
    [SerializeField] private float paintCooldown = 0.3f;

    private Transform _target;
    private string    _enemyTurretTag;
    private string    _killBallTag;       // 이 태그의 볼에 닿으면 삭제
    private bool      _isRed;
    private Animator  _animator;
    private Rigidbody _rb;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private readonly Dictionary<TileController, float> _paintCooldowns = new();

    private void Start()
    {
        _isRed          = gameObject.CompareTag("RedMan");
        _enemyTurretTag = _isRed ? "BlueTurret" : "RedTurret";
        _killBallTag    = _isRed ? "BlueBall"   : "RedBall";
        _animator       = GetComponentInChildren<Animator>();

        // Rigidbody 없으면 자동 추가 (밀려남 + 타일 트리거 감지)
        _rb = GetComponent<Rigidbody>();
        if (_rb == null)
        {
            _rb = gameObject.AddComponent<Rigidbody>();
            _rb.constraints = RigidbodyConstraints.FreezeRotation |
                              RigidbodyConstraints.FreezePositionY;
        }

        FindNearestTarget();
    }

    private void Update()
    {
        if (_target == null)
        {
            FindNearestTarget();
            if (_target == null) { SetAnimSpeed(0f); return; }
        }

        float dist = Vector3.Distance(transform.position, _target.position);
        if (dist <= stoppingDistance) { SetAnimSpeed(0f); return; }

        Vector3 dir = (_target.position - transform.position);
        dir.y = 0f;
        transform.position += dir.normalized * moveSpeed * Time.deltaTime;
        transform.forward   = dir.normalized;
        SetAnimSpeed(1f);
    }

    private void FindNearestTarget()
    {
        GameObject[] turrets = GameObject.FindGameObjectsWithTag(_enemyTurretTag);
        float best  = float.MaxValue;
        _target = null;

        foreach (GameObject t in turrets)
        {
            float d = Vector3.Distance(transform.position, t.transform.position);
            if (d < best) { best = d; _target = t.transform; }
        }
    }

    // ── 볼 피격 → 삭제 ──────────────────────────
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(_killBallTag))
        {
            Destroy(gameObject);
            return;
        }
        TryPaint(other);
    }

    private void OnTriggerStay(Collider other) => TryPaint(other);

    // ── 타일 도색 ────────────────────────────────
    private void TryPaint(Collider other)
    {
        TileController tile = other.GetComponentInParent<TileController>();
        if (tile == null) return;

        if (_paintCooldowns.TryGetValue(tile, out float next) && Time.time < next) return;
        _paintCooldowns[tile] = Time.time + paintCooldown;

        if (_isRed) tile.SetRed();
        else        tile.SetBlue();
    }

    private void SetAnimSpeed(float speed)
    {
        if (_animator == null) return;
        _animator.SetFloat(SpeedHash, speed);
    }
}

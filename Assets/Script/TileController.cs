using UnityEngine;
using System.Collections;

public class TileController : MonoBehaviour
{
    [HideInInspector] public GameObject redPrefab;
    [HideInInspector] public GameObject bluePrefab;

    [Header("Bounce Animation")]
    [SerializeField] private float bounceHeight   = 0.4f;
    [SerializeField] private float bounceDuration = 0.35f;

    private void Awake() => _baseLocalPos = transform.localPosition;

    private bool      _ballInside  = false;
    private Coroutine _bounceCoroutine;
    private Vector3   _baseLocalPos;

    // ── 볼 감지 ──────────────────────────────────
    private void OnTriggerEnter(Collider other)
    {
        if (_ballInside) return;

        if (other.CompareTag("RedBall"))
        {
            _ballInside = true;
            SetTile("RedTile", redPrefab);
            Invoke(nameof(ResetBallInside), 0.1f);
        }
        else if (other.CompareTag("BlueBall"))
        {
            _ballInside = true;
            SetTile("BlueTile", bluePrefab);
            Invoke(nameof(ResetBallInside), 0.1f);
        }
        else if (other.CompareTag("Ball"))
        {
            _ballInside = true;
            if (gameObject.CompareTag("RedTile")) SetTile("BlueTile", bluePrefab);
            else                                  SetTile("RedTile",  redPrefab);
            Invoke(nameof(ResetBallInside), 0.1f);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("RedBall") || other.CompareTag("BlueBall") || other.CompareTag("Ball"))
            ResetBallInside();
    }

    private void ResetBallInside() => _ballInside = false;

    // ── Public API ───────────────────────────────
    public void SetRed()  => SetTile("RedTile",  redPrefab);
    public void SetBlue() => SetTile("BlueTile", bluePrefab);
    public void Invert()
    {
        if (gameObject.CompareTag("RedTile")) SetTile("BlueTile", bluePrefab);
        else                                  SetTile("RedTile",  redPrefab);
    }

    // ── 타일 변경 ────────────────────────────────
    private void SetTile(string newTag, GameObject sourcePrefab)
    {
        if (gameObject.tag == newTag) return;

        MeshFilter   myFilter   = GetComponentInChildren<MeshFilter>();
        MeshRenderer myRenderer = GetComponentInChildren<MeshRenderer>();
        MeshFilter   srcFilter   = sourcePrefab.GetComponentInChildren<MeshFilter>();
        MeshRenderer srcRenderer = sourcePrefab.GetComponentInChildren<MeshRenderer>();

        if (myFilter  != null && srcFilter   != null) myFilter.sharedMesh      = srcFilter.sharedMesh;
        if (myRenderer != null && srcRenderer != null) myRenderer.sharedMaterials = srcRenderer.sharedMaterials;

        gameObject.tag = newTag;

        // 바운스 애니메이션
        if (_bounceCoroutine != null) StopCoroutine(_bounceCoroutine);
        _bounceCoroutine = StartCoroutine(BounceAnim());
    }

    // ── 바운스 코루틴 ─────────────────────────────
    private IEnumerator BounceAnim()
    {
        Vector3 origin = _baseLocalPos;   // 항상 원래 바닥 기준으로
        float   half   = bounceDuration * 0.5f;

        // 위로 올라가기
        float t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            float ratio = Mathf.SmoothStep(0f, 1f, t / half);
            transform.localPosition = origin + Vector3.up * (bounceHeight * ratio);
            yield return null;
        }

        // 출렁이며 내려오기 (스프링)
        t = 0f;
        while (t < bounceDuration)
        {
            t += Time.deltaTime;
            float ratio   = t / bounceDuration;
            // 감쇠 진동: 위치 = A * e^(-kt) * cos(wt)
            float damping = Mathf.Exp(-5f * ratio);
            float spring  = Mathf.Cos(ratio * Mathf.PI * 4f);
            transform.localPosition = origin + Vector3.up * (bounceHeight * damping * spring);
            yield return null;
        }

        transform.localPosition = origin;
        _bounceCoroutine = null;
    }
}

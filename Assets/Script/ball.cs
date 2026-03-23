using UnityEngine;

public class ball : MonoBehaviour
{
    [SerializeField] private float lifetime = 5f;

    private Vector3 _origin;
    private Vector3 _velocity;
    private float   _elapsed;
    private bool    _launched;

    public void Launch(Vector3 origin, Vector3 velocity)
    {
        _origin   = origin;
        _velocity = velocity;
        _elapsed  = 0f;
        _launched = true;

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (!_launched) return;

        _elapsed += Time.deltaTime;
        transform.position = _origin
                           + _velocity * _elapsed
                           + 0.5f * Physics.gravity * (_elapsed * _elapsed);

        // 타일에 닿으면 삭제
        foreach (Collider c in Physics.OverlapSphere(transform.position, 0.3f, ~0, QueryTriggerInteraction.Ignore))
        {
            if (c.CompareTag("RedTile") || c.CompareTag("BlueTile"))
            {
                Destroy(gameObject);
                return;
            }
        }
    }
}

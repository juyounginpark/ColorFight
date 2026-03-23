using UnityEngine;

public class TileController : MonoBehaviour
{
    [HideInInspector] public GameObject redPrefab;
    [HideInInspector] public GameObject bluePrefab;

    private bool _ballInside = false;

    private void OnTriggerEnter(Collider other)
    {
        if (_ballInside) return;

        if (other.CompareTag("RedBall"))
        {
            _ballInside = true;
            SetTile("RedTile", redPrefab);
            // 볼이 Destroy로 사라져도 확실히 리셋
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

    public void SetRed()    => SetTile("RedTile",  redPrefab);
    public void SetBlue()   => SetTile("BlueTile", bluePrefab);
    public void Invert()
    {
        if (gameObject.CompareTag("RedTile")) SetTile("BlueTile", bluePrefab);
        else                                  SetTile("RedTile",  redPrefab);
    }

    private void SetTile(string newTag, GameObject sourcePrefab)
    {
        // 이미 같은 팀이면 무시
        if (gameObject.tag == newTag) return;

        MeshFilter   myFilter   = GetComponentInChildren<MeshFilter>();
        MeshRenderer myRenderer = GetComponentInChildren<MeshRenderer>();

        MeshFilter   srcFilter   = sourcePrefab.GetComponentInChildren<MeshFilter>();
        MeshRenderer srcRenderer = sourcePrefab.GetComponentInChildren<MeshRenderer>();

        if (myFilter != null && srcFilter != null)
            myFilter.sharedMesh = srcFilter.sharedMesh;

        if (myRenderer != null && srcRenderer != null)
            myRenderer.sharedMaterials = srcRenderer.sharedMaterials;

        gameObject.tag = newTag;
    }
}

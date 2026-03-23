using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class mapBUILD : MonoBehaviour
{
    [Header("Block Prefabs")]
    [SerializeField] private GameObject redBlockPrefab;
    [SerializeField] private GameObject blueBlockPrefab;

    [Header("Map Settings")]
    [SerializeField] private int width = 15;   // X축 타일 수
    [SerializeField] private int depth = 20;   // Z축 타일 수 (팀당)

    public Transform RedTeamCenter  { get; private set; }
    public Transform BlueTeamCenter { get; private set; }

    [ContextMenu("Build Map")]
    public void BuildMap()
    {
        if (redBlockPrefab == null || blueBlockPrefab == null)
        {
            Debug.LogError("RedBlockPrefab 또는 BlueBlockPrefab이 할당되지 않았습니다.");
            return;
        }

        ClearMap();

        // 프리팹 실제 크기로 타일 간격 결정
        float tileSizeX = GetPrefabSize(redBlockPrefab).x;
        float tileSizeZ = GetPrefabSize(redBlockPrefab).z;

        float redTotalZ  = depth * tileSizeZ;
        float blueTotalZ = depth * tileSizeZ;

        // 레드팀: Z = 0 ~ redTotalZ - tileSizeZ
        Vector3 redCenter = new Vector3(
            (width - 1) * tileSizeX * 0.5f,
            0f,
            (depth - 1) * tileSizeZ * 0.5f);

        GameObject redObj = new GameObject("RedTeam");
        redObj.transform.SetParent(transform);
        redObj.transform.position = redCenter;
        RedTeamCenter = redObj.transform;

        for (int x = 0; x < width; x++)
            for (int z = 0; z < depth; z++)
                SetupTile(
                    Instantiate(redBlockPrefab,
                        new Vector3(x * tileSizeX, 0f, z * tileSizeZ),
                        Quaternion.identity, RedTeamCenter),
                    "RedTile");

        // 블루팀: Z = redTotalZ ~ redTotalZ + blueTotalZ - tileSizeZ  (레드팀 바로 다음 행부터)
        float blueZStart = redTotalZ;

        Vector3 blueCenter = new Vector3(
            (width - 1) * tileSizeX * 0.5f,
            0f,
            blueZStart + (depth - 1) * tileSizeZ * 0.5f);

        GameObject blueObj = new GameObject("BlueTeam");
        blueObj.transform.SetParent(transform);
        blueObj.transform.position = blueCenter;
        BlueTeamCenter = blueObj.transform;

        for (int x = 0; x < width; x++)
            for (int z = 0; z < depth; z++)
                SetupTile(
                    Instantiate(blueBlockPrefab,
                        new Vector3(x * tileSizeX, 0f, blueZStart + z * tileSizeZ),
                        Quaternion.identity, BlueTeamCenter),
                    "BlueTile");

        Debug.Log($"맵 생성 완료 | 타일 크기: ({tileSizeX}, {tileSizeZ})" +
                  $"\n레드 Z: 0 ~ {redTotalZ - tileSizeZ}  |  블루 Z: {blueZStart} ~ {blueZStart + blueTotalZ - tileSizeZ}");
    }

    [ContextMenu("Clear Map")]
    public void ClearMap()
    {
        Transform t;
        t = transform.Find("RedTeam");
        if (t != null) DestroyImmediate(t.gameObject);
        t = transform.Find("BlueTeam");
        if (t != null) DestroyImmediate(t.gameObject);
        RedTeamCenter  = null;
        BlueTeamCenter = null;
    }

    private void SetupTile(GameObject tile, string tag)
    {
        tile.tag = tag;

        // 렌더러 bounds로 콜라이더 크기 결정
        Renderer r = tile.GetComponentInChildren<Renderer>();
        Vector3 colCenter = Vector3.zero;
        Vector3 colSize   = Vector3.one;
        if (r != null)
        {
            colCenter = tile.transform.InverseTransformPoint(r.bounds.center);
            colSize   = r.bounds.size;
        }

        // 기존 콜라이더 모두 물리용(비트리거)으로 설정
        foreach (Collider c in tile.GetComponentsInChildren<Collider>())
            c.isTrigger = false;

        // 물리용 콜라이더가 없으면 추가
        if (tile.GetComponentInChildren<Collider>() == null)
        {
            BoxCollider phys = tile.AddComponent<BoxCollider>();
            phys.center    = colCenter;
            phys.size      = colSize;
            phys.isTrigger = false;
        }

        // 감지 전용 트리거 콜라이더 추가 (물리 차단과 분리)
        BoxCollider trigger = tile.AddComponent<BoxCollider>();
        trigger.center    = colCenter;
        trigger.size      = colSize;
        trigger.isTrigger = true;

        // Rigidbody (kinematic)
        Rigidbody rb = tile.GetComponent<Rigidbody>();
        if (rb == null) rb = tile.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        TileController tc = tile.GetComponent<TileController>();
        if (tc == null) tc = tile.AddComponent<TileController>();
        tc.redPrefab  = redBlockPrefab;
        tc.bluePrefab = blueBlockPrefab;
    }

    private Vector3 GetPrefabSize(GameObject prefab)
    {
        Renderer r = prefab.GetComponentInChildren<Renderer>();
        if (r != null) return r.bounds.size;

        Collider c = prefab.GetComponentInChildren<Collider>();
        if (c != null) return c.bounds.size;

        // 바운드를 못 찾으면 기본 1x1
        return Vector3.one;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(mapBUILD))]
public class mapBUILDEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GUILayout.Space(10);
        mapBUILD builder = (mapBUILD)target;
        if (GUILayout.Button("Build Map", GUILayout.Height(40)))
            builder.BuildMap();
        if (GUILayout.Button("Clear Map"))
            builder.ClearMap();
    }
}
#endif

using UnityEngine;
using System.Collections;

public class base_ : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private GameObject armyPrefab;
    [SerializeField] private float      spawnInterval = 4f;
    [SerializeField] private Transform  spawnParent;        // 비워두면 자동 생성

    [Header("Spawn Animation")]
    [SerializeField] private float spawnDepth    = 1.5f;   // 바닥 아래에서 시작하는 깊이
    [SerializeField] private float riseDuration  = 0.6f;   // 올라오는 시간
    [SerializeField] private float riseHeight    = 0.2f;   // 표면 위로 튀어오르는 높이

    private void Start()
    {
        if (spawnParent == null)
            spawnParent = new GameObject($"{gameObject.name}_Armies").transform;

        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnArmy();
        }
    }

    private void SpawnArmy()
    {
        if (armyPrefab == null) return;

        Vector3    spawnPos = transform.position + Vector3.down * spawnDepth;
        GameObject army     = Instantiate(armyPrefab, spawnPos, transform.rotation, spawnParent);

        StartCoroutine(RiseAnimation(army.transform, transform.position));
    }

    private IEnumerator RiseAnimation(Transform army, Vector3 targetPos)
    {
        Vector3 startPos = army.position;
        Vector3 peakPos  = targetPos + Vector3.up * riseHeight;
        float   t        = 0f;

        // 1. 밑에서 표면 위로 올라오며 약간 튀어오름
        while (t < riseDuration)
        {
            t += Time.deltaTime;
            float ratio = Mathf.Clamp01(t / riseDuration);

            // 위로 올라오면서 약간 오버슈트 (출렁)
            float y = Mathf.SmoothStep(0f, 1f, ratio);
            float bounce = Mathf.Sin(ratio * Mathf.PI) * riseHeight;
            army.position = Vector3.Lerp(startPos, targetPos, y) + Vector3.up * bounce;
            yield return null;
        }

        army.position = targetPos;
    }
}

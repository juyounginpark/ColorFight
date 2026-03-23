using UnityEngine;
using System.Collections.Generic;

public class Swiper : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField] private float rotateSpeed = 90f;

    [Header("Paint Mode")]
    [SerializeField] private bool paintRed;
    [SerializeField] private bool paintBlue;

    [Header("감지 쿨다운")]
    [SerializeField] private float changeCooldown = 0.5f;  // 같은 타일 재변경 최소 간격

    private readonly Dictionary<TileController, float> _cooldowns = new();

    private void Update()
    {
        transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f, Space.Self);
    }

    private void OnTriggerStay(Collider other)
    {
        TileController tile = other.GetComponentInParent<TileController>();
        if (tile == null) return;

        // 쿨다운 체크
        if (_cooldowns.TryGetValue(tile, out float nextTime) && Time.time < nextTime)
            return;

        _cooldowns[tile] = Time.time + changeCooldown;

        bool red  = paintRed  && !paintBlue;
        bool blue = paintBlue && !paintRed;

        if      (red)  tile.SetRed();
        else if (blue) tile.SetBlue();
        else           tile.Invert();
    }
}

using UnityEngine;

public class Swiper : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 90f;

    private void Update()
    {
        transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f, Space.Self);
    }
}

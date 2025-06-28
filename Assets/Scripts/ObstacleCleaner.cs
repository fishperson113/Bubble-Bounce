using UnityEngine;

public class ObstacleCleaner : MonoBehaviour
{
    [SerializeField] private float offsetBehindCamera = 30f; // bao xa phía sau camera thì xóa

    private Transform cam;

    private void Start()
    {
        cam = Camera.main.transform;
    }

    private void Update()
    {
        if (transform.position.x < cam.position.x - offsetBehindCamera)
        {
            Destroy(gameObject);
        }
    }
}

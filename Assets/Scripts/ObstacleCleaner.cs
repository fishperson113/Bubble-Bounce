using UnityEngine;

public class ObstacleCleaner : MonoBehaviour
{
    [SerializeField] private float offsetBehindCamera = 30f; // bao xa ph�a sau camera th� x�a

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

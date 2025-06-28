using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Obstacle Prefabs")]
    [SerializeField] private GameObject[] obstaclePrefabs;

    [Header("Spawn Settings")]
    [SerializeField] private float firstOffsetX = 10f;          // Offset cho obstacle đầu tiên
    [SerializeField] private float spawnDistance = 20f;         // Khoảng cách cố định giữa các obstacle

    [SerializeField] private float topY = 9.5f;
    [SerializeField] private float bottomY = -12.5f;

    [Header("Move Settings")]
    [SerializeField] private float obstacleSpeed = 3f;

    private float nextSpawnX;

    private void Start()
    {
        // Lấy mép phải camera làm mốc, spawn obstacle đầu tiên
        float cameraRightX = Camera.main.ViewportToWorldPoint(new Vector3(1, 0.5f, 0)).x;
        nextSpawnX = cameraRightX + firstOffsetX;

        // Spawn obstacle đầu tiên
        SpawnObstacle(nextSpawnX);
        // Update vị trí tiếp theo
        nextSpawnX += spawnDistance;
    }

    private void Update()
    {
        // Camera mép phải
        float cameraRightX = Camera.main.ViewportToWorldPoint(new Vector3(1, 0.5f, 0)).x;

        // Chỉ spawn khi obstacle sắp "lọt" vào camera view (ví dụ cách camera X đơn vị)
        if (nextSpawnX < cameraRightX + 20f) // bạn có thể chỉnh "20f" tuỳ ý
        {
            SpawnObstacle(nextSpawnX);
            nextSpawnX += spawnDistance; // Update vị trí spawn tiếp theo
        }
    }

    private void SpawnObstacle(float spawnX)
    {
        if (obstaclePrefabs.Length == 0)
        {
            Debug.LogWarning("Assign obstacle prefabs!");
            return;
        }

        // Random prefab
        int randIndex = Random.Range(0, obstaclePrefabs.Length);
        GameObject selectedPrefab = obstaclePrefabs[randIndex];

        // Chọn trên hoặc dưới
        bool spawnTop = Random.value < 0.5f;
        float yPos = spawnTop ? topY : bottomY;

        Vector3 spawnPos = new Vector3(spawnX, yPos, 0f);

        // Instantiate
        GameObject newObstacle = Instantiate(selectedPrefab, spawnPos, Quaternion.identity);

        if (newObstacle.GetComponent<ObstacleCleaner>() == null)
        {
            newObstacle.AddComponent<ObstacleCleaner>();
        }

        // Flip nếu trên
        if (spawnTop)
        {
            SpriteRenderer sr = newObstacle.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.flipY = true;
            else
            {
                Vector3 scale = newObstacle.transform.localScale;
                scale.y *= -1;
                newObstacle.transform.localScale = scale;
            }
        }

        Debug.Log($"Spawned at X: {spawnX}");
    }
}

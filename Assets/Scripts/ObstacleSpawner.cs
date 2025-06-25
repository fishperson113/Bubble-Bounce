using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Obstacle Prefab")]
    [SerializeField] private GameObject obstaclePrefab;

    [Header("Sprites List")]
    [SerializeField] private Sprite[] obstacleSprites;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private float leftLimit = -6f;
    [SerializeField] private float rightLimit = 6f;
    [SerializeField] private float topY = 4.5f;
    [SerializeField] private float bottomY = -3.5f;

    private void Start()
    {
        InvokeRepeating(nameof(SpawnObstacle), 1f, spawnInterval);
    }

    private void SpawnObstacle()
    {
        
        float x = Random.Range(leftLimit, rightLimit);
        float y = Random.value < 0.5f ? topY : bottomY;
        Vector3 spawnPos = new Vector3(x, y, 0);

        
        GameObject obj = Instantiate(obstaclePrefab, spawnPos, Quaternion.identity);

        
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        int randIndex = Random.Range(0, obstacleSprites.Length);
        sr.sprite = obstacleSprites[randIndex];

        
        sr.flipY = (y == topY);

        
        PolygonCollider2D pc = obj.GetComponent<PolygonCollider2D>();
        if (pc != null) Destroy(pc);
        obj.AddComponent<PolygonCollider2D>();
    }
}

using UnityEngine;

public class DrawManager : MonoBehaviour
{
    private Camera mainCamera;
    [SerializeField] private Line drawPrefab;
    public const float resolution = 0.1f;
    private Line currentLine;
    [SerializeField] private float aimLineLength = 2f;
    void Start()
    {
        mainCamera= Camera.main;
        if (currentLine == null && drawPrefab != null)
        {
            currentLine = Instantiate(drawPrefab, Vector3.zero, Quaternion.identity);
            currentLine.Hide();
        }
    }

    public void DrawAimingLine(Vector3 worldStart, Vector2 swipeScreenDirection)
    {
        Vector2 dir = -swipeScreenDirection.normalized;
        Vector3 worldEnd = worldStart + (Vector3)(dir * aimLineLength);

        currentLine.Show();
        currentLine.SetLine(worldStart, worldEnd);
    }
    public void HideAimingLine()
    {
        if (currentLine != null)
        {
            currentLine.Hide();
        }
    }
}

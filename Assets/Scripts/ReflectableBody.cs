using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Rigidbody2D))]
public class ReflectableBody : MonoBehaviour, IReflectable
{
    [Header("Reflection Settings")]
    [SerializeField] private float reflectionDuration = 0.5f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public void ApplyReflection(Vector2 direction, float distance)
    {
        Vector2 velocity = direction.normalized * (distance / reflectionDuration);
        rb.linearVelocity = velocity;
    }
}

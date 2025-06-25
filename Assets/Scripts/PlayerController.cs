using UnityEngine;
using DG.Tweening;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float gravity = 9.8f;
    [SerializeField] private float maxFallSpeed = 10f;

    [Header("Bubble Reflection")]
    [SerializeField] private float bubbleReflectionMultiplier = 2f;
    [SerializeField] private float reflectionDuration = 0.5f;
    [SerializeField] private Ease reflectionEase = Ease.OutBack;

    [Header("Animation")]
    [SerializeField] private float squashAmount = 0.3f;
    [SerializeField] private float squashDuration = 0.3f;


    [Header("Gizmo Settings")]
    [SerializeField] private bool showDirectionGizmo = true;
    [SerializeField] private float gizmoLength = 4f;         // Increased from 2f to 4f
    [SerializeField] private Color gizmoColor = Color.yellow;
    [SerializeField] private float dashSize = 0.4f;          // Increased from 0.2f to 0.4f
    [SerializeField] private float gapSize = 0.2f;           // Increased from 0.1f to 0.2f
    [SerializeField] private int lineThickness = 3;          // New parameter for simulated thickness
    [SerializeField] private float thicknessSpacing = 0.05f; // New parameter for spacing between lines

    [Header("Health Management")]
    [SerializeField] private PlayerHealthManager healthManager;

    [Header("Revive Position")]
    [SerializeField] private Vector2 screenOffset = new Vector2(-0.3f, 0.2f);

    private Animator playerAnim;
    private bool isJumping;
    private bool isDied = false;


    private Rigidbody2D rb;
    private bool isAlive = true;
    private float currentVerticalVelocity = 0f;
    private bool isGrounded = false;

    private bool isBeingReflected = false;
    private Vector3 reflectionTarget;
    private Vector2 currentDirection;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerAnim = GetComponent<Animator>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            Debug.Log("Rigidbody2D component added to player");
        }

        rb.gravityScale = 0f; 
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        currentDirection = Vector2.down;
        

    }

    void Update()
    {
        if (!isAlive|| isGrounded) return;

        ApplyGravity();
        currentDirection = new Vector2(0, currentVerticalVelocity).normalized;

        if (playerAnim != null)
        {
            playerAnim.SetBool("isJumping", isJumping);
        }
        if (playerAnim != null)
        {
            playerAnim.SetBool("isDied", isDied);
        }
    }

    private void ApplyGravity()
    {
        currentVerticalVelocity -= gravity * Time.deltaTime;
        currentVerticalVelocity = Mathf.Max(currentVerticalVelocity, -maxFallSpeed);

        transform.Translate(0, currentVerticalVelocity * Time.deltaTime, 0);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bubble"))
        {
            Bubble bubble = other.GetComponent<Bubble>();
            if (bubble != null)
            {
                // Calculate reflection direction (away from bubble center)
                Vector2 reflectionDirection = (transform.position - other.transform.position).normalized;

                float bubbleForce = bubble.GetBubbleForce();

                float reflectionDistance = bubbleReflectionMultiplier * bubbleForce;

                // Apply the reflection using DOTween
                ApplyReflection(reflectionDirection, reflectionDistance);

                // Pop the bubble
                bubble.Pop();
            }
        }
    }

    private void ApplyReflection(Vector2 direction, float distance)
    {
        // Flag that reflection is happening
        isBeingReflected = true;

        // Update current direction for gizmo
        currentDirection = direction;

        // Kill any existing tweens
        DOTween.Kill(transform);

        // Set the reflection target for the gizmo (this was missing)
        reflectionTarget = transform.position + (Vector3)(direction * distance);

        // Squash effect on impact
        transform.DOPunchScale(new Vector3(squashAmount, squashAmount, 0), squashDuration, 2, 0.5f);

        isJumping = true;
        // Move the player using DOTween
        transform.DOMove(reflectionTarget, reflectionDuration)
            .SetEase(reflectionEase)
            .OnComplete(() => {
                isBeingReflected = false;

                // This makes the player stop moving after the reflection and continue falling
                currentVerticalVelocity = direction.y * distance * 0.5f;
                isJumping = false;
            });
    }
    // Draw direction gizmo
    // Draw direction gizmo with dashed lines
    private void OnDrawGizmos()
    {
        if (!showDirectionGizmo || !Application.isPlaying) return;

        Gizmos.color = gizmoColor;

        // If being reflected, draw dashed line to reflection target
        if (isBeingReflected)
        {
            DrawDashedLine(transform.position, reflectionTarget);
        }
        // Otherwise draw current movement direction
        else
        {
            Vector3 endPoint = transform.position + new Vector3(currentDirection.x, currentDirection.y, 0) * gizmoLength;
            DrawDashedLine(transform.position, endPoint);
        }
    }

    // Helper method to draw a dashed line
    private void DrawDashedLine(Vector3 start, Vector3 end)
    {
        // Calculate the direction and total distance
        Vector3 direction = (end - start).normalized;
        float distance = Vector3.Distance(start, end);

        // Calculate perpendicular direction for thickness
        Vector3 perpendicular = Vector3.Cross(direction, Vector3.forward).normalized;

        // Draw multiple offset lines to simulate thickness
        for (int t = 0; t < lineThickness; t++)
        {
            // Calculate offset for this line (center line has no offset)
            float offset = 0;
            if (t > 0)
            {
                // Alternate between positive and negative offsets
                int side = (t % 2 == 0) ? 1 : -1;
                offset = side * thicknessSpacing * ((t + 1) / 2);
            }

            // Offset start and end points
            Vector3 offsetStart = start + perpendicular * offset;
            Vector3 offsetEnd = end + perpendicular * offset;

            // Calculate the number of segments
            float dashGapSum = dashSize + gapSize;
            int segmentCount = Mathf.FloorToInt(distance / dashGapSum);

            // Start position
            Vector3 currentPos = offsetStart;

            // Draw each dash segment
            for (int i = 0; i < segmentCount; i++)
            {
                // Calculate the start and end of this dash
                Vector3 dashStart = currentPos;
                Vector3 dashEnd = currentPos + direction * dashSize;

                // Make sure we don't draw past the end point
                if (Vector3.Distance(dashStart, offsetStart) + dashSize > distance)
                {
                    dashEnd = offsetEnd;
                }

                // Draw the dash
                Gizmos.DrawLine(dashStart, dashEnd);

                // Move to the next dash
                currentPos = dashStart + direction * dashGapSum;
            }
        }

        // Make the arrow tips thicker too
        if (isBeingReflected || currentDirection != Vector2.zero)
        {
            Vector3 endPoint = isBeingReflected ? reflectionTarget :
                transform.position + new Vector3(currentDirection.x, currentDirection.y, 0) * gizmoLength;

            // Draw an arrow tip with thickness
            float arrowSize = 0.4f;  // Increased from 0.2f
            Vector3 dir = isBeingReflected ?
                (reflectionTarget - transform.position).normalized : currentDirection.normalized;

            for (int t = 0; t < lineThickness; t++)
            {
                float offset = 0;
                if (t > 0)
                {
                    int side = (t % 2 == 0) ? 1 : -1;
                    offset = side * thicknessSpacing * ((t + 1) / 2);
                }

                Vector3 offsetEndPoint = endPoint + perpendicular * offset;
                Vector3 arrowPos = offsetEndPoint - dir * arrowSize;

                Vector3 right = Quaternion.Euler(0, 0, 30) * -dir * arrowSize;
                Vector3 left = Quaternion.Euler(0, 0, -30) * -dir * arrowSize;

                Gizmos.DrawLine(offsetEndPoint, arrowPos + right);
                Gizmos.DrawLine(offsetEndPoint, arrowPos + left);
            }
        }
    }

    private void BackToOffsetPositionInCamera()
    {
        float screenX = Screen.width * (0.5f + (screenOffset.x));
        float screenY = Screen.height * (0.5f + (screenOffset.y));

        Vector3 screenPosition = new Vector3(screenX, screenY, Camera.main.nearClipPlane);
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
        worldPosition.z = 0f;

        transform.position = worldPosition;
    }

    private IEnumerator BackAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isDied = false;
        BackToOffsetPositionInCamera();
    }



    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            isDied = true;

            currentVerticalVelocity = 0f;
            isGrounded = false;
            isBeingReflected = false;
            DOTween.Kill(transform);
            healthManager.LoseHeart();
            StartCoroutine(BackAfterDelay(5f)); 

        }
    }
    void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }

    private void OnDestroy()
    {
        DOTween.Kill(transform);
    }
}
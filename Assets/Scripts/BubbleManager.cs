using System.Collections.Generic;
using UnityEngine;

public class BubbleManager : MonoBehaviour
{
    [Header("Bubble Settings")]
    [SerializeField] private GameObject bubblePrefab;
    [SerializeField] private int initialPoolSize = 10;

    [Header("Bubble Force Settings")]
    [SerializeField] private float minBubbleSize = 0.3f;
    [SerializeField] private float maxBubbleSize = 1.5f;
    [SerializeField] private float timeToMaxForce = 1.0f;

    [Header("Gameplay Settings")]
    [SerializeField] private int maxActiveBubbles = 5;
    [SerializeField] private float bubbleForce = 5f;

    [Header("Draw Settings")]
    [SerializeField] private DrawManager drawManager;
    [SerializeField] private float minSwipeToShowLine = 20f;

    private List<GameObject> bubblePool;
    private List<GameObject> activeBubbles;
    private Camera mainCamera;

    private bool isHolding = false;
    private float holdDuration = 0f;
    private GameObject currentGrowingBubble = null;

    private Vector2 swipeStart;
    private Vector2 swipeEnd;
    Vector2 direction;
    Vector2 swipe;
    Vector3 worldStart;
    private void Awake()
    {
        bubblePool = new List<GameObject>();
        activeBubbles = new List<GameObject>();
        mainCamera = Camera.main;

        InitializeBubblePool();
    }

    private void Update()
    {
        // Check for mouse button down to start growing a bubble
        if (Input.GetMouseButtonDown(0))
        {
            swipeStart = Input.mousePosition;
            StartBubbleGrow();
        }

        // Check for ongoing hold to continue growing the bubble
        if (isHolding && currentGrowingBubble != null)
        {
            UpdateBubbleGrow();
        }

        // Check for mouse button release to finalize the bubble
        if (Input.GetMouseButtonUp(0) && isHolding)
        {
            FinalizeBubble();
        }

        // Check for any bubbles that need to be deactivated
        ManageActiveBubbles();
    }

    private void InitializeBubblePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject bubble = Instantiate(bubblePrefab, Vector3.zero, Quaternion.identity, transform);
            bubble.SetActive(false);
            bubblePool.Add(bubble);
        }
    }

    private void StartBubbleGrow()
    {
        PauseGameTime();
        // Limit the number of active bubbles
        if (activeBubbles.Count >= maxActiveBubbles)
        {
            // Deactivate the oldest bubble to make room for a new one
            ReturnBubbleToPool(activeBubbles[0]);
        }

        GameObject bubble = GetBubbleFromPool();
        if (bubble == null) return;

        // Set bubble position to mouse position
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        bubble.transform.position = mousePos;

        // Set initial size (minimum size)
        Bubble bubbleComponent = bubble.GetComponent<Bubble>();
        if (bubbleComponent != null)
        {
            bubble.transform.localScale = Vector3.one * minBubbleSize;
        }

        // Activate the bubble
        bubble.SetActive(true);

        // Start tracking the hold
        isHolding = true;
        currentGrowingBubble = bubble;
        holdDuration = 0f;

        // Add to active bubbles list
        activeBubbles.Add(bubble);
    }

    private void UpdateBubbleGrow()
    {
        if (currentGrowingBubble == null) return;

        holdDuration += Time.unscaledDeltaTime;

        //float forcePercentage = Mathf.Clamp01(holdTime / timeToMaxForce);
        float forcePercentage = Mathf.Clamp01(holdDuration / timeToMaxForce);

        float currentSize = Mathf.Lerp(minBubbleSize, maxBubbleSize, forcePercentage);

        currentGrowingBubble.transform.localScale = Vector3.one * currentSize;

        float currentForce = bubbleForce * forcePercentage;
        Bubble bubbleComponent = currentGrowingBubble.GetComponent<Bubble>();
        if (bubbleComponent != null)
        {
            bubbleComponent.SetBubbleForce(currentForce);
        }

        swipeEnd = Input.mousePosition;
        direction = Vector2.zero;
        swipe = swipeEnd - swipeStart;
        worldStart = currentGrowingBubble.transform.position;
        if (swipe.magnitude >= minSwipeToShowLine)
        {
            direction = -swipe.normalized;
        }
        drawManager.DrawAimingLine(worldStart, swipe);
        // Automatically finalize the bubble if it reaches maximum size
        if (forcePercentage >= 0.99f)
        {
            FinalizeBubble();
        }
    }

    private void FinalizeBubble()
    {
        if (currentGrowingBubble == null) return;
        ResumeGameTime();
        drawManager.HideAimingLine();

        Bubble bubbleComponent = currentGrowingBubble.GetComponent<Bubble>();
        if (bubbleComponent != null)
        {
            bubbleComponent.Launch(direction);
        }

        // Reset holding state
        isHolding = false;
        currentGrowingBubble = null;
    }

    private GameObject GetBubbleFromPool()
    {
        // Check if there's an available bubble in the pool
        foreach (GameObject bubble in bubblePool)
        {
            if (!bubble.activeInHierarchy)
            {
                return bubble;
            }
        }

        // If no bubbles are available, create a new one and add it to the pool
        GameObject newBubble = Instantiate(bubblePrefab, Vector3.zero, Quaternion.identity, transform);
        newBubble.SetActive(false);
        bubblePool.Add(newBubble);
        return newBubble;
    }

    private void ReturnBubbleToPool(GameObject bubble)
    {
        if (bubble == null) return;

        // Get the Bubble component and start the shrink animation
        Bubble bubbleComponent = bubble.GetComponent<Bubble>();
        if (bubbleComponent != null)
        {
            bubbleComponent.BeginShrink();
        }
        else
        {
            bubble.SetActive(false);
        }

        // Remove from active bubbles list
        activeBubbles.Remove(bubble);
    }

    private void ManageActiveBubbles()
    {
        // Check if any bubbles have moved off-screen and should be deactivated
        for (int i = activeBubbles.Count - 1; i >= 0; i--)
        {
            GameObject bubble = activeBubbles[i];
            if (bubble == null || !bubble.activeInHierarchy)
            {
                activeBubbles.RemoveAt(i);
                continue;
            }

            // Check if bubble is outside camera view
            Vector3 viewportPosition = mainCamera.WorldToViewportPoint(bubble.transform.position);
            if (viewportPosition.x < -0.1f || viewportPosition.x > 1.1f ||
                viewportPosition.y < -0.1f || viewportPosition.y > 1.1f)
            {
                ReturnBubbleToPool(bubble);
            }
        }
    }

    public void OnBubblePopped(GameObject bubble)
    {
        activeBubbles.Remove(bubble);
    }
    private void PauseGameTime()
    {
        Time.timeScale = 0f;
    }

    private void ResumeGameTime()
    {
        Time.timeScale = 1f;
    }
}
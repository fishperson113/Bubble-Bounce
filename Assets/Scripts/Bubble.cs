using DG.Tweening;
using System;
using UnityEngine;

public class Bubble : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float shrinkDuration = 0.3f;
    [SerializeField] private Ease shrinkEase = Ease.InBack;

    [Header("Effects")]
    [SerializeField] private ParticleSystem popEffect;
    [SerializeField] private AudioClip popSound;

    [Header("Stats")]
    [SerializeField] private float speed = 5f;

    private float bubbleForce;

    private Sequence currentAnimation;
    private BubbleManager bubbleManager;

    private void Awake()
    {
        if (bubbleManager == null)
        {
            bubbleManager = FindFirstObjectByType<BubbleManager>();
        }
    }

    private void OnEnable()
    {
        // Kill any existing animations
        if (currentAnimation != null)
        {
            currentAnimation.Kill();
        }
    }

    public void SetBubbleForce(float force)
    {
        bubbleForce = force;
    }

    public float GetBubbleForce()
    {
        return bubbleForce;
    }

    public void BeginShrink()
    {
        // Kill any existing animations
        if (currentAnimation != null)
        {
            currentAnimation.Kill();
        }

        // Create and play the shrink animation
        currentAnimation = DOTween.Sequence();
        currentAnimation.Append(transform.DOScale(Vector3.zero, shrinkDuration).SetEase(shrinkEase));
        currentAnimation.OnComplete(() => gameObject.SetActive(false));
    }

    public void Pop()
    {
        // Play pop effect if available
        if (popEffect != null)
        {
            popEffect.transform.position = transform.position;
            popEffect.Play();
        }

        // Play pop sound if available
        if (popSound != null)
        {
            AudioSource.PlayClipAtPoint(popSound, transform.position, 0.7f);
        }

        // Quickly shrink the bubble to give a popping effect
        if (currentAnimation != null)
        {
            currentAnimation.Kill();
        }

        currentAnimation = DOTween.Sequence();
        currentAnimation.Append(transform.DOScale(transform.localScale * 1.2f, 0.05f).SetEase(Ease.OutQuad));
        currentAnimation.Append(transform.DOScale(Vector3.zero, 0.1f).SetEase(Ease.InQuad));
        currentAnimation.OnComplete(() => {
            gameObject.SetActive(false);

            // Notify the bubble manager
            if (bubbleManager != null)
            {
                bubbleManager.OnBubblePopped(gameObject);
            }
        });
    }

    private void OnDisable()
    {
        // Ensure we kill any animations when the object is disabled
        if (currentAnimation != null)
        {
            currentAnimation.Kill();
            currentAnimation = null;
        }
    }

    public void Launch(Vector2 direction)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction*speed;
        }
    }
}
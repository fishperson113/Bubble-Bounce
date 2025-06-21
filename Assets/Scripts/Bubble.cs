using DG.Tweening;
using UnityEngine;

public class Bubble : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float initialGrowDuration = 0.2f;
    [SerializeField] private float shrinkDuration = 0.3f;
    [SerializeField] private Ease initialGrowEase = Ease.OutBack;
    [SerializeField] private Ease shrinkEase = Ease.InBack;

    [Header("Effects")]
    [SerializeField] private ParticleSystem popEffect;
    [SerializeField] private AudioClip popSound;

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

        Vector3 targetScale = transform.localScale;

        transform.localScale = Vector3.zero;

        currentAnimation = DOTween.Sequence();
        currentAnimation.Append(transform.DOScale(targetScale, initialGrowDuration).SetEase(initialGrowEase));
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
}
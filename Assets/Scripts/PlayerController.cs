using UnityEngine;
using DG.Tweening;
using System.Collections;
using System;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float gravity = 9.8f;

    [Header("Health Management")]
    [SerializeField] private PlayerHealthManager healthManager;

    [Header("Revive Position")]
    [SerializeField] private Vector2 screenOffset = new Vector2(-0.3f, 0.2f);

    [Header("Projection Settings")]
    [SerializeField] private Projection projection;

    [Header("UI")]
    [SerializeField] private TMP_Text distanceText;

    private float startX;
    private float distanceTravelled = 0f;

    private Animator playerAnim;
    private bool isDied = false;

    private Rigidbody2D rb;
    private bool isAlive = true;

    private bool isGrounded = false;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerAnim = GetComponent<Animator>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            Debug.Log("Rigidbody2D component added to player");
        }

        rb.gravityScale = gravity / 9.8f; 
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.linearVelocity = Vector2.zero;

        startX = transform.position.x;
        distanceTravelled = 0f;
    }

    void Update()
    {
        if (Time.timeScale == 0f && projection != null)
        {
            projection.SimulateTrajectory(rb.linearVelocity);
        }
        else if (Time.timeScale > 0f && projection != null)
        {
            projection.ClearLine();
        }
        if (playerAnim != null)
        {
            playerAnim.SetBool("isJumping", !isGrounded && rb.linearVelocity.y > 0.1f);
            playerAnim.SetBool("isDied", isDied);
        }
        if (isAlive)
        {
            distanceTravelled = transform.position.x - startX;
            if (distanceTravelled < 0) distanceTravelled = 0;
            distanceText.text = $"{Mathf.FloorToInt(distanceTravelled)} m";
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

        BackToOffsetPositionInCamera();
        isDied = false;
        isAlive = true; 
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isAlive) return; 

        if (collision.collider.CompareTag("Obstacle"))
        {
            Debug.Log("Hit Obstacle");
            HandleDeath();
        }
        else if (collision.collider.CompareTag("Ground"))
        {
            Debug.Log("Hit Ground");
            HandleDeath();
        }
    }

    private void HandleDeath()
    {
        isDied = true;
        isAlive = false;
        isGrounded = false;

        DOTween.Kill(transform);
        healthManager.LoseHeart();
        if (healthManager.GetCurrentHeart() <= 0)
        {
            healthManager.SetGameOverScore(distanceTravelled);
        }

        StartCoroutine(BackAfterDelay(2f));
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
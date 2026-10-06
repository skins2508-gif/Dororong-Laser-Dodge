using UnityEngine;
using UnityEngine.Events;

public class BeeHealth : MonoBehaviour
{
    public static BeeHealth I { get; private set; }

    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    [Header("Invincibility")]
    [SerializeField] private float invincibilityDuration = 1.5f;
    private float invincibilityTimer = 0f;
    public bool IsInvincible { get; private set; }

    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float blinkInterval = 0.1f;

    [Header("Events")]
    public UnityEvent<int> OnHealthChanged;
    public UnityEvent OnDeath;

    private void Awake()
    {
        I = this;
        currentHealth = maxHealth;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (IsInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0f)
            {
                IsInvincible = false;
                if (spriteRenderer != null)
                    spriteRenderer.color = Color.white;
            }
            else if (spriteRenderer != null)
            {
                float alpha = Mathf.PingPong(Time.time / blinkInterval, 1f);
                spriteRenderer.color = new Color(1f, 1f, 1f, alpha);
            }
        }
    }

    public void TakeDamage(int damage = 1)
    {
        if (IsInvincible || (GameFlow.I != null && GameFlow.I.IsGameOver)) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        OnHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            ActivateInvincibility(invincibilityDuration);
        }
    }

    public void Heal(int amount = 1)
    {
        if (GameFlow.I != null && GameFlow.I.IsGameOver) return;

        currentHealth += amount;
        OnHealthChanged?.Invoke(currentHealth);
    }

    public void ActivateInvincibility(float duration)
    {
        IsInvincible = true;
        invincibilityTimer = duration;
    }

    public void DeactivateInvincibility()
    {
        IsInvincible = false;
        invincibilityTimer = 0f;
        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;
    }

    private void Die()
    {
        OnDeath?.Invoke();
        if (GameFlow.I != null)
        {
            GameFlow.I.GameOver();
        }
    }

    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
}

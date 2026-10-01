using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BeeHealth : MonoBehaviour
{
    public static BeeHealth I { get; private set; }

    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    [Header("Damage Invincibility")]
    [SerializeField] private float invincibilityDuration = 0.75f;
    private float invincibilityTimer = 0f;
    [SerializeField] private float normalGaugeDuration = 4f;
    private float normalGaugeTimer = 0f;

    [Header("Safe Zone")]
    private int safeZoneCount = 0;

    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float blinkInterval = 0.1f;
    private SpriteRenderer shieldRenderer;
    private Transform shieldTransform;
    private Text invincibilityText;
    private Image gaugeBackground;
    private Image invincibilityGauge;
    private const float GaugeMaxWidth = 360f;
    private const int GaugeMaxValue = 100;

    [Header("Events")]
    public UnityEvent<int> OnHealthChanged;
    public UnityEvent OnDeath;

    // 피격 무적 시간이 남아 있거나
    // 꽃 안전지대 안에 있으면 무적 상태
    public bool IsInvincible
    {
        get
        {
            return invincibilityTimer > 0f || safeZoneCount > 0;
        }
    }

    private void Awake()
    {
        I = this;
        currentHealth = maxHealth;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        CreateShieldVisual();
        CreateInvincibilityTimer();
    }

    private void Update()
    {
        bool isGameplayRunning =
            GameFlow.I != null &&
            GameFlow.I.IsRunning &&
            !GameFlow.I.IsGameOver;

        // 게이지와 무적 시간은 실제 게임 진행 중에만 움직인다.
        if (isGameplayRunning && invincibilityTimer > 0f)
        {
            invincibilityTimer -= Time.deltaTime;

            if (invincibilityTimer < 0f)
            {
                invincibilityTimer = 0f;
            }
        }

        if (isGameplayRunning && safeZoneCount == 0 && invincibilityTimer <= 0f)
        {
            normalGaugeTimer += Time.deltaTime;
            if (normalGaugeTimer >= Mathf.Max(0.1f, normalGaugeDuration))
            {
                normalGaugeTimer = 0f;
                ActivateInvincibility(invincibilityDuration);
            }
        }

        UpdateVisual();
        UpdateInvincibilityTimer();
    }

    private void UpdateVisual()
    {
        if (spriteRenderer == null)
            return;

        // 꽃 안전지대 안에서는 노란빛 표시
        if (safeZoneCount > 0)
        {
            spriteRenderer.color =
                new Color(1f, 0.9f, 0.5f, 1f);

            if (shieldRenderer != null)
            {
                shieldRenderer.enabled = true;
                float pulse = 1f + Mathf.Sin(Time.time * 7f) * 0.1f;
                shieldTransform.localScale = Vector3.one * (1.45f * pulse);
                shieldRenderer.color = new Color(1f, 0.9f, 0.15f, 0.45f + Mathf.Sin(Time.time * 7f) * 0.12f);
                shieldTransform.Rotate(0f, 0f, 45f * Time.deltaTime);
            }

            return;
        }

        if (shieldRenderer != null) shieldRenderer.enabled = false;

        // 피격 무적 중에는 깜빡임
        if (invincibilityTimer > 0f)
        {
            float safeInterval =
                Mathf.Max(0.01f, blinkInterval);

            float alpha =
                Mathf.PingPong(
                    Time.time / safeInterval,
                    1f
                );

            spriteRenderer.color =
                new Color(1f, 1f, 1f, alpha);

            return;
        }

        // 일반 상태
        spriteRenderer.color = Color.white;
    }

    private void UpdateInvincibilityTimer()
    {
        if (invincibilityText == null || invincibilityGauge == null) return;

        if (safeZoneCount > 0)
        {
            invincibilityGauge.gameObject.SetActive(true);
            SetGaugeProgress(1f);
            gaugeBackground.color = new Color(1f, 0.82f, 0.05f, 1f);
            invincibilityText.text = "FLOWER SHIELD  MAX / MAX";
            invincibilityText.color = new Color(1f, 0.9f, 0.15f, 1f);
        }
        else if (invincibilityTimer > 0f)
        {
            invincibilityGauge.gameObject.SetActive(true);
            float progress = Mathf.Clamp01(
                1f - invincibilityTimer / Mathf.Max(0.01f, invincibilityDuration)
            );
            SetGaugeProgress(progress);
            gaugeBackground.color = new Color(0.01f, 0.01f, 0.01f, 0.95f);
            invincibilityText.text = "INVINCIBLE " + Mathf.RoundToInt(progress * GaugeMaxValue) + " / MAX";
            invincibilityText.color = Color.white;
        }
        else
        {
            invincibilityGauge.gameObject.SetActive(true);
            invincibilityGauge.color = new Color(1f, 0.82f, 0.05f, 1f);
            float progress = Mathf.Clamp01(
                normalGaugeTimer / Mathf.Max(0.1f, normalGaugeDuration)
            );
            SetGaugeProgress(progress);
            gaugeBackground.color = new Color(0.01f, 0.01f, 0.01f, 0.95f);
            invincibilityText.text = "NORMAL " + Mathf.RoundToInt(progress * GaugeMaxValue) + " / MAX";
            invincibilityText.color = Color.white;
        }

        if (safeZoneCount > 0 || invincibilityTimer > 0f)
        {
            invincibilityGauge.color = new Color(1f, 0.82f, 0.05f, 1f);
        }
    }

    private void SetGaugeProgress(float progress)
    {
        RectTransform gaugeRect = invincibilityGauge.rectTransform;
        Vector2 size = gaugeRect.sizeDelta;
        size.x = GaugeMaxWidth * Mathf.Clamp01(progress);
        gaugeRect.sizeDelta = size;
    }

    private void CreateInvincibilityTimer()
    {
        GameObject canvasObject = new GameObject("InvincibilityTimerCanvas");
        canvasObject.transform.SetParent(null, false);

        Canvas timerCanvas = canvasObject.AddComponent<Canvas>();
        timerCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        timerCanvas.sortingOrder = 30;
        CanvasScaler timerScaler = canvasObject.AddComponent<CanvasScaler>();
        timerScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        timerScaler.referenceResolution = new Vector2(1920f, 1080f);
        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();

        GameObject backgroundObject = new GameObject("NormalGauge");
        backgroundObject.transform.SetParent(canvasObject.transform, false);
        gaugeBackground = backgroundObject.AddComponent<Image>();
        gaugeBackground.color = new Color(0.02f, 0.02f, 0.02f, 0.9f);
        gaugeBackground.raycastTarget = false;
        RectTransform backgroundRect = gaugeBackground.rectTransform;
        backgroundRect.anchorMin = new Vector2(0.5f, 1f);
        backgroundRect.anchorMax = new Vector2(0.5f, 1f);
        backgroundRect.pivot = new Vector2(0.5f, 1f);
        backgroundRect.anchoredPosition = new Vector2(0f, -105f);
        backgroundRect.sizeDelta = new Vector2(360f, 24f);

        GameObject gaugeObject = new GameObject("InvincibleGauge");
        gaugeObject.transform.SetParent(canvasObject.transform, false);
        invincibilityGauge = gaugeObject.AddComponent<Image>();
        invincibilityGauge.color = new Color(1f, 0.82f, 0.05f, 1f);
        invincibilityGauge.raycastTarget = false;
        invincibilityGauge.type = Image.Type.Simple;
        RectTransform gaugeRect = invincibilityGauge.rectTransform;
        gaugeRect.anchorMin = new Vector2(0.5f, 1f);
        gaugeRect.anchorMax = new Vector2(0.5f, 1f);
        gaugeRect.pivot = new Vector2(0f, 1f);
        gaugeRect.anchoredPosition = new Vector2(-GaugeMaxWidth * 0.5f, -105f);
        gaugeRect.sizeDelta = new Vector2(0f, 24f);

        GameObject textObject = new GameObject("InvincibilityTimeText");
        textObject.transform.SetParent(canvasObject.transform, false);
        invincibilityText = textObject.AddComponent<Text>();
        invincibilityText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        invincibilityText.fontSize = 24;
        invincibilityText.fontStyle = FontStyle.Bold;
        invincibilityText.alignment = TextAnchor.MiddleCenter;
        invincibilityText.raycastTarget = false;
        invincibilityText.horizontalOverflow = HorizontalWrapMode.Overflow;
        RectTransform textRect = invincibilityText.rectTransform;
        textRect.anchorMin = new Vector2(0.5f, 1f);
        textRect.anchorMax = new Vector2(0.5f, 1f);
        textRect.pivot = new Vector2(0.5f, 1f);
        textRect.anchoredPosition = new Vector2(0f, -132f);
        textRect.sizeDelta = new Vector2(360f, 38f);
        invincibilityText.text = "NORMAL";
        invincibilityGauge.gameObject.SetActive(true);
    }

    private void CreateShieldVisual()
    {
        GameObject shield = new GameObject("InvincibleShield");
        shield.transform.SetParent(transform, false);
        shieldTransform = shield.transform;
        shieldTransform.localScale = Vector3.one * 1.45f;
        shieldRenderer = shield.AddComponent<SpriteRenderer>();
        shieldRenderer.sprite = CreateRingSprite();
        shieldRenderer.sortingOrder = spriteRenderer != null ? spriteRenderer.sortingOrder + 1 : 8;
        shieldRenderer.enabled = false;
    }

    private static Sprite CreateRingSprite()
    {
        const int size = 128;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.name = "InvincibleShieldRing";
        texture.filterMode = FilterMode.Bilinear;
        Vector2 center = Vector2.one * (size * 0.5f);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float distance = Vector2.Distance(new Vector2(x, y), center) / (size * 0.5f);
            float ring = Mathf.Clamp01(1f - Mathf.Abs(distance - 0.78f) / 0.1f);
            texture.SetPixel(x, y, new Color(1f, 1f, 1f, ring));
        }
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), Vector2.one * 0.5f, 100f);
    }

    public void TakeDamage(int damage = 1)
    {
        // 무적 상태거나 게임오버 상태라면 피해를 받지 않는다.
        if (IsInvincible)
            return;

        if (GameFlow.I != null && GameFlow.I.IsGameOver)
            return;

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
        if (GameFlow.I != null && GameFlow.I.IsGameOver)
            return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        OnHealthChanged?.Invoke(currentHealth);
    }

    // 피격 후 일정 시간 무적
    public void ActivateInvincibility(float duration)
    {
        invincibilityTimer =
            Mathf.Max(invincibilityTimer, duration);
        normalGaugeTimer = 0f;
    }

    // 피격 무적만 해제
    public void DeactivateInvincibility()
    {
        invincibilityTimer = 0f;
    }

    // 꽃 안전지대 진입
    public void EnterSafeZone()
    {
        safeZoneCount++;
    }

    // 꽃 안전지대 이탈
    public void ExitSafeZone()
    {
        safeZoneCount =
            Mathf.Max(0, safeZoneCount - 1);
    }

    private void Die()
    {
        OnDeath?.Invoke();

        if (GameFlow.I != null)
        {
            GameFlow.I.GameOver();
        }
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }
}

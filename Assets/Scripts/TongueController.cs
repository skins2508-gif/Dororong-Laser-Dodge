using System;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(EdgeCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class TongueController : MonoBehaviour
{
    [Header("Laser")]
    [SerializeField] private float laserHoldDuration = 0.12f;

    private LineRenderer lr;
    private LineRenderer coreLr;
    private EdgeCollider2D edge;
    private Rigidbody2D rb;

    private Vector2 start;
    private Vector2 dir;
    private float maxLen;
    private float extendSpeed;
    private float thickness;
    private Action onFinished;

    private float currentLen;
    private float activeTimer;
    private bool extending;
    private bool active;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        edge = GetComponent<EdgeCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        gameObject.tag = "Tongue";

        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.enabled = false;
        edge.enabled = false;

        // Initialize core LineRenderer
        Transform coreChild = transform.Find("LaserCore");
        if (coreChild == null)
        {
            GameObject go = new GameObject("LaserCore");
            go.transform.SetParent(transform, false);
            coreChild = go.transform;
        }
        coreLr = coreChild.GetComponent<LineRenderer>();
        if (coreLr == null)
        {
            coreLr = coreChild.gameObject.AddComponent<LineRenderer>();
        }
        coreLr.positionCount = 2;
        coreLr.useWorldSpace = true;
        coreLr.enabled = false;
        coreLr.sortingOrder = lr.sortingOrder + 1;
        coreLr.material = lr.material;
    }

    public void ShowWarningLine(Vector2 startWorld, Vector2 endWorld, float width, Color color)
    {
        if (coreLr != null)
        {
            coreLr.enabled = false;
        }

        lr.enabled = true;
        edge.enabled = false; // No damage during warning phase

        lr.startWidth = width;
        lr.endWidth = width;
        lr.SetPosition(0, startWorld);
        lr.SetPosition(1, endWorld);

        lr.startColor = color;
        lr.endColor = color;
    }

    public void InitAndFire(Vector2 startWorld, Vector2 dirNorm, float maxLength,
                            float extendSpd, float retractSpd, float width, Action onDone)
    {
        start = startWorld;
        dir = dirNorm.normalized;
        maxLen = Mathf.Max(0.1f, maxLength);
        extendSpeed = Mathf.Max(0.1f, extendSpd);
        thickness = width;
        onFinished = onDone;

        // Laser beam shoots instantly to its full length!
        currentLen = maxLen;
        activeTimer = Mathf.Max(0.01f, laserHoldDuration);
        extending = false; // No gradual extension
        active = true;

        lr.enabled = true;
        if (coreLr != null)
        {
            coreLr.enabled = true;
        }
        edge.enabled = true;

        ApplyRenderAndCollider();
    }

    private void Update()
    {
        if (!active || !GameFlow.I || !GameFlow.I.IsRunning || GameFlow.I.IsGameOver)
        {
            if (active && GameFlow.I && GameFlow.I.IsGameOver)
            {
                active = false;
                HideLaser();
            }
            return;
        }

        if (extending)
        {
            currentLen += extendSpeed * Time.deltaTime;
            if (currentLen >= maxLen)
            {
                currentLen = maxLen;
                extending = false;
            }

            ApplyRenderAndCollider();
            return;
        }

        activeTimer -= Time.deltaTime;
        ApplyRenderAndCollider(); // Continuously update render (flicker and fade out)

        if (activeTimer <= 0f)
        {
            active = false;
            HideLaser();
            onFinished?.Invoke();
        }
    }

    private void ApplyRenderAndCollider()
    {
        Vector2 end = start + dir * currentLen;

        // Calculate progress for fadeout
        float progress = 1f;
        if (!extending && laserHoldDuration > 0f)
        {
            progress = Mathf.Clamp01(activeTimer / laserHoldDuration);
        }

        // Add subtle high-frequency flicker to width to make it look like a high-energy ray
        float flicker = 1f + 0.08f * Mathf.Sin(Time.time * 60f);
        float currentThickness = thickness * progress * flicker;

        // Outer glow
        lr.startWidth = currentThickness;
        lr.endWidth = currentThickness;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        Color outerColorStart = new Color(1f, 0.08f, 0.16f, progress);
        Color outerColorEnd = new Color(1f, 0.42f, 0.18f, progress);
        lr.startColor = outerColorStart;
        lr.endColor = outerColorEnd;

        // Inner core
        if (coreLr != null)
        {
            coreLr.enabled = true;
            float coreThickness = currentThickness * 0.35f;
            coreLr.startWidth = coreThickness;
            coreLr.endWidth = coreThickness;
            coreLr.SetPosition(0, start);
            coreLr.SetPosition(1, end);

            Color coreColor = new Color(1f, 1f, 0.9f, progress);
            coreLr.startColor = coreColor;
            coreLr.endColor = coreColor;
        }

        // Edge collider (only updated during firing/fading, and has thickness 0 or is active)
        Vector2 p0 = transform.InverseTransformPoint(start);
        Vector2 p1 = transform.InverseTransformPoint(end);
        edge.points = new Vector2[] { p0, p1 };
        edge.isTrigger = true;
    }

    private void HideLaser()
    {
        lr.enabled = false;
        if (coreLr != null)
        {
            coreLr.enabled = false;
        }
        edge.enabled = false;
    }
}

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BeeController : MonoBehaviour
{
    public float moveSpeed = 6f;
    [SerializeField] private float screenMargin = 0.25f;

    private Rigidbody2D rb;
    private Vector2 input;
    private BeeHealth beeHealth;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        beeHealth = GetComponent<BeeHealth>();
    }

    private void Update()
    {
        if (!GameFlow.I || !GameFlow.I.IsRunning || GameFlow.I.IsGameOver)
        {
            input = Vector2.zero;
            return;
        }

        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        input = new Vector2(x, y).normalized;
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = input * moveSpeed;

        if (CameraBounds2D.I != null)
        {
            Vector2 clampedPosition = CameraBounds2D.I.ClampInside(rb.position, screenMargin);
            rb.position = clampedPosition;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!GameFlow.I || GameFlow.I.IsGameOver) return;

        if (other.CompareTag("Tongue"))
        {
            if (beeHealth != null)
            {
                if (!beeHealth.IsInvincible)
                {
                    beeHealth.TakeDamage(1);
                }
            }
            else
            {
                GameFlow.I.GameOver();
            }
        }
    }
}

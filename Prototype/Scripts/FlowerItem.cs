using UnityEngine;

public class FlowerItem : MonoBehaviour
{
    [Header("Safe Zone Visual")]
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.2f;
    [SerializeField] private Color safeColor = new Color(1f, 1f, 0.5f, 0.3f); // ���� �����

    [Header("Visual Effects")]
    [SerializeField] private SpriteRenderer auraRenderer; // ���� ���� ǥ�ÿ� (���� ����)

    private Vector3 startPos;
    private BeeController currentBee;

    private void Start()
    {
        startPos = transform.position;

        // ���� ���� ǥ�� (���� ���� ��) ����
        if (auraRenderer != null)
        {
            auraRenderer.color = safeColor;
        }
    }

    private void Update()
    {
        // ���� ���Ʒ��� ��¦ ���ٴϴ� ȿ��
        float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // ���� �� ���� �ִ� ���� ��� ����
        if (other.CompareTag("Player"))
        {
            var beeHealth = other.GetComponent<BeeHealth>();
            if (beeHealth != null && !beeHealth.IsInvincible)
            {
                beeHealth.ActivateInvincibility(999f); // �ſ� �� �ð����� ����
                currentBee = other.GetComponent<BeeController>();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // ���� �ɿ��� ����� ���� ����
        if (other.CompareTag("Player"))
        {
            var beeHealth = other.GetComponent<BeeHealth>();
            if (beeHealth != null)
            {
                beeHealth.DeactivateInvincibility();
            }
            currentBee = null;
        }
    }
}
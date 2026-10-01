using UnityEngine;

public class HoneyItem : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private int healAmount = 1;
    [SerializeField] private int scoreAmount = 100;
    [SerializeField] private float rotationSpeed = 90f;

    [Header("Visual")]
    [SerializeField] private GameObject pickupEffect;
    private bool collected;

    private void Update()
    {
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryCollect(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryCollect(other);
    }

    private void TryCollect(Collider2D other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;

        BeeHealth beeHealth = other.GetComponent<BeeHealth>();
        if (beeHealth == null) return;

        collected = true;

        beeHealth.Heal(healAmount);

        if (ScoreManager.I != null)
        {
            ScoreManager.I.AddScore(scoreAmount);
        }

        if (pickupEffect != null)
        {
            Instantiate(pickupEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}

using UnityEngine;

public class HoneyItem : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private int healAmount = 1;
    [SerializeField] private float rotationSpeed = 90f;

    [Header("Visual")]
    [SerializeField] private GameObject pickupEffect;

    private void Update()
    {
        // 아이템 회전 애니메이션
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var beeHealth = other.GetComponent<BeeHealth>();
            if (beeHealth != null)
            {
                beeHealth.Heal(healAmount);

                // 파티클 효과 (선택 사항)
                if (pickupEffect != null)
                {
                    Instantiate(pickupEffect, transform.position, Quaternion.identity);
                }

                // 아이템 제거
                Destroy(gameObject);
            }
        }
    }
}
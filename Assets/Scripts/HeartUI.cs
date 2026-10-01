using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [Header("HP Images")]
    [SerializeField] private Image[] hpImages = new Image[3];

    private BeeHealth boundBeeHealth;

    private void Start()
    {
        boundBeeHealth = BeeHealth.I;

        if (boundBeeHealth != null)
        {
            UpdateHearts(boundBeeHealth.GetCurrentHealth());
            boundBeeHealth.OnHealthChanged.AddListener(UpdateHearts);
        }
    }

    private void UpdateHearts(int currentHealth)
    {
        for (int i = 0; i < hpImages.Length; i++)
        {
            if (hpImages[i] != null)
            {
                hpImages[i].gameObject.SetActive(i < currentHealth);
            }
        }
    }

    private void OnDestroy()
    {
        if (boundBeeHealth != null)
        {
            boundBeeHealth.OnHealthChanged.RemoveListener(UpdateHearts);
        }
    }
}

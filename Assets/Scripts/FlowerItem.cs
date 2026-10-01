using UnityEngine;

public class FlowerItem : MonoBehaviour
{
    [Header("Safe Zone Visual")]

    // 꽃이 위아래로 움직이는 속도
    [SerializeField] private float bobSpeed = 2f;

    // 꽃이 위아래로 움직이는 높이
    [SerializeField] private float bobHeight = 0.2f;

    // 안전지대 오라 색상
    [SerializeField]
    private Color safeColor =
        new Color(1f, 1f, 0.5f, 0.3f);

    [Header("Visual Effects")]

    // 안전지대 범위를 보여주는 SpriteRenderer
    [SerializeField] private SpriteRenderer auraRenderer;

    // 꽃의 처음 위치
    private Vector3 startPos;

    // 현재 안전지대 안에 들어온 벌의 체력 컴포넌트
    private BeeHealth currentBeeHealth;

    private void Start()
    {
        // 꽃의 시작 위치 저장
        startPos = transform.position;

        // 오라 색상 적용
        if (auraRenderer != null)
        {
            auraRenderer.color = safeColor;
        }
    }

    private void Update()
    {
        // 꽃이 위아래로 부드럽게 움직이는 효과
        float newY =
            startPos.y +
            Mathf.Sin(Time.time * bobSpeed) * bobHeight;

        transform.position = new Vector3(
            transform.position.x,
            newY,
            transform.position.z
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Player 태그만 처리
        if (!other.CompareTag("Player"))
            return;

        BeeHealth beeHealth =
            other.GetComponent<BeeHealth>();

        if (beeHealth == null)
            return;

        // 꽃의 안전지대에 들어왔으므로 무적 적용
        beeHealth.EnterSafeZone();

        // 꽃이 사라질 경우를 대비해 저장
        currentBeeHealth = beeHealth;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Player 태그만 처리
        if (!other.CompareTag("Player"))
            return;

        BeeHealth beeHealth =
            other.GetComponent<BeeHealth>();

        if (beeHealth == null)
            return;

        // 꽃의 안전지대에서 나왔으므로 무적 해제
        beeHealth.ExitSafeZone();

        if (currentBeeHealth == beeHealth)
        {
            currentBeeHealth = null;
        }
    }

    private void OnDisable()
    {
        // 플레이어가 꽃 안에 있는 상태에서
        // 꽃이 시간 만료로 사라질 때 무적을 반드시 해제
        if (currentBeeHealth != null)
        {
            currentBeeHealth.ExitSafeZone();
            currentBeeHealth = null;
        }
    }
}
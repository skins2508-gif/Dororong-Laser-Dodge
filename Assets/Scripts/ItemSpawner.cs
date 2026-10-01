using System.Collections;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Spawn Prefabs")]
    [SerializeField] private GameObject honeyPrefab;
    [SerializeField] private GameObject flowerPrefab;

    [Header("Honey Spawn")]
    [SerializeField] private float honeySpawnInterval = 3f;
    [SerializeField] private float honeySpawnRandomRange = 0.75f;

    [Header("Flower Spawn")]
    [SerializeField] private float flowerSpawnInterval = 5f;
    [SerializeField] private float flowerSpawnRandomRange = 0f;
    [SerializeField] private float flowerLifetime = 2f;
    [SerializeField] private float flowerTimerSidePadding = 5f;

    [Header("Spawn Area")]
    [SerializeField] private float spawnMargin = 0.5f;

    private Coroutine honeyCoroutine;
    private Coroutine flowerCoroutine;
    private GameObject activeFlower;

    private void Start()
    {
        flowerCoroutine = StartCoroutine(SpawnFlowerRoutine());
    }

    private IEnumerator SpawnHoneyRoutine()
    {
        // 게임이 시작될 때까지 대기
        while (GameFlow.I == null || !GameFlow.I.IsRunning)
        {
            yield return null;
        }

        // 게임 시작 직후 꿀 생성
        SpawnHoney();

        while (!GameFlow.I.IsGameOver)
        {
            float waitTime = honeySpawnInterval +
                             Random.Range(
                                 -honeySpawnRandomRange,
                                 honeySpawnRandomRange
                             );

            waitTime = Mathf.Max(0.1f, waitTime);

            yield return new WaitForSeconds(waitTime);

            if (!GameFlow.I.IsGameOver)
            {
                SpawnHoney();
            }
        }
    }

    private IEnumerator SpawnFlowerRoutine()
    {
        // 게임이 시작될 때까지 대기
        while (GameFlow.I == null || !GameFlow.I.IsRunning)
        {
            yield return null;
        }

        while (!GameFlow.I.IsGameOver)
        {
            float waitTime = flowerSpawnInterval +
                             Random.Range(
                                 -flowerSpawnRandomRange,
                                 flowerSpawnRandomRange
                             );

            waitTime = Mathf.Max(0.1f, waitTime);

            yield return new WaitForSeconds(waitTime);

            if (!GameFlow.I.IsGameOver)
            {
                SpawnFlower();
            }
        }
    }

    private void SpawnHoney()
    {
        if (honeyPrefab == null)
        {
            Debug.LogError(
                "[ItemSpawner] Honey Prefab이 연결되지 않았습니다."
            );
            return;
        }

        Vector2 spawnPosition = GetRandomPositionInScreen();

        Instantiate(
            honeyPrefab,
            spawnPosition,
            Quaternion.identity
        );

        Debug.Log(
            "[ItemSpawner] 꿀 생성 위치: " + spawnPosition
        );
    }

    private void SpawnFlower()
    {
        if (flowerPrefab == null)
        {
            Debug.LogError(
                "[ItemSpawner] Flower Prefab이 연결되지 않았습니다."
            );
            return;
        }

        Vector2 spawnPosition = GetPositionNextToTimer();

        if (activeFlower != null)
        {
            Destroy(activeFlower);
        }

        activeFlower = Instantiate(
            flowerPrefab,
            spawnPosition,
            Quaternion.identity
        );

        Debug.Log(
            "[ItemSpawner] 꽃 생성 위치: " + spawnPosition
        );

        if (flowerLifetime > 0f)
        {
            Destroy(activeFlower, flowerLifetime);
        }
    }

    private Vector2 GetPositionNextToTimer()
    {
        Camera mainCamera = Camera.main;
        RectTransform timerRect =
            GameTimer.I != null ? GameTimer.I.TimerRectTransform : null;

        if (mainCamera == null || timerRect == null)
        {
            Debug.LogWarning(
                "[ItemSpawner] 시간 UI를 찾지 못해 꽃을 임의 위치에 생성합니다."
            );
            return GetRandomPositionInScreen();
        }

        Vector3[] corners = new Vector3[4];
        timerRect.GetWorldCorners(corners);

        Vector2 upperRight = RectTransformUtility.WorldToScreenPoint(
            null,
            corners[2]
        );
        Vector2 lowerRight = RectTransformUtility.WorldToScreenPoint(
            null,
            corners[3]
        );
        Vector2 screenPosition = new Vector2(
            upperRight.x + flowerTimerSidePadding,
            (upperRight.y + lowerRight.y) * 0.5f
        );

        float distanceFromCamera =
            Mathf.Abs(mainCamera.transform.position.z);
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(
            new Vector3(
                screenPosition.x,
                screenPosition.y,
                distanceFromCamera
            )
        );

        return new Vector2(worldPosition.x, worldPosition.y);
    }

    private Vector2 GetRandomPositionInScreen()
    {
        if (CameraBounds2D.I == null)
        {
            Debug.LogError(
                "[ItemSpawner] CameraBounds2D가 없습니다."
            );

            return Vector2.zero;
        }

        Rect screenRect =
            CameraBounds2D.I.GetWorldRect(0f);

        float minimumX =
            screenRect.xMin + spawnMargin;

        float maximumX =
            screenRect.xMax - spawnMargin;

        float minimumY =
            screenRect.yMin + spawnMargin;

        float maximumY =
            screenRect.yMax - spawnMargin;

        // Margin 값이 화면 크기보다 너무 큰 경우 방지
        if (minimumX > maximumX)
        {
            minimumX = screenRect.xMin;
            maximumX = screenRect.xMax;
        }

        if (minimumY > maximumY)
        {
            minimumY = screenRect.yMin;
            maximumY = screenRect.yMax;
        }

        return new Vector2(
            Random.Range(minimumX, maximumX),
            Random.Range(minimumY, maximumY)
        );
    }
}

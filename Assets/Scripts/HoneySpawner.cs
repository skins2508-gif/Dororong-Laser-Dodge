using System.Collections;
using UnityEngine;

public class HoneySpawner : MonoBehaviour
{
    [SerializeField] private GameObject honeyPrefab;
    [SerializeField] private float spawnInterval = 14f;
    [SerializeField] private float randomRange = 2.5f;
    [SerializeField] private float spawnMargin = 0.5f;

    private IEnumerator Start()
    {
        while (GameFlow.I == null || !GameFlow.I.IsRunning) yield return null;

        while (GameFlow.I != null && !GameFlow.I.IsGameOver)
        {
            float delay = Mathf.Max(1f, spawnInterval + Random.Range(-randomRange, randomRange));
            yield return new WaitForSeconds(delay);
            if (GameFlow.I.IsRunning && !GameFlow.I.IsGameOver) SpawnHoney();
        }
    }

    private void SpawnHoney()
    {
        if (honeyPrefab == null || CameraBounds2D.I == null) return;
        Rect area = CameraBounds2D.I.GetWorldRect(0f);
        Vector2 position = new Vector2(
            Random.Range(area.xMin + spawnMargin, area.xMax - spawnMargin),
            Random.Range(area.yMin + spawnMargin, area.yMax - spawnMargin)
        );
        Instantiate(honeyPrefab, position, Quaternion.identity);
    }
}

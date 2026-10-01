using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrogWaveManager : MonoBehaviour
{
    [Header("Wave Count Settings")]
    [Tooltip("ù ���̺� ������ ��")]
    public int startFrogs = 3;

    [Tooltip("���̺긶�� ������ ������ ��(1�� ����)")]
    public int frogIncrementPerWave = 1;

    [Header("Timing")]
    [Tooltip("���� ���� �� ù ���̺���� ����")]
    public float waveStartDelay = 0.2f;

    [Tooltip("���̺�� ���̺� ���� �޽�(����)")]
    public float interWaveDelay = 0.3f;

    [Header("Enter (�ۡ�� �̵�)")]
    [Tooltip("���� ���� ��� �ð�(��û: 0.3�� ����)")]
    public float waitBeforeEnter = 0.3f;

    [Tooltip("ȭ�� �ۿ��� �׵θ� �������� �����̵� ���ϴ� �� �ɸ��� �ð�")]
    public float enterDuration = 0.25f;

    [Tooltip("ȭ�� �׵θ� �������� ���� ���� ��ġ ����(���� ����)")]
    public float innerMargin = 0.5f;

    [Tooltip("���� ���� �� �� �߻���� �߰� ����(0�̸� ��� �߻�)")]
    public float fireDelayAfterEnter = 0.0f;

    [Header("Spawn")]
    public GameObject frogPrefab;

    [Tooltip("ȭ�� ��(�����ڸ� �ٱ�) ���� ���� �Ÿ�")]
    public float edgeOffset = 1.0f;

    [Tooltip("������ �� �ּ� ����(��ħ ����)")]
    public float minFrogSpacing = 2.0f;

    [Tooltip("������ �� ���� ��ġ �õ� Ƚ��(���� ���� ������)")]
    public int placeAttemptsPerFrog = 30;

    private readonly List<FrogController> aliveFrogs = new List<FrogController>();

    private void Start()
    {
        StartCoroutine(RunWaves());
    }

    private IEnumerator RunWaves()
    {
        // ���� ���� ���
        while (!GameFlow.I || !GameFlow.I.IsRunning) yield return null;
        yield return new WaitForSeconds(waveStartDelay);

        int waveIndex = 0;

        // === ���� ���̺� ���� ===
        while (true)
        {
            if (GameFlow.I.IsGameOver) yield break;

            int countThisWave = Mathf.Max(0, startFrogs + frogIncrementPerWave * waveIndex);
            aliveFrogs.Clear();

            // 1) ������ ����(ī�޶� 4�� '��' + ���� ����)
            Rect rect = CameraBounds2D.I.GetWorldRect(0f);
            List<Vector2> placed = new List<Vector2>();

            for (int i = 0; i < countThisWave; i++)
            {
                if (TryPlaceFrog(rect, edgeOffset, minFrogSpacing, placeAttemptsPerFrog, placed, out Vector2 pos))
                {
                    var frog = Instantiate(frogPrefab, pos, Quaternion.identity).GetComponent<FrogController>();
                    aliveFrogs.Add(frog);
                    placed.Add(pos);

                    // 2) �� ������: 0.3�� ��� �� ȭ�� �������� �����̵� �� �� �߻� ������
                    frog.PrepareEnterAndFire(waitBeforeEnter, enterDuration, innerMargin, fireDelayAfterEnter);
                }
                else
                {
                    Debug.LogWarning("[FrogWave] ��ġ ��ġ ���� -> minFrogSpacing�� �Ǵ� placeAttemptsPerFrog�� ���� ����");
                }
            }

            // 3) ��� ������ ���ŵ� ������ ���(= �� �պ� �Ϸ� �� FrogController�� �ڸ�)
            while (aliveFrogs.Exists(f => f != null))
            {
                if (GameFlow.I.IsGameOver) yield break;
                yield return null;
            }

            // 4) ���̺� ���� ����(����)
            if (interWaveDelay > 0f)
                yield return new WaitForSeconds(interWaveDelay);

            waveIndex++; // ���� ���̺�(������ +1)
        }
    }

    // --- ��ġ ��ƿ --- //
    private bool TryPlaceFrog(Rect camRect, float offset, float minSpacing, int attempts, List<Vector2> placed, out Vector2 pos)
    {
        // 4���� ����� Ȱ���ϱ� ���� �õ����� ���� �� ���� (�ٱ�������)
        for (int a = 0; a < attempts; a++)
        {
            int side = Random.Range(0, 4); // 0:Left 1:Right 2:Top 3:Bottom
            pos = RandomPointOnSideOutside(camRect, side, offset);

            bool ok = true;
            for (int i = 0; i < placed.Count; i++)
            {
                if (Vector2.Distance(placed[i], pos) < minSpacing)
                {
                    ok = false;
                    break;
                }
            }
            if (ok) return true;
        }

        pos = Vector2.zero;
        return false;
    }

    private Vector2 RandomPointOnSideOutside(Rect r, int side, float offset)
    {
        switch (side)
        {
            case 0: // Left(���� �ٱ�)
                return new Vector2(r.xMin - offset, Random.Range(r.yMin, r.yMax));
            case 1: // Right(������ �ٱ�)
                return new Vector2(r.xMax + offset, Random.Range(r.yMin, r.yMax));
            case 2: // Top(���� �ٱ�)
                return new Vector2(Random.Range(r.xMin, r.xMax), r.yMax + offset);
            default: // Bottom(�Ʒ��� �ٱ�)
                return new Vector2(Random.Range(r.xMin, r.xMax), r.yMin - offset);
        }
    }
}
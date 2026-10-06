using System.Collections;
using UnityEngine;

public class FrogController : MonoBehaviour
{
    [Header("Refs")]
    public TongueController tongue;   // ������ �ڽĿ� ���� TongueController
    public Transform mouth;           // ������ �� ��ġ(������ �ڱ� Transform)

    [Header("Tongue")]
    public float extendSpeed = 18f;
    public float retractSpeed = 24f;
    public float tongueThickness = 0.18f;
    public float extraPastScreen = 1.5f; // ȭ�� �ݴ����� �Ѿ �߰� ����

    private bool fired = false;
    private Coroutine sequenceCo;

    private void Awake()
    {
        if (!tongue) tongue = GetComponentInChildren<TongueController>();
    }

    /// <summary>
    /// ���� ��: waitBeforeEnter ���� ��� �� ȭ�� ����(innerMargin)���� slide-in(enterDuration) �� fireDelayAfterEnter ��� �� �� �߻� �� ���� �� �ڸ�
    /// </summary>
    public void PrepareEnterAndFire(float waitBeforeEnter, float enterDuration, float innerMargin, float fireDelayAfterEnter)
    {
        if (sequenceCo != null) StopCoroutine(sequenceCo);
        sequenceCo = StartCoroutine(EnterAndFireSequence(waitBeforeEnter, enterDuration, innerMargin, fireDelayAfterEnter));
    }

    private IEnumerator EnterAndFireSequence(float waitBeforeEnter, float enterDuration, float innerMargin, float fireDelayAfterEnter)
    {
        // 0) ���� ���
        float t = 0f;
        while (t < waitBeforeEnter)
        {
            if (!GameFlow.I || GameFlow.I.IsGameOver) yield break;
            t += Time.deltaTime;
            yield return null;
        }

        // 1) ȭ�� ���� ��ǥ ���� ���(���� ��ġ�� ��� �� '��'���� ����, �� ���� �������� ��¦)
        Vector3 startPos = transform.position;
        Vector3 targetPos = ComputeInnerEdgeTarget(startPos, innerMargin);

        // 2) �����̵� ��
        float dur = Mathf.Max(0.01f, enterDuration);
        t = 0f;
        while (t < dur)
        {
            if (!GameFlow.I || GameFlow.I.IsGameOver) yield break;
            float u = t / dur;
            transform.position = Vector3.Lerp(startPos, targetPos, u);
            t += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPos;

        // 3) ���� ���� �� �߰� ����(�ִٸ�)
        t = 0f;
        while (t < fireDelayAfterEnter)
        {
            if (!GameFlow.I || GameFlow.I.IsGameOver) yield break;
            t += Time.deltaTime;
            yield return null;
        }

        // 4) �� �߻�
        FireTongue();
    }

    /// <summary>
    /// ȭ�� ���� ���(inset by innerMargin) ���� �� ��ǥ ��ġ�� ���
    /// (��/��/��/�� �ٱ����� ���Դٰ� �����ϰ�, ���� ����� �ٱ� ���� ����)
    /// </summary>
    private Vector3 ComputeInnerEdgeTarget(Vector3 fromWorld, float innerMargin)
    {
        Rect r = CameraBounds2D.I.GetWorldRect(0f);
        // ���� ���(ī�޶� ��迡�� margin��ŭ ����)
        float xMinIn = r.xMin + innerMargin;
        float xMaxIn = r.xMax - innerMargin;
        float yMinIn = r.yMin + innerMargin;
        float yMaxIn = r.yMax - innerMargin;

        // �ٱ��� �Ÿ� ����: � �� �ٱ����� ����
        float dxLeft = Mathf.Abs(fromWorld.x - (r.xMin - 0.0001f));
        float dxRight = Mathf.Abs(fromWorld.x - (r.xMax + 0.0001f));
        float dyBot = Mathf.Abs(fromWorld.y - (r.yMin - 0.0001f));
        float dyTop = Mathf.Abs(fromWorld.y - (r.yMax + 0.0001f));

        // ���� �����(Ȥ�� ��Ȯ��) �ٱ� �� ����
        // �켱����: ������ �ٱ����� üũ �� �������̸� �Ÿ� ���� ��
        bool isLeft = fromWorld.x < r.xMin;
        bool isRight = fromWorld.x > r.xMax;
        bool isBottom = fromWorld.y < r.yMin;
        bool isTop = fromWorld.y > r.yMax;

        // �⺻��: ���� ����� ������ ����
        float best = float.MaxValue;
        Vector3 target = fromWorld;

        if (isLeft)
        {
            float yClamped = Mathf.Clamp(fromWorld.y, yMinIn, yMaxIn);
            target = new Vector3(xMinIn, yClamped, fromWorld.z);
            best = dxLeft;
        }
        if (isRight && dxRight < best)
        {
            float yClamped = Mathf.Clamp(fromWorld.y, yMinIn, yMaxIn);
            target = new Vector3(xMaxIn, yClamped, fromWorld.z);
            best = dxRight;
        }
        if (isTop && dyTop < best)
        {
            float xClamped = Mathf.Clamp(fromWorld.x, xMinIn, xMaxIn);
            target = new Vector3(xClamped, yMaxIn, fromWorld.z);
            best = dyTop;
        }
        if (isBottom && dyBot < best)
        {
            float xClamped = Mathf.Clamp(fromWorld.x, xMinIn, xMaxIn);
            target = new Vector3(xClamped, yMinIn, fromWorld.z);
            best = dyBot;
        }

        // Ȥ�� ������ ���ʿ� �ִ� ���(����)�� ���� ����� �׵θ������� ����
        if (!isLeft && !isRight && !isTop && !isBottom)
        {
            // ȭ�� �����̶��, �� �𼭸����� �Ÿ� �� �� ���� ����� ���� ����
            float toLeft = Mathf.Abs(fromWorld.x - xMinIn);
            float toRight = Mathf.Abs(fromWorld.x - xMaxIn);
            float toTop = Mathf.Abs(fromWorld.y - yMaxIn);
            float toBot = Mathf.Abs(fromWorld.y - yMinIn);

            float minEdge = Mathf.Min(toLeft, toRight, toTop, toBot);
            if (minEdge == toLeft) target = new Vector3(xMinIn, Mathf.Clamp(fromWorld.y, yMinIn, yMaxIn), fromWorld.z);
            else if (minEdge == toRight) target = new Vector3(xMaxIn, Mathf.Clamp(fromWorld.y, yMinIn, yMaxIn), fromWorld.z);
            else if (minEdge == toTop) target = new Vector3(Mathf.Clamp(fromWorld.x, xMinIn, xMaxIn), yMaxIn, fromWorld.z);
            else target = new Vector3(Mathf.Clamp(fromWorld.x, xMinIn, xMaxIn), yMinIn, fromWorld.z);
        }

        return target;
    }

    public void FireTongue()
    {
        if (fired || !tongue) return;
        fired = true;

        Vector3 start = (mouth ? mouth.position : transform.position);
        Vector2 dir = ComputeFireDirection(start);
        float maxLen = ComputeMaxLengthPastScreen(start, dir, extraPastScreen);

        tongue.InitAndFire(start, dir, maxLen, extendSpeed, retractSpeed, tongueThickness, OnTongueFinished);
    }

    private Vector2 ComputeFireDirection(Vector3 start)
    {
        var bee = FindAnyObjectByType<BeeController>();
        if (!bee)
            return (Vector2)(CameraBounds2D.I.GetWorldRect().center - (Vector2)start).normalized;

        Vector2 dir = ((Vector2)bee.transform.position - (Vector2)start).normalized;
        if (dir.sqrMagnitude < 0.001f) dir = Vector2.down;
        return dir;
    }

    private float ComputeMaxLengthPastScreen(Vector3 start, Vector2 dir, float extra)
    {
        var rect = CameraBounds2D.I.GetWorldRect(0f);
        float t = RayRectExitDistance((Vector2)start, dir, rect);
        return t + extra;
    }

    // ����(����, ����)�� rect�� �������� �������� �Ÿ�(���� ����)
    private float RayRectExitDistance(Vector2 origin, Vector2 dir, Rect r)
    {
        float t = Mathf.Infinity;
        if (dir.x > 0) t = Mathf.Min(t, (r.xMax - origin.x) / dir.x);
        if (dir.x < 0) t = Mathf.Min(t, (r.xMin - origin.x) / dir.x);
        if (dir.y > 0) t = Mathf.Min(t, (r.yMax - origin.y) / dir.y);
        if (dir.y < 0) t = Mathf.Min(t, (r.yMin - origin.y) / dir.y);
        if (float.IsInfinity(t) || t < 0f) t = 5f; // ����
        return t;
    }

    private void OnTongueFinished()
    {
        Destroy(gameObject); // �� �պ� �Ϸ� �� ������ ����
    }
}
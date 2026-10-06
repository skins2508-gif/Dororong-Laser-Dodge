using System;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(EdgeCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class TongueController : MonoBehaviour
{
    private LineRenderer lr;
    private EdgeCollider2D edge;
    private Rigidbody2D rb;

    private Vector2 start;
    private Vector2 dir;
    private float maxLen;
    private float extendSpeed;
    private float retractSpeed;
    private float thickness;
    private Action onFinished;

    private float curLen;
    private bool extending;
    private bool active;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        edge = GetComponent<EdgeCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        // 충돌 태그
        gameObject.tag = "Tongue";

        // LineRenderer 기본 세팅
        lr.positionCount = 2;
        lr.useWorldSpace = true;
    }

    public void InitAndFire(Vector2 startWorld, Vector2 dirNorm, float maxLength,
                            float extendSpd, float retractSpd, float width, Action onDone)
    {
        start = startWorld;
        dir = dirNorm.normalized;
        maxLen = Mathf.Max(0.1f, maxLength);
        extendSpeed = extendSpd;
        retractSpeed = retractSpd;
        thickness = width;
        onFinished = onDone;

        curLen = 0f;
        extending = true;
        active = true;

        // 초기 라인/콜라이더
        ApplyRenderAndCollider();
    }

    private void Update()
    {
        if (!active || !GameFlow.I || !GameFlow.I.IsRunning || GameFlow.I.IsGameOver)
        {
            if (active && GameFlow.I && GameFlow.I.IsGameOver) // 게임오버 시 즉시 정지
            {
                active = false;
            }
            return;
        }

        if (extending)
        {
            curLen += extendSpeed * Time.deltaTime;
            if (curLen >= maxLen)
            {
                curLen = maxLen;
                extending = false; // 최대 길이 도달 후 복귀
            }
        }
        else
        {
            curLen -= retractSpeed * Time.deltaTime;
            if (curLen <= 0f)
            {
                curLen = 0f;
                active = false;
                onFinished?.Invoke();
            }
        }

        ApplyRenderAndCollider();
    }

    private void ApplyRenderAndCollider()
    {
        Vector2 end = start + dir * curLen;

        lr.startWidth = thickness;
        lr.endWidth = thickness;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        // EdgeCollider2D는 로컬좌표 사용 → 월드→로컬 변환
        Vector2 p0 = transform.InverseTransformPoint(start);
        Vector2 p1 = transform.InverseTransformPoint(end);
        edge.points = new Vector2[] { p0, p1 };
        edge.isTrigger = true;
    }
}

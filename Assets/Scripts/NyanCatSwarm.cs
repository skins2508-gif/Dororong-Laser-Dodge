using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NyanCatSwarm : MonoBehaviour
{
    [Header("Media")]
    [SerializeField] private Texture2D[] nyanFrames;
    [SerializeField, Min(1f)] private float animationFps = 10f;
    [SerializeField] private AudioClip nyanSound;

    [Header("Pass Settings")]
    [SerializeField] private float firstSpawnDelay = 0.5f;
    [SerializeField] private float startInterval = 4f;
    [SerializeField] private float minimumInterval = 1.5f;
    [SerializeField] private float intervalRampSeconds = 90f;
    [SerializeField] private int maximumCats = 4;
    [SerializeField] private Vector2 speedRange = new Vector2(260f, 380f);
    [SerializeField] private float verticalSpread = 170f;
    [SerializeField, Range(0.3f, 1f)] private float hitboxScale = 0.55f;

    private readonly List<CatPass> cats = new List<CatPass>();
    private Canvas canvas;
    private AudioSource music;
    private float elapsed;
    private float spawnTimer;

    private sealed class CatPass
    {
        public GameObject gameObject;
        public RawImage image;
        public RectTransform rect;
        public Vector2 velocity;
        public float animationTime;
        public int frameIndex;
        public bool hitPlayer;
    }

    private void Awake()
    {
        if (nyanSound == null)
            nyanSound = Resources.Load<AudioClip>("NyanCat");

        GameObject canvasObject = new GameObject("NyanCatCanvas");
        canvasObject.transform.SetParent(transform, false);
        canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 25;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        music = gameObject.AddComponent<AudioSource>();
        music.clip = nyanSound;
        music.loop = true;
        music.playOnAwake = false;
        music.volume = 0.55f;

        if (nyanFrames == null || nyanFrames.Length == 0)
        {
            nyanFrames = Resources.LoadAll<Texture2D>("NyanCatFrames");
            System.Array.Sort(nyanFrames, (a, b) => string.CompareOrdinal(a.name, b.name));
        }

        if (nyanFrames == null || nyanFrames.Length == 0)
            Debug.LogError("[NyanCatSwarm] NyanCat animation frames were not found.");
    }

    private void Start()
    {
        if (GameFlow.I == null)
        {
            Debug.LogError("[NyanCatSwarm] GameFlow was not found.");
            return;
        }

        GameFlow.I.RunStarted += BeginRun;
        GameFlow.I.GameEnded += EndRun;
        canvas.enabled = GameFlow.I.IsRunning;
        if (GameFlow.I.IsRunning)
            BeginRun();
    }

    private void Update()
    {
        if (GameFlow.I == null || !GameFlow.I.IsRunning)
            return;

        elapsed += Time.deltaTime;
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f && cats.Count < maximumCats)
        {
            SpawnCat();
            float ramp = Mathf.Clamp01(elapsed / Mathf.Max(1f, intervalRampSeconds));
            spawnTimer = Mathf.Lerp(startInterval, minimumInterval, ramp);
        }

        Vector2 beePosition = GetBeeCanvasPosition();
        float screenLimit = GetCanvasHalfWidth() + 260f;

        for (int i = cats.Count - 1; i >= 0; i--)
        {
            CatPass cat = cats[i];

            cat.rect.anchoredPosition += cat.velocity * Time.deltaTime;
            UpdateAnimation(cat);
            CheckPlayerCollision(cat, beePosition);

            if (Mathf.Abs(cat.rect.anchoredPosition.x) > screenLimit)
                RemoveCat(i);
        }
    }

    private void SpawnCat()
    {
        Vector2 beePosition = GetBeeCanvasPosition();
        float halfWidth = GetCanvasHalfWidth();
        bool fromLeft = Random.value < 0.5f;

        GameObject catObject = new GameObject("NyanCatPass");
        catObject.transform.SetParent(canvas.transform, false);
        RawImage image = catObject.AddComponent<RawImage>();
        image.raycastTarget = false;
        image.color = Color.white;
        image.texture = nyanFrames != null && nyanFrames.Length > 0 ? nyanFrames[0] : null;
        image.enabled = image.texture != null;

        RectTransform rect = image.rectTransform;
        rect.sizeDelta = new Vector2(300f, 180f);
        rect.anchoredPosition = new Vector2(
            fromLeft ? -halfWidth - 180f : halfWidth + 180f,
            beePosition.y + Random.Range(-verticalSpread, verticalSpread));

        float speed = Random.Range(speedRange.x, speedRange.y) * (fromLeft ? 1f : -1f);
        rect.localScale = new Vector3(fromLeft ? 1f : -1f, 1f, 1f);
        cats.Add(new CatPass
        {
            gameObject = catObject,
            image = image,
            rect = rect,
            velocity = new Vector2(speed, 0f)
        });
    }

    private void UpdateAnimation(CatPass cat)
    {
        if (nyanFrames == null || nyanFrames.Length <= 1)
            return;

        cat.animationTime += Time.deltaTime;
        float frameDuration = 1f / animationFps;
        while (cat.animationTime >= frameDuration)
        {
            cat.animationTime -= frameDuration;
            cat.frameIndex = (cat.frameIndex + 1) % nyanFrames.Length;
            cat.image.texture = nyanFrames[cat.frameIndex];
        }
    }

    private Vector2 GetBeeCanvasPosition()
    {
        if (BeeHealth.I == null || Camera.main == null)
            return Vector2.zero;

        RectTransform canvasRect = canvas.transform as RectTransform;
        Vector2 screenPoint = Camera.main.WorldToScreenPoint(BeeHealth.I.transform.position);
        if (canvasRect != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, screenPoint, null, out Vector2 localPoint))
            return localPoint;
        return Vector2.zero;
    }

    private float GetCanvasHalfWidth()
    {
        RectTransform canvasRect = canvas.transform as RectTransform;
        return canvasRect != null && canvasRect.rect.width > 0f
            ? canvasRect.rect.width * 0.5f
            : 960f;
    }

    private void CheckPlayerCollision(CatPass cat, Vector2 beePosition)
    {
        if (cat.hitPlayer || !cat.image.enabled || BeeHealth.I == null)
            return;

        Vector2 halfHitbox = cat.rect.rect.size * hitboxScale * 0.5f;
        Vector2 delta = cat.rect.anchoredPosition - beePosition;
        if (Mathf.Abs(delta.x) > halfHitbox.x || Mathf.Abs(delta.y) > halfHitbox.y)
            return;

        cat.hitPlayer = true;
        BeeHealth.I.TakeDamage(1);
    }

    private void BeginRun()
    {
        elapsed = 0f;
        spawnTimer = Mathf.Min(firstSpawnDelay, 0.5f);
        canvas.enabled = true;
        if (nyanSound != null && !music.isPlaying)
            music.Play();
    }

    private void EndRun()
    {
        music.Stop();
        canvas.enabled = false;
        ClearCats();
    }

    private void RemoveCat(int index)
    {
        CatPass cat = cats[index];
        Destroy(cat.gameObject);
        cats.RemoveAt(index);
    }

    private void ClearCats()
    {
        for (int i = cats.Count - 1; i >= 0; i--)
            RemoveCat(i);
    }

    private void OnDestroy()
    {
        if (GameFlow.I != null)
        {
            GameFlow.I.RunStarted -= BeginRun;
            GameFlow.I.GameEnded -= EndRun;
        }
        ClearCats();
    }
}

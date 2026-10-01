using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;

public class GameOverUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject highscorePanel;
    [SerializeField] private Text finalTimeText;
    [SerializeField] private Text finalScoreText;
    [SerializeField] private Text[] rankTexts = new Text[5];
    [SerializeField] private Button restartButton;

    [Header("End Media")]
    [SerializeField] private string firstPlaceVideoFile = "DororongChu.mp4";
    [SerializeField] private string normalEndingImageFile = "DororongLaser.jpg";

    private RawImage endMediaBackground;
    private VideoPlayer endVideoPlayer;
    private RenderTexture endVideoTexture;
    private Coroutine imageLoadCoroutine;

    private bool isSubscribed;
    private bool isShowing;

    private void Awake()
    {
        ResolveMissingReferences();
        Hide();
    }

    private void OnEnable()
    {
        SubscribeGameFlow();
    }

    private void Start()
    {
        ResolveMissingReferences();
        SubscribeGameFlow();
    }

    private void Update()
    {
        if (GameFlow.I == null) return;

        if (GameFlow.I.IsGameOver && !isShowing)
        {
            Show();
        }
        else if (!GameFlow.I.IsGameOver && isShowing)
        {
            Hide();
        }
    }

    private void OnDisable()
    {
        if (GameFlow.I != null && isSubscribed)
        {
            GameFlow.I.RunStarted -= Hide;
            GameFlow.I.GameEnded -= Show;
            isSubscribed = false;
        }
    }

    private void Show()
    {
        ResolveMissingReferences();

        if (finalTimeText != null && GameTimer.I != null)
        {
            finalTimeText.text = "TIME  " + GameTimer.I.GetFormattedTime();
        }

        if (finalScoreText != null && ScoreManager.I != null)
        {
            finalScoreText.text = "SCORE  " + ScoreManager.I.GetFormattedScore();
        }

        if (RankingManager.I != null)
        {
            UpdateRankTexts();
        }

        if (highscorePanel != null)
        {
            highscorePanel.SetActive(true);
            highscorePanel.transform.SetAsLastSibling();
        }

        ShowEndMedia(
            RankingManager.I != null &&
            RankingManager.I.LastRunWasFirstPlace
        );

        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(true);
            restartButton.interactable = true;
            restartButton.transform.SetAsLastSibling();
            restartButton.onClick.RemoveListener(RestartGame);
            if (restartButton.onClick.GetPersistentEventCount() == 0)
            {
                restartButton.onClick.AddListener(RestartGame);
            }
        }

        isShowing = true;
    }

    private void Hide()
    {
        ResolveMissingReferences();
        HideEndMedia();

        if (highscorePanel != null)
        {
            highscorePanel.SetActive(false);
        }

        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(false);
        }

        isShowing = false;
    }

    private void ShowEndMedia(bool isFirstPlace)
    {
        EnsureEndMediaBackground();
        endMediaBackground.gameObject.SetActive(true);

        if (isFirstPlace)
            PlayFirstPlaceVideo();
        else
            ShowNormalEndingImage();
    }

    private void EnsureEndMediaBackground()
    {
        if (endMediaBackground != null) return;

        GameObject backgroundObject = new GameObject(
            "EndMediaBackground",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(RawImage),
            typeof(AspectRatioFitter)
        );
        backgroundObject.transform.SetParent(transform, false);
        backgroundObject.transform.SetAsFirstSibling();

        RectTransform rect = backgroundObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        endMediaBackground = backgroundObject.GetComponent<RawImage>();
        endMediaBackground.raycastTarget = false;
        backgroundObject.GetComponent<AspectRatioFitter>().aspectMode =
            AspectRatioFitter.AspectMode.EnvelopeParent;
    }

    private void PlayFirstPlaceVideo()
    {
        StopImageLoading();

        if (endVideoTexture == null)
        {
            endVideoTexture = new RenderTexture(1280, 720, 0);
            endVideoTexture.name = "EndVideoTexture";
            endVideoTexture.Create();
        }

        if (endVideoPlayer == null)
        {
            endVideoPlayer = endMediaBackground.gameObject.AddComponent<VideoPlayer>();
            endVideoPlayer.playOnAwake = false;
            endVideoPlayer.isLooping = true;
            endVideoPlayer.renderMode = VideoRenderMode.RenderTexture;
            endVideoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
            endVideoPlayer.prepareCompleted += OnVideoPrepared;
        }

        endVideoPlayer.targetTexture = endVideoTexture;
        endVideoPlayer.url = GetStreamingAssetPath(firstPlaceVideoFile);
        endMediaBackground.texture = endVideoTexture;
        endVideoPlayer.Prepare();
    }

    private void OnVideoPrepared(VideoPlayer player)
    {
        if (player.height > 0)
        {
            endMediaBackground.GetComponent<AspectRatioFitter>().aspectRatio =
                (float)player.width / player.height;
        }
        player.Play();
    }

    private void ShowNormalEndingImage()
    {
        StopVideo();
        StopImageLoading();
        imageLoadCoroutine = StartCoroutine(LoadNormalEndingImage());
    }

    private IEnumerator LoadNormalEndingImage()
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(
            GetStreamingAssetPath(normalEndingImageFile)))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("[GameOverUI] 엔드 이미지 로드 실패: " + request.error);
                yield break;
            }

            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            endMediaBackground.texture = texture;
            endMediaBackground.GetComponent<AspectRatioFitter>().aspectRatio =
                (float)texture.width / texture.height;
        }
        imageLoadCoroutine = null;
    }

    private string GetStreamingAssetPath(string fileName)
    {
        string path = Path.Combine(Application.streamingAssetsPath, fileName);
        if (!path.Contains("://"))
            path = "file:///" + path.Replace("\\", "/");
        return path;
    }

    private void HideEndMedia()
    {
        StopVideo();
        StopImageLoading();
        if (endMediaBackground != null)
            endMediaBackground.gameObject.SetActive(false);
    }

    private void StopVideo()
    {
        if (endVideoPlayer != null)
            endVideoPlayer.Stop();
    }

    private void StopImageLoading()
    {
        if (imageLoadCoroutine == null) return;
        StopCoroutine(imageLoadCoroutine);
        imageLoadCoroutine = null;
    }

    private void OnDestroy()
    {
        if (endVideoPlayer != null)
            endVideoPlayer.prepareCompleted -= OnVideoPrepared;

        if (endVideoTexture != null)
            endVideoTexture.Release();
    }

    public void RestartGame()
    {
        if (restartButton != null)
        {
            restartButton.interactable = false;
        }

        if (GameFlow.I != null)
        {
            GameFlow.I.Restart();
            return;
        }

        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("BeeSurvivalPrototype");
    }

    private void SubscribeGameFlow()
    {
        if (isSubscribed || GameFlow.I == null) return;

        GameFlow.I.RunStarted += Hide;
        GameFlow.I.GameEnded += Show;
        isSubscribed = true;
    }

    private void UpdateRankTexts()
    {
        for (int i = 0; i < rankTexts.Length; i++)
        {
            if (rankTexts[i] != null)
            {
                rankTexts[i].text = RankingManager.I.GetRankLine(i);
            }
        }
    }

    private void ResolveMissingReferences()
    {
        if (highscorePanel == null)
        {
            highscorePanel = GameObject.Find("HighscorePanel");
        }

        if (highscorePanel == null) return;

        Text[] texts = highscorePanel.GetComponentsInChildren<Text>(true);
        foreach (Text text in texts)
        {
            string objectName = text.gameObject.name.ToLowerInvariant();
            if (finalTimeText == null && objectName.Contains("time"))
            {
                finalTimeText = text;
            }

            if (finalScoreText == null && objectName.Contains("score"))
            {
                finalScoreText = text;
            }

            if (objectName.StartsWith("rank") && TryGetRankIndex(objectName, out int rankIndex) && rankIndex < rankTexts.Length)
            {
                rankTexts[rankIndex] = text;
            }
        }

        if (restartButton == null)
        {
            restartButton = highscorePanel.GetComponentInChildren<Button>(true);
        }
    }

    private bool TryGetRankIndex(string objectName, out int rankIndex)
    {
        rankIndex = -1;

        for (int i = 1; i <= rankTexts.Length; i++)
        {
            if (objectName.Contains(i.ToString()))
            {
                rankIndex = i - 1;
                return true;
            }
        }

        return false;
    }
}

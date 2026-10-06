using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject highscorePanel;
    [SerializeField] private TextMeshProUGUI finalTimeText;
    [SerializeField] private TextMeshProUGUI[] rankTexts = new TextMeshProUGUI[5];
    [SerializeField] private Button restartButton;

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

        if (RankingManager.I != null)
        {
            UpdateRankTexts();
        }

        if (highscorePanel != null)
        {
            highscorePanel.SetActive(true);
        }

        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(true);
        }

        isShowing = true;
    }

    private void Hide()
    {
        ResolveMissingReferences();

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

    public void RestartGame()
    {
        if (GameFlow.I != null)
        {
            GameFlow.I.Restart();
        }
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
            GameObject panelObject = GameObject.Find("highscorePanel");
            if (panelObject == null)
            {
                panelObject = GameObject.Find("HighscorePanel");
            }
            if (panelObject == null)
            {
                panelObject = GameObject.Find("HighScorePanel");
            }

            highscorePanel = panelObject;
        }

        if (highscorePanel == null) return;

        TextMeshProUGUI[] texts = highscorePanel.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (TextMeshProUGUI text in texts)
        {
            string objectName = text.gameObject.name.ToLowerInvariant();
            if (finalTimeText == null && objectName.Contains("time"))
            {
                finalTimeText = text;
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

using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager I { get; private set; }

    [Header("Score Settings")]
    [SerializeField] private float scorePerSecond = 10f;

    [Header("UI Reference")]
    [SerializeField] private Text scoreText;

    private float score;

    public int CurrentScore => Mathf.FloorToInt(score);

    private void Awake()
    {
        I = this;
        UpdateScoreDisplay();
    }

    private void OnDestroy()
    {
        if (I == this)
        {
            I = null;
        }
    }

    private void Update()
    {
        if (GameFlow.I != null && GameFlow.I.IsRunning && !GameFlow.I.IsGameOver)
        {
            score += scorePerSecond * Time.deltaTime;
            UpdateScoreDisplay();
        }
    }

    public void ResetScore()
    {
        score = 0f;
        UpdateScoreDisplay();
    }

    public void AddScore(int amount)
    {
        if (amount <= 0) return;

        score += amount;
        UpdateScoreDisplay();
    }

    public string GetFormattedScore()
    {
        return CurrentScore.ToString("000000");
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = "SCORE  " + GetFormattedScore();
        }
    }
}

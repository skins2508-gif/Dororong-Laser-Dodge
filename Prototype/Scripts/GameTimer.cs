using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    public static GameTimer I { get; private set; }

    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI timerText;

    private float survivalTime = 0f;
    public float SurvivalTime => survivalTime;

    private void Awake()
    {
        I = this;
    }

    private void Update()
    {
        if (GameFlow.I && GameFlow.I.IsRunning && !GameFlow.I.IsGameOver)
        {
            survivalTime += Time.deltaTime;
            UpdateTimerDisplay();
        }
    }

    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            timerText.text = FormatTime(survivalTime);
        }
    }

    public string GetFormattedTime()
    {
        return FormatTime(survivalTime);
    }

    public void ResetTimer()
    {
        survivalTime = 0f;
        UpdateTimerDisplay();
    }

    public static string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        int milliseconds = Mathf.FloorToInt((time * 100f) % 100f);

        return string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
    }
}

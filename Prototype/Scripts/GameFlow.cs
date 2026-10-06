using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlow : MonoBehaviour
{
    public static GameFlow I { get; private set; }
    public bool IsRunning { get; private set; } = false;
    public bool IsGameOver { get; private set; } = false;

    public event Action RunStarted;
    public event Action GameEnded;

    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }

        I = this;

        if (GetComponent<RankingManager>() == null)
        {
            gameObject.AddComponent<RankingManager>();
        }
    }

    private void Start()
    {
        StartRun();
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
        if (IsGameOver && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
        {
            Restart();
        }
    }

    public void StartRun()
    {
        if (IsRunning && !IsGameOver) return;

        IsGameOver = false;
        IsRunning = true;

        if (GameTimer.I != null)
        {
            GameTimer.I.ResetTimer();
        }

        RunStarted?.Invoke();
    }

    public void GameOver()
    {
        if (IsGameOver) return;

        IsRunning = false;
        IsGameOver = true;
        Debug.Log("[GameFlow] Game Over! IsGameOver: " + IsGameOver + ", IsRunning: " + IsRunning);
        GameEnded?.Invoke();
    }

    public void Restart()
    {
        IsRunning = true;
        IsGameOver = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        RunStarted?.Invoke();
    }
}

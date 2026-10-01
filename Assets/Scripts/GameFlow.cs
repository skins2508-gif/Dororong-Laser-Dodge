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
        // GameFlow가 중복으로 존재하는 것을 방지한다.
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;

        if (GetComponent<NyanCatSwarm>() == null)
        {
            gameObject.AddComponent<NyanCatSwarm>();
        }

        // RankingManager가 없으면 자동으로 추가한다.
        if (GetComponent<RankingManager>() == null)
        {
            gameObject.AddComponent<RankingManager>();
        }
    }

    private void OnDestroy()
    {
        // 현재 인스턴스가 삭제되면 싱글톤 참조도 제거한다.
        if (I == this)
        {
            I = null;
        }
    }

    private void Update()
    {
        // 게임오버 상태에서 Space 또는 Enter를 누르면 재시작한다.
        if (IsGameOver &&
            (Input.GetKeyDown(KeyCode.Space) ||
             Input.GetKeyDown(KeyCode.Return)))
        {
            Restart();
        }
    }

    public void StartRun()
    {
        // 이미 게임이 진행 중이면 다시 시작하지 않는다.
        if (IsRunning && !IsGameOver)
            return;

        IsGameOver = false;
        IsRunning = true;

        // 생존 시간을 초기화한다.
        if (GameTimer.I != null)
        {
            GameTimer.I.ResetTimer();
        }

        // 점수를 초기화한다.
        if (ScoreManager.I != null)
        {
            ScoreManager.I.ResetScore();
        }

        // 게임 시작 이벤트를 실행한다.
        RunStarted?.Invoke();
    }

    public void GameOver()
    {
        // 게임오버가 여러 번 실행되는 것을 방지한다.
        if (IsGameOver)
            return;

        IsRunning = false;
        IsGameOver = true;

        Debug.Log("[GameFlow] 게임오버 발생");

        // 게임오버 UI가 표시되기 전에
        // 현재 생존 시간을 랭킹에 저장한다.
        if (RankingManager.I != null)
        {
            RankingManager.I.RecordCurrentRun();

            Debug.Log("[GameFlow] 랭킹 저장 요청 완료");
        }
        else
        {
            Debug.LogError(
                "[GameFlow] RankingManager가 존재하지 않습니다."
            );
        }

        // 게임 종료 이벤트를 실행한다.
        GameEnded?.Invoke();
    }

    public void Restart()
    {
        // 현재 실행 중인 씬의 번호를 가져온다.
        Time.timeScale = 1f;
        Scene activeScene = SceneManager.GetActiveScene();

        // 현재 씬을 다시 불러온다.
        if (activeScene.buildIndex >= 0)
        {
            SceneManager.LoadScene(activeScene.buildIndex);
        }
        else
        {
            SceneManager.LoadScene("BeeSurvivalPrototype");
        }
    }
}

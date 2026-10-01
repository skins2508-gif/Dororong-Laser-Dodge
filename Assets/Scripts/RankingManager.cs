using System.Collections.Generic;
using UnityEngine;

public class RankingManager : MonoBehaviour
{
    public static RankingManager I { get; private set; }

    private const int MaxRankCount = 5;
    private const string RankKeyPrefix = "Ranking_";

    private readonly List<float> rankings = new List<float>();

    // 한 게임 기록이 여러 번 저장되는 것을 방지한다.
    private bool currentRunRecorded;
    public bool LastRunWasFirstPlace { get; private set; }

    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(this);
            return;
        }

        I = this;
        LoadRankings();
    }

    private void OnDestroy()
    {
        if (I == this)
        {
            I = null;
        }
    }

    // 새로운 게임이 시작될 때 저장 가능 상태로 초기화
    public void ResetCurrentRunRecord()
    {
        currentRunRecorded = false;
        LastRunWasFirstPlace = false;
    }

    // 현재 생존 시간을 랭킹에 저장
    public void RecordCurrentRun()
    {
        // 같은 플레이 기록 중복 저장 방지
        if (currentRunRecorded)
            return;

        if (GameTimer.I == null)
        {
            Debug.LogError("[RankingManager] GameTimer가 없습니다.");
            return;
        }

        float survivalTime = GameTimer.I.SurvivalTime;

        if (survivalTime <= 0f)
        {
            Debug.LogWarning(
                "[RankingManager] 생존 시간이 0이라 저장하지 않았습니다."
            );
            return;
        }

        currentRunRecorded = true;
        LastRunWasFirstPlace =
            rankings.Count == 0 || survivalTime > rankings[0];

        rankings.Add(survivalTime);

        // 오래 살아남은 기록이 위로 가도록 내림차순 정렬
        rankings.Sort((a, b) => b.CompareTo(a));

        // 5위까지만 저장
        while (rankings.Count > MaxRankCount)
        {
            rankings.RemoveAt(rankings.Count - 1);
        }

        SaveRankings();

        Debug.Log(
            "[RankingManager] 랭킹 저장 성공: " +
            GameTimer.FormatTime(survivalTime)
        );
    }

    // 해당 순위의 문장을 반환
    public string GetRankLine(int rankIndex)
    {
        if (rankIndex < 0 || rankIndex >= MaxRankCount)
            return string.Empty;

        if (rankIndex < rankings.Count)
        {
            return string.Format(
                "Rank #{0} : {1}",
                rankIndex + 1,
                GameTimer.FormatTime(rankings[rankIndex])
            );
        }

        return string.Format(
            "Rank #{0} : --:--:--",
            rankIndex + 1
        );
    }

    // 저장된 랭킹 불러오기
    private void LoadRankings()
    {
        rankings.Clear();

        for (int i = 0; i < MaxRankCount; i++)
        {
            string key = RankKeyPrefix + i;

            if (!PlayerPrefs.HasKey(key))
                continue;

            float savedTime = PlayerPrefs.GetFloat(key, 0f);

            if (savedTime > 0f)
            {
                rankings.Add(savedTime);
            }
        }

        rankings.Sort((a, b) => b.CompareTo(a));

        Debug.Log(
            "[RankingManager] 불러온 랭킹 개수: " +
            rankings.Count
        );
    }

    // 랭킹 저장
    private void SaveRankings()
    {
        for (int i = 0; i < MaxRankCount; i++)
        {
            string key = RankKeyPrefix + i;

            if (i < rankings.Count)
            {
                PlayerPrefs.SetFloat(key, rankings[i]);
            }
            else
            {
                PlayerPrefs.DeleteKey(key);
            }
        }

        PlayerPrefs.Save();
    }

    // 테스트용 랭킹 초기화
    public void ResetRankings()
    {
        for (int i = 0; i < MaxRankCount; i++)
        {
            PlayerPrefs.DeleteKey(RankKeyPrefix + i);
        }

        PlayerPrefs.Save();
        rankings.Clear();
        currentRunRecorded = false;
        LastRunWasFirstPlace = false;

        Debug.Log("[RankingManager] 랭킹 초기화 완료");
    }
}

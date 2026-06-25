using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스테이지별 클리어 실패 횟수를 추적하고,
/// 3회 이상 실패 시 몬스터 스탯/클리어 요구치를 점진적으로 낮춰
/// "간신히 클리어 가능"한 수준으로 자동 보정합니다.
/// </summary>
public class DifficultyBalancer : MonoBehaviour
{
    [Header("보정 설정")]
    [Tooltip("실패 1회당 난이도를 낮추는 비율 (예: 0.07 = 7%)")]
    public float step = 0.07f;

    [Tooltip("난이도 배율의 하한선 (더 이상 내려가지 않음)")]
    [Range(0.1f, 1f)]
    public float minMultiplier = 0.7f;

    [Tooltip("이 횟수 이상 실패하면 보정이 시작됩니다")]
    public int failThreshold = 3;

    // stageId -> 실패 누적 횟수
    private readonly Dictionary<string, int> failCounts = new Dictionary<string, int>();

    public int GetFailCount(string stageId)
    {
        return failCounts.TryGetValue(stageId, out int count) ? count : 0;
    }

    /// <summary>
    /// 스테이지 클리어 실패 시 호출합니다.
    /// </summary>
    public void RegisterFail(string stageId)
    {
        int count = GetFailCount(stageId) + 1;
        failCounts[stageId] = count;

        float multiplier = GetDifficultyMultiplier(stageId);
        Debug.Log($"[Difficulty] '{stageId}' 실패 누적 {count}회 → 난이도 배율 {multiplier:F2}");
    }

    /// <summary>
    /// 스테이지 클리어 성공 시 호출합니다. 실패 카운터를 0으로 리셋합니다.
    /// (다음 시도부터는 정상 난이도(1.0)로 돌아갑니다.)
    /// </summary>
    public void RegisterClear(string stageId)
    {
        failCounts[stageId] = 0;
        Debug.Log($"[Difficulty] '{stageId}' 클리어 성공 → 실패 카운터 리셋, 난이도 1.00으로 복귀");
    }

    /// <summary>
    /// 현재 실패 누적 횟수에 따른 난이도 배율을 계산합니다.
    /// failCount &lt; failThreshold 이면 1.0 (정상 난이도)
    /// failCount == failThreshold 부터 step만큼씩 감소, minMultiplier에서 멈춤
    /// </summary>
    public float GetDifficultyMultiplier(string stageId)
    {
        int failCount = GetFailCount(stageId);

        if (failCount < failThreshold)
            return 1.0f;

        // failThreshold번째 실패부터 1단계 하향 시작
        int overCount = failCount - (failThreshold - 1);
        float multiplier = 1.0f - overCount * step;

        return Mathf.Max(minMultiplier, multiplier);
    }
}

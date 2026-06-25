using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 보상 풀(RewardPool)을 관리하고, 몬스터 처치 시 3종 랜덤 후보를 추출합니다.
/// spawnWeight가 낮을수록(예: 무적 아이템) 등장 확률이 낮아집니다.
/// </summary>
public class RewardManager : MonoBehaviour
{
    [Header("보상 풀")]
    [Tooltip("설계 문서 5.4의 12종 RewardData 에셋을 모두 등록합니다.")]
    public List<RewardData> rewardPool = new List<RewardData>();

    [Header("연동 매니저")]
    public CombatPowerManager combatPowerManager;

    /// <summary>
    /// 가중치 기반으로 중복 없이 보상 후보 N개를 추출합니다. (기본 3개)
    /// </summary>
    public List<RewardData> DrawRewardChoices(int count = 3)
    {
        var pool = new List<RewardData>(rewardPool);
        var result = new List<RewardData>();

        int drawCount = Mathf.Min(count, pool.Count);
        for (int i = 0; i < drawCount; i++)
        {
            RewardData picked = WeightedPick(pool);
            result.Add(picked);
            pool.Remove(picked);
        }

        return result;
    }

    private RewardData WeightedPick(List<RewardData> candidates)
    {
        float totalWeight = candidates.Sum(r => Mathf.Max(0.0001f, r.spawnWeight));
        float roll = Random.Range(0f, totalWeight);

        float cumulative = 0f;
        foreach (var reward in candidates)
        {
            cumulative += Mathf.Max(0.0001f, reward.spawnWeight);
            if (roll <= cumulative)
                return reward;
        }

        return candidates[candidates.Count - 1];
    }

    /// <summary>
    /// 플레이어가 3개 후보 중 1개를 선택했을 때 호출합니다.
    /// 나머지 후보는 자동으로 폐기됩니다.
    /// </summary>
    public void SelectReward(RewardData chosen)
    {
        combatPowerManager.ApplyReward(chosen);
    }
}

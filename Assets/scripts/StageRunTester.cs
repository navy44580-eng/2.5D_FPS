using UnityEngine;

/// <summary>
/// FPS 에셋 없이도 보상/난이도 시스템이 정상 동작하는지
/// Console 로그로 확인할 수 있는 테스트 스크립트입니다.
///
/// 사용법:
/// 1. 빈 GameObject(예: "_TestRunner")를 만들고 이 스크립트를 추가합니다.
/// 2. 같은 GameObject 또는 다른 GameObject에 CombatPowerManager, RewardManager,
///    DifficultyBalancer를 추가하고 인스펙터에서 연결합니다.
/// 3. testStage와 rewardPool(12종 RewardData)을 인스펙터에서 할당합니다.
/// 4. Play를 누르면 5번의 보상 추첨 → 선택 → 클리어 판정 결과가 Console에 출력됩니다.
/// </summary>
public class StageRunTester : MonoBehaviour
{
    [Header("연동 매니저")]
    public CombatPowerManager combatPowerManager;
    public RewardManager rewardManager;
    public DifficultyBalancer difficultyBalancer;

    [Header("테스트 대상 스테이지")]
    public StageData testStage;

    [Header("테스트 옵션")]
    [Tooltip("이번 런이 좁은 맵인지 (이동속도 보상의 함정 여부에 영향)")]
    public bool isNarrowMap = false;

    [Tooltip("몬스터 처치 시뮬레이션 횟수 (= 보상 추첨 횟수)")]
    public int killSimulationCount = 5;

    private void Start()
    {
        if (!ValidateReferences()) return;

        Debug.Log($"===== '{testStage.displayName}' 스테이지 시뮬레이션 시작 =====");
        combatPowerManager.ResetForNewRun(isNarrowMap);

        for (int i = 0; i < killSimulationCount; i++)
        {
            var choices = rewardManager.DrawRewardChoices(3);

            Debug.Log($"--- 몬스터 처치 #{i + 1}: 보상 후보 ---");
            foreach (var reward in choices)
            {
                float previewDelta = reward.GetEffectiveCpiDelta(combatPowerManager.runState);
                Debug.Log($"  [{reward.rarity}] {reward.displayName} (예상 cpiDelta={previewDelta:+0.0;-0.0;0})");
            }

            // 테스트용 자동 선택: 후보 중 cpiDelta가 가장 큰 것을 선택
            RewardData best = choices[0];
            float bestDelta = best.GetEffectiveCpiDelta(combatPowerManager.runState);
            foreach (var reward in choices)
            {
                float delta = reward.GetEffectiveCpiDelta(combatPowerManager.runState);
                if (delta > bestDelta)
                {
                    best = reward;
                    bestDelta = delta;
                }
            }

            Debug.Log($"  → 선택: {best.displayName}");
            rewardManager.SelectReward(best);
        }

        float multiplier = difficultyBalancer.GetDifficultyMultiplier(testStage.stageId);
        float requirement = testStage.GetClearRequirement(multiplier);
        bool canClear = combatPowerManager.CanClearStage(testStage, multiplier);

        Debug.Log($"===== 결과 =====");
        Debug.Log($"최종 CPI: {combatPowerManager.runState.currentCpi:F1} / 요구치: {requirement:F1} (난이도 배율 {multiplier:F2})");
        Debug.Log(canClear ? "→ 클리어 성공!" : "→ 클리어 실패");

        if (canClear)
            difficultyBalancer.RegisterClear(testStage.stageId);
        else
            difficultyBalancer.RegisterFail(testStage.stageId);
    }

    private bool ValidateReferences()
    {
        if (combatPowerManager == null) { Debug.LogError("CombatPowerManager가 연결되지 않았습니다."); return false; }
        if (rewardManager == null) { Debug.LogError("RewardManager가 연결되지 않았습니다."); return false; }
        if (difficultyBalancer == null) { Debug.LogError("DifficultyBalancer가 연결되지 않았습니다."); return false; }
        if (testStage == null) { Debug.LogError("testStage가 연결되지 않았습니다."); return false; }
        if (rewardManager.rewardPool == null || rewardManager.rewardPool.Count == 0)
        {
            Debug.LogError("RewardManager.rewardPool이 비어있습니다. RewardData 에셋을 등록해주세요.");
            return false;
        }
        return true;
    }
}

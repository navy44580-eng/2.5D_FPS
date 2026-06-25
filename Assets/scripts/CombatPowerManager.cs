using UnityEngine;

/// <summary>
/// 플레이어의 전투력 지수(CPI)를 관리합니다.
/// 보상 선택에 따른 CPI 변화, 무적 상태, 스테이지 클리어 가능 여부 판정을 담당합니다.
/// </summary>
public class CombatPowerManager : MonoBehaviour
{
    [Header("런타임 상태")]
    public PlayerRunState runState = new PlayerRunState();

    [Header("기본값")]
    [Tooltip("새 런(스테이지 1회 시도)을 시작할 때의 기본 CPI")]
    public float startingCpi = 50f;

    /// <summary>
    /// 새로운 스테이지 시도를 시작할 때 호출합니다.
    /// </summary>
    public void ResetForNewRun(bool isNarrowMap)
    {
        runState.currentCpi = startingCpi;
        runState.hasWeapon = false;
        runState.isInvincible = false;
        runState.invincibleTimeRemaining = 0f;
        runState.isNarrowMap = isNarrowMap;
    }

    /// <summary>
    /// 플레이어가 선택한 보상을 CPI에 반영합니다.
    /// </summary>
    public void ApplyReward(RewardData reward)
    {
        float delta = reward.GetEffectiveCpiDelta(runState);
        runState.currentCpi += delta;

        if (reward.effectType == RewardEffectType.WeaponItem ||
            reward.effectType == RewardEffectType.WeaponUpgrade)
        {
            runState.hasWeapon = true;
        }

        if (reward.effectType == RewardEffectType.InvincibleBuff)
        {
            ActivateInvincibility(reward.effectValue);
        }

        Debug.Log($"[CPI] '{reward.displayName}' 적용 → cpiDelta={delta:+0.0;-0.0;0}, 현재 CPI={runState.currentCpi:F1}");
    }

    public void ActivateInvincibility(float durationSeconds)
    {
        runState.isInvincible = true;
        runState.invincibleTimeRemaining = durationSeconds;
        Debug.Log($"[CPI] 무적 활성화: {durationSeconds:F1}초");
    }

    private void Update()
    {
        if (!runState.isInvincible) return;

        runState.invincibleTimeRemaining -= Time.deltaTime;
        if (runState.invincibleTimeRemaining <= 0f)
        {
            runState.isInvincible = false;
            runState.invincibleTimeRemaining = 0f;
            Debug.Log("[CPI] 무적 종료");
        }
    }

    /// <summary>
    /// 현재 CPI가 해당 스테이지의 (난이도 보정 적용된) 요구치를 넘는지 판정합니다.
    /// </summary>
    public bool CanClearStage(StageData stage, float difficultyMultiplier)
    {
        // 무적 상태라면 수치 비교 없이 클리어 처리
        if (runState.isInvincible) return true;

        return runState.currentCpi >= stage.GetClearRequirement(difficultyMultiplier);
    }
}

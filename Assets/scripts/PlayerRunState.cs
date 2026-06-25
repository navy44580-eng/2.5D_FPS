using System;

/// <summary>
/// 한 번의 게임 런(run) 동안 유지되는 플레이어 상태입니다.
/// 보상을 선택했을 때 실제로 적용될 CPI 변화량을 계산하기 위해
/// RewardData.GetEffectiveCpiDelta()에서 이 값을 참조합니다.
/// </summary>
[Serializable]
public class PlayerRunState
{
    // 현재 전투력 지수 (Combat Power Index)
    public float currentCpi;

    // 무기 보유 여부 - "무기 강화" 보상의 유효성 판단에 사용
    public bool hasWeapon;

    // 현재 스테이지가 좁은 맵인지 여부 - "이동속도/쿨감" 보상의 유효성 판단에 사용
    public bool isNarrowMap;

    // 무적 상태 여부
    public bool isInvincible;

    // 무적 잔여 시간(초)
    public float invincibleTimeRemaining;
}

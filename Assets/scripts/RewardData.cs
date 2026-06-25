using UnityEngine;

/// <summary>
/// 설계 문서 5.4의 3개 그룹 분류
/// </summary>
public enum RewardCategory
{
    Growth,     // 그룹 A: 성장·자원형
    Offense,    // 그룹 B: 공격 강화형
    Survival    // 그룹 C: 생존·특수형
}

/// <summary>
/// 탕탕특공대 참고 6단계 등급 (카드 테두리 색상에 매핑)
/// </summary>
public enum RewardRarity
{
    Common,     // 일반 - 회색
    Uncommon,   // 우수 - 초록
    Rare,       // 레어 - 파랑
    Elite,      // 엘리트 - 보라
    Epic,       // 에픽 - 노랑
    Legendary   // 레전드 - 빨강
}

/// <summary>
/// 설계 문서 5.4의 12종 보상 아이템 타입
/// </summary>
public enum RewardEffectType
{
    ExpGain,            // 경험치 오브
    GoldGain,           // 골드 주머니
    MaxHpIncrease,      // 최대 체력 증가
    AllStatsBoost,      // 스탯 강화서
    WeaponItem,         // 무기 아이템
    WeaponUpgrade,      // 무기 강화
    CriticalBoost,      // 치명타 아이템
    AttackPowerBoost,   // 공격력 강화
    DefenseBoost,       // 방어력 아이템
    InstantHeal,        // 회복 아이템
    MoveSpeedOrCooldown,// 이동속도 / 쿨감
    InvincibleBuff      // 무적 아이템 (전설)
}

[CreateAssetMenu(fileName = "NewReward", menuName = "IdleFPS/Reward Data")]
public class RewardData : ScriptableObject
{
    [Header("기본 정보")]
    public string rewardId;
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("분류")]
    public RewardCategory category;
    public RewardRarity rarity;
    public RewardEffectType effectType;

    [Header("효과 값")]
    [Tooltip("효과 수치 (예: 0.15 = 15%, 무적 아이템은 지속 시간(초))")]
    public float effectValue;

    [Header("전투력(CPI) 영향")]
    [Tooltip("선택 시 CPI에 더해지는 기본값. 함정형 아이템은 GetEffectiveCpiDelta()에서 플레이어 상태에 따라 재계산됩니다.")]
    public float baseCpiDelta;

    [Header("등장 확률")]
    [Min(0f)]
    [Tooltip("3종 추출 시 가중치. 레전드(무적 아이템)는 매우 낮게 설정 (예: 1~3)")]
    public float spawnWeight = 10f;

    /// <summary>
    /// 플레이어의 현재 런 상태를 참조해, 이 보상을 선택했을 때
    /// 실제로 적용될 CPI 변화량을 계산합니다.
    /// "함정형" 보상은 빌드 상태에 따라 0이나 음수가 될 수 있습니다.
    /// </summary>
    public float GetEffectiveCpiDelta(PlayerRunState playerState)
    {
        switch (effectType)
        {
            // 무기를 보유하지 않은 상태에서 "무기 강화"를 뽑으면 효과 없음(함정)
            case RewardEffectType.WeaponUpgrade:
                return playerState.hasWeapon ? baseCpiDelta : 0f;

            // 이미 무기가 있는데 새 무기를 또 받으면 효율이 절반으로 감소
            case RewardEffectType.WeaponItem:
                return playerState.hasWeapon ? baseCpiDelta * 0.5f : baseCpiDelta;

            // 좁은 맵에서는 이동속도/쿨감 보상이 오히려 불리(함정)할 수 있음
            case RewardEffectType.MoveSpeedOrCooldown:
                return playerState.isNarrowMap ? -Mathf.Abs(baseCpiDelta) : Mathf.Abs(baseCpiDelta);

            default:
                return baseCpiDelta;
        }
    }
}

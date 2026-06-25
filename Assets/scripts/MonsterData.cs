using UnityEngine;

[CreateAssetMenu(fileName = "NewMonster", menuName = "IdleFPS/Monster Data")]
public class MonsterData : ScriptableObject
{
    [Header("기본 정보")]
    public string monsterId;
    public string displayName;

    [Tooltip("이후 몬스터 모델을 교체할 때 이 프리팹만 변경하면 됩니다.")]
    public GameObject prefab;

    [Header("기본 스탯 (난이도 보정 전 원본 값)")]
    public float baseHP = 100f;
    public float baseAttack = 10f;
    public float baseMoveSpeed = 3.5f;

    [Header("보상")]
    public int expReward = 10;
    public bool isBoss;

    /// <summary>
    /// DifficultyBalancer가 계산한 difficultyMultiplier를 적용한
    /// 실제 스폰 스탯을 반환합니다.
    /// </summary>
    public (float hp, float attack, float speed) GetAdjustedStats(float difficultyMultiplier)
    {
        return (
            baseHP * difficultyMultiplier,
            baseAttack * difficultyMultiplier,
            baseMoveSpeed * difficultyMultiplier
        );
    }
}

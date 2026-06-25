using System;
using UnityEngine;

[Serializable]
public struct MonsterSpawnEntry
{
    public MonsterData monster;
    [Min(1)] public int count;
}

[CreateAssetMenu(fileName = "NewStage", menuName = "IdleFPS/Stage Data")]
public class StageData : ScriptableObject
{
    [Header("기본 정보")]
    public string stageId;
    public string displayName;

    [Tooltip("이후 배경(환경)을 교체할 때 이 프리팹만 변경하면 됩니다.")]
    public GameObject environmentPrefab;

    [Header("스폰 구성")]
    public MonsterSpawnEntry[] monsterSpawnList;

    [Header("클리어 조건")]
    [Tooltip("난이도 보정 전, 클리어에 필요한 기본 CPI 요구치")]
    public float baseClearRequirement = 100f;

    /// <summary>
    /// DifficultyBalancer가 계산한 difficultyMultiplier를 적용한
    /// 실제 클리어 요구치를 반환합니다.
    /// (몬스터 스탯과 동일한 배율로 함께 낮아져 "간신히 클리어 가능"한 수준이 됩니다.)
    /// </summary>
    public float GetClearRequirement(float difficultyMultiplier)
    {
        return baseClearRequirement * difficultyMultiplier;
    }
}

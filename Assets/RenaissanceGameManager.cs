using UnityEngine;
using UnityEngine.UI; // 유니티 UI 시스템을 제어하기 위해 필요합니다.

public class RenaissanceGameManager : MonoBehaviour
{
    // 어디서나 이 스크립트에 접근할 수 있도록 만드는 싱글톤(Singleton) 설정
    public static RenaissanceGameManager Instance;

    [System.Serializable]
    public class StarterItems
    {
        public string tool1 = "낡은 나침반";
        public string tool2 = "딱딱한 빵 (비상식량)";
        public string weaponMin = "조잡한 나무 칼 한 자루";
        public string weaponSub = "작은 나이프";
        public string campGear = "야영 짐";
    }

    [Header("--- 📜 시나리오 기반 초반 소지품 ---")]
    public StarterItems playerInventory = new StarterItems();

    [Header("--- 📊 캐릭터 상태 (Status) ---")]
    public string playerTitle = "농민의 아들";
    public int currentLevel = 1;
    public int maxLevel = 100;
    public double currentXP = 0;
    public float currentHP = 100f;
    public float maxHP = 100f;
    public float currentMP = 50f;
    public float maxMP = 50f;

    [Header("--- 🎮 연결할 유니티 UI 요소들 (Inspector에서 드래그앤드롭) ---")]
    public Image hpBarGauge;       // 하단 왼쪽 빨간색 HP Bar 이미지
    public Image mpBarGauge;       // 하단 왼쪽 파란색 MP Bar 이미지
    public Text statusTextDisplay; // 캐릭터 정보나 레벨을 화면에 보여줄 텍스트 (선택사항)

    void Awake()
    {
        // 싱글톤 초기화
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        Debug.Log($"[게임 시작] {playerTitle}의 위대한 모험이 시작됩니다!");
        Debug.Log($"소지품 확인: {playerInventory.tool1}, {playerInventory.tool2}, {playerInventory.weaponMin}, {playerInventory.weaponSub}, {playerInventory.campGear}");
        
        UpdateUIStatus();
    }

    // 1. 레벨 밸런싱 시스템: 다음 레벨 요구 경험치 계산 (기본 1% + 레벨당 0.001% 가산 구조)
    public double GetRequiredXPForNextLevel(int level)
    {
        if (level >= maxLevel) return 0;

        double baseXPForLevel1 = 100; // 레벨 1일 때 기준 경험치 통
        double requiredXP = baseXPForLevel1;

        for (int i = 1; i < level; i++)
        {
            // 질문자님 공식: 기본 1%(0.01) + 레벨마다 0.001%(0.00001)씩 상승률 누적 누적
            double growthRate = 0.01 + ((i - 1) * 0.00001);
            requiredXP = requiredXP * (1.0 + growthRate);
        }

        return System.Math.Round(requiredXP, 0); // 소수점 깔끔하게 반올림
    }

    // 2. 경험치 획득 및 연속 레벨업 처리 함수
    public void EarnExperience(double amount)
    {
        if (currentLevel >= maxLevel) return;

        currentXP += amount;
        double requiredXP = GetRequiredXPForNextLevel(currentLevel);

        while (currentXP >= requiredXP && currentLevel < maxLevel)
        {
            currentXP -= requiredXP;
            currentLevel++;
            OnPlayerLevelUp();
            requiredXP = GetRequiredXPForNextLevel(currentLevel);
        }

        UpdateUIStatus();
    }

    // 3. 레벨업 시 발생하는 이벤트 (성장 연출 및 타이틀 변화)
    void OnPlayerLevelUp()
    {
        Debug.Log($"🎉 레벨업 달성! 현재 레벨: {currentLevel}");

        // 기획 반영: 특정 레벨 도달 시 신분(타이틀) 상승 연출 예시
        if (currentLevel == 20) { playerTitle = "이름 날리는 모험가"; Debug.Log("신분이 변경되었습니다: 모험가"); }
        else if (currentLevel == 50) { playerTitle = "여명의 군주 (왕)"; Debug.Log("신분이 변경되었습니다: 한 나라의 왕"); }
        else if (currentLevel == 100) { playerTitle = "세계 평화의 대군주"; Debug.Log("엔딩: 전 세계를 제패하고 평화를 이룩했습니다!"); }

        // 레벨업 보너스로 체력과 마력 통 최대치 증가 및 완전 회복
        maxHP += 15f;
        maxMP += 10f;
        currentHP = maxHP;
        currentMP = maxMP;
    }

    // 4. 대미지를 받거나 마법을 썼을 때 바(Bar) 게이지 UI를 깎아주는 함수들
    public void TakeDamage(float damage)
    {
        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;
        UpdateUIStatus();
    }

    public void UseMagic(float mpCost)
    {
        if (currentMP >= mpCost)
        {
            currentMP -= mpCost;
            UpdateUIStatus();
        }
    }

    // 5. 실시간 UI 반영 기능 (빨간색 HP바, 파란색 MP바 스케일 조정)
    void UpdateUIStatus()
    {
        // 유니티 이미지의 fillAmount(0.0 ~ 1.0)를 조절하여 게이지 형태 구현
        if (hpBarGauge != null) hpBarGauge.fillAmount = currentHP / maxHP;
        if (mpBarGauge != null) mpBarGauge.fillAmount = currentMP / maxMP;

        if (statusTextDisplay != null)
        {
            statusTextDisplay.text = $"[ {playerTitle} ]  LV.{currentLevel}\nXP: {currentXP} / {GetRequiredXPForNextLevel(currentLevel)}";
        }
    }
}
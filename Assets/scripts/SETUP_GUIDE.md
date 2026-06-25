# Unity 적용 가이드 — 보상/난이도 시스템 프로토타입

에셋 구매 전, 현재 빈 URP 프로젝트에서 보상·난이도 시스템만 먼저 동작시켜보는 가이드입니다.

---

## 1. 스크립트 파일 넣기

1. Unity의 **Project** 패널에서 `Assets` 폴더를 우클릭 → `Create > Folder` → 이름을 `Scripts`로 지정
2. `Scripts` 폴더 안에 다시 `Data`, `Systems`, `Test` 세 개의 하위 폴더 생성
3. 탐색기(파일 탐색기)에서 아래와 같이 파일을 드래그 앤 드롭으로 복사

```
Assets/Scripts/Data/
  PlayerRunState.cs
  RewardData.cs
  MonsterData.cs
  StageData.cs

Assets/Scripts/Systems/
  CombatPowerManager.cs
  RewardManager.cs
  DifficultyBalancer.cs

Assets/Scripts/Test/
  StageRunTester.cs
```

복사 후 Unity가 자동으로 컴파일합니다. Console에 빨간 에러가 없으면 정상입니다.

---

## 2. 보상 데이터(RewardData) 12종 생성

`Assets`에 `Data/Rewards` 폴더를 새로 만든 뒤, 그 안에서 우클릭 →
`Create > IdleFPS > Reward Data` 를 **12번** 실행해 아래 표대로 값을 채워줍니다.
(이전에 정리한 5.4 카탈로그 기준)

| displayName | category | rarity | effectType | effectValue | baseCpiDelta | spawnWeight |
|---|---|---|---|---|---|---|
| 경험치 오브 | Growth | Common | ExpGain | 20 | 0 | 20 |
| 골드 주머니 | Growth | Common | GoldGain | 50 | 0 | 20 |
| 최대 체력 증가 | Growth | Uncommon | MaxHpIncrease | 0.12 | 8 | 14 |
| 스탯 강화서 | Growth | Rare | AllStatsBoost | 0.05 | 6 | 10 |
| 무기 아이템 | Offense | Rare | WeaponItem | 1 | 15 | 10 |
| 무기 강화 | Offense | Uncommon | WeaponUpgrade | 0.12 | 10 | 14 |
| 치명타 아이템 | Offense | Elite | CriticalBoost | 0.2 | 9 | 8 |
| 공격력 강화 | Offense | Uncommon | AttackPowerBoost | 0.12 | 8 | 14 |
| 방어력 아이템 | Survival | Uncommon | DefenseBoost | 0.08 | 6 | 14 |
| 회복 아이템 | Survival | Common | InstantHeal | 0.4 | 1 | 20 |
| 이동속도/쿨감 | Survival | Rare | MoveSpeedOrCooldown | 8 | 8 | 10 |
| 무적 아이템 | Survival | Legendary | InvincibleBuff | 12 | 999 | 2 |

> `spawnWeight`는 상대값입니다. 무적 아이템(2)은 다른 항목(8~20) 대비 매우 낮게 설정해
> "거의 안 나오지만, 나오면 게임체인저"가 되도록 합니다.

---

## 3. 몬스터 / 스테이지 데이터 생성 (테스트용 최소 1개씩)

1. `Data/Monsters` 폴더에서 우클릭 → `Create > IdleFPS > Monster Data`
   - `monsterId`: `monster_001`, `baseHP`: 50, `baseAttack`: 5, `expReward`: 10
2. `Data/Stages` 폴더에서 우클릭 → `Create > IdleFPS > Stage Data`
   - `stageId`: `stage_001`
   - `monsterSpawnList`에 위에서 만든 몬스터를 1개 등록 (count: 5)
   - `baseClearRequirement`: **90** (테스트 시 의도적으로 빡빡하게 설정 — 보상에 따라 클리어 성공/실패가 갈리도록)

---

## 4. 테스트 씬 구성

1. `Hierarchy`에서 우클릭 → `Create Empty` → 이름을 `_GameSystems`로 변경
2. `_GameSystems`에 다음 컴포넌트를 **Add Component**로 모두 추가
   - `CombatPowerManager`
   - `RewardManager`
   - `DifficultyBalancer`
   - `StageRunTester`
3. Inspector에서 연결:
   - `RewardManager.combatPowerManager` ← 같은 오브젝트의 `CombatPowerManager`
   - `RewardManager.rewardPool` ← 2단계에서 만든 RewardData 12개를 모두 드래그하여 등록
   - `StageRunTester`의 4개 필드(`combatPowerManager`, `rewardManager`, `difficultyBalancer`, `testStage`) 모두 연결

---

## 5. 실행 및 결과 확인

1. 상단 `▶ Play` 버튼 클릭
2. `Console` 창(아직 안 보이면 `Window > General > Console`)에서 로그 확인
3. 출력 예시:

```
===== 'XXX 스테이지' 스테이지 시뮬레이션 시작 =====
--- 몬스터 처치 #1: 보상 후보 ---
  [Common] 경험치 오브 (예상 cpiDelta=0)
  [Uncommon] 무기 강화 (예상 cpiDelta=0)   <- 무기 미보유 상태라 함정으로 0 처리됨
  [Uncommon] 공격력 강화 (예상 cpiDelta=8)
  → 선택: 공격력 강화
...
===== 결과 =====
최종 CPI: 96.0 / 요구치: 90.0 (난이도 배율 1.00)
→ 클리어 성공!
```

4. `Play`를 다시 눌러 여러 번 반복해보면, 보상 추첨이 매번 달라지고 결과(성공/실패)도 달라지는 것을 확인할 수 있습니다.
5. 만약 같은 `testStage`에서 **3번 연속 "클리어 실패"** 로그가 나오면, 다음 실행부터 `DifficultyBalancer`가 난이도 배율을 자동으로 낮추는지 Console에서 확인해보세요.

---

## 6. 다음 단계

이 프로토타입은 FPS 컨트롤러 없이 "보상 선택 → CPI 변화 → 클리어 판정 → 난이도 보정" 흐름만 검증한 것입니다.
FPS Retro Shooter 에셋 구매/임포트가 끝나면:

- `StageRunTester`의 자동 선택 로직을 → 실제 UI(보상 3카드 클릭)로 교체
- `CombatPowerManager.ResetForNewRun()`을 → 실제 스테이지 입장 시점에 호출
- `MonsterData.GetAdjustedStats()` 결과를 → 에셋의 `EnemyAI` 스폰 로직에 연결

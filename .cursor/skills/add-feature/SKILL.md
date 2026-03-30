# Skill: 새 기능 추가

새 게임 기능을 어셈블리 구조에 맞게 추가하는 절차.

## 단계

### 1. 분류

기능이 어느 어셈블리에 속하는지 먼저 판단:

| 질문 | 어셈블리 |
|---|---|
| 데이터/설정/이벤트 정의? | Platformer.Data |
| 순수 게임 규칙, 인터페이스? | Platformer.Core |
| 플레이어/적/월드 동작? | Platformer.Game |
| 화면에 보이는 UI? | Platformer.UI |

Game ↔ UI 경계를 넘는 소통이 필요하면 → SO 이벤트 채널 추가 (Data에)

### 2. 데이터 SO 정의 (필요 시)

`Assets/_Project/Scripts/Data/` 에 설정값용 SO 클래스 생성:

```csharp
// EnemySettings.cs
namespace Platformer.Data
{
    [CreateAssetMenu(fileName = "EnemySettings", menuName = "Platformer/Settings/Enemy")]
    public class EnemySettings : ScriptableObject
    {
        public float moveSpeed = 2f;
        public int maxHealth = 3;
        public float patrolRange = 5f;
    }
}
```

그 다음 `Assets/_Project/Datas/` 에서 우클릭 → Create > Platformer/Settings/Enemy 로 인스턴스 생성.

### 3. 스크립트 생성

어셈블리 규칙 준수:
- 파일 위치 = namespace와 일치
- 1파일 1클래스
- 필요한 using만 추가

```csharp
// Assets/_Project/Scripts/Game/Enemy/EnemyController.cs
namespace Platformer.Game
{
    public class EnemyController : MonoBehaviour, IDamageable
    {
        [SerializeField] private EnemySettings _settings;

        private Rigidbody2D _rb;
        private int _currentHealth;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _currentHealth = _settings.maxHealth;
        }

        public void TakeDamage(int amount)
        {
            _currentHealth -= amount;
            if (_currentHealth <= 0) Die();
        }

        void Die() { /* 처리 */ }
    }
}
```

### 4. Prefab 생성

1. 씬에 임시 GameObject 배치 + 컴포넌트 추가 + Inspector에서 값 연결
2. `Assets/_Project/Prefabs/` 로 드래그 → Prefab 생성
3. 씬의 임시 GameObject 삭제 (Prefab에서 인스턴스로 배치)

### 5. 와이어링 (Inspector 연결)

- SerializeField 슬롯에 SO, Prefab, 컴포넌트 연결
- 이벤트 채널 사용 시 SO 인스턴스를 양쪽(발행/구독)에 모두 연결

### 6. 검증

- [ ] 컴파일 에러 없음
- [ ] 어셈블리 경계 위반 없음 (using 확인)
- [ ] 플레이 모드에서 동작 확인
- [ ] null 참조 에러 없음 (Inspector 슬롯 다 채워졌는지)

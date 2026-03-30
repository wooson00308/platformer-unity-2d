# Platformer Unity 2D

Unity 6.0.3, URP 2D 기반 2D 플랫포머 에셋 데모 프로젝트.
아트 디자이너가 AI(Cursor/Claude Code)와 함께 작업하는 것을 전제로 구성된 가드레일 환경.

## 어셈블리 맵

```
Platformer.Data
    └── (참조 없음)

Platformer.Core
    └── → Platformer.Data

Platformer.Game
    └── → Platformer.Core
    └── → Platformer.Data

Platformer.UI
    └── → Platformer.Core
    └── → Platformer.Data
```

참조 테이블:

| 어셈블리        | 참조 가능                      | 참조 불가              |
|----------------|-------------------------------|----------------------|
| Platformer.Data | (없음)                        | Core, Game, UI       |
| Platformer.Core | Data                          | Game, UI             |
| Platformer.Game | Data, Core                    | UI                   |
| Platformer.UI   | Data, Core                    | Game                 |

Game ↔ UI 직접 참조 절대 금지. 서로 참조하면 빌드가 순환 참조로 터짐.

## Game ↔ UI 소통 규칙

Game과 UI는 서로 직접 참조할 수 없다. 소통은 반드시 SO 이벤트 채널을 통해.

```csharp
// Assets/_Project/Datas/Events/ 에 SO 인스턴스 생성
// GameEvent.cs (Platformer.Data 어셈블리)

// 발행 (Game쪽):
[SerializeField] private GameEvent onPlayerDied;
onPlayerDied.Raise();

// 구독 (UI쪽):
[SerializeField] private GameEvent onPlayerDied;
void OnEnable() => onPlayerDied.AddListener(OnPlayerDied);
void OnDisable() => onPlayerDied.RemoveListener(OnPlayerDied);
```

## 코드 규칙

### 1파일 1클래스
파일 하나에 클래스 하나. private inner class는 예외.

### 네임스페이스
파일이 속한 어셈블리 이름과 동일하게.
- Scripts/Data/ → namespace Platformer.Data
- Scripts/Core/ → namespace Platformer.Core
- Scripts/Game/ → namespace Platformer.Game
- Scripts/UI/ → namespace Platformer.UI

### Arts/ 폴더 스크립트 금지
Arts/ 하위에는 스프라이트, 애니메이션, 타일셋, VFX, 오디오 에셋만. .cs 파일 절대 넣지 말 것.

### SO Settings 패턴 (하드코딩 금지)
숫자 값(속도, 점프력, 체력 등)은 반드시 ScriptableObject로 분리.

```csharp
// 나쁜 예
float speed = 5f;

// 좋은 예
[SerializeField] private PlayerSettings settings;
float speed = settings.moveSpeed;
```

SO 인스턴스는 Assets/_Project/Datas/ 에 저장.

### Input System
레거시 Input (Input.GetKey 등) 사용 금지. 반드시 Unity Input System 패키지 사용.

```csharp
// 금지
if (Input.GetKeyDown(KeyCode.Space)) { ... }

// 올바른 방법
// PlayerInputActions.inputactions 에셋 → Generate C# Class 체크
private PlayerInputActions _input;
void Awake() => _input = new PlayerInputActions();
void OnEnable() => _input.Enable();
void OnDisable() => _input.Disable();
```

## 커밋 컨벤션

```
<type>: <설명>
```

타입:
- feat — 새 기능
- fix — 버그 수정
- art — 에셋 추가/수정 (스프라이트, 애니메이션 등)
- docs — 문서
- chore — 기타

예시:
- feat: 이중 점프 구현
- art: 플레이어 달리기 스프라이트 교체
- fix: 경사면 미끄러짐 수정

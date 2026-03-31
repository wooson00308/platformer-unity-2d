# Platformer Unity 2D

Unity 6.0.3, URP 2D 기반 2D 플랫포머 에셋 데모 프로젝트.
아트 디자이너가 AI(Cursor/Claude Code)와 함께 작업하는 것을 전제로 구성된 가드레일 환경.

## 작업 시작 전 필수 확인

Unity MCP 연결이 되어있어야 에디터 제어(컴파일 체크, 씬 조작, 테스트)가 가능하다.
작업 시작 전 반드시 연결 상태를 확인할 것.

```bash
unity-mcp-cli run-tool assets-refresh --input '{}'
```

SUCCESS가 나오면 정상. 에러가 나오면 setup-mcp 스킬을 실행해서 연결을 세팅할 것.

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

### Unity 직렬화 파일 직접 수정 금지

.unity(씬), .prefab, .asset, .controller, .anim 파일을 텍스트로 열어서 직접 읽거나 수정하지 말 것.
이 파일들은 Unity YAML Serialization 포맷이라 fileID/GUID 참조가 얽혀 있어서, 직접 편집하면 참조가 깨진다.

씬/프리팹/에셋 조작이 필요하면:
- MCP 가용 시: script-execute 또는 MCP 도구(gameobject-*, assets-prefab-* 등) 사용
- MCP 불가 시: 사용자에게 에디터 조작을 단계별로 안내

유일한 예외: SO(.asset)의 단순 값 수정은 YAML sed + assets-refresh 허용 (ScriptableObject 수정 주의사항 참고).

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

## Unity MCP 활용

코드 수정 후 반드시 assets-refresh로 컴파일 상태를 확인할 것.
프로젝트 루트에서 실행해야 한다.

```bash
# 코드 수정 후 에디터에 반영
unity-mcp-cli run-tool assets-refresh --input '{}'

# 컴파일 에러 확인
grep -n "error CS" ~/Library/Logs/Unity/Editor.log | tail -30

# 런타임 에러 확인
grep -n "NullReferenceException\|InvalidOperationException\|MissingReferenceException" ~/Library/Logs/Unity/Editor.log | tail -20

# 경고 확인
grep -n "warning CS" ~/Library/Logs/Unity/Editor.log | tail -20

# 씬 하이라키 탐색, 컴포넌트 일괄 조회 등 (class + static method 구조 필수)
unity-mcp-cli run-tool script-execute --input-file - <<'SCRIPT'
{"csharpCode": "using UnityEngine;\npublic class Script { public static object Main() { return \"hello\"; } }"}
SCRIPT
```

검증 흐름:
1. 코드 수정
2. `unity-mcp-cli run-tool assets-refresh --input '{}'` — 에디터에 반영
3. `grep -n "error CS" ~/Library/Logs/Unity/Editor.log | tail -30` — 컴파일 에러 확인
4. 에러 있으면 수정 후 1번부터 반복
5. `unity-mcp-cli run-tool tests-run --input '{}'` — 테스트 통과 확인

## 테스트

테스트 실행:
```bash
unity-mcp-cli run-tool tests-run --input '{}'
```

테스트 파일 위치: 각 어셈블리의 Tests/ 폴더
- Scripts/Data/Tests/
- Scripts/Core/Tests/
- Scripts/Game/Tests/
- Scripts/UI/Tests/

새 기능 추가 시 대응하는 테스트도 함께 작성할 것. 테스트 없이 머지하지 말 것.

## ScriptableObject 수정 주의사항

assets-modify로 SO를 부분 수정하면 명시하지 않은 필드가 기본값(0)으로 리셋될 수 있다.

SO 에셋 수정이 필요할 때:
- 안전한 방법: YAML 직접 수정(sed 등) + assets-refresh 조합
- assets-modify는 전체 필드를 다 지정할 때만 사용

```bash
# YAML 직접 수정 후 Unity에 반영
unity-mcp-cli run-tool assets-refresh --input '{}'
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

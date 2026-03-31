---
name: add-feature
description: 만들어줘, 추가해줘, 구현해줘 요청 시 사용. 적, 플랫폼, 함정, 기믹, 아이템, UI, 체력바, 발사체 등 새 게임 기능을 추가할 때 이 절차를 반드시 따른다. 코드 작성 → 프리팹 생성 → 씬 배치 → 테스트까지 전부 실행.
---

# 새 기능 추가

기능 요청을 받으면 이 절차를 Step 1부터 순서대로 실행한다. 건너뛰지 않는다.

## 전제 조건

- Unity 프로젝트가 열려있고 에디트 모드여야 함
- 어셈블리 맵을 숙지할 것 (CLAUDE.md 참고)

## Step 0 — MCP 가용 여부 확인

```bash
unity-mcp-cli run-tool assets-refresh --input '{}'
```

- SUCCESS → 이후 모든 Step에서 "MCP 모드" 절차를 따른다.
- 실패/타임아웃 → 이후 모든 Step에서 "수동 모드" 절차를 따른다.

이 판정은 한 번만 하고, 이후 Step에서 반복하지 않는다.

---

## Step 0.5 — 스펙 확인 (사용자에게 반드시 질문할 것)

코드를 한 줄이라도 작성하기 전에, 아래 항목을 사용자에게 확인한다.
확인 없이 구현을 시작하지 말 것 — 나중에 전부 뒤집어야 하는 상황을 막기 위함.

사용자에게 질문할 항목:
1. 이 기능이 어떻게 동작해야 하는지 (예: "좌우로 왕복", "플레이어가 닿으면 데미지")
2. 기존 오브젝트와 어떻게 상호작용하는지 (예: "플레이어가 위에 탈 수 있어야 함", "적이 밟으면 죽어야 함")
3. 겉모습은 어떤지 (예: "기존 타일이랑 같은 느낌", "빨간색 가시") — 스프라이트 선택에 필요
4. 설정값 중 사용자가 신경쓰는 게 있는지 (예: "속도는 느리게", "데미지 3")

사용자가 "알아서 해"라고 하면, 위 항목에 대해 AI가 판단한 기본값을 제시하고 확인받는다.
"좋아", "ㅇㅇ" 같은 승인을 받은 후에만 다음 Step으로 진행한다.

---

## Step 1 — 어셈블리 판단

기능이 어디에 속하는지 판단한다:

| 질문 | 어셈블리 |
|---|---|
| 데이터 구조, 설정값, 이벤트 정의? | Platformer.Data |
| 순수 게임 규칙, 인터페이스? | Platformer.Core |
| 플레이어/적/월드 동작, 물리? | Platformer.Game |
| 화면에 보이는 UI 요소? | Platformer.UI |

Game ↔ UI 소통이 필요하면 → Data에 SO 이벤트 채널 추가.

참조 규칙:
- Data → 아무것도 참조 불가
- Core → Data만 참조 가능
- Game → Core, Data 참조 가능
- UI → Core, Data 참조 가능
- Game ↔ UI 직접 참조 절대 금지

---

## Step 2 — SO Settings 정의 (필요 시)

숫자 값(속도, 체력, 범위 등)이 있으면 반드시 SO로 분리한다. 하드코딩 금지.

### 2-1. SO 클래스 작성

`Assets/_Project/Scripts/Data/` 에 생성:

```csharp
// XxxSettings.cs
namespace Platformer.Data
{
    [CreateAssetMenu(fileName = "XxxSettings", menuName = "Platformer/Settings/Xxx")]
    public class XxxSettings : ScriptableObject
    {
        public float moveSpeed = 3f;
        public int maxHealth = 3;
    }
}
```

### 2-2. SO 인스턴스 생성

#### MCP 모드

script-execute로 인스턴스를 생성한다:

```csharp
using UnityEngine;
using UnityEditor;

public class Script
{
    public static object Main()
    {
        var so = ScriptableObject.CreateInstance<Platformer.Data.XxxSettings>();
        AssetDatabase.CreateAsset(so, "Assets/_Project/Datas/XxxSettings.asset");
        AssetDatabase.SaveAssets();
        return "XxxSettings.asset 생성 완료";
    }
}
```

#### 수동 모드

사용자에게 안내:
> Project 패널에서 Assets/_Project/Datas/ 폴더 우클릭 → Create → Platformer → Settings → Xxx 선택.

---

## Step 3 — 이벤트 채널 추가 (필요 시)

Game ↔ UI 소통이 필요하면 Data에 SO 이벤트 인스턴스를 추가한다.

### 3-1. GameEvent SO는 이미 있음

`Assets/_Project/Scripts/Data/GameEvent.cs` — 새 이벤트 타입이 필요하면 여기에 제네릭 버전 추가.

### 3-2. 이벤트 인스턴스 생성

#### MCP 모드

```csharp
using UnityEngine;
using UnityEditor;

public class Script
{
    public static object Main()
    {
        var evt = ScriptableObject.CreateInstance<Platformer.Data.GameEvent>();
        AssetDatabase.CreateAsset(evt, "Assets/_Project/Datas/Events/OnXxxHappened.asset");
        AssetDatabase.SaveAssets();
        return "이벤트 에셋 생성 완료";
    }
}
```

#### 수동 모드

> Assets/_Project/Datas/Events/ 폴더 우클릭 → Create → Platformer → Events → Game Event 선택.
> 이름을 OnXxxHappened 으로 변경.

---

## Step 4 — 스크립트 작성

이 단계는 MCP 모드/수동 모드 동일하다. 파일을 직접 작성한다.

규칙:
- 파일 위치 = 어셈블리 폴더 (예: `Assets/_Project/Scripts/Game/`)
- namespace = 어셈블리 이름 (예: `namespace Platformer.Game`)
- 1파일 1클래스
- SO Settings는 `[SerializeField]`로 주입, 생성자에서 하드코딩 금지
- Input은 반드시 Unity Input System 사용 (레거시 Input.GetKey 금지)

```csharp
// Assets/_Project/Scripts/Game/XxxController.cs
using UnityEngine;
using Platformer.Data;

namespace Platformer.Game
{
    public class XxxController : MonoBehaviour
    {
        [SerializeField] private XxxSettings _settings;

        void Awake()
        {
            // 초기화
        }
    }
}
```

---

## Step 5 — 컴파일 확인

```bash
unity-mcp-cli run-tool assets-refresh --input '{}'
grep -n "error CS" ~/Library/Logs/Unity/Editor.log | tail -30
```

에러가 있으면 수정 후 이 Step을 반복한다. 에러 0이 될 때까지 다음으로 넘어가지 않는다.

수동 모드에서도 이 명령은 실행 가능하다 (CLI만 있으면 됨).
CLI도 없으면 사용자에게 안내:
> Unity 에디터 하단 Console 창(Ctrl+Shift+C)에서 빨간 에러가 0개인지 확인해줘.

---

## Step 6 — 프리팹 생성/수정

MonoBehaviour를 만들었으면 프리팹이 필요하다.

화면에 보이는 오브젝트는 반드시 SpriteRenderer(또는 Tilemap)를 포함해야 한다.
콜라이더와 스크립트만 붙이고 스프라이트 없이 배치하면 안 된다 — 비개발자에게 "아무것도 안 보인다"는 최악의 경험이 된다.
기존 에셋(Assets/_Project/Arts/)에서 적절한 스프라이트를 찾아서 넣고, 마땅한 게 없으면 사용자에게 어떤 모양으로 할지 물어본다.

### MCP 모드

기존 프리팹에 컴포넌트 추가:

```bash
# 1. 프리팹 열기
unity-mcp-cli run-tool assets-prefab-open --input '{"path": "Assets/_Project/Prefabs/Xxx.prefab"}'

# 2. 루트 오브젝트 찾기
unity-mcp-cli run-tool gameobject-find --input '{"name": "Xxx"}'

# 3. 컴포넌트 추가
unity-mcp-cli run-tool gameobject-component-add --input '{"gameObjectPath": "/Xxx", "componentType": "Platformer.Game.XxxController"}'

# 4. 프리팹 저장 & 닫기
unity-mcp-cli run-tool assets-prefab-save --input '{}'
unity-mcp-cli run-tool assets-prefab-close --input '{}'
```

새 프리팹 생성:

```bash
# 1. 프리팹 생성
unity-mcp-cli run-tool assets-prefab-create --input '{"name": "Xxx", "savePath": "Assets/_Project/Prefabs/Xxx.prefab"}'

# 2. 필요한 컴포넌트 추가 (Rigidbody2D, Collider 등)
unity-mcp-cli run-tool gameobject-component-add --input '{"gameObjectPath": "/Xxx", "componentType": "UnityEngine.Rigidbody2D"}'
unity-mcp-cli run-tool gameobject-component-add --input '{"gameObjectPath": "/Xxx", "componentType": "UnityEngine.BoxCollider2D"}'
unity-mcp-cli run-tool gameobject-component-add --input '{"gameObjectPath": "/Xxx", "componentType": "Platformer.Game.XxxController"}'

# 3. 컴포넌트 속성 수정 (예: Rigidbody2D를 Static으로)
unity-mcp-cli run-tool gameobject-component-modify --input '{"gameObjectPath": "/Xxx", "componentType": "UnityEngine.Rigidbody2D", "properties": {"bodyType": 1}}'

# 4. 저장
unity-mcp-cli run-tool assets-prefab-save --input '{}'
unity-mcp-cli run-tool assets-prefab-close --input '{}'
```

### 수동 모드

사용자에게 안내:
> 1. Hierarchy 창에서 우클릭 → Create Empty. 이름을 "Xxx"로 변경.
> 2. Inspector 하단 Add Component 클릭 → "XxxController" 검색해서 추가.
> 3. 필요하면 Rigidbody2D, BoxCollider2D도 같은 방법으로 추가.
> 4. Hierarchy의 Xxx 오브젝트를 Project 패널의 Assets/_Project/Prefabs/ 폴더로 드래그.
> 5. Hierarchy에서 Xxx 오브젝트 삭제 (프리팹에서 인스턴스를 꺼내 쓸 거라서).

---

## Step 7 — Inspector 와이어링 (SO 슬롯 연결)

SerializeField 슬롯에 SO 인스턴스를 연결한다.

### MCP 모드

프리팹을 열고 컴포넌트의 SO 참조를 세팅한다:

```csharp
using UnityEngine;
using UnityEditor;

public class Script
{
    public static object Main()
    {
        // 프리팹 로드
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Xxx.prefab");
        var comp = prefab.GetComponent<Platformer.Game.XxxController>();

        // SO 로드
        var settings = AssetDatabase.LoadAssetAtPath<Platformer.Data.XxxSettings>("Assets/_Project/Datas/XxxSettings.asset");

        // SerializedObject로 private 필드 연결
        var so = new SerializedObject(comp);
        so.FindProperty("_settings").objectReferenceValue = settings;
        so.ApplyModifiedProperties();

        EditorUtility.SetDirty(prefab);
        AssetDatabase.SaveAssets();

        return "와이어링 완료";
    }
}
```

이벤트 채널도 같은 방식으로 연결:
```csharp
so.FindProperty("_onXxxHappened").objectReferenceValue =
    AssetDatabase.LoadAssetAtPath<Platformer.Data.GameEvent>("Assets/_Project/Datas/Events/OnXxxHappened.asset");
```

### 수동 모드

> 1. Project 패널에서 Assets/_Project/Prefabs/Xxx.prefab 더블클릭 (프리팹 편집 모드 진입).
> 2. Inspector에서 XxxController 컴포넌트의 "Settings" 슬롯 찾기.
> 3. Project 패널에서 Assets/_Project/Datas/XxxSettings 에셋을 해당 슬롯으로 드래그.
> 4. 이벤트 채널이 있으면 같은 방법으로 Assets/_Project/Datas/Events/ 에셋도 연결.
> 5. 상단 < 버튼으로 프리팹 편집 모드 나가기. 저장 확인 팝업 나오면 Save.

---

## Step 8 — 테스트 작성

새 기능에 대응하는 테스트를 작성한다. 이 단계는 MCP 모드/수동 모드 동일.

파일 위치: 해당 어셈블리의 Tests/ 폴더
- `Assets/_Project/Scripts/Data/Tests/`
- `Assets/_Project/Scripts/Core/Tests/`
- `Assets/_Project/Scripts/Game/Tests/`
- `Assets/_Project/Scripts/UI/Tests/`

```csharp
// Assets/_Project/Scripts/Game/Tests/XxxControllerTests.cs
using NUnit.Framework;
using UnityEngine;

namespace Platformer.Game.Tests
{
    public class XxxControllerTests
    {
        [Test]
        public void FeatureName_Condition_ExpectedResult()
        {
            var go = new GameObject();
            // 셋업 및 검증
            Object.DestroyImmediate(go);
        }
    }
}
```

테스트 실행:

```bash
unity-mcp-cli run-tool tests-run --input '{}'
```

CLI 없으면 사용자에게:
> Unity 메뉴 Window → General → Test Runner 열어서 Run All 클릭해줘.

전부 통과할 때까지 다음으로 넘어가지 않는다.

---

## Step 9 — 씬 배치 (필요 시)

프리팹을 씬에 배치해야 하는 경우.

### MCP 모드

```bash
# 프리팹 인스턴스를 씬에 배치
unity-mcp-cli run-tool assets-prefab-instantiate --input '{"path": "Assets/_Project/Prefabs/Xxx.prefab"}'

# 위치 조정이 필요하면
unity-mcp-cli run-tool gameobject-modify --input '{"gameObjectPath": "/Xxx(Clone)", "properties": {"transform.position": {"x": 5, "y": 2, "z": 0}}}'

# 씬 저장
unity-mcp-cli run-tool scene-save --input '{}'
```

### 수동 모드

> 1. Project 패널에서 Assets/_Project/Prefabs/Xxx.prefab을 씬(Scene 뷰 또는 Hierarchy)으로 드래그.
> 2. Scene 뷰에서 위치 조정. 또는 Inspector의 Transform 에서 Position 직접 입력.
> 3. Ctrl+S로 씬 저장.

---

## Step 10 — 최종 검증

```bash
# 컴파일
unity-mcp-cli run-tool assets-refresh --input '{}'
grep -n "error CS" ~/Library/Logs/Unity/Editor.log | tail -30

# 테스트
unity-mcp-cli run-tool tests-run --input '{}'

# 런타임 에러
grep -n "NullReferenceException\|MissingReferenceException" ~/Library/Logs/Unity/Editor.log | tail -20
```

체크리스트:
- [ ] 컴파일 에러 0
- [ ] 테스트 전부 통과
- [ ] 어셈블리 경계 위반 없음 (using 확인)
- [ ] SerializeField 슬롯 전부 연결됨 (null 참조 없음)
- [ ] 플레이 모드에서 동작 확인 (MCP면 에디터 플레이 → 로그 확인, 수동이면 사용자에게 확인 요청)

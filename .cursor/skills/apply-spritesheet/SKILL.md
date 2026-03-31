---
name: apply-spritesheet
description: 캐릭터/오브젝트 스프라이트 시트(PNG)를 임포트하고 슬라이싱 → 애니메이션 클립 → Animator → 씬 적용까지 처리한다. "시트 올렸어, 적용해줘" 요청 시 사용.
---

# 스프라이트 시트 적용

디자이너가 Arts/ 폴더에 PNG 시트를 넣으면, 슬라이싱부터 씬 적용까지 처리한다.

## 전제 조건

- PNG 파일이 Assets/_Project/Arts/Sprites/ 하위에 이미 있어야 함
- 에디트 모드여야 함 (플레이 모드 금지)

## Step 0 — MCP 가용 여부 확인

```bash
unity-mcp-cli run-tool assets-refresh --input '{}'
```

SUCCESS → MCP 모드, 실패 → 수동 모드.

---

## Step 1 — 임포트 설정

### MCP 모드

script-execute로 TextureImporter 세팅:

```csharp
using UnityEngine;
using UnityEditor;

public class Script
{
    public static object Main()
    {
        var path = "Assets/_Project/Arts/Sprites/Characters/MySheet.png";
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;

        if (importer == null) return $"파일 없음: {path}";

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple; // 시트니까
        importer.spritePixelsPerUnit = 16; // 프로젝트 기준 PPU
        importer.filterMode = FilterMode.Point; // 픽셀아트
        importer.textureCompression = TextureImporterCompression.Uncompressed;

        importer.SaveAndReimport();
        return $"임포트 설정 완료: {path}";
    }
}
```

PPU(Pixels Per Unit)는 프로젝트 기준 16. 반드시 맞출 것.

### 수동 모드

> 1. Project 패널에서 시트 PNG 파일 선택.
> 2. Inspector에서:
>    - Texture Type: Sprite (2D and UI)
>    - Sprite Mode: Multiple
>    - Pixels Per Unit: 16
>    - Filter Mode: Point (no filter)
>    - Compression: None
> 3. 하단 Apply 클릭.

---

## Step 2 — 슬라이싱 확인

### MCP 모드

이미 슬라이스 되어있는지 확인:

```csharp
using UnityEngine;
using UnityEditor;
using System.Linq;

public class Script
{
    public static object Main()
    {
        var path = "Assets/_Project/Arts/Sprites/Characters/MySheet.png";
        var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();

        if (sprites.Length == 0)
            return "슬라이싱 안 됨 — Sprite Editor에서 수동 슬라이싱 필요";

        var names = string.Join(", ", sprites.Select(s => s.name));
        return $"슬라이스 {sprites.Length}개 확인: {names}";
    }
}
```

슬라이스 안 되어있으면 → 사용자에게 Sprite Editor 슬라이싱 요청:
> Sprite Editor 열기: Project 패널에서 시트 선택 → Inspector 하단 Sprite Editor 버튼 클릭.
> Slice 드롭다운 → Type: Grid By Cell Size → 셀 크기 입력 (예: 16x16, 32x32) → Slice → Apply.

에셋팩처럼 이미 슬라이스 되어있으면 이 단계 스킵.

### 수동 모드

> 1. Project 패널에서 시트 PNG 선택 → Inspector 하단 Sprite Editor 클릭.
> 2. 왼쪽 상단 Slice 드롭다운 클릭.
> 3. Type: Grid By Cell Size 선택, 셀 크기 입력 (캐릭터 한 프레임 크기).
> 4. Slice 클릭 → 우측 상단 Apply.
> 5. 닫기.

---

## Step 3 — 스프라이트 이름 파악

### MCP 모드

슬라이스된 스프라이트 이름에서 애니메이션 그룹을 분류:

```csharp
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

public class Script
{
    public static object Main()
    {
        var path = "Assets/_Project/Arts/Sprites/Characters/MySheet.png";
        var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();

        // 이름 패턴: "SheetName_0", "SheetName_1" 또는 "Idle 0", "Run 0" 등
        var groups = new Dictionary<string, int>();
        foreach (var s in sprites)
        {
            // 이름에서 상태명 추출 (공백/언더스코어 앞부분)
            var parts = s.name.Split(' ', '_');
            var state = parts.Length > 1 ? parts[parts.Length - 2] : "Default";
            if (int.TryParse(parts.Last(), out _))
            {
                if (!groups.ContainsKey(state)) groups[state] = 0;
                groups[state]++;
            }
        }

        var result = string.Join("\n", groups.Select(g => $"{g.Key}: {g.Value} frames"));
        return $"애니메이션 그룹:\n{result}";
    }
}
```

이 결과로 어떤 클립을 만들지 판단한다.

### 수동 모드

> Sprite Editor에서 각 슬라이스의 이름을 확인해줘.
> 예: "Idle 0", "Idle 1", "Run 0", "Run 1", "Run 2" 이런 식이면
> → Idle (2프레임), Run (3프레임)으로 그룹이 나뉘는 거야.

---

## Step 4 — 애니메이션 클립 생성

### MCP 모드

각 그룹별로 AnimationClip 생성:

```csharp
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

public class Script
{
    public static object Main()
    {
        var sheetPath = "Assets/_Project/Arts/Sprites/Characters/MySheet.png";
        var outputDir = "Assets/_Project/Arts/Animations/Player";

        // 출력 폴더 확인
        if (!AssetDatabase.IsValidFolder(outputDir))
        {
            var parts = outputDir.Split('/');
            var current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        var allSprites = AssetDatabase.LoadAllAssetsAtPath(sheetPath)
            .OfType<Sprite>().OrderBy(s => s.name).ToArray();

        // 그룹 분류 (이름 패턴에 맞게 수정할 것)
        var groups = new Dictionary<string, List<Sprite>>();
        foreach (var s in allSprites)
        {
            var parts = s.name.Split(' ', '_');
            var state = parts.Length > 1 ? parts[parts.Length - 2] : "Default";
            if (!groups.ContainsKey(state)) groups[state] = new List<Sprite>();
            groups[state].Add(s);
        }

        // FPS 기준
        var fpsMap = new Dictionary<string, float>
        {
            { "Idle", 6f }, { "Run", 10f }, { "Jump", 8f },
            { "Fall", 8f }, { "Attack", 12f }, { "Death", 8f }
        };

        // 루프 여부
        var loopStates = new HashSet<string> { "Idle", "Run" };

        var created = new List<string>();
        foreach (var group in groups)
        {
            var clip = new AnimationClip();
            clip.frameRate = fpsMap.ContainsKey(group.Key) ? fpsMap[group.Key] : 8f;

            var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
            var keyframes = new ObjectReferenceKeyframe[group.Value.Count];
            for (int i = 0; i < group.Value.Count; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = i / clip.frameRate,
                    value = group.Value[i]
                };
            }
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loopStates.Contains(group.Key);
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            var clipPath = $"{outputDir}/Player_{group.Key}.anim";
            AssetDatabase.CreateAsset(clip, clipPath);
            created.Add($"{group.Key} ({group.Value.Count}f)");
        }

        AssetDatabase.SaveAssets();
        return $"클립 생성 완료: {string.Join(", ", created)}";
    }
}
```

권장 FPS: Idle 4~6, Run 8~10, Jump 6~8, Attack 10~12.

### 수동 모드

> 1. Project 패널에서 Assets/_Project/Arts/Animations/Player/ 폴더로 이동 (없으면 생성).
> 2. 폴더 우클릭 → Create → Animation Clip. 이름을 Player_Idle 으로.
> 3. Hierarchy에서 Player 오브젝트 선택 → Window → Animation → Animation 창 열기.
> 4. Animation 창 좌측 상단에서 방금 만든 클립 선택.
> 5. 타임라인에 스프라이트를 프레임 순서대로 드래그해서 배치.
> 6. Samples(FPS)를 Idle은 6, Run은 10 정도로 설정.
> 7. 각 상태(Run, Jump 등)에 대해 2~6번 반복.

---

## Step 5 — Animator Controller 생성/업데이트

### MCP 모드

```csharp
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class Script
{
    public static object Main()
    {
        var controllerPath = "Assets/_Project/Arts/Animations/Player/PlayerAnimator.controller";
        var animDir = "Assets/_Project/Arts/Animations/Player";

        // 기존 컨트롤러 확인
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        }

        // 파라미터 (중복 추가 방지)
        bool hasSpeed = false, hasGrounded = false;
        foreach (var p in controller.parameters)
        {
            if (p.name == "Speed") hasSpeed = true;
            if (p.name == "IsGrounded") hasGrounded = true;
        }
        if (!hasSpeed) controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        if (!hasGrounded) controller.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);

        // 상태 머신
        var rootSM = controller.layers[0].stateMachine;

        // 클립 로드 + 상태 추가
        var clips = new[] { "Player_Idle", "Player_Run", "Player_Jump", "Player_Fall" };
        foreach (var clipName in clips)
        {
            var clipPath = $"{animDir}/{clipName}.anim";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if (clip == null) continue;

            // 기존 상태 확인
            bool exists = false;
            foreach (var state in rootSM.states)
            {
                if (state.state.name == clipName) { exists = true; break; }
            }
            if (!exists)
            {
                var state = rootSM.AddState(clipName);
                state.motion = clip;
            }
        }

        AssetDatabase.SaveAssets();
        return "Animator Controller 설정 완료 — 전환 조건은 수동 확인 필요";
    }
}
```

파라미터 이름은 PlayerController.cs의 SetFloat/SetBool과 일치해야 한다:
- "Speed" — 이동 속도 (0이면 Idle, 0 초과면 Run)
- "IsGrounded" — 지면 접촉 (false면 Jump/Fall)

전환(Transition) 조건은 복잡하므로, 상태 추가 후 Animator 창에서 전환을 확인/수정하도록 사용자에게 안내한다.

### 수동 모드

> 1. Assets/_Project/Arts/Animations/Player/ 폴더 우클릭 → Create → Animator Controller. 이름: PlayerAnimator.
> 2. 더블클릭해서 Animator 창 열기.
> 3. Parameters 탭에서 + → Float → "Speed" 추가, + → Bool → "IsGrounded" 추가.
> 4. 만든 .anim 클립들을 Animator 창으로 드래그 (상태 자동 생성).
> 5. 상태 간 전환 설정:
>    - Idle → Run: Speed > 0.01
>    - Run → Idle: Speed < 0.01
>    - Any State → Jump: IsGrounded = false
>    - Jump → Idle: IsGrounded = true

---

## Step 6 — 씬에 적용

### MCP 모드

```csharp
using UnityEngine;
using UnityEditor;

public class Script
{
    public static object Main()
    {
        var player = GameObject.Find("Player");
        if (player == null) return "Player 오브젝트 없음";

        // 기본 스프라이트 할당
        var idleSprite = AssetDatabase.LoadAllAssetsAtPath("Assets/_Project/Arts/Sprites/Characters/MySheet.png");
        foreach (var a in idleSprite)
        {
            if (a is Sprite s && s.name.Contains("Idle"))
            {
                var sr = player.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sprite = s;
                    sr.color = Color.white;
                }
                break;
            }
        }

        // URP 보라색 방지
        var renderer = player.GetComponent<SpriteRenderer>();
        if (renderer != null && renderer.sharedMaterial != null)
        {
            if (renderer.sharedMaterial.shader.name != "Sprites/Default"
                && renderer.sharedMaterial.shader.name != "Universal Render Pipeline/2D/Sprite-Lit-Default")
            {
                renderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
            }
        }

        // Animator 할당
        var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
            "Assets/_Project/Arts/Animations/Player/PlayerAnimator.controller");
        if (controller != null)
        {
            var anim = player.GetComponent<Animator>();
            if (anim == null) anim = player.AddComponent<Animator>();
            anim.runtimeAnimatorController = controller;
        }

        EditorUtility.SetDirty(player);
        return "씬 적용 완료";
    }
}
```

### 수동 모드

> 1. Hierarchy에서 Player 선택.
> 2. SpriteRenderer의 Sprite 슬롯에 Idle 첫 프레임 스프라이트 드래그.
> 3. 색상이 보라색이면: Material 슬롯을 Sprites/Default 로 변경.
> 4. Animator 컴포넌트가 없으면 Add Component → Animator.
> 5. Controller 슬롯에 PlayerAnimator.controller 드래그.
> 6. Ctrl+S 로 씬 저장.

---

## Step 7 — 검증

```bash
unity-mcp-cli run-tool assets-refresh --input '{}'
grep -n "error CS" ~/Library/Logs/Unity/Editor.log | tail -30
```

체크리스트:
- [ ] 컴파일 에러 0
- [ ] 씬에서 스프라이트가 보라색 아닌 정상 색상
- [ ] 플레이 모드에서 Idle 애니메이션 재생
- [ ] 이동 시 Run 애니메이션 전환
- [ ] 점프 시 Jump 애니메이션 전환
- [ ] 좌우 반전 정상 작동

## 주의사항

- PPU가 기존과 다르면 캐릭터 크기가 달라진다. 반드시 16으로 맞출 것
- 시트 구조(프레임 수, 이름)가 바뀌면 애니메이션 클립을 새로 만들어야 한다
- 기존 시트를 같은 파일명으로 덮어쓰면 참조가 유지된다 (Unity GUID 기반)

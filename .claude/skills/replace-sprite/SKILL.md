---
name: replace-sprite
description: 기존 스프라이트를 새 이미지로 교체한다. 임포트 설정부터 참조 교체까지 AI가 처리한다.
---

# 스프라이트 교체

스프라이트 교체 요청을 받으면 이 절차를 순서대로 실행한다.

## 전제 조건

- 새 이미지 파일(PNG)이 이미 프로젝트 폴더에 있어야 함
- 에디트 모드여야 함

## Step 0 — MCP 가용 여부 확인

```bash
unity-mcp-cli run-tool assets-refresh --input '{}'
```

SUCCESS → MCP 모드, 실패 → 수동 모드.

---

## Step 1 — 파일 배치 확인

새 이미지가 올바른 폴더에 있는지 확인한다:

| 용도 | 경로 |
|---|---|
| 캐릭터 | Assets/_Project/Arts/Sprites/Characters/ |
| 배경/타일 | Assets/_Project/Arts/Tilesets/ |
| VFX | Assets/_Project/Arts/VFX/ |
| UI | Assets/_Project/Arts/Sprites/UI/ |

```bash
# 파일 존재 확인
ls Assets/_Project/Arts/Sprites/Characters/NewSprite.png
```

파일이 없으면 사용자에게 파일 경로 확인 요청.

---

## Step 2 — 임포트 설정

### MCP 모드

script-execute로 TextureImporter 세팅:

```csharp
using UnityEngine;
using UnityEditor;

public class Script
{
    public static object Main()
    {
        var path = "Assets/_Project/Arts/Sprites/Characters/NewSprite.png";
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;

        if (importer == null)
            return $"파일 없음: {path}";

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single; // 단일 스프라이트
        importer.spritePixelsPerUnit = 16; // 프로젝트 기준 PPU
        importer.filterMode = FilterMode.Point; // 픽셀아트
        importer.textureCompression = TextureImporterCompression.Uncompressed;

        importer.SaveAndReimport();
        return $"임포트 설정 완료: {path}";
    }
}
```

시트(Multiple)면 spriteImportMode를 SpriteImportMode.Multiple로 변경.

### 수동 모드

> 1. Project 패널에서 해당 이미지 파일 선택.
> 2. Inspector에서 확인:
>    - Texture Type: Sprite (2D and UI)
>    - Sprite Mode: Single (단일) 또는 Multiple (시트)
>    - Pixels Per Unit: 16 (프로젝트 기준)
>    - Filter Mode: Point (no filter)
>    - Compression: None
> 3. 하단 Apply 클릭.

---

## Step 3 — 참조 교체

### 3-A. SpriteRenderer 교체

#### MCP 모드

```csharp
using UnityEngine;
using UnityEditor;

public class Script
{
    public static object Main()
    {
        var newSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Arts/Sprites/Characters/NewSprite.png");
        if (newSprite == null) return "스프라이트 로드 실패";

        var target = GameObject.Find("Player");
        if (target == null) return "Player 오브젝트 없음";

        var sr = target.GetComponent<SpriteRenderer>();
        sr.sprite = newSprite;
        sr.color = Color.white; // 더미 색상 리셋

        EditorUtility.SetDirty(target);
        return "SpriteRenderer 교체 완료";
    }
}
```

프리팹 수정이 필요하면 프리팹 열기 → 수정 → 저장:

```csharp
using UnityEngine;
using UnityEditor;

public class Script
{
    public static object Main()
    {
        var prefabPath = "Assets/_Project/Prefabs/Player.prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        var contents = PrefabUtility.LoadPrefabContents(prefabPath);

        var sr = contents.GetComponent<SpriteRenderer>();
        var newSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Arts/Sprites/Characters/NewSprite.png");
        sr.sprite = newSprite;

        PrefabUtility.SaveAsPrefabAsset(contents, prefabPath);
        PrefabUtility.UnloadPrefabContents(contents);

        return "프리팹 스프라이트 교체 완료";
    }
}
```

#### 수동 모드

> 씬에서 해당 오브젝트 선택 → Inspector의 SpriteRenderer 컴포넌트에서 Sprite 슬롯에 새 스프라이트를 드래그.
> 프리팹이면: Project 패널에서 프리팹 더블클릭 → 같은 방법으로 교체 → 상단 < 버튼으로 나가면서 저장.

### 3-B. 애니메이션 클립 교체

#### MCP 모드

```csharp
using UnityEngine;
using UnityEditor;
using System.Linq;

public class Script
{
    public static object Main()
    {
        var clipPath = "Assets/_Project/Arts/Animations/Player/Player_Idle.anim";
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
        if (clip == null) return "클립 없음";

        // 새 스프라이트 로드
        var sprites = AssetDatabase.LoadAllAssetsAtPath("Assets/_Project/Arts/Sprites/Characters/NewSheet.png")
            .OfType<Sprite>()
            .Where(s => s.name.StartsWith("Idle"))
            .OrderBy(s => s.name)
            .ToArray();

        var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        var keyframes = new ObjectReferenceKeyframe[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe
            {
                time = i / clip.frameRate,
                value = sprites[i]
            };
        }
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

        EditorUtility.SetDirty(clip);
        AssetDatabase.SaveAssets();
        return $"애니메이션 클립 교체 완료: {sprites.Length} 프레임";
    }
}
```

#### 수동 모드

> 1. Project 패널에서 .anim 파일 더블클릭 → Animation 창 열기.
> 2. 프레임별 스프라이트 키프레임 선택.
> 3. 새 스프라이트로 교체.

### 3-C. Tilemap 타일 교체

#### MCP 모드

```csharp
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class Script
{
    public static object Main()
    {
        var newSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Arts/Tilesets/NewTile.png");
        var tilePath = "Assets/_Project/Arts/Tilesets/Tile_Ground.asset";
        var tile = AssetDatabase.LoadAssetAtPath<Tile>(tilePath);

        tile.sprite = newSprite;
        EditorUtility.SetDirty(tile);
        AssetDatabase.SaveAssets();

        return "타일 스프라이트 교체 완료 (씬의 타일맵에 자동 반영)";
    }
}
```

#### 수동 모드

> 1. Project 패널에서 Tile 에셋(.asset) 선택.
> 2. Inspector에서 Sprite 슬롯에 새 스프라이트 드래그.
> 3. 이미 깔린 타일은 자동으로 반영됨.

---

## Step 4 — 검증

```bash
unity-mcp-cli run-tool assets-refresh --input '{}'
grep -n "error CS\|MissingReferenceException" ~/Library/Logs/Unity/Editor.log | tail -20
```

체크리스트:
- [ ] 씬에서 스프라이트가 정상 표시 (보라색 아님)
- [ ] 크기/비율이 기존과 동일 (PPU 16 확인)
- [ ] 애니메이션 정상 재생
- [ ] 콜라이더가 있다면 외형과 맞는지 확인

## 주의사항

- 같은 파일명으로 덮어쓰는 게 가장 안전 (Unity GUID 참조 유지)
- 파일명을 바꾸면 기존 참조가 끊긴다
- PPU가 기존과 다르면 크기가 달라진다. 반드시 16으로 맞출 것
- 시트 구조(프레임 수, 이름)가 바뀌면 애니메이션 클립 재세팅 필요

---
name: apply-tileset
description: 타일셋 스프라이트 시트(PNG)를 임포트하고 슬라이싱 → Tile 에셋 생성 → 타일맵 적용까지 처리한다. "타일셋 올렸어, 적용해줘" 요청 시 사용.
---

# 타일셋 적용

디자이너가 타일셋 시트 PNG를 넣으면, Tile 에셋 생성부터 타일맵 배치까지 처리한다.

## 전제 조건

- PNG 파일이 Assets/_Project/Arts/Tilesets/ 또는 에셋팩 폴더에 있어야 함
- 에디트 모드여야 함 (플레이 모드 금지)

## Step 0 — MCP 가용 여부 확인

```bash
unity-mcp-cli run-tool assets-refresh --input '{}'
```

SUCCESS → MCP 모드, 실패 → 수동 모드.

---

## Step 1 — 임포트 설정

### MCP 모드

```csharp
using UnityEngine;
using UnityEditor;

public class Script
{
    public static object Main()
    {
        var path = "Assets/_Project/Arts/Tilesets/MyTiles.png";
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;

        if (importer == null) return $"파일 없음: {path}";

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = 16; // 프로젝트 기준 PPU
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;

        importer.SaveAndReimport();
        return $"임포트 설정 완료: {path}";
    }
}
```

### 수동 모드

> 1. Project 패널에서 타일셋 PNG 선택.
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

```csharp
using UnityEngine;
using UnityEditor;
using System.Linq;

public class Script
{
    public static object Main()
    {
        var path = "Assets/_Project/Arts/Tilesets/MyTiles.png";
        var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();

        if (sprites.Length == 0)
            return "슬라이싱 안 됨 — Sprite Editor에서 수동 슬라이싱 필요";

        var names = string.Join(", ", sprites.Select(s => s.name));
        return $"슬라이스 {sprites.Length}개: {names}";
    }
}
```

슬라이스 안 되어있으면 사용자에게 요청:
> Project 패널에서 타일셋 PNG 선택 → Inspector 하단 Sprite Editor 클릭.
> Slice → Type: Grid By Cell Size → 16x16 (프로젝트 기준) → Slice → Apply.

### 수동 모드

> 1. Project 패널에서 타일셋 PNG 선택 → Inspector 하단 Sprite Editor 클릭.
> 2. Slice → Type: Grid By Cell Size → 셀 크기 16x16 입력.
> 3. Slice 클릭 → Apply.

---

## Step 3 — 스프라이트 이름 패턴 파악

### MCP 모드

```csharp
using UnityEngine;
using UnityEditor;
using System.Linq;

public class Script
{
    public static object Main()
    {
        var path = "Assets/_Project/Arts/Tilesets/MyTiles.png";
        var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();

        var names = string.Join("\n", sprites.Select(s => $"  {s.name} ({s.rect.width}x{s.rect.height})"));
        return $"타일 스프라이트 목록:\n{names}";
    }
}
```

이름으로 용도 분류:
- Top/Middle/Bottom + Left/Center/Right → 두꺼운 바닥/벽용 (9-slice)
- Single + Left/Center/Right → 얇은 공중 플랫폼용
- Slope Left/Right → 경사면

### 수동 모드

> Sprite Editor에서 각 슬라이스 이름을 확인해줘.
> 이름 패턴으로 용도를 분류할 수 있어.

---

## Step 4 — Tile 에셋 생성

### MCP 모드

각 스프라이트마다 Tile 에셋(.asset) 생성:

```csharp
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using System.Linq;

public class Script
{
    public static object Main()
    {
        var sheetPath = "Assets/_Project/Arts/Tilesets/MyTiles.png";
        var tileDir = "Assets/_Project/Arts/Tilesets";

        var sprites = AssetDatabase.LoadAllAssetsAtPath(sheetPath).OfType<Sprite>().ToArray();
        if (sprites.Length == 0) return "스프라이트 없음";

        int created = 0;
        foreach (var sprite in sprites)
        {
            var tileName = "Tile_" + sprite.name.Replace(" ", "");
            var tilePath = $"{tileDir}/{tileName}.asset";

            // 이미 있으면 스킵
            if (AssetDatabase.LoadAssetAtPath<Tile>(tilePath) != null) continue;

            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprite;
            tile.colliderType = Tile.ColliderType.Grid; // 기본 사각 콜라이더

            AssetDatabase.CreateAsset(tile, tilePath);
            created++;
        }

        AssetDatabase.SaveAssets();
        return $"Tile 에셋 {created}개 생성 (기존 것은 스킵)";
    }
}
```

colliderType 옵션:
- Grid: 사각형 콜라이더 (일반 타일)
- Sprite: 스프라이트 외형대로 (경사면)
- None: 콜라이더 없음 (배경 장식)

### 수동 모드

> Tile 에셋은 수동으로 만들기 번거로우니, Tile Palette를 쓰는 게 낫다.
> 1. Window → 2D → Tile Palette 열기.
> 2. Create New Palette → 이름 입력 → Create.
> 3. Project 패널에서 슬라이스된 스프라이트를 Tile Palette 창으로 드래그.
> 4. 저장 위치를 Assets/_Project/Arts/Tilesets/ 로 지정.
> 5. Tile 에셋이 자동 생성된다.

---

## Step 5 — 타일맵 오브젝트 확인/생성

### MCP 모드

씬에 Grid + Tilemap이 없으면 생성:

```csharp
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class Script
{
    public static object Main()
    {
        // Grid 확인
        var grid = GameObject.Find("Grid");
        if (grid == null)
        {
            grid = new GameObject("Grid");
            grid.AddComponent<Grid>().cellSize = new Vector3(1f, 1f, 0f);
        }

        // Tilemap_Ground 확인
        var groundTF = grid.transform.Find("Tilemap_Ground");
        GameObject groundGO;
        if (groundTF == null)
        {
            groundGO = new GameObject("Tilemap_Ground");
            groundGO.transform.SetParent(grid.transform);
        }
        else
        {
            groundGO = groundTF.gameObject;
        }

        // 필수 컴포넌트
        if (groundGO.GetComponent<Tilemap>() == null)
            groundGO.AddComponent<Tilemap>();
        if (groundGO.GetComponent<TilemapRenderer>() == null)
            groundGO.AddComponent<TilemapRenderer>();
        if (groundGO.GetComponent<TilemapCollider2D>() == null)
        {
            var col = groundGO.AddComponent<TilemapCollider2D>();
            col.compositeOperation = Collider2D.CompositeOperation.Merge;
        }
        if (groundGO.GetComponent<Rigidbody2D>() == null)
        {
            var rb = groundGO.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
        }
        if (groundGO.GetComponent<CompositeCollider2D>() == null)
            groundGO.AddComponent<CompositeCollider2D>();

        // 레이어 설정
        groundGO.layer = LayerMask.NameToLayer("Ground");

        EditorUtility.SetDirty(groundGO);
        return "타일맵 오브젝트 준비 완료";
    }
}
```

CompositeCollider2D가 없으면 타일 이음새에서 플레이어가 걸린다. 반드시 추가.

### 수동 모드

> 씬에 Grid 오브젝트가 없으면:
> 1. Hierarchy 우클릭 → 2D Object → Tilemap → Rectangular.
> 2. 자동으로 Grid + Tilemap 오브젝트가 생성된다.
> 3. Tilemap 오브젝트 이름을 Tilemap_Ground 으로 변경.
> 4. Inspector에서 Add Component:
>    - TilemapCollider2D (Composite Operation: Merge)
>    - Rigidbody2D (Body Type: Static)
>    - CompositeCollider2D
> 5. Layer를 Ground 로 설정 (없으면 Add Layer에서 먼저 추가).

---

## Step 6 — 타일 배치

### MCP 모드

타일맵에 타일을 배치한다. 레벨 디자인에 맞게 좌표를 조정:

```csharp
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using System.Linq;

public class Script
{
    public static object Main()
    {
        var tilemap = GameObject.Find("Tilemap_Ground")?.GetComponent<Tilemap>();
        if (tilemap == null) return "Tilemap_Ground 없음";

        var tileDir = "Assets/_Project/Arts/Tilesets";

        // 타일 로드 (이름으로 매칭)
        var tTop = AssetDatabase.LoadAssetAtPath<Tile>($"{tileDir}/Tile_Top.asset");
        var tMiddle = AssetDatabase.LoadAssetAtPath<Tile>($"{tileDir}/Tile_Middle.asset");

        // 바닥 배치 예시: x=-10~10, 상단 y=0, 하단 y=-1
        if (tTop != null)
        {
            for (int x = -10; x <= 10; x++)
                tilemap.SetTile(new Vector3Int(x, 0, 0), tTop);
        }
        if (tMiddle != null)
        {
            for (int x = -10; x <= 10; x++)
                tilemap.SetTile(new Vector3Int(x, -1, 0), tMiddle);
        }

        EditorUtility.SetDirty(tilemap);
        return "타일 배치 완료";
    }
}
```

타일 이름과 좌표는 실제 레벨 디자인에 맞게 수정한다.

### 기존 타일 교체 (MCP 모드)

```csharp
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using System.Collections.Generic;

public class Script
{
    public static object Main()
    {
        var tilemap = GameObject.Find("Tilemap_Ground")?.GetComponent<Tilemap>();
        if (tilemap == null) return "Tilemap_Ground 없음";

        // 새 타일 매핑 (기존 이름 → 새 타일 경로)
        var mapping = new Dictionary<string, string>
        {
            { "Tile_OldTop", "Assets/_Project/Arts/Tilesets/Tile_NewTop.asset" },
            { "Tile_OldMiddle", "Assets/_Project/Arts/Tilesets/Tile_NewMiddle.asset" },
        };

        int replaced = 0;
        var bounds = tilemap.cellBounds;
        foreach (var pos in bounds.allPositionsWithin)
        {
            var oldTile = tilemap.GetTile(pos) as Tile;
            if (oldTile == null) continue;

            if (mapping.TryGetValue(oldTile.name, out var newPath))
            {
                var newTile = AssetDatabase.LoadAssetAtPath<Tile>(newPath);
                if (newTile != null)
                {
                    tilemap.SetTile(pos, newTile);
                    replaced++;
                }
            }
        }

        EditorUtility.SetDirty(tilemap);
        return $"타일 {replaced}개 교체 완료";
    }
}
```

### 수동 모드

> 1. Window → 2D → Tile Palette 열기.
> 2. 왼쪽 상단 드롭다운에서 사용할 Palette 선택.
> 3. 브러시 도구(B)로 타일 선택 → Scene 뷰에서 클릭/드래그해서 배치.
> 4. 지우개(D)로 잘못 놓은 타일 제거.
> 5. 넓은 영역은 Box Fill(U)로 채우기.
> 6. Ctrl+S 로 씬 저장.

---

## Step 7 — 검증

```bash
unity-mcp-cli run-tool assets-refresh --input '{}'
grep -n "error CS" ~/Library/Logs/Unity/Editor.log | tail -30
```

체크리스트:
- [ ] Tile 에셋이 Assets/_Project/Arts/Tilesets/ 에 생성됨
- [ ] 타일맵에 타일이 정상 표시 (보라색 아님)
- [ ] Tilemap_Ground 레이어가 Ground
- [ ] CompositeCollider2D 있음 (이음새 걸림 방지)
- [ ] 플레이 모드에서 플레이어가 타일 위를 정상 이동
- [ ] 플랫폼 끝에서 떨어지기 가능

## 주의사항

- PPU가 기존 타일셋과 다르면 타일 크기가 안 맞는다. 반드시 16으로 통일
- 타일맵에 CompositeCollider2D 없으면 이음새에서 캐릭터가 걸린다
- 기존 타일셋을 같은 파일명으로 덮어쓰면 Tile 에셋이 자동 업데이트된다
- 배경용 타일맵은 별도 오브젝트로 분리 (Tilemap_Background, 콜라이더 없이)

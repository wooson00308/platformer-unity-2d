---
name: apply-tileset
description: 타일셋 스프라이트 시트(PNG)를 임포트하고 슬라이싱 → Tile 에셋 생성 → 타일맵 적용까지 자동 처리. "타일셋 올렸어, 적용해줘" 요청 시 사용.
---

# 타일셋 적용

디자이너가 타일셋 시트 PNG를 넣으면, Tile 에셋 생성부터 타일맵 배치까지 처리하는 절차.

## 전제 조건

- PNG 파일이 Assets/_Project/Arts/Tilesets/ 또는 에셋팩 폴더에 있어야 함
- 에디트 모드여야 함 (플레이 모드 금지)

## Step 1 — 임포트 설정 확인/수정

```csharp
var path = "Assets/_Project/Arts/Tilesets/MyTiles.png";
var importer = AssetImporter.GetAtPath(path) as TextureImporter;

importer.textureType = TextureImporterType.Sprite;
importer.spriteImportMode = SpriteImportMode.Multiple;
importer.spritePixelsPerUnit = 16; // 프로젝트 기준 PPU
importer.filterMode = FilterMode.Point;
importer.textureCompression = TextureImporterCompression.Uncompressed;

importer.SaveAndReimport();
```

## Step 2 — 슬라이싱 확인

```csharp
var allAssets = AssetDatabase.LoadAllAssetsAtPath(path);
var sprites = new List<Sprite>();
foreach (var a in allAssets)
    if (a is Sprite sp) sprites.Add(sp);
```

슬라이스가 안 되어있으면 Grid 기반 자동 슬라이싱 필요.
이미 슬라이스 되어있으면 스킵.

## Step 3 — 스프라이트 이름 파악

타일셋 스프라이트 이름 패턴을 확인:
```
예: "Top Left", "Top", "Top Right", "Middle Left", "Middle", ...
→ 9-slice 패턴 (3x3 그리드: Top/Middle/Bottom x Left/Center/Right)

예: "Single Left", "Single", "Single Right"
→ 1행 플랫폼용
```

이름으로 용도를 분류한다:
- Top/Middle/Bottom + Left/Center/Right → 두꺼운 바닥/벽용
- Single + Left/Center/Right → 얇은 공중 플랫폼용
- Slope Left/Right → 경사면

## Step 4 — Tile 에셋 생성

각 스프라이트마다 Tile 에셋(.asset) 생성:

```csharp
var tileDir = "Assets/_Project/Arts/Tilesets";

foreach (var sprite in sprites)
{
    var tile = ScriptableObject.CreateInstance<Tile>();
    tile.sprite = sprite;
    tile.colliderType = Tile.ColliderType.Grid; // 콜라이더 자동 생성

    var tileName = "Tile_" + sprite.name.Replace(" ", "");
    AssetDatabase.CreateAsset(tile, $"{tileDir}/{tileName}.asset");
}
AssetDatabase.SaveAssets();
```

colliderType 옵션:
- Grid: 사각형 콜라이더 (일반 타일)
- Sprite: 스프라이트 외형대로 (경사면 등)
- None: 콜라이더 없음 (배경 장식)

## Step 5 — 타일맵 오브젝트 확인/생성

씬에 Grid + Tilemap이 없으면 생성:

```csharp
var grid = GameObject.Find("Grid");
if (grid == null)
{
    grid = new GameObject("Grid");
    grid.AddComponent<Grid>().cellSize = new Vector3(1f, 1f, 0f);
}

var tilemapGO = GameObject.Find("Tilemap_Ground");
if (tilemapGO == null)
{
    tilemapGO = new GameObject("Tilemap_Ground");
    tilemapGO.transform.SetParent(grid.transform);
    tilemapGO.AddComponent<Tilemap>();
    tilemapGO.AddComponent<TilemapRenderer>();
}
```

필수 컴포넌트 (없으면 추가):
```
Tilemap_Ground:
  - Tilemap
  - TilemapRenderer
  - TilemapCollider2D (compositeOperation = Merge)
  - CompositeCollider2D
  - Rigidbody2D (Static)
  - Layer: Ground
```

CompositeCollider2D가 없으면 타일 이음새에서 플레이어가 걸린다. 반드시 추가.

## Step 6 — 타일 배치

타일맵에 타일 배치. 용도별 패턴:

### 바닥 (넓은 땅)
```csharp
// 상단 행: TopLeft, Top..., TopRight
// 하단 행: BotLeft, Bot..., BotRight
for (int x = startX; x <= endX; x++)
{
    Tile topTile = (x == startX) ? tTopLeft : (x == endX) ? tTopRight : tTop;
    Tile botTile = (x == startX) ? tBotLeft : (x == endX) ? tBotRight : tBot;

    tilemap.SetTile(new Vector3Int(x, topY, 0), topTile);
    tilemap.SetTile(new Vector3Int(x, topY - 1, 0), botTile);
}
```

### 공중 플랫폼 (1행)
```csharp
// SingleLeft, Single..., SingleRight
tilemap.SetTile(new Vector3Int(startX, y, 0), tSingleLeft);
for (int x = startX + 1; x < endX; x++)
    tilemap.SetTile(new Vector3Int(x, y, 0), tSingle);
tilemap.SetTile(new Vector3Int(endX, y, 0), tSingleRight);
```

### 기존 타일 교체
새 타일셋으로 교체할 때는 기존 타일을 순회하며 대응하는 새 타일로 교체:
```csharp
var bounds = tilemap.cellBounds;
foreach (var pos in bounds.allPositionsWithin)
{
    var oldTile = tilemap.GetTile(pos);
    if (oldTile != null)
    {
        // 이름 매칭으로 대응하는 새 타일 찾기
        var newTile = FindMatchingTile(oldTile.name, newTiles);
        if (newTile != null)
            tilemap.SetTile(pos, newTile);
    }
}
```

## Step 7 — 검증

```bash
unity-mcp-cli run-tool assets-refresh --input '{}'
grep -n "error CS" ~/Library/Logs/Unity/Editor.log | tail -30
```

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

---
name: apply-spritesheet
description: 캐릭터/오브젝트 스프라이트 시트(PNG)를 임포트하고 슬라이싱 → 애니메이션 클립 → Animator → 씬 적용까지 자동 처리. "시트 올렸어, 적용해줘" 요청 시 사용.
---

# 스프라이트 시트 적용

디자이너가 Arts/ 폴더에 PNG 시트를 넣으면, 슬라이싱부터 씬 적용까지 전부 처리하는 절차.

## 전제 조건

- PNG 파일이 Assets/_Project/Arts/Sprites/ 하위에 이미 있어야 함
- 에디트 모드여야 함 (플레이 모드 금지)

## Step 1 — 임포트 설정 확인/수정

script-execute로 TextureImporter 세팅:

```csharp
var path = "Assets/_Project/Arts/Sprites/Characters/MySheet.png";
var importer = AssetImporter.GetAtPath(path) as TextureImporter;

// 필수 설정
importer.textureType = TextureImporterType.Sprite;
importer.spriteImportMode = SpriteImportMode.Multiple; // 시트니까
importer.spritePixelsPerUnit = 16; // 기존 에셋과 맞출 것
importer.filterMode = FilterMode.Point; // 픽셀아트
importer.textureCompression = TextureImporterCompression.Uncompressed;

importer.SaveAndReimport();
```

PPU(Pixels Per Unit)는 기존 프로젝트 스프라이트와 반드시 맞춰야 한다. 현재 프로젝트 기준: 16.

## Step 2 — 슬라이싱 확인

이미 슬라이스 되어있는지 확인:
```csharp
var sprites = AssetDatabase.LoadAllAssetsAtPath(path);
int count = 0;
foreach (var s in sprites)
    if (s is Sprite) count++;
// count가 0이면 슬라이싱 안 된 것 → 자동 슬라이싱 필요
```

슬라이스 안 되어있으면 SpriteEditorWindow로 자동 슬라이싱:
```csharp
// spritesheet에 grid 기반 슬라이싱 적용
var factory = new SpriteDataProviderFactories();
factory.Init();
var dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
dataProvider.InitSpriteEditorDataProvider();

// 또는 더 간단하게: importer의 spritesheet 메타데이터 직접 세팅
```

슬라이스가 이미 되어있으면 (에셋팩 등) 이 단계 스킵.

## Step 3 — 스프라이트 이름 패턴 파악

슬라이스된 스프라이트 이름을 확인해서 애니메이션 그룹 분류:
```
예: "Idle 0", "Idle 1", "Run 0", "Run 1", "Run 2", "Jump 0" ...
→ Idle 그룹 (2프레임), Run 그룹 (3프레임), Jump 그룹 (1프레임)
```

이름에서 공백 또는 _ 앞부분이 상태명, 뒷부분이 프레임 번호.

## Step 4 — 애니메이션 클립 생성

각 그룹별로 AnimationClip 생성:

```csharp
var clip = new AnimationClip();
clip.frameRate = 8f; // Run은 빠르게, Idle은 느리게 조절

var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
var keyframes = new ObjectReferenceKeyframe[frames.Count];
for (int i = 0; i < frames.Count; i++)
{
    keyframes[i] = new ObjectReferenceKeyframe
    {
        time = i / clip.frameRate,
        value = frames[i]
    };
}
AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

// 루프 설정 (Idle, Run은 루프 / Jump, Death는 루프 X)
var settings = AnimationUtility.GetAnimationClipSettings(clip);
settings.loopTime = true; // 또는 false
AnimationUtility.SetAnimationClipSettings(clip, settings);

AssetDatabase.CreateAsset(clip, "Assets/_Project/Arts/Animations/Player/Player_Run.anim");
```

권장 FPS:
- Idle: 4~6
- Run: 8~10
- Jump: 6~8
- Attack: 10~12

## Step 5 — Animator Controller 생성/업데이트

기존 컨트롤러가 있으면 상태 교체, 없으면 새로 생성:

```csharp
var controller = AnimatorController.CreateAnimatorControllerAtPath(
    "Assets/_Project/Arts/Animations/Player/PlayerAnimator.controller");

// 파라미터
controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
controller.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);

// 상태 + 전환 추가 (Idle ↔ Run, Any → Jump)
```

파라미터 이름은 PlayerController.cs의 SetFloat/SetBool과 일치해야 한다:
- "Speed" — 이동 속도 (0이면 Idle, 0 초과면 Run)
- "IsGrounded" — 지면 접촉 (false면 Jump)

## Step 6 — 씬에 적용

```csharp
var player = GameObject.Find("Player");
var sr = player.GetComponent<SpriteRenderer>();

// 스프라이트 할당
sr.sprite = idleSprites[0];
sr.color = Color.white; // 더미 색상 리셋

// 머티리얼 확인 (URP 보라색 방지)
if (sr.sharedMaterial.shader.name != "Sprites/Default")
    sr.sharedMaterial = new Material(Shader.Find("Sprites/Default"));

// Animator 할당
var anim = player.GetComponent<Animator>();
if (anim == null) anim = player.AddComponent<Animator>();
anim.runtimeAnimatorController = controller;
```

## Step 7 — 검증

```bash
unity-mcp-cli run-tool assets-refresh --input '{}'
grep -n "error CS" ~/Library/Logs/Unity/Editor.log | tail -30
```

- [ ] 컴파일 에러 0
- [ ] 씬에서 스프라이트가 보라색 아닌 정상 색상
- [ ] 플레이 모드에서 Idle 애니메이션 재생
- [ ] 이동 시 Run 애니메이션 전환
- [ ] 점프 시 Jump 애니메이션 전환
- [ ] 좌우 반전 정상 작동

## 주의사항

- PPU가 기존과 다르면 캐릭터 크기가 달라진다. 반드시 맞출 것 (현재 16)
- 시트 구조(프레임 수, 이름)가 바뀌면 애니메이션 클립을 새로 만들어야 한다
- 기존 시트를 같은 파일명으로 덮어쓰면 참조가 유지된다 (Unity GUID 기반)

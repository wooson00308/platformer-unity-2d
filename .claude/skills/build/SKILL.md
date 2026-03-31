---
name: build
description: 플랫폼별 빌드를 실행한다. 컴파일 체크부터 빌드 실행, 결과 검증까지 AI가 처리한다.
---

# 빌드

빌드 요청을 받으면 이 절차를 순서대로 실행한다.

## 전제 조건

- Unity 프로젝트가 열려있고 에디트 모드여야 함
- 빌드 대상 플랫폼: WebGL (기본) 또는 Standalone (사용자 지정 시)

## Step 0 — MCP 가용 여부 확인

```bash
unity-mcp-cli run-tool assets-refresh --input '{}'
```

SUCCESS → MCP 모드, 실패 → 수동 모드.

---

## Step 1 — 컴파일 확인

빌드 전 에러가 없어야 한다. 에러가 있으면 빌드가 깨진다.

```bash
unity-mcp-cli run-tool assets-refresh --input '{}'
grep -n "error CS" ~/Library/Logs/Unity/Editor.log | tail -30
```

에러가 있으면 수정 후 반복. 에러 0이 될 때까지 다음으로 넘어가지 않는다.

수동 모드에서 CLI도 없으면:
> Unity 하단 Console 창(Ctrl+Shift+C)에서 빨간 에러가 0개인지 확인해줘.

---

## Step 2 — 버전 확인

현재 버전을 읽고 사용자에게 확인한다:

```bash
grep "bundleVersion" ProjectSettings/ProjectSettings.asset
```

버전 업데이트가 필요하면 YAML 직접 수정:

```bash
sed -i '' 's/bundleVersion: .*/bundleVersion: 0.2.0/' ProjectSettings/ProjectSettings.asset
```

수정 후 assets-refresh로 에디터에 반영.

---

## Step 3 — 빌드 실행

### MCP 모드

#### WebGL (기본)

```csharp
using UnityEngine;
using UnityEditor;

public class Script
{
    public static object Main()
    {
        var scenes = new[] { "Assets/_Project/Scenes/Main.unity" };
        var path = "../Builds/WebGL";

        var report = BuildPipeline.BuildPlayer(scenes, path, BuildTarget.WebGL, BuildOptions.None);

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            return $"빌드 성공: {path} ({report.summary.totalSize} bytes)";
        else
            return $"빌드 실패: {report.summary.result}\n{report.summary.totalErrors} errors";
    }
}
```

#### Standalone (Windows)

```csharp
using UnityEngine;
using UnityEditor;

public class Script
{
    public static object Main()
    {
        var scenes = new[] { "Assets/_Project/Scenes/Main.unity" };
        var path = "../Builds/Windows/Game.exe";

        var report = BuildPipeline.BuildPlayer(scenes, path, BuildTarget.StandaloneWindows64, BuildOptions.None);

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            return $"빌드 성공: {path}";
        else
            return $"빌드 실패: {report.summary.result}";
    }
}
```

#### Standalone (macOS)

```csharp
using UnityEngine;
using UnityEditor;

public class Script
{
    public static object Main()
    {
        var scenes = new[] { "Assets/_Project/Scenes/Main.unity" };
        var path = "../Builds/macOS/Game.app";

        var report = BuildPipeline.BuildPlayer(scenes, path, BuildTarget.StandaloneOSX, BuildOptions.None);

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            return $"빌드 성공: {path}";
        else
            return $"빌드 실패: {report.summary.result}";
    }
}
```

빌드는 수 분 걸릴 수 있다. 타임아웃에 주의.

### 수동 모드

사용자에게 안내:
> 1. File → Build Settings (Ctrl+Shift+B) 열기.
> 2. Platform 목록에서 WebGL (또는 원하는 플랫폼) 선택 → Switch Platform (처음이면).
> 3. Scenes In Build 목록에 Assets/_Project/Scenes/Main.unity가 있는지 확인. 없으면 Add Open Scenes.
> 4. Build 버튼 클릭 → 출력 폴더를 프로젝트 바깥 (예: ../Builds/WebGL) 으로 지정.
> 5. 빌드 완료까지 대기 (WebGL은 5-15분).

---

## Step 4 — 검증

### MCP 모드

빌드 결과 확인:
```bash
ls -la ../Builds/WebGL/ 2>/dev/null || ls -la ../Builds/Windows/ 2>/dev/null || ls -la ../Builds/macOS/ 2>/dev/null
```

빌드 로그에서 에러 확인:
```bash
grep -n "Error\|Exception" ~/Library/Logs/Unity/Editor.log | tail -20
```

### 수동 모드

#### WebGL 검증
> 터미널에서 빌드 폴더로 이동 후 로컬 서버 실행:
> ```
> cd ../Builds/WebGL && python3 -m http.server 8080
> ```
> 브라우저에서 http://localhost:8080 접속해서 게임 시작되는지 확인.

#### Standalone 검증
> 생성된 .exe 또는 .app 파일을 직접 실행해서 확인.

체크리스트:
- [ ] 빌드 결과 파일이 존재함
- [ ] 시작 씬 로드 정상
- [ ] 입력 동작 확인
- [ ] 콘솔에 런타임 에러 없음

## 주의사항

- Builds/ 폴더는 프로젝트 밖에 생성한다. Assets/ 안에 넣으면 Unity가 임포트 시도함.
- Library/ 폴더와 빌드 아티팩트는 커밋하지 않는다.
- WebGL 빌드는 파일을 직접 열 수 없다. 반드시 로컬 서버를 거쳐야 한다.

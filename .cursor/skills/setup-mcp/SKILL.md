---
name: setup-mcp
description: Unity MCP 연결이 안 될 때 자동 세팅. "유니티 연결 안 돼", "MCP 안 됨", "에디터 제어 안 됨" 요청 시 사용.
---

# Unity MCP 연결 세팅

Unity 에디터를 AI가 제어하려면 MCP(Model Context Protocol) 연결이 필요하다.
연결이 안 되면 assets-refresh, script-execute 등이 동작하지 않는다.

## 진단

### 1. CLI 설치 확인

```bash
which unity-mcp-cli
```

없으면 설치:
```bash
npm install -g unity-mcp-cli
```

### 2. Unity 플러그인 설치 확인

```bash
grep "com.ivanmurzak.unity.mcp" Packages/manifest.json
```

없으면 설치:
```bash
unity-mcp-cli install-plugin .
```

설치 후 Unity 에디터가 자동으로 패키지를 임포트한다.

### 3. MCP 서버 상태 확인

```bash
unity-mcp-cli run-tool assets-refresh --input '{}'
```

"Connection refused" 에러가 나면 → Unity 에디터에서 MCP 서버가 안 켜진 것.

사용자에게 안내:
> Unity 에디터에서 Window → AI Game Developer 메뉴를 열어줘.
> 거기서 서버가 켜져있는지 확인하고, 꺼져있으면 Start 눌러줘.

### 4. 포트 확인

```bash
cat UserSettings/AI-Game-Developer-Config.json | grep host
```

기본 포트: 29127. 다른 포트면 CLI 호출 시 URL이 안 맞을 수 있다.

### 5. 인증 에러

"Authorization failed" 에러가 나면:
- Unity 에디터의 AI Game Developer 창에서 Connection Mode 확인
- Custom 모드면 토큰이 맞는지 확인
- 안 되면 Local 모드로 변경 후 재시도

---

## Cursor 사용 시

Cursor는 MCP를 `.cursor/mcp.json`으로 설정한다.
Unity MCP 플러그인은 에디터 내부에서 서버를 띄우는 방식이다.

### MCP를 지원하는 Cursor 모델 (유료)

Cursor Pro 이상에서 MCP 서버 연결이 가능하다.
`.cursor/mcp.json` 설정:

```json
{
  "mcpServers": {
    "unity": {
      "url": "http://localhost:29127/api"
    }
  }
}
```

이 설정이 되면 Cursor AI가 MCP 도구를 직접 호출할 수 있다.

### MCP 없는 Cursor 환경 (무료 모델)

MCP를 쓸 수 없는 환경에서는 "수동 모드"로 작업한다.
이 프로젝트의 모든 스킬은 수동 모드를 지원한다:

- AI가 코드를 작성한다 (스크립트, SO 클래스, 테스트)
- 에디터 조작이 필요한 부분은 AI가 구체적인 단계별 안내를 제공한다
- 사용자가 안내에 따라 에디터에서 직접 조작한다

수동 모드에서도 CLI가 설치되어 있으면 터미널에서 직접 실행 가능:
```bash
# 컴파일 확인
unity-mcp-cli run-tool assets-refresh --input '{}'
grep -n "error CS" ~/Library/Logs/Unity/Editor.log | tail -30

# 테스트 실행
unity-mcp-cli run-tool tests-run --input '{}'
```

CLI도 없으면 Unity 에디터에서 직접 확인:
> Console 창(Ctrl+Shift+C)에서 에러 확인.
> Window → General → Test Runner에서 테스트 실행.

---

## Claude Code 사용 시

Claude Code는 settings.json 또는 .claude.json에서 MCP 서버를 설정한다.
Unity MCP 플러그인이 AI Game Developer 창에서 Claude Code용 설정을 자동 생성해준다.

1. Unity 에디터에서 Window → AI Game Developer 열기
2. Auto-generate Skills 또는 Configure MCP 클릭
3. 표시되는 설정을 Claude Code에 적용

또는 CLI로:
```bash
unity-mcp-cli setup-skills claude-code .
```

---

## 연결 확인 테스트

설정 완료 후 아래 명령어로 확인:

```bash
# 에셋 리프레시 (가장 기본)
unity-mcp-cli run-tool assets-refresh --input '{}'

# 씬 정보 조회
unity-mcp-cli run-tool scene-list-opened --input '{}'

# 스크립트 실행 테스트
unity-mcp-cli run-tool script-execute --input-file - <<'SCRIPT'
{"csharpCode": "public class Script { public static object Main() { return \"MCP 연결 성공\"; } }"}
SCRIPT
```

세 개 다 SUCCESS면 정상. 이후 스킬에서 MCP 모드로 작업 가능.

---

## 자주 생기는 문제

| 증상 | 원인 | 해결 |
|---|---|---|
| Connection refused | MCP 서버 안 켜짐 | 에디터에서 Window → AI Game Developer → Start |
| Authorization failed | 토큰 불일치 | Connection Mode를 Local로 변경 |
| unity-mcp-cli not found | CLI 미설치 | npm install -g unity-mcp-cli |
| 플러그인 없음 | Unity 패키지 미설치 | unity-mcp-cli install-plugin . |
| 다른 프로젝트 포트 충돌 | 여러 에디터 동시 실행 | 하나만 열거나 포트 변경 |
| Cursor 무료 모델에서 MCP 안 됨 | MCP 미지원 모델 | 수동 모드로 작업 (스킬이 안내 제공) |

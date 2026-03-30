# platformer-unity-2d

유니티를 모르는 아트 디자이너가 AI(Cursor / Claude Code)와 함께 2D 플랫포머 게임을 만들 수 있도록 설계된 가드레일 환경.

## 배경

비개발자가 AI 도구를 활용해 에셋스토어 데모 수준의 게임을 직접 만들어보는 것이 목표.

## 프로젝트 스택

- Unity 6.0.3 (URP 2D)
- Input System
- 2D Animation / Tilemap
- [Unity MCP](https://github.com/IvanMurzak/Unity-MCP) (에디터 원격 제어)

## 가드레일 구조

BPE에서 검증된 3단 방어 패턴을 Unity에 적용.

### 1단계 — AI 규칙 (.cursor/rules/, CLAUDE.md)

| 규칙 | 적용 | 역할 |
|---|---|---|
| architecture.mdc | 항상 | 어셈블리 경계, import 규칙, 플레이 모드 규칙 |
| coding-style.mdc | 항상 | 네이밍, 라이프사이클, Input System, 컴파일 확인 |
| game-logic.mdc | Game/, Core/ | 물리 패턴, 콜라이더, 타일맵, 입력 처리 |
| ui.mdc | UI/ | Canvas 규칙, 이벤트 구독, Game 참조 금지 |
| unity-nondev-basics.mdc | 요청 시 | Unity 용어 쉬운 설명, 자주 생기는 문제/해결 |

### 2단계 — 작업 스킬 (.cursor/skills/, .claude/skills/)

| 스킬 | 용도 |
|---|---|
| apply-spritesheet | 캐릭터 시트 PNG → 슬라이싱 → 애니메이션 → 씬 적용 |
| apply-tileset | 타일셋 시트 PNG → Tile 에셋 → 타일맵 배치 |
| replace-sprite | 기존 스프라이트 단순 교체 |
| add-feature | 새 게임 기능 추가 절차 |
| build | 프로젝트 빌드 절차 |

### 3단계 — 검증

```bash
# 컴파일 체크
unity-mcp-cli run-tool assets-refresh --input '{}'
grep -n "error CS" ~/Library/Logs/Unity/Editor.log | tail -30

# 테스트 실행
unity-mcp-cli run-tool tests-run --input '{}'
```

## 어셈블리 구조

```
Platformer.Data  ←  Platformer.Core  ←  Platformer.Game
                 ←  Platformer.UI
```

- Data: 순수 데이터 (enum, SO 이벤트, 설정)
- Core: 캐릭터, 물리, 인터페이스
- Game: 게임 매니저, 픽업, 트랩
- UI: HUD, 메뉴 (Game 참조 금지, SO 이벤트로 소통)

단방향 참조. Game ↔ UI 직접 참조 불가.

## 폴더 구조

```
Assets/_Project/
├── Scripts/
│   ├── Data/        (Platformer.Data.asmdef)
│   ├── Core/        (Platformer.Core.asmdef)
│   ├── Game/        (Platformer.Game.asmdef)
│   └── UI/          (Platformer.UI.asmdef)
├── Arts/
│   ├── Sprites/
│   ├── Animations/
│   ├── Tilesets/
│   ├── VFX/
│   └── Audio/
├── Prefabs/
├── Resources/
├── Datas/
└── Scenes/
```

## 사용 흐름 (디자이너)

1. 스프라이트 시트(PNG) 만들어서 Arts/ 폴더에 넣기
2. AI에게 "시트 올렸어, 적용해줘" 요청
3. AI가 슬라이싱 → 애니메이션 → 씬 적용까지 처리
4. 플레이 버튼 눌러서 확인
5. "점프 높이 올려줘", "적 추가해줘" 등 자연어로 수정 요청

## 세팅 필요 사항

### Unity MCP

```bash
npm install -g unity-mcp-cli
unity-mcp-cli install-plugin .
```

에디터에서 Window → AI Game Developer 열어서 MCP 서버 활성화.

### Cursor

`.cursor/rules/`와 `.cursor/skills/`가 레포에 포함되어 있어 클론 즉시 가드레일 적용.

### Claude Code

`.claude/skills/`가 레포에 포함. CLAUDE.md가 프로젝트 루트에 있어 자동 로드.

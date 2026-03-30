# Skill: 빌드

플랫폼별 빌드 절차.

## 단계

### 1. 컴파일 체크

빌드 전 에러가 없는지 먼저 확인:
- Unity 하단 Console 창 열기 (Ctrl+Shift+C)
- 빨간 에러 0개 확인
- 경고(노란색)는 빌드에 영향 없지만 확인 권장

에러 있으면 빌드 진행하지 말 것. 빌드 중 에러가 나면 결과물이 깨짐.

### 2. 버전 확인

`ProjectSettings/ProjectVersion.txt` 또는 Player Settings에서:
- Product Name 확인
- Version 번호 확인 및 필요시 업데이트
  - `ProjectSettings > Player > Version`
  - 형식: `major.minor.patch` (예: `0.1.0`)

### 3. 플랫폼 설정

File > Build Settings (Ctrl+Shift+B):

**WebGL (데모 배포용):**
- Platform: WebGL 선택 → Switch Platform
- Compression Format: Disabled (빠른 테스트) 또는 Gzip
- Scenes In Build: 빌드할 씬이 목록에 있는지 확인

**Standalone (Windows/Mac):**
- Platform: PC, Mac & Linux Standalone
- Target Platform: 대상 OS 선택
- Architecture: x86_64

### 4. 빌드 실행

Build Settings 창에서:
1. 빌드 출력 폴더 지정 (프로젝트 루트 바깥 권장, 예: `../Builds/`)
2. `Build` 버튼 클릭
3. 빌드 완료까지 대기 (WebGL은 5-15분 소요)

### 5. 검증

**WebGL:**
- 빌드 폴더에서 로컬 서버 실행 필요 (파일 직접 열기 불가)
- 터미널: `cd 빌드폴더 && python3 -m http.server 8080`
- 브라우저에서 `http://localhost:8080` 접속

**Standalone:**
- 생성된 .exe / .app 직접 실행
- [ ] 시작 씬 로드 정상
- [ ] 입력 동작 확인
- [ ] 오디오 재생 확인

## 주의사항

- Library/ 폴더는 .gitignore에 포함 — 빌드 아티팩트도 커밋하지 말 것
- WebGL 빌드는 `Builds/` 폴더에만 저장, 프로젝트 Assets/ 안에 넣지 말 것
- 빌드 후 씬 변경사항 저장 여부 팝업 뜨면 반드시 Save

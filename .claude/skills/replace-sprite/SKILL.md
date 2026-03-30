# Skill: 스프라이트 교체

기존 스프라이트를 새 이미지로 교체하는 절차.

## 단계

### 1. 파일 배치

새 이미지 파일을 올바른 폴더에 복사:
- 캐릭터: `Assets/_Project/Arts/Sprites/Characters/`
- 배경/타일: `Assets/_Project/Arts/Tilesets/`
- VFX: `Assets/_Project/Arts/VFX/`
- UI 이미지: `Assets/_Project/Arts/Sprites/UI/`

파일 형식: PNG 권장 (투명도 필요 시 필수)

### 2. 임포트 설정 확인

Unity가 자동 임포트 후 Project 패널에서 해당 파일 선택 → Inspector 확인:

- Texture Type: `Sprite (2D and UI)`
- Sprite Mode: 단일 이미지면 `Single`, 스프라이트 시트면 `Multiple`
- Pixels Per Unit: 기존 스프라이트와 동일하게 (보통 32 또는 16)
- Filter Mode: `Point (no filter)` — 픽셀아트 계열은 필수
- Compression: `None` 또는 `Normal Quality`

변경 후 하단 `Apply` 버튼 클릭.

### 3-A. Sprite Renderer 교체

씬에서 해당 GameObject 선택 → Inspector의 Sprite Renderer 컴포넌트 확인:
- Sprite 슬롯에 새 스프라이트를 드래그 앤 드롭

### 3-B. Animator 교체

Animation 클립을 수정해야 하는 경우:
1. Project 패널에서 해당 .anim 파일 더블클릭 → Animation 창 열기
2. 프레임별 스프라이트 키프레임 선택
3. 새 스프라이트로 교체

### 3-C. Tilemap 교체

1. Window > 2D > Tile Palette 열기
2. 해당 타일 선택 → Inspector에서 Sprite 교체
3. 이미 깔린 타일은 자동으로 반영됨

### 4. 검증

- [ ] 씬에서 스프라이트가 올바르게 표시되는가
- [ ] 크기/비율이 기존과 동일한가 (Pixels Per Unit 확인)
- [ ] 애니메이션이 정상 재생되는가
- [ ] 콜라이더(있다면)가 새 스프라이트 외형과 맞는가

## 주의사항

- 파일 이름 변경 시 기존 참조가 끊길 수 있음 — 같은 이름 유지 권장
- 파일을 직접 덮어쓰는 게 가장 안전 (Unity가 GUID로 참조 관리)
- 스프라이트 시트 구조 변경 시 애니메이션 클립 재세팅 필요

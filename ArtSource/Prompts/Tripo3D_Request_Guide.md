# Tripo3D 요청 정리

작성일: 2026-11-16
입력 이미지 폴더: `ArtSource/Concepts/TripoInput/`
내려받은 모델을 둘 곳: `ArtSource/Models/` (정리 전 원본), 정리 뒤에는 `Assets/_Project/Art/` 아래(8절)

결정(2026-11-16): 보스는 v3 디자인으로 확정. 부술 수 있는 부위는 양쪽 팔 2개뿐이다.

---

## 0. 시작 전에 확인할 것

아래는 2026-11-16에 검색으로 확인한 내용이고 공식 요금 페이지를 직접 본 것은 아니다. 결제나 공개 전에 Tripo 사이트의 요금과 약관 페이지에서 다시 확인한다.

| 항목 | 확인된 내용 | 이 프로젝트에 미치는 영향 |
|---|---|---|
| 무료 요금제 | 월 300 크레딧, 동시 작업 1개, 기록 1일 보관으로 소개된다 | 모델 9개에 재시도까지 하면 모자랄 수 있다. 돌격형 1개로 먼저 소모량을 본다 |
| 무료 산출물의 권리 | 공개 처리되고 CC BY 4.0, 상업적 이용 불가로 소개된다 | 포트폴리오 저장소에 올리면 출처 표기가 필요하다. `RESOURCES.md` 9절의 미정 항목이다 |
| 내보내기 형식 | 무료는 형식이 제한된다고만 나온다. 정확한 목록은 확인하지 못했다 | FBX가 안 되면 GLB로 받고 Blender에서 FBX로 바꾼다 |
| 부위 분리, 쿼드 메시 | 유료 추가 기능으로 나온다 | 쓰지 않는다. 부위를 이미지 단계에서 나눠 따로 생성한다(2절) |
| 로우 폴리 옵션 | Smart Low Poly를 켜면 면 수 상한이 2만, 기본값 1만으로 나온다 | 모든 모델에 켠다 |

기록 보관이 1일이면 생성한 날 바로 내려받아야 한다.

---

## 1. 무엇을 Tripo에 맡기고 무엇을 맡기지 않는가

| 대상 | 방법 | 이유 |
|---|---|---|
| 돌격형, 원거리형 | Tripo, 이미지 1장에 모델 1개 | 단일 물체 |
| 보스 | Tripo, 본체 1 + 팔(온전) 1 + 팔(손상) 1 + 그루터기 1로 나눠 4번 생성 | 부위 파괴는 메시 교체로 구현한다. 한 덩어리로 생성한 뒤 자르는 것보다 처음부터 따로 만드는 편이 단면이 깨끗하다 |
| 엄폐물 | Tripo, 온전 1 + 파손 1 | 내구도가 0이 되면 메시 교체 |
| 도로 방벽 | Tripo 1개, 복제 배치 | 단일 물체 |
| 역 입구, 고가 구조물, 셔터 건물 | Unity 기본 도형(상자) 조립 + 툰 셰이더 | 고정 카메라에서 한 면만 보인다. 그림 한 장으로 건물 전체를 변환하면 뒷면과 내부가 엉망이 된다 |
| 도로 바닥 | Unity 평면 + 바닥 그림 | 평면이다 |
| 하늘, 원경 도시, 균열 | 2D 그림을 판에 붙인다 | 3D일 필요가 없다 |
| 깃발, 부적, 안내판 그림 | 2D 그림을 사각 판에 붙인다 | 얇은 천은 변환에서 깨진다 |
| 볼라드, 형광등 | Unity 원통과 상자 | 기본 도형으로 충분하다 |
| 파괴 파편 | Unity 상자를 찌그러뜨린 조각이나 손상 팔 메시에서 잘라낸 조각 | 작고 잠깐만 보인다 |

보스의 오른쪽 팔은 따로 만들지 않는다. 왼쪽 팔 모델을 좌우 반전해 쓴다.

---

## 2. 요청 목록

삼각형 목표는 `RESOURCES.md` 5절의 예산을 부위별로 나눈 값이다. 도구가 더 낮은 값을 받지 않으면 받을 수 있는 가장 낮은 값으로 생성하고 Blender에서 줄인다.

| 순서 | 모델 | 입력 이미지 | 삼각형 목표 | 텍스처 | 내려받을 이름 |
|---|---|---|---|---|---|
| 1 | 돌격형 | `Rusher.png` | 3,000 | 1024 | `Rusher_raw` |
| 2 | 원거리형 | `Ranged.png` | 5,000 | 1024 | `Ranged_raw` |
| 3 | 보스 본체 | `Boss_Body.png` | 10,000 | 2048 | `Boss_Body_raw` |
| 4 | 보스 팔 온전 | `Boss_Arm_Intact.png` | 4,000 | 1024 | `Boss_Arm_Intact_raw` |
| 5 | 보스 팔 손상 | `Boss_Arm_Damaged.png` | 4,000 | 1024 | `Boss_Arm_Damaged_raw` |
| 6 | 보스 팔 그루터기 | `Boss_Arm_Stump.png` | 500 | 512 | `Boss_Arm_Stump_raw` |
| 7 | 엄폐물 온전 | `Cover_Intact.png` | 1,500 | 1024 | `Cover_Intact_raw` |
| 8 | 엄폐물 파손 | `Cover_Broken.png` | 2,000 | 1024 | `Cover_Broken_raw` |
| 9 | 도로 방벽 | `Env_RoadBarrier.png` | 800 | 512 | `Env_RoadBarrier_raw` |

보스는 화면에 본체 1개, 팔 2개가 나오므로 합계가 약 18,000으로 예산 2만 안이다.

1번을 Unity 반입까지 끝내 본 뒤에 2-9번을 진행한다(`RESOURCES.md` 10절의 제작 순서 3번). 1번에서 막히는 지점이 있으면 나머지 입력 이미지나 설정을 그에 맞춰 고친다.

---

## 3. 요청 방법

### 3.1 공통 설정

Tripo 웹앱 기준이다. 메뉴 이름은 버전에 따라 조금 다를 수 있다.

1. 생성 방식은 이미지 입력(Image to 3D)을 고른다. 글 입력(Text to 3D)은 쓰지 않는다.
2. 입력 이미지 1장을 올린다. 여러 장을 한꺼번에 올리는 다중 시점 입력은 처음에는 쓰지 않는다(3.3).
3. 모델 버전은 목록의 최신 일반 모델을 고른다. 로우 폴리 옵션을 지원하지 않는 모델(P1 계열로 소개된다)은 피한다.
4. 옵션

| 옵션 | 값 | 이유 |
|---|---|---|
| Smart Low Poly | 켬 | 게임용 면 수로 받는다 |
| 면 수 상한(Face Limit) | 2절의 삼각형 목표 | |
| 쿼드(Quad) | 끔 | 유료이고 Unity에서는 삼각형으로 충분하다 |
| 텍스처 | 켬 | 색면 구분에 필요하다 |
| PBR | 끌 수 있으면 끔 | 툰 셰이더는 기본 색 그림만 쓴다 |
| 부위 생성(Generate Parts) | 끔 | 유료이고 이미 이미지 단계에서 나눴다 |
| 스타일 변환 | 없음 | 입력 그림의 화풍을 그대로 쓴다 |
| 자세(T 포즈 등), 리깅 | 끔 | 관절이 없는 기계다 |

실제 웹앱 화면(2026-11-16 사용자 화면 기준, 한국어 표시)에서는 위 옵션이 다음 이름으로 보인다. Smart Low Poly 토글은 따로 없고 폴리곤 수 입력으로 대신한다.

| 화면의 항목 | 값 |
|---|---|
| 탭 | HD 모델(먼저 시도). 면이 지저분하면 스마트 메시 탭과 비교 |
| 울트라 메시 품질 | 끔 |
| AI 완성 및 3D 강화 | 켬(결과가 입력 그림과 달라지면 끄고 다시) |
| 텍스처 | 켬, 품질 2K |
| 조명 제거 | 켬 |
| PBR | 끔 |
| 토폴로지 | 삼각형 |
| 폴리곤 수 | 2절의 삼각형 목표(최소 500) |
| 부분별 생성, 8K 텍스처 | 끔 |
| AI 모델 | H3.1 |

생성 1회에 30 크레딧으로 표시된다. 무료 요금제 화면에 "업그레이드하여 3D 모델 내보내기" 안내가 떠 있으므로, 크레딧을 쓰기 전에 기존 에셋 하나로 내보내기가 되는지와 형식을 먼저 확인한다.

5. 생성 결과를 돌려 보며 3.2의 항목을 확인한다. 통과하면 FBX로 내려받는다. FBX가 안 되면 GLB로 받는다.
6. 내려받은 파일을 2절의 이름으로 `ArtSource/Models/`에 둔다. 텍스처가 따로 나오면 같은 이름으로 함께 둔다.
7. `RESOURCES.md` 8절 표에 한 줄 적는다(도구와 모델 버전, 날짜, 프롬프트 위치, 손본 내용, 약관 확인).

### 3.2 생성 결과에서 볼 것

| 확인 | 통과 기준 | 실패하면 |
|---|---|---|
| 뒷면 | 앞면과 같은 재질과 형태로 이어진다. 납작하게 눌리거나 뚫려 있지 않다 | 다시 생성. 두 번 실패하면 3.3 |
| 좌우 대칭 | 본체, 돌격형, 원거리형은 좌우가 같다 | 다시 생성하거나 Blender에서 반쪽을 미러 |
| 덩어리 분리 | 움직일 파츠(4절) 사이에 눈에 보이는 단차나 틈이 있다 | 입력 이미지에서 틈을 더 벌려 다시 그린다 |
| 가는 부분 | 끊기거나 뭉친 곳이 없다 | 입력 이미지에서 그 부분을 굵게 고친다 |
| 코어 | 적색 면이 평평한 한 면으로 나온다 | 텍스처에서 손본다 |
| 구워진 음영 | 텍스처에 그림자와 하이라이트가 심하게 들어가 있지 않다 | 텍스처를 색면 단위로 다시 칠하거나 색 수를 줄인다 |
| 면 수 | 목표의 1.5배 이내 | Blender Decimate |

### 3.3 한 장으로 형태가 안 나올 때

- 같은 이미지로 한 번 더 생성한다(결과가 매번 다르다).
- 그래도 뒷면이 나쁘면 다중 시점 입력을 쓴다. 정면, 왼쪽, 뒤, 오른쪽 4장이 필요하므로 GPT에 같은 물체의 직교 시점 4장을 요청해 만든다. 네 장의 디자인이 서로 어긋나면 한 장 입력보다 결과가 나빠지므로 꼭 필요한 모델에만 쓴다.
- 글 프롬프트 칸이 함께 있으면 5절의 영문 설명을 붙인다. 이미지가 우선이고 글은 보조다.

---

## 4. 모델별 메모와 생성 뒤 작업

| 모델 | 분리할 파츠 | 피벗 | 그 밖에 |
|---|---|---|---|
| 돌격형 | 몸체, 칼날 판 2개, 부유 원반 | 몸체 중심. 칼날은 뒤쪽 축, 원반은 원 중심 | 화면에 수십 기가 나오므로 면 수를 가장 엄격하게 지킨다. 칼날이 몸체와 붙어 나오면 분리하지 않고 한 덩어리로 써도 된다(부유 원반 회전만으로 움직임 표현) |
| 원거리형 | 포탑과 포신(한 덩어리), 삼각대 | 포탑은 받침 회전축, 삼각대는 바닥 중심 | 포탑이 좌우로 도는 것만 구현한다 |
| 보스 본체 | 없음(한 덩어리) | 코어 중심 | 어깨 소켓 2곳의 위치를 빈 오브젝트로 표시한다. 팔은 이 위치에 붙는다 |
| 보스 팔 온전, 손상, 그루터기 | 없음 | 셋 모두 관절 돌기의 중심(소켓에 꽂히는 점) | 세 모델의 크기와 피벗을 맞춰 같은 자리에서 교체되게 한다. 오른쪽 팔은 X축 반전 복제 |
| 엄폐물 온전, 파손 | 없음 | 바닥 중심 | 두 모델의 바닥 크기를 맞춘다. 부적은 사각 판에 그림으로 붙인다 |
| 도로 방벽 | 없음 | 바닥 중심 | 복제해 도로 양옆에 줄지어 놓는다 |

공통 작업 순서:

1. Blender에서 열어 크기를 맞춘다(사람 키 1.7m 기준으로 돌격형 높이 약 1m, 원거리형 약 1.7m, 보스 본체 약 6m, 엄폐물 높이 약 1m).
2. 면 수를 확인하고 필요하면 줄인다. 파츠를 나누고 피벗을 맞춘다.
3. 발광 마스크를 만든다. 텍스처에서 밝은 적색(코어)과 암적색(줄) 영역을 골라 흑백 그림으로 저장한다. 코어는 흰색, 줄은 회색으로 두면 줄이 코어보다 어둡게 빛난다. 텍스처의 적색 부분은 짙은 회색으로 덮어 발광 색이 겹치지 않게 한다.
4. FBX로 내보내 `Assets/_Project/Art/`의 해당 폴더에 넣는다.
5. `AimCombat/ToonLit` 머티리얼을 만들어 기본 색 그림과 발광 마스크를 지정한다.
6. 지금의 도형 적 프리팹에서 메시만 바꿔 실제 카메라에서 크기와 가독성을 본다.

보스 팔 파괴의 구현 메모(코드는 아직 없다. `SPEC.md`에 규칙을 먼저 적는다):

- 팔마다 체력을 따로 둔다. 50% 이하에서 온전 메시를 손상 메시로, 0에서 그루터기 메시로 바꾼다.
- 파괴 순간에 파편 조각을 흩뿌리고 히트스톱과 카메라 흔들림을 건다.
- 팔의 콜라이더는 메시와 함께 바꾸고 그루터기는 피격 판정을 끈다.

---

## 5. 글 프롬프트 (보조 입력 또는 이미지가 안 될 때의 대안)

게임 제목, 캐릭터 이름, 작가 이름을 넣지 않는다. 영문으로 쓴다.

공통으로 뒤에 붙이는 문장:

```
Stylized low-poly game asset, hard-surface, chunky beveled shapes, flat cel-shaded colors, dark gunmetal armor with mid-gray top faces, no thin wires, no antennas, no floating pieces, single solid object, clean silhouette.
```

| 모델 | 프롬프트 |
|---|---|
| 돌격형 | `A small hovering attack drone shaped like a forward-pointing arrowhead wedge, low at the front and taller at the back. A large flat hexagonal red core panel set in a dark frame on the sloped top front. Two thick blade plates folded along both sides pointing forward, each attached by one thick cylindrical hinge at the rear. A flat round hover disc under the body. Dark red accent stripes on the armor plates.` |
| 원거리형 | `A stationary artillery turret robot. A boxy armored turret head with a wide horizontal red core panel on its front face, and one thick long octagonal cannon barrel coming out of the front below the core. A small horn-like sensor on top. The turret sits on a short round swivel base on three thick short splayed legs with no joints. Dark red accent stripes.` |
| 보스 본체 | `The armless torso of a large floating war machine, symmetrical. A faceted helmet-like head with a vertical dark red visor slot on a thick neck. A blocky chest with a large flat hexagonal red core in the center, framed by a V-shaped armor plate above and a V-shaped plate below. A long downward-pointing spear-like ram spike under the torso with a dark red stripe. On each shoulder a thick octagonal socket ring with a dark red recessed interior, no arms attached.` |
| 보스 팔 온전 | `A detached mechanical wing arm of a large war machine. A thick octagonal joint block with an octagonal plug on one end, and three long thick wedge-shaped armor plates fanning out from it like feathers. Each plate ends in a square dark red gun port. A dark red stripe on the top plate.` |
| 보스 팔 손상 | `The same detached mechanical wing arm, heavily damaged. Same thick octagonal joint block and plug. Of the three long wedge plates, the middle one is snapped off leaving a short broken stub. The other two plates have deep wide wedge-shaped gashes with bright red glowing interior, and large chipped corners. No thin exposed frame.` |
| 보스 팔 그루터기 | `A small broken mechanical joint stump. A thick octagonal plug with a rough faceted broken end made of dark red crystalline chunks.` |
| 엄폐물 온전 | `A waist-high wide thick concrete barricade block with chamfered corners, flat top, two thick metal handle loops on the top edge, and a worn yellow diagonal hazard stripe band near the top of the front face. Plain gray concrete, flat colors.` |
| 엄폐물 파손 | `The same waist-high concrete barricade block, broken. The upper third of one side is smashed away with a rough faceted break surface and three short thick rebar stubs sticking up. Only one metal handle loop remains. The base footprint is unchanged.` |
| 도로 방벽 | `A concrete road barrier with a trapezoid cross-section, wider at the base, with a yellow and black diagonal hazard stripe band along the upper part. Plain gray concrete, flat colors.` |

---

## 6. Tripo로 만들지 않는 배경 요소의 준비물

스테이지는 `Stage_Backdrop_v3.png`를 설계도로 삼아 Unity에서 조립한다.

| 요소 | 만드는 방법 | 필요한 그림(GPT 이미지) |
|---|---|---|
| 하늘과 원경 | 카메라 뒤쪽 끝에 세운 큰 판 | 검보라 하늘, 도시 실루엣, 균열 선이 들어간 가로 그림 1장(앞쪽 건물과 도로 없음) |
| 도로 | 평면 | 차선과 횡단보도가 있는 바닥 그림 1장, 또는 아스팔트 반복 그림 + 차선은 별도 판 |
| 역 입구와 고가 구조물 | 상자 조립 | 콘크리트 벽 반복 그림 1장, 안내판 그림 1장 |
| 셔터 건물 | 상자 조립 | 셔터 반복 그림 1장 |
| 깃발 | 사각 판 | 붉은 깃발 그림 1장(투명 배경) |
| 부적 | 사각 판 | 부적 그림 3종(투명 배경) |

이 그림들은 아직 만들지 않았다.

---

## 7. 기록

모델 하나를 받을 때마다 `RESOURCES.md` 8절 표에 적는다. 입력 이미지의 프롬프트는 `ArtSource/Prompts/Tripo_Input_Prompts_v1.md`에 있다.

## 8. 넣을 폴더

| 모델 | 폴더 |
|---|---|
| 돌격형 | `Assets/_Project/Art/Enemies/Rusher` |
| 원거리형 | `Assets/_Project/Art/Enemies/Ranged` |
| 보스 본체와 팔 | `Assets/_Project/Art/Enemies/Boss` |
| 엄폐물 | `Assets/_Project/Art/Environment/Cover` |
| 도로 방벽 | `Assets/_Project/Art/Environment/Structures` |

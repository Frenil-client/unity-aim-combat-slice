# 나머지 몬스터와 구조물 4면도 제작 기록

작성일: 2026-11-16
도구: 내장 image_gen, transparent_background=false
출력 폴더: ArtSource/Concepts/TripoMultiView/

## 최종 선택

- Ranged_4View_v1.png
- Boss_Body_4View_v1.png
- Boss_Arm_Intact_4View_v2.png
- Boss_Arm_Damaged_4View_v2.png
- Boss_Arm_Stump_4View_v2.png
- Cover_Intact_4View_v1.png
- Cover_Broken_4View_v1.png
- Env_RoadBarrier_4View_v1.png

배치: 왼쪽 위 앞, 오른쪽 위 왼쪽, 왼쪽 아래 뒤, 오른쪽 아래 오른쪽.
보스 팔은 분리된 부품 기준으로 넓은 외측 면을 앞, 내측 면을 뒤로 정의했다.
기존 Rusher 파일은 수정하지 않았다. 합본 4면도와 개별 시점 파일을 함께 제공한다.

## 확인 사항

- 보스 팔 정상, 손상, 그루터기는 v2를 사용한다. v1은 보정 전 기록이다.
- 그루터기 앞면의 사선 시점과 보스 팔 반대쪽 측면의 중복 포구를 보정했다.
- 생성 이미지이므로 정밀 CAD 정투영도는 아니다. 일부 사선 면 표현, 시점별 세부 비율 차이와 미세한 배경 질감이 남는다.
- 구조물은 넓은 앞뒤 면 때문에 열 너비가 동일하지 않다. 중앙에서 기계적으로 4등분하면 물체가 잘릴 수 있으므로 개별 입력으로 분리할 때 각 물체의 실제 외곽을 기준으로 잘라야 한다.

## 1차 공통 프롬프트

```text
Use case: stylized-concept. Generate a square orthographic turnaround sheet for Tripo multi-view image-to-3D reconstruction. Image 1 is the exact subject design reference; image 2 is ONLY the Rusher sheet layout and art-style reference, do not include the Rusher. Exactly four views of ONE identical object in a 2x2 grid: TOP LEFT FRONT, TOP RIGHT object's LEFT side (front points left), BOTTOM LEFT BACK, BOTTOM RIGHT object's RIGHT side (front points right). True orthographic, camera at mid-height, no perspective or tilt. Same physical scale in all cells, same baseline relative to each cell, generous 12% empty margins; no clipping or crossing cell boundaries. Match design, proportions, connected parts and flat two-tone cel shading, bold black outlines. All views must describe the same coherent 3D geometry. Front-only surfaces must NOT appear face-on in side or back views. Uniform near-white RGB245 background, no shadows, texture, floor, gradients, labels, text, grid lines, arrows, watermark, bloom, floating parts. Do not redesign the subject. 
```

## Ranged

참조 1: ArtSource/Concepts/TripoInput/Ranged.png
참조 2: ArtSource/Concepts/TripoMultiView/Rusher_4View_v2.png

공통 프롬프트에 아래 문장을 이어서 전달했다.

```text
A compact turret with ONE long chunky forward cannon below ONE wide rectangular bright-red frontal visor, layered dark gunmetal casing, crimson stripes, a short thick top sensor, round central swivel base and exactly THREE thick tripod legs (two front/outboard, one rear centered). Front cannon is seen muzzle-on in FRONT and in full length in SIDE views. No red frontal visor on back or facing viewer in side. Rear has solid layered armor.
```

## Boss_Body

참조 1: ArtSource/Concepts/TripoInput/Boss_Body.png
참조 2: ArtSource/Concepts/TripoMultiView/Rusher_4View_v2.png

공통 프롬프트에 아래 문장을 이어서 전달했다.

```text
Only the armless boss central body. Tall faceted head, vertical crimson frontal visor, central torso with ONE bright red hexagonal front core between attached thick chevron armor plates, long downward pointed ram, one thick octagonal shoulder socket on each side. No arms or legs. Side views show socket opening face-on and core edge-on only. Back is armored without frontal visor or red core. All parts one connected object.
```

## Boss_Arm_Intact

참조 1: ArtSource/Concepts/TripoInput/Boss_Arm_Intact.png
참조 2: ArtSource/Concepts/TripoMultiView/Rusher_4View_v2.png

공통 프롬프트에 아래 문장을 이어서 전달했다.

```text
Only ONE detached boss wing-arm module, not a whole monster and not a pair. Preserve the exact three long thick wedge gun plates arranged in a fan connected to a stout octagonal hinge and short octagonal mounting peg. Define FRONT as the broad decorated outer face of the fan, back as opposite inner face; left/right views are the perpendicular thin profiles of that same module, not two more front views. Three rectangular muzzle recesses are at the plate tips, one crimson stripe on upper plate. All plates stay attached to hinge. Maintain consistent mounting peg and physical thickness across views.
```

### 2차 보정

참조: Boss_Arm_Intact_4View_v1.png

```text
Correct this turnaround sheet to FOUR actually orthogonal views of the same three-bladed mechanical wing-arm. Keep the design, palette, 2x2 layout and white-gray background. TOP LEFT = front broad outer face seen completely flat-on, no 3/4 camera. It is a fan of three thick gun blades attached to an octagonal hub. Hide the inner mounting peg behind the hub in this outer view. BOTTOM LEFT = opposite broad inner face, horizontally mirrored fan silhouette, mounting peg visible centered on hub pointing toward camera, not sideways. TOP RIGHT = left edge view, you may see the three red muzzle holes on the forward tips edge-on to the broad fan. BOTTOM RIGHT = OPPOSITE right edge view, show the rear solid armor edges, absolutely NO muzzle openings or red muzzle holes on this rear-facing edge; mounting peg extends the opposite direction. Side views have identical physical thickness, height and hub position. Do not put front-facing muzzles on both opposing sides. No labels, no grid lines, no loose parts, no shadows. Generous margins.
```

## Boss_Arm_Damaged

참조 1: ArtSource/Concepts/TripoInput/Boss_Arm_Damaged.png
참조 2: ArtSource/Concepts/TripoMultiView/Rusher_4View_v2.png

공통 프롬프트에 아래 문장을 이어서 전달했다.

```text
Only ONE damaged boss wing-arm module. Same hinge, mounting peg, plate arrangement and scale as intact reference. Middle of three wedge plates is broken short to a stump, upper/lower plates remain long with large angular notches exposing crisp red interior. No debris. FRONT is broad decorated outer face of fan, BACK opposite inner face, LEFT/RIGHT perpendicular thin profiles. Damage is fixed geometry consistent in every view, not new cuts per viewpoint. Keep exactly two surviving long plates and one short central stump.
```

### 2차 보정

참조: Boss_Arm_Damaged_4View_v1.png, Boss_Arm_Intact_4View_v2.png

```text
Image 1 is the damaged wing-arm turnaround to correct. Image 2 is the corrected INTACT turnaround and is the exact camera/geometry/layout reference. Recreate image 2 with ONLY the damage from image 1: middle of three blades broken short near the hub, upper and lower blades have chunky angular broken edge notches with crisp exposed red interior. Preserve image 2 cameras, physical scale, octagonal hinge position and mounting peg, 2x2 layout. Top left outer broad face, top right forward thin edge with only TWO surviving muzzle holes, bottom left inner broad face with mounting peg, bottom right opposite rear thin edge with NO muzzle holes. Front and back fan silhouettes correspond. Same damage geometry across all views. Flat light gray background, no text, no shadows, no detached rubble. Keep everything within its own quadrant.
```

## Boss_Arm_Stump

참조 1: ArtSource/Concepts/TripoInput/Boss_Arm_Stump.png
참조 2: ArtSource/Concepts/TripoMultiView/Rusher_4View_v2.png

공통 프롬프트에 아래 문장을 이어서 전달했다.

```text
Only the small broken octagonal joint stump, no wing plates. Thick octagonal joint body, short smaller octagonal mounting peg on its inner side and a jagged dark-crimson solid fractured outer face. FRONT means the fractured outer end seen straight on; BACK means smaller mounting peg end straight on; LEFT/RIGHT show the short thick stepped side profile. One connected solid, no floating fragments, same dimensions and fracture geometry in all views.
```

### 2차 보정

참조: Boss_Arm_Stump_4View_v1.png

```text
Edit this four-view sheet. Preserve style, object identity, colors and all four cell positions. Critical correction: TOP LEFT must be the direct axial FRONT view looking straight at the broken dark-red fracture surface. It must be a large roughly octagonal face filled with jagged dark-red crystalline fracture facets, with a thick dark gunmetal rim. The small mounting peg is on the OPPOSITE back side and is completely hidden in this front view. Do NOT show any side surface or a 3/4 view in top left. Bottom left is existing back view looking at small mounting peg, keep it. Right column are the existing two side views, keep their geometry. Make front and back outer height and width identical; same scale throughout. Flat light gray background, no text. All geometry must be physically consistent.
```

## Cover_Intact

참조 1: ArtSource/Concepts/TripoInput/Cover_Intact.png
참조 2: ArtSource/Concepts/TripoMultiView/Rusher_4View_v2.png

공통 프롬프트에 아래 문장을 이어서 전달했다.

```text
One intact wide thick concrete cover block, beveled corners, two stout lifting handles on top, recessed lower center with chunky feet at both ends, worn yellow diagonal painted stripes along upper FRONT only. Gray concrete, flat cel shading, sparse cracks. Back plain concrete, not a duplicate front with stripes. Side views narrow and deep, handles seen edge-on and overlap as appropriate. Exactly two handles, same handle heights and concrete thickness in every view. No red core, talismans or metal monster parts.
```

## Cover_Broken

참조 1: ArtSource/Concepts/TripoInput/Cover_Broken.png
참조 2: ArtSource/Concepts/TripoMultiView/Rusher_4View_v2.png

공통 프롬프트에 아래 문장을 이어서 전달했다.

```text
One broken version of the concrete cover block. Preserve intact block footprint, depth, lower two thirds, end feet and width. Upper left/center third broken away with a jagged fixed fracture, exactly THREE short thick embedded rebar stubs, only ONE surviving lifting handle on object's right shoulder (viewer left in front view if reference definition differs: preserve reference visual arrangement). Keep original remaining yellow paint, do not relocate paint to fracture edge. Same actual asymmetric damage seen from four directions; back mirrors front silhouette horizontally, not an independent fracture. No loose rubble or thin wires. Keep surviving handle on viewer RIGHT in FRONT view, viewer LEFT in BACK. Place front/back centers exactly at 25% canvas width, side-view centers at 75%. Every view fully contained in its own equal square quadrant.
```

## Env_RoadBarrier

참조 1: ArtSource/Concepts/TripoInput/Env_RoadBarrier.png
참조 2: ArtSource/Concepts/TripoMultiView/Rusher_4View_v2.png

공통 프롬프트에 아래 문장을 이어서 전달했다.

```text
One long trapezoidal solid concrete road barrier, broad base and narrow flat top, thick chamfered edges, yellow/black diagonal hazard band across upper long faces. No handles. FRONT and BACK are long wide faces, LEFT/RIGHT are narrow trapezoid end profiles revealing actual depth and slopes. Same height and length in front/back, same depth in side views. No monster parts or extra props. STRICT layout: front/back images occupy x=8% to 42% of entire canvas, never extend past x=45%. Side images center at x=75%. Reduce ALL views uniformly so longest view fits this box. All four heights identical.
```



## 개별 시점 분리

사용자 후속 요청으로 최종본 8종에서 앞, 왼쪽, 뒤, 오른쪽 PNG 32장을 분리했다. 파일명은 각 이름 뒤에 _Front, _Left, _Back, _Right를 붙였다. 각 파일은 1024x1024이며 한 모델의 네 시점은 동일 배율과 하단 기준선을 사용한다. 물체 사이의 빈 영역을 찾아 분리했으므로 구조물도 중앙 분할로 잘리지 않는다. 기존 Rusher 개별 이미지는 유지했다. 새 디자인 생성 없이 기존 최종본 픽셀을 잘라 크기와 여백만 조정했다.

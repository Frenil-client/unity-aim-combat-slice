# Tripo 다중 시점(4면도) 입력 프롬프트 v1

작성일: 2026-11-16
도구: Tripo 웹앱 이미지 생성(Generate Image), 생성 모델 GPT Image 2.5
목적: 한 장 입력으로 뒷면이 나쁘게 나올 때 쓰는 다중 시점 입력(`Tripo3D_Request_Guide.md` 3.3)
출력 폴더: `ArtSource/Concepts/TripoMultiView/`

## 방식

- 네 시점을 한 장에 2x2로 그리게 한다. 따로 네 번 생성하면 디자인이 서로 어긋나기 쉽다.
- 배치: 왼쪽 위 앞, 오른쪽 위 왼쪽 옆, 왼쪽 아래 뒤, 오른쪽 아래 오른쪽 옆. Tripo 다중 시점 칸(앞, 왼쪽, 오른쪽, 뒷면)에 맞춰 네 칸으로 잘라 넣는다.
- 비율 1:1, 4K, 1장. 참조 이미지는 `ArtSource/Concepts/TripoInput/` 의 같은 모델 그림.
- 왼쪽/오른쪽은 물체 기준이다. 물체가 바라보는 방향이 앞이고, 물체의 왼쪽 옆면이 보이는 시점이 왼쪽이다.

## 공통 문장

```text
Orthographic turnaround sheet of the exact same object shown in the reference image, made as input for multi-view image-to-3D reconstruction. One square image divided into a 2x2 grid of four equal cells with NO visible dividing lines. Top-left cell: FRONT view. Top-right cell: LEFT side view (the object's own left side). Bottom-left cell: BACK view. Bottom-right cell: RIGHT side view (the object's own right side). All four views use the same scale, the same vertical position and the same ground line, camera exactly at the object's mid-height, true orthographic projection with no perspective and no tilt. Each view is centered in its own cell, filling about 75 percent of the cell, with clear empty margins, nothing cropped, nothing overlapping into another cell. No text, labels, numbers, arrows, grid, frame or guide lines.
Style: match the reference exactly. Dark gunmetal armor, mid-gray lit top faces and bevels, bold black outlines, dark crimson stripes, bright red core, flat two-tone cel shading, crisp flat colors, no glow or bloom. Every part is thick and solid, one connected object, no floating pieces, no thin wires or antennas.
Background: one perfectly uniform flat near-white light gray (RGB 245,245,245) across the whole image, no gradient, no paper texture, no floor, no cast or contact shadow.
```

## Rusher (돌격형)

참조 이미지: `ArtSource/Concepts/TripoInput/Rusher.png`

```text
Object: the small hovering attack drone from the reference. An arrowhead wedge body pointing forward, low and sharply pointed at the front, taller armored block at the back. A large flat bright red hexagonal core in a thick dark frame on the sloping upper front surface. Two thick blade plates run along the left and right sides and point forward, each with a dark crimson stripe, each attached at the rear by one stout cylindrical hinge. A flat round hover disc with dark crimson panels is attached directly under the body, touching it. The drone is bilaterally symmetric.
FRONT = looking straight at the pointed nose, the red core visible above and behind the nose, both blades visible left and right, the disc below. BACK = the rear armor block and blade hinges, no red core visible. LEFT and RIGHT = long side profiles of the wedge, nose pointing to the image edge it faces, mirror images of each other.
```

### 결과 (2026-11-16)

실제 생성: Codex CLI(`codex exec`)의 내장 image_gen, 1차는 위 공통 문장 + Rusher 문장 그대로, 2차는 같은 세션을 이어 아래 피드백을 보냈다.

| 판 | 파일 | 판정 | 문제 |
|---|---|---|---|
| 1차 | `Rusher_4View_v1.png` | 탈락 | 측면도에 코어가 정면을 향한 육각형으로 그려져 측면에 코어가 하나 더 있는 셈. 원반 폭이 앞뒤와 옆에서 다름. 정면도 칼날이 위로 솟은 판처럼 그려져 측면과 안 맞음 |
| 2차 | `Rusher_4View_v2.png` | 채택 | 네 시점 높이 434-451px로 4% 이내. 앞(429px)과 뒤(448px) 폭이 약 4% 다름. 왼쪽 시점의 코 끝이 칸 경계에 거의 닿음(자를 때 여백을 다시 줘서 해결) |

2차 피드백 전문:

```text
Keep everything from v1 (same 2x2 layout, same cell order FRONT / LEFT / BACK / RIGHT, same style, same background, no text) and fix these 3D-consistency errors:
1. The red hexagonal core is ONE face on the sloping upper front of the body. In v1 the LEFT and RIGHT side views show the core as a full face-on hexagon, which means a second core on the side. In the side views the core must be seen edge-on: only a thin sloped red strip on the top-front slope of the profile, with its dark frame, NOT a hexagon facing the viewer. The side of the body behind it is plain dark armor.
2. The hover disc is round, so it must have exactly the same width in all four views and the same thickness and height position. In v1 the disc is wider in FRONT/BACK than in the side views.
3. FRONT view must match the side profiles: the blades point forward, so in FRONT they are seen almost tip-on as short foreshortened wedges on both sides of the body, not tall plates pointing upward. The nose is seen head-on as a small pointed tip at the lower center in front of the core, not a long downward spike.
4. Heights must match: the top of the rear armor block and the bottom of the disc are at the same y-position in all four views. FRONT and BACK widths must be identical (same silhouette outline, mirrored details).
5. Keep the side profiles exactly mirror images of each other.
```

Tripo 칸에 넣을 파일(2차를 잘라 만든 1024 정사각, 네 장 같은 배율, 가장 긴 시점이 80%): `Rusher_Front.png`, `Rusher_Left.png`, `Rusher_Back.png`, `Rusher_Right.png`

# v2 이미지 생성 프롬프트 전문

내장 image_gen 도구를 사용했다. 아래는 실제 전달한 프롬프트와 입력 순서이며, 거절된 요청과 재생성 요청도 포함한다.

목업 입력은 도구의 최대 5장 제한 때문에 아군 원본 4장을 2x2 참조 시트로 묶었다. 원본은 수정하지 않았으며 전체 이미지를 비율 유지 축소해 배치했다. 좌상단 Rapid_Poses.png, 우상단 Charge_Cover_Source.png, 좌하단 Shotgun_Cover_Source.png, 우하단 Rapid_Fire_Source.png이다. 참조 시트는 임시 경로에만 저장했다.

## 1. Rusher_Concept_v2.png

저장 경로: `ArtSource/Concepts/Enemies/Rusher_Concept_v2.png`

상태: 생성 후 직접 열어 검토, 최종 채택

입력 이미지 순서:

1. `ArtSource/Concepts/Enemies/Rusher_Concept_v1.png`

```text
Use case: stylized-concept. Create one square 1:1 full-body enemy concept for image-to-3D conversion. Input image 1 is STYLE REFERENCE ONLY: preserve bold black outlines, chamfered low-poly volumes and flat two-tone cel shading, but redesign the silhouette. Rusher: a clear forward-pointing ARROWHEAD WEDGE, isosceles triangle in top view, broad high rear and low pointed front pointing toward lower-left foreground in front three-quarter elevated view. Large solid RED HEXAGON core on the forward sloping upper surface, facing the elevated front camera, occupying roughly 40 percent of that sloping face, inside a thick near-black frame. Two thick forward-pointing blade plates tucked tightly alongside the wedge, attached at the rear by short chunky cylindrical axles as thick as the blades. No C claws, no dumbbell shape. Single flat hover disc under body. Three distinct material values: light gray outer armor, medium gray body, near-black internal frame, joints and gaps. Only the core is colored. 3-5 simple major masses, no wires, antennas, thin sheets, walking legs or humanoid features. Uniform light gray background, NO ground shadow, NO gradient, no text, dimensions or arrows. Entire object with generous margins, crisp red with no bloom, no metallic reflections, no weathering.
```

## 2. Ranged_Concept_v2.png

저장 경로: `ArtSource/Concepts/Enemies/Ranged_Concept_v2.png`

상태: 도구 안전 필터 거절, 이미지 미생성

입력 이미지 순서:

1. `ArtSource/Concepts/Enemies/Ranged_Concept_v1.png`

```text
Use case: stylized-concept. Produce a square 1:1 full-body concept for image-to-3D conversion. Image 1 is STYLE ONLY: chunky chamfered mechanical masses, heavy black outline, flat two-tone cel shading. Redesign ranged artillery enemy. Broad low T silhouette: rotating turret with one long thick barrel emerging from its LOWER FRONT, aiming toward lower-left foreground. On the SAME FRONT face immediately ABOVE the barrel root put a broad horizontal solid RED core with thick near-black frame; from this front three-quarter camera BOTH muzzle opening and entire red core must be clearly visible. No side or rear core. Base is exactly THREE short thick solid unjointed fixed outriggers radiating outward as a tripod, with wide negative spaces between legs; show all three feet, no broad base plate, no walking legs. Light-gray outer armor, medium-gray main body, near-black internal structure and rotation joint. Only red core has color. Simple 3-5 major masses, robust connections. Uniform light-gray background, no floor shadow, no text, no arrows, no gradients, no bloom, no reflections, no scratches, no wires or antennas. Entire object fits with generous margins.
```

## 3. Ranged_Concept_v2.png

저장 경로: `ArtSource/Concepts/Enemies/Ranged_Concept_v2.png`

상태: 생성 후 직접 열어 검토, 최종 채택

입력 이미지 순서:

1. `ArtSource/Concepts/Enemies/Ranged_Concept_v1.png`

```text
Use case: stylized-concept. A nonviolent static fictional videogame robot asset design on a plain background, no action, no firing, no people or injury. Square 1:1 full object sheet for 3D modeling. Reference image 1 supplies only the bold black outlines, chamfered geometric shapes, flat cel-shaded art style. A low wide T-shaped stationary robot with one long rectangular tube extending from the lower front of its rotating head toward the lower-left foreground. Directly ABOVE the tube on the SAME FRONT FACE is a wide horizontal red rectangular panel inside a near-black thick frame. Both the open end of the tube and complete red panel are visible from the front three-quarter view. The red panel must not be on the side. Support is exactly three short thick unjointed solid fixed tripod outriggers, clearly separated by empty spaces; arrange camera to show all three feet. No large base plate. Light gray exterior panels, medium gray body, nearly black structure and rotary joint. Large simple solid parts, no thin details. Uniform light gray backdrop, no ground shadow, no text, no arrows, no reflections, no gradients or glow. Entire object with generous margins. Only the red panel is colored.
```

## 4. Boss_Concept_v2.png

저장 경로: `ArtSource/Concepts/Enemies/Boss_Concept_v2.png`

상태: 생성 후 직접 열어 검토, 최종 채택

입력 이미지 순서:

1. `ArtSource/Concepts/Enemies/Boss_Concept_v1.png`

```text
Use case: stylized-concept. Square 1:1 static fictional robotic boss asset for image-to-3D. Image 1 is an EXACT DESIGN REFERENCE. Preserve its wide inverted-triangle silhouette, central solid red diamond core, two separate floating shoulder shields with exactly three round dark sockets vertically on each, and downward triangular ram. Preserve all major shapes, but show a clear front THREE-QUARTER view with visible thickness. Apply sharply separated three material values: light gray shoulder exterior armor, near-black inset socket plates, medium gray central body, darkest thick diamond core frame and internal joints. The six round sockets remain readable inside their dark plates using simple gray inner rims. Widen the empty air gaps between shoulders and main body. Make the short solid neck connecting lower ram to body at least 60 percent as wide as the ram's top edge, no skinny connector. Bold black outlines, beveled low-poly geometry, flat two-tone cel shading, no texture, no reflection, no bloom. Entire object with margin. Uniform light gray backdrop, no ground shadow, no text, no arrows or measurements, no thin components. Only red core is colored.
```

## 5. Enemy_Lineup_v2.png

저장 경로: `ArtSource/Concepts/Enemies/Enemy_Lineup_v2.png`

상태: 재생성 전: 보스가 사람 키의 약 3.3배로 작고 돌격형도 큼

입력 이미지 순서:

1. `ArtSource/Concepts/Enemies/Rusher_Concept_v2.png`
2. `ArtSource/Concepts/Enemies/Ranged_Concept_v2.png`
3. `ArtSource/Concepts/Enemies/Boss_Concept_v2.png`

```text
Use case: stylized-concept. Wide 16:9 enemy size comparison lineup on uniform light-gray background, no lettering, no labels, no ground shadows. Three supplied images are exact design references, in order rusher, ranged robot, boss. Reproduce their geometry and colors faithfully, do not redesign. Arrange separate non-overlapping figures on a common baseline: small rusher at left, one neutral gray standing human silhouette for scale, ranged robot, then very large boss at right. Human height 1.0, ranged robot height 1.0, rusher height 0.5, boss height 4.5. Floating objects hover slightly above the same baseline. Entire figures fit with margin. Rusher preserves forward-pointing wedge, large red hexagonal upper-front panel, close-fitting thick blades and flat hover disc. Ranged preserves exactly three short fixed tripod feet, long tube and horizontal red FRONT panel ABOVE tube. Boss preserves two detached pale shoulder shields, three circular sockets in each dark plate, dark-framed red diamond core, broad connector and bottom triangular ram. Bold black outlines, light-gray armor, medium-gray body, near-black structure, flat cel shading, no glow, no textures, only red cores colored.
```

## 6. Enemy_Lineup_v2.png

저장 경로: `ArtSource/Concepts/Enemies/Enemy_Lineup_v2.png`

상태: 재생성 최종: 보스 비율 약 3.7배, 4-5배 목표 미달

입력 이미지 순서:

1. `ArtSource/Concepts/Enemies/Rusher_Concept_v2.png`
2. `ArtSource/Concepts/Enemies/Ranged_Concept_v2.png`
3. `ArtSource/Concepts/Enemies/Boss_Concept_v2.png`

```text
Use case: stylized-concept. Wide 16:9 enemy size comparison lineup on uniform light-gray background, no lettering, no labels, no ground shadows. Three supplied images are exact design references, in order rusher, ranged robot, boss. Reproduce their geometry and colors faithfully, do not redesign. Arrange separate non-overlapping figures on a common baseline: small rusher at left, one neutral gray standing human silhouette for scale, ranged robot, then very large boss at right. Human height 1.0, ranged robot height 1.0, rusher height 0.5, boss height 4.5. Floating objects hover slightly above the same baseline. Entire figures fit with margin. Rusher preserves forward-pointing wedge, large red hexagonal upper-front panel, close-fitting thick blades and flat hover disc. Ranged preserves exactly three short fixed tripod feet, long tube and horizontal red FRONT panel ABOVE tube. Boss preserves two detached pale shoulder shields, three circular sockets in each dark plate, dark-framed red diamond core, broad connector and bottom triangular ram. Bold black outlines, light-gray armor, medium-gray body, near-black structure, flat cel shading, no glow, no textures, only red cores colored. Critical scale correction: human silhouette must be only 19 percent of canvas height, boss 85 percent, ranged 19 percent and rusher only 10 percent. All bottoms at 94 percent canvas height. These are orthographic size comparison proportions, not perspective; do not enlarge the small figures to fill empty space. Keep the large empty upper-left background.
```

## 7. Stage_Backdrop_v2.png

저장 경로: `ArtSource/Concepts/Environment/Stage_Backdrop_v2.png`

상태: 재생성 전: 균열 폭이 약 16%로 좁고 왼쪽 계단이 올라가는 형태

입력 이미지 순서:

1. `ArtSource/Concepts/Environment/Stage_Backdrop_v1.png`

```text
Use case: stylized-concept. Create wide 16:9 videogame stage backdrop. Reference image 1 supplies exact bold-outline flat cel shading and concrete column structure, central vanishing-point composition. Night underground SUBWAY TRANSFER HALL checkpoint, recognizable as station. Lower camera to human shoulder height looking forward, not aerial. Retain paired rows of large beveled columns receding on both sides and uncluttered central floor. At far end place a clearly visible horizontal row of SIX low rectangular ticket gates, with a closed metal shutter BEHIND the gates. Left wall has framed subway stair entrance with steps visibly descending into darkness. Hang THREE simple wayfinding boards from ceiling, symbols only arrows circles squares, no letters. Dominant large VERTICAL irregular spatial rift above back wall extending upward into ceiling, width about ONE QUARTER of whole image, height about TWICE shutter height. Rift interior is a SOLID almost-black muted purple area, thin light-purple edge only, no bloom, rays or glow. This dark silhouette must provide a substantial backdrop behind future central gray robot. Restrained desaturated blue-gray environment, sparse fluorescent strips. Yellow-black warning bands ONLY at bases of TWO nearest foreground columns; no bands on remaining columns, door frames or other middle-height surfaces. Exactly two thin light-gray guidance stripes converge along floor toward vanishing point, otherwise empty floor and sparse middle band. No characters, no robots, no UI. Preserve low-poly toon style, clean readable geometry.
```

## 8. Stage_Backdrop_v2.png

저장 경로: `ArtSource/Concepts/Environment/Stage_Backdrop_v2.png`

상태: 생성 후 직접 열어 검토, 최종 채택

입력 이미지 순서:

1. `ArtSource/Concepts/Environment/Stage_Backdrop_v1.png`

```text
Use case: stylized-concept. Create wide 16:9 videogame stage backdrop. Reference image 1 supplies exact bold-outline flat cel shading and concrete column structure, central vanishing-point composition. Night underground SUBWAY TRANSFER HALL checkpoint, recognizable as station. Lower camera to human shoulder height looking forward, not aerial. Retain paired rows of large beveled columns receding on both sides and uncluttered central floor. At far end place a clearly visible horizontal row of SIX low rectangular ticket gates, with a closed metal shutter BEHIND the gates. Left wall has framed subway stair entrance with steps visibly descending into darkness. Hang THREE simple wayfinding boards from ceiling, symbols only arrows circles squares, no letters. Dominant large VERTICAL irregular spatial rift above back wall extending upward into ceiling, width about ONE QUARTER of whole image, height about TWICE shutter height. Rift interior is a SOLID almost-black muted purple area, thin light-purple edge only, no bloom, rays or glow. This dark silhouette must provide a substantial backdrop behind future central gray robot. Restrained desaturated blue-gray environment, sparse fluorescent strips. Yellow-black warning bands ONLY at bases of TWO nearest foreground columns; no bands on remaining columns, door frames or other middle-height surfaces. Exactly two thin light-gray guidance stripes converge along floor toward vanishing point, otherwise empty floor and sparse middle band. No characters, no robots, no UI. Preserve low-poly toon style, clean readable geometry. Critical corrections: the rift must span x=37.5 percent to x=62.5 percent of full canvas, a broad 25-percent-width dark void, from shutter top up into ceiling. Remove fluorescent fixtures from inside this void. LEFT STAIRWELL DESCENDS: show the top landing at the floor level and steps dropping below it behind a low parapet, with the opening disappearing downward, never steps climbing upward into wall. Keep exactly six ticket-gate boxes.
```

## 9. Battle_Mockup_Boss_v2.png

저장 경로: `ArtSource/Mockups/Battle_Mockup_Boss_v2.png`

상태: 도구 입력 경로 5개 제한으로 실행 전 거절

입력 이미지 순서:

1. `ArtSource/Concepts/Enemies/Boss_Concept_v2.png`
2. `ArtSource/Concepts/Environment/Stage_Backdrop_v2.png`
3. `ArtSource/Characters/Rapid/Rapid_Poses.png`
4. `ArtSource/Characters/Charge/Charge_Cover_Source.png`
5. `ArtSource/Characters/Shotgun/Shotgun_Cover_Source.png`
6. `ArtSource/Characters/Rapid/Rapid_Fire_Source.png`
7. `ArtSource/Concepts/Environment/Cover_Concept_v1.png`

```text
Use case: compositing. Create a boss encounter gameplay mockup. Actual input roles: image 1 exact boss design; image 2 exact environment; image 3 Rapid identity and assembled rear pose; image 4 left Charge cover pose; image 5 right Shotgun cover pose; image 6 Rapid rear firing parts to assemble; image 7 exact barrier design. Composition: wide 16:9 fixed-camera fictional game screenshot, bold black outlines and flat cel shading. Camera directly behind allied characters at shoulder height. Preserve reference station architecture with six ticket gates, shutter, large dark purple vertical rift, left stair entrance and two pale floor lines. Bottom foreground: exactly THREE allies in order LEFT purple long-ponytail Charge crouched low behind cover with weapon lowered, CENTER silver-gray bob Rapid with teal trim rising above cover and actively aiming/firing forward, RIGHT orange-haired orange-trim Shotgun crouched low behind cover with weapon lowered. Use supplied character designs exactly, all rear or rear three-quarter views facing into scene, never facing viewer. Assemble the detached arms/rifle in Rapid_Fire_Source onto the torso, no floating limbs. Each ally visible silhouette about 30 percent of image height, anchored to bottom edge; center head near y=68 percent, side heads near y=73 percent. Foreground cropped legs allowed, no tiny distant allies. Exactly three waist-high concrete barriers from cover reference IN FRONT of allies toward enemies; their bottoms may be cropped. Only center ally active, side allies visibly lower in crouched resting poses. Small portrait and ammo widgets clustered at extreme lower LEFT in reserved empty margin, not over any body; one thin horizontal burst gauge at extreme lower RIGHT, no body overlap. Small center reticle. Clean readable middle battle area, no blood or injury, no titles, names, watermarks. One large boss floats centrally in front of the dark rift, above the floor, fully visible behind allies, red diamond core unobscured. Maintain v2 boss pale detached shields, exactly three round sockets each, medium-gray body, thick dark core frame, robust neck and triangular bottom ram. Boss outline contrasts against dark rift. Wide boss health bar near top center, minimal interface. No other enemies.
```

## 10. Battle_Mockup_Boss_v2.png

저장 경로: `ArtSource/Mockups/Battle_Mockup_Boss_v2.png`

상태: 생성 후 직접 열어 검토, 최종 채택

입력 이미지 순서:

1. `ArtSource/Concepts/Enemies/Boss_Concept_v2.png`
2. `ArtSource/Concepts/Environment/Stage_Backdrop_v2.png`
3. `C:/Users/user/AppData/Local/Temp/enemy-v2-allied-reference-sheet.png`
4. `ArtSource/Concepts/Environment/Cover_Concept_v1.png`

```text
Use case: compositing. Create a boss encounter gameplay mockup. Actual input roles: image 1 exact boss design; image 2 exact environment; image 3 is a contact sheet containing all four actual allied reference files: upper-left Rapid_Poses, upper-right Charge_Cover_Source, lower-left Shotgun_Cover_Source, lower-right Rapid_Fire_Source. Use all four for identities and poses. Image 4 exact barrier design. Composition: wide 16:9 fixed-camera fictional game screenshot, bold black outlines and flat cel shading. Camera directly behind allied characters at shoulder height. Preserve reference station architecture with six ticket gates, shutter, large dark purple vertical rift, left stair entrance and two pale floor lines. Bottom foreground: exactly THREE allies in order LEFT purple long-ponytail Charge crouched low behind cover with weapon lowered, CENTER silver-gray bob Rapid with teal trim rising above cover and actively aiming/firing forward, RIGHT orange-haired orange-trim Shotgun crouched low behind cover with weapon lowered. Use supplied character designs exactly, all rear or rear three-quarter views facing into scene, never facing viewer. Assemble the detached arms/rifle in Rapid_Fire_Source onto the torso, no floating limbs. Each ally visible silhouette about 30 percent of image height, anchored to bottom edge; center head near y=68 percent, side heads near y=73 percent. Foreground cropped legs allowed, no tiny distant allies. Exactly three waist-high concrete barriers from cover reference IN FRONT of allies toward enemies; their bottoms may be cropped. Only center ally active, side allies visibly lower in crouched resting poses. Small portrait and ammo widgets clustered at extreme lower LEFT in reserved empty margin, not over any body; one thin horizontal burst gauge at extreme lower RIGHT, no body overlap. Small center reticle. Clean readable middle battle area, no blood or injury, no titles, names, watermarks. One large boss floats centrally in front of the dark rift, above the floor, fully visible behind allies, red diamond core unobscured. Maintain v2 boss pale detached shields, exactly three round sockets each, medium-gray body, thick dark core frame, robust neck and triangular bottom ram. Boss outline contrasts against dark rift. Wide boss health bar near top center, minimal interface. No other enemies.
```

## 11. Battle_Mockup_Wave_v2.png

저장 경로: `ArtSource/Mockups/Battle_Mockup_Wave_v2.png`

상태: 재생성 전: 중앙 아군이 화면 높이 약 41%, 개찰구가 8개로 증가

입력 이미지 순서:

1. `ArtSource/Concepts/Enemies/Rusher_Concept_v2.png`
2. `ArtSource/Concepts/Enemies/Ranged_Concept_v2.png`
3. `ArtSource/Concepts/Environment/Stage_Backdrop_v2.png`
4. `C:/Users/user/AppData/Local/Temp/enemy-v2-allied-reference-sheet.png`
5. `ArtSource/Concepts/Environment/Cover_Concept_v1.png`

```text
Use case: compositing. Create a regular wave encounter gameplay mockup. Actual input roles: image 1 exact rusher design; image 2 exact ranged design; image 3 exact v2 stage; image 4 contact sheet of four actual allied originals (upper-left Rapid_Poses, upper-right Charge_Cover_Source, lower-left Shotgun_Cover_Source, lower-right Rapid_Fire_Source); image 5 exact cover design. Composition: wide 16:9 fixed-camera fictional game screenshot, bold black outlines and flat cel shading. Camera directly behind allied characters at shoulder height. Preserve reference station architecture with six ticket gates, shutter, large dark purple vertical rift, left stair entrance and two pale floor lines. Bottom foreground: exactly THREE allies in order LEFT purple long-ponytail Charge crouched low behind cover with weapon lowered, CENTER silver-gray bob Rapid with teal trim rising above cover and actively aiming/firing forward, RIGHT orange-haired orange-trim Shotgun crouched low behind cover with weapon lowered. Use supplied character designs exactly, all rear or rear three-quarter views facing into scene, never facing viewer. Assemble the detached arms/rifle in Rapid_Fire_Source onto the torso, no floating limbs. Each ally visible silhouette about 30 percent of image height, anchored to bottom edge; center head near y=68 percent, side heads near y=73 percent. Foreground cropped legs allowed, no tiny distant allies. Exactly three waist-high concrete barriers from cover reference IN FRONT of allies toward enemies; their bottoms may be cropped. Only center ally active, side allies visibly lower in crouched resting poses. Small portrait and ammo widgets clustered at extreme lower LEFT in reserved empty margin, not over any body; one thin horizontal burst gauge at extreme lower RIGHT, no body overlap. Small center reticle. Clean readable middle battle area, no blood or injury, no titles, names, watermarks. Populate middle arena with EXACTLY FIVE rusher robots at varied depths, no overlapping silhouettes: two larger near foreground above covers, two medium at middle depth, one small near far center. Preserve arrowhead wedge pointing toward camera, thick close-fitting blades, large red hexagonal core on upper-front sloping face and single flat hover disc. EXACTLY TWO ranged robots, far left and far right near ticket gates, with short fixed three-foot tripod and long barrel below horizontal red front core. Far robots deliberately small but red cores stay visible. Seven enemies TOTAL. NO BOSS and no boss health bar. Only a small wave progress bar at top center. Match previous composition by placing ally centers near x=24 percent, 47 percent, 76 percent; head heights center y=67 percent, left y=71 percent, right y=74 percent, cropped at bottom. Center Rapid alone has a small muzzle flash. Side allies keep weapons lowered behind cover. Retain mostly empty middle floor around robots for readability.
```

## 12. Battle_Mockup_Wave_v2.png

저장 경로: `ArtSource/Mockups/Battle_Mockup_Wave_v2.png`

상태: 생성 후 직접 열어 검토, 최종 채택

입력 이미지 순서:

1. `ArtSource/Concepts/Enemies/Rusher_Concept_v2.png`
2. `ArtSource/Concepts/Enemies/Ranged_Concept_v2.png`
3. `ArtSource/Concepts/Environment/Stage_Backdrop_v2.png`
4. `C:/Users/user/AppData/Local/Temp/enemy-v2-allied-reference-sheet.png`
5. `ArtSource/Concepts/Environment/Cover_Concept_v1.png`

```text
Use case: compositing. Create a regular wave encounter gameplay mockup. Actual input roles: image 1 exact rusher design; image 2 exact ranged design; image 3 exact v2 stage; image 4 contact sheet of four actual allied originals (upper-left Rapid_Poses, upper-right Charge_Cover_Source, lower-left Shotgun_Cover_Source, lower-right Rapid_Fire_Source); image 5 exact cover design. Composition: wide 16:9 fixed-camera fictional game screenshot, bold black outlines and flat cel shading. Camera directly behind allied characters at shoulder height. Preserve reference station architecture with six ticket gates, shutter, large dark purple vertical rift, left stair entrance and two pale floor lines. Bottom foreground: exactly THREE allies in order LEFT purple long-ponytail Charge crouched low behind cover with weapon lowered, CENTER silver-gray bob Rapid with teal trim rising above cover and actively aiming/firing forward, RIGHT orange-haired orange-trim Shotgun crouched low behind cover with weapon lowered. Use supplied character designs exactly, all rear or rear three-quarter views facing into scene, never facing viewer. Assemble the detached arms/rifle in Rapid_Fire_Source onto the torso, no floating limbs. Each ally visible silhouette about 30 percent of image height, anchored to bottom edge; center head near y=68 percent, side heads near y=73 percent. Foreground cropped legs allowed, no tiny distant allies. Exactly three waist-high concrete barriers from cover reference IN FRONT of allies toward enemies; their bottoms may be cropped. Only center ally active, side allies visibly lower in crouched resting poses. Small portrait and ammo widgets clustered at extreme lower LEFT in reserved empty margin, not over any body; one thin horizontal burst gauge at extreme lower RIGHT, no body overlap. Small center reticle. Clean readable middle battle area, no blood or injury, no titles, names, watermarks. Populate middle arena with EXACTLY FIVE rusher robots at varied depths, no overlapping silhouettes: two larger near foreground above covers, two medium at middle depth, one small near far center. Preserve arrowhead wedge pointing toward camera, thick close-fitting blades, large red hexagonal core on upper-front sloping face and single flat hover disc. EXACTLY TWO ranged robots, far left and far right near ticket gates, with short fixed three-foot tripod and long barrel below horizontal red front core. Far robots deliberately small but red cores stay visible. Seven enemies TOTAL. NO BOSS and no boss health bar. Only a small wave progress bar at top center. Match previous composition by placing ally centers near x=24 percent, 47 percent, 76 percent; head heights center y=67 percent, left y=71 percent, right y=74 percent, cropped at bottom. Center Rapid alone has a small muzzle flash. Side allies keep weapons lowered behind cover. Retain mostly empty middle floor around robots for readability. REQUIRED CORRECTION: Center ally must occupy only bottom 30-32 percent vertically, head top at y=68 percent, not y=59 percent. Left and right head tops y=72 percent, cropped knees at bottom. Scale all three down slightly and move them downward, keeping them close foreground. Match original stage geometry, ticket gates exactly SIX not eight, horizon at y=64 percent as supplied stage. Preserve exactly five rushers and two ranged robots, all red cores visible.
```

## 최종 검토

- 돌격형: 앞쪽 쐐기 끝, 큰 육각 코어, 굵은 축, 아래 원반을 확인했다.
- 원거리형: 포신 위 정면 코어와 고정 지지대 3개를 확인했다.
- 보스: 어깨별 발사구 3개, 분리된 어깨, 굵은 충각 연결부를 확인했다.
- 라인업: 한 번 재생성했으나 보스가 사람 키의 약 3.7배로 목표 4-5배보다 작다. 돌격형 높이도 사람의 약 0.65배로 상반신 기준보다 크다.
- 스테이지: 개찰구 6개와 천장까지 이어지는 큰 균열을 확인했다. 재생성 후에도 왼쪽 계단의 하행 방향은 명확하지 않다.
- 보스 목업: 아군 높이는 중앙 약 34%, 좌우 약 26-30%이며 중앙 사격과 양옆 엄폐를 구분했다.
- 웨이브 목업: 재생성 후 중앙 아군 약 32%, 좌우 약 24-28%이다. 돌격형 5기와 원거리형 2기, 개찰구 6개와 작은 적의 적색 코어를 확인했다. 배경 원근과 엄폐물 파손 위치는 보스 목업과 완전히 같지 않다.


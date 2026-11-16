# Tripo 입력 이미지 실제 프롬프트 v1

작성일: 2026-11-16
도구: 내장 image_gen
출력 폴더: ArtSource/Concepts/TripoInput/
각 파일마다 최초 생성과 1회 재생성의 실제 프롬프트 전문을 기록한다. 모든 참조 경로는 referenced_image_paths로 전달했다. transparent_background는 false로 지정했다. 기존 이미지와 다른 문서는 수정하지 않았다.

## Boss_Body.png

재생성 이유: 화면 점유율 과다, 배경 질감. 최종 선택: 2차. 남은 차이: 높이가 약 90%이며 미세한 배경 질감이 남는다.

### 1차 생성

실제 입력 이미지:

- `E:/Git/Potpolio_Repos/unity-aim-combat-slice/ArtSource/Concepts/Enemies/Boss_Concept_v3.png`
- `E:/Git/Potpolio_Repos/unity-aim-combat-slice/ArtSource/Concepts/Enemies/Boss_Parts_v3.png`

```text
Use case: stylized-concept. Create one square 1:1 raster image for single-image-to-3D reconstruction. Exactly ONE connected solid object, centered, occupying about 80% of the image with clear margins and no cropping. Front three-quarter view from slightly above, showing front, one side and top, weak perspective. Background must be absolutely uniform near-white light gray, no gradient or texture, no floor, NO cast shadow or contact shadow. Match the supplied v3 reference design and art style: dark gunmetal, mid-gray top faces and bevels, bold black outlines, dark crimson stripes, bright red core where applicable, flat two-tone cel shading. All components have substantial solid thickness. No thin rods, wires, trusses, wafer-thin plates, cloth, talismans, flags, text, labels, symbols, arrows, frame, detached fragments, floating debris, smoke, fire, rays, bloom or glow. Bright areas are crisp flat color. Do not reproduce reference sheet labels or additional objects.
Input image 1 Boss_Concept_v3.png: primary assembled-design reference. Input image 2 Boss_Parts_v3.png: construction reference only. Output Boss_Body.png: isolate only the central boss body, preserving the tall faceted head with vertical crimson visor, torso with large bright red hexagonal core, attached thick upper and lower chevron armor and attached downward ram. These are joined into ONE continuous body, no exploded spacing. Remove BOTH wing arms entirely and remove every hovering shard. Leave a stout octagonal socket on each shoulder, a short cylindrical opening with dark crimson interior, sized to accept the arm joint seen in reference A. Bilaterally symmetric geometry despite the three-quarter camera. Preserve reference proportions and design. No extra limbs or stand.
```

### 2차 생성

실제 입력 이미지:

- `E:/Git/Potpolio_Repos/unity-aim-combat-slice/ArtSource/Concepts/Enemies/Boss_Concept_v3.png`
- `E:/Git/Potpolio_Repos/unity-aim-combat-slice/ArtSource/Concepts/Enemies/Boss_Parts_v3.png`

```text
Use case: stylized-concept. Create one square 1:1 raster image for single-image-to-3D reconstruction. Exactly ONE connected solid object, centered, occupying about 80% of the image with clear margins and no cropping. Front three-quarter view from slightly above, showing front, one side and top, weak perspective. Background must be absolutely uniform near-white light gray, no gradient or texture, no floor, NO cast shadow or contact shadow. Match the supplied v3 reference design and art style: dark gunmetal, mid-gray top faces and bevels, bold black outlines, dark crimson stripes, bright red core where applicable, flat two-tone cel shading. All components have substantial solid thickness. No thin rods, wires, trusses, wafer-thin plates, cloth, talismans, flags, text, labels, symbols, arrows, frame, detached fragments, floating debris, smoke, fire, rays, bloom or glow. Bright areas are crisp flat color. Do not reproduce reference sheet labels or additional objects.
Input image 1 Boss_Concept_v3.png: primary assembled-design reference. Input image 2 Boss_Parts_v3.png: construction reference only. Output Boss_Body.png: isolate only the central boss body, preserving the tall faceted head with vertical crimson visor, torso with large bright red hexagonal core, attached thick upper and lower chevron armor and attached downward ram. These are joined into ONE continuous body, no exploded spacing. Remove BOTH wing arms entirely and remove every hovering shard. Leave a stout octagonal socket on each shoulder, a short cylindrical opening with dark crimson interior, sized to accept the arm joint seen in reference A. Bilaterally symmetric geometry despite the three-quarter camera. Preserve reference proportions and design. No extra limbs or stand.
CORRECTION: object height must be exactly approximately 80 percent of the square, leaving 10 percent empty margin above and below. Background is a single flat RGB 245,245,245 fill, absolutely no paper grain, mottling, shading or vignette. Core is a uniform flat red face. Retain body design.
```

## Boss_Arm_Intact.png

재생성 이유: 배경 질감. 최종 선택: 2차. 남은 차이: 미세한 배경 질감.

### 1차 생성

실제 입력 이미지:

- `E:/Git/Potpolio_Repos/unity-aim-combat-slice/ArtSource/Concepts/Enemies/Boss_Parts_v3.png`

```text
Use case: stylized-concept. Create one square 1:1 raster image for single-image-to-3D reconstruction. Exactly ONE connected solid object, centered, occupying about 80% of the image with clear margins and no cropping. Front three-quarter view from slightly above, showing front, one side and top, weak perspective. Background must be absolutely uniform near-white light gray, no gradient or texture, no floor, NO cast shadow or contact shadow. Match the supplied v3 reference design and art style: dark gunmetal, mid-gray top faces and bevels, bold black outlines, dark crimson stripes, bright red core where applicable, flat two-tone cel shading. All components have substantial solid thickness. No thin rods, wires, trusses, wafer-thin plates, cloth, talismans, flags, text, labels, symbols, arrows, frame, detached fragments, floating debris, smoke, fire, rays, bloom or glow. Bright areas are crisp flat color. Do not reproduce reference sheet labels or additional objects.
Input Boss_Parts_v3.png is the design reference. Isolate ONLY arm A at screen left, not the body or other parts. Output Boss_Arm_Intact.png. Three thick long wedge-shaped launcher plates fan out from one stout octagonal joint at image right, the three square crimson firing apertures face lower-left. Top plate has the dark crimson stripe. Preserve the reference A silhouette and connected fan construction. The body-facing right end must have a short SOLID male octagonal insertion peg, fitting the boss shoulder socket, not another female socket. No extra pieces. Exactly three plates, generous solid thickness. Fit the object into central 80 percent of canvas. Use a perfectly uniform light gray fill background without paper texture.
```

### 2차 생성

실제 입력 이미지:

- `E:/Git/Potpolio_Repos/unity-aim-combat-slice/ArtSource/Concepts/Enemies/Boss_Parts_v3.png`

```text
Use case: stylized-concept. Create one square 1:1 raster image for single-image-to-3D reconstruction. Exactly ONE connected solid object, centered, occupying about 80% of the image with clear margins and no cropping. Front three-quarter view from slightly above, showing front, one side and top, weak perspective. Background must be absolutely uniform near-white light gray, no gradient or texture, no floor, NO cast shadow or contact shadow. Match the supplied v3 reference design and art style: dark gunmetal, mid-gray top faces and bevels, bold black outlines, dark crimson stripes, bright red core where applicable, flat two-tone cel shading. All components have substantial solid thickness. No thin rods, wires, trusses, wafer-thin plates, cloth, talismans, flags, text, labels, symbols, arrows, frame, detached fragments, floating debris, smoke, fire, rays, bloom or glow. Bright areas are crisp flat color. Do not reproduce reference sheet labels or additional objects.
Input Boss_Parts_v3.png is the design reference. Isolate ONLY arm A at screen left, not the body or other parts. Output Boss_Arm_Intact.png. Three thick long wedge-shaped launcher plates fan out from one stout octagonal joint at image right, the three square crimson firing apertures face lower-left. Top plate has the dark crimson stripe. Preserve the reference A silhouette and connected fan construction. The body-facing right end must have a short SOLID male octagonal insertion peg, fitting the boss shoulder socket, not another female socket. No extra pieces. Exactly three plates, generous solid thickness. Fit the object into central 80 percent of canvas. Use a perfectly uniform light gray fill background without paper texture.
CORRECTION: eliminate all background grain and variation. Pure uniform flat RGB 245,245,245 background. Keep arm silhouette, 3 plates and male peg, with full 10 percent blank margin on every side.
```

## Boss_Arm_Damaged.png

재생성 이유: 위쪽 판의 적색 홈이 2개보다 많음, 배경 질감. 최종 선택: 1차. 2차는 가운데 판이 분리된 조각처럼 보여 선택하지 않았다. 최종본의 관절 위치와 크기는 온전한 팔과 육안상 일치하지만 위쪽 판의 추가 적색 패임과 미세한 배경 질감이 남는다.

### 1차 생성

실제 입력 이미지:

- `E:/Git/Potpolio_Repos/unity-aim-combat-slice/ArtSource/Concepts/TripoInput/Boss_Arm_Intact.png`

```text
Use case: stylized-concept. Create one square 1:1 raster image for single-image-to-3D reconstruction. Exactly ONE connected solid object, centered, occupying about 80% of the image with clear margins and no cropping. Front three-quarter view from slightly above, showing front, one side and top, weak perspective. Background must be absolutely uniform near-white light gray, no gradient or texture, no floor, NO cast shadow or contact shadow. Match the supplied v3 reference design and art style: dark gunmetal, mid-gray top faces and bevels, bold black outlines, dark crimson stripes, bright red core where applicable, flat two-tone cel shading. All components have substantial solid thickness. No thin rods, wires, trusses, wafer-thin plates, cloth, talismans, flags, text, labels, symbols, arrows, frame, detached fragments, floating debris, smoke, fire, rays, bloom or glow. Bright areas are crisp flat color. Do not reproduce reference sheet labels or additional objects.
Use case: precise-object-edit. Input Boss_Arm_Intact.png is the exact edit target. Output Boss_Arm_Damaged.png. Lock the original canvas, camera, scale, position, right octagonal joint and male peg pixel-for-pixel. Do not recenter or rescale. Modify ONLY the three fan plates: middle plate is snapped halfway along its length leaving a short thick attached stump, with no detached piece. Upper and lower surviving long plates each have exactly TWO large thick wedge-shaped gouges filled crisp bright red; chip their outside corners substantially. Keep them solid thick wedges, not skeletal frames. Retain the top plate crimson stripe. No wires, struts, trusses, floating fragments. Preserve the entire right joint outline and its location exactly. Uniform near-white gray background.
```

### 2차 생성

실제 입력 이미지:

- `E:/Git/Potpolio_Repos/unity-aim-combat-slice/ArtSource/Concepts/TripoInput/Boss_Arm_Intact.png`

```text
Use case: stylized-concept. Create one square 1:1 raster image for single-image-to-3D reconstruction. Exactly ONE connected solid object, centered, occupying about 80% of the image with clear margins and no cropping. Front three-quarter view from slightly above, showing front, one side and top, weak perspective. Background must be absolutely uniform near-white light gray, no gradient or texture, no floor, NO cast shadow or contact shadow. Match the supplied v3 reference design and art style: dark gunmetal, mid-gray top faces and bevels, bold black outlines, dark crimson stripes, bright red core where applicable, flat two-tone cel shading. All components have substantial solid thickness. No thin rods, wires, trusses, wafer-thin plates, cloth, talismans, flags, text, labels, symbols, arrows, frame, detached fragments, floating debris, smoke, fire, rays, bloom or glow. Bright areas are crisp flat color. Do not reproduce reference sheet labels or additional objects.
Use case: precise-object-edit. Input Boss_Arm_Intact.png is the exact edit target. Output Boss_Arm_Damaged.png. Lock the original canvas, camera, scale, position, right octagonal joint and male peg pixel-for-pixel. Do not recenter or rescale. Modify ONLY the three fan plates: middle plate is snapped halfway along its length leaving a short thick attached stump, with no detached piece. Upper and lower surviving long plates each have exactly TWO large thick wedge-shaped gouges filled crisp bright red; chip their outside corners substantially. Keep them solid thick wedges, not skeletal frames. Retain the top plate crimson stripe. No wires, struts, trusses, floating fragments. Preserve the entire right joint outline and its location exactly. Uniform near-white gray background.
CORRECTION: absolutely flat untextured RGB 245,245,245 background, no grain. Exactly two bright red gouges per surviving plate, not three. Keep original intact input joint geometry and pixel position unchanged.
```

## Boss_Arm_Stump.png

재생성 이유: 연결부가 삽입 돌기보다 구멍처럼 보임, 점유율 부족, 배경 질감. 최종 선택: 2차. 남은 차이: 미세한 배경 질감. 팔각 연결 규격은 육안상 호환되나 실제 3D 치수 일치를 보장하는 도면은 아니다.

### 1차 생성

실제 입력 이미지:

- `E:/Git/Potpolio_Repos/unity-aim-combat-slice/ArtSource/Concepts/Enemies/Boss_Part_States_v3.png`

```text
Use case: stylized-concept. Create one square 1:1 raster image for single-image-to-3D reconstruction. Exactly ONE connected solid object, centered, occupying about 80% of the image with clear margins and no cropping. Front three-quarter view from slightly above, showing front, one side and top, weak perspective. Background must be absolutely uniform near-white light gray, no gradient or texture, no floor, NO cast shadow or contact shadow. Match the supplied v3 reference design and art style: dark gunmetal, mid-gray top faces and bevels, bold black outlines, dark crimson stripes, bright red core where applicable, flat two-tone cel shading. All components have substantial solid thickness. No thin rods, wires, trusses, wafer-thin plates, cloth, talismans, flags, text, labels, symbols, arrows, frame, detached fragments, floating debris, smoke, fire, rays, bloom or glow. Bright areas are crisp flat color. Do not reproduce reference sheet labels or additional objects.
Input Boss_Part_States_v3.png: isolate ONLY the small upper-right arm stump as the design reference. Output Boss_Arm_Stump.png: a single simple compact stout octagonal joint connector with a rough broken polygonal dark-crimson crystal-like end. Right end is a short solid octagonal male peg fitting the boss socket; left end is a firmly attached jagged dark-red fracture mass. No arm plates, no separate shards, no lower ram, no other state. Maintain the v3 octagonal joint proportions and rim. Center within 80 percent canvas. Absolutely flat RGB 245,245,245 background without grain or gradients.
```

### 2차 생성

실제 입력 이미지:

- `E:/Git/Potpolio_Repos/unity-aim-combat-slice/ArtSource/Concepts/Enemies/Boss_Part_States_v3.png`

```text
Use case: stylized-concept. Create one square 1:1 raster image for single-image-to-3D reconstruction. Exactly ONE connected solid object, centered, occupying about 80% of the image with clear margins and no cropping. Front three-quarter view from slightly above, showing front, one side and top, weak perspective. Background must be absolutely uniform near-white light gray, no gradient or texture, no floor, NO cast shadow or contact shadow. Match the supplied v3 reference design and art style: dark gunmetal, mid-gray top faces and bevels, bold black outlines, dark crimson stripes, bright red core where applicable, flat two-tone cel shading. All components have substantial solid thickness. No thin rods, wires, trusses, wafer-thin plates, cloth, talismans, flags, text, labels, symbols, arrows, frame, detached fragments, floating debris, smoke, fire, rays, bloom or glow. Bright areas are crisp flat color. Do not reproduce reference sheet labels or additional objects.
Input Boss_Part_States_v3.png: isolate ONLY the small upper-right arm stump as the design reference. Output Boss_Arm_Stump.png: a single simple compact stout octagonal joint connector with a rough broken polygonal dark-crimson crystal-like end. Right end is a short solid octagonal male peg fitting the boss socket; left end is a firmly attached jagged dark-red fracture mass. No arm plates, no separate shards, no lower ram, no other state. Maintain the v3 octagonal joint proportions and rim. Center within 80 percent canvas. Absolutely flat RGB 245,245,245 background without grain or gradients.
CORRECTION: the right end must protrude as a solid male octagonal peg, not a recessed crimson hole. Widen object to occupy 80 percent of canvas. Remove all background texture, fill with one single near-white gray color.
```

## Rusher.png

재생성 이유: 배경 질감, 여백 부족. 최종 선택: 2차. 남은 차이: 미세한 배경 질감과 코어 색면의 약한 명도 변화.

### 1차 생성

실제 입력 이미지:

- `E:/Git/Potpolio_Repos/unity-aim-combat-slice/ArtSource/Concepts/Enemies/Rusher_Concept_v3.png`

```text
Use case: stylized-concept. Create one square 1:1 raster image for single-image-to-3D reconstruction. Exactly ONE connected solid object, centered, occupying about 80% of the image with clear margins and no cropping. Front three-quarter view from slightly above, showing front, one side and top, weak perspective. Background must be absolutely uniform near-white light gray, no gradient or texture, no floor, NO cast shadow or contact shadow. Match the supplied v3 reference design and art style: dark gunmetal, mid-gray top faces and bevels, bold black outlines, dark crimson stripes, bright red core where applicable, flat two-tone cel shading. All components have substantial solid thickness. No thin rods, wires, trusses, wafer-thin plates, cloth, talismans, flags, text, labels, symbols, arrows, frame, detached fragments, floating debris, smoke, fire, rays, bloom or glow. Bright areas are crisp flat color. Do not reproduce reference sheet labels or additional objects.
Input Rusher_Concept_v3.png is the exact design reference. Output Rusher.png: redraw this same charging enemy as one connected solid model. Preserve arrowhead wedge body, big bright red hexagonal core on the sloping upper surface, two substantial side blade plates with crimson strips, rear armor, stout side joints, and attached lower hovering disk. Disk is physically joined to body, no floating separate component. Thicken blades into substantial wedges suitable for 3D. Keep the reference orientation. Object fits within central 80 percent canvas. Uniform solid RGB 245,245,245 background, no grain, no gradient. Bright red core is one flat color without bloom.
```

### 2차 생성

실제 입력 이미지:

- `E:/Git/Potpolio_Repos/unity-aim-combat-slice/ArtSource/Concepts/Enemies/Rusher_Concept_v3.png`

```text
Use case: stylized-concept. Create one square 1:1 raster image for single-image-to-3D reconstruction. Exactly ONE connected solid object, centered, occupying about 80% of the image with clear margins and no cropping. Front three-quarter view from slightly above, showing front, one side and top, weak perspective. Background must be absolutely uniform near-white light gray, no gradient or texture, no floor, NO cast shadow or contact shadow. Match the supplied v3 reference design and art style: dark gunmetal, mid-gray top faces and bevels, bold black outlines, dark crimson stripes, bright red core where applicable, flat two-tone cel shading. All components have substantial solid thickness. No thin rods, wires, trusses, wafer-thin plates, cloth, talismans, flags, text, labels, symbols, arrows, frame, detached fragments, floating debris, smoke, fire, rays, bloom or glow. Bright areas are crisp flat color. Do not reproduce reference sheet labels or additional objects.
Input Rusher_Concept_v3.png is the exact design reference. Output Rusher.png: redraw this same charging enemy as one connected solid model. Preserve arrowhead wedge body, big bright red hexagonal core on the sloping upper surface, two substantial side blade plates with crimson strips, rear armor, stout side joints, and attached lower hovering disk. Disk is physically joined to body, no floating separate component. Thicken blades into substantial wedges suitable for 3D. Keep the reference orientation. Object fits within central 80 percent canvas. Uniform solid RGB 245,245,245 background, no grain, no gradient. Bright red core is one flat color without bloom.
CORRECTION: no background texture or noise whatsoever; background must be one uniform flat light-gray color. Reduce object slightly so its maximum dimension is 80 percent of canvas. Preserve the v3 design and solid connected construction.
```

## Ranged.png

재생성 이유: 배경 질감. 최종 선택: 2차. 남은 차이: 미세한 배경 질감과 코어 색면의 약한 명도 변화.

### 1차 생성

실제 입력 이미지:

- `E:/Git/Potpolio_Repos/unity-aim-combat-slice/ArtSource/Concepts/Enemies/Ranged_Concept_v3.png`

```text
Use case: stylized-concept. Create one square 1:1 raster image for single-image-to-3D reconstruction. Exactly ONE connected solid object, centered, occupying about 80% of the image with clear margins and no cropping. Front three-quarter view from slightly above, showing front, one side and top, weak perspective. Background must be absolutely uniform near-white light gray, no gradient or texture, no floor, NO cast shadow or contact shadow. Match the supplied v3 reference design and art style: dark gunmetal, mid-gray top faces and bevels, bold black outlines, dark crimson stripes, bright red core where applicable, flat two-tone cel shading. All components have substantial solid thickness. No thin rods, wires, trusses, wafer-thin plates, cloth, talismans, flags, text, labels, symbols, arrows, frame, detached fragments, floating debris, smoke, fire, rays, bloom or glow. Bright areas are crisp flat color. Do not reproduce reference sheet labels or additional objects.
Input Ranged_Concept_v3.png is the exact design reference. Output Ranged.png. Redraw the same ranged enemy: squat gunmetal armored turret body, horizontal bright-red rectangular core on front, thick square-muzzle cannon directly below core, exactly three stout thick attached legs, small thick faceted horn on top with crimson inset. Preserve reference side armor, crimson bands and angular proportions. One connected solid object. No thin antenna. Fit within central 80 percent with generous empty margins. Background is single uniform RGB 245,245,245, absolutely no texture or gradient. Red core is one flat color, no glow.
```

### 2차 생성

실제 입력 이미지:

- `E:/Git/Potpolio_Repos/unity-aim-combat-slice/ArtSource/Concepts/Enemies/Ranged_Concept_v3.png`

```text
Use case: stylized-concept. Create one square 1:1 raster image for single-image-to-3D reconstruction. Exactly ONE connected solid object, centered, occupying about 80% of the image with clear margins and no cropping. Front three-quarter view from slightly above, showing front, one side and top, weak perspective. Background must be absolutely uniform near-white light gray, no gradient or texture, no floor, NO cast shadow or contact shadow. Match the supplied v3 reference design and art style: dark gunmetal, mid-gray top faces and bevels, bold black outlines, dark crimson stripes, bright red core where applicable, flat two-tone cel shading. All components have substantial solid thickness. No thin rods, wires, trusses, wafer-thin plates, cloth, talismans, flags, text, labels, symbols, arrows, frame, detached fragments, floating debris, smoke, fire, rays, bloom or glow. Bright areas are crisp flat color. Do not reproduce reference sheet labels or additional objects.
Input Ranged_Concept_v3.png is the exact design reference. Output Ranged.png. Redraw the same ranged enemy: squat gunmetal armored turret body, horizontal bright-red rectangular core on front, thick square-muzzle cannon directly below core, exactly three stout thick attached legs, small thick faceted horn on top with crimson inset. Preserve reference side armor, crimson bands and angular proportions. One connected solid object. No thin antenna. Fit within central 80 percent with generous empty margins. Background is single uniform RGB 245,245,245, absolutely no texture or gradient. Red core is one flat color, no glow.
CORRECTION: replace the textured background with perfectly uniform single-color near-white gray. Absolutely no paper grain, noise, vignette or gradient. Preserve all enemy geometry and its three thick legs.
```

## Cover_Intact.png

재생성 이유: 여백 부족, 노란 띠의 마모 부족, 배경 질감. 최종 선택: 2차. 남은 차이: 폭이 약 86%, 미세한 배경 질감.

### 1차 생성

실제 입력 이미지:

- `E:/Git/Potpolio_Repos/unity-aim-combat-slice/ArtSource/Concepts/Environment/Stage_Backdrop_v3.png`

```text
Use case: stylized-concept. Create one square 1:1 raster image for single-image-to-3D reconstruction. Exactly ONE connected solid object, centered, occupying about 80% of the image with clear margins and no cropping. Front three-quarter view from slightly above, showing front, one side and top, weak perspective. Background must be absolutely uniform near-white light gray, no gradient or texture, no floor, NO cast shadow or contact shadow. Match the supplied v3 reference design and art style: dark gunmetal, mid-gray top faces and bevels, bold black outlines, dark crimson stripes, bright red core where applicable, flat two-tone cel shading. All components have substantial solid thickness. No thin rods, wires, trusses, wafer-thin plates, cloth, talismans, flags, text, labels, symbols, arrows, frame, detached fragments, floating debris, smoke, fire, rays, bloom or glow. Bright areas are crisp flat color. Do not reproduce reference sheet labels or additional objects.
Input Stage_Backdrop_v3.png is the design reference, specifically ONE of the large foreground bottom cover blocks, not the small road barriers. Output Cover_Intact.png: one waist-high wide thick concrete block, chamfered corners and two stout dark metal lifting handles firmly attached on the left and right of its top. Broad dark-gray concrete front, lighter gray bevel and top, worn yellow diagonal stripe band near the upper front. Preserve the foreground block proportions and simple chunky silhouette. Remove all paper talismans, red clips and symbols. Concrete is flat cel-shaded gray rather than metallic; no red core on this object. Handles have very thick solid rectangular cross-sections, not thin rods. Show front, right side and top at slight elevated three-quarter angle. Block fills 80 percent canvas width, no ground plane or cast/contact shadow. Pure single-color near-white gray background without texture.
```

### 2차 생성

실제 입력 이미지:

- `E:/Git/Potpolio_Repos/unity-aim-combat-slice/ArtSource/Concepts/Environment/Stage_Backdrop_v3.png`

```text
Use case: stylized-concept. Create one square 1:1 raster image for single-image-to-3D reconstruction. Exactly ONE connected solid object, centered, occupying about 80% of the image with clear margins and no cropping. Front three-quarter view from slightly above, showing front, one side and top, weak perspective. Background must be absolutely uniform near-white light gray, no gradient or texture, no floor, NO cast shadow or contact shadow. Match the supplied v3 reference design and art style: dark gunmetal, mid-gray top faces and bevels, bold black outlines, dark crimson stripes, bright red core where applicable, flat two-tone cel shading. All components have substantial solid thickness. No thin rods, wires, trusses, wafer-thin plates, cloth, talismans, flags, text, labels, symbols, arrows, frame, detached fragments, floating debris, smoke, fire, rays, bloom or glow. Bright areas are crisp flat color. Do not reproduce reference sheet labels or additional objects.
Input Stage_Backdrop_v3.png is the design reference, specifically ONE of the large foreground bottom cover blocks, not the small road barriers. Output Cover_Intact.png: one waist-high wide thick concrete block, chamfered corners and two stout dark metal lifting handles firmly attached on the left and right of its top. Broad dark-gray concrete front, lighter gray bevel and top, worn yellow diagonal stripe band near the upper front. Preserve the foreground block proportions and simple chunky silhouette. Remove all paper talismans, red clips and symbols. Concrete is flat cel-shaded gray rather than metallic; no red core on this object. Handles have very thick solid rectangular cross-sections, not thin rods. Show front, right side and top at slight elevated three-quarter angle. Block fills 80 percent canvas width, no ground plane or cast/contact shadow. Pure single-color near-white gray background without texture.
CORRECTION: reduce block width to 80 percent of the square, leaving 10 percent margins left and right. Make yellow diagonal painted stripes visibly worn with a few solid gray paint chips. Background must be one single flat near-white gray color without grain or variation. Keep both handles very thick.
```

## Cover_Broken.png

재생성 이유: 노란 띠가 파손면을 따라 아래로 이동함, 배경 질감. 최종 선택: 2차. 밑부분 외곽과 위치는 온전한 이미지와 육안상 일치. 남은 차이: 폭이 약 86%, 미세한 배경 질감.

### 1차 생성

실제 입력 이미지:

- `E:/Git/Potpolio_Repos/unity-aim-combat-slice/ArtSource/Concepts/TripoInput/Cover_Intact.png`

```text
Use case: stylized-concept. Create one square 1:1 raster image for single-image-to-3D reconstruction. Exactly ONE connected solid object, centered, occupying about 80% of the image with clear margins and no cropping. Front three-quarter view from slightly above, showing front, one side and top, weak perspective. Background must be absolutely uniform near-white light gray, no gradient or texture, no floor, NO cast shadow or contact shadow. Match the supplied v3 reference design and art style: dark gunmetal, mid-gray top faces and bevels, bold black outlines, dark crimson stripes, bright red core where applicable, flat two-tone cel shading. All components have substantial solid thickness. No thin rods, wires, trusses, wafer-thin plates, cloth, talismans, flags, text, labels, symbols, arrows, frame, detached fragments, floating debris, smoke, fire, rays, bloom or glow. Bright areas are crisp flat color. Do not reproduce reference sheet labels or additional objects.
Use case: precise-object-edit. Input Cover_Intact.png is the exact edit target. Output Cover_Broken.png. Lock canvas, camera, size, placement, entire bottom two-thirds and the bottom silhouette pixel-for-pixel. Do not rescale or recenter the object. Break away roughly the upper third of the concrete block into a large jagged missing area, mostly left and center. Leave only the right thick lifting handle attached on a remaining concrete shoulder. Show exactly THREE SHORT THICK exposed rebar stubs firmly embedded in the fractured top, chunky solid cross-sections, never thin wires or long rods. No detached rubble. Preserve lower front, both bottom feet, bottom edge, right side below fracture and all lower cracks in their exact original positions. Keep concrete gray, black outlines, flat two-tone shading; no red core. Single uniform near-white light gray background, absolutely no shadow, grain or gradient.
```

### 2차 생성

실제 입력 이미지:

- `E:/Git/Potpolio_Repos/unity-aim-combat-slice/ArtSource/Concepts/TripoInput/Cover_Intact.png`

```text
Use case: stylized-concept. Create one square 1:1 raster image for single-image-to-3D reconstruction. Exactly ONE connected solid object, centered, occupying about 80% of the image with clear margins and no cropping. Front three-quarter view from slightly above, showing front, one side and top, weak perspective. Background must be absolutely uniform near-white light gray, no gradient or texture, no floor, NO cast shadow or contact shadow. Match the supplied v3 reference design and art style: dark gunmetal, mid-gray top faces and bevels, bold black outlines, dark crimson stripes, bright red core where applicable, flat two-tone cel shading. All components have substantial solid thickness. No thin rods, wires, trusses, wafer-thin plates, cloth, talismans, flags, text, labels, symbols, arrows, frame, detached fragments, floating debris, smoke, fire, rays, bloom or glow. Bright areas are crisp flat color. Do not reproduce reference sheet labels or additional objects.
Use case: precise-object-edit. Input Cover_Intact.png is the exact edit target. Output Cover_Broken.png. Lock canvas, camera, size, placement, entire bottom two-thirds and the bottom silhouette pixel-for-pixel. Do not rescale or recenter the object. Break away roughly the upper third of the concrete block into a large jagged missing area, mostly left and center. Leave only the right thick lifting handle attached on a remaining concrete shoulder. Show exactly THREE SHORT THICK exposed rebar stubs firmly embedded in the fractured top, chunky solid cross-sections, never thin wires or long rods. No detached rubble. Preserve lower front, both bottom feet, bottom edge, right side below fracture and all lower cracks in their exact original positions. Keep concrete gray, black outlines, flat two-tone shading; no red core. Single uniform near-white light gray background, absolutely no shadow, grain or gradient.
CORRECTION: make background truly a single flat gray color without paper grain or subtle mottling. Do not move the yellow paint down to the new fracture line: keep original painted stripe positions, simply remove the portions on missing concrete. Maintain exact original lower silhouette, bottom feet and camera. Three thick short rebars and only the right handle.
```

## Env_RoadBarrier.png

재생성 이유: 배경 질감, 점유율 조정. 최종 선택: 2차. 남은 차이: 폭이 약 85%, 미세한 배경 질감.

### 1차 생성

실제 입력 이미지:

- `E:/Git/Potpolio_Repos/unity-aim-combat-slice/ArtSource/Concepts/Environment/Stage_Backdrop_v3.png`

```text
Use case: stylized-concept. Create one square 1:1 raster image for single-image-to-3D reconstruction. Exactly ONE connected solid object, centered, occupying about 80% of the image with clear margins and no cropping. Front three-quarter view from slightly above, showing front, one side and top, weak perspective. Background must be absolutely uniform near-white light gray, no gradient or texture, no floor, NO cast shadow or contact shadow. Match the supplied v3 reference design and art style: dark gunmetal, mid-gray top faces and bevels, bold black outlines, dark crimson stripes, bright red core where applicable, flat two-tone cel shading. All components have substantial solid thickness. No thin rods, wires, trusses, wafer-thin plates, cloth, talismans, flags, text, labels, symbols, arrows, frame, detached fragments, floating debris, smoke, fire, rays, bloom or glow. Bright areas are crisp flat color. Do not reproduce reference sheet labels or additional objects.
Input Stage_Backdrop_v3.png is the design reference, specifically the small trapezoidal concrete barriers along the road edges, NOT the large foreground handled cover blocks. Output Env_RoadBarrier.png: exactly one wide trapezoidal concrete road barrier, broad sturdy base and narrower top, long sloping gray concrete front and thick beveled ends. Yellow and black diagonal hazard stripe band along its upper portion. Keep v3 chunky angular silhouette, gray flat concrete faces, bold black outlines, two-tone cel shading. No handles, talismans, signs, lamps, roadway or environment, no red core. Slight elevated front three-quarter view with right side and top visible. Maximum width 80 percent of square with 10 percent left/right margin. Uniform near-white gray background without grain or gradient, no floor shadow.
```

### 2차 생성

실제 입력 이미지:

- `E:/Git/Potpolio_Repos/unity-aim-combat-slice/ArtSource/Concepts/Environment/Stage_Backdrop_v3.png`

```text
Use case: stylized-concept. Create one square 1:1 raster image for single-image-to-3D reconstruction. Exactly ONE connected solid object, centered, occupying about 80% of the image with clear margins and no cropping. Front three-quarter view from slightly above, showing front, one side and top, weak perspective. Background must be absolutely uniform near-white light gray, no gradient or texture, no floor, NO cast shadow or contact shadow. Match the supplied v3 reference design and art style: dark gunmetal, mid-gray top faces and bevels, bold black outlines, dark crimson stripes, bright red core where applicable, flat two-tone cel shading. All components have substantial solid thickness. No thin rods, wires, trusses, wafer-thin plates, cloth, talismans, flags, text, labels, symbols, arrows, frame, detached fragments, floating debris, smoke, fire, rays, bloom or glow. Bright areas are crisp flat color. Do not reproduce reference sheet labels or additional objects.
Input Stage_Backdrop_v3.png is the design reference, specifically the small trapezoidal concrete barriers along the road edges, NOT the large foreground handled cover blocks. Output Env_RoadBarrier.png: exactly one wide trapezoidal concrete road barrier, broad sturdy base and narrower top, long sloping gray concrete front and thick beveled ends. Yellow and black diagonal hazard stripe band along its upper portion. Keep v3 chunky angular silhouette, gray flat concrete faces, bold black outlines, two-tone cel shading. No handles, talismans, signs, lamps, roadway or environment, no red core. Slight elevated front three-quarter view with right side and top visible. Maximum width 80 percent of square with 10 percent left/right margin. Uniform near-white gray background without grain or gradient, no floor shadow.
CORRECTION: background must be a perfectly uniform single near-white gray fill, no grain, texture or gradient. Keep this simple trapezoidal barrier design with thick concrete geometry, slightly reduce to maximum 80 percent canvas width.
```



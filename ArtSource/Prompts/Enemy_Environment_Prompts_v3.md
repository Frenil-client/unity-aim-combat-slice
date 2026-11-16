# 적과 배경 v3 실제 이미지 생성 프롬프트

생성 도구: 내장 image_gen. 아래는 실제 전달한 프롬프트 전문과 순서대로 전달한 참조 이미지 경로다. 모든 호출은 transparent_background=false를 사용했다. 재생성은 해당 이미지당 1회이며 마지막 결과를 지정 경로에 저장했다. 기존 원본과 v1, v2 이미지는 변경하지 않았다.

## Boss_Concept_v3.png

저장 경로: `ArtSource/Concepts/Enemies/Boss_Concept_v3.png`

### 최초 생성

실제 참조 입력:

1. `ArtSource/Mockups/Battle_Mockup_Original.png`

```text
Use case: stylized-concept. Create a square 1:1 isolated full-body front three-quarter production concept of the flying boss visible in the supplied reference image. Input image 1 is the approved original game art: preserve its boss silhouette and hand-drawn anime cel-shading, chunky black outlines, dark gunmetal overlapping angular wing plates, medium gray top bevels, crimson core and darker red narrow inlay stripes. This is the SAME boss, not a new simplified emblem. No humans, stage, UI, reticle or text. Plain uniform light gray background, no ground shadow, no bloom, generous margins.
Design exactly one permanent compact angular torso with large bright red hexagonal core and five removable modules. A on IMAGE LEFT and B on IMAGE RIGHT are mirrored splayed wings, each a cluster of three long overlapping thick pointed wedge plates and exactly three small front-facing dark red gun apertures; broad angular shoulder joints connect to torso. C is the long downward spear ram under the body. D is a pair of interlocking upper and lower shield plates in front of the core, leaving a clearly visible broad red central hexagonal opening; it is one removable module. E is one thick blunt faceted horn and sensor above the torso. Keep original broad wing silhouette and layered plate articulation. No thin antennae, wires or delicate struts. Every plate has visible thickness at least one fifth of its width. Permanent shoulder socket collars, lower ram collar and horn root stay on the torso when modules are removed. Distinct silhouettes and small separation gaps make A-E readable without labels. Up to four separate floating small angular shards, not connected to body, subordinate to main silhouette. Core is brightest largest red region; stripes and gun apertures are much darker. Keep the core hexagon itself free of crosshairs. All parts intact.
```

### 재생성

사유: 정면에 가까운 시점, 밝은 장갑 줄 수정

실제 참조 입력:

1. `ArtSource/Mockups/Battle_Mockup_Original.png`

```text
Use case: stylized-concept. Create a square 1:1 isolated full-body front three-quarter production concept of the flying boss visible in the supplied reference image. Input image 1 is the approved original game art: preserve its boss silhouette and hand-drawn anime cel-shading, chunky black outlines, dark gunmetal overlapping angular wing plates, medium gray top bevels, crimson core and darker red narrow inlay stripes. This is the SAME boss, not a new simplified emblem. No humans, stage, UI, reticle or text. Plain uniform light gray background, no ground shadow, no bloom, generous margins.
Design exactly one permanent compact angular torso with large bright red hexagonal core and five removable modules. A on IMAGE LEFT and B on IMAGE RIGHT are mirrored splayed wings, each a cluster of three long overlapping thick pointed wedge plates and exactly three small front-facing dark red gun apertures; broad angular shoulder joints connect to torso. C is the long downward spear ram under the body. D is a pair of interlocking upper and lower shield plates in front of the core, leaving a clearly visible broad red central hexagonal opening; it is one removable module. E is one thick blunt faceted horn and sensor above the torso. Keep original broad wing silhouette and layered plate articulation. No thin antennae, wires or delicate struts. Every plate has visible thickness at least one fifth of its width. Permanent shoulder socket collars, lower ram collar and horn root stay on the torso when modules are removed. Distinct silhouettes and small separation gaps make A-E readable without labels. Up to four separate floating small angular shards, not connected to body, subordinate to main silhouette. Core is brightest largest red region; stripes and gun apertures are much darker. Keep the core hexagon itself free of crosshairs. All parts intact.
CORRECTION: The previous attempt was too frontal. Show a clearly oblique front THREE-QUARTER view rotated 25 degrees, with nearer wing visibly larger and core side depth visible. Keep ample margins. ALL stripes, horn sensor and ram inlay must be dark burgundy, considerably darker than the bright core. Preserve the original reference winged layered boss.
```

## Boss_Front_v3.png

저장 경로: `ArtSource/Concepts/Enemies/Boss_Front_v3.png`

### 최초 생성

실제 참조 입력:

1. `ArtSource/Concepts/Enemies/Boss_Concept_v3.png`

```text
Use case: stylized-concept. Square 1:1. Produce a perfectly frontal symmetrical orthographic view of the exact intact boss in input image 1. Change camera only, preserve all module shapes: each wing has three stepped overlapping pointed plates with three end gun apertures, shoulder collars, long downward spear, single thick blunt horn with burgundy sensor, upper chevron and lower pointed core shield. Full body with generous margins. Four floating shards maximum. Style: exactly the supplied boss identity, anime game concept art, crisp chunky black contours and flat cel shading, almost black gunmetal layered thick wedge plates with gray top bevels, dark burgundy inlays, bright hexagonal red core as the strongest red. No bloom. Plain light gray background, no floor shadows, no text unless explicitly requested. A means image-left wing, B image-right wing, C downward spear ram, D upper and lower interlocking core shield plates as one module, E single thick top horn. Permanent torso holds the core and fixed shoulder collars, horn root and ram socket.
```

## Boss_Parts_v3.png

저장 경로: `ArtSource/Concepts/Enemies/Boss_Parts_v3.png`

### 최초 생성

실제 참조 입력:

1. `ArtSource/Concepts/Enemies/Boss_Concept_v3.png`
2. `ArtSource/Concepts/Enemies/Boss_Front_v3.png`

```text
Use case: stylized-concept. Landscape 16:9 exploded assembly diagram of the EXACT same boss in input 1 three-quarter and input 2 front. Front-facing parts, no redesign. Place permanent naked torso with fully exposed large red hexagonal core at center. Detach A three-plate left wing to far left, B mirrored wing to far right, E thick horn above torso, C long spear below torso, D two core shield plates grouped together in lower right separate from wing and torso. All five modules separated by clear empty space; show matching broad socket interfaces and fixed collars on torso. Torso must NOT retain D chevron shield or horn or spear. A and B each retain three gun apertures. Label only A B C D E once next to corresponding module, no other text, no arrows or extraneous floating shards. Enough margin so nothing overlaps. Style: exactly the supplied boss identity, anime game concept art, crisp chunky black contours and flat cel shading, almost black gunmetal layered thick wedge plates with gray top bevels, dark burgundy inlays, bright hexagonal red core as the strongest red. No bloom. Plain light gray background, no floor shadows, no text unless explicitly requested. A means image-left wing, B image-right wing, C downward spear ram, D upper and lower interlocking core shield plates as one module, E single thick top horn. Permanent torso holds the core and fixed shoulder collars, horn root and ram socket.
```

### 재생성

사유: 분리한 D 장갑이 본체에도 중복되어 제거 요청

실제 참조 입력:

1. `ArtSource/Concepts/Enemies/Boss_Concept_v3.png`
2. `ArtSource/Concepts/Enemies/Boss_Front_v3.png`

```text
Use case: stylized-concept. Landscape 16:9 exploded assembly diagram of the EXACT same boss in input 1 three-quarter and input 2 front. Front-facing parts, no redesign. Place permanent naked torso with fully exposed large red hexagonal core at center. Detach A three-plate left wing to far left, B mirrored wing to far right, E thick horn above torso, C long spear below torso, D two core shield plates grouped together in lower right separate from wing and torso. All five modules separated by clear empty space; show matching broad socket interfaces and fixed collars on torso. Torso must NOT retain D chevron shield or horn or spear. A and B each retain three gun apertures. Label only A B C D E once next to corresponding module, no other text, no arrows or extraneous floating shards. Enough margin so nothing overlaps. Style: exactly the supplied boss identity, anime game concept art, crisp chunky black contours and flat cel shading, almost black gunmetal layered thick wedge plates with gray top bevels, dark burgundy inlays, bright hexagonal red core as the strongest red. No bloom. Plain light gray background, no floor shadows, no text unless explicitly requested. A means image-left wing, B image-right wing, C downward spear ram, D upper and lower interlocking core shield plates as one module, E single thick top horn. Permanent torso holds the core and fixed shoulder collars, horn root and ram socket.
CRITICAL CORRECTION: The center torso must have NO silver V-shaped upper shield and NO lower pointed shield. Those exact two removable shields appear ONLY in the detached D group. Center torso is a dark simple octagonal housing around a much larger unobstructed hexagonal red core, with fixed shoulder collars and small horn/ram sockets. Do not duplicate D on the torso. Preserve A B C E and the same boss design.
```

## Boss_Damage_Stages_v3.png

저장 경로: `ArtSource/Concepts/Enemies/Boss_Damage_Stages_v3.png`

### 최초 생성

실제 참조 입력:

1. `ArtSource/Concepts/Enemies/Boss_Front_v3.png`

```text
Use case: stylized-concept. Landscape 16:9 production damage sequence sheet. Input 1 is the definitive FRONT of this boss. Four equal columns left to right, EXACT same front orthographic camera, identical torso size and core center height, fixed shoulder/horn/ram socket positions and sizes in every column. No perspective changes, no floating shards, no labels or text. Panel 1: intact exact input boss with all A B C D E. Panel 2: A left wing damaged with one outer plate peeled away exposing black inner frame and 2 broad wedge fracture grooves, chipped corners; D upper/lower shield chipped with 2 broad dark-red cracks and missing chunk. B C E unchanged. Panel 3: A completely absent leaving ONLY original fixed left shoulder collar with rough dark-red cut face; D both upper and lower shield plates completely gone exposing full broad red hexagonal core in simple dark hexagonal housing. B right wing, C long spear and E horn stay EXACTLY intact. Panel 4: all A B C D E absent, only SAME torso and core, both shoulder socket collars, short broken ram stump, short horn root remain. No resizing torso to fill empty space. Broken faces angular dim burgundy. Core brightest. Style: exactly the supplied boss identity, anime game concept art, crisp chunky black contours and flat cel shading, almost black gunmetal layered thick wedge plates with gray top bevels, dark burgundy inlays, bright hexagonal red core as the strongest red. No bloom. Plain light gray background, no floor shadows, no text unless explicitly requested. A means image-left wing, B image-right wing, C downward spear ram, D upper and lower interlocking core shield plates as one module, E single thick top horn. Permanent torso holds the core and fixed shoulder collars, horn root and ram socket.
```

### 재생성

사유: 파괴 단계에 D 상부 장갑과 E 뿔이 남아 수정 요청

실제 참조 입력:

1. `ArtSource/Concepts/Enemies/Boss_Front_v3.png`

```text
Use case: stylized-concept. Landscape 16:9 production damage sequence sheet. Input 1 is the definitive FRONT of this boss. Four equal columns left to right, EXACT same front orthographic camera, identical torso size and core center height, fixed shoulder/horn/ram socket positions and sizes in every column. No perspective changes, no floating shards, no labels or text. Panel 1: intact exact input boss with all A B C D E. Panel 2: A left wing damaged with one outer plate peeled away exposing black inner frame and 2 broad wedge fracture grooves, chipped corners; D upper/lower shield chipped with 2 broad dark-red cracks and missing chunk. B C E unchanged. Panel 3: A completely absent leaving ONLY original fixed left shoulder collar with rough dark-red cut face; D both upper and lower shield plates completely gone exposing full broad red hexagonal core in simple dark hexagonal housing. B right wing, C long spear and E horn stay EXACTLY intact. Panel 4: all A B C D E absent, only SAME torso and core, both shoulder socket collars, short broken ram stump, short horn root remain. No resizing torso to fill empty space. Broken faces angular dim burgundy. Core brightest. Style: exactly the supplied boss identity, anime game concept art, crisp chunky black contours and flat cel shading, almost black gunmetal layered thick wedge plates with gray top bevels, dark burgundy inlays, bright hexagonal red core as the strongest red. No bloom. Plain light gray background, no floor shadows, no text unless explicitly requested. A means image-left wing, B image-right wing, C downward spear ram, D upper and lower interlocking core shield plates as one module, E single thick top horn. Permanent torso holds the core and fixed shoulder collars, horn root and ram socket.
MANDATORY correction: Panel 3 must remove BOTH silver V-shaped upper and lower D armor, leaving a simple dark hexagonal ring around exposed core. Panel 4 must remove the ENTIRE tall horn E as well as BOTH D plates, leaving ONLY a short small broken root on top of the SAME torso. Do not leave the horn or upper silver chevron in final panel. Never crack the red core in panel 2: cracks belong on gray armor only. Omit ALL floating shards in ALL panels. Preserve fixed socket heights and torso scale. Four columns with ample outer margins.
```

## Boss_Part_States_v3.png

저장 경로: `ArtSource/Concepts/Enemies/Boss_Part_States_v3.png`

### 최초 생성

실제 참조 입력:

1. `ArtSource/Concepts/Enemies/Boss_Parts_v3.png`

```text
Use case: stylized-concept. Landscape 16:9 technical part damage sheet using the exact A wing and C spear in supplied exploded reference. Three aligned columns, two rows plus small bottom debris row. Top row: A intact three-plate wing with three apertures; same A damaged with ONE outer plate missing, black internal support exposed, two broad wedge-shaped red fissures and chipped edges while keeping wing silhouette; A destroyed with ONLY its permanent shoulder socket collar and rough dim burgundy end face. Bottom main row: C intact long spear; C damaged with one outer side casing broken away exposing black internal axis, two broad wedge grooves and chipped silhouette; C destroyed ONLY short stump and original top socket. Every corresponding socket stays exactly the same size, orientation and relative anchor position within each cell, same camera and part scale, do not recenter the destroyed small remainder. Socket sits on RIGHT edge of each A cell, TOP center of each C cell. Generous spacing, no overlap. Separate bottom strip with exactly SIX large angular detached wedge fragments. No text, no arrows, no extra body or wings. Style: exactly the supplied boss identity, anime game concept art, crisp chunky black contours and flat cel shading, almost black gunmetal layered thick wedge plates with gray top bevels, dark burgundy inlays, bright hexagonal red core as the strongest red. No bloom. Plain light gray background, no floor shadows, no text unless explicitly requested. A means image-left wing, B image-right wing, C downward spear ram, D upper and lower interlocking core shield plates as one module, E single thick top horn. Permanent torso holds the core and fixed shoulder collars, horn root and ram socket.
```

## Rusher_Concept_v3.png

저장 경로: `ArtSource/Concepts/Enemies/Rusher_Concept_v3.png`

### 최초 생성

실제 참조 입력:

1. `ArtSource/Concepts/Enemies/Rusher_Concept_v2.png`
2. `ArtSource/Concepts/Enemies/Boss_Concept_v3.png`

```text
Use case: stylized-concept. Square 1:1 isolated rusher enemy full body in front three-quarter view, uniform light gray background, generous margins, no text, no ground shadow or bloom. Input 1 is the rusher v2 STRUCTURE reference: retain its pointed arrowhead wedge central hull, large top-front hexagonal red core, exactly two lateral blade plates and one hovering disc underneath. Input 2 is boss v3 STYLE AND FACTION reference only: transfer almost-black gunmetal, thick overlapping wedge armor layers, middle-gray bevels, thick black anime outlines and flat cel shading, narrow dark burgundy inlays. Keep rusher compact, horizontal and clearly distinct from boss, not a winged humanoid. Add layered armor without losing the v2 arrowhead silhouette. Plate thickness at least one fifth width, sturdy parts animated with rotation and translation only, no limbs or thin cables. Core strongest brightest red, all stripes dark burgundy, no extra floating shards.
```

## Ranged_Concept_v3.png

저장 경로: `ArtSource/Concepts/Enemies/Ranged_Concept_v3.png`

### 최초 생성

실제 참조 입력:

1. `ArtSource/Concepts/Enemies/Ranged_Concept_v2.png`
2. `ArtSource/Concepts/Enemies/Boss_Concept_v3.png`

```text
Use case: stylized-concept. Square 1:1 isolated ranged enemy full-body front three-quarter production concept. Uniform light gray background, generous margins, no text, no ground shadows, no bloom. Input 1 is ranged v2 STRUCTURE: preserve its squat box turret with front red core above one thick long rectangular gun barrel, rotating base and exactly three sturdy tripod legs. Input 2 is boss v3 FACTION AND STYLE: almost black gunmetal layered thick wedge armor, gray top bevels, dark burgundy narrow inlays, crisp heavy black anime outlines and cel shading. Add one small thick blunt faceted horn above turret matching boss E sensor. Front core is brightest red plane, armor stripes and horn sensor are much darker burgundy. Retain compact turret/tripod silhouette and simple rigid-part movement, no humanoid joints, cables, wings or extra weapons. Overlapping wedge armor should enrich surfaces without hiding the front core or tripod.
```

## Stage_Backdrop_v3.png

저장 경로: `ArtSource/Concepts/Environment/Stage_Backdrop_v3.png`

### 최초 생성

실제 참조 입력:

1. `ArtSource/Mockups/Battle_Mockup_Original.png`

```text
Use case: stylized-concept. Create a landscape 16:9 clean environment plate for the EXACT SAME GAME and place as input 1 approved original battle mockup. Preserve original hand-drawn anime cel shading, thick black outlines, charcoal concrete with muted gray-purple tops, black-purple night sky and restrained dark violet angular sky fissures. Remove all people, enemies, bullets, crosshairs and UI. An OUTDOOR four-lane city road directly in front of a transit station, one-point perspective, road markings centerline and broad white zebra crossing. LEFT: elevated concrete structure with station entrance descending stairs under it, railing, white fluorescent strip lights and a sign containing only a bus pictogram and downward arrow. RIGHT: closed roll-up shutter buildings, rooftop ventilation fan unit and exposed pipes. Both sides have red hanging banners with gold geometric occult emblems, yellow-black striped road barricades and bollards. Distant urban silhouettes at horizon. Preserve original low foreground concrete cover blocks with carrying handles, worn yellow slashes and hanging paper talismans. Keep center combat corridor open: central horizontal region from 20 to 60 percent image height contains open sky and road, distant skyline only at horizon, no obstruction or overhead roof across street. Sky and fissures low saturation, darker and duller than character purple accents in original. Match original camera height, vanishing point and architectural arrangement, no indoor hall, no blue-gray monochrome redesign. No lettering.
```

### 재생성

사유: 역 입구 계단이 올라가는 형태여서 하행 계단으로 수정 요청

실제 참조 입력:

1. `ArtSource/Mockups/Battle_Mockup_Original.png`

```text
Use case: stylized-concept. Create a landscape 16:9 clean environment plate for the EXACT SAME GAME and place as input 1 approved original battle mockup. Preserve original hand-drawn anime cel shading, thick black outlines, charcoal concrete with muted gray-purple tops, black-purple night sky and restrained dark violet angular sky fissures. Remove all people, enemies, bullets, crosshairs and UI. An OUTDOOR four-lane city road directly in front of a transit station, one-point perspective, road markings centerline and broad white zebra crossing. LEFT: elevated concrete structure with station entrance descending stairs under it, railing, white fluorescent strip lights and a sign containing only a bus pictogram and downward arrow. RIGHT: closed roll-up shutter buildings, rooftop ventilation fan unit and exposed pipes. Both sides have red hanging banners with gold geometric occult emblems, yellow-black striped road barricades and bollards. Distant urban silhouettes at horizon. Preserve original low foreground concrete cover blocks with carrying handles, worn yellow slashes and hanging paper talismans. Keep center combat corridor open: central horizontal region from 20 to 60 percent image height contains open sky and road, distant skyline only at horizon, no obstruction or overhead roof across street. Sky and fissures low saturation, darker and duller than character purple accents in original. Match original camera height, vanishing point and architectural arrangement, no indoor hall, no blue-gray monochrome redesign. No lettering.
Correction: station steps must clearly DESCEND BELOW STREET LEVEL into a sunken entrance, not climb upward to an elevated doorway. Show the sidewalk-level top landing, a recessed stairwell opening and handrails sloping DOWN into darkness behind the low parapet at image left. Preserve original overall architecture and composition. Reduce saturation of purple sky fissures.
```

## Battle_Mockup_Boss_v3.png

저장 경로: `ArtSource/Mockups/Battle_Mockup_Boss_v3.png`

### 최초 생성

실제 참조 입력:

1. `ArtSource/Mockups/Battle_Mockup_Original.png`
2. `ArtSource/Concepts/Enemies/Boss_Damage_Stages_v3.png`
3. `ArtSource/Concepts/Environment/Stage_Backdrop_v3.png`

```text
Use case: compositing. Landscape 16:9 gameplay mockup. Input 1 approved ORIGINAL is the primary authority for anime drawing style, camera, composition, EXACT three allies and their size/pose/placement, low concrete covers, purple charcoal palette and HUD visual language. Input 2 boss damage sheet is the boss design authority: use ONLY THIRD COLUMN state. Input 3 is the exact outdoor transit-station road background. Make this unmistakably another screenshot of the SAME game as input 1, not a different render style. Keep left purple-accent dark ponytail sniper, center silver short-haired teal-accent rifle shooter firing, right auburn short-haired orange-accent shotgunner, same foreground scale and cover positions as original. Stage is open night road, descending station stairwell left, shutters and fan right, red gold-pattern banners and paper talismans. Boss hovers above road in original location, SAME dark gunmetal three-plate wings design from input 2: image-LEFT wing A destroyed, shoulder socket exposed; BOTH upper and lower D core shield plates removed, broad bright red hexagonal core exposed. Right wing B with three wedge plates and gun apertures remains intact, downward spear C and thick horn E intact. No silver chevron over core. Show exactly 4-6 large separated angular fragments falling outward from missing LEFT wing, clearly detached with empty air between pieces and socket. Permanent torso/socket scale maintained. Dim burgundy fracture face, core strongest red, no bloom veil. White aiming reticle located OVER RIGHT WING, never core. Top original-style red boss HP bar, directly below exactly five small status boxes ordered A B C D E: A and D dark, B C E bright. Bottom HUD only three ally portraits left with green bars, right ammo '24/30' and one horizontal burst gauge. NO medkit, grenade, skill cards or other icons. No added titles or captions.
```

## Battle_Mockup_Wave_v3.png

저장 경로: `ArtSource/Mockups/Battle_Mockup_Wave_v3.png`

### 최초 생성

실제 참조 입력:

1. `ArtSource/Mockups/Battle_Mockup_Original.png`
2. `ArtSource/Concepts/Enemies/Rusher_Concept_v3.png`
3. `ArtSource/Concepts/Enemies/Ranged_Concept_v3.png`
4. `ArtSource/Concepts/Environment/Stage_Backdrop_v3.png`

```text
Use case: compositing. Landscape 16:9 wave combat gameplay mockup for the EXACT same game as input 1 approved original. Input 1 controls hand-drawn anime cel-shaded style, heavy black outlines, purple charcoal palette, camera and EXACT three allied characters including pose, size and placement. Input 2 is exact rusher design. Input 3 is exact ranged turret design. Input 4 is the exact outdoor transit station road environment. Keep left dark purple ponytail sniper with purple accents, center silver bob teal-accent rifle shooter firing, right auburn bob orange-accent shotgunner, original foreground cover block layout with handles, worn yellow slashes and paper talismans. Use stage from input 4: open four-lane night road, zebra crossing, descending station entrance under elevated structure on left, closed shutters fan and pipes on right, red gold-pattern banners, muted violet sky fissures and distant city. NO BOSS. Exactly FIVE rusher hover enemies from input 2 advancing down road, spread across distinct depth lanes without overlap: two nearer, two middle, one distant central. Each has pointed arrowhead wedge body, bright top/front hex core, two side blades and underside hovering disc. Exactly TWO ranged enemies from input 3 at far left and far right behind rushers, each a dark layered box turret with long barrel, three tripod legs and short thick horn. Seven enemies total, no extra enemies or decorative flying shards. Enemy dark gunmetal and dark burgundy inlays with brightest red core. Ensure all seven visible above foreground cover and not hidden by allies. Bottom HUD only three original-style portraits with green bars at left, ammo '24/30' and one horizontal burst gauge at right; no medkit, grenade or skill icons. No boss health bar or boss part boxes. Small white reticle over one rusher, not stuck to its core. Original and output must look like adjacent screenshots of same game.
```

### 재생성

사유: 아군 크기가 원본보다 작고 아래로 이동해 원본 배치로 수정 요청

실제 참조 입력:

1. `ArtSource/Mockups/Battle_Mockup_Original.png`
2. `ArtSource/Concepts/Enemies/Rusher_Concept_v3.png`
3. `ArtSource/Concepts/Enemies/Ranged_Concept_v3.png`
4. `ArtSource/Concepts/Environment/Stage_Backdrop_v3.png`

```text
Use case: compositing. Landscape 16:9 wave combat gameplay mockup for the EXACT same game as input 1 approved original. Input 1 controls hand-drawn anime cel-shaded style, heavy black outlines, purple charcoal palette, camera and EXACT three allied characters including pose, size and placement. Input 2 is exact rusher design. Input 3 is exact ranged turret design. Input 4 is the exact outdoor transit station road environment. Keep left dark purple ponytail sniper with purple accents, center silver bob teal-accent rifle shooter firing, right auburn bob orange-accent shotgunner, original foreground cover block layout with handles, worn yellow slashes and paper talismans. Use stage from input 4: open four-lane night road, zebra crossing, descending station entrance under elevated structure on left, closed shutters fan and pipes on right, red gold-pattern banners, muted violet sky fissures and distant city. NO BOSS. Exactly FIVE rusher hover enemies from input 2 advancing down road, spread across distinct depth lanes without overlap: two nearer, two middle, one distant central. Each has pointed arrowhead wedge body, bright top/front hex core, two side blades and underside hovering disc. Exactly TWO ranged enemies from input 3 at far left and far right behind rushers, each a dark layered box turret with long barrel, three tripod legs and short thick horn. Seven enemies total, no extra enemies or decorative flying shards. Enemy dark gunmetal and dark burgundy inlays with brightest red core. Ensure all seven visible above foreground cover and not hidden by allies. Bottom HUD only three original-style portraits with green bars at left, ammo '24/30' and one horizontal burst gauge at right; no medkit, grenade or skill icons. No boss health bar or boss part boxes. Small white reticle over one rusher, not stuck to its core. Original and output must look like adjacent screenshots of same game.
Critical correction: preserve original foreground ally pixel-scale and vertical positions. Center silver-haired character HEAD TOP at 44 percent canvas height, left ponytail silhouette top near 49 percent, right auburn head top near 52 percent, exactly as input 1. Do not shrink or lower them to fit enemies. Original center character occupies full lower 56 percent of image and is large. Cover tops at 66 percent. Place all five rushers and two farther turrets in the visible road band ABOVE these covers, avoiding ally silhouettes. Turrets should be distinctly farther back and smaller than nearest rushers. Keep EXACT count five rushers and two turrets. White reticle over outer hull of a rusher, not in empty sky.
```

## 직접 열어 확인한 결과와 남은 차이

- 1, 2번: 원본 계열의 날개형 건메탈 보스, 코어와 암적색 줄, A-E 위치를 확인했다. 1번은 시점과 줄 밝기를 재생성으로 수정했다.
- 3번: A-E 분리와 결합부를 확인했다. D가 본체에도 중복된 첫 결과를 재생성해 중복을 제거했다.
- 4번: 파괴 단계의 D와 E 잔존 문제를 재생성으로 개선했다. 그러나 첫 칸 A에 손상이 남고 두 번째 칸 C에도 균열이 추가되어 요청한 단계와 완전히 일치하지 않는다. 소켓 중심은 대체로 유지되나 크기와 세부 외형은 미세하게 달라 정확한 메시 교체 설계도로 사용할 수 없다.
- 5번: A와 C의 온전, 손상, 파괴 및 하단 파편 6개를 확인했다. 연결부 방향과 높이는 유지되지만 생성 도해이므로 픽셀 단위 정렬이나 동일한 3D 결합면을 보증하지 않는다.
- 6, 7번: v2의 화살촉과 부유 원반, 포탑과 삼각대 구성을 각각 유지하고 v3 보스의 색과 판 겹침을 적용했다.
- 8번: 야외 환승역 앞 도로와 원본 계열 건물을 확인했다. 계단을 재생성하여 지하로 내려가는 입구로 수정했다.
- 9번: 4번 세 번째 상태를 참조한 보스, A와 D 파괴, 파편 5개, 오른쪽 날개 조준점, 상태 칸 5개, 원본 아군 배치와 제한된 HUD를 확인했다.
- 10번: 아군 크기를 재생성으로 수정했다. 돌격형 5기와 원거리형 2기, 원본 계열 아군 배치와 야외 도로를 확인했다. 배경 원경과 버스트 게이지의 세부 표현은 9번과 완전히 같지 않다.
- 정사각형은 1254x1254, 가로 이미지는 1672x941로 생성되었다. 가로 비율은 16:9에 근접하지만 정수 픽셀 기준 정확한 16:9는 아니다. 별도 리사이즈나 합성은 하지 않았다.


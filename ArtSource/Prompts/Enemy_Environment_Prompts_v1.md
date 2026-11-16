# 이미지 생성 프롬프트 기록 v1

내장 image_gen 도구 사용. 모든 호출은 transparent_background=false로 실행했다. 참조는 아래 순서대로 전달했다. 목업의 반복 항목은 같은 파일을 개선한 순서이며 해당 단계 직전 이미지를 참조했다. 참조 최대 5개 제한으로 실행되지 않은 6개 입력 요청은 제외하고 실제 이미지 생성에 사용한 모든 프롬프트 전문을 기록했다.

## ArtSource/Concepts/Enemies/Enemy_Lineup_v1.png

참조 입력:
없음

```text
Use case: stylized-concept. Create a 16:9 enemy scale lineup on a uniform light gray background, no cast shadows, no captions or dimensions. Original mechanical enemy design, stylized low-poly 3D concept with black outlines, flat matte neutral gray material blocks and exactly two tone cel shading. No humanoid anatomy, legs, animals, wires, antennae, thin plates, metallic reflections, wear, smoke, bloom, logos or text. Red is reserved exclusively for a crisp flat weak-point core; no teal, purple or orange on enemies. Strong simple separate rigid parts with thick steps and readable gaps. Show exactly three machines and one small plain gray human scale silhouette, all aligned to a shared implied ground baseline, weak perspective, front three-quarter views, generous margins. From left: human silhouette height H; rusher body height 0.55H, short forward-pointing wedge triangular in plan, two folded thick lateral claws with visible rotary joints and gaps, one broad underbody hover block, red core at front center; ranged machine total height H, broad low triangular base, raised separate rotating turret and one long horizontal thick cannon making a T silhouette, red core wrapping the turret's front-side corner so visible from front; boss height 4.5H, broad inverted-triangle silhouette, central torso and two detached hovering shield shoulders with clear air gaps, each shoulder has exactly three large black circular launch ports in a vertical row, large exposed red diamond core centered on torso, one substantial gray downward triangular ram under torso. Boss has four main masses, no arms or legs. Keep all machines fully visible, boss largest and dominating right half. This lineup establishes the exact design to reproduce in later single-object images.
```

## ArtSource/Concepts/Enemies/Rusher_Concept_v1.png

참조 입력:
1. `ArtSource/Concepts/Enemies/Enemy_Lineup_v1.png`

```text
Use case: stylized-concept. Input image 1 is the established enemy design reference. Extract ONLY the small rusher adjacent to the human, preserve its exact armor shape, paired claws, red front core and hover block. Produce a square 1:1 single-object 3D reconstruction reference, front three-quarter camera with clearly visible top and side, uniform light gray background, no floor shadow, text, scale human, other machines or diagrams. Whole object centered with 12% margins. Original mechanical enemy design, stylized low-poly 3D concept with black outlines, flat matte neutral gray material blocks and exactly two tone cel shading. No humanoid anatomy, legs, animals, wires, antennae, thin plates, metallic reflections, wear, smoke, bloom, logos or text. Red is reserved exclusively for a crisp flat weak-point core; no teal, purple or orange on enemies. Strong simple separate rigid parts with thick steps and readable gaps. Keep the short forward wedge, exactly two chunky folded side claws on visible thick rotary joints, one broad underside hover block, four main rigid masses. Make the body triangular in plan and retain the same gray panel proportions and crisp small red front core. No extra decorations.
```

## ArtSource/Concepts/Enemies/Ranged_Concept_v1.png

참조 입력:
1. `ArtSource/Concepts/Enemies/Enemy_Lineup_v1.png`
2. `ArtSource/Concepts/Enemies/Rusher_Concept_v1.png`

```text
Use case: stylized-concept. Input 1 establishes exact ranged enemy geometry: reproduce only the cannon machine between rusher and boss. Input 2 is material and outline reference only, do not add claws. Square 1:1 single-object reconstruction concept. Original mechanical enemy design, stylized low-poly 3D concept with black outlines, flat matte neutral gray material blocks and exactly two tone cel shading. No humanoid anatomy, legs, animals, wires, antennae, thin plates, metallic reflections, wear, smoke, bloom, logos or text. Red is reserved exclusively for a crisp flat weak-point core; no teal, purple or orange on enemies. Strong simple separate rigid parts with thick steps and readable gaps. Broad low triangular plinth base, thick circular turret rotation collar, compact angular turret and exactly one long thick horizontal cannon, T-shaped side silhouette. Preserve lineup geometry. Red core on turret front-side corner wraps slightly onto the front so it remains readable from the front, clear of barrel. Three primary assemblies: base, turret, barrel. Front three-quarter view showing top and side, entire cannon and base within 12% margins. Uniform light gray background, absolutely no ground shadow, human, labels, arrows, other objects or environment.
```

## ArtSource/Concepts/Enemies/Boss_Concept_v1.png

참조 입력:
1. `ArtSource/Concepts/Enemies/Enemy_Lineup_v1.png`
2. `ArtSource/Concepts/Enemies/Ranged_Concept_v1.png`

```text
Use case: stylized-concept. Input 1 is exact boss design reference, use only its largest right-hand machine; input 2 is shared materials reference only. Produce a square 1:1 isolated boss front three-quarter view for image-to-3D reconstruction. Original mechanical enemy design, stylized low-poly 3D concept with black outlines, flat matte neutral gray material blocks and exactly two tone cel shading. No humanoid anatomy, legs, animals, wires, antennae, thin plates, metallic reflections, wear, smoke, bloom, logos or text. Red is reserved exclusively for a crisp flat weak-point core; no teal, purple or orange on enemies. Strong simple separate rigid parts with thick steps and readable gaps. Preserve boss central faceted torso with broad upper cap, exposed large red diamond on front center, two detached thick shield shoulder pods with visible air gaps, exactly three black circular launcher holes arranged vertically on EACH shoulder, and single massive downward triangular gray ram separated from torso by a thick short connector. Exactly same armor panel arrangement, proportions, core shape and port count as lineup. Broad inverted triangular overall silhouette. Show moderate side depth and top planes while keeping all six launch holes and red core readable; no occlusion of core. Four large assemblies, no head, hands or limbs. Entire machine within 10% margin, light gray uniform backdrop, no floor shadow, other machines, human, writing or measurement guides.
```

## ArtSource/Concepts/Enemies/Boss_Front_v1.png

참조 입력:
1. `ArtSource/Concepts/Enemies/Boss_Concept_v1.png`

```text
Use case: stylized-concept. Input image is the exact boss identity reference. Change ONLY camera to a perfectly straight-on orthographic FRONT view, centered bilateral symmetry, square 1:1 canvas. Preserve every part, silhouette proportion, armor panel, large red diamond central core, two separated shield pods, exactly three black circular holes vertically per pod, wide top cap and lower triangular ram with short connector. Show distinct empty light-gray gaps between each shoulder and torso. Original mechanical enemy design, stylized low-poly 3D concept with black outlines, flat matte neutral gray material blocks and exactly two tone cel shading. No humanoid anatomy, legs, animals, wires, antennae, thin plates, metallic reflections, wear, smoke, bloom, logos or text. Red is reserved exclusively for a crisp flat weak-point core; no teal, purple or orange on enemies. Strong simple separate rigid parts with thick steps and readable gaps. No redesign, no additional details or text. All parts fully visible with 10% margin; uniform light gray background without floor, shadows, guides or labels. Core, six launch holes and ram must remain separate and unobstructed.
```

## ArtSource/Concepts/Environment/Cover_Concept_v1.png

참조 입력:
1. `ArtSource/Concepts/Enemies/Boss_Front_v1.png`

```text
Use case: stylized-concept. Input image is rendering-style reference ONLY: use the same clean black outlines, flat gray blocks and two-tone cel shading, do not depict the robot or any red core. Create a 16:9 landscape comparison concept of exactly two waist-high concrete checkpoint barriers, intact on left and damaged on right. Same front three-quarter camera, identical dimensions and design, same baseline and scale, ample separation and outer margins. Each is a long horizontal thick concrete barrier with broad flat top, chunky trapezoidal foot, neutral gray matte surfaces. Exactly one narrow muted yellow and black diagonal hazard band horizontally across its front, aligned identically on both states. Right barrier has a single large chipped section on the upper edge and exactly two very short thick exposed rebar stubs, simple solid fracture planes; keep remaining silhouette intact. Uniform light gray background, no floor shadows, labels, text, dimensions, debris, fine cracks, rust, photoreal texture, bloom or thin fragile details. These are two state references for later separate modeling.
```

## ArtSource/Concepts/Environment/Stage_Backdrop_v1.png

참조 입력:
1. `ArtSource/Concepts/Environment/Cover_Concept_v1.png`

```text
Use case: stylized-concept. Input image is concrete material and cel-shaded outline style reference only, not objects to insert. Create a 16:9 wide fixed-camera shooting-game STAGE BACKDROP ONLY: large underground transfer station checkpoint hall at night, camera slightly elevated behind where a future squad will stand, looking forward; vanishing point at x50%, y42%. Rows of massive concrete columns on left and right recede regularly into depth. Far wall has a closed metal shutter and framed subway entrance, wall vents, small signs using only geometric shapes and arrows, absolutely no lettering. Above far doorway is a modest jagged spatial rift in very dark desaturated black-purple with only a thin dim lavender edge, no bloom, far less saturated than bright ally purple. A few ceiling fluorescent strips, rest of room subdued cool gray-blue, flat two-tone cel-shaded low-poly 3D surfaces with black outlines. Preserve readable mid-value floor and wall in central combat region spanning y35%-68%; no clutter, busy patterns or props there. Bottom 30% is vacant foreground floor reserved for later allies and barriers. Side areas darker, background low saturation. No characters, robots, enemies, barriers, UI, crosshair, weapons, projectiles, fog, glossy reflections, title, watermark or text. Large restrained planes rather than dense tile grid.
```

## ArtSource/Mockups/Battle_Mockup_Boss_v1.png (초안)

참조 입력:
1. `ArtSource/Concepts/Environment/Stage_Backdrop_v1.png`
2. `ArtSource/Concepts/Enemies/Boss_Concept_v1.png`
3. `ArtSource/Characters/Rapid/Rapid_Poses.png`
4. `ArtSource/Characters/Charge/Charge_Cover_Source.png`
5. `ArtSource/Characters/Shotgun/Shotgun_Cover_Source.png`

```text
Use case: compositing. Create a polished 16:9 in-game boss battle mockup using five input images with strict roles. Image 1 is the exact stage background and camera: preserve hall layout, columns, closed shutter, lights, muted dark-purple rift, quiet floor and perspective. Image 2 is exact boss geometry/material reference: preserve gray faceted torso, central large red diamond weak point, detached shoulder shields, THREE black circular ports per shoulder and single triangular downward ram. Images 3,4,5 are ALLY character identity and 2D art references ONLY for three bottom allies and tiny portraits; NEVER transfer their colors, anatomy or anime style to enemies/environment. Image 3: white short-haired teal-accent rifle ally, use BACK pose in middle of sheet. Image 4: dark long ponytail purple-accent rifle ally, rotate to back view preserving ponytail, ribbon and coat. Image 5: short red-haired orange-accent shotgun ally, rotate to back view preserving hair, shell pouch and shotgun. Exactly three allies, left teal, middle purple, right orange, all crouched behind their own three separate gray concrete barriers, facing forward away from camera toward boss. Each barrier is waist-high, long horizontal, flat top, thick slab, two broad trapezoidal feet, single narrow yellow-black diagonal hazard band across front, matching the stage concrete style. Thick black outlines, flat 2D fills and one shadow tone for allies; cel-shaded low-poly 3D look for boss/environment. Composition: allies and barriers entirely within bottom 30% (heads no higher than y70%), three compact portraits and ammo panels within y92%-99%. Boss floats in mid-distance centered x50%, spanning y20%-66%, fully visible with ram above allies and barriers. Boss physically 4.5 times human height with perspective scaling; clear empty air between shoulders and torso; core unobstructed. Keep foreground compact to preserve gameplay space. UI: thin top boss health bar centered x25%-75% at y5%, red fill and dark empty section, no name; small white aiming reticle at screen center, thin enough not to hide core; bottom portraits with white ammo numbers exactly '30/120', '6/24', '8/32' respectively. No title, character or artist names, other words, extra enemies, projectiles, bloom, smoke or dramatic effects. Enemy red restricted to core, UI health bar may be red. Rift dim and desaturated, ally purple bright. Preserve established asset designs.
```

## ArtSource/Mockups/Battle_Mockup_Boss_v1.png (구도 보정)

참조 입력:
1. `ArtSource/Mockups/Battle_Mockup_Boss_v1.png`
2. `ArtSource/Concepts/Environment/Cover_Concept_v1.png`

```text
Use case: precise-object-edit. Image 1 is the battle mockup edit target; image 2 is exact cover design reference. Keep 16:9, same three ally identities, rear poses, colors, same boss design, six ports, gray armor, red diamond, station, top health bar and three bottom portrait/ammo panels. Correct composition only: shrink all three allies and their barriers so their highest hair/weapon points begin at y71% and feet end y91%; keep bottom portraits at y92%-99%. Exactly three allies behind exactly three intact barriers, matching image 2 left barrier shape, feet and single hazard stripe. Preserve clean empty midground between boss ram and ally heads. Move the white aiming reticle to exact image center x50%, y50%, rather than on the elevated core. Keep entire boss visible above y67%, no ally overlap. Restore a small dark desaturated purple jagged spatial rift on the far wall visibly just to the upper right of boss, subtle thin edge, not bright glow. Do not change any enemy geometry or add characters, lettering, effects, decoration or panels. Maintain established flat 2D allies against cel-shaded 3D environment.
```

## ArtSource/Mockups/Battle_Mockup_Boss_v1.png (최종 조준점 보정)

참조 입력:
1. `ArtSource/Mockups/Battle_Mockup_Boss_v1.png`

```text
Use case: precise-object-edit. Edit this exact image with a single UI correction, preserve everything else pixel-composition-wise. REMOVE BOTH existing white crosshair marks completely: the one drawn on the red diamond and the one on floor below the boss. Restore the red diamond to plain solid red and restore floor under its mark. Draw exactly ONE small thin white four-tick crosshair at the precise geometric CENTER of the entire canvas, x=50% of image width, y=50% of image height. This point overlaps the upper-middle part of the gray triangular ram, NOT the red core and NOT the floor. Do not move or resize any subject or alter background, allies, barriers, HUD portraits, ammo, boss, colors, rift, health bar or framing. Only one reticle must remain.
```

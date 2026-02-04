# Assessment 6.1: Pixel Art Animation Specification

## Overview

Complete animation specification for the player character and two enemy types. All animations target 60fps playback on iOS devices. Sprites are 32x32 pixels with a strict 16-color-per-sprite palette drawn from the master 256-color palette. No anti-aliasing, no runtime rotation or scaling -- all animation is frame-by-frame. The player character features weapon mount points for auto-fire attachments and visually evolves across eras.

---

## Technical Constraints

| Parameter | Value |
|-----------|-------|
| Sprite Size | 32x32 pixels |
| Colors Per Sprite | 16 maximum (including transparent) |
| Master Palette | 256 colors total |
| Anti-Aliasing | Prohibited (hard pixel edges only) |
| Runtime Rotation | Prohibited |
| Runtime Scaling | Prohibited |
| Sub-pixel Animation | Color-shift technique only |
| Target Framerate | 60fps |
| Sprite Sheet Format | PNG, power-of-two dimensions |
| Outline Style | 1px dark outline (#1A1A2E), consistent on all characters |
| Facing Direction | All sprites drawn facing right; engine flips horizontally for left |

### Palette Assignment (Player Character -- 16 Colors)

| Slot | Purpose | Hex Code | Description |
|------|---------|----------|-------------|
| 0 | Transparent | N/A | Background key |
| 1 | Outline | #1A1A2E | Near-black blue |
| 2 | Skin Base | #E8B796 | Warm peach |
| 3 | Skin Shadow | #C48B6E | Darker peach |
| 4 | Skin Highlight | #F5D5B8 | Light peach |
| 5 | Hair Base | #5C3A1E | Warm brown |
| 6 | Hair Highlight | #8B6B4A | Light brown |
| 7 | Armor Base | #455A64 | Steel blue-gray |
| 8 | Armor Shadow | #263238 | Dark steel |
| 9 | Armor Highlight | #607D8B | Light steel |
| 10 | Boots/Belt | #4E342E | Dark leather |
| 11 | Boots Highlight | #6D4C41 | Medium leather |
| 12 | Weapon Mount | #B0BEC5 | Cool gray (mount bracket) |
| 13 | Weapon Highlight | #ECEFF1 | Near-white (mount shine) |
| 14 | Weapon Barrel | #78909C | Medium gray (default weapon) |
| 15 | Muzzle Flash/FX | #FFFFFF | Pure white (used sparingly for flash) |

### Era-Based Palette Swaps

The player character visually evolves across eras by swapping palette slots 7-9 (armor colors) and 12-14 (weapon mount colors). The base character silhouette remains the same, but color palettes shift to match each era's aesthetic.

| Era | Armor Base (7) | Armor Shadow (8) | Armor Highlight (9) | Visual Theme |
|-----|---------------|------------------|---------------------|-------------|
| Stone Age | #795548 | #4E342E | #8D6E63 | Leather/bone armor |
| Bronze Age | #8D6E63 | #5D4037 | #A1887F | Bronze-tinted plates |
| Iron Age | #546E7A | #37474F | #78909C | Dark iron |
| Medieval | #455A64 | #263238 | #607D8B | Steel plate (default) |
| Renaissance | #6D4C41 | #4E342E | #8D6E63 | Ornate leather + gold |
| Industrial | #616161 | #424242 | #9E9E9E | Blackened steel |
| Modern | #37474F | #263238 | #546E7A | Tactical dark |
| Digital | #1A237E | #0D1B3E | #283593 | Neon-accented blue |
| Space Age | #E0E0E0 | #BDBDBD | #F5F5F5 | White space suit |
| Transcendent | Cycles 3 colors | Prismatic shift | Animated glow | Rainbow shift effect |

---

## Weapon Mount System

### Mount Point Specification

Weapons attach to the player character at a defined mount point. The mount point position shifts slightly per animation frame to maintain visual coherence.

| Property | Value |
|----------|-------|
| Default mount position | Right shoulder (x: +6, y: -10 relative to sprite center at 16,16) |
| Mount bracket size | 4x3 pixels (drawn in palette slots 12-13) |
| Weapon sprite size | 8x6 pixels max (extends beyond mount bracket) |
| Auto-fire direction | Toward current target (weapon barrel rotates in 8 cardinal directions using pre-drawn frames) |
| Weapon barrel directions | 8 pre-drawn orientations: Right, Right-Up, Up, Left-Up, Left, Left-Down, Down, Right-Down |

### Weapon Visual Specs

Each weapon attachment has a distinct visual profile mounted on the character:

| Weapon | Mount Visual | Barrel Size | Muzzle Flash | Projectile Sprite |
|--------|-------------|-------------|-------------|-------------------|
| Basic Blaster | Small box (4x3px) on shoulder | 6x2px barrel | 2x2px white flash, 2 frames | 3x2px yellow dot |
| Scatter Shot | Wide box (5x3px) on shoulder | 8x3px wide barrel | 4x3px cone flash, 2 frames | 3x 2x2px yellow dots in fan |
| Piercing Beam | Tall rod (3x5px) on shoulder | 2x8px long barrel | 2x2px blue flash, sustained | 1px wide beam line (blue) |
| Homing Missile | Pod (5x4px) on shoulder | 4x3px tube launcher | 3x3px smoke puff, 3 frames | 4x3px missile with 2px smoke trail |

### Weapon Recoil Animation

When auto-fire activates, the mounted weapon plays a recoil micro-animation:

| Property | Value |
|----------|-------|
| Frame Count | 3 |
| Frame Duration | 33ms (2 game frames each) |
| Total Duration | 99ms |
| Looping | Per shot (resets between shots) |

**Frame Descriptions:**

| Frame | Duration | Description |
|-------|----------|-------------|
| recoil_00 | 33ms | Weapon at rest position. Barrel pointing at target. Mount bracket static. |
| recoil_01 | 33ms | Weapon kicks back 1px toward player body. Muzzle flash appears at barrel tip (2-4px white sprite). Projectile spawns. |
| recoil_02 | 33ms | Weapon returns to rest position. Muzzle flash fades. Tiny smoke wisp (1px) at barrel tip. |

---

## Player Character Animations

### IDLE

| Property | Value |
|----------|-------|
| Frame Count | 4 |
| Frame Duration | 167ms (10 game frames at 60fps) |
| Total Loop Duration | 668ms |
| Looping | Yes, seamless |

**Frame Descriptions:**

| Frame | Duration | Pose Description |
|-------|----------|-----------------|
| idle_00 | 167ms | Standing upright, feet shoulder-width apart, arms relaxed. Weapon mounted on right shoulder, barrel pointing right (default). Neutral expression. Weight evenly distributed. |
| idle_01 | 167ms | Slight chest rise (1px upward shift on torso). Shoulders lift subtly. Weapon mount shifts 1px up with shoulder. Hair remains static. Simulates breathing in. |
| idle_02 | 167ms | Return to baseline position. Identical to frame 0 or near-identical with 1px variation in hair. Weapon returns to default position. |
| idle_03 | 167ms | Slight chest drop (1px downward on torso). Subtle weight shift -- one shoulder 1px lower. Weapon mount shifts 1px down. Breathing out. |

**Hitbox/Hurtbox Data (relative to sprite center at 16,16):**

| Frame | Hurtbox (x, y, w, h) | Hitbox |
|-------|----------------------|--------|
| idle_00 | (-6, -14, 12, 28) | None |
| idle_01 | (-6, -15, 12, 28) | None |
| idle_02 | (-6, -14, 12, 28) | None |
| idle_03 | (-6, -13, 12, 28) | None |

---

### WALK

| Property | Value |
|----------|-------|
| Frame Count | 8 |
| Frame Duration | 100ms (6 game frames at 60fps) |
| Total Loop Duration | 800ms |
| Looping | Yes, seamless |

**Frame Descriptions:**

| Frame | Duration | Pose Description |
|-------|----------|-----------------|
| walk_00 | 100ms | Contact pose: Right foot forward touching ground, left foot behind. Arms in opposition (left arm forward, right arm back). Weapon mount bobs 1px down with stride. Torso upright. |
| walk_01 | 100ms | Right foot flat on ground, body shifts forward. Left leg lifts off ground. Weight transfers to right leg. Weapon mount stable. Torso tilts forward 1px. |
| walk_02 | 100ms | Passing pose: Left leg swings forward past right leg. Body at highest vertical point (1px up). Both arms near center. Weapon mount at highest point. |
| walk_03 | 100ms | Left foot reaches forward. Right leg pushes off. Arms continue opposition swing. Weapon mount descends with torso. |
| walk_04 | 100ms | Contact pose (mirror): Left foot forward touching ground, right foot behind. Weapon mount bobs 1px down. |
| walk_05 | 100ms | Left foot flat, body shifts forward. Right leg lifts. Weight on left leg. |
| walk_06 | 100ms | Passing pose (mirror): Right leg swings forward past left. Body at highest point (1px up). |
| walk_07 | 100ms | Right foot reaches forward, left leg pushes off. Arms in full opposition. Returns to walk_00 on loop. |

**Hitbox/Hurtbox Data:**

| Frame | Hurtbox (x, y, w, h) | Hitbox |
|-------|----------------------|--------|
| walk_00 | (-6, -14, 12, 28) | None |
| walk_01 | (-5, -14, 12, 28) | None |
| walk_02 | (-6, -15, 12, 28) | None |
| walk_03 | (-5, -14, 12, 28) | None |
| walk_04 | (-6, -14, 12, 28) | None |
| walk_05 | (-7, -14, 12, 28) | None |
| walk_06 | (-6, -15, 12, 28) | None |
| walk_07 | (-7, -14, 12, 28) | None |

---

### RUN

| Property | Value |
|----------|-------|
| Frame Count | 6 |
| Frame Duration | 83ms (5 game frames at 60fps) |
| Total Loop Duration | 498ms |
| Looping | Yes, seamless |

**Frame Descriptions:**

| Frame | Duration | Pose Description |
|-------|----------|-----------------|
| run_00 | 83ms | Aggressive forward lean (torso tilted ~15deg). Right foot strikes ground ahead. Left arm forward, right arm trails behind. Hair and any loose clothing trail back. Weapon mount angled with torso lean. |
| run_01 | 83ms | Drive phase: Right leg pushes body forward. Left knee drives up high (knee at waist height). Torso maintains lean. Weapon barrel bounces 1px with motion. |
| run_02 | 83ms | Flight phase: Both feet off ground. Body at highest point (2px up from baseline). Arms in mid-swing. Dynamic pose with full extension. Weapon mount at peak height. |
| run_03 | 83ms | Left foot strikes ground. Torso still leaning. Impact frame -- slight compression (1px). Weapon mount rebounds down 1px. |
| run_04 | 83ms | Drive phase (mirror): Left leg pushes, right knee drives up. |
| run_05 | 83ms | Flight phase (mirror): Both feet off ground (2px up). Arms opposite of run_02. |

**Hitbox/Hurtbox Data:**

| Frame | Hurtbox (x, y, w, h) | Hitbox |
|-------|----------------------|--------|
| run_00 | (-5, -14, 13, 28) | None |
| run_01 | (-4, -14, 13, 28) | None |
| run_02 | (-5, -16, 13, 28) | None |
| run_03 | (-5, -14, 13, 28) | None |
| run_04 | (-6, -14, 13, 28) | None |
| run_05 | (-5, -16, 13, 28) | None |

---

### JUMP STARTUP (Anticipation)

| Property | Value |
|----------|-------|
| Frame Count | 2 |
| Frame Duration | 50ms (3 game frames at 60fps) |
| Total Duration | 100ms |
| Looping | No (plays once, transitions to jump apex) |

**Frame Descriptions:**

| Frame | Duration | Pose Description |
|-------|----------|-----------------|
| jump_startup_00 | 50ms | Anticipation crouch: Knees bend, body compresses downward 3-4px. Arms pull back and down. Head dips. Weapon mount dips with body. Coiling energy pose. Feet flat, wide stance. |
| jump_startup_01 | 50ms | Launch: Legs extending explosively. Body begins upward motion (1px above baseline). Arms swing upward. Weapon mount follows shoulder upward. Toes still touching ground. Energetic upward thrust. |

**Hitbox/Hurtbox Data:**

| Frame | Hurtbox (x, y, w, h) | Hitbox |
|-------|----------------------|--------|
| jump_startup_00 | (-7, -10, 14, 24) | None |
| jump_startup_01 | (-6, -13, 12, 27) | None |

---

### JUMP APEX

| Property | Value |
|----------|-------|
| Frame Count | 2 |
| Frame Duration | 83ms (5 game frames at 60fps) |
| Total Duration | 166ms |
| Looping | Yes (loops while vertical velocity near zero) |

**Frame Descriptions:**

| Frame | Duration | Pose Description |
|-------|----------|-----------------|
| jump_apex_00 | 83ms | Hang time: Body fully extended upward. Arms spread slightly outward for balance. Legs together, toes pointed down. Hair floats up (1px higher than normal). Weapon mount stable, barrel tracking target. Heroic silhouette. |
| jump_apex_01 | 83ms | Subtle shift: Arms adjust 1px. Slight body rotation suggestion through pixel placement. Weapon barrel may shift direction to track target. Maintains airborne feeling. |

**Hitbox/Hurtbox Data:**

| Frame | Hurtbox (x, y, w, h) | Hitbox |
|-------|----------------------|--------|
| jump_apex_00 | (-6, -15, 12, 28) | None |
| jump_apex_01 | (-6, -15, 12, 28) | None |

---

### FALL

| Property | Value |
|----------|-------|
| Frame Count | 3 |
| Frame Duration | 100ms (6 game frames at 60fps) |
| Total Duration | 300ms |
| Looping | Yes (loops while falling, frame 1-2 loop; frame 0 is transition-in only) |

**Frame Descriptions:**

| Frame | Duration | Pose Description |
|-------|----------|-----------------|
| fall_00 | 100ms | Transition from apex: Arms begin rising. Legs start to separate -- one knee bends. Body tilts slightly forward. Hair starts trailing upward from wind. Weapon mount follows body tilt. |
| fall_01 | 100ms | Full fall pose: Arms raised above shoulder level (wind resistance). Legs trailing below with knees bent. Hair and clothing pushed upward. Weapon mount angled downward slightly with body lean. Clear downward motion read. |
| fall_02 | 100ms | Arms shift 1px. Legs adjust. Maintains falling silhouette with subtle movement to prevent static feel. Weapon barrel continues tracking target. |

**Hitbox/Hurtbox Data:**

| Frame | Hurtbox (x, y, w, h) | Hitbox |
|-------|----------------------|--------|
| fall_00 | (-6, -14, 12, 28) | None |
| fall_01 | (-6, -13, 12, 28) | None |
| fall_02 | (-6, -13, 12, 28) | None |

---

### LAND

| Property | Value |
|----------|-------|
| Frame Count | 2 |
| Frame Duration | 67ms (4 game frames at 60fps) |
| Total Duration | 134ms |
| Looping | No (plays once, transitions to idle/walk/run) |

**Frame Descriptions:**

| Frame | Duration | Pose Description |
|-------|----------|-----------------|
| land_00 | 67ms | Impact: Deep crouch with knees heavily bent. Body compressed 4px below normal standing height. Arms forward for balance. Weapon mount dips with shoulder impact. Dust particle trigger point. Feet flat, wide stance. |
| land_01 | 67ms | Recovery: Rising from crouch, halfway to standing. Knees still slightly bent. Arms returning to sides. Weapon mount returning to default position. Transitioning back to idle height. |

**Hitbox/Hurtbox Data:**

| Frame | Hurtbox (x, y, w, h) | Hitbox |
|-------|----------------------|--------|
| land_00 | (-7, -10, 14, 24) | None |
| land_01 | (-6, -12, 12, 26) | None |

---

### WEAPON MOUNT (Pickup Animation)

| Property | Value |
|----------|-------|
| Frame Count | 4 |
| Frame Duration | 83ms (5 game frames at 60fps) |
| Total Duration | 332ms |
| Looping | No (plays once on weapon pickup) |

**Frame Descriptions:**

| Frame | Duration | Pose Description |
|-------|----------|-----------------|
| mount_00 | 83ms | Player stops, slight crouch. If replacing weapon, old weapon detaches from mount point -- flies off as separate sprite. Mount bracket on shoulder is empty/visible. Arms reach slightly outward. |
| mount_01 | 83ms | New weapon sprite begins tween from ground position toward mount point. Player straightens up. Eyes track upward following weapon. Anticipation pose -- body leans slightly toward weapon. |
| mount_02 | 83ms | Weapon contacts mount bracket. White flash burst at mount point (3px radius, slot 15). Weapon locks into position. Player's body jolts slightly from mounting force (1px recoil). Weapon-specific glow begins. |
| mount_03 | 83ms | Recovery: Player returns to idle stance with weapon mounted. Weapon glow fades to normal. Mount bracket reflects weapon weight. First auto-fire shot may trigger immediately. |

**Hitbox/Hurtbox Data:**

| Frame | Hurtbox (x, y, w, h) | Hitbox |
|-------|----------------------|--------|
| mount_00 | (-6, -12, 12, 26) | None |
| mount_01 | (-6, -14, 12, 28) | None |
| mount_02 | (-6, -14, 12, 28) | None |
| mount_03 | (-6, -14, 12, 28) | None |

---

### SPECIAL ATTACK (Hold Jump When Powered Up)

| Property | Value |
|----------|-------|
| Frame Count | 5 |
| Frame Duration | Variable (see below) |
| Total Duration | 500ms |
| Looping | No (plays once, returns to idle) |
| Invincibility | Frames 1-3 (during blast) |

**Frame Descriptions:**

| Frame | Duration | Pose Description |
|-------|----------|-----------------|
| special_00 | 67ms (4f) | Charge: Player crouches with weapon mount glowing intensely. Energy particles spiral inward toward weapon. Body tenses. Screen begins to brighten. |
| special_01 | 50ms (3f) | Release: Player's body snaps upright. Weapon fires massive blast -- radial shockwave from mount point. Arms thrown outward. Flash white on entire sprite for 2 frames. |
| special_02 | 83ms (5f) | Blast active: Shockwave ring expands outward (16px radius). All targets in range flash and take damage. Screen shake (6px). Weapon mount sparks and smokes. |
| special_03 | 133ms (8f) | Cooldown: Shockwave fades. Player's weapon mount smokes. Body recovers to standing. Arms lower. Weapon glow dims. |
| special_04 | 167ms (10f) | Recovery: Full return to idle stance. Weapon mount returns to normal color. Smoke dissipates. Weapon power-up state may be depleted. |

---

### HURT (Damage Taken)

| Property | Value |
|----------|-------|
| Frame Count | 3 |
| Frame Duration | Variable (see below) |
| Total Duration | 350ms |
| Looping | No |
| Invincibility | Full duration + 500ms additional i-frames |

**Frame Descriptions:**

| Frame | Duration | Pose Description |
|-------|----------|-----------------|
| hurt_00 | 67ms (4f) | Impact reaction: Body recoils backward. Head snaps back. Arms fly outward. Weapon mount jolts, barrel briefly misaligns. Knockback direction: away from damage source. Eyes squint (close). Whole sprite shifts 1-2px in knockback direction. |
| hurt_01 | 133ms (8f) | Stagger: Body bent slightly backward. One foot off ground. Arms still extended. Weapon mount returns to alignment. Pained expression. Flash white on odd game frames (i-frame blink). |
| hurt_02 | 150ms (9f) | Recovery: Body straightening. Foot returns to ground. Guarded pose with arms partially raised. Weapon resumes auto-fire targeting. Transitioning back to idle. Continue i-frame blink. |

**Hitbox/Hurtbox Data:**

| Frame | Hurtbox (x, y, w, h) | Hitbox |
|-------|----------------------|--------|
| hurt_00 | None (i-frames) | None |
| hurt_01 | None (i-frames) | None |
| hurt_02 | None (i-frames) | None |

---

### DEATH

| Property | Value |
|----------|-------|
| Frame Count | 6 |
| Frame Duration | Variable (see below) |
| Total Duration | 1200ms |
| Looping | No (holds on final frame) |
| Post-Animation | Hold frame 5 for 500ms, then fade out or trigger game over |

**Frame Descriptions:**

| Frame | Duration | Pose Description |
|-------|----------|-----------------|
| death_00 | 100ms (6f) | Initial hit: Dramatic recoil, more extreme than hurt. Head thrown back. Arms wide. Weapon detaches from mount point -- flies off as separate sprite. Mouth open. Flash fully white for 2 game frames. |
| death_01 | 150ms (9f) | Collapse start: Knees buckling. Weapon lands on ground nearby (separate sprite). Body tilting backward. Mount bracket visible but empty. |
| death_02 | 200ms (12f) | Falling: Body at 45-degree angle backward. One knee on ground. Arms limp. Detached weapon on ground or out of frame. Hair falls forward over face. |
| death_03 | 200ms (12f) | Ground impact: Body mostly horizontal. Both knees down, then torso hits ground. Dust particles trigger. Impact compression visible. |
| death_04 | 250ms (15f) | Settled: Body flat on ground. Arms splayed. Completely still. Empty mount bracket visible. Dramatic finality. |
| death_05 | 300ms (18f) | Final flash: Sprite blinks 3 times (50ms on, 50ms off pattern within this window). On last blink, sprite can dissolve into particles or remain for game-over screen. |

**Hitbox/Hurtbox Data:**

| Frame | Hurtbox | Hitbox |
|-------|---------|--------|
| All | None | None |

---

## Enemy 1: Charger (Simple Enemy)

### Palette (16 Colors)

| Slot | Purpose | Hex Code |
|------|---------|----------|
| 0 | Transparent | N/A |
| 1 | Outline | #1A1A2E |
| 2 | Body Base | #8D6E63 |
| 3 | Body Shadow | #5D4037 |
| 4 | Body Highlight | #A1887F |
| 5 | Body Specular | #D7CCC8 |
| 6 | Eye White | #FFFFFF |
| 7 | Eye Pupil | #BF360C |
| 8 | Mouth/Teeth | #F5F5F5 |
| 9 | Armor Plate | #546E7A |
| 10 | Armor Shadow | #37474F |
| 11-15 | Unused/Reserved | -- |

### Visual Description
A 32x32 stocky humanoid creature with heavy legs built for charging. Roughly 20px wide and 24px tall. Low center of gravity, broad shoulders, small head. A single armor plate on its front (shoulder guard). Glowing red eyes. Moves by stomping forward. Simple, readable silhouette -- a battering ram shape.

---

### CHARGER IDLE

| Property | Value |
|----------|-------|
| Frame Count | 2 |
| Frame Duration | 333ms (20 game frames) |
| Total Loop Duration | 666ms |
| Looping | Yes |

**Frame Descriptions:**

| Frame | Duration | Pose Description |
|-------|----------|-----------------|
| charger_idle_00 | 333ms | Standing hunched forward. Arms at sides, fists clenched. Red eyes glow steady. Weight on front foot, ready to charge. Shoulder plate catches light. |
| charger_idle_01 | 333ms | Slight torso shift (1px forward-back). Arms twitch. Eyes brighten 1px. Snorts (tiny dust puff from nostrils, 1px). Restless, aggressive idle. |

**Hitbox/Hurtbox Data:**

| Frame | Hurtbox (x, y, w, h) | Hitbox |
|-------|----------------------|--------|
| charger_idle_00 | (-8, -10, 16, 24) | None |
| charger_idle_01 | (-8, -10, 16, 24) | None |

---

### CHARGER RUN (Charge)

| Property | Value |
|----------|-------|
| Frame Count | 4 |
| Frame Duration | 100ms (6 game frames) |
| Total Loop Duration | 400ms |
| Looping | Yes (while charging toward player) |

**Frame Descriptions:**

| Frame | Duration | Pose Description |
|-------|----------|-----------------|
| charger_run_00 | 100ms | Lead foot slams ground. Body hunched forward, shoulder plate leading. Arms pumping. Head down (ramming posture). Dust burst at foot impact. |
| charger_run_01 | 100ms | Drive phase: Rear leg pushes off. Body surges forward. Both arms trail behind. Maximum forward lean. Eyes leave trailing red streak (1px). |
| charger_run_02 | 100ms | Airborne (brief): Both feet off ground. Compact pose. Shoulder plate leading edge catches highlight. |
| charger_run_03 | 100ms | Other foot lands. Impact compression (1px). Arms swing forward. Ready to stomp again. |

**Hitbox/Hurtbox Data:**

| Frame | Hurtbox (x, y, w, h) | Hitbox (x, y, w, h) |
|-------|----------------------|---------------------|
| charger_run_00 | (-8, -10, 16, 24) | (4, -8, 12, 16) |
| charger_run_01 | (-6, -10, 16, 24) | (6, -8, 12, 16) |
| charger_run_02 | (-6, -12, 16, 24) | (6, -10, 12, 16) |
| charger_run_03 | (-8, -10, 16, 24) | (4, -8, 12, 16) |

---

### CHARGER DEFEAT

| Property | Value |
|----------|-------|
| Frame Count | 3 |
| Frame Duration | Variable |
| Total Duration | 400ms |
| Looping | No (sprite removed after final frame) |

**Frame Descriptions:**

| Frame | Duration | Pose Description |
|-------|----------|-----------------|
| charger_defeat_00 | 100ms (6f) | Hit reaction: Flash white for 2 game frames. Body stumbles, shoulder plate cracks. Eyes flicker. Momentum carries forward 1px. |
| charger_defeat_01 | 133ms (8f) | Collapse: Legs give out. Body pitches forward. Shoulder plate breaks off (2-3 fragments). Arms go limp. Face-plant angle. |
| charger_defeat_02 | 167ms (10f) | Dissolve: Body crumbles into 6-8 pixel fragments that scatter outward. Dust cloud at impact. Fragments fade to transparent. Coin burst spawns at center. |

**Hitbox/Hurtbox Data:**

| Frame | Hurtbox | Hitbox |
|-------|---------|--------|
| All | None | None |

---

## Enemy 2: Shooter (Ranged Enemy)

### Palette (16 Colors)

| Slot | Purpose | Hex Code |
|------|---------|----------|
| 0 | Transparent | N/A |
| 1 | Outline | #1A1A2E |
| 2 | Body Base | #7E57C2 |
| 3 | Body Shadow | #512DA8 |
| 4 | Body Highlight | #9575CD |
| 5 | Eye Glow | #FF1744 |
| 6 | Eye Glow Bright | #FF5252 |
| 7 | Weapon Base | #546E7A |
| 8 | Weapon Shadow | #37474F |
| 9 | Weapon Highlight | #78909C |
| 10 | Projectile Core | #FFAB00 |
| 11 | Projectile Glow | #FFD740 |
| 12 | Cloth/Cloak | #311B92 |
| 13 | Cloth Shadow | #1A0E5E |
| 14 | Dark Fill | #263238 |
| 15 | FX Spark | #FFFFFF |

### Visual Description
A 32x32 cloaked figure standing upright. Distinctly different silhouette from the Charger -- tall, thin, hooded. Carries a small weapon/staff that fires projectiles. Glowing red eyes visible under hood. Tattered cloak drapes down. Stationary when firing -- takes position behind cover and shoots. Roughly 14px wide and 28px tall.

---

### SHOOTER IDLE

| Property | Value |
|----------|-------|
| Frame Count | 2 |
| Frame Duration | 250ms (15 game frames) |
| Total Loop Duration | 500ms |
| Looping | Yes |

**Frame Descriptions:**

| Frame | Duration | Pose Description |
|-------|----------|-----------------|
| shooter_idle_00 | 250ms | Standing upright with slight hunch. Weapon held at waist level, pointing toward player. Hood casts shadow over face, only red eye glow visible (2px red dots). Cloak hangs still. Alert posture. |
| shooter_idle_01 | 250ms | Subtle sway: Torso shifts 1px laterally. Eye glow flickers (1px shift in highlight). Cloak sways 1px. Weapon adjusts aim slightly. Watchful, still feel. |

**Hitbox/Hurtbox Data:**

| Frame | Hurtbox (x, y, w, h) | Hitbox |
|-------|----------------------|--------|
| shooter_idle_00 | (-5, -14, 10, 28) | None |
| shooter_idle_01 | (-5, -14, 10, 28) | None |

---

### SHOOTER ATTACK (Ranged Fire)

| Property | Value |
|----------|-------|
| Frame Count | 4 |
| Frame Duration | Variable |
| Total Duration | 467ms |
| Looping | No |
| Projectile | Spawns projectile sprite (6x4) at frame 2 |

**Frame Descriptions:**

| Frame | Duration | Pose Description |
|-------|----------|-----------------|
| shooter_attack_00 | 117ms (7f) | Wind-up: Weapon raises to shoulder height. Body leans back slightly to aim. Eye glow intensifies (add 1px brighter core). Hood shifts with head tilt. Energy gathers at weapon tip (1px orange dot grows to 2px). |
| shooter_attack_01 | 83ms (5f) | Aim lock: Weapon fully aimed. Body braces. Orange energy at weapon tip is at maximum (3px glow). Cloak billows backward from energy buildup. Brief pause -- telegraph for player to dodge. |
| shooter_attack_02 | 100ms (6f) | **Fire frame**: Projectile launches from weapon tip. Muzzle flash (3px orange-white burst). Body recoils 1px backward. Cloak snaps from recoil. Projectile sprite spawns traveling toward player position. DAMAGE FRAME (projectile). |
| shooter_attack_03 | 167ms (10f) | Recovery: Weapon lowers. Body straightens. Eye glow returns to normal. Cloak settles. Slow recovery -- punishable window for player. Weapon smoke wisp (1px). |

**Projectile (separate 6x4 sprite):**
- Orange-yellow energy bolt, pulsing glow
- Travels horizontally at 100px/sec
- Hitbox: (0, 0, 6, 4) centered
- Destroyed on contact with player, wall, or after 4 seconds
- Uses slots 10, 11 from shooter palette

**Hitbox/Hurtbox Data:**

| Frame | Hurtbox (x, y, w, h) | Hitbox |
|-------|----------------------|--------|
| shooter_attack_00 | (-5, -14, 10, 28) | None |
| shooter_attack_01 | (-5, -14, 10, 28) | None |
| shooter_attack_02 | (-5, -14, 10, 28) | None (projectile is separate) |
| shooter_attack_03 | (-5, -14, 10, 28) | None |

---

### SHOOTER DEFEAT

| Property | Value |
|----------|-------|
| Frame Count | 4 |
| Frame Duration | Variable |
| Total Duration | 600ms |
| Looping | No (sprite removed after final frame) |

**Frame Descriptions:**

| Frame | Duration | Pose Description |
|-------|----------|-----------------|
| shooter_defeat_00 | 100ms (6f) | Hit reaction: Flash white for 3 game frames. Eye glow flickers rapidly. Head snaps back. Weapon drops from hands (separate sprite or drawn leaving). Hood blows back revealing empty darkness beneath. |
| shooter_defeat_01 | 133ms (8f) | Collapse: Body crumbles inward. Cloak billows outward as body shrinks. Weapon on ground. Arms go limp inside cloak. Dramatic implosion feel. |
| shooter_defeat_02 | 167ms (10f) | Scatter: Cloak tears apart into 4-5 fabric fragments (2-4px each) that float outward. Body beneath dissolves into shadow particles. Eye glow fades -- final red spark. |
| shooter_defeat_03 | 200ms (12f) | Settle: Fabric scraps settle on ground. Shadow dissipates. Weapon remains briefly then fades. Small energy spark where eyes were, then dark. |

**Hitbox/Hurtbox Data:**

| Frame | Hurtbox | Hitbox |
|-------|---------|--------|
| All | None | None |

---

## Destruction Tile Animations

### Wall Crumble Animation (Per Tile)

When a destructible wall tile is destroyed, it plays a crumble animation before being removed:

| Property | Value |
|----------|-------|
| Tile Size | 16x16 pixels (standard tile) |
| Frame Count | 4 |
| Total Duration | 267ms |
| Fragment Count | 6-8 per tile |

**Frame Descriptions:**

| Frame | Duration | Description |
|-------|----------|-------------|
| crumble_00 | 33ms (2f) | Cracks appear across tile surface (2-3 crack lines using outline color). Tile shifts 1px in hit direction. Dust puff at impact point. |
| crumble_01 | 67ms (4f) | Tile splits into 6-8 fragments along crack lines. Fragments begin separating -- 1px gaps appear between pieces. Each fragment gets slight individual velocity. |
| crumble_02 | 83ms (5f) | Fragments scatter: each fragment falls with gravity (acceleration), small fragments faster than large. Dust cloud expands (4-6 dust particles, 1-2px each). Fragments rotate slightly using color-shift technique. |
| crumble_03 | 84ms (5f) | Fragments reach ground or leave tile area. Dust settles. Small debris pile remains for 0.5s then fades. Tile collision is removed at frame 01. |

### Material-Specific Crumble Variants

| Material | Fragment Color | Fragment Size | Dust Color | Special Effect |
|----------|---------------|---------------|-----------|----------------|
| Soft Clay | Brown/tan | 2-3px each | Tan dust | Fragments crumble on landing |
| Stone | Gray | 3-4px each | Gray dust | Fragments bounce once |
| Bronze | Bronze/gold | 3-4px each | Brown dust | Metallic glint on fragments |
| Metal | Dark gray | 4-5px each | Spark particles | Sparks fly on separation |
| Reinforced | Mixed metal/stone | 4-6px each | Heavy dust + sparks | Multi-stage crumble (slower) |

### Chain Reaction Visual

When a support wall is destroyed causing a chain reaction:

| Phase | Duration | Description |
|-------|----------|-------------|
| Trigger | 0ms | Support wall crumbles normally |
| Propagation | 100ms per connected tile | Cracks visually spread from destroyed support to connected walls (traveling crack line, 1px, moves at 160px/sec) |
| Cascade | 100ms intervals | Each connected wall crumbles in sequence (uses standard crumble animation but staggered) |
| Settling | 200ms | Massive dust cloud, all debris settles, camera shake subsides |

---

## Animation State Machine

### Player Character State Transitions

```
                    +--------+
                    |  IDLE  |<-----------+
                    +--------+            |
                   /    |    \            |
            move? /     |     \          | timeout
                 /      |      \         |
          +------+  jump?  +--------+    |
          | WALK |---+     | MOUNT  |----+
          +------+   |     +--------+
            |   |    |
       run? |   |    v
            v   | +----------+
          +-----+ | JUMP     |+------+
          | RUN |  | STARTUP  || HURT |
          +-----+  +----------++------+
            |         |            |
            |    +----v----+       | hp<=0?
            |    | JUMP    |       v
            |    | APEX    |   +-------+
            |    +---------+   | DEATH |
            |         |       +-------+
            |    +----v----+
            |    |  FALL   |
            |    +---------+
            |         |
            |    +----v----+
            +--->|  LAND   |---> IDLE / WALK / RUN
                 +---------+

Special states (overlay):
  WEAPON_RECOIL -- plays on mount point independently of body state
  SPECIAL_ATTACK -- triggered by holding jump when powered up (interrupts current state)
```

### State Transition Table

| Current State | Input/Condition | Next State | Priority |
|--------------|-----------------|------------|----------|
| IDLE | Horizontal input detected (D-pad) | WALK | 1 |
| IDLE | Jump button pressed | JUMP_STARTUP | 2 |
| IDLE | Jump button held + powered up | SPECIAL_ATTACK | 3 |
| IDLE | Walk over weapon drop | MOUNT | 2 |
| IDLE | Damage received | HURT | 10 |
| IDLE | HP <= 0 | DEATH | 99 |
| WALK | No horizontal input | IDLE | 1 |
| WALK | Horizontal input held > 500ms | RUN | 1 |
| WALK | Jump button pressed | JUMP_STARTUP | 2 |
| WALK | Walk over weapon drop | MOUNT | 2 |
| WALK | Damage received | HURT | 10 |
| WALK | HP <= 0 | DEATH | 99 |
| RUN | Horizontal input released | WALK | 1 |
| RUN | Jump button pressed | JUMP_STARTUP | 2 |
| RUN | Walk over weapon drop | MOUNT | 2 |
| RUN | Damage received | HURT | 10 |
| RUN | HP <= 0 | DEATH | 99 |
| JUMP_STARTUP | Animation complete | JUMP_APEX | Auto |
| JUMP_APEX | Vertical velocity < -threshold | FALL | Auto |
| JUMP_APEX | Jump button held + powered up | SPECIAL_ATTACK | 3 |
| JUMP_APEX | Damage received | HURT | 10 |
| FALL | Ground contact detected | LAND | Auto |
| FALL | Damage received | HURT | 10 |
| LAND | Animation complete | IDLE | Auto |
| LAND | Horizontal input held | WALK/RUN | 1 |
| LAND | Jump button (buffer window 100ms) | JUMP_STARTUP | 2 |
| MOUNT | Animation complete (frame 3) | Previous state (IDLE/WALK/RUN) | Auto |
| MOUNT | Damage received | HURT | 10 |
| MOUNT | HP <= 0 | DEATH | 99 |
| SPECIAL_ATTACK | Animation complete (frame 4) | IDLE | Auto |
| SPECIAL_ATTACK | HP <= 0 | DEATH | 99 |
| HURT | Animation complete | IDLE | Auto |
| HURT | HP <= 0 | DEATH | 99 |
| DEATH | Animation complete | HOLD (game over) | Final |

**Note**: Auto-fire and weapon recoil run as an independent overlay animation on the weapon mount point. They do not interrupt or conflict with the body state machine. Target cycling (right thumb button) changes the auto-fire target without affecting body animation state.

### Enemy State Machines

#### Charger State Machine

```
+--------+       +--------+
|  IDLE  |<----->|  RUN   |
+--------+       +--------+
     |                |
     v                v
+--------+       +--------+
| DEFEAT |       | DEFEAT |
+--------+       +--------+
```

| Current State | Condition | Next State |
|--------------|-----------|------------|
| IDLE | Player within 128px (awareness range) | RUN (charge toward player) |
| IDLE | Random timer (2-4s) | RUN (patrol) |
| IDLE | HP <= 0 | DEFEAT |
| RUN | Reached player position or wall | IDLE (stunned 800ms, vulnerable) |
| RUN | Player beyond 200px | IDLE |
| RUN | HP <= 0 | DEFEAT |

#### Shooter State Machine

```
+--------+       +--------+
|  IDLE  |<----->| ATTACK |
+--------+       +--------+
     |                |
     v                v
+--------+       +--------+
| DEFEAT |       | DEFEAT |
+--------+       +--------+
```

| Current State | Condition | Next State |
|--------------|-----------|------------|
| IDLE | Player within 160px and line of sight | ATTACK |
| IDLE | HP <= 0 | DEFEAT |
| ATTACK | Animation complete | IDLE (cooldown 1500ms) |
| ATTACK | Line of sight broken (wall between) | IDLE |
| ATTACK | HP <= 0 | DEFEAT |

---

## Sprite Sheet Layout

### Player Sprite Sheet (256x256 PNG)

```
Row 0 (y=0):   idle_00  idle_01  idle_02  idle_03  [empty]  [empty]  [empty]  [empty]
Row 1 (y=32):  walk_00  walk_01  walk_02  walk_03  walk_04  walk_05  walk_06  walk_07
Row 2 (y=64):  run_00   run_01   run_02   run_03   run_04   run_05   [empty]  [empty]
Row 3 (y=96):  jstart_0 jstart_1 japex_0  japex_1  fall_00  fall_01  fall_02  [empty]
Row 4 (y=128): land_00  land_01  mount_00 mount_01 mount_02 mount_03 [empty]  [empty]
Row 5 (y=160): hurt_00  hurt_01  hurt_02  spec_00  spec_01  spec_02  spec_03  spec_04
Row 6 (y=192): death_00 death_01 death_02 death_03 death_04 death_05 [empty]  [empty]
Row 7 (y=224): [weapon recoil frames + weapon direction sprites (8 directions)]
```

Each cell is 32x32. Sheet is 256x256 total (8 columns x 8 rows).

### Weapon Attachment Sprites (64x64 PNG)

```
Row 0 (y=0):   blaster_R  blaster_RU  blaster_U  blaster_LU
Row 1 (y=8):   blaster_L  blaster_LD  blaster_D  blaster_RD
Row 2 (y=16):  scatter_R  scatter_RU  scatter_U  scatter_LU
Row 3 (y=24):  scatter_L  scatter_LD  scatter_D  scatter_RD
Row 4 (y=32):  beam_R     beam_RU     beam_U     beam_LU
Row 5 (y=40):  beam_L     beam_LD     beam_D     beam_RD
Row 6 (y=48):  missile_R  missile_RU  missile_U  missile_LU
Row 7 (y=56):  missile_L  missile_LD  missile_D  missile_RD
```

Each cell is 16x8. Sheet is 64x64 total. Weapons are drawn at each of 8 cardinal directions.

### Enemy 1 (Charger) Sprite Sheet (128x128 PNG)

```
Row 0 (y=0):   idle_00  idle_01  [empty]  [empty]
Row 1 (y=32):  run_00   run_01   run_02   run_03
Row 2 (y=64):  [empty]  [empty]  [empty]  [empty]
Row 3 (y=96):  def_00   def_01   def_02   [empty]
```

Each cell is 32x32. Sheet is 128x128 total (4 columns x 4 rows).

### Enemy 2 (Shooter) Sprite Sheet (256x192 PNG)

```
Row 0 (y=0):   idle_00  idle_01  [empty]  [empty]  [empty]  [empty]  [empty]  [empty]
Row 1 (y=32):  [empty]  [empty]  [empty]  [empty]  [empty]  [empty]  [empty]  [empty]
Row 2 (y=64):  atk_00   atk_01   atk_02   atk_03   [empty]  [empty]  [empty]  [empty]
Row 3 (y=96):  proj_00  proj_01  [empty]  [empty]  [empty]  [empty]  [empty]  [empty]
Row 4 (y=128): def_00   def_01   def_02   def_03   [empty]  [empty]  [empty]  [empty]
Row 5 (y=160): [reserved for future: Flyer, Tank, Swarm enemy types]
```

Each cell is 32x32 (proj uses only top-left 8x8 of its cell). Sheet is 256x192 total.

### Destruction Tile Sheet (128x64 PNG)

```
Row 0 (y=0):   clay_crumble_00-03     stone_crumble_00-03
Row 1 (y=16):  bronze_crumble_00-03   metal_crumble_00-03
Row 2 (y=32):  reinforced_crumble_00-03  chain_crack_propagation_00-03
Row 3 (y=48):  debris_fragment_variants (8 variants, 4x4px each)
```

Each crumble cell is 16x16 (tile-sized). Fragment variants are 4x4px packed into 16x16 cells.

---

## Unity Integration Notes

### Animator Controller Setup

- Use Unity `Animator` with `AnimatorController` per character
- Each animation state corresponds to an `AnimationClip`
- Weapon recoil uses a separate `AnimatorController` on the weapon mount child object
- Transitions use `bool` and `trigger` parameters:
  - `isWalking` (bool)
  - `isRunning` (bool)
  - `isGrounded` (bool)
  - `verticalVelocity` (float)
  - `mountTrigger` (trigger) -- weapon pickup
  - `specialTrigger` (trigger) -- special attack
  - `hurtTrigger` (trigger)
  - `deathTrigger` (trigger)
- Set transition duration to 0 (no blending -- instant frame switches for pixel art)
- Disable `Has Exit Time` on interrupt-priority transitions (hurt, death)
- Enable `Has Exit Time` on natural completions (mount recovery, land)
- Weapon auto-fire recoil is driven by the weapon system, not the animator (fires independently)

### Sprite Import Settings

| Setting | Value |
|---------|-------|
| Pixels Per Unit | 32 |
| Filter Mode | Point (no filter) |
| Compression | None |
| Sprite Mode | Multiple |
| Pivot | Bottom Center (0.5, 0) |
| Max Size | 256 |
| Generate Mip Maps | Off |

---

**Version**: 1.1
**Last Updated**: 2026-02-04
**Status**: Active
**Assessment**: 6.1 - Pixel Art Animation Production

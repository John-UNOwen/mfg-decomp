# 34. Defect dossier 2026-11-08 — waiting-room reveal and seated leg animation

Facts for two live defects, gathered 2026-11-08 from the reconstruction source, the served
webroot, the live/staging/Windows lanes, and the original installed game on the share.
Defect 1 (waiting room) is mid-reproduction; its runtime section is marked and will carry
console evidence. Defect 2 (leg animation) is inventory only — no fix attempted, per instruction.

## Defect 1 — waiting room: only two of four characters reveal, screen frozen until the game loads

Owner report: solo + 3 CPUs loads two characters (player included); also reproduces with four
real players queued together. The casting/waiting screen then sits half-drawn until the game
scene switches in regardless.

### The client mechanics that produce exactly this symptom (static, proven by reading)

1. **Seats are marked established by a timer, before their reveal has loaded anything.**
   `MatchingFlow.OnUpdate` (`Assets/Assembly-CSharp/MatchingFlow.cs:285-303`) walks
   `PlayerInfos` and for every still-unestablished seat whose `IsActive` and whose two time
   gates have passed (`_elapsedTimeOfMatchingAnimePlayed >= _currentNeedWaitSec` and
   `Time.time - _matchingAnimPlayingTime >= _currentNeedWaitSec`, both seeded from
   `_matchingAnimeWaitSec`/`_proMatchingAnimeWaitSec` = 2 s/4 s, `:90-93`, `:171-172`) sets
   `m_isMatchingEstablished[i] = true` (`:299`) and *then* fires
   `StartCharaMatchingAnime(i, ...).Forget()` (`:300`). The flag does not wait for the reveal.
2. **The flow's exit gate reads only those flags.** `OnExecute` waits on
   `m_isMatchingEstablished[0..3]` (`:254`), then a fixed 8 s, roulette 2 s, voice 2 s, and
   `GotoNextScene("MahjongPlay")` (`:256-277`). A seat whose reveal died still holds its flag,
   so the scene advances on schedule with that seat blank — "frozen until the actual game loads".
3. **The reveal is a long await chain with many ways to stop before the slot is drawn.**
   `StartCharaMatchingAnime` (`:310-426`): `m_charaManager.Load(costume, parent2D)` (`:321`) →
   `m_standingCharaParent3D.Find($"{costumeItemID}/Camera")` (`:322`) → per-seat light disable
   (`:329`) → `Parts_SetDispAllOff/Parts_SetExclusiveDisp(i, ...)` (`:334-338`) →
   `MatchCutinSetup` (cutin/BG/trophy assets, `:340`) → `MatchingScreenPlayerSetup` (`:341`) →
   `ProCompSet` (`:342`) → `ResetNext` + `SetTrigger("Next")` (`:347-349`) — only at `:349` does
   the slot actually start appearing. Any throw or never-resolving await before `:349`, swallowed
   by the `Forget()`, leaves the seat permanently blank while the flag is already true.
4. **StandingCharaManager.Load caches by ItemID alone**
   (`Assets/Assembly-CSharp/StandingCharaManager.cs:233-263`): if an instance with the same
   `CostumeItemID` exists and duplicates are off, it returns that controller and instantiates
   nothing under the new seat's parent (`:240`, `:263`) — the second seat would render nothing,
   and the later `Parts_SetDispAllOff(i)` indexes `_instances` by seat index, so a skipped add
   shifts it onto the wrong model. **The Matching scene has `AllowDuplicatesInstance: 1`**
   (`Assets/MFG/Scenes/Matching/Matching.unity:39889`), so every Load instantiates; duplicates
   instead collide in name-space: two instances under `m_standingCharaParent3D` named
   `ToString()` of the same ItemID, and `Find("{itemID}/Camera")` (`MatchingFlow:322`) returns
   the FIRST one for both seats — the second seat disables the first seat's camera and light
   (`:325-330`), and `m_standingCharaCamera.Add` order then no longer matches seat order, which
   matters at the re-enable loop (`:413-416`, enables `m_standingCharaCamera[0..i]`).
5. **Server side, seat data is honest for the solo case.** CPU seats are announced
   `ptype="3"` with distinct characters: the pick `(tid + seat*7) % 19 + 1` skips every human
   `SelectChara` and every earlier CPU pick (`aog.ts:306-330`); the client synthesizes their
   costume as the character's default (`PlayerInfo.cs:166` via
   `CostumeItemMaster.GetDefaultEquipItemID`, a complete Chara01-21 table) and their character
   OID parses cleanly for 1-19 (`CharaTypeExtentions.cs:16-27`, falls back to Chara01 with a
   console error only for unparseable OIDs — the server never emits those). Distinct characters
   mean distinct default costumes, so the duplicate-instance theory does NOT explain the solo
   3-CPU case by itself; it remains a live candidate for the multiplayer case (two humans with
   the same character + costume is legal and unguarded).
6. **Fix history.** Round 1+2 (2026-11-05, `0d808e5` in the plugins repo, KB
   `matching-waiting-room-seat-authority`) rewrote seat authority server-side: honest pnum,
   CPU seats announced `ptype=3`, lobby pruning, reconnect re-seating. Those fixes are why the
   answer is now internally consistent; they deliberately never touched the client, and the KB
   already recorded the open question: "Why a reveal half-fails still needs one WebGL console
   capture from the client that shows the empty seats."

### Candidate failure points, ranked for the console capture to discriminate

- `Standing CharaManager: Loaded ItemID ...` logs one line per completed Load (`:249`): four
  lines ⇒ death is after Load; two ⇒ two Loads failed/hung.
- `ParseFromMatchingEndNode Error` (`PlayerInfo.cs:148`) ⇒ a CPU seat's synthesis threw.
- `ToCharaType ... 変換できませんでした` (`CharaTypeExtentions.cs:24`) ⇒ OID problem (not
  expected per 5).
- A WebGL `Failed to load`/Addressables error naming a bundle ⇒ asset miss for that costume/BG.
- An unhandled exception stack before `SetTrigger("Next")` ⇒ the named call is the root cause.
- Four loads + no exceptions ⇒ the camera-index race in 4; then the capture should show the
  reveal order (`Loaded ItemID` timestamps) not matching seat order.

### Runtime reproduction — complete, with the console evidence the table predicted

Lane facts first. The repro needs valid accounts: two of the four lane accounts had silently
wrong bcrypt hashes (CRLF-polluted signup lines); until they were verified offline against the
core's own hashes, every "browser can't log in" chase was chasing a wrong password. Registered
state can be injected server-side (`states.player_game` on the profile doc, with the core down so
its in-memory copy can't overwrite it) — the resume rule at `GameDataManager.cs:178` then skips
NewRegistration. (Side observation, separate defect: in the deployed webroot the NewRegistration
scene loads and its 99 s name-timer never advances — Unity's loop alive, no console output; the
injection sidesteps it, so it does not block the repro.)

With four accounts on one table (gate lobby → card flow → Login → Home → AUTOSERVE queue):
matching, seating, entry, and play are healthy — the core logs each seat fill, every client reaches
MahjongPlay, and matches run to Result. The failure is entirely client-side reveal:

- **Host clients reveal 4/4 seats. Non-host clients revealed 1/4** — only the first completed
  `StartCharaMatchingAnime` — and the one that completed was the host's girl, not their own.
  Owner's "only two of four load" is the same failure with a different survivor count.
- The missing seats print **no `Loaded ItemID` line and no console error**. The established flag is
  set *before* the reveal is invoked (`MatchingFlow.cs:299` before `:300 .Forget()`), so
  `WaitUntil` (`:254`) passes and the flow proceeds to MahjongPlay regardless — while the reveal
  chain died between the `m_charaManager.Load` call (`:321`) and the success log
  (`StandingCharaManager.cs:249`): inside `StandingCharaManager.Load`/`CharaModel.Load`. The
  `.Forget()` call site swallows the exception (the exact WebGL trap documented at
  `ResidentFlow.cs:219`) — the failure is silent by construction.
- **The visible "freeze" is bounded and proven**: post-confirm choreography is a fixed 8 s + 2 s +
  2 s (`:256/:270/:274`), and every seat whose chain died stays an empty frame for it (and for
  however long the surviving chains keep the room open). p3 timeline: Matching 173 s, one reveal
  178 s, exit 205 s — a 32 s room with three blank seats.
- Reveal timing is client-local (elapsed accumulation, no server clock): the same table staggered
  17–26 s apart on one client and collapsed to near-simultaneous on another — which seat survives
  is nondeterministic per client, matching "sometimes two, sometimes different ones".
- The same-costume variant of the run (four accounts injected with one donor profile) revealed
  **four loads everywhere, all `Costume03_001`** — four same-named instances under
  `AllowDuplicatesInstance:1`, and every seat's camera lookup `Find("{itemID}/Camera")` (`:322`)
  resolves to whichever instance completed first: the camera race from section 4, live on screen.

Why the two fix rounds did not change the symptom: they targeted duplicate-instance allowance and
seat liveness; the failure path observed here is a reveal chain that never returns, invoked
fire-and-forget after its seat flag is already true — nothing in the current design notices, waits,
or logs. The next discriminator needs the client: wrap the `:321` call (or log inside
`StandingCharaManager.Load` before/after each await) so the swallowed failure names itself —
a rebuild, worth doing before any candidate fix ships.

## Defect 2 — seated characters: leg motion swivels then teleports back (facts inventory, no fix)

Owner observation: at the side seats (left/right), a character whose idle includes leg motion
swivels a bit, snaps back to rest, and repeats — jarring. Task: inventory every mechanism that
controls leg movement in both trees. Facts only.

### A. Reconstruction (`C:\mfg\build\unity-project`)

**What animates the legs at a table seat**
- The seated idle is ONE full-body Generic clip. `CharaMotion01_Game.controller` base-layer
  default state `Wait0` → `Assets/MFG/Characters/Character1/3D/Animations/Motion/Default/HiyoriWait.anim`
  (636,292 B, 2.0 s loop): quaternion rotation curves on `Hips`, `Hips/Thigh_L`,
  `.../Shin_L`, `.../Foot_L`, `.../Toe_L(+_end)`, `.../Knee_L(+_end)`, `.../ThighTwist_L`,
  `.../ShinTwist_L`, `.../HipWeight_L(+_end)` and all `_R` mirrors, plus a Hips y-bob in
  `m_PositionCurves` (y≈0.9314). Verified identically leg-curved in Chara9 `PineWait.anim`
  (628,336 B); per-costume idle variants exist (`Motion/SwimWear/…`, `Motion/Toho/…_newjoint_toho.anim`).
- Controller layering: Base Layer (`m_IKPass:1`) + Face Layer (override, `m_DefaultWeight:1`,
  carries only face clips `Face_*.anim`). **No AvatarMask asset exists anywhere in the project**
  (grep `^AvatarMask:` = 0 hits) and both layers serialize `m_Mask: {fileID: 0}` — the Face
  layer overrides whatever weights the dump lost. **`Write Defaults` is not serialized in any
  dumped `.controller`** (the asset-dump tool dropped the per-state flag; Unity runtime default
  is ON) — this is exactly the flag that decides "a state that does not touch the legs resets
  them to bind pose", i.e. a reset candidate invisible in the dump.
- Rig: Generic avatar (`Hiyori_01Avatar.asset`: `m_Human` block empty, 249-node skeleton,
  paths `Reference/Hips/Thigh_R/Shin_R/...`); clips bind relative (`Hips/Thigh_L...`) so the
  Animator root is the `Reference` node. The `CharaModel.prefab` Animator serializes
  `m_Avatar: {fileID: 0}` (avatar re-attached at runtime by `ModelManager.Init`),
  `m_ApplyRootMotion: 0`, `m_CullingMode: 0` (AlwaysAnimate).
- Per-frame bone writers on the table model — the complete list: the **UTJ springbone DLL**
  (`Assets/Plugins/Springbone.Runtime.dll`, 91,136 B) with 131 components on `CharaModel.prefab`:
  1 `SpringManager` (node "Chara"), ~78 `SpringBone`-class components on **hair/skirt/breast/
  necktie chains only**, 49 bare pivot components, and 3 `SpringCapsuleCollider`s
  (`Spine2CapsuleCollider`, `Thigh_LCapsuleCollider`, `Thigh_RCapsuleCollider`) — the thighs
  exist only as SKIRT colliders; no SpringBone drives leg bones. Everything else per-frame is
  shader floats (Face/Eye/Mouth LateUpdate) or cameras (`StandingCharaFPSAdjuster`,
  `MahjongPlayCameraManager` adulation look — `CameraAdulationList` empty for Chara1).
  **No C# writes leg bone rotations; no `OnAnimatorIK` attached (`IKTest.cs` is dead code);
  no constraints; no re-parenting.** So the Animator is the only leg writer.
- Seat wiring (`MahjongPlay.unity` + `MahjongPlayFlow.cs`): seat parents Player1..4 authored at
  yaw 0/+90/180/-90; model instantiated under `m_chara3DParent[cha]` with per-seat render layers
  (`Chara3DLeft/Right/Center`, `CharacterModelController.cs:58-67`); `SetOffset` composes
  `_orgRotation + rotationOffset` once (`:53/73`); per-character `CharaOffsetParam` (all zero for
  Chara1) + `MahjongHeightOffsetParam`; the Animator gets `LookDirection` = seat-vs-own wind int
  on every `PlayMotionMahjong`, which wraps `SetTrigger` between two forced `animator.Update(0f)`
  calls (`ModelManager.cs:57-88`). `PlayCrossFadeInFixedTimeInMahjong` takes a `layerID` arg.
- The matching/casting view is a different stack: `StandingCharaManager` + Story controller +
  `PlayMotion("MatchingIn"/"MatchingOut")` — relevant only to the casting screenshots.

**The one mechanism in A that behaves differently at yaw ±90 than 0/180:** springbone physics —
its pivot frames and the thigh capsule colliders are world-space, so a 90°-rotated seat changes
skirt-vs-thigh collider dynamics (affects the skirt, not the legs themselves).

### B. Original (`\\Myappsandbox\c\Games\MFG`)

- `game/MFGClient_Data` is the Mono build the reconstruction was decompiled from
  (`Managed/Assembly-CSharp.dll` 4,291,072 B) and ships the **byte-identical**
  `Springbone.Runtime.dll` (91,136 B) — the physics side is unchanged from original.
- All original animation assets are BINARY: `StreamingAssets/aa/catalog.json` (46.9 MB) +
  `aa/StandaloneWindows64/` = 28,292 hashed `.bundle` files; catalog addresses match the
  reconstruction tree 1:1 (`.../CharaMotion01_Game.controller`, `.../Default/HiyoriWait.anim`,
  `Standing3D.prefab` all verified by name). `level0-61` + `sharedassets0-61(+.resS)` are the
  scenes (MahjongPlay seat transforms live there). `src/` contains only the spice2x source
  tarball; no decompiled original C# exists anywhere; BepInEx `plugins/` holds only S1Freeze
  (timer freeze, from this repo) — nothing patches animation.
- Therefore curve-level facts about the ORIGINAL's clips/controller (per-state Write Defaults,
  any AvatarMasks the dump dropped, the original `Animator.m_Avatar` link) needed tooling:
  (done after this section was first written, directly over the bundles + DLL with UnityPy 1.25.4 +
  ilspycmd 8.2 — see section E for the extracted ground truth).

### C. What this inventory means for the symptom (mechanism classes, all evidenced)

Given "Animator is the only leg writer", "swivel → snap back → repeat" can only be produced by:
1. **A clip/controller transition that lands the legs at a different pose periodically** — e.g.
   a state-entry crossfade re-driving the leg curves, or Write Defaults ON in a legless override
   state snapping legs to bind pose; the `Update(0f)`-flushed `SetTrigger` pattern in
   `ModelManager.PlayMotionMahjong` forces synchronous state evaluation twice per event.
2. **The Face/override layer masks the base** — masks would live in the lost `.controller` data
   (dump never carried masks; no AvatarMask asset exists to assign).
3. **Clip import differences** (loop time/blend curve, rotation interpolation of the quaternion
   curves vs the original) between the dumped clip and the original bundle clip — only the
   bundle export (B) can show it.
Why only side seats: seat yaw ±90 itself has no animation branch — but `LookDirection` (seat
wind int) selects transition conditions in the controller (parameter exists and is fed per
seat); if a side-seat condition routes through a different state or exit-time than the front
seat, the "reset" pose enters periodically only there. Whether the original controller's
conditions/exit-times survived the dump is again a (B)-tooling question.

### E. Original game ground truth (extracted from the shipped build)

Extracted 2026-11-08 directly from `\\Myappsandbox\c\Games\MFG\game\MFGClient_Data` with
UnityPy 1.25.4 (Python 3.14), a hand-written parser for `aa/catalog.json`'s
`m_KeyDataString`/`m_BucketDataString`/`m_EntryDataString`/`m_ExtraDataString` blobs
(91,779 keys, 68,550 location entries, 28,292 bundle locators, all offsets round-tripped) and
for the Unity `AnimatorController` `m_Controller` ControllerConstant blob (layout per
AssetStudio/aelurum `AnimatorController.cs` and unity-rs-core `animator_controller.rs`), plus
`ilspycmd 8.2` over `Managed\Assembly-CSharp.dll` (1,317 files) and `Managed\Springbone.Runtime.dll`
(34 files). Scratch artifacts (JSON dumps, parsers) under `C:\mfgtmp\`. Share untouched.

#### E.1 AvatarMask census: zero, everywhere it could exist

- `catalog.json` `m_resourceTypes` table lists 26 classes loadable as addressable resources
  (Texture2D, Material, `RuntimeAnimatorController`, AnimationClip, `AnimatorOverrideController`,
  Avatar, GameObject, Timeline, …). **`UnityEngine.AvatarMask` is not among them.**
- Object census (`obj.type`, no filtering) over `globalgamemanagers`, `globalgamemanagers.assets`,
  `resources.assets`: 0 AvatarMask.
- Type-table census over all 124 scene files (`level0`–`level61` + `sharedassets0`–`sharedassets61.assets`):
  0 AvatarMask (level18/sharedassets18 printed in full in `C:\mfgtmp\scene18.json`, `census2.json`).
- ~350 bundles opened one-by-one, 0 AvatarMask: all 68 dependency bundles of
  `Standing3D/Motion/Chara01/Game.controller` (incl. the controller's own bundle
  `00cfdf051286211456a5f38389143582.bundle`), all 122 dependencies of
  `Standing3D/Costume01_001_Model.prefab` (main `5005fb018f625f3651ca4ac49779b9fc.bundle`,
  model/avatar `d7359c0b36888bc21890a4b155995999.bundle`), the 17 deps of `Hiyori_01.fbx`,
  `76e3ef84d70fa9f0d7a6095afed93290.bundle` (Standing3D.prefab), clip bundles
  `c19fb16c…` / `da622f43…` / `6f3989de…`, the override controllers `15395bd0…` (SwimWear) and
  `db837788…` (Humanoid), `e36943ca…` (CharaMotion01_Story + 104 ObservableStateMachineTriggers),
  and the full dependency sets of CharaMotion02/09/21 `Game.controller`.
- In the controller blob itself, each `AnimatorControllerLayer` constant carries the *resolved*
  mask: `bodyMask` is the all-ones full-body triple (`[0xFFFFFFFF,0xFFFFFFFF,0x0007FFFF]`) on
  both layers and the per-transform skeletonMask array has **0 entries**, for all four sampled
  characters (Chara01/02/09/21). The original Face Layer is an override layer with NO mask.
- Not every one of the 28,292 bundles was opened; the sets above are exactly where a mask used
  at a seat could be serialized (catalog type table excludes addressable masks; scene tables
  exclude scene-embedded masks).

#### E.2 Write Defaults: serialized per state inside the blob, value TRUE on every state

- `AnimatorController` serializes as `m_Name` + `m_ControllerSize` + an opaque ControllerConstant
  blob + `m_TOS` (name→offset table) + `m_AnimationClips` + `m_StateMachineBehaviourVectorDescription`
  + `m_StateMachineBehaviours` + `m_MultiThreadedStateMachine`. State flags are NOT a named
  TypeTree field; they are 4 packed bools (`ikOnFeet`, `writeDefaultValues`, `loop`, `mirror`) in
  each `StateConstant` inside the blob (AssetStudio name: `m_WriteDefaultValues`, bit after
  `m_IKOnFeet`). The dump tooling's "Write Defaults not serialized" is a tool limitation, not a
  Unity fact — the values ARE there.
- Parse verified by cross-check: 103/103 state `fullPathID` and 2,856/2,856 transition IDs
  resolve through the same file's `m_TOS` (`C:\mfgtmp\CharaMotion01_Game.parsed.json`).
- **CharaMotion01_Game (bundle `00cfdf05…`): all 103 states, base + Face layer, `writeDefaultValues = true`.**
  `CharaMotion02_Game` 103/103 true, `CharaMotion09_Game` 104/104 true, `CharaMotion21_Game`
  119/119 true. Combos (wdx, ikOnFeet, loop) for Chara01: 62 (T,T,T), 28 (T,T,F), 3 (T,F,F),
  10 (T,F,T) — states with `ikOnFeet` OFF exist (13), WDX OFF: none.
- Idle states specifically: `Base Layer.Wait0` / `Base Layer.Wait1` / `Face Layer.Wait0` /
  `Face Layer.Wait1` all loop=true, ikOnFeet=true, wdx=true. Layers: layer 0 base state machine,
  `ikPass = true`, serialized defaultWeight read 0.0; layer 1 "Face Layer", blend mode Override(0),
  defaultWeight 1.0, `ikPass = false`. Both layers weight 1 by default ⇒ the Face override layer
  fully replaces base animation on its (unmasked) bones while active.
- 36 parameters: 33 triggers (Wait0/Wait1/Away1-7/Emotion1-7/EyeContact/Reach0/Reach1/ResultPlus0/1/
  ResultMinus0/1/ResultCritical0/1/Think/Yose/Shy/Anger/Surprise…), `IsReach` bool, `LookDirection`
  int (value index 0), `AwaySpeed` float. `m_MultiThreadedStateMachine = true`.
- `Animator` component on the original CharaModel: `m_WriteDefaultValuesOnDisable = false`
  (both Animator components in `5005fb01…`/`d7359c0b…`), cullingMode 0 (AlwaysAnimate) on the
  controller-bearing root Animator, 1 (CullUpdateTransforms) on the avatar-bearing inner one.

#### E.3 The original seated idle clip vs the reconstruction

- Table controller address `Standing3D/Motion/Chara01/Game.controller` → bundle file
  `00cfdf051286211456a5f38389143582.bundle`, `m_Name = CharaMotion01_Game`, 103 clip PPtrs,
  88 MonoBehaviours + 1 MonoScript (all `UniRx.Triggers.ObservableStateMachineTrigger`;
  `m_StateMachineBehaviourVectorDescription` maps 88 states to behaviours). Costume variants:
  `Game_SwimWear/_Toho/_RaceQueen.overrideController` + `Humanoid_Game.overrideController`
  (AnimatorOverrideControllers, bundles `15395bd0…`/`db837788…`); `ModelManager.MotionPathResolver`
  selects them per costume (`ModelManager.cs:206-351` in the ilspy dump).
- Default seated idle asset: `Assets/MFG/Characters/Character1/3D/Animations/Motion/Default/HiyoriWait.anim`
  (internal id), catalog address `Assets/MFG/Characters/Character1/3D/Animations/HiyoriWait.anim`
  → bundle `c19fb16c562398c57a94ea378bd322f5.bundle` (24,658 B). In a player build the clip's
  readable curve lists are empty; the data is `m_MuscleClip` (`ClipMuscleConstant`, 37,192 B) +
  `m_ClipBindingConstant` with **503 `GenericBinding`s** keyed by 32-bit path hashes: attr 1 ×192
  (position, incl. root), attr 2 ×192 (rotation, incl. root), attr 3 ×119 (scale). The path hash
  is `crc32(path)`; all 192 hashes decode against the reconstruction's bone names (`C:\mfgtmp\hiyoriwait_decoded.json`).
- Decoded leg-chain bindings (attrs): `Hips` (1,2,3); L/R `Thigh` (1,2,3); `ThighTwist` (1,2);
  `HipWeight`+`_end`; `Shin` (1,2,3); `ShinTwist` (1,2); `Knee`+`_end` (Knee 1,2,3); `Foot`
  (1,2,3); `Toe` (1,2,3); `Toe_end` (1,2,3). Full chain position+rotation(+scale on the main
  bones) — the claim "the whole leg chain is animated" holds for the ORIGINAL asset. Plus Hips
  translation (root: `m_HasGenericRootTransform = true`) and 165 non-leg bones (hair/skirt/breast/
  necktie/arms/twists). SampleRate 60; `m_LoopTime = true`; `m_StartTime = 0`, `m_StopTime = 2.0`
  (also on the Swim and Toho variants).
- Count cross-check: reconstruction `…\Motion\Default\HiyoriWait.anim` has 191 position +
  191 euler-rotation + 119 scale curves; the original binds 192 + 192 + 119 including root. Same set.
- Costume variants of the seated idle exist and differ: `3D/Animations/Swim/HiyoriWait.anim`
  (`da622f43…`, 312 bindings: legs rotation+scale but NO leg position curves, Hips 1,2,3) and
  `3D/Animations/Motion/Toho/HiyoriWait_newjoint_toho.anim` (`6f3989de…`, 141 bindings, zero leg
  bindings at all). The reconstruction ships only the Default variant.

#### E.4 The original's leg/IK mechanism: none — clip + UniRx triggers + UTJ springbone (not on legs)

- Full-assembly keyword sweep over the 1,317 ilspy files of `Managed\Assembly-CSharp.dll`:
  **zero** occurrences of TwoBoneIK, CCDIK, IKSolver, OnAnimatorIK (except dead `IKTest.cs`,
  same unreferenced file as the reconstruction), SetIKPosition, SetLayerWeight,
  SetLayerAffectsTiming, SetLookAtPosition, SetBoneIKOverride, LookAtConstraint, AimConstraint,
  PositionConstraint, RotationConstraint, ParentConstraint, HumanBodyBones, AvatarMask, Muscle.
  `Humanoid` appears once (`ModelManager.cs`, the `Humanoid_*` override-controller address),
  `LookAt` once (`MatchingCharaCamera.cs`, a camera transform LookAt).
- Everything the game writes to the seated Animator: `SetInteger("LookDirection")` +
  `SetTrigger` (`ModelManager.PlayMotionMahjong`, `ModelManager.cs:137-145`),
  `SetFloat` ×2 (`ModelManager.cs` AwaySpeed), `CrossFadeInFixedTime` (`PlayCrossFadeInFixedTimeInMahjong`
  `:157-164` + `CharacterModelController.cs`), `animator.Update(0)` flush (×4 in ModelManager) —
  identical to section A. No per-seat weight/mask/IK calls; seat differentiation is only the
  `LookDirection` int parameter consumed by controller transition conditions.
- Seated model prefab `CharaModel.prefab` (address `Standing3D/Costume01_001_Model.prefab`,
  bundle `5005fb018f625f3651ca4ac49779b9fc.bundle`, root GO `CharaModel`, 310 GameObjects):
  1 Animator (m_Avatar = 0, m_Controller = PPtr external FileID 10 — assigned for real at
  runtime by `ModelManager.Init` → `animator.runtimeAnimatorController = controller`,
  `ModelManager.cs:121`), `ModelManager`, `UTJ.SpringManager`, and spring components.
- Spring bones are a **separate assembly**: `Managed\Springbone.Runtime.dll` (UTJ package —
  `UTJ.SpringBone/SpringManager/SpringBonePivot/SpringCapsuleCollider/SpringSphereCollider/
  SpringPanelCollider/WindVolume/ForceVolume/HighLeg`); this is why they never appear in the
  Assembly-CSharp decompile. Instances on the seated prefab (read from `5005fb01…`):
  `SpringManager` on root `Chara` with `automaticUpdates=1, isPaused=0, simulationFrameRate=60,
  dynamicRatio=0.5, gravity=(0,-10,0), enableAngleLimits/enableCollision/enableLengthLimits=1,
  collideWithGround=0`, array of 78 SpringBones; **SpringBone ×78 all on hair/skirt/breast/
  necktie bones** (Yokogami/Maegami/Momi/HairFly/Pony/Ushirogami/Ahoge/Skirt_*/Breast_*/BrestTie/
  NeckTie_*) — zero on Thigh/Shin/Foot/Spine; 49 SpringBonePivots; 3 SpringCapsuleColliders
  (`Spine2CapsuleCollider`, `Thigh_LCapsuleCollider`, `Thigh_RCapsuleCollider`, r=0.09,
  h≈0.197) — colliders that stop skirt/hair from penetrating the seated thighs, they do not
  drive the legs. `UTJ.SpringManager.LateUpdate → UpdateDynamics` runs on its own every frame
  because `automaticUpdates=1`; Assembly-CSharp only ever calls
  `RefleshInitialPositionAndRotation` (`StandingCharaManager.cs:253-257`, standing characters).
  Nothing anywhere calls `UpdateBoneIsAnimatedStates`/`dynamicRatio`/`isPaused` for seat characters.
- So the original's leg motion at the seat is: (1) keyframed leg curves in the 2 s idle clip,
  (2) state machine transitions/crossfades with WriteDefaults ON on all states, (3) UniRx
  `ObservableStateMachineTrigger` state hooks — and nothing else. Same shape as the
  reconstruction, now proven from the original side.

#### E.5 The original avatar: Generic, empty m_Human — reconstruction claim confirmed

- `Hiyori_01Avatar` in `d7359c0b36888bc21890a4b155995999.bundle` (sub-asset of the
  `…/Character1/3D/C001/Fbxs/Hiyori_01.fbx` bundle group): `m_AvatarSize = 28276`,
  `m_HumanDescription.m_Human = []` (empty), skeleton TOS 249 nodes with `Reference/Hips/…`
  paths, `m_GlobalScale = 1.0`, `m_HasExtraRoot = true`; muscle fields (ArmTwist/LegTwist 0.5,
  Leg/ArmStretch 0.05) are the unmodified import defaults on an unused humanoid block. Generic.
  The model Animator in the same bundle references this avatar by pathID; the root Animator's
  m_Avatar stays 0 (avatar reached via child Animator in the prefab hierarchy).

#### E.6 MahjongPlay scene wiring: seats are transform anchors, characters instantiated at runtime

- Build scene list (from `globalgamemanagers` → `BuildSettings`): index 18 =
  `Assets/MFG/Scenes/MahjongPlay/MahjongPlay.unity` → `level18` (23,802,940 B) +
  `sharedassets18.assets` (50 AnimatorControllers, 138 AnimationClips, **0 Avatar, 0 AvatarMask**).
- `level18` census (`C:\mfgtmp\scene18.json`): seat anchors `PlayerZi`, `PlayerKami`,
  `PlayerToi`, `PlayerShimo` (+ `Players`), `Character/3DChara`, `MyChara`, `OtherCharacters*`,
  `PlayerAccessoryParent*`, `CharaSpotLight{Zi,Kami,Toi,Shimo,All}`, `CharaReactionManager`.
  1,228 Animator components in the scene — **every one has `m_Avatar = 0`** (UI/tile/prop
  controllers, several `m_Controller = 0`); no seat has a baked Animator/Avatar/IK component.
  Characters appear as `CharaModel.prefab` instances with the controller assigned by
  `ModelManager.Init`; seat identity only feeds `LookDirection`.
- `StreamingAssets\Characters\Chara1..Chara21\` are `.webm` movie folders only
  (Appearance/GachaSelect/News videos) — no bundles, no 3D content there.

#### E.7 Could not verify / open questions from this pass

- Blend-tree `clipID` values did not round-trip through `m_TOS` in my blob parser, so
  "Wait0's motion is HiyoriWait" was not re-derived from the original controller bytes
  (state name and clip address match the reconstruction's mapping, which was catalog-verified).
- The CharaModel Animator's serialized `m_Controller` PPtr (FileID 10, external CAB) was not
  resolved to a bundle name; the closure contains `CharaMotion01_Story` (`e36943ca…`) but the
  Game controller lives in `00cfdf05…` outside that closure. Runtime assignment
  (`ModelManager.cs:121`) is the effective wiring either way.
- 3 MonoBehaviours in `5005fb01…` have their MonoScript reference in another file; class names
  unresolved.
- Original editor-side clip import metadata (loop-time flags in .meta) does not exist in a
  player build; the serialized `ClipMuscleConstant` flags (loop=true, 2.0 s) were used instead.
- ~350 of 28,292 bundles were opened (sets enumerated in E.1); a mask packed in some unrelated
  bundle outside all sampled dependency closures would not have been seen, but nothing in the
  catalog type table, scene tables, or any controller's resolved layer mask indicates one exists.


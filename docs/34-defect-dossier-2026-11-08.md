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

### Runtime reproduction state

Windows lane, current production webroot stamp, four accounts, one table. What the harness runs so
far established that the LOCAL tier itself is unreliable for a client repro, three separate causes:

1. **Loopback bundle-drop = hard scene hang.** During the boot addressables fan-out Windows
   intermittently kills loopback sockets (`net::ERR_NO_BUFFER_SPACE`). Unity reports it as
   `RemoteProviderException ... ConnectionError, ResponseCode 500` and the dependent load chain
   never completes — the game freezes at whatever scene was loading (observed: pre-Login and at
   NewRegistration, console silent afterwards). The harness XHR shim cannot see this: Unity 2022
   WebGL downloads bundles through `fetch`+blob, so a fetch-level retry shim was added
   (`tmp/webgl_proof.cjs`).
2. **Headed windows freeze when occluded.** With several browser windows the non-focused ones stop
   getting animation frames and their Unity loop freezes mid-scene (screenshots byte-identical for
   minutes while `page.evaluate` still answers). Harness now pins `Page.setWebLifecycleState
   active` via CDP and single-window runs use headless, where the page is always visible.
3. **The harness login preamble was the failing door.** `fetch`-POSTed `/welcome`+`/login` get
   cookie-less POSTs in the puppeteer browser context (`[req] ... cookies=NONE` from the door
   probe), while the identical curl sequence authenticates (`login=302 sid=1 account=200`).
   Symptom: core's "Invalid username or password." page despite correct bodies. Fixed by driving
   the real forms (gate lobby `input[name=code]`, core `input[name=username/password]`) with
   submit + navigation, exactly as a human player does.

Repro loop armed for the next capture: console counts every `Standing CharaManager: Loaded ItemID`
(`StandingCharaManager.cs:249`) between entering Matching and `LoadScene:MahjongPlay` and prints a
`[verdict]` line — fewer than four loads, or a repeated ItemID, is the blank-seat symptom; plus
screenshot bursts every 2 s while in Matching. Awaiting the first clean run to registration → Home
→ match.

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
  any AvatarMasks the dump dropped, the original `Animator.m_Avatar` link) need tooling:
  UnityPy / AssetStudio-MF / AssetGrip over `aa/StandaloneWindows64/*.bundle` + `catalog.json`,
  exporting the same named assets, then diff vs the reconstruction's text dumps; ilspycmd over
  `Managed/Assembly-CSharp.dll` vs `Assets/Assembly-CSharp` to catch dropped members.

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

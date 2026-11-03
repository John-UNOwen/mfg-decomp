# 30. Audio inventory — original-game audio management and usage, documented before fixes

Phase A of the reconstruction audio goal. Ground truth: the original shipped client (`\\MYAPPSANDBOX\c\Games\MFG\game\MFGClient_Data`, extraction recorded in `docs/20-content-extraction.md`), the local dumps (`build/export-primary-content`, `build/unity-project-auxiliary/GameAssemblies`), and the decompiled working tree `build/unity-project`. Nothing in this document was inferred from filenames alone; every claim traces to a parsed table, a code line, or a counted scan. Audit tooling lives in `scripts/audio/` (`parse_labelsettings.py` regenerates the parsed label-table digest `scripts/audio/labelsettings_parsed.txt`; `scan_yaml_audio.py`, `code_label_gap.py`).

## 1. Architecture — how the original drives audio

- **Everything is label-driven.** No scene or prefab in the original carries an `AudioSource` or an AudioClip reference: a full scan of 3,132 scene/prefab YAMLs found zero `!u!82` components and zero clip GUID references. Audio exists only as labels in a shipped binary table plus code that plays them.
- **The engine is Konami's USnd layer** (`Assets/Plugins/Assembly-CSharp-firstpass/USnd/AudioManager.cs`, 5,613 lines). It loads three binary tables from `Assets/Resources/sound/xml/`: `LabelSettings.bytes` (2.79 MB, version 3, **10,433 labels**), `CategorySettings.bytes` (5 categories), `MasterSettings.bytes` (3 masters). Playback uses pooled AudioSources; `audioSource.loop` is set from the table's loop flag; fades, per-label playInterval re-trigger gates, category max-playbacks, ducking, pan, and random source selection are all table-driven. Clips are fetched on demand through Addressables as `"<ClipName>.wav"`.
- **Masters/mixes:** masters BGM 0.2 / SE 0.2 / VOICE 0.3; categories BGM (maxPlaybacks 1), SE, SE2, COIN, VOICE. Category volumes map to mixer groups on `Assets/Resources/sound/VirtualSurround.mixer` (Gain1..8 per cabinet speaker channel — exposed by commit `be37747d87`; the cabinet-only `VirtualSurroundNative.dll` is inert on WebGL by design).
- **The façade is `SoundManager`** (`Assets/Assembly-CSharp/SoundManager.cs`, 654 lines): `PlayBgm`/`StopBgm`/`StopBgmWithFadeoutDelay`, `PlaySe`, `PlayVoice`, `PlayVoiceCha` (pan per seat: Jicha/Simocha/Toimen/Kamicha), `PlayMyCharaMyRoomBGM`, `PlayMyCharaVoice`, `PlayAllCharaVoice`, `StopAll`, volume presets `SetDemoVolume`/`SetGamePlayVolume`/`SetSpeakerVolume`/`SetHeadPhoneVolume`/`SetAttractVolume`. A missing label logs `SE:<label> is not found!`. It initializes at boot: `ResidentFlow.cs:590 → SoundManager.Initialize(m_audioMixer, IsCloudStreaming)`, mixer bound in `Resident.unity:17339`.
- **Situation layers:** `VoiceManager` (mahjong call/comment selection), `KyokuCall` (per-hand calls + voices), `NakiCutin`/`ReachCutin`/`AgariCutin` + `SoundCall` (cheer SE families), `CoinSoundPlayer`, `ScorePanelSoundUtility`, `ScrambleFestivalSEManager`, `MyRoomCustomizeWindow`/`MusicGachaPlay` (BGM_THUMB previews), `StoryPlay` (its own PlayBgm/PlaySe/PlayVoice).
- **Call-site counts:** 34 `PlayBgm` sites; 769 `PlaySe`/`PlayVoice` sites across 199 files.

## 2. Catalog — what audio exists

Tree export == shipped catalog, exactly: **10,363 clips** (10,362 in bundles + `SFX_MJG_DISPLAY1` in resources.assets), names bit-identical to the shipped exports (no transcode on export). All clips are Vorbis-compressed .ogg with the original bundle hash preserved in import settings. There is **no ambience category** in the original.

| Family | Clips | Location | Notes |
|---|---|---|---|
| BGM | 751 | `Assets/MFG/Sound/BGM` | see breakdown below |
| SFX | 305 | `Assets/MFG/Sound/SFX` | UI + mahjong system sounds |
| VOX | 9,306 | `Assets/MFG/Characters/Character1..21/Sound` (419–467 each) | 21 characters, `VOX_G01..G21_<CONTEXT>` |

**BGM (751 clips / 781 labels; 659 loop=true):** `BGM_TITLE` (loop=FALSE by original design — one-shot), `BGM_MENU`, `BGM_HOME` + 71 HOME variants (per-character `BGM_HOME_BM1_G01..G12`, seasonal XMAS/NEWYEAR/TOHO/QMA/KAC), `BGM_MOVIE_A/B` (title PV), `BGM_CASTING` (+`_CM`), `BGM_GACYA`, `BGM_RESULT1..5`, `BGM_GAMEOVER`, `BGM_REACH3`, per-hand mahjong sets (`BGM_2MJG1_*`, `BGM_3MJG1/2_*`, `BGM_4MJG1..4MJG7`), per-character reach sets (`BGM_G01..G21_REACH_*`), 287 `BGM_THUMB_*` music-gacha previews.

**SFX (305 clips / 304 labels: SE 300 + SE2 1 + COIN 3):** click/confirm `SFX_SYS_INPUT1/2`, cancel `SFX_SYS_CANCEL1/2`, `SFX_SYS_COIN`, `SFX_PASELI_OK/ERROR`, `SFX_KONAMI` (logo), `SFX_EVT_BINGO*`, `SFX_CHECK`, `SFX_LOUDSOFT`, mahjong `SFX_MJG_CALL*/YAKU/RANK/DISPLAY*`, character-flavored cheers `SFX_MJG_CALL3_SP_G01..G12` + ~50 `SFX_MJG_CALL3_SR_<theme>`. 5 SE labels carry loop=true.

**VOX (9,306 clips / 9,348 labels; all loop=false):** occasions visible in the label grammar — mahjong play (`MJG_DAHAI/PON/CHII/RAI/Tsumo/Ron/TONG/NANG/HONBA/REACH/YAKU/RESULT`), flow (`CONTINUE/DEBUT/VICTORY`), MyRoom interaction (`TOUCH/APPEAL/FAVOR`), general (`GNL_*`), seasonal (`APRIL/XMAS`). Seat-variant suffixes (`_G1..G5`) exist in code; table-side they live behind the 3 randomSource labels.

**Table digest:** full parsed dump regenerable via `python scripts/audio/parse_labelsettings.py` (per-label loop/fade/category/randomSource rows; 10,393 distinct clipNames).

## 3. Occasion map — which scene plays what (code evidence)

| Scene / flow | Audio | Site |
|---|---|---|
| Title | `BGM_TITLE` (one-shot), `VoiceManager.PlayVoiceTitle()` | `TitleFlow.cs:208/:212` |
| Title PV movies | `BGM_MOVIE_A/B` via BGMSelecter; StopBgm on movie end | `TitleFlow.cs:630/:651/:671` |
| Logo | `SFX_KONAMI` only; `SetDemoVolume` | `LogoFlow.cs:106/:78` |
| Login / card entry | `BGM_MENU` | `CardEntryScene.cs:205`, `NewRegistrationFlow.cs:77` |
| Home / MyRoom / Dojo / Guide / GachaSelect | `PlayMyCharaMyRoomBGM()` → equipped `Category.MyRoomBgm` item label, default `MyRoomBgm001 = "BGM_HOME"` | `HomeFlow.cs:294` + flows; `ItemIDExtentions.cs:1955`; `SoundManager.cs:503-506` |
| Matching | `BGM_CASTING` (`_CM` variant) | `MatchingFlow.cs:158-163` |
| MahjongPlay | per-kyoku `GetBgm()` → `BGM_2/3/4MJG*`; calls/voices `VOX_G*_MJG_TONG/NANG/HONBA` | `KyokuCall.cs:116-118` |
| Agari / reach | `BGM_RESULT1`, `BGM_REACH3`, per-character REACH labels + `SFX_MJG_CALL3_*` cutins | `Agari`/`ExecAgari`, `Naki/Reach/AgariCutin` |
| Result / Continue / Gameover / EventResult | `BGM_RESULT3` / continue label / `BGM_GAMEOVER` / `BGM_RESULT5` | `ResultFlow.cs:158`, `ContinueFlow.cs:129`, `GameoverFlow.cs:44`, `EventResultFlow.cs:43` |
| GachaPlay | `BGM_GACYA` | `GachaPlayFlow.cs:106` |
| Music gacha / MyRoom BGM customize | `BGM_THUMB_*` previews | `MusicGachaPlay`, `MyRoomCustomizeWindow` |
| StoryPlay | PlayBgm/PlaySe/PlayVoice per script | `StoryPlay` |

AudioListener exists in 25 scenes; `AudioMixerSnapshot` components exist in `Title.unity:13107/13143`, `Home.unity:110128`, `GachaSelect.unity:2005821` (volume snapshots, e.g. attract vs gameplay).

## 4. What the lane plays today, and the suspects (updated 2026-11-03 by runtime + data probes)

Audible now: `BGM_TITLE` (title) and `BGM_MENU` (login/menu) — both from original code paths, no harness audio code involved. Everything deeper is wired and present but not heard. Probe results:

1. **~~lane-data gap~~ — FALSIFIED by savedata probe.** Every lane profile that has ever played already owns `ItemID 349` (`MyRoomBgm001`, plus 349-353) in `customize_item` and carries `_customizeItemEquips` with `MyRoomBgm=MyRoomBgm001` on all 21 charas (mfgowner/test/vicyush quoted in the storage-probe report). The IndexOutOfRange precondition (`_value:""` equip + zero owned) exists in no profile. The client even auto-adds defaults at `CharaGameData.cs:207-213`.
2. **Build re-encodes Vorbis→AAC.** Unity's WebGL pipeline ignores the importer settings (`compressionFormat: 1` Vorbis, `quality: 0`, no platform overrides in the metas) and stores clips as AAC: bundle CAB resources start `ftypmp42`, runtime `m_CompressionFormat=7`. HOME 5.66 MB→1.10 MB, TITLE 1.87 MB→0.38 MB, APPEAL1 46 KB→40 KB. Quality-0 AAC is aggressive: plausible source of audible degradation, and AAC encoder/decoder priming adds a gap at loop seams that Vorbis does not. This is the strongest candidate for "loops wrong".
3. **New suspect from storage probe:** `HomeFlow.cs:286-299` calls `PlayMyCharaMyRoomBGM().Forget()` only after `OnInitializeBeforeIntroduction` completes; the flow runs as `OnExecute(...).Forget()` with an `OperationCanceledException`-only catch (`BaseFlow.cs:55/61`), so any other exception during Home init silently skips the BGM call. Proof-run console shows such exceptions do occur on this build (boot `DriveInfo NotSupportedException` il2cpp icall at 73 s; `TitleFlowMoveNext: OperationCanceledException Catched.` at 82 s).
4. **Category BGM maxPlaybacks=1 + StopOldInstance semantics** on fast transitions; proof run shows Title→Login in 1 s and three `GET blob:` `ERR_ABORTED` at the Title→Login jump — mid-fetch cancellation of streamed content is real behavior on this lane.
5. Mixer plumbing confirmed live at runtime: `Exposed name does not exist: Gain1..4` warnings fire per scene (Logo/Title/Login) exactly where `SetDemoVolume`/presets call into the mixer (expected, by design).

## 4a. Runtime observation (headless proof 2026-11-03, `tmp/audio_proof.log.txt`)

Fresh throwaway account (gate lobby unlock + signup + cookie injection): Logo 72s → Title 80s → Login 81s → NewRegistration 92s. **The old "fresh signup stuck at Logo" defect no longer reproduces**; the walk now stalls at NewRegistration (needs name entry; AUTOSERVE does not type). 12 `/aog` front hits; zero `SE:<label> is not found!` entries; zero label-table load errors.

## 5. Known gaps (most are defects the original cabinet shares)

- **40 table labels reference clips that exist in neither the tree nor the shipped catalog** — the real cabinet logs "not found" for these too: `SFX_SYS_TOJO2`, `VOX_G0{1,2,4,5,7}_VICTORY1/2`, `VOX_G01/G05_MJG_NANG4`, `VOX_G05_MJG_RESULT15-20`, `VOX_G05/G07_MJG_YAKU51-54`, `VOX_G07_MJG_RESULT1-4/13-20`, `VOX_G13_GNL_SUPRISE1` (full list in `scripts/audio/labelsettings_parsed.txt`).
- **~46 complete code-only label literals** (seat variants `VOX_G03_MJG_*_G1..G5`, `SFX_MJG_CHEER2`, fragments like `SFX_MJG_RANK{n}`): most likely resolve at runtime through randomSource expansion; verification items, not confirmed gaps.
- **9 orphan clips** in the tree referenced by no label (`BGM_3MJG1_2/5`, `BGM_REACH2`, `SFX_MJG_PICK3`, `SFX_MJG_TILE7/8`, `VOX_G16_GET`, `VOX_G18_MJG_DAHAI4_6b/6c`) — probably randomSource pool members or dead content in the original.
- WebGL staged bundles 28,258 vs shipped 28,292 — bookkeeping delta only; all 10,362 audio keys verified present in the staged WebGL catalog and on disk.

## 6. Verification status

Verified: tree==shipped inventory (10,363/10,363, bit-identical); label table parsed and consistent (10,433 labels); Addressables plumbing covers every clip; zero serialized audio wiring (authentic); mixer binding live and exercised at runtime (per-scene Gain warnings); lane accounts own+equip MyRoomBgm001 (savedata probe); WebGL build stores clips as AAC re-encodes (`tmp/cab_compare.py` evidence); fresh-signup walk reaches NewRegistration. Not yet verified (Phase B): whether the BGM call fires at Home (proof2 telemetry pending), audible effect of AAC re-encode/loop seam (needs owner ears or instrumented build), randomSource expansion of the code-only labels, and cabinet audio A/B (no cabinet recordings yet — cabinet-only unknowns flagged, not guessed).

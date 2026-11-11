# BGM playback-site audit + L105 loop-exposure map (2026-11-09)

Read-only audit of the reconstruction's BGM wiring, in service of ledger row **L105** (`docs/31-open-defect-ledger.md:114`, major, OPEN; RCA in `docs/37-music-loop-rca-2026-11-09.md`). Question asked: for **every** BGM playback site in the decompiled client - literal labels, interpolated/variable labels, ScriptableObject-borne labels, table-driven story rows - does the label exist in the shipped label table, does the clip it names exist in the served WebGL content, and which loop path does that clip take? Result feeds the L105 fix decision (force ≥128 KB BGM onto the `decompress=1` buffer path, or pre-trim/re-pad the AAC).

Nothing under the repo, Unity project, or shipped game was modified. Scripts and intermediate dumps: `C:\mfgtmp\ost_audit\`.

## TL;DR

**No broken wiring.** All 781 BGM-category labels exist in the shipped table, all 748 distinct clips they name are present in the served WebGL bundles, and all 53 playback sites resolve. The reconstruction's table files are **byte-identical to the shipped TextAssets**, so every table fact below transfers to the original 1:1.

**Exposure is near-total.** Splitting the 748 clips at the served glue's `length<131072` force-decompress threshold: **395 clips / 353.5 MB AAC / 507.0 min of content on the `HTMLAudioElement` path**, 353 clips / 37.0 MB / 52.6 min on the sample-accurate `decodeAudioData` path. 392 of the 395 are `loop=true`; **389 are lane-reachable**, i.e. **500.8 minutes of looping music share the L105 seam mechanism**. The threshold reproduces across the whole sample with **no contradiction**, and the two buckets do not overlap the threshold by even one byte.

Findings worth a decision, none of them wiring defects: 4 dead BGM labels and 3 dead-but-served source files (≈5.26 MB of AAC no live label can play), deliberate label→clip aliasing in the original table (`BGM_3MJG1_2` plays `BGM_4MJG1_2`), 34+12 empty label slots in the kyoku/event-taku BGM arrays that reach `PlayBgm("")`, an unguarded `[1]` index in `SoundManager.cs:554` that throws on any music gacha whose second item is not a `ReachBgm` item, and one Home-scene debug sound-test window that would play *any* of the 10,433 labels but is force-hidden and unwired. Detail in §5.

---

## 1. Method and verification anchors

| anchor | how verified |
|---|---|
| label/category/master tables | parsed `LabelSettings.bytes`/`CategorySettings.bytes`/`MasterSettings.bytes` (recon) **and** the shipped TextAssets extracted from the original bundles: all three files byte-identical (`t1_tables.py` → `identical=True` ×3). 10,433 labels, 5 categories, 3 masters, `labelVer=3` |
| BGM-family counts | 781 labels in category `BGM`; 748 distinct `clipName` values; 659 `loop=true` / 122 `loop=false`; fan-out 715 clips ×1 label + 33 clips ×2 labels = 781 |
| clip presence + size | UnityPy over the **served** bundles `C:\mfgtmp\webroot\StreamingAssets\aa\WebGL\*.bundle`, resolved through the addressables catalog (`recon_catalog_manifest.tsv`): every clip read as an `AudioClip` with `m_CompressionFormat=7`, container magic `00000018 66747970 6d70 3432` (`ftypmp42`), `m_Resource.m_Size` = the AAC container length Unity hands to the JS loader (`t5_sizes.py` → `aac_sizes.tsv`, 748 rows) |
| pipeline cross-check | the same extractor reproduces docs/37's independently measured containers exactly: `BGM_CASTING` 201,974 B, `BGM_G07_REACH_7` 435,682 B ("436 KB" in the RCA), `BGM_HOME` 1,100,734 B, `BGM_TITLE` 376,337 B ("0.38 MB") |
| content duration | Vorbis total-sample granule from the shipped oggs (`C:\mfgtmp\music_rca\clips.tsv`), not the container's frame-rounded duration |
| loop path | the served glue `C:\mfgtmp\webroot\Build\webgl.framework.js`, verbatim: `function _JS_Sound_Load(ptr,length,decompress,fmodSoundType){…var audioData=HEAPU8.buffer.slice(ptr,ptr+length);if(length<131072)decompress=1;…}` → `decompress` branch = `jsAudioCreateUncompressedSoundClipFromCompressedAudio` → `decodeAudioData` → `AudioBufferSourceNode`; else `jsAudioCreateCompressedSoundClip` → `new Blob([audioData],{type:"audio/mp4"})` + `mediaElement:new Audio` |
| sites | regex enumeration over `Assets\Assembly-CSharp` (`SoundManager.(PlayBgm|PlayMyCharaMyRoomBGM|PlayMusicGachaBGM)\s*\(`) then each hit read in context; label sets resolved through `ItemIDExtentions.cs` dictionaries, `EventTakuMaster` data, `KyokuCall.cs` arrays, `FightGirlNews` ScriptableObjects, the 218 story CSVs, and `MovieSoundList` rows |

Serialized-column nuance (do not reuse naively): for these streamed clips `AudioClip.m_Length` carries **seconds**, not samples (`BGM_HOME` → 90, `BGM_CASTING` → 16). Durations in this report are Vorbis granule seconds.

Loop semantics that bound the blast radius, from the same table: all 781 BGM labels have `maxPlaybacksNum=1`, `isStealOldest=false`, `fadeIn=fadeOut=0`, `playInterval=0`, `playStartDelay=0`, `isPlayLastSamples=false`, `isRandomPlay=false` - verified across all 781 rows, so no BGM label re-triggers, cross-fades, or double-plays by table policy; `SoundManager.cs:516-520` short-circuits a re-request of a label that is already playing (`IsPlay` → `return 0`), which is why none of the sites below can stack or restart a BGM.

## 2. L105 exposure map

`loop=true` is the only thing that makes a clip L105-exposed: a one-shot on the media path just ends (`StopBgm`/scene exit handles it), while a looping one restarts through the browser's media-element wrap that docs/37 measured (container has no edit list, 372-sample priming, 304-sample tail truncation, ~12 ms Chrome pipeline gap).

| bucket | clips | served AAC | content | of which `loop≠false` | lane-reachable & looping |
|---|---:|---:|---:|---:|---:|
| media (`≥131,072 B`, HTMLAudioElement) | 395 | 353.5 MB | 507.0 min | 392 clips / 504.6 min | **389 clips / 500.8 min** |
| buffer (`<131,072 B`, AudioBufferSourceNode) | 353 | 37.0 MB | 52.6 min | 262 clips / 43.6 min | 262 clips |
| all BGM | 748 | 390.5 MB | 559.6 min | — | — |

Boundary facts, all measured on the served containers:

- **No clip sits at or near the threshold.** Largest buffer-path clip `BGM_STORY7` 130,039 B (10.576 s loop); smallest media-path clip `BGM_STORY6` 153,420 B (12.515 s loop). 1,033 B of clearance below the constant, 22,348 B above - no borderline flip risk in this build (they are adjacent story tracks, so a re-encode of either could move it).
- Only **3 media-path clips are one-shots** and therefore not exposed: `BGM_TITLE` 376,337 B (30.859 s), `BGM_MOVIE_A` 1,096,189 B (89.960 s), `BGM_MOVIE_B` 245,751 B (20.108 s).
- Longest exposed loop `BGM_HOME_BM2_G10` 1,670,825 B / 137.207 s; shortest `BGM_STORY6` 153,420 B / 12.515 s. `BGM_CASTING` (the reported case) sits mid-pack at 201,974 B / 16.508 s.
- **The re-encode, not the source, decides the bucket.** All 353 buffer-path clips are ≥131,072 B as shipped ogg (the `.ogg`→AAC shrink is what drops them below the threshold); nothing goes the other way. `docs/37` §5's exposed examples (HOME 1.10 MB, MENU 271 KB, G07_REACH_7 436 KB, CASTING 202 KB) all land in the media bucket here.
- 28 clips carry **mixed** loop flags (the `BGM_THUMB_G0x_REACH_[6-9]` previews: base label `loop=true`, the `…G` variant `loop=false`) - same container, two behaviours; all 28 are in the buffer bucket, so no ambiguity reaches the media path.
- **Route-level split.** The routes players actually live in are on the exposed side: MyRoom/Home/Dojo/GachaSelect BGM (85 labels, 560,501–1,670,825 B) and the reach-cutin BGM (235 labels, 239,213–1,557,202 B) are **100% media path**, as is the kyoku BGM (49 clips) and story BGM (44 of 45). Every short preview route is on the immune side: the 235 `BGM_THUMB_*` previews (122,736–124,063 B), the 85 MyRoom previews, the 28 `…G` variants, the 68 reach jingles and the 21 `*_REACH_5` per-character stingers are all buffer path. So a fix aimed at the media bucket touches the long-form music only, never the 10-second previews.
- Only 3 clips are **not** lane-reachable: `BGM_3MJG_KAC1`, `BGM_4MJG_KAC1`, `BGM_RESULT_KAC1` (dead labels, §5.1). The `*_KAC1` *room* variants (`BGM_HOME_KAC1`, `BGM_THUMB_HOME_KAC1`) are lane-reachable through the MyRoom BGM item route.

Runtime corroboration of the split (2026-11-03 loop telemetry: `C:\mfgtmp\aud_loop_2026-11-03T19-16-01-222Z.json`, `C:\mfg\tmp\audio_proof2_audtel_2026-11-03185452.json`): three blob-URL `AUDIO` elements with durations **30.859297 s / 22.151814 s / 90.348821 s** - matching `BGM_TITLE` (content 30.85932 s), `BGM_MENU` (22.14678 s, container 22.15181 s) and `BGM_HOME` (90.338413 s, container 90.348821 s), i.e. the three biggest clips routinely played before gameplay, all on the media path; the HOME element recorded `wraps: 2` (it looped) and the proof2 run logs successive `loopset` writes of `loop:false` then `loop:true` on its two blob elements. The same run counted 191 `decodeAudioData` calls for the small clips. Buffer-path **BGM** specifically is the one leg not attributed by name at runtime (see §6).

The full map, all 748 clips sorted by served AAC size (bucket, loop flag, label count, content duration, lane reachability), is Appendix A; machine-readable copy `C:\mfgtmp\ost_audit\l105_exposure_final.tsv`.

## 3. Playback sites

Enumeration reconciles with the docs/30 claim of "34 `PlayBgm` sites" (`docs/30-audio-inventory.md:12`): the literal text `PlayBgm(` occurs 34 times in the tree = 29 call expressions in game code + 5 in `SoundManager.cs` (definitions at :508 and :572, and the three internal calls at :543, :554, :576). My site table counts the 29 call expressions, the 17 `PlayMyCharaMyRoomBGM()` and 6 `PlayMusicGachaBGM()` wrappers that reach them, plus one BGM-family label played through the SE façade - 53 rows.

Columns: `labels` = resolvable label values at that site; `loop T/F` = labels flagged loop true/false; `min–max` = AAC container bytes across the site's clips; `med/buf` = clips on each loop path; `L105` = clips that are media-path **and** looping; `verdict` = `correct` / `correct-at-risk` (wiring fine, exposed or hazardous) / `unreachable-on-lane`.

| # | site | route | label set | labels | loop T/F | clips | AAC min–max B | med/buf | L105 | verdict |
|---:|---|---|---|---:|---|---:|---|---|---:|---|
| 1 | `TitleFlow.cs:208` | literal | `TITLE` | 1 | 0/1 | 1 | 376337 | 1/0 | 0 | correct |
| 2 | `TitleFlow.cs:630` | TitleFlow.BGMSelecter(false) | `MOVIE_PV` | 2 | 0/2 | 2 | 245751–1096189 | 2/0 | 0 | correct |
| 3 | `TitleFlow.cs:651` | TitleFlow.BGMSelecter(true) | `MOVIE_PV_NEWCHARA` | 1 | 0/1 | 1 | 245751 | 1/0 | 0 | correct |
| 4 | `CardEntryScene.cs:205` | literal | `MENU` | 1 | 1/0 | 1 | 270715 | 1/0 | 1 | correct |
| 5 | `NewRegistrationFlow.cs:77` | literal | `MENU` | 1 | 1/0 | 1 | 270715 | 1/0 | 1 | correct |
| 6 | `ContinueFlow.cs:129` | literal (awaited under UniTask.WhenAny 5s cap) | `RESULT3` | 1 | 1/0 | 1 | 286765 | 1/0 | 1 | correct |
| 7 | `ResultFlow.cs:158` | literal | `RESULT3` | 1 | 1/0 | 1 | 286765 | 1/0 | 1 | correct |
| 8 | `MissionEventAcquireWindow.cs:50` | literal | `RESULT3` | 1 | 1/0 | 1 | 286765 | 1/0 | 1 | correct |
| 9 | `EventResultFlow.cs:43` | literal | `RESULT5` | 1 | 1/0 | 1 | 100753 | 0/1 | 0 | correct |
| 10 | `GameoverFlow.cs:44` | literal | `GAMEOVER` | 1 | 0/1 | 1 | 118569 | 0/1 | 0 | correct |
| 11 | `GachaPlayFlow.cs:116` | literal | `GACYA` | 1 | 1/0 | 1 | 406609 | 1/0 | 1 | correct |
| 12 | `Agari.cs:695` | literal | `RESULT1` | 1 | 1/0 | 1 | 178586 | 1/0 | 1 | correct |
| 13 | `ScorePanelSoundUtility.cs:41` | literal | `RESULT1` | 1 | 1/0 | 1 | 178586 | 1/0 | 1 | correct |
| 14 | `ScorePanel.cs:360` | interpolated $BGM_RESULT{(isWorst?4:2)} | `RESULT2_OR_4` | 2 | 2/0 | 2 | 113590–251690 | 1/1 | 1 | correct |
| 15 | `MatchingFlow.cs:163` | local var label (CM gated) | `CASTING` | 2 | 2/0 | 2 | 201974–435682 | 2/0 | 2 | correct |
| 16 | `ExecAgari.cs:245` | literal | `REACH3_JINGLE` | 1 | 0/1 | 1 | 56929 | 0/1 | 0 | correct |
| 17 | `ExecAgari.cs:250` | local text = ReachBgmCustomJingle(item) | `REACH_JINGLES` | 68 | 0/68 | 68 | 34349–110373 | 0/68 | 0 | correct |
| 18 | `ExecAgari.cs:254` | GetCharaVoiceLabel("BGM_GXX_REACH_5", chara) | `REACH5_PERCHARA` | 21 | 0/21 | 21 | 61759–114330 | 0/21 | 0 | correct |
| 19 | `KyokuCall.cs:117` | GetBgm(takuType,ba,kyoku) arrays + EventTakuMaster | `KYOKU_TAKU` | 51 | 51/0 | 49 | 753702–1667756 | 49/0 | 49 | correct |
| 20 | `HomeFightGirlNews.cs:214` | Master.BgmLabel (ScriptableObject) | `FGN_MUSICGACHA_BGM` | 7 | 7/0 | 7 | 324025–435682 | 7/0 | 7 | unreachable-on-lane |
| 21 | `HomeFightGirlNews.cs:271` | MovieSoundList[].Label where IsBGM==1 | `FGN_MOVIE_STINGERS` | 3 | 0/3 | 3 | 77316–94599 | 0/3 | 0 | correct |
| 22 | `MFG.MahjongPlay.Cutin/CutinManager.cs:248` | GetCharaVoiceLabel(itemID.ReachBgmToBgmLabel(), charaType) | `REACHBGM_LABELS` | 235 | 235/0 | 235 | 239213–1557202 | 235/0 | 235 | correct |
| 23 | `MusicGachaPlay.cs:221` | itemID.ReachBgmToThumbBgmLabel() | `THUMB_LABELS` | 235 | 235/0 | 235 | 122736–124063 | 0/235 | 0 | correct |
| 24 | `FavoAchievementList.cs:70` | ReachBgmFromFavoOID(..).ReachBgmToThumbBgmLabel() | `THUMB_FAVO` | 85 | 85/0 | 85 | 122745–124063 | 0/85 | 0 | correct |
| 25 | `GachaVoiceGetList.cs:54` | itemID.ReachBgmToThumbBgmLabel() + "G" | `THUMB_G_GACHA` | 28 | 0/28 | 28 | 122769–123957 | 0/28 | 0 | correct |
| 26 | `MyRoomCustomizeWindow.cs:288` | itemID2.ReachBgmToThumbBgmLabel() | `THUMB_LABELS` | 235 | 235/0 | 235 | 122736–124063 | 0/235 | 0 | correct |
| 27 | `MissionEventAcquireWindow.cs:45` | info.GetMusicLabel(preview) via PlaySe (+StopSe :49) | `PREVIEW_MYROOM_OR_THUMB` | 259 | 259/0 | 259 | 121159–124063 | 0/259 | 0 | correct |
| 28 | `MFG.StoryPlay/PlayBgm.cs:16` | story script CSV column 3 (targetName) | `STORY_CSV` | 45 | 45/0 | 45 | 130039–1577660 | 44/1 | 44 | correct |
| 29 | `SoundButton.cs:34` | label set at runtime by SoundTest.Init(AudioManager.GetLabelNameList()) | `ALL_TABLE_LABELS` (all 10,433, all categories) | — | — | — | — | — | — | unreachable-on-lane |
| 30 | `TestModeSkeleton/SoundOptions.cs:208` | literal (service/test sound menu) | `HOME_TEST` | 1 | 1/0 | 1 | 1100734 | 1/0 | 1 | unreachable-on-lane |
| 31 | `DojoFlow.cs:421` | SoundManager.PlayMyCharaMyRoomBGM() -> MyRoomToMyRoomBgmLabel(equipped) | `MYROOM_BGM` | 85 | 85/0 | 85 | 560501–1670825 | 85/0 | 85 | correct |
| 32 | `EventStartFlow.cs:25` | SoundManager.PlayMyCharaMyRoomBGM() -> MyRoomToMyRoomBgmLabel(equipped) | `MYROOM_BGM` | 85 | 85/0 | 85 | 560501–1670825 | 85/0 | 85 | correct |
| 33 | `FirstCharaSelectFlow.cs:51` | SoundManager.PlayMyCharaMyRoomBGM() -> MyRoomToMyRoomBgmLabel(equipped) | `MYROOM_BGM` | 85 | 85/0 | 85 | 560501–1670825 | 85/0 | 85 | correct |
| 34 | `GuideFlow.cs:146` | SoundManager.PlayMyCharaMyRoomBGM() -> MyRoomToMyRoomBgmLabel(equipped) | `MYROOM_BGM` | 85 | 85/0 | 85 | 560501–1670825 | 85/0 | 85 | correct |
| 35 | `HomeFlow.cs:294` | SoundManager.PlayMyCharaMyRoomBGM() -> MyRoomToMyRoomBgmLabel(equipped) | `MYROOM_BGM` | 85 | 85/0 | 85 | 560501–1670825 | 85/0 | 85 | correct |
| 36 | `HomeFightGirlNews.cs:288` | SoundManager.PlayMyCharaMyRoomBGM() -> MyRoomToMyRoomBgmLabel(equipped) | `MYROOM_BGM` | 85 | 85/0 | 85 | 560501–1670825 | 85/0 | 85 | correct |
| 37 | `HomeFightGirlNews.cs:307` | SoundManager.PlayMyCharaMyRoomBGM() -> MyRoomToMyRoomBgmLabel(equipped) | `MYROOM_BGM` | 85 | 85/0 | 85 | 560501–1670825 | 85/0 | 85 | correct |
| 38 | `MFG.Scenes.GachaSelect/GachaSelectFlow.cs:308` | SoundManager.PlayMyCharaMyRoomBGM() -> MyRoomToMyRoomBgmLabel(equipped) | `MYROOM_BGM` | 85 | 85/0 | 85 | 560501–1670825 | 85/0 | 85 | correct |
| 39 | `MyRoomCharaSelectWindow.cs:54` | SoundManager.PlayMyCharaMyRoomBGM() -> MyRoomToMyRoomBgmLabel(equipped) | `MYROOM_BGM` | 85 | 85/0 | 85 | 560501–1670825 | 85/0 | 85 | correct |
| 40 | `MyRoomFlow.cs:71` | SoundManager.PlayMyCharaMyRoomBGM() -> MyRoomToMyRoomBgmLabel(equipped) | `MYROOM_BGM` | 85 | 85/0 | 85 | 560501–1670825 | 85/0 | 85 | correct |
| 41 | `MyRoomFlow.cs:218` | SoundManager.PlayMyCharaMyRoomBGM() -> MyRoomToMyRoomBgmLabel(equipped) | `MYROOM_BGM` | 85 | 85/0 | 85 | 560501–1670825 | 85/0 | 85 | correct |
| 42 | `MyRoomFavoWindow.cs:154` | SoundManager.PlayMyCharaMyRoomBGM() -> MyRoomToMyRoomBgmLabel(equipped) | `MYROOM_BGM` | 85 | 85/0 | 85 | 560501–1670825 | 85/0 | 85 | correct |
| 43 | `MyRoomCustomizeWindow.cs:128` | SoundManager.PlayMyCharaMyRoomBGM() -> MyRoomToMyRoomBgmLabel(equipped) | `MYROOM_BGM` | 85 | 85/0 | 85 | 560501–1670825 | 85/0 | 85 | correct |
| 44 | `MyRoomCustomizeWindow.cs:293` | SoundManager.PlayMyCharaMyRoomBGM() -> MyRoomToMyRoomBgmLabel(equipped) | `MYROOM_BGM` | 85 | 85/0 | 85 | 560501–1670825 | 85/0 | 85 | correct |
| 45 | `MyRoomCustomizeWindow.cs:382` | SoundManager.PlayMyCharaMyRoomBGM() -> MyRoomToMyRoomBgmLabel(equipped) | `MYROOM_BGM` | 85 | 85/0 | 85 | 560501–1670825 | 85/0 | 85 | correct |
| 46 | `MusicGachaPlay.cs:225` | SoundManager.PlayMyCharaMyRoomBGM() -> MyRoomToMyRoomBgmLabel(equipped) | `MYROOM_BGM` | 85 | 85/0 | 85 | 560501–1670825 | 85/0 | 85 | correct |
| 47 | `FavoAchievementList.cs:74` | SoundManager.PlayMyCharaMyRoomBGM() -> MyRoomToMyRoomBgmLabel(equipped) | `MYROOM_BGM` | 85 | 85/0 | 85 | 560501–1670825 | 85/0 | 85 | correct |
| 48 | `MFG.Scenes.GachaSelect/GachaSelectFlow.cs:304` | SoundManager.PlayMusicGachaBGM(info) -> ReachBgmToBgmLabel(sorted music item[1]) | `GACHA_MUSIC_BGM` | 28 | 28/0 | 28 | 323473–1101616 | 28/0 | 28 | correct-at-risk |
| 49 | `MFG.Scenes.GachaSelect/GachaSelectFlow.cs:373` | SoundManager.PlayMusicGachaBGM(info) -> ReachBgmToBgmLabel(sorted music item[1]) | `GACHA_MUSIC_BGM` | 28 | 28/0 | 28 | 323473–1101616 | 28/0 | 28 | correct-at-risk |
| 50 | `GachaVoiceListWindow.cs:31` | SoundManager.PlayMusicGachaBGM(info) -> ReachBgmToBgmLabel(sorted music item[1]) | `GACHA_MUSIC_BGM` | 28 | 28/0 | 28 | 323473–1101616 | 28/0 | 28 | correct-at-risk |
| 51 | `GachaVoiceGetList.cs:39` | SoundManager.PlayMusicGachaBGM(info) -> ReachBgmToBgmLabel(sorted music item[1]) | `GACHA_MUSIC_BGM` | 28 | 28/0 | 28 | 323473–1101616 | 28/0 | 28 | correct-at-risk |
| 52 | `GachaVoiceGetList.cs:58` | SoundManager.PlayMusicGachaBGM(info) -> ReachBgmToBgmLabel(sorted music item[1]) | `GACHA_MUSIC_BGM` | 28 | 28/0 | 28 | 323473–1101616 | 28/0 | 28 | correct-at-risk |
| 53 | `MusicGachaPlay.cs:192` | SoundManager.PlayMusicGachaBGM(info) -> ReachBgmToBgmLabel(sorted music item[1]) | `GACHA_MUSIC_BGM` | 28 | 28/0 | 28 | 323473–1101616 | 28/0 | 28 | correct-at-risk |

Full per-(site × label) expansion, 2,906 rows with presence/size/loop/bucket per label: `C:\mfgtmp\ost_audit\site_labels_expanded.csv`. Machine-readable site rows: `site_table.csv` / `site_table.json`.

Arithmetic checks that close the enumeration: the enumerated sites cover **777 of the 781** BGM table labels (the 4 uncovered ones are the dead labels of §5.1) plus the 3 SE-category labels `HomeFightGirlNews.cs:271` plays through `PlayBgm`; the union of their clips is **748** - every BGM clip in the shipped content.

## 4. Table facts that shape the listening result

- **Volumes are not unity.** BGM label volumes run 0.32–1.0 (e.g. `BGM_CASTING` 0.32, `BGM_CASTING_CM` 0.42, `BGM_G0x_REACH_*` 0.6–0.7, mahjong `BGM_*MJG*` 0.32–0.53). Any A/B of a loop fix must keep these, they are original mix intent.
- **No BGM fades in the table** (`fadeIn`/`fadeOut` zero on all 781): fades only arrive from explicit code calls (`StopBgmWithFadeout(fadeSec)`). A looping media-path clip therefore gets no masking envelope of its own.
- **One-shots are by design** (122 labels): `BGM_TITLE`, `BGM_GAMEOVER`, `BGM_MOVIE_A/B`, `BGM_REACH3`, the 89 per-character reach jingles (`*_REACH_5`, `*_REACH_10`, `*_REACH_BM*_3`, `*_REACH_TH1_3`) and the 28 `BGM_THUMB_G0x_REACH_[6-9]G` preview variants. Their sites (`ExecAgari.cs:245/250/254`, `CutinManager.cs:248`, `GachaVoiceGetList.cs:54`) expect a play-and-end, so `loop=false` is correct, not an oversight.
- **Deliberate aliasing**: 33 clips back two labels each. Three of them are cross-family: label `BGM_3MJG1_2` → clip `BGM_4MJG1_2`, `BGM_3MJG1_5` → clip `BGM_2MJG1_4`, `BGM_REACH2` → clip `BGM_REACH1`. The 3-player kyoku therefore audibly reuses 4-player and 2-player tracks; that is shipped data, faithfully reconstructed.

## 5. Findings

Wiring verdicts first: **0 broken, 44 correct, 6 correct-at-risk, 3 unreachable-on-lane** across the 53 sites. Everything below is a hazard, a hazard-shaped fact, or documentation drift - not a mis-wire.

1. **Dead BGM labels (4), and 5.26 MB of AAC no live label can play** - `BGM_3MJG_KAC1`, `BGM_4MJG_KAC1`, `BGM_RESULT_KAC1`, `BGM_REACH2` appear in no code literal (full literal scan: 456 distinct BGM-ish literals, none of them) and in no data-driven route (no `isRandomPlay` labels, `randomSource` empty, so no table-side expansion). The three `*MJG_KAC1`/`RESULT_KAC1` clips are still served (1,289,820 + 1,230,191 + 268,620 B). Separately, three source files under `Assets\MFG\Sound\BGM` are named by **no label at all** yet are still imported and served: `BGM_3MJG1_2.ogg` 864,505 B, `BGM_3MJG1_5.ogg` 912,297 B, `BGM_REACH2.ogg` 945,332 B. Faithful to the original; payload-only. Consequence for the L105 fix: an "exposed clip" allow-list should skip these 6.
2. **`SoundManager.cs:554` PlayMusicGachaBGM has no guard and a hard index** - `PlayBgm(CustomizeItemMaster.ToItemID(info.GetSortedMusicItems()[1].oid).ReachBgmToBgmLabel())`: `GetSortedMusicItems()[1]` throws `ArgumentOutOfRangeException` for a music gacha with fewer than two items, and `ReachBgmToBgmLabel()` does `s_labelDic[itemID]` (`ItemIDExtentions.cs`), which throws `KeyNotFoundException` if the second sorted item is a `MyRoomBgm` item rather than a `ReachBgm` item - plausibly reachable, since `BGM_THUMB_HOME_*` preview assets exist, i.e. home-type items do ship in the customize catalog. All six callers `.Forget()` the task (`GachaSelectFlow.cs:304/373`, `GachaVoiceListWindow.cs:31`, `GachaVoiceGetList.cs:39/58`, `MusicGachaPlay.cs:192`), so the failure surfaces as a music-gacha screen with no BGM and a swallowed/UniTask-logged exception, not a crash. Note the sibling wrapper `PlayMyCharaMyRoomBGM` **was** hardened in the reconstruction (`SoundManager.cs:536-549` try/catch + item/label logging); this one is still the raw one-liner. Whether any live lane gacha actually has such an item set is server data and unverified (§6).
3. **Empty label slots reach the façade as `""`** - `KyokuCall.cs` kyoku BGM arrays carry 12 empty slots, and `EventTakuMaster` has 34 empty positions across its 14 event rows (e.g. `BgmLableName[taku1][ba0][kyoku3]` unset). `KyokuCall.cs:117` hands those straight to `PlayBgm("")`. Behaviour measured in code: `AudioManager.prepareInstance` fails `sourceDict.TryGetValue("")` → returns `AudioDefine.INSTANCE_ID_ERROR` → `SoundManager.cs:523 AssertLabelError` logs `SE: is not found!` and playback is a no-op - one console line per kyoku, no crash, no BGM. Faithful to the original; keep it when triaging lane logs so these do not read as new faults.
4. **`BGM_*` labels through the SE façade, and SE labels through the BGM façade** - exactly two cross-category uses, both original: `MissionEventAcquireWindow.cs:45` plays a music-preview label with `PlaySe` - the preview branch of the item→label helpers, so either the 85 `BGM_THUMB_HOME*`/`BGM_THUMB_STORY*`/`BGM_THUMB_G0x_*` MyRoom-item previews (`MyRoomToMyRoomBgmLabel(itemID, isPreview: true)`) or the 235 `BGM_THUMB_*` ReachBgm-item previews, 259 distinct labels in total, stopped with `StopSe` at `:49` - while `HomeFightGirlNews.cs:271` plays `MovieSoundList` rows flagged `IsBGM=1` with `PlayBgm`; those three labels are SE-category one-shots (`SFX_SYS_INFO4` 77,316 B, `SFX_SYS_INFO5` 94,599 B, `SFX_SYS_TOJO_G11` 86,349 B; all present, all `loop=false`). Consequence: category-scoped stops (`StopCategory("BGM")` in `StopBgm`) miss the first, and `StopSe` misses the second - original behaviour, worth knowing before "fixing" it.
5. **Two arcade-only gates and one hidden debug window are unreachable on the lane** - (a) `HomeFightGirlNews.cs:214` (`Master.BgmLabel`, 7 non-empty values `BGM_G01..G08_REACH_7`): the items are `NewsType.MusicGacha` (=8), and `IsDisp()` → `IsEnable()` returns false for MusicGacha news unless PPEX or Konasta/Netcafe, so a WebGL lane never plays them (`HomeFightGirlNews.cs:163-178`); their clips stay live through the cutin/gacha routes. (b) `MatchingFlow.cs:163`'s `BGM_CASTING_CM` variant is gated the same PASELI way (clip `BGM_G07_REACH_7`, 435,682 B, 35.691 s loop, exposed if ever enabled) - matches docs/37 §1. (c) `SoundButton.cs:34` can play any of the 10,433 table labels, but its only entry point is dead three times over: `HomeFlow.cs:280` deactivates `m_soundTestButton` unconditionally in `OnInitialize`, that button's `m_OnClick.m_PersistentCalls.m_Calls` is empty in `MFG\Scenes\Home\Home.unity` (:160396-160398), and `HomeFlow` never instantiates the `m_soundTestSet` prefab it references (:204). (d) `TestModeSkeleton\SoundOptions.cs:208` (`BGM_HOME`) lives in the service-mode `SoundOptions` scene - it ships in the build's scene list (`ProjectSettings\EditorBuildSettings.asset:105`) and is entered only from the error-mode menu's Test item (`ErrorModeMenu.cs:185-188` sets the only `IsTestModeRequest = true`).
6. **Documentation drift (3 spots, all cosmetic)** - docs/30:12 "34 `PlayBgm` sites" is a textual count, not a site count (§3 explains the 29/34 reconciliation); docs/30:24 "BGM (751 clips / 781 labels)" counts **751 source files** in BGM folders, of which 3 no label names (748 are addressable through labels); docs/30:45 puts the gacha-pull BGM at `GachaPlayFlow.cs:106`, the call is at `:116`. Everything else in the docs/30 occasion map checked out against the measured lines (`TitleFlow.cs:208`, `:630/:651`, `CardEntryScene.cs:205`, `HomeFlow.cs:294`, `MatchingFlow.cs:158-163`, `KyokuCall.cs:116-118`, `Agari.cs:695`).
7. **Recon-side divergences in the audio path are logging/capping only** - the sites where the reconstruction differs from the decompiled original are `SoundManager.cs:536-549` (try/catch + item/label logging in `PlayMyCharaMyRoomBGM`), `ContinueFlow.cs:128-130` (`UniTask.WhenAny` 5 s caps on the `PlayBgm`/`PlayMyCharaVoice` awaits so presentation audio cannot gate the continue/exit button), and the `LoadAudioClipCompat` `.wav`→`.ogg` loader. None changes the playback target or loop flag; L105 is upstream of all of them (glue + container), so no recon-side edit is implicated in the seam.

## 6. Not verified / residual uncertainty

- **Buffer-path BGM at runtime.** The `<131,072 → decompress=1` branch is read from the served glue and matches the sizes, and small clips do decode at runtime (191 `decodeAudioData` calls in the 11-03 telemetry), but no per-label runtime attribution exists for a *BGM* clip under the threshold - the 262 looping buffer-path clips (largest: `BGM_STORY7` 130,039 B / 10.576 s, `BGM_THUMB_G19_REACH_2` 124,063 B / 10.100 s) are bucketed by size, not observed. An A/B of any fix should confirm one of them loops sample-accurately on the lane, which is the discriminating measurement between the two paths.
- **Server-driven gacha content.** Whether any live music-gacha series has fewer than two music items, or a second item that is a `MyRoomBgm` item (finding 2), is lane-server data; the hazard is proven statically, the trigger is not constructed.
- **Story CSV coverage** is static: 218 story CSVs / 275 `PlayBgm` rows / 45 distinct labels + `<StopBgm>`; no runtime story walk was replayed, so which story rows actually fire on the lane is unproven (all 45 labels resolve).
- **The 28 mixed-flag preview clips and the 3 SE-through-`PlayBgm` clips** were checked at table + container level, not by opening the respective windows on the lane.
- **Owner browser engine**: as in docs/37 §6, the ~12 ms gap figure is Chrome's; the exposure *set* (bucket membership) is engine-independent because it is decided in Unity's glue and container.

## 7. Artifacts

`C:\mfgtmp\ost_audit\`: `t1_tables.py` (table parse + shipped byte-identity) · `t2`–`t4` (label/derivation/dictionary extraction → `labels.json`, `derivations.json`, `code_dicts.json`) · `t5_sizes.py` → `aac_sizes.tsv` (748 served containers) · `t8_site_table.py` → `site_table.csv/.json`, `site_labels_expanded.csv` (2,906 rows) · `t9_se.py` → `se_clips.json` · `t10_checks.py` (independent counts, coverage, bucket arithmetic, loop inventory) · `t11`/`t13` → `l105_exposure_final.tsv` (+ `exposure_map.md`, Appendix A) · `t15`–`t17` (disk vs table vs served import cross-check) · `t20`–`t23` (SoundTest/SoundOptions reachability) · `t18`/`t19` (report numbers). Inputs read-only: `C:\mfg\build\unity-project`, `C:\mfgtmp\webroot\{Build,StreamingAssets}`, `C:\mfgtmp\music_rca\clips.tsv`, 11-03 loop telemetry under `C:\mfgtmp`.

## Appendix A - L105 exposure map (all 748 BGM clips, served AAC size desc)

`loop` is the label-level flag (mixed = some labels loop, some do not); `dur s` is Vorbis content duration; `lane` = a lane-reachable site can play it. `bucket`: media = AAC container >= 131,072 B (HTMLAudioElement, L105-exposed when looping), buffer = smaller (decodeAudioData, sample-accurate loop).

| # | clip | AAC B | bucket | loop | labels | dur s | lane |
|---:|---|---:|---|---|---:|---:|---|
| 1 | BGM_HOME_BM2_G10 | 1670825 | media | true | 1 | 137.21 | Y |
| 2 | BGM_MJG_ECSTASY2 | 1667756 | media | true | 1 | 136.92 | Y |
| 3 | BGM_HOME_BM3_G19 | 1645670 | media | true | 1 | 135.12 | Y |
| 4 | BGM_HOME_BM2_G16 | 1599883 | media | true | 1 | 131.37 | Y |
| 5 | BGM_HOME_BM2_G15 | 1582077 | media | true | 1 | 129.90 | Y |
| 6 | BGM_MJG_SCRAMBLE2 | 1577660 | media | true | 1 | 129.50 | Y |
| 7 | BGM_HOME_BM2_G12 | 1570206 | media | true | 1 | 128.94 | Y |
| 8 | BGM_HOME_BM2_G06 | 1562857 | media | true | 1 | 128.33 | Y |
| 9 | BGM_HOME_BM3_G17 | 1561603 | media | true | 1 | 128.20 | Y |
| 10 | BGM_G10_REACH_BM2_2 | 1557202 | media | true | 1 | 127.84 | Y |
| 11 | BGM_G16_REACH_BM2_2 | 1554095 | media | true | 1 | 127.58 | Y |
| 12 | BGM_HOME_BM3_G02 | 1550741 | media | true | 1 | 127.33 | Y |
| 13 | BGM_HOME_BM1_G02 | 1548725 | media | true | 1 | 127.16 | Y |
| 14 | BGM_HOME_BM2_G11 | 1545247 | media | true | 1 | 126.87 | Y |
| 15 | BGM_HOME_BM2_G01 | 1539681 | media | true | 1 | 126.43 | Y |
| 16 | BGM_HOME_KAC1 | 1525247 | media | true | 1 | 125.21 | Y |
| 17 | BGM_HOME_BM3_G16 | 1523570 | media | true | 1 | 125.10 | Y |
| 18 | BGM_HOME_TH1_G03 | 1523570 | media | true | 1 | 125.11 | Y |
| 19 | BGM_HOME_TH1_G04 | 1522440 | media | true | 1 | 125.00 | Y |
| 20 | BGM_G12_REACH_BM2_2 | 1520773 | media | true | 1 | 124.85 | Y |
| 21 | BGM_HOME_BM2_G07 | 1520744 | media | true | 1 | 124.87 | Y |
| 22 | BGM_HOME_BM3_G12 | 1510851 | media | true | 1 | 124.06 | Y |
| 23 | BGM_HOME_BM1_G10 | 1507177 | media | true | 1 | 123.75 | Y |
| 24 | BGM_HOME_TH1_G12 | 1504068 | media | true | 1 | 123.50 | Y |
| 25 | BGM_HOME_BM2_G08 | 1503503 | media | true | 1 | 123.45 | Y |
| 26 | BGM_HOME_BM3_G13 | 1495930 | media | true | 1 | 122.82 | Y |
| 27 | BGM_HOME_BM1_G12 | 1494741 | media | true | 1 | 122.74 | Y |
| 28 | BGM_HOME_BM3_G07 | 1494479 | media | true | 1 | 122.71 | Y |
| 29 | BGM_HOME_BM2_G05 | 1494458 | media | true | 1 | 122.72 | Y |
| 30 | BGM_G15_REACH_BM2_2 | 1491636 | media | true | 1 | 122.46 | Y |
| 31 | BGM_G06_REACH_BM2_2 | 1491489 | media | true | 1 | 122.43 | Y |
| 32 | BGM_HOME_BM3_G20 | 1486827 | media | true | 1 | 122.08 | Y |
| 33 | BGM_HOME_BM3_G18 | 1486232 | media | true | 1 | 122.02 | Y |
| 34 | BGM_HOME_BM3_G05 | 1480609 | media | true | 1 | 121.58 | Y |
| 35 | BGM_HOME_BM1_G11 | 1476369 | media | true | 1 | 121.23 | Y |
| 36 | BGM_HOME_BM2_G03 | 1474391 | media | true | 1 | 121.06 | Y |
| 37 | BGM_HOME_TH1_G05 | 1471847 | media | true | 1 | 120.84 | Y |
| 38 | BGM_HOME_BM3_G08 | 1469233 | media | true | 1 | 120.62 | Y |
| 39 | BGM_HOME_TH1_G01 | 1463085 | media | true | 1 | 120.14 | Y |
| 40 | BGM_HOME_BM1_G04 | 1461672 | media | true | 1 | 120.01 | Y |
| 41 | BGM_HOME_TH1_G02 | 1461107 | media | true | 1 | 119.97 | Y |
| 42 | BGM_HOME_BM3_G14 | 1452910 | media | true | 1 | 119.29 | Y |
| 43 | BGM_G16_REACH_BM3_2 | 1452652 | media | true | 1 | 119.25 | Y |
| 44 | BGM_G13_REACH_BM3_2 | 1452339 | media | true | 1 | 119.22 | Y |
| 45 | BGM_HOME_TH1_G10 | 1451568 | media | true | 1 | 119.16 | Y |
| 46 | BGM_HOME_TOHO2 | 1448671 | media | true | 1 | 118.94 | Y |
| 47 | BGM_HOME_BM1_G01 | 1447823 | media | true | 1 | 118.87 | Y |
| 48 | BGM_G07_REACH_BM2_2 | 1447347 | media | true | 1 | 118.80 | Y |
| 49 | BGM_HOME_BM1_G06 | 1446410 | media | true | 1 | 118.77 | Y |
| 50 | BGM_HOME_BM2_G13 | 1445562 | media | true | 1 | 118.69 | Y |
| 51 | BGM_G12_REACH_TH1_2 | 1444973 | media | true | 1 | 118.63 | Y |
| 52 | BGM_MJG_SCRAMBLE1 | 1444703 | media | true | 1 | 118.60 | Y |
| 53 | BGM_HOME_BM3_G11 | 1444647 | media | true | 1 | 118.60 | Y |
| 54 | BGM_HOME_BM3_G09 | 1444431 | media | true | 1 | 118.59 | Y |
| 55 | BGM_G05_REACH_BM2_2 | 1443870 | media | true | 1 | 118.52 | Y |
| 56 | BGM_G19_REACH_BM3_2 | 1443056 | media | true | 1 | 118.45 | Y |
| 57 | BGM_G05_REACH_TH1_2 | 1441051 | media | true | 1 | 118.29 | Y |
| 58 | BGM_HOME_BM1_G09 | 1439371 | media | true | 1 | 118.16 | Y |
| 59 | BGM_HOME_TH1_G09 | 1437367 | media | true | 1 | 118.01 | Y |
| 60 | BGM_G12_REACH_BM3_2 | 1436237 | media | true | 1 | 117.89 | Y |
| 61 | BGM_G01_REACH_BM2_2 | 1434585 | media | true | 1 | 117.75 | Y |
| 62 | BGM_HOME_TH1_G08 | 1432997 | media | true | 1 | 117.64 | Y |
| 63 | BGM_HOME_TH1_G07 | 1431995 | media | true | 1 | 117.59 | Y |
| 64 | BGM_G02_REACH_TH1_2 | 1428291 | media | true | 1 | 117.25 | Y |
| 65 | BGM_G08_REACH_BM2_2 | 1427795 | media | true | 1 | 117.20 | Y |
| 66 | BGM_HOME_TH1_G13 | 1424929 | media | true | 1 | 117.00 | Y |
| 67 | BGM_HOME_BM2_G04 | 1416396 | media | true | 1 | 116.29 | Y |
| 68 | BGM_HOME_TH1_G11 | 1415037 | media | true | 1 | 116.18 | Y |
| 69 | BGM_HOME_BM1_G08 | 1412776 | media | true | 1 | 116.00 | Y |
| 70 | BGM_G12_REACH_BM1_2 | 1408821 | media | true | 1 | 115.64 | Y |
| 71 | BGM_G17_REACH_BM3_2 | 1408598 | media | true | 1 | 115.62 | Y |
| 72 | BGM_MJG_TOHO1 | 1406264 | media | true | 1 | 115.44 | Y |
| 73 | BGM_G10_REACH_TH1_2 | 1404398 | media | true | 1 | 115.28 | Y |
| 74 | BGM_HOME_BM3_G03 | 1402318 | media | true | 1 | 115.13 | Y |
| 75 | BGM_4MJG7 | 1400370 | media | true | 1 | 114.95 | Y |
| 76 | BGM_G18_REACH_BM3_2 | 1400362 | media | true | 1 | 114.95 | Y |
| 77 | BGM_G10_REACH_BM1_2 | 1397917 | media | true | 1 | 114.75 | Y |
| 78 | BGM_HOME_BM2_G09 | 1392708 | media | true | 1 | 114.34 | Y |
| 79 | BGM_G05_REACH_BM3_2 | 1389900 | media | true | 1 | 114.09 | Y |
| 80 | BGM_G04_REACH_BM1_2 | 1382677 | media | true | 1 | 113.49 | Y |
| 81 | BGM_G11_REACH_BM1_2 | 1382587 | media | true | 1 | 113.48 | Y |
| 82 | BGM_4MJG6 | 1381687 | media | true | 1 | 113.43 | Y |
| 83 | BGM_G14_REACH_BM3_2 | 1378027 | media | true | 1 | 113.13 | Y |
| 84 | BGM_G08_REACH_TH1_2 | 1369560 | media | true | 1 | 112.41 | Y |
| 85 | BGM_G06_REACH_BM1_2 | 1365766 | media | true | 1 | 112.09 | Y |
| 86 | BGM_STORY_G09 | 1364773 | media | true | 1 | 112.03 | Y |
| 87 | BGM_HOME_BM3_G06 | 1364727 | media | true | 1 | 112.06 | Y |
| 88 | BGM_G09_REACH_3 | 1364450 | media | true | 1 | 112.00 | Y |
| 89 | BGM_G09_REACH_4 | 1364208 | media | true | 1 | 111.98 | Y |
| 90 | BGM_G20_REACH_BM3_2 | 1356343 | media | true | 1 | 111.32 | Y |
| 91 | BGM_HOME_TH1_G06 | 1355965 | media | true | 1 | 111.33 | Y |
| 92 | BGM_G02_REACH_BM1_2 | 1354258 | media | true | 1 | 111.17 | Y |
| 93 | BGM_G11_REACH_BM2_2 | 1353242 | media | true | 1 | 111.08 | Y |
| 94 | BGM_G09_REACH_BM3_2 | 1352397 | media | true | 1 | 111.01 | Y |
| 95 | BGM_G09_REACH_BM2_2 | 1349962 | media | true | 1 | 110.79 | Y |
| 96 | BGM_HOME_BM3_G04 | 1347995 | media | true | 1 | 110.66 | Y |
| 97 | BGM_G13_REACH_TH1_2 | 1330545 | media | true | 1 | 109.21 | Y |
| 98 | BGM_G04_REACH_TH1_2 | 1329399 | media | true | 1 | 109.13 | Y |
| 99 | BGM_G08_REACH_BM3_2 | 1328009 | media | true | 1 | 109.00 | Y |
| 100 | BGM_G01_REACH_BM1_2 | 1327433 | media | true | 1 | 108.95 | Y |
| 101 | BGM_MJG_BOMB3_5 | 1321778 | media | true | 1 | 108.49 | Y |
| 102 | BGM_G08_REACH_BM1_2 | 1320629 | media | true | 1 | 108.40 | Y |
| 103 | BGM_HOME_BM1_G07 | 1318939 | media | true | 1 | 108.29 | Y |
| 104 | BGM_G07_REACH_BM3_2 | 1318732 | media | true | 1 | 108.25 | Y |
| 105 | BGM_G03_REACH_TH1_2 | 1315816 | media | true | 1 | 108.01 | Y |
| 106 | BGM_G07_REACH_TH1_2 | 1313331 | media | true | 1 | 107.79 | Y |
| 107 | BGM_G04_REACH_BM2_2 | 1311627 | media | true | 1 | 107.67 | Y |
| 108 | BGM_3MJG2_4 | 1310251 | media | true | 1 | 107.53 | Y |
| 109 | BGM_G13_REACH_BM2_2 | 1309624 | media | true | 1 | 107.50 | Y |
| 110 | BGM_HOME_BM1_G03 | 1305938 | media | true | 1 | 107.23 | Y |
| 111 | BGM_HOME_TOHO1 | 1301416 | media | true | 1 | 106.86 | Y |
| 112 | BGM_G01_REACH_TH1_2 | 1299522 | media | true | 1 | 106.67 | Y |
| 113 | BGM_G03_REACH_BM2_2 | 1298939 | media | true | 1 | 106.61 | Y |
| 114 | BGM_HOME_BM1_G05 | 1296262 | media | true | 1 | 106.41 | Y |
| 115 | BGM_3MJG_KAC1 | 1289820 | media | true | 1 | 105.88 | N |
| 116 | BGM_HOME_BM2_G02 | 1288132 | media | true | 1 | 105.77 | Y |
| 117 | BGM_G02_REACH_BM3_2 | 1286563 | media | true | 1 | 105.60 | Y |
| 118 | BGM_G09_REACH_TH1_2 | 1285297 | media | true | 1 | 105.50 | Y |
| 119 | BGM_HOME_BM3_G10 | 1278804 | media | true | 1 | 104.99 | Y |
| 120 | BGM_G11_REACH_TH1_2 | 1277669 | media | true | 1 | 104.86 | Y |
| 121 | BGM_G11_REACH_BM3_2 | 1275693 | media | true | 1 | 104.71 | Y |
| 122 | BGM_G04_REACH_BM3_2 | 1275688 | media | true | 1 | 104.72 | Y |
| 123 | BGM_HOME_BM3_G15 | 1246018 | media | true | 1 | 102.30 | Y |
| 124 | BGM_G06_REACH_BM3_2 | 1239821 | media | true | 1 | 101.76 | Y |
| 125 | BGM_HOME_SONG1 | 1236691 | media | true | 1 | 101.54 | Y |
| 126 | BGM_HOME_SONG2 | 1235843 | media | true | 1 | 101.46 | Y |
| 127 | BGM_HOME_BM3_G01 | 1234147 | media | true | 1 | 101.31 | Y |
| 128 | BGM_4MJG_KAC1 | 1230191 | media | true | 1 | 100.98 | N |
| 129 | BGM_MJG_ASSAULT1 | 1218331 | media | true | 1 | 100.00 | Y |
| 130 | BGM_G03_REACH_BM1_2 | 1204307 | media | true | 1 | 98.83 | Y |
| 131 | BGM_G09_REACH_BM1_2 | 1201995 | media | true | 1 | 98.64 | Y |
| 132 | BGM_G03_REACH_BM3_2 | 1199145 | media | true | 1 | 98.43 | Y |
| 133 | BGM_G01_REACH_BM3_2 | 1186996 | media | true | 1 | 97.43 | Y |
| 134 | BGM_G06_REACH_TH1_2 | 1175966 | media | true | 1 | 96.51 | Y |
| 135 | BGM_G10_REACH_BM3_2 | 1171198 | media | true | 1 | 96.13 | Y |
| 136 | BGM_G15_REACH_BM3_2 | 1147938 | media | true | 1 | 94.21 | Y |
| 137 | BGM_3MJG2_1 | 1140641 | media | true | 1 | 93.62 | Y |
| 138 | BGM_4MJG2_1 | 1140641 | media | true | 1 | 93.62 | Y |
| 139 | BGM_G02_REACH_BM2_2 | 1137516 | media | true | 1 | 93.35 | Y |
| 140 | BGM_G07_REACH_BM1_2 | 1136381 | media | true | 1 | 93.25 | Y |
| 141 | BGM_G17_REACH_3 | 1120533 | media | true | 1 | 91.93 | Y |
| 142 | BGM_G17_REACH_4 | 1120160 | media | true | 1 | 91.91 | Y |
| 143 | BGM_G10_REACH_BM2_1 | 1109021 | media | true | 1 | 91.01 | Y |
| 144 | BGM_G05_REACH_BM1_2 | 1108090 | media | true | 1 | 90.95 | Y |
| 145 | BGM_3MJG2_3 | 1102770 | media | true | 1 | 90.50 | Y |
| 146 | BGM_HOME_QMA1 | 1101641 | media | true | 1 | 90.42 | Y |
| 147 | BGM_G08_REACH_8 | 1101616 | media | true | 1 | 90.41 | Y |
| 148 | BGM_HOME | 1100734 | media | true | 1 | 90.34 | Y |
| 149 | BGM_MOVIE_A | 1096189 | media | false | 1 | 89.96 | Y |
| 150 | BGM_G08_REACH_9 | 1091403 | media | true | 1 | 89.57 | Y |
| 151 | BGM_3MJG2_2 | 1084415 | media | true | 1 | 88.99 | Y |
| 152 | BGM_4MJG2_2 | 1084415 | media | true | 1 | 88.99 | Y |
| 153 | BGM_MJG_OPERATION1 | 1083810 | media | true | 1 | 88.94 | Y |
| 154 | BGM_HOME_BM2_G14 | 1079261 | media | true | 1 | 88.61 | Y |
| 155 | BGM_MJG_BOMB4_3 | 1078413 | media | true | 1 | 88.50 | Y |
| 156 | BGM_STORY_G10_3 | 1073946 | media | true | 1 | 88.11 | Y |
| 157 | BGM_G10_REACH_3 | 1072365 | media | true | 1 | 87.98 | Y |
| 158 | BGM_3MJG1_3 | 1072246 | media | true | 1 | 88.00 | Y |
| 159 | BGM_4MJG1_3 | 1072246 | media | true | 1 | 88.00 | Y |
| 160 | BGM_STORY4 | 1070849 | media | true | 1 | 87.87 | Y |
| 161 | BGM_G10_REACH_4 | 1068439 | media | true | 1 | 87.67 | Y |
| 162 | BGM_G20_REACH_3 | 1062034 | media | true | 1 | 87.15 | Y |
| 163 | BGM_STORY_G20 | 1062025 | media | true | 1 | 87.15 | Y |
| 164 | BGM_G20_REACH_4 | 1062014 | media | true | 1 | 87.15 | Y |
| 165 | BGM_G15_REACH_4 | 1061828 | media | true | 1 | 87.14 | Y |
| 166 | BGM_G07_REACH_9 | 1061753 | media | true | 1 | 87.12 | Y |
| 167 | BGM_MJG_BOMB3_4 | 1061242 | media | true | 1 | 87.09 | Y |
| 168 | BGM_G07_REACH_8 | 1060573 | media | true | 1 | 87.04 | Y |
| 169 | BGM_G01_REACH_9 | 1059244 | media | true | 1 | 86.92 | Y |
| 170 | BGM_G01_REACH_8 | 1049624 | media | true | 1 | 86.13 | Y |
| 171 | BGM_STORY3 | 1040861 | media | true | 1 | 85.43 | Y |
| 172 | BGM_4MJG4 | 1037714 | media | true | 1 | 85.16 | Y |
| 173 | BGM_MJG_BOMB3_1 | 1035020 | media | true | 1 | 84.91 | Y |
| 174 | BGM_2MJG1_1 | 1034270 | media | true | 1 | 84.88 | Y |
| 175 | BGM_3MJG1_1 | 1034270 | media | true | 1 | 84.88 | Y |
| 176 | BGM_4MJG1_1 | 1034270 | media | true | 1 | 84.88 | Y |
| 177 | BGM_MJG_BOMB4_2 | 1033890 | media | true | 1 | 84.84 | Y |
| 178 | BGM_MJG_GREEN1 | 1024180 | media | true | 1 | 84.05 | Y |
| 179 | BGM_G08_REACH_3 | 1013444 | media | true | 1 | 83.15 | Y |
| 180 | BGM_G08_REACH_4 | 1013443 | media | true | 1 | 83.15 | Y |
| 181 | BGM_STORY_G08 | 1013083 | media | true | 1 | 83.14 | Y |
| 182 | BGM_G04_REACH_9 | 1008993 | media | true | 1 | 82.78 | Y |
| 183 | BGM_MJG_ECSTASY1 | 1007837 | media | true | 1 | 82.70 | Y |
| 184 | BGM_2MJG1_3 | 1007571 | media | true | 1 | 82.68 | Y |
| 185 | BGM_3MJG1_4 | 1007571 | media | true | 1 | 82.68 | Y |
| 186 | BGM_G21_REACH_4 | 1007523 | media | true | 1 | 82.67 | Y |
| 187 | BGM_G21_REACH_3 | 1007480 | media | true | 1 | 82.67 | Y |
| 188 | BGM_G13_REACH_4 | 1005814 | media | true | 1 | 82.52 | Y |
| 189 | BGM_G19_REACH_4 | 1003820 | media | true | 1 | 82.36 | Y |
| 190 | BGM_STORY_G19 | 1000670 | media | true | 1 | 82.12 | Y |
| 191 | BGM_G05_REACH_9 | 1000406 | media | true | 1 | 82.11 | Y |
| 192 | BGM_G19_REACH_3 | 1000389 | media | true | 1 | 82.10 | Y |
| 193 | BGM_G05_REACH_8 | 999836 | media | true | 1 | 82.04 | Y |
| 194 | BGM_G14_REACH_BM2_2 | 995622 | media | true | 1 | 81.69 | Y |
| 195 | BGM_4MJG5 | 993691 | media | true | 1 | 81.55 | Y |
| 196 | BGM_G04_REACH_8 | 988952 | media | true | 1 | 81.14 | Y |
| 197 | BGM_G02_REACH_8 | 975050 | media | true | 1 | 80.00 | Y |
| 198 | BGM_G02_REACH_9 | 974994 | media | true | 1 | 79.99 | Y |
| 199 | BGM_G13_REACH_3 | 968822 | media | true | 1 | 79.50 | Y |
| 200 | BGM_STORY_G13 | 968815 | media | true | 1 | 79.50 | Y |
| 201 | BGM_STORY_G01 | 964285 | media | true | 1 | 79.12 | Y |
| 202 | BGM_MJG_BOMB3_3 | 963678 | media | true | 1 | 79.08 | Y |
| 203 | BGM_G01_REACH_4 | 959427 | media | true | 1 | 78.72 | Y |
| 204 | BGM_G01_REACH_3 | 959134 | media | true | 1 | 78.70 | Y |
| 205 | BGM_3MJG2_6 | 952405 | media | true | 1 | 78.14 | Y |
| 206 | BGM_4MJG2_4 | 952405 | media | true | 1 | 78.14 | Y |
| 207 | BGM_3MJG1_6 | 946982 | media | true | 1 | 77.72 | Y |
| 208 | BGM_G05_REACH_3 | 945640 | media | true | 1 | 77.60 | Y |
| 209 | BGM_STORY_G05 | 945295 | media | true | 1 | 77.58 | Y |
| 210 | BGM_G05_REACH_4 | 945283 | media | true | 1 | 77.58 | Y |
| 211 | BGM_STORY_G15 | 942771 | media | true | 1 | 77.36 | Y |
| 212 | BGM_G15_REACH_3 | 942739 | media | true | 1 | 77.36 | Y |
| 213 | BGM_3MJG2_5 | 942459 | media | true | 1 | 77.33 | Y |
| 214 | BGM_4MJG2_3 | 942459 | media | true | 1 | 77.33 | Y |
| 215 | BGM_REACH1 | 934567 | media | true | 2 | 76.69 | Y |
| 216 | BGM_G06_REACH_9 | 933709 | media | true | 1 | 76.61 | Y |
| 217 | BGM_G01_REACH_BM2_1 | 931803 | media | true | 1 | 76.45 | Y |
| 218 | BGM_STORY_G21 | 926393 | media | true | 1 | 76.01 | Y |
| 219 | BGM_MJG_ATOMIC1 | 924507 | media | true | 1 | 75.86 | Y |
| 220 | BGM_G14_REACH_3 | 923580 | media | true | 1 | 75.79 | Y |
| 221 | BGM_G14_REACH_4 | 923575 | media | true | 1 | 75.77 | Y |
| 222 | BGM_STORY_G14 | 923552 | media | true | 1 | 75.79 | Y |
| 223 | BGM_G07_REACH_3 | 917992 | media | true | 1 | 75.32 | Y |
| 224 | BGM_STORY_G07 | 917955 | media | true | 1 | 75.32 | Y |
| 225 | BGM_G07_REACH_4 | 917862 | media | true | 1 | 75.31 | Y |
| 226 | BGM_2MJG1_4 | 912297 | media | true | 2 | 74.84 | Y |
| 227 | BGM_4MJG1_4 | 899319 | media | true | 1 | 73.79 | Y |
| 228 | BGM_STORY2 | 894998 | media | true | 1 | 73.44 | Y |
| 229 | BGM_G16_REACH_BM2_1 | 893318 | media | true | 1 | 73.29 | Y |
| 230 | BGM_G15_REACH_BM2_1 | 886798 | media | true | 1 | 72.75 | Y |
| 231 | BGM_STORY5 | 880134 | media | true | 1 | 72.20 | Y |
| 232 | BGM_G06_REACH_8 | 871481 | media | true | 1 | 71.52 | Y |
| 233 | BGM_4MJG3_1 | 868723 | media | true | 1 | 71.27 | Y |
| 234 | BGM_2MJG1_2 | 864505 | media | true | 1 | 70.92 | Y |
| 235 | BGM_4MJG1_2 | 864505 | media | true | 2 | 70.92 | Y |
| 236 | BGM_STORY_G03 | 854020 | media | true | 1 | 70.06 | Y |
| 237 | BGM_G03_REACH_3 | 853707 | media | true | 1 | 70.05 | Y |
| 238 | BGM_G03_REACH_4 | 853691 | media | true | 1 | 70.05 | Y |
| 239 | BGM_4MJG3_2 | 843287 | media | true | 1 | 69.18 | Y |
| 240 | BGM_MJG_BOMB4_1 | 834505 | media | true | 1 | 68.47 | Y |
| 241 | BGM_MJG_BOMB3_2 | 830498 | media | true | 1 | 68.15 | Y |
| 242 | BGM_STORY1 | 825954 | media | true | 1 | 67.75 | Y |
| 243 | BGM_STORY_G12 | 823183 | media | true | 1 | 67.53 | Y |
| 244 | BGM_G12_REACH_3 | 823142 | media | true | 1 | 67.53 | Y |
| 245 | BGM_G12_REACH_BM2_1 | 822682 | media | true | 1 | 67.49 | Y |
| 246 | BGM_G12_REACH_4 | 822604 | media | true | 1 | 67.50 | Y |
| 247 | BGM_G05_REACH_BM2_1 | 820041 | media | true | 1 | 67.27 | Y |
| 248 | BGM_G11_REACH_4 | 814187 | media | true | 1 | 66.79 | Y |
| 249 | BGM_G11_REACH_3 | 814174 | media | true | 1 | 66.79 | Y |
| 250 | BGM_STORY_G11 | 814139 | media | true | 1 | 66.80 | Y |
| 251 | BGM_G08_REACH_BM3_1 | 804028 | media | true | 1 | 65.95 | Y |
| 252 | BGM_4MJG3_3 | 796386 | media | true | 1 | 65.33 | Y |
| 253 | BGM_G07_REACH_BM2_1 | 789909 | media | true | 1 | 64.79 | Y |
| 254 | BGM_G08_REACH_BM2_1 | 785672 | media | true | 1 | 64.43 | Y |
| 255 | BGM_G18_REACH_4 | 779718 | media | true | 1 | 63.95 | Y |
| 256 | BGM_G18_REACH_3 | 779645 | media | true | 1 | 63.95 | Y |
| 257 | BGM_STORY_G18 | 779622 | media | true | 1 | 63.95 | Y |
| 258 | BGM_MJG_BOMB3_6 | 765592 | media | true | 2 | 62.79 | Y |
| 259 | BGM_G12_REACH_BM1_1 | 764701 | media | true | 1 | 62.73 | Y |
| 260 | BGM_G15_REACH_BM3_1 | 763333 | media | true | 1 | 62.61 | Y |
| 261 | BGM_4MJG3_4 | 759569 | media | true | 1 | 62.30 | Y |
| 262 | BGM_MJG_BOMB4_4 | 753702 | media | true | 1 | 61.83 | Y |
| 263 | BGM_G13_REACH_BM3_1 | 751721 | media | true | 1 | 61.66 | Y |
| 264 | BGM_G10_REACH_BM1_1 | 749569 | media | true | 1 | 61.47 | Y |
| 265 | BGM_G02_REACH_4 | 738667 | media | true | 1 | 60.60 | Y |
| 266 | BGM_STORY_G04 | 735293 | media | true | 1 | 60.30 | Y |
| 267 | BGM_G04_REACH_4 | 735285 | media | true | 1 | 60.31 | Y |
| 268 | BGM_G04_REACH_3 | 735016 | media | true | 1 | 60.30 | Y |
| 269 | BGM_STORY_G02 | 729926 | media | true | 1 | 59.87 | Y |
| 270 | BGM_G02_REACH_3 | 729652 | media | true | 1 | 59.85 | Y |
| 271 | BGM_G06_REACH_BM2_1 | 702779 | media | true | 1 | 57.65 | Y |
| 272 | BGM_G14_REACH_BM3_1 | 696581 | media | true | 1 | 57.13 | Y |
| 273 | BGM_G13_REACH_BM2_1 | 672568 | media | true | 1 | 55.16 | Y |
| 274 | BGM_G04_REACH_BM2_1 | 670363 | media | true | 1 | 54.97 | Y |
| 275 | BGM_G04_REACH_BM3_1 | 659002 | media | true | 1 | 54.04 | Y |
| 276 | BGM_STORY_G10_1 | 652278 | media | true | 1 | 53.49 | Y |
| 277 | BGM_STORY_G06 | 642332 | media | true | 1 | 52.68 | Y |
| 278 | BGM_G06_REACH_4 | 641472 | media | true | 1 | 52.61 | Y |
| 279 | BGM_G19_REACH_BM3_1 | 633685 | media | true | 1 | 51.97 | Y |
| 280 | BGM_STORY_G16 | 631065 | media | true | 1 | 51.72 | Y |
| 281 | BGM_G16_REACH_4 | 631054 | media | true | 1 | 51.73 | Y |
| 282 | BGM_G16_REACH_3 | 630903 | media | true | 1 | 51.73 | Y |
| 283 | BGM_G05_REACH_BM3_1 | 625435 | media | true | 1 | 51.29 | Y |
| 284 | BGM_G02_REACH_BM1_1 | 616289 | media | true | 1 | 50.54 | Y |
| 285 | BGM_G10_REACH_BM3_1 | 613846 | media | true | 1 | 50.33 | Y |
| 286 | BGM_G09_REACH_BM2_1 | 613628 | media | true | 1 | 50.30 | Y |
| 287 | BGM_HOME_NEWYEAR1 | 611613 | media | true | 1 | 50.14 | Y |
| 288 | BGM_G09_REACH_BM3_1 | 607531 | media | true | 1 | 49.80 | Y |
| 289 | BGM_G18_REACH_BM3_1 | 599614 | media | true | 1 | 49.17 | Y |
| 290 | BGM_G02_REACH_BM3_1 | 599491 | media | true | 1 | 49.13 | Y |
| 291 | BGM_G01_REACH_BM1_1 | 596220 | media | true | 1 | 48.89 | Y |
| 292 | BGM_G01_REACH_BM3_1 | 585547 | media | true | 1 | 48.02 | Y |
| 293 | BGM_G06_REACH_3 | 582637 | media | true | 1 | 47.78 | Y |
| 294 | BGM_G03_REACH_TH1_1 | 580699 | media | true | 1 | 47.62 | Y |
| 295 | BGM_G11_REACH_BM2_1 | 570293 | media | true | 1 | 46.75 | Y |
| 296 | BGM_G16_REACH_BM3_1 | 561771 | media | true | 1 | 46.05 | Y |
| 297 | BGM_HOME_XMAS1 | 560501 | media | true | 1 | 45.94 | Y |
| 298 | BGM_G02_REACH_BM2_1 | 554118 | media | true | 1 | 45.44 | Y |
| 299 | BGM_G20_REACH_BM3_1 | 538289 | media | true | 1 | 44.14 | Y |
| 300 | BGM_G05_REACH_TH1_1 | 535752 | media | true | 1 | 43.93 | Y |
| 301 | BGM_G06_REACH_BM3_1 | 513753 | media | true | 1 | 42.11 | Y |
| 302 | BGM_G13_REACH_TH1_1 | 512300 | media | true | 1 | 42.00 | Y |
| 303 | BGM_G07_REACH_TH1_1 | 499648 | media | true | 1 | 40.96 | Y |
| 304 | BGM_G11_REACH_BM3_1 | 498721 | media | true | 1 | 40.88 | Y |
| 305 | BGM_G02_REACH_TH1_1 | 498707 | media | true | 1 | 40.87 | Y |
| 306 | BGM_G04_REACH_BM1_1 | 491841 | media | true | 1 | 40.30 | Y |
| 307 | BGM_G06_REACH_TH1_1 | 488058 | media | true | 1 | 40.00 | Y |
| 308 | BGM_G07_REACH_BM3_1 | 475303 | media | true | 1 | 38.92 | Y |
| 309 | BGM_G09_REACH_BM1_1 | 475249 | media | true | 1 | 38.94 | Y |
| 310 | BGM_G04_REACH_TH1_1 | 468488 | media | true | 1 | 38.38 | Y |
| 311 | BGM_STORY_G17 | 463393 | media | true | 1 | 37.98 | Y |
| 312 | BGM_G21_REACH_1 | 461733 | media | true | 1 | 37.84 | Y |
| 313 | BGM_G21_REACH_2 | 461731 | media | true | 1 | 37.83 | Y |
| 314 | BGM_G08_REACH_TH1_1 | 450396 | media | true | 1 | 36.92 | Y |
| 315 | BGM_G17_REACH_BM3_1 | 445992 | media | true | 1 | 36.53 | Y |
| 316 | BGM_G12_REACH_BM3_1 | 439329 | media | true | 1 | 36.01 | Y |
| 317 | BGM_G07_REACH_7 | 435682 | media | true | 2 | 35.69 | Y |
| 318 | BGM_G07_REACH_6 | 434834 | media | true | 1 | 35.62 | Y |
| 319 | BGM_G18_REACH_2 | 425494 | media | true | 1 | 34.87 | Y |
| 320 | BGM_G18_REACH_1 | 425455 | media | true | 1 | 34.87 | Y |
| 321 | BGM_G03_REACH_BM2_1 | 424995 | media | true | 1 | 34.83 | Y |
| 322 | BGM_STORY_G10_2 | 416592 | media | true | 1 | 34.11 | Y |
| 323 | BGM_G07_REACH_BM1_1 | 415091 | media | true | 1 | 34.00 | Y |
| 324 | BGM_G01_REACH_7 | 413718 | media | true | 1 | 33.88 | Y |
| 325 | BGM_G01_REACH_6 | 413644 | media | true | 1 | 33.88 | Y |
| 326 | BGM_GACYA | 406609 | media | true | 1 | 33.31 | Y |
| 327 | BGM_G05_REACH_7 | 404012 | media | true | 1 | 33.10 | Y |
| 328 | BGM_G05_REACH_6 | 403444 | media | true | 1 | 33.06 | Y |
| 329 | BGM_G01_REACH_1 | 393585 | media | true | 1 | 32.23 | Y |
| 330 | BGM_G01_REACH_2 | 393555 | media | true | 1 | 32.24 | Y |
| 331 | BGM_G01_REACH_TH1_1 | 390860 | media | true | 1 | 32.00 | Y |
| 332 | BGM_G12_REACH_TH1_1 | 387992 | media | true | 1 | 31.79 | Y |
| 333 | BGM_G13_REACH_2 | 384870 | media | true | 1 | 31.51 | Y |
| 334 | BGM_G13_REACH_1 | 384577 | media | true | 1 | 31.50 | Y |
| 335 | BGM_G11_REACH_TH1_1 | 378620 | media | true | 1 | 31.02 | Y |
| 336 | BGM_G05_REACH_BM1_1 | 377751 | media | true | 1 | 30.95 | Y |
| 337 | BGM_G14_REACH_1 | 376637 | media | true | 1 | 30.84 | Y |
| 338 | BGM_TITLE | 376337 | media | false | 1 | 30.86 | Y |
| 339 | BGM_G04_REACH_7 | 374142 | media | true | 1 | 30.63 | Y |
| 340 | BGM_G04_REACH_6 | 373890 | media | true | 1 | 30.63 | Y |
| 341 | BGM_G14_REACH_2 | 373568 | media | true | 1 | 30.59 | Y |
| 342 | BGM_G05_REACH_1 | 362793 | media | true | 1 | 29.70 | Y |
| 343 | BGM_G05_REACH_2 | 362790 | media | true | 1 | 29.70 | Y |
| 344 | BGM_G19_REACH_2 | 357426 | media | true | 1 | 29.26 | Y |
| 345 | BGM_G19_REACH_1 | 356281 | media | true | 1 | 29.17 | Y |
| 346 | BGM_G08_REACH_BM1_1 | 355950 | media | true | 1 | 29.16 | Y |
| 347 | BGM_G06_REACH_BM1_1 | 355741 | media | true | 1 | 29.13 | Y |
| 348 | BGM_G02_REACH_7 | 354625 | media | true | 1 | 29.03 | Y |
| 349 | BGM_G02_REACH_6 | 354619 | media | true | 1 | 29.04 | Y |
| 350 | BGM_G10_REACH_TH1_1 | 353262 | media | true | 1 | 28.92 | Y |
| 351 | BGM_G08_REACH_2 | 353138 | media | true | 1 | 28.93 | Y |
| 352 | BGM_G08_REACH_1 | 353113 | media | true | 1 | 28.93 | Y |
| 353 | BGM_G15_REACH_2 | 342684 | media | true | 1 | 28.07 | Y |
| 354 | BGM_G04_REACH_2 | 339355 | media | true | 1 | 27.78 | Y |
| 355 | BGM_G04_REACH_1 | 339089 | media | true | 1 | 27.75 | Y |
| 356 | BGM_G08_REACH_7 | 332848 | media | true | 1 | 27.26 | Y |
| 357 | BGM_G08_REACH_6 | 332574 | media | true | 1 | 27.22 | Y |
| 358 | BGM_G16_REACH_2 | 331221 | media | true | 1 | 27.08 | Y |
| 359 | BGM_G16_REACH_1 | 331094 | media | true | 1 | 27.08 | Y |
| 360 | BGM_G06_REACH_2 | 328000 | media | true | 1 | 26.84 | Y |
| 361 | BGM_G07_REACH_2 | 327738 | media | true | 1 | 26.82 | Y |
| 362 | BGM_G06_REACH_1 | 327467 | media | true | 1 | 26.80 | Y |
| 363 | BGM_G14_REACH_BM2_1 | 327455 | media | true | 1 | 26.80 | Y |
| 364 | BGM_G15_REACH_1 | 327428 | media | true | 1 | 26.81 | Y |
| 365 | BGM_G07_REACH_1 | 327413 | media | true | 1 | 26.82 | Y |
| 366 | BGM_G06_REACH_7 | 324025 | media | true | 1 | 26.54 | Y |
| 367 | BGM_G06_REACH_6 | 323473 | media | true | 1 | 26.49 | Y |
| 368 | BGM_G12_REACH_2 | 317543 | media | true | 1 | 26.00 | Y |
| 369 | BGM_G12_REACH_1 | 316966 | media | true | 1 | 25.94 | Y |
| 370 | BGM_G17_REACH_1 | 315907 | media | true | 1 | 25.84 | Y |
| 371 | BGM_G17_REACH_2 | 315623 | media | true | 1 | 25.84 | Y |
| 372 | BGM_G03_REACH_BM3_1 | 311600 | media | true | 1 | 25.50 | Y |
| 373 | BGM_G11_REACH_2 | 304304 | media | true | 1 | 24.91 | Y |
| 374 | BGM_G11_REACH_1 | 304276 | media | true | 1 | 24.90 | Y |
| 375 | BGM_G10_REACH_2 | 300991 | media | true | 1 | 24.63 | Y |
| 376 | BGM_G03_REACH_2 | 300310 | media | true | 1 | 24.58 | Y |
| 377 | BGM_G03_REACH_1 | 298913 | media | true | 1 | 24.45 | Y |
| 378 | BGM_G10_REACH_1 | 298839 | media | true | 1 | 24.43 | Y |
| 379 | BGM_G03_REACH_BM1_1 | 297568 | media | true | 1 | 24.36 | Y |
| 380 | BGM_RESULT3 | 286765 | media | true | 1 | 23.47 | Y |
| 381 | BGM_G11_REACH_BM1_1 | 281394 | media | true | 1 | 23.03 | Y |
| 382 | BGM_G02_REACH_1 | 278595 | media | true | 1 | 22.80 | Y |
| 383 | BGM_G02_REACH_2 | 278550 | media | true | 1 | 22.80 | Y |
| 384 | BGM_MENU | 270715 | media | true | 1 | 22.15 | Y |
| 385 | BGM_G09_REACH_TH1_1 | 269185 | media | true | 1 | 22.02 | Y |
| 386 | BGM_RESULT_KAC1 | 268620 | media | true | 1 | 21.98 | N |
| 387 | BGM_RESULT2 | 251690 | media | true | 1 | 20.57 | Y |
| 388 | BGM_MOVIE_B | 245751 | media | false | 1 | 20.11 | Y |
| 389 | BGM_G20_REACH_2 | 242662 | media | true | 1 | 19.85 | Y |
| 390 | BGM_G20_REACH_1 | 242332 | media | true | 1 | 19.82 | Y |
| 391 | BGM_G09_REACH_2 | 239940 | media | true | 1 | 19.60 | Y |
| 392 | BGM_G09_REACH_1 | 239213 | media | true | 1 | 19.56 | Y |
| 393 | BGM_CASTING | 201974 | media | true | 1 | 16.51 | Y |
| 394 | BGM_RESULT1 | 178586 | media | true | 1 | 14.58 | Y |
| 395 | BGM_STORY6 | 153420 | media | true | 1 | 12.51 | Y |
| 396 | BGM_STORY7 | 130039 | buffer | true | 1 | 10.58 | Y |
| 397 | BGM_THUMB_G19_REACH_2 | 124063 | buffer | true | 1 | 10.10 | Y |
| 398 | BGM_THUMB_G07_REACH_BM3_2 | 123974 | buffer | true | 1 | 10.10 | Y |
| 399 | BGM_THUMB_G05_REACH_6 | 123957 | buffer | mixed | 2 | 10.10 | Y |
| 400 | BGM_THUMB_G15_REACH_BM2_2 | 123100 | buffer | true | 1 | 10.00 | Y |
| 401 | BGM_THUMB_G15_REACH_1 | 123033 | buffer | true | 1 | 10.00 | Y |
| 402 | BGM_THUMB_G12_REACH_BM1_1 | 123013 | buffer | true | 1 | 10.00 | Y |
| 403 | BGM_THUMB_G01_REACH_BM3_1 | 123004 | buffer | true | 1 | 10.00 | Y |
| 404 | BGM_THUMB_G15_REACH_3 | 122984 | buffer | true | 1 | 10.00 | Y |
| 405 | BGM_THUMB_G16_REACH_2 | 122979 | buffer | true | 1 | 10.00 | Y |
| 406 | BGM_THUMB_G06_REACH_4 | 122978 | buffer | true | 1 | 10.00 | Y |
| 407 | BGM_THUMB_G17_REACH_BM3_1 | 122977 | buffer | true | 1 | 10.00 | Y |
| 408 | BGM_THUMB_G04_REACH_TH1_2 | 122977 | buffer | true | 1 | 10.00 | Y |
| 409 | BGM_THUMB_STORY_G06 | 122942 | buffer | true | 1 | 10.00 | Y |
| 410 | BGM_THUMB_G02_REACH_BM1_1 | 122939 | buffer | true | 1 | 10.00 | Y |
| 411 | BGM_THUMB_G06_REACH_3 | 122934 | buffer | true | 1 | 10.00 | Y |
| 412 | BGM_THUMB_G18_REACH_3 | 122931 | buffer | true | 1 | 10.00 | Y |
| 413 | BGM_THUMB_G03_REACH_TH1_1 | 122930 | buffer | true | 1 | 10.00 | Y |
| 414 | BGM_THUMB_G07_REACH_BM1_2 | 122926 | buffer | true | 1 | 10.00 | Y |
| 415 | BGM_THUMB_G06_REACH_BM1_1 | 122924 | buffer | true | 1 | 10.00 | Y |
| 416 | BGM_THUMB_G03_REACH_BM3_2 | 122924 | buffer | true | 1 | 10.00 | Y |
| 417 | BGM_THUMB_G06_REACH_BM1_2 | 122918 | buffer | true | 1 | 10.00 | Y |
| 418 | BGM_THUMB_G04_REACH_8 | 122913 | buffer | mixed | 2 | 10.01 | Y |
| 419 | BGM_THUMB_G04_REACH_BM1_2 | 122913 | buffer | true | 1 | 10.00 | Y |
| 420 | BGM_THUMB_G14_REACH_BM2_2 | 122905 | buffer | true | 1 | 10.00 | Y |
| 421 | BGM_THUMB_G16_REACH_4 | 122904 | buffer | true | 1 | 10.00 | Y |
| 422 | BGM_THUMB_G10_REACH_TH1_2 | 122903 | buffer | true | 1 | 10.00 | Y |
| 423 | BGM_THUMB_G02_REACH_4 | 122902 | buffer | true | 1 | 10.00 | Y |
| 424 | BGM_THUMB_G09_REACH_BM3_2 | 122901 | buffer | true | 1 | 10.00 | Y |
| 425 | BGM_THUMB_G01_REACH_9 | 122900 | buffer | mixed | 2 | 10.00 | Y |
| 426 | BGM_THUMB_G05_REACH_TH1_2 | 122898 | buffer | true | 1 | 10.00 | Y |
| 427 | BGM_THUMB_G09_REACH_BM2_2 | 122896 | buffer | true | 1 | 10.00 | Y |
| 428 | BGM_THUMB_G09_REACH_BM3_1 | 122894 | buffer | true | 1 | 10.00 | Y |
| 429 | BGM_THUMB_G16_REACH_3 | 122891 | buffer | true | 1 | 10.00 | Y |
| 430 | BGM_THUMB_G18_REACH_1 | 122888 | buffer | true | 1 | 10.00 | Y |
| 431 | BGM_THUMB_G14_REACH_4 | 122885 | buffer | true | 1 | 10.00 | Y |
| 432 | BGM_THUMB_G08_REACH_2 | 122882 | buffer | true | 1 | 10.00 | Y |
| 433 | BGM_THUMB_G13_REACH_3 | 122879 | buffer | true | 1 | 10.00 | Y |
| 434 | BGM_THUMB_G17_REACH_BM3_2 | 122879 | buffer | true | 1 | 10.00 | Y |
| 435 | BGM_THUMB_G12_REACH_TH1_1 | 122878 | buffer | true | 1 | 10.00 | Y |
| 436 | BGM_THUMB_G15_REACH_4 | 122877 | buffer | true | 1 | 10.00 | Y |
| 437 | BGM_THUMB_HOME_KAC1 | 122873 | buffer | true | 1 | 10.00 | Y |
| 438 | BGM_THUMB_G19_REACH_3 | 122872 | buffer | true | 1 | 10.00 | Y |
| 439 | BGM_THUMB_G16_REACH_BM3_2 | 122872 | buffer | true | 1 | 10.00 | Y |
| 440 | BGM_THUMB_G10_REACH_BM1_1 | 122871 | buffer | true | 1 | 10.00 | Y |
| 441 | BGM_THUMB_G12_REACH_BM2_2 | 122871 | buffer | true | 1 | 10.00 | Y |
| 442 | BGM_THUMB_G14_REACH_3 | 122870 | buffer | true | 1 | 10.00 | Y |
| 443 | BGM_THUMB_G09_REACH_BM1_2 | 122870 | buffer | true | 1 | 10.00 | Y |
| 444 | BGM_THUMB_G01_REACH_BM2_1 | 122869 | buffer | true | 1 | 10.00 | Y |
| 445 | BGM_THUMB_G09_REACH_3 | 122868 | buffer | true | 1 | 10.00 | Y |
| 446 | BGM_THUMB_G05_REACH_BM1_1 | 122864 | buffer | true | 1 | 10.00 | Y |
| 447 | BGM_THUMB_G10_REACH_TH1_1 | 122863 | buffer | true | 1 | 10.00 | Y |
| 448 | BGM_THUMB_G06_REACH_BM3_2 | 122860 | buffer | true | 1 | 10.00 | Y |
| 449 | BGM_THUMB_G02_REACH_3 | 122859 | buffer | true | 1 | 10.00 | Y |
| 450 | BGM_THUMB_G09_REACH_BM2_1 | 122859 | buffer | true | 1 | 10.00 | Y |
| 451 | BGM_THUMB_STORY_G02 | 122859 | buffer | true | 1 | 10.00 | Y |
| 452 | BGM_THUMB_G02_REACH_BM2_2 | 122858 | buffer | true | 1 | 10.00 | Y |
| 453 | BGM_THUMB_G11_REACH_BM2_2 | 122858 | buffer | true | 1 | 10.00 | Y |
| 454 | BGM_THUMB_G19_REACH_1 | 122857 | buffer | true | 1 | 10.00 | Y |
| 455 | BGM_THUMB_G18_REACH_4 | 122856 | buffer | true | 1 | 10.00 | Y |
| 456 | BGM_THUMB_G15_REACH_BM3_2 | 122856 | buffer | true | 1 | 10.00 | Y |
| 457 | BGM_THUMB_HOME_STORY2 | 122856 | buffer | true | 1 | 10.00 | Y |
| 458 | BGM_THUMB_G01_REACH_1 | 122855 | buffer | true | 1 | 10.00 | Y |
| 459 | BGM_THUMB_G02_REACH_6 | 122855 | buffer | mixed | 2 | 10.00 | Y |
| 460 | BGM_THUMB_G06_REACH_6 | 122855 | buffer | mixed | 2 | 10.00 | Y |
| 461 | BGM_THUMB_G11_REACH_BM1_1 | 122855 | buffer | true | 1 | 10.00 | Y |
| 462 | BGM_THUMB_HOME_SONG2 | 122851 | buffer | true | 1 | 10.00 | Y |
| 463 | BGM_THUMB_G06_REACH_1 | 122850 | buffer | true | 1 | 10.00 | Y |
| 464 | BGM_THUMB_G03_REACH_BM2_2 | 122850 | buffer | true | 1 | 10.00 | Y |
| 465 | BGM_THUMB_G06_REACH_TH1_2 | 122850 | buffer | true | 1 | 10.00 | Y |
| 466 | BGM_THUMB_G13_REACH_1 | 122847 | buffer | true | 1 | 10.00 | Y |
| 467 | BGM_THUMB_G11_REACH_BM2_1 | 122847 | buffer | true | 1 | 10.00 | Y |
| 468 | BGM_THUMB_G08_REACH_TH1_1 | 122846 | buffer | true | 1 | 10.00 | Y |
| 469 | BGM_THUMB_G11_REACH_TH1_1 | 122845 | buffer | true | 1 | 10.00 | Y |
| 470 | BGM_THUMB_G05_REACH_8 | 122844 | buffer | mixed | 2 | 10.00 | Y |
| 471 | BGM_THUMB_G06_REACH_2 | 122844 | buffer | true | 1 | 10.00 | Y |
| 472 | BGM_THUMB_G08_REACH_BM3_2 | 122844 | buffer | true | 1 | 10.00 | Y |
| 473 | BGM_THUMB_G12_REACH_TH1_2 | 122844 | buffer | true | 1 | 10.00 | Y |
| 474 | BGM_THUMB_STORY_G09 | 122844 | buffer | true | 1 | 10.00 | Y |
| 475 | BGM_THUMB_G11_REACH_BM3_1 | 122843 | buffer | true | 1 | 10.00 | Y |
| 476 | BGM_THUMB_G04_REACH_9 | 122842 | buffer | mixed | 2 | 10.00 | Y |
| 477 | BGM_THUMB_HOME_STORY1 | 122842 | buffer | true | 1 | 10.00 | Y |
| 478 | BGM_THUMB_G07_REACH_6 | 122841 | buffer | mixed | 2 | 10.00 | Y |
| 479 | BGM_THUMB_G10_REACH_3 | 122841 | buffer | true | 1 | 10.00 | Y |
| 480 | BGM_THUMB_G07_REACH_BM2_1 | 122841 | buffer | true | 1 | 10.00 | Y |
| 481 | BGM_THUMB_G09_REACH_4 | 122838 | buffer | true | 1 | 10.00 | Y |
| 482 | BGM_THUMB_G02_REACH_BM1_2 | 122838 | buffer | true | 1 | 10.00 | Y |
| 483 | BGM_THUMB_G13_REACH_4 | 122837 | buffer | true | 1 | 10.00 | Y |
| 484 | BGM_THUMB_G05_REACH_BM3_2 | 122837 | buffer | true | 1 | 10.00 | Y |
| 485 | BGM_THUMB_G11_REACH_TH1_2 | 122837 | buffer | true | 1 | 10.00 | Y |
| 486 | BGM_THUMB_G17_REACH_4 | 122835 | buffer | true | 1 | 10.00 | Y |
| 487 | BGM_THUMB_HOME_XMAS1 | 122835 | buffer | true | 1 | 10.00 | Y |
| 488 | BGM_THUMB_G07_REACH_BM2_2 | 122833 | buffer | true | 1 | 10.00 | Y |
| 489 | BGM_THUMB_G21_REACH_2 | 122832 | buffer | true | 1 | 10.00 | Y |
| 490 | BGM_THUMB_REACH1 | 122831 | buffer | true | 1 | 10.00 | Y |
| 491 | BGM_THUMB_G08_REACH_1 | 122830 | buffer | true | 1 | 10.00 | Y |
| 492 | BGM_THUMB_G10_REACH_BM2_1 | 122830 | buffer | true | 1 | 10.00 | Y |
| 493 | BGM_THUMB_G02_REACH_BM2_1 | 122829 | buffer | true | 1 | 10.00 | Y |
| 494 | BGM_THUMB_G05_REACH_7 | 122828 | buffer | mixed | 2 | 10.00 | Y |
| 495 | BGM_THUMB_G12_REACH_BM2_1 | 122828 | buffer | true | 1 | 10.00 | Y |
| 496 | BGM_THUMB_G08_REACH_BM3_1 | 122828 | buffer | true | 1 | 10.00 | Y |
| 497 | BGM_THUMB_G10_REACH_2 | 122827 | buffer | true | 1 | 10.00 | Y |
| 498 | BGM_THUMB_G08_REACH_BM2_2 | 122826 | buffer | true | 1 | 10.00 | Y |
| 499 | BGM_THUMB_G03_REACH_TH1_2 | 122826 | buffer | true | 1 | 10.00 | Y |
| 500 | BGM_THUMB_G07_REACH_TH1_1 | 122825 | buffer | true | 1 | 10.00 | Y |
| 501 | BGM_THUMB_G17_REACH_2 | 122824 | buffer | true | 1 | 10.00 | Y |
| 502 | BGM_THUMB_G11_REACH_BM1_2 | 122824 | buffer | true | 1 | 10.00 | Y |
| 503 | BGM_THUMB_G04_REACH_7 | 122822 | buffer | mixed | 2 | 10.00 | Y |
| 504 | BGM_THUMB_G15_REACH_BM3_1 | 122822 | buffer | true | 1 | 10.00 | Y |
| 505 | BGM_THUMB_G09_REACH_TH1_1 | 122822 | buffer | true | 1 | 10.00 | Y |
| 506 | BGM_THUMB_G02_REACH_8 | 122821 | buffer | mixed | 2 | 10.00 | Y |
| 507 | BGM_THUMB_G08_REACH_9 | 122821 | buffer | mixed | 2 | 10.00 | Y |
| 508 | BGM_THUMB_G20_REACH_1 | 122821 | buffer | true | 1 | 10.00 | Y |
| 509 | BGM_THUMB_G07_REACH_BM3_1 | 122821 | buffer | true | 1 | 10.00 | Y |
| 510 | BGM_THUMB_G08_REACH_BM1_2 | 122820 | buffer | true | 1 | 10.00 | Y |
| 511 | BGM_THUMB_G04_REACH_BM2_2 | 122820 | buffer | true | 1 | 10.00 | Y |
| 512 | BGM_THUMB_G10_REACH_BM3_1 | 122820 | buffer | true | 1 | 10.00 | Y |
| 513 | BGM_THUMB_G08_REACH_8 | 122817 | buffer | mixed | 2 | 10.00 | Y |
| 514 | BGM_THUMB_G21_REACH_3 | 122817 | buffer | true | 1 | 10.00 | Y |
| 515 | BGM_THUMB_G14_REACH_BM2_1 | 122817 | buffer | true | 1 | 10.00 | Y |
| 516 | BGM_THUMB_G03_REACH_3 | 122815 | buffer | true | 1 | 10.00 | Y |
| 517 | BGM_THUMB_HOME_TOHO2 | 122815 | buffer | true | 1 | 10.00 | Y |
| 518 | BGM_THUMB_G03_REACH_BM3_1 | 122814 | buffer | true | 1 | 10.00 | Y |
| 519 | BGM_THUMB_G01_REACH_2 | 122813 | buffer | true | 1 | 10.00 | Y |
| 520 | BGM_THUMB_G05_REACH_9 | 122813 | buffer | mixed | 2 | 10.00 | Y |
| 521 | BGM_THUMB_G21_REACH_4 | 122813 | buffer | true | 1 | 10.00 | Y |
| 522 | BGM_THUMB_G20_REACH_BM3_1 | 122813 | buffer | true | 1 | 10.00 | Y |
| 523 | BGM_THUMB_G01_REACH_BM2_2 | 122812 | buffer | true | 1 | 10.00 | Y |
| 524 | BGM_THUMB_G02_REACH_1 | 122811 | buffer | true | 1 | 10.00 | Y |
| 525 | BGM_THUMB_G10_REACH_BM1_2 | 122811 | buffer | true | 1 | 10.00 | Y |
| 526 | BGM_THUMB_G08_REACH_TH1_2 | 122811 | buffer | true | 1 | 10.00 | Y |
| 527 | BGM_THUMB_G01_REACH_7 | 122810 | buffer | mixed | 2 | 10.00 | Y |
| 528 | BGM_THUMB_G05_REACH_BM1_2 | 122810 | buffer | true | 1 | 10.00 | Y |
| 529 | BGM_THUMB_G02_REACH_7 | 122809 | buffer | mixed | 2 | 10.00 | Y |
| 530 | BGM_THUMB_G04_REACH_BM3_1 | 122809 | buffer | true | 1 | 10.00 | Y |
| 531 | BGM_THUMB_G14_REACH_1 | 122808 | buffer | true | 1 | 10.00 | Y |
| 532 | BGM_THUMB_G14_REACH_2 | 122808 | buffer | true | 1 | 10.00 | Y |
| 533 | BGM_THUMB_G16_REACH_1 | 122808 | buffer | true | 1 | 10.00 | Y |
| 534 | BGM_THUMB_HOME_SONG1 | 122808 | buffer | true | 1 | 10.00 | Y |
| 535 | BGM_THUMB_G05_REACH_BM2_1 | 122807 | buffer | true | 1 | 10.00 | Y |
| 536 | BGM_THUMB_G10_REACH_BM2_2 | 122807 | buffer | true | 1 | 10.00 | Y |
| 537 | BGM_THUMB_G13_REACH_BM2_2 | 122807 | buffer | true | 1 | 10.00 | Y |
| 538 | BGM_THUMB_G16_REACH_BM2_1 | 122807 | buffer | true | 1 | 10.00 | Y |
| 539 | BGM_THUMB_G10_REACH_BM3_2 | 122807 | buffer | true | 1 | 10.00 | Y |
| 540 | BGM_THUMB_G19_REACH_BM3_1 | 122807 | buffer | true | 1 | 10.00 | Y |
| 541 | BGM_THUMB_G07_REACH_TH1_2 | 122807 | buffer | true | 1 | 10.00 | Y |
| 542 | BGM_THUMB_G04_REACH_BM1_1 | 122806 | buffer | true | 1 | 10.00 | Y |
| 543 | BGM_THUMB_G02_REACH_BM3_2 | 122806 | buffer | true | 1 | 10.00 | Y |
| 544 | BGM_THUMB_G11_REACH_BM3_2 | 122806 | buffer | true | 1 | 10.00 | Y |
| 545 | BGM_THUMB_G07_REACH_2 | 122803 | buffer | true | 1 | 10.00 | Y |
| 546 | BGM_THUMB_G12_REACH_2 | 122803 | buffer | true | 1 | 10.00 | Y |
| 547 | BGM_THUMB_G15_REACH_2 | 122803 | buffer | true | 1 | 10.00 | Y |
| 548 | BGM_THUMB_G01_REACH_BM1_2 | 122803 | buffer | true | 1 | 10.00 | Y |
| 549 | BGM_THUMB_G01_REACH_TH1_2 | 122803 | buffer | true | 1 | 10.00 | Y |
| 550 | BGM_THUMB_STORY_G05 | 122803 | buffer | true | 1 | 10.00 | Y |
| 551 | BGM_THUMB_G04_REACH_2 | 122802 | buffer | true | 1 | 10.00 | Y |
| 552 | BGM_THUMB_G04_REACH_1 | 122801 | buffer | true | 1 | 10.00 | Y |
| 553 | BGM_THUMB_G05_REACH_1 | 122801 | buffer | true | 1 | 10.00 | Y |
| 554 | BGM_THUMB_G05_REACH_BM3_1 | 122801 | buffer | true | 1 | 10.00 | Y |
| 555 | BGM_THUMB_STORY_G03 | 122801 | buffer | true | 1 | 10.00 | Y |
| 556 | BGM_THUMB_G05_REACH_2 | 122800 | buffer | true | 1 | 10.00 | Y |
| 557 | BGM_THUMB_G06_REACH_7 | 122800 | buffer | mixed | 2 | 10.00 | Y |
| 558 | BGM_THUMB_G10_REACH_1 | 122800 | buffer | true | 1 | 10.00 | Y |
| 559 | BGM_THUMB_STORY_G10 | 122800 | buffer | true | 1 | 10.00 | Y |
| 560 | BGM_THUMB_G04_REACH_3 | 122799 | buffer | true | 1 | 10.00 | Y |
| 561 | BGM_THUMB_G05_REACH_3 | 122799 | buffer | true | 1 | 10.00 | Y |
| 562 | BGM_THUMB_G13_REACH_2 | 122799 | buffer | true | 1 | 10.00 | Y |
| 563 | BGM_THUMB_G09_REACH_1 | 122798 | buffer | true | 1 | 10.00 | Y |
| 564 | BGM_THUMB_G18_REACH_2 | 122798 | buffer | true | 1 | 10.00 | Y |
| 565 | BGM_THUMB_G07_REACH_BM1_1 | 122798 | buffer | true | 1 | 10.00 | Y |
| 566 | BGM_THUMB_G20_REACH_2 | 122797 | buffer | true | 1 | 10.00 | Y |
| 567 | BGM_THUMB_G07_REACH_1 | 122796 | buffer | true | 1 | 10.00 | Y |
| 568 | BGM_THUMB_G01_REACH_4 | 122795 | buffer | true | 1 | 10.00 | Y |
| 569 | BGM_THUMB_G06_REACH_9 | 122795 | buffer | mixed | 2 | 10.00 | Y |
| 570 | BGM_THUMB_G13_REACH_BM3_2 | 122795 | buffer | true | 1 | 10.00 | Y |
| 571 | BGM_THUMB_G05_REACH_4 | 122794 | buffer | true | 1 | 10.00 | Y |
| 572 | BGM_THUMB_G11_REACH_3 | 122794 | buffer | true | 1 | 10.00 | Y |
| 573 | BGM_THUMB_G14_REACH_BM3_2 | 122794 | buffer | true | 1 | 10.00 | Y |
| 574 | BGM_THUMB_G06_REACH_TH1_1 | 122793 | buffer | true | 1 | 10.00 | Y |
| 575 | BGM_THUMB_HOME_TOHO1 | 122793 | buffer | true | 1 | 10.00 | Y |
| 576 | BGM_THUMB_G11_REACH_4 | 122792 | buffer | true | 1 | 10.00 | Y |
| 577 | BGM_THUMB_G21_REACH_1 | 122792 | buffer | true | 1 | 10.00 | Y |
| 578 | BGM_THUMB_G04_REACH_BM3_2 | 122792 | buffer | true | 1 | 10.00 | Y |
| 579 | BGM_THUMB_G13_REACH_BM3_1 | 122792 | buffer | true | 1 | 10.00 | Y |
| 580 | BGM_THUMB_G20_REACH_BM3_2 | 122792 | buffer | true | 1 | 10.00 | Y |
| 581 | BGM_THUMB_STORY_G07 | 122792 | buffer | true | 1 | 10.00 | Y |
| 582 | BGM_THUMB_G04_REACH_6 | 122791 | buffer | mixed | 2 | 10.00 | Y |
| 583 | BGM_THUMB_G19_REACH_4 | 122791 | buffer | true | 1 | 10.00 | Y |
| 584 | BGM_THUMB_G05_REACH_BM2_2 | 122791 | buffer | true | 1 | 10.00 | Y |
| 585 | BGM_THUMB_G14_REACH_BM3_1 | 122790 | buffer | true | 1 | 10.00 | Y |
| 586 | BGM_THUMB_G04_REACH_4 | 122789 | buffer | true | 1 | 10.00 | Y |
| 587 | BGM_THUMB_G16_REACH_BM3_1 | 122789 | buffer | true | 1 | 10.00 | Y |
| 588 | BGM_THUMB_G08_REACH_3 | 122788 | buffer | true | 1 | 10.00 | Y |
| 589 | BGM_THUMB_G04_REACH_BM2_1 | 122788 | buffer | true | 1 | 10.00 | Y |
| 590 | BGM_THUMB_G01_REACH_BM3_2 | 122788 | buffer | true | 1 | 10.00 | Y |
| 591 | BGM_THUMB_G02_REACH_BM3_1 | 122788 | buffer | true | 1 | 10.00 | Y |
| 592 | BGM_THUMB_STORY_G12 | 122788 | buffer | true | 1 | 10.00 | Y |
| 593 | BGM_THUMB_G01_REACH_6 | 122787 | buffer | mixed | 2 | 10.00 | Y |
| 594 | BGM_THUMB_G10_REACH_4 | 122787 | buffer | true | 1 | 10.00 | Y |
| 595 | BGM_THUMB_G13_REACH_BM2_1 | 122787 | buffer | true | 1 | 10.00 | Y |
| 596 | BGM_THUMB_G18_REACH_BM3_1 | 122787 | buffer | true | 1 | 10.00 | Y |
| 597 | BGM_THUMB_G02_REACH_TH1_2 | 122787 | buffer | true | 1 | 10.00 | Y |
| 598 | BGM_THUMB_G13_REACH_TH1_1 | 122787 | buffer | true | 1 | 10.00 | Y |
| 599 | BGM_THUMB_G01_REACH_TH1_1 | 122786 | buffer | true | 1 | 10.00 | Y |
| 600 | BGM_THUMB_STORY_G04 | 122786 | buffer | true | 1 | 10.00 | Y |
| 601 | BGM_THUMB_HOME_BGR1 | 122786 | buffer | true | 1 | 10.00 | Y |
| 602 | BGM_THUMB_G11_REACH_1 | 122785 | buffer | true | 1 | 10.00 | Y |
| 603 | BGM_THUMB_G08_REACH_BM1_1 | 122784 | buffer | true | 1 | 10.00 | Y |
| 604 | BGM_THUMB_G19_REACH_BM3_2 | 122784 | buffer | true | 1 | 10.00 | Y |
| 605 | BGM_THUMB_HOME | 122784 | buffer | true | 1 | 10.00 | Y |
| 606 | BGM_THUMB_G06_REACH_8 | 122783 | buffer | mixed | 2 | 10.00 | Y |
| 607 | BGM_THUMB_G07_REACH_9 | 122783 | buffer | mixed | 2 | 10.00 | Y |
| 608 | BGM_THUMB_G09_REACH_BM1_1 | 122783 | buffer | true | 1 | 10.00 | Y |
| 609 | BGM_THUMB_STORY_G08 | 122783 | buffer | true | 1 | 10.00 | Y |
| 610 | BGM_THUMB_STORY_G11 | 122783 | buffer | true | 1 | 10.00 | Y |
| 611 | BGM_THUMB_G03_REACH_1 | 122782 | buffer | true | 1 | 10.00 | Y |
| 612 | BGM_THUMB_G02_REACH_9 | 122781 | buffer | mixed | 2 | 10.00 | Y |
| 613 | BGM_THUMB_G08_REACH_6 | 122781 | buffer | mixed | 2 | 10.00 | Y |
| 614 | BGM_THUMB_G09_REACH_TH1_2 | 122781 | buffer | true | 1 | 10.00 | Y |
| 615 | BGM_THUMB_STORY_G01 | 122781 | buffer | true | 1 | 10.00 | Y |
| 616 | BGM_THUMB_G01_REACH_8 | 122780 | buffer | mixed | 2 | 10.00 | Y |
| 617 | BGM_THUMB_G12_REACH_BM1_2 | 122780 | buffer | true | 1 | 10.00 | Y |
| 618 | BGM_THUMB_G08_REACH_BM2_1 | 122780 | buffer | true | 1 | 10.00 | Y |
| 619 | BGM_THUMB_G07_REACH_3 | 122779 | buffer | true | 1 | 10.00 | Y |
| 620 | BGM_THUMB_G03_REACH_BM1_2 | 122779 | buffer | true | 1 | 10.00 | Y |
| 621 | BGM_THUMB_G03_REACH_BM2_1 | 122779 | buffer | true | 1 | 10.00 | Y |
| 622 | BGM_THUMB_G20_REACH_3 | 122778 | buffer | true | 1 | 10.00 | Y |
| 623 | BGM_THUMB_G15_REACH_BM2_1 | 122778 | buffer | true | 1 | 10.00 | Y |
| 624 | BGM_THUMB_G05_REACH_TH1_1 | 122777 | buffer | true | 1 | 10.00 | Y |
| 625 | BGM_THUMB_G07_REACH_4 | 122776 | buffer | true | 1 | 10.00 | Y |
| 626 | BGM_THUMB_G06_REACH_BM2_1 | 122776 | buffer | true | 1 | 10.00 | Y |
| 627 | BGM_THUMB_G02_REACH_2 | 122775 | buffer | true | 1 | 10.00 | Y |
| 628 | BGM_THUMB_G03_REACH_2 | 122775 | buffer | true | 1 | 10.00 | Y |
| 629 | BGM_THUMB_G07_REACH_7 | 122775 | buffer | mixed | 2 | 10.00 | Y |
| 630 | BGM_THUMB_G07_REACH_8 | 122775 | buffer | mixed | 2 | 10.00 | Y |
| 631 | BGM_THUMB_G06_REACH_BM3_1 | 122774 | buffer | true | 1 | 10.00 | Y |
| 632 | BGM_THUMB_G18_REACH_BM3_2 | 122774 | buffer | true | 1 | 10.00 | Y |
| 633 | BGM_THUMB_G02_REACH_TH1_1 | 122773 | buffer | true | 1 | 10.00 | Y |
| 634 | BGM_THUMB_G12_REACH_3 | 122772 | buffer | true | 1 | 10.00 | Y |
| 635 | BGM_THUMB_G12_REACH_BM3_2 | 122772 | buffer | true | 1 | 10.00 | Y |
| 636 | BGM_THUMB_G12_REACH_4 | 122771 | buffer | true | 1 | 10.00 | Y |
| 637 | BGM_THUMB_G20_REACH_4 | 122771 | buffer | true | 1 | 10.00 | Y |
| 638 | BGM_THUMB_G08_REACH_7 | 122769 | buffer | mixed | 2 | 10.00 | Y |
| 639 | BGM_THUMB_G16_REACH_BM2_2 | 122769 | buffer | true | 1 | 10.00 | Y |
| 640 | BGM_THUMB_G06_REACH_BM2_2 | 122767 | buffer | true | 1 | 10.00 | Y |
| 641 | BGM_THUMB_G04_REACH_TH1_1 | 122767 | buffer | true | 1 | 10.00 | Y |
| 642 | BGM_THUMB_G13_REACH_TH1_2 | 122767 | buffer | true | 1 | 10.00 | Y |
| 643 | BGM_THUMB_G03_REACH_4 | 122762 | buffer | true | 1 | 10.00 | Y |
| 644 | BGM_THUMB_G08_REACH_4 | 122758 | buffer | true | 1 | 10.00 | Y |
| 645 | BGM_THUMB_G09_REACH_2 | 122758 | buffer | true | 1 | 10.00 | Y |
| 646 | BGM_THUMB_G17_REACH_1 | 122758 | buffer | true | 1 | 10.00 | Y |
| 647 | BGM_THUMB_G01_REACH_3 | 122753 | buffer | true | 1 | 10.00 | Y |
| 648 | BGM_THUMB_G01_REACH_BM1_1 | 122751 | buffer | true | 1 | 10.00 | Y |
| 649 | BGM_THUMB_G17_REACH_3 | 122749 | buffer | true | 1 | 10.00 | Y |
| 650 | BGM_THUMB_G11_REACH_2 | 122747 | buffer | true | 1 | 10.00 | Y |
| 651 | BGM_THUMB_G12_REACH_1 | 122745 | buffer | true | 1 | 10.00 | Y |
| 652 | BGM_THUMB_G03_REACH_BM1_1 | 122743 | buffer | true | 1 | 10.00 | Y |
| 653 | BGM_THUMB_G12_REACH_BM3_1 | 122736 | buffer | true | 1 | 10.00 | Y |
| 654 | BGM_THUMB_HOME_NEWYEAR1 | 122526 | buffer | true | 1 | 10.00 | Y |
| 655 | BGM_THUMB_HOME_QMA1 | 121159 | buffer | true | 1 | 9.85 | Y |
| 656 | BGM_GAMEOVER | 118569 | buffer | false | 1 | 9.66 | Y |
| 657 | BGM_G20_REACH_5 | 114330 | buffer | false | 1 | 9.32 | Y |
| 658 | BGM_RESULT4 | 113590 | buffer | true | 1 | 9.23 | Y |
| 659 | BGM_G06_REACH_BM3_3 | 110373 | buffer | false | 1 | 8.99 | Y |
| 660 | BGM_G11_REACH_5 | 105244 | buffer | false | 1 | 8.57 | Y |
| 661 | BGM_G19_REACH_BM3_3 | 104837 | buffer | false | 1 | 8.53 | Y |
| 662 | BGM_G19_REACH_5 | 102671 | buffer | false | 1 | 8.35 | Y |
| 663 | BGM_G17_REACH_BM3_3 | 101424 | buffer | false | 1 | 8.23 | Y |
| 664 | BGM_RESULT5 | 100753 | buffer | true | 1 | 8.19 | Y |
| 665 | BGM_G10_REACH_BM3_3 | 100213 | buffer | false | 1 | 8.16 | Y |
| 666 | BGM_G03_REACH_BM3_3 | 98562 | buffer | false | 1 | 8.00 | Y |
| 667 | BGM_G03_REACH_BM2_3 | 98465 | buffer | false | 1 | 8.00 | Y |
| 668 | BGM_G16_REACH_BM3_3 | 97913 | buffer | false | 1 | 7.95 | Y |
| 669 | BGM_G21_REACH_5 | 96553 | buffer | false | 1 | 7.85 | Y |
| 670 | BGM_G11_REACH_BM3_3 | 95035 | buffer | false | 1 | 7.72 | Y |
| 671 | BGM_G15_REACH_BM3_3 | 95025 | buffer | false | 1 | 7.72 | Y |
| 672 | BGM_G18_REACH_BM3_3 | 94828 | buffer | false | 1 | 7.71 | Y |
| 673 | BGM_G12_REACH_5 | 94214 | buffer | false | 1 | 7.65 | Y |
| 674 | BGM_G01_REACH_10 | 93662 | buffer | false | 1 | 7.61 | Y |
| 675 | BGM_G12_REACH_BM3_3 | 91205 | buffer | false | 1 | 7.43 | Y |
| 676 | BGM_G20_REACH_BM3_3 | 90646 | buffer | false | 1 | 7.37 | Y |
| 677 | BGM_G08_REACH_10 | 89458 | buffer | false | 1 | 7.27 | Y |
| 678 | BGM_G05_REACH_BM3_3 | 87479 | buffer | false | 1 | 7.11 | Y |
| 679 | BGM_G04_REACH_TH1_3 | 84022 | buffer | false | 1 | 6.82 | Y |
| 680 | BGM_G10_REACH_BM2_3 | 83750 | buffer | false | 1 | 6.80 | Y |
| 681 | BGM_G08_REACH_TH1_3 | 81517 | buffer | false | 1 | 6.61 | Y |
| 682 | BGM_G12_REACH_TH1_3 | 80464 | buffer | false | 1 | 6.52 | Y |
| 683 | BGM_G12_REACH_BM1_3 | 79608 | buffer | false | 1 | 6.43 | Y |
| 684 | BGM_G17_REACH_5 | 79015 | buffer | false | 1 | 6.42 | Y |
| 685 | BGM_G06_REACH_10 | 78943 | buffer | false | 1 | 6.40 | Y |
| 686 | BGM_G11_REACH_TH1_3 | 76739 | buffer | false | 1 | 6.23 | Y |
| 687 | BGM_G09_REACH_BM1_3 | 76474 | buffer | false | 1 | 6.19 | Y |
| 688 | BGM_G07_REACH_TH1_3 | 76173 | buffer | false | 1 | 6.17 | Y |
| 689 | BGM_G03_REACH_TH1_3 | 76073 | buffer | false | 1 | 6.16 | Y |
| 690 | BGM_G06_REACH_5 | 75875 | buffer | false | 1 | 6.14 | Y |
| 691 | BGM_G08_REACH_5 | 75566 | buffer | false | 1 | 6.12 | Y |
| 692 | BGM_G02_REACH_10 | 75559 | buffer | false | 1 | 6.13 | Y |
| 693 | BGM_G05_REACH_BM1_3 | 74938 | buffer | false | 1 | 6.06 | Y |
| 694 | BGM_G05_REACH_5 | 73912 | buffer | false | 1 | 5.99 | Y |
| 695 | BGM_G07_REACH_5 | 73912 | buffer | false | 1 | 5.99 | Y |
| 696 | BGM_G07_REACH_BM1_3 | 73909 | buffer | false | 1 | 5.98 | Y |
| 697 | BGM_G13_REACH_5 | 73607 | buffer | false | 1 | 5.96 | Y |
| 698 | BGM_G03_REACH_5 | 73347 | buffer | false | 1 | 5.94 | Y |
| 699 | BGM_G11_REACH_BM2_3 | 73272 | buffer | false | 1 | 5.93 | Y |
| 700 | BGM_G04_REACH_BM3_3 | 72725 | buffer | false | 1 | 5.89 | Y |
| 701 | BGM_G14_REACH_BM2_3 | 72246 | buffer | false | 1 | 5.85 | Y |
| 702 | BGM_G07_REACH_BM3_3 | 72217 | buffer | false | 1 | 5.85 | Y |
| 703 | BGM_G08_REACH_BM3_3 | 71893 | buffer | false | 1 | 5.82 | Y |
| 704 | BGM_G12_REACH_BM2_3 | 71885 | buffer | false | 1 | 5.81 | Y |
| 705 | BGM_G13_REACH_BM2_3 | 71586 | buffer | false | 1 | 5.80 | Y |
| 706 | BGM_G18_REACH_5 | 71092 | buffer | false | 1 | 5.76 | Y |
| 707 | BGM_G10_REACH_BM1_3 | 70771 | buffer | false | 1 | 5.73 | Y |
| 708 | BGM_G14_REACH_5 | 70526 | buffer | false | 1 | 5.70 | Y |
| 709 | BGM_G08_REACH_BM2_3 | 69634 | buffer | false | 1 | 5.64 | Y |
| 710 | BGM_G05_REACH_10 | 69390 | buffer | false | 1 | 5.63 | Y |
| 711 | BGM_G01_REACH_5 | 69136 | buffer | false | 1 | 5.59 | Y |
| 712 | BGM_G07_REACH_10 | 68825 | buffer | false | 1 | 5.57 | Y |
| 713 | BGM_G07_REACH_BM2_3 | 68825 | buffer | false | 1 | 5.58 | Y |
| 714 | BGM_G02_REACH_BM3_3 | 68758 | buffer | false | 1 | 5.57 | Y |
| 715 | BGM_G03_REACH_BM1_3 | 68576 | buffer | false | 1 | 5.54 | Y |
| 716 | BGM_G08_REACH_BM1_3 | 68570 | buffer | false | 1 | 5.55 | Y |
| 717 | BGM_G16_REACH_5 | 67439 | buffer | false | 1 | 5.46 | Y |
| 718 | BGM_G02_REACH_BM1_3 | 67274 | buffer | false | 1 | 5.42 | Y |
| 719 | BGM_G04_REACH_BM1_3 | 66515 | buffer | false | 1 | 5.38 | Y |
| 720 | BGM_G09_REACH_5 | 65998 | buffer | false | 1 | 5.35 | Y |
| 721 | BGM_G02_REACH_5 | 65977 | buffer | false | 1 | 5.33 | Y |
| 722 | BGM_G09_REACH_BM3_3 | 65366 | buffer | false | 1 | 5.29 | Y |
| 723 | BGM_G01_REACH_BM1_3 | 64595 | buffer | false | 1 | 5.23 | Y |
| 724 | BGM_G01_REACH_BM2_3 | 63989 | buffer | false | 1 | 5.17 | Y |
| 725 | BGM_G10_REACH_5 | 63414 | buffer | false | 1 | 5.12 | Y |
| 726 | BGM_G06_REACH_BM1_3 | 62127 | buffer | false | 1 | 5.01 | Y |
| 727 | BGM_G15_REACH_5 | 62126 | buffer | false | 1 | 5.00 | Y |
| 728 | BGM_G04_REACH_5 | 61759 | buffer | false | 1 | 4.99 | Y |
| 729 | BGM_G05_REACH_TH1_3 | 61459 | buffer | false | 1 | 4.96 | Y |
| 730 | BGM_G15_REACH_BM2_3 | 61194 | buffer | false | 1 | 4.97 | Y |
| 731 | BGM_G06_REACH_TH1_3 | 59498 | buffer | false | 1 | 4.82 | Y |
| 732 | BGM_G01_REACH_TH1_3 | 58085 | buffer | false | 1 | 4.70 | Y |
| 733 | BGM_G06_REACH_BM2_3 | 57802 | buffer | false | 1 | 4.67 | Y |
| 734 | BGM_REACH3 | 56929 | buffer | false | 1 | 4.60 | Y |
| 735 | BGM_G04_REACH_BM2_3 | 51809 | buffer | false | 1 | 4.17 | Y |
| 736 | BGM_G11_REACH_BM1_3 | 50719 | buffer | false | 1 | 4.08 | Y |
| 737 | BGM_G10_REACH_TH1_3 | 48464 | buffer | false | 1 | 3.89 | Y |
| 738 | BGM_G14_REACH_BM3_3 | 47225 | buffer | false | 1 | 3.79 | Y |
| 739 | BGM_G16_REACH_BM2_3 | 46779 | buffer | false | 1 | 3.78 | Y |
| 740 | BGM_G04_REACH_10 | 44473 | buffer | false | 1 | 3.57 | Y |
| 741 | BGM_G01_REACH_BM3_3 | 43387 | buffer | false | 1 | 3.49 | Y |
| 742 | BGM_G02_REACH_TH1_3 | 43345 | buffer | false | 1 | 3.47 | Y |
| 743 | BGM_G02_REACH_BM2_3 | 41670 | buffer | false | 1 | 3.34 | Y |
| 744 | BGM_G09_REACH_TH1_3 | 39699 | buffer | false | 1 | 3.18 | Y |
| 745 | BGM_G13_REACH_BM3_3 | 39148 | buffer | false | 1 | 3.14 | Y |
| 746 | BGM_G05_REACH_BM2_3 | 38101 | buffer | false | 1 | 3.03 | Y |
| 747 | BGM_G13_REACH_TH1_3 | 34683 | buffer | false | 1 | 2.75 | Y |
| 748 | BGM_G09_REACH_BM2_3 | 34349 | buffer | false | 1 | 2.72 | Y |

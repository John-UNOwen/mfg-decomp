# Waiting-room BGM loop defect - RCA (2026-11-09)

Read-only investigation. Nothing under the repo, Unity project, or shipped game was modified. All work products live in `C:\mfgtmp\music_rca\` (scripts, extracted clips, probe results).

## TL;DR

The waiting-room track is `BGM_CASTING` (Matching scene). Its loop is driven entirely by the browser: Unity WebGL stores the clip as an AAC re-encode in an MP4 **with no edit list**, plays it through an `HTMLAudioElement` with `loop=true`, and Chrome's media-element loop restarts the decoded stream at 0 with a ~12 ms digital-silence pipeline gap. Combined with the container's frame-rounded duration (69 samples of declared padding, 6.9 ms of real content truncated at the tail) and 372 samples of AAC priming at the head, every 16.51 s the music is **hard-cut mid-sustain, followed by ~20 ms of dead air, then the track's own soft swell** - while the original game's Vorbis/FMOD loop was sample-accurate and gapless. Nothing in the game code schedules, seeks, or restarts by duration; the loop flag and importer settings are faithful to the original. All ≥128 KB AAC'd BGM shares this mechanism; audibility is content-dependent, and BGM_CASTING (16.5 s loop, ends mid-note) is the worst case seen. The 2026-11-03 A2 "seam not audible" verdict was measured on BGM_HOME only (90 s loop, one rare tick, position-sampler only) and did not generalize.

---

## 1. Which track, which code (all paths verified in the served 2026-11-09 build)

- "Waiting room" = the **Matching** (casting) scene. No scene/label named WaitingRoom/LOBBY exists anywhere (full scan of 10,433 label table + 10,363 clip names + all scenes/prefabs).
- Play: `Assets\Assembly-CSharp\MatchingFlow.cs:158-163` - `SoundManager.PlayBgm("BGM_CASTING")`, or `BGM_CASTING_CM` (clip `BGM_G07_REACH_7`) only when `IsReachSongToytoyNoticeEnable() && (PPEX || Konasta/Netcafe)` - the PASELI/arcade-only CM path; the lane build plays **BGM_CASTING**.
- Stop at scene exit: `MatchingFlow.cs:275-277` → `SoundManager.StopBgmWithFadeout(1.0)` (`SoundManager.cs:590-603`) → `AudioManager.StopCategory("BGM",1.0)` - a stop, not a restart; cannot produce mid-scene seams.
- Play chain: `SoundManager.cs:508-532 PlayBgm` (semaphore, `IsPlayingLabel` short-circuit `SoundManager.cs:657-660` → `USnd AudioManager.cs:3410/5809`) → `AudioManager.Play:3036→5303→playOption:5365→prepareInstance→AudioPlayer.prepareImpl:1853-1953` → `AudioInstance.Play:456-474`.
- **Loop = `AudioSource.loop` set once**: `Assets\Plugins\Assembly-CSharp-firstpass\USnd\AudioManager.cs:1888` `audioSource.loop = audioLabelSettings.GetLoop();` (BGM_CASTING label: `isLoop=true`, volume 0.32, category BGM maxPlaybacks=1, `fadeIn/fadeOut=0`, `playStartDelay=0`, `playInterval=0`, `isPlayLastSamples=false`, duck SE2 - re-parsed in `C:\mfgtmp\music_rca\casting_labels.json`; table bytes sha `340dacb0…` same as shipped).
- Start = `PlayDelayed(0f)` (`AudioManager.cs:473`; delay=-1 resolves to `playStartDelay=0` at :1935-1939).
- **No duration-based scheduling anywhere in the BGM path**: `GetLabelLength`/`GetPlaySamples` (`AudioManager.cs:6100-6116/3500`) have zero callers; `playInterval` gate is 0 for all 10,433 labels.
- `Prepare` writes `source.timeSamples` (:436/:442, `playStartSample` branch :437-447 - inactive, `isPlayLastSamples=false`). WebGL cannot seek except via the offset passed at play time: the wasm imports exactly the 22 `JS_Sound_*` functions (verified string scan over the served `webgl.wasm`); **no seek/time export exists**, and play always carries offset 0 (`_JS_Sound_Play` → `playSoundClip(..., offset)` → `_startPlayback(offset)` → `mediaElement.currentTime = offset; play()`). So every start seeks to 0 and plays.
- Pause/resume re-anchor route excluded twice over: `PauseBgmWithFadeout`/`UnpauseBgmWithFadein` (the only `source.Pause()`/`OffPause→source.Play()` paths, `AudioManager.cs:555/593`) have zero game callers; and empirically Chrome 140 restarts a looping element at 0 even after `pause→seek(8.0)→play` (probe phase 2 below).
- All 781 BGM labels share the identical path; the 659 `loop=true` ones loop natively like this one.

## 2. Shipped vs reconstructed clip facts - `BGM_CASTING`

| fact | shipped original | reconstruction (served build) |
|---|---|---|
| file | `Assets\MFG\Sound\BGM\BGM_CASTING.ogg`, 1,060,503 B, OggS/Vorbis, sha256 `6a18cfd5…3ccbdb` (bit-identical in `build\export-primary-content`) | same ogg imported; Unity WebGL **force-re-encodes to AAC**: bundle CAB resource 201,974 B, magic `ftypmp42`, `m_CompressionFormat=7` |
| sample rate / channels | 44,100 / 2 | 44,100 / 2 (preserved — no resample) |
| content length | granule **727,995 samples = 16.507823129 s** (last-page granule, `C:\mfgtmp\music_rca\ogg_deep.py`) | serialized `m_Length = 16.507823944091797` (float32 of the granule value) |
| container | Vorbis, gapless by design | MP4: `stts` 711×1024 = **728,064** samples; `mdhd`/`mvhd` timescale 44100 duration **728,063** (muxer off-by-one); **no `edts`/`elst` box at all** (parse: `C:\mfgtmp\music_rca\mp4_probe.py`, file `shipped\aac_BGM_CASTING`) |
| importer meta | loadType 2 (Streaming), compressionFormat 1 (Vorbis), quality 0, no platform overrides | identical metas (`BGM_CASTING.ogg.meta`); metas are irrelevant to the WebGL output (Unity 2022 WebGL ignores them, KB `original-audio-architecture.md`) |
| load path in build | FMOD Vorbis, sample-accurate native loop | `loadType 2` → `_JS_Sound_Load` decompress=0 (201,974 B > 131,072 B force-decompress threshold) → **blob + `new Audio()` media element** (`webgl.framework.js`, compressed-clip factory @ char ~58,000–60,250 of the minified served file) |

Runtime length quirk (reported, not causal): for compressed clips the glue answers `JS_Sound_GetLength` with `mediaElement.duration*44100` → 728,063 - 68 samples more than the serialized `m_Length`; no BGM-path consumer exists.

Controls (same method, shipped → recon): BGM_HOME 3,983,924 sm → AAC 3,984,384 stts/3,984,383 mdhd; BGM_MENU 976,673 → 976,896/976,895; BGM_G07_REACH_7 1,573,968 → 1,574,912/1,574,911. Same pattern on every streamed BGM. (`C:\mfgtmp\music_rca\shipped_clip_facts.md`, `clips.tsv`.)

## 3. What the browser actually does - measured, not theorized

Probe: the **served** `aac_BGM_CASTING` bytes looped through the same stack the game uses (blob URL → `Audio` element → `createMediaElementSource` → AudioWorklet capture) in this machine's Chrome 156.0.8078.80 (`C:\mfgtmp\music_rca\probe.cjs` / `probe2.cjs`, results `probe_result.json`, `probe2_result.json`).

- Element `duration` = **16.509365 s** (= 728,063/44100, the container value, not the 16.507823 content value).
- Loop wraps: `currentTime 16.4786/16.5094 → 0.0074/0.0474` - **restart lands at 0 (correct position)**; also after `pause→seek(8.0)→play` the next wrap still restarts at ~0.04, so Chromium does **not** re-anchor the loop to the resume position.
- `decodeAudioData` view: buffer = **728,063 samples, no trim**; content occupies **[372, 728,062]** - 372 samples (8.4 ms) AAC priming near-silence at the head; **the true last ~304 samples (6.9 ms) of the 727,995-sample track are truncated** by frame rounding and never exist in the playable buffer.
- Audible cycle (autocorrelation of captured PCM): period **728,576 samples = 16.5210 s** - i.e. element duration **+11.6 ms** per cycle: Chrome emits ~12 ms of digital silence while the decode pipeline restarts at each wrap.
- Seam envelope (2 ms peaks, capture `probe_result.json`): sustain `0.845 0.975` → `0.086` → six bins of `0.000` (12 ms hard zero) → the track's own 90 ms soft swell `0.001…0.023…0.080…0.255…0.467 0.749 1.012`, matching the file head envelope bin-for-bin.
- Pitch at the seam (`pitch_analysis.py`): the last 1.5 s before the cut carries a sustained note (dominant f0 mode ≈ 553 Hz, median 802 Hz, salience 0.46); the head's first note is a different pitch (≈ 989 Hz). The track is **loop-authored** - it leans on a seamless, gapless join and has no natural release at its end.

So each 16.51 s cycle, in the served build, is: content (with the last 6.9 ms missing) → hard cut **mid-sustain** → ~12 ms digital silence + 8.4 ms priming near-silence → soft swell restart. Total audible dropout ≈ 20 ms, three-to-four times a minute, right where the track expects seamlessness. That is the owner's report: mid-phrase cut, then continuation stitched at the wrong point in time; the cut note ending abruptly into a hole and a soft re-entry is what a listener describes as a note repeating/being re-struck. (Exact mapping of "one note repeats" to samples is the one subjective residue - see §5.)

## 4. Why the original never had this

The shipped client plays the same ogg bytes through Unity's native FMOD Vorbis decoder on Windows: `loop=true` is a sample-accurate restart at the PCM level - zero gap, zero priming, zero truncation. Everything that creates the seam is WebGL-platform reconstruction artifact: (a) Unity's forced AAC re-encode, (b) its MP4 muxer omitting an edit list and rounding duration up to whole AAC frames (and off-by-one-ing `mdhd`), (c) Chrome media-element looping = re-decode restart with a ~12 ms silent pipeline gap. The importer metas are faithful and inert; no game code is involved.

## 5. Ranked hypotheses and evidence

1. **CONFIRMED (root cause): HTML5 media-element loop of the untrimmed, frame-rounded AAC re-encode.** Evidence: §2 (container boxes: no `elst`; stts 728,064 vs content 727,995; mdhd 728,063), §3 (measured wrap restarts, 22 ms seam holes at 16548–16570 ms and 33106–33126 ms capture time, audible period 16.5210 s, mid-sustain cut, 372-sample priming, 304-sample tail truncation). Applies to **every streamed BGM whose AAC ≥ 128 KB** (the `_JS_Sound_Load` threshold `length<131072 → decompress=1` sends smaller clips through `decodeAudioData`/AudioBufferSourceNode, which loops sample-accurately with no pipeline gap - small jingles are immune; HOME 1.10 MB, MENU 271 KB, G07_REACH_7 436 KB, CASTING 202 KB are all exposed). Audibility is content×duration-dependent; BGM_CASTING is the exposed case so far.
2. **Contributing process defect: the A2 closure.** `docs\31-open-defect-ledger.md:45` and `docs\30-audio-inventory.md:78` closed this defect class as "not audible" from a 4/13 ms `currentTime`-wrap measurement on BGM_HOME (90.35 s → ~4 seams/hour, a position sampler that cannot see PCM dropout width, and HOME's content may mask a tick). The owner's 11-09 report on a 16.5 s loop is consistent with the same seam being present all along and content-audible on the waiting-room track.
3. **RULED OUT - game code restart/seek/fade by duration**: zero callers of the duration APIs; `timeSamples` writes have no WebGL seek path and always carry offset 0; fade-end always stops, never restarts (`USnd AudioManager.cs:361-368/476-522`); watchdog at :399-402 only marks finished one-shots STOP.
4. **RULED OUT - loop flag propagation / importer misconfiguration**: flag verified propagating (`HTMLMediaElement.loop` setter telemetry 11-03; glue `channel.setLoop→mediaElement.loop` proxy in the served framework.js); metas byte-faithful.
5. **RULED OUT - pause/resume loop re-anchoring**: no in-game callers of the pause paths; Chrome 156 empirically restarts at 0 after seek+play (probe phase 2).
6. **RULED OUT - CM variant**: BGM_CASTING_CM/BGM_G07_REACH_7 requires PASELI-PPEX/Konasta behavior; if it ever played, same mechanism, 35.7 s loop.

## 6. Not verified / residual uncertainty

- Owner-side browser is assumed Chromium-family (lane standard); other engines may show different gap widths (the no-`elst`/frame-rounding container facts are browser-independent; the 12 ms gap is Chrome's).
- In-game capture not run (would confirm the same 20 ms seam with mixer/ducking in circuit; ducking affects SE2 only - a second duck occurs per loop only if SE2 activity coincides).
- Whether the owner's "one note repeats" is literally a repeated pitch: measured tail-note (≈553–802 Hz) ≠ head-note (≈989 Hz) at the seam, so the literal reading is not reproduced; the cut-note → dropout → soft-swell is the audible event that invites that description.
- Shipped-side *serialized* AudioClip columns (m_Length/m_CompressionFormat on Windows) still pending the share being up (`shipped_unitypy_dump.py` ready); irrelevant to the WebGL verdict since shipped behavior is established from the verbatim ogg + platform facts.
- The exact Unity AAC encoder's delay/padding policy: only inferred from the served container (372-sample content onset measured from actual decode output - that is the number that matters).

## 7. Single decisive next probe

Capture the **live game's** BGM element on the lane: hook `Audio()` creation + `createMediaElementSource` on the page before boot (the A2 harness pattern, `tmp/audio_loop_probe.cjs`), insert the AudioWorklet tap from `probe2.cjs`, sit in the Matching scene for 3–4 loops, and confirm the in-game per-seam signature equals the standalone measurement (wrap at 16.5094 s, audible period 16.5210 s, ≈20 ms dropout, restart at 0). If it matches - and it should, it's the same mechanism - the RCA is closed with runtime proof, and any fix can be A/B'd against the same signature (e.g. forcing BGM_CASTING onto the ≥128 KB `decompress=1` buffer path collapses the seam to the 8.4 ms priming only, which discriminates the pipeline-gap from the priming component).

## Appendix - artifact index (C:\mfgtmp\music_rca\)

`code_trace.md` (S1 full C# trace) · `shipped_clip_facts.md` + `clips.tsv` + `shipped\` (extracted ogg/aac bytes; `aac_BGM_CASTING` = the served container parsed here) · `casting_labels.json` (re-parsed label rows) · `mp4_probe.py`/`mp4_probe.json` (container boxes) · `probe.cjs`/`probe_result.json` + `analyze_probe.py` (loop timing, wrap positions, envelope, autocorr period) · `probe2.cjs`/`probe2_result.json` + `pitch_analysis.py` (PCM seam windows, pitch) · `glue_probe.py`/`glue_windows.py` (framework.js excerpts). Unity Library untouched throughout (probe targets: `C:\mfgtmp\webroot\StreamingAssets\aa\WebGL\9be2391b…bundle` and `webroot\Build\webgl.framework.js`/`webgl.wasm`, served build 2026-11-09 07:17).

## §8 Fix attempt 1 - glue force-decompress: failed and reverted (2026-11-09 12:00)

The section-7 discriminator was escalated to a real fix: an `assemble_webroot.ps1` step replacing the served glue's `if(length<131072)decompress=1;` with unconditional `decompress=1;` (every clip to the sample-accurate `decodeAudioData` buffer path, stamp `D7BFBB164DF6`, on the local lane + staging edge for roughly 15 minutes).

Measured result: the patched build boots, the WebGL context comes up, Unity C# runs ~6 s (translation-table log, Boot-scene traces through t=5.9), then the **main loop stops dead** - `fps=0`, no `PAGEERROR`, no console error. Three lanes on the patched root wedged identically; the unpatched previous root bisected fine on the same machine/harness/profile state (60 fps on a static-server copy), and the first lane after reverting the glue in place + regenerating the brotli sidecar booted `fps=120` and reached Matching with the predicted fingerprint (`AudioElement` constructed at `LoadScene:Matching` - media-element path, per the docs/38 exposure map for the 201,974 B clip). No audio-load error was ever emitted; forcing the buffer branch on Unity's streaming-class clips is the prime suspect and the mechanism stays unexplained.

The step is **OFF by default** in `assemble_webroot.ps1` (`-L105BufferLoop` arms it deliberately, this history in the comment); staging serves the unpatched revert build again - the re-assemble reproduces stamp `473601EA817D` because the unpatched glue is byte-equal to that build. The next candidate works upstream of the glue: pad the exposed loop clips content-side (silence head/tail, or container edit-list/padding correction at export) so the seam lands in silence - decode routing untouched.

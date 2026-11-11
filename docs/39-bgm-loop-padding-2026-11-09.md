# 39 - BGM loop-seam padding: fix build and verification (2026-11-09)

**SUPERSEDED 2026-11-09 same day: the padding fixed the wrong thing.** The owner's "nothing
changed" was right - the audible defect is the double onset from crossfade-authored loop tails,
analyzed and fixed in docs/40; this padding is fully reverted (unity repo `df0af9ed90`). The
instrumentation lessons below (MPSEAM taps, harness gotchas) still hold and are cited there.

Follows docs/37 (RCA + failed glue attempt) and docs/38 (exposure map). Fixes L105 content-side.

## The lever (owner-approved)

The seam is the container, not the decode routing: every `HTMLAudioElement.loop` wrap restarts the
stream with the AAC container's 372-sample priming, Chrome's ~12 ms digital-silence gap, and 304
samples of tail truncation (no edit list). docs/37 section 8 killed the glue-side approach (boot
wedge). The content-side lever: **prepend/append 20 ms of silence to the exposed looping clips at
export**, so priming, gap, and truncation all land inside authored silence. Owner accepted the
~40 ms total loop-period growth ("if i dont notice the obvious offset in looping"). Loop period
per track +40 ms; zero change to Unity, the glue, decode routing, or any bundle layout.

## Clip selection - 400 of 748 BGM clips

From `C:\mfgtmp\ost_audit\l105_exposure_final.tsv` (docs/38): all **392 media-bucket looping clips**
(`bucket=media`, `loop_flag!=false`) plus **8 buffer-clip flip guards** (123,000–131,071 B - within
the ~0.5 KB growth so a re-encode can't silently push an unpadded clip across the 131,072 threshold;
the largest actual growth measured was +490 B). Excluded: every clip already on the sample-accurate
buffer path, and all `loop=false` one-shots (`BGM_TITLE`, `BGM_MOVIE_A/B` - a padded tail on a
one-shot is dead air, and their ends are real ends). Pad list: `C:\mfgtmp\pad_list.txt`.

## Toolchain (reusable)

Static `ffmpeg 9.0.2` at `C:\mfgtmp\bin\ffmpeg.exe` (gyan.dev essentials zip; no system install).
`C:\mfgtmp\pad_bgm.ps1`: per clip - back up to `C:\mfgtmp\bgm_pad_backup\` (relative paths), encode
`-af "adelay=20:all=1,apad=pad_dur=0.02" -c:a libvorbis -q:a 6`, verify duration grew 0.040 s
(ffmpeg banner is 10 ms-precise, gate ±11 ms), swap. **400/400, no failures** (`C:\mfgtmp\pad_report.csv`).
Side effect accepted: the 400 source oggs become second-generation Vorbis; the shipped AAC re-encode
at quality 0 (19–33% of ogg size) dominates audibility, and originals stay in the backup dir + git
history (the change is commit-revertible).

## Build chain and byte-level verification

`build_webgl_fast.ps1 -Full` (Unity re-imports + re-encodes the 400 clips, bundles regenerate,
`result=Succeeded`). UnityPy spot check on the fresh `out/webgl` bundles (`t25_padcheck.py`,
guid-prefix match - bundle hashes change with content): padded clips grew ~+300 B of AAC silence
frames (`BGM_CASTING 201,974→202,262`), the `loop=false` exclusion (`BGM_TITLE 376,337`) and the
off-list control (`BGM_STORY_G18 779,663`) are **byte-identical** to docs/38. `t26_castbytes.py`
extracts the served AAC from the CAB and decodes it: `BGM_CASTING` = **38.8 ms head silence +
16.2 ms tail silence** (priming + pad / pad − truncation). Assemble: stamp **`D3B9D06D592A`**,
smoke 17/0/3.

## In-game verification - the MPSEAM harness

`tmp/webgl_proof.cjs` `MP_MUSIC=1` block: per `createMediaElementSource`, an AudioWorklet rms tap
(zero-gain path to destination - a dead-end worklet node is never pulled) plus element `currentTime`
polling; a backward CT jump is the ground-truth wrap, and each wrap dumps the last 40 rms blocks
(×10000, ~106 ms). Blob byte-size identifies the clip identity of every tap (202,262 = padded
CASTING) - attribution without touching Unity code.

LAN lane, padded build (`C:\mfgtmp\wr_p1.log.txt`): `tap#7 graph bytes=202262 dur=16.556`, CT wraps
`16.242→0.180` / `16.202→0.119`, `loop=Y` - native buffer-end loop, and the wrap dump:

    0,0,0,0,0,0,7,14,63,61,61,190,237,…,3477

≥6 blocks of pure digital zero at the boundary, then a swell rising out of silence. Compare the
docs/37 pre-pad fingerprint: hard cut at rms 0.975 mid-sustain + 12 ms zeros + instant onset.
The seam is now silent (≥16 ms digital zero visible, plus the authored pads on both sides).

Staging edge (public tunnel) lane, same build: door1 lobby → login → play page, Title/MyRoom BGM
taps carry padded byte sizes, heap stable, no boot wedge; game reached MyRoom at [191s] inside the
190 s window (WAN boot is slower than the lane; Matching wrap numbers from the edge run below).

Harness lessons (this is the kit, not the game): captureStream-based taps produce all-zero streams
headless (the `cap` taps are dead weight - kept as canaries); a zero-run logger with fixed
silence/onset thresholds misses the padded signature (the swell crosses the silence line at ~0.02,
under the 0.05 onset gate) - the CT-jump detector + block dump is the reliable instrument.

## Verdict

Padding is proven to land where the seam is. What remains is the owner's ear on staging
(`v=D3B9D06D592A`): the casting loop should now breathe silently at the wrap instead of cutting a
note; other exposed loops (Home, kyoku, story, reach) benefit identically.

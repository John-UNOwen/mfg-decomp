# 40 - BGM loops: the real root cause was authored crossfade tails; skip-head bake shipped (2026-11-09)

Supersedes docs/39 (silence padding - reverted, see that file's banner) and completes docs/37 (the
decode-seam analysis - real but secondary, it only sets the ~20 ms residual gap).

## What the owner actually hears
The waiting-room loop `BGM_CASTING` "hard skips" every 16.5 s: the phrase **start plays twice in a
row**. Fresh page load on the padded build, owner report 2026-11-09 afternoon: "nothing changed".
The padding diagnosis was wrong: the defect is not primarily the decode seam.

## Root cause (measured)
`BGM_CASTING.ogg` is one render of an endless loop: its **last 0.5078 s are a baked crossfade into
the next pass's start** (autocorrelation peak at lag exactly 16.0000 s = 705,600 sm, ncc 0.831;
sub-window gain fit rises 0→1 across the head's attack then holds 0.90-0.98 - the tail carries the
head phrase at full gain, not a quiet bed). The file must resume each cycle at sample 22,395
(μ = 0.5078 s), not at 0. Unity/Chrome loop the whole file from sample 0, so the start plays once
soft inside the tail and once full at the restart: the "hard skip" is the double onset plus the
discontinuity of replaying it, not the ~12 ms loop gap or the AAC priming (docs/37) - those remain
as a ~20-32 ms residual that now lands inside the crossfade.

## The fix Unity's naive loop=true can execute
**Skip-head bake**: cut the first μ samples off the file, then fold the last 1024 samples into the
first 1024 with a linear ramp (seam micro-crossfade) and drop the folded tail. The remainder starts
exactly where its own tail's crossfade points, so whole-file looping traces the authored endless
loop. No code, no decode-path, no padding. Loop period becomes λ = the detected autocorr lag.

Kit: `C:\mfgtmp\ost_audit\t30_bake.py` (scan+bake, `--all --apply`; originals from
`C:\mfgtmp\bgm_pad_backup` / git 955a7e1601~1), `t31_seam_audit.py` (late-window gate + stitched
real-loop demo), `t32_refine.py` (byte-exact reverts), `t34_bakecheck.py` (UnityPy bundle
verification). Report: `C:\mfgtmp\bake_report_all.csv`.

## Detection + audit policy
Scan: all 626 loop=true clips (docs/38 map). Detect: best autocorr lag λ in (0.55L, L-0.05 s) with
μ = n-λ within [0.15 s, min(3.0 s, 0.25 L)] and ncc ≥ 0.60 → 24 hits.
Audit (must pass or the bake is reverted byte-exact): late-third-of-window corr ≥ 0.80 and gain
0.4-1.6, on the ORIGINAL bytes. This caught 8 false/unsafe candidates: 5 `BGM_THUMB_*` 10 s jingles
(a_late≈0 - musical repetition, no head in tail) and 3 with tail running past the true overlap
(`BGM_G16_REACH_BM3_1` 0.748, `BGM_G07_REACH_BM3_1` 0.599, `BGM_RESULT1` 0.706). Lesson: raw
near-full-length autocorr alone is not enough; the late window is what proves a seam crossfade.

## Shipped set (16)
3MJG2_2 4MJG2_2 3MJG1_3 4MJG1_3 3MJG1_6 3MJG2_5 4MJG2_3 2MJG1_4 MJG_BOMB3_2 STORY1 G06_REACH_3
GACYA RESULT3 CASTING RESULT4 RESULT5 (unity repo `df0af9ed90`, net delta vs pre-pad = 16 oggs;
padding commit `955a7e1601` fully superseded). Controls byte-identical in the new bundles: BGM_HOME,
BGM_TITLE, BGM_G07_REACH_7, BGM_THUMB_G13_REACH_3 (t34).

## Verification
1. Owner ear: `C:\mfgtmp\waiting_room_audio\BGM_CASTING_FIXED_x3_backtoback.wav` "is fine now";
   full-set real-loop demo `BAKED16_realloops_demo.wav` delivered.
2. UnityPy on fresh bundles (t34): all 16 present with shrunk AAC (CASTING 195,748 B), 4 controls byte-identical.
3. Staging `v=9652C6377D4C` (smoke 17/0/3), MPSEAM LAN lane in-game: tap `bytes=195748 dur=15.999`,
   native wrap `ct 15.849->0.380 loop=Y` = 16.0 s period, level trace continuous across the boundary,
   residual `zeroRunMs=32.0` sitting inside the crossfade (was a mid-note 0.975 hard cut in docs/37).

## Post-fix residual: the media-element wrap gap is audible - buffer-path allowlist shipped
Owner retest on `9652C6377D4C`: double onset gone, "a little pause when the loop happens" remains.
That pause is the media-element wrap itself: ~12 ms Chrome loop-restart silence + ~8 ms AAC priming
= lane-measured `zeroRunMs=32.0` every cycle - real silence in continuous music, audible no matter
what the content does around it (my "lands harmlessly inside the crossfade" call was wrong).
Only the buffer path (`decodeAudioData` -> `AudioBufferSourceNode.loop`) wraps with no gap.
The docs/37 force-ALL experiment died because ~353 MB of AAC decodes to ~7 GB of PCM: a memory
event, not the logic event it was recorded as. The surgical version shipped the same day:
assemble step `-L105Baked16` patches the glue needle to
`if(length<131072||[13 exact served-AAC sizes].indexOf(length)>=0)decompress=1;` - only the 16
baked clips join the buffer path (~130 MB PCM, all sizes collision-checked against the docs/38
census via `t35_sizes.py`, list in `C:\mfgtmp\baked16_sizes.json`).
Verified on staging `D759A911DF0C` (smoke 17/0/3): boot fine (fps 94 at 176 s), CASTING plays as
`BufferSource start dur=15.9985 loop=true` (no media-element tap for it at all anymore), and the
MPSEAM zero-run detector - which logged the 32 ms gap on the media path - logged ZERO sustained
zero runs across ~4 loop cycles; heap 188-265 MB as budgeted. Harness gained a `kind=buf` tap
(parallel worklet tap attached in the `AudioBufferSourceNode.start` hook).

## Closure
Owner on `D759A911DF0C`: "way better now, almost completely unnoticeable" - accepted, with a
barely-noticeable wrap blip remaining. Two candidates if it is ever chased: the 1024-sm linear
micro-crossfade fold at the baked seam (an equal-power ramp or a longer fold is one rebuild away),
or Chrome resampling the decoded buffer 44.1→48 kHz (all buffers decode at 48k: samples=767929 =
15.9985x48000), whose edge behavior at the loop point can click faintly. No further action.

## Promotion to live (2026-11-10)
`mfg.0006767.xyz` (VPS production lane) moved from `D33DFEFDA262` to `D759A911DF0C`. The promoted
tree is the STAGING webroot itself, not a reassembly: `transfer_build.sh` shipped all 58,809 files
and the final convergence pass hashed every file on the VPS against the manifest - 4/4 chunks
skipped, 0 bytes moved - proving byte identity; the webroot is host-independent because the baked
`-FRONTURL http://127.0.0.1:8090/aog` in index.html is a placeholder the gate splices per request.
Swap was `systemctl stop mfg-gate` + two renames + chown + start (~15 s down); the old tree rode as
`webroot.old-20261110T042749Z` and was removed after smoke (17/0/3), the edge stamp readback, and
brotli-sidecar identity (`webgl.framework.js.br` at `?v=D759A911DF0C` hashes equal staging's, plain
and compressed alike - Chrome's compressed path gets the patched glue too). Cloudflare still holds
the pre-promotion `framework.js` under the BARE unversioned key; harmless, every game page loads
assets only under the `?v=` key and the watchdog reloads stale pages.

Near-miss worth keeping: the first transfer run ended "transfer done: 1 chunk(s) sent" with a
clean exit - `remote_chunk_lines()` ran ssh inside the `while read` chunk loop, and ssh ate the
loop's stdin, truncating the chunk list after the first entry. The ssh-shim fixtures cannot catch
this (a bash shim doesn't consume stdin); real ssh truncated EVERY run at chunk 1, so reruns never
converge. Fixed by holding the observation call's stdin shut (`</dev/null`, repo `9761a3b`); the
kit fixture suite is unchanged green by A/B (114 ok / 6 pre-existing bad both runs). VPS disk after
promotion: 4.1 G free (journal vacuum 3.3→0.5 G; the retired 9.2 G `/srv/mfg-staging` tree stays per
KB). Rollback, if ever needed: re-promote by the same path - `D33DFEFDA262` is reproducible from
content state `a70b969da0` + a content rebuild.

## Follow-ups
- `BGM_G16_REACH_BM3_1` / `BGM_G07_REACH_BM3_1` / `BGM_RESULT1`: double onset remains (original
  behavior preserved deliberately); need hand-checked resume points (tail runs past the head copy).
- If more tracks are ever re-rendered upstream, rerun the t30+t31+t32 pipeline; the CSV audit drives
  it, and regenerate `baked16_sizes.json` (t35) before assembling with `-L105Baked16`.

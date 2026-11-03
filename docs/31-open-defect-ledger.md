# 31. Open-defect ledger (merged, single source of truth for the fix phase)

Merged sources: `docs/29-checkup-2026-11-02-findings.md` (26 items, 2026-11-02 checkup + red team), KB `hosted-build-open-defects.md` (mirrors + owner-observed items), and owner-reported defects since. Statuses: **open** = unfixed; **fix-deployed** = patched and live on the lane; **queued** = awaiting owner senses/data; **resolved** = closed with evidence. The 2026-11-01 identity ledger (docs/28 §checkup remediation) is fully resolved and not carried here.

| # | Sev | Defect (condensed) | Status | Next action |
|---|---|---|---|---|
| L1 | critical | 20-pull is label-only: client Count()=20 but every request asks server for 5; play scene runs off the delivered list and aborts before the result dialog; 20-state animator rig does not exist (fixing the request count alone is not enough) | open | design decision: revert to honest 5-pull labels or full 20-pull implementation (request, rig, thumbnails, bookkeeping); ledger owner chooses with audio phase checkpoint |
| L2 | major | Gate relays core profile pages anonymously (join/last-seen leak, DataID→username oracle) — contradicts docs/28 gating | open | gate-side session/lobby check before relay |
| L3 | major | Gate exposes POST /profile/:refid/card: self-mint arbitrary DataIDs, no core rate limiter | open | gate block or core limiter + ownership rules |
| L4 | major | TicketFive charges 200 tickets for a 5-item draw; low balances wiped-and-persisted instead of refused | open | fix alongside L1 decision; Consume must refuse before committing |
| L5 | major | `gachaCount == 5` gates now always false: MemorialCards dead, gacha-total counters dead, mission reward unobtainable | open | resolve with L1 (count semantics) |
| L6 | major | 20-pull priced/paid as 5-pull end to end (fee, server roll, spirits, exchange points) | open | resolve with L1 |
| L7 | major | Server-side half of gacha change untracked (dist plugin git-ignored; cited hash b7fd394 only exists in tmp\asphyxia_plugins) | open | vendor plugin tree or commit handler changes to core repo |
| L8 | major | TileSurface fallback covers only TileSurface001; equipped surfaces 002+ render blank mini-tiles (atlas address missing from catalog) | open | add per-surface 2D keys to groups or bundle atlases as addresses |
| L9 | major | EnableUnlimitedButton=true hard flip also kills IsReservedGameOver guard on Home GACHA → unclickable reserved-over gacha entry | open | scope flag to the intended menu locks only |
| L10 | major | Case-collision takeover via scripts/hosting_claim.cjs (writes raw-case usernames, case-sensitive dup check); no storage-level enforcement | open | normalize in claim script + add boot migration/unique index (identity-adjacent; owner-notify) |
| L11 | minor | Lobby lockout ladder never escalates past 60 s (counter deleted with the lockout) | open | persist fail counts across penalty expiry |
| L12 | minor | 1–4-fail history never decays; whole-table Clear drops others' active lockouts | open | per-entry TTL + per-entry pruning |
| L13 | minor | Correct code cannot clear an active lockout (lockout checked before code match) | open | check code before lockout refusal |
| L14 | minor | /about redirect rule GET-only; HEAD/POST/… relay to core | open | method-agnostic gate rule |
| L15 | minor | Lobby unlock cookie lacks Secure while core cookies are Secure | open | set Secure via X-Forwarded-Proto branch |
| L16 | minor | Gate body cap 256 KB vs core 5 MB media uploads; /uploads not relayed | open | raise cap + relay /uploads |
| L17 | minor | Auto authpass flattens EPASSWD/unbound-card states into NetworkError; unbound cards never heal | open | port per-status handling into the auto lane |
| L18 | minor | Auto authpass UniTask has no timeout/cancel/lifetime binding; can stall Login forever and pause the shared countdown | open | deadline + cancellation token + scene binding |
| L19 | minor | Tenkey "Window" Find is inert (Window is an ancestor); hide depends on a different patch | open | fix or delete dead code |
| L20 | minor | Red-dora fallback substitutes base tiles for 31 '...R' values (visual lie, latent) | queued | only matters if such tiles ever display |
| L21 | minor | Leftover 5回ガチャ text vs new 20回 labels across windows/missions/translation | open | resolve with L1 decision |
| L22 | minor | Cabinet bookkeeping path-dependent (select adds 20, retry adds 5) | open | resolve with L1; cabinet-only |
| L23 | minor | EnableUnlimitedButton applies to every lane incl. cabinet; debug toggle inert | open | add lane discriminator |
| L24 | minor | dist/src mirror drift in 3 core files (hand-edited dist) | open | rebuild dist from src or commit parity |
| L25 | minor | Live account DataIDs committed in docs/28 (identity handle disclosure) | open | scrub docs/28 to placeholders (owner-notify) |
| L26 | minor | Batchmode Unity opens silently drop generated link.xml; player builds proceed without it | open | regenerate/verify link.xml before artifacts-only builds (fast-mode builds hit this) |
| L27 | minor | Telop corner bar vertical alignment (thickness fixed via sprite rect; padding.top iterated -13.7 → -15.5 → too high → -14.5) | fix-deployed (build pending owner look) | owner confirm at next visit; self-verify via headless lane shot when feasible |
| L28 | minor | Telop Text(TMP) rect height (and thus bar position) is content-dependent across languages/scene names | queued | content-independent variant (MiddleLeft alignment) — only if owner demands JP parity |
| A1 | major | Lane Home/MyRoom/Dojo/GachaSelect BGM silent. ROOT CAUSE (2026-11-03, holder-instrumented build): `SoundManager._playSemaphore` permit LEAK — UniTask cancellation at the `await _playSemaphore.WaitAsync()` boundary (scene-switch cancels of `Forget()` façade calls) consumes the permit then abandons the continuation before try/finally exists; after the Title→Login swap nothing else ever acquires (Home `PlayBgm BGM_HOME` waits forever; Home SE/voice equally blocked). Facade body awaits verified bounded; no reentrancy (scanner `scripts/audio/scan_sem_reentry.ps1` clean) | in-fix | leak-proof acquire (`WaitAsync(4000)` + `SuppressCancellationThrow` + late-release guard + conditional finally) deployed in rebuild; verify Home media play + `SEM acquire ... got=True` trail via proof2 |
| A2 | major | BGM loop "wrong" (owner). Corrected diagnosis: loop flag DOES propagate (`LOOPSET loop=True` observed for BGM_MENU; verified glue `_JS_Sound_SetLoop→channel.setLoop→mediaElement.loop`) — the audible defect is the HTML5 media-loop restart seam on AAC (priming gap each cycle) vs cabinet's seamless Vorbis buffer loops; TITLE one-shot is by design | open | A/B experiment ready: BGM metas loadType 2→0 (decompress-on-load → seamless BufferSource loops), `scripts/audio/set_bgm_loadtype.ps1` staged (751 metas, dry-run verified); owner ears decide |
| A3 | major | Server grants never reach the client: plugin writes `player_item` entries with field `Count` while the client serializes `m_count` (JsonUtility drops unknown members); 131 profiles hold server-spelled items that the next client write overwrites | open | fix spelling in canonical plugin tree (aog.ts:573-577) + mirror via ryunet_stage; core restart is owner's call (PID 7824 not agent-owned) |

Counts: 26 checkup items (1 critical / 9 major / 16 minor) + 1 carried owner item (L27) + 1 caveat (L28) + 3 audio-phase items (A1–A3, tracked under the audio goal). Resolved and not carried: web-lane boot freeze (async-void/init fixes shipped), corner thickness (sprite rect patch live), fresh-signup stuck-at-Logo (proof 2026-11-03 walks to NewRegistration; remaining stall is name-entry automation, a harness gap not a game defect).

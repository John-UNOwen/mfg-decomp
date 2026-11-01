# 28 — Web identity & public hosting plan

Status: **draft for owner review.** No implementation has started. Decisions recorded in OKF:
`projects/mfg/decisions/web-front-identity-with-password-signup.md` (this plan is the spec; OKF keeps only the decision + reasons).

## Owner decisions (settled in the grill)

| # | Decision |
|---|---|
| 1 | Public-facing hosting, locally or on a VPS, **always behind Cloudflare on the owner's own domain**. |
| 2 | Identity pages = the core WebUI's existing `/signup` / `/login` (Express + pug, bcrypt, rate-limited, session-backed). Branded custom pages are a later coat of paint. |
| 3 | 1 web user = 1 game account. The DataID is **minted server-side at signup**, crypto-random 16-hex, never user-chosen, never reused. |
| 4 | The existing preview account `6AE73807BA1CE8CB` (26 saved states, 9級) is **imported** by binding it to a web user (admin/claim tooling). |
| 5 | The player name is set in-game: a first regular boot with no registered name routes through the original **NewRegistration** flow (screens 6-8 of the 2026-11-01 cabinet captures). Web name editing is a later feature. |
| 6 | Dev/test lanes (guest pages, fixture identities, static server 8094) stay alive but physically separated; they are never exposed publicly. |
| 7 | A page carrying a pinned `-DATAID` boots the client as a **regular (non-guest)** account. Guest boot stays for `-AUTOSERVE` test lanes and an explicit opt-out. (Implemented 2026-11-01 as the `-REGULAR` arg; see Done below.) |
| 8 | **Every account is premium by default** (owner override). Premium is granted server-side, not deferred. |
| 9 | The bound DataID is shown on the logged-in account page (recovery + future native client). |
| 10 | Accept current session semantics: logging in elsewhere **takes over** (grace 0) — one player per account at a time, product behavior, not a bug. |
| 11 | Free-text player names at v1 (server-side taboo screening `chk_tabooword` already runs); moderation policy is a public-launch item. |
| 12 | This repo doc is the implementation spec; OKF holds the decision record. |
| 13 | Vendored core webui **may be patched** (minimally, `dist` + `src` mirror in sync) so open signup mints the DataID + card itself. Invite-code minting is the fallback if we ever want zero core edits. |
| 14 | The session gate lives in **WebFront** (our C# front door): it validates `connect.sid` against core `GET /api/me`, serves game pages only to authenticated sessions, and injects the bound DataID. The core stays the identity provider. |
| 15 | WebFront performs a **card presentation** (`cardmng.inquire` with the account's minted 16-hex card id) whenever it relays `/aog/login` or `/aog/create_player` for a verified session — making the existing card-credential gate (`VFG_ACCOUNT_AUTH=card`, `card_auth` stamp, 900 s window, refusal 770) live for every hosted login. A bare DataID without a valid web session is refused at login. Adopted from the start. |

Assumed defaults for the public-phase round (not grilled; change at review): single hostname `mfg.<owner-domain>` with WebFront as the only edge; Cloudflare Tunnel (`cloudflared`) origin so no ports open; origin accepts tunnel traffic only; open signup at launch with the core's existing rate limits + Cloudflare rules; existing `admin/admin` seed rotated before public exposure; nightly SQLite backups.

## Done already (2026-11-01)

- **Regular boot works**: `AccountGameData.NotifyGuestUserStart()` honors `-REGULAR` with a pinned `-DATAID` → `IsGuestUser=false` → `guest=0` logins, full Home hub. Verified headless: fixture `REGPROBE1` (`login ... guest=false`, `is_guest=false` in the plugin DB, fresh account routed to NewRegistration), and the real account `6AE73807BA1CE8CB` flipped from `guest=true` to `guest=false` and resumed straight to Home with progress intact.
- `preview_game.ps1` defaults to regular boot (`-Guest` opts out); documented in OKF reference `endgame-continue-chain-fix-2026-10-31.md`.

## Architecture (target)

```
browser (https://mfg.<domain>)
   └─ Cloudflare (TLS, bot/rate rules)
        └─ cloudflared tunnel → origin 127.0.0.1:8090  WebFront (our C# edge)
             ├─ static game pages          (WEBFRONT_ROOT; -DATAID/-REGULAR spliced per session)
             ├─ /signup /login /logout …   forward → core :8083 (Cookie + Set-Cookie relay)
             ├─ /aog/*  AOG game API       card-present, then forward → core :8083
             └─ session check              core GET /api/me (username, cardNumber = DataID)
core (Asphyxia fork, loopback)  ── webui (identity, sessions) + AOG handlers + mfg@asphyxia plugin
plugin DB (documents / plugins_profile)  ── game accounts (refid=DataID, states, is_guest)
dev lane 8094 (fixture/guest pages, loopback only, never published)
```

Rationale: cookies ignore ports but not hostnames — one hostname means `connect.sid` set by core pages is automatically visible to WebFront on the same host; no cross-subdomain cookie hacks, same-origin COOP/COEP preserved. WebFront is a 235-line single-file C# app we own; the core is patched only in the signup handler.

## Phase 1 — Mint + open signup (core patch)

Goal: `/signup` works with just username + password + confirm; the game account materializes at signup.

1. Patch `webui/routes/auth.js` (dist **and** `src` mirror): replace "card must already exist" with mint-on-signup —
   - mint DataID = `crypto.randomBytes(8).toString('hex').toUpperCase()` (16-hex);
   - reject reserved shapes: `MID*` (mint-forbidden), `^01[0-9A-F]{14}$` and `^E004[0-9A-F]{12}$` (the core `cardSanitizer` auto-create patterns), the plugin `DEFAULT_CARDID` exact value, `''`;
   - collision-check against core `card` docs and plugin profiles (app-level only today — the store has **no unique indexes**; the mint tool must check, and we keep the signup-time dupe checks the core already has);
   - `CreateUserAccount(username, password, dataId, false, countryCode)` unchanged;
   - create the core card doc (`CreateCard(cid=dataId, refid=dataId)`) so `/api/me` resolves `cardNumber → card.__refid` cleanly;
   - seed the plugin profile + **premium** (Phase 1c).
2. Keep password/username rules as-is (3-32, `/^[\w\-.@]+$/`, password ≥4); raise min password to 8 at this touch.
3. **1c — premium by default (Q8 override):** grant premium *state-side*, not control-side:
   - seed the account's `player_game` state with `_IsPro: true` (server-authored state, same pattern as the friend-grant work), for new mints and as a **one-time migration for existing profiles** (incl. `6AE73807BA1CE8CB`);
   - deliberately keep `game_control → pro_stats` = `{now_pro_stats:false, ProStayDatas:[]}`: `IsProStay()` feeds the `pro_stay=1` param at `entry_game`, i.e. premium *table-stay* semantics we have not verified against the match engine; `pro_stay=1` could silently change table behavior. The client premium gates that matter for the hub (gacha NETCAFE branch HomeFlow.cs:352, private-match button, pro banners) key on `Player.IsPro()` = `_IsPro`. Spike at implementation: confirm seeded `_IsPro` survives the client's own state write-back (it should — the field round-trips in the blob).
   - If we later want premium *stay* matching, that is its own verified feature (flag in review).
4. Verify: signup headless (curl/cookie jar) → account page shows the minted DataID → page load as that account → `login ... guest=false` + presentation line in server log → Home. Existing account import (Q4) is the admin-claim script: bind `6AE73807BA1CE8CB` to a chosen web user (sets `cardNumber` + card doc accordingly) + rotate the seeded `admin` password in the same step.

## Phase 2 — WebFront session gate + card presentation (our C# file)

1. Parse `Cookie:`; call core `GET /api/me` (HttpClient exists); 200 → bind (username, cardNumber); 401 → `401` for XHR, `302 → /login` for page GETs.
2. Game pages: serve `index.html` template with `"-REGULAR", "-DATAID=<bound>"` spliced into `config.arguments` per authenticated request (same rewrite `New-PlayPage` does, now dynamic; small per-session cache). Anonymous never receives a playable page.
3. Auth paths (`/signup`, `/login`, `/logout`, `/account*`, `/static*`): forward to core **relaying `Cookie` and `Set-Cookie`** (today Forward sends no request headers and drops upstream headers — the gap to close), preserving the existing COOP/COEP/CORS response envelope.
4. `/aog/login` and `/aog/create_player`: before relay, issue the card presentation — XRPC `cardmng.inquire` (or the `vfgcard` shadow module) with `cardid = <bound cardNumber>` → `card_auth` stamp (utils.ts `recordCardAuthIntent`); then relay. Other `/aog/*` endpoints keep today's plain relay (they are pcuid-session-keyed). Note: XRPC POST shape (the eamuse envelope) to be pinned by a spike first — gate behavior itself is already proven by the audit sweeps.
5. Fix the static defect found in the sweep: `Cache-Control: public, max-age=31536000, immutable` is currently stamped on **proxied /aog responses too** — split policy: statics stay immutable (content-hashed enough), `/aog/*` becomes `no-store`.
6. Keep WebFront bind loopback (tunnel reaches it via localhost).
7. Verify: headless matrix — anonymous page → 302; logged-in page boots as the bound regular account; second browser = takeover only *after* its own login (with presentation); bare DataID POST to `/aog/login` without a web session → refused 770 (the card gate is live); long session > 900 s re-presents on next login fine; gacha/menus not broken by the cache policy change.

## Phase 3 — Cloudflare + deployment

1. **Tunnel**: `cloudflared` ingress `mfg.<domain> → http://127.0.0.1:8090` (works from the current PC or a VPS identically). Origin core + WebFront stay loopback-bound; nothing else published.
2. **DNS/SSL**: hostname on the owner zone; SSL mode **Full** with an origin certificate (self-signed on the tunnel side is fine); "Secure cookies": set express-session `cookie.secure` at the core patch point (it is currently unset — `trust proxy` is already on) so the `Secure` flag applies in production but the LAN dev lane keeps working over http (toggle via env `WEB_SECURE_COOKIE=1`, new small core webui patch, or accept `secure: 'lax'` behavior — pick at implementation).
3. **Hardening before go-live**:
   - rotate the seeded `admin/admin` (core `SeedDefaultAdmin`);
   - plugin's raw listener `mfg_http_port=22421` currently binds **0.0.0.0 with no auth** — bind loopback or disable (integrated mode makes it unnecessary);
   - Cloudflare: rate-limit rules on `/login`, `/signup`, `/aog/*`; bot fields on auth pages later if abused;
   - `SESSION_SECRET` env explicit (fallback file already exists; sessions are **in-memory** — a core restart logs everyone out, acceptable v1, persistent store is a later item);
   - nightly copies of `dist/savedata` (core) + plugin `savedata` SQLite.
4. Dev lane untouched: 8094 statics + fixture identities remain loopback-only (Q6). `preview_game.ps1` keeps driving it.
5. Verify: full flow through the public hostname (signup → dashboard → play → match → logout), takeover test across two machines, refusal test without login, brotli delivery over the tunnel, no Konami/provider traffic anywhere.

## Phase 4 — Later / optional (explicitly out of v1)

- Branded sign-in/dashboard (pug replacement or new front on the same endpoints).
- Persistent session store; password reset; email verification.
- Web-side player-name editing; profile pages beyond the existing plugin pages.
- Premium *stay* matching semantics (needs its own spike).
- Multiple game accounts per web user; card/PIN presentation on a future native Windows client (the minted card id already supports it).
- Invite-code signup gate if abuse appears.

## Known risks / open items to resolve during implementation

1. `pro_stay=1` semantics deliberately avoided in v1 (see 1c); revisit only with a match-engine spike.
2. Client state write-back over a server-seeded `_IsPro` — must be re-verified live.
3. XRPC cardmng envelope shape from WebFront (C#) — spike before phase 2.4; failure mode if wrong: logins proceed bearer-style (grace rule) rather than presentation-gated, so gate status must be asserted in the verify matrix, not assumed.
4. Minted DataID lives in the same 16-hex namespace as canonical card ids — reserved-prefix exclusions and collision checks are mandatory (Phase 1.1).
5. No DB unique index on `cardNumber` — enforced at app level only (core seeded admin has `cardNumber:''`); keep mints non-empty and checked.
6. Core patches (signup, secure-cookie toggle) must land in both `dist` (runtime) and `src` (mirror) so future rebuilds don't silently drop them.
7. Cloudflare + WebGL static sizes: ~3.4 GB brotli webroot — first-load caching/immutable policy already correct for statics; consider a CDN cache rule for `Build/*` after go-live measurements.
8. `log.txt` and probe fixtures contain real ids — keep out of any public artifacts (audit-sweep convention stands).

## Definition of done (v1)

Open `https://mfg.<domain>` → sign up (username/password) → see own minted DataID → play immediately as a **regular, premium-granted** account with the full hub (gacha private-match gates open per premium; dojo per rank rules; My Room character select), one session at a time; anonymous visitors cannot play or reach any AOG endpoint for someone else's account; existing `6AE73807BA1CE8CB` history intact and claimed by the owner account.

## Delivery record (implemented + verified 2026-11-01, local hosted-mode lane)

Two plan mechanisms were replaced by better facts found while implementing:

1. **Premium for everyone (Q8) needs no server-side grant at all.** `GameUtility.IsProCardId()` (GameUtility.cs:411) returns **true for every card id whenever `IsKamunityBootEnable()` is false**, and `IsKonamiNetConnection` - the only thing that flips it - is set solely by the `-KONAMINET` arg (ResidentFlow.cs:987), which every lane build passes in its base `index.html`. Consequence, verified live: the NETCAFE premium clauses (gacha HomeFlow.cs:352, GachaSelect, BtGachaPlayRetry) and `Account.IsForceFreePlayUser` (GameDataManager.cs:179, no-credit play) are already true for every regular account. The plan's `_IsPro` seed + migration is therefore dropped - and would have collided with the settled "the server does not invent client-owned state" rule (OKF `no-read-time-state-rewrite`).
2. **Card presentation goes through a plugin AOG route with a shared secret, not an XRPC client in WebFront.** `POST /aog/vfgcard_present` (aog.ts handler -> utils.ts `presentViaPresenter`) requires `VFG_PRESENTER_SECRET`, stamps `card_auth` (one spend per login, existing semantics) and binds `profile.card_id` **only for ids with a core card document** (signup mints + admin claims), so test fixtures and legacy cabinet ids keep their bearer semantics everywhere. This closes plan risk item 3 without implementing an XRPC binary client.

Implemented pieces:

- **Core webui (`tmp\RyuNET-core` dist + src mirrors + views):** signup mints the account itself - crypto 16-hex DataID excluding reserved shapes (`01…`, `E004…`, `E0047CC78DFBA459`, existing card/user/profile ids via FindCard/FindUserByCardNumber/FindProfile), `CreateCard(cid == refid)`, password min 8, signup page no longer asks for a card; `WEB_SECURE_COOKIE=1` gives `connect.sid` the Secure flag; new plugin knob `mfg_http_bind` (native 22421 listener; hosted posture 127.0.0.1).
- **Plugin (`plugins/mfg@asphyxia`):** `VFG_PRESENTER_SECRET` knob, `presentViaPresenter` rules, `/aog/vfgcard_present` route.
- **WebFront (`webfront/Program.cs`, `WEBFRONT_GATE=1`):** session gate via core `/api/me` (30s cache) - anonymous page 302 `/login?next=`, anonymous `/aog` refused 770; play pages spliced per session from the canonical template (`-ENTERCARD -TRANSLATE -REGULAR -DATAID=<id>`, stale dev artifacts never served); auth relay with Cookie/Set-Cookie/Location passthrough + visitor UA relay (the core webui 404s non-browser UAs - found the hard way) + `CF-Connecting-IP` -> `X-Forwarded-For` for the core's auth rate limits; presentation before every relayed `/aog/login|create_player` (also on the gate-off dev relay when a secret is armed); cache split fixed (statics stay immutable, `/aog` and pages `no-store`).
- **Scripts:** `hosting_claim.cjs` (offline admin rotation + account claim; ran: `mfgowner` -> `6AE73807BA1CE8CB`), `hosting_up.ps1` (production start), `hosting_backups.ps1` (nightly savedata copies), `hosting_deploy.md` + `hosting_cloudflared.yml` (Cloudflare steps), `preview_game.ps1` now arms the presenter secret on the dev lane.

Verified matrix (headless Edge + scripted callers + DB/log reads):

| Probe | Result |
|---|---|
| anonymous `/` and `/aog` at the gate | 302 -> /login; 403 refusal 770 |
| signup with no card | minted `C42F74698A509DD6`, no reserved-shape id, card doc `cid == refid` |
| gated play page | boots `Title -> NewRegistration` (original name flow), `login ... guest=0` |
| server-side gate | `web presenter bound card ... credential-bound`, `card credential presented`, `login ... auth=card ... spent by this login` |
| bare DataID straight to core (scripted UA) | refused 770, `ACCOUNT REFUSED rule=card` - both for the minted and the claimed account |
| dev fixture `REGPROBE1` | untouched bearer, session issued as before |
| claimed real account | hosted game login `guest=false`, `is_guest=false`, card bound `card_issued=web`, name + 26 saved states intact; LAN lane still plays it (`auth=card presented 0s ago`, Home with progress) |

Remaining for public go-live (owner-side, per `scripts/hosting_deploy.md`): create/run the tunnel on the chosen box, DNS + WAF rules, backup schedule, first login + password change of `mfgowner` (credentials printed once to `C:\mfgtmp\owner-credentials.txt`), and the same checklist against the public hostname.

### Checkup remediation pass (2026-11-01)

A multi-angle checkup of the web identity portion found 3 critical + 19 major issues; all were fixed and re-verified (24-assertion fix suite + the regression suite above). The fixes, so the record stays honest about the first version:

- **Cookie bleed (critical):** the shared upstream `HttpClient` had cookie management on - `connect.sid` from one visitor leaked into every later upstream call. Cookie handling is now off everywhere; cookies travel only inside the per-request relay. Verified: two accounts through one gate instance see strictly their own identity.
- **Settings POST exposed (critical):** the core's `POST /plugin/:plugin` (writes config.ini - including `VFG_PRESENTER_SECRET`) was reachable by any signed-up user through the gate. Now admin-gated in the core itself (`adminMiddleware` on the route, dist + src) and unreachable at the edge (only `GET /plugin/*/static/*` relays; POST refused 404).
- **Junk-cookie flood DoS + unbounded caches (critical):** the session cache is keyed on the `connect.sid` token alone, capped at 5000 entries with TTL sweeps; negative checks cache 10 s; upstream failures are a tri-state (`UpstreamError` -> 503, no cache eviction, no bounce-to-login charade); the gate itself rate-limits 600 req/min per visitor. `CheckSession` sends the gate's honest visitor-ip hop so the core's per-IP limiters see real clients instead of one 127.0.0.1 bucket.
- **Identity swap via `POST /account` (major):** card editing is now admin-only (and the empty-string card-clear is gone); the account page shows the id read-only. Mint/claim are the only ways an id is assigned.
- **Path spelling bypasses (major):** every request is classified on a canonicalized path (percent-decoded, dot segments collapsed, control chars and trailing dots refused, re-encoded identically for relay), so `/login/../admin`, `/index.html.`, `PUT`-style `..` tricks cannot disagree with the core router; arbitrary `.html` names no longer serve the index page; AOG endpoint names outside `/aog/` are refused unless the path is exactly a webui page (login/signup/logout are AOG names too - the guard knows that now); the auth relay always travels as a browser agent, which is also what keeps core-side AOG middleware from claiming webui paths.
- **Transport bugs (major):** requests are read byte-accurate (raw reader, chunked framing implemented, multi-byte UTF-8 bodies relay intact), bodies capped at 256 KB (413), `Expect: 100-continue` answered before the read.
- **Core signup hardening (major):** signup creation is serialized in-process (concurrent same-username POSTs land exactly 1 account - verified with an 8-way race), the taken username is decided before any id is minted (no orphan cards), card document is stored before the user row, storage failures no longer masquerade as "Username already exists", bcrypt's 72-byte truncation boundary is enforced at signup and /account, the /account floor is raised to 8, `GET /logout` + gate eviction now end a session immediately (clearing `Set-Cookie` included), login and signup rotate the session id, and signup no longer echoes passwords back into the form.
- **Startup refusals (major):** the gate refuses to start hosted without an explicit `WEBFRONT_AUTH`, and refuses gate-off + armed presenter + non-loopback bind. `mfg_http_bind` precedence is env > config.ini > default and the boot log prints the bind actually used (verified 127.0.0.1:22421 live).
- **Smaller ones:** presenter success is now judged by the `<presented>` marker (AOG answers refusals with 200), `next=` survives one honest encoding round-trip and core honors it (validated same-site paths only), `X-Forwarded-For` reaching the core is only ever the gate's single hop (`WEBFRONT_TRUST_PROXY=cloudflare` chooses CF-Connecting-IP vs the socket peer), self-service account edits invalidate the core's 30 s user cache, the splice template check fails closed, `-FRONTURL` is rewritten positionally, and error pages no longer print .NET exception text.

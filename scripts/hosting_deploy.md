# Hosting this lane publicly (docs/28 runbook)

Target shape: `https://mfg.<your-domain>` -> Cloudflare -> `cloudflared` tunnel ->
origin `127.0.0.1:8090` (WebFront, gate ON) -> core `127.0.0.1:8083` (RyuNET +
mfg@asphyxia plugin). The origin binds loopback only; nothing else is reachable
from outside. Works the same on your own PC or a Windows VPS.

## One-time setup (already done on this machine, redo on a VPS)

1. Presenter secret: `%USERPROFILE%\.mfg-web\presenter.secret` (create once,
   random bytes; hosted_lane.ps1 generates it). The core gets it as
   `VFG_PRESENTER_SECRET` (env, or `config.ini` section `[mfg@asphyxia]`),
   WebFront gets `WEBFRONT_PRESENTER_SECRET_FILE` pointing at the same file.
2. Claims (core stopped): `node scripts\hosting_claim.cjs --rotate-admin` then
   `--claim-user <name> --claim-refid <16HEX>` for each pre-existing game
   account. Passwords print once. The owner can change them in the webui at
   `/account` after first login.
3. `config.ini` (`tmp\RyuNET-core\dist\config.ini`) on a hosted box:
   ```ini
   [mfg@asphyxia]
   VFG_PRESENTER_SECRET=<contents of presenter.secret>
   mfg_http_bind=127.0.0.1        ; the native 22421 listener never faces out
   VFG_ACCOUNT_AUTH=card          ; (default) hosted logins need presentations
   ```

## Start / stop

- Start hosted: `pwsh scripts\hosting_up.ps1` (core armed + WebFront gate ON +
  optional cloudflared via `-Tunnel`). Stop: `scripts\preview_game.ps1 -Stop` kills the lane.
- The hosted start patches `mfg_http_bind=127.0.0.1` into `config.ini` while the
  core is stopped (the core rewrites that file live); the plugin precedence is
  env > config.ini > default, and the boot log prints the bind actually used.
- The gate refuses to start hosted without `WEBFRONT_AUTH` (no silent fallback to
  a public default), and refuses `WEBFRONT_GATE=0` + armed presenter + `WEBFRONT_BIND=any`.
- The LAN dev lane is unchanged: `scripts\preview_game.ps1` (gate OFF, presenter
  armed from the same secret file, so claimed accounts play there too - loopback-bound).

## Cloudflare (one hostname, single origin)

Live shape (2026-11-01): tunnel `mfg` (` <tunnel-uuid>`) routes
`https://mfg.0006767.xyz` (zone `0006767.xyz` - ` <provider-host>` is the provider's domain, not on
this account) to the gated WebFront on `127.0.0.1:8090`. Config lives at
`%USERPROFILE%\.cloudflared\config.yml`.

1. Install `cloudflared`, `cloudflared tunnel create mfg`.
2. Copy `scripts\hosting_cloudflared.yml` to `%USERPROFILE%\.cloudflared\config.yml`,
   replace `mfg.example.com` and the tunnel id.
3. DNS: `cloudflared tunnel route dns mfg mfg.<your-domain>` (orange-cloud proxy).
4. SSL/TLS mode: **Full** (the tunnel terminates TLS at the edge; origin stays
   plain http on loopback).
5. Run `cloudflared tunnel run mfg` (service: `cloudflared service install`).
6. Recommended rules for `mfg.<domain>` (dashboard -> Security -> WAF / Rules):
   - `/login`, `/signup`: rate-limit rule, e.g. 30 req / 15 min per IP
     (the core also has its own 10-per-15-min limiter, fed by `CF-Connecting-IP`
     relayed as `X-Forwarded-For`);
   - Bot Fight Mode on; WebSockets setting stays OFF (the game is plain POSTs);
   - A Cache rule is optional: StreamingAssets bundle names are content-hashed and served
      `immutable`; entry build files (loader/framework/data/wasm) share fixed names, so they
      need revalidation after a rebuild (front-side cache policy rework pending). Pages/API
      come through `no-store` from the front.
7. `WEB_SECURE_COOKIE=1` gives the session cookie the `Secure` flag - use it only
    when the lane really terminates https (`hosting_up.ps1 -Tunnel` sets it for
    exactly that shape). Setting it on a plain-http origin makes express-session
    silently drop the cookie: logins never stick. Unset keeps the LAN lane usable.
8. `WEBFRONT_TRUST_PROXY=cloudflare` (hosting_up.ps1 -Tunnel sets it) makes
    `CF-Connecting-IP` the visitor identity for the gate's own limiter (600 req/min
    per visitor) and for the core's auth rate limits - the gate replaces any incoming
    `X-Forwarded-For` with that single honest hop and never adopts a client-supplied
    one for identity. Without it the gate keys visitors on the real socket peer and
    ignores client-supplied ip headers entirely. A tunneled origin without this knob
    puts every visitor into one shared 127.0.0.1 rate-limit bucket.
9. Admin accounts: `--rotate-admin` leaves the core admin card-less, and a
   card-less session is bounced to `/account` instead of the play page. Claim an
   id for it too (`--claim-user admin --claim-refid <16HEX>`), or use a claimed
   non-admin account as the everyday identity.
10. Discord stays fully off unless `discord_bot_token` / `discord_client_id` are
    set in `tmp\RyuNET-core\dist\config.ini`: the bot does not start, the
    `/auth/discord` routes are not mounted, and the login/signup/about pages
    render no Discord links at all (the community-invite links are gated on the
    same knob). Setting `discord_client_id` re-enables the OAuth buttons and the
    community links together.
 11. Access-code lobby (`WEBFRONT_ACCESS_CODE`, or `WEBFRONT_ACCESS_CODE_FILE`
    pointing at e.g. `%USERPROFILE%\.mfg-web\access.code`): until the shared code
    is entered at `/welcome`, the signup/login feature is not shown at all -
    anonymous visitors meet a code form, POSTs to `/login` and `/signup` are
    refused, and the play page bounces to `/welcome`. A correct code mints a 24 h
    HttpOnly cookie (`mfg_lobby`); unlock attempts are throttled to 10/min per
    visitor. Unset = lobby off. The code gates the hosted entrance only; accounts,
    passwords and the core itself are untouched by it.

## Backups

`pwsh scripts\hosting_backups.ps1` on a schedule (Task Scheduler, nightly):
copies core `dist\savedata` + plugin savedata into `%USERPROFILE%\.mfg-web\backups`
with a date stamp, keeps 14 copies. Stop nothing - SQLite WAL copies are
consistency-safe for a warm copy of this scale; the sure method is a lane
restart right after a backup if you care about the last seconds.

## What the public edge looks like (verify checklist)

- `/`, `/aog/index_play.html` anonymous -> 302 to `/login`.
- `/signup` needs no card; the account gets a minted 16-hex DataID shown at
  `/account` (and `/api/me`).
- The play page is built per session: `-REGULAR -DATAID=<your id>` only.
- Every game login shows server-side as
  `login ... auth=card <id> presented 0s ago (spent by this login)`; a bare
  DataID posted straight to the origin answers refusal 770
  (`ACCOUNT REFUSED ... rule=card`).
- One live session per account (takeover on next presented login).
- `/settings`, `/admin/*` are not exposed publicly (origin-machine business).

## Known v1 edges (deliberate, see docs/28 Phase 4)

- Sessions are in-memory: a core restart signs everyone out (next login works).
- Signup is open (rate-limited); invite gating is a later knob.
- Player name is chosen in-game on first play (NewRegistration scene).
- Premium for everyone: in a non-Konami boot `GameUtility.IsProCardId()` is true
  for every card id, which is exactly the "everyone premium" behavior the plan
  calls for; no server-side state seeding was needed or done.

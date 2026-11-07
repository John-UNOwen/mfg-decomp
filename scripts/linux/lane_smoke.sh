#!/usr/bin/env bash
# lane_smoke.sh - read-only acceptance suite for a lane front, curl only.
#
# Run it against a lane gate base URL. It writes nothing to the lane beyond what a
# login and logout do: the only body-carrying requests are POST /aog/appli_info (the
# read-only AOG info endpoint already used as a probe by lane_probe_up.ps1:92-95) and,
# in the optional authenticated section, POST /login and POST /logout.
#
# Usage
# -----
#   scripts/linux/lane_smoke.sh [-h|--help]
#
#   BASE_URL            target front.                default: http://127.0.0.1:8090
#                       Set it to the gate (WEBFRONT_PORT) or to any front that
#                       answers with the gate's behaviour. On a box also hosting the
#                       Windows lane, aim this at the Linux gate port only - the
#                       Windows lane ports (8083/8090/8091/8095 there) are not targets.
#   SMOKE_BR_FILE       base name of a brotli-sidecar'd webroot file.
#                       default: Build/webgl.framework.js
#   SMOKE_HASH_FILE     webroot asset whose name carries a 16-hex content token
#                       (an addressables bundle). default: the WebGL bundle
#                       StreamingAssets/aa/WebGL/0000249d...c0fc.bundle in the current
#                       build; override it when your build's bundle list differs.
#   SMOKE_ENTRY_FILE    plain-named entry file.      default: Build/webgl.loader.js
#   SMOKE_CSS           core webui stylesheet.       default: /static/css/modern.css
#   SMOKE_IMG           core webui image.            default: /static/img/loginBG.jpg
#                       (both live under ASSETS_PATH/static - build-env/assets/static/
#                        css/modern.css, img/loginBG.jpg - reached through the gate's
#                        /static relay, webfront/Program.cs:1157)
#   SMOKE_LOGIN_MARKER  string the login page must carry. default: FoxNET
#                       (build-env/assets/views/login.pug:7 'FoxNET - Login')
#   SMOKE_FRONT_URL     expected -FRONTURL value after the gate's positional rewrite.
#                       default: /aog  (Program.cs:50, WEBFRONT_PAGE_FRONTURL)
#   SMOKE_USER/SMOKE_PASS  both set -> run the authenticated section (login, account
#                       page, spliced play page, logout, session-gone check). Never set.
#                       The password is only ever sent to BASE_URL, never printed.
#   SMOKE_TIMEOUT       curl --max-time seconds.     default: 15
#
# Mode notes: with WEBFRONT_GATE=1 the anonymous play page and /aog traffic bounce at
# the gate (302 to /login, 403 XML with <serv_st>); with WEBFRONT_GATE=0 (LAN relay,
# assemble_webroot.sh --serve) pages and /aog answer directly. The suite accepts the
# gate shape or the relay shape per test and prints which one it saw. With
# WEBFRONT_ACCESS_CODE_FILE set the lobby answers /login itself (Program.cs:814-821)
# and the login_page test then sees the lobby page - that is a known mode interaction,
# not a lane fault.
#
# Exit code: 0 when no test FAILED (SKIPs allowed), 1 otherwise. Each test prints one
# PASS/FAIL/SKIP line with the evidence.

set -euo pipefail
export LC_ALL=C

# ---------------------------- configuration ----------------------------
SCRIPT_PATH=${BASH_SOURCE[0]}

BASE_URL=${BASE_URL:-'http://127.0.0.1:8090'}
BR_FILE=${SMOKE_BR_FILE:-'Build/webgl.framework.js'}
HASH_FILE=${SMOKE_HASH_FILE:-'StreamingAssets/aa/WebGL/0000249d5a04557566b2c7fa690d36d9_e337695af3dc349533a3ff2407a6c0fc.bundle_assets_all_e337695af3dc349533a3ff2407a6c0fc.bundle'}
ENTRY_FILE=${SMOKE_ENTRY_FILE:-'Build/webgl.loader.js'}
CSS_PATH=${SMOKE_CSS:-'/static/css/modern.css'}
IMG_PATH=${SMOKE_IMG:-'/static/img/loginBG.jpg'}
LOGIN_MARKER=${SMOKE_LOGIN_MARKER:-'FoxNET'}
SMOKE_FRONT_URL=${SMOKE_FRONT_URL:-'/aog'}
TIMEOUT=${SMOKE_TIMEOUT:-15}

usage() {
  sed -n '2,/^set -euo/p' "$SCRIPT_PATH" | sed '$d'
}

while [ $# -gt 0 ]; do
  case $1 in
    -h|--help) usage; exit 0 ;;
    *)         printf 'lane_smoke: unknown argument: %s\n' "$1" >&2; exit 2 ;;
  esac
  shift
done

BASE=${BASE_URL%/}
PASS_N=0 FAIL_N=0 SKIP_N=0

pass() { PASS_N=$((PASS_N+1)); printf 'PASS %s | %s\n' "$1" "$2"; }
fail() { FAIL_N=$((FAIL_N+1)); printf 'FAIL %s | %s\n' "$1" "$2"; }
skip() { SKIP_N=$((SKIP_N+1)); printf 'SKIP %s | %s\n' "$1" "$2"; }

tmp=$(mktemp -d)
trap 'rm -rf -- "$tmp"' EXIT
BODY=$tmp/body.bin
HDR=$tmp/headers.txt
JAR=$tmp/cookies.jar
: > "$JAR"

get_url() {  # get_url <url> [extra curl args]
  curl -sS --max-time "$TIMEOUT" -o "$BODY" -D "$HDR" -w '%{http_code}' "$@"
}
post_form() {  # post_form <url> <form-body> [extra curl args]
  _u=$1 _d=$2; shift 2
  curl -sS --max-time "$TIMEOUT" -o "$BODY" -D "$HDR" -w '%{http_code}' \
       -X POST -H 'Content-Type: application/x-www-form-urlencoded' \
       --data-binary "$_d" "$_u" "$@"
}
GET() { GET_CODE=$(get_url "$@") || GET_CODE=ERR; }
POST() { POST_CODE=$(post_form "$@") || POST_CODE=ERR; }

hdr() { sed -n "s/^[^:]*$1:[[:space:]]*//p" "$HDR" 2>/dev/null | tr -d '\r' | head -n 1; }
has_hdr() { grep -qi "$1" "$HDR" 2>/dev/null; }
body_has() { grep -F -q -- "$1" "$BODY" 2>/dev/null; }
body_snip() { head -c 160 "$BODY" 2>/dev/null | tr '\n\r' '  ' | tr -s ' '; }
loc_hdr() { sed -n 's/^[Ll]ocation:[[:space:]]*//p' "$HDR" 2>/dev/null | tr -d '\r' | head -n 1; }
status_of() { printf 'HTTP %s' "$1"; }

printf 'lane_smoke: target %s (curl only, read-only)\n' "$BASE"

# ---------------------------- 1. COOP / COEP headers ----------------------------
# The gate emits both headers on every single response (webfront/Program.cs:973) -
# they are what Unity's SharedArrayBuffer build needs (Program.cs:21-22 comment).
# The status is part of the assertion: headers on a 302 that bounces an anonymous visitor
# to /login say nothing about the answer that carries the page, and headers on a 404 or a
# 500 say nothing either. Only the 200 answer is a pass (the same rule index_no_store
# learned in L48); the bounce is a SKIP that points at the checks that can reach the page.
GET "$BASE/"
coop='' coep=''
has_hdr '^[Cc]ross-origin-opener-policy:[[:space:]]*same-origin' && coop=same-origin
has_hdr '^[Cc]ross-origin-embedder-policy:[[:space:]]*require-corp' && coep=require-corp
coop_seen=$( [ -n "$coop" ] && printf 'same-origin' || printf 'missing' )
coep_seen=$( [ -n "$coep" ] && printf 'require-corp' || printf 'missing' )
case $GET_CODE in
  ERR)
    fail coop_coep_headers "cannot reach $BASE/"
    ;;
  200)
    if [ -n "$coop" ] && [ -n "$coep" ]; then
      pass coop_coep_headers "HTTP 200 Cross-Origin-Opener-Policy: same-origin; Cross-Origin-Embedder-Policy: require-corp"
    else
      fail coop_coep_headers "$(status_of "$GET_CODE") missing $( [ -z "$coop" ] && printf 'COOP; '; [ -z "$coep" ] && printf 'COEP;' ) headers seen: $(hdr Cross-Origin-Opener-Policy) / $(hdr Cross-Origin-Embedder-Policy)"
    fi
    ;;
  301|302|303|307|308)
    skip coop_coep_headers "$(status_of "$GET_CODE") Location: $(loc_hdr) (COOP: $coop_seen, COEP: $coep_seen on the redirect): the gate bounced this visitor to the identity page, so no answer carrying the page was seen. Run the suite against the assembled relay (assemble_webroot.sh --serve) or sign in first (SMOKE_USER/SMOKE_PASS)"
    ;;
  *)
    fail coop_coep_headers "$(status_of "$GET_CODE") COOP: $coop_seen, COEP: $coep_seen - the header pair only proves anything on the 200 answer that serves the page"
    ;;
esac

# ---------------------------- 2. brotli sidecar rule ----------------------------
# Base name + Accept-Encoding: br -> served with Content-Encoding: br
# (Program.cs:1058-1059 picks file+'.br', :977 emits the header).
# Naming the .br path directly -> 404 (Program.cs:1050-1057 - it would skip gating).
GET "$BASE/$BR_FILE" -H 'Accept-Encoding: br'
br_code=$GET_CODE br_ce=$(hdr Content-Encoding)
GET "$BASE/$BR_FILE.br"
direct_code=$GET_CODE
if [ "$br_code" = 200 ] && [ "$br_ce" = br ] && [ "$direct_code" = 404 ]; then
  pass brotli_sidecar "GET /$BR_FILE (Accept-Encoding: br) -> HTTP $br_code Content-Encoding: $br_ce; GET /$BR_FILE.br -> HTTP $direct_code (404 required)"
else
  fail brotli_sidecar "GET /$BR_FILE (Accept-Encoding: br) -> HTTP $br_code Content-Encoding: ${br_ce:-<none>}; GET /$BR_FILE.br -> HTTP $direct_code (want 200/br and 404)"
fi

# ---------------------------- 3. hashed asset immutable cache ----------------------------
# Names containing a 16-hex token get 'public, max-age=31536000, immutable'
# (Program.cs:1090-1097).
GET "$BASE/$HASH_FILE"
cc=$(hdr Cache-Control)
if [ "$GET_CODE" = 200 ] && [ "$cc" = 'public, max-age=31536000, immutable' ]; then
  pass hashed_asset_immutable "GET /$HASH_FILE -> HTTP 200 Cache-Control: $cc"
else
  fail hashed_asset_immutable "GET /$HASH_FILE -> HTTP ${GET_CODE:-?} Cache-Control: ${cc:-<none>} (want 200 + 'public, max-age=31536000, immutable'; override SMOKE_HASH_FILE for your build)"
fi

# ---------------------------- 4. plain entry file gets the short TTL ----------------------------
GET "$BASE/$ENTRY_FILE"
cc=$(hdr Cache-Control)
if [ "$GET_CODE" = 200 ] && printf '%s' "$cc" | grep -q 'max-age=3600' && ! printf '%s' "$cc" | grep -q immutable; then
  pass entry_file_ttl "GET /$ENTRY_FILE -> HTTP 200 Cache-Control: $cc (not immutable)"
else
  fail entry_file_ttl "GET /$ENTRY_FILE -> HTTP ${GET_CODE:-?} Cache-Control: ${cc:-<none>} (want 200 + max-age=3600, no immutable - Program.cs:1097)"
fi

# ---------------------------- 5. index.html is not cached ----------------------------
# The 200 answer is what must carry no-store (Program.cs:1070 + CachePolicyOf :1094-1096).
# Status first: every gate answer defaults to Cache-Control: no-store (Program.cs:982),
# and gate mode answers an arbitrary .html name with 404 + no-store (Program.cs:902-907),
# so a header-only check passes on a webroot with no index.html at all.
GET "$BASE/index.html"
cc=$(hdr Cache-Control)
if [ "$GET_CODE" = 200 ] && printf '%s' "$cc" | grep -q 'no-store'; then
  pass index_no_store "GET /index.html -> HTTP 200 Cache-Control: $cc ($(wc -c < "$BODY") body bytes)"
elif [ "$GET_CODE" = 404 ] && ! body_has '<html'; then
  skip index_no_store "GET /index.html -> HTTP 404 body: $(body_snip) (gate mode refuses arbitrary .html names - Program.cs:902-907): nothing was served, so there is no cache policy to prove. Run this suite against the assembled relay (assemble_webroot.sh --serve) for the 200 answer, or read play_page_autoserve / play_page_splice for the gate's own play page)"
else
  fail index_no_store "GET /index.html -> HTTP ${GET_CODE:-?} Cache-Control: ${cc:-<none>} body: $(body_snip) (want 200 + no-store)"
fi

# ---------------------------- 6. login page ----------------------------
# /login is relayed to the core webui (Program.cs:1157-1158, :900); the core renders
# build-env/assets/views/login.pug, whose title carries the FoxNET marker.
GET "$BASE/login"
ct=$(hdr Content-Type)
title=$(grep -o '<title>[^<]*</title>' "$BODY" 2>/dev/null | head -n 1 || true)   # || true: pipefail must not abort on a no-match grep
if [ "$GET_CODE" = 200 ] && printf '%s' "$ct" | grep -qi 'text/html' && body_has "$LOGIN_MARKER"; then
  pass login_page "GET /login -> HTTP 200 '$title' contains '$LOGIN_MARKER' (Content-Type: $ct)"
else
  fail login_page "GET /login -> HTTP ${GET_CODE:-?} Content-Type: ${ct:-<none>} marker '$LOGIN_MARKER' $(body_has "$LOGIN_MARKER" && printf found || printf 'not found') ${title:+title: $title} (lobby mode replaces /login with the access-code page - Program.cs:814-821)"
fi

# ---------------------------- 7/8. core webui static assets ----------------------------
GET "$BASE$CSS_PATH"
ct=$(hdr Content-Type)
if [ "$GET_CODE" = 200 ] && printf '%s' "$ct" | grep -qi 'text/css'; then
  pass static_css "GET $CSS_PATH -> HTTP 200 Content-Type: $ct"
else
  fail static_css "GET $CSS_PATH -> HTTP ${GET_CODE:-?} Content-Type: ${ct:-<none>} (want 200 text/css)"
fi
GET "$BASE$IMG_PATH"
ct=$(hdr Content-Type)
if [ "$GET_CODE" = 200 ] && printf '%s' "$ct" | grep -qi '^image/' && [ -s "$BODY" ]; then
  pass static_img "GET $IMG_PATH -> HTTP 200 Content-Type: $ct body bytes: $(wc -c < "$BODY")"
else
  fail static_img "GET $IMG_PATH -> HTTP ${GET_CODE:-?} Content-Type: ${ct:-<none>} body bytes: $(wc -c < "$BODY") (want 200 + Content-Type image/* + a non-empty body; a 200 text/html or an empty body is not the core asset)"
fi

# ---------------------------- 9. core session page rejects a cookie-less visitor ----------------------------
# The gate answers /account itself for anonymous visitors: 302 to /login
# (Program.cs:878-888); the core would also bounce. Any 302-to-login/401/403 passes.
GET "$BASE/account"
loc=$(loc_hdr)
case "$GET_CODE" in
  302)
    if printf '%s' "$loc" | grep -q '/login'; then
      pass session_page_anonymous "GET /account (no cookie) -> HTTP 302 Location: $loc"
    else
      fail session_page_anonymous "GET /account -> HTTP 302 Location: $loc (not a login bounce)"
    fi ;;
  401|403) pass session_page_anonymous "GET /account (no cookie) -> HTTP $GET_CODE $(body_snip)" ;;
  *) fail session_page_anonymous "GET /account (no cookie) -> HTTP ${GET_CODE:-?} Location: ${loc:-<none>} (want 302-to-login, 401 or 403)" ;;
esac

# ---------------------------- 10. POST /aog/appli_info is routed to the core, not the static handler ----------------------------
# Gate mode, anonymous: 403 XML <serv_st><code>770 (Program.cs:1396-1399) - proof the
# request entered the gated API handler instead of the static handler (whose 404 body
# is plain 'not found', Program.cs:1060-1064). Relay/core: 200 with the <root> XML
# (lane_probe_up.ps1:92-95 shape).
POST "$BASE/aog/appli_info" 'web_id=1' -H 'User-Agent: Mozilla/5.0 lane-smoke'
ct=$(hdr Content-Type)
if [ "$POST_CODE" = 200 ] && body_has '<root>'; then
  pass aog_core_relay "POST /aog/appli_info -> HTTP 200 Content-Type: $ct body: $(body_snip)"
elif [ "$POST_CODE" = 403 ] && body_has '<serv_st>'; then
  pass aog_core_relay "POST /aog/appli_info (anonymous, gate mode) -> HTTP 403 Content-Type: $ct body: $(body_snip) - routed to the gated /aog handler, not the static handler"
else
  fail aog_core_relay "POST /aog/appli_info -> HTTP ${POST_CODE:-?} Content-Type: ${ct:-<none>} body: $(body_snip) (want 200+<root> via core or 403+<serv_st> at the gate)"
fi

# ---------------------------- 11. play page carries -AUTOSERVE ----------------------------
# Relay/static mode serves the webroot index.html as assembled: it keeps the standalone
# "-AUTOSERVE" element and the baked -FRONTURL literal (assemble_webroot.sh:252,313).
# Gate mode bounces anonymous visitors to /login (Program.cs:1337-1346); the splice
# rewrite is then proven by play_page_splice in the authenticated section.
GET "$BASE/"
loc=$(loc_hdr)
front=$(grep -o '"-FRONTURL",[[:space:]]*"[^"]*"' "$BODY" 2>/dev/null | head -n 1 || true)   # || true for pipefail (see login_page)
if [ "$GET_CODE" = 200 ] && body_has '"-AUTOSERVE"'; then
  pass play_page_autoserve "GET / -> HTTP 200 with standalone \"-AUTOSERVE\" element; front element seen: ${front:-<none>}"
elif [ "$GET_CODE" = 302 ] && printf '%s' "$loc" | grep -q '/login'; then
  skip play_page_autoserve "GET / -> HTTP 302 Location: $loc (gate mode: play page only exists behind a session; splice rewrite checked by play_page_splice)"
else
  fail play_page_autoserve "GET / -> HTTP ${GET_CODE:-?} Location: ${loc:-<none>} body starts: $(body_snip) (want 200 with \"-AUTOSERVE\" or the gate's 302 to /login)"
fi

# ---------------------------- 12-16. optional authenticated section ----------------------------
if [ -n "${SMOKE_USER:-}" ] && [ -n "${SMOKE_PASS:-}" ]; then
  urlencode() {
    _s=$1; _out=''; _i=0; _c=''
    while [ $_i -lt ${#_s} ]; do
      _c=${_s:$_i:1}
      case $_c in
        [a-zA-Z0-9._~-]) _out="$_out$_c" ;;
        *) _out="$_out$(printf '%%%02X' "'$_c")" ;;
      esac
      _i=$((_i+1))
    done
    printf '%s' "$_out"
  }
  POST "$BASE/login" "username=$(urlencode "$SMOKE_USER")&password=$(urlencode "$SMOKE_PASS")" \
       -c "$JAR" -H 'User-Agent: Mozilla/5.0 lane-smoke'
  sc=$(hdr Set-Cookie)
  if [ "$POST_CODE" != ERR ] && [ "$POST_CODE" -lt 400 ] && printf '%s' "$sc" | grep -qi 'connect\.sid'; then
    pass auth_login "POST /login (form username/password, core webui auth - dist/webui/routes/auth.js:83) -> HTTP $POST_CODE Set-Cookie: $sc"
  else
    fail auth_login "POST /login -> HTTP ${POST_CODE:-?} Set-Cookie: ${sc:-<none>} body: $(body_snip) (want <400 and a connect.sid cookie; lobby mode blocks /login until /welcome)"
  fi

  GET "$BASE/account" -b "$JAR" -c "$JAR"
  if [ "$GET_CODE" = 200 ]; then
    pass auth_account_page "GET /account (session cookie) -> HTTP 200 $(body_snip)"
  else
    fail auth_account_page "GET /account (session cookie) -> HTTP ${GET_CODE:-?} Location: $(loc_hdr)"
  fi

  GET "$BASE/" -b "$JAR" -c "$JAR"
  front=$(grep -o '"-FRONTURL",[[:space:]]*"[^"]*"' "$BODY" 2>/dev/null | head -n 1 || true)
  front_val=$(printf '%s' "$front" | sed 's/.*,[[:space:]]*"//; s/"$//')
  if [ "$GET_CODE" = 200 ] && body_has '"-DATAID=' && [ -n "$front" ] && [ "$front_val" = "$SMOKE_FRONT_URL" ] && ! body_has '"-AUTOSERVE"'; then
    pass play_page_splice "GET / (session) -> HTTP 200 with $(grep -o '"-DATAID=[^"]*"' "$BODY" | head -n 1) spliced in place of -AUTOSERVE and $front rewritten to '$SMOKE_FRONT_URL' (Program.cs:1375-1384)"
  else
    fail play_page_splice "GET / (session) -> HTTP ${GET_CODE:-?} front element: ${front:-<none>} (want value '$SMOKE_FRONT_URL'); -DATAID spliced: $(body_has '"-DATAID=' && printf yes || printf no); -AUTOSERVE leftover: $(body_has '"-AUTOSERVE"' && printf yes || printf no)"
  fi

  POST "$BASE/logout" '' -b "$JAR" -c "$JAR" -H 'User-Agent: Mozilla/5.0 lane-smoke'
  if [ "$POST_CODE" != ERR ] && [ "$POST_CODE" -lt 400 ]; then
    pass auth_logout "POST /logout -> HTTP $POST_CODE (gate evicts its session view and clears the cookie - Program.cs:1453-1462)"
  else
    fail auth_logout "POST /logout -> HTTP ${POST_CODE:-?} $(body_snip)"
  fi

  GET "$BASE/account" -b "$JAR" -c "$JAR"
  loc=$(loc_hdr)
  case "$GET_CODE" in
    302|401|403) pass auth_session_gone "GET /account after logout -> HTTP $GET_CODE Location: ${loc:-<none>} (session gone; a 302 bounce back to /login proves it)" ;;
    *) fail auth_session_gone "GET /account after logout -> HTTP $GET_CODE Location: ${loc:-<none>} (want 302/401/403; the session is still alive)" ;;
  esac
else
  printf 'SKIP auth_section | set SMOKE_USER and SMOKE_PASS to run login / account page / play page splice / logout / session-gone\n'
  SKIP_N=$((SKIP_N+1))
fi

# ---------------------------- summary ----------------------------
printf 'lane_smoke: %d PASS, %d FAIL, %d SKIP (target %s)\n' "$PASS_N" "$FAIL_N" "$SKIP_N" "$BASE"
if [ $FAIL_N -gt 0 ]; then
  printf 'lane_smoke: FAILED\n'
  exit 1
fi
printf 'lane_smoke: OK\n'
exit 0

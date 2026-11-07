#!/usr/bin/env bash
# lane_up.sh - Linux replacement for scripts/hosting_up.ps1 (92 lines), no systemd.
#
# Starts the three lane services:
#   core   node AsphyxiaCore.js with cwd <CORE_DIR>/dist            (hosting_up.ps1:53-54)
#   gate   dotnet <WEBFRONT_DIR>/bin/Release/net8.0/WebFront.dll    (hosting_up.ps1:76-78)
#          (the .dll runs through the runtime on Linux; the .exe apphost is Windows-only,
#           same reasoning as assemble_webroot.sh:58-60)
#   tunnel cloudflared tunnel run <TUNNEL_NAME>                     (hosting_up.ps1:87-91)
#          skipped when CLOUDFLARED=0
#
# Deliberate differences from hosting_up.ps1:
#   * nothing is ever reclaimed or killed by process name (hosting_up.ps1:31-34 is NOT
#     ported). Occupancy is decided only by probing the port (:36). A pid recorded in
#     PID_DIR is stopped only by lane_down.sh, after a command-line check.
#   * config.ini gets a timestamped copy before mfg_http_bind is rewritten.
#   * each child's pid is recorded in PID_DIR so lane_down.sh has something to stop.
#
# Usage
# -----
#   scripts/linux/lane_up.sh [--dry-run] [-h|--help]
#
#   --dry-run prints every command it would run and the full environment it would
#   export, without probing ports, without touching config.ini and without starting
#   anything. Secret values are never printed, dry-run included.
#
#   CORE_DIR        RyuNET-core checkout root.   default: <repo>/tmp/RyuNET-core
#                   <repo> = parent of scripts/ (the assemble_webroot.sh:92-98 model).
#                   hosting_up.ps1:23-25 resolved the C:\mfg junction; a Linux box has
#                   no junction, the path is used as given.
#   WEBFRONT_DIR    webfront checkout root.      default: <repo>/webfront
#   WEBROOT         gate webroot (WEBFRONT_ROOT). default: ~/mfgtmp/webroot (hosting_up.ps1:64)
#   CORE_HOST       core listen host, probed.    default: 127.0.0.1
#   CORE_PORT       core webui port, probed.     default: 8083
#                   (config.ini:1 port=8083; probed like hosting_up.ps1:36,56-60)
#   CORE_HTTP_PORT  plugin native listener port. default: 22421
#                   (config.ini:29 mfg_http_port; hosting_up.ps1:38 comment "native 22421
#                   listener"). Probed too: the plugin would fail EADDRINUSE with it
#                   taken. 0 = do not probe it.
#   GATE_PORT       gate port (WEBFRONT_PORT).   default: 8090 (hosting_up.ps1:67,80-84)
#   GATE_BIND       WEBFRONT_BIND: 'loopback' or 'any'. default: loopback
#                   (hosting_up.ps1:68; accepted values in Program.cs:343-344). Anything
#                   else is refused: Program.cs reads a value it does not recognise as
#                   loopback, so an unvalidated GATE_BIND looks like a reachable lane and is
#                   not one. The gate is then occupied/probed on the addresses this value
#                   actually listens on (loopback only, or loopback plus CORE_HOST).
#   CLOUDFLARED     0 = skip the tunnel entirely. default: 0
#                   hosting_up.ps1 arms this posture only with the explicit -Tunnel
#                   switch (:52, :71) and the verified VPS lane runs with CLOUDFLARED=0,
#                   cloudflared installed but no tunnel configured (README.md, "Still
#                   unverified"). A default of 1 armed Secure cookies and CF-Connecting-IP
#                   trust on a plain http loopback lane before any tunnel existed.
#                   With the tunnel on, WEB_SECURE_COOKIE=1 and WEBFRONT_TRUST_PROXY=
#                   cloudflare are exported ONLY after the tunnel is proven live.
#   TUNNEL_NAME     cloudflared tunnel name.     default: mfg (hosting_up.ps1:88)
#   TUNNEL_CONFIG   cloudflared config that must name TUNNEL_NAME before the posture is
#                   armed.                        default: ~/.cloudflared/config.yml
#                   (the config mfg-tunnel.service:25-26 and hosting_up.ps1:88 rely on)
#   TUNNEL_UP_TRIES poll attempts (0.5 s apart) for the tunnel to register a connection.
#                   default: 60 (= 30 s). The tunnel starts BEFORE the core and gate:
#                   they inherit the posture at exec, so it must be proven first.
#   PID_DIR         holds core.pid gate.pid tunnel.pid. default: ~/mfgtmp/lane_pids
#                   lane_up refuses to overwrite a pid file whose pid is alive.
#   LOG_DIR         per-child stdout/stderr.     default: ~/mfgtmp/lane_logs
#                   (hosting_up.ps1:53-54,77-78,88-89 redirected to
#                    C:\mfgtmp\ryunet.out.txt/.err.txt, webfront8090.out.txt/.err.txt,
#                    cloudflared.out.txt/.err.txt)
#   SEC_DIR         secrets directory.           default: ~/.mfg-web (hosting_up.ps1:19-21,73-74)
#                   presenter.secret is required (exported to the core as
#                   VFG_PRESENTER_SECRET, handed to the gate as
#                   WEBFRONT_PRESENTER_SECRET_FILE); access.code is optional
#                   (WEBFRONT_ACCESS_CODE_FILE). On Linux these files must be mode 600
#                   or stricter or lane_up refuses. Their contents are never printed.
#   WEBFRONT_PUBLIC_URL  caller-supplied absolute front URL, kept across the env clear
#                   and exported to the gate as WEBFRONT_PAGE_FRONTURL
#                   (hosting_up.ps1:11,75; Program.cs:50 default "/aog").
#   CORE_UP_TRIES   poll attempts (0.5 s apart) before "core did not bind". default: 120 (hosting_up.ps1:56-58)
#   GATE_UP_TRIES   poll attempts for the gate port.            default: 60  (hosting_up.ps1:80-82)
#
# Inherited WEBFRONT_* / mfg_http_* / VFG_PRESENTER_SECRET / WEB_SECURE_COOKIE values
# are cleared first, like hosting_up.ps1:12-17 (that list is 16 names; the caller's
# WEBFRONT_PUBLIC_URL is the one value :11 keeps).

set -euo pipefail
export LC_ALL=C

# ---------------------------- configuration ----------------------------
SCRIPT_PATH=${BASH_SOURCE[0]}
SCRIPT_DIR=${SCRIPT_PATH%/*}                 # <repo>/scripts/linux
REPO_ROOT=${SCRIPT_DIR%/*}                   # <repo>/scripts
REPO_ROOT=${REPO_ROOT%/*}                    # <repo>

CORE_DIR=${CORE_DIR:-"$REPO_ROOT/tmp/RyuNET-core"}
WEBFRONT_DIR=${WEBFRONT_DIR:-"$REPO_ROOT/webfront"}
WEBROOT=${WEBROOT:-"$HOME/mfgtmp/webroot"}
CORE_HOST=${CORE_HOST:-'127.0.0.1'}
CORE_PORT=${CORE_PORT:-8083}
CORE_HTTP_PORT=${CORE_HTTP_PORT:-22421}
GATE_PORT=${GATE_PORT:-8090}
GATE_BIND=${GATE_BIND:-'loopback'}
CLOUDFLARED=${CLOUDFLARED:-0}                # posture is armed only on a proven tunnel
TUNNEL_NAME=${TUNNEL_NAME:-'mfg'}
TUNNEL_CONFIG=${TUNNEL_CONFIG:-"$HOME/.cloudflared/config.yml"}
TUNNEL_UP_TRIES=${TUNNEL_UP_TRIES:-60}
PID_DIR=${PID_DIR:-"$HOME/mfgtmp/lane_pids"}
LOG_DIR=${LOG_DIR:-"$HOME/mfgtmp/lane_logs"}
SEC_DIR=${SEC_DIR:-"$HOME/.mfg-web"}
CORE_UP_TRIES=${CORE_UP_TRIES:-120}
GATE_UP_TRIES=${GATE_UP_TRIES:-60}
POLL_STEP=0.5                                # never above 1 s per iteration (hosting_up.ps1:58,82 use 500 ms)

DRY_RUN=0

usage() {
  sed -n '2,/^set -euo/p' "$SCRIPT_PATH" | sed '$d'
}

die() { printf 'lane_up: %s\n' "$1" >&2; exit 1; }
say() { printf 'lane_up: %s\n' "$1"; }

while [ $# -gt 0 ]; do
  case $1 in
    --dry-run) DRY_RUN=1 ;;
    -h|--help) usage; exit 0 ;;
    *)         die "unknown argument: $1" ;;
  esac
  shift
done

# WEBFRONT_BIND is a two-value switch on the front side as well: webfront/Program.cs:343-344
# treats anything that is not 'any' as loopback. An unvalidated GATE_BIND (a typo, 'true',
# 'lan') was exported as-is, so the operator believed the lane was reachable while the gate
# bound loopback. Refused here, and the addresses the gate can be reached on are derived
# from the validated value rather than guessed from CORE_HOST.
case $GATE_BIND in
  loopback|any) : ;;
  *) die "GATE_BIND must be 'loopback' or 'any' (webfront/Program.cs:343-344 reads anything else as loopback)" ;;
esac

# Where the gate answers once it is up. The gate always starts on this box, so CORE_HOST -
# where the CORE lives - is not automatically where the GATE listens: a loopback-bound gate
# answers on 127.0.0.1 only, and probing CORE_HOST for it reports a healthy gate as dead.
# 'any' listens everywhere, loopback included, so both are probed.
gate_probe_hosts='127.0.0.1'
gate_probe_desc='127.0.0.1'
if [ "$GATE_BIND" = any ] && [ "$CORE_HOST" != '127.0.0.1' ]; then
  gate_probe_hosts="127.0.0.1 $CORE_HOST"
  gate_probe_desc="127.0.0.1 or $CORE_HOST"
fi

# ---------------------------- helpers ----------------------------

_norm_name() { printf '%s' "$1" | tr -d '"' | tr 'A-Z' 'a-z' | sed 's/\.exe$//; s/\.dll$//'; }   # tr -d '"' tolerates MSYS's quoted argv0 synthesis

# Port occupancy is decided by probing the port, never by looking at process names
# (hosting_up.ps1:36 used Get-NetTCPConnection -LocalPort 8083 -State Listen).
# /dev/tcp is a bash built-in pseudo-device: a connect attempt succeeds only while
# something listens, and the subshell closes the fd on exit.
port_in_use() {
  (exec 3<>"/dev/tcp/$1/$2") 2>/dev/null
}

# first line, blanks trimmed - same reading rule lane_down.sh and assemble_webroot.sh:199-204 use
read_pid_file() { sed -n '1p' "$1" 2>/dev/null | tr -d '\r\t' | sed 's/^ *//; s/ *$//'; }

# command-line fields of a pid (NUL-separated /proc entry -> one argv word per line)
cmdline_fields() {
  mapfile -t _cf_out < <(tr '\0' '\n' < "/proc/$1/cmdline" 2>/dev/null)
}

# Does the pid's command line belong to the lane child we think it is?
# core:  argv[0] is node/nodejs AND some argv word is AsphyxiaCore.js
# gate:  argv[0] or argv[1] basename normalizes to webfront (dotnet WebFront.dll form)
# tunnel:argv[0] basename normalizes to cloudflared
cmdline_matches() {
  cmdline_fields "$2"
  local _kind=$1 _f0=${_cf_out[0]:-} _f1=${_cf_out[1]:-} _n0 _n1 _ok=0 _w
  _n0=$(_norm_name "${_f0##*[\\/]}")
  _n1=$(_norm_name "${_f1##*[\\/]}")
  case $_kind in
    core)
      case $_n0 in node|nodejs) : ;; *) return 1 ;; esac
      for _w in "${_cf_out[@]}"; do
        [ "${_w##*[\\/]}" = 'AsphyxiaCore.js' ] && _ok=1
      done
      [ $_ok = 1 ]
      ;;
    gate)
      [ "$_n0" = webfront ] || [ "$_n1" = webfront ]
      ;;
    tunnel)
      [ "$_n0" = cloudflared ]
      ;;
    *) return 1 ;;
  esac
}

pid_alive() { [ -e "/proc/$1" ] || kill -0 "$1" 2>/dev/null; }

# Identity rule two: the pid's working directory. AsphyxiaCore rewrites the node process
# title, so a running core's /proc/<pid>/cmdline reads "MFG Local v1.70b | P" and no
# command-line rule can see node or AsphyxiaCore.js (observed on the VPS lane,
# 2026-11-05). lane_up.sh starts the core with cwd at <CORE_DIR>/dist and the gate with
# cwd at <WEBFRONT_DIR>/bin/Release/net8.0, so the cwd is a stronger signal than argv.
# readlink -f resolves the kernel symlink and the expected path the same way, so the
# comparison only ever matches the directory itself.
cwd_of() { readlink -f "/proc/$1/cwd" 2>/dev/null || true; }

cwd_matches() {
  local _want _cwd
  case $1 in
    core) _want="$CORE_DIR/dist" ;;
    gate) _want="$WEBFRONT_DIR/bin/Release/net8.0" ;;
    *) return 1 ;;
  esac
  _want=$(readlink -f "$_want" 2>/dev/null) || true
  [ -n "$_want" ] || return 1
  _cwd=$(cwd_of "$2")
  [ -n "$_cwd" ] && [ "$_cwd" = "$_want" ]
}

entry_matches() { cmdline_matches "$1" "$2" || cwd_matches "$1" "$2"; }

# refuse to overwrite a pid file whose pid is alive (lane_up requirement);
# a stale file (process gone) is replaced with a warning
guard_pid_file() {
  local _kind=$1 _name=$2 _file="$PID_DIR/$1.pid" _pid
  [ -f "$_file" ] || return 0
  _pid=$(read_pid_file "$_file")
  case $_pid in ''|*[!0-9]*) die "$_name pid file '$_file' holds no usable pid; clear it or run lane_down.sh first" ;; esac
  if [ "$_pid" -le 1 ]; then die "$_name pid file holds pid $_pid; refusing to touch it"; fi
  if [ "$_pid" = "$$" ] || [ "$_pid" = "$PPID" ]; then die "$_name pid file holds our own pid $_pid; refusing to touch it"; fi
  if ! pid_alive "$_pid"; then say "$_name pid file is stale (pid $_pid not running); it will be replaced"; return 0; fi
  if entry_matches "$_kind" "$_pid"; then
    die "$_name already running (pid $_pid, verified by command line or working directory) - run lane_down.sh before starting a new lane"
  else
    die "$_name pid file points at live pid $_pid that is neither ($_name by command line nor by working directory $(cwd_of "$_pid")); refusing to overwrite it"
  fi
}

# bounded poll; each sleep is POLL_STEP (0.5 s, under the 1 s cap)
wait_up() {
  local _host=$1 _port=$2 _tries=$3 _i=1
  while [ $_i -le $_tries ]; do
    port_in_use "$_host" "$_port" && return 0
    sleep "$POLL_STEP"
    _i=$((_i+1))
  done
  return 1
}

# The same rules for the gate, over the addresses its WEBFRONT_BIND actually listens on.
gate_port_in_use() {   # is the gate port already taken on any address the gate would use?
  for _h in $gate_probe_hosts; do port_in_use "$_h" "$1" && return 0; done
  return 1
}

wait_gate_up() {       # bounded poll over those addresses, same cadence as wait_up
  local _i=1
  while [ $_i -le "$GATE_UP_TRIES" ]; do
    for _h in $gate_probe_hosts; do port_in_use "$_h" "$GATE_PORT" && return 0; done
    sleep "$POLL_STEP"
    _i=$((_i+1))
  done
  return 1
}

# The tunnel proves itself before anything that depends on it is armed: the child must
# still be alive AND its own log must report a registered connection (cloudflared prints
# "Registered tunnel connection" when the tunnel is up at the Cloudflare edge). Nothing
# about the tunnel is inferred from the exit status of `tunnel run`.
wait_tunnel() {
  local _pid=$1 _tries=$2 _i=1
  while [ $_i -le $_tries ]; do
    pid_alive "$_pid" || return 1
    grep -q 'Registered tunnel connection' "$tunnelOut" "$tunnelErr" 2>/dev/null && return 0
    sleep "$POLL_STEP"
    _i=$((_i+1))
  done
  return 1
}

# A posture is only armable against a tunnel this box can actually start: the config must
# exist and must name TUNNEL_NAME (`tunnel: <name>` at the top level, or an entry of a
# `tunnels:` map - the shape scripts/hosting_cloudflared.yml:1 shows).
tunnel_preflight() {
  [ -f "$TUNNEL_CONFIG" ] || die "CLOUDFLARED=$CLOUDFLARED but there is no cloudflared config at $TUNNEL_CONFIG: refusing to arm WEB_SECURE_COOKIE/WEBFRONT_TRUST_PROXY with no tunnel behind them (set CLOUDFLARED=0 for a plain http lane)"
  grep -Eq "^[[:space:]]*tunnels?[[:space:]]*:[[:space:]]*['\"]?${TUNNEL_NAME}(['\"]|[[:space:]]*$)|^[[:space:]]+${TUNNEL_NAME}[[:space:]]*:" "$TUNNEL_CONFIG" \
    || die "$TUNNEL_CONFIG does not name tunnel \"$TUNNEL_NAME\": refusing to arm the Secure-cookie posture (set CLOUDFLARED=0, or point TUNNEL_CONFIG/TUNNEL_NAME at the real tunnel)"
}
# identity cannot be proven the pid file is KEPT - a live process must never be left
# without a handle (same rule as lane_down.sh, and the 2026-11-05 core incident).
cleanup_child() {
  local _kind=$1 _pid=$2 _file="$PID_DIR/$1.pid"
  if ! pid_alive "$_pid"; then rm -f -- "$_file"; return 0; fi   # nothing alive to strand (lane_down.sh:169-173 rule)
  if entry_matches "$_kind" "$_pid"; then
    kill -TERM "$_pid" 2>/dev/null || true
    sleep 1
    kill -KILL "$_pid" 2>/dev/null || true
    rm -f -- "$_file"
  else
    say "warning: pid $_pid cannot be verified as the $_kind child just started (working directory $(cwd_of "$_pid")); not signalled; pid file KEPT ($_file) - run lane_down.sh to deal with it"
  fi
}

trim_line() { sed 's/^[[:space:]]*//; s/[[:space:]]*$//'; }

# ---------------------------- 1. clean inherited posture ----------------------------
# hosting_up.ps1:11-17. WEBFRONT_PUBLIC_URL is captured before the clear (:11) and
# re-exported to the gate later (:75). Everything else in the :12-15 list is unset.
publicUrl=${WEBFRONT_PUBLIC_URL:-''}
for _n in WEBFRONT_ROOT WEBFRONT_UPSTREAM WEBFRONT_AUTH WEBFRONT_PORT WEBFRONT_BIND \
          WEBFRONT_GATE WEBFRONT_TRUST_PROXY WEBFRONT_PRESENTER_SECRET \
          WEBFRONT_PRESENTER_SECRET_FILE WEBFRONT_ACCESS_CODE WEBFRONT_ACCESS_CODE_FILE \
          WEBFRONT_PAGE_FRONTURL mfg_http_bind mfg_http_port VFG_PRESENTER_SECRET \
          WEB_SECURE_COOKIE; do
  unset "$_n" 2>/dev/null || true
done

# ---------------------------- 2. secrets (hosting_up.ps1:19-21) ----------------------------
secFile="$SEC_DIR/presenter.secret"
if [ ! -f "$secFile" ]; then
  die "presenter secret missing: create $secFile first (random bytes, see hosting_deploy.md)"
fi
# hosting_up.ps1:21 did (Get-Content -Raw).Trim(); the lane keeps single-line secrets:
if [ "$(awk 'END{print NR}' "$secFile")" -gt 1 ]; then die "$secFile must hold a single line"; fi
secret=$(head -n 1 "$secFile" | tr -d '\r' | trim_line)
[ -n "$secret" ] || die "$secFile is empty"

# mode 600 or stricter, enforced on Linux (the deployment target); warned elsewhere
_is_linux=0
[ "$(uname -s 2>/dev/null || echo unknown)" = 'Linux' ] && _is_linux=1
if [ $_is_linux = 1 ]; then
  for _f in "$secFile" "$SEC_DIR/access.code"; do
    [ -f "$_f" ] || continue
    _mode=$(stat -c '%a' "$_f" 2>/dev/null) || die "cannot stat '$_f'"
    _other=$(( 8#$_mode & 8#7 ))
    _group=$(( (8#$_mode >> 3) & 8#7 ))
    if [ $_other -ne 0 ] || [ $_group -ne 0 ]; then
      die "'$_f' is mode $_mode; secrets under $SEC_DIR must be 600 or stricter (no group/other bits)"
    fi
  done
else
  say "note: secret-file mode enforcement (600 or stricter) applies on Linux; this host is not Linux, continuing"
fi
codeFile="$SEC_DIR/access.code"

# ---------------------------- 3. paths and tools ----------------------------
coreDist="$CORE_DIR/dist"
gateWd="$WEBFRONT_DIR/bin/Release/net8.0"
coreErr="$LOG_DIR/core.err.txt"
coreOut="$LOG_DIR/core.out.txt"
gateErr="$LOG_DIR/gate.err.txt"
gateOut="$LOG_DIR/gate.out.txt"
tunnelErr="$LOG_DIR/tunnel.err.txt"
tunnelOut="$LOG_DIR/tunnel.out.txt"

require_path() {
  if [ ! -e "$2" ]; then
    if [ $DRY_RUN = 1 ]; then say "dry-run: missing $1: $2 (would be fatal for real)"
    else die "no $1 at $2"; fi
  fi
}
require_path "core dist dir" "$coreDist"
require_path "AsphyxiaCore.js" "$coreDist/AsphyxiaCore.js"
require_path "WebFront.dll" "$gateWd/WebFront.dll"
# dist is not self-contained: the plugin's handler fallback (dist/plugins/mfg@asphyxia/
# index.ts:369-370) requires <core>/src/aog/AogRegistry, so src must travel with dist
if [ ! -e "$CORE_DIR/src/aog/AogRegistry.ts" ] && [ ! -e "$coreDist/src/aog/AogRegistry.js" ]; then
  say "warning: no src/aog/AogRegistry under $CORE_DIR - the mfg@asphyxia handler fallback (index.ts:370) will fail"
fi
for _t in node dotnet; do
  if ! command -v "$_t" >/dev/null 2>&1; then
    if [ $DRY_RUN = 1 ]; then say "dry-run: '$_t' not on PATH (would be fatal for real)"
    else die "'$_t' not on PATH"; fi
  fi
done
if [ "$CLOUDFLARED" != 0 ] && ! command -v cloudflared >/dev/null 2>&1; then
  if [ $DRY_RUN = 1 ]; then say "dry-run: 'cloudflared' not on PATH (would be fatal for real)"
  else die "cloudflared not on PATH (set CLOUDFLARED=0 to skip the tunnel)"; fi
fi
# the config must name the tunnel before any posture can be armed (read-only check, run
# in dry-run too so the operator sees the refusal before a real run)
if [ "$CLOUDFLARED" != 0 ]; then tunnel_preflight; fi

# ---------------------------- 4. pid files must not cover live children ----------------------------
guard_pid_file core core
guard_pid_file gate gate
guard_pid_file tunnel tunnel

# ---------------------------- 5. refuse occupied ports ----------------------------
# The hosting_up.ps1:36 shape, extended to every port the lane takes: a lane is
# never started on top of a listening port, and nothing is reclaimed by name.
if [ $DRY_RUN = 1 ]; then
  say "dry-run: port probes skipped (no connect attempts are made)"
else
  port_in_use "$CORE_HOST" "$CORE_PORT" && \
    die "a core already answers $CORE_HOST:$CORE_PORT - the server is still up (close it, or run lane_down.sh)"
  if [ "$CORE_HTTP_PORT" != 0 ]; then
    port_in_use "$CORE_HOST" "$CORE_HTTP_PORT" && \
      die "something already answers $CORE_HOST:$CORE_HTTP_PORT (the plugin native listener port, config.ini mfg_http_port)"
  fi
  gate_port_in_use "$GATE_PORT" && \
    die "something already answers $gate_probe_desc:$GATE_PORT (the gate port, probed where WEBFRONT_BIND=$GATE_BIND actually listens) - refusing to start the gate on it"
fi

# ---------------------------- 6. config.ini (hosting_up.ps1:39-45) ----------------------------
# Patched only while the core is not listening - the port probes above are exactly
# the hosting_up invariant (:36 runs before :41-45; :39 comment "config.ini is
# patched while the core is stopped"). hosting_up.ps1:43 replaced the regex
# 'mfg_http_bind=[^\r\n]*' with 'mfg_http_bind=127.0.0.1' over the raw text;
# sed's [^[:cntrl:]]* is the same character set minus CR/LF, so a CRLF file keeps
# its line endings.
ini="$coreDist/config.ini"
iniStamp=$(date +%Y%m%d%H%M%S)
if [ -f "$ini" ]; then
  iniBackup="$ini.bak.$iniStamp"
  if [ $DRY_RUN = 1 ]; then
    say "dry-run: cp -p '$ini' '$iniBackup'"
    say "dry-run: sed 's/mfg_http_bind=[^[:cntrl:]]*/mfg_http_bind=127.0.0.1/g' applied to '$ini' in place"
  else
    cp -p "$ini" "$iniBackup"
    _tmp="$ini.lane.tmp"
    sed 's/mfg_http_bind=[^[:cntrl:]]*/mfg_http_bind=127.0.0.1/g' "$ini" > "$_tmp"
    cat "$_tmp" > "$ini"
    rm -f -- "$_tmp"
    say "config.ini: mfg_http_bind rewritten to 127.0.0.1 (timestamped copy: $iniBackup)"
  fi
else
  die "no config.ini at $coreDist - keep mfg_http_bind=127.0.0.1 in the environment"   # hosting_up.ps1:45
fi

# ---------------------------- 7. the plan (--dry-run exits here) ----------------------------
if [ $DRY_RUN = 1 ]; then
  printf '%s\n' \
    'lane_up --dry-run: nothing executed, no probes, no writes' \
    "  mkdir -p '$LOG_DIR' '$PID_DIR'"
  if [ "$CLOUDFLARED" != 0 ]; then
    printf '%s\n' \
      '' \
      "  tunnel FIRST (hosting_up.ps1:88-89, moved before the children because the posture is inherited at exec):" \
      "    nohup cloudflared tunnel run '$TUNNEL_NAME' >'$tunnelOut' 2>'$tunnelErr' &" \
      "    echo \$! > '$PID_DIR/tunnel.pid'" \
      "    wait: 'Registered tunnel connection' in $tunnelOut/$tunnelErr, $TUNNEL_UP_TRIES tries, sleep $POLL_STEP" \
      "    no registered connection -> tunnel stopped, lane NOT started (no armed posture without a proven tunnel)"
  else
    printf '%s\n' '' '  tunnel: skipped (CLOUDFLARED=0) - plain http LAN posture'
  fi
  printf '%s\n' \
    '' \
    '  environment exported for the CORE child (hosting_up.ps1:46-52):' \
    "    VFG_PRESENTER_SECRET=<value read from $secFile - content not printed>" \
    '    mfg_http_bind=127.0.0.1'
  [ "$CLOUDFLARED" != 0 ] && printf '%s\n' '    WEB_SECURE_COOKIE=1   # armed only on the proven tunnel above'
  printf '%s\n' \
    '' \
    "  core (cwd $coreDist, like hosting_up.ps1:53 -WorkingDirectory <ryuDist>):" \
    "    nohup bash -c 'cd \"\$1\" && exec node AsphyxiaCore.js' _ '$coreDist' >'$coreOut' 2>'$coreErr' &" \
    "    echo \$! > '$PID_DIR/core.pid'" \
    "  wait: probe the core at $CORE_HOST:$CORE_PORT (and $CORE_HOST:$CORE_HTTP_PORT for the plugin listener), $CORE_UP_TRIES tries, sleep $POLL_STEP" \
    '' \
    '  environment exported for the GATE child (hosting_up.ps1:63-75):' \
    "    WEBFRONT_ROOT='$WEBROOT'" \
    "    WEBFRONT_UPSTREAM='http://$CORE_HOST:$CORE_PORT'" \
    "    WEBFRONT_AUTH='http://$CORE_HOST:$CORE_PORT'" \
    "    WEBFRONT_PORT='$GATE_PORT'" \
    "    WEBFRONT_BIND='$GATE_BIND'" \
    "    WEBFRONT_GATE='1'" \
    "    WEBFRONT_PRESENTER_SECRET_FILE='$secFile'"
  [ "$CLOUDFLARED" != 0 ] && printf '%s\n' "    WEBFRONT_TRUST_PROXY='cloudflare'   # armed only on the proven tunnel above"
  [ -f "$codeFile" ] && printf '%s\n' "    WEBFRONT_ACCESS_CODE_FILE='$codeFile'"
  [ -n "$publicUrl" ] && printf '%s\n' "    WEBFRONT_PAGE_FRONTURL='$publicUrl'   # from caller's WEBFRONT_PUBLIC_URL (hosting_up.ps1:11,75)"
  printf '%s\n' \
    '' \
    "  gate (cwd $gateWd, like hosting_up.ps1:76-77):" \
    "    nohup bash -c 'cd \"\$1\" && exec dotnet WebFront.dll' _ '$gateWd' >'$gateOut' 2>'$gateErr' &" \
    "    echo \$! > '$PID_DIR/gate.pid'" \
    "  wait: probe the gate at $gate_probe_desc:$GATE_PORT (WEBFRONT_BIND=$GATE_BIND), $GATE_UP_TRIES tries, sleep $POLL_STEP"
  printf '%s\n' '' 'lane_up --dry-run: done (no secrets printed)'
  exit 0
fi

# ---------------------------- 8. tunnel first: it is what arms the posture ----------------------------
# The core and gate inherit the posture at exec, so the tunnel has to be proven before
# they start. hosting_up.ps1 starts the tunnel last (:87-91) but only arms the posture
# with the explicit -Tunnel switch (:52, :71); with a default-on tunnel that ordering
# reported a hosted lane with Secure cookies and CF-Connecting-IP trust and no proof
# either of them had a tunnel behind them.
mkdir -p -- "$LOG_DIR" "$PID_DIR"
tunnelPid=''
if [ "$CLOUDFLARED" != 0 ]; then
  nohup cloudflared tunnel run "$TUNNEL_NAME" >"$tunnelOut" 2>"$tunnelErr" &
  tunnelPid=$!
  echo "$tunnelPid" > "$PID_DIR/tunnel.pid"
  if ! wait_tunnel "$tunnelPid" "$TUNNEL_UP_TRIES"; then
    tail -n 12 "$tunnelErr" >&2 2>/dev/null || true
    tail -n 6 "$tunnelOut" 2>/dev/null | sed 's/^/tunnel: /' >&2 || true
    cleanup_child tunnel "$tunnelPid"
    die "cloudflared tunnel \"$TUNNEL_NAME\" did not register a connection (see $tunnelOut / $tunnelErr): the lane stays down rather than run with WEB_SECURE_COOKIE=1 and WEBFRONT_TRUST_PROXY=cloudflare on a plain http front"
  fi
  say "tunnel proven: \"$TUNNEL_NAME\" registered a connection (pid $tunnelPid, log $tunnelOut)"
else
  say "tunnel skipped (CLOUDFLARED=0): plain http LAN posture - no WEB_SECURE_COOKIE, no WEBFRONT_TRUST_PROXY"
fi

stop_tunnel_on_lane_failure() {   # the lane is not up: do not leave a registered tunnel fronting nothing
  if [ "$CLOUDFLARED" != 0 ] && [ -n "$tunnelPid" ]; then
    cleanup_child tunnel "$tunnelPid"
    say "tunnel pid $tunnelPid stopped too (the lane is down)"
  fi
}

# ---------------------------- 9. core up (hosting_up.ps1:46-61) ----------------------------
export VFG_PRESENTER_SECRET=$secret
export mfg_http_bind=127.0.0.1
if [ "$CLOUDFLARED" != 0 ]; then export WEB_SECURE_COOKIE=1; fi   # hosting_up.ps1:52; tunnel proven in section 8

nohup bash -c 'cd "$1" && exec node AsphyxiaCore.js' _ "$coreDist" >"$coreOut" 2>"$coreErr" &
corePid=$!
echo "$corePid" > "$PID_DIR/core.pid"

if ! wait_up "$CORE_HOST" "$CORE_PORT" "$CORE_UP_TRIES"; then
  tail -n 12 "$coreErr" >&2 2>/dev/null || true        # hosting_up.ps1:60 prints the core error tail
  cleanup_child core "$corePid"
  stop_tunnel_on_lane_failure
  die "core did not bind $CORE_HOST:$CORE_PORT (see $coreErr); core pid file removed only while the child was still identifiable"
fi
say "core up: presenter armed, mfg_http_bind=127.0.0.1 (pid $corePid, cwd $coreDist)"

# ---------------------------- 10. gate up (hosting_up.ps1:63-85) ----------------------------
export WEBFRONT_ROOT=$WEBROOT
export WEBFRONT_UPSTREAM="http://$CORE_HOST:$CORE_PORT"
export WEBFRONT_AUTH="http://$CORE_HOST:$CORE_PORT"
export WEBFRONT_PORT=$GATE_PORT
export WEBFRONT_BIND=$GATE_BIND
export WEBFRONT_GATE=1
export WEBFRONT_PRESENTER_SECRET_FILE=$secFile
if [ "$CLOUDFLARED" != 0 ]; then export WEBFRONT_TRUST_PROXY=cloudflare; fi
if [ -f "$codeFile" ]; then export WEBFRONT_ACCESS_CODE_FILE=$codeFile; fi
if [ -n "$publicUrl" ]; then export WEBFRONT_PAGE_FRONTURL=$publicUrl; fi

nohup bash -c 'cd "$1" && exec dotnet WebFront.dll' _ "$gateWd" >"$gateOut" 2>"$gateErr" &
gatePid=$!
echo "$gatePid" > "$PID_DIR/gate.pid"

if ! wait_gate_up; then
  tail -n 6 "$gateErr" >&2 2>/dev/null || true         # hosting_up.ps1:84 prints the gate error tail
  cleanup_child gate "$gatePid"
  stop_tunnel_on_lane_failure
  say "the core is still up (pid $(cat "$PID_DIR/core.pid" 2>/dev/null || echo '?')); run lane_down.sh to stop it"
  die "webfront did not answer on $gate_probe_desc:$GATE_PORT (WEBFRONT_BIND=$GATE_BIND, see $gateErr)"
fi
tail -n 2 "$gateOut" 2>/dev/null || true               # hosting_up.ps1:85 echoes the gate banner
say "gate up (pid $gatePid, cwd $gateWd, WEBFRONT_GATE=1, bind $GATE_BIND, answering $gate_probe_desc:$GATE_PORT)"

say "lane up: core pid $corePid, gate pid $gatePid$([ "$CLOUDFLARED" != 0 ] && printf ', tunnel pid %s (proven)' "$tunnelPid"); pids in $PID_DIR"
if [ "$CLOUDFLARED" = 0 ]; then
  say "lane up on plain http: WEB_SECURE_COOKIE and WEBFRONT_TRUST_PROXY are NOT set - anonymous visitors reach the gate directly (lane_smoke.sh checks the bounce)"
else
  say "hosted lane up - anonymous visitors must bounce to /login (hosting_up.ps1:92); lane_smoke.sh checks it"
fi

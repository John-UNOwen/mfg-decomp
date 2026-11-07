#!/usr/bin/env bash
# install_systemd.sh - stage substituted systemd units for the lane. NEVER runs sudo.
#
# Reads the template units next to this script (mfg-lane.service, mfg-gate.service,
# mfg-tunnel.service), substitutes the real paths and the real user into every
# @PLACEHOLDER@ token, and writes the results into a staging directory - not into
# /etc. The human reviewer installs them with the exact sudo commands this script
# prints at the end. This script never invokes sudo, systemctl, or writes anything
# outside the staging directory.
#
# Usage
# -----
#   scripts/linux/install_systemd.sh TARGET_DIR [--staging DIR] [-h|--help]
#       [--user NAME] [--home DIR] [--core-dir DIR] [--webfront-dir DIR]
#       [--webroot DIR] [--sec-dir DIR] [--env-file PATH] [--tunnel-name NAME]
#       [--node-bin PATH] [--dotnet-bin PATH] [--cloudflared-bin PATH]
#       [--core-port N] [--gate-port N]
#
#   TARGET_DIR        the lane repo root on the Linux box. Defaults under it:
#                     core @CORE_DIR@      = TARGET_DIR/tmp/RyuNET-core
#                                          (hosting_up.ps1:23-25 shape on Linux)
#                     gate @WEBFRONT_DIR@  = TARGET_DIR/webfront
#   --staging DIR     where substituted units + lane.env are written.
#                     default: <this script's directory>/systemd-staging
#   --user NAME       User= in every unit. default: current user id
#   --home DIR        target user's home (for @HOME@ in the tunnel unit). default: $HOME
#   --core-dir / --webfront-dir / --webroot / --sec-dir   override the paths
#   --env-file PATH   EnvironmentFile= shared by core and gate. default: /etc/mfg/lane.env
#   --tunnel-name     cloudflared tunnel name. default: mfg (hosting_up.ps1:88)
#   --node-bin etc.   absolute binaries for ExecStart= (systemd needs absolute paths).
#                     default: /usr/bin/node /usr/bin/dotnet /usr/bin/cloudflared;
#                     overridden by the matching options
#   --core-port       core webui port written into lane.env. default: 8083 (config.ini:1)
#   --gate-port       gate port written into lane.env (WEBFRONT_PORT). default: 8090 (hosting_up.ps1:67)
#   --bind            WEBFRONT_BIND value in lane.env. default: loopback (hosting_up.ps1:68)
#   (no tunnel flag)   plain http lane: no mfg-tunnel.service, and WEBFRONT_TRUST_PROXY stays
#                     out of lane.env. This is the default, the same posture lane_up.sh uses
#                     with CLOUDFLARED=0 (L46): arming the Secure-cookie/CF-Connecting-IP
#                     posture with no tunnel behind it lets a client claim any
#                     CF-Connecting-IP it likes.
#   --tunnel          stage mfg-tunnel.service and arm WEBFRONT_TRUST_PROXY=cloudflare, but
#                     only after the cloudflared config is read and names --tunnel-name. The
#                     preflight runs before anything is staged: a refused tunnel leaves the
#                     staging directory empty, not half a hosted posture.
#   --tunnel-config   cloudflared config to check. default: <home>/.cloudflared/config.yml
#                     (lane_up.sh:99; the same directory mfg-tunnel.service's ReadWritePaths=)
#   --no-tunnel       explicit plain http (the default; kept for scripts that pass it)
#
# Values containing a newline, a '|' or a '&' are refused (they would corrupt the
# substitution). The staged lane.env holds no secret values; VFG_PRESENTER_SECRET
# belongs in <sec-dir>/core.env at mode 600 (see mfg-lane.service).

set -euo pipefail
export LC_ALL=C

# ---------------------------- configuration ----------------------------
SCRIPT_PATH=${BASH_SOURCE[0]}
SCRIPT_DIR=${SCRIPT_PATH%/*}

STAGING_DIR="$SCRIPT_DIR/systemd-staging"
LANE_USER=$(id -un 2>/dev/null || echo mfg)
TARGET_HOME=$HOME
TARGET_DIR=''
ENV_FILE='/etc/mfg/lane.env'
TUNNEL_NAME='mfg'
NODE_BIN='/usr/bin/node'
DOTNET_BIN='/usr/bin/dotnet'
CLOUDFLARED_BIN='/usr/bin/cloudflared'
CORE_PORT=8083
GATE_PORT=8090
GATE_BIND='loopback'
WITH_TUNNEL=0                                  # plain http unless --tunnel says otherwise (L46 posture)

usage() {
  sed -n '2,/^set -euo/p' "$SCRIPT_PATH" | sed '$d'
}

die() { printf 'install_systemd: %s\n' "$1" >&2; exit 1; }
say() { printf 'install_systemd: %s\n' "$1"; }

if [ $# -gt 0 ] && [ "${1#--}" = "$1" ]; then
  TARGET_DIR=$1
  shift
fi
while [ $# -gt 0 ]; do
  case $1 in
    --staging)         [ $# -ge 2 ] || die "--staging needs a value"; STAGING_DIR=$2; shift 2 ;;
    --user)            [ $# -ge 2 ] || die "--user needs a value"; LANE_USER=$2; shift 2 ;;
    --home)            [ $# -ge 2 ] || die "--home needs a value"; TARGET_HOME=$2; shift 2 ;;
    --core-dir)        [ $# -ge 2 ] || die "--core-dir needs a value"; CORE_DIR=$2; shift 2 ;;
    --webfront-dir)    [ $# -ge 2 ] || die "--webfront-dir needs a value"; WEBFRONT_DIR=$2; shift 2 ;;
    --webroot)         [ $# -ge 2 ] || die "--webroot needs a value"; WEBROOT=$2; shift 2 ;;
    --sec-dir)         [ $# -ge 2 ] || die "--sec-dir needs a value"; SEC_DIR=$2; shift 2 ;;
    --env-file)        [ $# -ge 2 ] || die "--env-file needs a value"; ENV_FILE=$2; shift 2 ;;
    --tunnel-name)     [ $# -ge 2 ] || die "--tunnel-name needs a value"; TUNNEL_NAME=$2; shift 2 ;;
    --node-bin)        [ $# -ge 2 ] || die "--node-bin needs a value"; NODE_BIN=$2; shift 2 ;;
    --dotnet-bin)      [ $# -ge 2 ] || die "--dotnet-bin needs a value"; DOTNET_BIN=$2; shift 2 ;;
    --cloudflared-bin) [ $# -ge 2 ] || die "--cloudflared-bin needs a value"; CLOUDFLARED_BIN=$2; shift 2 ;;
    --core-port)       [ $# -ge 2 ] || die "--core-port needs a value"; CORE_PORT=$2; shift 2 ;;
    --gate-port)       [ $# -ge 2 ] || die "--gate-port needs a value"; GATE_PORT=$2; shift 2 ;;
    --bind)            [ $# -ge 2 ] || die "--bind needs a value"; GATE_BIND=$2; shift 2 ;;
    --tunnel)          WITH_TUNNEL=1; shift ;;
    --tunnel-config)   [ $# -ge 2 ] || die "--tunnel-config needs a value"; TUNNEL_CONFIG=$2; shift 2 ;;
    --no-tunnel)       WITH_TUNNEL=0; shift ;;
    -h|--help)         usage; exit 0 ;;
    *)                 die "unknown argument: $1" ;;
  esac
done

[ -n "$TARGET_DIR" ] || die "TARGET_DIR is required (the lane repo root on the Linux box)"
case $TARGET_DIR in /*) : ;; *) die "TARGET_DIR must be an absolute path: $TARGET_DIR" ;; esac

CORE_DIR=${CORE_DIR:-"$TARGET_DIR/tmp/RyuNET-core"}
WEBFRONT_DIR=${WEBFRONT_DIR:-"$TARGET_DIR/webfront"}
WEBROOT=${WEBROOT:-"$TARGET_HOME/mfgtmp/webroot"}
SEC_DIR=${SEC_DIR:-"$TARGET_HOME/.mfg-web"}
TUNNEL_CONFIG=${TUNNEL_CONFIG:-"$TARGET_HOME/.cloudflared/config.yml"}   # lane_up.sh:99

case $CORE_PORT in *[!0-9]*|'') die "--core-port must be a number" ;; esac
case $GATE_PORT in *[!0-9]*|'') die "--gate-port must be a number" ;; esac
case $GATE_BIND in loopback|any) : ;; *) die "--bind must be 'loopback' or 'any' (webfront/Program.cs:343-344)" ;; esac

# substitution values: refuse strings that would corrupt the literal replacement
_check_value() {
  case $2 in
    *$'\n'*) die "value for $1 contains a newline" ;;
    *\|*|*\&*) die "value for $1 contains '|' or '&': refusing (substitution safety)" ;;
  esac
}
_check_value user "$LANE_USER"
_check_value core-dir "$CORE_DIR"
_check_value webfront-dir "$WEBFRONT_DIR"
_check_value webroot "$WEBROOT"
_check_value sec-dir "$SEC_DIR"
_check_value home "$TARGET_HOME"
_check_value env-file "$ENV_FILE"
_check_value tunnel-name "$TUNNEL_NAME"
_check_value node-bin "$NODE_BIN"
_check_value dotnet-bin "$DOTNET_BIN"
_check_value cloudflared-bin "$CLOUDFLARED_BIN"

if [ ! -d "$TARGET_DIR" ]; then say "note: TARGET_DIR does not exist yet ($TARGET_DIR) - staging anyway; it is where the lane will live" ; fi

# ---------------------------- tunnel preflight (the L46 order) ----------------------------
# The lane_up.sh lesson applied to the systemd path: the hosted posture (a Secure cookie and
# WEBFRONT_TRUST_PROXY=cloudflare, which makes the gate believe CF-Connecting-IP came from
# Cloudflare) may only be armed after the cloudflared config is read and names the tunnel the
# unit will run. It runs before anything is staged, so a refused tunnel leaves no half-hosted
# lane.env behind for someone to install.
tunnel_preflight() {
  [ -f "$TUNNEL_CONFIG" ] || die "--tunnel but there is no cloudflared config at $TUNNEL_CONFIG: refusing to stage mfg-tunnel.service or arm WEBFRONT_TRUST_PROXY with no tunnel behind it (staging a plain http lane is the default; add --tunnel only with a real tunnel)"
  grep -Eq "^[[:space:]]*tunnels?[[:space:]]*:[[:space:]]*['\"]?${TUNNEL_NAME}(['\"]|[[:space:]]*$)|^[[:space:]]+${TUNNEL_NAME}[[:space:]]*:" "$TUNNEL_CONFIG" \
    || die "$TUNNEL_CONFIG does not name tunnel \"$TUNNEL_NAME\": refusing to stage the tunnel unit or arm the Secure-cookie posture (point --tunnel-config/--tunnel-name at the real tunnel, or stage without --tunnel)"
  say "tunnel proven: $TUNNEL_CONFIG names $TUNNEL_NAME - staging mfg-tunnel.service and WEBFRONT_TRUST_PROXY=cloudflare"
}

if [ $WITH_TUNNEL = 1 ]; then tunnel_preflight; else say 'tunnel: not staged (no --tunnel) - plain http lane, WEBFRONT_TRUST_PROXY left out of lane.env'; fi

# ---------------------------- staging ----------------------------
mkdir -p -- "$STAGING_DIR"
STAGING_DIR=$(cd "$STAGING_DIR" && pwd)
say "staging into $STAGING_DIR (nothing is written outside it; no sudo is run)"

# litreplace <file> <needle> <replacement> (from assemble_webroot.sh:121-142; ENVIRON
# travel so needles/replacements never need escaping)
litreplace() {
  _lr_needle=$2 _lr_repl=$3 _lr_mode=all awk '
    BEGIN { needle = ENVIRON["_lr_needle"]; repl = ENVIRON["_lr_repl"]; mode = ENVIRON["_lr_mode"] }
    {
      out = ""; s = $0
      for (;;) {
        i = index(s, needle)
        if (i == 0) break
        out = out substr(s, 1, i - 1) repl
        s = substr(s, i + length(needle))
        if (mode == "first") break
      }
      print out s
    }
  ' "$1" > "$4"
}

substitute_unit() {  # substitute_unit <template file> <staged file>
  _src=$1 _dst=$2
  for _tok in USER:LANE_USER CORE_DIR:CORE_DIR WEBFRONT_DIR:WEBFRONT_DIR WEBROOT:WEBROOT \
              SEC_DIR:SEC_DIR HOME:TARGET_HOME ENV_FILE:ENV_FILE TUNNEL_NAME:TUNNEL_NAME \
              NODE_BIN:NODE_BIN DOTNET_BIN:DOTNET_BIN CLOUDFLARED_BIN:CLOUDFLARED_BIN; do
    _ph=@${_tok%%:*}@
    eval _val=\$${_tok#*:}
    [ -f "$_dst" ] || cp -- "$_src" "$_dst"
    _tmp="$_dst.lit.tmp"
    litreplace "$_dst" "$_ph" "$_val" "$_tmp"
    cat "$_tmp" > "$_dst"
    rm -f -- "$_tmp"
  done
  # nothing may survive unsubstituted
  if grep -n '@[A-Z_]*@' "$_dst"; then die "placeholder left in $_dst"; fi
}

units='mfg-lane.service mfg-gate.service'
[ $WITH_TUNNEL = 1 ] && units="$units mfg-tunnel.service"
for _u in $units; do
  [ -f "$SCRIPT_DIR/$_u" ] || die "template unit missing: $SCRIPT_DIR/$_u"
  substitute_unit "$SCRIPT_DIR/$_u" "$STAGING_DIR/$_u"
  say "staged $_u"
done

# lane.env - non-secret shared environment (mirrors lane_up.sh exports, sections 8-9)
lane_env="$STAGING_DIR/lane.env"
{
  printf '%s\n' \
    '# /etc/mfg/lane.env - shared non-secret lane environment.' \
    '# Installed by the human from this staging directory (see the sudo list below).' \
    '# No secret values live here: presenter material stays under '"$SEC_DIR"' at mode 600.' \
    '# core/gate env names come from webfront/Program.cs:25-74 and hosting_up.ps1:46-75.' \
    'WEBFRONT_ROOT='"$WEBROOT" \
    'WEBFRONT_UPSTREAM=http://127.0.0.1:'"$CORE_PORT" \
    'WEBFRONT_AUTH=http://127.0.0.1:'"$CORE_PORT" \
    'WEBFRONT_PORT='"$GATE_PORT" \
    'WEBFRONT_BIND='"$GATE_BIND" \
    'WEBFRONT_GATE=1' \
    'WEBFRONT_PRESENTER_SECRET_FILE='"$SEC_DIR"'/presenter.secret' \
    'WEBFRONT_ACCESS_CODE_FILE='"$SEC_DIR"'/access.code' \
    'mfg_http_bind=127.0.0.1'
  if [ $WITH_TUNNEL = 1 ]; then
    printf '%s\n' \
      'WEBFRONT_TRUST_PROXY=cloudflare' \
      '# export WEB_SECURE_COOKIE=1 here too when the tunnel really terminates https' \
      '# (hosting_up.ps1:52; express-session silently drops a Secure cookie on plain http)'
  fi
  printf '%s\n' \
    '# VFG_PRESENTER_SECRET goes in '"$SEC_DIR"'/core.env (chmod 600, owner '"$LANE_USER"'), not here.' \
    '# core.env line shape: VFG_PRESENTER_SECRET=<random bytes from '"$SEC_DIR"'/presenter.secret>'
} > "$lane_env"
say "staged lane.env (no secret values)"

# ---------------------------- the human's sudo commands ----------------------------
printf '%s\n' \
  '' \
  'install_systemd staged files; it performed no privileged action itself.' \
  'Review the staged files, then run these yourself, on the Linux box:' \
  '' \
  "  sudo mkdir -p '$(dirname "$ENV_FILE")'" \
  "  sudo install -o root -g root -m 0644 '$lane_env' '$ENV_FILE'" \
  "  sudo install -o root -g root -m 0644 '$STAGING_DIR/mfg-lane.service' '/etc/systemd/system/mfg-lane.service'" \
  "  sudo install -o root -g root -m 0644 '$STAGING_DIR/mfg-gate.service' '/etc/systemd/system/mfg-gate.service'"
if [ $WITH_TUNNEL = 1 ]; then
  printf '%s\n' \
    "  sudo install -o root -g root -m 0644 '$STAGING_DIR/mfg-tunnel.service' '/etc/systemd/system/mfg-tunnel.service'"
fi
printf '%s\n' \
  '' \
  '  # the core secret env file (fill it in yourself; never world-readable):' \
  "  mkdir -p '$SEC_DIR'" \
  "  printf 'VFG_PRESENTER_SECRET=%s\\n' \"\$(cat '$SEC_DIR/presenter.secret')\" > '$SEC_DIR/core.env'" \
  "  chmod 600 '$SEC_DIR/core.env'" \
  '' \
  '  sudo systemctl daemon-reload' \
  '  sudo systemctl enable --now mfg-lane.service mfg-gate.service'
if [ $WITH_TUNNEL = 1 ]; then
  printf '%s\n' '  sudo systemctl enable --now mfg-tunnel.service'
fi
printf '%s\n' \
  '  sudo systemctl status mfg-lane.service mfg-gate.service' \
  '' \
  'lane_down semantics under systemd: systemctl stop mfg-gate.service mfg-tunnel.service mfg-lane.service' \
  'lane_up.sh/lane_down.sh (the pid-file path) must NOT run alongside these units - pick one manager.' \
  'Do not run any of this against the Windows box that already hosts the lane.'

say "done (staged only; no sudo executed)"

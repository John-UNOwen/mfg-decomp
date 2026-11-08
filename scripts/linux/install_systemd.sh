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
#   --home DIR        the TARGET USER's home. WEBROOT, SEC_DIR (core.env), @HOME@ in the
#                     tunnel unit and --tunnel-config all derive from it - never from the
#                     home of whoever runs this script. Without --home the home is the
#                     caller's $HOME only when --user IS the caller; otherwise it is read
#                     from getent passwd <user>. A different user this box cannot resolve
#                     without --home is refused, naming both candidate homes.
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
#   --tunnel-config   cloudflared config to check. default: <target user's home>/.cloudflared/config.yml
#                     (lane_up.sh:99; the same directory mfg-tunnel.service's ReadWritePaths=)
#   --no-tunnel       explicit plain http (the default; kept for scripts that pass it)
#
# Values containing a newline, a '|' or a '&' are refused (they would corrupt the
# substitution). The staged lane.env holds no secret values; VFG_PRESENTER_SECRET
# belongs in <sec-dir>/core.env at mode 600 (see mfg-lane.service) - and that file
# only arms the core's presenter while config.ini's [mfg@asphyxia] section carries
# no value of the same key: the plugin resolves the key config.ini BEFORE the
# environment (handlers/utils.ts:1516-1522), so before staging the run reads
# <core-dir>/dist/config.ini and refuses over a non-empty value it has not proven
# equal to the gate's <sec-dir>/presenter.secret.
#
# Re-running into a staging directory that already holds units is the normal development case and is
# honest: every unit is rewritten from its template with THIS run's values, the run says so, and a
# unit this run does not stage (last --tunnel run's mfg-tunnel.service after a plain run) is removed
# from the directory instead of being left there installable and unnamed.
#
# Before anything is staged the run proves on this box every path those units start from - the three
# ExecStart binaries, <core-dir>/dist/AsphyxiaCore.js, <webfront-dir>/bin/Release/net8.0/WebFront.dll,
# <webroot>/index.html and <sec-dir>/presenter.secret (plus <cloudflared-bin> only when --tunnel, and
# <sec-dir>/access.code stays optional and is only noted) - because nothing in this kit ships the core,
# the gate assembly or the webroot, and the sudo list it prints ends in `systemctl enable --now`. A
# host missing any of them is refused with every missing path named and NOTHING staged: an enabled
# unit whose ExecStart is not there does not fail quietly, and a missing presenter.secret or a mistyped
# WEBFRONT_ROOT is quieter still (webfront/Program.cs:57, :341).

set -euo pipefail
export LC_ALL=C

# ---------------------------- configuration ----------------------------
SCRIPT_PATH=${BASH_SOURCE[0]}
case $SCRIPT_PATH in
  */*) SCRIPT_DIR=$(cd "${SCRIPT_PATH%/*}" && pwd) ;;   # invoked with a path: the directory in front of the name
  *)   SCRIPT_DIR=$(pwd) ;;                             # invoked by bare name: bash resolved it against the cwd
esac

STAGING_DIR="$SCRIPT_DIR/systemd-staging"
LANE_USER=$(id -un 2>/dev/null || echo mfg)
TARGET_HOME=''   # the TARGET USER's home, resolved after argument parsing - never silently the caller's
HOME_SET=0
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
    --home)            [ $# -ge 2 ] || die "--home needs a value"; TARGET_HOME=$2; HOME_SET=1; shift 2 ;;
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

# ---------------------------- whose home is the lane's home? ----------------------------
# Every default below (WEBROOT=<home>/mfgtmp/webroot, SEC_DIR=<home>/.mfg-web and its core.env,
# --tunnel-config, the tunnel unit's ReadWritePaths=@HOME@/.cloudflared) names a directory the
# process running as User=@USER@ must be able to read. The line this replaces took TARGET_HOME
# from the caller's $HOME no matter what --user said, so the kit's own README step 6
# (install_systemd.sh /path/to/repo --user <svcuser>) staged the whole layout under the CALLER:
# core.env gets chmod 600 and is unreadable to User=<svcuser> (mfg-lane.service loads it with the
# '-' prefix, so the core starts "successfully" with the presenter un-armed), and the tunnel's
# ReadWritePaths named a ~/.cloudflared that cloudflared - whose HOME comes from the target user's
# passwd entry - never looks at. Derive from the user the units actually run as; when that is not
# the caller, print both homes so nobody is surprised by where the files landed.
_caller_user=$(id -un 2>/dev/null || printf '%s' "$LANE_USER")
if [ $HOME_SET = 1 ]; then
  case $TARGET_HOME in
    /*) : ;;
    *) die "--home must be an absolute path: $TARGET_HOME" ;;
  esac
  _home_from='--home'
elif [ "$LANE_USER" = "$_caller_user" ]; then
  TARGET_HOME=$HOME          # the units run as the caller: this shell's $HOME is that user's home
  _home_from='--user is the caller, so $HOME'
else
  _th=''
  if command -v getent >/dev/null 2>&1; then
    _th=$(getent passwd "$LANE_USER" 2>/dev/null | cut -d: -f6 | head -n 1) || _th=''
  fi
  case $_th in
    /*) TARGET_HOME=$_th; _home_from="getent passwd $LANE_USER" ;;
    *)  die "cannot resolve a home for User=$LANE_USER (getent passwd $LANE_USER gave no home): refusing to pick between the two candidate homes - the caller's home $HOME (WEBROOT, SEC_DIR and a chmod-600 core.env staged there are permanently unreadable to a User=$LANE_USER service) and $LANE_USER's home, which this box does not record. Pass --home DIR (the real home of $LANE_USER), or create the user and re-run, or stage for the caller (drop --user)." ;;
  esac
fi
if [ "$TARGET_HOME" = "$HOME" ]; then
  say "paths resolve under $TARGET_HOME ($_home_from; unit User=$LANE_USER is the caller, so the service can read them)"
else
  say "the units run as User=$LANE_USER: WEBROOT, SEC_DIR (core.env) and the tunnel unit's ReadWritePaths resolve under the target user's home $TARGET_HOME ($_home_from), NOT the caller's home $HOME"
fi

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

if [ ! -d "$TARGET_DIR" ]; then say "note: TARGET_DIR does not exist yet ($TARGET_DIR) - the host checks below still have to find the core and the gate where this run points at them (--core-dir/--webfront-dir), or nothing is staged" ; fi

# ---------------------------- is the lane actually on this box? ----------------------------
# The sudo list printed below ends in `systemctl enable --now`, so every path the staged units start
# from is a claim about this machine. Nothing in this kit ships those paths: the core lives under
# tmp/* and the gate assembly under webfront/bin/, both gitignored; the webroot is what
# assemble_webroot.sh writes; the secret files are the operator's. What this replaces asked for none
# of them - on a half-populated host it substituted an ExecStart pointing at a file systemd cannot
# exec, said 'staged mfg-lane.service', said 'done', and printed the enable command anyway
# (2026-11-08 fixture: RC=0 and 'sudo systemctl enable --now mfg-lane.service mfg-gate.service' with
# no <core>/dist/AsphyxiaCore.js on the box). Two of the misses are quieter than a failed start,
# which is worse: webfront/Program.cs:57 reads an absent WEBFRONT_PRESENTER_SECRET_FILE as an empty
# secret (the gate prints 'listening' and no card ever presents), and Program.cs:341
# Directory.CreateDirectory(Root) creates a mistyped WEBFRONT_ROOT empty instead of failing, so an
# enabled lane answers 404 for every asset (Program.cs:1359-1362 is the same missing index at
# request time). The pid-file path already refuses this shape - lane_up.sh:322-324 dies on the same
# presenter.secret and :364-366 on the same two ExecStart targets - so the systemd path was the only
# one that enabled what it had never seen.
# So: look at every path this run stages a unit for, collect all the misses and refuse once with each
# one named (a reviewer fixing a half-built box sees the whole list, not one per re-run). Nothing is
# written and no sudo list is printed on that path. A binary only the staged-away tunnel unit would
# use is not asked for: --tunnel away, cloudflared is irrelevant to what is being installed.
_host_misses=''
_host_file() {   # _host_file <what stages it> <path> <how the path gets to exist / the flag that points elsewhere>
  if [ ! -f "$2" ]; then
    _host_misses="$_host_misses
    - $1: no such file: $2 ($3)"
  fi
}
_host_bin() {    # systemd execs this path itself, so it must be an executable regular file
  if [ ! -f "$2" ]; then
    _host_misses="$_host_misses
    - $1: no such file: $2 ($3)"
  elif [ ! -x "$2" ]; then
    _host_misses="$_host_misses
    - $1: $2 is not executable, and systemd execs this path literally ($3)"
  fi
}
_host_bin 'mfg-lane.service ExecStart=@NODE_BIN@' "$NODE_BIN" \
  'Ubuntu 24.04 apt node is 18.x and the lane needs >=22.5, so the node that box has is usually not /usr/bin/node - pass --node-bin (lane_up.sh:372-377 asks the same question)'
_host_bin 'mfg-gate.service ExecStart=@DOTNET_BIN@' "$DOTNET_BIN" \
  'the gate runs as "dotnet WebFront.dll" and needs a .NET 8 runtime on PATH-or-here - pass --dotnet-bin'
if [ $WITH_TUNNEL = 1 ]; then
  _host_bin 'mfg-tunnel.service ExecStart=@CLOUDFLARED_BIN@' "$CLOUDFLARED_BIN" \
    'cloudflared must be installed and the tunnel already registered in its account - pass --cloudflared-bin, or stage a plain http lane (no --tunnel)'
fi
_host_file 'mfg-lane.service ExecStart target (WorkingDirectory=@CORE_DIR@/dist)' "$CORE_DIR/dist/AsphyxiaCore.js" \
  'the kit ships no core (tmp/* is gitignored): build it on this box, copy a built one in, or pass --core-dir; without it the unit dies at the exec step right after enable --now'
_host_file 'mfg-gate.service ExecStart target (WorkingDirectory=@WEBFRONT_DIR@/bin/Release/net8.0)' "$WEBFRONT_DIR/bin/Release/net8.0/WebFront.dll" \
  'webfront/bin/ is gitignored: build the net8.0 assembly on this box (assemble_webroot.sh runs the same directory) or pass --webfront-dir'
_host_file 'WEBFRONT_ROOT the gate serves (lane.env)' "$WEBROOT/index.html" \
  'assemble_webroot.sh writes it (README step 2) - check --webroot: a mistyped root is created empty by Program.cs:341 and served as 404s, never reported'
_host_file 'lane.env WEBFRONT_PRESENTER_SECRET_FILE, and the file the core.env recipe below cats' "$SEC_DIR/presenter.secret" \
  "random bytes, mode 600, owned by User=$LANE_USER (hosting_deploy.md); lane_up.sh:322-324 requires the same file for the pid-file path"
if [ -n "$_host_misses" ]; then
  die "this host cannot start the units this run would stage - refusing to stage, nothing was written, no sudo commands printed:${_host_misses}"
fi
say "host proven for every path these units start from: $NODE_BIN + $CORE_DIR/dist/AsphyxiaCore.js (lane), $DOTNET_BIN + $WEBFRONT_DIR/bin/Release/net8.0/WebFront.dll (gate), $WEBROOT/index.html (WEBFRONT_ROOT), $SEC_DIR/presenter.secret (WEBFRONT_PRESENTER_SECRET_FILE)$([ $WITH_TUNNEL = 1 ] && printf ', %s (tunnel)' "$CLOUDFLARED_BIN")"
# What that does NOT prove: that User=$LANE_USER can read them. This run can only ask from the
# caller's side, and on the README step 6 command the secrets live in the service user's home at mode
# 600 where the caller cannot read them by design - so that question is answered by ownership, which
# is exactly why the core.env recipe below creates the file as the unit user, and never by a check here.
say "note: this proves those paths exist here, and are executable where systemd execs them; whether User=$LANE_USER can read them is an ownership question, answered by the core.env recipe below creating that file as the unit user"
if [ ! -f "$SEC_DIR/access.code" ]; then
  say "note: no $SEC_DIR/access.code - lane.env still names it as WEBFRONT_ACCESS_CODE_FILE and webfront/Program.cs:67-74 reads an absent file as no access-code lobby; optional (README 'What the box must have'), so not a reason to refuse this install"
fi

# ---------------------------- who actually arms the core's presenter? ----------------------------
# Everything below stages core.env as the core's presenter source, and the printed core.env recipe
# ends by arming it - but the plugin does not read the environment first. handlers/utils.ts:1623-1624
# presenterSecret() calls configString (handlers/utils.ts:1516-1522), which takes U.GetConfig(key)
# BEFORE process.env[key], and ExternalPluginLoader.ts:170-176 serves U.GetConfig straight out of the
# parsed config.ini [mfg@asphyxia] section (ArgConfig.ts). So on a host whose section already carries
# a value - written once through the WebUI config page, for instance, or left by an earlier deploy -
# the environment core.env provides loses silently: the core answers vfgcard_present with the ini
# value, the gate presents the value read from <sec-dir>/presenter.secret (webfront/Program.cs:57),
# and every card presentation fails the comparison (handlers/utils.ts:1644) while the install run
# reported the secret armed from core.env. Prove which source wins BEFORE staging: read the key from
# the same config.ini the unit's WorkingDirectory starts next to (EXEC_PATH/config.ini, ArgConfig.ts:10,
# and the unit works in <core>/dist), and refuse when it would outrank the staged env file with a
# different secret.
_ini_key_value() {   # print config.ini $1's [mfg@asphyxia] VFG_PRESENTER_SECRET value, '' when absent/empty
  awk '
    /^[[:space:]]*\[/ { insec = ($0 ~ /^[[:space:]]*\[mfg@asphyxia\][[:space:]]*$/); next }
    insec && /^[[:space:]]*VFG_PRESENTER_SECRET[[:space:]]*=/ {
      sub(/^[^=]*=/, ""); sub(/^[[:space:]]+/, ""); sub(/[[:space:]]+$/, ""); v = $0
    }
    END { printf "%s", v }
  ' "$1"
}
PRESENTER_INI="$CORE_DIR/dist/config.ini"
PRESENTER_INI_VALUE=''
if [ ! -e "$PRESENTER_INI" ]; then
  say "core presenter: no $PRESENTER_INI yet (a fresh core tree writes it on first run) - nothing can outrank it, so the staged core.env is what arms the presenter on this box"
  PRESENTER_ENV_NOTE='this run proved nothing can beat core.env: no config.ini at '"$PRESENTER_INI"' yet'
elif [ ! -r "$PRESENTER_INI" ]; then
  die "cannot read $PRESENTER_INI to prove whether its [mfg@asphyxia] VFG_PRESENTER_SECRET would beat the staged core.env (the plugin takes that key config.ini-first, handlers/utils.ts:1516-1522): refusing to stage - make the file readable to this caller (it lives in the core tree the unit user owns) or fix --core-dir"
else
  PRESENTER_INI_VALUE=$(_ini_key_value "$PRESENTER_INI")
  if [ -z "$PRESENTER_INI_VALUE" ]; then
    say "core presenter: config.ini $PRESENTER_INI carries no VFG_PRESENTER_SECRET value under [mfg@asphyxia] (key absent or empty) - the staged core.env is what arms the presenter on this box; set the key there later and it beats core.env (handlers/utils.ts:1516-1522)"
    PRESENTER_ENV_NOTE='this run proved no VFG_PRESENTER_SECRET value in '"$PRESENTER_INI"' [mfg@asphyxia], so core.env is what arms the core'
  else
    if [ ! -r "$SEC_DIR/presenter.secret" ]; then
      die "config.ini $PRESENTER_INI carries a non-empty VFG_PRESENTER_SECRET under [mfg@asphyxia], and this caller cannot read $SEC_DIR/presenter.secret to compare - the plugin reads that ini key BEFORE the environment (handlers/utils.ts:1516-1522), so the staged core.env would be silently ignored and could not even be proven equal to what the gate sends (webfront/Program.cs:57): refusing to stage"
    fi
    if [ "$PRESENTER_INI_VALUE" = "$(printf '%s' "$(cat "$SEC_DIR/presenter.secret")")" ]; then
      say "core presenter: $PRESENTER_INI [mfg@asphyxia] carries VFG_PRESENTER_SECRET and beats the staged core.env (handlers/utils.ts:1623-1624 configString reads config.ini before the environment) - but this run proved it equals $SEC_DIR/presenter.secret (contents not printed), so both halves already agree and the lane works"
      PRESENTER_ENV_NOTE='config.ini '"$PRESENTER_INI"' [mfg@asphyxia] carries the key and beats core.env, proven equal to '"$SEC_DIR"'/presenter.secret by this run - core.env stays as the fallback'
    else
      die "config.ini $PRESENTER_INI carries a non-empty VFG_PRESENTER_SECRET under [mfg@asphyxia], and that key beats the environment the staged core.env would arm (the plugin reads config.ini first: handlers/utils.ts:1623-1624 presenterSecret -> configString :1516-1522 -> U.GetConfig from the parsed ini, ExternalPluginLoader.ts:170-176): the core would answer vfgcard_present with the ini value while the gate presents $SEC_DIR/presenter.secret (webfront/Program.cs:57), so the env-file secret is silently ignored and every presentation fails the comparison (handlers/utils.ts:1644). The two were proved DIFFERENT by this run (values not printed). Refusing to stage. Fix one side and re-run: empty the ini key (the VFG_PRESENTER_SECRET= line in the [mfg@asphyxia] section of $PRESENTER_INI) so core.env arms the core, or make $SEC_DIR/presenter.secret hold exactly the ini value so both sides already agree."
    fi
  fi
fi

# ---------------------------- tunnel preflight (the L46 order) ----------------------------
# The lane_up.sh lesson applied to the systemd path: the hosted posture (a Secure cookie and
# WEBFRONT_TRUST_PROXY=cloudflare, which makes the gate believe CF-Connecting-IP came from
# Cloudflare) may only be armed after the cloudflared config is read and names the tunnel the
# unit will run. It runs before anything is staged, so a refused tunnel leaves no half-hosted
# lane.env behind for someone to install.
tunnel_preflight() {
  [ -e "$TUNNEL_CONFIG" ] || die "--tunnel but there is no cloudflared config at $TUNNEL_CONFIG: refusing to stage mfg-tunnel.service or arm WEBFRONT_TRUST_PROXY with no tunnel behind it (staging a plain http lane is the default; add --tunnel only with a real tunnel)"
  # exists-but-unreadable says something different from missing: the default now derives from
  # User=@USER@'s home (resolved above), so a real config there is routinely unreadable to the
  # caller - say that, and point at --tunnel-config, instead of claiming the file is absent.
  if [ ! -f "$TUNNEL_CONFIG" ] || [ ! -r "$TUNNEL_CONFIG" ]; then
    die "$TUNNEL_CONFIG exists but is not a regular file the caller can read (the default derives from User=$LANE_USER's home, and ~/.cloudflared is routinely mode 700 there): refusing to stage mfg-tunnel.service or arm the Secure-cookie posture on a tunnel this run cannot actually read (point --tunnel-config at a copy the caller can read)"
  fi
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

# Every staged unit is written from the template, every run. What this replaces keyed the copy on the
# destination: '[ -f "$_dst" ] || cp -- "$_src" "$_dst"', and that test sat inside the per-token loop,
# so a re-run into a --staging directory that already held units skipped the copy and ran every
# litreplace against the previous run's output - where each @TOKEN@ had already been replaced, so all
# eleven substitutions were no-ops. The leftover-marker guard below could not catch it either: a
# completed first run leaves no '@...@' anywhere in the file, grep found nothing, and the script said
# 'staged mfg-lane.service' about bytes it had not written this run. The damage is not cosmetic:
# stage lane A, then re-stage lane B into the same directory (the README's dev loop re-runs this
# script after every move), and B's install kept A's WorkingDirectory=, A's EnvironmentFile= and A's
# ReadWritePaths= - under ProtectSystem=strict that core writes core.db, savedata and uploads into
# lane A's tree while looking like its own healthy start. lane.env was rewritten fresh by '>', so
# units and lane.env silently disagreed: --webroot changed lane.env's WEBFRONT_ROOT while the gate
# unit's ReadWritePaths= still named the old root, which is not writable under ProtectSystem=strict.
# So: copy the template to a temporary, substitute every token into that temporary, check the
# temporary, move it onto the destination. A staged unit is this run's bytes or this run failed.
substitute_unit() {  # substitute_unit <template file> <staged file>
  _src=$1 _dst=$2
  _tmp="${_dst}.sub.tmp"
  _rtmp="$_tmp.lit"
  _unit_replaced=0
  [ ! -d "$_dst" ] || die "$_dst is a directory, not a staged unit: refusing to stage into it"
  if [ -e "$_dst" ]; then _unit_replaced=1; fi
  rm -f -- "$_tmp" "$_rtmp"
  cp -- "$_src" "$_tmp" || die "cannot write a working copy of the template $_src: refusing to stage $_dst"
  for _tok in USER:LANE_USER CORE_DIR:CORE_DIR WEBFRONT_DIR:WEBFRONT_DIR WEBROOT:WEBROOT \
              SEC_DIR:SEC_DIR HOME:TARGET_HOME ENV_FILE:ENV_FILE TUNNEL_NAME:TUNNEL_NAME \
              NODE_BIN:NODE_BIN DOTNET_BIN:DOTNET_BIN CLOUDFLARED_BIN:CLOUDFLARED_BIN; do
    _ph=@${_tok%%:*}@
    eval _val=\$${_tok#*:}
    if ! litreplace "$_tmp" "$_ph" "$_val" "$_rtmp"; then
      rm -f -- "$_tmp" "$_rtmp"
      die "substitution of $_ph failed while building $_dst from $_src: $_dst was not changed"
    fi
    cat "$_rtmp" > "$_tmp"
    rm -f -- "$_rtmp"
  done
  # nothing may survive unsubstituted - and this now checks the bytes about to be staged, so a
  # template that gained a placeholder this script does not substitute is a refusal, not a unit that
  # quietly keeps the previous run's content and loses the new field.
  if _left=$(grep -o '@[A-Z_]*@' "$_tmp" | sort -u | tr '\n' ' '); then
    rm -f -- "$_tmp"
    die "template $_src leaves placeholder(s) this script does not substitute: ${_left}in $_dst - refusing to stage it (an unsubstituted token is not a path systemd can start)"
  fi
  if ! mv -f -- "$_tmp" "$_dst"; then
    rm -f -- "$_tmp" "$_rtmp"
    die "cannot replace $_dst with the freshly substituted copy of $_src: nothing was staged for this run"
  fi
  rm -f -- "$_rtmp"
}

units='mfg-lane.service mfg-gate.service'
[ $WITH_TUNNEL = 1 ] && units="$units mfg-tunnel.service"
# A staging directory that outlives one run is still this script's to keep honest, and that includes
# the units from earlier runs. The block this replaced never touched them: run 1 with --tunnel, run 2
# without, and last run's mfg-tunnel.service stayed in the directory next to a lane.env that no longer
# arms WEBFRONT_TRUST_PROXY - an installable hosted unit that no sudo command printed below names.
# So any unit this script can stage but this run does not is removed, and the removal is said.
for _stale in mfg-lane.service mfg-gate.service mfg-tunnel.service; do
  case " $units " in *" $_stale "*) continue ;; esac
  if [ -e "$STAGING_DIR/$_stale" ]; then
    rm -f -- "$STAGING_DIR/$_stale"
    say "removed the stale $_stale an earlier run staged in $STAGING_DIR (this run stages: $units) - an installable unit no sudo command below names is not left for the reviewer to mistake for this run's"
  fi
done
for _u in $units; do
  [ -f "$SCRIPT_DIR/$_u" ] || die "template unit missing: $SCRIPT_DIR/$_u"
  substitute_unit "$SCRIPT_DIR/$_u" "$STAGING_DIR/$_u"
  if [ "$_unit_replaced" = 1 ]; then
    say "staged $_u (re-staged from $SCRIPT_DIR/$_u over the copy already in $STAGING_DIR, every token from this run: User=$LANE_USER CORE_DIR=$CORE_DIR WEBFRONT_DIR=$WEBFRONT_DIR WEBROOT=$WEBROOT SEC_DIR=$SEC_DIR)"
  else
    say "staged $_u"
  fi
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
      '# Behind this tunnel the gate already marks its lobby cookie Secure on its own (Program.cs:788' \
      '# reads the proxy protocol). WEB_SECURE_COOKIE is a different knob: the CORE webui reads it' \
      '# (dist/webui/index.js:101) and it stamps Secure on connect.sid always - put it in core.env,' \
      '# never here (lane.env is shared and the gate ignores it). Note the trap: Secure-always makes' \
      '# http clients refuse to send connect.sid back over plain http, so the loopback smoke breaks' \
      '# the moment it is set - point BASE_URL at the public https URL instead (lane_smoke.sh:56).'
  fi
  printf '%s\n' \
    '# VFG_PRESENTER_SECRET goes in '"$SEC_DIR"'/core.env (chmod 600, owner '"$LANE_USER"'), not here.' \
    '# core.env line shape: VFG_PRESENTER_SECRET=<random bytes from '"$SEC_DIR"'/presenter.secret>' \
    '# config.ini [mfg@asphyxia] VFG_PRESENTER_SECRET beats core.env when it carries a value (handlers/utils.ts:1516-1522).' \
    '# '"$PRESENTER_ENV_NOTE"
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
# The core.env recipe follows the same rule as the paths above: whoever the units run as must
# be able to READ the file. A caller-owned 600 core.env is unreadable to the service forever,
# and mfg-lane.service loads it with the '-' prefix, so the core starts and stays up with no
# presenter secret at all. When the caller IS the unit user the old unprivileged trio is exactly
# right; otherwise every step has to run as that user, hence the sudo -u form below.
if [ "$LANE_USER" = "$_caller_user" ]; then
  printf '%s\n' \
    '' \
    '  # the core secret env file (fill it in yourself; never world-readable):' \
    "  mkdir -p '$SEC_DIR'" \
    "  printf 'VFG_PRESENTER_SECRET=%s\\n' \"\$(cat '$SEC_DIR/presenter.secret')\" > '$SEC_DIR/core.env'" \
    "  chmod 600 '$SEC_DIR/core.env'"
else
  printf '%s\n' \
    '' \
    "  # the core secret env file, created as User=$LANE_USER so the core can actually read it" \
    "  # (a caller-owned 600 core.env is silently skipped by the unit's \"-\"-prefixed EnvironmentFile," \
    '  # and the core then runs un-armed); presenter.secret must already live in that directory:' \
    "  sudo -u '$LANE_USER' mkdir -p '$SEC_DIR'" \
    "  sudo -u '$LANE_USER' sh -c 'test -f \"$SEC_DIR/presenter.secret\" && printf \"VFG_PRESENTER_SECRET=%s\\n\" \"\$(cat \"$SEC_DIR/presenter.secret\")\" > \"$SEC_DIR/core.env\" && chmod 600 \"$SEC_DIR/core.env\"'"
fi
printf '%s\n' "  # (${PRESENTER_ENV_NOTE})"
printf '%s\n' \
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

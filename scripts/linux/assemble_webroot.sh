#!/usr/bin/env bash
# assemble_webroot.sh - bash port of scripts/assemble_webroot.ps1 (147 lines) for a Linux host.
#
# Mirrors a Unity WebGL build tree into the lane webroot, patches it the same way the
# PowerShell script does, stamps the build asset URLs, and (optionally) runs the brotli
# sidecar tool. Step order and stdout messages match the PowerShell original.
#
#   1. validate the build, the sidecar, the serve target and WEBROOT itself   (ps1 :25, extended)
#   2. stop the recorded gate only once the probe proves it serves this root (ps1 :261-315)
#   3. refuse a root a front is serving, stage the build beside it (ps1 :318-380)
#   4. bake config.arguments merge function into index.html    (ps1 :39-60)
#   5. canvas 960x600 -> 1280x720 plus letterbox-fill CSS      (ps1 :61-75)
#   6. framework.js URL-query -> argv rewrite                  (ps1 :76-90)
#   7. hash stamp over the four Build artifacts                (ps1 :91-104)
#   8. build_stamp.txt, catalog XHR stamp, refresh watchdog    (ps1 :105-131)
#   9. brotli sidecars, on the staged copy                     (ps1 :132-135)
#   9b. swap the staged copy into WEBROOT with two renames     (ps1 :485-547)
#  10. optional webfront serve, and the answer that frees the undo handle (ps1 :136-147, :581-609)
#
# Usage
# -----
#   scripts/linux/assemble_webroot.sh [--no-gate-stop] [--no-brotli] [--serve] [-h|--help]
#
#   Everything the PowerShell version took as a parameter is taken from the
#   environment here; the three switches map to the three flags above.
#
#   BUILD_DIR       Build tree to mirror.        default: ~/mfg/out/webgl
#                   (Unity WebGL output: index.html, Build/, StreamingAssets/, TemplateData/)
#   WEBROOT         Assembled webroot.           default: ~/mfgtmp/webroot
#                   Replaced on every run - but not in place. The build is mirrored into
#                   WEBROOT.assembling-XXXX beside the target, patched and sidecar'd there,
#                   and two renames move it into the leaf name (step 9b): the previous root
#                   only leaves the tree once the new one stands at the path, and it is then
#                   parked as WEBROOT.old-<stamp>-<pid> - the undo handle - until an answer
#                   this run observed proves the new root serves (only --serve can observe it).
#                   Refused before any of that when WEBROOT is, or sits above, the operator home, the
#                   repo root, the lane pid dir or the lane log dir (the typo WEBROOT=$HOME/mfgtmp
#                   carries the lane's records away with the root), when it is a path that contains
#                   (or is contained by) BUILD_DIR, or when a live front is answering out of it -
#                   step 3 asks the fronts, because the rm -rf this replaced had no undo and
#                   its old guard only refused '' and /.
#   UPSTREAM        Identity origin URL.         default: http://127.0.0.1:8080
#                   Only used by --serve (WEBFRONT_UPSTREAM).
#   FRONT_URL       URL baked into the player args as the -FRONTURL value.
#                                                  default: http://127.0.0.1:8090/aog
#                   The gate rewrites this value positionally at serve time
#                   (webfront/Program.cs:1384), so any literal is fine as long as
#                   -FRONTURL stays its own array element.
#   BIND            'any' (all interfaces, guest VMs can load the page) or 'loopback'.
#                                                  default: any
#                   Only used by --serve (WEBFRONT_BIND).
#   GATE_PORT       Port --serve puts the gate on (WEBFRONT_PORT).
#                                                  default: 8090 (lane_up.sh:95)
#                   Probed before anything is started: an already answering port is refused
#                   rather than stacked on (the lane_up.sh:356-363 posture). A port the cloudflared
#                   ingress forwards to is refused too - before anything is replaced, and again on
#                   the line that binds it: see TUNNEL_CONFIG.
#   TUNNEL_CONFIG   cloudflared config the ingress ports are read out of (read-only, never written).
#                                                  default: ~/.cloudflared/config.yml (lane_up.sh:103)
#                   Two rules come from it, both learned the hard way on 2026-11-07 (ps1 :88-124,
#                   :278-286) when an assemble run took the hosted front down through a pid record:
#                     * the stop step refuses to signal a live recorded gate that owns one of those
#                       ports. That front is what the public hostname serves; stopping it is an
#                       operator decision, never a side effect of assembling a webroot - whatever
#                       the pid record claims, and whatever the stamp probe says it serves.
#                     * --serve refuses to START a front on one at all. What --serve starts is
#                       WEBFRONT_GATE=0 on $BIND (default any), so an ingress port would put a
#                       relay that checks no access control behind the public hostname - and that
#                       port is normally free only because the hosted gate behind it is down, so
#                       the port probe alone never catches it. The stop-side rule cannot cover a
#                       front this run starts itself, which is why both ends need the list.
#                   Set GATE_PORT to a port this config does not name, or edit the config.
#                   Re-read at each of the three points that ask, not cached: it is an operator
#                   file and an assembly runs for minutes (the mirror + brotli pass).
#   LOG_DIR         Where --serve writes webfront.out.txt / webfront.err.txt.
#                                                  default: ~/mfgtmp/lane_logs
#                   The same directory lane_up.sh:102 uses. The old fixed
#                   ${TMPDIR:-/tmp}/webfront.*.txt put the lane's log - with its request
#                   lines - in a world-writable directory with a predictable name.
#   PID_DIR         Lane pid directory - the one lane_up.sh:88 and lane_down.sh:50 use.
#                                                  default: ~/mfgtmp/lane_pids
#   GATE_PID_FILE   File holding the pid of the gate this lane owns.
#                                                  default: $PID_DIR/gate.pid
#                   ONE recorded gate pid: lane_up.sh:427 writes exactly this file,
#                   lane_down.sh:209 stops it, and --serve here records the gate it
#                   starts into the same file. The old default (~/mfgtmp/gate_pid.txt)
#                   was read by this script and assemble_webroot.ps1:29 and written by
#                   nothing in the repo, so the stop step was a guaranteed no-op for a
#                   lane_up.sh lane.
#                   If the file is absent, nothing is signalled. A pid is signalled only
#                   when its command line names the gate program AND its working
#                   directory is the gate's own bin dir - the cwd rule lane_up.sh:171-182
#                   and lane_down.sh:120-131 use. When a live recorded gate cannot be
#                   stopped, replacing the webroot is refused instead of run under it - and
#                   since the staged-swap pass, a missing record is not a licence to replace
#                   the root either: step 3 asks every front on the box what it answers
#                   /build_stamp.txt with and refuses on that answer, record or none (the
#                   systemd units write no pid file, so the record alone never covered them).
#                   And a record is no licence to STOP anything either: the stop step answers
#                   the same question first, and signals the recorded pid only when that pid
#                   is proved to be the front serving the root this run replaces (ps1 :287-295).
#   GATE_PROCESS    Process name the recorded pid must match before any signal.
#                                                  default: WebFront
#                   Matches comm, argv[0] basename or argv[1] basename (dotnet
#                   WebFront.dll form), case-insensitive, .exe/.dll suffix ignored,
#                   exact match only (WebFrontTools.exe is not WebFront).
#   NO_GATE_STOP    1 = leave every gate process alone (probe assembly), = -NoGateStop.
#                                                  default: 0
#   NO_BROTLI       1 = skip the brotli sidecar pass, = -NoSidecar.  default: 0
#   BROTLI_QUALITY  Passed to the sidecar tool; the tool's own default is 6.  default: 6
#   SIDECAR_DLL     Sidecar tool assembly.
#                   default: <repo>/scripts/brotli-sidecar/bin/Release/net8.0/sidecar.dll
#                   Run as 'dotnet <SIDECAR_DLL> <webroot> <quality>' - the tool is
#                   net8.0 and framework-dependent, so the .dll runs on Linux where
#                   the .exe apphost does not. Needs a dotnet 8 runtime on PATH.
#   WEBFRONT_DLL    Gate/relay assembly, only used by --serve.
#                   default: <repo>/webfront/bin/Release/net8.0/WebFront.dll
#                   (AssemblyName is WebFront, so the assembly is named that on Linux too)
#   SERVE           1 = start webfront after assembling, = -Serve.   default: 0
#
# Portability notes
# -----------------
#   * bash only (set -o pipefail, ${BASH_SOURCE[0]}); the text surgery itself is plain
#     POSIX awk/sed/grep/tr/cut.
#   * GNU/coreutils features used, each noted at its call site: cp -a (POSIX fallback
#     cp -R), sha256sum (POSIX fallback openssl dgst -sha256), head -c (POSIX fallback
#     cut -b 1-N).
#   * No GNU-only regex tricks: every patch is a literal (non-regex) search/replace,
#     matching .NET String.Replace and the fixed strings the PowerShell -replace
#     patterns were matching. awk index()/substr() is used so the needles' " / \ & ( )
#     characters never need escaping.
#   * LC_ALL=C keeps the byte stream byte-exact (the build artifacts are pure ASCII).

set -euo pipefail
export LC_ALL=C

# ---------------------------- configuration ----------------------------
BUILD_DIR=${BUILD_DIR:-"$HOME/mfg/out/webgl"}
WEBROOT=${WEBROOT:-"$HOME/mfgtmp/webroot"}
UPSTREAM=${UPSTREAM:-'http://127.0.0.1:8080'}
FRONT_URL=${FRONT_URL:-'http://127.0.0.1:8090/aog'}
BIND=${BIND:-'any'}
GATE_PORT=${GATE_PORT:-8090}                      # same default as lane_up.sh:95
TUNNEL_CONFIG=${TUNNEL_CONFIG:-"$HOME/.cloudflared/config.yml"}   # same default as lane_up.sh:103
PID_DIR=${PID_DIR:-"$HOME/mfgtmp/lane_pids"}           # same default as lane_up.sh:88
LOG_DIR=${LOG_DIR:-"$HOME/mfgtmp/lane_logs"}           # same default as lane_up.sh:102
GATE_PID_FILE=${GATE_PID_FILE:-"$PID_DIR/gate.pid"}   # the file lane_up.sh:427 writes
GATE_PROCESS=${GATE_PROCESS:-'WebFront'}
NO_GATE_STOP=${NO_GATE_STOP:-0}
NO_BROTLI=${NO_BROTLI:-0}
BROTLI_QUALITY=${BROTLI_QUALITY:-6}
SERVE=${SERVE:-0}

# ps1 :23 - $root = parent of the parent of the script path; the sidecar tool is found
# relative to the repo root, not relative to the caller's cwd. BASH_SOURCE is bash's
# stand-in for $MyInvocation.MyCommand.Path.
SCRIPT_PATH=${BASH_SOURCE[0]}
SCRIPT_DIR=${SCRIPT_PATH%/*}                 # <repo>/scripts/linux
REPO_ROOT=${SCRIPT_DIR%/*}                   # <repo>/scripts
REPO_ROOT=${REPO_ROOT%/*}                    # <repo>
SIDECAR_DLL=${SIDECAR_DLL:-"$REPO_ROOT/scripts/brotli-sidecar/bin/Release/net8.0/sidecar.dll"}
WEBFRONT_DLL=${WEBFRONT_DLL:-"$REPO_ROOT/webfront/bin/Release/net8.0/WebFront.dll"}

usage() {
  sed -n '2,/^set -euo/p' "$SCRIPT_PATH" | sed '$d'
}

die() { printf 'assemble_webroot: %s\n' "$1" >&2; exit 1; }

while [ $# -gt 0 ]; do
  case $1 in
    --no-gate-stop) NO_GATE_STOP=1 ;;
    --no-brotli)    NO_BROTLI=1 ;;
    --serve)        SERVE=1 ;;
    -h|--help)      usage; exit 0 ;;
    *)              die "unknown argument: $1" ;;
  esac
  shift
done

# ---------------------------- helpers ----------------------------

# litreplace <file> <needle> <replacement> <all|first> <out>
# Literal (not regex) search/replace, whole file, line by line. The needle and the
# replacement travel through the environment on purpose: awk -v would interpret
# backslashes in them, ENVIRON[] does not. 'all' mirrors .NET String.Replace (replaces
# every occurrence, which is what the PowerShell version used); 'first' mirrors the
# [regex]::Replace(..., 1) call at ps1 :86. Its exit status is the answer this helper never
# used to give: 0 when the needle occurred, 1 when it occurred nowhere and <out> is <file>
# unchanged. A no-match used to exit 0 exactly like a match - which is what let every
# "patched ..." line below print even on a template whose loader markup had moved and the
# anchor had matched nothing (a page with no config.arguments and no resized canvas, shipped
# as "done").
litreplace() {
  _lr_needle=$2 _lr_repl=$3 _lr_mode=$4 awk '
    BEGIN { needle = ENVIRON["_lr_needle"]; repl = ENVIRON["_lr_repl"]; mode = ENVIRON["_lr_mode"]; _lr_hits = 0 }
    {
      out = ""; s = $0
      for (;;) {
        i = index(s, needle)
        if (i == 0) break
        _lr_hits++
        out = out substr(s, 1, i - 1) repl
        s = substr(s, i + length(needle))
        if (mode == "first") break
      }
      print out s
    }
    END { exit (_lr_hits > 0 ? 0 : 1) }   # 1 = the needle never occurred: <out> is <file> unchanged
  ' "$1" > "$5"
}

# apply_patch <file> <needle> <replacement> <all|first>
# Writes the patched text back in place, preserving the file's trailing-newline state
# (Set-Content -NoNewline wrote the string back exactly as Get-Content -Raw read it).
# It hands litreplace's answer straight back: 0 when the needle matched and the file changed,
# 1 when the needle was absent and the file was left byte-for-byte as it was. Every caller
# that prints a success line must branch on that return - a success line after a 1 is a claim
# about a byte that was never written.
apply_patch() {
  _p_file=$1 _p_needle=$2 _p_repl=$3 _p_mode=$4
  _p_tmp=${_p_file}.lit.tmp
  if ! litreplace "$_p_file" "$_p_needle" "$_p_repl" "$_p_mode" "$_p_tmp"; then
    rm -f -- "$_p_tmp"
    return 1                                # needle absent: file untouched, caller decides
  fi
  if [ -n "$(tail -c 1 "$_p_file")" ]; then
    # input ended without a newline; awk's print added one, so drop it.
    # head -c is a coreutils/BSD extension; POSIX alternative: cut -b 1-N.
    _p_size=$(wc -c < "$_p_tmp")
    head -c "$((_p_size - 1))" "$_p_tmp" > "$_p_file"
  else
    cat "$_p_tmp" > "$_p_file"
  fi
  rm -f -- "$_p_tmp"
  return 0
}

has() { grep -F -q -- "$2" "$1"; }   # has <file> <literal>

# SHA-256 hex, uppercase, over raw bytes. sha256sum is coreutils; openssl is the
# fallback. PowerShell used Get-FileHash -Algorithm SHA256 plus
# BitConverter.ToString (ps1 :97-99), which emits UPPER-CASE hex - the webroot's
# build_stamp.txt ("F5FACEA75F49") shows that casing, so the bash port upper-cases too.
if command -v sha256sum >/dev/null 2>&1; then
  _hash_stdin() { sha256sum | cut -d' ' -f1; }
elif command -v openssl >/dev/null 2>&1; then
  _hash_stdin() { openssl dgst -r -sha256 | cut -d' ' -f1; }
else
  die 'no sha256sum (coreutils) and no openssl on PATH'
fi
hash_upper() { _hash_stdin < "$1" | tr '[:lower:]' '[:upper:]'; }

_norm_name() { printf '%s' "$1" | tr 'A-Z' 'a-z' | sed 's/\.exe$//; s/\.dll$//'; }

# ---------------------------- the tunnel ingress ports (read-only) ----------------------------
# The local ports a cloudflared ingress forwards to - the ports behind the public hostname - read
# out of $TUNNEL_CONFIG. Both ends of the run need the same list and neither may cache it: the stop
# step refuses to signal a gate owning one (ps1 :278-286), and --serve refuses to START a gate-off
# front on one at all (Assert-ServePortNotIngress, ps1 :101-124, asked at :124 before anything is
# replaced and again at :557 on the line that binds). Prints one port per line; prints nothing and
# returns 0 when there is no config - a lane with no tunnel has no ingress ports to disturb.
# rc 1 = the config cannot be read as a file: the question could not be asked, which every caller
# turns into a refusal, never into a pass. A path that exists but is not a readable regular file
# counts as unreadable on purpose: awk only WARNS and exits 0 when handed a directory, so without
# this test a config path that is not a file would answer "no ingress ports" - a vacuous pass, the
# exact shape this kit keeps refusing (an unasked question is never evidence).
_tunnel_ingress_ports() {
  if [ ! -e "$TUNNEL_CONFIG" ]; then return 0; fi
  if [ ! -f "$TUNNEL_CONFIG" ] || [ ! -r "$TUNNEL_CONFIG" ]; then return 1; fi
  # Literal parsing (index/substr), not a regex: the same needle rules as litreplace above, so a
  # value with " / : ( )" needs no escaping. A "service:" key whose value is an http(s) URL is a
  # local listener; the port is what follows the LAST ':' before any path, so a bracketed [::1]
  # host costs nothing, and a service with no port (https://host, ssh:user@host) is skipped - that
  # is not a local port either. The trailing - service: form of a YAML ingress item is a service:
  # line once the leading dash is dropped, so one strip covers both spellings.
  awk '
    {
      l = $0; sub(/\r$/, "", l)
      sub(/^[ \t]+/, "", l); sub(/^-+[ \t]+/, "", l)
      if (substr(l, 1, 8) != "service:") next
      u = substr(l, 9)
      gsub(/["'\'']/, "", u)                       # a quoted YAML value
      hi = index(u, "http://"); hs = index(u, "https://")
      if (hs > 0) { u = substr(u, hs + 8) } else if (hi > 0) { u = substr(u, hi + 7) } else next
      sl = index(u, "/"); if (sl > 0) u = substr(u, 1, sl - 1)
      c = 0
      for (i = length(u); i >= 1; i--) if (substr(u, i, 1) == ":") { c = i; break }
      if (c == 0) next                             # no port: not a local listener to collide with
      host = substr(u, 1, c - 1); port = substr(u, c + 1)
      gsub(/[\[\]]/, "", host)                     # [::1] -> ::1
      if (port !~ /^[0-9]+$/) next
      host = tolower(host)
      if (host != "localhost" && host != "127.0.0.1" && host != "::1" && host != "0.0.0.0") next
      if (!(port in seen)) { seen[port] = 1; print port }
    }' "$TUNNEL_CONFIG"
}

# assert_serve_port_not_ingress <what to tell the operator about what this run has already done>:
# what --serve is about to start is WEBFRONT_GATE=0 on $BIND (default 'any') - an ungated relay. On
# a port the ingress forwards to, the public hostname would answer from that relay with no session
# check at all. The stop guard protects only a front ALREADY sitting there; it says nothing about
# the front this run starts itself, and the port probe says nothing either: the ingress port is
# usually free precisely because the hosted gate behind it is down. So the config is the only
# witness, and the answer is a refusal naming both values compared.
assert_serve_port_not_ingress() {
  if ! _aspi_ports=$(_tunnel_ingress_ports); then
    die "cannot read $TUNNEL_CONFIG to learn which ports are tunnel ingress ports: refusing to start a front whose port cannot be compared against them. $1"
  fi
  [ -n "$_aspi_ports" ] || return 0
  while IFS= read -r _aspi_p; do
    case $_aspi_p in ''|*[!0-9]*) continue ;; esac
    if [ "$_aspi_p" = "$GATE_PORT" ]; then
      die "--serve would start a gate-off front (WEBFRONT_GATE=0, WEBFRONT_BIND=$BIND) on 127.0.0.1:$GATE_PORT, which the cloudflared ingress in $TUNNEL_CONFIG forwards to: the public hostname would be served by an ungated relay. $1 Set GATE_PORT to a port $TUNNEL_CONFIG does not name, or edit that config."
    fi
  done <<<"$_aspi_ports"
}

# ---------------------------- 1. validate before destroying ----------------------------
# ps1 :25 checked one thing (the build directory exists) and this port stopped there, so
# every failure below was discovered after step 3 had already wiped a working webroot: no
# index.html in the build (was: died after the wipe), a missing sidecar or dotnet (was: died
# after the wipe), a WEBROOT that is not a webroot at all (was: wiped, with no undo). The
# checks are all here now, in front of the gate stop and the wipe.
if [ ! -d "$BUILD_DIR" ]; then die "no webgl build at $BUILD_DIR"; fi
if [ ! -f "$BUILD_DIR/index.html" ]; then
  die "no index.html in $BUILD_DIR: refusing to wipe $WEBROOT for a build that cannot be assembled"
fi
_have_artifact=0
for _a in webgl.loader.js webgl.framework.js webgl.wasm webgl.data; do
  [ -f "$BUILD_DIR/Build/$_a" ] && _have_artifact=1
done
if [ $_have_artifact = 0 ]; then
  die "no webgl.loader.js / framework.js / wasm / data under $BUILD_DIR/Build: refusing to wipe $WEBROOT"
fi
if [ "$NO_BROTLI" != 1 ]; then
  if [ ! -f "$SIDECAR_DLL" ]; then
    die "no sidecar tool at $SIDECAR_DLL: refusing to wipe $WEBROOT for an assembly that cannot finish (pass --no-brotli to skip the sidecars)"
  fi
  if ! command -v dotnet >/dev/null 2>&1; then
    die "dotnet runtime not on PATH (needed for $SIDECAR_DLL): refusing to wipe $WEBROOT"
  fi
fi
if [ "$SERVE" = 1 ]; then
  if [ ! -f "$WEBFRONT_DLL" ]; then die "no webfront build at $WEBFRONT_DLL (--serve): refusing to wipe $WEBROOT"; fi
  if ! command -v dotnet >/dev/null 2>&1; then
    die "dotnet runtime not on PATH (needed for $WEBFRONT_DLL, --serve): refusing to wipe $WEBROOT"
  fi
  # The port --serve is about to bind, asked here - before the gate stop, the staging and the swap -
  # because a run that would put an ungated relay behind the public hostname must fail while refusing
  # still costs nothing at all (the -Serve prerequisites block, ps1 :118-135).
  assert_serve_port_not_ingress 'Nothing is replaced: this refusal is in front of the gate stop, the staging and the swap.'
fi

# WEBROOT itself. The old guard was 'case $WEBROOT in ""|"/")', which accepts $HOME, the
# repo root, the pid dir and any directory holding the build. Paths are normalised first so
# a trailing slash or a ../ spelling cannot slip past the comparison.
case $WEBROOT in ''|'/') die "refusing to wipe WEBROOT=$WEBROOT" ;; esac
_abs() { readlink -f "$1" 2>/dev/null || printf '%s' "$1"; }
_webroot_norm=$(_abs "$WEBROOT")
_build_norm=$(_abs "$BUILD_DIR")
_refuse_if_same() {  # _refuse_if_same <what the path is> <path>
  _n=$(_abs "$2")
  if [ -n "$_n" ] && [ "$_webroot_norm" = "$_n" ]; then
    die "refusing to wipe WEBROOT=$WEBROOT: it is the $1 ($_n), not a webroot"
  fi
  return 0
}
_refuse_if_same "operator home" "$HOME"
_refuse_if_same "repo root (this script lives under it)" "$REPO_ROOT"
_refuse_if_same "lane pid dir" "$PID_DIR"
_refuse_if_same "lane log dir" "$LOG_DIR"
# The same list, the other direction: equality is not containment. WEBROOT=$HOME/mfgtmp - the typo
# for $HOME/mfgtmp/webroot - equals none of the four paths above, so it passed every comparison while
# the lane's pid dir and log dir hang one level under it (lane_up.sh:105-106 defaults:
# $HOME/mfgtmp/lane_pids and $HOME/mfgtmp/lane_logs). The swap then takes that whole tree with the
# root: parked as the undo handle, and rm -rf'd on --serve's proof - so the lane loses core.pid and
# tunnel.pid while its core goes on serving, and lane_down.sh:230 then answers 'lane already down
# (no pid files in ...)' over a live game server. Step 2 cannot cover this: it knows only the recorded
# gate, a systemd gate leaves no record at all, and this script never touches the core. The trailing
# slash in the pattern keeps this at directory boundaries: $WEBROOT-lane_pids is a sibling, not inside.
_refuse_if_holds() {  # _refuse_if_holds <what the path is> <path>
  _n=$(_abs "$2")
  if [ -n "$_n" ] && [ "${_n#"$_webroot_norm/"}" != "$_n" ]; then
    die "refusing to wipe WEBROOT=$WEBROOT: the $1 ($_n) is inside it, so the swap would carry it away with the root"
  fi
  return 0
}
_refuse_if_holds "operator home" "$HOME"
_refuse_if_holds "repo root (this script lives under it)" "$REPO_ROOT"
_refuse_if_holds "lane pid dir" "$PID_DIR"
_refuse_if_holds "lane log dir" "$LOG_DIR"
case "$_build_norm" in
  "$_webroot_norm"/*) die "refusing to wipe WEBROOT=$WEBROOT: the build tree ($_build_norm) is inside it, so the wipe would delete the build" ;;
esac
case "$_webroot_norm" in
  "$_build_norm"/*) die "refusing to assemble WEBROOT=$WEBROOT inside the build tree ($_build_norm): the mirror would copy the build into itself" ;;
esac

# ---------------------------- 2. stop the recorded gate ----------------------------
# Say which ports this run will not disturb, so the guard is visible rather than implicit: the value
# the messages below compare against is the value printed here.
if ! _ingress_now=$(_tunnel_ingress_ports); then
  printf 'assemble_webroot: cannot read %s to learn the tunnel ingress ports; the stop step and --serve will refuse anything they cannot compare\n' "$TUNNEL_CONFIG" >&2
elif [ -n "$_ingress_now" ]; then
  printf 'tunnel ingress ports this script will never disturb (from %s): %s\n' "$TUNNEL_CONFIG" "$(printf '%s' "$_ingress_now" | tr '\n' ',' | sed 's/,$//')"
fi
# ps1 :26-36. The PowerShell version read the pid out of gate_pid.txt and stopped that
# one process only after Get-CimInstance confirmed its name was WebFront.exe.
# Linux equivalent: same pid file, same single pid, verified through /proc/<pid> before
# any signal. Never a name kill, never pkill/pgrep, never a pattern, never a port owner:
# if the pid does not match the gate, nothing is signalled. If the file is absent,
# nothing happens at all. Port ownership is consulted only ever the other way - to refuse a signal
# this run would otherwise send (a recorded gate holding a tunnel ingress port is left alone).
pid_alive() { [ -e "/proc/$1" ] || kill -0 "$1" 2>/dev/null; }   # lane_down.sh:112 rule
port_in_use() { (exec 3<>"/dev/tcp/$1/$2") 2>/dev/null; }        # lane_up.sh:134 rule: probe the port, never a name

# ---------------------------- who serves a root: ask, do not assume ----------------------------
# A pid record says who somebody last restarted. It does not say which process serves the root this
# run is about to replace - and that second question has an answer that needs no record at all
# (ps1 :166-177): a front answers /build_stamp.txt out of its own root (webfront/Program.cs
# ServeStatic; measured byte-identical to the file on disk), and step 8 writes that file every run.
# So the probe below asks whatever fronts this box has and compares what they answer with the stamp
# file on disk. That is the guard the 2026-11-07 outages left behind (ledger L72), and it is asked
# twice: before staging, and again on the line that destroys (L91 - the rename is not a net, this
# probe is). The undo-handle rule it hands over to is L90: an unproven swap never deletes what it
# could have rolled back to.

# _probe_front <port> -> two lines: the HTTP status, then the first non-empty body line;
# rc 1 = nothing answered at all. One raw read (-d '' -n 65536): build_stamp.txt is written with no
# trailing newline, so a line-oriented read never sees the last line of the body and a serving front
# would read as "no front". The answer is parsed by awk, not by shell patterns: a $'\r\n\r\n'
# pattern measured here (bash 5.2.26 msys) matches at top level and MISSES inside a command
# substitution, CRs present in the value and all - parsed that way a serving front answers an empty
# body and walks straight past this guard, which is the exact failure the guard exists to stop.
_probe_front() {
  _pf_out=$(
    exec 3<>"/dev/tcp/127.0.0.1/$1" || exit 1
    printf 'GET /build_stamp.txt HTTP/1.0\r\nHost: 127.0.0.1\r\nConnection: close\r\n\r\n' >&3
    IFS= read -r -d '' -n 65536 -t 5 _pf_all <&3 || true
    exec 3>&-
    [ -n "$_pf_all" ] || exit 1
    printf '%s\n' "$_pf_all" | awk '
      NR == 1 {
        line = $0; sub(/\r$/, "", line)
        if (line !~ /^HTTP\//) { print "0"; print line; done = 1; exit }   # a banner, not http
        split(line, f, " "); code = f[2]
        next
      }
      !done && !inbody { l = $0; sub(/\r$/, "", l); if (l == "") inbody = 1; next }
      !done && inbody  { l = $0; sub(/\r$/, "", l); if (l != "") { print code; print l; done = 1; exit } }
      END { if (!done && code != "") { print code; print "" } }'
  ) || return 1
  [ -n "$_pf_out" ] || return 1
  printf '%s\n' "$_pf_out"
}

_disk_stamp() {   # <dir> -> trimmed first line of its build_stamp.txt, empty when it has none
  if [ -f "$1/build_stamp.txt" ]; then
    sed -n '1p' "$1/build_stamp.txt" 2>/dev/null | tr -d '\r\t' | sed 's/^ *//; s/ *$//'
  fi
}

# _listener_rows -> "<port>\t<pid>" for the listening TCP sockets this box can show, the pid empty
# when the owner is not visible to us (ss hides another user's socket). rc 1 = the box cannot be
# asked at all, which is a refusal, never a pass: a question never asked proves nothing.
_listener_rows() {
  if command -v ss >/dev/null 2>&1; then
    ss -lntp 2>/dev/null | awk '
      {
        lp = ""
        for (i = 1; i <= NF; i++) if (lp == "" && $i ~ /:[0-9]+$/) lp = $i
        if (lp == "") next
        c = length(lp)
        while (c > 0 && substr(lp, c, 1) != ":") c--
        if (c == 0) next
        port = substr(lp, c + 1); addr = substr(lp, 1, c - 1)
        okaddr = (addr == "*" || addr == "0.0.0.0" || addr == "[*]" || addr == "::" || addr == "[::]" || addr == "[::1]" || substr(addr, 1, 4) == "127.")
        if (!okaddr) next                                 # not dialable where the probe dials
        pid = ""
        if (match($0, /pid=[0-9]+/)) pid = substr($0, RSTART + 4, RLENGTH - 4)
        print port "\t" pid
      }'
    return 0
  fi
  if [ -r /proc/net/tcp ] || [ -r /proc/net/tcp6 ]; then
    # no ss on this box: the LISTEN rows (state 0A) of procfs. Ports only - procfs names no owner,
    # so every row here is unattributed and only a stamp answer can refuse.
    for _f in /proc/net/tcp /proc/net/tcp6; do
      [ -r "$_f" ] || continue
      awk 'NR > 1 && $4 == "0A" { print $2 }' "$_f" | while IFS= read -r _lr_local; do
        _lr_addr=${_lr_local%:*}; _lr_hex=${_lr_local##*:}
        case $_lr_hex in ''|*[!0-9A-Fa-f]*) continue ;; esac
        _lr_u=${_lr_addr^^}
        # The filter is on the ADDRESS, not the port (testing this branch caught the two mixed up):
        # 00000000 is 0.0.0.0 and 24 trailing zeros is the v6 wildcard, *0100007F is 127.0.0.1
        # including the v4-mapped form, *01000000 catches ::1. A socket bound to some other address
        # is not reachable where the probe dials; a v6 that merely ends that way is probed on
        # loopback, does not answer, and an unattributed row never refuses on a non-answer.
        case $_lr_u in 00000000|*000000000000000000000000|*0100007F|*01000000) : ;; *) continue ;; esac
        printf '%d\t\n' "0x$_lr_hex"
      done
    done
    return 0
  fi
  return 1
}

# _front_of_pid <pid> -> a short identity for the message. rc 0 when the command line names a front
# this kit keeps: the gate program by the same name rule stop_recorded_gate uses, or node running
# serve_aog.js (ps1 :182-197 - that front serves a root like any other, and image names alone
# never decide anything: it is the command line that proves which script an interpreter runs).
_front_of_pid() {
  [ -n "$1" ] || return 1
  [ -r "/proc/$1/cmdline" ] || return 1
  _fp_cmd=$(cat "/proc/$1/cmdline" 2>/dev/null | tr '\0' ' ' | cut -c1-140) || _fp_cmd=''
  _fp_a0=$(cat "/proc/$1/cmdline" 2>/dev/null | tr '\0' '\n' | sed -n '1p') || _fp_a0=''
  _fp_a1=$(cat "/proc/$1/cmdline" 2>/dev/null | tr '\0' '\n' | sed -n '2p') || _fp_a1=''
  _fp_want=$(_norm_name "$GATE_PROCESS")
  if [ "$(_norm_name "${_fp_a0##*/}")" = "$_fp_want" ] || [ "$(_norm_name "${_fp_a1##*/}")" = "$_fp_want" ]; then
    printf 'pid %s (%s)' "$1" "${_fp_cmd:-$GATE_PROCESS}"
    return 0
  fi
  case $_fp_cmd in
    *serve_aog.js*) printf 'pid %s (%s)' "$1" "${_fp_cmd:-node serve_aog.js}"; return 0 ;;
  esac
  printf 'pid %s (%s)' "$1" "${_fp_cmd:-command line unreadable}"
  return 1
}

# assert_root_not_served <root>: refuse when a front is answering out of <root>, and refuse when a
# front holds a port and the question cannot be answered at all. It never signals anything, so it
# also runs for --no-gate-stop probe assembly, and it is read-only by construction.
assert_root_not_served() {
  _an_root=$1
  if [ ! -e "$_an_root" ]; then return 0; fi          # nothing is served out of a root that is not there
  if ! _an_rows=$(_listener_rows); then
    die "cannot list the listening sockets to check who serves $_an_root (no ss and no readable /proc/net/tcp): refusing to replace a webroot that may be served"
  fi
  [ -n "$_an_rows" ] || return 0                      # nothing listening on loopback to ask
  _an_disk=$(_disk_stamp "$_an_root")
  while IFS=$'\t' read -r _an_port _an_pid; do
    case $_an_port in ''|*[!0-9]*) continue ;; esac
    _an_who=''
    _an_front=0
    if [ -n "$_an_pid" ] && _an_who=$(_front_of_pid "$_an_pid"); then _an_front=1; fi
    _an_who=${_an_who:-pid unknown (owner not visible to us)}
    _an_asked=0
    _an_st=''; _an_body=''
    if _an_pf=$(_probe_front "$_an_port"); then
      _an_asked=1
      { read -r _an_st; read -r _an_body; } <<<"$_an_pf" || true
    fi
    if [ $_an_asked = 1 ] && [ "$_an_st" = 200 ] && [ -n "$_an_disk" ] && [ "$_an_body" = "$_an_disk" ]; then
      die "$_an_who is serving $_an_root on 127.0.0.1:$_an_port: it answers /build_stamp.txt with $_an_body, the stamp in $_an_root/build_stamp.txt. Refusing to replace a root a front is serving - stop that front first (scripts/linux/lane_down.sh, or systemctl stop mfg-gate.service) or point WEBROOT at another root."
    elif [ $_an_asked = 1 ] && [ "$_an_st" = 200 ] && [ -n "$_an_disk" ]; then
      printf 'listener %s on 127.0.0.1:%s answers /build_stamp.txt with %s, not the stamp of this webroot (%s): a different build, not this root\n' "$_an_who" "$_an_port" "$_an_body" "$_an_disk"
    elif [ $_an_asked = 1 ] && [ -n "$_an_disk" ] && [ "$_an_st" != 0 ]; then
      printf 'listener %s on 127.0.0.1:%s answers /build_stamp.txt with HTTP %s, not the stamp of this webroot (%s): not this root\n' "$_an_who" "$_an_port" "$_an_st" "$_an_disk"
    elif [ $_an_asked = 1 ] && [ -z "$_an_disk" ] && [ $_an_front = 1 ]; then
      die "cannot prove $_an_who is not serving $_an_root: it answered HTTP $_an_st and $_an_root has no build_stamp.txt to compare against. Assemble into a fresh root, or stop that front, first."
    elif [ $_an_front = 1 ]; then
      die "cannot prove $_an_who is not serving $_an_root: it holds 127.0.0.1:$_an_port but did not answer /build_stamp.txt. Stop it first or choose another WEBROOT."
    fi
  done <<<"$_an_rows"
}

# _find_serving_front <root> -> "pid<TAB>port" of the front answering <root>'s build_stamp.txt with its
# own stamp, or nothing with rc 1 (no front serves it: the swap can go on); rc 2 = the box cannot be
# asked, which the caller must treat as a refusal. This is the same question assert_root_not_served
# asks, split out so the stop step can ask it the OTHER way - not "may I replace this root" but "is the
# recorded gate the one serving it" - and answer with a signal or with leaving it alone. A pid record
# says who was last restarted; only the stamp answer says what a live process actually serves
# (ps1 :287-295, ledger L72).
_find_serving_front() {
  _fs_root=$1
  [ -e "$_fs_root" ] || return 1
  if ! _fs_rows=$(_listener_rows); then
    printf 'cannot list the listening sockets to ask who serves %s (no ss and no readable /proc/net/tcp)\n' "$_fs_root" >&2
    return 2
  fi
  [ -n "$_fs_rows" ] || return 1                       # nothing listening on loopback to ask
  _fs_disk=$(_disk_stamp "$_fs_root")
  while IFS=$'\t' read -r _fs_port _fs_pid; do
    case $_fs_port in ''|*[!0-9]*) continue ;; esac
    _fs_asked=0; _fs_st=''; _fs_body=''
    if _fs_pf=$(_probe_front "$_fs_port"); then
      _fs_asked=1
      { read -r _fs_st; read -r _fs_body; } <<<"$_fs_pf" || true
    fi
    if [ $_fs_asked = 1 ] && [ "$_fs_st" = 200 ] && [ -n "$_fs_disk" ] && [ "$_fs_body" = "$_fs_disk" ]; then
      printf '%s\t%s\n' "$_fs_pid" "$_fs_port"
      return 0
    fi
  done <<<"$_fs_rows"
  return 1
}

stop_recorded_gate() {
  _sr_target=$1                              # the root this run is about to replace (step 9b)
  if [ "$NO_GATE_STOP" = 1 ]; then
    printf 'gate stop skipped (NO_GATE_STOP=1)\n'
    return 0
  fi
  if [ ! -f "$GATE_PID_FILE" ]; then
    return 0                                   # no pid file: do nothing (ps1 :30)
  fi
  # Get-Content -Raw + .Trim() (ps1 :32): first line, leading/trailing blanks dropped.
  # Anything else (inner spaces, text) fails the digit test below and nothing is killed.
  pid=$(sed -n '1p' "$GATE_PID_FILE" 2>/dev/null | tr -d '\r\t' | sed 's/^ *//; s/ *$//') || pid=''
  case $pid in
    ''|*[!0-9]*) printf 'gate pid file %s has no usable pid; nothing signalled\n' "$GATE_PID_FILE"; return 0 ;;
  esac                                       # [int]::TryParse failure (ps1 :32)
  if [ "$pid" -le 1 ]; then
    printf 'gate pid file holds pid %s; refusing to signal it\n' "$pid"; return 0
  fi
  if [ "$pid" = "$$" ] || [ "$pid" = "$PPID" ]; then
    printf 'gate pid file holds our own pid %s; refusing to signal it\n' "$pid"; return 0
  fi
  if ! pid_alive "$pid"; then
    printf 'gate pid %s is not running; stale pid file %s removed\n' "$pid" "$GATE_PID_FILE"
    rm -f -- "$GATE_PID_FILE"
    return 0
  fi
  if [ ! -r "/proc/$pid/cmdline" ]; then
    printf 'gate pid %s is alive but its command line cannot be read; nothing signalled\n' "$pid"
    return 1                                 # no identity proof: the wipe below must not run under it
  fi
  argv0=$(tr '\0' '\n' < "/proc/$pid/cmdline" | sed -n '1p') || argv0=''
  argv1=$(tr '\0' '\n' < "/proc/$pid/cmdline" | sed -n '2p') || argv1=''
  comm=$(sed -n '1p' "/proc/$pid/comm" 2>/dev/null) || comm=''
  want=$(_norm_name "$GATE_PROCESS")
  got_comm=$(_norm_name "$comm")
  got_exe=$(_norm_name "${argv0##*/}")
  got_arg=$(_norm_name "${argv1##*/}")       # dotnet WebFront.dll form
  if [ "$got_comm" != "$want" ] && [ "$got_exe" != "$want" ] && [ "$got_arg" != "$want" ]; then
    printf 'gate pid %s is "%s", not "%s"; not signalled\n' "$pid" "${got_exe:-$got_comm}" "$GATE_PROCESS"
    return 1                                 # never kill a pid that does not match
  fi
  # Identity rule two (lane_up.sh:171-182, lane_down.sh:120-131): the gate this lane owns
  # runs with its cwd at the bin dir of the assembly named by WEBFRONT_DLL. A recycled pid
  # that merely carries a gate-looking name is somebody else's process.
  gate_bin_dir=$(readlink -f "${WEBFRONT_DLL%/*}" 2>/dev/null) || gate_bin_dir=''
  pid_cwd=$(readlink -f "/proc/$pid/cwd" 2>/dev/null) || pid_cwd=''
  if [ -n "$gate_bin_dir" ] && [ "$pid_cwd" != "$gate_bin_dir" ]; then
    printf 'gate pid %s names the gate program but its working directory is %s, not %s: not signalled (not the gate this lane started)\n' "$pid" "${pid_cwd:-<unreadable>}" "$gate_bin_dir"
    return 1
  fi
  # A front owning a tunnel ingress port is what the public hostname serves. This script never stops
  # one, whatever the pid record claims and whatever it turns out to serve (ps1 :278-286): on
  # 2026-11-07 an assemble run on a preview lane stopped the hosted gate through exactly such a
  # record and took the public site down. Asked BEFORE the serving probe, because owning that port is
  # a refusal on its own - the probe answers what a front serves, not what the hostname in front of
  # it is pointed at. Read again here, not taken from the step-1 reading: the config can change.
  if ! _sg_ingress=$(_tunnel_ingress_ports); then
    printf 'cannot read %s to learn which ports are tunnel ingress ports: refusing to signal the recorded gate pid %s, which may own one of them\n' "$TUNNEL_CONFIG" "$pid"
    return 1
  fi
  if [ -n "$_sg_ingress" ]; then
    if ! _sg_rows=$(_listener_rows); then
      printf 'cannot list the listening sockets to ask whether the recorded gate pid %s owns one of the tunnel ingress ports in %s: refusing to signal it\n' "$pid" "$TUNNEL_CONFIG"
      return 1
    fi
    while IFS=$'\t' read -r _sg_port _sg_lpid; do
      case $_sg_port in ''|*[!0-9]*) continue ;; esac
      printf '%s\n' "$_sg_ingress" | grep -Fxq -- "$_sg_port" || continue
      if [ -z "$_sg_lpid" ]; then
        # The socket is there and its owner is not: an ss that hides another user's socket cannot
        # prove this live recorded gate is NOT the front behind the hostname.
        printf 'the cloudflared ingress in %s forwards to 127.0.0.1:%s and the owner of that listening socket is not visible to us, so this run cannot prove the recorded gate pid %s is not the front behind the public hostname: nothing signalled, record %s kept\n' "$TUNNEL_CONFIG" "$_sg_port" "$pid" "$GATE_PID_FILE"
        return 1
      fi
      if [ "$_sg_lpid" = "$pid" ]; then
        printf 'the recorded gate pid %s owns 127.0.0.1:%s, which the cloudflared ingress in %s forwards to: that is the hosted front behind the public hostname. Stop the hosted lane first (scripts/linux/lane_down.sh, or systemctl stop mfg-gate.service) if this root really is meant to be replaced\n' "$pid" "$_sg_port" "$TUNNEL_CONFIG"
        return 1
      fi
    done <<<"$_sg_rows"
  fi
  # The probe, not the record, decides who is safe to stop (ps1 :287-295). Name and cwd prove what
  # this pid is; they do not prove what it serves. On 2026-11-07 an assemble of a staging root
  # stopped the live hosted gate through the shared $PID_DIR/gate.pid - that gate served a
  # different root - and took the public hostname down. So ask, before any signal, the same
  # question step 3 asks: is a front answering /build_stamp.txt with the stamp of the root this
  # run replaces, and is it this pid? A live gate that is not it is left running with its record:
  # the root this run replaces is not the root it answers from.
  _sr_rc=0; _sr_srv=$(_find_serving_front "$_sr_target") || _sr_rc=$?
  if [ $_sr_rc = 2 ]; then
    printf 'cannot list the listening sockets to ask whether the recorded gate pid %s serves %s: refusing to replace a webroot that may be served\n' "$pid" "$_sr_target"
    return 1
  fi
  if [ $_sr_rc != 0 ]; then
    if [ ! -e "$_sr_target" ]; then
      printf 'the recorded gate pid %s is live, but %s is not there, so no front can be serving it: gate left running, record %s kept\n' "$pid" "$_sr_target" "$GATE_PID_FILE"
    elif [ -z "$(_disk_stamp "$_sr_target")" ]; then
      printf 'the recorded gate pid %s is live, but %s carries no build_stamp.txt to match a front answer against: gate left running, record %s kept (step 3 still asks the root itself)\n' "$pid" "$_sr_target" "$GATE_PID_FILE"
    else
      printf 'the recorded gate pid %s is live but does not serve %s: no front on this box answers /build_stamp.txt with %s (the stamp in %s/build_stamp.txt): gate left running, record %s kept\n' "$pid" "$_sr_target" "$(_disk_stamp "$_sr_target")" "$_sr_target" "$GATE_PID_FILE"
    fi
    return 0
  fi
  _sr_pid=${_sr_srv%%$'\t'*}
  _sr_port=${_sr_srv#*$'\t'}
  if [ "$_sr_pid" != "$pid" ]; then
    printf 'pid %s is the recorded gate, but the front serving %s on 127.0.0.1:%s is pid %s: this script will not stop a process that is not the one serving the root. Stop that front first (scripts/linux/lane_down.sh, or systemctl stop mfg-gate.service), or point WEBROOT at another root\n' "$pid" "$_sr_target" "$_sr_port" "${_sr_pid:-an owner not visible to us}"
    return 1
  fi
  # Identity proved twice over - recorded (name+cwd) and proved by its own stamp answer to serve the
  # root this run replaces: this is the one pid this step may signal.
  kill -TERM "$pid" 2>/dev/null || true      # let the gate close its listener before Force
  _st=1
  while [ $_st -le 10 ]; do pid_alive "$pid" || break; sleep 0.5; _st=$((_st+1)); done
  if pid_alive "$pid"; then
    kill -KILL "$pid" 2>/dev/null || true    # Stop-Process -Id <pid> -Force (ps1 :34)
    _st=1
    while [ $_st -le 10 ]; do pid_alive "$pid" || break; sleep 0.5; _st=$((_st+1)); done
  fi
  if pid_alive "$pid"; then
    printf 'gate pid %s survived SIGTERM and SIGKILL; refusing to wipe %s under a serving gate\n' "$pid" "$WEBROOT"
    return 1
  fi
  printf 'stopped gate pid=%s (%s), the front serving %s (stamp %s); pid file %s cleared\n' "$pid" "${got_exe:-$got_comm}" "$_sr_target" "$(_disk_stamp "$_sr_target")" "$GATE_PID_FILE"
  rm -f -- "$GATE_PID_FILE"
  return 0
}
if ! stop_recorded_gate "$_webroot_norm"; then
  die "the recorded gate ($GATE_PID_FILE) is live and was not signalled: refusing to wipe $WEBROOT under a serving gate. Stop that gate first (scripts/linux/lane_down.sh, or systemctl stop mfg-gate.service) and run again, or pass --no-gate-stop for a probe assembly."
fi

# ---------------------------- 3. refuse a served root, then stage beside it ----------------------------
# What this used to be (ps1 :37-38): Remove-Item $Webroot -Recurse -Force, then copy the build in.
# Two faults, both of them the 2026-11-07 story (ledger L72):
#   * the only thing standing between the rm -rf and a live front was the step-2 pid record, and a
#     record is not a fact. The kit's documented permanent form - README step 6, the systemd units,
#     and mfg-gate.service names no PIDFile= - leaves no pid file at all, so stop_recorded_gate
#     returned 0 having done nothing and the wipe tore the tree out from under a serving gate and
#     left it as the work-in-progress tree for the whole mirror + brotli pass (9 GB / 58,809 files
#     by the README's own transfer numbers), with the served root answering from it per request.
#   * every check and patch after the wipe could fail, and left the lane with a half-deleted root.
# So the front is asked, not assumed (assert_root_not_served), the build is mirrored into
# WEBROOT.assembling-XXXX beside the target, and the target is destroyed only in step 9b, by two
# renames, after the probe is asked again: the previous root is parked as WEBROOT.old-<stamp>-<pid>
# and that aside is the undo handle - removed only on an answer this run observed (step 10, L90).
assert_root_not_served "$WEBROOT"
TARGET_ROOT=$_webroot_norm         # the path this run replaces; WEBROOT below becomes the staged copy
_staged=''                         # the staged copy; the exit trap removes it unless it was published
_keep_staged=0                     # 1 = a failure kept it for inspection instead of removing it
_aside=''                          # the previous root, parked aside; only step 10's proof removes it
_on_exit() {
  _oe_rc=$?
  if [ -n "$_staged" ] && [ -e "$_staged" ]; then
    if [ "$_keep_staged" = 1 ]; then
      printf 'assemble_webroot: assembly failed; the staged copy was kept for inspection at %s\n' "$_staged" >&2
    else
      rm -rf -- "$_staged" 2>/dev/null || true
      printf 'assemble_webroot: assembly failed (exit %s); the staged copy was removed and %s was left untouched\n' "$_oe_rc" "$TARGET_ROOT" >&2
    fi
  fi
}
trap _on_exit EXIT
if [ "${TARGET_ROOT%/*}" != "$TARGET_ROOT" ]; then mkdir -p -- "${TARGET_ROOT%/*}" 2>/dev/null || true; fi
if ! _staged=$(mktemp -d "${TARGET_ROOT}.assembling-XXXXXXXX" 2>/dev/null); then
  die "cannot create a staging directory beside $TARGET_ROOT: refusing to replace the webroot in place"
fi
# cp -a is coreutils (mode+timestamps preserved, symlinks copied as links); POSIX
# fallback is cp -R. Copy-Item copied attributes too and did not follow symlinks.
cp -a "$BUILD_DIR"/. "$_staged"/
WEBROOT=$_staged                   # steps 4-9 patch the staged copy; step 9b swaps it into TARGET_ROOT

# ---------------------------- 4. config.arguments merge ----------------------------
# ps1 :43-60. Skip when the page already carries an "arguments:" key.
# Anchors that must survive for the gate to splice the page (webfront/Program.cs:1369-1384):
#   * the literal token  "-AUTOSERVE"  must be present (fail-closed check) and the
#     splice replaces ", \"-AUTOSERVE\"" or "\"-AUTOSERVE\", " with the per-account
#     cluster, so it must stay a standalone array element.
#   * no literal  "-DATAID="  may appear in the template (identity must not be pinned).
#   * -FRONTURL is rewritten positionally by regex  -FRONTURL",\s*"[^"]*"  so it must
#     stay its own element followed by ", " and its own quoted value.
idx="$WEBROOT/index.html"                    # ps1 :43 (non-recursive -Filter index.html)
if [ ! -f "$idx" ]; then die "no index.html in $WEBROOT"; fi

MERGE_JS='(function(){var base=["-KONAMINET", "-FRONTURL", "__FRONTURL__", "-GAMELOG", "-NOIO", "-appBehavior", "NETCAFE", "-AUTOSERVE", "-TRANSLATE"];try{var q=location.search.substring(1).split("&");for(var i=0;i<q.length;i++){if(!q[i])continue;var kv=q[i].split("=");var k=decodeURIComponent(kv[0].replace(/\+/g," "));if(kv.length>1){base.push(k);base.push(decodeURIComponent(kv[1].replace(/\+/g," ")))}else{base.push(k)}}}catch(e){}return base})()'
MERGE_JS=${MERGE_JS//__FRONTURL__/$FRONT_URL}   # ps1 :50 (FrontUrl spliced in)

if ! has "$idx" 'arguments:'; then
  _cfg_ok=0
  if has "$idx" 'var config = {'; then
    # ps1 :51-53: 'var config = { arguments: <merge>,' replaces 'var config = {'
    if apply_patch "$idx" 'var config = {' "var config = { arguments: $MERGE_JS," all; then _cfg_ok=1; fi
  else
    # ps1 :55-56: loaders with no config literal get the args object as 2nd parameter
    if apply_patch "$idx" 'createUnityInstance(canvas, ' "createUnityInstance(canvas, { arguments: $MERGE_JS }, " all; then _cfg_ok=1; fi
  fi
  if [ "$_cfg_ok" = 1 ]; then
    printf 'patched index.html config.arguments\n'
  else
    # Neither anchor spliced: no "arguments:" block was written, and the gate splices the play
    # page only through config.arguments (it answers 500 "play page template not splicable"
    # without it, webfront/Program.cs:1359-1385). Publishing this root would ship a lane whose
    # every play page 500s while stdout says all done, so the assembly fails here - on the
    # staged copy, before anything is swapped in.
    die "the config.arguments patch found no anchor to splice into: neither 'var config = {' nor 'createUnityInstance(canvas, ' occurs in $idx, so no arguments: block was written; this build's loader markup differs from the 2022.3 Default template and the webroot was NOT published."
  fi
else
  printf 'index.html already has arguments\n'
fi

# ---------------------------- 5. canvas + letterbox CSS ----------------------------
# ps1 :65-75. Unity's Default template pins 960x600 (1.6) while the CanvasScaler
# reference resolution is 1920x1080; the CSS makes the canvas fill the window as a
# 16:9 letterbox. ps1 :67 used -replace with a pattern that has no metacharacters, so
# the literal needle below matches the same text.
_canvas_ok=0
if apply_patch "$idx" '<canvas id="unity-canvas" width=960 height=600 ' '<canvas id="unity-canvas" width=1280 height=720 ' all; then
  _canvas_ok=1
elif has "$idx" '<canvas id="unity-canvas" width=1280 height=720 '; then
  _canvas_ok=1                                 # already the resized canvas (an already-assembled build root)
fi

CANVAS_CSS='<style>/*MFG-CANVAS*/html,body{margin:0;padding:0;overflow:hidden;background:#000}#unity-container{position:fixed!important;left:0!important;top:0!important;width:100vw!important;height:100vh!important;transform:none!important}#unity-canvas{position:fixed!important;left:50%!important;top:50%!important;transform:translate(-50%,-50%)!important;width:min(100vw,177.78vh)!important;height:min(56.25vw,100vh)!important;display:block!important}#unity-loading-bar,#unity-logo,#unity-fullscreen-button,#unity-progress-bar-full,#unity-webgl-logo,#unity-build-title{display:none!important}</style>'
_css_ok=0
if has "$idx" 'MFG-CANVAS'; then                # ps1 :73 idempotence marker: already there, observed
  _css_ok=1
elif has "$idx" '</head>'; then
  if apply_patch "$idx" '</head>' "${CANVAS_CSS}</head>" all; then _css_ok=1; fi
fi
if [ "$_canvas_ok" = 1 ] && [ "$_css_ok" = 1 ]; then
  printf 'patched index.html canvas->letterbox-fill\n'
else
  # The 960x600 unity-canvas is what this step resizes and the /*MFG-CANVAS*/ block is what splices
  # the 16:9 letterbox CSS in; without them the end-of-match / continue button falls off-screen (the
  # reason the patch exists, ps1 :65-75). Claiming the letterbox "applied" on a template whose canvas
  # or </head> markup had moved would hide exactly that, so the assembly fails here instead.
  die "the canvas->letterbox patch did not take: the 960x600 unity-canvas tag was not present to resize (canvas resized to 1280x720: $_canvas_ok) and the /*MFG-CANVAS*/ letterbox CSS was not spliced into '</head>' (CSS spliced: $_css_ok), checked in $idx; a template whose canvas markup differs from the 2022.3 Default page clips the end-of-match UI, and the webroot was NOT published."
fi

# ---------------------------- 6. framework.js url -> argv ----------------------------
# ps1 :80-90. Single-point edit: replace only the FIRST 'var arguments_=[]' (the loader
# factory's frozen argv) with a query parser, tagged with the /*MFG-URLARGV*/ marker so
# the patch is idempotent and survives a Unity rebuild - a fresh framework.js has no
# marker, so the same single needle is found again on the next assemble. Everything else
# in the 516KB framework.js is left untouched, byte for byte.
FW="$WEBROOT/Build/webgl.framework.js"       # ps1 :80
if [ -f "$FW" ]; then
  if ! has "$FW" 'MFG-URLARGV'; then
    FW_INJECT='var arguments_=/*MFG-URLARGV*/(function(){var out=[];try{var s=location.search.substring(1).split("&");for(var i=0;i<s.length;i++){if(!s[i])continue;var kv=s[i].split("=");var k=decodeURIComponent(kv[0].replace(/\+/g," "));if(kv.length>1){out.push(k);out.push(decodeURIComponent(kv[1].replace(/\+/g," ")))}else{out.push(k)}}if(typeof Module!=="undefined"&&Module["arguments"]){out=Module["arguments"].concat(out)}}catch(e){}return out})()'
    if apply_patch "$FW" 'var arguments_=[]' "$FW_INJECT" first; then
      printf 'patched framework.js url->argv\n'
    else
      # The loader factory freezes `var arguments_=[]` at script load, before config.arguments can
      # reach it, so this is the only path by which URL-query arguments reach Environment.GetCommandLineArgs
      # in the wasm. If that needle moved, the argv rewrite never ran and the client silently starts with no
      # arguments - so fail here instead of claiming a patch that was never made.
      die "the framework.js url->argv patch found no anchor: 'var arguments_=[]' does not occur in $FW, so the loader's frozen argv was never rewritten and the client receives no URL-query arguments; this build's framework.js differs from the 2022.3 Default loader and the webroot was NOT published."
    fi
  else
    printf 'framework.js already url->argv patched\n'
  fi
fi

# ---------------------------- 7. build stamp ----------------------------
# ps1 :96-99. Same input set, same order: the four Build artifacts that exist in the
# webroot (framework.js here is the ALREADY-patched copy, exactly as in the PowerShell
# run, because step 6 ran first). Same algorithm:
#   sha256(file bytes) -> UPPER-CASE hex, concatenated with no separator
#   -> sha256 of those ASCII bytes -> UPPER-CASE hex -> first 12 characters.
# An array, not a space-joined string: a WEBROOT containing a space split into pieces that
# matched no file, so every hash was skipped, the stamp became the hash of the empty string
# (E3B0C44298FC...) and every build - including a different one - stamped identically.
stamp_files=("$WEBROOT/Build/webgl.loader.js" "$WEBROOT/Build/webgl.framework.js" \
             "$WEBROOT/Build/webgl.wasm" "$WEBROOT/Build/webgl.data")
joined=''
for f in "${stamp_files[@]}"; do
  if [ -f "$f" ]; then joined="$joined$(hash_upper "$f")"; fi   # ps1 :96 Where-Object Test-Path
done
STAMP=$(printf '%s' "$joined" | _hash_stdin | tr '[:lower:]' '[:upper:]' | cut -c1-12)

# ps1 :101-104 literal '"/<asset>"' -> '"/<asset>?v=<stamp>"' (the gate ignores queries). Count how
# many of the four the patch actually rewrote, so the line below reports what was observed rather than
# asserting all four on a template that spells the asset urls another way (or a previously assembled
# root, where they are already "...?v=OLD" and the fresh needle matches nothing).
_stamp_n=0
for asset in webgl.loader.js webgl.framework.js webgl.data webgl.wasm; do
  if apply_patch "$idx" "\"/$asset\"" "\"/$asset?v=$STAMP\"" all; then _stamp_n=$((_stamp_n+1)); fi
done

# ps1 :114 build_stamp.txt, UTF-8 no BOM, no trailing newline (Set-Content -NoNewline)
printf '%s' "$STAMP" > "$WEBROOT/build_stamp.txt"

# ---------------------------- 8. catalog stamp + refresh watchdog ----------------------------
# ps1 :105-131. Two <script> blocks injected before </head> so they run before the
# loader: (a) an XMLHttpRequest.open hook that appends ?v=<stamp> to every
# .../aa/catalog.json fetch (UnityWebRequest rides XHR; per-build cache key), and (b) a
# watchdog that polls /build_stamp.txt?n=<random> every 2 minutes (first hit after 20s)
# and shows a Reload prompt when the stamp changed.
WATCHDOG=$(cat <<'BLOCK'
<script>
(function(){var V="__STAMP__";var o=XMLHttpRequest.prototype.open;XMLHttpRequest.prototype.open=function(m,u){try{if(typeof u==="string"&&/\/aa\/catalog\.json(\?|$)/.test(u)){u+=(u.indexOf("?")>=0?"&":"?")+"v="+V;}}catch(e){}return o.apply(this,arguments);};})();
</script>
<script>
(function(){var STAMP="__STAMP__";function chk(){fetch("/build_stamp.txt?n="+Math.random(),{cache:"no-store"}).then(function(r){return r.ok?r.text():"";}).then(function(t){var s=(t||"").trim();if(s&&s!==STAMP)show();}).catch(function(){});}function show(){if(document.getElementById("mfg-update"))return;var d=document.createElement("div");d.id="mfg-update";d.style.cssText="position:fixed;left:50%;bottom:18px;transform:translateX(-50%);z-index:99999;background:#181820;color:#fff;padding:10px 14px;border-radius:8px;font:14px system-ui,sans-serif;box-shadow:0 4px 14px rgba(0,0,0,.45);display:flex;gap:10px;align-items:center";var m=document.createElement("span");m.textContent="New game version available";var b=document.createElement("button");b.textContent="Reload";b.style.cssText="background:#4a6cf7;color:#fff;border:0;border-radius:6px;padding:6px 12px;cursor:pointer";b.onclick=function(){location.reload();};d.appendChild(m);d.appendChild(b);document.body.appendChild(d);}setTimeout(chk,20000);setInterval(chk,120000);})();
</script>
BLOCK
)
WATCHDOG=${WATCHDOG//__STAMP__/$STAMP}       # ps1 :123-124 (two Replace calls; all
#                                            occurrences each - one pass here is the same)
if has "$idx" '</head>'; then
  apply_patch "$idx" '</head>' "${WATCHDOG}</head>" all
else
  # ps1 :127-129 no </head> to splice into: the block is appended at the end
  printf '%s' "$WATCHDOG" >> "$idx"
fi
if [ "$_stamp_n" = 0 ]; then
  printf 'build asset urls NOT stamped: none of "/webgl.loader.js", "/webgl.framework.js", "/webgl.data", "/webgl.wasm" occur in %s to carry v=%s (already stamped, or this template spells the asset urls another way); catalog-fetch stamp + auto-refresh watchdog baked\n' "$idx" "$STAMP"
else
  printf 'stamped %s/4 build asset urls v=%s; catalog-fetch stamp + auto-refresh watchdog baked\n' "$_stamp_n" "$STAMP"
fi

# ---------------------------- 9. brotli sidecars ----------------------------
# ps1 :133-135 called scripts\brotli-sidecar\bin\Release\net8.0\sidecar.exe <webroot>.
# The tool is net8.0 and framework-dependent (Program.cs), so on Linux run its assembly
# through the runtime: dotnet <SIDECAR_DLL> <webroot> <quality>. Same tool means the same
# skip rules (png/jpg/webm/ogg/mp3 magic bytes, files under 1024 B, *.br itself) and the
# same quality 6 default (Program.cs:15,22-28); wasm is NOT skipped.
# If a web server front-ends this webroot later, nginx brotli_static on; can serve these
# same .br sidecars instead of the gate, no re-encoding needed.
if [ "$NO_BROTLI" = 1 ]; then
  printf 'skipped brotli sidecars (--no-brotli)\n'
else
  if [ ! -f "$SIDECAR_DLL" ]; then die "no sidecar tool at $SIDECAR_DLL"; fi
  if ! command -v dotnet >/dev/null 2>&1; then die "dotnet runtime not on PATH (needed for $SIDECAR_DLL)"; fi
  dotnet "$SIDECAR_DLL" "$WEBROOT" "$BROTLI_QUALITY"
fi

# ---------------------------- 9b. swap the staged root into place ----------------------------
# Two renames instead of a wipe: the previous root only leaves the tree once the new one stands at
# its path, and that is all the renames do - they are NOT a net. A live process does not block
# renaming the directory it works in, and a front that opens files per request never holds the root
# at all (measured on the Windows twin: probe_rename_is_not_a_net case P, ledger L91), so the net
# is the probe - and since step 3 answered, the 9 GB mirror and the whole patch pipeline have run,
# which is minutes in which a front can have come up. Same question, same function, here again.
# The old root then waits at WEBROOT.old-<stamp>-<pid> as the undo handle, and nothing in this
# script removes it before something is observed answering out of the new root (step 10, L90).
publish_staged_webroot() {
  assert_root_not_served "$TARGET_ROOT"                   # asked again on the line that destroys
  _pw_leaf=${TARGET_ROOT##*/}
  if [ -z "$_pw_leaf" ]; then die "refusing to publish into $TARGET_ROOT: it has no leaf name to swap"; fi
  if [ -e "$TARGET_ROOT" ] || [ -L "$TARGET_ROOT" ]; then
    _aside="$TARGET_ROOT.old-$(date +%Y%m%d%H%M%S)-$$"
    if ! _pw_why=$(mv -- "$TARGET_ROOT" "$_aside" 2>&1); then
      die "cannot move the existing webroot $TARGET_ROOT aside ($_pw_why): nothing was replaced."
    fi
  fi
  if [ -e "$TARGET_ROOT" ] || [ -L "$TARGET_ROOT" ]; then
    # Somebody took the leaf name while the copy was staging, and mv would land the staged tree
    # inside it. Put the old root back where it was and change nothing else.
    _keep_staged=1
    if _pw_back=$(mv -- "$_aside" "$TARGET_ROOT" 2>&1); then
      die "the leaf name $TARGET_ROOT was taken while the copy was staging: the previous webroot is put back in place, the staged copy is kept at $_staged"
    fi
    die "the leaf name $TARGET_ROOT was taken while the copy was staging, and the previous webroot could NOT be put back ($_pw_back): it is still at $_aside and $TARGET_ROOT is not it. Put it back by hand: mv -- '$_aside' '$TARGET_ROOT'. The staged copy is kept at $_staged"
  fi
  if ! _pw_why=$(mv -- "$_staged" "$TARGET_ROOT" 2>&1); then
    _keep_staged=1                                        # the exit trap must not delete what this names
    if [ -n "$_aside" ]; then
      if _pw_back=$(mv -- "$_aside" "$TARGET_ROOT" 2>&1); then
        die "cannot move the assembled webroot into place ($_pw_why): the previous webroot was put back at $TARGET_ROOT, the staged copy is kept at $_staged"
      fi
      die "cannot move the assembled webroot into place ($_pw_why): the previous webroot could NOT be put back ($_pw_back) - it is still at $_aside and $TARGET_ROOT is not it. Put it back by hand: mv -- '$_aside' '$TARGET_ROOT'. The staged copy is kept at $_staged"
    fi
    die "cannot move the assembled webroot into place ($_pw_why): the target was not there to begin with, the staged copy is kept at $_staged"
  fi
  _staged=''                     # published: the exit trap must not clean up the target
  if [ -n "$_aside" ]; then
    printf 'webroot swapped in: %s - the previous root is kept at %s, because nothing has proved the new root answers yet (it is the undo handle: remove it by hand once you have checked the new root, or --serve removes it once a front answers for it)\n' "$TARGET_ROOT" "$_aside"
  else
    printf 'webroot swapped in: %s (there was no previous root to remove)\n' "$TARGET_ROOT"
  fi
}
publish_staged_webroot
WEBROOT=$TARGET_ROOT             # steps 4-9 worked on the staged copy; from here the real path again

# ---------------------------- 10. optional serve ----------------------------
# ps1 :136-147. Off by default; nothing is started unless SERVE=1 / --serve. The assembly
# (dotnet, WEBFRONT_DLL) was already checked in step 1; what is checked here is the port and
# the pid record, because starting a gate is the one action in this script that can collide
# with a gate another lane owns.
# WEBFRONT_GATE=0 is relay mode, as in the PowerShell dev serve. The pid this script
# starts is recorded in GATE_PID_FILE - the same single gate pid file lane_up.sh writes
# and lane_down.sh stops - so the next assemble run can stop it. It is never recorded
# over a live pid: step 2 already stopped the recorded gate (or --no-gate-stop was asked
# for, in which case the record belongs to somebody else and is left alone).
if [ "$SERVE" = 1 ]; then
  # Probe before starting (lane_up.sh:356-363 posture): a gate that cannot bind its port
  # fails inside its own log, after this script already reported nothing wrong.
  if port_in_use 127.0.0.1 "$GATE_PORT"; then
    die "something already answers 127.0.0.1:$GATE_PORT (the gate port) - refusing to start a gate on it. Stop that front first (scripts/linux/lane_down.sh, or systemctl stop mfg-gate.service) or set GATE_PORT"
  fi
  # Free is not enough. A free INGRESS port is exactly where a gate-off relay must not be put, and it
  # is free most of the time only because the hosted gate that normally sits behind it is down - so the
  # probe above passes on precisely the setup this rule exists to refuse (ps1 :122-124).
  assert_serve_port_not_ingress 'The new webroot is published but no front was started, so nothing is serving it. The gate record and the undo handle (if any) were left exactly as they are.'
  if [ -f "$GATE_PID_FILE" ]; then
    _live=$(sed -n '1p' "$GATE_PID_FILE" 2>/dev/null | tr -d '\r\t' | sed 's/^ *//; s/ *$//')
    case $_live in
      ''|*[!0-9]*) : ;;
      *) if pid_alive "$_live"; then
           die "--serve would overwrite the gate record $GATE_PID_FILE while pid $_live is live: stop that gate first (lane_down.sh) or assemble without --serve"
         fi ;;
    esac
  fi
  WEBFRONT_ROOT=$WEBROOT
  WEBFRONT_UPSTREAM=$UPSTREAM
  WEBFRONT_PORT=$GATE_PORT
  WEBFRONT_BIND=$BIND
  WEBFRONT_GATE=0
  export WEBFRONT_ROOT WEBFRONT_UPSTREAM WEBFRONT_PORT WEBFRONT_BIND WEBFRONT_GATE
  mkdir -p -- "$LOG_DIR"
  ( cd "${WEBFRONT_DLL%/*}" && exec dotnet "${WEBFRONT_DLL##*/}" ) \
    >"$LOG_DIR/webfront.out.txt" 2>"$LOG_DIR/webfront.err.txt" &
  servePid=$!
  mkdir -p -- "$PID_DIR"
  echo "$servePid" > "$GATE_PID_FILE"
  # Ask whether the process this run just started actually answers, BEFORE any up claim. $! proves
  # the shell forked a child, not that the child bound its port: dotnet exits 1 when it cannot bind
  # (webfront/Program.cs:371), and the old line printed "webfront started (pid ...)" either way - the
  # exact unobserved success lane_up.sh refuses with wait_gate_up (:546). The question is the one step
  # 3 and 9b ask (/build_stamp.txt against this root's own stamp), and it is now asked whether or not a
  # previous root was moved aside: a first-time swap used to start a front that NOTHING ever checked.
  _disk=$(_disk_stamp "$WEBROOT")
  _proved=0
  _last="it never answered on 127.0.0.1:$GATE_PORT"
  _pt=1
  while [ $_pt -le 40 ]; do                       # ~10s, the window a dotnet front needs to bind
    _st=''; _body=''
    if _pf=$(_probe_front "$GATE_PORT"); then
      { read -r _st; read -r _body; } <<<"$_pf" || true
      if [ "$_st" = 200 ] && [ -n "$_disk" ] && [ "$_body" = "$_disk" ]; then _proved=1; break; fi
      _last="it answers /build_stamp.txt with HTTP $_st (body: ${_body:-<empty>})"
    fi
    sleep 0.25
    _pt=$((_pt+1))
  done
  if [ "$_proved" = 1 ]; then
    printf 'webfront listening (pid %s, recorded in %s, logs in %s) bind=%s port=%s WEBFRONT_UPSTREAM=%s: it answered /build_stamp.txt with %s, the stamp in %s\n' "$servePid" "$GATE_PID_FILE" "$LOG_DIR" "$BIND" "$GATE_PORT" "$UPSTREAM" "$_disk" "$WEBROOT/build_stamp.txt"
  else
    printf 'webfront pid %s was recorded in %s, but this run did NOT PROVE it is listening: %s, so the new root is UNPROVEN against %s (its stamp is %s)\n' "$servePid" "$GATE_PID_FILE" "$_last" "$WEBROOT/build_stamp.txt" "${_disk:-<none>}"
  fi
  # Step 9b's other half, and the only place its rule can be met: only the answer above proves the new
  # root serves, so only on that answer does the previous root go; if it never came the aside stays put
  # and this says so, instead of quietly deleting the last thing the lane could have rolled back to (L90).
  if [ -n "$_aside" ]; then
    if [ "$_proved" = 1 ]; then
      printf 'the new root answers for itself on 127.0.0.1:%s: the previous root can go\n' "$GATE_PORT"
      if _rm_why=$(rm -rf -- "$_aside" 2>&1); then
        printf 'the previous root at %s was removed\n' "$_aside"
      else
        printf 'the previous root at %s could NOT be removed (%s): it is still there and nothing else was changed\n' "$_aside" "$_rm_why"
      fi
    else
      printf 'the front this run started (pid %s) never answered /build_stamp.txt with the stamp of this root (%s): the new root is UNPROVEN, so the previous root was kept at %s\n' "$servePid" "$_last" "$_aside"
    fi
  fi
  if [ "$_proved" != 1 ]; then
    _kept='there was no previous root to fall back to'
    if [ -n "$_aside" ]; then _kept="the previous root was kept at $_aside (the undo handle)"; fi
    die "the webroot was assembled and stands at $WEBROOT, but --serve could not prove webfront pid $servePid is listening: it never answered /build_stamp.txt on 127.0.0.1:$GATE_PORT with this root's stamp (${_disk:-<none>}). The gate record $GATE_PID_FILE was kept, and $_kept."
  fi
fi

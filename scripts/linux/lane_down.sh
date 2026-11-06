#!/usr/bin/env bash
# lane_down.sh - Linux replacement for scripts/hosting_down.ps1 (13 lines).
#
# hosting_down.ps1 stopped lane components by WMI name match
# (WebFront.exe / cloudflared.exe / node+AsphyxiaCore.js, :6-11). This port never
# matches a name: it stops exactly the pids lane_up.sh recorded in PID_DIR, and it
# verifies each pid's command line before sending any signal. Nothing is ever killed
# by name, pattern, pgrep/pkill, or port ownership. pid 1 and this script's own
# process (and its parent) are never signalled.
#
# What it reads: PID_DIR/core.pid, PID_DIR/gate.pid, PID_DIR/tunnel.pid.
# For each entry:
#   no pid file            -> reported, nothing to do
#   malformed pid file     -> refused with the reason, file kept
#   pid 1 / own pid        -> refused with the reason, file kept
#   pid not running        -> reported stale, stale pid file removed
#   identity mismatch      -> NOT signalled, pid file KEPT: the pid is alive, so deleting
#                             its file would leave an unmanaged server holding its port
#                             (the 2026-11-05 core/127.0.0.1:8083 incident); the mismatch
#                             and the CORE_DIR/WEBFRONT_DIR assumed are printed
#   command line match     -> SIGTERM, bounded poll, SIGKILL if still alive, then
#                             the pid file is removed
#
# Usage
# -----
#   scripts/linux/lane_down.sh [-h|--help]
#
#   PID_DIR    directory holding core.pid gate.pid tunnel.pid. default: ~/mfgtmp/lane_pids
#   CORE_DIR   core repo root. Identity rule one: a pid is the core when its working
#              directory is <CORE_DIR>/dist. This matters because AsphyxiaCore rewrites
#              the node process title, so /proc/<pid>/cmdline reads
#              "MFG Local v1.70b | P" and no command-line rule can match it (observed
#              on the VPS lane on 2026-11-05). default: <repo>/tmp/RyuNET-core - the
#              same value lane_up.sh:78 starts the core from (<repo> = parent of scripts/)
#   WEBFRONT_DIR  gate repo root. Same rule for the gate: working directory
#              <WEBFRONT_DIR>/bin/Release/net8.0. default: <repo>/webfront (lane_up.sh:79)
#   STOP_TRIES poll attempts (0.5 s apart, never over 1 s each) between SIGTERM and
#              SIGKILL. default: 40 (= 20 s)
#   KILL_TRIES poll attempts after SIGKILL before declaring survival. default: 10 (= 5 s)
#
# Exit code: 0 when every recorded entry was stopped, absent, or stale-cleaned;
# 1 when at least one entry was refused (malformed, protected pid, command-line
# mismatch kept) or survived SIGKILL - a refusal is a deliberate refusal, and the
# reason is printed for every one of them.

set -euo pipefail
export LC_ALL=C

# ---------------------------- configuration ----------------------------
SCRIPT_PATH=${BASH_SOURCE[0]}
SCRIPT_DIR=${SCRIPT_PATH%/*}                 # <repo>/scripts/linux
REPO_ROOT=${SCRIPT_DIR%/*}                   # <repo>/scripts
REPO_ROOT=${REPO_ROOT%/*}                    # <repo>  (the lane_up.sh:74-76 model)

PID_DIR=${PID_DIR:-"$HOME/mfgtmp/lane_pids"}
CORE_DIR=${CORE_DIR:-"$REPO_ROOT/tmp/RyuNET-core"}        # = lane_up.sh:78
WEBFRONT_DIR=${WEBFRONT_DIR:-"$REPO_ROOT/webfront"}       # = lane_up.sh:79
STOP_TRIES=${STOP_TRIES:-40}
KILL_TRIES=${KILL_TRIES:-10}
POLL_STEP=0.5

usage() {
  sed -n '2,/^set -euo/p' "$SCRIPT_PATH" | sed '$d'
}

say() { printf 'lane_down: %s\n' "$1"; }

while [ $# -gt 0 ]; do
  case $1 in
    -h|--help) usage; exit 0 ;;
    *)         printf 'lane_down: unknown argument: %s\n' "$1" >&2; exit 2 ;;
  esac
  shift
done

# ---------------------------- helpers ----------------------------
_norm_name() { printf '%s' "$1" | tr -d '"' | tr 'A-Z' 'a-z' | sed 's/\.exe$//; s/\.dll$//'; }   # tr -d '"' tolerates MSYS's quoted argv0 synthesis

read_pid_file() { sed -n '1p' "$1" 2>/dev/null | tr -d '\r\t' | sed 's/^ *//; s/ *$//'; }

cmdline_fields() {
  mapfile -t _cf_out < <(tr '\0' '\n' < "/proc/$1/cmdline" 2>/dev/null)
}

# same identity rules as lane_up.sh guard_pid_file: command line only, never comm,
# never pgrep, never a name pattern
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

cmdline_pretty() {
  cmdline_fields "$1"
  local _out='' _w
  for _w in "${_cf_out[@]}"; do _out="$_out${_out:+ }$_w"; done
  printf '%s' "${_out:-<unreadable>}"
}

pid_alive() { [ -e "/proc/$1" ] || kill -0 "$1" 2>/dev/null; }

# Identity rule two, same as lane_up.sh: the pid's working directory. AsphyxiaCore
# rewrites the node process title, so a live core's /proc/<pid>/cmdline reads
# "MFG Local v1.70b | P" and no command-line rule can match it; the cwd lane_up.sh
# started it in is the proof that survives the title rewrite.
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

poll_gone() {  # poll_gone <pid> <tries>; 0 = gone, 1 = still alive
  local _i=1
  while [ $_i -le $2 ]; do
    pid_alive "$1" || return 0
    sleep "$POLL_STEP"
    _i=$((_i+1))
  done
  return 1
}

stopped=0 refused=0 stale=0 absent=0 survived=0

# entry <kind> <file name>
stop_entry() {
  local _kind=$1 _file="$PID_DIR/$2" _label=$1 _pid _cmd
  if [ ! -f "$_file" ]; then
    say "$_label: no pid file ($_file); nothing recorded, nothing signalled"
    absent=$((absent+1))
    return 0
  fi
  _pid=$(read_pid_file "$_file")
  case $_pid in
    ''|*[!0-9]*)
      say "$_label: REFUSED - pid file '$_file' does not hold a plain pid (line 1: '$_pid'); file kept"
      refused=$((refused+1)); return 1 ;;
  esac
  if [ "$_pid" -le 1 ]; then
    say "$_label: REFUSED - pid file holds pid $_pid; never signalling pid 0/1"
    refused=$((refused+1)); return 1
  fi
  if [ "$_pid" = "$$" ] || [ "$_pid" = "$PPID" ]; then
    say "$_label: REFUSED - pid file holds this script or its parent (pid $_pid)"
    refused=$((refused+1)); return 1
  fi
  if ! pid_alive "$_pid"; then
    say "$_label: pid $_pid is not running; stale pid file '$_file' removed"
    rm -f -- "$_file"
    stale=$((stale+1)); return 0
  fi
  if [ ! -r "/proc/$_pid/cmdline" ] && ! cwd_matches "$_kind" "$_pid"; then
    say "$_label: REFUSED - pid $_pid is alive, /proc/$_pid/cmdline cannot be read and its working directory is not the $_label directory; signalling without identity proof is refused; file kept"
    refused=$((refused+1)); return 1
  fi
  if ! entry_matches "$_kind" "$_pid"; then
    _cmd=$(cmdline_pretty "$_pid")
    say "$_label: REFUSED - pid $_pid is neither the $_label program by command line ('$_cmd') nor by working directory ('$(cwd_of "$_pid")'); not signalled; pid file KEPT (deleting it would strand a live server with no handle)"
    say "$_label: if this is the lane's own $_label, run lane_down.sh with the CORE_DIR/WEBFRONT_DIR lane_up.sh used (this run assumed CORE_DIR=$CORE_DIR WEBFRONT_DIR=$WEBFRONT_DIR PID_DIR=$PID_DIR)"
    refused=$((refused+1)); return 1
  fi
  _cmd=$(cmdline_pretty "$_pid")
  kill -TERM "$_pid" 2>/dev/null || {
    say "$_label: REFUSED - kill -TERM $_pid failed (permission or race); file kept"
    refused=$((refused+1)); return 1
  }
  if poll_gone "$_pid" "$STOP_TRIES"; then
    say "$_label: stopped (SIGTERM) pid $_pid [$_cmd]"
    rm -f -- "$_file"
    stopped=$((stopped+1)); return 0
  fi
  kill -KILL "$_pid" 2>/dev/null || true
  if poll_gone "$_pid" "$KILL_TRIES"; then
    say "$_label: stopped (SIGKILL after $_label did not exit on SIGTERM) pid $_pid [$_cmd]"
    rm -f -- "$_file"
    stopped=$((stopped+1)); return 0
  fi
  say "$_label: pid $_pid survived SIGKILL; investigate what it is blocking on"
  survived=$((survived+1)); return 1
}

# ---------------------------- stop exactly what lane_up.sh started ----------------------------
# hosting_down.ps1:6-11 matched process names; here the entries are fixed by role and
# the order does not matter (core, gate, tunnel each verified independently).
rc=0
stop_entry core   core.pid   || rc=1
stop_entry gate   gate.pid   || rc=1
stop_entry tunnel tunnel.pid || rc=1

say "summary: $stopped stopped, $refused refused, $stale stale-cleaned, $absent not recorded, $survived survived"
if [ $rc -ne 0 ] || [ $survived -gt 0 ]; then
  say "lane down: INCOMPLETE - see the refusal reasons above"
  exit 1
fi
if [ $((stopped+stale)) -eq 0 ] && [ $absent -eq 3 ]; then
  say "lane already down (no pid files in $PID_DIR)"   # hosting_down.ps1:13 wording
else
  say "lane down"
fi
exit 0

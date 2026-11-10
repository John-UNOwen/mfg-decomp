#!/usr/bin/env bash
# swap_webroot.sh - promote a staged webroot over the live one, prove it, roll back if it fails.
#
# This is the production webroot swap, productized out of the one-off promote_swap.sh the
# 2026-11-10 promotion ran. That script's whole precondition was two lines - does
# <staged>/build_stamp.txt exist, and does it equal the stamp - and then it stopped the gate,
# renamed the staged tree into the live path and printed success. The stamp is not the tree.
# transfer_build.sh ships one chunk per top-level directory plus one last chunk for the
# top-level files, whose members go in path order, so build_stamp.txt is unpacked BEFORE
# index.html: a transport that dies inside that last chunk leaves a staged tree with a COMPLETE
# build_stamp.txt over an absent or truncated index.html (measured 2026-11-10: the link dying
# 1600 bytes into the last chunk left Build/a.js, StreamingAssets/s.dat, build_stamp.txt = 13
# bytes and index.html = 0 bytes). promote_swap.sh promoted exactly that and exited 0, so the
# gate came back up on a 0-byte play page with the working tree already moved aside. The same
# transfer cut 700 bytes earlier WAS refused, because a missing stamp file is the only
# incompleteness that script could see: the gate it brings up turns on a single tar member.
#
# So the precondition here is the whole staged tree, checked file for file against the manifest
# of the transfer that shipped it - count and per-file size - BEFORE anything is stopped or
# renamed. After the renames the swap proves the new root over HTTP (bounded poll, the answer
# compared with the expected stamp, never just "a port answered"), and any failure after the
# first rename puts the renames back and restarts the services. Nothing is ever deleted: the
# previous root stays at <WEBROOT>.old-<UTC time>-<pid> as the undo handle and every failure
# line prints it.
#
# Usage
# -----
#   WEBROOT=/srv/mfg/webroot STAMP=D759A911DF0C scripts/linux/swap_webroot.sh [--dry-run]
#
#   WEBROOT           the live root the front serves. Required, absolute, and refused when it
#                     normalises to '/' or to/inside a system directory (/bin /boot /dev /etc
#                     /lib /proc /root /sbin /sys /usr /var): the first rename moves whatever
#                     stands there, so the guard transfer_build.sh runs on its extraction target
#                     belongs here too.
#   STAMP             the build stamp the staged tree must carry and the front must answer with
#                     after the swap. Required. No whitespace and no '/'.
#   STAGED            the tree to promote. Default: <WEBROOT>.new-<STAMP> - the name the
#                     delta-staging flow leaves the incoming tree at (cp -al the live root to
#                     that path, ship the delta into it, verify, swap).
#   SERVICES          space-separated systemd units to stop before the renames and start after
#                     them. Default: mfg-gate. Use 'mfg-gate mfg-lane' when the core is a unit
#                     too (and raise PROOF_TRIES: the core needs ~20 s to bind). SERVICES=''
#                     means the front is stopped some other way: nothing is stopped or started
#                     here, and the HTTP proof below is then the only net.
#   BOOKKEEPING_DIR   where the bookkeeping of the transfer run that shipped this tree lives.
#                     Default: $HOME/mfgtmp/transfer_stage - transfer_build.sh's STAGE_DIR, which
#                     holds the local manifest.tsv. Deliberately OUTSIDE any webroot: the
#                     manifest is what the pre-check reads, and a bookkeeping dir inside the live
#                     root is carried off by the first rename, so the path a refusal prints would
#                     no longer hold the file that refusal was based on (and the
#                     .lane_transfer/manifest.tsv + chunk_hashes inside the tree travel with the
#                     old root). A MANIFEST that resolves inside WEBROOT is refused for the same
#                     reason; point BOOKKEEPING_DIR= at the STAGE_DIR of that transfer run.
#   MANIFEST          the manifest of the tree being promoted.
#                     Default: $BOOKKEEPING_DIR/manifest.tsv
#   CHOWN_TO          owner:group the incoming tree is chowned to (the service user must read
#                     what it serves). Default: mfg:mfg. Empty = skip. This runs BEFORE the
#                     services stop: it is an -R over the whole tree (58,809 files / 8.6 GB on
#                     the VPS box), and the one-off promotion did it between the stop and the
#                     start - the part of the outage that never needed the gate down.
#   GATE_PORT         port the HTTP proof asks /build_stamp.txt on. Default: 8090.
#   PROOF_HOST        address the proof dials. Default: 127.0.0.1.
#   PROOF_TRIES       proof polls. Default: 60.   PROOF_SLEEP seconds between them. Default: 0.5
#                     (60 x 0.5 s covers a .NET gate that takes a few seconds to bind its port).
#   REPORT_MAX        how many MISSING / SIZE / EXTRA names to print per class. Default: 20 (0 = all).
#   STAGED_VERIFY_CMD optional; run as: bash -c "$cmd" swap_webroot <STAGED>, before the down
#                     window, and a non-zero exit refuses the swap. This is where a
#                     content-level check plugs in: the pre-check below is count + size, which
#                     is what makes a half-shipped tree visible, while verify_transfer.sh's hash
#                     pass is what proves bytes (a file that changed at the same size is its
#                     report, not this one's).
#
#   --dry-run  validate and run the full staged-tree verification, print what the swap would do
#              (chown, services, aside path, proof) and change nothing: no chown, no stop, no rename.
#
# Order of operations, in one line: verify the staged tree against the manifest -> chown ->
# stop services -> rename live aside -> rename staged in -> start services -> poll the stamp.
# Everything left of the first rename is reversible by construction (nothing has moved); the
# exit trap covers everything right of it.
#
# Disk accounting on a small box: the swap itself costs no space (two renames inside one
# directory on one filesystem), which is the reason it renames rather than extracting - a 30 GB
# VPS box with ~9 GB free cannot hold a second untarred webroot. But while the aside stands
# there are two roots in the tree, so this script never deletes it: smoke the new root, then
# remove the aside by hand (the success line prints the path and the command).
#
# Assumptions: file names contain no newlines (Unity output satisfies this), find -printf is GNU
# find (the README's "What the box must have" already says so for the transfer scripts), and the
# front answers /build_stamp.txt out of its own root (webfront/Program.cs ServeStatic) - the same
# question assemble_webroot.sh asks before it replaces a root.

set -euo pipefail
export LC_ALL=C

# ---------------------------- configuration ----------------------------
WEBROOT=${WEBROOT:-''}
STAMP=${STAMP:-''}
STAGED=${STAGED:-''}
SERVICES=${SERVICES-'mfg-gate'}
BOOKKEEPING_DIR=${BOOKKEEPING_DIR:-"$HOME/mfgtmp/transfer_stage"}
MANIFEST=${MANIFEST:-''}
CHOWN_TO=${CHOWN_TO-'mfg:mfg'}
GATE_PORT=${GATE_PORT:-8090}
PROOF_HOST=${PROOF_HOST:-'127.0.0.1'}
PROOF_TRIES=${PROOF_TRIES:-60}
PROOF_SLEEP=${PROOF_SLEEP:-0.5}
REPORT_MAX=${REPORT_MAX:-20}
STAGED_VERIFY_CMD=${STAGED_VERIFY_CMD:-''}
DRY_RUN=0

SCRIPT_PATH=${BASH_SOURCE[0]}

usage() {
  sed -n '2,/^set -euo/p' "$SCRIPT_PATH" | sed '$d'
}

die() { printf 'swap_webroot: %s\n' "$1" >&2; exit 1; }
say() { printf 'swap_webroot: %s\n' "$1"; }

while [ $# -gt 0 ]; do
  case $1 in
    --dry-run) DRY_RUN=1 ;;
    -h|--help) usage; exit 0 ;;
    *)         die "unknown argument: $1" ;;
  esac
  shift
done

# ---------------------------- the rollback state, declared first ----------------------------
# The trap reads these, so they exist before any step that could fail: nothing moved, nothing
# stopped, no aside, no rollback yet, no temp file.
_live_moved=0
_staged_moved=0
_stopped=''
_rolled=0
_aside=''
_staged_list=''

# ---------------------------- validation ----------------------------
# Local lexical normalisation: collapse '//', resolve './' and '../', drop trailing slashes. The
# renames below act on this value and every message prints it, so nothing is compared that is
# not used (the rule transfer_build.sh states for its remote target, applied to a local path).
_abs_norm() {
  _np=$1
  case $_np in /*) ;; *) return 1 ;; esac
  _np=${_np#/}
  _out=''
  while [ -n "$_np" ]; do
    _c=${_np%%/*}
    if [ "$_c" = "$_np" ]; then _np=''; else _np=${_np#*/}; fi
    case $_c in
      ''|.) continue ;;
      ..)   _out=${_out%/*} ;;
      *)    _out=$_out/$_c ;;
    esac
  done
  printf '%s' "${_out:-/}"
}

[ -n "$WEBROOT" ] || die "WEBROOT is required (the live root to replace, e.g. /srv/mfg/webroot)"
case $WEBROOT in
  *\'*) die "WEBROOT=$WEBROOT contains a single quote - refused, nothing was stopped or renamed" ;;
esac
WEBROOT=$(_abs_norm "$WEBROOT") || die "WEBROOT was not an absolute path - refused, nothing was stopped or renamed"
case $WEBROOT in
  /) die "WEBROOT=$WEBROOT normalises to '/' - refused: the first rename would move the whole filesystem aside" ;;
esac
for _sys in bin boot dev etc lib proc root sbin sys usr var; do
  _sys=/$_sys
  case $WEBROOT in
    "$_sys")   die "WEBROOT=$WEBROOT is the system directory $_sys - refused, nothing was stopped or renamed" ;;
    "$_sys"/*) die "WEBROOT=$WEBROOT is inside the system directory $_sys - refused, nothing was stopped or renamed" ;;
  esac
  case $_sys in
    "$WEBROOT"/*) die "WEBROOT=$WEBROOT contains the system directory $_sys - refused, nothing was stopped or renamed" ;;
  esac
done

[ -n "$STAMP" ] || die "STAMP is required (the build stamp the staged tree must carry, and the front must answer with afterwards)"
case $STAMP in
  *[[:space:]]*|*/*|*\'*) die "STAMP=$STAMP contains whitespace, '/' or a quote - refused: it is compared against the front's /build_stamp.txt answer and it appears in the default STAGED path and the aside name" ;;
esac
case $GATE_PORT in ''|*[!0-9]*) die "GATE_PORT=$GATE_PORT is not a decimal port number" ;; esac
case $PROOF_TRIES in ''|*[!0-9]*) die "PROOF_TRIES=$PROOF_TRIES is not a decimal count" ;; esac
case $REPORT_MAX in ''|*[!0-9]*) die "REPORT_MAX=$REPORT_MAX is not a decimal count (0 = print every name)" ;; esac
case $PROOF_SLEEP in ''|*[!0-9.]*|*.|*.) die "PROOF_SLEEP=$PROOF_SLEEP is not a number of seconds" ;; esac

[ -n "$STAGED" ] || STAGED="$WEBROOT.new-$STAMP"
case $STAGED in
  *\'*) die "STAGED=$STAGED contains a single quote - refused, nothing was stopped or renamed" ;;
esac
STAGED=$(_abs_norm "$STAGED") || die "STAGED was not an absolute path - refused, nothing was stopped or renamed"

case $WEBROOT in
  "$STAGED") die "STAGED=$STAGED and WEBROOT=$WEBROOT are the same tree - refused: the swap would rename the live root onto itself" ;;
esac
case $STAGED in
  "$WEBROOT"/*) die "STAGED=$STAGED is inside WEBROOT=$WEBROOT - refused: the first rename would carry the incoming tree aside with the live root, and the second rename would have nothing to move" ;;
esac
case $WEBROOT in
  "$STAGED"/*) die "WEBROOT=$WEBROOT is inside STAGED=$STAGED - refused: the two paths nest, so no pair of renames can swap them" ;;
esac

[ -n "$MANIFEST" ] || MANIFEST="$BOOKKEEPING_DIR/manifest.tsv"
MANIFEST=$(_abs_norm "$MANIFEST") || die "MANIFEST was not an absolute path - refused: the pre-check below reads it and refuses rather than agreeing with nothing"
case $MANIFEST in
  # The default ($HOME/mfgtmp/transfer_stage, transfer_build.sh's STAGE_DIR) is outside every
  # webroot for this exact reason.
  "$WEBROOT"/*) die "MANIFEST=$MANIFEST resolves inside WEBROOT=$WEBROOT, so the first rename would carry it to the aside path: the manifest this swap verified against would not sit at the path its message prints, and the .lane_transfer bookkeeping would travel with the old root. Point MANIFEST= at the manifest.tsv of the transfer_build.sh run that shipped $STAGED, or BOOKKEEPING_DIR= at that run's STAGE_DIR (default \$HOME/mfgtmp/transfer_stage). Nothing was stopped or renamed." ;;
esac

case $CHOWN_TO in
  *[[:space:]]*) die "CHOWN_TO=$CHOWN_TO contains whitespace - refused (one owner:group token, or CHOWN_TO='' to skip the chown)" ;;
esac

if [ -L "$STAGED" ] || [ ! -d "$STAGED" ]; then
  die "no staged tree at STAGED=$STAGED (it must be a directory, not a symlink: a directory is what gets renamed into the live path) - nothing was stopped or renamed"
fi
if [ -L "$WEBROOT" ] || ([ -e "$WEBROOT" ] && [ ! -d "$WEBROOT" ]); then
  die "WEBROOT=$WEBROOT is not a directory - refused, nothing was stopped or renamed"
fi

# Defined before the trap that calls it: a refusal earlier than the down window still runs the
# trap, and an unknown name there would take the handler down with set -e instead of exiting
# with the refusal's own code. It starts what this run stopped and never stops anything else.
_restore_services() {
  [ -n "$_stopped" ] || return 0
  for _rs_u in $_stopped; do
    if ! _rs_why=$(systemctl start "$_rs_u" 2>&1); then
      printf 'swap_webroot: COULD NOT RESTART %s (%s) - start it yourself: systemctl start %s\n' "$_rs_u" "${_rs_why:-no output}" "$_rs_u" >&2
    else
      printf 'swap_webroot: restarted %s\n' "$_rs_u" >&2
    fi
  done
}

# Any failure from here on leaves the box exactly as it found it, and the trap also removes the
# listing file this run makes. Nothing has moved yet, so the trap has no rename to undo.
on_exit() {
  _oe_rc=$?
  if [ -n "$_staged_list" ]; then rm -f -- "$_staged_list"; fi
  if [ $_oe_rc -eq 0 ]; then exit 0; fi
  if [ $_rolled -eq 1 ]; then exit $_oe_rc; fi
  _rolled=1
  if [ $_live_moved -eq 0 ] && [ $_staged_moved -eq 0 ]; then
    _restore_services                          # nothing moved: the die above already said why
    exit $_oe_rc
  fi
  # Everything right of the first rename comes through here: the renames go back, the services
  # come up, and each line says what ended up where - including the aside path, which is the
  # handle a hand-restore needs if a rename back fails (the Windows twin learned this as L90).
  printf 'swap_webroot: swap failed (exit %s) - rolling the renames back\n' "$_oe_rc" >&2
  if [ $_staged_moved -eq 1 ] && { [ -e "$WEBROOT" ] || [ -L "$WEBROOT" ]; }; then
    if _rb=$(mv -- "$WEBROOT" "$STAGED" 2>&1); then
      printf 'swap_webroot: the promoted tree is back at %s (out of the live path)\n' "$STAGED" >&2
    else
      printf 'swap_webroot: could NOT move the promoted tree out of %s (%s): it is still there, and the live root is NOT in place\n' "$WEBROOT" "${_rb:-no output}" >&2
    fi
  fi
  if [ $_live_moved -eq 1 ]; then
    if [ -e "$_aside" ] || [ -L "$_aside" ]; then
      if _rb=$(mv -- "$_aside" "$WEBROOT" 2>&1); then
        printf 'swap_webroot: the previous root is back at %s (it was kept at %s)\n' "$WEBROOT" "$_aside" >&2
      else
        printf 'swap_webroot: could NOT put the previous root back (%s): it is still at %s - restore it by hand: mv -- %s %s\n' "${_rb:-no output}" "$_aside" "'$_aside'" "'$WEBROOT'" >&2
      fi
    else
      printf 'swap_webroot: %s is gone, so the previous root cannot be put back at %s\n' "$_aside" "$WEBROOT" >&2
    fi
  fi
  _restore_services
  printf 'swap_webroot: rollback finished: live root %s, staged tree %s%s\n' \
    "'$WEBROOT'" "'$STAGED'" "${_aside:+, previous root aside at '$_aside' (the undo handle - nothing was deleted)}" >&2
  exit $_oe_rc
}
trap on_exit EXIT

# ---------------------------- pre-check 1: the stamp token ----------------------------
# The precondition promote_swap.sh had is kept - it is cheap and it catches promoting a
# different build than the operator named - but it is now the first of three, not the only one.
_disk_stamp() {   # <dir> -> trimmed first line of its build_stamp.txt, empty when it has none
  if [ -f "$1/build_stamp.txt" ]; then
    sed -n '1p' "$1/build_stamp.txt" 2>/dev/null | tr -d '\r\t' | sed 's/^ *//; s/ *$//'
  fi
}
_staged_stamp=$(_disk_stamp "$STAGED")
if [ -z "$_staged_stamp" ]; then
  die "no build_stamp.txt in $STAGED - refused: nothing was stopped or renamed"
fi
if [ "$_staged_stamp" != "$STAMP" ]; then
  die "the staged tree $STAGED carries stamp $_staged_stamp, not STAMP=$STAMP - refused: promoting it would replace the live root with a build you did not name (nothing was stopped or renamed)"
fi

# ---------------------------- pre-check 2: the whole staged tree vs the manifest ----------------------------
# This is the check the promotion was missing. The manifest of the transfer that shipped this
# tree is the authority on what that tree holds, so the staged root is compared with it file for
# file - count and per-file size - and any difference refuses the swap before a single service is
# stopped. Sizes rather than contents on purpose: a hash pass over 8.6 GB is ~40 min on that box
# and is verify_transfer.sh's job (it compares sha256 on both sides, and a same-size content
# change is its report); what belongs here is the question promote_swap.sh never asked - is every
# file this build lists actually in the tree I am about to rename into the live path, or did that
# transfer stop partway through the last chunk? The two counts are printed because they are the
# denominator: "the stamp matches" says nothing about 58,809 files, and neither does "the tree
# looks fine".
# MISSING/SIZE is the half-shipped tree. EXTRA is a name the staged tree holds and this build
# does not list: the delta flow says to delete the remote-only names from the staged tree before
# promoting, and a swap that promoted them instead would serve files no catalog points at, which
# is a different build wearing this build's stamp. .lane_transfer/ is the bookkeeping the transfer
# itself wrote into the tree and is excluded, exactly as verify_transfer.sh excludes it.
if [ ! -f "$MANIFEST" ]; then
  die "no manifest at $MANIFEST - refusing to promote a tree this script cannot count against: point MANIFEST= (or BOOKKEEPING_DIR=) at the manifest.tsv of the transfer_build.sh run that shipped $STAGED, or re-run that transfer. The manifest is what makes 'the tree is complete' mean something; nothing was stopped or renamed."
fi
if [ ! -s "$MANIFEST" ]; then
  die "manifest at $MANIFEST has 0 entries - refusing: a comparison against zero expected files agrees with any staged tree, an empty one included; nothing was stopped or renamed"
fi

_staged_list=${TMPDIR:-/tmp}/swap_webroot_staged_list.$$
if [ -e "$_staged_list" ]; then
  _staged_list=''
  die "a listing file already exists at ${TMPDIR:-/tmp}/swap_webroot_staged_list.$$ - refusing to overwrite someone else's file"
fi
if ! ( cd "$STAGED" && find . -type f -not -path './.lane_transfer/*' -printf '%s\t%p\n' | LC_ALL=C sort -t$'\t' -k2 ) > "$_staged_list"; then
  die "cannot list $STAGED to count it against $MANIFEST - refusing; nothing was stopped or renamed"
fi
_cmp_rc=0
_cmp=$(awk -F'\t' -v report="$REPORT_MAX" -v manpath="$MANIFEST" -v staged="$STAGED" '
  function show(tag, p) {
    printed[tag]++
    if (printed[tag] <= report || report == 0) printf "  %s %s\n", tag, p
  }
  # 1st input, the manifest: sha256 <TAB> bytes <TAB> ./path  (transfer_build.sh, verify_transfer.sh)
  NR == FNR {
    if (NF != 3 || $2 !~ /^[0-9]+$/ || $3 == "") {
      printf "%s line %d is not a manifest line (want sha256<TAB>bytes<TAB>./path): %s\n", manpath, FNR, $0 > "/dev/stderr"
      badline = 1; exit 2
    }
    man_size[$3] = $2; man_n++; man_b += $2; next
  }
  # 2nd input, the staged tree: bytes <TAB> ./path  (find -printf, relative to the staged root)
  {
    if (NF != 2 || $1 !~ /^[0-9]+$/ || $2 == "") {
      printf "the staged-tree listing line %d is not a size line (want bytes<TAB>./path): %s\n", FNR, $0 > "/dev/stderr"
      badline = 1; exit 2
    }
    st_size[$2] = $1; st_n++; st_b += $1; seen[$2] = 1; next
  }
  END {
    if (badline) exit 2
    printf "staged tree %s: %d files, %d bytes | manifest %s: %d files, %d bytes\n", staged, st_n, st_b, manpath, man_n, man_b
    if (man_n == 0) { printf "%s resolved to 0 entries - refusing\n", manpath > "/dev/stderr"; exit 2 }
    if (st_n == 0)  { printf "%s holds 0 files outside .lane_transfer - refusing\n", staged > "/dev/stderr"; exit 2 }
    for (p in man_size) {
      if (!(p in seen)) { show("MISSING", p); miss++; continue }
      if (st_size[p] != man_size[p]) { show("SIZE", p); sizebad++; }
    }
    for (p in seen) if (!(p in man_size)) { show("EXTRA", p); extra++ }
    printf "tally: %d MISSING, %d size-mismatch, %d EXTRA (names above, REPORT_MAX=%s per class)\n", miss+0, sizebad+0, extra+0, report
    if (miss || sizebad || extra) exit 1
  }
' "$MANIFEST" "$_staged_list") || _cmp_rc=$?
rm -f -- "$_staged_list"; _staged_list=''
if [ $_cmp_rc -ne 0 ]; then
  printf '%s\n' "$_cmp" >&2
  if [ $_cmp_rc -eq 2 ]; then
    die "the manifest and the staged tree did not both yield files to compare - refusing; nothing was stopped or renamed"
  fi
  die "REFUSED: the staged tree $STAGED is not the tree $MANIFEST describes. A rename promotes a directory whole, and a directory is as complete as its last tar member: build_stamp.txt is unpacked BEFORE index.html in the top-level-files chunk, so a transfer that died inside it leaves a matching stamp over a truncated play page and every check the old promotion had said yes. Nothing was stopped and nothing was renamed: the live root at $WEBROOT is untouched and the staged tree is still at $STAGED. Fix the transfer (transfer_build.sh re-sends the broken chunk whole, then verify_transfer.sh) or promote a tree that matches this manifest."
fi
say "$_cmp"
say "verify: the staged tree is the tree this manifest describes (file count + per-file size; bytes are verify_transfer.sh's proof): $MANIFEST"

# Optional content-level hook, still left of the down window.
if [ -n "$STAGED_VERIFY_CMD" ]; then
  say "running STAGED_VERIFY_CMD against $STAGED"
  if ! bash -c "$STAGED_VERIFY_CMD" swap_webroot "$STAGED"; then
    die "STAGED_VERIFY_CMD exited non-zero for $STAGED - refusing; nothing was stopped or renamed"
  fi
fi

# ---------------------------- the plan ----------------------------
_aside="$WEBROOT.old-$(date -u +%Y%m%dT%H%M%SZ)-$$"
if [ -e "$_aside" ] || [ -L "$_aside" ]; then
  die "the aside path this swap would use already exists: $_aside - refused, nothing was stopped or renamed (an aside is an undo handle: check what holds it, or wait a second and re-run for a fresh name)"
fi
say "plan: chown ${CHOWN_TO:-<skipped>} $STAGED -> stop ${SERVICES:-<nothing>} -> rename $WEBROOT to $_aside -> rename $STAGED to $WEBROOT -> start ${SERVICES:-<nothing>} -> ask $PROOF_HOST:$GATE_PORT /build_stamp.txt until it answers $STAMP"
if [ -e "$WEBROOT" ]; then
  say "the previous root would be kept at $_aside as the undo handle; this script never deletes it"
else
  say "nothing stands at $WEBROOT, so there would be no undo handle: the HTTP proof is the only check on this swap"
fi
if [ $DRY_RUN = 1 ]; then
  say "--dry-run: verification finished, nothing was chowned, stopped or renamed"
  exit 0
fi

# ---------------------------- the tools the down window needs ----------------------------
if [ -n "$SERVICES" ] && ! command -v systemctl >/dev/null 2>&1; then
  die "SERVICES='$SERVICES' was given but systemctl is not on PATH - refusing before anything is stopped or renamed (stop the front yourself and re-run with SERVICES='' so the HTTP proof is the only net)"
fi
if [ -n "$CHOWN_TO" ] && ! command -v chown >/dev/null 2>&1; then
  die "CHOWN_TO='$CHOWN_TO' was given but chown is not on PATH - refusing before anything is stopped or renamed (re-run with CHOWN_TO='' only if the incoming tree is already owned by the service user)"
fi

# ---------------------------- chown, inside the up window ----------------------------
# Before the stop, on purpose: an -R over 58,809 files is work the gate does not need to be down
# for, and the one-off promotion put it between the stop and the start - which is part of why that
# outage was longer than two renames.
if [ -n "$CHOWN_TO" ]; then
  say "chown -R $CHOWN_TO $STAGED (before the services stop: the incoming tree must be readable by $CHOWN_TO while it is still not serving anything)"
  if ! _ch_why=$(chown -R -- "$CHOWN_TO" "$STAGED" 2>&1); then
    die "chown -R $CHOWN_TO $STAGED failed (${_ch_why:-no output}) - refusing inside the up window: nothing was stopped or renamed, and the live root at $WEBROOT is still serving"
  fi
else
  say "skipped the chown (CHOWN_TO is empty): the incoming tree is assumed owned by the service user already"
fi

# ---------------------------- the down window ----------------------------
if [ -n "$SERVICES" ]; then
  for _su in $SERVICES; do
    say "systemctl stop $_su"
    if ! _st_why=$(systemctl stop "$_su" 2>&1); then
      # Still left of the first rename, so the only thing to undo is the service already stopped.
      printf 'swap_webroot: systemctl stop %s failed (%s) - refusing before any rename\n' "$_su" "${_st_why:-no output}" >&2
      _restore_services
      die "could not stop $_su: the live root at $WEBROOT is still serving and nothing was renamed"
    fi
    _stopped="$_stopped $_su"
  done
else
  say "SERVICES is empty: no service was stopped, so the HTTP proof below is the only net on this swap"
fi

if [ -e "$WEBROOT" ]; then
  say "rename $WEBROOT -> $_aside"
  if ! _mv1=$(mv -- "$WEBROOT" "$_aside" 2>&1); then
    _mv1_aside=$_aside
    _aside=''                                   # never used: the trap must not offer to restore it
    printf 'swap_webroot: renaming the live root aside failed (%s)\n' "${_mv1:-no output}" >&2
    _restore_services
    die "cannot move the live root aside: nothing was renamed (the intended aside path $_mv1_aside was not used), $WEBROOT is still in place and still the live root, and $STAGED is still the staged tree"
  fi
  _live_moved=1
fi

say "rename $STAGED -> $WEBROOT"
if ! _mv2=$(mv -- "$STAGED" "$WEBROOT" 2>&1); then
  # The trap puts the aside back and restarts the services; the staged tree never left its path.
  printf 'swap_webroot: renaming the staged tree into the live path failed (%s)\n' "${_mv2:-no output}" >&2
  die "cannot move $STAGED into $WEBROOT: rolling back - the previous root goes back to $WEBROOT from the aside $_aside, the staged tree stays at $STAGED, and the services are restarted"
fi
_staged_moved=1

if [ -n "$SERVICES" ]; then
  for _su in $SERVICES; do
    say "systemctl start $_su"
    if ! _sta_why=$(systemctl start "$_su" 2>&1); then
      # Not fatal on its own: what decides this swap is the answer asked below, and systemd can
      # report a failed unit while the front behind it is up. Say it, then let the proof rule.
      printf 'swap_webroot: systemctl start %s reported failure (%s) - the HTTP proof below decides whether the new root serves\n' "$_su" "${_sta_why:-no output}" >&2
    fi
  done
fi

# ---------------------------- the proof ----------------------------
# One raw read of GET /build_stamp.txt - the probe assemble_webroot.sh:432-459 uses: the front
# answers it out of its own root (webfront/Program.cs ServeStatic), build_stamp.txt is written
# with no trailing newline so a line-oriented read never sees it, and the answer is parsed by awk
# rather than shell patterns. Bounded, and compared with the stamp the operator named. The
# one-off promotion printed 'stamp now: $(curl -sf ...)' under set -e, and a curl that could not
# connect still left that line printed with an empty value - a failed command substitution in an
# argument does not fail echo - so nothing it printed could tell "the new root answers" from "the
# gate is down and nothing answered at all".
_probe_front() {
  _pf_out=$(
    exec 3<>"/dev/tcp/$PROOF_HOST/$GATE_PORT" || exit 1
    printf 'GET /build_stamp.txt HTTP/1.0\r\nHost: %s\r\nConnection: close\r\n\r\n' "$PROOF_HOST" >&3
    # Declared before the read: a connection that comes back reset leaves the variable unset,
    # and under set -u the line after it would die as "unbound variable" instead of answering
    # the one question this function asks - did the front say nothing.
    _pf_all=''
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

say "proof: asking $PROOF_HOST:$GATE_PORT for /build_stamp.txt, expecting $STAMP (up to $PROOF_TRIES tries, $PROOF_SLEEP s apart)"
_proved=0
_last="nothing answered on $PROOF_HOST:$GATE_PORT"
_pt=1
while [ "$_pt" -le "$PROOF_TRIES" ]; do
  _st=''; _body=''
  if _pf=$(_probe_front); then
    { read -r _st; read -r _body; } <<<"$_pf" || true
    if [ "$_st" = 200 ] && [ "$_body" = "$STAMP" ]; then _proved=1; break; fi
    _last="it answered HTTP $_st (body: ${_body:-<empty>})"
  fi
  sleep "$PROOF_SLEEP"
  _pt=$((_pt+1))
done

if [ "$_proved" != 1 ]; then
  # The exit trap does the undo (renames back, services up); this says what was wrong and where
  # the two trees are when the dust settles.
  printf 'swap_webroot: the new root is UNPROVEN: %s - %s:%s never answered /build_stamp.txt with STAMP=%s\n' \
    "$_last" "$PROOF_HOST" "$GATE_PORT" "$STAMP" >&2
  if [ $_live_moved -eq 1 ]; then
    printf 'swap_webroot: the previous root is at %s and is being put back at %s now\n' "$_aside" "$WEBROOT" >&2
  fi
  die "the swap is being rolled back: $WEBROOT ends up as the root that was serving before this run, the unproven tree stays at $STAGED, and the services were restarted. Nothing was deleted; the aside path is $_aside."
fi

say "proof: $PROOF_HOST:$GATE_PORT answered /build_stamp.txt with $STAMP - the live root is this build"
say "swap done: $WEBROOT now serves stamp $STAMP (proven over http on $PROOF_HOST:$GATE_PORT; services restarted: ${SERVICES:-none})"
if [ -n "$_aside" ]; then
  say "previous root kept at $_aside - this script does not delete it: smoke the new root, then free the disk yourself (rm -rf '$_aside'); until you do, the old and the new root both occupy the filesystem, which on a 30 GB box is most of what is free"
else
  say "there was no previous root at $WEBROOT, so there is no aside to remove"
fi
say "the staged tree path $STAGED is now $WEBROOT; the transfer bookkeeping stayed outside the swap at $(dirname -- "$MANIFEST")"

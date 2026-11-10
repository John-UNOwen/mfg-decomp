#!/usr/bin/env bash
# verify_transfer.sh - compare a transfer_build.sh manifest against the remote tree.
#
# Answers "is the 5.47 GB actually there?" instead of assuming it: it pulls the
# remote file size list and the remote sha256 list, compares them with the local
# manifest, and reports missing files, extra files, mismatched files, plus total
# file count and total byte count on both sides. Read-only against the remote: the
# only commands it runs there are cat/find/sha256sum.
#
# Both listings run on the far side under 'bash -c' with 'set -o pipefail' and must end with this
# run's observation-proof trailer line. Every listing is a shell pipeline, and the transport only
# ever reports the LAST stage's status - a far-side find error or killed sha256sum used to arrive as
# exit 0 with a short or empty listing, and the script would then report MISSING/HASH verdicts (or
# empty-tree refusals) for an observation that had errored rather than refuse the errored
# observation itself. A failed far-side stage now ends the listing; a missing, misplaced, or
# duplicated trailer refuses the comparison (2026-11-10 checkup, dead remote guards).
#
# Both listing calls hold the transport's stdin shut ('</dev/null'): nothing in this script reads stdin - the
# comparison is fed by the two listing files - and a transport that behaves like ssh reads its own stdin in
# order to forward it, so a read-only call left holding the caller's stdin eats what the caller was reading:
# a while-read wrapper around this script loses every iteration after the first, under exit 0 (the
# transfer_build.sh chunk-loop incident, repo 9761a3b).
#
# The three inputs are taken by ARGUMENT POSITION - manifest, remote size list, remote hash list - and
# each is then checked against the role it was handed, never by matching the file name. It refuses,
# rather than agreeing, when the manifest resolves to zero entries, when a line of $MANIFEST is not a
# manifest line (so a file that is not the manifest cannot be mistaken for it), and when the remote tree
# holds no files outside .lane_transfer. The agree line therefore means at least one file was compared
# on both sides.
#
# REMOTE_DIR is required, absolute, and guarded exactly as in transfer_build.sh: the value is normalised
# lexically first ('//' collapsed, './' and '../' resolved, trailing '/' removed; the raw and normalised
# values are both printed when they differ) and then refused, before any listing is run, when it
# normalises to '/', to a system directory, to or inside one, or holds a single quote - so a typoed
# REMOTE_DIR can never turn this into a /proc walk, a cd into the invisible ssh login cwd, or an
# 'agree' about a tree the shipping script would never have written. The normalised value is the one
# both listing commands use.

set -euo pipefail
export LC_ALL=C

# ---------------------------- configuration ----------------------------
SSH_TARGET=${SSH_TARGET:-''}
REMOTE_DIR=${REMOTE_DIR:-''}
STAGE_DIR=${STAGE_DIR:-"$HOME/mfgtmp/transfer_stage"}
MANIFEST=${MANIFEST:-"$STAGE_DIR/manifest.tsv"}
VERIFY_TRANSPORT=${VERIFY_TRANSPORT:-'ssh'}
REPORT_MAX=${REPORT_MAX:-40}

SCRIPT_PATH=${BASH_SOURCE[0]}

usage() {
  sed -n '2,/^set -euo/p' "$SCRIPT_PATH" | sed '$d'
}

die() { printf 'verify_transfer: %s\n' "$1" >&2; exit 1; }
say() { printf 'verify_transfer: %s\n' "$1"; }

while [ $# -gt 0 ]; do
  case $1 in
    -h|--help) usage; exit 0 ;;
    *)         die "unknown argument: $1" ;;
  esac
  shift
done

[ -n "$SSH_TARGET" ] || die "SSH_TARGET is required"
[ -n "$REMOTE_DIR" ] || die "REMOTE_DIR is required"

# The guard decides on the value the far side will actually act on, and that is not the string typed.
# The form this replaces (verify_transfer.sh:53-55 before the port) tested non-empty, stripped exactly
# ONE trailing slash and dispatched: 'REMOTE_DIR=/' became 'cd ''' - an empty operand is a successful
# bash cd no-op, so the listings walked the ssh login cwd and the run could print 'agree' about a tree
# nobody serves - while '//', '/usr//' and '/bin///' survived as live system directories (a procfs
# walk, an xargs sha256sum sitting forever on /proc/kmsg) and a relative value resolved against a
# login cwd this script cannot see. So: normalise, then decide, exactly as transfer_build.sh does -
# readlink is not available for this job, the path lives on the far side of ssh and resolving it here
# would resolve the wrong tree, so the normalisation is lexical and the value printed here is the
# value every remote command in this script then uses. The list below is transfer_build.sh's list
# on purpose: the two scripts must accept exactly the same REMOTE_DIR values, or verify can be made
# to agree about (or refuse) a tree the shipping script would never have written.
_norm_remote_dir() {                 # '//' -> '/', './' and '../' resolved, trailing '/' removed
  _np=$1
  case $_np in /*) ;; *) return 1 ;; esac
  _np=${_np#/}
  _out=''
  while [ -n "$_np" ]; do
    _c=${_np%%/*}
    if [ "$_c" = "$_np" ]; then _np=''; else _np=${_np#*/}; fi
    case $_c in
      ''|.) continue ;;
      ..)   _out=${_out%/*} ;;       # '..' at the root stays at the root, as realpath -m resolves it
      *)    _out=$_out/$_c ;;
    esac
  done
  printf '%s' "${_out:-/}"
}
case $REMOTE_DIR in
  /*) ;;
  *) die "REMOTE_DIR=$REMOTE_DIR is not an absolute path - refused, no listing is run (a relative value resolves against the ssh login cwd, which this script cannot see, and a leading ~ is never expanded inside the single-quoted remote commands, so it would make a directory literally named '~')" ;;
esac
case $REMOTE_DIR in
  *\'*) die "REMOTE_DIR=$REMOTE_DIR contains a single quote - refused, no listing is run (every remote command wraps this path in single quotes, so a quote inside the value ends that quoting early: this guard would compare one string while the remote shell acted on another)" ;;
esac
REMOTE_DIR_RAW=$REMOTE_DIR
REMOTE_DIR=$(_norm_remote_dir "$REMOTE_DIR") || die "REMOTE_DIR=$REMOTE_DIR_RAW could not be normalised - refused, no listing is run"
if [ "$REMOTE_DIR" != "$REMOTE_DIR_RAW" ]; then
  say "REMOTE_DIR=$REMOTE_DIR_RAW normalised to $REMOTE_DIR - the guard and every remote command use the normalised value"
fi
case $REMOTE_DIR in
  /) die "REMOTE_DIR=$REMOTE_DIR_RAW normalises to '/' - refused, no listing is run (that is the filesystem root: the listings would walk /proc and xargs sha256sum would sit forever on /proc/kmsg, and a full-tree comparison of the whole filesystem against one build manifest is not an answer about any served tree)" ;;
esac
# Equality is not containment, and read-only is not harmless: listing a system directory, a tree
# inside one, or a tree that holds one compares the operating system's own files against a build
# manifest - a wall of MISSING/EXTRA noise (and /proc, /sys, /dev under the walk) about a tree
# this build never wrote. All three directions are compared against the normalised value.
for _sys in bin boot dev etc lib proc root sbin sys usr var; do
  _sys=/$_sys
  case $REMOTE_DIR in
    "$_sys")   die "REMOTE_DIR=$REMOTE_DIR_RAW normalises to $_sys - refused, no listing is run ($_sys is a system directory)" ;;
    "$_sys"/*) die "REMOTE_DIR=$REMOTE_DIR_RAW normalises to $REMOTE_DIR, inside the system directory $_sys - refused, no listing is run" ;;
  esac
  case $_sys in
    "$REMOTE_DIR"/*) die "REMOTE_DIR=$REMOTE_DIR_RAW normalises to $REMOTE_DIR, which contains the system directory $_sys - refused, no listing is run" ;;
  esac
done

[ -f "$MANIFEST" ] || die "no manifest at $MANIFEST (run transfer_build.sh first, or set MANIFEST=)"
# A manifest with no lines is not one side of a comparison. Refuse here, where the message can name the
# file, rather than let the comparison run over zero entries and report agreement.
[ -s "$MANIFEST" ] || die "manifest at $MANIFEST has 0 entries - refusing: an empty manifest compared against any remote tree would print agreement without having compared a single file (run transfer_build.sh against a non-empty LOCAL_BUILD_DIR, or point MANIFEST= at a real manifest)"

run_remote() {
  # Stdin is INHERITED by this helper: it is the one place a payload could come from, and
  # this script sends no payload - both listings run through run_remote_obs, which closes
  # stdin. A transport that behaves like ssh reads its own stdin to forward it, so a
  # read-only call holding the caller's stdin eats what the caller was reading: a while-read
  # wrapper around this script loses every iteration after the first, under exit 0 (the
  # transfer_build.sh chunk-loop incident, repo 9761a3b, from the other side).
  "$VERIFY_TRANSPORT" "$SSH_TARGET" "$1"
}

# Per-run observation-proof token (same scheme as transfer_build.sh): a listing line starts with 64
# hex, this starts with letters, and the 40 hex bytes are drawn per run, so the trailer can never
# collide with listing data by accident and can never be replayed from another run.
_OBS_OK="MFG_OBS_OK_$( { printf '%s %s %s %s %s' "$$" "$PPID" "$RANDOM" "$RANDOM" "$(date +%s 2>/dev/null || echo 0)"; } | sha256sum | cut -c1-40 )"

run_remote_obs() {   # <out file> <listing command>; 0 ok (trailer stripped), 1 far-side/transport failure,
                     # 2 trailer found inside the listing data, 3 trailer missing or not the last line
  # 'set -o pipefail' inside the far-side bash is the part that makes ssh's exit status mean
  # "the listing happened": without it the transport reports the last pipeline stage's status, and a
  # failing find/sort upstream of a content sort cannot change that status - the guard becomes dead
  # code. The trailer is the second layer: proof the stream arrived whole, from a transport that
  # already promised exit 0. Inner single quotes are re-quoted as the standard '\'' for the outer
  # bash -c argument; REMOTE_DIR is guard-checked to contain no single quote.
  _or_inner="set -o pipefail; $2 && printf '%s\\n' '$_OBS_OK'"
  # '</dev/null' on the listing call, for both listings: this runs in the script body, where
  # the caller's stdin may be a wrapper's work list, and nothing in this script reads stdin
  # (the comparison gets its data from the two files above), so the only thing an inherited
  # stdin can do here is eat that list off under the caller (9761a3b class).
  run_remote "bash -c '${_or_inner//\'/\'\\\'\'}'" </dev/null > "$1" || return 1
  _or_n=$(grep -Fxc -- "$_OBS_OK" "$1" 2>/dev/null || true)
  _or_n=${_or_n:-0}
  if [ "$_or_n" -gt 1 ]; then return 2; fi
  if [ "$_or_n" -eq 0 ] || [ "$(tail -n 1 -- "$1")" != "$_OBS_OK" ]; then return 3; fi
  sed '$d' "$1" > "$1.obs_stripped" && mv -f -- "$1.obs_stripped" "$1"
}

_sizes=$STAGE_DIR/_verify_sizes_remote.tsv
_hashes=$STAGE_DIR/_verify_hashes_remote.txt
mkdir -p -- "$STAGE_DIR"

# Manifest format (written by transfer_build.sh):
#   sha256 <TAB> bytes <TAB> ./path
# Remote size list: bytes <TAB> ./path    (find -printf, relative to REMOTE_DIR)
# Remote hash list:  sha256sum output, "64-hex  ./path"
say "reading remote tree via $VERIFY_TRANSPORT $SSH_TARGET (cat/find/sha256sum only)"
_orc=0
run_remote_obs "$_sizes" "cd '$REMOTE_DIR' && find . -type f -not -path './.lane_transfer/*' -printf '%s\t%p\n' | LC_ALL=C sort -t$'\t' -k2" || _orc=$?
if [ "$_orc" -eq 1 ]; then
  die "remote size listing failed (is REMOTE_DIR=$REMOTE_DIR there?) - a far-side cd/find/sort failure now ends the listing instead of hiding behind a successful sort"
fi
if [ "$_orc" -eq 2 ]; then
  die "the remote size listing carries this run's observation-proof trailer ($_OBS_OK) more than once - proof and listing data cannot be told apart, so the comparison is refused rather than guessed"
fi
if [ "$_orc" -ne 0 ]; then
  die "the remote size listing arrived without this run's observation-proof trailer ($_OBS_OK) as its last line - a listing that cannot prove it arrived whole is not a tree state: refusing to compare"
fi
# Both sides must hold something before anything may be called an agreement. Without these guards a
# fresh or wiped REMOTE_DIR against an empty manifest produced 'totals manifest: 0 files, 0 bytes /
# totals remote: 0 files, 0 bytes / result: 0 ok, 0 missing ...' and then the agree line: the one check
# that exists to catch a failed or never-run transfer reported success for exactly that. This size-list
# check sits before the hash listing is asked for because on an empty tree 'xargs -0 sha256sum' receives
# no operands at all and would sit reading stdin instead of reporting an empty tree.
if [ ! -s "$_sizes" ]; then
  rm -f -- "$_sizes"
  die "the remote tree at $SSH_TARGET:$REMOTE_DIR has 0 files outside .lane_transfer - refusing: an empty remote tree is not an agreement with a manifest (is the transfer finished?)"
fi
_orc=0
run_remote_obs "$_hashes" "cd '$REMOTE_DIR' && find . -type f -not -path './.lane_transfer/*' -print0 | LC_ALL=C sort -z | xargs -0 sha256sum" || _orc=$?
if [ "$_orc" -eq 1 ]; then
  die "remote sha256 listing failed - a far-side cd/find/sort/xargs/sha256sum failure now ends the listing instead of hiding behind the last stage's exit status"
fi
if [ "$_orc" -eq 2 ]; then
  die "the remote sha256 listing carries this run's observation-proof trailer ($_OBS_OK) more than once - proof and listing data cannot be told apart, so the comparison is refused rather than guessed"
fi
if [ "$_orc" -ne 0 ]; then
  die "the remote sha256 listing arrived without this run's observation-proof trailer ($_OBS_OK) as its last line - a listing that cannot prove it arrived whole is not a tree state: refusing to compare"
fi
if [ ! -s "$_hashes" ]; then
  rm -f -- "$_sizes" "$_hashes"
  die "the remote sha256 listing of $SSH_TARGET:$REMOTE_DIR came back empty while its size list was not - refusing to compare sizes against no hashes"
fi

rc=0
# Roles are the argument order, full stop: $1 manifest, $2 remote size list, $3 remote hash list. The
# dispatch this replaced (verify_transfer.sh:68-73) asked each input what it was by matching FILENAME
# against /manifest/ and /_sizes/, so MANIFEST= at a path without that word put a good manifest into the
# sha256sum branch and left man_n at 0, while a STAGE_DIR merely named .../manifests/ mis-read both
# remote lists as the manifest. Position decides; the line shape then proves the role it was given.
awk -F'\t' -v report="$REPORT_MAX" -v manpath="$MANIFEST" '
  function show(tag, p) {
    printed[tag]++
    if (printed[tag] <= report) printf "%s %s\n", tag, p
  }
  # FNR==1 counting yields the argument index without ARGIND, which is a gawk extension while mawk is
  # the awk a plain Ubuntu box ships. All three inputs are proved non-empty in bash first: an empty file
  # emits no records, so it could otherwise pass its role on to the file behind it.
  FNR == 1 { arg++ }
  # pass 1: manifest - sha256 <TAB> bytes <TAB> ./path
  arg == 1 {
    if (NF != 3 || length($1) != 64 || $1 !~ /^[0-9a-f]+$/ || $2 !~ /^[0-9]+$/ || $3 == "") {
      printf "%s line %d is not a manifest line (want sha256<TAB>bytes<TAB>./path): %s\n", manpath, FNR, $0 > "/dev/stderr"
      badline = 1
      exit 2
    }
    man_hash[$3] = $1; man_size[$3] = $2; man_n++; man_b += $2; next
  }
  # pass 2: remote sizes - bytes <TAB> ./path
  arg == 2 {
    if (NF != 2 || $1 !~ /^[0-9]+$/ || $2 == "") {
      printf "the remote size list line %d is not a size line (want bytes<TAB>./path): %s\n", FNR, $0 > "/dev/stderr"
      badline = 1
      exit 2
    }
    r_size[$2] = $1; r_n++; r_b += $1; seen[$2] = 1; next
  }
  # pass 3: remote sha256sum output - 64 hex + two spaces + path
  { h2 = substr($0, 1, 64); p2 = substr($0, 67); r_hash[p2] = h2 }
  END {
    if (badline) exit 2
    printf "totals manifest: %d files, %d bytes\n", man_n, man_b
    printf "totals remote : %d files, %d bytes\n", r_n, r_b
    if (man_n == 0) {
      printf "%s resolved to 0 entries - refusing: a comparison of nothing is not an agreement\n", manpath > "/dev/stderr"
      exit 2
    }
    if (r_n == 0) {
      printf "the remote tree resolved to 0 files - refusing: an empty tree is not an agreement\n" > "/dev/stderr"
      exit 2
    }
    for (p in man_hash) {
      if (!(p in seen))        { show("MISSING", p); miss++; continue }
      if (r_size[p] != man_size[p]) { show("SIZE",   p); sizebad++; }
      else if (r_hash[p] != man_hash[p]) { show("HASH",  p); hashbad++; }
      else ok++
    }
    for (p in seen) if (!(p in man_hash)) { show("EXTRA", p); extra++ }
    printf "result: %d ok, %d missing, %d extra, %d size-mismatch, %d hash-mismatch\n", \
           ok, miss, extra, sizebad, hashbad
    if (miss || extra || sizebad || hashbad) exit 1
  }
' "$MANIFEST" "$_sizes" "$_hashes" || rc=$?

rm -f -- "$_sizes" "$_hashes"
# awk exits 2 when an input did not describe the side of the comparison it was given: that is a refusal,
# not a mismatch, and the two lines say opposite things to whoever decides whether to bring a lane up on
# this tree. Only a 1 - files were compared and did not line up - may be called a mismatch.
if [ $rc -eq 2 ]; then
  say "verify: REFUSED - the manifest and the remote tree did not both yield files to compare (see the line above); no agreement claimed"
  exit 1
fi
if [ $rc -ne 0 ]; then
  say "verify: MISMATCH - the remote tree does not match the manifest"
  exit 1
fi
say "verify: manifest and remote tree agree"
exit 0

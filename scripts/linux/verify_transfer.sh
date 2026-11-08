#!/usr/bin/env bash
# verify_transfer.sh - compare a transfer_build.sh manifest against the remote tree.
#
# Answers "is the 5.47 GB actually there?" instead of assuming it: it pulls the
# remote file size list and the remote sha256 list, compares them with the local
# manifest, and reports missing files, extra files, mismatched files, plus total
# file count and total byte count on both sides. Read-only against the remote: the
# only commands it runs there are cat/find/sha256sum.
#
# The three inputs are taken by ARGUMENT POSITION - manifest, remote size list, remote hash list - and
# each is then checked against the role it was handed, never by matching the file name. It refuses,
# rather than agreeing, when the manifest resolves to zero entries, when a line of $MANIFEST is not a
# manifest line (so a file that is not the manifest cannot be mistaken for it), and when the remote tree
# holds no files outside .lane_transfer. The agree line therefore means at least one file was compared
# on both sides.

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
REMOTE_DIR=${REMOTE_DIR%/}
[ -f "$MANIFEST" ] || die "no manifest at $MANIFEST (run transfer_build.sh first, or set MANIFEST=)"
# A manifest with no lines is not one side of a comparison. Refuse here, where the message can name the
# file, rather than let the comparison run over zero entries and report agreement.
[ -s "$MANIFEST" ] || die "manifest at $MANIFEST has 0 entries - refusing: an empty manifest compared against any remote tree would print agreement without having compared a single file (run transfer_build.sh against a non-empty LOCAL_BUILD_DIR, or point MANIFEST= at a real manifest)"

run_remote() {
  "$VERIFY_TRANSPORT" "$SSH_TARGET" "$1"
}

_sizes=$STAGE_DIR/_verify_sizes_remote.tsv
_hashes=$STAGE_DIR/_verify_hashes_remote.txt
mkdir -p -- "$STAGE_DIR"

# Manifest format (written by transfer_build.sh):
#   sha256 <TAB> bytes <TAB> ./path
# Remote size list: bytes <TAB> ./path    (find -printf, relative to REMOTE_DIR)
# Remote hash list:  sha256sum output, "64-hex  ./path"
say "reading remote tree via $VERIFY_TRANSPORT $SSH_TARGET (cat/find/sha256sum only)"
run_remote "cd '$REMOTE_DIR' && find . -type f -not -path './.lane_transfer/*' -printf '%s\t%p\n' | LC_ALL=C sort -t$'\t' -k2" > "$_sizes" || \
  die "remote size listing failed (is REMOTE_DIR=$REMOTE_DIR there?)"
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
run_remote "cd '$REMOTE_DIR' && find . -type f -not -path './.lane_transfer/*' -print0 | LC_ALL=C sort -z | xargs -0 sha256sum" > "$_hashes" || \
  die "remote sha256 listing failed"
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

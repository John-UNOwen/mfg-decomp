#!/usr/bin/env bash
# verify_transfer.sh - compare a transfer_build.sh manifest against the remote tree.
#
# Answers "is the 5.47 GB actually there?" instead of assuming it: it pulls the
# remote file size list and the remote sha256 list, compares them with the local
# manifest, and reports missing files, extra files, mismatched files, plus total
# file count and total byte count on both sides. Read-only against the remote: the
# only commands it runs there are cat/find/sha256sum.

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
run_remote "cd '$REMOTE_DIR' && find . -type f -not -path './.lane_transfer/*' -print0 | LC_ALL=C sort -z | xargs -0 sha256sum" > "$_hashes" || \
  die "remote sha256 listing failed"

rc=0
awk -F'\t' -v report="$REPORT_MAX" '
  function show(tag, p) {
    printed[tag]++
    if (printed[tag] <= report) printf "%s %s\n", tag, p
  }
  # pass 1: manifest (manifest.tsv)
  FNR==1 && FILENAME ~ /manifest/ { }
  FILENAME ~ /manifest/ { man_hash[$3] = $1; man_size[$3] = $2; man_n++; man_b += $2; next }
  # pass 2: remote sizes (size<TAB>path)
  FILENAME ~ /_sizes/ { r_size[$2] = $1; r_n++; r_b += $1; seen[$2] = 1; next }
  # pass 3: remote sha256sum output (64 hex + two spaces + path)
  { h2 = substr($0, 1, 64); p2 = substr($0, 67); r_hash[p2] = h2 }
  END {
    for (p in man_hash) {
      if (!(p in seen))        { show("MISSING", p); miss++; continue }
      if (r_size[p] != man_size[p]) { show("SIZE",   p); sizebad++; }
      else if (r_hash[p] != man_hash[p]) { show("HASH",  p); hashbad++; }
      else ok++
    }
    for (p in seen) if (!(p in man_hash)) { show("EXTRA", p); extra++ }
    printf "totals manifest: %d files, %d bytes\n", man_n, man_b
    printf "totals remote : %d files, %d bytes\n", r_n, r_b
    printf "result: %d ok, %d missing, %d extra, %d size-mismatch, %d hash-mismatch\n", \
           ok, miss, extra, sizebad, hashbad
    if (miss || extra || sizebad || hashbad) exit 1
  }
' "$MANIFEST" "$_sizes" "$_hashes" || rc=$?

rm -f -- "$_sizes" "$_hashes"
if [ $rc -ne 0 ]; then
  say "verify: MISMATCH - the remote tree does not match the manifest"
  exit 1
fi
say "verify: manifest and remote tree agree"
exit 0

#!/usr/bin/env bash
# transfer_build.sh - ship the Unity WebGL build output to the VPS over ssh, no rsync.
#
# One chunk per top-level directory of the build tree (plus one chunk holding the
# top-level files), sent as a tar stream through ssh. A chunk is resumable ONLY as a
# whole: a chunk interrupted mid-transfer is re-sent from the beginning on the next
# run. What makes a rerun cheap is that before sending a chunk, the script computes
# the chunk tar's sha256 and skips it when that hash is already recorded on the
# remote side (.lane_transfer/chunk_hashes). A chunked tar cannot resume a partial
# chunk; it can only skip complete ones.
#
# The script never deletes anything remote. It issues no rm/unlink/truncate outside
# its own .lane_transfer bookkeeping, and refuses obviously dangerous REMOTE_DIR
# values outright.
#
# Usage
# -----
#   scripts/linux/transfer_build.sh [--dry-run] [-h|--help]
#
#   SSH_TARGET          ssh target, e.g. deploy@vps.example.com. Required for real runs.
#   REMOTE_DIR          destination tree on the target. Required. Refuses: an empty
#                       value, '/', or an exact system directory (/bin /boot /dev /etc
#                       /lib /proc /root /sbin /sys /usr /var).
#   LOCAL_BUILD_DIR     Unity WebGL output tree.  default: ~/mfg/out/webgl
#                       (same default as assemble_webroot.sh:26; contains index.html,
#                        Build/, StreamingAssets/, TemplateData/)
#   STAGE_DIR           local staging for chunk tars + manifest. default: ~/mfgtmp/transfer_stage
#   COMPRESS            gzip | none | zstd.      default: gzip
#                       gzip = tar -z (gzip is nearly everywhere), zstd = tar --zstd
#                       (needs the zstd binary), none = plain tar (fastest CPU).
#   TRANSFER_TRANSPORT  command used as: $TRANSFER_TRANSPORT <target> <remote-command>
#                       (tar streams travel on its stdin). default: ssh
#                       Offline test hook: point it at a local shim that runs the
#                       command string in a local bash - used by the scratch tests,
#                       never in production.
#   MANIFEST_LIMIT      report cap for dry-run listings. default: 20 (0 = show all)
#
#   --dry-run  computes the manifest locally (reads every file - a 5.47 GB build will
#              take minutes to hash), stages nothing, runs no transport command, and
#              prints the chunk list with the exact commands a real run would use.
#
# Manifests (format documented in verify_transfer.sh):
#   $STAGE_DIR/manifest.tsv        sha256 <TAB> bytes <TAB> ./path    (local truth)
#   $STAGE_DIR/chunk_hashes.tsv    sha256 <TAB> chunk-tar-name        (skip ledger, local)
#   $REMOTE_DIR/.lane_transfer/    uploaded copies of both, written by this script
#
# Assumptions: build file names contain no newlines (Unity output satisfies this).

set -euo pipefail
export LC_ALL=C

# ---------------------------- configuration ----------------------------
SSH_TARGET=${SSH_TARGET:-''}
REMOTE_DIR=${REMOTE_DIR:-''}
LOCAL_BUILD_DIR=${LOCAL_BUILD_DIR:-"$HOME/mfg/out/webgl"}
STAGE_DIR=${STAGE_DIR:-"$HOME/mfgtmp/transfer_stage"}
COMPRESS=${COMPRESS:-'gzip'}
TRANSFER_TRANSPORT=${TRANSFER_TRANSPORT:-'ssh'}
MANIFEST_LIMIT=${MANIFEST_LIMIT:-20}
DRY_RUN=0

SCRIPT_PATH=${BASH_SOURCE[0]}

usage() {
  sed -n '2,/^set -euo/p' "$SCRIPT_PATH" | sed '$d'
}

die() { printf 'transfer_build: %s\n' "$1" >&2; exit 1; }
say() { printf 'transfer_build: %s\n' "$1"; }

while [ $# -gt 0 ]; do
  case $1 in
    --dry-run) DRY_RUN=1 ;;
    -h|--help) usage; exit 0 ;;
    *)         die "unknown argument: $1" ;;
  esac
  shift
done

# ---------------------------- validation ----------------------------
[ -d "$LOCAL_BUILD_DIR" ] || die "no build tree at LOCAL_BUILD_DIR=$LOCAL_BUILD_DIR"
[ -n "$REMOTE_DIR" ] || die "REMOTE_DIR is required"
case $REMOTE_DIR in /) die "REMOTE_DIR must not be '/'" ;; esac
REMOTE_DIR=${REMOTE_DIR%/}
case $REMOTE_DIR in
  /bin|/boot|/dev|/etc|/lib|/proc|/root|/sbin|/sys|/usr|/var)
    die "REMOTE_DIR=$REMOTE_DIR is a system directory - refused" ;;
esac
case $COMPRESS in
  gzip) TAR_FLAG='-z' SUFFIX='.tar.gz' ;;
  none) TAR_FLAG=''   SUFFIX='.tar'   ;;
  zstd) TAR_FLAG='--zstd' SUFFIX='.tar.zst'
        command -v zstd >/dev/null 2>&1 || die "COMPRESS=zstd needs the zstd binary" ;;
  *) die "COMPRESS must be gzip, none or zstd" ;;
esac
if [ $DRY_RUN = 0 ] && [ -z "$SSH_TARGET" ]; then
  die "SSH_TARGET is required for a real transfer (or use --dry-run)"
fi

mkdir -p -- "$STAGE_DIR"

MANIFEST=$STAGE_DIR/manifest.tsv
CHUNK_HASHES=$STAGE_DIR/chunk_hashes.tsv
CHUNK_LIST=$STAGE_DIR/chunks.txt

# ---------------------------- manifest: every file, sha256 + size ----------------------------
say "building manifest from $LOCAL_BUILD_DIR (reads every file)"
_hashes=$STAGE_DIR/_hashes.txt
_sizes=$STAGE_DIR/_sizes.txt
(
  cd "$LOCAL_BUILD_DIR"
  find . -type f -print0 | LC_ALL=C sort -z | xargs -0 sha256sum
) > "$_hashes"
(
  cd "$LOCAL_BUILD_DIR"
  find . -type f -printf '%s\t%p\n' | LC_ALL=C sort -t$'\t' -k2
) > "$_sizes"

awk -F'\t' '
  NR==FNR { sz[$2] = $1; next }
  {
    h = substr($0, 1, 64)
    p = substr($0, 67)                      # sha256sum format: 64 hex + two spaces + path
    if (p in sz) { print h "\t" sz[p] "\t" p }
    else { print h "\t?\t" p; bad = 1 }
  }
  END { if (bad) exit 3 }
' "$_sizes" "$_hashes" > "$MANIFEST" || die "manifest join failed (file lists disagree)"
rm -f -- "$_hashes" "$_sizes"

read _mfiles _mbytes <<EOF
$(awk -F'\t' '{ n++; b += $2 } END { print n+0, b+0 }' "$MANIFEST")
EOF
say "manifest: $_mfiles files, $_mbytes bytes -> $MANIFEST"

# ---------------------------- chunk list ----------------------------
(
  cd "$LOCAL_BUILD_DIR"
  find . -maxdepth 1 -mindepth 1 -type d | LC_ALL=C sort
) > "$CHUNK_LIST"
_rootfiles=$STAGE_DIR/_rootfiles.txt
(
  cd "$LOCAL_BUILD_DIR"
  find . -maxdepth 1 -mindepth 1 -type f | LC_ALL=C sort
) > "$_rootfiles"
[ -s "$_rootfiles" ] && CHUNK_LIST_ROOT=1 || CHUNK_LIST_ROOT=0

chunk_names() {
  cut -c3- "$CHUNK_LIST"                  # strip the leading './'
  [ $CHUNK_LIST_ROOT = 1 ] && printf '%s\n' '_root_files_'
}

chunk_members() {                          # chunk name -> member paths ('./name')
  if [ "$1" = '_root_files_' ]; then
    cat "$_rootfiles"
  else
    printf './%s\n' "$1"
  fi
}

chunk_stats() {                            # chunk name -> files bytes  (from the manifest)
  _cn=$1
  if [ "$_cn" = '_root_files_' ]; then
    awk -F'\t' '
      NR==FNR { want[$0] = 1; next }
      ($3 in want) { n++; b += $2 }
      END { print n+0, b+0 }
    ' "$_rootfiles" "$MANIFEST"
  else
    awk -F'\t' -v p="./$_cn/" 'index($3, p) == 1 { n++; b += $2 } END { print n+0, b+0 }' "$MANIFEST"
  fi
}

tar_name() { printf 'chunk__%s%s' "$1" "$SUFFIX"; }

stage_chunk() {                            # build the chunk tar locally
  _cn=$1 _tn=$(tar_name "$_cn")
  chunk_members "$_cn" > "$STAGE_DIR/_members.txt"
  mapfile -t _mem < "$STAGE_DIR/_members.txt"
  [ ${#_mem[@]} -gt 0 ] || { rm -f -- "$STAGE_DIR/_members.txt"; return 0; }
  ( cd "$LOCAL_BUILD_DIR" && tar $TAR_FLAG -cf "$STAGE_DIR/$_tn" -- "${_mem[@]}" )
  rm -f -- "$STAGE_DIR/_members.txt"
}

run_remote() {                             # run a command string on the remote side
  "$TRANSFER_TRANSPORT" "$SSH_TARGET" "$1"
}

# ---------------------------- dry-run: show the plan and exit ----------------------------
if [ $DRY_RUN = 1 ]; then
  printf '%s\n' \
    'transfer_build --dry-run: no ssh, no staging writes beyond manifests:' \
    "  source      $LOCAL_BUILD_DIR" \
    "  target      ${SSH_TARGET:-<SSH_TARGET unset - would be required>}:$REMOTE_DIR" \
    "  compression $COMPRESS (tar $TAR_FLAG), chunk tars in $STAGE_DIR" \
    '' \
    '  chunk list (one per top-level directory, then the top-level files):'
  shown=0
  while IFS= read -r _cn; do
    set -- $(chunk_stats "$_cn")
    _tn=$(tar_name "$_cn")
    printf '    %-24s files=%-6s bytes=%-12s tar=%s\n' "$_cn" "$1" "$2" "$_tn"
    shown=$((shown+1))
  done < <(chunk_names)
  [ $CHUNK_LIST_ROOT = 1 ] || printf '    (no top-level loose files -> no _root_files_ chunk)\n'
  printf '%s\n' \
    '' \
    '  per chunk, a real run does:' \
    "    ( cd '$LOCAL_BUILD_DIR' && tar $TAR_FLAG -cf '$STAGE_DIR/<tar>' -- <members> )" \
    "    sha256sum '$STAGE_DIR/<tar>'" \
    "    ssh \"\$SSH_TARGET\" \"cat '$REMOTE_DIR/.lane_transfer/chunk_hashes' 2>/dev/null || :\"" \
    '      -> if the line "<sha256>\t<tar>" is present: SKIP the chunk (complete already)' \
    "    ssh \"\$SSH_TARGET\" \"tar $TAR_FLAG -xf - -C '$REMOTE_DIR'\" < '$STAGE_DIR/<tar>'" \
    "    printf '<sha256>\\t<tar>\\n' | ssh \"\$SSH_TARGET\" \"cat >> '$REMOTE_DIR/.lane_transfer/chunk_hashes'\"" \
    '' \
    "  finally: upload the manifest -> $REMOTE_DIR/.lane_transfer/manifest.tsv" \
    "  manifest written for this dry run: $MANIFEST ($_mfiles files, $_mbytes bytes)" \
    '  NOTE: an interrupted chunk is NOT resumable mid-tar; the next run re-sends' \
    '        that whole chunk. Complete chunks are skipped by their sha256.' \
    '  NOTE: this script issues no remote deletions, ever.'
  exit 0
fi

# ---------------------------- real run ----------------------------
say "target: $SSH_TARGET:$REMOTE_DIR (transport: $TRANSFER_TRANSPORT, compress: $COMPRESS)"
remote_ledger=$(run_remote "cat '$REMOTE_DIR/.lane_transfer/chunk_hashes' 2>/dev/null || :") || \
  die "cannot read the remote chunk ledger via $TRANSFER_TRANSPORT"
run_remote "mkdir -p '$REMOTE_DIR/.lane_transfer'" || die "remote mkdir -p '$REMOTE_DIR/.lane_transfer' failed"
: > "$CHUNK_HASHES"

sent=0 skipped=0
while IFS= read -r _cn; do
  _tn=$(tar_name "$_cn")
  stage_chunk "$_cn"
  _hash=$(sha256sum "$STAGE_DIR/$_tn" | cut -d' ' -f1)
  printf '%s\t%s\n' "$_hash" "$_tn" >> "$CHUNK_HASHES"
  if printf '%s\n' "$remote_ledger" | grep -F -q -- "$(printf '%s\t%s' "$_hash" "$_tn")"; then
    say "skip  $_cn ($_tn): sha256 $_hash already complete on the remote"
    skipped=$((skipped+1))
    continue
  fi
  set -- $(chunk_stats "$_cn")
  say "send  $_cn: files=$1 bytes=$2 tar=$_tn sha256=$_hash"
  if ! "$TRANSFER_TRANSPORT" "$SSH_TARGET" "tar $TAR_FLAG -xf - -C '$REMOTE_DIR'" < "$STAGE_DIR/$_tn"; then
    die "chunk '$_cn' failed mid-transfer. A partial chunk cannot be resumed: the next run re-sends it whole. Nothing was deleted remote."
  fi
  if ! printf '%s\t%s\n' "$_hash" "$_tn" | run_remote "cat >> '$REMOTE_DIR/.lane_transfer/chunk_hashes'"; then
    die "chunk '$_cn' landed but its ledger line could not be appended; rerunning will re-send this whole chunk"
  fi
  sent=$((sent+1))
done < <(chunk_names)

if ! "$TRANSFER_TRANSPORT" "$SSH_TARGET" "cat > '$REMOTE_DIR/.lane_transfer/manifest.tsv'" < "$MANIFEST"; then
  die "upload of manifest.tsv to the remote bookkeeping dir failed"
fi

say "transfer done: $sent chunk(s) sent, $skipped chunk(s) skipped as already complete"
say "local manifest: $_mfiles files, $_mbytes bytes (see $MANIFEST)"
say "confirm it for real with: VERIFY_TRANSPORT=... scripts/linux/verify_transfer.sh"

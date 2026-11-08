#!/usr/bin/env bash
# transfer_build.sh - ship the Unity WebGL build output to the VPS over ssh, no rsync.
#
# One chunk per top-level directory of the build tree (plus one chunk holding the
# top-level files), sent as a tar stream through ssh. A chunk is resumable ONLY as a
# whole: a chunk interrupted mid-transfer is re-sent from the beginning on the next
# run. What makes a rerun cheap is the payload digest: each chunk's sha256 over its
# manifest's per-file hash lines (file bytes, never the compressed container, so a
# gzip header timestamp cannot defeat the resume). Before sending a chunk the script
# asks the remote to list the sha256 of every file behind that chunk's paths in
# $REMOTE_DIR right now, keeps the lines for the files this build's manifest lists for
# that chunk, and folds those - so both sides of the comparison cover exactly the same
# files. Equal digests means the tree holds these exact bytes for these exact files and
# the chunk is skipped with 0 bytes sent; a file of the chunk that is absent or holds
# other bytes means it goes again. A file the build no longer lists is in neither fold,
# on purpose: this script issues no remote deletions, so re-sending a tar could never
# remove one, and a leftover there must not be allowed to block the skip - it is
# verify_transfer.sh's EXTRA for whoever maintains the tree to remove. One leftover
# ./Build/webgl.oldname.wasm, the output name a previous build used, used to make the
# two folds unequal forever: the same digest pair on every rerun, that chunk re-sent from
# the top every run, and nothing in the kit able to converge it (2026-11-08 re-check of
# the remote-observation fix below, which folded the whole subtree instead of the chunk).
# The skip is an observation of the tree, never a reading of .lane_transfer/chunk_hashes:
# that ledger is append-only history of what was SENT, and a chunk whose bytes were
# later replaced on the remote read its old ledger line back forever and was reported
# complete while the tree held another build (2026-11-08 checkup, problem 20/48 - see
# the real-run block for the guard this replaced). A chunked tar cannot resume a partial
# chunk; it can only skip ones whose bytes it just looked at.
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
#   REMOTE_DIR          destination tree on the target. Required, absolute, and guarded on its
#                       NORMALISED value (// collapsed, ./ and ../ resolved by string rules,
#                       trailing slashes gone) - the old guard decided on the raw string and
#                       stripped exactly one trailing slash, so '//', '/usr//', '/bin///' and
#                       '/bin/../boot' all passed it and the build tars went into /, /usr, /bin
#                       and /boot. Refused, before anything is staged or extracted: an empty value,
#                       anything relative or ~-prefixed, '/' and anything that normalises to it, a
#                       system directory (/bin /boot /dev /etc /lib /proc /root /sbin /sys /usr
#                       /var) with or without trailing slashes, anything inside one of them,
#                       anything that contains one of them, and a value holding a single quote (the
#                       path is wrapped in single quotes in every remote command). Each refusal
#                       echoes the value it compared and extracts nothing.
#   LOCAL_BUILD_DIR     Unity WebGL output tree.  default: ~/mfg/out/webgl
#                       (same default as assemble_webroot.sh:26; contains index.html,
#                        Build/, StreamingAssets/, TemplateData/)
#                       Refused, in a real run and in --dry-run alike, before anything is staged or sent: a
#                       tree that holds no files. [ -d ] proves the directory exists, and an empty directory is
#                       precisely what a build cleaned out of the path - or renamed away from it, leaving the
#                       old name standing empty - leaves behind; shipping zero chunks used to close with
#                       'transfer done: 0 chunk(s) sent' and exit 0 (2026-11-08 checkup, problem 38/68).
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
#   $STAGE_DIR/manifest.tsv        sha256 <TAB> bytes <TAB> ./path    (local truth; also the source of the payload digests)
#   $STAGE_DIR/chunk_hashes.tsv    sha256 <TAB> chunk-tar-name        (the chunk tars SENT this run, local record)
#   $REMOTE_DIR/.lane_transfer/    manifest uploaded + sent-ledger appended, both by this script;
#                                  the remote ledger is what-was-sent history and never decides a skip
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

# The guard decides on the value the remote will actually act on, and that is not the string typed.
# The block this replaces (transfer_build.sh:83-88 before the fix) tested '/' on the raw value,
# stripped exactly ONE trailing slash, then matched an exact list - so '//' survived as '/', '/usr//'
# and '/bin///' matched no list item, and '/bin/../boot' never equalled '/boot'. Each reached the
# extraction at the end of this file ('tar $TAR_FLAG -xf - -C "$REMOTE_DIR"') and untarred the build -
# 5.47 GB, index.html, Build/, StreamingAssets/ - into /, /usr, /bin, /boot. So: normalise, then
# decide. readlink is not available for this job: the path lives on the far side of ssh, and resolving
# it here would resolve the wrong tree - the normalisation below is lexical, and the value it prints is
# the value every remote command in this script then uses, so nothing is compared that is not used.
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
  *) die "REMOTE_DIR=$REMOTE_DIR is not an absolute path - refused, nothing extracted (a relative value resolves against the ssh login cwd, which this script cannot see, and a leading ~ is never expanded inside the single-quoted remote commands, so it would make a directory literally named '~')" ;;
esac
case $REMOTE_DIR in
  *\'*) die "REMOTE_DIR=$REMOTE_DIR contains a single quote - refused, nothing extracted (every remote command wraps this path in single quotes, so a quote inside the value ends that quoting early: this guard would compare one string while the remote shell acted on another)" ;;
esac
REMOTE_DIR_RAW=$REMOTE_DIR
REMOTE_DIR=$(_norm_remote_dir "$REMOTE_DIR") || die "REMOTE_DIR=$REMOTE_DIR_RAW could not be normalised - refused, nothing extracted"
if [ "$REMOTE_DIR" != "$REMOTE_DIR_RAW" ]; then
  say "REMOTE_DIR=$REMOTE_DIR_RAW normalised to $REMOTE_DIR - the guard and every remote command use the normalised value"
fi
case $REMOTE_DIR in
  /) die "REMOTE_DIR=$REMOTE_DIR_RAW normalises to '/' - refused, nothing extracted (that is the filesystem root: the build tars would be untarred across it)" ;;
esac
# Equality is not containment. Untarring into a directory writes through that whole tree, so a target
# that holds a system directory is as fatal as one that is a system directory, and a target inside one
# puts the build files among /usr's, /var's or /bin's own - which is why all three directions are
# compared here against the normalised value, not against what was typed.
for _sys in bin boot dev etc lib proc root sbin sys usr var; do
  _sys=/$_sys
  case $REMOTE_DIR in
    "$_sys")   die "REMOTE_DIR=$REMOTE_DIR_RAW normalises to $_sys - refused, nothing extracted ($_sys is a system directory)" ;;
    "$_sys"/*) die "REMOTE_DIR=$REMOTE_DIR_RAW normalises to $REMOTE_DIR, inside the system directory $_sys - refused, nothing extracted" ;;
  esac
  case $_sys in
    "$REMOTE_DIR"/*) die "REMOTE_DIR=$REMOTE_DIR_RAW normalises to $REMOTE_DIR, which contains the system directory $_sys - refused, nothing extracted" ;;
  esac
done
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

# That total is the first observation in this script of whether a build is actually there, and printing
# it was the whole handling of it. An existing-but-empty tree - a build cleaned out of the path, or renamed
# away from it, which leaves the old name standing empty - gave 'manifest: 0 files, 0 bytes', an empty chunk
# list, a chunk loop that never ran, an empty manifest uploaded to the remote bookkeeping dir, and the
# closing 'transfer done: 0 chunk(s) sent (0 payload bytes moved), 0 chunk(s) skipped' with exit 0; a wrong
# LOCAL_BUILD_DIR then ended the documented pipeline (README step 1 -> step 2) with a success line here and
# verify_transfer.sh answering for whatever the remote already held (2026-11-08 checkup, problem 38/68).
# [ -d ] above cannot catch this: a directory is exactly what a cleaned-out or renamed build leaves behind.
# So the file count decides, here - after the manifest that carries the answer and before a chunk is staged
# or a single transport command runs, so a refusal stages nothing and sends nothing.
if [ "$_mfiles" -eq 0 ]; then
  die "no build files under LOCAL_BUILD_DIR=$LOCAL_BUILD_DIR (0 files) - refused: nothing staged, nothing sent, no manifest uploaded (the directory exists, and that is all [ -d ] can prove; an empty tree is a build cleaned out of this path or renamed away from it, not a build to ship - re-run with LOCAL_BUILD_DIR at the tree holding index.html, Build/, StreamingAssets/ and TemplateData/)"
fi

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

# Every chunk path now appears inside a single-quoted remote command (the tree observation below), so
# REMOTE_DIR's quoting rule applies to it too: a name holding a single quote would end that quoting
# early on the remote side - the script would ask about one path and observe another. Refuse it up
# front, in the dry run as well, since the plan printed must be the plan a real run can execute.
while IFS= read -r _p; do
  case $_p in
    *\'*) die "a chunk path contains a single quote ('$_p') - refused, nothing extracted: it cannot be quoted inside the remote observation command, and a path this script cannot quote is a path it cannot truthfully ask the remote about" ;;
  esac
done < <(cat "$CHUNK_LIST" "$_rootfiles")

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
    "    payload digest = sha256 over this chunk's manifest lines written as '<file-sha256>  <./path>' (path order)" \
    "    ssh \"\$SSH_TARGET\" \"cd '$REMOTE_DIR' && find ./<chunk> -type f -print0 | LC_ALL=C sort -z | xargs -0 -r sha256sum | LC_ALL=C sed 's/^\\(.\\{64\\}\\)../\\1  /'\"  > per-file lines the remote tree holds under ./<chunk>" \
    '      -> the remote lists each file behind ./<chunk> as one "<sha256>  ./<path>" line (read-only: cd/find/sort/xargs/sed/sha256sum)' \
    "      -> local side keeps only the lines for files this build's manifest lists for this chunk, then sha256s that set;" \
    '         a surplus remote file under ./<chunk> is not in the manifest set, so it stays out of the fold and the skip' \
    "      -> if that equals this build's payload digest: SKIP the chunk, 0 bytes sent - an observation of the tree," \
    '         never a reading of the sent-ledger, so bytes replaced by another build or a pruned directory re-send' \
    "    ( cd '$LOCAL_BUILD_DIR' && tar $TAR_FLAG -cf '$STAGE_DIR/<tar>' -- <members> )" \
    "    ssh \"\$SSH_TARGET\" \"tar $TAR_FLAG -xf - -C '$REMOTE_DIR'\" < '$STAGE_DIR/<tar>'" \
    "    printf '<tar-sha256>\\t<tar>\\n' | ssh \"\$SSH_TARGET\" \"cat >> '$REMOTE_DIR/.lane_transfer/chunk_hashes'\"  # records what was SENT; never decides a skip" \
    '' \
    "  finally: upload the manifest -> $REMOTE_DIR/.lane_transfer/manifest.tsv" \
    "  manifest written for this dry run: $MANIFEST ($_mfiles files, $_mbytes bytes)" \
    '  NOTE: an interrupted chunk is NOT resumable mid-tar; the next run re-sends' \
    '        that whole chunk. A chunk is skipped only after the remote files behind it are hashed.' \
    '  NOTE: that observation re-reads the tree on the VPS every run - minutes for a full-size' \
    '        build of local disk, still cheaper than streaming it over ssh again, and the only' \
    '        way the skip line rests on a look at the tree instead of on what this script remembers.' \
    '  NOTE: this script issues no remote deletions, ever.'
  exit 0
fi

# ---------------------------- real run ----------------------------
say "target: $SSH_TARGET:$REMOTE_DIR (transport: $TRANSFER_TRANSPORT, compress: $COMPRESS)"
run_remote "mkdir -p '$REMOTE_DIR/.lane_transfer'" || die "remote mkdir -p '$REMOTE_DIR/.lane_transfer' failed"
: > "$CHUNK_HASHES"

# A chunk may be skipped only on what the remote tree holds NOW - an observation - never on what a
# past run of this script wrote down. The block this replaces (transfer_build.sh:284-299 before the
# fix) read $REMOTE_DIR/.lane_transfer/chunk_hashes and grepped it for "<tar-sha256>\t<name>". That
# ledger is append-only history: the line proves a tar was streamed here once, never that its files
# survived there afterwards, and nothing in the kit ever rewrites or prunes it (the only truncation
# is the LOCAL copy's ': > "$CHUNK_HASHES"' above). Rebuild to B, transfer, revert the build to A
# (or share one STAGE_DIR across two builds): the run for A read A's old line out of the history,
# printed "already complete on the remote", sent 0 bytes, and left B's bytes in the tree the lane
# would serve - the exact 2026-11-08 checkup problem 20/48, and checkup hypothesis (e) of 2026-11-06
# in weaker form (a partial REMOTE_DIR reported complete). So the skip decision now reads the tree.
# Both sides fold a chunk's per-file sha256 lines - path-sorted, normalised to "64hex  ./path", the
# same line shape the manifest join parses with substr($0,67) - into one digest, and both folds cover
# exactly the same set of files: the manifest's lines for the chunk on this side, and of the lines the
# remote returns, only those whose path the manifest lists for that chunk. That scoping is the whole of
# this pass. What it replaces folded the remote's WHOLE subtree ('find ./Build -type f' -> sha256sum ->
# sha256sum) against a fold of the manifest's files, so one file under the chunk path that this build
# does not list - an output name the previous build used, something an upstream step dropped - held the
# two digests apart forever: the send line blamed "other bytes or absent", neither of which was true, the
# chunk re-sent whole on every run of a development loop (Build/ is the multi-GB chunk), and because this
# script issues no remote deletions and a tar extraction cannot remove a file, no number of reruns could
# converge it. Surplus remote bytes are therefore not a reason to re-send; they are verify_transfer.sh's
# EXTRA to report to whoever maintains the tree. The local fold comes from the manifest this run just
# built, so it is over file BYTES, not the compressed container: a gzip header timestamp cannot make a
# rerun look different (the payload-fold answer to the reproducibility problem the README's 'skip
# verified' claim always leaned on). The remote lines come from the live files. A chunk the remote has
# pruned lists nothing, so its fold is the empty-input digest, which equals no real payload, so a chunk
# the ledger remembers but the tree lost re-sends; every observation failure therefore lands on the send
# side, never on a false skip. Cost is stated where it is paid: the observation re-reads the tree on the
# VPS each run (sha256 of every file) and returns one line per file, minutes for a 5.47 GB tree of local
# disk - cheaper than streaming it over ssh again, and the price of the claim.

payload_lines_build() {                     # chunk -> this build's lines for it: "file-sha256  ./path", path order
  if [ "$1" = '_root_files_' ]; then
    awk -F'\t' '
      NR==FNR { want[$0] = 1; next }
      ($3 in want) { printf "%s  %s\n", $1, $3 }
    ' "$_rootfiles" "$MANIFEST"
  else
    awk -F'\t' -v p="./$1/" 'index($3, p) == 1 { printf "%s  %s\n", $1, $3 }' "$MANIFEST"
  fi
}

remote_chunk_lines() {                      # chunk -> "file-sha256  ./path" for every file the remote has behind it
  if [ "$1" = '_root_files_' ]; then
    _paths=''
    while IFS= read -r _rf; do _paths="$_paths '$_rf'"; done < "$_rootfiles"
  else
    _paths="'./$1'"
  fi
  # Read-only: cd/find/sort/xargs/sed/sha256sum - no deletion, no write. And deliberately NOT folded on
  # the far side: the set to compare is the manifest's, not the subtree's, so the fold has to wait for
  # the scope step below. 'xargs -0 -r' because a pruned chunk must list NOTHING, not hang: bare
  # 'xargs -0 sha256sum' runs sha256sum with no operands on an empty list and it sits reading the ssh
  # session's stdin (the verify_transfer.sh empty-tree hang class, checkup problem 53/64). The sed
  # rewrites the two bytes after each hash to "  " so a sha256sum that marks its lines binary
  # ("hash *path", Git-for-Windows) and one that leaves them text ("hash  path", Ubuntu) return the same
  # lines - the comparison must not depend on which box hashed the files.
  run_remote "cd '$REMOTE_DIR' && find $_paths -type f -print0 | LC_ALL=C sort -z | \
xargs -0 -r sha256sum | LC_ALL=C sed 's/^\\(.\\{64\\}\\)../\\1  /'"
}

chunk_scope() {                             # build-lines file, remote-lines file -> "seen differ"; scoped lines -> $3
  awk -v out="$3" '
    NR==FNR { want[substr($0, 67)] = substr($0, 1, 64); next }
    {
      p = substr($0, 67)
      if (!(p in want)) next     # not a file this build lists: not bytes of this chunk, so not comparable
      print > out
      seen++
      if (want[p] != substr($0, 1, 64)) differ++
    }
    END { print seen+0, differ+0 }
  ' "$1" "$2"
}

sent=0 skipped=0 bytes_sent=0
_build_lines=$STAGE_DIR/_chunk_build_lines.txt
_remote_lines=$STAGE_DIR/_chunk_remote_lines.txt
_scoped_lines=$STAGE_DIR/_chunk_scoped_lines.txt
while IFS= read -r _cn; do
  _tn=$(tar_name "$_cn")
  payload_lines_build "$_cn" > "$_build_lines"
  _cnfiles=$(awk 'END { print NR+0 }' "$_build_lines")
  _ph=$(sha256sum < "$_build_lines" | cut -d' ' -f1)
  remote_chunk_lines "$_cn" > "$_remote_lines" || \
    die "cannot observe the remote copy of chunk '$_cn' via $TRANSFER_TRANSPORT: refusing to claim anything about it - a chunk is skipped only when its bytes on the remote were actually looked at"
  : > "$_scoped_lines"
  _scope=$(chunk_scope "$_build_lines" "$_remote_lines" "$_scoped_lines") || \
    die "could not scope the remote listing of chunk '$_cn' against this build's manifest - refusing to guess: a chunk is skipped only when its bytes on the remote were actually looked at"
  set -- $_scope
  _rseen=$1 _rdiff=$2
  _rh=$(sha256sum < "$_scoped_lines" | cut -d' ' -f1)
  rm -f -- "$_build_lines" "$_remote_lines" "$_scoped_lines"
  if [ "$_ph" = "$_rh" ]; then
    say "skip  $_cn ($_tn): the remote holds all $_cnfiles files this chunk's manifest lists, hashing to payload sha256 $_ph - just observed on the remote tree, not the sent-ledger"
    skipped=$((skipped+1))
    continue
  fi
  stage_chunk "$_cn"
  _hash=$(sha256sum "$STAGE_DIR/$_tn" | cut -d' ' -f1)
  printf '%s\t%s\n' "$_hash" "$_tn" >> "$CHUNK_HASHES"
  set -- $(chunk_stats "$_cn")
  say "send  $_cn: files=$1 bytes=$2 tar=$_tn sha256=$_hash (of the $_cnfiles files this chunk's manifest lists the remote holds $_rseen, $_rdiff of them with other bytes -> payload fold $_rh differs from build payload $_ph; a file the build no longer lists is not counted here - this script never deletes remote files, and verify_transfer.sh reports those as EXTRA)"
  if ! "$TRANSFER_TRANSPORT" "$SSH_TARGET" "tar $TAR_FLAG -xf - -C '$REMOTE_DIR'" < "$STAGE_DIR/$_tn"; then
    die "chunk '$_cn' failed mid-transfer. A partial chunk cannot be resumed: the next run re-sends it whole. Nothing was deleted remote."
  fi
  if ! printf '%s\t%s\n' "$_hash" "$_tn" | run_remote "cat >> '$REMOTE_DIR/.lane_transfer/chunk_hashes'"; then
    die "chunk '$_cn' landed on the remote, but its sent-ledger line could not be appended: the bytes are there (the next run will observe them and skip), only the what-was-sent record is missing"
  fi
  sent=$((sent+1))
  bytes_sent=$((bytes_sent+$2))
done < <(chunk_names)

# 'transfer done' has to mean chunks were looked at. The empty-tree refusal above makes that unreachable for
# a build that was already empty when it was read, so this can only fire on the other way the count can come
# out zero: the manifest, the chunk list and the top-level file list are three separate reads of a tree a
# build job may still be writing, and a run that hashed files the chunk list no longer finds watched a tree
# change underneath it. That is a refusal, not a transfer of nothing - and it comes before the manifest
# upload, because a manifest describing bytes this run never sent is the same lie in a different file.
if [ $((sent + skipped)) -eq 0 ]; then
  die "looked at 0 chunks against a manifest of $_mfiles files - refused, no 'transfer done' line and no manifest upload: the build tree changed while this script was reading it (re-run it once the build is finished)"
fi

if ! "$TRANSFER_TRANSPORT" "$SSH_TARGET" "cat > '$REMOTE_DIR/.lane_transfer/manifest.tsv'" < "$MANIFEST"; then
  die "upload of manifest.tsv to the remote bookkeeping dir failed"
fi

say "transfer done: $sent chunk(s) sent ($bytes_sent payload bytes moved), $skipped chunk(s) skipped on an observation of the remote tree"
if [ $sent -eq 0 ] && [ $skipped -gt 0 ]; then
  say "nothing was re-sent: 0 bytes went over the transport - every skipped chunk's payload was hashed on the remote this run and matched this build"
fi
say "local manifest: $_mfiles files, $_mbytes bytes (see $MANIFEST)"
say "confirm it for real with: VERIFY_TRANSPORT=... scripts/linux/verify_transfer.sh"

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
# And the look has to have happened. Every remote observation is a shell pipeline, and ssh answers
# with the exit status of its LAST stage - before, a far-side find error or a killed sha256sum came
# back exit 0 with a short or empty listing (the live promotion log carries an unattributed
# 'find: ./Build: No such file or directory' between send lines over a clean exit), the
# 'refusing to claim' guard could not fire, and send/skip lines quoted counts for an observation
# that had errored. Each listing therefore now runs on the far side under 'bash -c' with
# 'set -o pipefail', and a listing that completes closes with a per-run observation-proof trailer
# line; a far-side failure or a missing/misplaced/duplicated trailer refuses the chunk instead of
# claiming a tree state it never saw (2026-11-10 checkup, dead remote guards).
#
# And the run that looked at every chunk has to say what every chunk was. The enumeration that feeds the chunk
# loop is counted once ($_expected_chunks) and the done line states sent + skipped against that total: a rerun
# is convergence only when the two agree, because a loop cut short after 1 of 4 chunks closed with '0 sent, 1
# skipped' and exit 0 - shape-identical to the converged '4 skipped' the promotion proof was waiting for (the
# 2026-11-09 D759A911DF0C promotion log; the stdin closure in remote_chunk_lines removed that particular cut,
# the count is what refuses ANY cut, and a mismatch exits nonzero with no done line and no manifest upload).
# The same done output states the surplus the scoped fold collected: remote files under the observed chunk
# paths that this build's manifest does not list stay out of the skip fold on purpose (the EXTRA paragraph
# above), but they are no longer invisible in this script's own output - the count is its share of
# verify_transfer.sh's EXTRA, and a converged rerun over a tree still holding a dropped name now says so.
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

# The denominator of every claim made at the close of a run: exactly how many chunk names the enumeration
# above feeds the chunk loop. Counted once here - at the enumeration itself - so the guard after the loop
# weighs what was looked at against what there was to look at, and the done line can state both numbers:
# '0 sent, 1 skipped' and '4 skipped' are only ever shapes until something prints how many there were.
_expected_chunks=$(chunk_names | awk 'END { print NR+0 }')

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

# Per-run observation-proof token. A listing line is '<64hex>  <path>', so the first 64 bytes of any
# real listing data are hex; this token starts with letters, and its 40 hex bytes are drawn per run,
# so a trailer inside listing data cannot be confused with a different build's or run's trailer, and
# the guard below can tell 'listing complete' from 'listing cut short' without trusting the exit
# status of a transport that just promised it was fine. Defined before --dry-run because the plan a
# dry run prints must be the exact command a real run of this invocation would execute.
_OBS_OK="MFG_OBS_OK_$( { printf '%s %s %s %s %s' "$$" "$PPID" "$RANDOM" "$RANDOM" "$(date +%s 2>/dev/null || echo 0)"; } | sha256sum | cut -c1-40 )"

remote_obs_command() {                     # chunk -> the EXACT far-side command run for it (--dry-run prints this)
  if [ "$1" = '_root_files_' ]; then
    _paths=''
    while IFS= read -r _rf; do _paths="$_paths '$_rf'"; done < "$_rootfiles"
  else
    _paths="'./$1'"
  fi
  # Read-only: cd/test/printf/find/sort/xargs/sed/sha256sum - no deletion, no write. And deliberately
  # NOT folded on the far side: the set to compare is the manifest's, not the subtree's, so the fold
  # has to wait for the scope step below. 'xargs -0 -r' because a pruned chunk must list NOTHING, not
  # hang: bare 'xargs -0 sha256sum' runs sha256sum with no operands on an empty list and it sits
  # reading the ssh session's stdin (the verify_transfer.sh empty-tree hang class, checkup problem
  # 53/64). The sed rewrites the two bytes after each hash to "  " so a sha256sum that marks its
  # lines binary ("hash *path", Git-for-Windows) and one that leaves them text ("hash  path", Ubuntu)
  # return the same lines - the comparison must not depend on which box hashed the files.
  # A chunk the remote tree does not hold yet is not an error: it is the normal first-transfer state,
  # and it must answer 'nothing here, send it'. So each chunk path is tested for existence on the far
  # side FIRST and only the paths that exist are handed to find; an absent operand lists empty
  # (exactly as the masked old form answered, but without find's stderr error line riding through the
  # log unattributed). What still fails the listing - and after pipefail, fails it LOUDLY, where
  # before a successful last stage hid it - is a path that exists and then breaks the walk: cd,
  # find, sort, xargs, sha256sum or sed failing mid-observation on bytes they were asked to read.
  # '-I{}' is what puts each path BEFORE find's expression - xargs appends its operands at the END
  # of the command line by default, and GNU find refuses 'find -type f -print0 ./Build'.
  # The whole listing runs under 'bash -c' with 'set -o pipefail': ssh (or the shim) reports the
  # wrapper's status, and pipefail makes that status the first failed stage's instead of sed's -
  # without it, a far-side find error or killed sha256sum answers exit 0 behind a happy sed and the
  # guard at the call site is dead code. A listing that completes appends this run's trailer line as
  # its LAST line; a far-side failure never prints it, and obs_require below refuses a listing whose
  # trailer is missing, misplaced, or present inside the listing data.
  _inner="set -o pipefail; cd '$REMOTE_DIR' && for _p in $_paths; do if [ -e \"\$_p\" ]; then printf '%s\\0' \"\$_p\"; fi; \
done | xargs -0 -r -I{} find {} -type f -print0 | LC_ALL=C sort -z | xargs -0 -r sha256sum | LC_ALL=C sed 's/^\\(.\\{64\\}\\)../\\1  /' && printf '%s\\n' '$_OBS_OK'"
  # Re-quote every single quote in the inner command as the standard '\'' so the outer single-quoted
  # bash -c argument reassembles it byte for byte at the remote (or shim) shell - the path guard above
  # is what makes the inner command's own single quotes safe to nest.
  printf "bash -c '%s'" "${_inner//\'/\'\\\'\'}"
}

obs_require() {                            # chunk, lines-file -> strip this run's trailer line, or refuse the observation
  _or_n=$(grep -Fxc -- "$_OBS_OK" "$2" 2>/dev/null || true)
  _or_n=${_or_n:-0}
  if [ "$_or_n" -eq 0 ]; then
    die "cannot observe the remote copy of chunk '$1': the listing arrived without this run's observation-proof trailer ($_OBS_OK) as its last line - the transport answered success but the line a completed far-side listing always appends never came, so this is an observation that cannot prove it finished: refusing to claim anything about the chunk (a chunk is skipped only when its bytes on the remote were actually looked at)"
  fi
  if [ "$_or_n" -gt 1 ]; then
    die "cannot observe the remote copy of chunk '$1': this run's observation-proof trailer ($_OBS_OK) appears $_or_n times in the listing - a trailer inside the listing data means the stream is not a listing this script can tell apart from corruption, so the observation is refused rather than guessed at"
  fi
  if [ "$(tail -n 1 -- "$2")" != "$_OBS_OK" ]; then
    die "cannot observe the remote copy of chunk '$1': the observation-proof trailer is present but is not the LAST line of the listing - bytes were cut or inserted after it, so the listing cannot be trusted as an observation of the remote tree"
  fi
  sed '$d' "$2" > "$2.obs_stripped" && mv -f -- "$2.obs_stripped" "$2"
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
  printf '  chunks enumerated: %s (a real run states this total in its done line and refuses to close having looked at fewer)\n' "$_expected_chunks"
  printf '%s\n' \
    '' \
    '  per chunk, a real run does:' \
    "    payload digest = sha256 over this chunk's manifest lines written as '<file-sha256>  <./path>' (path order)" \
    "    ssh \"\$SSH_TARGET\" \"$(remote_obs_command '<chunk>')\"  > per-file lines the remote tree holds under ./<chunk>" \
    '      -> the remote lists each file behind ./<chunk> as one "<sha256>  ./<path>" line (read-only: cd/test/find/sort/xargs/sed/sha256sum),' \
    "         under 'bash -c' with set -o pipefail so a far-side cd/find/sort/xargs/sed failure IS the listing's exit status (ssh only ever" \
    "         reports the LAST pipeline stage's, and that stage cannot fail just because find did), and closed by this run's observation-proof" \
    "         trailer '$_OBS_OK' as the last line: a failed far-side pipeline never prints it, and a listing without it - or with it inside the" \
    '         listing data - is refused, never read as a tree state' \
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
  # The transport call runs INSIDE the chunk loop, whose stdin is the chunk list: an ssh with an
  # inherited stdin reads that list to the end and closes it, so the loop died after the first
  # chunk and every real run shipped 1 of 4 chunks under a clean 'transfer done' (observed
  # 2026-11-09 promoting D759A911DF0C; the ssh-shim fixtures never caught it - a bash shim
  # doesn't consume stdin the way ssh does). Hold the transport's stdin shut.
  run_remote "$(remote_obs_command "$1")" </dev/null
}

chunk_scope() {                             # build-lines file, remote-lines file -> "seen differ surplus"; scoped lines -> $3
  awk -v out="$3" '
    NR==FNR { want[substr($0, 67)] = substr($0, 1, 64); next }
    {
      p = substr($0, 67)
      if (!(p in want)) { surplus++; next }   # not a file this build lists: not bytes of this chunk, so not comparable for the fold - but it IS surplus, and the count is kept for the done output (this script shares the EXTRA of verify_transfer.sh)
      print > out
      seen++
      if (want[p] != substr($0, 1, 64)) differ++
    }
    END { print seen+0, differ+0, surplus+0 }
  ' "$1" "$2"
}

sent=0 skipped=0 bytes_sent=0 _surplus_total=0
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
  obs_require "$_cn" "$_remote_lines"
  : > "$_scoped_lines"
  _scope=$(chunk_scope "$_build_lines" "$_remote_lines" "$_scoped_lines") || \
    die "could not scope the remote listing of chunk '$_cn' against this build's manifest - refusing to guess: a chunk is skipped only when its bytes on the remote were actually looked at"
  set -- $_scope
  _rseen=$1 _rdiff=$2 _rsurp=${3:-0}
  _surplus_total=$((_surplus_total + _rsurp))
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

# The zero check above catches the run that looked at nothing; it cannot catch the run that looked at almost
# everything. The 2026-11-09 D759A911DF0C promotion log is the shape: the chunk loop died after the first
# chunk (its transport read the loop's stdin - see remote_chunk_lines) and the script closed with
# 'transfer done: 0 chunk(s) sent ..., 1 chunk(s) skipped' and exit 0, indistinguishable by line shape from
# the converged '4 chunk(s) skipped' the promotion proof was waiting for - the chunk list was enumerated,
# but nothing had ever counted it against sent+skipped. The stdin closure removed that cut; this count
# refuses ANY cut, whenever or however a future one appears: looked-at must equal the number of chunk names
# the enumeration fed the loop. Like the zero check it sits before the manifest upload - a manifest closed
# out by a run that did not look at every chunk is the same lie in a different file.
if [ $((sent + skipped)) -ne "$_expected_chunks" ]; then
  die "looked at $((sent + skipped)) of $_expected_chunks chunks - refused, no 'transfer done' line and no manifest upload: the chunk list enumerated $_expected_chunks chunk(s) and this run sent $sent + skipped $skipped = $((sent + skipped)) of them, so it ended before every chunk had been looked at; a truncated run is neither a transfer nor a convergence (find why the run ended early, then re-run it - the next run re-observes every chunk from the tree)"
fi

if ! "$TRANSFER_TRANSPORT" "$SSH_TARGET" "cat > '$REMOTE_DIR/.lane_transfer/manifest.tsv'" < "$MANIFEST"; then
  die "upload of manifest.tsv to the remote bookkeeping dir failed"
fi

say "transfer done: $sent chunk(s) sent ($bytes_sent payload bytes moved), $skipped chunk(s) skipped on an observation of the remote tree - $sent sent + $skipped skipped = $((sent + skipped)) of $_expected_chunks chunk(s) enumerated, every one of them looked at"
# The second number the promotion proof needs beside the first: the skip fold scoped the remote listing to
# this build's manifest on purpose (header, EXTRA paragraph), so surplus remote bytes never block a skip -
# but the fold was also THROWING the surplus count away, and a converged '0 sent, N skipped' printed exit 0
# over a remote that still held ./Build/webgl.oldname.wasm, the name a previous build used: the promotion
# said converged, only verify_transfer.sh's EXTRA said otherwise, on the same tree. chunk_scope's third
# number now keeps the count the fold was already seeing; the statement prints on every done output - 0 is
# the shape a converged tree has, nonzero names the condition only the tree maintainer (and
# verify_transfer.sh's whole-tree scan, which also sees surplus the per-chunk observations never enumerate)
# can clear, since this script issues no remote deletions.
say "surplus: $_surplus_total remote file(s) under the observed chunk paths that this build's manifest does not list - verify_transfer.sh reports these as EXTRA; the skip ignores them by design and this script never deletes remote files, so a nonzero count is a name the build dropped that no rerun of this script will clear"
if [ $sent -eq 0 ] && [ $skipped -gt 0 ]; then
  say "nothing was re-sent: 0 bytes went over the transport - every skipped chunk's payload was hashed on the remote this run and matched this build"
fi
say "local manifest: $_mfiles files, $_mbytes bytes (see $MANIFEST)"
say "confirm it for real with: VERIFY_TRANSPORT=... scripts/linux/verify_transfer.sh"

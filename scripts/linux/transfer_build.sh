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
# The skip is an observation of the tree, never a reading of the remote sent-ledger (chunk_hashes):
# that ledger is append-only history of what was SENT, and a chunk whose bytes were
# later replaced on the remote read its old ledger line back forever and was reported
# complete while the tree held another build (2026-11-08 checkup, problem 20/48 - see
# the real-run block for the guard this replaced). A chunked tar cannot resume a partial
# chunk; it can only skip ones whose bytes it just looked at.
#
# And a chunk name has to mean one part of the tree. The top-level-files chunk is named by the reserved
# string '_root_files_', and every per-chunk helper resolves a chunk by that string, so a real top-level
# DIRECTORY of that name would BE that chunk: the loop would ask for the loose files twice and never for
# the directory, its files would enter no chunk, no tar and no send, the manifest this run publishes would
# still list them as part of the build, and the done line would state a chunk count that adds up - the
# denominator guard cannot see it, because the colliding name is enumerated AND iterated (2026-11-10
# checkup: '_root_files_/payload.bin' never reached the remote, 'transfer done: 3 chunk(s) sent ... 4 of 4
# chunk(s) enumerated, every one of them looked at', exit 0; verify_transfer.sh's 'MISSING' was the only
# thing that said otherwise, on a tree the swap goes on to promote). The name is refused at the chunk list,
# in --dry-run as well, before a chunk is staged or a transport command runs: rename the directory or move
# it out of the tree's top level.
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
# 2026-11-09 D759A911DF0C promotion log; the stdin closure in remote_chunk_lines removed that particular cut -
# every read-only transport call holds its stdin shut now (the mkdir, the existence test, the observations, both probes, the manifest
# rename) and only the three calls handed a payload keep it (the tar send, the sent-ledger and the manifest pipe)
# - the count is what refuses ANY cut, and a mismatch exits nonzero with no done line and no manifest upload).
# The same done output states the surplus the scoped fold collected: remote files under the observed chunk
# paths that this build's manifest does not list stay out of the skip fold on purpose (the EXTRA paragraph
# above), but they are no longer invisible in this script's own output - the count is its share of
# verify_transfer.sh's EXTRA, and a converged rerun over a tree still holding a dropped name now says so.
#
# And the run has to know both ends can hold the tree BEFORE it streams. 'df' appeared nowhere in this script, so a real
# run's first capacity observation was the write that fails - mid-chunk, after every chunk before it had landed: the
# 2026-11-10 promotion hit 1.2 GB free on the filesystem the gate serves (the same one the staged tree sits beside; the
# documented full-transfer path no longer fits its 4 GB-free box with a 9,038,740,287-byte build), and a tar extraction
# that fills the filesystem is not transactional - it leaves the destination a mix of this build's bytes and whatever
# the half-extracted chunk left behind, which only a full re-send un-mixes. The manifest already totals the tree
# ($_mbytes); the gate compares that total against the free space read from both ends before the first byte moves -
# STAGE_DIR's filesystem locally ('df -B1 --output=avail', the one the chunk tars stage on) and REMOTE_DIR's on the
# target (the same probe through $TRANSFER_TRANSPORT; a first transfer has no REMOTE_DIR yet, so the far side walks up
# to the nearest existing ancestor - same filesystem, an operand df accepts). Either answer below the total is a refusal
# naming both numbers; either answer unreadable or not-a-number is a refusal too - guessing at capacity is what filled
# the disk. The threshold is deliberately the uncompressed manifest total rather than what this run will probably send:
# skipped chunks and gzip make the real figure smaller, but a premature refusal is one rerun while a half-written
# promotion is a corrupt destination. --skip-space-check / SKIP_SPACE_CHECK=1 opts out for the operator who measured the
# fit by hand (the honest case: a converged rerun that sends 0 bytes over a nearly-full target fs).
# One thing that denominator cannot see is hardlinks. A delta refresh ships into a tree staged as `cp -al` of the tree
# already on the box, so most of the manifest's bytes are resident under REMOTE_DIR before a byte moves and the gate
# refuses a run whose real cost is the delta. The measurement that settles it is df before and after, never du: 'du -sh'
# on the live tree and on the handle, one tree per call, reports two full trees (it folds hardlinks only within one
# invocation) and 0 for the handle in a single call naming both, so the figure it prints follows the command line, not
# the disk - 'df -B1 --output=avail' before the cp -al and after the delta lands is the answer. That measured fit is
# the --skip-space-check case; the README's swap section carries the rule with the numbers.
#
# And the bookkeeping itself must not sit inside the served tree. The location this replaces put manifest.tsv and the
# chunk_hashes ledger at $REMOTE_DIR/.lane_transfer/ - $REMOTE_DIR is the document root the front serves, so that was
# two public files inside it: a GET of /.lane_transfer/manifest.tsv on the live lane answered 200 with 13,126,733
# bytes, the full 58,809-path asset inventory of the production tree (hashes included), until the directory was moved
# off the live tree by hand. The path is now a knob, $BOOKKEEPING_DIR, defaulted OUTSIDE REMOTE_DIR as a leading-dot
# sibling (/srv/mfg/webroot -> /srv/mfg/.webroot.lane_transfer), and any value that resolves to REMOTE_DIR or under it
# - the old location included - is refused before anything is staged or sent. The dot-name is not the protection:
# webfront's ServeStatic (webfront/Program.cs:1041) confines opens to the root but never rejects a path segment
# starting with '.', so a bookkeeping dir NAMED '.lane_transfer' under the root is served exactly like any other file
# (see the README follow-up). Being outside the served tree is the protection. verify_transfer.sh takes the same
# $BOOKKEEPING_DIR with the same default and the same refusal, so neither script ever works a location the other
# would not.
#
# And the destination has to be the tree the operator meant, not one this run invented. The bookkeeping mkdir is
# 'mkdir -p', which creates every missing parent - while the bookkeeping sat under REMOTE_DIR, that single command
# created REMOTE_DIR too, so a REMOTE_DIR typo one letter off the real path passed
# every guard (it is absolute, it is not a system directory, it reads plausible), was SILENTLY CREATED by the run meant
# to fill the real tree, shipped every chunk into it, closed with a clean done line, and verify_transfer.sh then agreed
# about a tree that had existed for ninety seconds: the two-fold comparison compares manifest against whatever is at
# REMOTE_DIR, so agreement about the run's own typo is guaranteed, and no line in the log said the destination had
# been made (2026-11-10 checkup). A real run now asks the target 'is $REMOTE_DIR a directory' - one read-only,
# stdin-shut transport call before the first write - and 'absent' is a refusal naming the path and the way to mean it:
# --create / CREATE_REMOTE_DIR=1, under which the destination gets its own loud CREATION line and its own mkdir
# (separate from the bookkeeping mkdir, which since the move above can no longer reach REMOTE_DIR at all).
# An answer the transport cannot
# give at all (a dead link) is its own refusal: an existence this script cannot read is an existence it will not
# write into. --dry-run dispatches nothing and prints the probe instead.
#
# And what a run writes down about its own sends has to be true. $STAGE_DIR/chunk_hashes.tsv is history of what was
# SENT, and it is read by nothing: the skip decision is an observation of the remote tree (above), and verify_transfer.sh
# and swap_webroot.sh read manifest.tsv only. Two things made it lie. Its line was appended BEFORE the transport call, at
# the moment of intent, so a chunk whose stream died was recorded as sent on the machine that staged the tar - a mid-tar
# death closed exit 1 with a local ledger naming the dead chunk's tar, a remote ledger that never saw that line, and a
# tree holding only the chunks that had actually landed. And the file was truncated at run start (': > "$CHUNK_HASHES"'
# where the ledger is opened now), so the history it exists to be was destroyed by every rerun before that rerun had sent
# a byte: the run that finally shipped the interrupted chunk wiped the honest record of the chunks that HAD landed, and
# the local and remote ledgers were left permanently disagreeing about what shipped. So the local line is appended only
# after the tar streamed to the end AND the remote acknowledged its copy of the line - the two ledgers can then only agree
# or be missing the same line, never claim different ones - and nothing truncates the file: every line carries a per-run
# tag as its first field, '<UTC-stamp>\t<tar-sha256>\t<tar-name>', so runs stack up as one history instead of the last run
# replacing them. The file must stay history-only: no decision in this script reads it, and none may start to.
#
# And what a run leaves in the bookkeeping dir has to be whole. $BOOKKEEPING_DIR/manifest.tsv is the file the
# readers after this run treat as truth: the KB delta procedure diffs against it and verify_transfer.sh agrees with
# whatever sits at that path. The form this replaces shipped it with 'cat >' straight onto that path - a write that
# TRUNCATES the file on the far side before a single byte arrives, streams into it, and leaves the local side watching
# only the transport's exit status, a status that cannot see a cut stream (the same class the pipefail pass killed for
# the listings, never once asked of this write): an offline repro cut the upload to 10 bytes of a 337-byte manifest,
# the transport answered 0, 'transfer done:' printed, and the truth path held 10 bytes of one manifest line - which
# verify_transfer.sh then agreed with, because agreement between two files says nothing about the bytes a third held.
# So the upload now lands in manifest.tsv.tmp, the landed copy is measured ON the target ('bash -c' with 'set -o
# pipefail': wc -c and sha256sum; the answer is re-read HERE and compared against the local file this script wrote -
# the expected numbers never leave this machine, so a short or mangled answer lands on the refusal side wherever it
# was cut), and only a byte-for-byte AND hash-equal tmp is renamed onto the manifest path. A rename inside
# the bookkeeping dir is atomic, so a reader ever sees the previous whole manifest or this run's whole manifest, never a
# half-written one, and a cut or corrupted stream can only ever refuse: the canonical path is not opened for writing
# until a verified complete copy of this run's manifest already exists beside it. A stale manifest.tsv.tmp of an
# interrupted run is truncated by the next upload; nothing reads it, and verify_transfer.sh's tree scan already
# excludes the in-tree '.lane_transfer' leftovers of the trees this bookkeeping move replaced.
#
# The script never deletes anything remote - but it does overwrite, and overwriting needs no
# delete to destroy bytes: every chunk lands as 'tar -xf - -C REMOTE_DIR', replacing each file
# the stream reaches as it arrives. A run that completes is a convergence - every file this
# build's manifest lists ends up holding exactly this build's bytes. A run that dies mid-chunk
# is NOT a no-op: REMOTE_DIR is then a MIXED tree - the members reached hold this build's bytes
# and the bytes they held are gone, the member being written is left partial, everything not
# yet reached still holds the previous tree's bytes - and if that path is being served, the mix
# is what is being served (2026-11-10 lab: died mid chunk-2, game.js new / blob truncated 999424
# of 2000000 / index.html old). The two recoveries are completing a full transfer into the same
# REMOTE_DIR or restoring the operator's aside (the swapped-out old tree); the failure line at
# the send site carries the same statement. Beyond that: the script issues no rm/unlink/truncate
# outside its own bookkeeping dir, and refuses obviously dangerous REMOTE_DIR values
# outright. And it refuses a REMOTE_DIR that does not EXIST on the target yet unless --create is
# passed: while the bookkeeping sat inside the tree, its mkdir -p silently CREATED whatever
# REMOTE_DIR it was handed - a wrong-but-plausible path, a typo one letter off passes every guard
# above - the run shipped the build into the seconds-old typo, the done line and verify_transfer.sh
# both agreed about it, and no line in the log ever said the destination had
# been created. With --create the run creates REMOTE_DIR itself (its own mkdir, behind a loud
# CREATION line naming the path), so the creation sits in the log that records the promotion.
#
# Usage
# -----
#   scripts/linux/transfer_build.sh [--dry-run] [--keep-tars] [--skip-space-check] [--create] [-h|--help]
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
#                       A real run additionally requires the normalised path to EXIST on the target:
#                       before anything is created or written there it asks the target ('test -d',
#                       read-only, stdin held shut) and an absent answer is a refusal. That gate is
#                       the whole protection against a plausible typo: the bookkeeping mkdir is
#                       mkdir -p, so the run itself used to CREATE whatever REMOTE_DIR it was handed
#                       (2026-11-10 checkup: the typo'd tree converged, 'verified', and nothing in
#                       the log said it had been made). --create / CREATE_REMOTE_DIR=1 opts in to
#                       creating a first-transfer destination, with a loud CREATION line first.
#   LOCAL_BUILD_DIR     Unity WebGL output tree.  default: ~/mfg/out/webgl
#                       (same default as assemble_webroot.sh:26; contains index.html,
#                        Build/, StreamingAssets/, TemplateData/)
#                       Refused, in a real run and in --dry-run alike, before anything is staged or sent: a
#                       tree that holds no files. [ -d ] proves the directory exists, and an empty directory is
#                       precisely what a build cleaned out of the path - or renamed away from it, leaving the
#                       old name standing empty - leaves behind; shipping zero chunks used to close with
#                       'transfer done: 0 chunk(s) sent' and exit 0 (2026-11-08 checkup, problem 38/68).
#   STAGE_DIR           local staging for chunk tars + manifest. default: ~/mfgtmp/transfer_stage
#                       A chunk tar lives here only while it is useful: a real run deletes each tar as
#                       soon as its send is confirmed, and a run that completes sweeps the chunk tars
#                       earlier runs left behind - the tar of a chunk that now skips, or of a top-level
#                       name the build renamed or dropped (an old chunk__<name>.tar* was named by no run
#                       after the rename, so nothing ever removed it: every promotion left a second full
#                       copy of the tree in STAGE_DIR for an operator to delete by hand). The peak is
#                       now one chunk, not the whole build. A send that died mid-transfer keeps its tar;
#                       the documented rerun re-stages it anyway. manifest.tsv is bookkeeping
#                       (verify_transfer.sh and swap_webroot.sh read it) and chunk_hashes.tsv is the
#                       append-only history of confirmed sends (read by nothing); neither is ever swept.
#   KEEP_TARS           1, or the --keep-tars flag: delete no chunk tar - neither after a confirmed send
#                       nor at the end-of-run sweep. Forensics by request: this puts the second full copy
#                       of the tree back on disk, and the output says so on every send and at the close.
#   SKIP_SPACE_CHECK    1, or the --skip-space-check flag: stream without the up-front capacity gate. A real
#                       run otherwise reads the free space of both filesystems ('df -B1 --output=avail':
#                       the one holding STAGE_DIR, and the one holding REMOTE_DIR - or, on a first transfer
#                       with no REMOTE_DIR yet, that path's nearest existing ancestor) before the first byte
#                       moves, and refuses naming both numbers when either holds less than the manifest's
#                       total bytes (an upper bound: skipped chunks and gzip make what actually streams
#                       smaller, and for a delta refresh into a `cp -al` staged tree the manifest total
#                       over-counts what lands, because most of those bytes are already resident under
#                       REMOTE_DIR). Opt out only when you have measured the fit yourself, with df before
#                       and after, never du - `du -sh` prices a hardlink handle at the full tree, one tree
#                       per call, and at 0 in a single call naming both.
#   CREATE_REMOTE_DIR   1, or the --create flag: allow a real run to CREATE a REMOTE_DIR that does not
#                       exist on the target yet. Without it, before anything is created or written there
#                       the run asks the target whether REMOTE_DIR is a directory, and 'absent' (or an
#                       answer the transport could not give at all) is a refusal: the bookkeeping mkdir
#                       is mkdir -p, so without this gate a wrong-but-plausible path - every value the
#                       guard above accepts - would be created by the run meant to fill the real tree,
#                       the build would ship into the seconds-old typo, the done line would close, and
#                       verify_transfer.sh would agree about it, no line saying the destination was made.
#                       With --create the run prints a loud 'CREATION:' line naming the path right before
#                       it creates it, so the creation sits in the promotion log. A rerun once the tree
#                       exists needs no --create.
#   BOOKKEEPING_DIR     remote dir for the manifest publish + the remote sent-ledger.  default:
#                       REMOTE_DIR's leading-dot sibling - /srv/mfg/webroot -> /srv/mfg/.webroot.lane_transfer
#                       Deliberately OUTSIDE REMOTE_DIR: REMOTE_DIR is the document root the front
#                       serves, and the old fixed location under it ($REMOTE_DIR/.lane_transfer/) made
#                       the two bookkeeping files public - a GET /.lane_transfer/manifest.tsv on the
#                       live lane answered 200 with the whole 58,809-path production asset inventory
#                       (13 MB), until the dir was moved off the live tree by hand. Any value that
#                       normalises to REMOTE_DIR or a path inside it - the old location included - is
#                       refused before anything is staged or sent, in the dry run as well; the same
#                       absolute/quote/system-directory guard REMOTE_DIR gets applies here. The
#                       dot-name hides the dir from listings, it does NOT protect it: the gate's
#                       ServeStatic (webfront/Program.cs:1041) rejects nothing that starts with '.'
#                       (README follow-up) - being outside the served tree is the protection.
#                       verify_transfer.sh takes the same name, the same default and the same refusal.
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
#   $STAGE_DIR/chunk_hashes.tsv    run <TAB> chunk-tar-sha256 <TAB> chunk-tar-name   (what was SENT, across runs; a line
#                       is appended only after that chunk's tar streamed to the end and the remote ledger append of the
#                       same line succeeded, and the file is never truncated - local record of the remote ledger below)
#   $BOOKKEEPING_DIR/              manifest published there (uploaded to manifest.tsv.tmp, measured on the
#                       target, and renamed onto manifest.tsv only when bytes+sha256 match the
#                       local manifest) + remote sent-ledger appended, both by this script;
#                       the remote ledger is what-was-sent history and never decides a skip.
#                       Default: REMOTE_DIR's leading-dot sibling, OUTSIDE the served tree
#                       (the old $REMOTE_DIR/.lane_transfer/ location put both files in the document
#                       root - see BOOKKEEPING_DIR above; verify_transfer.sh reads the same location)
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
KEEP_TARS=${KEEP_TARS:-0}
SKIP_SPACE_CHECK=${SKIP_SPACE_CHECK:-0}
CREATE_REMOTE_DIR=${CREATE_REMOTE_DIR:-0}
BOOKKEEPING_DIR=${BOOKKEEPING_DIR:-''}
DRY_RUN=0

SCRIPT_PATH=${BASH_SOURCE[0]}

usage() {
  sed -n '2,/^set -euo/p' "$SCRIPT_PATH" | sed '$d'
}

die() { printf 'transfer_build: %s\n' "$1" >&2; exit 1; }
say() { printf 'transfer_build: %s\n' "$1"; }

while [ $# -gt 0 ]; do
  case $1 in
    --dry-run)   DRY_RUN=1 ;;
    --keep-tars) KEEP_TARS=1 ;;
    --skip-space-check) SKIP_SPACE_CHECK=1 ;;
    --create)    CREATE_REMOTE_DIR=1 ;;
    -h|--help)   usage; exit 0 ;;
    *)           die "unknown argument: $1" ;;
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

# The remote bookkeeping dir: manifest publish + remote sent-ledger live here, and the whole point of the variable is
# that they do NOT live inside REMOTE_DIR (header: the GET of /.lane_transfer/manifest.tsv that served 13 MB of asset
# inventory out of the served root). Default: a leading-dot SIBLING of REMOTE_DIR (/srv/mfg/webroot ->
# /srv/mfg/.webroot.lane_transfer) - outside the document root, hidden from plain listings. Guarded with the same
# rules as REMOTE_DIR because the same remote commands single-quote it, and refused whenever it normalises to
# REMOTE_DIR or under it - the old in-tree location included: a value under the served tree is a value being served.
_BK_PARENT=${REMOTE_DIR%/*}          # '' when REMOTE_DIR is one component under / (the default below then roots at /)
_BK_BASE=${REMOTE_DIR##*/}
_BK_DEFAULT="$_BK_PARENT/.${_BK_BASE}.lane_transfer"
if [ -z "$BOOKKEEPING_DIR" ]; then
  BOOKKEEPING_DIR=$_BK_DEFAULT
fi
case $BOOKKEEPING_DIR in
  /*) ;;
  *) die "BOOKKEEPING_DIR=$BOOKKEEPING_DIR is not an absolute path - refused, nothing staged or sent (like REMOTE_DIR it names a path on the far side: a relative value resolves against the ssh login cwd, which this script cannot see, and a leading ~ is never expanded inside the single-quoted remote commands, so it would make a directory literally named '~')" ;;
esac
case $BOOKKEEPING_DIR in
  *\'*) die "BOOKKEEPING_DIR=$BOOKKEEPING_DIR contains a single quote - refused, nothing staged or sent (every remote command wraps this path in single quotes, so a quote inside the value ends that quoting early: this guard would compare one string while the remote shell acted on another)" ;;
esac
BOOKKEEPING_DIR_RAW=$BOOKKEEPING_DIR
BOOKKEEPING_DIR=$(_norm_remote_dir "$BOOKKEEPING_DIR") || die "BOOKKEEPING_DIR=$BOOKKEEPING_DIR_RAW could not be normalised - refused, nothing staged or sent"
if [ "$BOOKKEEPING_DIR" != "$BOOKKEEPING_DIR_RAW" ]; then
  say "BOOKKEEPING_DIR=$BOOKKEEPING_DIR_RAW normalised to $BOOKKEEPING_DIR - the guard and every remote command use the normalised value"
fi
case $BOOKKEEPING_DIR in
  /) die "BOOKKEEPING_DIR=$BOOKKEEPING_DIR_RAW normalises to '/' - refused, nothing staged or sent (the manifest publish and sent-ledger would sit directly on the filesystem root, next to /etc and /bin)" ;;
esac
for _sys in bin boot dev etc lib proc root sbin sys usr var; do
  _sys=/$_sys
  case $BOOKKEEPING_DIR in
    "$_sys")   die "BOOKKEEPING_DIR=$BOOKKEEPING_DIR_RAW normalises to $_sys - refused, nothing staged or sent ($_sys is a system directory)" ;;
    "$_sys"/*) die "BOOKKEEPING_DIR=$BOOKKEEPING_DIR_RAW normalises to $BOOKKEEPING_DIR, inside the system directory $_sys - refused, nothing staged or sent" ;;
  esac
  case $_sys in
    "$BOOKKEEPING_DIR"/*) die "BOOKKEEPING_DIR=$BOOKKEEPING_DIR_RAW normalises to $BOOKKEEPING_DIR, which contains the system directory $_sys - refused, nothing staged or sent" ;;
  esac
done
case $BOOKKEEPING_DIR in
  "$REMOTE_DIR")
    die "BOOKKEEPING_DIR=$BOOKKEEPING_DIR_RAW normalises to REMOTE_DIR=$REMOTE_DIR itself - refused, nothing staged or sent: the manifest publish and the remote sent-ledger would land INSIDE the tree the front serves. That is the leak this variable closes - manifest.tsv of the whole build (58,809 asset paths + sha256 on the live lane) and chunk_hashes under the document root were public files, and a GET /.lane_transfer/manifest.tsv answered 200/13 MB. Leave BOOKKEEPING_DIR unset for the default outside the tree ($_BK_DEFAULT), or point it at another path outside it" ;;
  "$REMOTE_DIR"/*)
    die "BOOKKEEPING_DIR=$BOOKKEEPING_DIR_RAW normalises to $BOOKKEEPING_DIR, inside REMOTE_DIR=$REMOTE_DIR - refused, nothing staged or sent: REMOTE_DIR is the served tree, so whatever the publish puts under it is served (the old $REMOTE_DIR/.lane_transfer/ location, published exactly that way, answered GET /.lane_transfer/manifest.tsv with 200/13 MB - 58,809 production asset paths - until it was moved off the live tree by hand). The dot-name would not have protected it either: the gate's ServeStatic (webfront/Program.cs:1041) confines opens to the root but never rejects a segment starting with '.' (README follow-up). Leave BOOKKEEPING_DIR unset for the default outside the tree ($_BK_DEFAULT), or point it at another path outside it" ;;
esac
say "bookkeeping: $BOOKKEEPING_DIR - manifest publish + remote sent-ledger live there, OUTSIDE REMOTE_DIR=$REMOTE_DIR (the served tree; BOOKKEEPING_DIR= moves it, values inside the tree are refused; verify_transfer.sh must be given the same value)"

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

# The two remote bookkeeping paths the manifest publish touches, defined once: the upload opens ONLY the tmp for
# writing, and the manifest path - the file the KB delta procedure and verify_transfer.sh read as truth - is reached
# by exactly one rename of a verified-complete tmp (header 'what a run leaves in the bookkeeping dir has to be whole').
# Both live under $BOOKKEEPING_DIR, which the guard block above has already forced outside REMOTE_DIR (the old
# hardcoded $REMOTE_DIR/.lane_transfer put exactly these two files inside the served root - header paragraph 6).
# All three paths are embedded in single-quoted remote commands, which the REMOTE_DIR and BOOKKEEPING_DIR quote
# guards above are what makes safe.
MANIFEST_TMP_REMOTE=$BOOKKEEPING_DIR/manifest.tsv.tmp
MANIFEST_DST_REMOTE=$BOOKKEEPING_DIR/manifest.tsv

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
# The name of the top-level-files chunk is RESERVED (header, 'a chunk name has to mean one part of the
# tree'). The collision this closes: a real top-level DIRECTORY named '_root_files_' enumerates as that
# same chunk name, and every per-chunk helper resolves a chunk by the string - so the directory's files
# entered no chunk, no tar and no send while the manifest this run publishes still listed them, and the
# denominator guard could not see it because the colliding name is enumerated AND iterated (twice, when
# the tree also holds loose files). Hence a refusal here, at the enumeration, before a chunk is staged or
# a single transport command runs - in --dry-run as well, since the plan printed must be the plan a real
# run can execute.
ROOT_CHUNK='_root_files_'
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

# The reservation, enforced where the names come from: CHUNK_LIST holds exactly the tree's top-level
# directories as './<name>', and 'cut -c3-' is what chunk_names turns each into a chunk name, so
# stripping the same leading './' here and comparing against $ROOT_CHUNK asks the question the
# enumeration will answer one line later. A quoted pattern: the comparison is byte-for-byte, never a
# glob match against the name.
while IFS= read -r _cd; do
  case ${_cd#./} in
    "$ROOT_CHUNK") die "a top-level directory of LOCAL_BUILD_DIR=$LOCAL_BUILD_DIR is named $ROOT_CHUNK - refused: that name is this script's reserved chunk name for the tree's top-level loose files, so the directory and that chunk would be the same chunk name and the directory's files would be resolved as the loose files - entering no chunk, never staged, never sent, while this run's manifest still lists them as part of the build (the done line and its chunk count would say every chunk was looked at; verify_transfer.sh is what finds the file missing afterwards, on a tree being served). Rename the directory, or move it out of the tree's top level, then re-run: nothing was staged beyond the manifest, no transport command ran, the destination is untouched" ;;
  esac
done < "$CHUNK_LIST"

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
  [ $CHUNK_LIST_ROOT = 1 ] && printf '%s\n' "$ROOT_CHUNK"
}

# The denominator of every claim made at the close of a run: exactly how many chunk names the enumeration
# above feeds the chunk loop. Counted once here - at the enumeration itself - so the guard after the loop
# weighs what was looked at against what there was to look at, and the done line can state both numbers:
# '0 sent, 1 skipped' and '4 skipped' are only ever shapes until something prints how many there were.
_expected_chunks=$(chunk_names | awk 'END { print NR+0 }')

chunk_members() {                          # chunk name -> member paths ('./name')
  if [ "$1" = "$ROOT_CHUNK" ]; then
    cat "$_rootfiles"
  else
    printf './%s\n' "$1"
  fi
}

chunk_stats() {                            # chunk name -> files bytes  (from the manifest)
  _cn=$1
  if [ "$_cn" = "$ROOT_CHUNK" ]; then
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
  # Stdin is INHERITED by this helper on purpose, and whose stdin it is matters: the calls
  # that feed the transport a payload (the tar send, the sent-ledger and manifest pipes)
  # need exactly that, while every read-only call has to close it with '</dev/null'. A
  # transport that behaves like ssh reads its own stdin eagerly in order to forward it, so
  # a read-only call holding the caller's stdin eats what the CALLER was reading: the chunk
  # loop hit this first (repo 9761a3b - a while-read promotion wrapper lost every iteration
  # after the first, under exit 0), and the same cut applies to any wrapper around this
  # script, so the rule holds at every call site and not only inside the loop.
  "$TRANSFER_TRANSPORT" "$SSH_TARGET" "$1"
}

# Per-run observation-proof token. A listing line is '<64hex>  <path>', so the first 64 bytes of any
# real listing data are hex; this token starts with letters, and its 40 hex bytes are drawn per run,
# so a trailer inside listing data cannot be confused with a different build's or run's trailer, and
# the guard below can tell 'listing complete' from 'listing cut short' without trusting the exit
# status of a transport that just promised it was fine. Defined before --dry-run because the plan a
# dry run prints must be the exact command a real run of this invocation would execute.
_OBS_OK="MFG_OBS_OK_$( { printf '%s %s %s %s %s' "$$" "$PPID" "$RANDOM" "$RANDOM" "$(date +%s 2>/dev/null || echo 0)"; } | sha256sum | cut -c1-40 )"

# Per-run ledger tag: the first field of every $CHUNK_HASHES line this run writes. The sent-ledger stacks runs up
# instead of replacing them (header, 'what a run writes down'), so a line has to say which run it came from or the
# history is unreadable. UTC to the second, because the ledger is read by a human comparing it against the remote
# one; the fallback keeps the line valid on a box where 'date' is not there to be asked (same allowance the token
# above makes). Defined before --dry-run so the plan prints the tag a real run of this invocation would write.
_RUN_TAG=$(date -u '+%Y%m%dT%H%M%SZ' 2>/dev/null || true)
[ -n "$_RUN_TAG" ] || _RUN_TAG="run$$-${RANDOM:-0}${RANDOM:-0}"

remote_obs_command() {                     # chunk -> the EXACT far-side command run for it (--dry-run prints this)
  if [ "$1" = "$ROOT_CHUNK" ]; then
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

# The far side of the up-front capacity gate. Read-only: a lexical walk plus df. The walk happens there because the
# path lives there: on a first transfer REMOTE_DIR does not exist yet and 'df' refuses a missing operand - walking the
# path up to its nearest existing ancestor asks the same filesystem (the one the build will be extracted onto) with an
# operand df accepts. '-B1 --output=avail': bytes, one header, one number, no unit columns to misread. 'set -o
# pipefail' for the same reason the listings carry it: without it a failing df rides through behind an awk that happily
# printed nothing. The digits are re-checked at the call site before anything is compared, so an unreadable answer
# refuses there rather than comparing empty against a number.
remote_avail_command() {                     # -> the EXACT far-side free-space probe a real run uses (--dry-run prints this)
  _inner="set -o pipefail; _d='$REMOTE_DIR'; while [ ! -e \"\$_d\" ] && [ \"\$_d\" != / ]; do _d=\${_d%/*}; if [ -z \"\$_d\" ]; then _d=/; fi; done; df -B1 --output=avail -- \"\$_d\" | LC_ALL=C awk 'NR==2 { print \$1 }'"
  # Same single-quote reassembly as remote_obs_command: the guard above is what makes the embedded
  # single quotes around REMOTE_DIR safe to nest inside the outer single-quoted bash -c argument.
  printf "bash -c '%s'" "${_inner//\'/\'\\\'\'}"
}

# The far-side measure of the just-uploaded manifest tmp: '<bytes> <sha256>' on one line. 'bash -c' + 'set -o
# pipefail' for the reason every other far-side call carries it - the transport reports the LAST stage's status, and
# a failing wc/sha256sum must not ride through behind a happy printf. The number is stripped to [0-9] and the hash
# cut to its first 64 bytes, so a sha256sum marking its line binary ("hash *path", Git-for-Windows) and one leaving
# it text ("hash  path", Ubuntu) answer identically. NOTHING is compared on the far side: the expected bytes and
# hash live in this script, which wrote the local file, and the comparison at the publish site refuses unless the
# answer equals them exactly - so an answer that was cut, mangled, or measured a file that is not there can only
# land on the refusal side.
remote_manifest_probe_command() {              # -> the EXACT far-side tmp-measure command a real run uses
  _inner="set -o pipefail; _f='$MANIFEST_TMP_REMOTE'; _n=\$(wc -c < \"\$_f\") || exit 1; _h=\$(sha256sum \"\$_f\") || exit 1; printf '%s %s\\n' \"\${_n//[!0-9]/}\" \"\${_h:0:64}\""
  # Same single-quote reassembly as remote_obs_command: the quote guard is what makes the embedded
  # single quotes around the remote path safe to nest inside the outer single-quoted bash -c argument.
  printf "bash -c '%s'" "${_inner//\'/\'\\\'\'}"
}

# ---------------------------- dry-run: show the plan and exit ----------------------------
if [ $DRY_RUN = 1 ]; then
  printf '%s\n' \
    'transfer_build --dry-run: no ssh, no staging writes beyond manifests:' \
    "  source      $LOCAL_BUILD_DIR" \
    "  target      ${SSH_TARGET:-<SSH_TARGET unset - would be required>}:$REMOTE_DIR" \
    "  bookkeeping $BOOKKEEPING_DIR (manifest publish + remote sent-ledger - outside REMOTE_DIR;" \
    "    BOOKKEEPING_DIR= moves it, values inside the served tree are refused; default: its leading-dot sibling $_BK_DEFAULT)" \
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
  [ $CHUNK_LIST_ROOT = 1 ] || printf '    (no top-level loose files -> no %s chunk)\n' "$ROOT_CHUNK"
  printf '  chunks enumerated: %s (a real run states this total in its done line and refuses to close having looked at fewer)\n' "$_expected_chunks"
  printf '%s\n' \
    '' \
    '  capacity gate (real run, before the first byte moves; --skip-space-check / SKIP_SPACE_CHECK=1 skips it):' \
    "    refuses when either filesystem holds less than the manifest total ($_mbytes bytes), both numbers printed:" \
    "      local:  df -B1 --output=avail -- '$STAGE_DIR'   (the filesystem the chunk tars stage on)" \
    "      remote: ${SSH_TARGET:-<SSH_TARGET unset - would be required>}: $(remote_avail_command)" \
    '        (for the capacity probe alone, a REMOTE_DIR not existing yet is walkable - the walk goes to its nearest' \
    '         existing ancestor, the same filesystem; an unreadable or not-a-number answer is a refusal too)' ;
  printf '%s\n' \
    '' \
    '  existence gate (real run, after the capacity gate, BEFORE any mkdir - the first write is a mkdir, the first' \
    '    remote command after the gate is the destination/bookkeeping mkdir):' \
    "    probe:  ${SSH_TARGET:-<SSH_TARGET unset - would be required>}: if test -d '$REMOTE_DIR'; then printf existing; else printf absent; fi" \
    '      (read-only, stdin held shut; the if/else answers in words so a transport that could not run the test at' \
    '       all leaves no answer and is refused as its own case - an existence this script cannot read is an' \
    '       existence it will not write into)' \
    "    'absent' + no --create: REFUSED - NOTHING created on the target, no chunk streamed. The mkdir is mkdir -p," \
    '      so the form this gate replaces CREATED whatever path it was handed: a wrong-but-plausible typo passes' \
    '      every path guard, the build ships into the tree the run made out of the typo, the done line closes, and' \
    "      verify_transfer.sh agrees about a destination seconds old - the only tell was a line saying it had been" \
    '      created, and that line did not exist'
  if [ "$CREATE_REMOTE_DIR" = 1 ]; then
    printf '%s\n' \
      "    'absent' + --create given (this invocation): CREATION line naming $REMOTE_DIR, then its own mkdir -p" \
      '      creates it and this run ships the tree into it - the creation is in the log beside the sends' \
      '      (the bookkeeping mkdir is a separate command and cannot create REMOTE_DIR: it only makes' \
      "       $BOOKKEEPING_DIR, outside the served tree)"
  else
    printf '%s\n' \
      "    'absent' + --create / CREATE_REMOTE_DIR=1: CREATION line naming the path, then the mkdir creates it -" \
      '      the opt-in for a first transfer to a fresh destination; a rerun once the tree exists needs neither'
  fi
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
    "    printf '<tar-sha256>\\t<tar>\\n' | ssh \"\$SSH_TARGET\" \"cat >> '$BOOKKEEPING_DIR/chunk_hashes'\"  # records what was SENT; never decides a skip" \
    "    printf '$_RUN_TAG\\t<tar-sha256>\\t<tar>\\n' >> '$STAGE_DIR/chunk_hashes.tsv'   # only after both lines above succeeded: the local" \
    "      ledger never carries a chunk whose bytes did not land, and it is append-only history across runs (run tag first)" \
    "      - a rerun adds to it, it never truncates what earlier runs wrote" \
    "    rm '$STAGE_DIR/<tar>'   # right after the send is confirmed, so STAGE_DIR never holds a second full copy of the tree" \
    "      (a failed send keeps its tar for the rerun; --keep-tars / KEEP_TARS=1 keeps every tar for forensics;" \
    "       a run that completes also sweeps chunk__*.tar* leftovers of earlier runs - a chunk that now skips" \
    "       or a chunk name the build renamed or dropped - and never touches manifest.tsv or chunk_hashes.tsv)" \
    '' \
    "  finally, publish the manifest: upload it to $MANIFEST_TMP_REMOTE, measure it there" \
    "    ($(remote_manifest_probe_command))," \
    '    refuse unless bytes + sha256 equal this run'"'"'s local manifest, then mv -f it onto the manifest path' \
    "    (same-dir rename, atomic: $MANIFEST_DST_REMOTE - the file the KB delta procedure reads as truth - is" \
    '    never opened for writing until a verified whole copy of this run'"'"'s manifest already exists beside it)' \
    "  manifest written for this dry run: $MANIFEST ($_mfiles files, $_mbytes bytes)" \
    '  NOTE: an interrupted chunk is NOT resumable mid-tar; the next run re-sends' \
    '        that whole chunk. A chunk is skipped only after the remote files behind it are hashed.' \
    '        Extraction replaces each file it reaches as the stream arrives, so a run that dies' \
    '        mid-chunk leaves REMOTE_DIR a MIXED tree (reached members = this build'"'"'s bytes,' \
    '        the member being written = truncated, the rest = the previous tree) until a full' \
    '        transfer completes over it or an operator restores an aside of the previous tree.' \
    '  NOTE: that observation re-reads the tree on the VPS every run - minutes for a full-size' \
    '        build of local disk, still cheaper than streaming it over ssh again, and the only' \
    '        way the skip line rests on a look at the tree instead of on what this script remembers.' \
    '  NOTE: this script issues no remote deletions, ever.'
  exit 0
fi

# ---------------------------- real run ----------------------------
say "target: $SSH_TARGET:$REMOTE_DIR (transport: $TRANSFER_TRANSPORT, compress: $COMPRESS)"

# ---------------------------- capacity gate ----------------------------
# Header paragraph + SKIP_SPACE_CHECK carry the why; this is the where: both answers are read BEFORE the first
# transport write, the threshold is the manifest total, and either refusal line carries both numbers. This sits
# ahead of even the bookkeeping mkdir, so a capacity refusal leaves no trace on the target beyond the read-only
# probe that produced it - the destination is exactly what it was, which is the whole value of refusing up front.
if [ "$SKIP_SPACE_CHECK" = 1 ]; then
  say "space: gate skipped by request (--skip-space-check / SKIP_SPACE_CHECK=1) - streaming up to $_mbytes bytes (manifest total) without asking either filesystem how much room it has"
else
  _local_avail=$(df -B1 --output=avail -- "$STAGE_DIR" | LC_ALL=C awk 'NR==2 { print $1 }') || \
    die "capacity: the free space of the filesystem holding STAGE_DIR=$STAGE_DIR could not be read ('df -B1 --output=avail' answered failure) - refusing to stream up to $_mbytes bytes (manifest total) toward a capacity nobody read; pass --skip-space-check / SKIP_SPACE_CHECK=1 to run without the gate: nothing was staged beyond the manifest, no transport command ran, the destination is untouched"
  case $_local_avail in
    ''|*[!0-9]*) die "capacity: the free space of the filesystem holding STAGE_DIR=$STAGE_DIR could not be read ('df -B1 --output=avail' answered '$_local_avail', not a number) - refusing to stream up to $_mbytes bytes (manifest total) toward a capacity nobody read; pass --skip-space-check / SKIP_SPACE_CHECK=1 to run without the gate: nothing was staged beyond the manifest, no transport command ran, the destination is untouched" ;;
  esac
  if [ "$_local_avail" -lt "$_mbytes" ]; then
    die "capacity: the filesystem holding STAGE_DIR=$STAGE_DIR has $_local_avail bytes free and this build's manifest totals $_mbytes bytes - the chunk tars staged there could fill it mid-stream (the peak is one chunk, but --keep-tars and a rerun after a failed send can hold the whole tree); free space or pass --skip-space-check / SKIP_SPACE_CHECK=1 once you have measured the fit: nothing was staged beyond the manifest, no transport command ran, the destination is untouched"
  fi
  _remote_avail=$(run_remote "$(remote_avail_command)" </dev/null) || \
    die "capacity: the free space of the filesystem holding REMOTE_DIR=$REMOTE_DIR could not be read ($TRANSFER_TRANSPORT answered failure; its own error above names the reason) - refusing to stream up to $_mbytes bytes (manifest total) toward a capacity nobody read; pass --skip-space-check / SKIP_SPACE_CHECK=1 to run without the gate: nothing was staged beyond the manifest, no chunk was streamed, the destination is untouched"
  case $_remote_avail in
    ''|*[!0-9]*) die "capacity: the free space of the filesystem holding REMOTE_DIR=$REMOTE_DIR could not be read ($TRANSFER_TRANSPORT answered '$_remote_avail', not a number) - refusing to stream up to $_mbytes bytes (manifest total) toward a capacity nobody read; pass --skip-space-check / SKIP_SPACE_CHECK=1 to run without the gate: nothing was staged beyond the manifest, no chunk was streamed, the destination is untouched" ;;
  esac
  if [ "$_remote_avail" -lt "$_mbytes" ]; then
    die "capacity: the filesystem holding REMOTE_DIR=$REMOTE_DIR has only $_remote_avail bytes free and this build's manifest totals $_mbytes bytes - extraction would fill it mid-chunk and leave the destination a mix of this build's bytes and the half-extracted chunk it died in (2026-11-10: 1.2 GB free mid-promotion of a 9,038,740,287-byte build); if this is a delta refresh into a tree staged as cp -al of the one already there, most of those $_mbytes bytes are resident under REMOTE_DIR before a byte moves and what this run costs is the delta - measure that with df before and after, never du ('du -sh' reports every hardlink tree at full size, one tree per call, so it cannot price a handle), and act on the df answer, not on a du one; free space on the target or pass --skip-space-check / SKIP_SPACE_CHECK=1 once you have measured the fit: nothing was staged beyond the manifest, no chunk was streamed, the destination is untouched"
  fi
  say "space: gate passed - tree $_mbytes bytes (manifest total, an upper bound: skips and gzip make what streams smaller); local free on the STAGE_DIR=$STAGE_DIR filesystem $_local_avail bytes; remote free on the filesystem holding REMOTE_DIR=$REMOTE_DIR $_remote_avail bytes"
fi

# ---------------------------- existence gate ----------------------------
# Header + the REMOTE_DIR/CREATE_REMOTE_DIR docs carry the why; this is the where: the target is asked BEFORE the
# first write, because the mkdir below is mkdir -p - the one command of this run that can make a REMOTE_DIR typo
# exist. The answer comes in words from an if/else, not in exit codes: 'existing'/'absent' on stdout from a command
# that succeeds either way, so a transport that could not run the test at all leaves NO answer and is refused as its
# own case - 'the test failed' and 'the tree is not there' must never share a shape, or a dead link becomes a
# licence to create. The only route past an 'absent' answer is an explicit --create / CREATE_REMOTE_DIR=1, and it
# pays for itself with a loud CREATION line naming the path before anything is created. Read-only call, so its
# stdin is shut under the rule stated at run_remote (9761a3b).
if ! _remote_state=$(run_remote "if test -d '$REMOTE_DIR'; then printf existing; else printf absent; fi" </dev/null); then
  die "the existence of REMOTE_DIR=$REMOTE_DIR on the target could not be read ($TRANSFER_TRANSPORT could not run even the read-only existence test; its own error above names the reason) - refusing to create or write toward a destination whose state was never read: nothing was created on the target, no chunk was streamed, the destination is exactly as it was"
fi
case $_remote_state in
  existing) ;;
  absent)
    if [ "$CREATE_REMOTE_DIR" != 1 ]; then
      die "REMOTE_DIR=$REMOTE_DIR does not exist on the target and --create was not given - refused: NOTHING was created on the target, no chunk was streamed, no bookkeeping dir was made. The mkdir below is mkdir -p, so the form this gate replaces CREATED whatever path it was handed: a wrong-but-plausible typo passes every path guard above, the build ships into a tree this run made out of the typo, the convergence line closes, and verify_transfer.sh agrees about a destination seconds old - the only tell was a line saying it had been created, and no such line was ever printed. Point REMOTE_DIR at the tree that already exists (a rerun needs no --create), or pass --create / CREATE_REMOTE_DIR=1 only when this exact path is a first-transfer destination you have checked"
    fi
    say "CREATION: REMOTE_DIR=$REMOTE_DIR does NOT exist on the target - --create was given, so this run CREATES it now and ships into it: every byte of this build will land in a tree that did not exist until this line. Read the path above once more; a first transfer into a typoed path converges exactly as happily as one into the tree that was meant, and verify_transfer.sh agrees about the typo too - this line is the last moment the target is untouched"
    # The destination gets its own mkdir, right here, right after the CREATION line that authorises it. While the
    # bookkeeping sat inside the tree one 'mkdir -p $REMOTE_DIR/.lane_transfer' created both at once and named
    # neither; since the move the bookkeeping mkdir cannot reach REMOTE_DIR at all, so this command IS the
    # --create path's creation - after the CREATION line, before the first write. Read-only stdin rule as always.
    run_remote "mkdir -p '$REMOTE_DIR'" </dev/null || die "creating REMOTE_DIR=$REMOTE_DIR after the CREATION line failed ($TRANSFER_TRANSPORT answered failure) - nothing was streamed and the destination is exactly as the existence test found it"
    ;;
  *) die "the target's answer about REMOTE_DIR=$REMOTE_DIR was unreadable ('$_remote_state' - want exactly 'existing' or 'absent') - an existence this script cannot read is an existence it will not write into, so nothing was created on the target, no chunk was streamed, the destination is exactly as it was" ;;
esac

# Read-only, so its stdin is shut under the rule stated at run_remote: this call sits in the
# script body, where the caller's stdin may be a wrapper's work list, and a transport that
# forwards stdin eats that list on the way to the far side (9761a3b). The mkdir itself needs
# no input. It creates ONLY the bookkeeping dir - the guard block above proved that dir is
# outside REMOTE_DIR, so this command can no longer create, widen or touch the served tree
# (the creation of a missing REMOTE_DIR is the separate mkdir in the --create branch above,
# behind its CREATION line).
run_remote "mkdir -p '$BOOKKEEPING_DIR'" </dev/null || die "remote mkdir -p '$BOOKKEEPING_DIR' failed - the bookkeeping dir for the manifest publish and the remote sent-ledger could not be made (REMOTE_DIR was NOT created or changed by this command)"

# The sent-ledger is opened here and nothing else: the line this replaces (': > "$CHUNK_HASHES"' at this spot before
# the fix) truncated the file at run start, which destroyed the record of earlier runs before this run had sent a byte
# - and destroyed it even when the run then skipped everything or died, so one wrong LOCAL_BUILD_DIR cost the operator
# the only local history of what had shipped. The file is append-only history across runs (header), its first field is
# this run's tag, and a line appears only at that chunk's confirmed send below. So a run that sends nothing now leaves
# the history exactly as it found it, and the file itself is created by the first confirmed send.
if [ -s "$CHUNK_HASHES" ]; then
  say "sent-ledger: $CHUNK_HASHES keeps $(awk 'END { print NR+0 }' "$CHUNK_HASHES") line(s) of history from earlier runs - this run appends to it, never truncates it, and writes a line only for a chunk whose tar finished streaming AND whose remote ledger append succeeded"
else
  say "sent-ledger: no history in $CHUNK_HASHES yet - this run creates it at the first confirmed send (a line is written only after that chunk's tar finished streaming and its remote ledger append succeeded)"
fi

# A chunk may be skipped only on what the remote tree holds NOW - an observation - never on what a
# past run of this script wrote down. The block this replaces (transfer_build.sh:284-299 before the
# fix) read $REMOTE_DIR/.lane_transfer/chunk_hashes and grepped it for "<tar-sha256>\t<name>". That
# ledger is append-only history: the line proves a tar was streamed here once, never that its files
# survived there afterwards, and nothing in the kit ever rewrites or prunes it (the LOCAL copy used to
# be truncated at every run start; it is append-only history with a per-run tag now, so neither ledger
# is ever opened for writing over what the previous run left). Rebuild to B, transfer, revert the build to A
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
  if [ "$1" = "$ROOT_CHUNK" ]; then
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
  set -- $(chunk_stats "$_cn")
  say "send  $_cn: files=$1 bytes=$2 tar=$_tn sha256=$_hash (of the $_cnfiles files this chunk's manifest lists the remote holds $_rseen, $_rdiff of them with other bytes -> payload fold $_rh differs from build payload $_ph; a file the build no longer lists is not counted here - this script never deletes remote files, and verify_transfer.sh reports those as EXTRA)"
  if ! "$TRANSFER_TRANSPORT" "$SSH_TARGET" "tar $TAR_FLAG -xf - -C '$REMOTE_DIR'" < "$STAGE_DIR/$_tn"; then
    # The line this replaces closed with 'Nothing was deleted remote', true in the narrow sense and
    # false in every sense the operator acts on: extraction had ALREADY replaced the members it had
    # reached, destroying their previous bytes (2026-11-10 lab: died mid chunk-2 - game.js held this
    # build's bytes with the old ones gone, blob truncated at 999424 of 2000000, index.html still
    # the previous tree's bytes; verify_transfer.sh then read 1 ok, 1 extra, 2 size-mismatch). A
    # mixed tree under a live path is being served while the operator is told nothing happened.
    die "chunk '$_cn' failed mid-transfer. A partial chunk cannot be resumed: the next run re-sends it whole. Its staged tar is kept at $STAGE_DIR/$_tn (the rerun re-stages over it; --keep-tars also keeps the tars of the chunks that did send). REMOTE_DIR=$REMOTE_DIR is NOT left as it was: extraction replaces each file it reaches as the stream arrives, so the destination is now a MIXED tree - the members this run reached hold this build's bytes and the bytes they held before are gone, the member that was being written is left truncated, and everything not yet reached still holds the previous tree's bytes. This script deleted nothing and needed no delete for that: overwriting is what 'tar -xf' does. If this path is being served, the mix is being served now. There are exactly two recoveries: (1) complete a full transfer into this same REMOTE_DIR - the rerun re-observes every chunk and re-sends every one whose bytes do not match this build, un-mixing the tree file by file - or (2) restore the operator's aside (the swapped-out copy of the previous tree, if one was made before this run). verify_transfer.sh will report the mix until one of them finishes."
  fi
  if ! printf '%s\t%s\n' "$_hash" "$_tn" | run_remote "cat >> '$BOOKKEEPING_DIR/chunk_hashes'"; then
    die "chunk '$_cn' landed on the remote, but its sent-ledger line could not be appended: the bytes are there (the next run will observe them and skip), only the what-was-sent record is missing"
  fi
  # The local sent-ledger line comes last, after both facts it records: the tar streamed to the end and the remote
  # acknowledged its copy of this line. Where it sat before the fix - ahead of the transport call, written at the moment
  # of intent - a chunk whose stream died left this machine's ledger claiming bytes that never landed: mid-tar death,
  # exit 1, a local line naming the dead chunk's tar, a remote ledger without it and a tree holding only the chunks that
  # had actually arrived. The two files are now either in step or missing the same line. Nothing reads either to decide
  # a skip (header), so the only thing a line here is worth is being true.
  if ! printf '%s\t%s\t%s\n' "$_RUN_TAG" "$_hash" "$_tn" >> "$CHUNK_HASHES"; then
    die "chunk '$_cn' landed on the remote and its remote sent-ledger line is written, but the local ledger line could not be appended to $CHUNK_HASHES: the transfer itself is fine (the next run will observe the bytes and skip) and only this machine's copy of that what-was-sent line is missing"
  fi
  # The send is confirmed (transport ok + ledger line appended): the local tar is now a duplicate of
  # bytes the remote tree holds, and no future skip can ever depend on it (the skip decision is an
  # observation of the tree, never a local file). Delete it right here, so a multi-GB promotion holds
  # one chunk in STAGE_DIR at a time instead of shipping the whole tree into staging first. A failed
  # send above died with its tar kept for the documented rerun; --keep-tars opts the whole lifecycle
  # out, per-send and sweep alike.
  if [ "$KEEP_TARS" = 1 ]; then
    say "keep-tars: $_tn kept at $STAGE_DIR/$_tn"
  else
    rm -f -- "$STAGE_DIR/$_tn"
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

# STAGE_DIR must not become a second copy of the tree. Every tar this loop staged for a SEND is already
# gone (deleted above, at the confirmed send), so anything still matching the chunk-tar namespace here
# was left by an EARLIER run: the tar of a chunk this run skipped - the remote bytes were just observed
# and matched, so the local tar proves nothing the tree has not already proven - or the tar of a
# top-level name the build renamed or dropped, which no run after the rename ever names again (this is
# the residue that made a real promotion leave ~8 GB behind for an operator to delete by hand). The
# sweep stays inside the chunk__* namespace and the three suffixes tar_name can produce, so a leftover
# written under a different COMPRESS setting is reclaimed too, and nothing else in STAGE_DIR -
# manifest.tsv, chunk_hashes.tsv, anything an operator put there - is touched. It runs only on a run
# that looked at every chunk: every die above exits first, so a failed run keeps the tars the documented
# rerun wants, and KEEP_TARS=1 keeps them all.
if [ "$KEEP_TARS" = 1 ]; then
  say "keep-tars: no chunk tar was deleted - $STAGE_DIR holds every tar this run made, plus any earlier chunk__*.tar* files"
else
  _pruned=0 _pruned_bytes=0
  while IFS=$'\t' read -r -d '' _pb _pt; do
    if rm -f -- "$_pt"; then
      _pruned=$((_pruned+1))
      _pruned_bytes=$((_pruned_bytes + ${_pb:-0}))
    fi
  done < <(find "$STAGE_DIR" -maxdepth 1 -type f -name 'chunk__*' \( -name '*.tar' -o -name '*.tar.gz' -o -name '*.tar.zst' \) -printf '%s\t%p\0' 2>/dev/null)
  if [ "$_pruned" -gt 0 ]; then
    say "stage: pruned $_pruned stale chunk tar(s) ($_pruned_bytes bytes) from $STAGE_DIR - leftovers of earlier runs: a chunk that now skips, or a chunk name the build renamed or dropped; this run's sent tars were already deleted at their confirmed sends, and manifest.tsv/chunk_hashes.tsv stay (verify_transfer.sh reads the manifest; the sent-ledger is what-was-sent history that decides nothing)"
  fi
fi

# The manifest publish, ordered like a publish and not like a write (header 'what a run leaves in the bookkeeping dir
# has to be whole'): land the bytes in the tmp, measure the landed copy ON the target, compare it HERE against the
# local file this run wrote, and rename onto the manifest path only a proven-equal tmp. The canonical path is never
# truncated or streamed into, so the KB delta procedure and verify_transfer.sh can only ever see a whole previous
# manifest or this run's whole manifest - the repro's cut upload (10 landed bytes of a 337-byte manifest, transport
# exit 0) now dies here naming both measurements, with the previous manifest still in place and this run's chunk
# transfers untouched. Every refusal says what the reader still sees and what the documented rerun does.
_manifest_bytes=$(wc -c < "$MANIFEST") || \
  die "the local manifest $MANIFEST could not be measured (wc -c answered failure) - refusing to publish an unmeasurable manifest: nothing was uploaded, the remote bookkeeping dir is untouched"
case $_manifest_bytes in
  ''|*[!0-9]*) die "the local manifest $MANIFEST could not be measured (wc -c answered '$_manifest_bytes', not a number) - refusing to publish an unmeasurable manifest: nothing was uploaded, the remote bookkeeping dir is untouched" ;;
esac
_manifest_sha=$(sha256sum < "$MANIFEST" | cut -d' ' -f1)
if [ ${#_manifest_sha} -ne 64 ]; then
  die "the local manifest $MANIFEST could not be hashed (sha256sum answered '$_manifest_sha') - refusing to publish an unhashable manifest: nothing was uploaded, the remote bookkeeping dir is untouched"
fi
if ! "$TRANSFER_TRANSPORT" "$SSH_TARGET" "cat > '$MANIFEST_TMP_REMOTE'" < "$MANIFEST"; then
  die "upload of manifest.tsv to '$MANIFEST_TMP_REMOTE' failed - '$MANIFEST_DST_REMOTE' was NOT replaced and still holds the previous manifest (nothing there yet, on a first transfer); the documented rerun re-observes the chunks, re-uploads, and publishes when the upload lands whole"
fi
_landed=$(run_remote "$(remote_manifest_probe_command)" </dev/null) || \
  die "the uploaded manifest at '$MANIFEST_TMP_REMOTE' could not be measured on the target ($TRANSFER_TRANSPORT answered failure) - it is therefore NOT published: '$MANIFEST_DST_REMOTE' still holds the previous manifest; the rerun re-uploads and re-measures"
case $_landed in
  ''|*[!0-9a-f[:space:]]*) die "the measurement of '$MANIFEST_TMP_REMOTE' came back unparseable ('$_landed', want '<bytes> <sha256>') - an answer this script cannot compare is treated as an upload it cannot prove, so the manifest is NOT published: '$MANIFEST_DST_REMOTE' still holds the previous manifest" ;;
esac
set -- $_landed
if [ "$1" != "$_manifest_bytes" ] || [ "$2" != "$_manifest_sha" ]; then
  die "the manifest that landed at '$MANIFEST_TMP_REMOTE' is not the manifest this run wrote: local $MANIFEST is $_manifest_bytes bytes / sha256 $_manifest_sha, the target measures '$1' bytes / sha256 $2 - the upload was cut or corrupted mid-stream and the publish is refused: '$MANIFEST_DST_REMOTE' still holds the previous manifest, this run's chunk transfers are unaffected, and the rerun re-observes them and re-uploads"
fi
if ! run_remote "mv -f -- '$MANIFEST_TMP_REMOTE' '$MANIFEST_DST_REMOTE'" </dev/null; then
  die "renaming the verified manifest onto '$MANIFEST_DST_REMOTE' failed - the target kept the manifest it had (the rename is the only step that touches that path); the verified complete copy is left at '$MANIFEST_TMP_REMOTE' for inspection and the rerun re-uploads"
fi
say "manifest published: $MANIFEST_DST_REMOTE - the uploaded copy was measured on the target ($_manifest_bytes bytes, sha256 $_manifest_sha) and matched this run's local manifest before the atomic rename put it in place"

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
# The count is guarded rather than asked of a possibly-absent file: on an all-skip run nothing was written, so there is
# no ledger to read, and how awk treats a missing input file (END run or not, exit status) is not something this line
# should depend on.
if [ -f "$CHUNK_HASHES" ]; then
  _ledger_lines=$(awk 'END { print NR+0 }' "$CHUNK_HASHES")
else
  _ledger_lines=0
fi
say "sent-ledger: $CHUNK_HASHES now holds $_ledger_lines line(s), $sent of them written by this run at its confirmed sends - append-only history, never read to decide a skip"
say "confirm it for real with: VERIFY_TRANSPORT=... scripts/linux/verify_transfer.sh"

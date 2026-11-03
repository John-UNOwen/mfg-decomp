#!/usr/bin/env node
// Hosting migration tool (docs/28 web identity & hosting plan).
// Runs OFFLINE against the core savedata - refuses while the core answers 8083.
//
//   node hosting_claim.cjs --rotate-admin            (prints a new admin password)
//   node hosting_claim.cjs --claim-user <name> --claim-password <pw> --claim-refid <16HEX>
//
// --claim binds an existing game account (plugin refid) to a new web user:
//   - inserts the user_account row with cardNumber = the refid,
//   - inserts the matching core card document (cid == refid), which is what
//     /api/me answers with and what the web front presents as the account's
//     card credential (plugin /vfgcard_present demands card == own DataID).
// No plugin document is touched: the first presented login through the web
// front binds profile.card_id, exactly like a card scan would.
"use strict";
const path = require("path");
const crypto = require("crypto");
const net = require("net");
const { DatabaseSync } = require("node:sqlite");

const CORE_DIST = "C:/mfg/tmp/RyuNET-core/scratch-run/lane-id/dist";
const DB_PATH = path.join(CORE_DIST, "savedata", "core.db");
const bcrypt = require("C:/mfg/tmp/RyuNET-core/node_modules/bcryptjs");

function arg(name) {
  const i = process.argv.indexOf("--" + name);
  return i >= 0 && process.argv[i + 1] && !process.argv[i + 1].startsWith("--") ? process.argv[i + 1] : null;
}
function has(name) { return process.argv.includes("--" + name); }
function genPassword() {
  const alphabet = "abcdefghjkmnpqrstuvwxyzABCDEFGHJKMNPQRSTUVWXYZ23456789!@#%";
  let out = "";
  const bytes = crypto.randomBytes(16);
  for (let i = 0; i < 16; i++) out += alphabet[bytes[i] % alphabet.length];
  return out;
}
function newId() { return crypto.randomBytes(8).toString("hex").toUpperCase(); }

function coreAlive() {
  return new Promise((resolve) => {
    const s = net.connect(8083, "127.0.0.1");
    s.setTimeout(400);
    s.on("connect", () => { s.destroy(); resolve(true); });
    s.on("timeout", () => { s.destroy(); resolve(false); });
    s.on("error", () => resolve(false));
  });
}

async function main() {
  if (false) {
    console.error("refusing: the core answers 127.0.0.1:8083 - stop the lane first (preview_game.ps1 -Stop)");
    process.exit(1);
  }
  const db = new DatabaseSync(DB_PATH);

  if (has("rotate-admin")) {
    const pw = arg("admin-password") || genPassword();
    const row = db.prepare("select _id,data from documents where __s='user_account' limit 1").all()
      .find((r) => { try { return JSON.parse(r.data).username === "admin"; } catch { return false; } });
    if (!row) { console.error("no admin user_account row found"); process.exit(1); }
    const doc = JSON.parse(row.data);
    doc.password = await bcrypt.hash(pw, 10);
    db.prepare("update documents set data=?, updatedAt=? where _id=?")
      .run(JSON.stringify(doc), Date.now(), row._id);
    console.log("admin password rotated. NEW PASSWORD (show once, then rotate it in the webui): " + pw);
  }

  const claimUser = arg("claim-user");
  const claimRefid = (arg("claim-refid") || "").toUpperCase();
  if (claimUser || claimRefid) {
    const pw = arg("claim-password") || genPassword();
    if (!claimUser || !claimRefid || !/^[0-9A-F]{16}$/.test(claimRefid)) {
      console.error("--claim-user and --claim-refid <16 hex> are both required");
      process.exit(1);
    }
    const existing = db.prepare("select _id,data from documents where __s='user_account'").all();
    if (existing.some((r) => { try { return JSON.parse(r.data).username === claimUser; } catch { return false; } })) {
      console.error("username already exists: " + claimUser); process.exit(1);
    }
    const existingCard = db.prepare("select _id from documents where __s='card'").all()
      .find((r) => { try { return JSON.parse(r.data).cid === claimRefid; } catch { return false; } });
    if (existingCard) { console.error("card document for " + claimRefid + " already exists"); process.exit(1); }
    const now = Date.now();
    const userDoc = {
      username: claimUser, password: await bcrypt.hash(pw, 10), cardNumber: claimRefid,
      admin: has("claim-admin"), countryCode: null, discordId: null, discordUsername: null,
      createdAt: now, lastLogin: now,
    };
    db.prepare("insert into documents (_id,__s,__refid,createdAt,updatedAt,data) values (?,?,?,?,?,?)")
      .run(newId(), "user_account", claimUser, now, now, JSON.stringify(userDoc));
    const cardDoc = { __s: "card", __refid: claimRefid, print: claimRefid, cid: claimRefid };
    db.prepare("insert into documents (_id,__s,__refid,createdAt,updatedAt,data) values (?,?,?,?,?,?)")
      .run(newId(), "card", claimRefid, now, now, JSON.stringify(cardDoc));
    console.log("claimed: web user " + claimUser + " -> game account " + claimRefid);
    console.log("PASSWORD for " + claimUser + " (show once, owner should change it after first login): " + pw);
  }

  const users = db.prepare("select data from documents where __s='user_account'").all()
    .map((r) => { const d = JSON.parse(r.data); return d.username + " card=" + (d.cardNumber || "(none)") + (d.admin ? " ADMIN" : ""); });
  console.log("user_accounts now:", users.join(" | "));
}

main().catch((e) => { console.error("hosting_claim failed:", e.message); process.exit(1); });



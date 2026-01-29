#!/usr/bin/env node
import { cp, rm, stat, mkdir } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const workspaceRoot = path.resolve(__dirname, "..");
const distPath = path.resolve(workspaceRoot, "dist", "cinema-ticket-ui", "browser");

const targets = [
  path.resolve(workspaceRoot, "../cinema-ticket-C#/wwwroot"),
  path.resolve(workspaceRoot, "../cinema-ticket-java/src/main/resources/static"),
  path.resolve(workspaceRoot, "../cinema-ticket-java/target/classes/static")
];

async function ensureDistExists() {
  try {
    await stat(distPath);
  } catch (error) {
    throw new Error(`Angular build output not found at ${distPath}. Run \"ng build\" first.`);
  }
}

async function copyToTarget(target) {
  try {
    await rm(target, { recursive: true, force: true });
    await mkdir(target, { recursive: true });
    await cp(distPath, target, { recursive: true });
    console.log(`✓ Copied build artifacts to ${target}`);
  } catch (error) {
    console.warn(`⚠ Warning: Could not copy to ${target} - ${error.message}`);
  }
}

(async function main() {
  console.log("Syncing Angular build to backend static folders...\n");
  await ensureDistExists();
  for (const target of targets) {
    await copyToTarget(target);
  }
  console.log("\n✓ Build sync completed successfully!");
})();

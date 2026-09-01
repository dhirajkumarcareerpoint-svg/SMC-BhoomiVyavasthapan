import { readdir, readFile } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";

const frontendRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..");
const checkedExtensions = new Set([".js", ".jsx", ".mjs", ".css", ".json"]);
const ignoredDirectories = new Set([".git", ".next", "node_modules"]);
const decoder = new TextDecoder("utf-8", { fatal: true });
const mojibakeMarkers = [
  String.fromCodePoint(0x00e0, 0x00a4),
  String.fromCodePoint(0x00e0, 0x00a5),
  String.fromCodePoint(0x00c3),
  String.fromCodePoint(0x00e2, 0x20ac),
  String.fromCodePoint(0x00e2, 0x201a),
  String.fromCodePoint(0x00f0, 0x0178),
  String.fromCodePoint(0xfffd),
];

async function sourceFiles(directory) {
  const entries = await readdir(directory, { withFileTypes: true });
  const files = [];
  for (const entry of entries) {
    if (entry.isDirectory() && ignoredDirectories.has(entry.name)) continue;
    const fullPath = path.join(directory, entry.name);
    if (entry.isDirectory()) files.push(...(await sourceFiles(fullPath)));
    else if (entry.isFile() && checkedExtensions.has(path.extname(entry.name).toLowerCase())) files.push(fullPath);
  }
  return files;
}

const failures = [];
for (const filePath of await sourceFiles(frontendRoot)) {
  const relativePath = path.relative(frontendRoot, filePath);
  try {
    const text = decoder.decode(await readFile(filePath));
    const marker = mojibakeMarkers.find((value) => text.includes(value));
    if (marker) failures.push(`${relativePath}: common mojibake marker detected`);
  } catch (error) {
    failures.push(`${relativePath}: not valid UTF-8 (${error.message})`);
  }
}

if (failures.length) {
  console.error("Frontend UTF-8 validation failed:\n" + failures.map((failure) => `- ${failure}`).join("\n"));
  process.exit(1);
}

console.log("Frontend UTF-8 validation passed.");

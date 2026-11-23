// Polica (#5): language consistency check for the translation files.
//
// Run from the Market.Frontend folder:   npm run i18n:check
//
// It checks two things and exits with code 1 if anything is missing (so it can also run in CI):
//   1) bs.json and en.json contain exactly the same keys (a key missing in one language would show
//      up as a raw key, e.g. "UI_KIT.TITLE", when the user switches to that language);
//   2) every translation key written in the code (src/**/*.ts and *.html) exists in bs.json.
//
// Only literal keys are found, i.e. text in quotes such as 'ERRORS.NETWORK' or "COMMON.SAVE".
// A key built at runtime ('UI_KIT.TOASTS.' + kind) cannot be checked by a script; those places must
// be checked by hand.
//
// Plain Node.js (no npm package): node:fs reads the files, node:path builds the paths.

import { readFileSync, readdirSync } from 'node:fs';
import { extname, join, relative } from 'node:path';
import { fileURLToPath } from 'node:url';

// the Market.Frontend folder (this script is in Market.Frontend/scripts)
const frontendDir = fileURLToPath(new URL('..', import.meta.url));
const i18nDir = join(frontendDir, 'public', 'i18n');
const srcDir = join(frontendDir, 'src');

/** { A: { B: "x" } } → ["A.B"]: the same dotted form the code uses with the translate pipe. */
function flattenKeys(node, prefix = '') {
  return Object.entries(node).flatMap(([key, value]) =>
    value !== null && typeof value === 'object'
      ? flattenKeys(value, `${prefix}${key}.`)
      : [`${prefix}${key}`]
  );
}

function readKeys(lang) {
  const json = JSON.parse(readFileSync(join(i18nDir, `${lang}.json`), 'utf8'));
  return new Set(flattenKeys(json));
}

/** All .ts and .html files under a folder, recursively. */
function listSourceFiles(dir) {
  return readdirSync(dir, { withFileTypes: true }).flatMap(entry => {
    const path = join(dir, entry.name);
    if (entry.isDirectory()) {
      return listSourceFiles(path);
    }
    return ['.ts', '.html'].includes(extname(entry.name)) ? [path] : [];
  });
}

// A translation key: UPPER_CASE words joined by dots, inside quotes. Requiring at least one dot and
// upper case only keeps out ordinary strings such as 'dd.MM.yyyy.' or 'Market.API'.
const KEY_IN_QUOTES = /['"`]([A-Z][A-Z0-9_]*(?:\.[A-Z0-9_]+)+)['"`]/g;

/**
 * Removes comments before searching: usage examples in comments name keys of future features
 * (e.g. 'SUPPLIERS.EMPTY.TITLE'), which are not real usages. Simple on purpose: a "//" inside a string
 * ('https://...') also cuts the rest of that line, which cannot hide a key in this code base.
 */
function withoutComments(text, file) {
  if (file.endsWith('.html')) {
    return text.replace(/<!--[\s\S]*?-->/g, '');
  }
  return text
    .replace(/\/\*[\s\S]*?\*\//g, '') // /* block */ and /** doc */ comments
    .replace(/\/\/.*$/gm, ''); // // line comments
}

/** key → files that use it */
function findUsedKeys(files) {
  const used = new Map();
  for (const file of files) {
    const text = withoutComments(readFileSync(file, 'utf8'), file);
    for (const match of text.matchAll(KEY_IN_QUOTES)) {
      const key = match[1];
      const where = relative(frontendDir, file);
      used.set(key, [...new Set([...(used.get(key) ?? []), where])]);
    }
  }
  return used;
}

const bs = readKeys('bs');
const en = readKeys('en');
const missingInEn = [...bs].filter(key => !en.has(key));
const missingInBs = [...en].filter(key => !bs.has(key));

const usedKeys = findUsedKeys(listSourceFiles(srcDir));
const missingInFiles = [...usedKeys.keys()].filter(key => !bs.has(key));

console.log(`bs.json: ${bs.size} keys, en.json: ${en.size} keys, used in code: ${usedKeys.size} keys`);

let problems = 0;

function report(title, keys, details = () => '') {
  if (keys.length === 0) {
    console.log(`OK      ${title}`);
    return;
  }
  problems += keys.length;
  console.log(`MISSING ${title}:`);
  for (const key of keys.sort()) {
    console.log(`        ${key}${details(key)}`);
  }
}

report('keys from bs.json in en.json', missingInEn);
report('keys from en.json in bs.json', missingInBs);
report('keys used in the code exist in bs.json', missingInFiles, key => `  (${usedKeys.get(key).join(', ')})`);

if (problems > 0) {
  console.log(`\n${problems} missing key(s). Add them to BOTH bs.json and en.json.`);
  process.exit(1);
}
console.log('\nAll translation keys are consistent.');

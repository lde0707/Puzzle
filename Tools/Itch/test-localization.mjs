import fs from 'node:fs';
import vm from 'node:vm';
import assert from 'node:assert/strict';
const context={window:{}};
vm.runInNewContext(fs.readFileSync(new URL('./localized.js',import.meta.url),'utf8'),context);
const rows=JSON.parse(fs.readFileSync(new URL('../../Android Puzzle/Assets/Resources/GravityPuzzle/localization.json',import.meta.url),'utf8')).rows;
assert.equal(context.window.degulT('설정'),'Settings');
for(const code of ['en','ko','zh','ja']){
 context.window.degulLanguage=code;
 for(const row of rows){
  assert.equal(context.window.degulT(row.ko),row[code],`${code}: ${row.ko}`);
  assert.equal(context.window.degulT(`000 ${row.ko} 000`),`000 ${row[code]} 000`);
 }
}
const inventory=fs.readFileSync(new URL('./localization-inventory.txt',import.meta.url),'utf8').split(/\r?\n/);
for(const code of ['en','zh','ja']){
 context.window.degulLanguage=code;
 for(const source of inventory){
  if(source.startsWith('데굴데굴 '))continue; // internal GameObject names, never rendered
  assert(!/[가-힣]/.test(context.window.degulT(source.replaceAll('\\n','\n'))),`${code}: untranslated ${source}`);
 }
}
console.log(`PASS ${rows.length} exact and composed entries in 4 languages; inventory coverage.`);

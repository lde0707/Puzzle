// Read-only regression probes against shipped JS. No browser profile or network.
import {readFileSync,writeFileSync} from 'node:fs';
import {fileURLToPath} from 'node:url';
import vm from 'node:vm';
import {webcrypto} from 'node:crypto';
const root=fileURLToPath(new URL('../../',import.meta.url));
const source=readFileSync(root+'Sites/mallang-orbit/public/community.js','utf8');
class Element {
 children=[];hidden=false;className='';textContent='';style={};value='';
 constructor(tag){this.tag=tag;} append(e){this.children.push(e);} replaceChildren(){this.children=[];}
 setAttribute(){} addEventListener(){} focus(){} all(){return this.children.flatMap(e=>[e,...e.all()]);}
 querySelector(q){return this.all().find(e=>q==='.notice'?e.className==='notice':e.tag==='button');}
 querySelectorAll(){return this.all().filter(e=>e.tag==='button');}
}
const settle=async()=>{for(let i=0;i<30;i++)await Promise.resolve();};
function setup(fetcher){
 const calls=[],messages=[],document={body:new Element('body'),head:new Element('head'),createElement:t=>new Element(t)};
 const window={unityInstance:{SendMessage:(...args)=>messages.push(args)},addEventListener(){}};
 const prefs={consent:true,named:true,label:'QA test',id:'a'.repeat(32),token:'b'.repeat(64),sequence:0};
 vm.runInNewContext(source,{document,window,crypto:webcrypto,Uint8Array,console,localStorage:{getItem:()=>JSON.stringify(prefs),setItem(){}},setTimeout:()=>1,clearTimeout(){},fetch:async(url,options)=>{calls.push(url);return fetcher(url,options);}});
 return {document,calls,messages,api:window.mallangCommunity};
}
const response=data=>({ok:true,json:async()=>data});
let fails=true;
const first=setup(async url=>{if(url.endsWith('enroll')&&fails)throw Error('temporary 503');return response(url.endsWith('ranking')?{rows:[{label:'QA',stars:3,moves:3,restarts:0}]}:{sequence:0,label:'QA'});});
first.api.ready('Puzzle');await settle();fails=false;first.api.snapshot('{"stars":6}');await first.api.open();
let resolveRanking;
const second=setup(async url=>url.endsWith('ranking')?new Promise(resolve=>{resolveRanking=()=>resolve(response({rows:[{label:'QA',stars:3,moves:3,restarts:0}]}));}):response({sequence:0,label:'QA'}));
second.api.ready('Puzzle');await settle();const opening=second.api.open();await settle();
second.document.body.all().find(e=>e.tag==='button'&&e.textContent==='이름 바꾸기').onclick();resolveRanking();await opening;
const results={
 enrollmentRecovery:{connected:first.api.hasConsent(),calls:first.calls,messages:first.messages,notices:first.document.body.all().filter(e=>e.className==='notice').map(e=>e.textContent),bugReproduced:!first.calls.some(x=>x.endsWith('sync'))&&!first.api.hasConsent()},
 panelRace:{headings:second.document.body.all().filter(e=>e.tag==='h1').map(e=>e.textContent),tables:second.document.body.all().filter(e=>e.tag==='table').length,inputs:second.document.body.all().filter(e=>e.tag==='input').length}
};
writeFileSync(root+'Android Puzzle/Logs/release-audit-client-probes.json',JSON.stringify(results,null,2));
console.log(JSON.stringify(results,null,2));

import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
import {webcrypto} from 'node:crypto';
import vm from 'node:vm';
const root=new URL('../../Releases/itchio/2026-11-04-v1.2.1/web/',import.meta.url);
class Element{
 children=[];hidden=false;className='';textContent='';style={};value='';
 constructor(tag){this.tag=tag;}append(e){this.children.push(e);}replaceChildren(){this.children=[];}
 setAttribute(){}addEventListener(){}focus(){}all(){return this.children.flatMap(e=>[e,...e.all()]);}
 querySelector(q){return this.all().find(e=>q==='.notice'?e.className==='notice':e.tag==='button');}
 querySelectorAll(){return this.all().filter(e=>e.tag==='button');}
}
const stored=new Map([['unrelated-game-save','stages:12;skins:24;wallet:31']]),calls=[],messages=[],timers=new Map(),events={};let timerId=0,offline=false;
const doc={body:new Element('body'),head:new Element('head'),createElement:t=>new Element(t)};
const snapshot={schema:2,moves:1427,restarts:31,stars:100,wallet:31,spentStars:69,stages:[],records:[]};
const window={degulLanguage:'en',addEventListener:(k,f)=>events[k]=f,unityInstance:{SendMessage:(target,name,value)=>{messages.push({name,value});if(name==='SetCommunityConsent'&&value==='granted')window.mallangCommunity.snapshot(JSON.stringify(snapshot));}}};
const context=vm.createContext({window,document:doc,crypto:webcrypto,Uint8Array,console,AbortController,localStorage:{getItem:k=>stored.get(k),setItem:(k,v)=>stored.set(k,v)},setTimeout:fn=>{timers.set(++timerId,fn);return timerId;},clearTimeout:id=>timers.delete(id),fetch:async(url,options)=>{
 calls.push({url,options});if(offline)throw Error('offline');
 return {ok:true,json:async()=>url.endsWith('/ranking')?{rows:[{label:'ItchTester',stars:100,moves:1427,restarts:31}]}:url.endsWith('/tiers')?{rows:[]}:{ok:true,label:'ItchTester',sequence:1}};
}});
for(const file of ['ranking-config.js','ranking-i18n.js','community.js'])vm.runInContext(readFileSync(new URL(file,root),'utf8'),context);
const click=text=>{const b=doc.body.all().find(e=>e.tag==='button'&&e.textContent===text);assert.ok(b,text);return b.onclick();};
const settle=async()=>{for(let i=0;i<30;i++)await Promise.resolve();};
const api=window.mallangCommunity;api.ready('Puzzle');assert.equal(calls.length,0,'no account/records transmitted before consent');
await click('Play without joining');assert.equal(calls.length,0);
await api.open();assert.equal(calls.at(-1).url,'https://mallang-orbit-puzzle.lde0707.chatgpt.site/api/community/ranking');
click('Choose a name and join');doc.body.all().find(e=>e.tag==='input').value='ItchTester';await click('Join with this name');await settle();
await api.open();
const sent=calls.filter(r=>r.url.endsWith('/sync'));assert.ok(sent.length);assert.equal(JSON.parse(sent[0].options.body).snapshot.stars,100,'already-earned and spent stars sent on consent without replay');
assert.ok(calls.filter(r=>r.options?.method==='POST').every(r=>r.options.credentials==='omit'));
assert.equal(stored.get('unrelated-game-save'),'stages:12;skins:24;wallet:31');
assert.deepEqual([...stored.keys()].sort(),['degul-itch-5285502-community-v1','unrelated-game-save']);
assert.ok(doc.body.all().some(e=>e.tag==='td'&&e.textContent==='100'));
const id=JSON.parse(stored.get('degul-itch-5285502-community-v1')).id;
offline=true;api.snapshot(JSON.stringify({...snapshot,stars:103}));await api.open();
assert.ok(doc.body.all().some(e=>e.className==='notice'&&e.textContent.includes('Could not load')));
assert.equal(JSON.parse(stored.get('degul-itch-5285502-community-v1')).id,id);
offline=false;await click('Stop sending records');assert.equal(api.hasConsent(),false);
assert.equal(JSON.parse(stored.get('degul-itch-5285502-community-v1')).id,id);
assert.ok(!calls.some(r=>r.url.endsWith('/withdraw')));
assert.equal(stored.get('unrelated-game-save'),'stages:12;skins:24;wallet:31');
for(const lang of ['en','zh','ja','ko']){window.degulLanguage=lang;assert.ok(window.degulRankingT('별친구 랭킹'));}
console.log('PASS packaged itch client: opt-in only, production API routing, prior stars sync, scoped identity, all save keys preserved, offline retention, withdrawal and four-language labels.');

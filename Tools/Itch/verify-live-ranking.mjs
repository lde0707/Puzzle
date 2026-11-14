// Read-only production smoke check; player deletion is retired. Exercise writes
// against the in-memory SQLite suite instead of leaving QA players in rankings.
import assert from 'node:assert/strict';
import {randomBytes} from 'node:crypto';
const base='https://mallang-orbit-puzzle.lde0707.chatgpt.site/api/community/',origin='https://html-classic.itch.zone';
const identity={id:randomBytes(16).toString('hex'),token:randomBytes(32).toString('hex')};
const label='QA_itch_'+identity.id.slice(0,4);
async function post(path,extra={}){const r=await fetch(base+path,{method:'POST',headers:{origin,'content-type':'application/json'},body:JSON.stringify({...identity,...extra})});assert.equal(r.headers.get('access-control-allow-origin'),origin);assert.equal(r.status,200,await r.clone().text());return r.json();}
async function ranking(){const r=await fetch(base+'ranking',{headers:{origin}});assert.equal(r.status,200);return (await r.json()).rows;}
const preflight=await fetch(base+'sync',{method:'OPTIONS',headers:{origin,'access-control-request-method':'POST','access-control-request-headers':'content-type'}});
assert.equal(preflight.status,204);assert.equal(preflight.headers.get('access-control-allow-origin'),origin);
assert.ok(Array.isArray(await ranking()));
for(const method of ['GET','POST','DELETE']){
 const r=await fetch(base+'withdraw',{method,headers:{origin,'content-type':'application/json'},...(method==='POST'?{body:JSON.stringify(identity)}:{})});
 assert.equal(r.status,403);assert.equal((await r.json()).error,'player_deletion_disabled');
 assert.equal(r.headers.get('access-control-allow-origin'),origin);
}
const client=await (await fetch(new URL('/community.js',base))).text();
assert.ok(client.includes('pauseParticipation'));assert.ok(!client.includes("post('withdraw')"));
assert.ok(client.includes('match(/\\p{L}/gu)'));
console.log('PASS live ranking/CORS/preflight, all deletion methods blocked and updated account client served. No player writes.');

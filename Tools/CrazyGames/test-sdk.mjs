import assert from 'node:assert/strict';
import {createCrazyBridge,checksum} from './crazy-bridge.mjs';
const payload=JSON.stringify({levelId:'1A',dataVersion:'v',blockIds:[0],current:{moves:0}});
const raw=JSON.stringify({version:1,sequence:1,payload,checksum:checksum(payload)});
function fixture(options={}){
  const data=new Map(),events=[],messages=[],timers=new Map();let ad,auth,settings,id=0,writes=0;
  const sdk={init:async()=>{if(options.initError)throw Error('init');},
    data:{getItem:k=>{if(options.readError)throw Error('read');return data.get(k)??null;},setItem:(k,v)=>{if(options.writeError)throw Error('quota');writes++;data.set(k,v);}},
    game:{settings:{muteAudio:true},addSettingsChangeListener:f=>settings=f,loadingStart:()=>events.push('loadingStart'),loadingStop:()=>events.push('loadingStop'),gameplayStart:()=>events.push('start'),gameplayStop:()=>events.push('stop')},
    user:{addAuthListener:f=>auth=f},ad:{requestAd:(type,callbacks)=>{assert.equal(type,'rewarded');ad=callbacks;}}
  };
  const api=createCrazyBridge({sdk,monetization:options.ads??true,send:(...args)=>messages.push(args),reload:()=>events.push('reload'),schedule:fn=>{timers.set(++id,fn);return id;},cancel:id=>timers.delete(id)});
  return {api,data,events,messages,timers,get ad(){return ad;},get writes(){return writes;},auth:()=>auth(),settings:s=>settings(s)};
}
let f=fixture();f.data.set('degul.progress.v1',raw);await f.api.init();assert.equal(f.api.read('degul.progress.v1'),raw);
f.api.ready('Game');assert.equal(f.api.muted(),true);f.settings({muteAudio:false});assert.equal(f.api.muted(),false);
f.api.playing(true);f.api.playing(true);assert.equal(f.events.filter(x=>x==='start').length,1);
f.api.reward('Game','one');assert.equal(f.api.canReward(),false);f.ad.adStarted();f.ad.adFinished();f.ad.adFinished();f.ad.adError({code:'unfilled'});
assert.equal(f.messages.filter(x=>x[1]==='PortalAdResult'&&x[2]==='one|finished').length,1);
f.api.reward('Game','two');f.ad.adError({code:'unfilled'});f.ad.adFinished();assert(!f.messages.some(x=>x[2]==='two|finished'));assert(f.api.canReward());
f.api.reward('Game','three');f.ad.adError({code:'adsDisabled'});assert(!f.api.canReward());assert(!f.messages.some(x=>x[2]==='three|finished'));
f.auth();assert(!f.api.write('degul.progress.v1',raw));assert(f.events.includes('reload'));
f=fixture({ads:false});await f.api.init();f.api.reward('Game','basic');assert.equal(f.ad,undefined);assert(!f.api.canReward());
f=fixture();await f.api.init();f.api.reward('Game','timeout');const late=f.ad;[...f.timers.values()][0]();late.adStarted();late.adFinished();assert.deepEqual(f.messages.filter(x=>x[1]==='PortalAdResult').map(x=>x[2]),['timeout|timeout']);assert.deepEqual(f.messages.filter(x=>x[1]==='SetPortalAdAudio').map(x=>x[2]),['0','1','0']);assert(!f.api.canReward());
f=fixture({writeError:true});await f.api.init();assert(!f.api.write('degul.progress.v1',raw));assert.equal(f.api.read('degul.progress.v1'),null);
for(const opts of [{initError:true},{readError:true}]){f=fixture(opts);await assert.rejects(f.api.init());assert(!f.api.write('degul.progress.v1',raw));assert.equal(f.writes,0);}
f=fixture();f.data.set('degul.progress.v1','broken');await assert.rejects(f.api.init());assert.equal(f.writes,0);
f=fixture();await f.api.init();assert(f.api.write('degul.progress.v1',raw));assert(!f.api.write('unexpected','anything'));assert(!f.api.write('degul.progress.v1',raw.replace('1A','2A')));
console.log('PASS SDK: read before write; corrupt/error storage fail closed; Basic ads off; reward exactly once; error/timeout no reward; account reload; mute and gameplay events.');

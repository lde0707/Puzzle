const PROGRESS='degul.progress.v1';
const KEYS=[PROGRESS,'degul.before-import.v1','degul.language.v1'];
export function checksum(value){let h=2166136261;for(let i=0;i<value.length;i++)h=Math.imul(h^value.charCodeAt(i),16777619);return (h>>>0).toString(16).padStart(8,'0');}
export function validateSave(raw){
  if(!raw)return;
  const e=JSON.parse(raw);
  if(e.version!==1||!Number.isSafeInteger(e.sequence)||e.sequence<1||typeof e.payload!=='string'||checksum(e.payload)!==e.checksum)throw Error('Invalid saved progress. It has not been replaced.');
  const p=JSON.parse(e.payload);
  if(!p||typeof p.levelId!=='string'||!p.levelId||typeof p.dataVersion!=='string'||!Array.isArray(p.blockIds)||!p.current)throw Error('Incomplete saved progress. It has not been replaced.');
}

// SDK is injected for deterministic tests; production always uses the official v3 SDK.
export function createCrazyBridge({sdk,monetization=false,send=()=>{},status=()=>{},reload=()=>{},schedule=setTimeout,cancel=clearTimeout}){
  let initialized=false,blocked=false,target='',running=false,pending=null,adsAllowed=monetization,mute=false,writeHadError=false;
  const values=new Map();
  const notify=(method,value)=>{if(target)send(target,method,String(value));};
  const muteChanged=settings=>{mute=!!settings?.muteAudio;notify('SetPortalMute',mute?'1':'0');};
  const api={
    async init(){
      await sdk.init();
      // Read everything before Unity starts; SDK errors must not become a new save.
      for(const key of KEYS){const value=sdk.data.getItem(key);if(value!==null&&value!==undefined&&typeof value!=='string')throw Error('Unsupported save data');values.set(key,value??null);}
      validateSave(values.get(PROGRESS));validateSave(values.get('degul.before-import.v1'));
      initialized=true;
      muteChanged(sdk.game.settings);
      sdk.game.addSettingsChangeListener(muteChanged);
      sdk.user.addAuthListener(()=>{blocked=true;api.playing(false);status('Account changed. Reloading your progress…',true);reload();});
      sdk.game.loadingStart();
    },
    read(key){if(!initialized||blocked)throw Error('CrazyGames storage is not ready');return values.get(key)??null;},
    write(key,value){
      if(!initialized||blocked||!KEYS.includes(key))return false;
      try{
        if(key!== 'degul.language.v1')validateSave(value);
        const next=Object.fromEntries(values);next[key]=value;
        if(new TextEncoder().encode(JSON.stringify(next)).length>900000)throw Error('Save exceeds the cloud storage budget');
        sdk.data.setItem(key,value);values.set(key,value);if(writeHadError){writeHadError=false;status('',false);}return true;
      }catch(e){writeHadError=true;status('Could not save progress. Please keep this page open and try again.',false);return false;}
    },
    ready(name){target=name;notify('SetPortalMute',mute?'1':'0');sdk.game.loadingStop();},
    playing(value){
      value=!!value&&!blocked&&!pending;
      if(!initialized||value===running)return;
      running=value;
      try{sdk.game[value?'gameplayStart':'gameplayStop']();}catch{}
    },
    canReward(){return initialized&&!blocked&&adsAllowed&&!pending;},
    muted(){return mute;},
    reward(receiver,token){
      if(!api.canReward()){send(receiver,'PortalAdResult',token+'|unavailable');return;}
      const request={receiver,token,done:false,started:false,late:false,timer:null};pending=request;api.playing(false);
      const finish=result=>{
        if(request.done){if(request.late){request.late=false;send(receiver,'SetPortalAdAudio','0');}return;}
        request.done=true;cancel(request.timer);pending=null;
        send(receiver,'SetPortalAdAudio','0');
        send(receiver,'PortalAdResult',token+'|'+result);
      };
      request.timer=schedule(()=>{if(!request.started){adsAllowed=false;finish('timeout');}},45000);
      try{
        sdk.ad.requestAd('rewarded',{
          adStarted(){if(request.done){request.late=true;send(receiver,'SetPortalAdAudio','1');return;}request.started=true;cancel(request.timer);send(receiver,'SetPortalAdAudio','1');},
          adFinished(){finish('finished');},
          adError(error){if(['adsDisabled','adsDisabledBasicLaunch'].includes(error?.code))adsAllowed=false;finish(error?.code==='adblock'?'adblock':'unavailable');}
        });
      }catch{finish('unavailable');}
    }
  };
  return api;
}

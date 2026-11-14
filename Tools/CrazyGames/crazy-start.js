import {createCrazyBridge} from './crazy-bridge.mjs';
const status=(message,blocking)=>{
  let node=document.getElementById('portal-message');
  if(!node){node=document.createElement('div');node.id='portal-message';node.setAttribute('role','status');node.style.cssText='position:fixed;z-index:500;bottom:0;left:0;right:0;padding:14px;background:#fff5df;color:#453354;text-align:center;font:14px system-ui';document.body.append(node);}
  node.textContent=message;node.hidden=!message;if(blocking)node.style.inset='0';
};
const send=(target,method,value)=>window.unityInstance?.SendMessage(target,method,value);
window.degulCG=createCrazyBridge({sdk:window.CrazyGames?.SDK,monetization:window.DEGUL_CG_MONETIZATION===true,send,status,reload:()=>location.reload()});
try{
  await window.degulCG.init();
  await import('./game-loader.js');
}catch(error){
  document.getElementById('error').textContent='Could not connect to CrazyGames saves. Your progress has not been replaced. Check your connection and the Data Module submission setting, then retry.';
  document.getElementById('retry').hidden=false;
  document.getElementById('retry').onclick=()=>location.reload();
  console.error('CrazyGames initialization failed:',error?.code||error?.message);
}

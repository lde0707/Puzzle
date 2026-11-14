import {decodeBackup} from './backup-core.js';
const panel=document.createElement('section');panel.id='backup-panel';panel.hidden=true;
panel.setAttribute('role','dialog');panel.setAttribute('aria-modal','true');panel.setAttribute('aria-label','기록 옮기기');document.body.append(panel);
const style=document.createElement('style');style.textContent='#backup-panel[hidden]{display:none}#backup-panel{position:fixed;z-index:110;inset:0;padding:24px max(20px,calc((100vw - 650px)/2));overflow:auto;box-sizing:border-box;background:#faf5fffa;color:#453354;font:16px system-ui;line-height:1.7}#backup-panel button{border:0;border-radius:16px;background:#e4daf6;padding:14px 18px;margin:5px;color:#453354;font:inherit;cursor:pointer}#backup-panel .primary{background:#ffd49b}#backup-panel input{display:block;margin:18px 0;max-width:100%;font:inherit}#backup-panel .notice{padding:16px;background:white;border-radius:16px}#backup-panel textarea{display:block;box-sizing:border-box;width:100%;height:100px;margin:12px 0;font:12px monospace}#backup-panel button:focus-visible{outline:3px solid #9964cc}';document.head.append(style);
let target='',current='',previous='',revision=0,focus;
const el=(tag,value,parent=panel)=>{const e=document.createElement(tag);e.textContent=window.degulT(value??'');parent.append(e);return e;};
function button(value,fn,primary=false){const b=el('button',value);b.onclick=fn;if(primary)b.className='primary';return b;}
function close(){panel.hidden=true;focus?.focus?.();}
function shell(title){revision++;if(panel.hidden)focus=document.activeElement;panel.hidden=false;panel.replaceChildren();el('h1',title);button('게임으로 돌아가기',close);}
function status(value){let e=panel.querySelector('.notice');if(!e)e=el('p');e.className='notice';e.textContent=window.degulT(value);}
function summary(s){
  const values={en:`Cleared ${s.completed} ${s.completed===1?'stage':'stages'} · ${s.skins} skins · ${s.wallet} ${s.wallet===1?'star':'stars'}`,ko:`완료 ${s.completed}단계 · 보유 스킨 ${s.skins}개 · 보유 별 ${s.wallet}개`,zh:`已完成 ${s.completed} 关 · ${s.skins} 款皮肤 · ${s.wallet} 颗星`,ja:`${s.completed}ステージクリア · スキン${s.skins}個 · 星${s.wallet}個`};
  return values[window.degulLanguage]??values.en;
}
function download(raw,name){
  const url=URL.createObjectURL(new Blob([raw],{type:'application/json'}));const a=el('a');a.href=url;a.download=name;a.click();a.remove();setTimeout(()=>URL.revokeObjectURL(url),60000);
  status('저장 파일 다운로드를 요청했어요. 다운로드가 막히면 아래 저장 코드를 복사해서 보관하세요.');
}
function review(raw){
  try{
    const data=decodeBackup(raw);shell('이어받을 기록을 확인해 주세요');el('p',summary(data)).className='notice';
    el('p','단계별 최고 별과 보유 스킨을 합칩니다. 보유 별을 단순히 더하지 않으며, 누적 기록은 더 큰 값을 유지해요. 현재 기록은 먼저 내보내서 보관해 주세요.');
    button('현재 기록을 먼저 내려받기',()=>download(current,'degul-before-import.json'));
    const apply=button('이 기록 이어받기',()=>{apply.disabled=true;status('기록을 저장하고 있어요…');window.unityInstance.SendMessage(target,'ImportProgressBackup',raw);},true);
    button('취소',main);
  }catch(e){status(e.message||'저장 파일을 읽지 못했어요.');}
}
function main(){
  shell('기록을 함께 데려가요');
  el('p','단계·스킨·별·설정을 파일 또는 저장 코드로 보관하고 다른 주소나 기기에서 이어받아요. 이 기능은 파일을 서버에 업로드하지 않아요.');
  try{el('p','현재 기록: '+summary(decodeBackup(current))).className='notice';}catch{}
  button('현재 기록 내보내기',()=>download(current,'degul-progress.json'),true);
  if(previous)button('불러오기 전 기록 내보내기',()=>download(previous,'degul-before-import.json'));
  el('h2','파일로 불러오기');const label=el('label','JSON 저장 파일');label.htmlFor='degul-import';
  const file=el('input');file.id='degul-import';file.type='file';file.accept='.json,application/json';
  file.onchange=async()=>{const chosen=file.files?.[0];if(!chosen)return;if(chosen.size>1048576){status('1MB 이하 저장 파일을 골라 주세요.');return;}const r=revision;try{const raw=await chosen.text();if(r===revision)review(raw);}catch{status('파일을 읽지 못했어요.');}};
  const details=el('details');el('summary','파일 저장이 안 될 때: 저장 코드 복사 / 붙여넣기',details);
  const exportLabel=el('label','내 저장 코드',details);exportLabel.htmlFor='degul-export-code';const output=el('textarea','',details);output.id='degul-export-code';output.readOnly=true;output.value=current;
  const select=el('button','내 코드 전체 선택',details);select.onclick=()=>{output.focus();output.select();};
  el('p','전체 선택 후 Ctrl+C 또는 휴대폰의 복사 메뉴를 사용해 개인 메모에 보관하세요. 저장 코드에는 플레이 기록이 들어 있으니 공개 댓글에는 붙이지 마세요.',details);
  const importLabel=el('label','옮길 저장 코드 붙여넣기',details);importLabel.htmlFor='degul-import-code';const input=el('textarea','',details);input.id='degul-import-code';input.maxLength=1048576;
  const check=el('button','붙여넣은 기록 확인',details);check.onclick=()=>review(input.value.trim());
  el('p','기존 공개 링크에서도 설정 → 기록 옮기기에서 파일을 내보낼 수 있어요. itch.io 로그인만으로 게임 기록이 자동 동기화되지는 않아요.');
  panel.querySelector('button').focus();
}
panel.addEventListener('keydown',e=>{if(e.key==='Escape')close();if(e.key==='Tab'){const a=[...panel.querySelectorAll('button:not([disabled]),input,textarea,summary')].filter(x=>x.getClientRects().length);if(a.length&&(e.shiftKey&&document.activeElement===a[0]||!e.shiftKey&&document.activeElement===a.at(-1))){e.preventDefault();(e.shiftKey?a.at(-1):a[0]).focus();}}});
window.mallangBackup={open(name,raw,before){target=name;current=raw;previous=before;main();},result(text){shell('기록 이어받기');status(text);button('게임으로 돌아가기',close,true);}};

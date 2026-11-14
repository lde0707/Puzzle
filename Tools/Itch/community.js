// itch.io standalone edition: no identity, telemetry, ad or API requests.
(() => {
  const panel=document.createElement('section');
  panel.id='community-panel'; panel.hidden=true;
  panel.setAttribute('role','dialog');panel.setAttribute('aria-modal','true');
  panel.setAttribute('aria-label','itch.io 기록 안내');document.body.append(panel);
  const style=document.createElement('style');
  style.textContent='#community-panel[hidden]{display:none}#community-panel{position:fixed;z-index:100;inset:0;overflow:auto;box-sizing:border-box;padding:28px max(20px,calc((100vw - 600px)/2));background:#faf5fff9;color:#453354;font:16px system-ui;line-height:1.8}#community-panel button{border:0;border-radius:16px;padding:14px 22px;background:#e5d7f6;color:#453354;font:inherit;cursor:pointer}#community-panel button:focus-visible{outline:3px solid #9363c6}';
  document.head.append(style);
  let focus,gameTarget='';
  function close(){panel.hidden=true;focus?.focus?.();}
  function text(tag,value){const e=document.createElement(tag);e.textContent=window.degulT(value);panel.append(e);return e;}
  window.mallangCommunity={
    hasConsent:()=>false,
    snapshot(){},
    ready(receiver){
      gameTarget=receiver;
      const notify=()=>{if(!window.unityInstance){setTimeout(notify,100);return;}window.unityInstance.SendMessage(receiver,'SetCommunityConsent','denied');};
      notify();
    },
    tiers(receiver){window.unityInstance?.SendMessage(receiver,'ReceiveStageTiers','{"rows":[]}');},
    open(){
      focus=document.activeElement;panel.replaceChildren();panel.hidden=false;
      text('h1','나의 데굴데굴 기록');
      text('p','이 itch.io 버전은 광고와 온라인 랭킹 없이 플레이할 수 있어요. 게임이 닉네임이나 플레이 기록을 별도 랭킹 서버로 전송하지 않아요.');
      text('p','단계·별·스킨은 현재 브라우저에 저장돼요. 단계 선택에서 최고 별과 예상 상위 등급을, 단계 상세에서 누적 이동·초기화 횟수를 볼 수 있어요.');
      text('p','예상 상위 등급은 게임의 기본 난이도 기준이며, 다른 사람들과 비교한 실시간 순위가 아니에요.');
      text('p','다른 주소나 기기로 옮기거나 업데이트 전에 기록을 보관하려면 설정 → 기록 옮기기 · 저장 파일을 이용해 주세요. 브라우저 데이터 삭제·비공개 모드·저장 제한으로 기록이 사라질 수 있어요.');
      text('p','itch.io 자체의 접속 정보 처리는 itch.io 개인정보처리방침을 따라요.');
      const transfer=text('button','기록 옮기기 · 저장 파일');
      transfer.onclick=()=>{
        if(!gameTarget||!window.unityInstance)return;
        close();
        window.unityInstance.SendMessage(gameTarget,'OpenHomeSettings');
        window.unityInstance.SendMessage(gameTarget,'OpenBackup');
      };
      const b=text('button','게임으로 돌아가기');b.onclick=close;b.focus();
    }
  };
  panel.addEventListener('keydown',e=>{if(e.key==='Escape')close();if(e.key==='Tab'){const a=[...panel.querySelectorAll('button')];if(e.shiftKey&&document.activeElement===a[0]||!e.shiftKey&&document.activeElement===a.at(-1)){e.preventDefault();(e.shiftKey?a.at(-1):a[0]).focus();}}});
})();

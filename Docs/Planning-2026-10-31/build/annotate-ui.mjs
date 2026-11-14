import fs from 'node:fs/promises';
import {Presentation,PresentationFile} from '@oai/artifact-tool';
const p=Presentation.create({slideSize:{width:1280,height:720}});
const content=JSON.parse(await fs.readFile('content.json','utf8')).ui;
const dims=JSON.parse(await fs.readFile('image-dimensions.json','utf8'));
const color='#B1446C';
const config={};
function define(id,items,labels,details){config[id]={items,labels,details};}
// Rectangles are normalized against the visible source screenshot, not the contain frame.
// [number, image index, x, y, width, height, side, optional badge-center-y].
define(3,[[1,0,.05,.05,.9,.055,'L'],[2,0,.055,.14,.89,.42,'L'],[3,0,.12,.625,.76,.255,'L']],['1 상단 메뉴','2 로고·친구','3 시작·선택']);
define(4,[[1,0,.1,.35,.8,.135,'L'],[2,0,.1,.495,.8,.06,'L'],[3,0,.1,.565,.8,.06,'L'],[3,0,.1,.705,.8,.06,'L',null,true]],['1 사운드','2 빠른 진행','3 기록·닫기']);
define(5,[[1,0,.09,.25,.78,.033,'L'],[2,0,.3,.29,.17,.115,'R'],[3,0,.09,.29,.185,.115,'L']],['1 챕터 구획','2 카드 정보','3 카드 선택']);
define(6,[[1,0,.14,.207,.72,.277,'L'],[2,0,.09,.495,.82,.085,'L'],[3,0,.09,.67,.82,.14,'L']],['1 단계·등급','2 누적 기록','3 시작·복귀']);
define(7,[[1,0,.055,.008,.89,.127,'L'],[2,0,.06,.145,.88,.045,'L'],[3,0,.055,.69,.89,.295,'L']],['1 상단 정보','2 상태 띠','3 조작 영역']);
define(9,[[1,0,.04,.236,.92,.527,'L'],[2,0,.08,.328,.84,.315,'R'],[3,0,.1,.662,.8,.06,'L']],['1 첫 안내','2 사용 규칙','3 안내 닫기'],[
 '포탈 기능 첫 진입 때 한 번 표시한다.\n중앙 별문 규칙 버튼으로 다시 열 수 있다.',
 '빛나는 면·연결 출구·출구 막힘·외벽 규칙을 설명한다.\n실제 포탈의 입구와 연결 표시는 다음 쪽을 참고한다.',
 '알겠어요, 출발!을 누르면 닫고 퍼즐 입력을 재개한다.\n안내를 보는 동안 보드 이동을 받지 않는다.'
]);
define(10,[[1,0,.39,.47,.08,.03,'L'],[2,0,.65,.338,.08,.022,'R'],[3,0,.2,.653,.63,.025,'L']],['1 진입 면','2 연결 출구','3 이동 안내'],[
 '아래 포탈의 빛나는 왼쪽 면이 진입 면인 예시.\n블록의 옆·뒷면 접촉은 전송으로 처리하지 않는다.',
 '1번 포탈을 누르면 연결된 2번 출구를 강하게 강조한다.\n출구 강조와 실제 전송을 구분한다.',
 '보드 아래 문구로 진입·막힘을 안내한다.\n전송 연출은 입구 축소 → 숨김 → 출구 확대 순서다.'
]);
define(11,[[1,0,.635,.349,.175,.08,'R'],[2,0,.64,.309,.075,.037,'R',.29],[3,0,.545,.266,.175,.08,'L']],['1 변환대','2 현재 친구','3 목표 지점']);
define(12,[[1,0,.29,.33,.07,.032,'L'],[1,0,.5,.265,.07,.032,'R',null,true],[2,0,.23,.69,.54,.24,'L'],[3,0,.2,.65,.63,.028,'R']],['1 열림·닫힘','2 턴 입력','3 상태 안내']);
define(13,[[1,0,.06,.697,.28,.055,'L'],[2,0,.4,.681,.2,.09,'R'],[3,0,.31,.651,.4,.025,'L',.63]],['1 힌트 버튼','2 추천 방향','3 안내 문구'],[
 '단계별 새 힌트 3회. 현재 상태의 다음 방향을 찾는다.\n같은 배치에서 이미 본 힌트는 다시보기로 제공한다.',
 '추천하는 방향 버튼을 노랗게 강조한다.\n탐색 실패·막힌 상태는 횟수를 차감하지 않는다.',
 '추천 방향을 짧은 문장으로 함께 알려준다.\n힌트는 일반 별 등급에 영향 없고 퍼펙트에는 제외된다.'
]);
define(14,[[1,0,.07,.603,.86,.043,'L'],[2,0,.23,.69,.54,.24,'R'],[3,0,.17,.653,.7,.027,'L']],['1 낙서 도구','2 방향 조작','3 되돌림']);
define(15,[[1,0,.1,.30,.8,.48,'L'],[2,1,.1,.488,.8,.06,'R'],[3,1,.1,.56,.8,.06,'R']],['1 정지 메뉴','2 초기화','3 취소·복귀']);
define(16,[[1,0,.11,.306,.79,.14,'L'],[2,0,.18,.46,.64,.136,'L'],[3,0,.1,.613,.8,.203,'L']],['1 획득 등급','2 결과 안내','3 다음 행동']);
define(17,[[1,0,.11,.306,.79,.14,'L'],[2,1,.69,.02,.26,.038,'R'],[3,2,.11,.306,.79,.14,'L']],null);
define(18,[[1,0,.09,.153,.82,.263,'L'],[2,0,.09,.537,.82,.318,'L'],[3,0,.1,.88,.8,.052,'L']],['1 미리보기','2 보유 목록','3 저장·닫기']);
define(20,[[1,0,.18,.30,.65,.32,'L'],[2,1,.18,.30,.65,.32,'L'],[3,2,.1,.738,.8,.135,'L']],null);
define(21,[[1,0,.1,.24,.8,.357,'L'],[2,0,.19,.625,.62,.045,'L'],[3,0,.1,.74,.8,.132,'L']],['1 결과 카드','2 선택 상태','3 착용·보관']);
define(22,[[1,0,.055,.18,.06,.045,'L'],[2,0,.025,.363,.05,.042,'L'],[3,0,.145,.355,.075,.055,'L',.44]],['1 네모 형태','2 이동 효과','3 도착 효과']);
function frame(d,index){if(d.kind==='sequence')return [55+index*405,186,360,310];if(d.kind==='pair')return [42+index*220,185,210,460];return [58,183,400,470];}
function bounds(d,index){const name=d.image||d.images[index];const [iw,ih]=dims[name];let [x,y,w,h]=frame(d,index),scale=Math.min(w/iw,h/ih);return [x+(w-iw*scale)/2,y+(h-ih*scale)/2,iw*scale,ih*scale];}
function shape(s,name,geo,x,y,w,h,fill='none',stroke=color,weight=1.8){return s.shapes.add({name,geometry:geo,position:{left:x,top:y,width:w,height:h},fill,line:{fill:stroke,width:weight}});}
function text(s,name,value,x,y,w,h,size=17,fill=color){const q=shape(s,name,'textbox',x,y,w,h,'none','none',0);q.text=value;q.text.style={typeface:'Malgun Gothic',fontSize:size,bold:true,color:fill,insets:0,alignment:'center',verticalAlignment:'middle'};return q;}
let count=0;
for(let slide=1;slide<=31;slide++){
 const s=p.slides.add();let id=slide-2,d=content[id-1],c=config[id];if(!c)continue;
 for(let j=0;j<c.items.length;j++){
  const [num,index,rx,ry,rw,rh,side,at,silent]=c.items[j];const [x,y,w,h]=bounds(d,index);
  const tx=x+rx*w,ty=y+ry*h,tw=rw*w,th=rh*h;
  shape(s,`UI_callout_${num}_target_${j}`,'rect',tx,ty,tw,th);
  if(silent)continue;
  let bx=side==='R'?x+w+25:x-25,by=at==null?ty+th/2:y+at*h;
  if(id===22){bx=38;} if(id===15){bx=side==='R'?x+w+19:24;}
  const edge=side==='R'?tx+tw:tx,ex=side==='R'?bx-14:bx+14,ey=ty+th/2;
  // Orthogonal leader avoids adding a misleading direction arrow inside the game.
  const mx=side==='R'?Math.max(edge+7,ex-9):Math.min(edge-7,ex+9);
  shape(s,`UI_callout_${num}_lead1_${j}`,'rect',Math.min(edge,mx),ey-.7,Math.max(.5,Math.abs(mx-edge)),1.4,color,'none',0);
  if(Math.abs(by-ey)>.1)shape(s,`UI_callout_${num}_lead2_${j}`,'rect',mx-.7,Math.min(by,ey),1.4,Math.abs(by-ey),color,'none',0);
  shape(s,`UI_callout_${num}_lead3_${j}`,'rect',Math.min(mx,ex),by-.7,Math.max(.5,Math.abs(mx-ex)),1.4,color,'none',0);
  shape(s,`UI_callout_${num}_badge_${j}`,'ellipse',bx-14,by-14,28,28,color,'#FFFFFF',1.5);
  text(s,`UI_callout_${num}_number_${j}`,String(num),bx-14,by-14,28,28,18,'#FFFFFF');count++;
 }
}
await(await PresentationFile.exportPptx(p)).save('ui-number-overlays.pptx');
await fs.writeFile('ui-annotation-map.json',JSON.stringify(config,null,2));
console.log(JSON.stringify({annotatedSlides:Object.keys(config).length,numberBadges:count}));


import fs from 'node:fs/promises';
import path from 'node:path';
import {Presentation,PresentationFile} from '@oai/artifact-tool';
const W=path.resolve('..'),media=path.join(W,'media');
const all=JSON.parse(await fs.readFile('content.json','utf8'));
const imageBytes=new Map();for(const n of await fs.readdir(media))if(n.endsWith('.png'))imageBytes.set(n,new Uint8Array(await fs.readFile(path.join(media,n))));
const ink='#403359',muted='#746A82',purple='#7760A1',cream='#FCFAF6',lav='#EEE7F6',peach='#F7E4D4';
function txt(s,v,x,y,w,h,size=24,color=ink,bold=false){let q=s.shapes.add({geometry:'textbox',position:{left:x,top:y,width:w,height:h},fill:'none',line:{fill:'none',width:0}});q.text=v;q.text.style={typeface:'Malgun Gothic',fontSize:size,color,bold,insets:0,verticalAlignment:'middle'};return q;}
function box(s,x,y,w,h,fill=lav){return s.shapes.add({geometry:'roundRect',position:{left:x,top:y,width:w,height:h},fill,line:{fill:'none',width:0},borderRadius:14});}
function pic(s,name,x,y,w,h){if(!imageBytes.has(name))throw new Error('Missing image '+name);s.images.add({blob:imageBytes.get(name),contentType:'image/png',alt:name,fit:'contain',position:{left:x,top:y,width:w,height:h}});}
function header(s,d,n,type){s.background.fill=cream;txt(s,`${type==='ui'?'UI 기획':'SYSTEM 기획'}  /  ${d.id}  /  ${d.status}`,48,23,1170,30,17,purple,true);txt(s,d.title,48,65,1180,57,36,ink,true);txt(s,d.subtitle,48,128,1180,50,20,muted);txt(s,'말랑궤도  ·  2026.10.31  ·  근거와 상세 주의사항: 발표자 노트',48,678,1110,24,14,muted);txt(s,String(n).padStart(2,'0'),1170,676,64,28,18,purple,true);}
function table(s,headers,rows,x,y,w,h,widths,size=23){let vals=[headers,...rows];let rh=(h-42)/rows.length;let t=s.tables.add({rows:vals.length,columns:headers.length,left:x,top:y,width:w,height:h,columnWidths:widths,values:vals});t.borders.assign({fill:'#DFD7E8',width:1});for(let r=0;r<vals.length;r++){t.rows[r].height=r===0?42:rh;for(let c=0;c<headers.length;c++){let cell=t.getCell(r,c);cell.fill=r===0?purple:c===0?lav:'#FFFFFF';cell.text.style={typeface:'Malgun Gothic',fontSize:r===0?18:size,color:r===0?'#FFFFFF':ink,bold:r===0||c===0,insets:{left:14,right:14,top:9,bottom:9},verticalAlignment:'middle'};}}return t;}
for(const type of ['ui','system']){
 const pages=all[type],p=Presentation.create({slideSize:{width:1280,height:720}}),owners=[];
 let s=p.slides.add();s.background.fill=cream;txt(s,'MALLANG ORBIT  /  DESIGN SPECIFICATION',64,50,1140,30,18,purple,true);
 pic(s,'wordmark.png',55,105,665,220);txt(s,type==='ui'?'UI 기획서':'System 기획서',64,346,720,85,60,ink,true);
 txt(s,type==='ui'?'화면 구성 · 버튼 동작 · 예외 · 시청각 연출':'처리 규칙 · 계산식 · 상태 전이 · 저장 · 검수',68,447,740,45,25,muted);
 txt(s,'64개 단계 · 40종 스킨 · 모바일 중력 퍼즐\n현재 구현과 확정 요구를 함께 정리한 개발 문서',68,524,730,80,24,ink);
 pic(s,type==='ui'?'stage-detail-matte-390x844.png':'blink-board.png',850,65,335,580);
 txt(s,'v1.0  |  기준일 2026.10.31',68,660,1100,30,18,purple);
 s.speakerNotes.text='말랑궤도 현재 Unity/Web 구현 기준. UI 기획서 참고: Rushitadel UI기획서.pptx의 화면+번호별 Description 구조. 폰트와 색상은 말랑궤도 문서 가독성을 위해 별도 설계. 게임 수정/배포 없음. 표지 화면은 격리된 테스트 데이터.';
 s=p.slides.add();s.background.fill=cream;txt(s,'문서 탐색',48,45,1150,60,40,ink,true);
 txt(s,'문서 ID로 검색하거나 오른쪽 페이지 번호로 이동하세요.',48,113,1150,40,22,muted);
 let groups=type==='ui'?[[0,5,'시작·선택'],[6,13,'플레이·장치·힌트·낙서'],[14,20,'정지·클리어·옷장·뽑기'],[21,25,'스킨·랭킹·백업·공통 연출'],[26,27,'검수·수정 항목']]:[[0,9,'플레이 규칙과 특수 장치'],[10,13,'캠페인·난이도·Undo·힌트'],[14,20,'별 경제·뽑기·착용'],[21,25,'연출·낙서·저장'],[26,33,'통계·서버·랭킹·플랫폼·검수'],[34,41,'부록: 64단계·40스킨 데이터']];
 groups.forEach(([a,b,label],i)=>{let y=190+i*65;box(s,48,y,1180,54,i%2?lav:'#F5EEE7');txt(s,label,66,y+7,750,40,25,ink,true);txt(s,`${pages[a].id}–${pages[b].id}   ·   ${a+3}–${b+3}쪽`,840,y+7,370,40,21,purple);});
 txt(s,'시청각 자료 열기',48,625,1150,36,22,purple,true);txt(s,'동봉된 「시청각 자료.html」에서 실제 효과음과 장면별 이미지를 확인합니다.',48,663,1150,25,17,muted);
 s.speakerNotes.text='본문에서 현행/요구/제안/미구현을 구분. 시청각 자료는 같은 폴더의 HTML 및 media 디렉터리. 문서의 테스트값은 실제 사용자의 기록이 아님.';
 for(let i=0;i<pages.length;i++){
  const d=pages[i];s=p.slides.add();header(s,d,i+3,type);
  if(d.kind==='sequence'){
   d.images.forEach((im,k)=>{pic(s,im,55+k*405,186,360,310);box(s,55+k*405,503,360,39);txt(s,d.rows[k][0],70+k*405,506,330,32,22,purple,true);txt(s,d.rows[k][1],55+k*405,552,367,98,22,ink);});
  }else if(d.kind==='pair'){
   d.images.forEach((im,k)=>pic(s,im,42+k*220,185,210,460));
   table(s,d.headers,d.rows,510,195,718,440,[154,564],22);owners.push(i+3);
  }else if(d.kind==='flow'){
   d.rows.forEach((r,k)=>{let y=198+k*88;box(s,48,y,236,68,k%2?peach:lav);txt(s,`${String(k+1).padStart(2,'0')}  ${r[0]}`,64,y+10,205,48,26,purple,true);txt(s,'→',301,y+16,50,40,30,purple,true);txt(s,r[1],371,y+6,820,58,26,ink);});
  }else if(d.kind==='catalog'){
   if(d.rows.length===16){for(let k=0;k<2;k++)table(s,d.headers,d.rows.slice(k*8,k*8+8),48+k*600,196,580,452,[190,130,130,130],22);}
   else table(s,d.headers,d.rows,48,196,1180,452,[90,290,390,410],22);
   owners.push(i+3);
  }else if(d.image){
   pic(s,d.image,58,183,400,470);
   txt(s,'화면 예시 · 테스트 데이터',58,650,400,22,14,muted);
   table(s,d.headers,d.rows,450,198,778,445,[150,628],22);owners.push(i+3);
  }else{
   table(s,d.headers,d.rows,48,198,1180,445,d.id==='SYS-16'?[540,640]:[220,960],25);owners.push(i+3);
  }
  s.speakerNotes.text=`${d.id} ${d.title}\n상태: ${d.status}\n${d.note}\n기준: 로컬 소스 2026.10.31. 검수 항목은 요구 명세이며 모든 실기기 검수가 끝났다는 뜻이 아님.\n`+d.refs.map(x=>`${x.path}\nSHA256 ${x.sha256||'확인 필요'}`).join('\n')+`\n이미지: ${[d.image,...d.images].filter(Boolean).join(', ')}. 최신 격리 렌더 또는 2026.10.31 감사에 사용한 Unity 렌더. 사용자 데이터 미포함.\n관련 시청각 자료: 시청각 자료.html`;
 }
 s=p.slides.add();header(s,{id:'AV',status:'실제 오디오',title:'시청각 예시 · 눌러서 듣기',subtitle:'슬라이드 쇼에서 스피커 아이콘을 눌러 재생합니다. 별도 HTML 자료에서도 들을 수 있습니다.'},p.slides.items.length,type);
 [['home.png','배경음악','잔잔한 48초 루프 · 게임 기본 볼륨 0.075'],['mochi-clear-3-finale.png','클리어','별 등장 → 수집 → 마지막 강조음'],['candy-draw-reward.png','별사탕 캡슐','별 모으기 → 열림 → 새 스킨 등장']].forEach(([im,title,desc],i)=>{pic(s,im,58+i*405,190,350,306);txt(s,title,58+i*405,512,350,37,26,purple,true);txt(s,desc,58+i*405,552,350,48,18,muted);txt(s,'재생',130+i*405,614,260,31,21,ink);});
 s.speakerNotes.text='동봉 WAV: mallang-lullaby.wav는 프로젝트 원본. clear-sequence.wav/draw-sequence.wav는 Unity AudioClip에서 추출한 클립을 코드 이벤트 순서대로 배열한 청음 예시이며 실시간 화면 녹화는 아님. PowerPoint 삽입 오디오와 media 폴더 원본을 함께 제공.';
 await fs.mkdir(`${type}-renders`,{recursive:true});await(await PresentationFile.exportPptx(p)).save(`${type}-candidate.pptx`);console.log('WRITTEN',type,p.slides.items.length);
 for(let i=0;i<p.slides.items.length;i++){await fs.writeFile(`${type}-renders/${String(i+1).padStart(2,'0')}.png`,new Uint8Array(await(await p.export({slide:p.slides.items[i],format:'png',scale:1})).arrayBuffer()));if(i%10===0)console.log('RENDER',type,i+1);}
 await fs.writeFile(`${type}-meta.json`,JSON.stringify({slides:p.slides.items.length,owners}));
}

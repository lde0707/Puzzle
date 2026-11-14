from pathlib import Path
import wave,numpy as np,json,hashlib,html
W=Path(__file__).resolve().parents[1]; M=W/'media'
def mix(name,cues,length):
 rate=44100;y=np.zeros(int(length*rate))
 for t,n in cues:
  with wave.open(str(M/n),'rb') as f:d=np.frombuffer(f.readframes(f.getnframes()),dtype='<i2').astype(float)/32768
  a=int(t*rate);n=min(len(d),len(y)-a);y[a:a+n]+=d[:n]
 with wave.open(str(M/name),'wb') as f:f.setnchannels(1);f.setsampwidth(2);f.setframerate(rate);f.writeframes((np.clip(y,-1,1)*32767).astype('<i2').tobytes())
mix('draw-sequence.wav',[(.28,'draw-cue-0.wav'),(.56,'draw-cue-1.wav'),(.84,'draw-cue-2.wav'),(1.68,'draw-cue-3.wav'),(1.9,'draw-cue-4.wav'),(3.15,'draw-cue-5.wav')],4.4)
mix('clear-sequence.wav',[(.18/1.3,'clear-note-0.wav'),(.52/1.3,'clear-note-1.wav'),(.86/1.3,'clear-note-2.wav'),(1.6,'clear-note-3.wav'),(1.8,'clear-note-4.wav'),(2,'clear-note-5.wav'),(2.25,'clear-finale.wav')],3.2)
cards=[('잔잔한 배경음악','mallang-lullaby.wav','프로젝트 원본 48초 루프. 게임에서는 낮은 볼륨 0.075로 재생.','home.png'),('클리어 상승음','clear-sequence.wav','실제 Unity 클립. 등장 3음 → 수집 3음 → 마지막 화음. 수집 시점은 설명용 예시.','mochi-clear-3-finale.png'),('별사탕 캡슐','draw-sequence.wav','실제 클립을 코드의 이벤트 시간대로 배열. 실시간 플레이 녹화가 아닌 청음 예시.','candy-draw-reward.png')]
body=''.join(f'<article><img src="media/{im}" alt="{title}"><div><h2>{title}</h2><p>{desc}</p><audio controls preload="metadata" src="media/{audio}"></audio></div></article>' for title,audio,desc,im in cards)
body+='<h2>장면별 비교</h2><p>프레임을 바꾸어 시각적 변화를 확인하세요. 장면별 렌더이며 실제 프레임 간 동작을 녹화한 영상은 아닙니다.</p>'
for title,files in [('클리어 별',['mochi-clear-3-first.png','mochi-clear-3-pop.png','mochi-clear-3-finale.png']),('캡슐',['candy-draw-capsule.png','candy-draw-burst.png','candy-draw-reward.png'])]:
 body+=f'<section><h2>{title}</h2><div class="frames">'+''.join(f'<figure><img src="media/{n}" alt="{title} {i+1}"><figcaption>{i+1} / {len(files)}</figcaption></figure>' for i,n in enumerate(files))+'</div></section>'
body+='<h2>화면 자료</h2><div class="gallery">'+''.join(f'<a href="media/{n}.png"><img loading="lazy" src="media/{n}.png" alt="{n}"><span>{n}</span></a>' for n in ['home','settings','board','levels','portal-board','portal-help','resize-board','blink-board','hint','drawing','wardrobe'])+'</div>'
doc='''<!doctype html><html lang="ko"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>말랑궤도 시청각 자료</title><style>*{box-sizing:border-box}body{margin:0;background:#fcfaf6;color:#403359;font:18px/1.7 "Malgun Gothic",sans-serif}main{max-width:1100px;margin:auto;padding:36px}h1{font-size:36px}h2{font-size:25px}article{display:flex;gap:35px;background:#eee7f6;margin:22px 0;padding:25px;border-radius:18px;align-items:center}article img{width:180px;max-height:300px;object-fit:contain}audio{max-width:100%}.frames,.gallery{display:grid;grid-template-columns:repeat(3,1fr);gap:18px}.frames img{width:100%;height:430px;object-fit:contain}.gallery img{width:100%;height:250px;object-fit:contain}.gallery a{background:#f2e9df;padding:12px;color:inherit;text-align:center;text-decoration:none}.gallery span{display:block}figure{margin:0;text-align:center}@media(max-width:650px){main{padding:20px}article{display:block}article img{display:block;margin:auto}.frames,.gallery{grid-template-columns:1fr 1fr}.frames img{height:300px}}</style><main><h1>말랑궤도 · 시청각 자료</h1><p>UI·System 기획서의 보조 자료 · 2026.10.31<br>사용자 기록을 사용하지 않은 격리 화면과 프로젝트 오디오입니다.</p>'''+body+'''<p>자료 출처: 현재 프로젝트 Assets/Scripts/GravityPuzzle, Assets/Resources/GravityPuzzle, Logs의 Unity 렌더. 상세 출처·해시는 PPT 발표자 노트와 media-manifest.json에 기록했습니다.</p></main><script>document.querySelectorAll('audio').forEach(a=>{a.volume=.35;a.addEventListener('play',()=>document.querySelectorAll('audio').forEach(b=>{if(b!==a)b.pause()}))})</script></html>'''
(W/'시청각 자료.html').write_text(doc,encoding='utf8')
manifest=[{'file':p.name,'bytes':p.stat().st_size,'sha256':hashlib.sha256(p.read_bytes()).hexdigest()} for p in M.iterdir() if p.is_file()]
(W/'media-manifest.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf8')
print('MEDIA_READY',len(manifest))

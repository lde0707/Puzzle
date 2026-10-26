"""Deterministic authoring and an independent phase-aware shortest-path oracle."""
from collections import deque
from copy import deepcopy
from pathlib import Path
import json, random, sys
from resize_levels import move as resize_move, start, goal, valid
from portal_levels import solids, footprint, connected, contact

ROOT=Path(__file__).resolve().parents[2]
ASSETS=ROOT/'Android Puzzle/Assets'
NAMES=['처음 열린 길','한 박자 기다려','다시 생기는 발판','멈추면 달라져요','번갈아 숨바꼭질','두 색의 약속','친구와 한 박자','먼저 열어 줄게','별문 앞의 기다림','건너편도 한 박자','둘이 여는 길','돌아오는 타이밍','작아질 때 열리는 길','몸집과 박자를 맞춰','함께 건너는 길','모두 맞닿는 순간']

def walls_for(l,state,phase,enabled=True):
    walls=solids(l)
    if enabled:
        occupied=set().union(*(footprint(p[:2],p[2]) for p in state))
        walls|={(b['x'],b['y']) for b in l['blinkBlocks'] if b['phase']==phase and (b['x'],b['y']) not in occupied}
    return walls

def transition(l,state,phase,d,enabled=True,portals=True,resize=True):
    result,used,blocked=resize_move(l,state,d,resize=resize,portals=portals,geom=walls_for(l,state,phase,enabled))
    return (result,1-phase),used

def bfs(l,depth,enabled=True,portals=True,resize=True,limit=45000):
    initial=(start(l),0);routes={initial:()};uses={initial:0};q=deque([initial])
    shortest={initial[0]:()}
    while q:
        key=q.popleft();state,phase=key;route=routes[key]
        if len(route)>=depth:continue
        for d in range(4):
            nxt,u=transition(l,state,phase,d,enabled,portals,resize)
            if nxt in routes:continue
            if len(routes)>=limit:raise RuntimeError('state cap')
            routes[nxt]=route+(d,);uses[nxt]=uses[key]|u;q.append(nxt)
            shortest.setdefault(nxt[0],route+(d,))
    return routes,uses,shortest

def generate():
    progress=ROOT/'Android Puzzle/Logs/blink-generation-progress.json'
    levels=json.loads(progress.read_text(encoding='utf-8'))['layouts'] if '--resume' in sys.argv and progress.exists() else []
    resize_bases=json.loads((ASSETS/'Resources/GravityPuzzle/resize_levels.json').read_text(encoding='utf-8'))['layouts']
    for idx in range(len(levels),16):
        rng=random.Random(810260+idx);depth=idx+3
        for attempt in range(1800):
            w,h=(8,8) if idx<4 else (9,9) if idx<8 else (10,9)
            n=1 if idx<6 else 2
            if idx>=12:
                # Reuse proven compact geometry, then author a new phase-dependent objective.
                l=deepcopy(resize_bases[8+(idx-12+attempt//30)%4])
                w,h=l['width'],l['height']
                # Keep two friends when introducing the third interacting mechanic.
                l['cargo']=l['cargo'][:2]
            else:
                border={(x,y) for x in range(w) for y in range(h) if x in (0,w-1) or y in (0,h-1)}
                walls=border|set(rng.sample([(x,y) for x in range(2,w-2) for y in range(2,h-2)],3+idx//3))
                portals=[] if idx<8 else [dict(id='A',x=0,y=2,face=3,exitX=w-1,exitY=h-3,exitFace=1,size=1)]
                l=dict(family=4,width=w,height=h,walls=[dict(x=x,y=y) for x,y in sorted(walls)],cargo=[],portals=portals,resizers=[])
                free=[(x,y) for x in range(1,w-1) for y in range(1,h-1) if (x,y) not in solids(l)]
                for x,y in rng.sample(free,n):l['cargo'].append(dict(x=x,y=y,size=1,gx=0,gy=0,goalSize=1))
            l.update(room=f'B{idx+1:02}',displayName=NAMES[idx],lesson='친구들이 모두 멈추면 길이 바뀌어요. 점선 자리는 통과할 수 있고, 친구가 있으면 블록은 기다려요.',route=[])
            reserved=solids(l)|set().union(*(footprint(p[:2],p[2]) for p in start(l)))
            for p in l['resizers']:reserved|=footprint((p['x'],p['y']),2)
            for p in l['portals']:
                reserved|={contact(p,0),contact(p,1)}
            available=[(x,y) for x in range(1,w-1) for y in range(1,h-1) if (x,y) not in reserved]
            count=2 if idx<2 else 3 if idx<4 else 4 if idx<8 else 5
            if len(available)<count or not connected(l):continue
            l['blinkBlocks']=[dict(x=x,y=y,phase=0 if idx<4 else j%2) for j,(x,y) in enumerate(rng.sample(available,count))]
            try:routes,uses,shortest=bfs(l,depth)
            except RuntimeError:continue
            forbidden={(b['x'],b['y']) for b in l['blinkBlocks']}
            for p in l['resizers']:forbidden|=footprint((p['x'],p['y']),2)
            candidates=[key for key,r in routes.items() if len(r)==depth and len(shortest[key[0]])==depth
                        and all(not footprint(p[:2],p[2])&forbidden for p in key[0])
                        and all(p[:2]!=q[:2] for p,q in zip(key[0],start(l)))
                        and (not l['portals'] or uses[key]&4)
                        and (not l['resizers'] or uses[key]&3)]
            if not candidates:continue
            try:_,_,plain=bfs(l,depth,enabled=False)
            except RuntimeError:continue
            candidates=[key for key in candidates if key[0] not in plain]
            if not candidates:continue
            if l['portals']:
                try:_,_,plain=bfs(l,depth,portals=False)
                except RuntimeError:continue
                candidates=[key for key in candidates if key[0] not in plain]
            if l['resizers']:
                try:_,_,plain=bfs(l,depth,resize=False)
                except RuntimeError:continue
                candidates=[key for key in candidates if key[0] not in plain]
            if not candidates:continue
            key=min(candidates,key=lambda k:sum(abs(p[0]-q[0])+abs(p[1]-q[1]) for p in k[0] for q in k[0]))
            for c,(x,y,size) in zip(l['cargo'],key[0]):c.update(gx=x,gy=y,goalSize=size)
            l['route']=list(routes[key]);levels.append(l)
            print(l['room'],'min',depth,'friends',len(l['cargo']),'attempt',attempt,'states',len(routes),flush=True)
            (ROOT/'Android Puzzle/Logs/blink-generation-progress.json').write_text(json.dumps(dict(layouts=levels),ensure_ascii=False),encoding='utf-8')
            break
        else:raise RuntimeError(f'No blink level {idx+1}')
    (ASSETS/'Resources/GravityPuzzle/blink_levels.json').write_text(json.dumps(dict(layouts=levels),ensure_ascii=False,indent=2),encoding='utf-8')
    return levels

def verify(levels):
    rng=random.Random(20261027);cases=[];report=[]
    for l in levels:
        assert valid(l,start(l)) and valid(l,goal(l))
        routes,uses,shortest=bfs(l,len(l['route']))
        assert len(shortest[goal(l)])==len(l['route'])
        state=start(l);phase=0;used=0
        for d in l['route']:
            (state,phase),u=transition(l,state,phase,d);used|=u;assert valid(l,state)
        assert state==goal(l)
        for feature in ['enabled']+(['portals'] if l['portals'] else [])+(['resize'] if l['resizers'] else []):
            _,_,ordinary=bfs(l,len(l['route']),**{feature:False});assert goal(l) not in ordinary,feature
        for _ in range(120):
            state,phase=rng.choice(list(routes));d=rng.randrange(4);(after,newphase),_=transition(l,state,phase,d)
            cases.append(dict(room=l['room'],direction=d,turn=phase,before=[dict(x=p[0],y=p[1]) for p in state],beforeSizes=[p[2] for p in state],after=[dict(x=p[0],y=p[1]) for p in after],afterSizes=[p[2] for p in after]))
        report.append(dict(room=l['room'],shortest=len(l['route']),states=len(routes),friends=len(l['cargo']),blinkBlocks=len(l['blinkBlocks']),portalPairs=len(l['portals']),resizePads=len(l['resizers']),featuresRequiredWithinShortest=True))
        print('verified',l['room'],len(l['route']),len(routes),flush=True)
    (ASSETS/'Editor/GravityPuzzle/blink_reference_cases.json').write_text(json.dumps(dict(cases=cases)),encoding='utf-8')
    (ROOT/'Android Puzzle/Logs/blink-balance-report.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
    print('PASS',len(levels),'levels;',len(cases),'phase-aware transitions',flush=True)

if __name__=='__main__':
    levels=generate() if '--generate' in sys.argv else json.loads((ASSETS/'Resources/GravityPuzzle/blink_levels.json').read_text(encoding='utf-8'))['layouts']
    verify(levels)

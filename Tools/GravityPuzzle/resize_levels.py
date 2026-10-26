"""Independent variable-footprint oracle, seeded level search, and Unity parity cases."""
from collections import deque
from pathlib import Path
import json, random, sys
from portal_levels import AXES, footprint, contact, solids, connected
ROOT=Path(__file__).resolve().parents[2]
ASSETS=ROOT/'Android Puzzle/Assets'

def start(l): return tuple((c['x'],c['y'],c['size']) for c in l['cargo'])
def goal(l): return tuple((c['gx'],c['gy'],c.get('goalSize',c['size'])) for c in l['cargo'])
def valid(l,state):
    occupied=solids(l)
    for x,y,s in state:
        cells=footprint((x,y),s)
        if s not in (1,2) or cells&occupied or any(a<0 or b<0 or a>=l['width'] or b>=l['height'] for a,b in cells):return False
        occupied|=cells
    return True

def move(l,state,d,resize=True,portals=True,geom=None):
    walls=geom if geom is not None else solids(l); dx,dy=AXES[d]
    sizes=[p[2] for p in state];pos=[p[:2] for p in state];entries=[-1]*len(pos)
    triggers={}
    if portals:
        for k,p in enumerate(l['portals']):
            for side in (0,1): triggers[(contact(p,side),(p['face'] if side==0 else p['exitFace'])^2,p['size'])]=2*k+side
    def occupied(i,points):
        out=walls.copy()
        for j,p in enumerate(points):
            if i!=j:out|=footprint(p,sizes[j])
        return out
    def fits(p,s,occ):return p[0]>=0 and p[1]>=0 and p[0]+s<=l['width'] and p[1]+s<=l['height'] and not footprint(p,s)&occ
    for i in sorted(range(len(pos)),key=lambda i:-(pos[i][0]*dx+pos[i][1]*dy)):
        occ=occupied(i,pos)
        while True:
            k=triggers.get((pos[i],d,sizes[i]),-1)
            if k>=0:entries[i]=k;break
            nxt=pos[i][0]+dx,pos[i][1]+dy
            if not fits(nxt,sizes[i],occ):break
            pos[i]=nxt
    slide=pos.copy();used=0
    for i,k in enumerate(entries):
        if k<0:continue
        dest=contact(l['portals'][k//2],1-k%2)
        if fits(dest,sizes[i],occupied(i,slide)):pos[i]=dest;used|=4
    new=sizes.copy();blocked=[False]*len(pos)
    pads={(p['x'],p['y']):p['targetSize'] for p in l['resizers']} if resize else {}
    for i,p in enumerate(pos):
        target=pads.get(p,sizes[i])
        if fits(p,target,occupied(i,pos)):new[i]=target
        else:blocked[i]=True
    for i in range(len(pos)):
        for j in range(i):
            if footprint(pos[i],new[i])&footprint(pos[j],new[j]):blocked[i]=blocked[j]=True
    for i in range(len(pos)):
        if blocked[i]:new[i]=sizes[i]
        if new[i]!=sizes[i]:used|=1 if new[i]==2 else 2
    return tuple((p[0],p[1],s) for p,s in zip(pos,new)),used,blocked

def bfs(l,depth,resize=True,portals=True,limit=60000):
    initial=start(l);routes={initial:()};uses={initial:0};q=deque([initial]);geom=solids(l)
    while q:
        state=q.popleft();r=routes[state]
        if len(r)>=depth:continue
        for d in range(4):
            nxt,u,_=move(l,state,d,resize,portals,geom)
            if nxt in routes:continue
            if len(routes)>=limit:raise RuntimeError('state limit')
            routes[nxt]=r+(d,);uses[nxt]=uses[state]|u;q.append(nxt)
    return routes,uses

def generate():
    names=['처음 만난 변환대','작아지는 연습','넓은 몸 작은 길','한 번 더 말랑','몸집을 바꿔 돌아서','작은 문으로 쏙','커져서 멈춰요','별문 너머 변신','둘이 다른 크기','친구가 비켜 주면','좁은 길 양보하기','변신의 순서를 찾아','셋이 맞추는 크기','별문 앞 자리 바꾸기','크고 작은 약속','우리 모두 꼭 맞게']
    levels=[]
    for idx in range(16):
        rng=random.Random(63142+idx);w,h=[(9,9),(11,10),(12,11),(14,12)][idx//4];n=1 if idx<8 else 2 if idx<12 else 3
        depth=idx+3
        for attempt in range(1200):
            walls={(x,y) for x in range(w) for y in range(h) if x in (0,w-1) or y in (0,h-1)}
            walls|=set(rng.sample([(x,y) for x in range(2,w-2) for y in range(2,h-2)],5+idx//3))
            gates=[]
            if idx>=5:
                gates=[dict(id='A',x=0,y=2,face=3,exitX=w-1,exitY=h-3,exitFace=1,size=1)]
            l=dict(room=f'R{idx+1:02}',displayName=names[idx],family=4,width=w,height=h,walls=[dict(x=x,y=y) for x,y in sorted(walls)],cargo=[],portals=gates,resizers=[],route=[],lesson='넓은 틀은 크게, 작은 틀은 작게. 변환대의 왼쪽 아래에 맞춰 멈춰요.')
            geom=solids(l)
            anchors=[(x,y) for x in range(1,w-2) for y in range(1,h-2) if not footprint((x,y),2)&geom]
            if len(anchors)<5 or not connected(l):continue
            # Place grow stations at real stopping positions, not arbitrary empty floor.
            spawn=rng.choice(anchors);initial_size=2 if idx==1 else 1
            l['cargo']=[dict(x=spawn[0],y=spawn[1],size=initial_size,gx=0,gy=0,goalSize=1)]
            try:base,_=bfs(l,8)
            except RuntimeError:continue
            choices=list({s[0][:2] for s in base if s[0][:2] in anchors and s[0][:2]!=spawn})
            choices.sort()
            if not choices:continue
            a=rng.choice(choices);l['resizers']=[dict(x=a[0],y=a[1],targetSize=1 if idx==1 else 2)]
            if idx>=2:
                try:grown,_=bfs(l,9)
                except RuntimeError:continue
                choices=sorted({s[0][:2] for s in grown if s[0][2]==2 and s[0][:2] in anchors and not footprint(s[0][:2],2)&footprint(a,2)})
                if not choices:continue
                b=rng.choice(choices);l['resizers'].append(dict(x=b[0],y=b[1],targetSize=1))
            reserved=set().union(*(footprint((p['x'],p['y']),2) for p in l['resizers']))|footprint(spawn,initial_size)
            extras=[p for p in anchors if not footprint(p,2)&reserved]
            if len(extras)<n-1:continue
            for p in rng.sample(extras,n-1):l['cargo'].append(dict(x=p[0],y=p[1],size=1,gx=0,gy=0,goalSize=1))
            try:routes,used=bfs(l,depth)
            except RuntimeError:continue
            required=1 if idx==0 else 2 if idx==1 else 3
            if gates:required|=4
            pad_cells=set().union(*(footprint((p['x'],p['y']),2) for p in l['resizers']))
            candidates=[s for s,r in routes.items() if len(r)==depth and used[s]&required==required and not any(footprint(p[:2],p[2])&pad_cells for p in s)]
            if not candidates:continue
            # At least one target requires changed size, or resizing must shorten the route.
            try:ordinary,_=bfs(l,depth,resize=False)
            except RuntimeError:continue
            candidates=[s for s in candidates if s not in ordinary]
            if not candidates:continue
            if gates:
                try:noportal,_=bfs(l,depth,portals=False)
                except RuntimeError:continue
                candidates=[s for s in candidates if s not in noportal]
                if not candidates:continue
            selected=max(candidates,key=lambda s:sum(abs(p[0]-q[0])+abs(p[1]-q[1]) for p in s for q in s))
            for c,(x,y,size) in zip(l['cargo'],selected):c.update(gx=x,gy=y,goalSize=size)
            l['route']=list(routes[selected]);levels.append(l)
            print(l['room'],'min',depth,'friends',n,'attempt',attempt,'states',len(routes),flush=True)
            (ROOT/'Android Puzzle/Logs/resize-generation-progress.json').write_text(json.dumps(dict(layouts=levels),ensure_ascii=False),encoding='utf-8')
            break
        else:raise RuntimeError(f'No level {idx}')
    (ASSETS/'Resources/GravityPuzzle/resize_levels.json').write_text(json.dumps(dict(layouts=levels),ensure_ascii=False,indent=2),encoding='utf-8')
    return levels

def verify(levels):
    rng=random.Random(20261024);cases=[];report=[]
    for l in levels:
        assert connected(l) and valid(l,start(l)) and valid(l,goal(l))
        pad_cells=set().union(*(footprint((p['x'],p['y']),2) for p in l['resizers']))
        assert not any(footprint(p[:2],p[2])&pad_cells for p in goal(l)), 'goal overlaps resize pad'
        routes,uses=bfs(l,len(l['route']))
        assert goal(l) in routes and len(routes[goal(l)])==len(l['route'])
        state=start(l);mask=0
        for d in l['route']:state,u,_=move(l,state,d);mask|=u;assert valid(l,state)
        assert state==goal(l) and mask&3
        if l['portals']:assert mask&4
        ordinary,_=bfs(l,len(l['route']),resize=False);assert goal(l) not in ordinary
        if l['portals']:
            noportal,_=bfs(l,len(l['route']),portals=False);assert goal(l) not in noportal
        pool=list(routes)
        for _ in range(200):
            before=rng.choice(pool);d=rng.randrange(4);after,_,blocked=move(l,before,d);assert valid(l,after)
            cases.append(dict(room=l['room'],direction=d,before=[dict(x=x,y=y) for x,y,s in before],beforeSizes=[s for x,y,s in before],after=[dict(x=x,y=y) for x,y,s in after],afterSizes=[s for x,y,s in after],blocked=blocked))
        report.append(dict(room=l['room'],shortest=len(l['route']),states=len(routes),friends=len(l['cargo']),portalPairs=len(l['portals']),resizeRequiredWithinShortest=True))
        print('verified',l['room'],len(routes),flush=True)
    (ASSETS/'Editor/GravityPuzzle/resize_reference_cases.json').write_text(json.dumps(dict(cases=cases)),encoding='utf-8')
    (ROOT/'Android Puzzle/Logs/resize-balance-report.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
    print('PASS',len(levels),'levels;',len(cases),'independent transitions',flush=True)

if __name__=='__main__':
    levels=generate() if '--generate' in sys.argv else json.loads((ASSETS/'Resources/GravityPuzzle/resize_levels.json').read_text(encoding='utf-8'))['layouts']
    verify(levels)

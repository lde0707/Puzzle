"""Face-contact solid portal oracle, connected-map BFS and C# fixtures."""
from collections import deque
from pathlib import Path
import json, random
ROOT=Path(__file__).resolve().parents[2];ASSETS=ROOT/'Android Puzzle/Assets'
AXES=((0,-1),(-1,0),(0,1),(1,0))

def footprint(p,size):return {(p[0]+x,p[1]+y) for x in range(size) for y in range(size)}
def contact(g,side):
    x,y,face=(g['x'],g['y'],g['face']) if side==0 else (g['exitX'],g['exitY'],g['exitFace'])
    dx,dy=AXES[face];return (x+dx*g['size'],y+dy*g['size'])
def solids(level):
    result={(p['x'],p['y']) for p in level['walls']}
    for g in level['portals']:
        result |= footprint((g['x'],g['y']),g['size'])|footprint((g['exitX'],g['exitY']),g['size'])
    return result

def tilt(level,state,direction,portals=True,geometry=None):
    axis=AXES[direction];sizes=[c['size'] for c in level['cargo']];walls=geometry if geometry is not None else solids(level)
    gates=level['portals'];positions=list(state);entered=[-1]*len(state)
    triggers={}
    if portals:
        for k,g in enumerate(gates):
            for side in (0,1):
                face=g['face'] if side==0 else g['exitFace']
                triggers[(contact(g,side),(face+2)%4,g['size'])]=k*2+side
    for i in sorted(range(len(state)),key=lambda i:-(state[i][0]*axis[0]+state[i][1]*axis[1])):
        occupied=walls.copy()
        for j,p in enumerate(positions):
            if i!=j:occupied |= footprint(p,sizes[j])
        while True:
            key=(positions[i],direction,sizes[i])
            if key in triggers:entered[i]=triggers[key];break
            nextp=(positions[i][0]+axis[0],positions[i][1]+axis[1]);cells=footprint(nextp,sizes[i])
            if cells & occupied or any(x<0 or y<0 or x>=level['width'] or y>=level['height'] for x,y in cells):break
            positions[i]=nextp
    slide=tuple(positions);used=[];blocked=[]
    for i,k in enumerate(entered):
        if k<0:continue
        out=contact(gates[k//2],1-k%2);occupied=walls.copy()
        for j,p in enumerate(slide):
            if j!=i:occupied |= footprint(p,sizes[j])
        if footprint(out,sizes[i])&occupied:blocked.append(i)
        else:positions[i]=out;used.append(k//2)
    return tuple(positions),slide,used,blocked

def connected(level):
    w,h=level['width'],level['height'];wall=solids(level)
    floor={(x,y) for x in range(w) for y in range(h)}-wall
    reached={next(iter(floor))};queue=list(reached)
    for x,y in queue:
        for dx,dy in AXES:
            p=x+dx,y+dy
            if p in floor and p not in reached:reached.add(p);queue.append(p)
    return reached==floor

def make(room,name,w,h,stones,starts,gates,lesson):
    walls={(x,y) for x in range(w) for y in range(h) if x in (0,w-1) or y in (0,h-1)}|set(stones)
    l=dict(room=room,displayName=name,family=4,width=w,height=h,walls=[dict(x=x,y=y) for x,y in sorted(walls)],cargo=[dict(x=x,y=y,size=1,gx=0,gy=0) for x,y in starts],portals=[dict(id=chr(65+i),x=x,y=y,face=f,exitX=ex,exitY=ey,exitFace=ef,size=1) for i,(x,y,f,ex,ey,ef) in enumerate(gates)],lesson=lesson,route=[])
    assert connected(l),room+' has partitioned floor'
    for g in l['portals']:
        for side in (0,1):assert not footprint(contact(g,side),g['size'])&solids(l)
    return l

def bfs(level,max_depth,portals=True,max_states=35000):
    geometry=solids(level)
    start=tuple((c['x'],c['y']) for c in level['cargo']);routes={start:()};used={start:frozenset()};queue=deque([start])
    while queue:
        state=queue.popleft();route=routes[state]
        if len(route)>=max_depth:continue
        for d in range(4):
            nxt,_,g,_=tilt(level,state,d,portals,geometry)
            if nxt in routes:continue
            if len(routes)>=max_states:raise RuntimeError(f"{level['room']}: BFS bound reached")
            routes[nxt]=route+(d,);used[nxt]=used[state]|set(g);queue.append(nxt)
    return routes,used

def search(level,depth):
    routes,used=bfs(level,depth)
    ordinary,_=bfs(level,depth+2,False)
    adjacent={contact(g,side) for g in level['portals'] for side in (0,1)}
    candidates=[(state,r) for state,r in routes.items() if len(r)==depth and len(used[state])==len(level['portals']) and not any(p in adjacent for p in state) and (state not in ordinary or len(ordinary[state])>=depth+2)]
    if not candidates:raise RuntimeError(f"{level['room']}: no portal-relevant goal at {depth}; states {len(routes)}")
    state,route=max(candidates,key=lambda pair:sum(abs(a[0]-b[0])+abs(a[1]-b[1]) for a in pair[0] for b in pair[0]))
    for c,(x,y) in zip(level['cargo'],state):c['gx']=x;c['gy']=y
    level['route']=list(route)
    print(level['room'],'shortest',len(route),'states',len(routes),'route',route,'goal',state,flush=True)

def generate():
    names=['빛나는 문턱','벽에 숨은 문','위아래 지름길','빙글 돌아 만나','둘이 건너는 밤','먼저 비켜 줄래','두 색의 별문','엇갈린 출구','세 친구의 여행','벽 너머 무지개','한 박자 기다려','돌아오는 길','세 갈래 은하','작은 순서의 차이','별문 교차로','우리의 넓은 우주']
    lessons=['문 앞 화살표를 따라 들어가요. 같은 문자로 나오며 포탈을 누르면 연결을 볼 수 있어요.',
        '벽에도 별문이 있어요. 밝은 문턱을 향해 움직이면 연결된 문 앞으로 나와요.',
        '같은 색 목적지를 찾아요. 도착한 친구도 다음 중력에 함께 움직여요.',
        '출구가 막히면 들어갈 수 없어요. 친구들의 이동 순서를 생각해 보세요.']
    levels=[]
    for index in range(16):
        w,h=[(13,12),(14,13),(16,15),(18,17)][index//4]
        count=1 if index<4 else 2 if index<8 else 3
        pair_count=1 if index<6 else 2 if index<12 else 3
        starts=[(1,3),(3,1),(4,1)][:count]
        if index==0:a=(5,3,1,w-4,h-4,0)
        elif index%3==1:a=(0,3,3,w-4,h-1,0)
        elif index%3==2:a=(w-1,3,1,3,h-1,0)
        else:a=(0,h-4,3,w-4,0,2)
        gates=[a]
        if pair_count>=2:gates.append((6,h-5,0,w-4,5,1))
        if pair_count>=3:gates.append((w-1,h-4,1,4,0,2))
        # Portal endpoints deliberately occupy wall cells; their active faces open inward.
        l=make('T'+str(index+1).zfill(2),names[index],w,h,[],starts,gates,lessons[index//4])
        base=list(l['walls']);rng=random.Random(8110+index)
        reserved=solids(l)|set(starts)|{contact(g,side) for g in l['portals'] for side in (0,1)}
        options=[(x,y) for x in range(1,w-1) for y in range(1,h-1) if (x,y) not in reserved]
        for attempt in range(400):
            stones=[(3,5),(6,6),(10,4),(9,4)] if index==0 and attempt==0 else rng.sample(options,8+index//2)
            l['walls']=base+[dict(x=x,y=y) for x,y in stones]
            if not connected(l):continue
            try:search(l,index+3);break
            except RuntimeError:
                if attempt==399:raise
        else:raise RuntimeError('No connected candidate')
        levels.append(l)
        (ROOT/'Android Puzzle/Logs/portal-generation-progress.json').write_text(json.dumps(dict(layouts=levels),ensure_ascii=False),encoding='utf-8')
    (ASSETS/'Resources/GravityPuzzle/late_levels.json').write_text(json.dumps(dict(layouts=levels),ensure_ascii=False,indent=2),encoding='utf-8')
    return levels

def verify(levels):
    cases=[];report=[];rng=random.Random(20261023);previous=13*11
    assert len(levels)==16
    previous_depth=0
    for l in levels:
        assert len(l["route"])>previous_depth;previous_depth=len(l["route"])
        assert connected(l);assert l['width']*l['height']>=previous;previous=l['width']*l['height']
        start=tuple((c['x'],c['y']) for c in l['cargo']);goal=tuple((c['gx'],c['gy']) for c in l['cargo']);state=start;used=set()
        for d in l['route']:state,_,g,_=tilt(l,state,d);used.update(g)
        assert state==goal and len(used)==len(l['portals'])
        routes,_=bfs(l,len(l['route']));assert len(routes[goal])==len(l['route'])
        ordinary,_=bfs(l,len(l['route'])+2,False)
        assert goal not in ordinary or len(ordinary[goal])>=len(l['route'])+2
        state=start
        for n in range(400):
            d=rng.randrange(4);after,_,_,_=tilt(l,state,d)
            cases.append(dict(room=l['room'],direction=d,before=[dict(x=x,y=y) for x,y in state],after=[dict(x=x,y=y) for x,y in after]))
            state=after if n%17 else start
        line=f"PASS {l['room']}: {l['width']}x{l['height']}, connected floor, shortest={len(l['route'])}, BFS states={len(routes)}, portal shortcut verified"
        report.append(line);print(line,flush=True)
    (ASSETS/'Editor/GravityPuzzle/portal_reference_cases.json').write_text(json.dumps(dict(cases=cases),separators=(',',':')),encoding='utf-8')
    (ROOT/'Android Puzzle/Logs/portal-balance-validation.txt').write_text('\n'.join(report)+f'\nParity cases: {len(cases)}\n',encoding='utf-8')

if __name__=='__main__':
    import sys
    levels=generate() if '--generate' in sys.argv else json.loads((ASSETS/'Resources/GravityPuzzle/late_levels.json').read_text(encoding='utf-8'))['layouts']
    verify(levels)


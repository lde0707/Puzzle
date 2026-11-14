"""Seeded 4 x 16 expansion. Exact BFS, mechanic ablation and independent parity.

Original four level files are read-only. Checkpoints and evidence live in Logs;
the extra runtime resource is written only after all four themes pass.
"""
from collections import deque
from concurrent.futures import ProcessPoolExecutor
from copy import deepcopy
from pathlib import Path
import hashlib, json, random, sys
from resize_levels import start, goal, move as reference_move, valid
from blink_levels import transition as reference_blink
from portal_levels import solids, footprint, contact, connected, AXES

ROOT=Path(__file__).resolve().parents[2]
SOURCE=ROOT/'Android Puzzle/Assets/Resources/GravityPuzzle'
LOG=ROOT/'Android Puzzle/Logs'
FILES=['layouts.json','late_levels.json','resize_levels.json','blink_levels.json']
TITLES=[['넓어진 산책길','모퉁이의 약속','함께 돌아가는 길','먼 별의 정거장'],
        ['별문 너머 정원','엇갈린 출구','먼 곳의 친구들','별문 미로의 끝'],
        ['커다란 놀이터','작은 몸 긴 여행','크기의 갈림길','꼭 맞는 우주'],
        ['넓어진 숨바꼭질','열리는 길의 박자','한 박자 건너편','마지막 길의 약속']]

class Simulator:
    """Bitset implementation cross-checked against the existing Python oracle and C#."""
    def __init__(self,l,feature=True,portals=True,resize=True):
        self.l=l;self.w=l['width'];self.h=l['height'];self.blink=bool(l['blinkBlocks'])
        self.fixed=sum(1<<(x+y*self.w) for x,y in solids(l));self.masks={}
        self.phase=[sum(1<<(b['x']+b['y']*self.w) for b in l['blinkBlocks'] if b['phase']==p) if feature else 0 for p in range(2)]
        self.gates={}
        if portals:
            for p in l['portals']:
                for side in (0,1):
                    x,y=contact(p,side);face=p['face'] if side==0 else p['exitFace']
                    self.gates[(x,y,p['size'],face^2)]=contact(p,1-side)
        self.pads={(p['x'],p['y']):p['targetSize'] for p in l['resizers']} if resize else {}
    def mask(self,x,y,s):
        if x<0 or y<0 or x+s>self.w or y+s>self.h:return -1
        key=x,y,s
        if key not in self.masks:self.masks[key]=sum(1<<((x+a)+(y+b)*self.w) for a in range(s) for b in range(s))
        return self.masks[key]
    def step(self,key,d):
        state,phase=key;pos=[p[:2] for p in state];sizes=[p[2] for p in state]
        occ=[self.mask(*p) for p in state];all_occ=0
        for m in occ:all_occ|=m
        walls=self.fixed|(self.phase[phase]&~all_occ);dx,dy=AXES[d];entries={};used=0
        for i in sorted(range(len(pos)),key=lambda i:-(pos[i][0]*dx+pos[i][1]*dy)):
            blocked=walls
            for j,m in enumerate(occ):
                if j!=i:blocked|=m
            x,y=pos[i];s=sizes[i]
            while True:
                dest=self.gates.get((x,y,s,d))
                if dest is not None:entries[i]=dest;break
                nx,ny=x+dx,y+dy;m=self.mask(nx,ny,s)
                if m<0 or m&blocked:break
                x,y=nx,ny
            pos[i]=x,y;occ[i]=self.mask(x,y,s)
        settled=occ.copy()
        for i,(x,y) in entries.items():
            blocked=walls
            for j,m in enumerate(settled):
                if j!=i:blocked|=m
            m=self.mask(x,y,sizes[i])
            if m>=0 and not m&blocked:pos[i]=x,y;used|=4
        occ=[self.mask(*p,s) for p,s in zip(pos,sizes)];new=sizes.copy();rejected=set()
        for i,(x,y) in enumerate(pos):
            target=self.pads.get((x,y),sizes[i]);blocked=walls
            for j,m in enumerate(occ):
                if j!=i:blocked|=m
            m=self.mask(x,y,target)
            if m>=0 and not m&blocked:new[i]=target
        for i in range(len(pos)):
            for j in range(i):
                if self.mask(*pos[i],new[i])&self.mask(*pos[j],new[j]):rejected.update((i,j))
        for i in rejected:new[i]=sizes[i]
        for a,b in zip(sizes,new):
            if a!=b:used|=1 if b==2 else 2
        return (tuple((x,y,s) for (x,y),s in zip(pos,new)),1-phase if self.blink else 0),used

def search(sim,depth,cap=24000):
    initial=(start(sim.l),0);routes={initial:()};uses={initial:0};q=deque([initial]);shortest={initial[0]:()}
    while q:
        key=q.popleft();route=routes[key]
        if len(route)>=depth:continue
        for d in range(4):
            nxt,u=sim.step(key,d)
            if nxt in routes:continue
            if len(routes)>=cap:raise RuntimeError('state cap')
            routes[nxt]=route+(d,);uses[nxt]=uses[key]|u;q.append(nxt);shortest.setdefault(nxt[0],route+(d,))
    return routes,uses,shortest

def enlarged(base,w,h,rng):
    l=deepcopy(base);ow,oh=l['width'],l['height']
    # Insert traversable rows/columns, preserving 2x2 pads, friends and portal mouths.
    protected_x=set();protected_y=set()
    for c in l['cargo']:
        for k in range(1,c['size']):protected_x.add(c['x']+k);protected_y.add(c['y']+k)
    for p in l.get('resizers',[]):protected_x.add(p['x']+1);protected_y.add(p['y']+1)
    for p in l.get('portals',[]):
        for side in (0,1):
            x,y=(p['x'],p['y']) if side==0 else (p['exitX'],p['exitY']);a,b=contact(p,side)
            protected_x.update(range(min(x,a)+1,max(x,a)+p['size']))
            protected_y.update(range(min(y,b)+1,max(y,b)+p['size']))
    xs=[x for x in range(2,ow-1) if x not in protected_x];ys=[y for y in range(2,oh-1) if y not in protected_y]
    if not xs or not ys:return None
    xcuts=[rng.choice(xs) for _ in range(w-ow)];ycuts=[rng.choice(ys) for _ in range(h-oh)]
    fx=lambda x:x+sum(k<=x for k in xcuts);fy=lambda y:y+sum(k<=y for k in ycuts)
    walls={(fx(p['x']),fy(p['y'])) for p in l['walls'] if 0<p['x']<ow-1 and 0<p['y']<oh-1}
    walls|={(x,y) for x in range(w) for y in range(h) if x in (0,w-1) or y in (0,h-1)}
    for c in l['cargo']:c.update(x=fx(c['x']),y=fy(c['y']),gx=0,gy=0)
    for p in l.get('portals',[]):p.update(x=fx(p['x']),y=fy(p['y']),exitX=fx(p['exitX']),exitY=fy(p['exitY']))
    for p in l.get('resizers',[]):p.update(x=fx(p['x']),y=fy(p['y']))
    for p in l.get('blinkBlocks',[]):p.update(x=fx(p['x']),y=fy(p['y']))
    l.update(width=w,height=h,walls=[dict(x=x,y=y) for x,y in sorted(walls)])
    l.setdefault('portals',[]);l.setdefault('resizers',[]);l.setdefault('blinkBlocks',[])
    reserved=solids(l)|set().union(*(footprint(p[:2],p[2]) for p in start(l)))
    for p in l['resizers']:reserved|=footprint((p['x'],p['y']),2)
    for p in l['portals']:
        for side in (0,1):reserved|=footprint(contact(p,side),p['size'])
    reserved|={(b['x'],b['y']) for b in l['blinkBlocks']}
    options=[(x,y) for x in range(2,w-2) for y in range(2,h-2) if (x,y) not in reserved]
    for x,y in rng.sample(options,min(len(options),rng.randrange(1,5))):l['walls'].append(dict(x=x,y=y))
    return l

def author(theme):
    bases=json.loads((SOURCE/FILES[theme]).read_text(encoding='utf-8-sig'))['layouts']
    bases=[l for l in bases if len(l['cargo'])==(2 if theme==3 else 3)]
    if theme==2:bases=[l for l in bases if l.get('resizers') and l.get('portals')]
    checkpoint=LOG/f'expansion-theme-{theme}.json'
    done=json.loads(checkpoint.read_text(encoding='utf-8')) if checkpoint.exists() else []
    for i in range(len(done),16):
        rng=random.Random(2026110200+theme*100+i);band=i//4
        w,h=[(14,12),(19,18),(15,13),(13,12)][theme];w+=band*2;h+=band*2
        depth=(13 if theme==0 else 19)+i//2
        for attempt in range(2500):
            l=enlarged(rng.choice(bases),w,h,rng)
            if l is None or not connected(l) or not valid(l,start(l)):continue
            l.update(room=f'E{theme}_{i+17:02}',displayName=TITLES[theme][band]+f' {i%4+1}',route=[],
                     lesson=['넓어진 길에서 친구들이 멈출 자리를 차례로 만들어 보세요.',
                     '별문 양쪽의 도착 자리를 살펴보고 친구가 지나갈 순서를 찾아요.',
                     '넓은 몸으로 멈추고 작은 몸으로 통과해요. 변환 순서가 중요해요.',
                     '멀리 있는 길도 같은 박자로 바뀌어요. 다음에 열릴 길을 살펴보세요.'][theme])
            sim=Simulator(l)
            try:routes,uses,shortest=search(sim,depth)
            except RuntimeError:continue
            forbidden={(b['x'],b['y']) for b in l['blinkBlocks']}
            for p in l['resizers']:forbidden|=footprint((p['x'],p['y']),2)
            required=(4 if l['portals'] else 0)|(3 if l['resizers'] else 0)
            candidates=[k for k,r in routes.items() if len(r)==depth and len(shortest[k[0]])==depth
                        and uses[k]&required==required and all(p[:2]!=q[:2] for p,q in zip(k[0],start(l)))
                        and all(not footprint(p[:2],p[2])&forbidden for p in k[0])]
            if not candidates:continue
            # Featured mechanics must improve the shortest solution, not serve as scenery.
            try:
                for flags in ([{'feature':False}] if theme==3 else [])+([{'resize':False}] if l['resizers'] else [])+([{'portals':False}] if l['portals'] else []):
                    _,_,without=search(Simulator(l,**flags),depth)
                    candidates=[k for k in candidates if k[0] not in without]
                    if not candidates:break
            except RuntimeError:continue
            if not candidates:continue
            rng.shuffle(candidates)
            chosen=None
            for key in candidates[:80]:
                route=routes[key];s=(start(l),0);xs=[];ys=[]
                for d in route:
                    xs.extend(p[0] for p in s[0]);ys.extend(p[1] for p in s[0]);s,_=sim.step(s,d)
                xs.extend(p[0] for p in s[0]);ys.extend(p[1] for p in s[0])
                if max(xs)-min(xs)>=.65*(w-2) and max(ys)-min(ys)>=.65*(h-2):chosen=key;break
            if chosen is None:continue
            for c,(x,y,size) in zip(l['cargo'],chosen[0]):c.update(gx=x,gy=y,goalSize=size)
            l['route']=list(routes[chosen]);assert valid(l,goal(l))
            # Sample independent reference parity before accepting a map.
            samples=rng.sample(list(routes),min(32,len(routes)));cases=[]
            for key in samples:
                for d in range(4):
                    after,_=sim.step(key,d)
                    expected=reference_blink(l,*key,d)[0] if theme==3 else (reference_move(l,key[0],d)[0],0)
                    assert after==expected,(l['room'],key,d,'oracle mismatch')
                    cases.append(dict(room=l['room'],direction=d,turn=key[1],before=[dict(x=p[0],y=p[1]) for p in key[0]],beforeSizes=[p[2] for p in key[0]],after=[dict(x=p[0],y=p[1]) for p in after[0]],afterSizes=[p[2] for p in after[0]]))
            done.append(dict(level=l,states=len(routes),shortest=depth,cases=cases,attempt=attempt))
            checkpoint.write_text(json.dumps(done,ensure_ascii=False),encoding='utf-8')
            print(f"PASS {l['room']} {w}x{h} minimum={depth} states={len(routes)} attempt={attempt}",flush=True)
            break
        else:raise RuntimeError(f'No expansion {theme}/{i+17}')
    return done

def main():
    LOG.mkdir(exist_ok=True)
    before={f:hashlib.sha256((SOURCE/f).read_bytes()).hexdigest() for f in FILES}
    with ProcessPoolExecutor(max_workers=4) as pool:groups=list(pool.map(author,range(4)))
    entries=[e for group in groups for e in group];levels=[e['level'] for e in entries]
    assert len(levels)==64 and len({l['room'] for l in levels})==64
    assert before=={f:hashlib.sha256((SOURCE/f).read_bytes()).hexdigest() for f in FILES}
    (SOURCE/'expansion_levels.json').write_text(json.dumps(dict(layouts=levels),ensure_ascii=False,indent=2),encoding='utf-8')
    (ROOT/'Android Puzzle/Assets/Editor/GravityPuzzle/expansion_reference_cases.json').write_text(json.dumps(dict(cases=[c for e in entries for c in e['cases']])),encoding='utf-8')
    (LOG/'expansion-balance-report.json').write_text(json.dumps(dict(originalSHA256=before,levels=[dict(room=e['level']['room'],width=e['level']['width'],height=e['level']['height'],shortest=e['shortest'],states=e['states'],solution=e['level']['route'],featuresRequiredWithinShortest=True) for e in entries],limitation='Shortest depth and spatial coverage are proxies, not a human playtest difficulty rating.'),ensure_ascii=False,indent=2),encoding='utf-8')
    print('PASS 64 new maps; original 64 unchanged; 8192 independent transitions',flush=True)
if __name__=='__main__':main()

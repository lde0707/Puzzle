"""Read-only campaign audit; writes evidence to Logs, never changes levels.

BFS enumerates all states through the authored solution length. This proves
shortest solutions, but does not prove recovery from every arbitrary state.
"""
from collections import deque
from datetime import datetime, timezone
import hashlib
import json
from pathlib import Path
from resize_levels import start, goal, move, valid
from portal_levels import solids, tilt as portal_move
from verify_layouts import tilt as base_move
from blink_levels import bfs as blink_bfs, transition as blink_transition

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / 'Android Puzzle/Assets/Resources/GravityPuzzle'


def audit(level, number):
    if level.get("blinkBlocks"):
        return audit_blink(level,number)
    initial, target = start(level), goal(level)
    geometry = solids(level)
    assert valid(level, initial) and valid(level, target), level['room']
    state = initial
    for direction in level['route']:
        before = state
        state, _, _ = move(level, state, direction, geom=geometry)
        assert valid(level, state), (level['room'], 'invalid route state')
        if not level['resizers']:
            if level['portals']:
                expected = portal_move(level, tuple(p[:2] for p in before), direction)[0]
            else:
                expected = base_move(tuple(p[:2] for p in before), tuple(p[2] for p in before),
                                     geometry, direction, level['width'], level['height'])
            assert expected == tuple(p[:2] for p in state), 'independent oracle mismatch'
    assert state == target, (level['room'], 'authored route fails')
    routes = {initial: ()}
    ways = {initial: 1}
    queue = deque([initial])
    while queue:
        state = queue.popleft()
        route = routes[state]
        if len(route) >= len(level['route']):
            continue
        for direction in range(4):
            nxt, _, _ = move(level, state, direction, geom=geometry)
            if nxt not in routes:
                if len(routes) >= 250000:
                    raise RuntimeError('BFS limit reached; verification incomplete')
                routes[nxt] = route + (direction,)
                ways[nxt] = ways[state]
                queue.append(nxt)
            elif len(routes[nxt]) == len(route) + 1:
                ways[nxt] += ways[state]
    assert target in routes
    result = dict(stage=number, room=level['room'], name=level.get('displayName', level['room']),
                  chapter=2 if level['resizers'] else 1 if level['portals'] else 0,
                  shortest=len(routes[target]), authoredLength=len(level['route']),
                  solution=list(routes[target]), shortestSolutionCount=ways[target],
                  statesThroughAuthoredDepth=len(routes), friends=len(level['cargo']),
                  width=level['width'], height=level['height'],
                  portalPairs=len(level['portals']), resizePads=len(level['resizers']),
                  initialSizes=[p[2] for p in initial], goalSizes=[p[2] for p in target])
    print(f"PASS {number:02} {level['room']}: min={result['shortest']} "
          f"solutions={ways[target]} states={len(routes)} friends={result['friends']}", flush=True)
    return result


def audit_blink(level, number):
    routes, uses, shortest = blink_bfs(level, len(level['route']))
    target = goal(level); length = len(shortest[target])
    ways = {(start(level),0):1}
    for key, route in routes.items():
        for direction in range(4):
            nxt, _ = blink_transition(level, *key, direction)
            if nxt in routes and len(routes[nxt]) == len(route)+1:
                ways[nxt] = ways.get(nxt,0)+ways.get(key,0)
    count = sum(ways.get(key,0) for key in routes if key[0]==target and len(routes[key])==length)
    state,phase=start(level),0
    for direction in level['route']:
        (state,phase),_ = blink_transition(level,state,phase,direction)
        assert valid(level,state)
    assert state==target
    result=dict(stage=number,room=level['room'],name=level['displayName'],chapter=3,
                shortest=length,authoredLength=len(level['route']),solution=list(shortest[target]),
                shortestSolutionCount=count,statesThroughAuthoredDepth=len(routes),friends=len(level['cargo']),
                width=level['width'],height=level['height'],portalPairs=len(level['portals']),
                resizePads=len(level['resizers']),blinkBlocks=len(level['blinkBlocks']),
                initialSizes=[p[2] for p in start(level)],goalSizes=[p[2] for p in target])
    print(f"PASS {number:02} {level['room']}: min={length} solutions={count} states={len(routes)}",flush=True)
    return result


def main():
    levels, hashes = [], {}
    for filename in ('layouts.json', 'late_levels.json', 'resize_levels.json', 'blink_levels.json'):
        raw = (SOURCE / filename).read_bytes()
        hashes[filename] = hashlib.sha256(raw).hexdigest()
        for level in json.loads(raw.decode('utf-8-sig'))['layouts']:
            level.setdefault('resizers', [])
            level.setdefault('portals', [])
            level.setdefault('blinkBlocks', [])
            levels.append(level)
    # Exact stable sort used by PuzzleCampaign.Order in the Unity runtime.
    levels.sort(key=lambda l: (3 if l['blinkBlocks'] else 2 if l['resizers'] else 1 if l['portals'] else 0,
                              len(l['route']), len(l['cargo']),
                              max(c['size'] for c in l['cargo']), l['width'] * l['height']))
    results = [audit(level, i + 1) for i, level in enumerate(levels)]
    drops = [dict(previous=a['stage'], stage=b['stage'], before=a['shortest'], after=b['shortest'])
             for a, b in zip(results, results[1:]) if b['shortest'] < a['shortest']]
    report = dict(timestamp=datetime.now(timezone.utc).isoformat(), sourceSHA256=hashes,
                  method='Exhaustive BFS through authored solution depth; fixed-size route oracle cross-check; blink searches include turn parity',
                  limitation='Initial-state solvability only. Human difficulty and recovery from every reachable state are not proven.',
                  levels=results, shortestDrops=drops,
                  allAuthoredRoutesShortest=all(r['shortest'] == r['authoredLength'] for r in results))
    dest = ROOT / 'Android Puzzle/Logs/campaign-audit.json'
    dest.write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding='utf-8')
    print('RESULT', len(results), 'solvable; shortest drops:', drops, flush=True)


if __name__ == '__main__':
    main()

"""Author compact bridge maps without touching mechanic tutorials or stage IDs.

Seeded search keeps the exact shortest-move ladder and requires the featured
mechanics. Reachable-state caps are a complexity guardrail, not human ratings.
Run this tool, portal_levels.py, resize_levels.py, then audit_campaign.py.
"""
from copy import deepcopy
import json
import random
from pathlib import Path
from portal_levels import make, solids, footprint, contact, connected, bfs as portal_bfs
from resize_levels import bfs, start, valid

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / 'Android Puzzle/Assets/Resources/GravityPuzzle'


def portal_bridge(old, width, height, cap):
    idx = int(old['room'][1:]) - 1
    rng = random.Random(2026102500 + idx)
    count = len(old['cargo'])
    pairs = len(old['portals'])
    gates = [(0, 3, 3, width - 4, height - 1, 0)]
    if pairs >= 2:
        gates.append((6, height - 5, 0, width - 4, 5, 1))
    if pairs >= 3:
        gates.append((width - 1, height - 4, 1, 4, 0, 2))
    starts = [(1, 3), (3, 1), (4, 1)][:count]
    level = make(old['room'], old['displayName'], width, height, [], starts, gates,
                 '친구가 늘어도 작은 맵에서 차근차근. 문 앞을 비워 주고 함께 움직여요.')
    border = list(level['walls'])
    reserved = solids(level) | set(starts) | {contact(g, side) for g in level['portals'] for side in (0, 1)}
    options = [(x, y) for x in range(1, width - 1) for y in range(1, height - 1) if (x, y) not in reserved]
    depth = len(old['route'])
    for attempt in range(2000):
        level['walls'] = border + [dict(x=x, y=y) for x, y in rng.sample(options, 5 + pairs)]
        if not connected(level):
            continue
        try:
            routes, uses = portal_bfs(level, depth, max_states=cap + 1)
        except RuntimeError:
            continue
        candidates = [(s, r) for s, r in routes.items() if len(r) == depth and len(uses[s]) == pairs
                      and all(p != q for p, q in zip(s, starts))
                      and not any(p in reserved for p in s)]
        if not candidates:
            continue
        ordinary, _ = portal_bfs(level, depth + 2, False)
        candidates = [(s, r) for s, r in candidates if s not in ordinary or len(ordinary[s]) >= depth + 2]
        if not candidates:
            continue
        state, route = min(candidates, key=lambda pair: sum(abs(a[0]-b[0])+abs(a[1]-b[1]) for a in pair[0] for b in pair[0]))
        for c, p in zip(level['cargo'], state):
            c['gx'], c['gy'] = p
        level['route'] = list(route)
        print('BRIDGE', old['room'], 'attempt', attempt, 'states', len(routes), flush=True)
        return level
    raise RuntimeError('No compact portal bridge: ' + old['room'])


def resize_bridge(old, width, height, cap):
    idx = int(old['room'][1:]) - 1
    rng = random.Random(2026102600 + idx)
    count, depth = len(old['cargo']), len(old['route'])
    for attempt in range(4000):
        walls = {(x, y) for x in range(width) for y in range(height) if x in (0, width-1) or y in (0, height-1)}
        walls |= set(rng.sample([(x, y) for x in range(2, width-2) for y in range(2, height-2)], 5))
        level = deepcopy(old)
        level.update(width=width, height=height, walls=[dict(x=x, y=y) for x, y in sorted(walls)],
                     cargo=[], resizers=[], route=[],
                     lesson='좁은 맵에서 한 친구씩 크기를 맞춰요. 다른 친구가 설 자리도 남겨 주세요.',
                     portals=[dict(id='A', x=0, y=2, face=3, exitX=width-1, exitY=height-3, exitFace=1, size=1)])
        geometry = solids(level)
        anchors = [(x, y) for x in range(1, width-2) for y in range(1, height-2) if not footprint((x, y), 2) & geometry]
        if len(anchors) < 5 or not connected(level):
            continue
        spawn = rng.choice(anchors)
        level['cargo'] = [dict(x=spawn[0], y=spawn[1], size=1, gx=0, gy=0, goalSize=1)]
        base, _ = bfs(level, 8)
        choices = sorted({s[0][:2] for s in base if s[0][:2] in anchors and s[0][:2] != spawn})
        if not choices:
            continue
        a = rng.choice(choices)
        level['resizers'] = [dict(x=a[0], y=a[1], targetSize=2)]
        grown, _ = bfs(level, 9)
        choices = sorted({s[0][:2] for s in grown if s[0][2] == 2 and s[0][:2] in anchors and not footprint(s[0][:2], 2) & footprint(a, 2)})
        if not choices:
            continue
        b = rng.choice(choices)
        level['resizers'].append(dict(x=b[0], y=b[1], targetSize=1))
        pad_cells = footprint(a, 2) | footprint(b, 2)
        reserved = pad_cells | footprint(spawn, 1)
        extras = [p for p in anchors if not footprint(p, 2) & reserved]
        if len(extras) < count-1:
            continue
        for p in rng.sample(extras, count-1):
            level['cargo'].append(dict(x=p[0], y=p[1], size=1, gx=0, gy=0, goalSize=1))
        try:
            routes, uses = bfs(level, depth, limit=cap + 1)
        except RuntimeError:
            continue
        initial = start(level)
        candidates = [s for s, r in routes.items() if len(r) == depth and uses[s] & 7 == 7
                      and all(p != q for p, q in zip(s, initial))
                      and not any(footprint(p[:2], p[2]) & pad_cells for p in s)]
        if not candidates:
            continue
        ordinary, _ = bfs(level, depth, resize=False)
        noportal, _ = bfs(level, depth, portals=False)
        candidates = [s for s in candidates if s not in ordinary and s not in noportal]
        if not candidates:
            continue
        selected = min(candidates, key=lambda s: sum(abs(p[0]-q[0])+abs(p[1]-q[1]) for p in s for q in s))
        for c, (x, y, size) in zip(level['cargo'], selected):
            c.update(gx=x, gy=y, goalSize=size)
        level['route'] = list(routes[selected])
        assert valid(level, initial) and valid(level, selected)
        print('BRIDGE', old['room'], 'attempt', attempt, 'states', len(routes), flush=True)
        return level
    raise RuntimeError('No compact resize bridge: ' + old['room'])


def main():
    configs = {
        'late_levels.json': {'T05': (13, 12, 75), 'T09': (14, 13, 520), 'T13': (16, 15, 2400)},
        'resize_levels.json': {'R09': (11, 10, 110), 'R13': (12, 11, 500), 'R14': (13, 11, 850)},
    }
    results = {}
    for filename, targets in configs.items():
        document = json.loads((SOURCE / filename).read_text(encoding='utf-8'))
        # Preserve the exact pre-edit source once for comparison and recovery.
        backup = ROOT / 'Android Puzzle/Logs' / ('before-bridge-' + filename)
        if not backup.exists():
            backup.write_text(json.dumps(document, ensure_ascii=False, indent=2), encoding='utf-8')
        for index, level in enumerate(document['layouts']):
            if level['room'] in targets:
                generate = resize_bridge if level.get('resizers') else portal_bridge
                document['layouts'][index] = generate(level, *targets[level['room']])
        results[filename] = document
    # Publish together only once every search succeeds.
    for filename, document in results.items():
        (SOURCE / filename).write_text(json.dumps(document, ensure_ascii=False, indent=2), encoding='utf-8')


if __name__ == '__main__':
    main()

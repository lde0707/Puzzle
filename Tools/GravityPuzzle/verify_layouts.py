"""Portable reference for the Book-tower grid model, not Unity physics.

Read-only validation: python verify_layouts.py [--shortest] [--file PATH]
Derived from Tools/gravity_layouts.py. Explicit bounds and input checks added.
"""
import argparse
from collections import deque
import hashlib
import json
from pathlib import Path

AXES = ((0, -1), (-1, 0), (0, 1), (1, 0))


def cells(position, size):
    x, y = position
    return {(x + dx, y + dy) for dx in range(size) for dy in range(size)}


def inside(occupied, width, height):
    return all(0 <= x < width and 0 <= y < height for x, y in occupied)


def tilt(state, sizes, walls, direction, width, height):
    """Return a new settled state. Cargo identity is its stable array index."""
    if type(direction) is not int or direction not in range(4):
        raise ValueError("direction must be 0, 1, 2, or 3")
    dx, dy = AXES[direction]
    out = list(state)
    # Preserve the original generator's ordering, including stable index ties.
    order = sorted(range(len(out)),
                   key=lambda i: out[i][0] * dx + out[i][1] * dy,
                   reverse=True)
    for i in order:
        occupied = set(walls)
        for j, position in enumerate(out):
            if j != i:
                occupied.update(cells(position, sizes[j]))
        while True:
            candidate = (out[i][0] + dx, out[i][1] + dy)
            footprint = cells(candidate, sizes[i])
            if not inside(footprint, width, height) or footprint & occupied:
                break
            out[i] = candidate
    return tuple(out)


def integer(value, name, minimum=None):
    if type(value) is not int or (minimum is not None and value < minimum):
        raise ValueError(f"{name}: invalid integer {value!r}")
    return value


def validate(level):
    if not isinstance(level, dict):
        raise ValueError("level must be an object")
    if not isinstance(level.get("room"), str) or not level["room"]:
        raise ValueError("room must be a nonempty string")
    width = integer(level["width"], "width", 1)
    height = integer(level["height"], "height", 1)
    family = integer(level["family"], "family", 0)
    if family > 4:
        raise ValueError("family must be in 0..4")
    if not isinstance(level["walls"], list):
        raise ValueError("walls must be an array")
    wall_list = [(integer(p["x"], "wall.x"), integer(p["y"], "wall.y"))
                 for p in level["walls"]]
    walls = set(wall_list)
    if len(walls) != len(wall_list) or not inside(walls, width, height):
        raise ValueError("duplicate or out-of-bounds wall")
    cargo = level["cargo"]
    if not isinstance(cargo, list) or not cargo:
        raise ValueError("cargo must be a nonempty array")
    sizes = tuple(integer(c["size"], "cargo.size", 1) for c in cargo)
    if any(size not in (1, 2) for size in sizes):
        raise ValueError("this specification supports sizes 1 and 2")
    start = tuple((integer(c["x"], "cargo.x"), integer(c["y"], "cargo.y")) for c in cargo)
    goal = tuple((integer(c["gx"], "cargo.gx"), integer(c["gy"], "cargo.gy")) for c in cargo)
    for name, state in (("start", start), ("goal", goal)):
        occupied = set(walls)
        for i, position in enumerate(state):
            footprint = cells(position, sizes[i])
            if not inside(footprint, width, height) or footprint & occupied:
                raise ValueError(f"{name} cargo {i}: overlap or out of bounds")
            occupied.update(footprint)
    if start == goal:
        raise ValueError("level starts already solved")
    route = level["route"]
    if not isinstance(route, list) or not route:
        raise ValueError("route must be a nonempty array")
    if any(type(d) is not int or d not in range(4) for d in route):
        raise ValueError("route contains an invalid direction")
    return width, height, walls, sizes, start, goal, route


def shortest_length(start, goal, sizes, walls, width, height, max_depth, max_states):
    """BFS to the goal; hitting a search bound is an error, not a proof."""
    seen = {start}
    queue = deque([(start, 0)])
    while queue:
        state, depth = queue.popleft()
        if state == goal:
            return depth, len(seen)
        if depth >= max_depth:
            continue
        for direction in range(4):
            nxt = tilt(state, sizes, walls, direction, width, height)
            if nxt in seen:
                continue
            if len(seen) >= max_states:
                raise ValueError(f"BFS state limit {max_states} reached; shortest path unverified")
            seen.add(nxt)
            queue.append((nxt, depth + 1))
    raise ValueError(f"goal not found within depth {max_depth}; shortest path unverified")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--file", type=Path, default=Path(__file__).with_name("layouts.json"))
    parser.add_argument("--shortest", action="store_true")
    parser.add_argument("--max-states", type=int, default=40000)
    args = parser.parse_args()
    if args.max_states < 1:
        parser.error("--max-states must be positive")
    try:
        raw = args.file.read_bytes()
        document = json.loads(raw.decode("utf-8-sig"))
        levels = document["layouts"]
        if not isinstance(levels, list) or not levels:
            raise ValueError("layouts must be a nonempty array")
    except (OSError, ValueError, KeyError, TypeError) as error:
        print(f"FAIL input: {error}")
        return 1
    print("Model: original grid ordering + explicit board bounds; not Unity physics", flush=True)
    print(f"Input SHA256: {hashlib.sha256(raw).hexdigest()}", flush=True)
    failures = 0
    room_ids = set()
    for level in levels:
        label = level.get("room", "?") if isinstance(level, dict) else "?"
        try:
            width, height, walls, sizes, start, goal, route = validate(level)
            if level["room"] in room_ids:
                raise ValueError("duplicate room ID")
            room_ids.add(level["room"])
            state = start
            for direction in route:
                state = tilt(state, sizes, walls, direction, width, height)
            if state != goal:
                raise ValueError(f"route ends at {state}, expected {goal}")
            detail = f"cargo={len(sizes)} route={len(route)}"
            if args.shortest:
                length, states = shortest_length(start, goal, sizes, walls, width, height,
                                                  len(route), args.max_states)
                if length != len(route):
                    raise ValueError(f"route has {len(route)} moves, shortest has {length}")
                detail += f" shortest={length} visited={states}"
            print(f"PASS {label}: {detail}", flush=True)
        except (ValueError, KeyError, TypeError) as error:
            failures += 1
            print(f"FAIL {label}: {error}", flush=True)
    print(f"RESULT: {'PASS' if not failures else 'FAIL'}; levels={len(levels)}; failures={failures}")
    return 1 if failures else 0


if __name__ == "__main__":
    raise SystemExit(main())

"""Produce deterministic cross-language cases with the transferred Python oracle."""
import importlib.util
import json
import random
from pathlib import Path
root = Path(__file__).resolve().parents[2]
spec = importlib.util.spec_from_file_location('reference', Path(__file__).with_name('verify_layouts.py'))
reference = importlib.util.module_from_spec(spec)
spec.loader.exec_module(reference)
levels = json.loads((root/'Android Puzzle/Assets/Resources/GravityPuzzle/layouts.json').read_text(encoding='utf-8-sig'))['layouts']
rng = random.Random(1023)
cases = []
for level in levels:
    width, height, walls, sizes, start, goal, route = reference.validate(level)
    state = start
    for step in range(100):
        direction = rng.randrange(4)
        result = reference.tilt(state, sizes, walls, direction, width, height)
        cases.append(dict(room=level['room'], direction=direction,
            before=[dict(x=x,y=y) for x,y in state], after=[dict(x=x,y=y) for x,y in result]))
        state = result if step % 13 else start
output=root/'Android Puzzle/Assets/Editor/GravityPuzzle/reference_cases.json'
output.write_text(json.dumps(dict(cases=cases), separators=(',', ':')),encoding='utf-8')
print(f'{len(cases)} Python oracle cases written to {output}')

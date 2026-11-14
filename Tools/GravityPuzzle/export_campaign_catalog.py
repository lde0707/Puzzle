"""Export stable server IDs and theme-local labels together with each web build."""
import json
from pathlib import Path

THEMES=['기본 중력','별문 여행','크기 변환','숨바꼭질']
def theme(level):
    return 3 if level.get('blinkBlocks') else 2 if level.get('resizers') else 1 if level.get('portals') else 0
def build_catalog(repo):
    levels=[]
    for name in ['layouts','late_levels','resize_levels','blink_levels','expansion_levels']:
        path=repo/f'Android Puzzle/Assets/Resources/GravityPuzzle/{name}.json'
        levels+=json.loads(path.read_text(encoding='utf-8-sig'))['layouts']
    def extension(l):
        room=l['room'];return int(room[3:]) if len(room)==5 and room.startswith('E') and room[2]=='_' else 0
    levels.sort(key=lambda l:(theme(l),extension(l),len(l['route']),len(l['cargo']),max(c['size'] for c in l['cargo']),l['width']*l['height']))
    counts=[0]*4;catalog=[]
    for l in levels:
        t=theme(l);counts[t]+=1
        catalog.append(dict(room=l['room'],stage=counts[t],theme=t,themeName=THEMES[t],stageLabel=f'{THEMES[t]} {counts[t]}단계',optimum=len(l['route'])))
    assert len({l['room'] for l in catalog})==len(catalog)
    return catalog
def write_catalog(repo,site):
    catalog=build_catalog(repo);text=json.dumps(catalog,ensure_ascii=False,indent=2)
    for path in [site/'worker/campaign.json',site/'public/campaign.json']:path.write_text(text,encoding='utf-8')
    print(f'Exported {len(catalog)} server and public stages with stable IDs and theme-local labels')
if __name__=='__main__':
    repo=Path(__file__).resolve().parents[2];write_catalog(repo,repo/'Sites/mallang-orbit')

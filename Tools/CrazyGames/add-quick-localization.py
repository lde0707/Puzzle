import json
from pathlib import Path

path=Path(__file__).resolve().parents[2]/'Android Puzzle/Assets/Resources/GravityPuzzle/localization.json'
data=json.loads(path.read_text(encoding='utf-8-sig'))
rows=[
    ('다음 단계','Next stage','下一关','次のステージ'),
    ('사용법','How to play','玩法说明','遊び方'),
    ('방향을 누르면 길이 열리고 닫혀요.','Each move opens and closes paths.','每次移动都会切换道路的开合。','動くたびに道が開いたり閉じたりします。'),
    ('변환대에 멈춰 크기를 바꾸세요. 목적지의 색과 크기를 맞춰요.','Stop on a pad to resize. Match your goal’s color and size.','停在变形台上改变大小。匹配终点的颜色和大小。','変身台で止まるとサイズが変化。ゴールの色と大きさを合わせよう。'),
    ('빛나는 면으로 들어가요. 포탈을 누르면 출구가 보여요.','Enter the glowing face. Tap a portal to see its exit.','从发光的一面进入。点击传送门查看出口。','光る面から入ろう。ポータルをタップすると出口がわかります。'),
    ('방향 하나로 모두 데굴! 같은 색 목적지에 도착하세요.','One direction rolls everyone! Reach the matching colors.','一个方向，全员滚动！到达相同颜色的终点。','ひとつの方向でみんなコロコロ！同じ色のゴールへ。'),
]
existing={r['ko']:r for r in data['rows']}
for values in rows:
    row=dict(zip(('ko','en','zh','ja'),values))
    if row['ko'] in existing:existing[row['ko']].update(row)
    else:data['rows'].append(row)
path.write_text(json.dumps(data,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')

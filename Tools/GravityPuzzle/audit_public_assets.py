from pathlib import Path
import urllib.request, json, time, hashlib
root=Path(__file__).resolve().parents[2]
site=root/'Sites/mallang-orbit'
base='https://mallang-orbit-puzzle.lde0707.chatgpt.site/'
manifest=json.loads((site/'public/game-assets.json').read_text())
paths=['','game-assets.json','community.js','game-loader.js',manifest['data']['url'],manifest['wasm']['url']]
rows=[]
for path in paths:
    start=time.perf_counter()
    try:
        with urllib.request.urlopen(urllib.request.Request(base+path,headers={'User-Agent':'ReleaseAudit/1.0'}),timeout=40) as response:
            headers=dict(response.headers);ttfb=time.perf_counter()-start;body=response.read()
            local=site/'public'/path.split('?')[0]
            matches=local.is_file() and hashlib.sha256(local.read_bytes()).digest()==hashlib.sha256(body).digest()
            rows.append(dict(path=path,status=response.status,bytes=len(body),ttfb=round(ttfb,3),seconds=round(time.perf_counter()-start,3),matchesLocal=matches,contentType=headers.get('Content-Type'),cache=headers.get('Cache-Control')))
    except Exception as e:rows.append(dict(path=path,error=str(e)))
out=root/'Android Puzzle/Logs/release-audit-public-assets.json'
out.write_text(json.dumps(rows,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps(rows,ensure_ascii=False,indent=2))

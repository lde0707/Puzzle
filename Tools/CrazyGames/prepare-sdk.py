"""Package official HTML5 SDK integration, preserving the separate itch.io release."""
from pathlib import Path
import gzip,hashlib,json,zipfile,argparse

parser=argparse.ArgumentParser()
parser.add_argument('--version',default='1.4.0')
parser.add_argument('--date',default='2026-11-04')
parser.add_argument('--log',default='build-quick.log')
args=parser.parse_args()

ROOT=Path(__file__).resolve().parents[2]
HERE=Path(__file__).parent
OLD=ROOT/'Releases/itchio/2026-11-02-v1.1.0'
BUILD=ROOT/'Android Puzzle/Builds/iPhoneWebTest'
OUT=ROOT/f'Releases/crazygames/{args.date}-v{args.version}-sdk'
baseline=json.loads((OLD/'package-checks.json').read_text(encoding='utf-8'))
log=(HERE/args.log).read_text(encoding='utf-8',errors='replace')
assert 'CRAZYGAMES CHECKS PASS' in log and 'iPhone web test built successfully' in log, 'Run CrazyPortalChecks.Build successfully first'
files={}
for name,digest in baseline['files'].items():
    data=(OLD/'web'/name).read_bytes()
    assert hashlib.sha256(data).hexdigest()==digest,name
    files[name]=data
manifest=json.loads(files['game-assets.json'])
for kind in ('data','wasm'):
    raw=(BUILD/'Build'/('iPhoneWebTest.'+kind)).read_bytes()
    name='Build/iPhoneWebTest.'+kind+'.gz';packed=gzip.compress(raw,compresslevel=9,mtime=0)
    files[name]=packed
    manifest[kind].update(url=name+'?v='+hashlib.sha256(raw).hexdigest()[:16],bytes=len(packed),decodedBytes=len(raw))
for kind in ('framework','loader'):
    name='Build/iPhoneWebTest.'+kind+'.js';files[name]=(BUILD/name).read_bytes()
    manifest[kind]=name+'?v='+hashlib.sha256(files[name]).hexdigest()[:16]
files['game-assets.json']=json.dumps(manifest,indent=2).encode()
files['localized.js']=(ROOT/'Tools/Itch/localized.js').read_bytes()
files['crazy-bridge.js']=(HERE/'crazy-bridge.mjs').read_bytes()
files['crazy-start.js']=(HERE/'crazy-start.js').read_text(encoding='utf-8').replace('./crazy-bridge.mjs','./crazy-bridge.js').encode()
# Keep original lookup keys in the shared bridge; substitute provider-specific paragraphs.
community=files['community.js'].decode()
community=community.replace('// itch.io standalone edition:', '// CrazyGames SDK edition:')
community=community.replace("'itch.io 기록 안내'","'Progress information'")
community=community.replace('이 itch.io 버전은 광고와 온라인 랭킹 없이 플레이할 수 있어요. 게임이 닉네임이나 플레이 기록을 별도 랭킹 서버로 전송하지 않아요.','진행 상황은 CrazyGames 저장 시스템을 사용해요. 온라인 랭킹은 제공하지 않아요.')
community=community.replace('단계·별·스킨은 현재 브라우저에 저장돼요. 단계 선택에서 최고 별과 예상 상위 등급을, 단계 상세에서 누적 이동·초기화 횟수를 볼 수 있어요.','로그인한 계정은 클라우드에, 게스트는 이 브라우저에 저장돼요. 힌트는 계정 전체에서 공유해요.')
community=community.replace('itch.io 자체의 접속 정보 처리는 itch.io 개인정보처리방침을 따라요.','CrazyGames의 데이터 처리는 해당 서비스의 개인정보처리방침을 따라요.')
files['community.js']=community.encode()
backup=files['backup.js'].decode().replace('기존 공개 링크에서도 설정 → 기록 옮기기에서 파일을 내보낼 수 있어요. itch.io 로그인만으로 게임 기록이 자동 동기화되지는 않아요.','다른 사이트의 기록은 저장 파일로 옮겨 주세요. CrazyGames 계정의 진행 상황은 SDK로 저장돼요.')
files['backup.js']=backup.encode()
html=files['index.html'].decode().replace('<title>데굴데굴</title>','<title>Degul Degul</title>').replace('aria-label="데굴데굴 퍼즐"','aria-label="Degul Degul puzzle"')
html=html.replace('?v=itch-1.1.0','?v=cg-sdk-'+args.version)
html=html.replace('<script type="module" src="game-loader.js?v=cg-sdk-'+args.version+'"></script>','<script src="https://sdk.crazygames.com/crazygames-sdk-v3.js"></script>\n<script>window.DEGUL_CG_MONETIZATION=MODE;</script>\n<script type="module" src="crazy-start.js?v='+args.version+'"></script>')
assert 'MODE' in html
html=html.replace('overflow:hidden;background:', 'overflow:hidden;-webkit-user-select:none;user-select:none;background:')
for mode in ('basic','monetized'):
    files['index.html']=html.replace('=MODE;', '=true;' if mode=='monetized' else '=false;').encode()
    dest=OUT/mode;web=dest/'web';web.mkdir(parents=True,exist_ok=True)
    for name,data in files.items():
        target=web/name;target.parent.mkdir(parents=True,exist_ok=True);target.write_bytes(data)
    assert {p.relative_to(web).as_posix() for p in web.rglob('*') if p.is_file()}==set(files)
    archive=dest/f'degul-degul-crazygames-{mode}-sdk-{args.version}.zip'
    with zipfile.ZipFile(archive,'w',zipfile.ZIP_DEFLATED,compresslevel=6) as z:
        for name,data in sorted(files.items()):z.writestr(name,data)
    with zipfile.ZipFile(archive) as z:assert z.testzip() is None
    total=sum(map(len,files.values()));assert total<50_000_000 and len(files)<1500
    report=dict(mode=mode,sdk='Official CrazyGames HTML5 SDK v3',engineSelection='HTML5',saveSelection='Yes, using the Data Module from the CrazyGames SDK',zip=archive.name,zipBytes=archive.stat().st_size,bundleBytes=total,fileCount=len(files),sha256=hashlib.sha256(archive.read_bytes()).hexdigest(),files={n:hashlib.sha256(v).hexdigest() for n,v in files.items()},adsEnabled=mode=='monetized',published=False,portalQA='pending',hintPolicy='3 account-wide initial hints; +1/hour up to 3; optional ad reveals one extra hint; no stage grants')
    (dest/'package-checks.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
    print(mode,archive.stat().st_size,total,report['sha256'])

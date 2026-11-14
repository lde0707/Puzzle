"""Package the verified public Unity build for itch.io; never publish or copy saves."""
from pathlib import Path
import gzip
import hashlib
import json
import shutil
import zipfile
import argparse

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / 'Sites/mallang-orbit/public'
RELEASE = ROOT / 'Releases/itchio/2026-11-02-v1.1.0'
WEB = RELEASE / 'web'
TEMPLATES = Path(__file__).parent

def prepare():
    global RELEASE, WEB
    parser=argparse.ArgumentParser()
    parser.add_argument('--build',type=Path,required=True,help='Successful multilingual Unity WebGL build directory')
    parser.add_argument('--release',type=Path,default=RELEASE)
    parser.add_argument('--version',default='1.1.0')
    args=parser.parse_args()
    RELEASE=args.release.resolve();WEB=RELEASE/'web'
    WEB.mkdir(parents=True, exist_ok=True)
    manifest = json.loads((SOURCE / 'game-assets.json').read_text(encoding='utf-8'))
    catalog = json.loads((SOURCE / 'campaign.json').read_text(encoding='utf-8'))
    assert len(catalog) == 128
    names = ['index.html', 'game-loader.js', 'game-assets.js', 'game-assets.json',
             'degul-degul-wordmark-ko.png', 'degul-degul-wordmark-en.png',
             'mallang-orbit-icon.png', 'mallang-orbit-icon.svg']
    for kind in ('data', 'wasm'):
        info = manifest[kind]
        name = info['url'].split('?')[0]
        packed = (SOURCE / name).read_bytes()
        decoded = gzip.decompress(packed)
        assert len(packed) == info['bytes'] and len(decoded) == info['decodedBytes']
        assert hashlib.sha256(decoded).hexdigest()[:16] == info['url'].split('?v=')[1]
        names.append(name)
    names += [manifest[k].split('?')[0] for k in ('framework', 'loader')]
    for name in names:
        dst = WEB / name
        dst.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(SOURCE / name, dst)
    for name in ('community.js', 'backup.js'):
        shutil.copy2(TEMPLATES / name, WEB / name)
    shutil.copy2(TEMPLATES/'localized.js',WEB/'localized.js');names.append('localized.js')
    # Some static hosts serve .mjs as text/plain, which browsers reject as modules.
    shutil.copy2(SOURCE / 'backup-core.mjs', WEB / 'backup-core.js')
    names.append('backup-core.js')
    legacy = WEB / 'backup-core.mjs'
    if legacy.exists():
        legacy.unlink()
    html = (WEB / 'index.html').read_text(encoding='utf-8')
    html = html.replace('community.js?v=5-theme-128', 'community.js?v=itch-local-1')
    html = html.replace('backup.js?v=2-degul', 'backup.js?v=itch-transfer-1')
    html = html.replace('<script src="community.js','<script src="localized.js"></script>\n<script src="community.js')
    html = html.replace('<html lang="ko">','<html lang="en">')
    html = html.replace('게임 파일을 준비하고 있어요…','Preparing game files…').replace('다시 불러오기','Retry')
    html = html.replace('src="degul-degul-wordmark-ko.png"','src="degul-degul-wordmark-en.png"')
    html = html.replace('<img class="brand-english"','<img hidden class="brand-english"')
    html = html.replace('.brand-english{display:block;','.brand-english{display:none;')
    html = html.replace('game-loader.js?v=2-degul','game-loader.js?v=itch-'+args.version)
    html = html.replace('community.js?v=itch-local-1','community.js?v=itch-'+args.version)
    html = html.replace('backup.js?v=itch-transfer-1','backup.js?v=itch-'+args.version)
    html = html.replace('localized.js"','localized.js?v=itch-'+args.version+'"')
    (WEB / 'index.html').write_text(html, encoding='utf-8')
    names += ['community.js', 'backup.js']
    for font in ('Font-OFL.txt', 'Jua-OFL.txt', 'NotoCJK-OFL.txt'):
        name = 'Licenses/' + font
        (WEB/'Licenses').mkdir(exist_ok=True)
        shutil.copy2(ROOT/'Android Puzzle/Assets/Resources/GravityPuzzle'/font, WEB/name)
        names.append(name)
    if args.build:
        for kind in ('data','wasm'):
            raw=(args.build/'Build'/('iPhoneWebTest.'+kind)).read_bytes()
            name='Build/iPhoneWebTest.'+kind+'.gz';packed=gzip.compress(raw,compresslevel=9,mtime=0)
            (WEB/name).write_bytes(packed)
            manifest[kind].update(url=name+'?v='+hashlib.sha256(raw).hexdigest()[:16],bytes=len(packed),decodedBytes=len(raw))
        for kind in ('framework','loader'):
            name='Build/iPhoneWebTest.'+kind+'.js';shutil.copy2(args.build/name,WEB/name)
            manifest[kind]=name+'?v='+hashlib.sha256((WEB/name).read_bytes()).hexdigest()[:16]
        (WEB/'game-assets.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
    loader=(WEB/'game-loader.js').read_text(encoding='utf-8')
    for ko,en in {'실행 파일을 불러오지 못했어요.':'Could not load the game script.','게임 목록을 불러오지 못했어요.':'Could not load the game manifest.','게임 파일 준비 중… ':'Preparing game files… ','말랑한 우주를 준비하고 있어요…':'Preparing your squishy universe…','저장된 게임 파일을 사용했어요.':'Using cached game files.','다음 접속부터 저장된 파일을 사용해요.':'Next visit will use cached files.','실행하지 못했어요: ':'Could not start: '}.items():loader=loader.replace(ko,en)
    (WEB/'game-loader.js').write_text(loader,encoding='utf-8')
    assert set(names) == {p.relative_to(WEB).as_posix() for p in WEB.rglob('*') if p.is_file()}, 'Unexpected staged file; inspect before packaging'
    archive = RELEASE / ('degul-degul-itchio-'+args.version+'.zip')
    with zipfile.ZipFile(archive, 'w', zipfile.ZIP_DEFLATED, compresslevel=6) as z:
        for name in sorted(names):
            z.write(WEB / name, name)
    with zipfile.ZipFile(archive) as z:
        assert z.testzip() is None and 'index.html' in z.namelist()
        assert len(z.infolist()) < 1000
        assert sum(i.file_size for i in z.infolist()) < 500_000_000
        assert all(i.file_size < 200_000_000 and len(i.filename) <= 240 for i in z.infolist())
    report = dict(build=str(args.build) if args.build else 'public-v20 / Unity 2026-11-02', stages=128, themes=4,
                  languages=['en','ko','zh','ja'],defaultLanguage='en',
                  stagesPerTheme=32, mode='local-save / no ads / no online ranking',
                  zip=archive.name, bytes=archive.stat().st_size,
                  sha256=hashlib.sha256(archive.read_bytes()).hexdigest(),
                  files={n: hashlib.sha256((WEB/n).read_bytes()).hexdigest() for n in sorted(names)},
                  verified=['ZIP integrity','index.html at root','128-stage source catalog',
                            'Unity data/wasm lengths and SHA256 revisions','itch.io archive limits',
                            'allowlisted assets only; no source, credentials or player saves'])
    (RELEASE/'package-checks.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
    print(json.dumps({k:report[k] for k in ('zip','bytes','sha256','stages','mode')},ensure_ascii=False))

if __name__ == '__main__':
    prepare()

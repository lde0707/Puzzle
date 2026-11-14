"""Add opt-in shared rankings to the published v1.1 build without replacing Unity/save files."""
from pathlib import Path
import argparse,gzip,hashlib,json,re,shutil,zipfile
ROOT=Path(__file__).resolve().parents[2]
BASE=ROOT/'Releases/itchio/2026-11-02-v1.1.0/web'
RELEASE=ROOT/'Releases/itchio/2026-11-04-v1.2.1'
WEB=RELEASE/'web'
API='https://mallang-orbit-puzzle.lde0707.chatgpt.site'
def digest(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def main():
 parser=argparse.ArgumentParser();parser.add_argument('--build',type=Path);args=parser.parse_args()
 WEB.mkdir(parents=True,exist_ok=True)
 for p in BASE.rglob('*'):
  if p.is_file():
   dest=WEB/p.relative_to(BASE);dest.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(p,dest)
 shutil.copy2(ROOT/'Sites/mallang-orbit/public/community.js',WEB/'community.js')
 shutil.copy2(ROOT/'Sites/mallang-orbit/public/campaign.json',WEB/'campaign.json')
 shutil.copy2(ROOT/'Tools/Itch/ranking-i18n.js',WEB/'ranking-i18n.js')
 shutil.copy2(ROOT/'Tools/Itch/save-continuity.js',WEB/'save-continuity.js')
 if args.build:
  manifest=json.loads((WEB/'game-assets.json').read_text(encoding='utf-8'))
  for kind in ('data','wasm'):
   raw=(args.build/'Build'/('iPhoneWebTest.'+kind)).read_bytes()
   name='Build/iPhoneWebTest.'+kind+'.gz';packed=gzip.compress(raw,compresslevel=9,mtime=0)
   (WEB/name).write_bytes(packed)
   manifest[kind].update(url=name+'?v='+hashlib.sha256(raw).hexdigest()[:16],bytes=len(packed),decodedBytes=len(raw))
  for kind in ('framework','loader'):
   name='Build/iPhoneWebTest.'+kind+'.js';shutil.copy2(args.build/name,WEB/name)
   manifest[kind]=name+'?v='+digest(WEB/name)[:16]
  (WEB/'game-assets.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
 loader=(WEB/'game-loader.js').read_text(encoding='utf-8')
 loader="import {preserveItchSave} from './save-continuity.js?v=1.2.0';\n"+loader
 loader=loader.replace('async function boot(){','async function boot(){\n  await preserveItchSave();')
 (WEB/'game-loader.js').write_text(loader,encoding='utf-8')
 (WEB/'ranking-config.js').write_text('window.DEGUL_COMMUNITY_CONFIG='+json.dumps(dict(apiBase=API,storageKey='degul-itch-5285502-community-v1',platform='itch',campaignUrl='campaign.json'))+';\n',encoding='utf-8')
 html=(WEB/'index.html').read_text(encoding='utf-8')
 html=re.sub(r'community\.js\?[^"\s]+','community.js?v=itch-ranking-1.2.1',html)
 html=html.replace('<script src="community.js','<script src="ranking-config.js?v=1.2.1"></script>\n<script src="ranking-i18n.js?v=1.2.1"></script>\n<script src="community.js')
 (WEB/'index.html').write_text(html,encoding='utf-8')
 unchanged=[p.relative_to(BASE).as_posix() for p in BASE.rglob('*') if p.is_file() and p.name not in ['index.html','community.js','game-loader.js'] and not(args.build and ('Build' in p.parts or p.name=='game-assets.json'))]
 assert all(digest(BASE/n)==digest(WEB/n) for n in unchanged)
 names=sorted(p.relative_to(WEB).as_posix() for p in WEB.rglob('*') if p.is_file())
 assert set(names)==set(p.relative_to(BASE).as_posix() for p in BASE.rglob('*') if p.is_file())|{'campaign.json','ranking-config.js','ranking-i18n.js','save-continuity.js'}
 archive=RELEASE/'degul-degul-itchio-1.2.1.zip'
 with zipfile.ZipFile(archive,'w',zipfile.ZIP_DEFLATED,compresslevel=6) as z:
  for n in names:z.write(WEB/n,n)
 with zipfile.ZipFile(archive) as z:
  assert z.testzip() is None and 'index.html' in z.namelist()
  assert len(names)<1000 and sum(i.file_size for i in z.infolist())<500_000_000
  assert all(i.file_size<200_000_000 and len(i.filename)<240 for i in z.infolist())
 report=dict(version='1.2.1',build=str(args.build) if args.build else None,baseline='Published itch.io multilingual v1.1.0 (live manifest/loader matched)',apiBase=API,zip=archive.name,bytes=archive.stat().st_size,sha256=digest(archive),unchangedFiles=unchanged,files={n:digest(WEB/n) for n in names},ads=False,ranking='explicit opt-in; shared ranking; pause retains identity and records; player deletion disabled')
 (RELEASE/'package-checks.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
 print(json.dumps({k:report[k] for k in ['zip','bytes','sha256']},ensure_ascii=False))
if __name__=='__main__':main()

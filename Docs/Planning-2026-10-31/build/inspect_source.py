from pathlib import Path
from zipfile import ZipFile
from lxml import etree as E
import json,collections
p=Path('C:/Users/doun0/OneDrive/바탕 화면/File/GitHub/Roguelike-Defense/Rushitadel UI기획서.pptx')
ns={'a':'http://schemas.openxmlformats.org/drawingml/2006/main','p':'http://schemas.openxmlformats.org/presentationml/2006/main'}
out=[]
with ZipFile(p) as z:
 names=sorted([n for n in z.namelist() if __import__('re').match(r'ppt/slides/slide\d+.xml$',n)],key=lambda n:int(__import__('re').search(r'(\d+)\.xml',n).group(1)))
 for n in names:
  e=E.fromstring(z.read(n));out.append({'slide':n,'text':e.xpath('//a:t/text()',namespaces=ns),'fonts':e.xpath('//a:latin/@typeface',namespaces=ns),'sizes':e.xpath('//a:rPr/@sz',namespaces=ns)})
 print(z.read('ppt/presentation.xml').decode()[:1500])
Path('Docs/Planning-2026-10-31/build/reference.json').write_text(json.dumps(out,ensure_ascii=False,indent=2),encoding='utf8')
for s in out: print(s['slide'], ' | '.join(s['text'])[:700])
print('FONTS',collections.Counter(x for s in out for x in s['fonts']))

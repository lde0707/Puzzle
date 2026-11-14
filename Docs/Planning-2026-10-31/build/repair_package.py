from zipfile import ZipFile,ZIP_DEFLATED
from pathlib import Path
from lxml import etree as E
W=Path(__file__).resolve().parent
R='http://schemas.openxmlformats.org/officeDocument/2006/relationships'
for kind in ['ui','system']:
 src=W/f'{kind}-media.pptx';dst=W/f'{kind}-ready.pptx';fixed=0
 with ZipFile(src) as z,ZipFile(dst,'w',ZIP_DEFLATED) as out:
  for n in z.namelist():
   d=z.read(n)
   if n.startswith('ppt/slides/slide') and n.endswith('.xml'):
    e=E.fromstring(d)
    for t in e.iter():
     for k,v in list(t.attrib.items()):
      if k.startswith('{'+R+'}') and not v:del t.attrib[k];fixed+=1
    d=E.tostring(e,xml_declaration=True,encoding='UTF-8',standalone=True)
   out.writestr(n,d)
 print(kind,'removed empty optional relationship attributes',fixed)
 with ZipFile(dst) as z:
  e=E.fromstring(z.read('ppt/slides/slide6.xml'));ns={'a':'http://schemas.openxmlformats.org/drawingml/2006/main'}
  print('slide6 table header',E.tostring(e.xpath('//a:tbl/a:tr',namespaces=ns)[0]).decode()[:1800])

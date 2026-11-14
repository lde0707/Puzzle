from pathlib import Path
from lxml import etree as E
import zipfile,json,copy,hashlib
W=Path(__file__).resolve().parents[1]
ns={'p':'http://schemas.openxmlformats.org/presentationml/2006/main','a':'http://schemas.openxmlformats.org/drawingml/2006/main'}
config=json.loads((W/'build/ui-annotation-map.json').read_text('utf8'))
content=json.loads((W/'build/content.json').read_text('utf8'))['ui']
source=W/'build/ui-original-v1.0.pptx'
def replace_text(body,value):
    paragraphs=body.findall('a:p',ns)
    base=copy.deepcopy(paragraphs[0])
    for old in paragraphs:body.remove(old)
    for line in value.split('\n'):
        p=copy.deepcopy(base)
        runs=p.findall('a:r',ns)
        if runs:
            r=copy.deepcopy(runs[0])
            for child in list(p):
                if E.QName(child).localname not in ['pPr','endParaRPr']:p.remove(child)
            t=r.find('a:t',ns)
            if t is None:t=E.SubElement(r,'{'+ns['a']+'}t')
            t.text=line
            p.insert(1 if p.find('a:pPr',ns) is not None else 0,r)
        else:
            r=E.SubElement(p,'{'+ns['a']+'}r');E.SubElement(r,'{'+ns['a']+'}t').text=line
        body.append(p)
def shape_text(root,old,new):
    for sp in root.findall('.//p:sp',ns):
        b=sp.find('p:txBody',ns)
        if b is not None and ''.join(b.itertext()).strip():
            text=''.join(b.findall('.//a:t',ns)[i].text or '' for i in range(len(b.findall('.//a:t',ns))))
            if text==old:replace_text(b,new)
with zipfile.ZipFile(source) as z,zipfile.ZipFile(W/'build/ui-number-overlays.pptx') as overlays:
    blobs={n:z.read(n) for n in z.namelist()}
    for key,c in config.items():
        id=int(key);num=id+2;name=f'ppt/slides/slide{num}.xml'
        root=E.fromstring(blobs[name]);ov=E.fromstring(overlays.read(name))
        tree=root.find('p:cSld/p:spTree',ns)
        nextid=max(int(n.attrib['id']) for n in root.findall('.//p:cNvPr',ns))+1
        for sp in ov.findall('p:cSld/p:spTree/p:sp',ns):
            sp=copy.deepcopy(sp)
            for prop in sp.findall('.//p:cNvPr',ns):prop.set('id',str(nextid));nextid+=1
            tree.append(sp)
        tbl=root.find('.//a:tbl',ns)
        if c.get('labels'):
            rows=tbl.findall('a:tr',ns)[1:]
            assert len(rows)==len(c['labels'])
            for i,(row,label) in enumerate(zip(rows,c['labels'])):
                cells=row.findall('a:tc',ns)
                replace_text(cells[0].find('a:txBody',ns),label)
                if c.get('details'):replace_text(cells[1].find('a:txBody',ns),c['details'][i])
            replace_text(tbl.findall('a:tr',ns)[0].findall('a:tc',ns)[0].find('a:txBody',ns),'번호 · 요소')
        else:
            for i,row in enumerate(content[id-1]['rows']):shape_text(root,row[0],f'{i+1}  {row[0]}')
        shape_text(root,'화면 예시 · 테스트 데이터','번호 = 설명 표 · 테스트 화면')
        blobs[name]=E.tostring(root,xml_declaration=True,encoding='UTF-8',standalone=True)
    name='ppt/slides/slide3.xml';root=E.fromstring(blobs[name]);cells=root.findall('.//a:tbl/a:tr',ns)[3].findall('a:tc',ns)
    replace_text(cells[1].find('a:txBody',ns),'사진의 번호·테두리와 설명 표의 같은 번호를 연결한다.\n번호는 슬라이드마다 1부터 시작하며 도형으로 편집 가능.')
    blobs[name]=E.tostring(root,xml_declaration=True,encoding='UTF-8',standalone=True)
    name='ppt/slides/slide1.xml';root=E.fromstring(blobs[name]);shape_text(root,'v1.0  |  기준일 2026.10.31','v1.1  |  화면 요소 번호 추가 · 2026.10.31');blobs[name]=E.tostring(root,xml_declaration=True,encoding='UTF-8',standalone=True)
    with zipfile.ZipFile(W/'build/ui-numbered-candidate.pptx','w',zipfile.ZIP_DEFLATED) as dest:
        for name,data in blobs.items():dest.writestr(name,data)
    assert all(blobs[n]==z.read(n) for n in z.namelist() if n.startswith('ppt/media/') or n.endswith('.rels'))
print('Native annotations merged; images, media, and relationships preserved byte-for-byte.')


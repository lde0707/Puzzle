from pathlib import Path
import json,re,sys
ROOT=Path(__file__).resolve().parents[2]
HERE=Path(__file__).parent
RES=ROOT/'Android Puzzle/Assets/Resources/GravityPuzzle'
rows={}
for path in sorted(HERE.glob('translations*.tsv')):
    for line in path.read_text(encoding='utf-8').splitlines():
        if not line: continue
        fields=[x.replace('\\n','\n') for x in line.split('|')]
        assert len(fields)==4,(path,line)
        ko,en,zh,ja=fields
        assert ko not in rows,ko
        rows[ko]=dict(ko=ko,en=en,zh=zh,ja=ja)
(RES/'localization.json').write_text(json.dumps(dict(rows=list(rows.values())),ensure_ascii=False,indent=2),encoding='utf-8')
# Identical vocabulary for browser backup screens and Unity UI.
js='const rows='+json.dumps(list(rows.values()),ensure_ascii=False)+';\n'
js+='''const dict=new Map(rows.map(r=>[r.ko,r]));
const escape=s=>s.replace(/[.*+?^${}()|[\\]\\\\]/g,'\\\\$&');
const pattern=new RegExp([...dict.keys()].sort((a,b)=>b.length-a.length).map(escape).join('|'),'g');
window.degulT=value=>{const lang=window.degulLanguage||'en';if(lang==='ko'||!value)return value;const pick=r=>r[lang]??r.en;return dict.has(value)?pick(dict.get(value)):value.replace(pattern,m=>pick(dict.get(m)));};
'''
(HERE/'localized.js').write_text(js,encoding='utf-8')
# Subset official OFL fonts. Renamed subsets avoid presenting modified files as originals.
sys.path.insert(0,str(HERE/'.deps'))
from fontTools import subset
from fontTools.ttLib import TTFont
for code,locale in [('zh','SC'),('ja','JP')]:
    text=''.join(r[code] for r in rows.values())+'English한국어简体中文日本語✓★●0123456789% /×+−·…→←↑↓'
    text+=''.join(chr(i) for i in range(32,127))
    font=TTFont(HERE/'font-sources'/('sc.otf' if code=='zh' else 'jp.otf'))
    missing=set(map(ord,text))-set(font.getBestCmap())
    missing-={10,13}
    assert not missing,missing
    options=subset.Options();options.name_IDs=['*'];options.name_legacy=True;options.name_languages=['*']
    worker=subset.Subsetter(options=options);worker.populate(text=text);worker.subset(font)
    for record in font['name'].names:
        if record.nameID in (1,3,4,6,16,17):
            value='Regular' if record.nameID==17 else 'Degul CJK '+locale if record.nameID!=6 else 'DegulCJK-'+locale
            record.string=value.encode(record.getEncoding())
    font.save(RES/('DegulCJK-'+locale+'.otf'))
print('Prepared',len(rows),'four-language entries and subset fonts.')

"""Create a Basic Launch preview bundle from the verified multilingual release.

Does not publish, accept terms, add SDK/ads, or access player data.
"""
from pathlib import Path
import hashlib
import json
import zipfile

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / 'Releases/itchio/2026-11-02-v1.1.0'
DEST = ROOT / 'Releases/crazygames/2026-11-02-v1.1.0'


def prepare():
    baseline = json.loads((SOURCE / 'package-checks.json').read_text(encoding='utf-8'))
    files = {}
    for name, expected in baseline['files'].items():
        data = (SOURCE / 'web' / name).read_bytes()
        assert hashlib.sha256(data).hexdigest() == expected, f'Source changed: {name}'
        files[name] = data
    html = files['index.html'].decode('utf-8')
    html = html.replace('<title>데굴데굴</title>', '<title>Degul Degul</title>')
    html = html.replace('content="데굴데굴"', 'content="Degul Degul"')
    html = html.replace('aria-label="데굴데굴 퍼즐"', 'aria-label="Degul Degul puzzle"')
    html = html.replace('alt="데굴데굴"', 'alt="Degul Degul"').replace('?v=itch-', '?v=crazy-basic-')
    files['index.html'] = html.encode('utf-8')
    community = files['community.js'].decode('utf-8')
    community = community.replace('// itch.io standalone edition:', '// CrazyGames Basic Launch preview:')
    community = community.replace("panel.setAttribute('aria-label','itch.io 기록 안내')", "panel.setAttribute('aria-label','Progress information')")
    # Retain translation lookup keys; adapt the displayed platform after translation.
    community = community.replace('window.degulT(value)', "window.degulT(value).replaceAll('itch.io','CrazyGames')")
    files['community.js'] = community.encode('utf-8')
    assert 'fetch(' not in community and 'XMLHttpRequest' not in community
    size = sum(map(len, files.values()))
    assert size <= 50_000_000, 'SDK-free bundle exceeds the Basic Launch 50 MB limit'
    assert len(files) <= 1500
    assert 'index.html' in files
    DEST.mkdir(parents=True, exist_ok=True)
    archive = DEST / 'degul-degul-crazygames-basic-1.1.0.zip'
    for name, data in files.items():
        path = DEST / 'web' / name
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_bytes(data)
    with zipfile.ZipFile(archive, 'w', zipfile.ZIP_DEFLATED, compresslevel=6) as z:
        for name, data in sorted(files.items()):
            z.writestr(name, data)
    with zipfile.ZipFile(archive) as z:
        assert z.testzip() is None
        assert set(z.namelist()) == set(files)
    report = {
        'target': 'CrazyGames Basic Launch preview, SDK-free',
        'zip': archive.name, 'zipBytes': archive.stat().st_size,
        'bundleBytes': size, 'fileCount': len(files),
        'sha256': hashlib.sha256(archive.read_bytes()).hexdigest(),
        'sourceRelease': str(SOURCE.relative_to(ROOT)),
        'sourceFilesVerified': len(files), 'defaultLanguage': 'en',
        'languages': ['en', 'ko', 'zh', 'ja'],
        'ads': False, 'onlineRanking': False, 'cloudSave': False,
        'mobileHomepage20MBLimitMet': size <= 20_000_000,
        'validation': ['Source hashes', 'ZIP integrity', 'Root index.html', '50MB SDK-free bundle limit', '1500-file limit'],
        'pending': ['Developer Portal preview and QA', 'Cover assets in three required sizes', 'Landscape and portrait gameplay preview videos', 'Chrome/Edge and physical mobile validation'],
        'published': False,
    }
    (DEST / 'package-checks.json').write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding='utf-8')
    print(json.dumps(report, ensure_ascii=False, indent=2))


if __name__ == '__main__':
    prepare()

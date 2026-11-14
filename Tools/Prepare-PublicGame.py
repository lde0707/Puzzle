"""Refresh the existing Sites game's assets after a successful Unity Web build.

Keep the hosted index.html: it expands gzip assets explicitly before invoking Unity.
This script does not publish or alter the local Unity build.
"""
from pathlib import Path
import gzip
import hashlib
import json
import re
import shutil
from GravityPuzzle.export_campaign_catalog import write_catalog

repo = Path(__file__).resolve().parents[1]
source = repo / "Android Puzzle/Builds/iPhoneWebTest"
site = repo / "Sites/mallang-orbit"
target = site / ("public" if (site / "public/index.html").is_file() else "dist")
assert (source / "index.html").is_file(), "Build Unity Web first"
assert (target / "index.html").is_file(), "Existing hosted loader is required"
assert "game-loader.js" in (target / "index.html").read_text(encoding="utf-8")
for asset in source.rglob("*"):
    if not asset.is_file() or asset.name == "index.html":
        continue
    output = target / asset.relative_to(source)
    output.parent.mkdir(parents=True, exist_ok=True)
    if asset.suffix in (".wasm", ".data"):
        original = asset.read_bytes()
        packed = gzip.compress(original, compresslevel=9, mtime=0)
        assert gzip.decompress(packed) == original
        output = output.with_name(output.name + ".gz")
        output.write_bytes(packed)
        print(f"{asset.name}: {len(original)} -> {len(packed)} bytes")
    else:
        shutil.copy2(asset, output)
    assert output.stat().st_size < 25 * 1024 * 1024
revision = hashlib.sha256((source / "Build/iPhoneWebTest.wasm").read_bytes() + (source / "Build/iPhoneWebTest.data").read_bytes()).hexdigest()[:16]
manifest = {"version": 1}
for kind, suffix, mime in (("data", ".data", "application/octet-stream"), ("wasm", ".wasm", "application/wasm")):
    raw = source / ("Build/iPhoneWebTest" + suffix)
    packed = target / ("Build/iPhoneWebTest" + suffix + ".gz")
    digest = hashlib.sha256(raw.read_bytes()).hexdigest()[:16]
    manifest[kind] = {"url": "Build/" + packed.name + "?v=" + digest, "bytes": packed.stat().st_size, "decodedBytes": raw.stat().st_size, "type": mime}
for kind in ("framework", "loader"):
    name = "Build/iPhoneWebTest." + kind + ".js"
    digest = hashlib.sha256((target / name).read_bytes()).hexdigest()[:16]
    manifest[kind] = name + "?v=" + digest
(target / "game-assets.json").write_text(json.dumps(manifest, ensure_ascii=False, indent=2), encoding="utf-8")
index = target / "index.html"
html = index.read_text(encoding="utf-8")
html = re.sub(r"(Build/iPhoneWebTest\.(?:loader\.js|framework\.js|data\.gz|wasm\.gz))(?:\?v=[a-f0-9]+)?", lambda m: m.group(1) + "?v=" + revision, html)
index.write_text(html, encoding="utf-8")
print("Hosted assets updated; loader retained and asset cache revision: " + revision)
write_catalog(repo,site)

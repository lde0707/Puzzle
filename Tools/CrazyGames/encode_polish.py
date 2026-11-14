"""Encode actual Unity-rendered frames; never interpolate a low-FPS recording."""
from pathlib import Path
import subprocess, json, shutil, argparse

parser=argparse.ArgumentParser()
parser.add_argument('--version',default='1.4.0')
parser.add_argument('--date',default='2026-11-04')
args=parser.parse_args()

root=Path(__file__).resolve().parents[2]
release=root/f'Releases/crazygames/{args.date}-v{args.version}-sdk'
assets=release/'store-assets'
assets.mkdir(parents=True,exist_ok=True)
previous=root/'Tools/CrazyGames/store-art'
for name in ('cover-landscape.jpg','cover-portrait.jpg','cover-square.jpg','image-prompts.json'):
    shutil.copy2(previous/name,assets/name)
ffmpeg=root/'Tools/CrazyGames/.video-deps/imageio_ffmpeg/binaries/ffmpeg-win-x86_64-v9.0.2.exe'
report={}
for shape,dimensions in [('landscape',[1920,1080]),('portrait',[1080,1620])]:
    frames=release/'qa'/('frames-'+shape)
    assert len(list(frames.glob('frame-*.png')))==540
    out=assets/('preview-'+shape+'.mp4')
    subprocess.run([str(ffmpeg),'-hide_banner','-loglevel','error','-y','-framerate','30','-i',str(frames/'frame-%04d.png'),'-c:v','libx264','-preset','medium','-crf','18','-pix_fmt','yuv420p','-an','-movflags','+faststart',str(out)],check=True)
    subprocess.run([str(ffmpeg),'-hide_banner','-loglevel','error','-i',str(out),'-f','null','-'],check=True)
    report[shape]=dict(file=out.name,dimensions=dimensions,fps=30,frames=540,seconds=18,bytes=out.stat().st_size,source='Actual Unity UI and PuzzleSession simulation at fixed 1/30 s, isolated save, no frame interpolation')
    print(shape,out.stat().st_size,flush=True)
(assets/'video-checks.json').write_text(json.dumps(report,indent=2),encoding='utf-8')

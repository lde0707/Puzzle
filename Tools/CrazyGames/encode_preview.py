"""Encode captured CUA frames at their original timing into silent portal previews."""
import json
import subprocess
from pathlib import Path

root = Path(__file__).resolve().parents[2]
assets = root / 'Releases/crazygames/2026-11-02-v1.2.0-sdk/store-assets'
frames = assets / 'frames-release'
ffmpeg = root / 'Tools/CrazyGames/.video-deps/imageio_ffmpeg/binaries/ffmpeg-win-x86_64-v9.0.2.exe'
timing = json.loads((frames / 'timing.json').read_text())
lines = ['ffconcat version 1.0']
for i, item in enumerate(timing):
    end = timing[i + 1]['time'] if i + 1 < len(timing) else 17.15
    lines += [f"file '{item['file']}'", f"duration {max(.02, end-item['time']):.6f}"]
lines += [f"file '{timing[-1]['file']}'"]
(frames / 'frames.ffconcat').write_text('\n'.join(lines), encoding='utf-8')

def run(args):
    subprocess.run([str(ffmpeg), '-hide_banner', '-loglevel', 'error', '-y', *args], check=True)

for shape, size, crop in [('landscape', '1920:1080', '1840:1035:40:0:exact=1'),
                          ('portrait', '1080:1620', '690:1035:615:0:exact=1')]:
    common = ['-c:v', 'libx264', '-preset', 'medium', '-crf', '20', '-pix_fmt', 'yuv420p', '-an', '-movflags', '+faststart']
    intro = assets / f'intro-{shape}.mp4'
    play = assets / f'gameplay-{shape}.mp4'
    run(['-loop', '1', '-i', str(assets / f'cover-{shape}.jpg'), '-vf', f'scale={size},setsar=1,fps=30', '-t', '1', *common, str(intro)])
    run(['-f', 'concat', '-safe', '0', '-i', str(frames / 'frames.ffconcat'), '-vf', f'crop={crop},scale={size}:flags=lanczos,setsar=1,fps=30', '-t', '17.15', *common, str(play)])
    sequence = assets / f'{shape}.ffconcat'
    sequence.write_text(f"file '{intro.name}'\nfile '{play.name}'\n", encoding='utf-8')
    run(['-f', 'concat', '-safe', '0', '-i', str(sequence), '-c', 'copy', '-movflags', '+faststart', str(assets / f'preview-{shape}.mp4')])
    print(shape, (assets / f'preview-{shape}.mp4').stat().st_size, flush=True)

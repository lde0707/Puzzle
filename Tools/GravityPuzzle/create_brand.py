"""Draw the game's code-native brand with the bundled OFL font and UI palette.

Requires Pillow. Produces a transparent wordmark PNG, transparent icon PNG,
and editable SVG icon. No network assets or font installation required.
"""
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont, ImageFilter

ROOT = Path(__file__).resolve().parents[2]
ASSETS = ROOT / 'Android Puzzle/Assets'
BRAND = ASSETS / 'Resources/GravityPuzzle/Brand'
WEB = ASSETS / 'WebGLTemplates/MobileTest'
INK = '#3d2b5e'
LILAC = '#c7b8e8'
COLORS = ('#fca17d', '#66d1d9', '#ffd163')
SCALE = 4


def icon():
    im = Image.new('RGBA', (160*SCALE, 160*SCALE))
    orbit = Image.new('RGBA', im.size)
    d = ImageDraw.Draw(orbit)
    d.ellipse(tuple(v*SCALE for v in (7, 43, 153, 117)), outline=LILAC, width=4*SCALE)
    im.alpha_composite(orbit.rotate(32, resample=Image.Resampling.BICUBIC))
    d = ImageDraw.Draw(im)
    def line(points, fill, width):
        d.line([(x*SCALE, y*SCALE) for x,y in points], fill=fill, width=width*SCALE, joint='curve')
    def friend(x, y, size, color):
        box = tuple(round(v*SCALE) for v in (x,y,x+size,y+size))
        d.rounded_rectangle(box, radius=round(size*.28*SCALE), fill=color)
        d.rounded_rectangle(tuple(round(v*SCALE) for v in (x+size*.12,y+size*.09,x+size*.68,y+size*.17)), radius=2*SCALE, fill='#ffffff88')
        for ex in (.34,.67):
            px,py=x+size*ex,y+size*.39
            d.rounded_rectangle(tuple(round(v*SCALE) for v in (px-1,py,px+1,py+size*.115)), radius=SCALE, fill=INK)
        d.arc(tuple(round(v*SCALE) for v in (x+size*.36,y+size*.43,x+size*.67,y+size*.72)), 15,165, fill=INK,width=2*SCALE)
    friend(51,53,62,COLORS[0])
    friend(112,20,34,COLORS[1])
    friend(9,108,36,COLORS[2])
    for x,y,size in [(53,21,6),(129,126,5)]:
        d.polygon([(x*SCALE,(y-size)*SCALE),((x+2)*SCALE,(y-2)*SCALE),((x+size)*SCALE,y*SCALE),((x+2)*SCALE,(y+2)*SCALE),(x*SCALE,(y+size)*SCALE),((x-2)*SCALE,(y+2)*SCALE),((x-size)*SCALE,y*SCALE),((x-2)*SCALE,(y-2)*SCALE)], fill=LILAC)
    return im


def main():
    BRAND.mkdir(parents=True, exist_ok=True)
    emblem = icon()
    logo = Image.new('RGBA',(660*SCALE,160*SCALE))
    logo.alpha_composite(emblem)
    font=ImageFont.truetype(str(ASSETS/'Resources/GravityPuzzle/NanumGothic-Regular.ttf'),112*SCALE)
    # Plump individual letters, rounded corners and gently uneven baselines.
    # All geometry is regenerated from the licensed font, not an edited bitmap.
    for i, (letter, angle, lift) in enumerate(zip('말랑궤도', (2,-2,1,-1), (0,-3,1,-2))):
        mask=Image.new('L',(132*SCALE,156*SCALE))
        ImageDraw.Draw(mask).text((10*SCALE,8*SCALE),letter,font=font,fill=255,stroke_width=5*SCALE)
        mask=mask.filter(ImageFilter.GaussianBlur(1.25*SCALE)).point(lambda p: 255 if p>=128 else 0)
        mask=mask.rotate(angle,Image.Resampling.BICUBIC)
        x,y=(174+i*115)*SCALE,(7+lift)*SCALE
        shade=Image.new('RGBA',mask.size,'#c7b8e8');shade.putalpha(mask)
        face=Image.new('RGBA',mask.size,INK);face.putalpha(mask)
        logo.alpha_composite(shade,(x,y+4*SCALE))
        logo.alpha_composite(face,(x,y))
    logo=logo.resize((1320,320),Image.Resampling.LANCZOS)
    emblem=emblem.resize((320,320),Image.Resampling.LANCZOS)
    logo.save(BRAND/'mallang-orbit-logo.png')
    emblem.save(BRAND/'mallang-orbit-icon.png')
    logo.save(WEB/'mallang-orbit-logo.png')
    emblem.save(WEB/'mallang-orbit-icon.png')
    # Lightweight scalable favicon; rounded friends and the same orbit motif.
    svg='''<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 160 160" role="img" aria-label="말랑궤도 세 친구 심볼">
<ellipse cx="80" cy="80" rx="73" ry="37" transform="rotate(-32 80 80)" fill="none" stroke="#c7b8e8" stroke-width="4"/>
<g fill="#c7b8e8"><path d="m53 15 2 4 4 2-4 2-2 4-2-4-4-2 4-2z"/><path d="m129 121 2 3 3 2-3 2-2 3-2-3-3-2 3-2z"/></g>
<rect x="51" y="53" width="62" height="62" rx="17" fill="#fca17d"/>
<rect x="112" y="20" width="34" height="34" rx="10" fill="#66d1d9"/>
<rect x="9" y="108" width="36" height="36" rx="10" fill="#ffd163"/>
<g fill="none" stroke="#3d2b5e" stroke-width="2" stroke-linecap="round"><path d="M72 77v7m21-7v7m-19 6q9 10 17 0M124 33v4m11-4v4m-10 5q5 5 10 0M21 122v4m12-4v4m-11 5q5 5 10 0"/></g>
</svg>'''
    (BRAND/'mallang-orbit-icon.svg').write_text(svg,encoding='utf-8')
    (WEB/'mallang-orbit-icon.svg').write_text(svg,encoding='utf-8')
    print('Brand assets generated:',BRAND)


if __name__=='__main__':
    main()

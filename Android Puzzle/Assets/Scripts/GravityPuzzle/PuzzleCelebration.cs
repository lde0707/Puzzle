using UnityEngine;

namespace Puzzle.Gravity
{
    // A fixed pool of UI sparkles. No objects are created during animation.
    public sealed class PuzzleCelebration
    {
        private readonly RectTransform[] pieces=new RectTransform[30];
        private readonly PuzzleGlyph[] glyphs=new PuzzleGlyph[30];
        private readonly RectTransform root;
        private float elapsed=10;
        public PuzzleCelebration(RectTransform parent)
        {
            root=PuzzleUI.Rect("Sugar stardust",parent);PuzzleUI.Fill(root);
            for(int i=0;i<pieces.Length;i++)
            {
                var glyph=PuzzleUI.Glyph(root,i%3==0?GlyphKind.Ring:GlyphKind.Star,PuzzleUI.Candy[i%3]);
                pieces[i]=glyph.rectTransform;glyphs[i]=glyph;glyph.gameObject.SetActive(false);
            }
        }
        public void Play(bool reduced) { elapsed=reduced?10:0;Tick(0,reduced); }
        public void Tick(float delta,bool reduced)
        {
            elapsed+=delta;
            for(int i=0;i<pieces.Length;i++)
            {
                float t=elapsed-(i%4)*.04f;bool show=!reduced&&t>=0&&t<1.6f;
                pieces[i].gameObject.SetActive(show);if(!show)continue;
                float angle=(i*137.5f+20)*Mathf.Deg2Rad;
                float speed=65+(i%5)*19;
                float x=root.rect.width/2+Mathf.Cos(angle)*speed*t;
                float y=root.rect.height*.62f+Mathf.Sin(angle)*speed*t+70*t-90*t*t;
                float size=7+i%3*3;PuzzleUI.Box(pieces[i],x,y,size,size);
                pieces[i].localRotation=Quaternion.Euler(0,0,i*23+t*(i%2==0?90:-90));
                Color c=i%3==0?new Color(1,.82f,.56f):i%3==1?new Color(.84f,.73f,1):new Color(1,.86f,.82f);c.a=Mathf.Clamp01((1.6f-t)/.55f);glyphs[i].color=c;
            }
        }
    }
}

using System;
using UnityEngine;

namespace Puzzle.Gravity
{
    // The modal owns this small fixed pool; leaving the result cancels everything.
    public sealed class CompletionStars
    {
        private readonly RectTransform[] stars = new RectTransform[3];
        private readonly CanvasGroup[] opacity = new CanvasGroup[3];
        private readonly PuzzleGlyph[,] sparks = new PuzzleGlyph[3, 12];
        private readonly PuzzleGlyph[] rings = new PuzzleGlyph[3];
        private readonly CompletionStarGlow[] glows = new CompletionStarGlow[3];
        private readonly PuzzleGlyph[] glints = new PuzzleGlyph[3];
        private readonly Vector2[] resting = new Vector2[3];
        private readonly bool[] sounded = new bool[3];
        private readonly int rating;
        public int Rating=>rating;
        private readonly Action<int> chime;
        private readonly Action finaleSound;
        private float finaleAt=float.PositiveInfinity;
        private bool finalePlayed;
        private float elapsed;
        public RectTransform Star(int index)=>stars[index];
        public static float RevealAt(int index)=>.18f+index*.46f;
        public static float RevealDuration(int rating)=>RevealAt(rating-1)+.72f;
        public void FinishWhenReady()
        {
            if(float.IsPositiveInfinity(finaleAt))finaleAt=Mathf.Max(elapsed+.12f,RevealAt(rating-1)+.40f);
        }
        public CompletionStars(RectTransform parent, int rating, bool instant, Action<int> chime,Action finaleSound=null)
        {
            this.rating = rating; this.chime = chime;this.finaleSound=finaleSound;
            var row = PuzzleUI.Rect("Soft earned stars", parent);
            PuzzleUI.Box(row, parent.sizeDelta.x / 2 - 156, 339, 312, 118);
            row.pivot=new Vector2(.5f,.5f);row.anchoredPosition=new Vector2(parent.sizeDelta.x/2,398);
            row.localScale=Vector3.one*Mathf.Min(1,(parent.sizeDelta.x-28)/312);
            for (int i = 0; i < 3; i++)
            {
                float x = 56 + i * 100, y = i == 1 ? 63 : 55;
                resting[i]=new Vector2(x,y);
                var halo=PuzzleUI.Rect("Creamy star bloom "+i,row);
                PuzzleUI.Box(halo,x-72,y-72,144,144);
                glows[i]=halo.gameObject.AddComponent<CompletionStarGlow>();glows[i].raycastTarget=false;
                rings[i] = PuzzleUI.Glyph(row, GlyphKind.Ring, new Color(1, .77f, .36f));
                var r = PuzzleUI.Rect("Reward star " + (i + 1), row); stars[i] = r;
                float size = i == 1 ? 122 : 108;
                PuzzleUI.Box(r, x, y, size, size); r.pivot = new Vector2(.5f, .5f);
                var graphic = r.gameObject.AddComponent<PuffyStar>(); graphic.earned = i < rating; graphic.painted=true;graphic.raycastTarget = false;
                opacity[i] = r.gameObject.AddComponent<CanvasGroup>(); opacity[i].blocksRaycasts = false;
                for (int j = 0; j < 12; j++) sparks[i, j] = PuzzleUI.Glyph(row, j % 3 == 0 ? GlyphKind.Ring : GlyphKind.Star, PuzzleUI.Candy[j % 3]);
                glints[i]=PuzzleUI.Glyph(row,GlyphKind.Star,Color.white);
            }
            Tick(0, instant);
        }
        public void Tick(float delta, bool instant)
        {
            elapsed += delta;
            bool finaleReady=!float.IsPositiveInfinity(finaleAt)&&(instant||elapsed>=finaleAt);
            if(finaleReady&&!finalePlayed){finalePlayed=true;finaleSound?.Invoke();}
            float finaleAge=elapsed-finaleAt;
            bool finale=finaleReady&&!instant&&finaleAge>=0&&finaleAge<.72f;
            for (int i = 0; i < 3; i++)
            {
                float t = elapsed - RevealAt(i);
                bool earned = i < rating, visible = !earned || instant || t >= 0;
                opacity[i].alpha = visible ? 1 : 0;
                float scale = 1, squash = 0;
                if (earned && !instant && t >= 0 && t < .72f)
                {
                    scale = t < .18f ? Mathf.Lerp(.12f, 1.20f, 1 - Mathf.Pow(1 - t / .18f, 3)) : 1 + .20f * Mathf.Cos((t - .18f) * 20) * Mathf.Exp(-(t - .18f) * 8);
                    squash = Mathf.Sin(t * 23) * .17f * Mathf.Exp(-t * 4);
                }
                if(finale)
                {
                    float swell=Mathf.Sin(Mathf.Clamp01(finaleAge/.72f)*Mathf.PI);
                    scale=1+(earned?.16f:.055f)*swell;
                    squash=.035f*Mathf.Sin(finaleAge*18)*swell;
                }
                stars[i].localScale = new Vector3(scale * (1 + squash), scale * (1 - squash), 1);
                float lift=earned&&!instant&&t>=0&&t<.72f?-24*Mathf.Exp(-t*8)*Mathf.Cos(t*13):0;
                if(finale)lift+=4*Mathf.Sin(Mathf.Clamp01(finaleAge/.72f)*Mathf.PI);
                stars[i].anchoredPosition=resting[i]+Vector2.up*lift;
                stars[i].localRotation = Quaternion.Euler(0, 0, earned && !instant && t >= 0 ? Mathf.Sin(t * 18) * 9 * Mathf.Exp(-t * 6) : 0);
                if (earned && !sounded[i] && (instant || t >= 0))
                { sounded[i] = true; if (!instant) chime?.Invoke(i); }
                float burst = finale?finaleAge-.06f:t-.12f;
                bool show = earned && !instant && burst >= 0 && burst < .65f;
                var center = stars[i].anchoredPosition;
                float bloom=earned&&visible?(instant?.15f:.16f+.68f*Mathf.Clamp01(1-Mathf.Abs(t-.20f)/.55f)):0;
                if(finale&&earned)bloom+=.65f*Mathf.Sin(Mathf.Clamp01(finaleAge/.72f)*Mathf.PI);
                glows[i].Pose(bloom);
                bool shine=earned&&!instant&&(finale||t>=.13f&&t<.70f);
                glints[i].gameObject.SetActive(shine);
                if(shine)
                {
                    float p=finale?Mathf.Clamp01(finaleAge/.72f):Mathf.Clamp01((t-.13f)/.57f);
                    float s=19*Mathf.Sin(p*Mathf.PI);
                    PuzzleUI.Box(glints[i].rectTransform,center.x-27+p*42-s/2,center.y+25-s/2,s,s);
                    glints[i].color=new Color(1,1,.96f,Mathf.Sin(p*Mathf.PI));
                }
                rings[i].gameObject.SetActive(show);
                if (show)
                {
                    float diameter = 68 + burst * 100;
                    PuzzleUI.Box(rings[i].rectTransform, center.x - diameter / 2, center.y - diameter / 2, diameter, diameter);
                    rings[i].color = new Color(1, .84f, .61f, .38f * (1 - burst / .65f));
                }
                for (int j = 0; j < 12; j++)
                {
                    var spark = sparks[i, j]; spark.gameObject.SetActive(show); if (!show) continue;
                    float angle = (j * 30 + i * 17) * Mathf.Deg2Rad, radius = 35 + burst * 65;
                    float size = (j % 3 == 0 ? 8 : 12) * (1 - burst / .8f);
                    Vector2 p = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                    p.y+=8*Mathf.Sin(burst*Mathf.PI)-18*burst*burst;
                    PuzzleUI.Box(spark.rectTransform, p.x - size / 2, p.y - size / 2, size, size);
                    Color c = j % 3 == 0 ? new Color(.84f,.74f,1) : j%3==1?new Color(1,.82f,.52f):new Color(1,.91f,.81f); c.a = 1 - burst / .65f; spark.color = c;
                }
            }
        }
    }
}

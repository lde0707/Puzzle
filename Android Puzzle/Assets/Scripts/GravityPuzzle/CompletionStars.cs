using System;
using UnityEngine;

namespace Puzzle.Gravity
{
    // The modal owns this small fixed pool; leaving the result cancels everything.
    public sealed class CompletionStars
    {
        private readonly RectTransform[] stars = new RectTransform[3];
        private readonly CanvasGroup[] opacity = new CanvasGroup[3];
        private readonly PuzzleGlyph[,] sparks = new PuzzleGlyph[3, 8];
        private readonly PuzzleGlyph[] rings = new PuzzleGlyph[3];
        private readonly bool[] sounded = new bool[3];
        private readonly int rating;
        private readonly Action<int> chime;
        private readonly Action finaleSound;
        private float finaleAt=float.PositiveInfinity;
        private bool finalePlayed;
        private float elapsed;
        public RectTransform Star(int index)=>stars[index];
        public static float RevealDuration(int rating)=>.18f+(rating-1)*.34f+.72f;
        public void FinishWhenReady()
        {
            if(float.IsPositiveInfinity(finaleAt))finaleAt=Mathf.Max(elapsed+.16f,RevealDuration(rating)+.25f);
        }
        public CompletionStars(RectTransform parent, int rating, bool instant, Action<int> chime,Action finaleSound=null)
        {
            this.rating = rating; this.chime = chime;this.finaleSound=finaleSound;
            var row = PuzzleUI.Rect("Soft earned stars", parent);
            PuzzleUI.Box(row, parent.sizeDelta.x / 2 - 150, 339, 300, 118);
            for (int i = 0; i < 3; i++)
            {
                float x = 56 + i * 94, y = i == 1 ? 63 : 55;
                rings[i] = PuzzleUI.Glyph(row, GlyphKind.Ring, new Color(1, .77f, .36f));
                var r = PuzzleUI.Rect("Reward star " + (i + 1), row); stars[i] = r;
                float size = i == 1 ? 112 : 100;
                PuzzleUI.Box(r, x, y, size, size); r.pivot = new Vector2(.5f, .5f);
                var graphic = r.gameObject.AddComponent<PuffyStar>(); graphic.earned = i < rating; graphic.raycastTarget = false;
                opacity[i] = r.gameObject.AddComponent<CanvasGroup>(); opacity[i].blocksRaycasts = false;
                for (int j = 0; j < 8; j++) sparks[i, j] = PuzzleUI.Glyph(row, j % 3 == 0 ? GlyphKind.Ring : GlyphKind.Star, PuzzleUI.Candy[j % 3]);
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
                float t = elapsed - .18f - i * .34f;
                bool earned = i < rating, visible = !earned || instant || t >= 0;
                opacity[i].alpha = visible ? 1 : 0;
                float scale = 1, squash = 0;
                if (earned && !instant && t >= 0 && t < .72f)
                {
                    scale = t < .18f ? Mathf.Lerp(.12f, 1.23f, 1 - Mathf.Pow(1 - t / .18f, 3)) : 1 + .23f * Mathf.Cos((t - .18f) * 22) * Mathf.Exp(-(t - .18f) * 7);
                    squash = Mathf.Sin(t * 23) * .11f * Mathf.Exp(-t * 4);
                }
                if(finale)
                {
                    float swell=Mathf.Sin(Mathf.Clamp01(finaleAge/.72f)*Mathf.PI);
                    scale=1+(earned?.16f:.055f)*swell;
                    squash=.035f*Mathf.Sin(finaleAge*18)*swell;
                }
                stars[i].localScale = new Vector3(scale * (1 + squash), scale * (1 - squash), 1);
                stars[i].localRotation = Quaternion.Euler(0, 0, earned && !instant && t >= 0 ? Mathf.Sin(t * 18) * 9 * Mathf.Exp(-t * 6) : 0);
                if (earned && !sounded[i] && (instant || t >= 0))
                { sounded[i] = true; if (!instant) chime?.Invoke(i); }
                float burst = finale?finaleAge-.06f:t-.12f;
                bool show = earned && !instant && burst >= 0 && burst < .65f;
                var center = stars[i].anchoredPosition;
                rings[i].gameObject.SetActive(show);
                if (show)
                {
                    float diameter = 68 + burst * 100;
                    PuzzleUI.Box(rings[i].rectTransform, center.x - diameter / 2, center.y - diameter / 2, diameter, diameter);
                    rings[i].color = new Color(1, .74f, .29f, .45f * (1 - burst / .65f));
                }
                for (int j = 0; j < 8; j++)
                {
                    var spark = sparks[i, j]; spark.gameObject.SetActive(show); if (!show) continue;
                    float angle = (j * 45 + i * 17) * Mathf.Deg2Rad, radius = 34 + burst * 59;
                    float size = (j % 3 == 0 ? 8 : 12) * (1 - burst / .8f);
                    Vector2 p = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                    PuzzleUI.Box(spark.rectTransform, p.x - size / 2, p.y - size / 2, size, size);
                    Color c = j % 2 == 0 ? new Color(1, .74f, .29f) : PuzzleUI.Candy[j % 3]; c.a = 1 - burst / .65f; spark.color = c;
                }
            }
        }
    }
}

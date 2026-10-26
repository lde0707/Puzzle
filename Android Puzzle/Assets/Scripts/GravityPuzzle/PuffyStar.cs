using UnityEngine;
using UnityEngine.UI;

namespace Puzzle.Gravity
{
    // Rounded vector outline, a thick underside and a softly lit domed face.
    public sealed class PuffyStar : MaskableGraphic
    {
        public bool earned = true;
        private static readonly Vector2[] Outline = MakeOutline();
        private static Vector2[] MakeOutline()
        {
            var corners = new Vector2[10];
            for (int i = 0; i < 10; i++)
            {
                float a = (90 + i * 36) * Mathf.Deg2Rad, r = i % 2 == 0 ? .46f : .255f;
                corners[i] = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r;
            }
            var points = new Vector2[80];
            for (int i = 0; i < 10; i++)
            {
                Vector2 p = corners[i], a = Vector2.Lerp(p, corners[(i + 9) % 10], .23f), b = Vector2.Lerp(p, corners[(i + 1) % 10], .23f);
                for (int j = 0; j < 8; j++)
                {
                    float t = j / 7f;
                    points[i * 8 + j] = (1 - t) * (1 - t) * a + 2 * (1 - t) * t * p + t * t * b;
                }
            }
            return points;
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear(); var rect = rectTransform.rect;
            Vector2 center = new Vector2(.5f, .52f);
            void Vertex(Vector2 p, Color c) => vh.AddVert(new Vector3(rect.xMin + p.x * rect.width, rect.yMin + p.y * rect.height), c, Vector2.zero);
            void Tri(Vector2 a, Vector2 b, Vector2 c, Color ca, Color cb, Color cc)
            { int n = vh.currentVertCount; Vertex(a, ca); Vertex(b, cb); Vertex(c, cc); vh.AddTriangle(n, n + 1, n + 2); }
            void Flat(float scale, Vector2 shift, Color c)
            {
                for (int i = 0; i < Outline.Length; i++)
                    Tri(center + shift, center + shift + Outline[i] * scale, center + shift + Outline[(i + 1) % Outline.Length] * scale, c, c, c);
            }
            Flat(1.04f, new Vector2(0, -.055f), earned ? new Color(.55f, .32f, .19f, .12f) : new Color(.38f, .30f, .50f, .08f));
            Flat(1, new Vector2(0, -.033f), earned ? new Color(.85f, .48f, .16f) : new Color(.72f, .67f, .79f));
            Color core = earned ? new Color(1, .91f, .57f) : new Color(.91f, .88f, .95f);
            for (int i = 0; i < Outline.Length; i++)
            {
                Vector2 a = Outline[i], b = Outline[(i + 1) % Outline.Length];
                Color Edge(Vector2 p) => earned
                    ? Color.Lerp(new Color(.96f, .61f, .20f), new Color(1, .96f, .75f), Mathf.Clamp01(.45f + p.y - p.x * .6f))
                    : new Color(.82f, .78f, .88f);
                Color ea = Edge(a), eb = Edge(b), ma = Color.Lerp(core, ea, .18f), mb = Color.Lerp(core, eb, .18f);
                Tri(center, center + a * .74f, center + b * .74f, core, ma, mb);
                Tri(center + a * .74f, center + a, center + b, ma, ea, eb);
                Tri(center + a * .74f, center + b, center + b * .74f, ma, eb, mb);
            }
            if (!earned) return;
            var m = new SkinMesh(vh, rect);
            m.Line(.465f, .78f, .485f, .84f, .035f, new Color(1, 1, .91f, .85f));
            m.Oval(.38f, .65f, .055f, .028f, new Color(1, 1, .92f, .65f));
            m.Oval(.63f, .37f, .035f, .018f, new Color(1, .96f, .72f, .55f));
        }
    }
}

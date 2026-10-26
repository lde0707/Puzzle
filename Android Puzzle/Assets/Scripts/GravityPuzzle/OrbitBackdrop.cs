using UnityEngine;
using UnityEngine.UI;

namespace Puzzle.Gravity
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class OrbitBackdrop : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var m=new SkinMesh(vh,rectTransform.rect);
            m.Poly(new Color(.985f,.970f,.995f),Vector2.zero,Vector2.right,Vector2.one,Vector2.up);
            // Translucent, layered pools keep the stage calm behind small puzzle pieces.
            for(int i=12;i>0;i--)
            {
                float r=i/12f;
                m.Oval(.16f,.77f,.40f*r,.26f*r,new Color(.81f,.72f,.95f,.025f));
                m.Oval(.88f,.27f,.39f*r,.32f*r,new Color(1,.74f,.60f,.022f));
            }
            for(int i=0;i<22;i++)
            {
                float x=.05f+Mathf.Repeat(i*.618034f,.90f),y=.07f+Mathf.Repeat(i*.371f,.86f);
                float r=i%3==0?.004f:.0016f;
                if(i%3==0)m.Star(x,y,r,new Color(.69f,.58f,.82f,.22f),45,4,.25f);
                else m.Oval(x,y,r,r,new Color(.69f,.58f,.82f,.17f));
            }
        }
        public static void Add(Transform parent)
        {
            var rect=PuzzleUI.Rect("Pearl sky",parent);PuzzleUI.Fill(rect);
            rect.gameObject.AddComponent<OrbitBackdrop>().raycastTarget=false;
        }
    }
}

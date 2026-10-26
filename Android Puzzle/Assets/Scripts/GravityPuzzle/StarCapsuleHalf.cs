using UnityEngine;
using UnityEngine.UI;

namespace Puzzle.Gravity
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class StarCapsuleHalf : MaskableGraphic
    {
        public bool top;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var m=new SkinMesh(vh,rectTransform.rect);
            Color shade=top?new Color(.72f,.58f,.91f):new Color(.97f,.65f,.50f);
            Color fill=top?new Color(.89f,.80f,1):new Color(1,.80f,.63f);
            for(int i=0;i<32;i++)
            {
                float a=(i/32f*Mathf.PI)+(top?0:Mathf.PI),b=((i+1)/32f*Mathf.PI)+(top?0:Mathf.PI);
                var center=new Vector2(.5f,.5f);
                m.Triangle(center,center+new Vector2(Mathf.Cos(a)*.46f,Mathf.Sin(a)*.46f),center+new Vector2(Mathf.Cos(b)*.46f,Mathf.Sin(b)*.46f),shade);
                m.Triangle(center,center+new Vector2(Mathf.Cos(a)*.425f,Mathf.Sin(a)*.425f),center+new Vector2(Mathf.Cos(b)*.425f,Mathf.Sin(b)*.425f),fill);
            }
            m.Line(.065f,.5f,.935f,.5f,.025f,new Color(1,.96f,.82f));
            if(top)
            {
                m.Arc(.5f,.51f,.32f,.32f,70,140,.055f,new Color(1,1,1,.75f));
                m.Star(.51f,.69f,.11f,new Color(1,.94f,.64f));
            }
            else
            {
                m.Arc(.5f,.49f,.33f,.33f,220,300,.024f,new Color(.90f,.53f,.42f,.5f));
                m.Round(.35f,.34f,.3f,.08f,.035f,new Color(1,.93f,.79f));
            }
        }
    }
}

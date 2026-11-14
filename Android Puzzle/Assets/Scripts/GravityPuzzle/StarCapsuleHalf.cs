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
            vh.Clear();var m=new SkinMesh(vh,rectTransform.rect,true);
            Color shade=top?new Color(.72f,.58f,.91f):new Color(.97f,.65f,.50f);
            Color fill=top?new Color(.89f,.80f,1):new Color(1,.80f,.63f);
            for(int i=0;i<64;i++)
            {
                float a=(i/64f*Mathf.PI)+(top?0:Mathf.PI),b=((i+1)/64f*Mathf.PI)+(top?0:Mathf.PI);
                var center=new Vector2(.5f,.5f);
                m.Triangle(center,center+new Vector2(Mathf.Cos(a)*.46f,Mathf.Sin(a)*.46f),center+new Vector2(Mathf.Cos(b)*.46f,Mathf.Sin(b)*.46f),shade);
                m.Triangle(center,center+new Vector2(Mathf.Cos(a)*.425f,Mathf.Sin(a)*.425f),center+new Vector2(Mathf.Cos(b)*.425f,Mathf.Sin(b)*.425f),fill);
            }
            m.Line(.065f,.5f,.935f,.5f,.040f,new Color(.77f,.52f,.67f));
            m.Line(.065f,.512f,.935f,.512f,.023f,new Color(1,.96f,.82f));
            if(top)
            {
                m.Arc(.5f,.51f,.32f,.32f,70,140,.055f,new Color(1,1,1,.75f));
                m.Oval(.75f,.67f,.028f,.045f,new Color(1,1,1,.65f));
                m.Oval(.51f,.68f,.145f,.145f,new Color(.73f,.54f,.86f));
                m.Oval(.51f,.69f,.126f,.126f,new Color(1,.88f,.61f));
                m.Star(.51f,.70f,.097f,new Color(1,.98f,.83f));
            }
            else
            {
                m.Arc(.5f,.49f,.33f,.33f,220,300,.024f,new Color(.90f,.53f,.42f,.5f));
                m.Oval(.32f,.32f,.055f,.027f,new Color(1,.55f,.60f,.65f));
                m.Oval(.68f,.32f,.055f,.027f,new Color(1,.55f,.60f,.65f));
                m.Oval(.40f,.36f,.018f,.026f,PuzzleUI.Ink);m.Oval(.60f,.36f,.018f,.026f,PuzzleUI.Ink);
                m.Arc(.50f,.32f,.045f,.035f,190,350,.012f,PuzzleUI.Ink);
                m.Arc(.5f,.49f,.35f,.35f,190,220,.026f,new Color(1,1,1,.6f));
            }
        }
    }
}

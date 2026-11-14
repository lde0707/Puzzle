using UnityEngine;
using UnityEngine.UI;

namespace Puzzle.Gravity
{
    // Resolution-independent candy stage; all particles stay inside the illustration area.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class DrawStageArt : MaskableGraphic
    {
        public float age;
        public bool still,preview;
        public void Pose(float time,bool reduced){age=time;still=reduced;SetVerticesDirty();}
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var m=new SkinMesh(vh,rectTransform.rect,true);
            float reveal=preview?0:Mathf.Clamp01((age-1.7f)/.8f);
            for(int i=12;i>=0;i--)
                m.Oval(.5f,.54f,.19f+i*.022f,.19f+i*.023f,new Color(1,.94f,.76f,.025f+reveal*.012f));
            for(int i=0;i<16;i++)
            {
                float a=i*Mathf.PI/8+(still?0:age*.12f);
                Vector2 center=new Vector2(.5f,.54f);
                Vector2 P(float angle,float r)=>center+new Vector2(Mathf.Cos(angle)*r,Mathf.Sin(angle)*r);
                m.Triangle(P(a,.10f),P(a-.035f,.46f),P(a+.035f,.46f),new Color(1,.91f,.60f,.07f+reveal*.24f));
            }
            m.Oval(.5f,.15f,.34f,.047f,new Color(.58f,.40f,.77f,.13f));
            m.Round(.15f,.13f,.70f,.08f,.04f,new Color(.78f,.66f,.92f));
            m.Oval(.5f,.21f,.35f,.074f,new Color(.96f,.90f,1));
            m.Arc(.5f,.21f,.29f,.05f,185,350,.008f,new Color(.76f,.66f,.86f,.45f));
            for(int side=0;side<2;side++)
            {
                float x=side==0?.06f:.94f;
                m.Oval(x,.12f,.055f,.045f,new Color(1,.95f,.97f));
                m.Oval(x+(side==0?.05f:-.05f),.10f,.06f,.035f,new Color(1,.95f,.97f));
            }
            for(int i=0;i<14;i++)
            {
                float a=i*2.39996f;
                float x=.5f+Mathf.Cos(a)*(.37f+(i%2)*.05f),y=.53f+Mathf.Sin(a)*.37f;
                float twinkle=still?1:.75f+.25f*Mathf.Sin(age*2.1f+i*1.8f);
                m.Star(x,y,(i%3==0?.022f:.012f)*twinkle,new Color(1,.83f,.45f,.7f),90,4,.23f);
            }
            if(preview||still||age<1.8f||age>4.5f)return;
            for(int i=0;i<54;i++)
            {
                float p=Mathf.Clamp01((age-1.8f-(i%6)*.035f)/2.3f);
                if(p<=0||p>=1)continue;
                float a=i*2.39996f;
                float x=.5f+Mathf.Cos(a)*(.23f+(i%5)*.035f)*(1-Mathf.Pow(1-p,3));
                float y=.54f+Mathf.Sin(a)*.26f*p+.40f*p-.65f*p*p;
                var c=PuzzleUI.Candy[i%3];c.a=Mathf.Min(1,(1-p)*3);
                float size=.009f+(i%3)*.003f;
                if(i%3==0)m.Star(x,y,size*1.5f,c,age*90+i*17,5);
                else
                {
                    float spin=age*5+i;float dx=Mathf.Cos(spin)*size,dy=Mathf.Sin(spin)*size;
                    m.Line(x-dx,y-dy,x+dx,y+dy,.009f,c);
                }
            }
        }
    }
}

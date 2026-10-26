using UnityEngine;
using UnityEngine.UI;

namespace Puzzle.Gravity
{
    // A smooth transparent halo and a small cream glint, built once then animated by opacity.
    public sealed class TitleGlint : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var r=rectTransform.rect;
            Vector2 center=r.center;
            Color warm=new Color(1,.78f,.38f,.24f),clear=new Color(1,.78f,.38f,0);
            for(int i=0;i<48;i++)
            {
                float a=i*Mathf.PI/24,b=(i+1)*Mathf.PI/24;
                int n=vh.currentVertCount;
                vh.AddVert(center,warm,Vector2.zero);
                vh.AddVert(center+new Vector2(Mathf.Cos(a)*r.width*.5f,Mathf.Sin(a)*r.height*.5f),clear,Vector2.zero);
                vh.AddVert(center+new Vector2(Mathf.Cos(b)*r.width*.5f,Mathf.Sin(b)*r.height*.5f),clear,Vector2.zero);
                vh.AddTriangle(n,n+1,n+2);
            }
            var mesh=new SkinMesh(vh,r);
            mesh.Star(.5f,.5f,.27f,new Color(1,.96f,.77f,.9f),90,4,.24f);
            mesh.Oval(.5f,.5f,.055f,.055f,new Color(1,1,.95f,.9f));
        }
    }
}

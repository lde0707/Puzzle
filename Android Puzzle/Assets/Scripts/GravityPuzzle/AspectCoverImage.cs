using UnityEngine;
using UnityEngine.UI;

namespace Puzzle.Gravity
{
    // Fill a panel by cropping the source, never by stretching the artwork.
    public sealed class AspectCoverImage : RawImage
    {
        public Rect SourceUV
        {
            get
            {
                var r=rectTransform.rect;
                if(!texture||r.width<=0||r.height<=0)return new Rect(0,0,1,1);
                float view=r.width/r.height,source=(float)texture.width/texture.height;
                float w=Mathf.Min(1,view/source),h=Mathf.Min(1,source/view);
                return new Rect((1-w)*.5f,(1-h)*.5f,w,h);
            }
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var r=GetPixelAdjustedRect();var uv=SourceUV;
            vh.AddVert(new Vector3(r.xMin,r.yMin),color,new Vector2(uv.xMin,uv.yMin));
            vh.AddVert(new Vector3(r.xMin,r.yMax),color,new Vector2(uv.xMin,uv.yMax));
            vh.AddVert(new Vector3(r.xMax,r.yMax),color,new Vector2(uv.xMax,uv.yMax));
            vh.AddVert(new Vector3(r.xMax,r.yMin),color,new Vector2(uv.xMax,uv.yMin));
            vh.AddTriangle(0,1,2);vh.AddTriangle(2,3,0);
        }
    }
}

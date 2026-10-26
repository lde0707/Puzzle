using UnityEngine;
using UnityEngine.UI;

namespace Puzzle.Gravity
{
    // Vertex lighting preserves Image tint changes, masks, hit areas and sliced corners.
    public sealed class SoftSurface : BaseMeshEffect
    {
        public override void ModifyMesh(VertexHelper mesh)
        {
            if(!IsActive())return;
            var rect=((RectTransform)transform).rect;var vertex=new UIVertex();
            for(int i=0;i<mesh.currentVertCount;i++)
            {
                mesh.PopulateUIVertex(ref vertex,i);
                float y=Mathf.InverseLerp(rect.yMin,rect.yMax,vertex.position.y);
                Color original=vertex.color;
                Color tint=y>.35f?Color.Lerp(original,Color.white,(y-.35f)*.24f):Color.Lerp(original,new Color(.28f,.19f,.40f),(.35f-y)*.19f);
                tint.a=original.a;vertex.color=tint;mesh.SetUIVertex(vertex,i);
            }
        }
    }
}

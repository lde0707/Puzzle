using UnityEngine;
using UnityEngine.UI;

namespace Puzzle.Gravity
{
    // Small, pooled soft bloom; no full-screen flash or post-processing required.
    public sealed class CompletionStarGlow : MaskableGraphic
    {
        private float strength;
        public void Pose(float value){if(Mathf.Approximately(value,strength))return;strength=value;SetVerticesDirty();}
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();if(strength<=0)return;
            var m=new SkinMesh(vh,rectTransform.rect,false,24);
            for(int i=11;i>=0;i--)
            {
                float radius=.15f+i*.028f;
                m.Oval(.5f,.5f,radius,radius,new Color(1,.86f,.57f,strength*(.014f+(11-i)*.003f)));
            }
        }
    }
}

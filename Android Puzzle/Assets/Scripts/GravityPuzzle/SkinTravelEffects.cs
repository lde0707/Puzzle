using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzle.Gravity
{
    // Board-space particles retain their old position when the character slides away.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class SkinTravelEffects : MaskableGraphic
    {
        public const int Capacity=256;
        private struct Particle
        {
            public Vector2 position,velocity;
            public float age,life,size,rotation;
            public int skin,variant;
            public bool arrival,ring;
        }
        private sealed class Emitter { public Vector2 previous;public float age;public bool initialized; }
        private readonly List<Particle> particles=new List<Particle>(Capacity);
        private readonly Dictionary<int,Emitter> emitters=new Dictionary<int,Emitter>();
        public int ParticleCount=>particles.Count;
        public void Clear()
        {particles.Clear();emitters.Clear();SetVerticesDirty();}
        public void Tick(float delta,bool reduced)
        {
            if(reduced){if(particles.Count>0||emitters.Count>0)Clear();return;}
            if(delta<=0)return;
            for(int i=particles.Count-1;i>=0;i--)
            {
                var p=particles[i];p.age+=delta;
                if(p.age>=p.life)particles.RemoveAt(i);else particles[i]=p;
            }
            SetVerticesDirty();
        }
        private void Add(Particle p)
        {if(particles.Count>=Capacity)particles.RemoveAt(0);particles.Add(p);SetVerticesDirty();}
        public void Trail(int actor,int skin,int variant,Vector2 center,float extent,bool moving,float delta,bool reduced)
        {
            if(reduced)return;
            if(!emitters.TryGetValue(actor,out var e)){e=new Emitter();emitters.Add(actor,e);}
            if(!moving){e.initialized=false;e.age=0;return;}
            if(delta<=0)return;
            if(!e.initialized){e.previous=center;e.initialized=true;return;}
            Vector2 distance=center-e.previous;
            // Never paint a line across a teleport or a layout change.
            if(distance.magnitude>extent*2.5f){e.previous=center;e.age=0;return;}
            e.age+=delta;
            if(distance.sqrMagnitude>.01f&&e.age>=.028f)
            {
                int count=Mathf.Clamp(Mathf.CeilToInt(distance.magnitude/(extent*.16f)),2,5);
                Vector2 side=new Vector2(-distance.y,distance.x).normalized;
                for(int i=0;i<count;i++)
                {
                    float lane=(i%2==0?-1:1)*extent*.48f;
                    // Every motif starts on the centre path, then blossoms outwards.
                    Add(new Particle{skin=skin,variant=variant,position=Vector2.Lerp(e.previous,center,(i+1f)/count),
                        velocity=side*lane-distance.normalized*extent*.18f,size=extent*(.33f+(skin%3)*.035f),life=.64f+(skin%4)*.06f,
                        rotation=Mathf.Atan2(distance.y,distance.x)*Mathf.Rad2Deg-90});
                }
                e.age=0;
            }
            e.previous=center;
        }
        public void Arrive(int skin,int variant,Vector2 center,float extent)
        {
            int count=18+(skin%4)*2;
            for(int i=0;i<count;i++)
            {
                float a=(i/(float)count)*Mathf.PI*2+skin*.31f;
                var axis=new Vector2(Mathf.Cos(a),Mathf.Sin(a));
                Add(new Particle{skin=skin,variant=variant,arrival=true,position=center,
                    velocity=axis*extent*(1.15f+(i%3)*.18f),size=extent*(.36f+(i%3)*.045f),life=.85f+(i%3)*.10f,rotation=i*37});
            }
            Add(new Particle{skin=skin,variant=variant,arrival=true,ring=true,position=center,size=extent,life=.85f});
            Add(new Particle{skin=skin,variant=variant,arrival=true,ring=true,position=center,size=extent*.68f,life=1.05f});
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var bounds=rectTransform.rect;
            foreach(var p in particles)
            {
                if(vh.currentVertCount>45000)break; // Leave headroom for a layered motif, even after rapid inputs.
                float t=p.age/p.life,fade=Mathf.Pow(1-t,1.25f);
                Vector2 position=p.position+p.velocity*(p.age*(1-.38f*t));
                if(p.arrival&&!p.ring)position.y-=p.size*.6f*t*t;
                float size=p.size*(p.ring?1.05f+t*1.35f:p.arrival?.7f+.3f*Mathf.Sin(Mathf.Min(1,t*3)*Mathf.PI/2):1-t*.45f);
                var box=new Rect(bounds.xMin+position.x-size/2,bounds.yMin+position.y-size/2,size,size);
                var m=new SkinMesh(vh,box,false,16);
                var tint=Color.Lerp(SkinDecoration.ColorFor(p.skin,p.variant),new Color(.43f,.20f,.65f),.12f);tint.a=fade;
                if(p.ring)
                {
                    // A different contour for each family, with the concept motif as a seal.
                    tint.a*=.72f;
                    int points=4+p.skin%5;
                    if(p.skin==8||p.skin==12||p.skin==22||p.skin==29)m.Arc(.5f,.5f,.45f,.24f,0,360,.018f,tint);
                    else if(p.skin==13||p.skin==18||p.skin==37){m.Arc(.5f,.5f,.44f,.44f,0,360,.012f,tint);m.Star(.5f,.5f,.40f,tint,p.skin*7,points,.82f);}
                    else m.Arc(.5f,.5f,.44f,.44f,20+p.skin*9,320+p.skin*9,.018f,tint);
                    continue;
                }
                int first=vh.currentVertCount;
                var glow=tint;glow.a=fade*.11f;
                m.Oval(.5f,.5f,.58f,.58f,glow);
                glow.a=fade*.18f;m.Oval(.5f,.5f,.43f,.43f,glow);
                SkinEffectMotifs.Draw(m,p.skin,p.arrival,tint);
                m.Star(.36f,.68f,.10f,new Color(1,1,.94f,fade*.90f),0,4,.22f);
                float spin=p.arrival?p.rotation+p.age*(p.skin%2==0?55:-55):p.rotation;
                var rotation=Quaternion.Euler(0,0,spin);var vertex=new UIVertex();
                for(int v=first;v<vh.currentVertCount;v++)
                {vh.PopulateUIVertex(ref vertex,v);vertex.position=box.center+(Vector2)(rotation*(vertex.position-(Vector3)box.center));vh.SetUIVertex(vertex,v);}
            }
        }
    }
}

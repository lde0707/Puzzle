using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzle.Gravity
{
    // Small shared mesh primitives keep the artwork vector based and inexpensive to rebuild.
    public sealed class SkinMesh
    {
        private readonly VertexHelper mesh;
        private readonly Rect rect;
        private readonly bool polished;
        private readonly int ovalSegments;
        public SkinMesh(VertexHelper mesh,Rect rect,bool polished=false,int ovalSegments=40){this.mesh=mesh;this.rect=rect;this.polished=polished;this.ovalSegments=Mathf.Clamp(ovalSegments,12,64);}
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Triangle(Vector2 a,Vector2 b,Vector2 c,Color color)
        {
            TriangleTint(a,b,c,Shade(color,a),Shade(color,b),Shade(color,c));
        }
        private Color Shade(Color c,Vector2 point)
        {
            if(!polished||c.a<.8f||Mathf.Max(c.r,c.g,c.b)<.38f)return c;
            float lift=(point.y-.35f)*.23f+(.5f-point.x)*.12f;
            var result=Color.Lerp(c,lift>0?Color.white:new Color(.29f,.20f,.40f),Mathf.Abs(lift));result.a=c.a;return result;
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void TriangleTint(Vector2 a,Vector2 b,Vector2 c,Color ca,Color cb,Color cc)
        {
            int n=mesh.currentVertCount;
            mesh.AddVert(new Vector3(rect.xMin+a.x*rect.width,rect.yMin+a.y*rect.height),ca,Vector2.zero);
            mesh.AddVert(new Vector3(rect.xMin+b.x*rect.width,rect.yMin+b.y*rect.height),cb,Vector2.zero);
            mesh.AddVert(new Vector3(rect.xMin+c.x*rect.width,rect.yMin+c.y*rect.height),cc,Vector2.zero);
            mesh.AddTriangle(n,n+1,n+2);
        }
        // Three tones on each curved surface: soft centre, rounded rim, shaded underside.
        // All geometry remains inside the original silhouette and collision footprint.
        private void Cushion(Vector2 center,Vector2[] outline,Color color)
        {
            Color middle=Color.Lerp(color,Color.white,.22f);middle.a=color.a;
            for(int i=0;i<outline.Length;i++)
            {
                Vector2 a=outline[i],b=outline[(i+1)%outline.Length];
                Vector2 aa=Vector2.Lerp(center,a,.82f),bb=Vector2.Lerp(center,b,.82f);
                Color Edge(Vector2 p){float facing=(p-center).normalized.y*.7f-(p-center).normalized.x*.3f;var c=Color.Lerp(color,facing>0?Color.white:new Color(.29f,.20f,.40f),Mathf.Abs(facing)*.28f);c.a=color.a;return c;}
                Color ca=Shade(color,aa),cb=Shade(color,bb),ea=Edge(a),eb=Edge(b);
                TriangleTint(center,aa,bb,middle,ca,cb);
                TriangleTint(aa,a,b,ca,ea,eb);TriangleTint(aa,b,bb,ca,eb,cb);
            }
        }
        public void Poly(Color color,params Vector2[] points)
        {for(int i=1;i<points.Length-1;i++)Triangle(points[0],points[i],points[i+1],color);}
        public void Oval(float x,float y,float rx,float ry,Color color)
        {
            if(polished&&rx>=.075f&&ry>=.065f&&color.a>.8f)
            {
                var outline=new Vector2[48];for(int i=0;i<48;i++){float a=i*Mathf.PI/24;outline[i]=new Vector2(x+rx*Mathf.Cos(a),y+ry*Mathf.Sin(a));}
                Cushion(new Vector2(x,y),outline,color);return;
            }
            for(int i=0;i<ovalSegments;i++)
            {
                float a=i*Mathf.PI*2/ovalSegments,b=(i+1)*Mathf.PI*2/ovalSegments;
                Triangle(new Vector2(x,y),new Vector2(x+rx*Mathf.Cos(a),y+ry*Mathf.Sin(a)),new Vector2(x+rx*Mathf.Cos(b),y+ry*Mathf.Sin(b)),color);
            }
        }
        public void Round(float x,float y,float w,float h,float radius,Color color)
        {
            float r=Mathf.Min(radius,Mathf.Min(w,h)/2);
            if(polished&&w>=.24f&&h>=.2f&&color.a>.8f)
            {
                var outline=new Vector2[48];
                for(int corner=0;corner<4;corner++)for(int j=0;j<12;j++)
                {
                    float angle=(corner*90+j*90f/11)*Mathf.Deg2Rad;
                    float cx=corner==0||corner==3?x+w-r:x+r,cy=corner<=1?y+h-r:y+r;
                    outline[corner*12+j]=new Vector2(cx+r*Mathf.Cos(angle),cy+r*Mathf.Sin(angle));
                }
                Cushion(new Vector2(x+w/2,y+h/2),outline,color);return;
            }
            var center=new Vector2(x+w/2,y+h/2);Vector2 previous=Vector2.zero,first=Vector2.zero;
            for(int corner=0;corner<4;corner++)for(int j=0;j<=8;j++)
            {
                float angle=(corner*90+j*90f/8)*Mathf.Deg2Rad;
                float cx=corner==0||corner==3?x+w-r:x+r,cy=corner<=1?y+h-r:y+r;
                var p=new Vector2(cx+r*Mathf.Cos(angle),cy+r*Mathf.Sin(angle));
                if(corner==0&&j==0)first=p;else Triangle(center,previous,p,color);
                previous=p;
            }
            Triangle(center,previous,first,color);
        }
        public void Line(float x,float y,float xx,float yy,float width,Color color)
        {
            var a=new Vector2(x,y);var b=new Vector2(xx,yy);var n=new Vector2(-(b-a).y,(b-a).x).normalized*width/2;
            Poly(color,a+n,b+n,b-n,a-n);Oval(x,y,width/2,width/2,color);Oval(xx,yy,width/2,width/2,color);
        }
        public void Arc(float x,float y,float rx,float ry,float from,float to,float width,Color color)
        {
            for(int i=0;i<24;i++)
            {
                float a=Mathf.Lerp(from,to,i/24f)*Mathf.Deg2Rad,b=Mathf.Lerp(from,to,(i+1)/24f)*Mathf.Deg2Rad;
                // Butt joints avoid redundant circles on every arc segment.
                var p=new Vector2(x+rx*Mathf.Cos(a),y+ry*Mathf.Sin(a));var q=new Vector2(x+rx*Mathf.Cos(b),y+ry*Mathf.Sin(b));var n=new Vector2(-(q-p).y,(q-p).x).normalized*width/2;
                Poly(color,p+n,q+n,q-n,p-n);
            }
        }
        public void Star(float x,float y,float r,Color color,float angle=90,int points=5,float inner=.5f)
        {
            if(polished&&r>.2f)
            {
                var outline=new Vector2[points*2];for(int i=0;i<outline.Length;i++){float a=(angle+i*180f/points)*Mathf.Deg2Rad,rr=i%2==0?r:r*inner;outline[i]=new Vector2(x+rr*Mathf.Cos(a),y+rr*Mathf.Sin(a));}
                Cushion(new Vector2(x,y),outline,color);return;
            }
            for(int j=0;j<points*2;j++)
            {
                float a=(angle+j*180f/points)*Mathf.Deg2Rad,b=(angle+(j+1)*180f/points)*Mathf.Deg2Rad;
                float ra=j%2==0?r:r*inner,rb=j%2==0?r*inner:r;
                Triangle(new Vector2(x,y),new Vector2(x+ra*Mathf.Cos(a),y+ra*Mathf.Sin(a)),new Vector2(x+rb*Mathf.Cos(b),y+rb*Mathf.Sin(b)),color);
            }
        }
        public void Heart(float x,float y,float r,Color color)
        {
            var outline=new Vector2[64];
            for(int i=0;i<64;i++)
            {
                float a=i*Mathf.PI/32;
                outline[i]=new Vector2(x+r*Mathf.Pow(Mathf.Sin(a),3),y+r*(13*Mathf.Cos(a)-5*Mathf.Cos(2*a)-2*Mathf.Cos(3*a)-Mathf.Cos(4*a))/17);
            }
            if(polished&&r>.2f)Cushion(new Vector2(x,y),outline,color);
            else for(int i=0;i<64;i++)Triangle(new Vector2(x,y),outline[i],outline[(i+1)%64],color);
        }
    }
}

using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Puzzle.Gravity
{
    public sealed class PuzzleTap : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public Func<bool> CanUse;
        public Func<int> Epoch;
        public Action Activate;
        public Image surface;
        public Color normal;
        private static int owner=int.MinValue;
        private int pointer=int.MinValue, epoch;
        private bool armed;
        private float press;
        public static void CancelAll() { owner=int.MinValue; }
        public void OnPointerDown(PointerEventData e)
        {
            if(owner!=int.MinValue || !Enabled()) return;
            pointer=e.pointerId; owner=pointer; epoch=Epoch(); armed=true;
            if(surface) surface.color=normal*new Color(.9f,.9f,.9f,1);
        }
        public void OnPointerUp(PointerEventData e)
        {
            if(e.pointerId!=pointer)return;
            bool fire=armed && e.pointerId==pointer && owner==pointer && epoch==Epoch() && Enabled() &&
                RectTransformUtility.RectangleContainsScreenPoint((RectTransform)transform,e.position,e.pressEventCamera);
            Release();
            if(fire) Activate?.Invoke();
        }
        public void OnPointerExit(PointerEventData e) { if(e.pointerId==pointer) Release(); }
        private bool Enabled() => CanUse == null || CanUse();
        private void Release() { if(owner==pointer) owner=int.MinValue; pointer=int.MinValue; armed=false; if(surface)surface.color=normal; }
        private void OnDisable() { Release(); }
        private void Update()
        {
            if(armed && (epoch!=Epoch() || owner!=pointer || !Enabled())) Release();
            if(surface && !armed) surface.color=Enabled()?normal:Color.Lerp(normal,new Color(.87f,.88f,.86f),.65f);
            press=PuzzleUI.ReduceMotion?0:Mathf.MoveTowards(press,armed?1:0,Time.unscaledDeltaTime*14);
            transform.localScale=Vector3.one*(1-.055f*press);
        }
    }
    // ScrollRect receives drags from its child rows. Disarm their tap before release can select one.
    public sealed class PuzzleLevelScroll : ScrollRect
    {
        public override void OnBeginDrag(PointerEventData eventData)
        {
            PuzzleTap.CancelAll();
            base.OnBeginDrag(eventData);
        }
    }
    public enum GlyphKind { Up, Left, Down, Right, Pause, Undo, Reset, Grid, Check, Smile, Star, Ring, Face, Happy }
    public sealed class PuzzleGlyph : MaskableGraphic
    {
        public GlyphKind kind;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear(); var rect=rectTransform.rect;
            Vector2 P(float x,float y)=>new Vector2(rect.xMin+rect.width*x,rect.yMin+rect.height*y);
            void Triangle(Vector2 a,Vector2 b,Vector2 c)
            {
                int n=vh.currentVertCount; vh.AddVert(a,color,Vector2.zero);vh.AddVert(b,color,Vector2.zero);vh.AddVert(c,color,Vector2.zero);vh.AddTriangle(n,n+1,n+2);
            }
            void Line(Vector2 a,Vector2 b,float thickness)
            {
                var n=(b-a).normalized; n=new Vector2(-n.y,n.x)*thickness/2;
                Triangle(a+n,b+n,b-n);Triangle(a+n,b-n,a-n);
            }
            float w=Mathf.Min(rect.width,rect.height)*.095f;
            if(kind==GlyphKind.Ring)
            {
                for(int i=0;i<96;i++) { float a=i*Mathf.PI/48,b=(i+1)*Mathf.PI/48;Line(P(.5f+.44f*Mathf.Cos(a),.5f+.44f*Mathf.Sin(a)),P(.5f+.44f*Mathf.Cos(b),.5f+.44f*Mathf.Sin(b)),Mathf.Clamp(w*.24f,1f,4f)); }
                return;
            }
            if(kind==GlyphKind.Star)
            {
                for(int i=0;i<8;i++) { float a=i*Mathf.PI/4,b=(i+1)*Mathf.PI/4;float ra=i%2==0?.46f:.15f,rb=i%2==0?.15f:.46f;Triangle(P(.5f,.5f),P(.5f+ra*Mathf.Cos(a),.5f+ra*Mathf.Sin(a)),P(.5f+rb*Mathf.Cos(b),.5f+rb*Mathf.Sin(b))); }
                return;
            }
            if(kind==GlyphKind.Face || kind==GlyphKind.Happy)
            {
                if(kind==GlyphKind.Face) { Line(P(.28f,.68f),P(.28f,.85f),w);Line(P(.72f,.68f),P(.72f,.85f),w); }
                else { Line(P(.18f,.70f),P(.28f,.85f),w*.8f);Line(P(.28f,.85f),P(.38f,.70f),w*.8f);Line(P(.62f,.70f),P(.72f,.85f),w*.8f);Line(P(.72f,.85f),P(.82f,.70f),w*.8f); }
                for(int i=0;i<12;i++) { float a=(200+i*12)*Mathf.Deg2Rad,b=(212+i*12)*Mathf.Deg2Rad;Line(P(.5f+.19f*Mathf.Cos(a),.55f+.19f*Mathf.Sin(a)),P(.5f+.19f*Mathf.Cos(b),.55f+.19f*Mathf.Sin(b)),w*.65f); }
                return;
            }
            if(kind<=GlyphKind.Right)
            {
                Vector2 a=P(.5f,.2f), b=P(.5f,.8f), l=P(.25f,.55f),r=P(.75f,.55f);
                float angle=kind==GlyphKind.Up?0:kind==GlyphKind.Left?90:kind==GlyphKind.Down?180:-90;
                Vector2 Rot(Vector2 p) { var q=p-rect.center; return (Vector2)(Quaternion.Euler(0,0,angle)*q)+rect.center; }
                Line(Rot(a),Rot(b),w);Line(Rot(l),Rot(b),w);Line(Rot(r),Rot(b),w);
            }
            else if(kind==GlyphKind.Pause) { Line(P(.35f,.25f),P(.35f,.75f),w*1.3f);Line(P(.65f,.25f),P(.65f,.75f),w*1.3f); }
            else if(kind==GlyphKind.Check) { Line(P(.2f,.48f),P(.43f,.25f),w);Line(P(.43f,.25f),P(.83f,.78f),w); }
            else if(kind==GlyphKind.Grid)
            {
                for(int i=0;i<4;i++) { float x=.22f+(i%2)*.34f,y=.22f+(i/2)*.34f;Triangle(P(x,y),P(x+.22f,y),P(x+.22f,y+.22f));Triangle(P(x,y),P(x+.22f,y+.22f),P(x,y+.22f)); }
            }
            else if(kind==GlyphKind.Smile)
            {
                for(int i=0;i<10;i++) { float a=(205+i*13)*Mathf.Deg2Rad,b=(218+i*13)*Mathf.Deg2Rad; Line(P(.5f+.3f*Mathf.Cos(a),.65f+.3f*Mathf.Sin(a)),P(.5f+.3f*Mathf.Cos(b),.65f+.3f*Mathf.Sin(b)),w*.65f); }
            }
            else
            {
                for(int i=0;i<18;i++) { float a=(-60+i*14)*Mathf.Deg2Rad,b=(-46+i*14)*Mathf.Deg2Rad; Line(P(.52f+.29f*Mathf.Cos(a),.48f+.29f*Mathf.Sin(a)),P(.52f+.29f*Mathf.Cos(b),.48f+.29f*Mathf.Sin(b)),w*.8f); }
                Line(P(.18f,.72f),P(.45f,.72f),w);Line(P(.18f,.72f),P(.18f,.96f),w);
            }
        }
    }
    public static class PuzzleUI
    {
        public static readonly Color Ink=new Color(.24f,.17f,.37f); // Blackberry #3D2B5E
        public static readonly Color Muted=new Color(.38f,.31f,.48f);
        public static readonly Color Cream=new Color(.98f,.96f,1f);
        public static readonly Color Mint=new Color(.82f,.76f,.96f); // Signature lilac
        public static readonly Color Accent=new Color(.99f,.63f,.49f);
        public static readonly Color[] Candy={Accent,new Color(.40f,.82f,.85f),new Color(1f,.82f,.37f)};
        public static Font Font;
        public static bool ReduceMotion;
        private static Sprite round;
        public static Sprite Round
        {
            get
            {
                if(round) return round;
                const int size=128; const float radius=32;
                var texture=new Texture2D(size,size,TextureFormat.RGBA32,false) { name="Rounded UI", filterMode=FilterMode.Bilinear, wrapMode=TextureWrapMode.Clamp };
                var pixels=new Color[size*size];
                for(int y=0;y<size;y++) for(int x=0;x<size;x++)
                {
                    float dx=Mathf.Max(Mathf.Abs(x+.5f-size/2f)-(size/2f-radius),0),dy=Mathf.Max(Mathf.Abs(y+.5f-size/2f)-(size/2f-radius),0);
                    pixels[y*size+x]=new Color(1,1,1,Mathf.Clamp01(radius-Mathf.Sqrt(dx*dx+dy*dy)+.5f));
                }
                texture.SetPixels(pixels);texture.Apply(false,true);
                round=Sprite.Create(texture,new Rect(0,0,size,size),new Vector2(.5f,.5f),200,0,SpriteMeshType.FullRect,new Vector4(32,32,32,32));
                return round;
            }
        }
        public static RectTransform Rect(string name,Transform parent)
        {
            var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);return go.GetComponent<RectTransform>();
        }
        public static void Fill(RectTransform r,float inset=0) { r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=Vector2.one*inset;r.offsetMax=-Vector2.one*inset; }
        public static void Box(RectTransform r,float x,float y,float w,float h)
        { r.anchorMin=r.anchorMax=Vector2.zero;r.pivot=Vector2.zero;r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(w,h); }
        public static Image Panel(string name,Transform parent,Color color,bool rounded=true)
        {
            var r=Rect(name,parent);var image=r.gameObject.AddComponent<Image>();image.color=color;image.raycastTarget=false;
            if(rounded){image.sprite=Round;image.type=Image.Type.Sliced;image.pixelsPerUnitMultiplier=2;image.gameObject.AddComponent<SoftSurface>();}
            return image;
        }
        public static Text Label(string name,Transform parent,string value,int size,Color color,TextAnchor alignment=TextAnchor.MiddleCenter)
        {
            var r=Rect(name,parent);var text=r.gameObject.AddComponent<Text>();text.font=Font;text.text=value;text.fontSize=size;text.color=color;text.alignment=alignment;
            text.raycastTarget=false;text.horizontalOverflow=HorizontalWrapMode.Wrap;text.verticalOverflow=VerticalWrapMode.Truncate;return text;
        }
        public static PuzzleGlyph Glyph(Transform parent,GlyphKind kind,Color color)
        {
            var r=Rect(kind.ToString(),parent);var glyph=r.gameObject.AddComponent<PuzzleGlyph>();glyph.kind=kind;glyph.color=color;glyph.raycastTarget=false;return glyph;
        }
    }
}



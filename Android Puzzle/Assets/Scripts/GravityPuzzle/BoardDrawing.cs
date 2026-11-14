using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Puzzle.Gravity
{
    public sealed class DrawingUndoShortcut : System.IDisposable
    {
        private readonly InputAction action;
        private int performedFrame=-1;
        public bool PerformedThisFrame=>performedFrame==Time.frameCount;
        public DrawingUndoShortcut(System.Action undo)
        {
            action=new InputAction("Undo drawing",InputActionType.Button);
            foreach(string modifier in new[]{"<Keyboard>/ctrl","<Keyboard>/leftMeta","<Keyboard>/rightMeta"})
                action.AddCompositeBinding("ButtonWithOneModifier").With("Modifier",modifier).With("Button","<Keyboard>/z");
            action.performed+=context=>{performedFrame=Time.frameCount;if(Keyboard.current?.shiftKey.isPressed!=true)undo();};
            action.Enable();
        }
        public void Dispose(){action.Dispose();}
    }
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class BoardDrawing : MaskableGraphic,IPointerDownHandler,IDragHandler,IPointerUpHandler,IInitializePotentialDragHandler
    {
        public struct Segment {public Vector2 a,b;public int color;}
        public readonly List<Segment> Lines=new List<Segment>();
        public static readonly Color[] Ink={new Color(.88f,.24f,.40f),new Color(.18f,.48f,.89f),new Color(.16f,.64f,.43f),new Color(.64f,.32f,.83f)};
        public bool Editing,Erasing;
        public int ColorIndex;
        public System.Func<bool> CanDraw;
        public System.Func<Vector2,bool> InspectTap;
        private int pointer=int.MinValue;
        private Vector2 previous;
        private bool outside,dragged;
        private readonly List<Segment[]> undoHistory=new List<Segment[]>();
        private Segment[] beforeStroke;
        public const int UndoLimit=64;
        public bool Full=>Lines.Count>=3000;
        public void OnInitializePotentialDrag(PointerEventData e){e.useDragThreshold=false;}
        public void BeginStroke(){Cancel();beforeStroke=Lines.ToArray();}
        public void EndStroke()
        {
            if(beforeStroke!=null&&!Lines.SequenceEqual(beforeStroke))Remember(beforeStroke);
            beforeStroke=null;
        }
        private void Remember(Segment[] previous)
        {
            if(undoHistory.Count==UndoLimit)undoHistory.RemoveAt(0);
            undoHistory.Add(previous);
        }
        public bool UndoStroke()
        {
            Cancel();
            if(undoHistory.Count==0)return false;
            int last=undoHistory.Count-1;Lines.Clear();Lines.AddRange(undoHistory[last]);undoHistory.RemoveAt(last);
            SetVerticesDirty();return true;
        }
        public void Clear(){Cancel();if(Lines.Count==0)return;Remember(Lines.ToArray());Lines.Clear();SetVerticesDirty();}
        public void Cancel(){EndStroke();pointer=int.MinValue;}
        private bool Position(PointerEventData e,out Vector2 uv)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,e.position,e.pressEventCamera,out var p);
            var r=rectTransform.rect;uv=new Vector2((p.x-r.xMin)/r.width,(p.y-r.yMin)/r.height);
            return uv.x>=0&&uv.x<=1&&uv.y>=0&&uv.y<=1;
        }
        public void OnPointerDown(PointerEventData e)
        {
            if(!Editing||pointer!=int.MinValue||CanDraw?.Invoke()==false||!Position(e,out var p))return;
            BeginStroke();pointer=e.pointerId;previous=p;outside=false;dragged=false;
        }
        public void OnDrag(PointerEventData e)
        {
            if(pointer!=e.pointerId)return;
            if(!Editing||CanDraw?.Invoke()==false){Cancel();return;}
            if(!Position(e,out var p)){outside=true;dragged=true;return;}
            if(outside){previous=p;outside=false;Mark(p,p);return;}
            if(Vector2.Scale(p-previous,rectTransform.rect.size).sqrMagnitude<.5f)return;
            dragged=true;Mark(previous,p);previous=p;
        }
        public void OnPointerUp(PointerEventData e)
        {
            if(pointer!=e.pointerId)return;
            if(Editing&&!outside&&CanDraw?.Invoke()!=false&&Position(e,out var p))
            {
                // A short tap keeps portal/resize inspection available. Dragging
                // draws over any tile; erasing always erases, including special tiles.
                bool inspected=!dragged&&!Erasing&&(InspectTap?.Invoke(p)??false);
                if(!inspected)Mark(previous,p);
            }
            Cancel();
        }
        public void Mark(Vector2 a,Vector2 b)
        {
            if(Erasing)
            {
                var size=rectTransform.rect.size;var x=Vector2.Scale(a,size);var y=Vector2.Scale(b,size);
                var kept=new List<Segment>();
                foreach(var s in Lines)
                {
                    int steps=Mathf.Max(1,Mathf.CeilToInt(Vector2.Scale(s.b-s.a,size).magnitude/2));
                    Vector2? start=null;Vector2 last=s.a;
                    for(int i=0;i<=steps;i++)
                    {
                        var p=Vector2.Lerp(s.a,s.b,(float)i/steps);
                        if(Distance(Vector2.Scale(p,size),x,y)>=10){if(!start.HasValue)start=p;last=p;}
                        else if(start.HasValue){kept.Add(new Segment{a=start.Value,b=last,color=s.color});start=null;}
                    }
                    if(start.HasValue)kept.Add(new Segment{a=start.Value,b=last,color=s.color});
                }
                Lines.Clear();Lines.AddRange(kept);
            }
            else if(Lines.Count<3000)Lines.Add(new Segment{a=a,b=b,color=ColorIndex});
            SetVerticesDirty();
        }
        private static float Distance(Vector2 p,Vector2 a,Vector2 b){var d=b-a;return Vector2.Distance(p,a+d*(d.sqrMagnitude<.001f?0:Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude)));}
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var r=rectTransform.rect;
            foreach(var s in Lines)
            {
                if(vh.currentVertCount>64000)break;
                Vector2 a=r.min+Vector2.Scale(s.a,r.size),b=r.min+Vector2.Scale(s.b,r.size);
                const float radius=2.4f;
                Round(vh,a,radius,Ink[s.color]);
                if((b-a).sqrMagnitude<.01f)continue;
                Round(vh,b,radius,Ink[s.color]);
                Vector2 n=new Vector2(-(b-a).y,(b-a).x).normalized*radius;int k=vh.currentVertCount;
                vh.AddVert(a-n,Ink[s.color],Vector2.zero);vh.AddVert(a+n,Ink[s.color],Vector2.zero);vh.AddVert(b+n,Ink[s.color],Vector2.zero);vh.AddVert(b-n,Ink[s.color],Vector2.zero);
                vh.AddTriangle(k,k+1,k+2);vh.AddTriangle(k,k+2,k+3);
            }
        }
        private static void Round(VertexHelper vh,Vector2 p,float radius,Color color)
        {
            int k=vh.currentVertCount;vh.AddVert(p,color,Vector2.zero);
            for(int i=0;i<6;i++){float a=i*Mathf.PI/3;vh.AddVert(p+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,color,Vector2.zero);}
            for(int i=0;i<6;i++)vh.AddTriangle(k,k+1+i,k+1+(i+1)%6);
        }
    }
    public sealed partial class GravityPuzzleApp
    {
        private DrawingUndoShortcut drawingUndoShortcut;
        private BoardDrawing drawing;
        private RectTransform drawingTools;
        private PuzzleTap[] colorTools;
        private PuzzleTap eraserTool;
        private int selectedDrawingColor;
        private bool selectedDrawingEraser;
        private void CreateDrawingUI()
        {
            drawingUndoShortcut=new DrawingUndoShortcut(()=>{if(drawing&&drawing.Editing&&Active&&modal=="")drawing.UndoStroke();});
            drawingTools=PuzzleUI.Rect("Map memo tools",safe);
            colorTools=new PuzzleTap[4];
            for(int i=0;i<4;i++){int c=i;colorTools[i]=MakeButton(drawingTools,"Pen color "+i,"",null,()=>Active&&modal==""&&drawing&&session.Ready,()=>SelectDrawingTool(c,false),BoardDrawing.Ink[i]);PuzzleUI.Box((RectTransform)colorTools[i].transform,i*44,0,38,38);}
            eraserTool=MakeButton(drawingTools,"Eraser","지우개",null,()=>Active&&modal==""&&drawing&&session.Ready,()=>SelectDrawingTool(selectedDrawingColor,true),PuzzleUI.Cream);PuzzleUI.Box((RectTransform)eraserTool.transform,180,0,60,38);
            var clear=MakeButton(drawingTools,"Clear all ink","전체 지우기",null,()=>Active&&modal==""&&drawing&&session.Ready,()=>drawing.Clear(),PuzzleUI.Cream);PuzzleUI.Box((RectTransform)clear.transform,246,0,82,38);
            foreach(var t in drawingTools.GetComponentsInChildren<Text>())t.fontSize=11;
        }
        private void SelectDrawingTool(int color,bool erasing)
        {
            drawing.Cancel();selectedDrawingColor=color;selectedDrawingEraser=erasing;
            drawing.ColorIndex=color;drawing.Erasing=erasing;
        }
        private void AttachDrawing()
        {
            var clip=PuzzleUI.Rect("Drawing clip",boardRoot);PuzzleUI.Fill(clip);clip.gameObject.AddComponent<RectMask2D>();
            var surface=PuzzleUI.Rect("Map drawing",clip);PuzzleUI.Fill(surface);drawing=surface.gameObject.AddComponent<BoardDrawing>();drawing.raycastTarget=false;
            drawing.Editing=true;drawing.ColorIndex=selectedDrawingColor;drawing.Erasing=selectedDrawingEraser;
            drawing.CanDraw=()=>Active&&modal==""&&session.Ready;
            drawing.InspectTap=board.InspectAt;
        }
        private void TickDrawing()
        {
            if(!drawing)return;
            drawing.raycastTarget=drawing.Editing&&Active&&modal==""&&session.Ready;
            if(!drawing.raycastTarget)drawing.Cancel();
            if(drawing.Full)hint.text="메모가 가득 찼어요 · 지우거나 Ctrl+Z로 되돌려요";
            for(int i=0;i<4;i++)colorTools[i].surface.GetComponent<Outline>().effectColor=drawing.Editing&&!drawing.Erasing&&drawing.ColorIndex==i?Color.white:new Color(0,0,0,.1f);
            eraserTool.surface.GetComponent<Outline>().effectColor=drawing.Erasing?PuzzleUI.Accent:new Color(0,0,0,.1f);
        }
    }
}

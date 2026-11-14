using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzle.Gravity
{
    // Presentation only: logical positions and shared travel distance remain in PuzzleSession.
    public sealed partial class BoardView
    {
        private sealed class Friend
        {
            public RectTransform root,body,halo;
            public PuzzleGlyph ring;
            public SkinDecoration skin;
            public bool travelling,atHome,motionInitialized;
            public Cell destination;
            public float impact=10,arrivalAge=10;
        }
        private sealed class GateVisual { public int index; public PuzzleGlyph landing; public Image face,opening,aura,glow,pool; public Color color; }
        private sealed class LinkVisual { public int pair; public RectTransform[] dots; }
        private sealed class ResizeVisual { public ResizeDefinition definition;public Image frame;public Color color; }
        private readonly List<ResizeVisual> resizePads=new List<ResizeVisual>();
        public System.Action<string> OnHint;
        private readonly List<LinkVisual> links=new List<LinkVisual>();
        private PuzzleSession lastSession;
        private int generation,selectedGate=-1;
        private float previewTime;
        private static readonly Color[] PortalColors={new Color(.13f,.48f,.76f),new Color(.68f,.24f,.73f),new Color(.24f,.58f,.32f)};
        private readonly List<GateVisual> gates=new List<GateVisual>();
        private readonly RectTransform root;
        private readonly List<Friend> friends=new List<Friend>();
        private readonly List<RectTransform> slots=new List<RectTransform>();
        private readonly List<Cell> slotCells=new List<Cell>();
        private readonly List<int> slotSizes=new List<int>();
        private PuzzleDefinition level;
        private float cell,clock,celebration=-1;
        private int direction;
        public BoardView(RectTransform root) { this.root=root; }
        private int selectedSkin;
        private SkinTravelEffects skinEffects;
        private readonly List<Image> goalColors=new List<Image>();
        public void ApplySkin(int id)
        {
            selectedSkin=id;
            if(skinEffects)skinEffects.Clear();
            foreach(var friend in friends){friend.arrivalAge=10;friend.skin.Motion(false,10,Vector2.zero);friend.skin.skin=id;friend.skin.SetVerticesDirty();}
            for(int i=0;i<goalColors.Count;i++)goalColors[i].color=SkinDecoration.ColorFor(id,i%3);
        }
        private static void Region(RectTransform r,float x,float y,float right,float top)
        { r.anchorMin=new Vector2(x,y);r.anchorMax=new Vector2(right,top);r.offsetMin=r.offsetMax=Vector2.zero; }
        public void Build(PuzzleDefinition definition)
        {
            for(int i=root.childCount-1;i>=0;i--) { var child=root.GetChild(i);child.gameObject.SetActive(false);PuzzleUI.DisposeView(child.gameObject); }
            blinkVisuals.Clear();friends.Clear();goalColors.Clear();gates.Clear();resizePads.Clear();links.Clear();slots.Clear();slotCells.Clear();slotSizes.Clear();level=definition;clock=0;celebration=-1;generation++;selectedGate=-1;previewTime=0;lastSession=null;
            var shadow=PuzzleUI.Panel("Floating tray shadow",root,new Color(.28f,.18f,.42f,.18f));PuzzleUI.Fill(shadow.rectTransform,-11);shadow.rectTransform.anchoredPosition=new Vector2(0,-6);
            var rim=PuzzleUI.Panel("Blackberry rim",root,PuzzleUI.Ink);PuzzleUI.Fill(rim.rectTransform,-8);
            var board=PuzzleUI.Panel("Vanilla moon tray",root,new Color(.94f,.91f,.98f));PuzzleUI.Fill(board.rectTransform,-4);
            var blocked=level.SolidCells();
            for(int y=0;y<level.height;y++) for(int x=0;x<level.width;x++)
            {
                if(blocked.Contains(new Cell(x,y)))continue;
                var floor=PuzzleUI.Rect("Floor coordinate",root);AddSlot(floor,new Cell(x,y),1);
                var dot=PuzzleUI.Panel("Sugar dot",floor,new Color(.75f,.69f,.83f,.5f));Region(dot.rectTransform,.46f,.46f,.54f,.54f);
            }
            if(level.HasResizers)foreach(var p in level.resizers)
            {
                Color tint=p.targetSize==2?new Color(.25f,.61f,.49f):new Color(.77f,.40f,.20f);
                var pad=PuzzleUI.Panel("Resize pad "+p.targetSize,root,Color.Lerp(tint,Color.white,.74f));AddSlot(pad.rectTransform,new Cell(p.x,p.y),2);
                var frame=PuzzleUI.Panel("Resize target outline",pad.transform,tint);
                float extent=p.targetSize==2?.96f:.48f;Region(frame.rectTransform,.04f,.04f,extent,extent);
                var inset=PuzzleUI.Panel("Resize target inset",frame.transform,Color.Lerp(tint,Color.white,.86f));PuzzleUI.Fill(inset.rectTransform,2);
                var symbol=PuzzleUI.Label("Resize symbol",pad.transform,p.targetSize==2?"+":"−",23,tint);Region(symbol.rectTransform,.48f,.48f,.98f,.98f);
                var anchor=PuzzleUI.Panel("Alignment corner",pad.transform,Color.white);Region(anchor.rectTransform,.06f,.06f,.20f,.20f);
                pad.raycastTarget=true;var tap=pad.gameObject.AddComponent<PuzzleTap>();tap.Epoch=()=>generation;tap.CanUse=()=>lastSession!=null&&!lastSession.Paused&&!lastSession.Moving;
                tap.Activate=()=>OnHint?.Invoke(p.targetSize==2?"확대대: 왼쪽 아래에 멈추면 2×2로 커져요.":"축소대: 왼쪽 아래에 멈추면 1×1로 작아져요.");
                resizePads.Add(new ResizeVisual{definition=p,frame=frame,color=tint});
            }
            if(level.HasPortals)for(int i=0;i<level.portals.Length;i++)
            {
                var p=level.portals[i];Color tint=PortalColors[i%PortalColors.Length];
                var link=new LinkVisual{pair=i,dots=new RectTransform[24]};links.Add(link);
                for(int dot=0;dot<link.dots.Length;dot++)
                {
                    var mark=PuzzleUI.Panel("Portal connection",root,new Color(tint.r,tint.g,tint.b,.55f));link.dots[dot]=mark.rectTransform;mark.gameObject.SetActive(false);
                }
                for(int side=0;side<2;side++)
                {
                    var landingRoot=PuzzleUI.Rect("Portal landing "+p.id,root);AddSlot(landingRoot,p.Contact(side),p.size);
                    var pool=PuzzleUI.Panel("Exit light pool",landingRoot,new Color(tint.r,tint.g,tint.b,0));PuzzleUI.Fill(pool.rectTransform,-4);
                    var landing=PuzzleUI.Glyph(landingRoot,GlyphKind.Ring,new Color(tint.r,tint.g,tint.b,0));PuzzleUI.Fill(landing.rectTransform,-2);
                    var pad=Stone("Portal stone "+p.id+" / "+side,p.Origin(side),p.size);
                    var mouth=PuzzleUI.Rect("Portal aperture",pad.transform);
                    switch(p.Face(side))
                    {
                        case 0: Region(mouth,.03f,-.13f,.97f,.29f);break;
                        case 1: Region(mouth,-.13f,.03f,.29f,.97f);break;
                        case 2: Region(mouth,.03f,.71f,.97f,1.13f);break;
                        default: Region(mouth,.71f,.03f,1.13f,.97f);break;
                    }
                    // Only the aperture emits light; the shared stone silhouette stays untouched.
                    var aura=PuzzleUI.Panel("Soft portal aura",mouth,new Color(tint.r,tint.g,tint.b,.13f));PuzzleUI.Fill(aura.rectTransform,-3);
                    var glow=PuzzleUI.Panel("Near portal glow",mouth,new Color(tint.r,tint.g,tint.b,.3f));PuzzleUI.Fill(glow.rectTransform,-1.5f);
                    var stripe=PuzzleUI.Panel("Portal mouth rim",mouth,Color.white);PuzzleUI.Fill(stripe.rectTransform);
                    var slit=PuzzleUI.Panel("Portal mouth light",stripe.transform,Color.Lerp(tint,Color.white,.35f));PuzzleUI.Fill(slit.rectTransform,1.1f);
                    var cavity=PuzzleUI.Panel("Portal mouth depth",slit.transform,Color.Lerp(tint,PuzzleUI.Ink,.65f));PuzzleUI.Fill(cavity.rectTransform,1.2f);
                    int endpoint=i*2+side;pad.raycastTarget=true;var tap=pad.gameObject.AddComponent<PuzzleTap>();tap.Epoch=()=>generation;tap.CanUse=()=>lastSession!=null&&!lastSession.Paused&&!lastSession.Moving;tap.Activate=()=>{selectedGate=endpoint;previewTime=4;};
                    gates.Add(new GateVisual{index=i*2+side,opening=slit,aura=aura,glow=glow,face=stripe,color=tint,pool=pool,landing=landing});
                }
            }
            for(int i=0;i<level.cargo.Length;i++)
            {
                var c=level.cargo[i];var color=SkinDecoration.ColorFor(selectedSkin,i%3);
                var target=PuzzleUI.Panel("Home "+(i+1),root,color);AddSlot(target.rectTransform,new Cell(c.gx,c.gy),level.GoalSizes[i]);goalColors.Add(target);
                var orbit=PuzzleUI.Glyph(target.transform,GlyphKind.Ring,Color.white);PuzzleUI.Fill(orbit.rectTransform,1);
                var star=PuzzleUI.Glyph(target.transform,GlyphKind.Star,PuzzleUI.Ink);Region(star.rectTransform,.27f,.27f,.73f,.73f);
            }
            var portalCells=new HashSet<Cell>();
            if(level.HasPortals)foreach(var portal in level.portals)for(int side=0;side<2;side++)for(int dx=0;dx<portal.size;dx++)for(int dy=0;dy<portal.size;dy++)
            { var origin=portal.Origin(side);portalCells.Add(new Cell(origin.x+dx,origin.y+dy)); }
            foreach(var c in level.walls)
            {
                if(portalCells.Contains(c))continue;
                Stone("Moon pebble",c,1);
            }
            BuildBlinkBlocks();
            // Foreground motifs stay visible over the jelly body; the board still clips them.
            var effectsClip=PuzzleUI.Rect("Skin effect boundary",root);PuzzleUI.Fill(effectsClip);effectsClip.gameObject.AddComponent<RectMask2D>();
            var effectsRoot=PuzzleUI.Rect("Skin trails and arrivals",effectsClip);PuzzleUI.Fill(effectsRoot);
            skinEffects=effectsRoot.gameObject.AddComponent<SkinTravelEffects>();skinEffects.raycastTarget=false;
            for(int i=0;i<level.cargo.Length;i++)
            {
                var actor=new Friend { root=PuzzleUI.Rect("Friend "+(i+1),root) };friends.Add(actor);
                var body=PuzzleUI.Rect("Jelly body",actor.root);PuzzleUI.Fill(body);body.pivot=new Vector2(.5f,.5f);actor.body=body;
                Color color=SkinDecoration.ColorFor(selectedSkin,i%3);
                actor.skin=SkinDecoration.Add(body,selectedSkin,i%3);
                var ring=PuzzleUI.Glyph(actor.root,GlyphKind.Ring,color);PuzzleUI.Fill(ring.rectTransform,-2);actor.ring=ring;actor.halo=ring.rectTransform;ring.color=new Color(color.r,color.g,color.b,0);
            }
        }
        private Image Stone(string name,Cell position,int size)
        {
            var stone=PuzzleUI.Panel(name,root,new Color(.56f,.44f,.68f));AddSlot(stone.rectTransform,position,size);stone.pixelsPerUnitMultiplier=3;
            var top=PuzzleUI.Panel("Soft stone cap",stone.transform,new Color(.76f,.68f,.87f));PuzzleUI.Fill(top.rectTransform,1);top.rectTransform.offsetMin=new Vector2(1,3);top.pixelsPerUnitMultiplier=3;
            var lip=PuzzleUI.Panel("Porcelain rim light",top.transform,new Color(1,1,1,.40f));Region(lip.rectTransform,.17f,.86f,.83f,.92f);
            if((position.x*3+position.y)%7==0)
            {
                var freckle=PuzzleUI.Panel("Stone freckle",top.transform,new Color(1,1,1,.25f));Region(freckle.rectTransform,.24f,.66f,.44f,.76f);
            }
            return stone;
        }
        private void AddSlot(RectTransform rect,Cell c,int size) { slots.Add(rect);slotCells.Add(c);slotSizes.Add(size); }
        public void Layout(float availableWidth,float availableHeight,float centerX,float centerY)
        {
            if(level==null)return;
            if(skinEffects)skinEffects.Clear();
            cell=Mathf.Min(availableWidth/level.width,availableHeight/level.height);
            PuzzleUI.Box(root,centerX-level.width*cell/2,centerY-level.height*cell/2,level.width*cell,level.height*cell);
            for(int i=0;i<slots.Count;i++)PuzzleUI.Box(slots[i],slotCells[i].x*cell+1,slotCells[i].y*cell+1,slotSizes[i]*cell-2,slotSizes[i]*cell-2);
        }
        public bool InspectAt(Vector2 uv)
        {
            if(level==null||lastSession==null||!lastSession.Ready)return false;
            var p=new Vector2(uv.x*level.width,uv.y*level.height);
            if(level.HasPortals)for(int i=0;i<level.portals.Length;i++)for(int side=0;side<2;side++)
            {
                var portal=level.portals[i];var origin=portal.Origin(side);
                if(new Rect(origin.x,origin.y,portal.size,portal.size).Contains(p))
                {selectedGate=i*2+side;previewTime=4;return true;}
            }
            if(level.HasResizers)foreach(var pad in level.resizers)
                if(new Rect(pad.x,pad.y,2,2).Contains(p))
                {OnHint?.Invoke(pad.targetSize==2?"확대대: 왼쪽 아래에 멈추면 2×2로 커져요.":"축소대: 왼쪽 아래에 멈추면 1×1로 작아져요.");return true;}
            return false;
        }
        public void BeginTilt(PuzzleSession session)
        {
            direction=session.Direction;celebration=-1;selectedGate=-1;previewTime=0;
            for(int i=0;i<friends.Count;i++)
            {
                var f=friends[i];f.arrivalAge=10;f.destination=session.Target[i];f.travelling=session.Moving&&(!f.destination.Equals(session.Stable.positions[i])||session.Plan.resized[i]);f.impact=10;
            }
        }
        public void Celebrate() { celebration=0; }
        public void CancelMotion()
        {
            celebration=-1;selectedGate=-1;previewTime=0;
            if(skinEffects)skinEffects.Clear();
            foreach(var b in blinkVisuals)b.initialized=false;
            foreach(var f in friends){f.travelling=false;f.impact=10;f.arrivalAge=10;f.motionInitialized=false;f.skin.Motion(false,10,Vector2.zero);}
        }
        // Scales stay inside the logical footprint, including contact with adjacent blocks.
        public static Vector2 JellyScale(float impact,bool travelling,bool vertical)
        {
            float squash=impact<.32f?Mathf.Sin(Mathf.Clamp01(impact/.32f)*Mathf.PI)*.18f:0;
            return vertical?new Vector2(travelling?.91f:1,1-squash):new Vector2(1-squash,travelling?.91f:1);
        }
        // Pull the jelly to the active threshold while narrowing it; reverse on emergence.
        // The transformed body remains within its contact cell, including beside another friend.
        public static void PortalPose(float openness,int face,out Vector2 scale,out Vector2 offset)
        {
            float normal=Mathf.Clamp01(openness);float tangent=.4f*normal+.6f*Mathf.Sqrt(normal);
            var axis=TiltSimulator.Axes[face];scale=face%2==0?new Vector2(tangent,normal):new Vector2(normal,tangent);
            offset=new Vector2(-axis.x,-axis.y)*(.5f*(1-normal));
        }
        public void Render(PuzzleSession session,float delta=0,bool reduced=false)
        {
            float dt=session.Paused?0:delta;clock+=dt;if(celebration>=0)celebration+=dt;
            if(skinEffects)skinEffects.Tick(dt,reduced);
            RenderBlinkBlocks(session,dt,reduced);
            lastSession=session;previewTime=Mathf.Max(0,previewTime-dt);if(previewTime==0)selectedGate=-1;
            foreach(var pad in resizePads)
            {
                bool blocked=false,active=false;
                if(session.Plan!=null)for(int i=0;i<friends.Count;i++)if(session.Plan.result[i].Equals(new Cell(pad.definition.x,pad.definition.y)))
                {blocked|=session.Plan.resizeBlocked[i];active|=session.InResizePhase&&session.Plan.resized[i];}
                pad.frame.color=blocked?new Color(.9f,.25f,.25f):Color.Lerp(pad.color,Color.white,active?.6f:0);
            }
            foreach(var link in links)
            {
                bool active=selectedGate>=0&&selectedGate/2==link.pair;
                if(session.InPortalPhase)for(int i=0;i<friends.Count;i++)if(session.Plan.entered[i]>=0&&session.Plan.entered[i]/2==link.pair&&!session.Plan.blocked[i])active=true;
                var portal=level.portals[link.pair];var a=portal.Contact(0);var b=portal.Contact(1);
                Vector2 from=new Vector2(a.x+.5f,a.y+.5f)*cell,to=new Vector2(b.x+.5f,b.y+.5f)*cell;
                Vector2 bend=new Vector2(-(to-from).y,(to-from).x).normalized*cell*1.2f;
                for(int j=0;j<link.dots.Length;j++)
                {
                    var dot=link.dots[j];dot.gameObject.SetActive(active);if(!active)continue;
                    float t=(j+.5f)/link.dots.Length;var pos=Vector2.Lerp(from,to,t)+bend*Mathf.Sin(t*Mathf.PI);
                    float diameter=Mathf.Max(2,cell*.12f);PuzzleUI.Box(dot,pos.x-diameter/2,pos.y-diameter/2,diameter,diameter);
                }
            }
            foreach(var gate in gates)
            {
                bool incoming=false,outgoing=false,blocked=false;
                if(session.InPortalPhase)for(int i=0;i<friends.Count;i++)
                {
                    int entered=session.Plan.entered[i];if(entered<0||entered/2!=gate.index/2)continue;
                    incoming|=entered==gate.index;outgoing|=entered!=gate.index&&!session.Plan.blocked[i];blocked|=session.Plan.blocked[i];
                }
                bool previewExit=selectedGate>=0&&selectedGate/2==gate.index/2&&selectedGate!=gate.index;
                bool previewSource=selectedGate==gate.index;
                float fade=Mathf.SmoothStep(0,1,Mathf.Clamp01(previewTime/.35f));
                float pulse=(incoming||outgoing)&&!reduced?Mathf.Sin(session.PortalProgress*Mathf.PI):0;
                float focus=previewExit?fade:previewSource?.18f*fade:outgoing?1:0;
                float shimmer=reduced?0:(.5f+.5f*Mathf.Sin(clock*2.1f+gate.index*.4f));
                Color light=blocked?PuzzleUI.Accent:gate.color;
                gate.face.rectTransform.localScale=Vector3.one*(1+.06f*pulse);
                gate.opening.color=Color.Lerp(light,Color.white,.35f+.45f*Mathf.Max(pulse,focus));
                gate.aura.color=new Color(light.r,light.g,light.b,.13f+.04f*shimmer+.36f*focus+.1f*pulse);
                gate.glow.color=new Color(light.r,light.g,light.b,.28f+.06f*shimmer+.45f*focus+.12f*pulse);
                PuzzleUI.Fill(gate.aura.rectTransform,-3-5*focus);
                PuzzleUI.Fill(gate.glow.rectTransform,-1.5f-2*focus);
                gate.face.color=blocked?PuzzleUI.Accent:Color.white;
                gate.pool.color=new Color(light.r,light.g,light.b,.32f*focus);
                gate.landing.color=new Color(light.r,light.g,light.b,.85f*focus);
                float ripple=reduced?0:.12f*shimmer*focus;
                gate.pool.rectTransform.localScale=Vector3.one*(1+ripple);
                gate.landing.rectTransform.localScale=Vector3.one*(1+ripple);

            }
            for(int i=0;i<friends.Count;i++)
            {
                var f=friends[i];session.VisualPosition(i,out float x,out float y);
                f.skin.Pose(clock,reduced);
                float size=session.VisualSize(i);
                PuzzleUI.Box(f.root,x*cell,y*cell,size*cell,size*cell);
                if(f.travelling&&!session.InPortalPhase&&!session.InResizePhase&&!(session.Moving&&session.Plan.resized[i])&&Mathf.Abs(x-f.destination.x)<.001f&&Mathf.Abs(y-f.destination.y)<.001f) { f.travelling=false;f.impact=0; }
                f.impact+=dt;
                bool home=Mathf.Abs(x-level.cargo[i].gx)<.001f&&Mathf.Abs(y-level.cargo[i].gy)<.001f&&Mathf.Abs(size-level.GoalSizes[i])<.001f&&!f.travelling;
                // A pass over the goal, a portal entrance, or a pending resize is not an arrival.
                home=home&&!session.Moving;
                f.arrivalAge+=dt;
                if(!session.Moving)
                {
                    if(f.motionInitialized&&home&&!f.atHome)
                    {
                        f.arrivalAge=0;
                    }
                    f.atHome=home;
                }
                if(reduced)f.arrivalAge=10;
                f.motionInitialized=true;
                var motionAxis=TiltSimulator.Axes[direction];
                bool sliding=session.Moving&&!session.InPortalPhase&&!session.InResizePhase&&
                    (Mathf.Abs(x-session.Plan.slide[i].x)+Mathf.Abs(y-session.Plan.slide[i].y)>.001f);
                f.skin.Motion(sliding,f.arrivalAge,new Vector2(motionAxis.x,motionAxis.y));
                Vector2 scale=reduced||session.InResizePhase?Vector2.one:JellyScale(f.impact,f.travelling&&!session.InPortalPhase,direction==0||direction==2);
                float wave=celebration-i*.12f;
                if(!reduced&&wave>=0&&wave<.55f) { float bounce=Mathf.Sin(wave/.55f*Mathf.PI);scale*=1-.09f*bounce; }
                float warp=reduced?1:session.VisualPortalScale(i);
                f.body.anchoredPosition=Vector2.zero;
                if(!reduced&&session.InPortalPhase&&session.Plan.entered[i]>=0&&!session.Plan.blocked[i])
                {
                    int entered=session.Plan.entered[i];var portal=level.portals[entered/2];int side=session.PortalProgress<.5f?entered%2:1-entered%2;
                    PortalPose(warp,portal.Face(side),out var portalScale,out var offset);
                    scale=Vector2.Scale(scale,portalScale);f.body.anchoredPosition=offset*cell*size;
                }
                f.body.localScale=new Vector3(scale.x,scale.y,1);
                // Use the rendered body's centre, including portal offsets and resize interpolation.
                var effectRect=skinEffects.rectTransform;
                var effectCenter=(Vector2)effectRect.InverseTransformPoint(f.body.TransformPoint(f.body.rect.center))-effectRect.rect.min;
                float effectExtent=size*cell*Mathf.Max(scale.x,scale.y);
                if(f.arrivalAge==0&&!reduced&&dt>0)skinEffects.Arrive(selectedSkin,i%3,effectCenter,effectExtent);
                skinEffects.Trail(i,selectedSkin,i%3,effectCenter,effectExtent,sliding,dt,reduced);
                var axis=TiltSimulator.Axes[direction];
                f.skin.Mood(home,!reduced&&f.travelling&&!session.InPortalPhase?new Vector2(axis.x,axis.y):Vector2.zero);
                Color color=SkinDecoration.ColorFor(selectedSkin,i%3);float pulse=!reduced&&f.impact<.42f?1-f.impact/.42f:0;
                f.halo.localScale=Vector3.one*(1+.24f*(1-pulse));f.ring.color=new Color(color.r,color.g,color.b,pulse*.65f);
            }
            skinEffects.transform.parent.SetAsLastSibling();
        }
    }
}

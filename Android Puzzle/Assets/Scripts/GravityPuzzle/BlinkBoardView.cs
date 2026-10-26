using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzle.Gravity
{
    // A dashed footprint remains visible while open; phase dots also distinguish the two colours.
    public sealed class BlinkBlockGraphic : MaskableGraphic
    {
        public int phase;
        public float reveal;
        public bool deferred;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var m=new SkinMesh(vh,GetPixelAdjustedRect(),true);
            Color tint=phase==0?new Color(.88f,.47f,.31f):new Color(.22f,.62f,.57f);
            var pale=Color.Lerp(tint,Color.white,.77f);pale.a=.28f;
            m.Round(.035f,.035f,.93f,.93f,.14f,pale);
            var dash=Color.Lerp(tint,PuzzleUI.Ink,.15f);dash.a=.65f*(1-reveal);
            for(int i=0;i<3;i++)
            {
                float a=.23f+i*.20f,b=a+.10f;
                m.Line(a,.06f,b,.06f,.035f,dash);m.Line(a,.94f,b,.94f,.035f,dash);
                m.Line(.06f,a,.06f,b,.035f,dash);m.Line(.94f,a,.94f,b,.035f,dash);
            }
            m.Arc(.20f,.20f,.14f,.14f,180,270,.035f,dash);m.Arc(.80f,.20f,.14f,.14f,270,360,.035f,dash);
            m.Arc(.80f,.80f,.14f,.14f,0,90,.035f,dash);m.Arc(.20f,.80f,.14f,.14f,90,180,.035f,dash);
            float inset=.035f+(1-reveal)*.10f;
            var baseColor=Color.Lerp(tint,PuzzleUI.Ink,.20f);baseColor.a=reveal;
            m.Round(inset,inset,1-2*inset,1-2*inset,.14f,baseColor);
            var cap=Color.Lerp(tint,Color.white,.25f);cap.a=reveal;
            m.Round(inset+.025f,inset+.085f,1-2*inset-.05f,1-2*inset-.11f,.12f,cap);
            m.Line(.26f,.84f,.71f,.84f,.025f,new Color(1,1,1,.65f*reveal));
            var ink=Color.Lerp(tint,PuzzleUI.Ink,.45f);ink.a=.40f+.60f*reveal;
            for(int i=0;i<=phase;i++)m.Oval(.5f+(i-phase*.5f)*.20f,.51f,.052f,.052f,ink);
            if(deferred)m.Arc(.50f,.51f,.23f,.23f,25,325,.025f,ink);
        }
    }

    public sealed partial class BoardView
    {
        private sealed class BlinkVisual
        { public BlinkDefinition definition;public BlinkBlockGraphic graphic;public bool initialized; }
        private readonly List<BlinkVisual> blinkVisuals=new List<BlinkVisual>();
        private void BuildBlinkBlocks()
        {
            if(!level.HasBlinkBlocks)return;
            foreach(var definition in level.blinkBlocks)
            {
                var r=PuzzleUI.Rect("Hide and seek block",root);AddSlot(r,new Cell(definition.x,definition.y),1);
                var graphic=r.gameObject.AddComponent<BlinkBlockGraphic>();graphic.phase=definition.phase;graphic.raycastTarget=true;
                var visual=new BlinkVisual{definition=definition,graphic=graphic};blinkVisuals.Add(visual);
                var tap=r.gameObject.AddComponent<PuzzleTap>();tap.Epoch=()=>generation;
                tap.CanUse=()=>lastSession!=null&&!lastSession.Paused&&!lastSession.Moving;
                tap.Activate=()=>
                {
                    var state=lastSession.Stable;bool closed=level.BlinkSolid(definition,state.positions,state.sizes,state.moves);
                    string name=definition.phase==0?"● 살구":"●● 민트";
                    OnHint?.Invoke(name+(closed?" 닫힘 · 한 턴 뒤 열려요":definition.phase==(state.moves&1)?" 대기 중 · 친구가 있어 열린 채 기다려요":" 열림 · 한 턴 뒤 닫혀요"));
                };
            }
        }
        private void RenderBlinkBlocks(PuzzleSession session,float dt,bool reduced)
        {
            foreach(var visual in blinkVisuals)
            {
                var state=session.Stable;var d=visual.definition;var g=visual.graphic;
                bool solid=level.BlinkSolid(d,state.positions,state.sizes,state.moves);
                bool deferred=d.phase==(state.moves&1)&&!solid;
                float value=!visual.initialized||reduced?(solid?1:0):Mathf.MoveTowards(g.reveal,solid?1:0,dt/.20f);
                if(!visual.initialized||g.reveal!=value||g.deferred!=deferred)
                {g.reveal=value;g.deferred=deferred;g.SetVerticesDirty();}
                visual.initialized=true;
            }
        }
    }
}

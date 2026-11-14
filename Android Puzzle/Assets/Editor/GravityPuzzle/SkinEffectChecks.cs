using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Puzzle.Gravity;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace Puzzle.Editor
{
    public static class SkinEffectChecks
    {
        [MenuItem("Puzzle/Prototype/Check Skin Effects (No Web Build)")]
        public static void Run()
        {
            Require(SkinEffectMotifs.Descriptions.Length==PuzzleCollection.Names.Length,"all forty skins described");
            var signatures=new HashSet<string>();
            for(int id=0;id<40;id++)for(int mode=0;mode<2;mode++)
            {
                using(var vh=new VertexHelper())
                {
                    SkinEffectMotifs.Draw(new SkinMesh(vh,new Rect(0,0,100,100),false,16),id,mode==1,Color.white);
                    Require(vh.currentVertCount>0,"every skin has both motifs");
                    var signature=new System.Text.StringBuilder();var v=new UIVertex();
                    for(int i=0;i<vh.currentVertCount;i++){vh.PopulateUIVertex(ref v,i);Require(float.IsFinite(v.position.x)&&float.IsFinite(v.position.y),"finite motif geometry");signature.Append(v.position.ToString("F3"));}
                    Require(signatures.Add(signature.ToString()),"distinct geometry for all 80 motion/arrival motifs: "+id+" / "+mode);
                }
            }
            var root=PuzzleUI.Rect("Effect integration test",null);
            try
            {
                var level=new PuzzleDefinition{room="effect-test",width=7,height=3,walls=new Cell[0],cargo=new[]{new CargoDefinition{x=0,y=1,gx=6,gy=1,size=1}},route=new[]{3}};
                var run=new PuzzleSession(level);var board=new BoardView(root);board.ApplySkin(17);board.Build(level);board.Layout(420,180,210,90);board.Render(run);
                var effects=root.GetComponentInChildren<SkinTravelEffects>();Require(effects.ParticleCount==0,"load has no false celebration");
                run.Command(3);board.BeginTilt(run);
                for(int i=0;i<4;i++){run.Tick(.04f);board.Render(run,.04f);}
                Require(effects.ParticleCount>0,"real slide emits trail");
                int paused=effects.ParticleCount;run.SetPause(PauseReason.Menu,true);board.Render(run,5);Require(effects.ParticleCount==paused,"pause freezes particles");run.SetPause(PauseReason.Menu,false);
                run.Tick(100);board.Render(run,.01f);int arrived=effects.ParticleCount;Require(arrived>paused,"settled goal emits burst");
                board.Render(run,0);Require(effects.ParticleCount==arrived,"settled goal does not emit repeatedly");
                board.Render(run,2);Require(effects.ParticleCount==0,"effects expire");
                effects.Arrive(17,0,new Vector2(100,100),50);run.Undo();board.CancelMotion();Require(effects.ParticleCount==0,"undo clears effects");
                effects.Arrive(17,0,new Vector2(100,100),50);board.ApplySkin(5);Require(effects.ParticleCount==0,"equipping clears old concept");
                effects.Arrive(17,0,new Vector2(100,100),50);board.Render(run,.1f,true);Require(effects.ParticleCount==0,"fast mode suppresses particles");
                effects.Trail(0,2,0,Vector2.zero,50,true,.1f,false);effects.Trail(0,2,0,new Vector2(400,0),50,true,.1f,false);Require(effects.ParticleCount==0,"teleport discontinuity leaves no cross-board line");
                effects.Trail(0,2,0,new Vector2(400,0),50,false,.1f,false);effects.Trail(0,2,0,new Vector2(20,0),50,true,.1f,false);Require(effects.ParticleCount==0,"portal exit starts a new emitter");
                for(int j=0;j<30;j++)effects.Arrive(17,j%3,new Vector2(100,100),50);
                Require(effects.ParticleCount<=SkinTravelEffects.Capacity,"particle cap");
                using(var vh=new VertexHelper())
                {
                    typeof(SkinTravelEffects).GetMethod("OnPopulateMesh",BindingFlags.NonPublic|BindingFlags.Instance,null,new[]{typeof(VertexHelper)},null).Invoke(effects,new object[]{vh});
                    Require(vh.currentVertCount<60000,"UI mesh stays below vertex limit");
                }
            }
            finally{Object.DestroyImmediate(root.gameObject);}
            GravityPrototypeChecks.ValidateStarsOnly();
            Export(0);Export(20);
            Debug.Log("PASS skin effects: 80 unique motifs, slide, arrival once, pause, expiry, undo, skin change, fast mode, portal discontinuity and bounded particles/mesh. Two Unity previews; no web build.");
        }
        static void Export(int first)
        {
            const int width=1200,height=1450;
            PuzzleUI.Font=Resources.Load<Font>("GravityPuzzle/NanumGothic-Regular");
            var root=new GameObject("Skin effects sheet",typeof(RectTransform),typeof(Canvas));var cameraObject=new GameObject("Effects camera",typeof(Camera));
            var target=new RenderTexture(width*2,height*2,24){antiAliasing=4};var old=RenderTexture.active;
            try
            {
                var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;
                root.GetComponent<RectTransform>().sizeDelta=new Vector2(width,height);
                var cam=cameraObject.GetComponent<Camera>();cam.orthographic=true;cam.orthographicSize=height/2f;cam.transform.position=new Vector3(0,0,-10);cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=PuzzleUI.Cream;cam.targetTexture=target;cam.cullingMask=1<<5;cam.nearClipPlane=.1f;cam.farClipPlane=30;canvas.worldCamera=cam;
                var title=PuzzleUI.Label("Title",root.transform,"데굴데굴 · 스킨별 이동과 도착 "+(first+1)+"–"+(first+20),32,PuzzleUI.Ink);title.fontStyle=FontStyle.Bold;PuzzleUI.Box(title.rectTransform,20,1370,1160,60);
                var subtitle=PuzzleUI.Label("Legend",root.transform,"왼쪽은 이동 흔적 · 오른쪽은 목적지 도착 효과",19,PuzzleUI.Muted);PuzzleUI.Box(subtitle.rectTransform,20,1330,1160,35);
                for(int j=0;j<20;j++)
                {
                    int id=first+j;
                    var card=PuzzleUI.Panel("Effect "+id,root.transform,j%2==0?new Color(.93f,.89f,.97f):new Color(.97f,.92f,.86f));PuzzleUI.Box(card.rectTransform,15+(j%4)*297,20+(4-j/4)*260,282,248);
                    var titleLabel=PuzzleUI.Label("Skin",card.transform,PuzzleCollection.Names[id],20,PuzzleUI.Ink);titleLabel.fontStyle=FontStyle.Bold;PuzzleUI.Box(titleLabel.rectTransform,10,211,262,30);
                    var label=PuzzleUI.Label("Concept",card.transform,SkinEffectMotifs.Descriptions[id],13,PuzzleUI.Muted);PuzzleUI.Box(label.rectTransform,7,174,268,32);
                    var layer=PuzzleUI.Rect("Effects",card.transform);PuzzleUI.Fill(layer);var fx=layer.gameObject.AddComponent<SkinTravelEffects>();fx.raycastTarget=false;
                    for(int step=0;step<10;step++){fx.Tick(.036f,false);fx.Trail(0,id,0,new Vector2(18+step*8,95),58,true,.036f,false);}
                    fx.Arrive(id,1,new Vector2(205,95),58);fx.Tick(.28f,false);
                    var moving=SkinDecoration.Portrait(card.transform,id,0,out var movingSkin);PuzzleUI.Box(moving,61,66,58,58);movingSkin.Motion(true,10,Vector2.right);movingSkin.Pose(.9f,false);
                    var home=SkinDecoration.Portrait(card.transform,id,1,out var homeSkin);PuzzleUI.Box(home,176,66,58,58);homeSkin.Motion(false,.28f,Vector2.zero);homeSkin.Mood(true,Vector2.zero);homeSkin.Pose(.9f,false);
                    layer.SetAsLastSibling();
                    var captions=PuzzleUI.Label("States",card.transform,"이동                    도착",13,PuzzleUI.Muted);PuzzleUI.Box(captions.rectTransform,10,18,262,25);
                }
                foreach(var t in root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=5;
                Canvas.ForceUpdateCanvases();cam.Render();RenderTexture.active=target;
                var image=new Texture2D(target.width,target.height,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,target.width,target.height),0,0);image.Apply();Directory.CreateDirectory("Logs");File.WriteAllBytes("Logs/skin-effects-"+(first+1)+"-"+(first+20)+".png",image.EncodeToPNG());Object.DestroyImmediate(image);
            }
            finally{RenderTexture.active=old;Object.DestroyImmediate(root);Object.DestroyImmediate(cameraObject);target.Release();Object.DestroyImmediate(target);}
        }
        static void Require(bool valid,string message){if(!valid)throw new Exception(message);}
    }
}

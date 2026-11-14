using System;
using System.IO;
using System.Reflection;
using System.Linq;
using Puzzle.Gravity;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace Puzzle.Editor
{
    public static class DrawPresentationChecks
    {
        const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
        [MenuItem("Puzzle/Prototype/Check Candy Draw (No Web Build)")]
        public static void Run()
        {
            for(int cue=0;cue<6;cue++)
            {
                var clip=SkinDrawAudio.Make(cue);var data=new float[clip.samples];clip.GetData(data,0);
                float peak=0;foreach(float value in data){Require(!float.IsNaN(value)&&!float.IsInfinity(value),"finite audio");peak=Mathf.Max(peak,Mathf.Abs(value));}
                Require(peak>.02f&&peak<.65f,"audible cues with headroom");Require(Mathf.Abs(data[0])<.001f&&Mathf.Abs(data[data.Length-1])<.001f,"click-free sound edges");
                Object.DestroyImmediate(clip);
            }
            PuzzleUI.Font=Resources.Load<Font>("GravityPuzzle/NanumGothic-Regular");
            var root=new GameObject("Draw isolated preview",typeof(RectTransform),typeof(Canvas));
            var host=new GameObject("Draw preview controller");var app=host.AddComponent<GravityPuzzleApp>();app.enabled=false;
            var cameraObject=new GameObject("Draw preview camera",typeof(Camera));
            var target=new RenderTexture(780,1688,24){antiAliasing=4};var previous=RenderTexture.active;
            void Set(string name,object value)=>typeof(GravityPuzzleApp).GetField(name,Flags).SetValue(app,value);
            object Get(string name)=>typeof(GravityPuzzleApp).GetField(name,Flags).GetValue(app);
            void Call(string name,params object[] args)=>typeof(GravityPuzzleApp).GetMethod(name,Flags).Invoke(app,args);
            try
            {
                var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;
                var rect=root.GetComponent<RectTransform>();rect.sizeDelta=new Vector2(390,844);
                var cam=cameraObject.GetComponent<Camera>();cam.orthographic=true;cam.orthographicSize=422;cam.transform.position=new Vector3(0,0,-10);cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=PuzzleUI.Cream;cam.targetTexture=target;cam.cullingMask=1<<5;cam.nearClipPlane=.1f;cam.farClipPlane=30;canvas.worldCamera=cam;
                var collection=new PuzzleCollection{wallet=12,owned=new[]{0,3}};
                Set("safe",rect);Set("collection",collection);Set("sound",false);
                Call("OpenSkinDraw",3);
                var frame=root.GetComponentInChildren<AspectCoverImage>();
                Require(frame&&frame.texture,"aspect-preserving frame bound");
                var originalSize=frame.rectTransform.sizeDelta;
                foreach(var size in new[]{new Vector2(288,690),new Vector2(360,750),new Vector2(800,400)})
                {
                    frame.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,size.x);
                    frame.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,size.y);
                    var uv=frame.SourceUV;
                    Require(Mathf.Abs(size.x/(uv.width*frame.texture.width)-size.y/(uv.height*frame.texture.height))<.001f,"source pixels keep equal x/y scale");
                    Require(uv.xMin>=0&&uv.yMin>=0&&uv.xMax<=1&&uv.yMax<=1,"crop stays inside source");
                }
                frame.rectTransform.sizeDelta=originalSize;
                Require(!((RectTransform)Get("drawActions")).gameObject.activeSelf,"actions hidden before reveal");
                void Capture(string file,float time,bool reveal=true)
                {
                    if(reveal){Set("drawTime",time);Call("RenderSkinDraw");}
                    foreach(var t in root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=5;
                    Canvas.ForceUpdateCanvases();cam.Render();RenderTexture.active=target;
                    var png=new Texture2D(target.width,target.height,TextureFormat.RGBA32,false);png.ReadPixels(new Rect(0,0,target.width,target.height),0,0);png.Apply();Directory.CreateDirectory("Logs");File.WriteAllBytes("Logs/"+file,png.EncodeToPNG());Object.DestroyImmediate(png);
                }
                Capture("candy-draw-capsule.png",.9f);
                Call("TickSkinDraw",0f);Require((float)Get("drawTime")==.9f,"inactive presentation does not advance");
                Capture("candy-draw-burst.png",2.35f);
                Call("FinishSkinDraw");Call("FinishSkinDraw");
                Require((bool)Get("drawReady")&&((RectTransform)Get("drawActions")).gameObject.activeSelf&&!((RectTransform)Get("drawSkip")).gameObject.activeSelf,"skip finishes once and exposes actions");
                Require(collection.wallet==12&&collection.owned.Length==2,"presentation never spends or grants again");
                Capture("candy-draw-reward.png",3.25f);
                Set("reduceMotion",true);Call("OpenSkinDraw",3);
                Require(!(bool)Get("drawReady")&&((RectTransform)Get("drawSkip")).gameObject.activeSelf,"fast play starts the full reveal with manual skip available");
                Call("TickCollection",.4f);
                Require(Mathf.Approximately((float)Get("drawTime"),.4f)&&!(bool)Get("drawReady"),"fast play advances the reveal without auto-skipping");
                Require(((RectTransform[])Get("gatheringStars"))[0].gameObject.activeSelf&&!((DrawStageArt)Get("drawArt")).still,"fast play keeps gathering stars and animated stage");
                Require((int)Get("drawNotes")==1,"fast play schedules the gathering sound cue");
                Call("TickCollection",0f);Require(Mathf.Approximately((float)Get("drawTime"),.4f),"paused fast-play reveal stays paused");
                Call("TickCollection",1.95f);
                Require(!(bool)Get("drawReady")&&((RectTransform)Get("drawReward")).gameObject.activeSelf&&((CanvasGroup)Get("capsuleFade")).alpha<.2f,"fast play opens the capsule and reveals friends before finishing");
                foreach(var star in (RectTransform[])Get("burstStars"))Require(star.gameObject.activeSelf,"fast play keeps the star burst");
                Capture("candy-draw-fast-mode.png",2.35f);
                Call("TickCollection",.9f);
                Require((bool)Get("drawReady")&&((RectTransform)Get("drawActions")).gameObject.activeSelf&&(int)Get("drawNotes")==5,"fast play completes naturally after all reveal cues");
                Call("OpenSkinDraw",3);Call("TickCollection",.4f);Call("FinishSkinDraw");Call("FinishSkinDraw");
                Require((bool)Get("drawReady")&&!((RectTransform)Get("drawSkip")).gameObject.activeSelf,"manual skip still finishes once in fast play");
                Require(collection.wallet==12&&collection.owned.Length==2&&(bool)Get("reduceMotion"),"reveal preserves inventory and the fast-play preference");
                var batchCollection=new PuzzleCollection{wallet=12,owned=Enumerable.Range(0,11).ToArray()};Set("collection",batchCollection);
                Call("OpenSkinBatch",(object)Enumerable.Range(1,10).ToArray());
                Require(!(bool)Get("drawReady")&&((RectTransform)Get("capsuleRoot")).gameObject.activeSelf,"ten rewards start with one capsule even in fast play");
                Call("SelectBatchReward",6);Require((int)Get("drawnSkin")==1,"selection waits until reveal completes");
                Call("TickCollection",2.05f);
                var cards=(RectTransform[])Get("batchCards");
                Require(cards.Count(c=>c.gameObject.activeSelf)==1&&!((RectTransform)Get("drawReward")).gameObject.activeSelf,"first batch friend appears without the single reward overlay");
                Call("TickCollection",.5f);Require(cards.Count(c=>c.gameObject.activeSelf)==4,"batch friends arrive in sequence");
                Capture("candy-draw-ten-stream.png",2.55f);
                Call("TickCollection",2f);
                Require((bool)Get("drawReady")&&cards.All(c=>c.gameObject.activeSelf)&&((CanvasGroup[])Get("batchFades")).All(c=>c.alpha==1),"ten rewards all settle before actions unlock");
                Call("SelectBatchReward",6);Require((int)Get("drawnSkin")==6,"any revealed friend can be selected for equip");
                Call("SelectBatchReward",39);Require((int)Get("drawnSkin")==6,"unrelated skin cannot be selected from results");
                Capture("candy-draw-ten-results.png",4.6f);
                Call("OpenSkinBatch",(object)Enumerable.Range(1,10).ToArray());Call("FinishSkinDraw");Call("FinishSkinDraw");
                Require((bool)Get("drawReady")&&((RectTransform[])Get("batchCards")).All(c=>c.gameObject.activeSelf),"manual skip reveals all ten once");
                Require(batchCollection.wallet==12&&batchCollection.owned.Length==11,"ten-reward presentation never spends or grants again");
                Call("OpenSkinDraw",3);Require(((int[])Get("drawnBatch")).Length==0,"single draw clears batch state");
                Set("collection",collection);
                Object.DestroyImmediate(((RectTransform)Get("dialog")).gameObject);Set("dialog",null);
                var level=JsonUtility.FromJson<LayoutDocument>(Resources.Load<TextAsset>("GravityPuzzle/layouts").text).layouts[0];
                Set("session",new PuzzleSession(level));Set("reduceMotion",false);Call("OpenCollection",null,false);Call("TickCollection",.2f);
                Capture("candy-draw-wardrobe.png",0,false);
                Debug.Log("PASS candy draw: audio, pause, skip, inventory; full reveal in fast play; ten sequential rewards, result selection and single/batch isolation. Seven Unity renders exported. No web build.");
            }
            finally
            {
                var clips=Get("drawClips") as AudioClip[];if(clips!=null)foreach(var clip in clips)if(clip)Object.DestroyImmediate(clip);Set("drawClips",null);
                RenderTexture.active=previous;Object.DestroyImmediate(host);Object.DestroyImmediate(root);Object.DestroyImmediate(cameraObject);target.Release();Object.DestroyImmediate(target);
            }
        }
        static void Require(bool ok,string message){if(!ok)throw new Exception(message);}
    }
}

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Puzzle.Gravity;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace Puzzle.Editor
{
    public static class ReleasePolishChecks
    {
        const BindingFlags Flags=BindingFlags.NonPublic|BindingFlags.Instance;
        static readonly string Output=Path.GetFullPath("../Releases/crazygames/2026-11-04-v1.4.0-sdk/qa");
        static void Check(bool ok,string label){if(!ok)throw new Exception(label);}
        public static void Build()
        {
            CrazyPortalChecks.Run();LocalizationChecks.Run();
            PuzzleLanguage.Set("ko",false);CampaignExpansionChecks.Run();PuzzleLanguage.Set("en",false);
            Run();IphoneTestSetup.BuildGame();
        }
        public static void Run()
        {
            Directory.CreateDirectory(Output);
            using(var rig=new CaptureRig(780,1688))
            {
                rig.Call("StartFromHome");rig.Step();
                Check(rig.Get<bool>("coaching"),"First stage has a playable introduction");
                Check(rig.Get<HintWallet>("hintWallet").balance==3,"Introduction does not consume hints");
                rig.Capture("phone-first-roll.png");
                foreach(int d in rig.Get<PuzzleSession>("session").Level.route)
                {rig.Call("Tilt",d);for(int n=0;n<60;n++)rig.Step();}
                Check(rig.Get<PuzzleSession>("session").Solved,"Introduction reaches the real goal");
                Check(rig.Get<HintWallet>("hintWallet").balance==3,"All three coached moves are free");
                Check(!rig.Get<bool>("coaching"),"Completion dismisses the introduction");
                Check(rig.Get<ProgressSaveService>("saves").Load().seenLessons.Contains("intro-gravity-v2"),"Introduction completion is saved");
                rig.Capture("phone-clear.png");
                rig.Call("LoadLevel",0,null,true);rig.Step();
                Check(!rig.Get<bool>("coaching"),"Replay does not repeat the introduction");
            }
            foreach(var spec in new[]{new[]{640,1688},new[]{726,668},new[]{1920,1080}})
            using(var rig=new CaptureRig(spec[0],spec[1]))
            {
                rig.Call("StartFromHome");rig.Step();
                rig.Capture(spec[0]==640?"phone-narrow.png":"desktop-first-roll.png");
                var safe=rig.Get<RectTransform>("safe");var layout=PuzzlePlayLayout.For(safe.rect.width,safe.rect.height);
                Check(layout.left>=0&&layout.left+layout.width<=safe.rect.width+.1f,"Layout fits safe area");
                Check(layout.wide==(spec[0]==1920),"Device-appropriate presentation");
                var board=rig.Get<RectTransform>("boardRoot");var level=rig.Get<PuzzleSession>("session").Level;
                Check(Mathf.Abs(board.rect.width/level.width-board.rect.height/level.height)<.001f,"Square cells: board never stretches one axis");
                var tools=rig.Get<RectTransform>("drawingTools");
                Check(tools.gameObject.activeInHierarchy&&tools.childCount==6,"Four colors, eraser and clear are always visible, without a draw toggle");
                float toolTop=tools.anchoredPosition.y+tools.rect.height*tools.localScale.y;
                Check(toolTop+6<board.anchoredPosition.y-17,"Drawing tools clear both the board and its shadow");
                rig.Capture("board-ratio-"+spec[0]+"x"+spec[1]+".png");
                rig.Call("EndCoach",true);
                if(spec[0]==726)Check(board.rect.width>500,"Medium-width window uses spare space for a taller board");
                rig.Call("SelectDrawingTool",2,false);
                var drawing=rig.Get<BoardDrawing>("drawing");
                Check(drawing.Editing&&drawing.ColorIndex==2&&!drawing.Erasing,"Color selection is immediately ready to draw");
                drawing.BeginStroke();drawing.Mark(new Vector2(.2f,.4f),new Vector2(.7f,.6f));drawing.EndStroke();
                rig.Call("Tilt",2);for(int n=0;n<60;n++)rig.Step();
                Check(rig.Get<PuzzleSession>("session").Moves==1&&drawing.Lines.Count>0,"Movement remains usable with visible ink");
                Check(drawing.UndoStroke()&&drawing.Lines.Count==0,"Drawing undo does not undo movement");
                rig.Call("SelectDrawingTool",2,true);Check(drawing.Erasing,"Eraser selection");
                rig.Call("SelectDrawingTool",1,false);
                foreach(string language in new[]{"en","ko","zh","ja"})
                {
                    PuzzleLanguage.Set(language,false);rig.Step();
                    rig.Capture((layout.wide?"desktop-":"narrow-")+language+".png");
                }
            }
            Debug.Log("RELEASE POLISH PASS: first-play guide, hint isolation, real completion, saved dismissal, repeat play, narrow phone, landscape and four-language captures.");
        }
        public static void BuildLayout(){CrazyPortalChecks.Run();LocalizationChecks.Run();DrawingUndoChecks.Run();Run();IphoneTestSetup.BuildGame();}
        public static void BuildQuick()
        {
            CrazyPortalChecks.Run();LocalizationChecks.Run();DrawingUndoChecks.Run();
            PuzzleLanguage.Set("ko",false);CampaignExpansionChecks.Run();PuzzleLanguage.Set("en",false);
            Run();RunQuick();IphoneTestSetup.BuildGame();
        }
        public static void RunQuick()
        {
            Directory.CreateDirectory(Output);
            foreach(var size in new[]{new[]{640,1688},new[]{726,668},new[]{1920,1080}})
            {
                string save=Path.Combine(Output,"quick-saves",Guid.NewGuid().ToString("N"));
                using(var rig=new CaptureRig(size[0],size[1],true,save))
                {
                    rig.Step();
                    Check(rig.Get<string>("modal")==""&&rig.Get<RectTransform>("home")==null,"Quick boot opens playable board without home or modal");
                    Check(rig.Get<PuzzleSession>("session").Ready,"First input is immediately available");
                    Check(!rig.Get<PuzzleTap>("levelButton").gameObject.activeSelf&&!rig.Get<PuzzleTap>("resetButton").gameObject.activeSelf,"Secondary actions are tucked into menu");
                    var board=rig.Get<RectTransform>("boardRoot");var tools=rig.Get<RectTransform>("drawingTools");var level=rig.Get<PuzzleSession>("session").Level;
                    Check(Mathf.Abs(board.rect.width/level.width-board.rect.height/level.height)<.001f,"Quick board cells remain square");
                    Check(tools.anchoredPosition.y+tools.rect.height*tools.localScale.y+6<board.anchoredPosition.y-17,"Quick tools clear board shadow");
                    rig.Capture("quick-"+size[0]+"x"+size[1]+".png");
                    foreach(int d in level.route){rig.Call("Tilt",d);for(int f=0;f<90;f++)rig.Step();}
                    Check(rig.Get<string>("modal")=="solved","Actual route reaches compact result");
                    Check(rig.Get<PuzzleCollection>("collection").wallet==3,"Clear awards exactly three stars");
                    Check(rig.Get<int>("pendingWalletStars")==0,"Accelerated star flight settles displayed wallet");
                    Check(rig.Get<RectTransform>("dialog").GetComponentsInChildren<PuzzleTap>().Length==2,"Result has only next and replay");
                    rig.Capture("quick-clear-"+size[0]+".png");
                }
                using(var resumed=new CaptureRig(size[0],size[1],true,save))
                {
                    Check(resumed.Get<int>("levelIndex")==1&&resumed.Get<string>("modal")=="","Completed saved level advances directly on return");
                    resumed.Call("OpenPause");resumed.Call("OpenCollection",null,false);resumed.Call("CloseCollection");
                    Check(resumed.Get<string>("modal")=="pause"&&resumed.Get<RectTransform>("home")==null,"Wardrobe returns to compact menu");
                    resumed.Call("Resume");resumed.Call("Tilt",3);for(int n=0;n<45;n++)resumed.Step();
                }
                using(var resumed=new CaptureRig(size[0],size[1],true,save))
                {
                    Check(resumed.Get<PuzzleSession>("session").Moves==1&&resumed.Get<string>("modal")=="","In-progress move resumes without extra clicks");
                    var levels=resumed.Get<PuzzleDefinition[]>("levels");
                    foreach(int theme in new[]{1,2,3})
                    {
                        int index=Array.FindIndex(levels,l=>PuzzleCampaign.Theme(l)==theme);
                        resumed.Call("LoadLevel",index,null,true);
                        Check(resumed.Get<string>("modal")==""&&resumed.Get<PuzzleSession>("session").Ready,"Special mechanic starts with inline instructions");
                    }
                }
            }
            Debug.Log("QUICK PLAY PASS: immediate boot, saved resume/advance, compact result, reward settlement, menu navigation, special stages, square board and tool spacing.");
        }
        public static void Video()
        {
            Directory.CreateDirectory(Output);
            foreach(var spec in new[]{new[]{1920,1080},new[]{1080,1620}})
            using(var rig=new CaptureRig(spec[0],spec[1],true))
            {
                rig.Call("StartFromHome");rig.Call("EndCoach",true);
                var levels=rig.Get<PuzzleDefinition[]>("levels");
                int selected=Array.FindIndex(levels,l=>l.cargo.Length==3&&!l.HasPortals&&l.route.Length<=8);
                Check(selected>=0,"Three-friend gameplay clip");
                var cleared=rig.Get<System.Collections.Generic.HashSet<string>>("completed");
                for(int i=0;i<selected;i++)cleared.Add(levels[i].room);
                rig.Call("LoadLevel",selected,null,true);
                var route=rig.Get<PuzzleSession>("session").Level.route;
                string folder=spec[0]==1920?"frames-landscape":"frames-portrait";
                Directory.CreateDirectory(Path.Combine(Output,folder));
                int move=0;
                for(int frame=0;frame<540;frame++)
                {
                    if(move<route.Length&&frame==30+move*48){rig.Call("Tilt",route[move]);move++;}
                    rig.Step();rig.Capture(folder+"/frame-"+frame.ToString("D4")+".png");
                }
                Check(rig.Get<PuzzleSession>("session").Solved,"Video shows an actual completed stage");
            }
            Debug.Log("POLISH VIDEO PASS: 540 distinct simulation steps per orientation at 30 fps.");
        }
        sealed class CaptureRig:IDisposable
        {
            readonly GameObject host,cameraObject;readonly GravityPuzzleApp app;
            readonly Camera camera;readonly RenderTexture target,previous;readonly Texture2D texture;
            readonly string priorDirectory;
            public CaptureRig(int width,int height,bool quick=false,string saveDirectory=null)
            {
                priorDirectory=ProgressSaveService.PreviewDirectory;
                ProgressSaveService.PreviewDirectory=saveDirectory??Path.Combine(Output,"isolated-saves",Guid.NewGuid().ToString("N"));
                host=new GameObject("Release preview");app=host.AddComponent<GravityPuzzleApp>();app.enabled=false;
                app.PreviewQuickPlay=quick;
                Call("Start");PuzzleLanguage.Set("en",false);
                typeof(GravityPuzzleApp).GetField("sound",Flags).SetValue(app,false);
                var canvas=host.GetComponentInChildren<Canvas>();canvas.GetComponent<CanvasScaler>().enabled=false;
                Get<RectTransform>("safe").GetComponent<Puzzle.UI.SafeAreaFitter>().enabled=false;
                PuzzleUI.Fill(Get<RectTransform>("safe"));
                canvas.renderMode=RenderMode.WorldSpace;
                canvas.scaleFactor=Mathf.Max(1,height/844f);
                var rect=(RectTransform)canvas.transform;rect.sizeDelta=new Vector2(844f*width/height,844);rect.position=Vector3.zero;rect.localScale=Vector3.one;
                cameraObject=new GameObject("Release camera",typeof(Camera));camera=cameraObject.GetComponent<Camera>();
                target=new RenderTexture(width,height,24){antiAliasing=4};previous=RenderTexture.active;
                texture=new Texture2D(width,height,TextureFormat.RGB24,false);
                camera.orthographic=true;camera.orthographicSize=422;camera.transform.position=new Vector3(0,0,-10);
                camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=PuzzleUI.Cream;
                camera.targetTexture=target;camera.nearClipPlane=.1f;camera.farClipPlane=30;camera.cullingMask=1<<5;canvas.worldCamera=camera;
                Call("Layout");
            }
            public T Get<T>(string name)=>(T)typeof(GravityPuzzleApp).GetField(name,Flags).GetValue(app);
            public void Call(string name,params object[] args)=>typeof(GravityPuzzleApp).GetMethod(name,Flags).Invoke(app,args);
            public void Step()=>Call("TickFrame",1f/30,false);
            public void Capture(string name)
            {
                foreach(var t in host.GetComponentsInChildren<Transform>(true))t.gameObject.layer=5;
                Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;
                texture.ReadPixels(new Rect(0,0,target.width,target.height),0,0);texture.Apply();
                File.WriteAllBytes(Path.Combine(Output,name),texture.EncodeToPNG());
            }
            public void Dispose()
            {
                RenderTexture.active=previous;Object.DestroyImmediate(host);Object.DestroyImmediate(cameraObject);
                target.Release();Object.DestroyImmediate(target);Object.DestroyImmediate(texture);
                ProgressSaveService.PreviewDirectory=priorDirectory;
            }
        }
    }
}

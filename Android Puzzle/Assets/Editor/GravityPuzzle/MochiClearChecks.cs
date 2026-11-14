using System;
using System.IO;
using System.Reflection;
using Puzzle.Gravity;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace Puzzle.Editor
{
    public static class MochiClearChecks
    {
        const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
        public static void Run()
        {
            foreach(string name in new[]{"mochi-clear-panel","mochi-reward-star"})
            {
                var importer=(TextureImporter)AssetImporter.GetAtPath("Assets/Resources/GravityPuzzle/Brand/"+name+".png");
                importer.textureType=TextureImporterType.Default;importer.mipmapEnabled=false;
                importer.alphaIsTransparency=true;importer.npotScale=TextureImporterNPOTScale.None;
                importer.maxTextureSize=name.EndsWith("star")?1024:2048;
                importer.textureCompression=TextureImporterCompression.Uncompressed;
                importer.filterMode=FilterMode.Bilinear;importer.wrapMode=TextureWrapMode.Clamp;importer.SaveAndReimport();
            }
            ClearFinaleChecks.Run();WalletFlightChecks.Run();
            CheckRatings();
            Export(3,false);Export(2,false);Export(1,true);
            Debug.Log("PASS mochi clear: one/two/three ratings, painted stars, ordered reveals, finale after wallet, pause, fast mode, no duplicate awards; Unity screen renders exported.");
        }
        static void CheckRatings()
        {
            var root=PuzzleUI.Rect("Clear sequencing",null);root.sizeDelta=new Vector2(360,590);
            try
            {
                for(int rating=1;rating<=3;rating++)
                {
                    int notes=0;var stars=new CompletionStars(root,rating,false,i=>{Require(i==notes,"chimes ordered");notes++;});
                    for(int i=0;i<3;i++)
                    {
                        var star=stars.Star(i).GetComponent<PuffyStar>();
                        Require(star.painted&&star.earned==(i<rating),"only earned stars use painted gold");
                        if(i<rating)Require(star.mainTexture.name=="mochi-reward-star","generated star bound");
                    }
                    stars.Tick(.17f,false);Require(notes==0,"first star waits for entrance");
                    stars.Tick(.02f,false);Require(notes==1,"first star pops alone");
                    var atPause=stars.Star(0).anchoredPosition;stars.Tick(0,false);
                    Require(stars.Star(0).anchoredPosition==atPause,"pause holds star bounce");
                    stars.Tick(.34f,false);Require(notes==Math.Min(2,rating),"second reveal follows");
                    stars.Tick(.34f,false);Require(notes==rating,"third reveal follows only when earned");
                    stars.Tick(2,false);Require(stars.Star(0).localScale==Vector3.one,"bounce settles");
                }
                root.sizeDelta=new Vector2(288,590);
                var narrow=new CompletionStars(root,3,true,null);
                for(int i=0;i<3;i++)
                {
                    var bounds=RectTransformUtility.CalculateRelativeRectTransformBounds(root,narrow.Star(i));
                    Require(bounds.min.x>=root.rect.xMin&&bounds.max.x<=root.rect.xMax,"stars fit a 320px phone result panel");
                }
            }
            finally{Object.DestroyImmediate(root.gameObject);}
        }
        static void Export(int rating,bool fast)
        {
            PuzzleUI.Font=Resources.Load<Font>("GravityPuzzle/NanumGothic-Regular");
            var root=new GameObject("Clear preview",typeof(RectTransform),typeof(Canvas));
            var host=new GameObject("Clear controller");var app=host.AddComponent<GravityPuzzleApp>();app.enabled=false;
            var cameraObject=new GameObject("Clear preview camera",typeof(Camera));
            var target=new RenderTexture(780,1688,24){antiAliasing=4};var old=RenderTexture.active;
            void Set(string name,object value)=>typeof(GravityPuzzleApp).GetField(name,Flags).SetValue(app,value);
            object Get(string name)=>typeof(GravityPuzzleApp).GetField(name,Flags).GetValue(app);
            void Call(string name,params object[] args)=>typeof(GravityPuzzleApp).GetMethod(name,Flags).Invoke(app,args);
            try
            {
                var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;
                var rect=root.GetComponent<RectTransform>();rect.sizeDelta=new Vector2(390,844);
                var cam=cameraObject.GetComponent<Camera>();cam.orthographic=true;cam.orthographicSize=422;
                cam.transform.position=new Vector3(0,0,-10);cam.clearFlags=CameraClearFlags.SolidColor;
                cam.backgroundColor=PuzzleUI.Cream;cam.targetTexture=target;cam.cullingMask=1<<5;cam.nearClipPlane=.1f;cam.farClipPlane=30;canvas.worldCamera=cam;
                var level=JsonUtility.FromJson<LayoutDocument>(Resources.Load<TextAsset>("GravityPuzzle/layouts").text).layouts[0];
                var session=new PuzzleSession(level);
                // Exercise real attempts for all three ratings, without touching player saves.
                if(rating<3)
                {
                    session.Command(level.route[0]);session.Tick(100);session.Undo();
                    if(rating==1)for(int n=0;n<=PuzzleCollection.StarMoveAllowance;n++){session.Command(2);session.Tick(100);}
                }
                foreach(var move in level.route){session.Command(move);session.Tick(100);}
                Require(session.Solved&&PuzzleCollection.Rating(session.Moves,level.route.Length,session.UsedUndo)==rating,"real attempt reaches expected rating");
                Set("safe",rect);Set("session",session);Set("levels",new[]{level,level});Set("levelIndex",0);
                Set("collection",new PuzzleCollection{wallet=rating,selected=13,owned=new[]{0,13}});
                Set("sound",false);Set("reduceMotion",fast);Set("solvedPresented",true);Set("celebrationPending",true);
                Set("earnedStars",rating);Set("pendingWalletStars",rating);
                var badge=PuzzleUI.Panel("Owned stars",rect,new Color(1,.92f,.74f)).rectTransform;PuzzleUI.Box(badge,270,795,100,32);
                var icon=PuzzleUI.Rect("Wallet star",badge);PuzzleUI.Box(icon,7,4,25,25);icon.gameObject.AddComponent<PuffyStar>();
                var count=PuzzleUI.Label("Wallet count",badge,"0",18,PuzzleUI.Ink);PuzzleUI.Box(count.rectTransform,34,0,57,32);
                Set("walletBadge",badge);Set("walletIcon",icon);Set("walletLabel",count);
                Call("Complete");
                var stars=(CompletionStars)Get("completionStars");var flight=(WalletStarFlight)Get("walletFlight");var confetti=(PuzzleCelebration)Get("celebration");
                void Capture(string phase)
                {
                    foreach(var t in root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=5;
                    Canvas.ForceUpdateCanvases();cam.Render();RenderTexture.active=target;
                    var png=new Texture2D(target.width,target.height,TextureFormat.RGBA32,false);
                    png.ReadPixels(new Rect(0,0,target.width,target.height),0,0);png.Apply();Directory.CreateDirectory("Logs");
                    File.WriteAllBytes("Logs/mochi-clear-"+rating+"-"+phase+".png",png.EncodeToPNG());Object.DestroyImmediate(png);
                }
                for(int frame=0;frame<165;frame++)
                {
                    stars.Tick(.025f,fast);flight?.Tick(.025f,fast);confetti?.Tick(.025f,fast);
                    if(frame==15)Capture("first");if(frame==41)Capture("pop");if(frame==119)Capture("finale");
                }
                Capture(fast?"fast":"settled");
                Require(count.text==rating.ToString(),"wallet counter arrives at exact total");
            }
            finally{RenderTexture.active=old;Object.DestroyImmediate(host);Object.DestroyImmediate(root);Object.DestroyImmediate(cameraObject);target.Release();Object.DestroyImmediate(target);}
        }
        public static void ValidateAndBuild(){Run();IphoneTestSetup.BuildGame();}
        static void Require(bool valid,string message){if(!valid)throw new Exception(message);}
    }
}

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Puzzle.Gravity;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace Puzzle.Editor
{
    // Uses isolated in-memory progress. Never writes to a player's save or the community server.
    public static class StageRatingSurfaceChecks
    {
        const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
        public static void Run()
        {
            AuditStageDetailChecks.ValidateAndBuild();
            Export(390,844,false);Export(320,640,true);Export(844,390,false);
            ClearFinaleChecks.Run();WalletFlightChecks.Run();
            Debug.Log("PASS rating surfaces: four independent themes, all grade rules, async tiers, perfect persistence, clear layout, 3 viewport sizes, wallet/finale sequencing. No build or deployment.");
        }
        static void Export(int width,int height,bool large)
        {
            PuzzleUI.Font=Resources.Load<Font>("GravityPuzzle/NanumGothic-Regular");
            var root=new GameObject("Rating preview",typeof(RectTransform),typeof(Canvas));
            var host=new GameObject("Rating controller");var app=host.AddComponent<GravityPuzzleApp>();app.enabled=false;
            var cameraObject=new GameObject("Rating camera",typeof(Camera));
            var target=new RenderTexture(width*2,height*2,24){antiAliasing=4};var old=RenderTexture.active;
            void Set(string name,object value)=>typeof(GravityPuzzleApp).GetField(name,Flags).SetValue(app,value);
            object Get(string name)=>typeof(GravityPuzzleApp).GetField(name,Flags).GetValue(app);
            void Call(string name,params object[] args)=>typeof(GravityPuzzleApp).GetMethods(Flags).Single(m=>m.Name==name&&m.GetParameters().Length==args.Length).Invoke(app,args);
            try
            {
                var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;
                var rect=root.GetComponent<RectTransform>();rect.sizeDelta=new Vector2(width,height);
                var cam=cameraObject.GetComponent<Camera>();cam.orthographic=true;cam.orthographicSize=height/2f;
                cam.transform.position=new Vector3(0,0,-10);cam.clearFlags=CameraClearFlags.SolidColor;
                cam.backgroundColor=PuzzleUI.Cream;cam.targetTexture=target;cam.cullingMask=1<<5;cam.nearClipPlane=.1f;cam.farClipPlane=30;canvas.worldCamera=cam;
                var levels=JsonUtility.FromJson<LayoutDocument>(Resources.Load<TextAsset>("GravityPuzzle/layouts").text).layouts;
                foreach(string name in new[]{"late_levels","resize_levels","blink_levels","expansion_levels"})levels=levels.Concat(JsonUtility.FromJson<LayoutDocument>(Resources.Load<TextAsset>("GravityPuzzle/"+name).text).layouts).ToArray();
                levels=PuzzleCampaign.Order(levels);
                var collection=new PuzzleCollection();
                collection.Award(levels[0].room,levels[0].route.Length,levels[0].route.Length,false,false);
                collection.Award(levels[1].room,levels[1].route.Length*2,levels[1].route.Length,false,true);
                var activity=new PuzzleActivity();activity.For(levels[0].room).moves=123;activity.For(levels[0].room).restarts=7;
                string progressBefore=JsonUtility.ToJson(collection),activityBefore=JsonUtility.ToJson(activity);
                Set("safe",rect);Set("levels",levels);Set("baseLevelCount",16);Set("session",new PuzzleSession(levels[0]));
                Set("collection",collection);Set("activity",activity);Set("sound",false);Set("largeText",large);
                void ClearDialog(){var d=(RectTransform)Get("dialog");if(d)Object.DestroyImmediate(d.gameObject);Set("dialog",null);Set("modal","");}
                void Capture(string name)
                {
                    foreach(var t in root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=5;
                    foreach(var g in root.GetComponentsInChildren<Graphic>(true))
                        if(!g.GetComponent<CanvasRenderer>())
                        {var cr=g.gameObject.AddComponent<CanvasRenderer>();typeof(Graphic).GetField("m_CanvasRenderer",Flags)?.SetValue(g,cr);}
                    Canvas.ForceUpdateCanvases();cam.Render();RenderTexture.active=target;
                    var png=new Texture2D(target.width,target.height,TextureFormat.RGBA32,false);
                    png.ReadPixels(new Rect(0,0,target.width,target.height),0,0);png.Apply();Directory.CreateDirectory("Logs");
                    File.WriteAllBytes($"Logs/percentile-{name}-{width}x{height}.png",png.EncodeToPNG());Object.DestroyImmediate(png);
                }
                Call("OpenLevels",false);
                var labels=(Text[])Get("levelRanks");
                Require(labels.Length==128&&labels.Count(t=>t)==32&&labels.Where(t=>t).All(t=>t.text.StartsWith("예상 상위 ")),"active theme has 32 percentile cards");
                Require(labels[0].text=="예상 상위 0.1%"&&labels[1].text=="예상 상위 30%"&&labels[2].text=="예상 상위 100%","perfect, ordinary and unplayed tiers");
                Require(!root.GetComponentsInChildren<Text>().Any(t=>t.name.StartsWith("Stage cumulative")),"list does not repeat detailed counters");
                var subtitle=root.GetComponentsInChildren<Text>().Single(t=>t.text=="최고 기록 기준 · 위아래로 드래그");
                Require(subtitle.preferredWidth<=subtitle.rectTransform.rect.width,"stage list guidance fits narrow screens");
                Capture("levels");
                var data=new StageTierCatalog{rows=new[]{new StageTierData{room=levels[1].room,players=30,thresholds=Enumerable.Repeat(levels[1].route.Length*2,7).ToArray()}}};
                var scroll=(PuzzleLevelScroll)Get("levelListScroll");scroll.verticalNormalizedPosition=.43f;
                app.ReceiveStageTiers(JsonUtility.ToJson(data));
                Require(labels[1].text=="예상 상위 1%"&&Mathf.Abs(scroll.verticalNormalizedPosition-.43f)<.001f,"late server tiers update without resetting scroll");
                app.ReceiveStageTiers("not json");Require(labels[1].text=="예상 상위 1%","malformed response preserves current labels");
                for(int theme=0;theme<4;theme++)
                {
                    ClearDialog();Call("SelectTheme",theme);
                    labels=(Text[])Get("levelRanks");
                    Require(labels.Count(t=>t)==32,"32 cards per theme");
                    for(int i=0;i<levels.Length;i++)Require((bool)labels[i]==(PuzzleCampaign.Theme(levels[i])==theme),"only selected theme visible");
                    var numbers=root.GetComponentsInChildren<Text>().Where(t=>t.name=="Stage number").Select(t=>t.text).ToArray();
                    Require(numbers.SequenceEqual(Enumerable.Range(1,32).Select(n=>n.ToString("00"))),"theme starts at 01 and ends at 32");
                    var sc=(PuzzleLevelScroll)Get("levelListScroll");sc.verticalNormalizedPosition=1;Capture("theme-"+theme+"-first");
                    sc.verticalNormalizedPosition=0;Capture("theme-"+theme+"-last");
                    var last=Array.FindLastIndex(levels,l=>PuzzleCampaign.Theme(l)==theme);
                    ClearDialog();Call("OpenStageDetail",last);
                    Require(root.GetComponentsInChildren<Text>().Any(t=>t.text==PuzzleCampaign.StageLabel(levels,last)),"detail has theme-local number");
                    Capture("theme-"+theme+"-detail");
                }
                ClearDialog();Call("OpenStageDetail",1);
                Require(((Text)Get("stageRank")).text=="1%","detail matches list");
                ClearDialog();
                var session=new PuzzleSession(levels[0]);foreach(int move in levels[0].route){session.Command(move);session.Tick(100);}
                Require(session.Solved,"test clear is a real solved session");
                Set("session",session);Set("solvedPresented",true);Set("reduceMotion",false);
                var badge=PuzzleUI.Panel("Owned stars",rect,PuzzleUI.Cream).rectTransform;
                PuzzleUI.Box(badge,width-110,height-38,100,32);
                Set("walletBadge",badge);Set("walletLabel",PuzzleUI.Label("Wallet count",badge,"",18,PuzzleUI.Ink));
                Call("Complete");var rank=(Text)Get("clearRank");var basis=(Text)Get("clearRankBasis");
                Require(rank.text=="예상 상위 0.1%"&&basis.text.Contains("최고 기록"),"clear labels retained best record explicitly");
                app.ReceiveStageTiers(JsonUtility.ToJson(data));Require(rank.text=="예상 상위 0.1%","population updates cannot remove perfect grade");
                var stars=(CompletionStars)Get("completionStars");stars.Tick(4,false);Capture("clear-perfect");
                Require(rank.rectTransform.anchoredPosition.y>=244&&basis.rectTransform.anchoredPosition.y>=227,"tier is above action buttons");
                foreach(var t in new[]{rank,basis})Require(t.preferredWidth<=t.rectTransform.rect.width+1,"clear tier text fits at "+width);
                ClearDialog();Set("session",new PuzzleSession(levels[1]));Set("stageTiers",new StageTierCatalog());Call("Complete");
                rank=(Text)Get("clearRank");Require(rank.text=="예상 상위 30%","clear uses fallback without server");
                app.ReceiveStageTiers(JsonUtility.ToJson(data));Require(rank.text=="예상 상위 1%","clear refreshes when server responds");
                Set("modal","home");app.ReceiveStageTiers("{\"rows\":[]}");Require(rank.text=="예상 상위 1%","late reply does not update a closed result screen");
                Require(progressBefore==JsonUtility.ToJson(collection)&&activityBefore==JsonUtility.ToJson(activity),"opening and refreshing ratings never changes saved progression");
            }
            finally{RenderTexture.active=old;Object.DestroyImmediate(host);Object.DestroyImmediate(root);Object.DestroyImmediate(cameraObject);target.Release();Object.DestroyImmediate(target);}
        }
        static void Require(bool value,string message){if(!value)throw new Exception(message);}
    }
}

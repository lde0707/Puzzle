using System;
using System.Collections.Generic;
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
    public static class CampaignExpansionChecks
    {
        static int checks;
        static void Require(bool value,string message){checks++;if(!value)throw new Exception(message);}
        [MenuItem("Puzzle/Prototype/Validate 128 Stage Campaign")]
        public static void Run()
        {
            checks=0;
            var legacy=new[]{"layouts","late_levels","resize_levels","blink_levels"}.SelectMany(Load).ToArray();
            var extra=Load("expansion_levels");var all=PuzzleCampaign.Order(legacy.Concat(extra));
            Require(legacy.Length==64&&extra.Length==64&&all.Length==128,"64 original plus 64 new levels");
            Require(all.Select(l=>l.room).Distinct().Count()==128,"unique stable IDs");
            foreach(var l in all)Require(all[PuzzleCampaign.ResumeIndex(all,l.room)]==l,"resume by stable room ID");
            Require(PuzzleCampaign.Theme(all[PuzzleCampaign.ResumeIndex(all,"P-old")])==1&&PuzzleCampaign.Theme(all[PuzzleCampaign.ResumeIndex(all,"S-old")])==1,"legacy portal fallback still enters portal theme");
            Require(PuzzleCampaign.ResumeIndex(all,null)==0&&PuzzleCampaign.ResumeIndex(all,"unknown")==0,"unknown save falls back safely");
            for(int theme=0;theme<4;theme++)
            {
                var group=all.Where(l=>PuzzleCampaign.Theme(l)==theme).ToArray();
                Require(group.Length==32,"32 independent stages per theme");
                Require(group.Take(16).Select(l=>l.room).SequenceEqual(PuzzleCampaign.Order(legacy).Where(l=>PuzzleCampaign.Theme(l)==theme).Select(l=>l.room)),"original theme order preserved");
                for(int i=0;i<group.Length;i++)
                {
                    int index=Array.IndexOf(all,group[i]);
                    Require(PuzzleCampaign.StageNumber(all,index)==i+1,"theme local number");
                    Require(PuzzleCampaign.NextInTheme(all,index)==(i==31?-1:index+1),"next never crosses theme boundary");
                    if(i>0)Require(group[i].route.Length>=group[i-1].route.Length,"shortest length does not fall within theme");
                    if(i>=16)Require(group[i].width>=group[i-1].width&&group[i].height>=group[i-1].height,"extension map dimensions never shrink");
                }
            }
            var log=new List<string>();
            foreach(var level in all)
            {
                level.Validate();var normal=new PuzzleSession(level);var quick=new PuzzleSession(level);
                foreach(int d in level.route)
                {
                    Require(normal.Command(d)&&quick.Command(d),"route accepted "+level.room);
                    normal.Tick(100);quick.Tick(0,true);
                    Require(!normal.Moving&&normal.Stable.positions.SequenceEqual(quick.Stable.positions)&&normal.Stable.sizes.SequenceEqual(quick.Stable.sizes),"normal and fast modes agree "+level.room);
                    Require(level.IsValidState(normal.Stable.positions,normal.Stable.sizes),"valid settled state "+level.room);
                }
                Require(normal.Solved&&quick.Solved&&normal.Moves==level.route.Length,"authored route completes "+level.room);
                if(PuzzleCampaign.ExtensionNumber(level)>0)
                {
                    int shortest=Shortest(level);Require(shortest==level.route.Length,"independent Unity BFS shortest "+level.room);
                    var version=ProgressSaveService.LevelVersion(level,"f0dda662");
                    normal.Reset();normal.Command(level.route[0]);normal.Tick(100);
                    var data=new ProgressData{levelId=level.room,dataVersion=version,blockIds=Enumerable.Range(0,level.cargo.Length).ToArray(),sizes=level.StartSizes,current=normal.Stable.Copy(),history=normal.StableHistory()};
                    var restored=new PuzzleSession(all.Single(l=>l.room==data.levelId));
                    Require(ProgressSaveService.Restore(JsonUtility.FromJson<ProgressData>(JsonUtility.ToJson(data)),version,restored),"new map checkpoint restores");
                    Require(restored.Stable.positions.SequenceEqual(normal.Stable.positions)&&restored.Stable.sizes.SequenceEqual(normal.Stable.sizes)&&restored.Moves==normal.Moves,"restored phase and footprints");
                    Require(restored.Undo()&&restored.Moves==0&&restored.Stable.positions.SequenceEqual(level.Start),"undo restores initial turn");
                    log.Add("PASS "+PuzzleCampaign.StageLabel(all,Array.IndexOf(all,level))+" "+level.room+" minimum="+shortest+" map="+level.width+"x"+level.height);
                }
            }
            var reference=JsonUtility.FromJson<BlinkReferenceCases>(File.ReadAllText("Assets/Editor/GravityPuzzle/expansion_reference_cases.json"));
            Require(reference.cases.Length==8192,"8192 independent reference transitions");
            var byId=all.ToDictionary(l=>l.room);
            foreach(var c in reference.cases)
            {
                var p=TiltSimulator.Plan(byId[c.room],c.before,c.direction,c.beforeSizes,c.turn);
                Require(p.result.SequenceEqual(c.after)&&p.sizes.SequenceEqual(c.afterSizes),"Python / Unity parity "+c.room);
            }
            TestTransfer(all,legacy);
            GravityPrototypeChecks.Run(); // Original 64 maps, movement, saves and star economy.
            HintChecks.Run();
            StageRatingSurfaceChecks.Run();
            RenderLargeBoards(all);
            log.Add("PASS "+checks+" expansion assertions; original rules, hints, UI surfaces and backup checks passed. No web build or deployment.");
            File.WriteAllLines("Logs/campaign-expansion-checks.txt",log);
            Debug.Log(log.Last());
        }
        static PuzzleDefinition[] Load(string name)=>JsonUtility.FromJson<LayoutDocument>(Resources.Load<TextAsset>("GravityPuzzle/"+name).text).layouts;
        static int Shortest(PuzzleDefinition level)
        {
            var root=new PuzzleSnapshot{positions=level.Start,sizes=level.StartSizes};
            var queue=new Queue<PuzzleSnapshot>();queue.Enqueue(root);
            var seen=new HashSet<string>{PuzzleHintSearch.Key(level,root.positions,root.sizes,0)};
            while(queue.Count>0)
            {
                var s=queue.Dequeue();if(level.IsSolved(s.positions,s.sizes))return s.moves;
                if(s.moves>=level.route.Length)continue;
                for(int d=0;d<4;d++)
                {
                    var p=TiltSimulator.Plan(level,s.positions,d,s.sizes,s.moves);int moves=s.moves+1;
                    if(!seen.Add(PuzzleHintSearch.Key(level,p.result,p.sizes,moves)))continue;
                    Require(seen.Count<100000,"bounded solver "+level.room);
                    queue.Enqueue(new PuzzleSnapshot{positions=p.result,sizes=p.sizes,moves=moves});
                }
            }
            return -1;
        }
        static void TestTransfer(PuzzleDefinition[] all,PuzzleDefinition[] legacy)
        {
            var collection=new PuzzleCollection();var activity=new PuzzleActivity();
            foreach(var l in all){collection.Award(l.room,l.route.Length,l.route.Length,false,false);activity.For(l.room).moves=l.route.Length;}
            var l0=all[0];var save=new ProgressData{levelId=l0.room,dataVersion=ProgressSaveService.LevelVersion(l0,"f0dda662"),completed=all.Select(l=>l.room).ToArray(),collection=collection,activity=activity};
            Require(collection.wallet==384,"128 maps award up to 384 stars");
            Require(ProgressTransfer.Decode(ProgressTransfer.Encode(save),all,out var decoded)&&decoded.collection.wallet==384,"128-stage backup accepts all records and stars");
            var merged=ProgressTransfer.Merge(save,decoded,all.Select(l=>l.room).ToArray());
            Require(merged.collection.wallet==384&&merged.collection.records.Length==128,"reimport neither clips nor doubles new stars");
            decoded.collection.wallet=385;
            Require(!ProgressTransfer.Decode(ProgressTransfer.Encode(decoded),all,out _),"reject impossible wallet above campaign maximum");
            var old=new PuzzleCollection{wallet=31,owned=Enumerable.Range(0,25).ToArray(),selected=24,records=legacy.Select(l=>new StageStars{id=l.room,stars=1,bestMoves=l.route.Length}).ToArray()};
            var oldJson=JsonUtility.ToJson(old);
            var restored=PuzzleCollection.Restore(old,legacy.Select(l=>l.room),all.Select(l=>l.room));
            Require(JsonUtility.ToJson(restored)==oldJson,"existing stars skins and records unchanged after expansion");
        }
        static void RenderLargeBoards(PuzzleDefinition[] all)
        {
            PuzzleUI.Font=Resources.Load<Font>("GravityPuzzle/NanumGothic-Regular");
            for(int theme=0;theme<4;theme++)
            {
                var map=all.Last(l=>PuzzleCampaign.Theme(l)==theme);
                var root=new GameObject("Expansion board",typeof(RectTransform),typeof(Canvas));
                var camObject=new GameObject("Expansion camera",typeof(Camera));
                var rt=new RenderTexture(780,1688,24){antiAliasing=4};var previous=RenderTexture.active;
                try
                {
                    var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;root.GetComponent<RectTransform>().sizeDelta=new Vector2(390,844);
                    var cam=camObject.GetComponent<Camera>();cam.orthographic=true;cam.orthographicSize=422;cam.transform.position=new Vector3(0,0,-10);cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=PuzzleUI.Cream;cam.targetTexture=rt;cam.cullingMask=1<<5;canvas.worldCamera=cam;
                    var board=new BoardView(PuzzleUI.Rect("Board",root.transform));board.Build(map);board.Layout(330,580,195,425);board.Render(new PuzzleSession(map),0,true);
                    var title=PuzzleUI.Label("Preview title",root.transform,PuzzleCampaign.ThemeName(theme)+" · 32단계",22,PuzzleUI.Ink);PuzzleUI.Box(title.rectTransform,15,770,360,40);
                    var note=PuzzleUI.Label("Preview note",root.transform,map.displayName+"\n"+map.width+"×"+map.height+" · 최단 "+map.route.Length+"회",16,PuzzleUI.Muted);PuzzleUI.Box(note.rectTransform,15,45,360,65);
                    foreach(var t in root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=5;
                    foreach(var g in root.GetComponentsInChildren<Graphic>(true))if(!g.GetComponent<CanvasRenderer>())
                    {var renderer=g.gameObject.AddComponent<CanvasRenderer>();typeof(Graphic).GetField("m_CanvasRenderer",BindingFlags.NonPublic|BindingFlags.Instance)?.SetValue(g,renderer);}
                    Canvas.ForceUpdateCanvases();cam.Render();RenderTexture.active=rt;
                    var png=new Texture2D(rt.width,rt.height,TextureFormat.RGBA32,false);png.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);png.Apply();File.WriteAllBytes("Logs/expansion-theme-"+theme+"-board.png",png.EncodeToPNG());Object.DestroyImmediate(png);
                }
                finally{RenderTexture.active=previous;Object.DestroyImmediate(root);Object.DestroyImmediate(camObject);rt.Release();Object.DestroyImmediate(rt);}
            }
        }
    }
}

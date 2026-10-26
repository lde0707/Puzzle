using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Puzzle.Gravity;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Puzzle.Editor
{
    public static class GravityPrototypeSetup
    {
        public const string ScenePath="Assets/Scenes/GravityPrototype.unity";
        [MenuItem("Puzzle/Prototype/Prepare and Validate")]
        public static void PrepareAndValidate()
        {
            if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            if(File.Exists(ScenePath)) EditorSceneManager.OpenScene(ScenePath);
            else
            {
                var scene=EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects,NewSceneMode.Single);
                var camera=UnityEngine.Object.FindFirstObjectByType<Camera>();camera.orthographic=true;camera.backgroundColor=PuzzleUI.Cream;camera.allowHDR=false;
                new GameObject("말랑궤도",typeof(GravityPuzzleApp));
                EditorSceneManager.SaveScene(scene,ScenePath);
            }
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
            PlayerSettings.WebGL.template="PROJECT:MobileTest";
            AssetDatabase.SaveAssets();
            GravityPrototypeChecks.Run();
            Debug.Log("말랑궤도 prototype prepared. Play GravityPrototype or use Puzzle/iPhone Test/Build Game (Web).");
        }
        [MenuItem("Puzzle/Prototype/Build Web")]
        public static void BuildWeb()
        {
            PrepareAndValidate();
            IphoneTestSetup.BuildGame();
        }
    }
    [Serializable] public sealed class ReferenceCase { public string room; public int direction; public Cell[] before,after; }
    [Serializable] public sealed class ReferenceCases { public ReferenceCase[] cases; }
    public static partial class GravityPrototypeChecks
    {
        private static int count;
        private static void Check(bool value,string label){if(!value)throw new Exception("FAIL: "+label);count++;}
        [MenuItem("Puzzle/Prototype/Run Rule and Save Checks")]
        public static void Run()
        {
            count=0;
            TestStarAttempts();
            TestCollection();
            Check(ProgressSaveService.StorageDirectory("C:/PuzzleStudio/말랑궤도",true)==ProgressSaveService.StorageDirectory("C:/PuzzleStudio/Android Puzzle",true),"display rename preserves Windows saves");
            Check(ProgressSaveService.StorageDirectory("/idbfs/existing",false)==Path.Combine("/idbfs/existing","MellowGravity"),"display rename preserves Web and mobile saves");
            Check(Resources.Load<Texture2D>("GravityPuzzle/Brand/mallang-orbit-wordmark")!=null,"generated title wordmark included in game resources");
            var levels=JsonUtility.FromJson<LayoutDocument>(Resources.Load<TextAsset>("GravityPuzzle/layouts").text).layouts;
            Check(levels.Length==16 && levels.Select(l=>l.room).Distinct().Count()==16,"16 original levels retained");
            var late=JsonUtility.FromJson<LayoutDocument>(Resources.Load<TextAsset>("GravityPuzzle/late_levels").text).layouts;
            levels=levels.Concat(late).ToArray();
            Check(levels.Length==32&&late.Length==16&&levels.Select(l=>l.room).Distinct().Count()==32,"32 unique levels");
            var campaign=PuzzleCampaign.Order(levels);
            Check(campaign.Length==32&&campaign.Select(l=>l.room).Distinct().Count()==32,"campaign retains all stable IDs");
            Check(campaign.Take(16).All(l=>!l.HasPortals)&&campaign.Skip(16).All(l=>l.HasPortals),"non-portal stages precede portal stages");
            Check(campaign[0].room=="F0"&&campaign[1].room=="3C","intro puzzles appear first");
            for(int i=1;i<campaign.Length;i++)if(i!=16)Check(campaign[i-1].route.Length<=campaign[i].route.Length,"difficulty rises within each mechanic group");
            var oldLevel=levels.First(l=>l.room=="2A");var beforeSort=new PuzzleSession(oldLevel);beforeSort.Command(oldLevel.route[0]);beforeSort.Tick(100);
            var checkpoint=new ProgressData{dataVersion="order-test",levelId=oldLevel.room,blockIds=Enumerable.Range(0,oldLevel.cargo.Length).ToArray(),sizes=oldLevel.cargo.Select(c=>c.size).ToArray(),current=beforeSort.Stable.Copy(),history=beforeSort.StableHistory()};
            var reordered=new PuzzleSession(campaign.First(l=>l.room==checkpoint.levelId));
            Check(ProgressSaveService.Restore(checkpoint,"order-test",reordered)&&reordered.Stable.positions.SequenceEqual(beforeSort.Stable.positions)&&reordered.Moves==beforeSort.Moves,"stage renumbering restores existing board by stable ID");
            var log=new List<string>();
            foreach(var level in levels)
            {
                level.Validate();var session=new PuzzleSession(level);
                foreach(int d in level.route)
                {
                    Check(session.Command(d),level.room+" accepts route");
                    while(session.Moving)
                    {
                        session.Tick(1f/30);
                        Check(NoVisualOverlap(session),level.room+" animated footprints never overlap");
                    }
                    Check(level.IsValidState(session.Stable.positions),level.room+" valid settled state");
                }
                Check(session.Solved,level.room+" solved by route");
                log.Add("PASS "+level.room+" / "+session.Moves+" directions");
            }
            var cases=JsonUtility.FromJson<ReferenceCases>(File.ReadAllText("Assets/Editor/GravityPuzzle/reference_cases.json")).cases;
            foreach(var c in cases)Check(TiltSimulator.Tilt(levels.First(l=>l.room==c.room),c.before,c.direction).SequenceEqual(c.after),"Python parity "+c.room);
            log.Add("PASS Python/C# parity: "+cases.Length+" cases");
            var portalCases=JsonUtility.FromJson<ReferenceCases>(File.ReadAllText("Assets/Editor/GravityPuzzle/portal_reference_cases.json")).cases;
            foreach(var c in portalCases)Check(TiltSimulator.Tilt(levels.First(l=>l.room==c.room),c.before,c.direction).SequenceEqual(c.after),"Portal Python parity "+c.room);
            log.Add("PASS independent portal parity: "+portalCases.Length+" cases");
            TestPortals(late);
            TestResize(levels,log);
            TestBlink(levels,log);
            var f0=levels[0];var s=new PuzzleSession(f0);
            Check(s.Command(0)&&!s.Moving&&s.Moves==1&&s.History.Count==1,"no-op counts without animation");
            Check(s.Undo()&&s.Moves==0&&s.Direction==0,"no-op undo");
            s.Command(2);s.Tick(.2f);s.VisualPosition(0,out float x,out float y);
            Check(Math.Abs(y-1.24f)<.001f,"12 cell acceleration");
            Check(s.Stable.positions[0].Equals(new Cell(1,1))&&s.Stable.moves==0&&s.StableHistory().Length==0,"moving checkpoint is pre-command");
            Check(!s.Command(3),"no queued direction");
            s.SetPause(PauseReason.Reset,true);s.SetPause(PauseReason.Inactive,true);float elapsed=s.Elapsed;s.Tick(100);
            Check(s.Elapsed==elapsed,"pause freezes elapsed time");s.SetPause(PauseReason.Reset,false);s.Tick(100);Check(s.Elapsed==elapsed,"overlapping pause reasons");
            s.SetPause(PauseReason.Inactive,false);Check(s.Undo()&&!s.Moving&&s.Moves==0,"undo during animation");s.Tick(100);Check(s.Stable.positions[0].Equals(new Cell(1,1)),"no stale completion after undo");
            s.Command(2);s.Tick(100);Check(s.Stable.positions[0].Equals(new Cell(1,5)),"large delta clamps exactly");
            s.Command(3);s.Tick(100);s.Command(0);Check(!s.Solved,"goal not solved before arrival");s.Tick(100);Check(s.Solved,"settled goal solved");
            s.Undo();Check(!s.Solved&&s.Moves==2&&s.Direction==3,"undo restores previous gravity and count");s.Command(0);s.Reset();s.Tick(100);Check(s.Moves==0&&s.History.Count==0&&s.Direction==0&&!s.Solved,"reset cancels motion");
            var slow=new PuzzleSession(f0);var fast=new PuzzleSession(f0);slow.Command(2);fast.Command(2);
            for(int i=0;i<15;i++)slow.Tick(1f/30);for(int i=0;i<30;i++)fast.Tick(1f/60);
            slow.VisualPosition(0,out x,out y);fast.VisualPosition(0,out float fx,out float fy);Check(Math.Abs(y-fy)<.0001,"30/60 fps common-time positions");
            slow.Tick(100);fast.Tick(100);Check(slow.Stable.positions.SequenceEqual(fast.Stable.positions),"30/60 fps identical result");
            var narrow=new PuzzleDefinition {room="narrow",width=5,height=5,family=3,walls=Enumerable.Range(0,5).Select(v=>new Cell(2,v)).ToArray(),cargo=new[]{new CargoDefinition{x=0,y=0,size=2,gx=0,gy=3}},route=new[]{2}};
            narrow.Validate();Check(TiltSimulator.Tilt(narrow,narrow.Start,3)[0].Equals(new Cell(0,0)),"2x2 cannot enter 1-cell gap");Check(TiltSimulator.Tilt(narrow,narrow.Start,2)[0].Equals(new Cell(0,3)),"bounds apply without perimeter walls");
            var identity=new PuzzleDefinition {room="identity",width=6,height=3,family=2,walls=new Cell[0],cargo=new[]{new CargoDefinition{x=0,y=0,size=1,gx=5,gy=0},new CargoDefinition{x=5,y=0,size=1,gx=0,gy=0}},route=new[]{2}};
            identity.Validate();Check(!identity.IsSolved(identity.Start),"matching sizes do not erase identity");
            TestSaves(f0); TestTouches();
            for(int frame=0;frame<=40;frame++)
                foreach(bool vertical in new[]{false,true})
                    foreach(bool travelling in new[]{false,true})
                    {
                        var scale=BoardView.JellyScale(frame*.01f,travelling,vertical);
                        Check(scale.x>=.81f&&scale.x<=1&&scale.y>=.81f&&scale.y<=1,"jelly stays inside logical footprint");
                    }
            Check(BoardView.JellyScale(1,false,true)==Vector2.one,"landing returns to rest");
            log.Add("PASS no-op, moving Undo/Reset, overlapping pauses, frame rates, bounds, identities, two-slot save recovery");
            log.Add("TOTAL assertions: "+count+"; failures: 0");
            Directory.CreateDirectory("Logs");File.WriteAllLines("Logs/gravity-prototype-validation.txt",log);
            Debug.Log(string.Join("\n",log));
        }
        private static void TestPortals(PuzzleDefinition[] late)
        {
            int previousArea=13*11,previousDepth=0;
            foreach(var map in late)
            {
                Check(map.width*map.height>=previousArea,"late maps grow in total area");previousArea=map.width*map.height;
                Check(map.route.Length>previousDepth,"late shortest routes increase");previousDepth=map.route.Length;
                var solid=map.SolidCells();var visited=new HashSet<Cell>();var queue=new Queue<Cell>();queue.Enqueue(map.Start[0]);visited.Add(map.Start[0]);
                while(queue.Count>0)
                {
                    var c=queue.Dequeue();foreach(var axis in TiltSimulator.Axes)
                    {
                        var next=new Cell(c.x+axis.x,c.y+axis.y);
                        if(next.x>=0&&next.y>=0&&next.x<map.width&&next.y<map.height&&!solid.Contains(next)&&visited.Add(next))queue.Enqueue(next);
                    }
                }
                Check(visited.Count==map.width*map.height-solid.Count,"whole floor connected without portal transport");
            }
            var level=new PuzzleDefinition{room="face-test",width=13,height=12,family=4,walls=new Cell[0],cargo=new[]{new CargoDefinition{x=1,y=3,size=1,gx=11,gy=1}},portals=new[]{new PortalDefinition{id="A",x=5,y=3,face=1,exitX=9,exitY=8,exitFace=0,size=1}},route=new[]{3}};
            var s=new PuzzleSession(level);var before=s.Stable.Copy();
            Check(s.Command(3)&&s.Plan.entered[0]==0,"inward contact activates selected face");
            Check(s.Plan.slide[0].Equals(new Cell(4,3))&&s.Target[0].Equals(new Cell(9,7)),"solid face stops slide; exit adjacent to other active face");
            float travel=(float)Math.Sqrt(3/6f);s.Tick(travel+.10f);
            Check(s.InPortalPhase&&s.Moving&&!s.Solved,"warp remains an active command");
            s.VisualPosition(0,out float x,out float y);Check(x==4&&y==3,"first warp half stays at contact");
            Check(s.VisualPortalScale(0)<1&&s.VisualPortalScale(0)>=0,"warp compresses inside footprint");
            float progress=s.PortalProgress;s.SetPause(PauseReason.Menu,true);s.Tick(5);Check(s.PortalProgress==progress,"pause freezes warp");s.SetPause(PauseReason.Menu,false);
            var save=new ProgressData{dataVersion="face-test",levelId=level.room,blockIds=new[]{0},sizes=new[]{1},current=s.Stable.Copy(),history=s.StableHistory()};
            var recovered=new PuzzleSession(level);Check(ProgressSaveService.Restore(save,"face-test",recovered)&&recovered.Moves==0&&recovered.Stable.positions.SequenceEqual(before.positions),"mid-warp save restores pre-command checkpoint");
            s.Tick(PuzzleSession.PortalDuration*.5f);s.VisualPosition(0,out x,out y);Check(x==9&&y==7,"second warp half uses exit without crossing walls");
            Check(s.Undo()&&s.Plan==null&&s.Stable.positions.SequenceEqual(before.positions),"undo cancels warp");s.Tick(100);Check(s.Stable.positions.SequenceEqual(before.positions),"cancelled warp never teleports later");
            s.Command(3);s.Tick(travel+.2f);s.Reset();s.Tick(100);Check(s.Moves==0&&s.Plan==null&&s.Stable.positions.SequenceEqual(level.Start),"reset cancels warp");
            s.Command(3);s.Tick(0,true);Check(!s.Moving&&s.Stable.positions[0].Equals(new Cell(9,7))&&s.VisualPortalScale(0)==1,"reduced motion commits exact portal result");
            Check(s.Stable.direction==3&&s.Moves==1,"warp does not add a command or change gravity");
            Check(s.Command(2)&&s.Moving&&s.InPortalPhase&&s.Plan.entered[0]==1,"adjacent face accepts reverse trip with zero slide distance");s.Tick(100);
            Check(s.Stable.positions[0].Equals(new Cell(4,3))&&s.Moves==2,"paired block works in both directions");
            for(int face=0;face<4;face++)
            {
                var axis=TiltSimulator.Axes[face];var contact=new Cell(5+axis.x,3+axis.y);
                var plan=TiltSimulator.Plan(level,new[]{contact},(face+2)%4);
                Check(face==1?plan.result[0].Equals(new Cell(9,7)):plan.entered[0]==-1&&plan.result[0].Equals(contact),"only selected face activates; other faces remain solid");
            }
            var occupied=JsonUtility.FromJson<PuzzleDefinition>(JsonUtility.ToJson(level));
            occupied.walls=new[]{new Cell(10,7)};occupied.cargo=new[]{new CargoDefinition{x=1,y=3,size=1,gx=11,gy=1},new CargoDefinition{x=9,y=7,size=1,gx=1,gy=1}};
            var blocked=new PuzzleSession(occupied);blocked.Command(3);
            Check(blocked.Plan.blocked[0]&&blocked.Target[0].Equals(new Cell(4,3)),"occupied exit leaves friend at contact");blocked.Tick(100);
            Check(occupied.IsValidState(blocked.Stable.positions),"blocked portal never overlaps");
            blocked.Command(3);Check(blocked.Plan.entered[0]==0&&blocked.Plan.blocked[0],"contact can retry on next inward command");blocked.Tick(100);
            Check(blocked.Undo()&&blocked.Stable.moves==1,"blocked retry remains undoable");
            var large=new PuzzleDefinition{room="large-port",width=14,height=11,family=4,walls=new Cell[0],cargo=new[]{new CargoDefinition{x=0,y=2,size=2,gx=12,gy=8}},portals=new[]{new PortalDefinition{id="A",x=4,y=2,face=1,exitX=9,exitY=6,exitFace=0,size=2}},route=new[]{3}};
            large.Validate();Check(TiltSimulator.Tilt(large,large.Start,3)[0].Equals(new Cell(9,4)),"matching 2x2 face supports whole footprint");
            large=JsonUtility.FromJson<PuzzleDefinition>(JsonUtility.ToJson(large));
            large.walls=new[]{new Cell(11,5)};large.cargo=new[]{new CargoDefinition{x=0,y=2,size=2,gx=12,gy=8},new CargoDefinition{x=10,y=5,size=1,gx=1,gy=9}};
            large.Validate();Check(TiltSimulator.Plan(large,large.Start,3).blocked[0],"partial occupancy blocks 2x2 exit");
            large.portals[0].exitX=13;bool rejected=false;try{large.Validate();}catch(ArgumentException){rejected=true;}Check(rejected,"out-of-bounds solid portal rejected");
            var wallPortal=new PuzzleDefinition{room="wall-port",width=8,height=8,family=4,walls=new[]{new Cell(0,3),new Cell(5,7)},cargo=new[]{new CargoDefinition{x=2,y=3,size=1,gx=6,gy=6}},portals=new[]{new PortalDefinition{id="A",x=0,y=3,face=3,exitX=5,exitY=7,exitFace=0,size=1}},route=new[]{1}};
            wallPortal.Validate();var wallTrip=TiltSimulator.Tilt(wallPortal,wallPortal.Start,1);
            Check(wallTrip[0].Equals(new Cell(5,6)),"wall-mounted portal exits inside perimeter");
            Check(TiltSimulator.Tilt(wallPortal,wallTrip,2)[0].Equals(new Cell(1,3)),"wall-mounted portal reverse trip");
            wallPortal.portals[0].face=1;rejected=false;try{wallPortal.Validate();}catch(ArgumentException){rejected=true;}Check(rejected,"wall face cannot open outside the map");
            Check(PuzzleSession.PortalOpenness(0)==1&&PuzzleSession.PortalOpenness(1)==1,"portal motion starts and ends at full size");
            foreach(float moment in new[]{.44f,.49f,.5f,.51f,.52f})Check(PuzzleSession.PortalOpenness(moment)==0,"endpoint swap happens while fully invisible");
            float last=1;
            for(int step=1;step<=44;step++){float value=PuzzleSession.PortalOpenness(step/100f);Check(value<=last&&value>=0,"entry closes continuously");last=value;}
            last=0;
            for(int step=53;step<=100;step++){float value=PuzzleSession.PortalOpenness(step/100f);Check(value>=last&&value<=1,"exit opens continuously");last=value;}
            for(int face=0;face<4;face++)for(int step=0;step<=32;step++)
            {
                BoardView.PortalPose(step/32f,face,out var scale,out var offset);
                Check(Math.Abs(offset.x)+scale.x/2<=.50001f&&Math.Abs(offset.y)+scale.y/2<=.50001f,"portal suction never overlaps adjacent cells");
                if(step==0)Check(scale==Vector2.zero,"jelly fully vanishes at threshold");
                if(step==32)Check(scale==Vector2.one&&offset==Vector2.zero,"jelly emerges at exact resting pose");
            }
            foreach(var map in late)
            {
                var slow=new PuzzleSession(map);var fast=new PuzzleSession(map);
                foreach(int d in map.route)
                {
                    slow.Command(d);fast.Command(d);
                    while(slow.Moving)slow.Tick(1f/30);while(fast.Moving)fast.Tick(1f/60);
                    Check(slow.Stable.positions.SequenceEqual(fast.Stable.positions),"portal 30/60 fps deterministic");
                }
                Check(slow.Solved&&fast.Solved,"portal complete only after final landing");
            }
        }
        private static bool NoVisualOverlap(PuzzleSession s)
        {
            for(int i=0;i<s.Level.cargo.Length;i++)
            {
                s.VisualPosition(i,out float x,out float y);float size=s.VisualSize(i);
                foreach(var wall in s.Level.SolidCells())if(Overlap(x,y,size,wall.x,wall.y,1))return false;
                for(int j=i+1;j<s.Level.cargo.Length;j++){s.VisualPosition(j,out float bx,out float by);if(Overlap(x,y,size,bx,by,s.VisualSize(j)))return false;}
            }
            return true;
        }
        private static bool Overlap(float x,float y,float size,float bx,float by,float bsize)=>x+size>bx+.0001f&&bx+bsize>x+.0001f&&y+size>by+.0001f&&by+bsize>y+.0001f;
        private static void TestTouches()
        {
            var events=new GameObject("Test EventSystem",typeof(UnityEngine.EventSystems.EventSystem));
            var go=new GameObject("Test touch",typeof(RectTransform),typeof(PuzzleTap));
            var tap=go.GetComponent<PuzzleTap>();var eventSystem=events.GetComponent<UnityEngine.EventSystems.EventSystem>();
            int generation=0,commands=0;bool ready=true;
            tap.Epoch=()=>generation;tap.CanUse=()=>ready;tap.Activate=()=>commands++;
            var first=new UnityEngine.EventSystems.PointerEventData(eventSystem){pointerId=1,position=Vector2.zero};
            var second=new UnityEngine.EventSystems.PointerEventData(eventSystem){pointerId=2,position=Vector2.zero};
            try
            {
                PuzzleTap.CancelAll();tap.OnPointerDown(first);tap.OnPointerDown(second);tap.OnPointerUp(second);tap.OnPointerUp(first);
                Check(commands==1,"first valid finger retained when second finger releases");
                tap.OnPointerUp(first);Check(commands==1,"release cannot repeat command");
                ready=false;tap.OnPointerDown(first);ready=true;tap.OnPointerUp(first);Check(commands==1,"held moving touch cannot queue command");
                tap.OnPointerDown(first);generation++;tap.OnPointerUp(first);Check(commands==1,"transition cancels in-progress press");
                tap.OnPointerDown(first);tap.OnPointerExit(first);tap.OnPointerUp(first);Check(commands==1,"drag outside cancels press");
                tap.OnPointerDown(first);PuzzleTap.CancelAll();tap.OnPointerUp(first);Check(commands==1,"lifecycle cancels captured touch");
                tap.OnPointerDown(first);tap.OnPointerUp(first);Check(commands==2,"new valid touch works after cancellation");
                var scrollObject=new GameObject("Test drag scroll",typeof(RectTransform),typeof(PuzzleLevelScroll));
                try
                {
                    var scroll=scrollObject.GetComponent<PuzzleLevelScroll>();scroll.content=go.GetComponent<RectTransform>();scroll.viewport=scrollObject.GetComponent<RectTransform>();
                    tap.OnPointerDown(first);scroll.OnBeginDrag(first);tap.OnPointerUp(first);scroll.OnEndDrag(first);
                    Check(commands==2,"scroll drag release cannot select the pressed stage");
                    tap.OnPointerDown(first);tap.OnPointerUp(first);Check(commands==3,"fresh tap works after scroll drag");
                }
                finally{UnityEngine.Object.DestroyImmediate(scrollObject);}

            }
            finally { UnityEngine.Object.DestroyImmediate(go);UnityEngine.Object.DestroyImmediate(events);PuzzleTap.CancelAll(); }
        }
        private static void TestSaves(PuzzleDefinition level)
        {
            string folder=Path.Combine("Logs","save-check-"+Guid.NewGuid().ToString("N"));
            var service=new ProgressSaveService(folder);var session=new PuzzleSession(level);
            var data=new ProgressData{dataVersion="test",levelId=level.room,blockIds=new[]{0},sizes=new[]{1},current=session.Stable.Copy(),history=session.StableHistory(),completed=new[]{"F0"},largeText=true};
            Check(service.Save(data),"first stable save");session.Command(2);session.Tick(100);data.current=session.Stable.Copy();data.history=session.StableHistory();Check(service.Save(data),"second stable save");
            var loaded=new ProgressSaveService(folder).Load();Check(loaded.current.moves==1&&loaded.largeText,"latest valid generation");
            var restored=new PuzzleSession(level);Check(ProgressSaveService.Restore(loaded,"test",restored)&&restored.Undo()&&restored.Moves==0,"saved undo roundtrip");
            Check(!ProgressSaveService.Restore(loaded,"changed",restored),"data version invalidates current board");loaded.sizes[0]=2;Check(!ProgressSaveService.Restore(loaded,"test",restored),"size identity validation");
            File.WriteAllText(Path.Combine(folder,"gravity-progress-0.json"),"truncated");
            File.WriteAllText(Path.Combine(folder,"gravity-progress-1.json.tmp"),"interrupted write");
            loaded=new ProgressSaveService(folder).Load();Check(loaded.current.moves==0&&loaded.completed.Contains("F0"),"corrupt latest falls back, temp ignored");
            loaded.current.positions[0]=new Cell(-1,1);Check(!ProgressSaveService.Restore(loaded,"test",restored),"invalid stored coordinates rejected");
            File.WriteAllText(Path.Combine(folder,"gravity-progress-1.json"),"also broken");Check(new ProgressSaveService(folder).Load()==null,"all invalid saves permit clean start");
        }
    }
}


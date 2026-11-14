using System;
using System.IO;
using System.Linq;
using Puzzle.Gravity;
using UnityEngine;

namespace Puzzle.Editor
{
    public static partial class GravityPrototypeChecks
    {
        public static void ValidateStarsAndBuild(){Run();IphoneTestSetup.BuildGame();}
        public static void ValidateStarsOnly(){TestStarAttempts();TestCollection();Debug.Log("PASS star allowance checks without web build");}
        private static void TestStarAttempts()
        {
            var level=new PuzzleDefinition{room="star-attempt",width=5,height=3,walls=new Cell[0],cargo=new[]{new CargoDefinition{x=0,y=1,gx=4,gy=1,size=1}},route=new[]{3}};
            var s=new PuzzleSession(level);
            int Stars(PuzzleSession run)=>PuzzleCollection.Rating(run.Moves,run.Level.route.Length,run.UsedUndo);
            void Move(int direction){Check(s.Command(direction),"star attempt accepts command");s.Tick(100);}
            Check(!s.Undo()&&!s.UsedUndo,"empty undo is not usage");
            Move(3);Check(s.Solved&&Stars(s)==3,"actual minimal no-undo clear earns three");
            Check(s.Undo()&&s.Moves==0&&s.UsedUndo,"rewinding all moves does not erase undo usage");
            Move(3);Check(s.Solved&&Stars(s)==2,"actual minimal clear after undo earns two");
            s.Reset();Check(!s.UsedUndo&&s.Moves==0,"restart begins a fresh attempt");
            Move(1);Move(1);Move(1);Move(3);Check(s.Solved&&Stars(s)==3,"actual minimum plus three clear without undo earns three");
            s.Reset();Move(1);Check(s.Undo(),"no-op move is undoable");Move(1);Move(3);
            Check(s.Solved&&Stars(s)==2,"actual within-allowance clear with undo earns two");
            s.Reset();Move(1);Move(1);Move(1);Move(1);Move(3);
            Check(s.Solved&&Stars(s)==2,"actual minimum plus four without undo earns two");
            s.Reset();Move(1);Check(s.Undo(),"undo marks attempt before longer clear");Move(1);Move(1);Move(1);Move(1);Move(3);
            Check(s.Solved&&Stars(s)==1,"actual minimum plus four with undo earns one");
            s.Reset();Move(1);s.SetPause(PauseReason.Menu,true);Check(!s.Undo()&&!s.UsedUndo&&s.Moves==1,"rejected paused undo is not usage");s.SetPause(PauseReason.Menu,false);s.Reset();
            s.Command(3);Check(s.Moving&&s.Undo()&&s.UsedUndo&&!s.Moving,"cancelling an in-flight move counts as undo");

            string root=Path.GetFullPath("Logs");
            string folder=Path.Combine(root,"star-save-check-"+Guid.NewGuid().ToString("N"));
            Check(Path.GetDirectoryName(folder)==root,"test save directory stays under project logs");
            try
            {
                var service=new ProgressSaveService(folder);
                ProgressData Capture()=>new ProgressData{dataVersion="stars",levelId=level.room,blockIds=new[]{0},sizes=level.StartSizes,current=s.Stable.Copy(),history=s.StableHistory(),usedUndo=s.UsedUndo};
                Check(service.Save(Capture()),"save zero-move attempt after undo");
                var restored=new PuzzleSession(level);
                Check(ProgressSaveService.Restore(service.Load(),"stars",restored)&&restored.UsedUndo&&restored.Moves==0,"reload cannot erase undo even at move zero");
                restored.Command(3);restored.Tick(100);Check(restored.Solved&&Stars(restored)==2,"resumed minimal clear retains two-star limit");
                s=restored;Check(service.Save(Capture()),"save solved attempt before reward recovery");
                var recovery=new PuzzleSession(level);Check(ProgressSaveService.Restore(service.Load(),"stars",recovery)&&recovery.Solved&&Stars(recovery)==2,"solved save recovery preserves rating");
                var collection=new PuzzleCollection();
                Check(collection.Award(level.room,recovery.Moves,level.route.Length,recovery.UsedUndo)==2,"recovered clear awards the correct two stars");
                Check(collection.Award(level.room,recovery.Moves,level.route.Length,recovery.UsedUndo)==0,"recovery does not pay twice");
                s.Reset();s.Command(3);Check(service.Save(Capture()),"save unfinished fresh move");
                var pending=new PuzzleSession(level);Check(ProgressSaveService.Restore(service.Load(),"stars",pending)&&!pending.UsedUndo&&pending.Moves==0,"stable pending snapshot retains fresh attempt metadata");
                s.Tick(100);s.Undo();s.Command(3);Check(service.Save(Capture()),"save unfinished move after prior undo");
                Check(ProgressSaveService.Restore(service.Load(),"stars",pending)&&pending.UsedUndo,"pending snapshots retain previous undo usage");
                var invalid=Capture();invalid.current.positions[0]=new Cell(-1,0);invalid.usedUndo=false;
                Check(!ProgressSaveService.Restore(invalid,"stars",pending)&&pending.UsedUndo,"invalid save cannot partially reset attempt flag");
                // Older files did not record undo usage; retain their progress and earned stars.
                var old=JsonUtility.FromJson<ProgressData>("{\"collection\":{\"wallet\":5,\"selected\":0,\"owned\":[0],\"records\":[{\"id\":\"star-attempt\",\"stars\":3,\"bestMoves\":3}]}}");
                Check(!old.usedUndo,"missing legacy attempt flag defaults to unused");
                var retained=PuzzleCollection.Restore(old.collection,new[]{level.room},new[]{level.room});
                Check(retained.wallet==5&&retained.Stars(level.room)==3&&retained.Award(level.room,5,1,true)==0,"new policy does not revoke historical stars or repay old rewards");
            }
            finally{if(Directory.Exists(folder))Directory.Delete(folder,true);}
            Debug.Log("PASS star rules: four outcomes, successful undo only, restart, zero-move and pending save, solved recovery and legacy rewards");
        }
    }
}

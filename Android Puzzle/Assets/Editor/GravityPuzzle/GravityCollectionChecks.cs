using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Puzzle.Gravity;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzle.Editor
{
    public static partial class GravityPrototypeChecks
    {
        private static void TestCollection()
        {
            int total=PuzzleCollection.Names.Length;
            Check(total==40&&PuzzleCollection.Names.Distinct().Count()==40,"forty unique skin designs");
            Check(SkinDecoration.Concepts.Length==total,"every skin has a concept description");
            var oldComplete=PuzzleCollection.Restore(new PuzzleCollection{owned=Enumerable.Range(0,20).ToArray(),selected=19,wallet=9},null,new[]{"a"});
            Check(oldComplete.owned.SequenceEqual(Enumerable.Range(0,20))&&oldComplete.selected==19&&oldComplete.wallet==9&&oldComplete.Remaining==20,"old complete collection preserves ownership selection and wallet with twenty new candidates");
            Check(oldComplete.TryDraw(n=>{Check(n==20,"expanded pool contains exactly twenty new skins");return n-1;},()=>true,out int newSkin)&&newSkin==39&&oldComplete.wallet==6,"old collection can draw the last new skin");
            for(int optimum=3;optimum<=18;optimum++)
            {
                Check(PuzzleCollection.Rating(optimum,optimum,false)==3,"minimum moves without undo earns three stars");
                Check(PuzzleCollection.Rating(optimum,optimum,true)==2,"minimum moves with undo earns two stars");
                for(int extra=1;extra<=3;extra++)
                {
                    Check(PuzzleCollection.Rating(optimum+extra,optimum,false)==3,"up to three extra moves without undo earns three stars");
                    Check(PuzzleCollection.Rating(optimum+extra,optimum,true)==2,"within allowance with undo earns two stars");
                }
                Check(PuzzleCollection.Rating(optimum+4,optimum,false)==2&&PuzzleCollection.Rating(optimum+50,optimum,false)==2,"outside allowance without undo earns two stars");
                Check(PuzzleCollection.Rating(optimum+4,optimum,true)==1,"outside allowance with undo earns one star");
            }
            var c=PuzzleCollection.Restore(null,new[]{"a","b","a","removed"},new[]{"a","b","c"});
            Check(c.wallet==2&&c.records.Length==2&&c.Stars("a")==1&&c.owned.SequenceEqual(new[]{0}),"legacy clears migrate once without inventing move records");
            var legacy=JsonUtility.FromJson<ProgressData>("{\"completed\":[\"a\",\"b\"]}");
            var migrated=PuzzleCollection.Restore(legacy.collection,legacy.completed,new[]{"a","b"});
            Check(migrated.wallet==2&&migrated.records.Length==2,"legacy JSON inline objects migrate completed stages");
            var empty=PuzzleCollection.Restore(new PuzzleCollection(),new[]{"a","b"},new[]{"a","b"});
            Check(empty.wallet==2&&PuzzleCollection.Restore(empty,new[]{"a","b"},new[]{"a","b"}).wallet==2,"empty inline collection recovery is idempotent");
            Check(c.Award("a",9,3,true)==0&&c.wallet==2,"one star replay cannot farm currency");
            Check(c.Award("a",7,3,false)==1&&c.wallet==3,"one to two stars pays only improvement");
            Check(c.Award("a",3,3,false)==1&&c.wallet==4,"two to three stars pays one");
            Check(c.Award("a",3,3,true)==0&&c.Award("a",12,3,true)==0&&c.records.First(r=>r.id=="a").bestMoves==3,"undo and replay cannot repay or worsen best record");
            Check(c.Award("c",3,3,false)==3&&c.wallet==7,"new clear pays all earned stars");
            string previous=JsonUtility.ToJson(c);
            Check(!c.TryDraw(n=>0,()=>false,out _)&&previous==JsonUtility.ToJson(c),"failed save rolls back wallet selection and ownership");
            try{c.TryDraw(n=>0,()=>throw new IOException("test save failure"),out _);}catch(IOException){}
            Check(previous==JsonUtility.ToJson(c),"thrown save failure also rolls back transaction");
            Check(!c.TryDraw(n=>n,()=>true,out _)&&previous==JsonUtility.ToJson(c),"invalid picker cannot spend stars");
            c.wallet=total*PuzzleCollection.DrawCost;
            var unlocked=new HashSet<int>{0};
            for(int i=0;i<total-1;i++)
            {
                int expectedCount=total-1-i;
                Check(c.TryDraw(n=>{Check(n==expectedCount,"pool excludes owned skins");return n-1;},()=>true,out int skin),"draw succeeds with funds");
                Check(unlocked.Add(skin)&&c.selected==0,"draw unlocks a new skin without changing equipped skin");
            }
            Check(c.wallet==3&&c.owned.Length==total&&!c.TryDraw(n=>0,()=>true,out _),"complete collection cannot consume remaining stars");
            Check(!c.TryEquip(999,()=>throw new Exception("must not save")),"unowned skin cannot be equipped");
            Check(!c.TryEquip(2,()=>false)&&c.selected==0,"failed equip keeps previous skin");
            try{c.TryEquip(2,()=>throw new IOException("equip save failure"));}catch(IOException){}
            Check(c.selected==0,"thrown equip failure keeps previous skin");
            Check(c.TryEquip(2,()=>true)&&c.selected==2&&c.wallet==3,"explicit equip does not spend more stars");
            c=new PuzzleCollection{wallet=2};
            Check(!c.TryDraw(n=>0,()=>throw new Exception("must not persist"),out _)&&c.wallet==2,"insufficient balance prevents draw");
            c=PuzzleCollection.Restore(new PuzzleCollection{owned=new[]{-2,0,1,1,99},selected=19,wallet=-5},null,new[]{"a"});
            Check(c.owned.SequenceEqual(new[]{0,1})&&c.selected==0&&c.wallet==0,"invalid saved selection safely falls back");
            string directory=Path.Combine(Path.GetTempPath(),"puzzle-skins-"+Guid.NewGuid().ToString("N"));
            try
            {
                var saves=new ProgressSaveService(directory);
                c=new PuzzleCollection();c.Award("a",3,3,false);
                Check(saves.Save(new ProgressData{collection=c,completed=new[]{"a"}}),"reward saved through real two-slot service");
                Check(c.TryDraw(n=>n-1,()=>saves.Save(new ProgressData{collection=c,completed=new[]{"a"}}),out int skin)&&skin==39,"new skin spend and unlock saved together");
                var loaded=saves.Load();var restored=PuzzleCollection.Restore(loaded.collection,loaded.completed,new[]{"a"});
                Check(restored.wallet==0&&restored.selected==0&&restored.owned.Contains(skin)&&restored.Stars("a")==3,"wallet skin and rating survive reload");
                Check(restored.TryEquip(skin,()=>saves.Save(new ProgressData{collection=restored,completed=new[]{"a"}})),"explicit equip persists separately from unlock");
                Check(saves.Load().collection.selected==skin&&saves.Load().collection.wallet==0,"equipped reward survives reload without another spend");
                Check(restored.Award("a",3,3,false)==0,"reloaded solved stage cannot pay again");
            }
            finally{if(Directory.Exists(directory))Directory.Delete(directory,true);}
            var go=new GameObject("Skin mesh check",typeof(RectTransform),typeof(SkinDecoration));
            try
            {
                var graphic=go.GetComponent<SkinDecoration>();var vertex=new UIVertex();
                var hashes=new HashSet<string>();
                for(int mode=0;mode<3;mode++)
                for(int id=0;id<total;id++)for(int variant=0;variant<3;variant++)
                {
                    graphic.skin=id;graphic.variant=variant;
                    for(int frame=0;frame<4;frame++)
                    {
                        graphic.Motion(mode==1,mode==2?frame*.21f:10,new Vector2(1,0));
                        graphic.Mood(mode==2,mode==1?Vector2.right:Vector2.zero);
                        graphic.Pose(frame*.7f,false);
                        using(var mesh=new VertexHelper())
                        {
                            graphic.Draw(mesh,new Rect(0,0,100,100));
                            Check(mesh.currentVertCount>0,"complete character mesh present for all variants");
                            var signature=new System.Text.StringBuilder();
                            for(int v=0;v<mesh.currentVertCount;v++)
                            {
                                mesh.PopulateUIVertex(ref vertex,v);
                                Check(vertex.position.x>=-.01f&&vertex.position.x<=100.01f&&vertex.position.y>=-.01f&&vertex.position.y<=100.01f,"animated character stays inside collision footprint: "+id);
                                if(mode==0&&frame==0&&variant==0)signature.Append(vertex.position.ToString("F3"));
                            }
                            if(mode==0&&frame==0&&variant==0)Check(hashes.Add(signature.ToString()),"visually distinct complete character meshes");
                        }
                    }
                }
                graphic.Motion(true,.35f,Vector2.right);graphic.Pose(1,true);
                Check(!graphic.IsTravelling&&graphic.ArrivalPulse==0,"fast mode suppresses skin action animation");
                graphic.Pose(1,false);Check(graphic.IsTravelling&&graphic.ArrivalPulse>.99f,"movement and peak arrival action active");
                graphic.Motion(false,1,Vector2.zero);Check(!graphic.IsTravelling&&graphic.ArrivalPulse==0,"arrival effect ends after short duration");
                for(int id=0;id<total;id++)for(int v=0;v<3;v++)
                {
                    Color a=SkinDecoration.ColorFor(id,v),b=SkinDecoration.ColorFor(id,(v+1)%3);
                    Check(new Vector3(a.r-b.r,a.g-b.g,a.b-b.b).magnitude>.15f,"three identifiable colors per concept");
                }
                var boardRoot=new GameObject("Skin goal colors check",typeof(RectTransform));
                try
                {
                    var board=new BoardView(boardRoot.GetComponent<RectTransform>());
                    board.Build(new PuzzleDefinition{width=8,height=8,walls=new Cell[0],cargo=Enumerable.Range(0,3).Select(i=>new CargoDefinition{x=1+i*2,y=1,size=1,gx=1+i*2,gy=6}).ToArray()});
                    var goals=boardRoot.GetComponentsInChildren<Image>().Where(i=>i.name.StartsWith("Home ")).OrderBy(i=>i.name).ToArray();
                    Check(goals.Length==3,"three visible destinations");
                    for(int id=0;id<total;id++)
                    {
                        board.ApplySkin(id);
                        for(int v=0;v<3;v++)Check(goals[v].color==SkinDecoration.ColorFor(id,v),"goal updates to equipped friend color");
                        Check(boardRoot.GetComponentsInChildren<SkinDecoration>().All(s=>s.skin==id),"all board friends equip the same concept");
                    }
                }
                finally{UnityEngine.Object.DestroyImmediate(boardRoot);}
            }
            finally{UnityEngine.Object.DestroyImmediate(go);}
            var motionRoot=new GameObject("Skin arrival events",typeof(RectTransform));
            try
            {
                var level=new PuzzleDefinition{room="skin-arrival",family=0,width=5,height=3,walls=new Cell[0],cargo=new[]{new CargoDefinition{x=0,y=1,size=1,gx=4,gy=1}},route=new[]{3}};
                var session=new PuzzleSession(level);var board=new BoardView(motionRoot.GetComponent<RectTransform>());board.Build(level);board.Layout(300,180,150,90);board.Render(session);
                var actor=motionRoot.GetComponentInChildren<SkinDecoration>();
                Check(actor.ArrivalPulse==0,"loading a board does not celebrate");
                session.Command(3);board.BeginTilt(session);session.Tick(.1f);board.Render(session,.1f);
                Check(actor.IsTravelling&&actor.ArrivalPulse==0,"skin movement only while actually sliding");
                session.Tick(100);board.Render(session);board.Render(session,.2f);
                Check(!actor.IsTravelling&&actor.ArrivalPulse>0&&actor.happy,"settled goal triggers a short concept celebration");
                float pausedPulse=actor.ArrivalPulse;session.SetPause(PauseReason.Menu,true);board.Render(session,2);
                Check(actor.ArrivalPulse==pausedPulse,"pause freezes concept arrival animation");
                session.SetPause(PauseReason.Menu,false);board.Render(session,1);board.Render(session,.1f);
                Check(actor.ArrivalPulse==0,"remaining on a goal does not replay celebration");
                session.Undo();board.CancelMotion();board.Render(session,.1f);Check(actor.ArrivalPulse==0&&!actor.IsTravelling,"undo cancels transient skin animation");
                session.Command(3);session.Tick(100);board.CancelMotion();board.Render(session,.2f);
                Check(actor.ArrivalPulse==0,"restoring a settled goal does not falsely retrigger");
            }
            finally{UnityEngine.Object.DestroyImmediate(motionRoot);}
            Debug.Log("PASS skin collection: rewards, migration, nonduplicating draws, failed saves, persistence, 40 designs x 3 colors and animation bounds");
        }
    }
}

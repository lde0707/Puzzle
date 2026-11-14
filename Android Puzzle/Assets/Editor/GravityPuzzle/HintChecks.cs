using System;
using System.IO;
using System.Linq;
using Puzzle.Gravity;
using UnityEngine;

namespace Puzzle.Editor
{
    public static class HintChecks
    {
        public static void ValidateAndBuild(){Run();GravityPrototypeChecks.Run();IphoneTestSetup.BuildGame();}
        public static void Run()
        {
            var all=new[]{"layouts","late_levels","resize_levels","blink_levels","expansion_levels"}.SelectMany(name=>JsonUtility.FromJson<LayoutDocument>(Resources.Load<TextAsset>("GravityPuzzle/"+name).text).layouts).ToArray();
            int verified=0,offRoute=0,noPath=0,bounded=0;
            foreach(var level in all)
            {
                var state=new PuzzleSnapshot{positions=level.Start,sizes=level.StartSizes};
                for(int step=0;step<level.route.Length;step++)
                {
                    if(!level.IsSolved(state.positions,state.sizes))
                    {
                        var search=Solve(level,state);Require(search.Route!=null&&search.Route.Length>0,"authored position gets hint "+level.room);Verify(level,state,search.Route);verified++;
                    }
                    var plan=TiltSimulator.Plan(level,state.positions,level.route[step],state.sizes,state.moves);
                    state=new PuzzleSnapshot{positions=plan.result,sizes=plan.sizes,moves=state.moves+1};
                }
                // Different first move for each direction exercises states outside the authored route.
                for(int d=0;d<4;d++)
                {
                    var plan=TiltSimulator.Plan(level,level.Start,d,level.StartSizes,0);
                    var alternate=new PuzzleSnapshot{positions=plan.result,sizes=plan.sizes,moves=1};
                    var search=Solve(level,alternate);
                    if(search.Route!=null){Verify(level,alternate,search.Route);offRoute++;}
                    else if(search.Exhausted)noPath++;else bounded++;
                }
            }
            var hints=new PuzzleHints();int writes=0;
            Require(hints.Remaining("one","v")==3,"old save starts with three hints");
            for(int i=0;i<3;i++)Require(hints.Reveal("one","v","state"+i,i,()=>{writes++;return true;}),"three reveals accepted");
            Require(!hints.Reveal("one","v","fourth",3,()=>{writes++;return true;})&&writes==3,"fourth unique hint blocked");
            Require(hints.Reveal("one","v","state1",1,()=>throw new Exception("duplicate must not save"))&&hints.Remaining("one","v")==0,"repeat is free after limit");
            Require(hints.Remaining("two","v")==3,"separate stage allowance");
            Require(!hints.Reveal("two","v","x",0,()=>false)&&hints.Remaining("two","v")==3,"failed save rolls back");
            try{hints.Reveal("two","v","x",0,()=>throw new IOException("test"));}catch(IOException){}
            Require(hints.Remaining("two","v")==3,"thrown save rolls back");
            var restored=PuzzleHints.Restore(JsonUtility.FromJson<ProgressData>(JsonUtility.ToJson(new ProgressData{hints=hints})).hints);
            Require(restored.Remaining("one","v")==0&&restored.Known("one","v","state2")==2,"restart and reload retain uses and clues");
            Require(PuzzleHints.Restore(JsonUtility.FromJson<ProgressData>("{}").hints).Remaining("one","v")==3,"legacy progress migration");
            var blink=all.First(l=>l.HasBlinkBlocks);Require(PuzzleHintSearch.Key(blink,blink.Start,blink.StartSizes,0)!=PuzzleHintSearch.Key(blink,blink.Start,blink.StartSizes,1),"blink turn parity is in cache key");
            var normal=all.First(l=>!l.HasBlinkBlocks);Require(PuzzleHintSearch.Key(normal,normal.Start,normal.StartSizes,0)==PuzzleHintSearch.Key(normal,normal.Start,normal.StartSizes,2),"same normal board reuses hint regardless of move count");
            var blocked=new PuzzleDefinition{room="no-route",width=3,height=3,walls=new[]{new Cell(1,0),new Cell(1,1),new Cell(1,2)},cargo=new[]{new CargoDefinition{x=0,y=0,gx=2,gy=0,size=1}},route=new[]{3}};
            var start=new PuzzleSnapshot{positions=blocked.Start,sizes=blocked.StartSizes};
            var impossible=Solve(blocked,start);Require(impossible.Exhausted&&impossible.Route==null,"no route yields no direction");
            var limited=new PuzzleHintSearch(blocked,start,1);limited.Step(100,1000);Require(limited.Done&&!limited.Exhausted&&limited.Route==null,"budget is not misreported as impossible");
            Debug.Log($"PASS hints: {all.Length} stages, {verified} authored states, {offRoute} alternate routes verified, {noPath} exhausted, {bounded} bounded; three-use cap, free recheck, persistence, failed saves, parity, impossible and limited search.");
        }
        static PuzzleHintSearch Solve(PuzzleDefinition level,PuzzleSnapshot state)
        {var s=new PuzzleHintSearch(level,state);while(!s.Done)s.Step(1000,1000);return s;}
        static void Verify(PuzzleDefinition level,PuzzleSnapshot state,int[] route)
        {
            var p=state.positions;var sizes=state.sizes;int turn=state.moves;
            foreach(int d in route){var next=TiltSimulator.Plan(level,p,d,sizes,turn++);p=next.result;sizes=next.sizes;}
            Require(level.IsSolved(p,sizes),"hint route must solve from the actual state "+level.room);
        }
        static void Require(bool ok,string why){if(!ok)throw new Exception(why);}
    }
}

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Puzzle.Gravity;
using UnityEngine;

namespace Puzzle.Editor
{
    [Serializable] public sealed class BlinkReferenceCase
    { public string room;public int direction,turn;public Cell[] before,after;public int[] beforeSizes,afterSizes; }
    [Serializable] public sealed class BlinkReferenceCases { public BlinkReferenceCase[] cases; }
    public static partial class GravityPrototypeChecks
    {
        private static void TestBlink(PuzzleDefinition[] originals,List<string> log)
        {
            var resize=JsonUtility.FromJson<LayoutDocument>(Resources.Load<TextAsset>("GravityPuzzle/resize_levels").text).layouts;
            var levels=JsonUtility.FromJson<LayoutDocument>(Resources.Load<TextAsset>("GravityPuzzle/blink_levels").text).layouts;
            var old=PuzzleCampaign.Order(originals.Concat(resize));var campaign=PuzzleCampaign.Order(old.Concat(levels));
            Check(levels.Length==16&&campaign.Length==64&&campaign.Select(l=>l.room).Distinct().Count()==64,"64 unique stages");
            Check(campaign.Take(48).Select(l=>l.room).SequenceEqual(old.Select(l=>l.room)),"existing 48 stage order retained");
            Check(campaign.Skip(48).Select(l=>l.room).SequenceEqual(levels.Select(l=>l.room)),"blink chapter is stages 49 through 64");
            Check(levels.Take(4).All(l=>l.blinkBlocks.All(b=>b.phase==0)),"first four teach one phase");
            Check(levels.Skip(4).All(l=>l.blinkBlocks.Select(b=>b.phase).Distinct().Count()==2),"later stages alternate two phases");
            Check(levels.Take(8).All(l=>!l.HasPortals&&!l.HasResizers)&&levels.Skip(8).All(l=>l.HasPortals)&&levels.Skip(12).All(l=>l.HasResizers),"mechanics introduced gradually");
            for(int index=0;index<levels.Length;index++)
            {
                var l=levels[index];l.Validate();Check(l.route.Length==index+3,"blink shortest ladder authored 3 through 18");
                var s=new PuzzleSession(l);var quick=new PuzzleSession(l);
                foreach(int direction in l.route)
                {
                    var before=s.Stable.Copy();var walls=l.TurnSolids(before.positions,before.sizes,before.moves);
                    Check(s.Command(direction)&&quick.Command(direction),"blink direction accepted");
                    if(s.Moving)
                    {
                        Check(s.Stable.moves==before.moves,"phase does not change before settling");
                        var restoredPending=new PuzzleSession(l);
                        Check(restoredPending.Restore(s.Stable.Copy(),s.StableHistory())&&restoredPending.Moves==before.moves,"mid-move save restores pre-move phase");
                    }
                    int ticks=0;
                    while(s.Moving&&ticks++<300)
                    {
                        s.Tick(1f/60);
                        Check(NoVisualOverlap(s),"blink friends never visually overlap");
                        if(s.Moving)Check(walls.SetEquals(l.TurnSolids(s.Stable.positions,s.Stable.sizes,s.Stable.moves)),"walls frozen during travel, portal and resize");
                    }
                    Check(!s.Moving&&s.Stable.moves==before.moves+1,"exactly one phase change per settled move");
                    quick.Tick(0,true);
                    Check(s.Stable.positions.SequenceEqual(quick.Stable.positions)&&s.Stable.sizes.SequenceEqual(quick.Stable.sizes)&&s.Moves==quick.Moves,"fast play preserves blink puzzle result");
                    Check(l.IsValidState(s.Stable.positions,s.Stable.sizes),"blink result has valid footprints");
                    var data=new ProgressData{dataVersion=ProgressSaveService.LevelVersion(l,"unused"),levelId=l.room,blockIds=Enumerable.Range(0,l.cargo.Length).ToArray(),sizes=l.StartSizes,current=s.Stable.Copy(),history=s.StableHistory()};
                    var restored=new PuzzleSession(l);var json=JsonUtility.FromJson<ProgressData>(JsonUtility.ToJson(data));
                    Check(ProgressSaveService.Restore(json,data.dataVersion,restored)&&restored.Moves==s.Moves&&l.TurnSolids(restored.Stable.positions,restored.Stable.sizes,restored.Moves).SetEquals(l.TurnSolids(s.Stable.positions,s.Stable.sizes,s.Moves)),"save and reload restore identical open paths");
                    Check(restored.Undo()&&restored.Moves==before.moves&&restored.Stable.positions.SequenceEqual(before.positions)&&l.TurnSolids(restored.Stable.positions,restored.Stable.sizes,restored.Moves).SetEquals(walls),"undo restores phase, size and positions");
                }
                Check(s.Solved&&quick.Solved,"blink authored route solved in actual session "+l.room);
                Check(BlinkShortest(l)==l.route.Length,"C# shortest route agrees with independent solver "+l.room);
                s.Reset();Check(s.Moves==0&&s.Stable.positions.SequenceEqual(l.Start),"reset restores phase zero");
                log.Add("PASS blink "+l.room+" / stage "+(index+49)+" / shortest "+l.route.Length);
            }
            var cases=JsonUtility.FromJson<BlinkReferenceCases>(File.ReadAllText("Assets/Editor/GravityPuzzle/blink_reference_cases.json")).cases;
            Check(cases.Length==1920,"1920 independent blink cases present");
            foreach(var c in cases)
            {
                var l=levels.First(v=>v.room==c.room);var p=TiltSimulator.Plan(l,c.before,c.direction,c.beforeSizes,c.turn);
                Check(p.result.SequenceEqual(c.after)&&p.sizes.SequenceEqual(c.afterSizes),"independent phase-aware transition parity "+c.room);
            }
            var unit=new PuzzleDefinition{room="blink-unit",family=4,width=7,height=6,walls=new Cell[0],cargo=new[]{new CargoDefinition{x=0,y=0,size=1,gx=3,gy=3}},blinkBlocks=new[]{new BlinkDefinition{x=1,y=0,phase=0}},route=new[]{1}};
            var turn=new PuzzleSession(unit);Check(turn.Command(1)&&!turn.Moving&&turn.Moves==1,"no-motion command toggles paths and counts one move");
            Check(!unit.TurnSolids(turn.Stable.positions,turn.Stable.sizes,turn.Moves).Contains(new Cell(1,0)),"no-motion opens barrier");
            Check(turn.Command(3)&&turn.Moving&&turn.Stable.moves==1,"open path can be crossed");
            turn.SetPause(PauseReason.Menu,true);turn.Tick(100);Check(turn.Stable.moves==1&&turn.Elapsed==0,"pause freezes phase and travel");
            turn.SetPause(PauseReason.Menu,false);Check(turn.Undo()&&turn.Moves==1&&!turn.Moving,"undo cancels pending phase change");
            turn.Command(3);turn.Tick(100);Check(turn.Stable.positions[0].x==6&&turn.Moves==2,"path stays open for entire traversal");
            var deferred=new PuzzleDefinition{room="blink-deferred",family=4,width=7,height=6,walls=new Cell[0],cargo=new[]{new CargoDefinition{x=2,y=1,size=1,gx=5,gy=3},new CargoDefinition{x=1,y=1,size=1,gx=4,gy=3}},blinkBlocks=new[]{new BlinkDefinition{x=2,y=1,phase=0}},route=new[]{3}};
            deferred.Validate();var plan=TiltSimulator.Plan(deferred,deferred.Start,3,deferred.StartSizes,0);
            Check(plan.result[0].x==6&&plan.result[1].x==5,"occupied barrier stays open after its occupant leaves during same turn");
            Check(!deferred.BlinkSolid(deferred.blinkBlocks[0],new[]{new Cell(1,0)},new[]{2},0),"large friend defers every covered barrier cell");
            Check(deferred.BlinkSolid(deferred.blinkBlocks[0],plan.result,plan.sizes,2),"vacant barrier closes when phase next activates");
            var portal=new PuzzleDefinition{room="blink-portal",family=4,width=7,height=6,walls=new Cell[0],cargo=new[]{new CargoDefinition{x=1,y=1,size=1,gx=4,gy=3}},portals=new[]{new PortalDefinition{id="A",x=0,y=1,face=3,exitX=6,exitY=4,exitFace=1,size=1}},blinkBlocks=new[]{new BlinkDefinition{x=5,y=4,phase=0}},route=new[]{1}};
            portal.Validate();Check(TiltSimulator.Plan(portal,portal.Start,1,null,0).blocked[0],"closed blink blocks portal exit");
            Check(TiltSimulator.Plan(portal,portal.Start,1,null,1).result[0].Equals(new Cell(5,4)),"open blink permits portal exit");
            foreach(var l in resize)
            {
                string previousJson=JsonUtility.ToJson(l).Replace(",\"blinkBlocks\":[]","");
                Check(ProgressSaveService.LevelVersion(l,"unused")==ProgressSaveService.Checksum("resize-v1|"+previousJson),"existing resize save checksum unchanged");
            }
            foreach(int badPhase in new[]{-1,2})
            {
                unit.blinkBlocks[0].phase=badPhase;bool rejected=false;try{unit.Validate();}catch(ArgumentException){rejected=true;}
                Check(rejected,"invalid blink phase rejected");
            }
            log.Add("PASS 1920 independent blink transitions; 16 C# shortest-path searches; phase, save, undo, reset, pause, deferred occupancy and portal exits");
        }
        private static int BlinkShortest(PuzzleDefinition level)
        {
            string Key(Cell[] p,int[] s,int phase)=>phase+":"+string.Join(";",p.Select((v,i)=>v.x+","+v.y+","+s[i]));
            var queue=new Queue<PuzzleSnapshot>();var first=new PuzzleSnapshot{positions=level.Start,sizes=level.StartSizes};queue.Enqueue(first);
            var visited=new HashSet<string>{Key(first.positions,first.sizes,0)};
            while(queue.Count>0)
            {
                var state=queue.Dequeue();if(level.IsSolved(state.positions,state.sizes))return state.moves;
                if(state.moves>=level.route.Length)continue;
                for(int direction=0;direction<4;direction++)
                {
                    var p=TiltSimulator.Plan(level,state.positions,direction,state.sizes,state.moves);
                    if(visited.Add(Key(p.result,p.sizes,(state.moves+1)&1)))queue.Enqueue(new PuzzleSnapshot{positions=p.result,sizes=p.sizes,moves=state.moves+1});
                }
                if(visited.Count>50000)throw new Exception("Unexpected blink search size");
            }
            return -1;
        }
    }
}

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Puzzle.Gravity;
using UnityEngine;

namespace Puzzle.Editor
{
    [Serializable] public sealed class ResizeReferenceCase
    { public string room;public int direction;public Cell[] before,after;public int[] beforeSizes,afterSizes;public bool[] blocked; }
    [Serializable] public sealed class ResizeReferenceCases { public ResizeReferenceCase[] cases; }
    public static partial class GravityPrototypeChecks
    {
        private static void TestResize(PuzzleDefinition[] originals,List<string> log)
        {
            var levels=JsonUtility.FromJson<LayoutDocument>(Resources.Load<TextAsset>("GravityPuzzle/resize_levels").text).layouts;
            var campaign=PuzzleCampaign.Order(originals.Concat(levels));
            Check(campaign.Length==48&&campaign.Select(l=>l.room).Distinct().Count()==48,"48 unique stages");
            Check(campaign.Take(32).Select(l=>l.room).SequenceEqual(PuzzleCampaign.Order(originals).Select(l=>l.room)),"original 32 stage order retained");
            Check(campaign.Skip(32).Select(l=>l.room).SequenceEqual(levels.Select(l=>l.room)),"resize chapter comes last even without portals");
            int previous=0;
            foreach(var l in levels)
            {
                l.Validate();Check(l.HasResizers&&l.route.Length>previous,"resize difficulty rises");previous=l.route.Length;
                var s=new PuzzleSession(l);bool changed=false,grew=false,shrank=false,warped=false;
                foreach(int d in l.route)
                {
                    var before=s.Stable.Copy();Check(s.Command(d),"resize route accepts move");
                    changed|=s.Plan.HasResize;warped|=s.Plan.entered.Where((p,i)=>p>=0&&!s.Plan.blocked[i]).Any();
                    for(int i=0;i<before.sizes.Length;i++){grew|=s.Plan.sizes[i]>before.sizes[i];shrank|=s.Plan.sizes[i]<before.sizes[i];}
                    var restored=new PuzzleSession(l);
                    Check(restored.Restore(s.Stable.Copy(),s.StableHistory()),"resize mid-animation checkpoint remains restorable");
                    while(s.Moving)
                    {
                        s.Tick(1f/60);Check(NoVisualOverlap(s),"resize animation footprints never overlap");
                        for(int i=0;i<before.sizes.Length;i++)Check(s.VisualSize(i)>=1&&s.VisualSize(i)<=2,"size stays in valid visual range");
                    }
                    Check(l.IsValidState(s.Stable.positions,s.Stable.sizes),"dynamic settled footprints valid");
                    var data=new ProgressData{dataVersion="resize-test",levelId=l.room,blockIds=Enumerable.Range(0,l.cargo.Length).ToArray(),sizes=l.StartSizes,current=s.Stable.Copy(),history=s.StableHistory()};
                    var roundTrip=JsonUtility.FromJson<ProgressData>(JsonUtility.ToJson(data));
                    Check(ProgressSaveService.Restore(roundTrip,"resize-test",restored)&&restored.Stable.positions.SequenceEqual(s.Stable.positions)&&restored.Stable.sizes.SequenceEqual(s.Stable.sizes),"size and positions survive JSON save/load");
                    Check(restored.Undo()&&restored.Stable.positions.SequenceEqual(before.positions)&&restored.Stable.sizes.SequenceEqual(before.sizes),"undo restores size and position together");
                }
                Check(s.Solved&&changed,"resize solution uses transformation");
                if(l.room!="R01"&&l.room!="R02")Check(grew&&shrank,"advanced routes use both grow and shrink");
                if(l.HasPortals)Check(warped,"mixed route actually uses portal");
                Check(!l.IsSolved(l.Goal,l.GoalSizes.Select(v=>v==1?2:1).ToArray()),"goal requires matching size");
                s.Reset();Check(s.Stable.sizes.SequenceEqual(l.StartSizes),"reset restores starting size");
                var invalid=s.Stable.Copy();invalid.sizes[0]=3;Check(!s.Restore(invalid,new PuzzleSnapshot[0]),"reject invalid saved size");
                invalid=s.Stable.Copy();invalid.sizes=null;Check(!s.Restore(invalid,new PuzzleSnapshot[0]),"resize save cannot omit sizes");
                log.Add("PASS resize "+l.room+" / shortest "+l.route.Length);
            }
            var cases=JsonUtility.FromJson<ResizeReferenceCases>(File.ReadAllText("Assets/Editor/GravityPuzzle/resize_reference_cases.json")).cases;
            foreach(var c in cases)
            {
                var l=levels.First(v=>v.room==c.room);var p=TiltSimulator.Plan(l,c.before,c.direction,c.beforeSizes);
                Check(p.result.SequenceEqual(c.after)&&p.sizes.SequenceEqual(c.afterSizes)&&p.resizeBlocked.SequenceEqual(c.blocked),"independent resize parity "+c.room);
            }
            var sample=new PuzzleDefinition{room="resize-unit",width=7,height=7,family=4,walls=new Cell[0],cargo=new[]{new CargoDefinition{x=1,y=1,size=1,gx=4,gy=4,goalSize=2},new CargoDefinition{x=2,y=1,size=1,gx=6,gy=6}},resizers=new[]{new ResizeDefinition{x=1,y=0,targetSize=2}},route=new[]{0}};
            sample.Validate();var plan=TiltSimulator.Plan(sample,sample.Start,0);
            Check(plan.resizeBlocked[0]&&plan.sizes[0]==1,"occupied expansion keeps original size");
            var single=new PuzzleDefinition{room="resize-single",width=sample.width,height=sample.height,family=4,walls=sample.walls,cargo=sample.cargo.Take(1).ToArray(),resizers=sample.resizers,route=sample.route};
            var session=new PuzzleSession(single);session.Command(0);
            session.Tick((float)Math.Sqrt(1/6f)+.1f);Check(session.InResizePhase&&session.VisualSize(0)>1&&session.VisualSize(0)<2,"smooth resize phase");
            float elapsed=session.Elapsed;session.SetPause(PauseReason.Menu,true);session.Tick(100);Check(session.Elapsed==elapsed,"pause freezes resize");
            session.SetPause(PauseReason.Menu,false);Check(session.Undo()&&session.Stable.sizes[0]==1,"undo cancels resizing");session.Command(0);session.Tick(0,true);Check(!session.Moving&&session.Stable.sizes[0]==2,"reduced motion applies size immediately");
            var portalSize=new PuzzleDefinition{room="resize-portal-size",width=7,height=7,family=4,walls=new Cell[0],cargo=new[]{new CargoDefinition{x=1,y=1,size=1,gx=4,gy=4,goalSize=2}},portals=new[]{new PortalDefinition{id="A",x=0,y=1,face=3,exitX=6,exitY=4,exitFace=1,size=1}},route=new[]{1}};
            portalSize.Validate();
            Check(TiltSimulator.Plan(portalSize,portalSize.Start,1,new[]{1}).entered[0]==0,"small friend enters small portal");
            Check(TiltSimulator.Plan(portalSize,portalSize.Start,1,new[]{2}).entered[0]==-1,"grown friend cannot enter small portal");
            var slow=new PuzzleSession(single);var fast=new PuzzleSession(single);slow.Command(0);fast.Command(0);
            for(int i=0;i<16;i++)slow.Tick(1f/30);for(int i=0;i<32;i++)fast.Tick(1f/60);
            Check(Math.Abs(slow.VisualSize(0)-fast.VisualSize(0))<.0001f,"resize curve agrees at 30 and 60 fps");
            var legacy=new PuzzleSession(originals[0]);var old=legacy.Stable.Copy();old.sizes=null;Check(legacy.Restore(old,new PuzzleSnapshot[0])&&legacy.Stable.sizes.SequenceEqual(originals[0].StartSizes),"old snapshots migrate initial sizes");
            foreach(var l in originals.Where(l=>l.HasPortals))
            {
                string oldJson=JsonUtility.ToJson(l).Replace(",\"blinkBlocks\":[]","").Replace(",\"resizers\":[]","").Replace(",\"goalSize\":0","");
                Check(!oldJson.Contains("resizers")&&ProgressSaveService.LevelVersion(l,"unused")==ProgressSaveService.Checksum("face-portal-v2|"+oldJson),"old portal checksum preserved");
            }
            log.Add("PASS independent resize parity: "+cases.Length+" cases; dynamic size, save, undo, reset, pause, goals, collision and legacy migration");
        }
    }
}

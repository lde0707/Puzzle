using System;
using System.Linq;
using Puzzle.Gravity;
using UnityEngine;

namespace Puzzle.Editor
{
    public static class AuditLifetimeStarsChecks
    {
        static void Require(bool value,string message){if(!value)throw new Exception(message);}
        public static void ValidateAndBuild()
        {
            var c=new PuzzleCollection();
            for(int i=0;i<12;i++)c.Award("stage"+i,3,3,false);
            Require(c.TotalEarnedStars==36,"earned total begins at awarded amount");
            Require(c.TryDraw(_=>0,()=>true,out _)&&c.wallet==33&&c.TotalEarnedStars==36,"single draw keeps lifetime total");
            Require(c.TryDrawMany(10,_=>0,()=>true,out _)&&c.wallet==3&&c.TotalEarnedStars==36,"ten draws keep lifetime total");
            Require(!c.TryDraw(_=>0,()=>false,out _)&&c.wallet==3&&c.TotalEarnedStars==36,"failed save rolls back spend and total");
            Require(c.Award("stage0",3,3,false)==0&&c.TotalEarnedStars==36,"repeat clears do not double count");
            c.Award("improve",12,3,true);Require(c.TotalEarnedStars==37,"one-star clear adds one");
            c.Award("improve",3,3,false);Require(c.TotalEarnedStars==39,"improvement adds only two");
            var saved=JsonUtility.FromJson<PuzzleCollection>(JsonUtility.ToJson(c));
            saved=PuzzleCollection.Restore(saved,Array.Empty<string>(),saved.records.Select(r=>r.id));
            Require(saved.TotalEarnedStars==39,"save reload keeps spent rewards");
            var legacy=PuzzleCollection.Restore(new PuzzleCollection{wallet=2,owned=new[]{0,1,2}},Array.Empty<string>(),Array.Empty<string>());
            Require(legacy.TotalEarnedStars==8,"old incomplete records recover wallet plus six spent stars");
            legacy.records=new[]{new StageStars{id="historical",stars=3},new StageStars{id="other",stars=3},new StageStars{id="third",stars=3}};
            Require(legacy.TotalEarnedStars==9,"historical records remain a minimum");
            var snapshot=CommunityBridge.Snapshot(new PuzzleActivity(),saved);
            Require(snapshot.schema==2&&snapshot.stars==39&&snapshot.wallet==6&&snapshot.spentStars==33,"wire payload distinguishes lifetime from wallet");
            Debug.Log("PASS lifetime stars: single/ten draws, save failure, repeat clear, rating improvement, reload, legacy spent-star recovery and sync payload.");
            
        }
    }
}

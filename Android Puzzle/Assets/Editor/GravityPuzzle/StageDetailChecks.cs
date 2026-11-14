using System;
using System.Linq;
using Puzzle.Gravity;
using UnityEngine;
namespace Puzzle.Editor
{
    public static class StageDetailChecks
    {
        public static void ValidateAndBuild()
        {
            var c=new PuzzleCollection();
            c.Award("A",4,4,false,true);Require(!c.records[0].perfect,"hint excludes perfect");
            c.Award("A",4,4,true,false);Require(!c.records[0].perfect,"undo excludes perfect");
            c.Award("A",5,4,false,false);Require(!c.records[0].perfect,"three stars are not perfect");
            c.Award("A",4,4,false,false);Require(c.records[0].perfect,"minimum unaided clear is perfect");
            c.Award("A",10,4,true,true);Require(c.records[0].perfect,"perfect achievement is retained");
            var restored=PuzzleCollection.Restore(JsonUtility.FromJson<PuzzleCollection>(JsonUtility.ToJson(c)),new[]{"A"},new[]{"A"});
            Require(StageRating.Evaluate(restored.records[0],4)=="0.1%","perfect survives reload");
            var legacy=PuzzleCollection.Restore(new PuzzleCollection{records=new[]{new StageStars{id="A",stars=3,bestMoves=4}}},new[]{"A"},new[]{"A"});
            Require(!legacy.records[0].perfect,"legacy independent star and move records cannot prove perfect");
            int[] moves={100,110,140,180,250,350,500,700};
            for(int i=0;i<moves.Length;i++)Require(StageRating.Evaluate(new StageStars{bestMoves=moves[i]},100)==StageRating.Bands[i],"fallback band "+i);
            Require(StageRating.Evaluate(null,4)=="100%","unplayed starts at 100");
            var data=new StageTierData{players=30,thresholds=new[]{1,5,10,30,50,70,99}};
            for(int i=0;i<7;i++)Require(StageRating.Evaluate(new StageStars{bestMoves=data.thresholds[i]},1,data)==StageRating.Bands[i],"population band "+i);
            Require(StageRating.Evaluate(new StageStars{bestMoves=100},1,data)=="100%","last population band");
            data.players=29;Require(!StageRating.HasData(data),"small cohorts use fallback");
            var save=JsonUtility.FromJson<ProgressData>(JsonUtility.ToJson(new ProgressData{usedHint=true,hintAudit=true}));
            Require(save.usedHint&&save.hintAudit,"hint usage survives continuation");
            Debug.Log("PASS stage detail: perfect rules, persistence, legacy safety and all nine grade bands.");
            IphoneTestSetup.BuildGame();
        }
        private static void Require(bool value,string message){if(!value)throw new Exception(message);}
    }
}

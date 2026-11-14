using System;
using System.Linq;

namespace Puzzle.Gravity
{
    [Serializable] public sealed class StageTierData { public string room;public int players;public int[] thresholds; }
    [Serializable] public sealed class StageTierCatalog { public StageTierData[] rows=Array.Empty<StageTierData>(); }
    public static class StageRating
    {
        public const int MinimumPlayers=30;
        public static readonly string[] Bands={"1%","5%","10%","30%","50%","70%","99%","100%"};
        private static readonly double[] Ratios={1.05,1.25,1.5,2,3,4,6};
        public static bool HasData(StageTierData data)=>data!=null&&data.players>=MinimumPlayers&&data.thresholds!=null&&data.thresholds.Length==7&&data.thresholds.All(n=>n>0)&&data.thresholds.Zip(data.thresholds.Skip(1),(a,b)=>a<=b).All(v=>v);
        public static string Evaluate(StageStars record,int optimum,StageTierData data=null)
        {
            if(record==null||record.bestMoves<=0)return "100%";
            if(record.perfect)return "0.1%";
            for(int i=0;i<7;i++)
                if(HasData(data)?record.bestMoves<=data.thresholds[i]:(double)record.bestMoves/Math.Max(1,optimum)<=Ratios[i])return Bands[i];
            return "100%";
        }
    }
}

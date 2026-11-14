using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Puzzle.Gravity
{
    [Serializable] public sealed class HintClue { public string state;public int direction; }
    [Serializable] public sealed class StageHints { public string room,version;public HintClue[] clues=new HintClue[0]; }
    [Serializable] public sealed class PuzzleHints
    {
        public const int Limit=3;
        public StageHints[] stages=new StageHints[0];
        public static PuzzleHints Restore(PuzzleHints data)
        {
            var result=new PuzzleHints();
            result.stages=(data?.stages??Array.Empty<StageHints>()).Where(s=>s!=null&&!string.IsNullOrEmpty(s.room)&&!string.IsNullOrEmpty(s.version))
                .GroupBy(s=>s.room+"/"+s.version).Select(g=>new StageHints{room=g.First().room,version=g.First().version,
                    clues=g.SelectMany(s=>s.clues??Array.Empty<HintClue>()).Where(c=>c!=null&&!string.IsNullOrEmpty(c.state)&&c.direction>=0&&c.direction<4).GroupBy(c=>c.state).Select(x=>x.First()).TakeLast(256).ToArray()}).ToArray();
            return result;
        }
        private StageHints Stage(string room,string version)=>stages.FirstOrDefault(s=>s.room==room&&s.version==version);
        public int Remaining(string room,string version)=>Limit-(Stage(room,version)?.clues.Length??0);
        public int Known(string room,string version,string key)=>Stage(room,version)?.clues.FirstOrDefault(c=>c.state==key)?.direction??-1;
        public bool Reveal(string room,string version,string key,int direction,Func<bool> save,bool accountBudget=false)
        {
            if(direction<0||direction>3)return false;
            if(Known(room,version,key)>=0)return true;
            if(!accountBudget&&Remaining(room,version)<=0)return false;
            var previous=stages;var stage=Stage(room,version);
            var replacement=new StageHints{room=room,version=version,clues=(stage?.clues??Array.Empty<HintClue>()).Append(new HintClue{state=key,direction=direction}).TakeLast(256).ToArray()};
            stages=stages.Where(s=>s!=stage).Append(replacement).ToArray();bool saved=false;
            try{saved=save();return saved;}finally{if(!saved)stages=previous;}
        }
    }
    // Incremental search uses the exact game simulator, including sizes and blink parity.
    public sealed class PuzzleHintSearch
    {
        private sealed class Node { public Cell[] p;public int[] sizes;public int turn,parent,direction; }
        private readonly PuzzleDefinition level;
        private readonly int limit;
        private readonly List<Node> nodes=new List<Node>();
        private readonly HashSet<string> visited=new HashSet<string>();
        private readonly Dictionary<string,int[]> guide=new Dictionary<string,int[]>();
        private int head;
        public bool Done {get;private set;}
        public bool Exhausted {get;private set;}
        public int[] Route {get;private set;}
        public int Visited=>nodes.Count;
        public static string Key(PuzzleDefinition level,Cell[] p,int[] sizes,int moves)=>(level.HasBlinkBlocks?moves&1:0)+":"+string.Join(";",p.Select((c,i)=>c.x+","+c.y+","+sizes[i]));
        public PuzzleHintSearch(PuzzleDefinition level,PuzzleSnapshot state,int limit=50000)
        {
            this.level=level;this.limit=limit;
            // Verified authored suffixes speed up hints near the intended route.
            var p=level.Start;var sizes=level.StartSizes;var keys=new List<string>();
            for(int i=0;i<level.route.Length;i++){keys.Add(Key(level,p,sizes,i));var plan=TiltSimulator.Plan(level,p,level.route[i],sizes,i);p=plan.result;sizes=plan.sizes;}
            if(level.IsSolved(p,sizes))for(int i=keys.Count-1;i>=0;i--)if(!guide.ContainsKey(keys[i]))guide.Add(keys[i],level.route.Skip(i).ToArray());
            var root=new Node{p=(Cell[])state.positions.Clone(),sizes=(int[])state.sizes.Clone(),turn=state.moves,parent=-1,direction=-1};
            nodes.Add(root);visited.Add(Key(level,root.p,root.sizes,root.turn));
        }
        public void Step(int maxNodes=64,double milliseconds=2)
        {
            if(Done)return;var timer=Stopwatch.StartNew();int count=0;
            while(head<nodes.Count&&count++<maxNodes)
            {
                int index=head++;var n=nodes[index];string key=Key(level,n.p,n.sizes,n.turn);
                if(level.IsSolved(n.p,n.sizes)){Finish(index,Array.Empty<int>());return;}
                if(guide.TryGetValue(key,out var suffix)){Finish(index,suffix);return;}
                for(int d=0;d<4;d++)
                {
                    var plan=TiltSimulator.Plan(level,n.p,d,n.sizes,n.turn);string next=Key(level,plan.result,plan.sizes,n.turn+1);
                    if(!visited.Add(next))continue;
                    if(nodes.Count>=limit){Done=true;return;}
                    nodes.Add(new Node{p=plan.result,sizes=plan.sizes,turn=n.turn+1,parent=index,direction=d});
                }
                if(timer.Elapsed.TotalMilliseconds>=milliseconds)return;
            }
            if(head>=nodes.Count){Done=true;Exhausted=true;}
        }
        private void Finish(int index,int[] suffix)
        {
            var path=new List<int>();while(nodes[index].parent>=0){path.Add(nodes[index].direction);index=nodes[index].parent;}path.Reverse();path.AddRange(suffix);
            Route=path.ToArray();Done=true;
        }
    }
}

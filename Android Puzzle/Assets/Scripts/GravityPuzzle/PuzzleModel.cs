using System;
using System.Collections.Generic;
using System.Linq;

namespace Puzzle.Gravity
{
    [Serializable] public struct Cell : IEquatable<Cell>
    {
        public int x, y;
        public Cell(int x, int y) { this.x = x; this.y = y; }
        public bool Equals(Cell other) => x == other.x && y == other.y;
        public override bool Equals(object obj) => obj is Cell c && Equals(c);
        public override int GetHashCode() => x * 397 ^ y;
    }
    [Serializable] public sealed class CargoDefinition { public int x, y, size, gx, gy, goalSize; }
    [Serializable] public sealed class BlinkDefinition { public int x,y,phase; }
    [Serializable] public sealed class ResizeDefinition { public int x,y,targetSize; }
    [Serializable] public sealed class PortalDefinition
    {
        public string id;
        public int x,y,face,exitX,exitY,exitFace,size=1;
        public Cell Origin(int side)=>side==0?new Cell(x,y):new Cell(exitX,exitY);
        public int Face(int side)=>side==0?face:exitFace;
        public Cell Contact(int side) { var origin=Origin(side);var axis=TiltSimulator.Axes[Face(side)];return new Cell(origin.x+axis.x*size,origin.y+axis.y*size); }
        public int Approach(int side)=>(Face(side)+2)%4;
    }
    [Serializable] public sealed class LayoutDocument { public PuzzleDefinition[] layouts; }
    [Serializable] public sealed class PuzzleDefinition
    {
        public string room, displayName, lesson;
        public PortalDefinition[] portals;
        public ResizeDefinition[] resizers;
        public BlinkDefinition[] blinkBlocks;
        public bool HasBlinkBlocks => blinkBlocks!=null&&blinkBlocks.Length>0;
        public bool HasPortals => portals!=null && portals.Length>0;
        public bool HasResizers => resizers!=null && resizers.Length>0;
        public int family, width, height;
        public Cell[] walls;
        public CargoDefinition[] cargo;
        public int[] route;
        public Cell[] Start => cargo.Select(c => new Cell(c.x, c.y)).ToArray();
        public int[] StartSizes => cargo.Select(c=>c.size).ToArray();
        public int[] GoalSizes => cargo.Select(c=>c.goalSize==0?c.size:c.goalSize).ToArray();
        private Cell[] cachedGoal;
        public Cell[] Goal => cachedGoal ?? (cachedGoal = cargo.Select(c => new Cell(c.gx, c.gy)).ToArray());
        public bool IsSolved(Cell[] state,int[] sizes=null) => state != null && state.SequenceEqual(Goal) && (sizes??StartSizes).SequenceEqual(GoalSizes);
        public HashSet<Cell> SolidCells()
        {
            var result=new HashSet<Cell>(walls);
            if(HasPortals)foreach(var p in portals)
            {
                if(p==null || p.size<1 || p.size>2)continue;
                for(int side=0;side<2;side++)for(int x=0;x<p.size;x++)for(int y=0;y<p.size;y++)
                { var origin=p.Origin(side);result.Add(new Cell(origin.x+x,origin.y+y)); }
            }
            return result;
        }
        public bool BlinkSolid(BlinkDefinition block,Cell[] positions,int[] sizes,int turn)
        {
            if(block.phase!=(turn&1))return false;
            for(int i=0;i<positions.Length;i++)
                if(block.x>=positions[i].x&&block.x<positions[i].x+sizes[i]&&block.y>=positions[i].y&&block.y<positions[i].y+sizes[i])return false;
            return true;
        }
        public HashSet<Cell> TurnSolids(Cell[] positions,int[] sizes,int turn)
        {
            var solids=SolidCells();
            if(HasBlinkBlocks)foreach(var block in blinkBlocks)if(BlinkSolid(block,positions,sizes,turn))solids.Add(new Cell(block.x,block.y));
            return solids;
        }
        public bool IsValidState(Cell[] state,int[] sizes=null)
        {
            if (state == null || cargo == null || state.Length != cargo.Length) return false;
            sizes=sizes??StartSizes;
            if(sizes.Length!=state.Length||sizes.Any(s=>s!=1&&s!=2))return false;
            var occupied = SolidCells();
            for (int i = 0; i < state.Length; i++)
                for (int x = 0; x < sizes[i]; x++)
                    for (int y = 0; y < sizes[i]; y++)
                    {
                        var cell = new Cell(state[i].x + x, state[i].y + y);
                        if (cell.x < 0 || cell.y < 0 || cell.x >= width || cell.y >= height || !occupied.Add(cell)) return false;
                    }
            return true;
        }
        public void Validate()
        {
            if (string.IsNullOrEmpty(room) || width < 1 || height < 1 || width > 32 || height > 32 || family < 0 || family > 4 ||
                walls == null || cargo == null || cargo.Length == 0 || cargo.Length > 16 || cargo.Any(c => c == null || (c.size != 1 && c.size != 2) || c.goalSize<0 || c.goalSize>2))
                throw new ArgumentException("Invalid puzzle definition: " + room);
            if (walls.Distinct().Count() != walls.Length || walls.Any(c => c.x < 0 || c.y < 0 || c.x >= width || c.y >= height) ||
                !IsValidState(Start) || !IsValidState(Goal,GoalSizes) || IsSolved(Start) || route == null || route.Length == 0 || route.Any(d => d < 0 || d > 3))
                throw new ArgumentException("Invalid puzzle layout: " + room);
            if(HasBlinkBlocks)
            {
                var seen=new HashSet<Cell>();var fixedWalls=SolidCells();
                foreach(var b in blinkBlocks)
                {
                    if(b==null||b.phase<0||b.phase>1||b.x<0||b.y<0||b.x>=width||b.y>=height||fixedWalls.Contains(new Cell(b.x,b.y))||!seen.Add(new Cell(b.x,b.y)))
                        throw new ArgumentException("Invalid blink block: "+room);
                    if(HasResizers&&resizers.Any(p=>p!=null&&b.x>=p.x&&b.x<p.x+2&&b.y>=p.y&&b.y<p.y+2))throw new ArgumentException("Blink block overlaps resize pad: "+room);
                }
            }
            if(HasResizers)
            {
                var pads=new HashSet<Cell>();var solid=SolidCells();
                foreach(var pad in resizers)
                {
                    if(pad==null||(pad.targetSize!=1&&pad.targetSize!=2))throw new ArgumentException("Invalid resize pad: "+room);
                    for(int x=0;x<2;x++)for(int y=0;y<2;y++)
                    {
                        var p=new Cell(pad.x+x,pad.y+y);
                        if(p.x<0||p.y<0||p.x>=width||p.y>=height||solid.Contains(p)||!pads.Add(p))throw new ArgumentException("Invalid resize footprint: "+room);
                    }
                }
            }
            if(HasPortals)
            {
                var ids=new HashSet<string>();var endpoints=new HashSet<Cell>();var wallSet=new HashSet<Cell>(walls);
                foreach(var portal in portals)
                {
                    if(portal==null || string.IsNullOrEmpty(portal.id) || !ids.Add(portal.id) || (portal.size!=1&&portal.size!=2) || portal.face<0 || portal.face>3 || portal.exitFace<0 || portal.exitFace>3)
                        throw new ArgumentException("Invalid portal identity: "+room);
                    foreach(var origin in new[]{new Cell(portal.x,portal.y),new Cell(portal.exitX,portal.exitY)})
                        for(int x=0;x<portal.size;x++)for(int y=0;y<portal.size;y++)
                        {
                            var c=new Cell(origin.x+x,origin.y+y);
                            if(c.x<0||c.y<0||c.x>=width||c.y>=height||!endpoints.Add(c))
                                throw new ArgumentException("Overlapping or invalid portal footprint: "+room);
                        }
                }
                var solid=SolidCells();var arrivals=new HashSet<Cell>();
                foreach(var portal in portals)for(int side=0;side<2;side++)
                {
                    var origin=portal.Contact(side);
                    for(int x=0;x<portal.size;x++)for(int y=0;y<portal.size;y++)
                    {
                        var c=new Cell(origin.x+x,origin.y+y);
                        if(c.x<0||c.y<0||c.x>=width||c.y>=height||solid.Contains(c)||!arrivals.Add(c))
                            throw new ArgumentException("Blocked or overlapping portal landing: "+room);
                    }
                }
            }
        }
    }
    public static class PuzzleCampaign
    {
        // Stable sorting preserves authored order for equally ranked puzzles; IDs remain unchanged.
        public static PuzzleDefinition[] Order(IEnumerable<PuzzleDefinition> source)=>source
            .OrderBy(level=>level.HasBlinkBlocks?3:level.HasResizers?2:level.HasPortals?1:0)
            .ThenBy(level=>level.route.Length)
            .ThenBy(level=>level.cargo.Length)
            .ThenBy(level=>level.cargo.Max(c=>c.size))
            .ThenBy(level=>level.width*level.height).ToArray();
    }
    public sealed class TiltPlan
    {
        public Cell[] slide, result;
        public int[] entered;
        public bool[] blocked;
        public int[] sizes;
        public bool[] resized,resizeBlocked;
        public bool HasResize => resized.Any(b=>b);
        public bool HasEntries => entered.Any(i=>i>=0);
        public bool HasBlocked => blocked.Any(b=>b);
    }
    public static class TiltSimulator
    {
        public static readonly Cell[] Axes = { new Cell(0,-1), new Cell(-1,0), new Cell(0,1), new Cell(1,0) };
        public static Cell[] Tilt(PuzzleDefinition level,Cell[] state,int direction) => Plan(level,state,direction).result;
        public static TiltPlan Plan(PuzzleDefinition level,Cell[] state,int direction,int[] sizes=null,int turn=0)
        {
            sizes=sizes??level.StartSizes;
            if(direction<0||direction>3||!level.IsValidState(state,sizes))throw new ArgumentException("Invalid tilt input");
            Cell axis=Axes[direction];var result=(Cell[])state.Clone();
            // Freeze this turn's walls before any friend moves. Deferred cells stay open for the entire turn.
            var turnWalls=level.TurnSolids(state,sizes,turn);
            var plan=new TiltPlan{entered=Enumerable.Repeat(-1,state.Length).ToArray(),blocked=new bool[state.Length],sizes=(int[])sizes.Clone(),resized=new bool[state.Length],resizeBlocked=new bool[state.Length]};
            // Preserve the original front-first movement contract, with original-index ties.
            var order=Enumerable.Range(0,result.Length).OrderByDescending(i=>state[i].x*axis.x+state[i].y*axis.y).ThenBy(i=>i);
            foreach(int i in order)
            {
                var occupied=Occupied(turnWalls,result,i,sizes);
                for(int step=0;step<Math.Max(level.width,level.height);step++)
                {
                    int portal=-1;
                    if(level.HasPortals)for(int k=0;k<level.portals.Length;k++)for(int side=0;side<2;side++)
                    {
                        var p=level.portals[k];
                        if(p.size==sizes[i]&&p.Approach(side)==direction&&p.Contact(side).Equals(result[i]))portal=k*2+side;
                    }
                    if(portal>=0){plan.entered[i]=portal;break;}
                    var next=new Cell(result[i].x+axis.x,result[i].y+axis.y);
                    if(!Fits(level,next,sizes[i],occupied))break;
                    result[i]=next;
                }
            }
            plan.slide=(Cell[])result.Clone();
            // Test every exit against the same settled board. No chained vacancy, swapping, or loops.
            for(int i=0;i<result.Length;i++)
            {
                int index=plan.entered[i];if(index<0)continue;
                var portal=level.portals[index/2];var exit=portal.Contact(1-index%2);
                if(Fits(level,exit,sizes[i],Occupied(turnWalls,plan.slide,i,sizes)))result[i]=exit;
                else plan.blocked[i]=true;
            }
            // Resize only on the settled anchor, after portals; never continue sliding this turn.
            if(level.HasResizers)for(int i=0;i<result.Length;i++)
            {
                var pad=level.resizers.FirstOrDefault(p=>p.x==result[i].x&&p.y==result[i].y);
                if(pad==null||pad.targetSize==sizes[i])continue;
                if(Fits(level,result[i],pad.targetSize,Occupied(turnWalls,result,i,sizes)))plan.sizes[i]=pad.targetSize;
                else plan.resizeBlocked[i]=true;
            }
            // Simultaneous expansions may not claim the same free cell; reject both.
            for(int i=0;i<result.Length;i++)for(int j=i+1;j<result.Length;j++)
                if(result[i].x+plan.sizes[i]>result[j].x&&result[j].x+plan.sizes[j]>result[i].x&&result[i].y+plan.sizes[i]>result[j].y&&result[j].y+plan.sizes[j]>result[i].y)
                { plan.resizeBlocked[i]=plan.resizeBlocked[j]=true; }
            for(int i=0;i<result.Length;i++){if(plan.resizeBlocked[i])plan.sizes[i]=sizes[i];plan.resized[i]=plan.sizes[i]!=sizes[i];}
            plan.result=result;return plan;
        }
        private static HashSet<Cell> Occupied(HashSet<Cell> turnWalls,Cell[] state,int except,int[] sizes)
        {
            var occupied=new HashSet<Cell>(turnWalls);
            for(int j=0;j<state.Length;j++)if(j!=except)
                for(int x=0;x<sizes[j];x++)for(int y=0;y<sizes[j];y++)
                    occupied.Add(new Cell(state[j].x+x,state[j].y+y));
            return occupied;
        }
        private static bool Fits(PuzzleDefinition level,Cell origin,int size,HashSet<Cell> occupied)
        {
            for(int x=0;x<size;x++)for(int y=0;y<size;y++)
            {
                var c=new Cell(origin.x+x,origin.y+y);
                if(c.x<0||c.y<0||c.x>=level.width||c.y>=level.height||occupied.Contains(c))return false;
            }
            return true;
        }
    }
    [Serializable] public sealed class PuzzleSnapshot
    {
        public Cell[] positions;
        public int[] sizes;
        public int direction, moves;
        public PuzzleSnapshot Copy() => new PuzzleSnapshot { positions=(Cell[])positions.Clone(), sizes=sizes==null?null:(int[])sizes.Clone(),direction=direction, moves=moves };
    }
    [Flags] public enum PauseReason { None=0, Menu=1, Reset=2, Inactive=4, Levels=8 }
    public sealed class PuzzleSession
    {
        public PuzzleDefinition Level { get; private set; }
        public PuzzleSnapshot Stable { get; private set; }
        public List<PuzzleSnapshot> History { get; private set; } = new List<PuzzleSnapshot>();
        public Cell[] Target { get; private set; }
        public TiltPlan Plan { get; private set; }
        public const float PortalDuration=.84f;
        public const float ResizeDuration=.32f;
        private float TravelDuration => (float)Math.Sqrt(maximumDistance/6f);
        private float ResizeStart => TravelDuration+(Plan.HasEntries?PortalDuration:0);
        public bool InResizePhase => Moving&&Plan.HasResize&&Elapsed>=ResizeStart;
        public float ResizeProgress => InResizePhase?Math.Max(0,Math.Min(1,(Elapsed-ResizeStart)/ResizeDuration)):0;
        public float VisualSize(int index) { float p=ResizeProgress;p=p*p*(3-2*p);return Moving?Stable.sizes[index]+(Plan.sizes[index]-Stable.sizes[index])*p:Stable.sizes[index]; }
        public bool InPortalPhase => Moving && Plan!=null && Plan.HasEntries && CommonDistance>=maximumDistance && Elapsed<TravelDuration+PortalDuration;
        public float PortalProgress => InPortalPhase?Math.Max(0,Math.Min(1,(Elapsed-TravelDuration)/PortalDuration)):0;
        public float VisualPortalScale(int index) => InPortalPhase && Plan.entered[index]>=0 && !Plan.blocked[index] ? PortalOpenness(PortalProgress):1;
        public static float PortalOpenness(float progress)
        {
            float p=Math.Max(0,Math.Min(1,progress));
            float Smooth(float t)=>t*t*(3-2*t);
            if(p<.44f)return 1-Smooth(p/.44f);
            if(p<=.52f)return 0; // Change endpoints only while fully hidden.
            return Smooth((p-.52f)/.48f);
        }
        public float Elapsed { get; private set; }
        public bool Moving { get; private set; }
        public PauseReason Pauses { get; private set; }
        public bool Paused => Pauses != PauseReason.None;
        public bool Solved => !Moving && Level.IsSolved(Stable.positions,Stable.sizes);
        public bool Ready => !Paused && !Moving && !Solved;
        public int Direction => Moving ? pendingDirection : Stable.direction;
        public int Moves => Stable.moves + (Moving ? 1 : 0);
        // Attempt metadata must not rewind with the board's undo history.
        public bool UsedUndo { get; private set; }
        public float CommonDistance => 6f * Elapsed * Elapsed; // 12 cells / second squared.
        private int pendingDirection;
        private int maximumDistance;
        public PuzzleSession(PuzzleDefinition level) { Level = level; level.Validate(); Reset(); }
        public void SetPause(PauseReason reason, bool value) { if (value) Pauses |= reason; else Pauses &= ~reason; }
        public bool Command(int direction)
        {
            if (!Ready || direction < 0 || direction > 3) return false;
            Plan=TiltSimulator.Plan(Level,Stable.positions,direction,Stable.sizes,Stable.moves);
            Target=Plan.result;
            History.Add(Stable.Copy());
            pendingDirection = direction;
            maximumDistance = Plan.slide.Select((p,i) => Math.Abs(p.x-Stable.positions[i].x)+Math.Abs(p.y-Stable.positions[i].y)).Max();
            Moving = true; Elapsed = 0;
            if (maximumDistance == 0 && !Plan.HasEntries && !Plan.HasResize) Finish();
            return true;
        }
        public bool Tick(float deltaTime, bool reduceMotion = false)
        {
            if (!Moving || Paused) return false;
            Elapsed += Math.Max(0, deltaTime);
            if (reduceMotion || Elapsed>=ResizeStart+(Plan.HasResize?ResizeDuration:0)) { Finish(); return true; }
            return false;
        }
        private void Finish()
        {
            Stable = new PuzzleSnapshot { positions=Target,sizes=Plan.sizes, direction=pendingDirection, moves=Stable.moves+1 };
            Moving = false; Elapsed = 0;
        }
        public bool Undo()
        {
            if (Paused || History.Count == 0) return false;
            UsedUndo=true;
            Stable = History[History.Count-1].Copy(); History.RemoveAt(History.Count-1);
            Moving = false; Target = null; Plan=null; Elapsed = 0;
            return true;
        }
        public void Reset()
        {
            UsedUndo=false;
            Stable = new PuzzleSnapshot { positions=Level.Start,sizes=Level.StartSizes, direction=0, moves=0 };
            History.Clear(); Target = null; Plan=null; Moving = false; Elapsed = 0;
        }
        public PuzzleSnapshot[] StableHistory() => History.Take(History.Count - (Moving ? 1 : 0)).Select(s => s.Copy()).ToArray();
        public bool Restore(PuzzleSnapshot snapshot, PuzzleSnapshot[] history,bool usedUndo=false)
        {
            bool Valid(PuzzleSnapshot s) => s != null && (s.sizes!=null||!Level.HasResizers) && s.direction >= 0 && s.direction <= 3 && s.moves >= 0 && s.moves <= 100000 && Level.IsValidState(s.positions,s.sizes) && (Level.HasResizers||s.sizes==null||s.sizes.SequenceEqual(Level.StartSizes));
            if (!Valid(snapshot) || history == null || history.Length != snapshot.moves || history.Any(s => !Valid(s))) return false;
            for (int i=0;i<history.Length;i++) if (history[i].moves != i) return false;
            Stable=snapshot.Copy(); History=history.Select(s=>s.Copy()).ToList(); Moving=false; Elapsed=0; Target=null; Plan=null;
            UsedUndo=usedUndo;
            if(Stable.sizes==null)Stable.sizes=Level.StartSizes;foreach(var s in History)if(s.sizes==null)s.sizes=Level.StartSizes;
            return true;
        }
        public void VisualPosition(int index, out float x, out float y)
        {
            Cell start = Stable.positions[index]; x=start.x; y=start.y;
            if (!Moving) return;
            if(InResizePhase){x=Target[index].x;y=Target[index].y;return;}
            if(InPortalPhase && Plan.entered[index]>=0 && !Plan.blocked[index] && PortalProgress>=.5f)
            { x=Target[index].x;y=Target[index].y;return; }
            Cell axis = TiltSimulator.Axes[pendingDirection];
            float distance = Math.Min(CommonDistance, Math.Abs(Plan.slide[index].x-start.x)+Math.Abs(Plan.slide[index].y-start.y));
            x += axis.x*distance; y += axis.y*distance;
        }
    }
}


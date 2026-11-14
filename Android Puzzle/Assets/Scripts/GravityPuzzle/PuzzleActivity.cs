using System;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Puzzle.Gravity
{
    [Serializable] public sealed class StageActivity
    {
        public string room;
        public int moves,restarts,undos,hints,visits,clears;
    }
    [Serializable] public sealed class PuzzleActivity
    {
        public StageActivity[] stages=Array.Empty<StageActivity>();
        public StageActivity For(string room)
        {
            var stage=stages.FirstOrDefault(s=>s.room==room);
            if(stage==null){stage=new StageActivity{room=room};stages=stages.Append(stage).ToArray();}
            return stage;
        }
        public int TotalMoves=>stages.Sum(s=>s.moves);
        public int TotalRestarts=>stages.Sum(s=>s.restarts);
    }
    [Serializable] public sealed class CommunitySnapshot
    {
        public int schema=2,moves,restarts,stars,wallet,spentStars;
        public StageActivity[] stages;
        public StageStars[] records;
    }
    public static class CommunityBridge
    {
        // Starts denied on every launch. Only the explicit consent UI can enable transmission.
        public static bool ConsentGranted {get;set;}
        public static CommunitySnapshot Snapshot(PuzzleActivity activity,PuzzleCollection collection)=>new CommunitySnapshot
        {
            moves=activity.TotalMoves,restarts=activity.TotalRestarts,stars=collection.TotalEarnedStars,
            wallet=collection.wallet,spentStars=collection.SpentStars,stages=activity.stages,records=collection.records
        };
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void MallangCommunitySync(string json);
        [DllImport("__Internal")] private static extern void MallangCommunityOpen();
        [DllImport("__Internal")] private static extern void MallangCommunityReady(string target);
#endif
        public static void Sync(PuzzleActivity activity,PuzzleCollection collection)
        {
            if(!ConsentGranted)return;
#if UNITY_WEBGL && !UNITY_EDITOR
            MallangCommunitySync(JsonUtility.ToJson(Snapshot(activity,collection)));
#endif
        }
        public static void Ready(string target)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            MallangCommunityReady(target);
#endif
        }
        public static void Open()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            MallangCommunityOpen();
#endif
        }
    }
}

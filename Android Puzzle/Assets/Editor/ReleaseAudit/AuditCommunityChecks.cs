using System;
using System.Linq;
using Puzzle.Gravity;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzle.Editor
{
    public static class AuditCommunityChecks
    {
        public static void ValidateAndBuild()
        {
            var levels=JsonUtility.FromJson<LayoutDocument>(Resources.Load<TextAsset>("GravityPuzzle/layouts").text).layouts;
            var first=levels.First(l=>l.room=="F0");var second=levels.First(l=>l.room=="3C");
            Require(!first.walls.SequenceEqual(second.walls)&&second.route.Length==4,"second stage is a different four-move puzzle");
            var search=new PuzzleHintSearch(second,new PuzzleSnapshot{positions=second.Start,sizes=second.StartSizes});while(!search.Done)search.Step(1000,1000);
            Require(search.Route!=null,"new second stage solvable");
            var state=second.Start;foreach(int d in second.route)state=TiltSimulator.Tilt(second,state,d);Require(second.IsSolved(state,second.StartSizes),"authored second route solves");
            Require(ProgressSaveService.LevelVersion(first,"f0dda662")=="f0dda662"&&ProgressSaveService.LevelVersion(second,"f0dda662")!="f0dda662","only changed basic map invalidates its state");
            var activity=new PuzzleActivity();activity.For("F0").moves=7;activity.For("3C").moves=4;activity.For("F0").restarts=2;
            var restored=JsonUtility.FromJson<ProgressData>(JsonUtility.ToJson(new ProgressData{activity=activity,portalHelpSeen=true}));
            Require(restored.portalHelpSeen&&restored.activity.TotalMoves==11&&restored.activity.TotalRestarts==2,"lifetime stats and one-time portal flag persist");
            Require(!CommunityBridge.ConsentGranted,"community transmission starts denied");
            var go=new GameObject("Memo check",typeof(RectTransform),typeof(CanvasRenderer),typeof(BoardDrawing));
            try
            {
                var drawing=go.GetComponent<BoardDrawing>();drawing.rectTransform.sizeDelta=new Vector2(300,300);
                for(int i=0;i<4;i++){drawing.ColorIndex=i;drawing.Mark(new Vector2(.1f,.1f+i*.2f),new Vector2(.9f,.1f+i*.2f));}
                Require(drawing.Lines.Select(s=>s.color).Distinct().Count()==4,"four pen colors");
                drawing.Erasing=true;drawing.Mark(new Vector2(.5f,.1f),new Vector2(.5f,.1f));Require(drawing.Lines.Count(s=>s.color==0)==2,"eraser removes only the touched middle, preserving both ends");
                drawing.Clear();Require(drawing.Lines.Count==0,"clear all ink");
            }
            finally{UnityEngine.Object.DestroyImmediate(go);}
            
            Debug.Log("PASS community: different second map, save compatibility, lifetime statistics, portal flag, default-denied telemetry and four-color memo eraser.");
            
        }
        private static void Require(bool value,string text){if(!value)throw new Exception(text);}
    }
}

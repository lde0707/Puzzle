using System;
using System.Linq;
using Puzzle.Gravity;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Puzzle.Editor
{
    public static class AuditProgressTransferChecks
    {
        public static void ValidateAndBuild()
        {
            var levels=JsonUtility.FromJson<LayoutDocument>(Resources.Load<TextAsset>("GravityPuzzle/layouts").text).layouts;
            var ids=levels.Select(l=>l.room).ToArray();
            var legacy=new ProgressData{levelId="F0",dataVersion="f0dda662",completed=new[]{"F0","3C"},collection=new PuzzleCollection{wallet=3,selected=2,owned=new[]{0,2},records=new[]{new StageStars{id="F0",stars=3,bestMoves=3},new StageStars{id="3C",stars=3,bestMoves=3}}}};
            var blank=new ProgressData{levelId="F0",dataVersion="f0dda662",collection=new PuzzleCollection()};
            string raw=ProgressTransfer.Encode(legacy);
            Require(ProgressTransfer.Decode(raw,levels,out var decoded),"legacy envelope accepted");
            Require(!ProgressTransfer.Decode(raw.Replace("f0dda662","ffffffff"),levels,out _),"checksum rejects corruption");
            Require(!ProgressTransfer.Decode("{}",levels,out _),"unrelated JSON rejected");
            var merged=ProgressTransfer.Merge(blank,decoded,ids);
            Require(merged.collection.wallet==3&&merged.collection.selected==2&&merged.collection.owned.SequenceEqual(new[]{0,2})&&merged.completed.Length==2,"fresh device preserves progress skins stars and selection");
            var again=ProgressTransfer.Merge(merged,decoded,ids);Require(again.collection.wallet==3,"reimport does not award duplicate currency");
            merged.collection.owned=new[]{0,2,5};merged.collection.wallet=0;
            again=ProgressTransfer.Merge(merged,decoded,ids);
            Require(again.collection.wallet==0&&again.collection.owned.Contains(5),"import after spending preserves new skin and does not refill spent stars");
            merged.activity=new PuzzleActivity();merged.activity.For("F0").moves=19;
            Require(ProgressTransfer.Merge(merged,decoded,ids).activity.TotalMoves==19,"new cumulative statistics survive legacy import");
            var go=new GameObject("Freehand check",typeof(RectTransform),typeof(CanvasRenderer),typeof(BoardDrawing));
            try
            {
                var draw=go.GetComponent<BoardDrawing>();draw.rectTransform.sizeDelta=new Vector2(300,300);
                var pointer=new PointerEventData(null){useDragThreshold=true};draw.OnInitializePotentialDrag(pointer);Require(!pointer.useDragThreshold,"small freehand movements bypass drag threshold");
                draw.Mark(new Vector2(.1f,.5f),new Vector2(.9f,.5f));draw.Erasing=true;
                draw.Mark(new Vector2(.5f,.1f),new Vector2(.5f,.9f));
                Require(draw.Lines.Count==2&&draw.Lines.All(s=>s.a.x>.5f||s.b.x<.5f),"swept eraser cuts crossing line but keeps untouched ends");
            }
            finally{UnityEngine.Object.DestroyImmediate(go);}
            Debug.Log("PASS transfer: legacy decode, corruption rejection, skin/progress preservation, repeat-import and spent-star protection; freehand input and partial eraser.");
            
        }
        private static void Require(bool value,string name){if(!value)throw new Exception(name);}
    }
}

using System;
using System.Linq;
using Puzzle.Gravity;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Puzzle.Editor
{
    public static class AuditDrawingUndoChecks
    {
        public static void ValidateAndBuild()
        {
            var activity=new PuzzleActivity();activity.For("F0").moves=7;activity.For("3C").moves=4;
            activity.For("F0").restarts=2;
            var restored=JsonUtility.FromJson<ProgressData>(JsonUtility.ToJson(new ProgressData{activity=activity})).activity;
            Require(restored.For("F0").moves==7&&restored.For("3C").moves==4&&restored.For("F0").restarts==2&&restored.For("3C").restarts==0,"stage totals remain separate after save/load");
            Require(restored.TotalMoves==11&&restored.TotalRestarts==2,"overall ranking totals remain intact");
            // Batch edit mode normally routes events to Editor, where actions intentionally do not run.
            // Enable the package's test mode for this isolated check and restore it afterwards.
            var manager=typeof(InputSystem).GetField("s_Manager",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic).GetValue(null);
            var playerUpdates=manager.GetType().GetProperty("runPlayerUpdatesInEditMode");
            bool previousUpdates=(bool)playerUpdates.GetValue(manager);playerUpdates.SetValue(manager,true);
            var keyboard=InputSystem.AddDevice<Keyboard>();
            try
            {
                int shortcuts=0;
                using var shortcut=new DrawingUndoShortcut(()=>shortcuts++);
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.LeftCtrl,Key.Z));
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());InputSystem.Update();
                Require(shortcuts==1,"quick Ctrl+Z survives same-frame key release");
                InputSystem.Update();InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Z));InputSystem.Update();
                Require(shortcuts==1,"bare Z is not drawing undo");
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.LeftCtrl,Key.LeftShift,Key.Z));InputSystem.Update();
                Require(shortcuts==1,"Ctrl+Shift+Z is not undo");
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());InputSystem.Update();
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.LeftMeta,Key.Z));
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());InputSystem.Update();
                Require(shortcuts==2,"Cmd+Z is supported");
            }
            finally{InputSystem.RemoveDevice(keyboard);playerUpdates.SetValue(manager,previousUpdates);}
            var go=new GameObject("Drawing undo checks",typeof(RectTransform),typeof(CanvasRenderer),typeof(BoardDrawing));
            try
            {
                var d=go.GetComponent<BoardDrawing>();d.rectTransform.sizeDelta=new Vector2(300,300);
                d.BeginStroke();d.Mark(new Vector2(.1f,.5f),new Vector2(.5f,.5f));d.Mark(new Vector2(.5f,.5f),new Vector2(.9f,.5f));d.EndStroke();
                var first=d.Lines.ToArray();
                d.ColorIndex=2;d.BeginStroke();d.Mark(new Vector2(.2f,.2f),new Vector2(.3f,.3f));d.Mark(new Vector2(.3f,.3f),new Vector2(.4f,.2f));d.EndStroke();
                Require(d.UndoStroke()&&d.Lines.SequenceEqual(first),"whole gesture undone, earlier stroke preserved");
                d.Erasing=true;d.BeginStroke();d.Mark(new Vector2(.5f,.1f),new Vector2(.5f,.9f));d.EndStroke();
                Require(!d.Lines.SequenceEqual(first)&&d.UndoStroke()&&d.Lines.SequenceEqual(first),"partial eraser can be undone exactly");
                d.Clear();d.Clear();Require(d.UndoStroke()&&d.Lines.SequenceEqual(first),"clear all is undoable and empty clear is ignored");
                d.BeginStroke();d.EndStroke();Require(d.UndoStroke()&&d.Lines.Count==0,"empty gesture does not consume history");
                Require(!d.UndoStroke(),"empty history is harmless");
                d.Erasing=false;d.BeginStroke();d.Mark(Vector2.zero,Vector2.one);
                Require(d.UndoStroke()&&d.Lines.Count==0,"undo during drag cancels current stroke");
                for(int i=0;i<DrawingUndoChecksCount;i++){d.BeginStroke();d.Mark(Vector2.one*.2f,Vector2.one*.2f);d.EndStroke();}
                int undone=0;while(d.UndoStroke())undone++;
                Require(undone==BoardDrawing.UndoLimit,"history memory is bounded");
                Debug.Log("PASS drawing undo: gesture grouping, eraser, clear all, no-op, active drag and history limit.");
            }
            finally{UnityEngine.Object.DestroyImmediate(go);}
            
        }
        private const int DrawingUndoChecksCount=BoardDrawing.UndoLimit+3;
        private static void Require(bool value,string text){if(!value)throw new Exception(text);}
    }
}

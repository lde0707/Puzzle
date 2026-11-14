using System;
using System.IO;
using UnityEngine;
using Puzzle.Gravity;
namespace Puzzle.Editor {
public static class SaveConflictProbe {
 public static void Run() {
  var folder=Path.Combine("Logs","release-audit-conflict-"+Guid.NewGuid().ToString("N"));
  var initial=new ProgressSaveService(folder); initial.Save(new ProgressData{collection=new PuzzleCollection{wallet=9}});
  var a=new ProgressSaveService(folder);var b=new ProgressSaveService(folder);
  var da=a.Load();var db=b.Load();
  int skin; bool purchased=da.collection.TryDraw(n=>0,()=>a.Save(da),out skin);
  db.sound=false; bool staleSaved=b.Save(db);
  var actual=new ProgressSaveService(folder).Load();
  var report="Purchased="+purchased+", new skin="+skin+", staleSaved="+staleSaved+", finalWallet="+actual.collection.wallet+", finalOwned="+string.Join(",",actual.collection.owned)+", expected wallet=6 and owned=0,1";
  Directory.CreateDirectory("Logs");File.WriteAllText("Logs/release-audit-save-conflict.txt",report);
  Debug.Log(report);
 }
}}

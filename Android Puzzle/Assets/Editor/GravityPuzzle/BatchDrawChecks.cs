using System;
using System.IO;
using System.Linq;
using Puzzle.Gravity;
using UnityEditor;
using UnityEngine;

namespace Puzzle.Editor
{
    public static class BatchDrawChecks
    {
        [MenuItem("Puzzle/Prototype/Check Ten Skin Draw (No Web Build)")]
        public static void Run()
        {
            var c=new PuzzleCollection{wallet=35};int saves=0,picks=0;
            Require(c.TryDrawMany(10,n=>{Require(n==39-picks,"pool shrinks after each distinct draw");picks++;return n-1;},()=>{saves++;return c.wallet==5&&c.owned.Length==11;},out var skins),"ten draws succeed atomically");
            Require(saves==1&&skins.Length==10&&skins.Distinct().Count()==10&&skins.All(i=>i>=30)&&c.selected==0,"one save, ten new skins, outfit unchanged");
            string before=JsonUtility.ToJson(c);
            Require(!c.TryDrawMany(10,n=>throw new Exception("no pick"),()=>throw new Exception("no save"),out _)&&before==JsonUtility.ToJson(c),"insufficient stars never draw or save");
            c.wallet=30;before=JsonUtility.ToJson(c);
            Require(!c.TryDrawMany(10,n=>0,()=>false,out var failed)&&failed.Length==0&&before==JsonUtility.ToJson(c),"failed save rolls back all ten and all thirty stars");
            try{c.TryDrawMany(10,n=>0,()=>throw new IOException("save failure"),out _);}catch(IOException){}
            Require(before==JsonUtility.ToJson(c),"thrown save also restores the full batch");
            int index=0;Require(!c.TryDrawMany(10,n=>++index==5?n:0,()=>throw new Exception("no save"),out _)&&before==JsonUtility.ToJson(c),"late invalid picker never partially unlocks");
            index=0;try{c.TryDrawMany(10,n=>++index==5?throw new IOException("picker failure"):0,()=>true,out _);}catch(IOException){}
            Require(before==JsonUtility.ToJson(c),"late thrown picker leaves collection unchanged");
            foreach(int count in new[]{-1,0,11,int.MaxValue})Require(!c.TryDrawMany(count,n=>0,()=>true,out _)&&before==JsonUtility.ToJson(c),"invalid batch size cannot spend");
            c=new PuzzleCollection{wallet=30,owned=Enumerable.Range(0,31).ToArray()};
            Require(!c.TryDrawMany(10,n=>0,()=>true,out _)&&c.wallet==30&&c.Remaining==9,"fewer than ten remaining disables the batch without partial charge");
            Require(c.TryDraw(n=>0,()=>true,out _)&&c.wallet==27&&c.Remaining==8,"last few skins remain available as singles");
            string directory=Path.Combine(Path.GetTempPath(),"puzzle-ten-draw-"+Guid.NewGuid().ToString("N"));
            try
            {
                var service=new ProgressSaveService(directory);
                c=new PuzzleCollection{wallet=30,owned=Enumerable.Range(0,30).ToArray()};
                Require(c.TryDrawMany(10,n=>n-1,()=>service.Save(new ProgressData{collection=c}),out skins),"exactly ten remaining and thirty stars succeeds");
                var restored=PuzzleCollection.Restore(service.Load().collection,null,Array.Empty<string>());
                Require(restored.wallet==0&&restored.Remaining==0&&skins.All(restored.owned.Contains),"all ten survive reload even before the reveal finishes");
                Require(restored.TryEquip(skins[5],()=>service.Save(new ProgressData{collection=restored}))&&service.Load().collection.selected==skins[5],"a batch reward can be equipped and saved");
                Require(!restored.TryDrawMany(10,n=>0,()=>true,out _)&&restored.wallet==0,"complete collection cannot spend again");
            }
            finally{if(Directory.Exists(directory))Directory.Delete(directory,true);}
            DrawPresentationChecks.Run();
            Debug.Log("PASS ten draw: atomic thirty-star spend, distinct rewards, shrinking equal-odds pool, failures, remaining boundary, reload and equip. No web build.");
        }
        private static void Require(bool value,string message){if(!value)throw new Exception(message);}
    }
}

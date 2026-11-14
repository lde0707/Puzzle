using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Puzzle.Gravity;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace Puzzle.Editor
{
    public static class SequentialUnlockChecks
    {
        const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
        static void Require(bool value,string message){if(!value)throw new Exception(message);}
        public static void ValidateAndBuild(){Run();StageRatingSurfaceChecks.Run();IphoneTestSetup.BuildGame();}
        public static void Run()
        {
            var levels=PuzzleCampaign.Order(new[]{"layouts","late_levels","resize_levels","blink_levels","expansion_levels"}.SelectMany(n=>JsonUtility.FromJson<LayoutDocument>(Resources.Load<TextAsset>("GravityPuzzle/"+n).text).layouts));
            var cleared=new HashSet<string>();
            Require(Enumerable.Range(0,128).Count(i=>PuzzleCampaign.IsUnlocked(levels,i,cleared.Contains))==4,"only each theme's first stage starts unlocked");
            for(int theme=0;theme<4;theme++)
            {
                cleared.Clear();var indices=Enumerable.Range(0,128).Where(i=>PuzzleCampaign.Theme(levels[i])==theme).ToArray();
                for(int count=0;count<32;count++)
                {
                    for(int n=0;n<32;n++)Require(PuzzleCampaign.IsUnlocked(levels,indices[n],cleared.Contains)==(n<=count),"sequential frontier per theme");
                    foreach(int other in Enumerable.Range(0,4).Where(t=>t!=theme))Require(!PuzzleCampaign.IsUnlocked(levels,other*32+1,cleared.Contains),"progress cannot unlock a different theme");
                    cleared.Add(levels[indices[count]].room);
                }
                cleared.Remove(levels[indices[3]].room);
                Require(!PuzzleCampaign.IsUnlocked(levels,indices[31],cleared.Contains),"out-of-order historical clears cannot bypass a gap");
                Require(PuzzleCampaign.FirstUncleared(levels,theme,cleared.Contains)==indices[3],"resume goes to first missing prerequisite");
            }
            var host=new GameObject("Isolated sequential unlock check");var app=host.AddComponent<GravityPuzzleApp>();app.enabled=false;
            void Set(string n,object v)=>typeof(GravityPuzzleApp).GetField(n,Flags).SetValue(app,v);
            object Get(string n)=>typeof(GravityPuzzleApp).GetField(n,Flags).GetValue(app);
            object Call(string n,params object[] args)=>typeof(GravityPuzzleApp).GetMethods(Flags).Single(m=>m.Name==n&&m.GetParameters().Length==args.Length).Invoke(app,args);
            try
            {
                PuzzleUI.Font=Resources.Load<Font>("GravityPuzzle/NanumGothic-Regular");
                var collection=new PuzzleCollection();
                Set("levels",levels);Set("collection",collection);Set("activity",new PuzzleActivity());Set("hints",new PuzzleHints());Set("version","f0dda662");Set("sound",false);Set("musicMuted",true);
                Set("saves",new ProgressSaveService(Path.GetFullPath("Logs/sequential-check-save")));
                Call("CreateUI");Call("LoadLevel",0,null,false);
                Call("OpenLevels",false);Call("OpenStageDetail",1);
                var before=(PuzzleSession)Get("session");var dialog=(RectTransform)Get("dialog");
                var start=dialog.GetComponentsInChildren<PuzzleTap>().Single(t=>t.name=="앞 단계를 먼저 완료해 주세요");
                Require(!start.CanUse(),"locked detail start is disabled");start.Activate();Call("SelectLevel",1);
                Require(ReferenceEquals(before,Get("session")),"locked detail callback and direct selection do not start level");
                // Even a legacy save / direct load cannot resume beyond missing prerequisites.
                var later=levels[38];var data=new ProgressData{levelId=later.room,dataVersion=ProgressSaveService.LevelVersion(later,"f0dda662"),current=new PuzzleSnapshot{positions=later.Start,sizes=later.StartSizes}};
                Call("LoadLevel",38,data,false);Require(((PuzzleSession)Get("session")).Level.room==levels[32].room,"locked portal save resumes portal stage 1");
                collection.Award(levels[32].room,3,3,false,false);
                Require((bool)Call("CanPlayLevel",33)&&!(bool)Call("CanPlayLevel",34),"star records alone unlock exactly the next stage");
                Call("LoadLevel",33,null,false);Require(((PuzzleSession)Get("session")).Level.room==levels[33].room,"newly unlocked stage can load");
                var done=(HashSet<string>)Get("completed");done.Add(levels[0].room);
                Require((bool)Call("CanPlayLevel",1),"pre-stars legacy completed IDs unlock next stage");
                var serialized=JsonUtility.ToJson(collection);Call("OpenLevels",false);Call("OpenStageDetail",63);
                Require(JsonUtility.ToJson(collection)==serialized,"viewing locked history never removes stars or skins");
            }
            finally{Object.DestroyImmediate(host);}
            Debug.Log("PASS sequential unlock: four independent frontiers; all 128 slots; gap protection; legacy stars/completed; disabled start; guarded selection/load; safe resume; no player save touched.");
        }
    }
}

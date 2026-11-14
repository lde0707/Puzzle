using System;
using Puzzle.Gravity;
using UnityEngine;
namespace Puzzle.Editor
{
    public static class CrazyPortalChecks
    {
        static void Check(bool value,string message){if(!value)throw new Exception(message);}
        public static void Run()
        {
            long t=100000;var bank=HintWallet.Restore(null,t);
            Check(bank.balance==3,"Initial account grant");
            for(int i=0;i<3;i++)Check(bank.Spend(t,()=>true),"Spend initial hints");
            Check(!bank.Spend(t,()=>true),"No extra free hint");
            Check(!bank.Recharge(t+3599)&&bank.balance==0,"One hour boundary");
            Check(bank.Recharge(t+3600)&&bank.balance==1,"Hourly recharge");
            Check(bank.Recharge(t+3600*24)&&bank.balance==3,"Offline cap three");
            long stamp=bank.rechargeAt;Check(!bank.Spend(t+3600*24,()=>false)&&bank.balance==3&&bank.rechargeAt==stamp,"Failed save rollback");
            bank.Spend(t+3600*24,()=>true);Check(!bank.Recharge(t)&&bank.balance==2,"Clock rollback does not grant hints");
            var copy=HintWallet.Restore(JsonUtility.FromJson<HintWallet>(JsonUtility.ToJson(bank)),t+3600*24);
            Check(copy.balance==2,"Reload preserves spent hints");
            var clues=new PuzzleHints();
            for(int i=0;i<5;i++)Check(clues.Reveal("stage","v","s"+i,i%4,()=>true,true),"Global budget permits more than three clues per stage");
            Check(PuzzleHints.Restore(clues).Known("stage","v","s4")==0,"Extra clue survives reload");
            Check(!clues.Reveal("stage","v","failed",0,()=>false,true)&&clues.Known("stage","v","failed")==-1,"Failed clue save rollback");
            var old=new ProgressData{hintWallet=copy,collection=new PuzzleCollection(),activity=new PuzzleActivity()};
            var incoming=new ProgressData{hintWallet=HintWallet.Restore(null,HintWallet.Now),collection=new PuzzleCollection(),activity=new PuzzleActivity()};
            copy.rechargeAt=HintWallet.Now;copy.balance=0;
            Check(ProgressTransfer.Merge(old,incoming,new[]{"stage"}).hintWallet.balance==0,"Backup import cannot refill account hints");
            Debug.Log("CRAZYGAMES CHECKS PASS: account hint grant, hourly boundary, offline cap, rollback, clock rollback, persistence, extra clues and import isolation.");
        }
        public static void Build(){Run();LocalizationChecks.Run();IphoneTestSetup.BuildGame();}
    }
}

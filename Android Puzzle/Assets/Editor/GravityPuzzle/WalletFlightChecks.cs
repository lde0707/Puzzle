using System;
using Puzzle.Gravity;
using UnityEngine;

namespace Puzzle.Editor
{
    public static class WalletFlightChecks
    {
        public static void ValidateAndBuild()
        {
            var root=PuzzleUI.Rect("Wallet flight check",null);root.sizeDelta=new Vector2(390,844);
            try
            {
                var card=PuzzleUI.Rect("Result",root);PuzzleUI.Box(card,15,100,360,590);
                var target=PuzzleUI.Rect("Wallet",root);PuzzleUI.Box(target,280,748,25,25);
                var stars=new CompletionStars(card,3,false,null);
                int received=0;
                var flight=new WalletStarFlight(root,stars,target,3,3,()=>received++);
                flight.Tick(CompletionStars.RevealDuration(3)+.25f,false);
                Require(received==0,"No counter increase before flight");
                flight.Tick(0,false);Require(received==0,"Inactive time cannot collect stars");
                flight.Tick(.67f,false);Require(received==0,"Counter waits for arrival");
                flight.Tick(.02f,false);Require(received==1,"First arrival adds one");
                flight.Tick(.18f,false);Require(received==2,"Second arrival adds one");
                flight.Tick(.18f,false);Require(received==3,"Third arrival adds one");
                flight.Tick(20,false);flight.Tick(0,true);Require(received==3,"Completed flights cannot pay twice");

                var collection=new PuzzleCollection();
                Require(collection.Award("flight",4,3,false)==2,"First result earns two");
                int added=collection.Award("flight",3,3,false);
                Require(added==1&&collection.wallet==3,"Improved result adds only the difference");
                received=0;var upgrade=new WalletStarFlight(root,stars,target,3,added,()=>received++);
                upgrade.Tick(0,true);Require(received==1,"Fast mode immediately collects only the added star");
                upgrade.Tick(2,true);Require(received==1,"Fast mode cannot repeat the collection");
                Require(collection.Award("flight",3,3,false)==0&&collection.wallet==3,"Replay grants no extra currency");
                received=0;var cancelled=new WalletStarFlight(root,stars,target,3,3,()=>received++);
                cancelled.Tick(.1f,false);
                Require(received==0&&collection.wallet==3,"Saved wallet is independent of cancelled cosmetics");
                Debug.Log("Wallet flight checks passed: ordered arrival, pause, immediate mode, delta rewards, no duplicate collection.");
            }
            finally{UnityEngine.Object.DestroyImmediate(root.gameObject);}
            IphoneTestSetup.BuildGame();
        }
        private static void Require(bool value,string message){if(!value)throw new Exception(message);}
    }
}

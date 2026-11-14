using System;
using Puzzle.Gravity;
using UnityEngine;

namespace Puzzle.Editor
{
    public static class ClearFinaleChecks
    {
        public static void ValidateAndBuild(){Run();WalletFlightChecks.ValidateAndBuild();}
        public static void Run()
        {
            var root=PuzzleUI.Rect("Finale check",null);root.sizeDelta=new Vector2(390,844);
            try
            {
                var card=PuzzleUI.Rect("Result",root);PuzzleUI.Box(card,15,100,360,590);
                var target=PuzzleUI.Rect("Wallet",root);PuzzleUI.Box(target,280,748,25,25);
                int received=0,finales=0,notes=0;float time=0,lastArrival=0,finish=0,maxScale=1;
                var stars=new CompletionStars(card,3,false,i=>{Require(i==notes,"Reveal notes remain ordered");notes++;},()=>
                {Require(received==3,"Final sound waits for all arrivals");finales++;finish=time;});
                var flight=new WalletStarFlight(root,stars,target,3,3,()=>
                {received++;lastArrival=time;if(received==3)stars.FinishWhenReady();});
                for(int frame=0;frame<240;frame++)
                {
                    time+=.025f;float presentationDelta=.025f*GravityPuzzleApp.ClearPresentationSpeed;stars.Tick(presentationDelta,false);flight.Tick(presentationDelta,false);
                    if(finales>0)maxScale=Mathf.Max(maxScale,stars.Star(1).localScale.x);
                }
                Require(notes==3&&received==3&&finales==1,"Exactly one finale per clear");
                Require(finish>1.24f&&finish<1.32f,"Interleaved three-star flight and finale run at 1.3x speed");
                Require(finish-lastArrival>=.11f/GravityPuzzleApp.ClearPresentationSpeed&&finish-lastArrival<.17f/GravityPuzzleApp.ClearPresentationSpeed,"Connected finale after last arrival");
                Require(maxScale>1.1f&&Mathf.Abs(stars.Star(1).localScale.x-1)<.001f,"Final emphasis grows then settles");
                stars.FinishWhenReady();stars.Tick(10,false);Require(finales==1,"Repeated finish request cannot replay the sound");
                int quickFinal=0,quickNotes=0;
                var quick=new CompletionStars(card,2,true,i=>quickNotes++,()=>quickFinal++);
                quick.FinishWhenReady();quick.Tick(0,true);quick.Tick(10,true);
                Require(quickFinal==1&&quickNotes==0&&quick.Star(0).localScale==Vector3.one,"Fast mode skips the ascent and pulse, keeps one finish");
                int replayFinal=0;var replay=new CompletionStars(card,1,false,null,()=>replayFinal++);
                replay.FinishWhenReady();replay.Tick(0,false);Require(replayFinal==0,"Paused timeline does not advance finale");
                replay.Tick(3,false);Require(replayFinal==1,"No new currency still gets a completed result finale");
                for(int i=0;i<ClearRewardAudio.Notes.Length;i++)
                {
                    Require(i==0||ClearRewardAudio.Notes[i]>ClearRewardAudio.Notes[i-1],"Strictly rising reveal and collection pitches");
                    CheckClip(ClearRewardAudio.MakeClip(i));
                }
                var sustain=ClearRewardAudio.MakeClip(0);
                Require(sustain.length>(WalletStarFlight.ArrivalAt(0)-CompletionStars.RevealAt(0))/GravityPuzzleApp.ClearPresentationSpeed,"Reward sound sustains until collection");
                UnityEngine.Object.DestroyImmediate(sustain);
                Require(ClearRewardAudio.FinalNote>ClearRewardAudio.Notes[5],"Final melody note is highest");
                CheckClip(ClearRewardAudio.MakeFinale());
                Debug.Log("Clear finale checks passed: rising audio, last-arrival timing, single finale, emphasis settling, fast mode and replay.");
            }
            finally{UnityEngine.Object.DestroyImmediate(root.gameObject);}
        }
        private static void CheckClip(AudioClip clip)
        {
            try
            {
                var samples=new float[clip.samples];Require(clip.GetData(samples,0),"Readable generated clip");float peak=0;
                foreach(float sample in samples){Require(!float.IsNaN(sample)&&!float.IsInfinity(sample),"Finite audio samples");peak=Mathf.Max(peak,Mathf.Abs(sample));}
                Require(peak>.01f&&peak<.4f,"Audible headroom without clipping");
                Require(Mathf.Abs(samples[0])<.001f&&Mathf.Abs(samples[samples.Length-1])<.001f,"No edge clicks");
            }
            finally{UnityEngine.Object.DestroyImmediate(clip);}
        }
        private static void Require(bool condition,string message){if(!condition)throw new Exception(message);}
    }
}

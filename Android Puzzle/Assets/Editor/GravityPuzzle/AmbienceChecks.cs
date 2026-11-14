using System;
using Puzzle.Gravity;
using UnityEngine;

namespace Puzzle.Editor
{
    public static class AmbienceChecks
    {
        public static void ValidateAndBuild()
        {
            var clip=Resources.Load<AudioClip>("GravityPuzzle/Audio/mallang-lullaby");
            Require(clip&&Mathf.Abs(clip.length-48)<.1f,"48-second original music loop is imported");
            clip.LoadAudioData();
            var samples=new float[clip.samples*clip.channels];
            Require(clip.GetData(samples,0),"Music samples can be read");
            float peak=0;double energy=0;
            foreach(var sample in samples)
            {
                Require(!float.IsNaN(sample)&&!float.IsInfinity(sample),"Finite music samples");
                peak=Mathf.Max(peak,Mathf.Abs(sample));energy+=sample*sample;
            }
            double rms=Math.Sqrt(energy/samples.Length)*GravityPuzzleApp.QuietMusicVolume;
            Require(peak>.1f&&peak<.8f&&rms>.001&&rms<.012,"Music has headroom and a deliberately quiet mix");
            Require(Mathf.Abs(samples[0]-samples[samples.Length-clip.channels])*GravityPuzzleApp.QuietMusicVolume<.002f,"Loop seam stays inaudible at playback gain");
            Require(!JsonUtility.FromJson<ProgressData>("{\"sound\":true}").musicMuted,"Old saves enable the new quiet music");
            Require(JsonUtility.FromJson<ProgressData>(JsonUtility.ToJson(new ProgressData{musicMuted=true})).musicMuted,"Music mute preference persists independently");
            Debug.Log("Ambience checks passed: loop import, sample integrity, quiet mix, smooth seam and saved music setting.");
            ClearFinaleChecks.ValidateAndBuild();
        }
        private static void Require(bool value,string message){if(!value)throw new Exception(message);}
    }
}

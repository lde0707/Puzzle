using UnityEngine;

namespace Puzzle.Gravity
{
    // Fixed-pitch clips keep overlapping notes in tune: never repitch the AudioSource.
    public static class ClearRewardAudio
    {
        public static readonly float[] Notes={659.25f,783.99f,880f,1046.5f,1174.66f,1318.51f};
        public const float FinalNote=1567.98f;
        public static AudioClip MakeClip(int index)=>Create("Star ascent "+index,Notes[index],.38f,false);
        public static AudioClip MakeFinale()=>Create("Soft star finale",FinalNote,.68f,true);
        private static AudioClip Create(string name,float frequency,float duration,bool finale)
        {
            const int rate=44100;
            var samples=new float[Mathf.CeilToInt(duration*rate)];
            for(int i=0;i<samples.Length;i++)
            {
                float t=i/(float)rate;
                float attack=Mathf.Min(1,t/(finale?.006f:.008f));
                float tail=Mathf.Clamp01((duration-t)/.06f);
                float fundamental=Mathf.Sin(t*2*Mathf.PI*frequency);
                float shimmer=Mathf.Sin(t*2*Mathf.PI*frequency*2)*.16f*Mathf.Exp(-t*18);
                float tone=(fundamental+shimmer)*Mathf.Exp(-t*(finale?6:9));
                if(finale)
                {
                    // Warm C-major body below the bright final G, with a soft attack.
                    tone+=(Mathf.Sin(t*2*Mathf.PI*1046.5f)*.30f+Mathf.Sin(t*2*Mathf.PI*1318.51f)*.23f)*Mathf.Exp(-t*9);
                }
                samples[i]=tone*attack*tail*(finale?.16f:.13f);
            }
            var clip=AudioClip.Create(name,samples.Length,1,rate,false);clip.SetData(samples,0);return clip;
        }
    }
}

using UnityEngine;

namespace Puzzle.Gravity
{
    public static class SkinDrawAudio
    {
        // Original soft bells, a falling bubble pop, and an ascending major-sixth flourish.
        public static AudioClip Make(int cue)
        {
            const int rate=44100;
            float duration=cue==3?.32f:cue==4?1.15f:cue==5?1.05f:.38f;
            var data=new float[Mathf.CeilToInt(rate*duration)];
            float[] chord={523.25f,659.25f,783.99f,1046.5f,1318.5f};
            for(int i=0;i<data.Length;i++)
            {
                float t=i/(float)rate,value=0;
                if(cue==3)
                {
                    float phase=2*Mathf.PI*(720*t-850*t*t);
                    value=Mathf.Sin(phase)*Mathf.Exp(-t*17)*Mathf.Min(1,t*200)*.17f;
                }
                else
                {
                    int count=cue>=4?5:1;
                    for(int n=0;n<count;n++)
                    {
                        float local=t-(cue==4?n*.075f:cue==5?n*.022f:0);
                        if(local<0)continue;
                        float frequency=cue<3?chord[cue]:chord[n]*(cue==5?1.25f:1);
                        float phase=local*frequency*2*Mathf.PI;
                        float envelope=Mathf.Min(1,local*160)*Mathf.Exp(-local*(cue>=4?6:12));
                        value+=(Mathf.Sin(phase)+.20f*Mathf.Sin(phase*2)+.07f*Mathf.Sin(phase*3))*envelope*(cue>=4?.055f:.12f);
                    }
                }
                data[i]=value*Mathf.Clamp01((duration-t)/.04f);
            }
            var clip=AudioClip.Create("Candy capsule cue "+cue,data.Length,1,rate,false);clip.SetData(data,0);return clip;
        }
    }
}

using System;
using System.Runtime.InteropServices;

namespace Puzzle.Gravity
{
    // Enabled only by the CrazyGames HTML5 wrapper. Other exports keep their storage.
    public static class CrazyPortal
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern int DegulCGEnabled();
        [DllImport("__Internal")] private static extern string DegulCGGet(string key);
        [DllImport("__Internal")] private static extern int DegulCGSet(string key,string value);
        [DllImport("__Internal")] private static extern void DegulCGReady(string target);
        [DllImport("__Internal")] private static extern void DegulCGPlaying(int playing);
        [DllImport("__Internal")] private static extern void DegulCGReward(string target,string token);
        [DllImport("__Internal")] private static extern int DegulCGAds();
        [DllImport("__Internal")] private static extern int DegulCGMute();
        public static bool Enabled => DegulCGEnabled()!=0;
        public static bool AdsEnabled => Enabled&&DegulCGAds()!=0;
        public static bool Muted=>Enabled&&DegulCGMute()!=0;
        public static string Read(string key)=>DegulCGGet(key);
        public static void Write(string key,string value){if(DegulCGSet(key,value)==0)throw new InvalidOperationException("CrazyGames save unavailable");}
        public static void Ready(string target){if(Enabled)DegulCGReady(target);}
        public static void Playing(bool value){if(Enabled)DegulCGPlaying(value?1:0);}
        public static void Reward(string target,string token){if(Enabled)DegulCGReward(target,token);}
#else
        public static bool Enabled=>false;
        public static bool AdsEnabled=>false;
        public static bool Muted=>false;
        public static string Read(string key)=>null;
        public static void Write(string key,string value)=>throw new InvalidOperationException("CrazyGames is not active");
        public static void Ready(string target){}
        public static void Playing(bool value){}
        public static void Reward(string target,string token){}
#endif
    }
}

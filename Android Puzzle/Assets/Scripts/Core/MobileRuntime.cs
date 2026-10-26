using UnityEngine;

namespace Puzzle.Core
{
    public static class MobileRuntime
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            // Mobile uses targetFrameRate; disabling vSync also makes editor previews consistent.
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
        }
    }
}

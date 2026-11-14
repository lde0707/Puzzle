using UnityEngine;

namespace Puzzle.Gravity
{
    public sealed partial class GravityPuzzleApp
    {
        public const float ClearPresentationSpeed=1.3f;
        public const float QuietMusicVolume=.075f;
        private bool musicMuted,musicStarted;
        private AudioSource musicSource;

        private void CreateMusic()
        {
            musicSource=gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake=false;musicSource.loop=true;musicSource.spatialBlend=0;
            musicSource.volume=0;musicSource.priority=192;
            musicSource.clip=Resources.Load<AudioClip>("GravityPuzzle/Audio/mallang-lullaby");
        }
        private void UnlockMusic(){musicStarted=true;}
        private void ToggleMusic()
        {
            musicMuted=!musicMuted;Save();TickMusic(0);
        }
        private void TickMusic(float delta)
        {
            if(!musicSource||!musicSource.clip)return;
            if(!Active||musicMuted)
            {
                musicSource.Pause();musicSource.volume=0;return;
            }
            if(!musicStarted)return; // Browser audio begins only after a real interaction.
            if(!musicSource.isPlaying)
            {
                musicSource.UnPause();
                if(!musicSource.isPlaying)musicSource.Play();
            }
            float target=QuietMusicVolume*((modal=="solved"||modal=="skin-draw"||celebrationPending)?.4f:1f);
            musicSource.volume=Mathf.MoveTowards(musicSource.volume,target,QuietMusicVolume*delta/1.2f);
        }
    }
}

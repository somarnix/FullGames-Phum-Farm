using UnityEngine;

namespace PhumFarm.Core
{
    public sealed class AudioManager
    {
        private readonly GameState state;
        private AudioSource music;
        private AudioSource effects;

        public AudioManager(GameState state) => this.state = state;

        public void Attach(GameObject host)
        {
            music = host.AddComponent<AudioSource>();
            effects = host.AddComponent<AudioSource>();
            music.loop = true;
            music.playOnAwake = false;
            effects.playOnAwake = false;
            ApplySettings();
        }

        public void ApplySettings()
        {
            if (music != null) music.volume = Mathf.Clamp01(state.Data.settings.musicVolume);
            if (effects != null) effects.volume = Mathf.Clamp01(state.Data.settings.effectsVolume);
        }

        public void PlayMusic(AudioClip clip)
        {
            if (music == null || clip == null || music.clip == clip) return;
            music.clip = clip;
            music.Play();
        }

        public void PlayEffect(AudioClip clip)
        {
            if (effects != null && clip != null) effects.PlayOneShot(clip);
        }
    }
}

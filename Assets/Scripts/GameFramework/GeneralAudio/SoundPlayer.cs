using System;
using UnityEngine;

namespace RhythmGame.GeneralAudio
{
    public class SoundPlayer : MonoBehaviour
    {
        [SerializeField]
        private AudioSource[] sources;

        public void PlayOneShot(AudioClip clip) => sources[0].PlayOneShot(clip);

        public int Play(AudioClip clip)
        {
            var freeSource = System.Array.Find(sources, a => !a.isPlaying);

            if (freeSource == null)
            {
                Debug.LogError("No open sources found!");
                return -1;
            }

            freeSource.clip = clip;
            freeSource.volume = 1f;
            freeSource.Play();

            return Array.IndexOf(sources, freeSource);
        }

        public void Stop(int index)
        {
            if (index < 0 || index >= sources.Length)
            {
                Debug.LogError("Invalid index!");
                return;
            }

            sources[index].Stop();
        }

        public void StopAll()
        {
            foreach (var source in sources)
                source.Stop();
        }
    }
}

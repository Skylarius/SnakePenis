using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSystem : MonoBehaviour
{
    private AudioSource[] audioSources;
    public AudioClip[] growSounds, deathSounds;
    // Start is called before the first frame update
    void Start()
    {
        audioSources = GetComponents<AudioSource>();
    }

    void Play(AudioClip[] clipArray, bool setAsAudioSourceClip = false)
    {
        if (clipArray.Length > 0 && audioSources.Length > 0)
        {
            int clipIndex = Random.Range(0, clipArray.Length);
            AudioSource targetSource = null;
            foreach(AudioSource audioSource in audioSources)
            {
                if (audioSource.isPlaying == false)
                {
                    targetSource = audioSource;
                    break;
                }
            }
            if (targetSource == null)
            {
                audioSources[0].Stop();
                targetSource = audioSources[0];
            }
            targetSource.PlayOneShot(clipArray[clipIndex]);
            if (setAsAudioSourceClip)
            {
                targetSource.clip = clipArray[clipIndex];
            }
        }
    }

    void Stop(AudioClip[] clipArray, bool clearAudioSourceClip = false)
    {
        if (clipArray.Length > 0 && audioSources.Length > 0)
        {
            foreach (AudioSource audioSource in audioSources)
            {
                if (System.Array.IndexOf(clipArray, audioSource.clip) > -1 && audioSource.isPlaying)
                {
                    audioSource.Stop();
                    if (clearAudioSourceClip)
                    {
                        audioSource.clip = null;
                    }
                    return;
                }
            }
        }
    }

    bool IsSoundAlreadyPlaying(AudioClip[] clipArray)
    {
        foreach (AudioSource audioSource in audioSources)
        {
            if (System.Array.IndexOf(clipArray, audioSource.clip) > -1 && audioSource.isPlaying)
            {
                return true;
            }
        }
        return false;
    }

    public void PlayGrowSounds()
    {
        Play(growSounds);
    }
    public void PlayDeathSounds()
    {
        Play(deathSounds);
    }
}

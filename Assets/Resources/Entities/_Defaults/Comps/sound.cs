using UnityEngine;
using System;
/// <summary>
/// Drzi a prehrava zvuk pre jednotlive zdroje
/// </summary>
[Serializable] public class Sound
{
    [SerializeField] AudioClip audioClip;
    [SerializeField] float volume;

    public Sound(AudioClip clip, float v = 1)
    {
        audioClip = clip;
        volume = v;
    }
    /// <summary>
    /// Prehrava zvuk
    /// </summary>
    /// <param name="source"></param>
    public void Play(ref AudioSource source) => source.PlayOneShot(audioClip, volume);
    /// <summary>
    /// Prehrava zvuk so zmenenou rychlostou
    /// </summary>
    /// <param name="source"></param>
    /// <param name="pitch"></param>
    public void Play(ref AudioSource source, float pitch = 1) 
    { 
        float p = source.pitch;
        source.pitch = pitch;

        source.PlayOneShot(audioClip, volume); 
        
        source.pitch = p;
    }
}
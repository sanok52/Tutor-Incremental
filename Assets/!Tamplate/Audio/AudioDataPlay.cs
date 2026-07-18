using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

[Serializable]
public class AudioDataPlay
{
    public string ID;
    public AudioClip clip;
    public AudioClip[] additionClips;
    public float volume = 1f;
    public float pitch = 1f;
    public bool loop = false;

    [Range(0f, 1f)]
    public float spatialBlend = 1f;

    public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;
    public float dopplerLevel = 0f;

    [Space, Header("Offsets")]
    public Vector2 randomPitchOffset = new Vector2(0f, 0f);
    public Vector2 randomVolumeOffset = new Vector2(0f, 0f);

    [Space, Header("Reverb")]
    public bool useReverb = false;
    public AudioReverbPreset preset;

    [Space]
    public AudioMixerGroup MixerGroop;

    public AudioDataPlay() { }

    public AudioDataPlay(AudioSource source)
    {
        if (source == null)
            return;

        clip = source.clip;
        volume = source.volume;
        pitch = source.pitch;
        loop = source.loop;
        spatialBlend = source.spatialBlend;
        rolloffMode = source.rolloffMode;
        dopplerLevel = source.dopplerLevel;
    }

    private AudioClip previousClip;
    public AudioClip GetClip()
    {
        if (additionClips == null || additionClips.Length == 0)
            return clip;
        else
        {
            AudioClip clipResult = previousClip;
            if (additionClips.Length > 1)
                while (clipResult == previousClip)
                    clipResult = additionClips[Random.Range(0, additionClips.Length)];
            previousClip = clipResult;
            return clipResult;
        }
    }

    public float GetPitch()
    {
        return pitch + Random.Range(randomPitchOffset.x, randomPitchOffset.y);
    }

    public float GetVolume()
    {
        return volume + Random.Range(randomVolumeOffset.x, randomVolumeOffset.y);
    }

    public void ApplyData(AudioSource source, float volumeCoef = 1f)
    {
        source.clip = GetClip();

        source.volume = this.volume * volumeCoef + Random.Range(this.randomVolumeOffset.x, this.randomVolumeOffset.y);
        source.pitch = this.pitch + Random.Range(this.randomPitchOffset.x, this.randomPitchOffset.y);
        source.loop = this.loop;
        source.rolloffMode = this.rolloffMode;
        source.dopplerLevel = this.dopplerLevel;
        source.spatialBlend = this.spatialBlend;

        if (useReverb)
        {
            AudioReverbFilter filter = source.GetComponent<AudioReverbFilter>();
            if(filter == null)
                filter = source.AddComponent<AudioReverbFilter>();
            filter.reverbPreset = preset;
        }
    }
}


public static class AudioSourceExtensions
{
    public static void Play(this AudioSource source, AudioDataPlay data, float volumeCoef = 1f)
    {
        if (source == null || data == null)
            return;

        data.ApplyData(source, volumeCoef);
        source.Play();
    }

    public static void PlayOneShot(this AudioSource source, AudioDataPlay data, float volumeCoef = 1f)
    {
        if (source == null || data == null)
            return;

        var clip = data.GetClip();
        if (clip == null)
            return;

        float volume =
            (data.volume * volumeCoef) +
            Random.Range(data.randomVolumeOffset.x, data.randomVolumeOffset.y);

        data.ApplyData(source, volumeCoef);
        source.PlayOneShot(clip, volume);
    }
}
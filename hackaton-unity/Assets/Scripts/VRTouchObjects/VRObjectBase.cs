using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRObjectBase : MonoBehaviour
{
    public AudioSource audioSource;
    Dictionary<List<AudioClip>, Randomizer> rndClips;

    int lockCount = 0;

    public int LockCount => lockCount;

    public bool IsLocked
    {
        get
        {
            return lockCount > 0;
        }
    }

    virtual public void Lock(bool isLocked = true)
    {
        lockCount = Mathf.Max(0, lockCount + (isLocked ? 1 : -1));
    }

    public void UnLock()
    {
        Lock(false);
    }

    public void ResetLock()
    {
        lockCount = 0;
    }

    AudioSource audioPlayer
    {
        get
        {
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();

            if (audioSource == null)
                audioSource = CreateAudioSource(gameObject);

            return audioSource;
        }
    }

    protected bool IsAudioPlaying
    {
        get
        {
            return audioPlayer != null && audioPlayer.isPlaying;
        }
    }

    public static AudioSource CreateAudioSource(GameObject gameObject)
    {
        var audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.minDistance = 1f;
            audioSource.maxDistance = 20f;
            audioSource.loop = false;
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1;
        }
        return audioSource;
    }

    protected AudioClip PlayAudio(AudioClip clip)
    {
        if (clip != null)
        {
            if (audioPlayer.clip != clip)
                audioPlayer.clip = clip;
            audioPlayer.Play();
            return audioPlayer.clip;
        }
        return null;
    }

    Randomizer GetRndClip(List<AudioClip> clips)
    {
        if (rndClips == null)
            rndClips = new Dictionary<List<AudioClip>, Randomizer>();

        if (rndClips.ContainsKey(clips))
            return rndClips[clips];

        Randomizer rnd = new Randomizer(clips.Count);
        rndClips.Add(clips, rnd);
        return rnd;
    }

    protected AudioClip PlayAudio(List<AudioClip> clips)
    {
        if (clips != null && clips.Count > 0)
        {
            var rnd = GetRndClip(clips);
            return PlayAudio(clips[rnd.GetRandom()]);
        }
        return null;
    }
}

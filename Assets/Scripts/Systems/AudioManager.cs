using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Mixer")]
    public AudioMixer audioMixer;
    private AudioMixerGroup musicGroup;
    private AudioMixerGroup sfxGroup;
    private AudioMixerGroup ambientGroup;
    private AudioMixerGroup uiGroup;

    [Header("SFX Pool")]
    public int poolSize = 10;
    private List<AudioSource> sfxSources;
    private AudioSource musicSource;
    private readonly Dictionary<string, AudioChannel> channels = new();

    private int currentSourceIndex = 0;
    public bool isTesting = false;

    public enum MixerGroups
    {
        musicGroup,
        sfxGroup,
        ambientGroup,
        uiGroup
    }

    void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        InitializeMixerGroups();
        InitializeAudioPool();
        InitializeMusicSource();
    }

    private void InitializeAudioPool()
    {
        if (sfxSources != null)
            return;
        sfxSources = new List<AudioSource>();

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = new GameObject("SFX_Source_" + i);
            obj.transform.parent = transform;

            AudioSource source = obj.AddComponent<AudioSource>();
            source.spatialBlend = 1f; // 3D sound

            sfxSources.Add(source);
        }
    }

    private void InitializeMusicSource()
    {
        if (musicSource != null)
            return;
        GameObject musicObj = new GameObject("Music_Source");
        musicObj.transform.parent = transform;

        musicSource = musicObj.AddComponent<AudioSource>();
        musicSource.outputAudioMixerGroup = musicGroup;
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f; // 2D sound
    }

    private void InitializeMixerGroups()
    {
        musicGroup = audioMixer.FindMatchingGroups("Master/Music")[0];
        sfxGroup = audioMixer.FindMatchingGroups("Master/SFX")[0];
        ambientGroup = audioMixer.FindMatchingGroups("Master/Ambient")[0];
        uiGroup = audioMixer.FindMatchingGroups("Master/UI")[0];
    }

    private AudioSource GetNextSource()
    {
        currentSourceIndex = (currentSourceIndex + 1) % poolSize;
        if (sfxSources == null)
            InitializeAudioPool();
        return sfxSources[currentSourceIndex];
    }

    public AudioSource GetMusicSource()
    {
        InitializeMusicSource();
        return musicSource;
    }

    public void SetMixerGroup(AudioSource source, MixerGroups group)
    {
        switch(group)
        {
            case MixerGroups.musicGroup:
            source.outputAudioMixerGroup = musicGroup;
            break;
            case MixerGroups.sfxGroup:
            source.outputAudioMixerGroup = sfxGroup;
            break;
            case MixerGroups.ambientGroup:
            source.outputAudioMixerGroup = ambientGroup;
            break;
            case MixerGroups.uiGroup:
            source.outputAudioMixerGroup = uiGroup;
            break;
        }
    }

    /// <summary>
    /// Play 3D SFX at position (SFX audio group)
    /// </summary>
    public void PlaySoundAtPosition(AudioClip clip, Vector3 position, float volume = 1f)
    {
        AudioSource source = GetNextSource();

        source.spatialBlend = 1f;
        source.transform.position = position;
        source.outputAudioMixerGroup = sfxGroup;
        source.clip = clip;
        source.volume = volume;
        source.pitch = Random.Range(0.95f, 1.05f);
        source.Play();
    }

    /// <summary>
    /// Play 2D Ambient SFX (Ambient audio group)
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="volume"></param>
    public void PlaySoundAmbient(AudioClip clip, float volume = 1f)
    {
        AudioSource source = GetNextSource();

        source.spatialBlend = 1f;
        source.outputAudioMixerGroup = ambientGroup;
        source.clip = clip;
        source.volume = volume;
        source.pitch = Random.Range(0.95f, 1.05f);
        source.Play();
    }

    /// </summary>
    /// Play a 2D SFX (UI audio group)
    /// </summary>
    public void PlaySound2D(AudioClip clip, float volume = 1f)
    {
        AudioSource source = GetNextSource();

        source.spatialBlend = 0f;
        source.outputAudioMixerGroup = uiGroup;
        source.clip = clip;
        source.volume = volume;
        source.Play();
    }
    /// </summary>
    /// Play a music clip (Music audio group)
    /// </summary>
    public void PlayMusic(AudioClip music, float volume = 1f, bool loop = true)
    {
        // SetMusicVolume(0.6f);
        musicSource.clip = music;
        musicSource.volume = volume;
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void StopMusic()
    {
        StartCoroutine(FadeOut(musicSource, 1f));
        musicSource.Stop();
    }

    public AudioChannel GetChannel(string id)
    {
        if (channels.TryGetValue(id, out AudioChannel channel))
            return channel;

        channel = new AudioChannel(id, CreateChannelSource(), CreateChannelSource());
        channels.Add(id, channel);

        return channel;
    }


    private AudioSource CreateChannelSource()
    {
        GameObject obj = new GameObject("Audio Channel Source");
        obj.transform.SetParent(base.transform);
        AudioSource source = obj.AddComponent<AudioSource>();

        source.loop = false;
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.outputAudioMixerGroup = ambientGroup;

        return source;
    }

    public void SetMasterVolume(float volume)
    {
        SetVolume("MasterVolume", volume);
    }


    public void SetMusicVolume(float volume)
    {
        SetVolume("MusicVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        SetVolume("SFXVolume", volume);
    }

    public void SetAmbientVolume(float volume)
    {
        SetVolume("AmbientVolume", volume);
    }

    public void SetUIVolume(float volume)
    {
        SetVolume("UIVolume", volume);
    }

    private void SetVolume(string parameter, float volume)
    {
        // float minDb = -80f;
        // float maxDb = 0f;
        // float db = Mathf.Lerp(minDb, maxDb, volume);

        volume = Mathf.Pow(volume, 4f); // tweak exponent for volume curve (2–4 works well)
        float db = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20;

        audioMixer.SetFloat(parameter, db);
    }

    private static IEnumerator FadeOut (AudioSource audioSource, float FadeTime) {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0) {
            audioSource.volume -= startVolume * Time.deltaTime / FadeTime;

            yield return null;
        }

        audioSource.Stop ();
        audioSource.volume = startVolume;
    }

    public class AudioChannel
    {
        private AudioManager audioManager;
        private readonly string id;
        private AudioSource current;
        private AudioSource incoming;
        private Coroutine fadeRoutine;

        public AudioChannel(string id, AudioSource sourceA, AudioSource sourceB)
        {
            audioManager = GameManager.Instance.audioManager;
            this.id = id;
            current = sourceA;
            incoming = sourceB;

            current = sourceA;
            incoming = sourceB;
        }


        public void CrossfadeTo(AudioClip clip, float duration, float pitch = 1f)
        {
            if (fadeRoutine != null)
                audioManager.StopCoroutine(fadeRoutine);

            incoming.clip = clip;
            incoming.volume = 0f;
            incoming.pitch = pitch;
            incoming.Play();

            fadeRoutine = audioManager.StartCoroutine(CrossfadeRoutine(duration));
        }


        private IEnumerator CrossfadeRoutine(float duration)
        {
            float timer = 0f;
            float startVolume = current.volume;

            if (duration <= 0f)
            {
                current.Stop();

                incoming.volume = 1f;
                incoming.Play();

                (current, incoming) = (incoming, current);

                yield break;
            }

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = timer / duration;

                current.volume = Mathf.Lerp(startVolume, 0f, t);
                incoming.volume = Mathf.Lerp(0f, 1f, t);

                yield return null;
            }

            current.Stop();
            current.volume = 0f;

            (current, incoming) = (incoming, current);
        }


        public void SetVolume(float volume)
        {
            current.volume = volume;
        }

        public void SetPitch(float pitch)
        {
            current.pitch = pitch;
        }

        public void SetLooping(bool value)
        {
            current.loop = value;
            incoming.loop = value;
        }

        public void SetChannelMixerGroup(MixerGroups group)
        {
            audioManager.SetMixerGroup(current, group);
            audioManager.SetMixerGroup(incoming, group);
        }

        public void Stop(float fadeDuration = 0f)
        {
            if (fadeDuration <= 0f)
            {
                current.Stop();
                incoming.Stop();
                return;
            }

            audioManager.StartCoroutine(FadeOut(current, fadeDuration));
            audioManager.StartCoroutine(FadeOut(incoming, fadeDuration));
        }
    }
}

using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    [SerializeField] AudioMixer mixer;
    AudioSource music;
    AudioMixerGroup sfxGroup;
    float musicLevel = 1f;
    float sfxLevel = 1f;

    public float MusicLevel => musicLevel;
    public float SfxLevel => sfxLevel;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        music = GetComponent<AudioSource>();
        Debug.Assert(music != null && music.clip != null, "AudioManager missing music clip");
        Debug.Assert(mixer != null, "AudioManager missing mixer");
        AudioMixerGroup[] musicGroups = mixer.FindMatchingGroups("Music");
        AudioMixerGroup[] sfxGroups = mixer.FindMatchingGroups("SFX");
        Debug.Assert(musicGroups.Length == 1 && sfxGroups.Length == 1, "Music or SFX group missing");
        music.outputAudioMixerGroup = musicGroups[0];
        sfxGroup = sfxGroups[0];
        music.loop = true;
        music.spatialBlend = 0f;
        SetMusicVolume(musicLevel);
        SetSfxVolume(sfxLevel);
        music.Play();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void SetMusicVolume(float linear)
    {
        musicLevel = linear;
        bool set = mixer.SetFloat("MusicVolume", ToDecibels(linear));
        Debug.Assert(set, "MusicVolume missing");
    }

    public void SetSfxVolume(float linear)
    {
        sfxLevel = linear;
        bool set = mixer.SetFloat("SfxVolume", ToDecibels(linear));
        Debug.Assert(set, "SfxVolume missing");
    }

    static float ToDecibels(float linear)
    {
        return linear <= 0.0001f ? -80f : Mathf.Log10(linear) * 20f;
    }

    void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Main Menu" || music == null)
            return;
        music.time = 0f;
        music.Play();
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        StartCoroutine(PlaySFXCoroutine(clip, volume));
    }

    IEnumerator PlaySFXCoroutine(AudioClip clip, float volume = 1f)
    {
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.outputAudioMixerGroup = sfxGroup;
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.Play();

        yield return new WaitForSeconds(clip.length * 2);

        Destroy(audioSource);
    }
}

using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public float musicVolume = 0.32f;
    public float ambientVolume = 0.20f;
    public float sfxVolume = 0.65f;

    private AudioSource musicSource;
    private AudioSource ambientSource;
    private AudioSource sfxSource;

    private GameTheme currentTheme = GameTheme.Menu;
    private bool hasTheme;
    private bool muted;

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = musicVolume;

        ambientSource = gameObject.AddComponent<AudioSource>();
        ambientSource.loop = true;
        ambientSource.playOnAwake = false;
        ambientSource.volume = ambientVolume;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.volume = sfxVolume;
    }

    private void Start()
    {
        if (!hasTheme)
        {
            GameTheme theme = LevelCatalog.ThemeFor(gameObject.scene.name);
            PlayTheme(theme);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            muted = !muted;
            AudioListener.volume = muted ? 0f : 1f;
        }
    }

    public void PlayTheme(GameTheme theme)
    {
        if (hasTheme && currentTheme == theme) {
            return;
        }

        hasTheme = true;
        currentTheme = theme;

        AudioClip music = SoundSynth.Music(theme);

        if (musicSource == null) {
            return;
        }

        musicSource.Stop();
        musicSource.clip = music;
        musicSource.loop = theme != GameTheme.GameOver && theme != GameTheme.Victory;
        musicSource.volume = musicVolume;
        musicSource.Play();

        AudioClip ambient = SoundSynth.Ambient(theme);
        ambientSource.Stop();
        ambientSource.clip = ambient;
        ambientSource.volume = ambientVolume;
        ambientSource.Play();
    }

    public void PlaySfx(Sfx sfx)
    {
        if (sfxSource == null) {
            return;
        }

        AudioClip clip = SoundSynth.SfxClip(sfx);

        if (clip != null) {
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }

    public void StopMusic()
    {
        musicSource.Stop();
        ambientSource.Stop();
    }

}

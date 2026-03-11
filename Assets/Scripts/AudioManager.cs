using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<AudioManager>();
                if (instance == null)
                {
                    GameObject audioManagerObject = new GameObject("AudioManager");
                    instance = audioManagerObject.AddComponent<AudioManager>();
                    audioManagerObject.AddComponent<AudioSource>();
                }
            }
            return instance;
        }
    }

    private AudioSource audioSource;
    private AudioSource musicSource1;
    private AudioSource musicSource2;
    public AudioClip clickSound;
    public AudioClip backgroundMusic1;
    public AudioClip backgroundMusic2;
    [Range(0f, 1f)]
    public float sfxVolume = 0.5f;
    [Range(0f, 1f)]
    public float musicVolume = 0.3f;
    private float lastPlayTime = 0f;
    private const float SOUND_COOLDOWN = 0.1f;
    private const float CROSSFADE_DURATION = 3f;
    private bool isPlayingMusic1 = true;
    private bool isMusicPlaying = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            AudioSource[] audioSources = GetComponents<AudioSource>();
            
            if (audioSources.Length == 0)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                musicSource1 = gameObject.AddComponent<AudioSource>();
                musicSource2 = gameObject.AddComponent<AudioSource>();
            }
            else if (audioSources.Length == 1)
            {
                audioSource = audioSources[0];
                musicSource1 = gameObject.AddComponent<AudioSource>();
                musicSource2 = gameObject.AddComponent<AudioSource>();
            }
            else if (audioSources.Length == 2)
            {
                audioSource = audioSources[0];
                musicSource1 = audioSources[1];
                musicSource2 = gameObject.AddComponent<AudioSource>();
            }
            else
            {
                audioSource = audioSources[0];
                musicSource1 = audioSources[1];
                musicSource2 = audioSources[2];
            }
            
            if (musicSource1 != null)
            {
                musicSource1.loop = false;
                musicSource1.playOnAwake = false;
                musicSource1.volume = musicVolume;
            }
            
            if (musicSource2 != null)
            {
                musicSource2.loop = false;
                musicSource2.playOnAwake = false;
                musicSource2.volume = 0f;
            }
            
            if (audioSource != null)
            {
                audioSource.volume = sfxVolume;
            }
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (backgroundMusic1 != null)
        {
            StartMusic();
        }
    }

    void Update()
    {
        if (!isMusicPlaying)
        {
            return;
        }

        AudioSource currentSource = isPlayingMusic1 ? musicSource1 : musicSource2;
        
        if (currentSource != null && !currentSource.isPlaying)
        {
            StartCrossfade();
        }
    }

    private void StartMusic()
    {
        if (musicSource1 == null || backgroundMusic1 == null)
        {
            return;
        }

        musicSource1.clip = backgroundMusic1;
        musicSource1.volume = musicVolume;
        musicSource1.Play();
        isPlayingMusic1 = true;
        isMusicPlaying = true;
    }

    private void StartCrossfade()
    {
        if (isPlayingMusic1)
        {
            if (backgroundMusic2 == null || musicSource2 == null)
            {
                return;
            }

            musicSource2.clip = backgroundMusic2;
            musicSource2.volume = 0f;
            musicSource2.Play();
            
            StartCoroutine(CrossfadeCoroutine(musicSource1, musicSource2));
            isPlayingMusic1 = false;
        }
        else
        {
            if (backgroundMusic1 == null || musicSource1 == null)
            {
                return;
            }

            musicSource1.clip = backgroundMusic1;
            musicSource1.volume = 0f;
            musicSource1.Play();
            
            StartCoroutine(CrossfadeCoroutine(musicSource2, musicSource1));
            isPlayingMusic1 = true;
        }
    }

    private System.Collections.IEnumerator CrossfadeCoroutine(AudioSource fadeOutSource, AudioSource fadeInSource)
    {
        float elapsedTime = 0f;
        float startVolumeOut = fadeOutSource != null ? fadeOutSource.volume : 0f;
        float startVolumeIn = fadeInSource != null ? fadeInSource.volume : 0f;

        while (elapsedTime < CROSSFADE_DURATION)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / CROSSFADE_DURATION;

            if (fadeOutSource != null)
            {
                fadeOutSource.volume = Mathf.Lerp(startVolumeOut, 0f, t);
            }

            if (fadeInSource != null)
            {
                fadeInSource.volume = Mathf.Lerp(startVolumeIn, musicVolume, t);
            }

            yield return null;
        }

        if (fadeOutSource != null)
        {
            fadeOutSource.volume = 0f;
            fadeOutSource.Stop();
        }

        if (fadeInSource != null)
        {
            fadeInSource.volume = musicVolume;
        }
    }

    public void PlaySFX()
    {
        float currentTime = Time.time;
        if (currentTime - lastPlayTime < SOUND_COOLDOWN)
        {
            return;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
            lastPlayTime = currentTime;
        }
    }
}

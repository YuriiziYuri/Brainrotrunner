using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BrainrotAudioManager : MonoBehaviour
{
    public static BrainrotAudioManager Instance { get; private set; }

    [Header("Опціональні власні MP3/WAV треки")]
    public AudioClip customMenuMusic;
    public AudioClip customRunMusic;

    [Range(0f, 1f)] public float masterVolume = 0.5f;

    private AudioSource musicSource;
    private volatile bool isRunMode = false;
    private volatile bool useProceduralSynth = true;
    private volatile float threadMasterVolume = 0.5f;

    private double sampleRate = 44100.0;
    private double phase = 0.0;
    private double noteTimer = 0.0;
    private int noteIndex = 0;

    private readonly float[] menuNotes = { 261.63f, 329.63f, 392.00f, 523.25f, 392.00f, 329.63f, 293.66f, 349.23f };
    private readonly float[] runNotes = {
        130.81f, 261.63f, 196.00f, 261.63f, 146.83f, 293.66f, 220.00f, 293.66f,
        164.81f, 329.63f, 246.94f, 329.63f, 174.61f, 349.23f, 261.63f, 392.00f
    };

    private volatile float sfxFreq = 0f;
    private volatile float sfxTimeLeft = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        AudioListener.pause = false;
        AudioListener.volume = 1f;

        if (FindFirstObjectByType<AudioListener>() == null)
        {
            Camera cam = Camera.main;
            if (cam != null) cam.gameObject.AddComponent<AudioListener>();
            else gameObject.AddComponent<AudioListener>();
        }

        sampleRate = AudioSettings.outputSampleRate;
        if (sampleRate <= 0) sampleRate = 44100.0;

        musicSource = GetComponent<AudioSource>();
        musicSource.playOnAwake = true;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.volume = 1f;
        musicSource.mute = false;

        AudioClip dummy = AudioClip.Create("SynthDriver", 44100, 1, 44100, false);
        musicSource.clip = dummy;
        musicSource.Play();

        UpdateSynthFlags();
    }

    private void Update()
    {
        threadMasterVolume = masterVolume;
    }

    private void UpdateSynthFlags()
    {
        if (!isRunMode && customMenuMusic != null)
        {
            useProceduralSynth = false;
            if (musicSource.clip != customMenuMusic)
            {
                musicSource.clip = customMenuMusic;
                musicSource.Play();
            }
        }
        else if (isRunMode && customRunMusic != null)
        {
            useProceduralSynth = false;
            if (musicSource.clip != customRunMusic)
            {
                musicSource.clip = customRunMusic;
                musicSource.Play();
            }
        }
        else
        {
            useProceduralSynth = true;
        }
    }

    public void PlayMenuMusic()
    {
        isRunMode = false;
        noteIndex = 0;
        UpdateSynthFlags();
    }

    public void PlayRunMusic()
    {
        isRunMode = true;
        noteIndex = 0;
        UpdateSynthFlags();
    }

    public void PlayClick()
    {
        sfxFreq = 880f;
        sfxTimeLeft = 0.08f;
    }

    public void PlayLevelUp()
    {
        sfxFreq = 1046.5f;
        sfxTimeLeft = 0.35f;
    }

    public void PlayGameOver()
    {
        sfxFreq = 196f;
        sfxTimeLeft = 0.4f;
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        // Працюємо тільки з чистими змінними без звернення до UnityEngine.Object
        if (!useProceduralSynth)
            return;

        float[] currentPattern = isRunMode ? runNotes : menuNotes;
        double noteDuration = isRunMode ? 0.18 : 0.35;
        double dt = 1.0 / sampleRate;
        float vol = threadMasterVolume;

        for (int i = 0; i < data.Length; i += channels)
        {
            noteTimer += dt;
            if (noteTimer >= noteDuration)
            {
                noteTimer -= noteDuration;
                noteIndex = (noteIndex + 1) % currentPattern.Length;
            }

            float freq = currentPattern[noteIndex];
            phase += 2.0 * System.Math.PI * freq * dt;
            if (phase > 2.0 * System.Math.PI) phase -= 2.0 * System.Math.PI;

            float env = Mathf.Exp((float)(-noteTimer * (isRunMode ? 5.0 : 2.8)));
            float musicSample = (float)System.Math.Sin(phase) * env * 0.4f;

            if (isRunMode && (noteIndex % 2 == 0) && noteTimer < 0.07)
            {
                float kickEnv = 1f - (float)(noteTimer / 0.07);
                musicSample += Mathf.Sin((float)(noteTimer * 550.0)) * kickEnv * 0.35f;
            }

            float sfxSample = 0f;
            if (sfxTimeLeft > 0f)
            {
                sfxTimeLeft -= (float)dt;
                sfxSample = Mathf.Sin((float)(sfxTimeLeft * sfxFreq * 6.283)) * Mathf.Clamp01(sfxTimeLeft * 8f) * 0.45f;
            }

            float finalSample = Mathf.Clamp((musicSample + sfxSample) * vol, -1f, 1f);

            for (int c = 0; c < channels; c++)
            {
                data[i + c] = finalSample;
            }
        }
    }
}
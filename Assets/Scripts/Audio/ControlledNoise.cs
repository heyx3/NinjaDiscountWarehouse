using UnityEngine;
using System.Collections;


/// <summary>
/// Controls a collection of audio sources that belong to this component's GameObject.
/// Allows fading in/out of sources.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class ControlledNoise : MonoBehaviour
{
    AudioSource source;

    /// <summary>
    /// The exponent representing the volume drop-off for a fading noise.
    /// For example, a value of 3 means a cubic drop-off.
    /// </summary>
    public float FadeRate = 1.0f;

    public float elapsed;

    public bool PlayOnStart = false;

    public float MaxVolume = 1.0f, MinVolume = 0.0f;
    public float MinPitch = 1.0f, MaxPitch = 1.0f;
    public float FadeTime = 1.0f;
    public bool Fade = true;
    public bool Playing { get; private set; }

    public float FadeInBy { get { return FadeTime; } }
    public float FadeOutStartBy { get { return source.clip.length - FadeTime; } }

    public float ElapsedTime { get; private set; }
    public float TimeUntilFadeOutStart { get { if (source == null || source.clip == null || !Fade) return System.Single.PositiveInfinity; return source.clip.length - FadeTime - ElapsedTime; } }
    public float TimeUntilSoundEnd { get { if (source == null || source.clip == null) return System.Single.PositiveInfinity; return source.clip.length - ElapsedTime; } }

    void Awake()
    {
        if (PlayOnStart)
            StartClip();
        else Playing = false;
    }

    /// <summary>
    /// Randomly chooses one of the audio sources owned by this object,
    /// or "null" if there are no audio sources to choose from.
    /// </summary>
    private AudioSource RandomSource()
    {
        AudioSource[] sources = GetComponents<AudioSource>();
        if (sources.Length == 0)
        {
            return null;
        }

        int i = Random.Range(0, sources.Length);

        return sources[i];
    }
    /// <summary>
    /// Chooses the given audio source owned by this object, or "null" if that source doesn't exist.
    /// </summary>
    private AudioSource NumberedSource(int index)
    {
        AudioSource[] sources = GetComponents<AudioSource>();
        if (sources.Length - 1 < index)
        {
            return null;
        }

        return sources[index];
    }

    /// <summary>
    /// Starts playing one of this object's audio sources.
    /// </summary>
    /// <param name="index">If passed a parameter larger than 0, uses it as an index into the array of AudioSource components.
    /// Otherwise, uses a random AudioSource.</param>
    public void StartClip(int index = -1)
    {
        if (Playing) throw new System.InvalidOperationException();

        Playing = true;

        if (index < 0)
        {
            source = RandomSource();
        }
        else
        {
            source = NumberedSource(index);
        }
        if (source == null || source.clip == null)
        {
            Destroy(gameObject);
            return;
        }

        source.pitch = Mathf.Lerp(MinPitch, MaxPitch, Random.value);

        MaxVolume = source.volume;

        if (Fade)
        {
            source.volume = MinVolume;
        }
        else
        {
            source.volume = MaxVolume;
        }
        source.Play();
    }

    void FixedUpdate()
    {
        if (!Playing) return;

        //If the sound is done playing, destroy this object.
        if (TimeUntilSoundEnd <= 0.0f)
        {
            GameObject.Destroy(gameObject);
            return;
        }
        //If the sound shouldn't fade, leave the volume alone.
        if (!Fade)
        {
            source.volume = MaxVolume;
            ElapsedTime += Time.fixedDeltaTime;
            return;
        }


        //Fading in.
        if (ElapsedTime < FadeInBy)
        {
            float lerpVal = ElapsedTime / FadeInBy;
            float lerped = Mathf.Pow(Mathf.Lerp(0.0f, 1.0f, lerpVal), FadeRate);
            source.volume = (lerped * (MaxVolume - MinVolume)) + MinVolume;
        }
        //Fading out.
        else if (ElapsedTime > FadeOutStartBy)
        {
            float lerpVal = (ElapsedTime - FadeOutStartBy) / FadeTime;
            float lerped = Mathf.Pow(Mathf.Lerp(1.0f, 0.0f, lerpVal), FadeRate);
            source.volume = (lerped * (MaxVolume - MinVolume)) + MinVolume;
        }
        //Just playing normally.
        else
        {
            source.volume = MaxVolume;
        }

        ElapsedTime += Time.fixedDeltaTime;

        elapsed = ElapsedTime;
    }
}

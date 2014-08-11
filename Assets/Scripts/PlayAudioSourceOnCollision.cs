using UnityEngine;


[RequireComponent(typeof(AudioSource))]
public class PlayAudioSourceOnCollision : MonoBehaviour
{
	private const int maxSounds = 6;
	private static int currentSounds = 0;


	public AudioSource Source { get; private set; }
	public AudioClip ClipToPlay = null;

	private bool isPlaying = false;


	void Awake()
	{
		Source = audio;
	}

	void Update()
	{
		if (isPlaying && !Source.isPlaying)
		{
			isPlaying = false;
			currentSounds -= 1;
		}
	}

	void OnCollisionEnter(Collision coll)
	{
		if (ClipToPlay != null && currentSounds < maxSounds)
		{
			Source.PlayOneShot(ClipToPlay);
			currentSounds += 1;
			isPlaying = true;
		}
	}
}
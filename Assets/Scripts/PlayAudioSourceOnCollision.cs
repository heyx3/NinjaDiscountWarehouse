using UnityEngine;


[RequireComponent(typeof(AudioSource))]
public class PlayAudioSourceOnCollision : MonoBehaviour
{
	public AudioSource Source { get; private set; }
	public AudioClip ClipToPlay = null;

	void Awake()
	{
		Source = audio;
	}

	void OnCollisionEnter(Collision coll)
	{
		if (ClipToPlay != null)
			Source.PlayOneShot(ClipToPlay);
	}
}
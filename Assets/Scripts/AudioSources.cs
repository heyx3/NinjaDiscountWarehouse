using UnityEngine;


/// <summary>
/// Holds references to all the different audio sources.
/// </summary>
public class AudioSources : MonoBehaviour
{
	public static AudioSources Instance { get; private set; }


	public AudioClip EnemyKillNoise, EnemyComboNoise,
					 LevitateStartNoise;
	public GameObject LevitateLoopNoiseSourcePrefab;


	void Awake()
	{
		if (Instance != null)
			throw new UnityException("More than one AudioSources component right now!");
		Instance = this;
	}
}
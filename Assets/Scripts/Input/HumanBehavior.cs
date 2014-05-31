using UnityEngine;


/// <summary>
/// Handles behavior for the player playing this game.
/// </summary>
public class HumanBehavior : MonoBehaviour
{
	public static HumanBehavior Instance { get; private set; }


	public Transform MyTransform { get; private set; }


	void Awake()
	{
		if (Instance != null) throw new UnityException("More than one instance of 'HumanBehavior'!");
		Instance = this;

		MyTransform = transform;
	}
}
using UnityEngine;


/// <summary>
/// The overall director for the game. Provides game logic, global variables,
/// and references to important components/objects.
/// </summary>
public class GameDirector : MonoBehaviour
{
	public static GameDirector Instance { get; private set; }


	void Awake()
	{
		if (Instance != null)
			throw new UnityException("More than one GameDirector instance in play!");
		Instance = this;
	}
}
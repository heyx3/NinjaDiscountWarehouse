using UnityEngine;


/// <summary>
/// The overall director for the game. Provides game logic, global variables,
/// and references to important components/objects.
/// </summary>
public class GameDirector : MonoBehaviour
{
	public static GameDirector Instance { get; private set; }


	public GameObject LinePrefab = null;
	public Transform CreateLine(Vector3 start, Vector3 dir)
	{
		Transform trns = ((GameObject)Instantiate(LinePrefab)).transform;
		trns.position = start;
		trns.rotation = Quaternion.FromToRotation(new Vector3(0.0f, 1.0f, 0.0f), dir);
		return trns;
	}


	void Awake()
	{
		if (Instance != null)
			throw new UnityException("More than one GameDirector instance in play!");
		Instance = this;

		if (LinePrefab == null)
			throw new UnityException("'LinePrefab' field is null!");
	}
}
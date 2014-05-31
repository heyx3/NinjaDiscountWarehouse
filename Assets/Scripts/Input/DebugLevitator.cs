using UnityEngine;


/// <summary>
/// Allows for debugging of levitate behavior.
/// </summary>
[RequireComponent(typeof(Levitatable))]
public class DebugLevitator : MonoBehaviour
{
	public bool PressThisToLevitate = false;
	public bool PressThisToThrow = false;

	public Vector3 ThrowDir = new Vector3(1.0f, 0.0f, 1.0f).normalized;


	private Levitatable levitator;


	void Awake()
	{
		levitator = GetComponent<Levitatable>();
	}


	void Update()
	{
		if (PressThisToLevitate)
		{
			PressThisToLevitate = false;
			levitator.Levitate();
		}
		if (PressThisToThrow)
		{
			PressThisToThrow = false;
			levitator.Throwing.Direction = ThrowDir.normalized;
			levitator.Throw(ThrowDir);
		}
	}
}